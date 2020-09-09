//import { CssSelectorParser, Selector } from 'css-selector-parser'
import { storage } from 'uxp'
import * as consts from './consts'
import { GlobalVars } from "./globals";
import {
  getChildIndex,
  isFirstChild,
  isLastChild,
  isOnlyChild, isRootNode,
  sameParentBounds
} from "./node";
import { asBool } from './tools'
import * as tools from './tools'
const CssSelectorParser = require('./node_modules/css-selector-parser/lib/index')
  .CssSelectorParser
// import {CssSelectorParser} from 'css-selector-parser'

let cssSelectorParser = new CssSelectorParser()
//cssSelectorParser.registerSelectorPseudos('has')
cssSelectorParser.registerNumericPseudos('root')
cssSelectorParser.registerNumericPseudos('nth-child')
cssSelectorParser.registerSelectorPseudos('first-child')
cssSelectorParser.registerSelectorPseudos('last-child')
cssSelectorParser.registerSelectorPseudos('same-parent-bounds')
cssSelectorParser.registerNestingOperators('>', '+', '~', ' ')
cssSelectorParser.registerAttrEqualityMods('^', '$', '*', '~')
cssSelectorParser.enableSubstitutes()

class CssSelector {
  private json: any
  /**
   * @param {string} selectorText
   */
  constructor(selectorText) {
    if (!selectorText) {
      throw 'CssSelectorがNULLで作成された'
    }
    // console.log("SelectorTextをパースします",selectorText)
    this.json = cssSelectorParser.parse(selectorText.trim())
    /*
      console.log(
        'SelectorTextをパースした',
        JSON.stringify(this.json, null, '  '),
      )
       */
  }

  /**
   * 擬似クラスの:rooｔであるか
   * @return {boolean}
   */
  isRoot() {
    const rule = this.json['rule']
    if (!rule) return false
    const pseudos = rule['pseudos']
    // console.log("isRoot() pseudos確認:", pseudos)
    return pseudos && pseudos[0].name === 'root'
  }

  /**
   *
   * @param {{name:string, parent:*}} node
   * @param {{type:string, classNames:string[], id:string, tagName:string, pseudos:*[], nestingOperator:string, rule:*, selectors:*[] }|null} rule
   * @param verboseLog
   * @return {null|*}
   */
  matchRule(node, rule = null, verboseLog = false) {
    if (verboseLog) console.log('# matchRule')
    if (!rule) {
      rule = this.json
    }
    if (!rule) {
      return null
    }
    let checkNode = node
    let ruleRule = rule.rule
    switch (rule.type) {
      case 'rule': {
        if (verboseLog) console.log('## type:rule')
        // まず奥へ入っていく
        if (ruleRule) {
          checkNode = this.matchRule(node, ruleRule, verboseLog)
          if (!checkNode) {
            return null
          }
        }
        break
      }
      case 'selectors': {
        if (verboseLog) console.log('## type:selectors')
        // 複数あり、どれかに適合するかどうか
        for (let selector of rule.selectors) {
          ruleRule = selector.rule
          checkNode = this.matchRule(node, ruleRule, verboseLog)
          if (checkNode) break
        }
        if (!checkNode) {
          return null
        }
        break
      }
      case 'ruleSet': {
        if (verboseLog) console.log('## type:ruleSet')
        return this.matchRule(node, ruleRule, verboseLog)
      }
      default:
        return null
    }
    if (ruleRule && ruleRule.nestingOperator === null) {
      if (verboseLog) console.log('nullオペレータ確認をする')
      while (checkNode) {
        let result = CssSelector.check(checkNode, rule, verboseLog)
        if (result) {
          if (verboseLog) console.log('nullオペレータで整合したものをみつけた')
          return checkNode
        }
        checkNode = checkNode.parent
      }
      if (verboseLog)
        console.log('nullオペレータで整合するものはみつからなかった')
      return null
    }
    let result = CssSelector.check(checkNode, rule, verboseLog)
    if (!result) {
      if (verboseLog) console.log('このruleは適合しなかった')
      return null
    }
    if (verboseLog) console.log('check成功')
    if (rule.nestingOperator === '>' || rule.nestingOperator == null) {
      if (verboseLog)
        console.log(
          `nestingオペレータ${rule.nestingOperator} 確認のため、checkNodeを親にすすめる`,
        )
      checkNode = checkNode.parent
    }
    return checkNode
  }

  /**
   * @param {{name:string, parent:*}} node マスクチェックのために node.maskとすることがある
   * @param {{type:string, classNames:string[], id:string, tagName:string, attrs:*[], pseudos:*[], nestingOperator:string, rule:*, selectors:*[] }|null} rule
   * @return {boolean}
   */
  static check(node, rule, verboseLog = false) {
    if (!node) return false
    const nodeName = node.name.trim()
    const parsedNodeName = cssParseNodeName(nodeName)
    if (verboseLog) {
      console.log('# rule check')
      console.log('- name:', node.name)
      console.log(parsedNodeName)
      console.log('## 以下のruleと照らし合わせる')
      console.log(rule)
    }
    if (rule.tagName && rule.tagName !== '*') {
      if (
        rule.tagName !== parsedNodeName.tagName &&
        rule.tagName !== nodeName
      ) {
        if (verboseLog) console.log('tagNameが適合しない')
        return false
      }
    }
    if (rule.id && rule.id !== parsedNodeName.id) {
      if (verboseLog) console.log('idが適合しない')
      return false
    }
    if (rule.classNames) {
      if (!parsedNodeName.classNames) {
        if (verboseLog) console.log('ruleはclassを求めたがclassが無い')
        return false
      }
      for (let className of rule.classNames) {
        if (!parsedNodeName.classNames.find(c => c === className)) {
          if (verboseLog) console.log('classが適合しない')
          return false
        }
      }
    }
    if (rule.attrs) {
      // console.log('attrチェック')
      for (let attr of rule.attrs) {
        if (!this.checkAttr(node, parsedNodeName, attr)) {
          return false
        }
      }
    }
    if (rule.pseudos) {
      if (verboseLog) console.log('## 疑似クラスチェック')
      for (let pseudo of rule.pseudos) {
        if (!this.checkPseudo(node, pseudo)) {
          if (verboseLog) console.log(`- ${pseudo.name} が適合しません`)
          return false
        }
        if (verboseLog) console.log(`- ${pseudo.name} が適合した`)
      }
    }
    //console.log(nodeName)
    //console.log(JSON.stringify(parsedNodeName, null, '  '))
    if (verboseLog) console.log('このruleは適合した')
    return true
  }

  static checkAttr(node, parsedNodeName, attr) {
    switch (attr.name) {
      case 'class': {
        if (
          !CssSelector.namesCheck(
            attr.operator,
            parsedNodeName.classNames,
            attr.value,
          )
        )
          return false
        break
      }
      case 'id': {
        if (
          !CssSelector.nameCheck(attr.operator, parsedNodeName.id, attr.value)
        )
          return false
        break
      }
      case 'tag-name': {
        if (
          !CssSelector.nameCheck(
            attr.operator,
            parsedNodeName.tagName,
            attr.value,
          )
        )
          return false
        break
      }
      case 'type-of':
      case 'typeof': {
        if (
          !CssSelector.nameCheck(
            attr.operator,
            node.constructor.name,
            attr.value,
          )
        )
          return false
        break
      }
      default:
        console.log('**error** 未対応の要素名です:', attr.name)
        return false
    }
    return true
  }

  static checkPseudo(node, pseudo) {
    let result = false
    switch (pseudo.name) {
      case 'nth-child':
        const nthChild = parseInt(pseudo.value)
        const nodeChildIndex = getChildIndex(node) + 1
        result = nthChild === nodeChildIndex
        break
      case 'first-child':
        result = isFirstChild(node)
        break
      case 'last-child':
        result = isLastChild(node)
        break
      case 'only-child':
        result = isOnlyChild(node)
        break
      case 'same-parent-bounds':
        result = sameParentBounds(node)
        break
      case 'root':
        result = isRootNode(node)
        break
      default:
        console.log('**error** 未対応の疑似要素です', pseudo.name)
        result = false
        break
    }
    return result
  }

  /**
   * @param {string} op
   * @param {string[]} names
   * @param value
   */
  static namesCheck(op, names, value) {
    if (!op || names == null) return false
    for (let name of names) {
      if (this.nameCheck(op, name, value)) return true
    }
    return false
  }

  /**
   * @param {string} op
   * @param {string} name
   * @param value
   */
  static nameCheck(op, name, value) {
    if (!op || name == null || value == null) return false
    switch (op) {
      case '=':
        return name === value
      case '*=':
        return name.includes(value) > 0
      case '^=':
        return name.startsWith(value)
      case '$=':
        return name.endsWith(value)
      case '|=':
        if (name === value) return true
        return name.startsWith(value + '-')
    }
    return false
  }
}

class CssDeclarations {
  private declarations: {}
  constructor(declarationBlock: string = null) {
    if (declarationBlock) {
      this.declarations = parseCssDeclarationBlock(declarationBlock)
    } else {
      this.declarations = {}
    }
  }

  properties() {
    return Object.keys(this.declarations)
  }

  values(property: string): string[] {
    return this.declarations[property]
  }

  /**
   * @param {string} property
   * @return {*|null}
   */
  first(property) {
    const values = this.values(property)
    if (values == null) return null
    return values[0]
  }

  setFirst(property, value) {
    let values = this.values(property)
    if (!values) {
      values = this.declarations[property] = []
    }
    values[0] = value
  }

  firstAsBool(property) {
    return asBool(this.first(property))
  }
}

function parseCss(text, errorThrow = true):any {
  // コメントアウト処理 エラー時に行数を表示するため、コメント内の改行を残す
  //TODO: 文字列内の /* */について正しく処理できない
  text = text.replace(/\/\*[\s\S]*?\*\//g, str => {
    let replace = ''
    for (let c of str) {
      if (c === '\n') replace += c
    }
    return replace
  })
  // declaration部がなくてもSelectorだけで取得できるようにする　NodeNameのパースに使うため
  // const tokenizer = /(?<at_rule>\s*@[^;]+;\s*)|((?<selector>(("([^"\\]|\\.)*")|[^{"]+)+)({(?<decl_block>(("([^"\\]|\\.)*")|[^}"]*)*)}\s*)?)/gi
  // シングルクオーテーション
  const tokenizer = /(?<at_rule>\s*@[^;]+;\s*)|((?<selector>(('([^'\\]|\\.)*')|[^{']+)+)({(?<decl_block>(('([^'\\]|\\.)*')|[^}']*)*)}\s*)?)/gi
  const rules = []
  let token
  while ((token = tokenizer.exec(text))) {
    try {
      const tokenAtRule = token.groups.at_rule
      const tokenSelector = token.groups.selector
      const tokenDeclBlock = token.groups.decl_block
      if (tokenAtRule) {
        rules.push({ at_rule: tokenAtRule })
      } else if (tokenSelector) {
        const selector = new CssSelector(tokenSelector)
        let declarations = null
        if (tokenDeclBlock) {
          declarations = new CssDeclarations(tokenDeclBlock)
        }
        rules.push({
          selector,
          declarations,
        })
      }
    } catch (e) {
      if (errorThrow) {
        // エラー行の算出
        const parsedText = text.substr(0, token.index) // エラーの起きた文字列までを抜き出す
        const lines = parsedText.split(/\n/)
        //const errorIndex = text.indexOf()
        //const errorLastIndex = text.lastIndexOf("\n",token.index)
        const errorLine = text.substring(token.index - 30, token.index + 30)
        const errorText =
          `CSSのパースに失敗した: ${lines.length}行目:${errorLine}\n` +
          e.message
        console.log(errorText)
        // console.log(e.stack)
        // console.log(text)
        throw errorText
      }
    }
  }
  return rules
}

export function cssParseNodeName(nodeName) {
  nodeName = nodeName.trim()
  const cache = GlobalVars.cacheParseNodeName[nodeName]
  if (cache) {
    return cache
  }
  // コメントアウトチェック
  let result = null
  if (nodeName.startsWith('//')) {
    // コメントアウトのスタイルを追加する
    const declarations = new CssDeclarations()
    declarations.setFirst(consts.STYLE_COMMENT_OUT, true)
    result = { declarations }
  } else {
    // nameの作成　{} を除いた部分
    let name = nodeName
    const pattern = /(.*)({.*})/g
    const r = pattern.exec(nodeName)
    if (r && r.length > 1) {
      name = r[1].trim()
    }
    try {
      // ascii文字以外 _ に変換する
      const asciiNodeName = nodeName.replace(/[^\x01-\x7E]/g, function(s) {
        return '_'
      })
      let rules = parseCss(asciiNodeName, false) // 名前はエラーチェックしない
      if (!rules || rules.length === 0 || !rules[0].selector) {
        // パースできなかった場合はそのまま返す
        result = { name, tagName: nodeName }
      } else {
        result = rules[0].selector.json['rule'] // 一番外側の｛｝をはずす
        Object.assign(result, {
          name,
          declarations: rules[0].declarations,
        })
      }
    } catch (e) {
      console.log('parseNodeName: exception')
      result = { name, tagName: nodeName }
    }
  }
  GlobalVars.cacheParseNodeName[nodeName] = result
  return result
}

function parseCssDeclarationBlock(declarationBlock: string): {} {
  declarationBlock = declarationBlock.trim()
  // const tokenizer = /(?<property>[^:";\s]+)\s*:\s*|(?<value>"(?<string>([^"\\]|\\.)*)"|var\([^\)]+\)|[^";:\s]+)/gi
  const tokenizer = /(?<property>[^:';\s]+)\s*:\s*|(?<value>'(?<string>([^'\\]|\\.)*)'|var\([^\)]+\)|[^';:\s]+)/gi
  /** @type {string[][]}　*/
  let values = {}
  /** @type {string[]}　*/
  let currentValues = null
  let token
  while ((token = tokenizer.exec(declarationBlock))) {
    const property = token.groups.property
    if (property) {
      currentValues = []
      values[property] = currentValues
    }
    let value = token.groups.value
    if (value) {
      if (token.groups.string) {
        value = token.groups.string
      }
      if (!currentValues) {
        // Propertyが無いのに値がある場合
        throw 'DeclarationBlockのパースに失敗した'
      }
      currentValues.push(value)
    }
  }
  return values
}

/**
 * @param currentFolder
 * @param filename
 */
export async function loadCssRules(currentFolder: storage.Folder, filename):Promise<any> {
  if (!currentFolder) return null
  // console.log(`${filename}の読み込みを開始します`)
  let file
  try {
    file = await currentFolder.getEntry(filename)
  } catch (e) {
    // console.log("cssフォルダ以下にもあるかチェック")
    file = await currentFolder.getEntry('css/' + filename)
    if (!file) return null
  }
  const contents = await file.read()
  let parsed = parseCss(contents)
  for (let parsedElement of parsed) {
    const atRule = parsedElement.at_rule
    if (atRule) {
      const importTokenizer = /\s*@import\s*url\("(?<file_name>.*)"\);/
      let token = importTokenizer.exec(atRule)
      const importFileName = token.groups.file_name
      if (importFileName) {
        const p = await loadCssRules(currentFolder, importFileName)
        //TODO: 接続する位置とループ対策
        parsed = parsed.concat(p)
      }
    }
  }
  console.log(`- ${file.name} loaded.`)
  return parsed
}
