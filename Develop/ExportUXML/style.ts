import * as consts from './consts'
import { GlobalVars } from "./globals";
import { getNodeNameAndStyle, isContentChild } from './node'
import { asBool, hasAnyValue } from './tools'

/**
 * @param {[]} styleFix
 * @returns {null|{top: boolean, left: boolean, bottom: boolean, width: boolean, right: boolean, height: boolean}}
 */
export function getStyleFix(styleFix) {
  if (styleFix == null) {
    return null
  }

  // null：わからない　true:フィックス　false:フィックスされていないで確定 いずれ数字に変わる
  // この関数はFixオプションで指定されているかどうかを返すので、TrueかFalse
  let styleFixWidth = false
  let styleFixHeight = false
  let styleFixTop = false
  let styleFixBottom = false
  let styleFixLeft = false
  let styleFixRight = false

  if (hasAnyValue(styleFix, 'w', 'width', 'size')) {
    styleFixWidth = true
  }
  if (hasAnyValue(styleFix, 'h', 'height', 'size')) {
    styleFixHeight = true
  }
  if (hasAnyValue(styleFix, 't', 'top')) {
    styleFixTop = true
  }
  if (hasAnyValue(styleFix, 'b', 'bottom')) {
    styleFixBottom = true
  }
  if (hasAnyValue(styleFix, 'l', 'left')) {
    styleFixLeft = true
  }
  if (hasAnyValue(styleFix, 'r', 'right')) {
    styleFixRight = true
  }
  if (hasAnyValue(styleFix, 'x')) {
    styleFixLeft = true
    styleFixRight = true
  }
  if (hasAnyValue(styleFix, 'y')) {
    styleFixTop = true
    styleFixBottom = true
  }

  return {
    left: styleFixLeft,
    right: styleFixRight,
    top: styleFixTop,
    bottom: styleFixBottom,
    width: styleFixWidth,
    height: styleFixHeight,
  }
}

export class Style {
  public style: {[key:string]:any}
  /**
   *
   * @param {*[][]} style
   */
  constructor(style = null) {
    if (style != null) {
      this.style = style
    } else {
      this.style = {}
    }
  }

  /**
   * スタイルへ宣言部を追加する
   * ここで VAR()など値に変わる
   * @param {CssDeclarations} declarations
   */
  addDeclarations(declarations) {
    const properties = declarations.properties()
    for (let property of properties) {
      const declValues = declarations.values(property)
      const values = []
      for (let declValue of declValues) {
        // console.log('declValue:', declValue)
        if (typeof declValue == 'string' && declValue.startsWith('var(')) {
          const tokenizer = /var\(\s*(?<id>\S*)\s*\)/
          let token = tokenizer.exec(declValue.trim())
          const id = token.groups.id
          let value = id ? GlobalVars.cssVars[id] : null
          // console.log(`var(${id})をみつけました値は${value}`)
          values.push(value)
        } else {
          values.push(declValue)
        }
      }
      this.style[property] = values
    }
  }

  values(property) {
    return this.style[property]
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

  /**
   *
   * @param {string} property
   * @param {*} value
   */
  setFirst(property, value) {
    let values = this.values(property)
    if (!values) {
      values = this.style[property] = []
    }
    values[0] = value
  }

  /**
   * @param {string} property
   * @return {boolean}
   */
  has(property) {
    let values = this.values(property)
    return !!values
  }

  /**
   * @param {string} property
   * @param checkValues
   * @return {boolean}
   */
  hasValue(property, ...checkValues) {
    //hasAnyValue(this.values(), checkValues)
    let values = this.values(property)
    if (!values) {
      return false
    }
    for (let value of values) {
      for (let checkValue of checkValues) {
        if (value === checkValue) return true
      }
    }
    return false
  }

  firstAsBool(property) {
    const first = this.first(property)
    return asBool(first)
  }

  firstCheck(property, node) {
    let first = this.first(property)
    let result = null
    switch (first) {
      case 'if-not-content-only-child-has-layout-properties': {
        // console.log('if-not-content-only-child-has-layout-properties')
        const contents = node.children.filter(child => {
          return isContentChild(child)
        })
        if (contents.length !== 1) {
          result = true
          break
        }
        const contentChild = contents[0]
        const { style: contentStyle } = getNodeNameAndStyle(contentChild)
        result = !hasLayoutProperties(contentStyle)
        break
      }
      default:
        result = asBool(first)
        break
    }
    return result
  }

  /**
   * Valuesの値を連結した文字列を返す
   * @param {string} property
   * @return {string|null}
   */
  str(property) {
    const values = this.values(property)
    if (!values) return null
    let str = ''
    for (let value of values) {
      str += value.toString() + ' '
    }
    return str
  }

  forEach(callback: (key: string, value) => void) {
    for (let styleKey in this.style) {
      callback(styleKey, this.style[styleKey])
    }
  }
}

export function hasLayoutProperties(style) {
  return (
    style.firstAsBool(consts.STYLE_TEXT) ||
    style.firstAsBool(consts.STYLE_TEXTMP) ||
    style.firstAsBool(consts.STYLE_IMAGE) ||
    style.firstAsBool(consts.STYLE_LAYOUT_GROUP)
  )
}
