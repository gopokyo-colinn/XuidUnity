import { SceneNode } from "scenegraph";
import * as consts from "./consts";
import { cssParseNodeName } from "./css";
import { GlobalVars } from "./globals";
import { Style } from "./style";
import { asBool } from "./tools";

export function traverseNode(node, func) {
  let result = func(node)
  if (result === false) return // 明確なFalseの場合、子供へはいかない
  node.children.forEach(child => {
    traverseNode(child, func)
  })
}

/**
 * 自分が何番目の子供か
 * @param node
 * @return {number}
 */
export function getChildIndex(node: SceneNode) {
  const parentNode = node.parent
  if (!parentNode) return -1
  for (
    let childIndex = 0;
    childIndex < parentNode.children.length;
    childIndex++
  ) {
    if (parentNode.children.at(childIndex) === node) {
      return childIndex
    }
  }
  return -1
}

/**
 * @param node
 * @return {boolean}
 */
export function isFirstChild(node: SceneNode) {
  const parent = node.parent
  if (!parent) return false
  return parent.children.at(0) === node
}

export function isLastChild(node) {
  const parent = node.parent
  if (!parent) return false
  const lastIndex = parent.children.length - 1
  return parent.children.at(lastIndex) === node
}

export function isOnlyChild(node: SceneNode) {
  const parent = node.parent
  if (!parent) return false
  const lastIndex = parent.children.length - 1
  return parent.children.at(lastIndex) === node
}



/**
 * Viewportの役割をもつノードを返す
 * Maskをもっている場合はMask
 * @param node
 * @return {null|SceneNode|{show_mask_graphic: boolean}|string|*}
 */
function getViewport(node) {
  const { style } = getNodeNameAndStyle(node)
  const styleViewport = style.first(consts.STYLE_CREATE_CONTENT)
  if (asBool(styleViewport)) return node
  if (node.mask) return node.mask
  if (node.constructor.name === 'RepeatGrid') return node
  return null
}

/**
 * 親と同じBoundsかどうか
 * Padding、*-Stackで、nodeが背景になっているかどうかの判定につかう
 * paddingがマイナスだと判定できない
 * @param node
 * @return {boolean|boolean}
 */
export function sameParentBounds(node: SceneNode) {
  const parent = node.parent
  if (!parent) return false
  // 判定に使う値は、cacheにあるものを使わない
  // 同じかどうかわかれば良いので、getGlobalBounds関数もつかわなくて良い
  // ただ、Artboardのリサイズには対応できない
  const bounds = node.globalBounds
  const parentBounds = parent.globalBounds
  if (!bounds || !parentBounds) return false
  // TODO:誤差を許容する判定をつかわなくてよいか
  return (
    bounds.x === parentBounds.x &&
    bounds.y === parentBounds.y &&
    bounds.width === parentBounds.width &&
    bounds.height === parentBounds.height
  )
}


export function isRootNode(node:SceneNode)
{
  return node === GlobalVars.rootNode;
}

/**
 *
 * @param {SceneNode|SceneNodeClass} node
 * @return {boolean}
 */
export function isContentChild(node) {
  const {style} = getNodeNameAndStyle(node);
  if (style.firstAsBool(consts.STYLE_COMPONENT)) return false;
  if (node.parent.mask === node) return false;
  return hasContentBounds(node.parent);
}


/**
 * nodeがnode.Boundsとことなる　content boundsをもつかどうか
 * Contentとは、Childrenをもっていて、なにかでMaskされているグループのこと
 * nodeと、ChildrenのDrawBoundsが異なる場合 Contentを持つという
 * ・MaskedGroup
 * ※RepeatGridはもたない node.Boundsの中で 子供ノードの位置を計算する
 * @param {SceneNode|SceneNodeClass} node
 * @return {boolean}
 */
export function hasContentBounds(node) {
  const { style } = getNodeNameAndStyle(node)
  if (style.firstAsBool(consts.STYLE_CREATE_CONTENT)) return true
  if (style.firstAsBool(consts.STYLE_LAYOUT_GROUP)) return true
  if (node.mask) return true
  return false
}

/**
 * リサイズされる前のグローバル座標とサイズを取得する
 * ハッシュからデータを取得する
 * @param {SceneNode|SceneNodeClass} node
 * @return {{ex: number, ey: number, x: number, width: number, y: number, height: number}}
 */
export function getBeforeGlobalDrawBounds(node) {
  // レスポンシブパラメータ作成用で､すでに取得した変形してしまう前のパラメータがあった場合
  // それを利用するようにする
  let bounds = null
  const hashBounds = GlobalVars.responsiveBounds
  if (hashBounds) {
    const hBounds = hashBounds[node.guid]
    if (hBounds && hBounds.before) {
      bounds = Object.assign({}, hBounds.before.global_draw_bounds)
    }
  }
  if (bounds) return bounds
  // throw `リサイズ前のGlobalDrawBoundsの情報がありません: ${node}`
  return null
}

/**
 *
 * @param {{name:string, parent:*}} node
 * @returns {Style}
 */
export function getStyleFromNode(node) {
  let style = new Style()
  let localCss = null
  try {
    localCss = cssParseNodeName(node.name)
  } catch (e) {
    //node.nameがパースできなかった
  }

  for (const rule of GlobalVars.cssRules) {
    /** @type {CssSelector} */
    const selector = rule.selector
    if (
      selector &&
      selector.matchRule(
        node,
        null,
        rule.declarations.firstAsBool(consts.STYLE_CHECK_LOG),
      )
    ) {
      // console.log('マッチした宣言をスタイルに追加', rule)
      style.addDeclarations(rule.declarations)
    }
  }

  if (localCss && localCss.declarations) {
    // console.log('nodeNameのCSSパースに成功した ローカル宣言部を持っている')
    style.addDeclarations(localCss.declarations)
  }

  if (style.has(consts.STYLE_MATCH_LOG)) {
    // マッチログは上書きされるため、一つのNodeに一つだけ表示される
    console.log(`match-log:${style.values(consts.STYLE_MATCH_LOG)}`)
  }

  //console.log('Style:',style)
  return style
}

export interface NodeNameAndStyle {
  node_name: string
  style: Style
}


/**
 * node.nameをパースしオプションに分解する
 * この関数が基底にあり、正しくNodeName Styleが取得できるようにする
 * ここで処理しないと辻褄があわないケースがでてくる
 * 例：repeatgrid-child-nameで得られる名前
 * @param {SceneNode|SceneNodeClass} node ここのNodeはSceneNodeClassでないといけない
 * @returns {{node_name: string, name: string, style: Style}|null}
 */
export function getNodeNameAndStyle(node): NodeNameAndStyle {
  if (node == null) {
    return null
  }

  // キャッシュ確認
  if (node.guid) {
    const cache = GlobalVars.cacheNodeNameAndStyle[node.guid]
    if (cache) {
      return cache
    }
  }

  let parentNode = node.parent
  /**
   * @type {string}
   */
  let nodeName = node.name.trim()
  const style = getStyleFromNode(node)

  const value = {
    node_name: nodeName,
    style,
  }
  // ここでキャッシュに書き込むことで、呼び出しループになることを防ぐ
  // 注意する箇所
  // 上： getStyleFromNodeName(nodeName, parentNode, cssRules, ...) で親への参照
  // 下： node.children.some(child => { const childStyle = getNodeNameAndStyle(child).style　で、子供への参照
  GlobalVars.cacheNodeNameAndStyle[node.guid] = value

  if (parentNode && parentNode.constructor.name === 'RepeatGrid') {
    // 親がリピートグリッドの場合､名前が適当につけられるようです
    // Buttonといった名前やオプションが勝手につき､機能してしまうことを防ぐ
    // item_button
    // item_text
    // 2つセットをリピートグリッド化した場合､以下のような構成になる
    // リピートグリッド 1
    //   - item0
    //     - item_button
    //     - item_text
    //   - item1
    //     - item_button
    //     - item_text
    //   - item2
    //     - item_button
    //     - item_text
    // 以上のような構成になる
    nodeName = 'repeatgrid-child'

    value['node_name'] = nodeName

    // RepeatGridで、子供がすべてコメントアウトなら、子供を包括するグループもコメントアウトする
    style.setFirst(
      consts.STYLE_COMMENT_OUT,
      !node.children.some(child => {
        // コメントアウトしてないものが一つでもあるか
        const childStyle = getNodeNameAndStyle(child).style
        return !childStyle.first(consts.STYLE_COMMENT_OUT)
      }),
    )
  }

  return value
}


function getNodeName(node:SceneNode) {
  return getNodeNameAndStyle(node).node_name
}


export function getUnityName(node:SceneNode):string {
  const { node_name: nodeName, style } = getNodeNameAndStyle(node)

  //スタイルで名前の指定がある場合
  let unityName = style.first(consts.STYLE_UNITY_NAME)
  if (unityName) {
    //childIndexを子供番号で置き換え
    const childIndex = getChildIndex(node)
    unityName = unityName.replace(/\${childIndex}/, childIndex)
    return unityName
  }

  const parsed = cssParseNodeName(getNodeName(node))
  if (parsed) {
    if (parsed.id) return parsed.id
    if (parsed.name) return parsed.name
    if (parsed.tagName) return parsed.tagName
    if (parsed.classNames && parsed.classNames.length > 0)
      return '.' + parsed.classNames.join('.')
  }

  return nodeName
}