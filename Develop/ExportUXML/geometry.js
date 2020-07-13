"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const consts = require("./consts");
const globals_1 = require("./globals");
const node_1 = require("./node");
const style_1 = require("./style");
const tools_1 = require("./tools");
/**
 * nodeからスケールを考慮したglobalBoundsを取得する
 * Artboardであった場合の、viewportHeightも考慮する
 * ex,eyがつく
 * ハッシュをつかわない
 * @param node
 * @return {{ex: number, ey: number, x: number, width: number, y: number, height: number}}
 */
function getGlobalBounds(node) {
    const bounds = node.globalBounds;
    // Artboardにあるスクロール領域のボーダー
    const viewPortHeight = node.viewportHeight;
    if (viewPortHeight != null)
        bounds.height = viewPortHeight;
    return {
        x: bounds.x * globals_1.GlobalVars.scale,
        y: bounds.y * globals_1.GlobalVars.scale,
        width: bounds.width * globals_1.GlobalVars.scale,
        height: bounds.height * globals_1.GlobalVars.scale,
        ex: (bounds.x + bounds.width) * globals_1.GlobalVars.scale,
        ey: (bounds.y + bounds.height) * globals_1.GlobalVars.scale,
    };
}
exports.getGlobalBounds = getGlobalBounds;
/**
 * nodeからスケールを考慮したglobalDrawBoundsを取得する
 * Artboardであった場合の、viewportHeightも考慮する
 * ex,eyがつく
 * ハッシュをつかわないで取得する
 * Textのフォントサイズ情報など、描画サイズにかかわるものを取得する
 * アートボードの伸縮でサイズが変わってしまうために退避できるように
 */
function getGlobalDrawBounds(node) {
    let bounds = node.globalDrawBounds;
    const viewPortHeight = node.viewportHeight;
    if (viewPortHeight != null)
        bounds.height = viewPortHeight;
    let b = {
        x: bounds.x * globals_1.GlobalVars.scale,
        y: bounds.y * globals_1.GlobalVars.scale,
        width: bounds.width * globals_1.GlobalVars.scale,
        height: bounds.height * globals_1.GlobalVars.scale,
        ex: (bounds.x + bounds.width) * globals_1.GlobalVars.scale,
        ey: (bounds.y + bounds.height) * globals_1.GlobalVars.scale,
    };
    return b;
}
exports.getGlobalDrawBounds = getGlobalDrawBounds;
/**
 * リサイズされる前のグローバル座標とサイズを取得する
 * @param {SceneNode|SceneNodeClass} node
 * @return {{ex: number, ey: number, x: number, width: number, y: number, height: number}}
 */
function getBeforeGlobalBounds(node) {
    const hashBounds = globals_1.GlobalVars.responsiveBounds;
    let bounds = null;
    if (hashBounds != null) {
        const hBounds = hashBounds[node.guid];
        if (hBounds && hBounds.before) {
            bounds = Object.assign({}, hBounds.before.global_bounds);
        }
    }
    if (bounds)
        return bounds;
    console.log('**error** リサイズ前のGlobalBoundsの情報がありません' + node.name);
    return null;
}
exports.getBeforeGlobalBounds = getBeforeGlobalBounds;
function getBeforeTextFontSize(node) {
    const hBounds = globals_1.GlobalVars.responsiveBounds[node.guid];
    return hBounds.before.text_font_size;
}
/**
 * GlobalBoundsでのレスポンシブパラメータの取得
 * @param {SceneNode|SceneNodeClass} node
 */
function getRectTransform(node) {
    const bounds = globals_1.GlobalVars.responsiveBounds[node.guid];
    return bounds ? bounds.responsiveParameterGlobal : null;
}
exports.getRectTransform = getRectTransform;
function calcRect(parentBeforeBounds, beforeBounds, horizontalConstraints, verticalConstraints, style) {
    // console.log(`----------------------${node.name}----------------------`)
    // null：わからない
    // true:フィックスで確定
    // false:フィックスされていないで確定 いずれ数字に変わる
    let styleFixWidth = null;
    let styleFixHeight = null;
    let styleFixTop = null;
    let styleFixBottom = null;
    let styleFixLeft = null;
    let styleFixRight = null;
    const styleFix = style.values(consts.STYLE_FIX);
    if (styleFix != null) {
        // オプションが設定されたら、全ての設定が決まる(NULLではなくなる)
        const fix = style_1.getStyleFix(styleFix);
        // console.log(node.name, 'のfixが設定されました', fix)
        styleFixWidth = fix.width;
        styleFixHeight = fix.height;
        styleFixTop = fix.top;
        styleFixBottom = fix.bottom;
        styleFixLeft = fix.left;
        styleFixRight = fix.right;
    }
    if (!horizontalConstraints || !verticalConstraints) {
        // BooleanGroupの子供,RepeatGridの子供等、情報がとれないものがある
        // console.log(`${node.name} constraints情報がありません`)
    }
    //console.log(`${node.name} constraints`)
    //console.log(horizontalConstraints)
    //console.log(verticalConstraints)
    if (styleFixLeft == null && horizontalConstraints != null) {
        //styleFixLeft = approxEqual(beforeLeft, afterLeft)
        styleFixLeft =
            horizontalConstraints.position === consts.FIXED_LEFT ||
                horizontalConstraints.position === consts.FIXED_BOTH;
    }
    if (styleFixRight == null && horizontalConstraints != null) {
        //styleFixRight = approxEqual(beforeRight, afterRight)
        styleFixRight =
            horizontalConstraints.position === consts.FIXED_RIGHT ||
                horizontalConstraints.position === consts.FIXED_BOTH;
    }
    if (styleFixTop == null && verticalConstraints != null) {
        // styleFixTop = approxEqual(beforeTop, afterTop)
        styleFixTop =
            verticalConstraints.position === consts.FIXED_TOP ||
                verticalConstraints.position === consts.FIXED_BOTH;
    }
    if (styleFixBottom == null && verticalConstraints != null) {
        // styleFixBottom = approxEqual(beforeBottom, afterBottom)
        styleFixBottom =
            verticalConstraints.position === consts.FIXED_BOTTOM ||
                verticalConstraints.position === consts.FIXED_BOTH;
    }
    if (styleFixWidth == null && horizontalConstraints != null) {
        //styleFixWidth = approxEqual(beforeBounds.width, afterBounds.width)
        styleFixWidth = horizontalConstraints.size === consts.SIZE_FIXED;
    }
    if (styleFixHeight == null && verticalConstraints != null) {
        // styleFixHeight = approxEqual(beforeBounds.height, afterBounds.height)
        styleFixHeight = verticalConstraints.size === consts.SIZE_FIXED;
    }
    // ここまでに
    // fixOptionWidth,fixOptionHeight : boolean
    // fixOptionTop,fixOptionBottom : boolean
    // fixOptionLeft,fixOptionRight : boolean
    // true: 固定されている
    // false: 固定されていない
    // number: 辺の位置割合
    // になっていないといけない
    const leftRate = (beforeBounds.x - parentBeforeBounds.x) / parentBeforeBounds.width;
    const rightRate = (parentBeforeBounds.ex - beforeBounds.ex) / parentBeforeBounds.width;
    const topRate = (beforeBounds.y - parentBeforeBounds.y) / parentBeforeBounds.height;
    const bottomRate = (parentBeforeBounds.ey - beforeBounds.ey) / parentBeforeBounds.height;
    // anchorの値を決める
    // null: 定義されていない widthかheightが固定されている
    // number: 親に対しての割合 anchorに割合をいれ､offsetを0
    // true: 固定されている anchorを0か1にし､offsetをピクセルで指定
    // console.log("left:" + fixOptionLeft, "right:" + fixOptionRight)
    // console.log("top:" + fixOptionTop, "bottom:" + fixOptionBottom)
    // console.log("width:" + fixOptionWidth, "height:" + fixOptionHeight)
    let pivot = { x: 0.5, y: 0.5 };
    let offsetMin = {
        x: null,
        y: null,
    }; // left(x), bottom(h)
    let offsetMax = {
        x: null,
        y: null,
    }; // right(w), top(y)
    let anchorMin = { x: null, y: null }; // left, bottom
    let anchorMax = { x: null, y: null }; // right, top
    // レスポンシブパラメータが不確定のままきた場合
    // RepeatGrid以下のコンポーネント,NULLになる
    if (styleFixWidth === null || styleFixHeight === null) {
        // console.log("fix情報がありませんでした", node.name)
        const beforeCenter = beforeBounds.x + beforeBounds.width / 2;
        const parentBeforeCenter = parentBeforeBounds.x + parentBeforeBounds.width / 2;
        anchorMin.x = anchorMax.x =
            (beforeCenter - parentBeforeCenter) / parentBeforeBounds.width + 0.5;
        // サイズを設定　センターからの両端サイズ
        offsetMin.x = -beforeBounds.width / 2;
        offsetMax.x = +beforeBounds.width / 2;
        const beforeMiddle = beforeBounds.y + beforeBounds.height / 2;
        const parentBeforeMiddle = parentBeforeBounds.y + parentBeforeBounds.height / 2;
        anchorMin.y = anchorMax.y =
            -(beforeMiddle - parentBeforeMiddle) / parentBeforeBounds.height + 0.5;
        offsetMin.y = -beforeBounds.height / 2;
        offsetMax.y = +beforeBounds.height / 2;
        return {
            fix: {
                left: styleFixLeft,
                right: styleFixRight,
                top: styleFixTop,
                bottom: styleFixBottom,
                width: styleFixWidth,
                height: styleFixHeight,
            },
            pivot: { x: 0.5, y: 0.5 },
            anchor_min: anchorMin,
            anchor_max: anchorMax,
            offset_min: offsetMin,
            offset_max: offsetMax,
        };
    }
    if (styleFixWidth) {
        // 横幅が固定されている
        // AnchorMin.xとAnchorMax.xは同じ値になる（親の大きさに左右されない）
        //   <-> これが違う値の場合、横幅は親に依存に、それにoffset値を加算した値になる
        //        -> pivotでoffsetの値はかわらない
        // offsetMin.yとoffsetMax.yの距離がHeight
        if (styleFixLeft !== true && styleFixRight !== true) {
            //左右共ロックされていない
            anchorMin.x = anchorMax.x = (leftRate + 1 - rightRate) / 2;
            offsetMin.x = -beforeBounds.width / 2;
            offsetMax.x = beforeBounds.width / 2;
        }
        else if (styleFixLeft === true && styleFixRight !== true) {
            // 親のX座標から､X座標が固定値できまる
            anchorMin.x = 0;
            anchorMax.x = 0;
            offsetMin.x = beforeBounds.x - parentBeforeBounds.x;
            offsetMax.x = offsetMin.x + beforeBounds.width;
        }
        else if (styleFixLeft !== true && styleFixRight === true) {
            // 親のX座標から､X座標が固定値できまる
            anchorMin.x = 1;
            anchorMax.x = 1;
            offsetMax.x = beforeBounds.ex - parentBeforeBounds.ex;
            offsetMin.x = offsetMax.x - beforeBounds.width;
        }
        else {
            // 不確定な設定
            // 1)サイズが固定、左右固定されている
            // 2)サイズが固定されているが、どちらも実数
            // サイズ固定で、位置が親の中心にたいして、絶対値できまるようにする
            // console.log( `${node.name} fix-right(${styleFixRight}) & fix-left(${styleFixLeft}) & fix-width(${styleFixWidth})`)
            anchorMin.x = anchorMax.x = 0.5;
            const parentCenterX = parentBeforeBounds.x + parentBeforeBounds.width / 2;
            const centerX = beforeBounds.x + beforeBounds.width / 2;
            const offsetX = centerX - parentCenterX;
            offsetMin.x = offsetX - beforeBounds.width / 2;
            offsetMax.x = offsetX + beforeBounds.width / 2;
        }
    }
    else {
        if (styleFixLeft === true) {
            // 親のX座標から､X座標が固定値できまる
            anchorMin.x = 0;
            offsetMin.x = beforeBounds.x - parentBeforeBounds.x;
        }
        else {
            anchorMin.x = leftRate;
            offsetMin.x = 0;
        }
        if (styleFixRight === true) {
            // 親のX座標から､X座標が固定値できまる
            anchorMax.x = 1;
            offsetMax.x = beforeBounds.ex - parentBeforeBounds.ex;
        }
        else {
            anchorMax.x = 1 - rightRate;
            offsetMax.x = 0;
        }
    }
    // AdobeXD と　Unity2D　でY軸の向きがことなるため､Top→Max　Bottom→Min
    if (styleFixHeight) {
        // 高さが固定されている
        // AnchorMin.yとAnchorMax.yは同じ値になる（親の大きさに左右されない）
        //   <-> これが違う値の場合、高さは親に依存に、それにoffset値を加算した値になる　つまりpivotでoffsetの値はかわらない
        // offsetMin.yとoffsetMax.yの距離がHeight
        if (styleFixTop !== true && styleFixBottom !== true) {
            //両方共ロックされていない
            anchorMin.y = anchorMax.y = 1 - (topRate + 1 - bottomRate) / 2;
            offsetMin.y = -beforeBounds.height / 2;
            offsetMax.y = beforeBounds.height / 2;
        }
        else if (styleFixTop === true && styleFixBottom !== true) {
            // 親のY座標から､Y座標が固定値できまる
            anchorMax.y = 1;
            anchorMin.y = 1;
            offsetMax.y = -(beforeBounds.y - parentBeforeBounds.y);
            offsetMin.y = offsetMax.y - beforeBounds.height;
        }
        else if (styleFixTop !== true && styleFixBottom === true) {
            // 親のY座標から､Y座標が固定値できまる
            anchorMin.y = 0;
            anchorMax.y = anchorMin.y;
            offsetMin.y = -(beforeBounds.ey - parentBeforeBounds.ey);
            offsetMax.y = offsetMin.y + beforeBounds.height;
        }
        else {
            // 不正な設定
            // サイズが固定されて、上下固定されている
            // 上下共ロックされていない　と同じ設定をする
            anchorMin.y = anchorMax.y = 1 - (topRate + 1 - bottomRate) / 2;
            offsetMin.y = -beforeBounds.height / 2;
            offsetMax.y = beforeBounds.height / 2;
            // 不確定な設定
            // 1)サイズが固定、左右固定されている
            // 2)サイズが固定されているが、どちらも実数
            // サイズ固定で、位置が親の中心にたいして、絶対値できまるようにする
            // console.log(`${node.name} fix-right(${styleFixRight}) & fix-left(${styleFixLeft}) & fix-width(${styleFixWidth})`)
            anchorMin.y = anchorMax.y = 0.5;
            const parentCenterY = parentBeforeBounds.y + parentBeforeBounds.height / 2;
            const centerY = beforeBounds.y + beforeBounds.height / 2;
            const offsetY = -centerY + parentCenterY;
            offsetMin.y = offsetY - beforeBounds.height / 2;
            offsetMax.y = offsetY + beforeBounds.height / 2;
        }
    }
    else {
        if (styleFixTop === true) {
            // 親のY座標から､Y座標が固定値できまる
            anchorMax.y = 1;
            offsetMax.y = -(beforeBounds.y - parentBeforeBounds.y);
        }
        else {
            anchorMax.y = 1 - topRate;
            offsetMax.y = 0;
        }
        if (styleFixBottom === true) {
            // 親のY座標から､Y座標が固定値できまる
            anchorMin.y = 0;
            offsetMin.y = -(beforeBounds.ey - parentBeforeBounds.ey);
        }
        else {
            anchorMin.y = bottomRate;
            offsetMin.y = 0;
        }
    }
    if (style.hasValue(consts.STYLE_FIX, 'c', 'center')) {
        const beforeCenter = beforeBounds.x + beforeBounds.width / 2;
        const parentBeforeCenter = parentBeforeBounds.x + parentBeforeBounds.width / 2;
        anchorMin.x = anchorMax.x =
            (beforeCenter - parentBeforeCenter) / parentBeforeBounds.width + 0.5;
        // サイズを設定　センターからの両端サイズ
        offsetMin.x = -beforeBounds.width / 2;
        offsetMax.x = +beforeBounds.width / 2;
    }
    if (style.hasValue(consts.STYLE_FIX, 'm', 'middle')) {
        const beforeMiddle = beforeBounds.y + beforeBounds.height / 2;
        const parentBeforeMiddle = parentBeforeBounds.y + parentBeforeBounds.height / 2;
        anchorMin.y = anchorMax.y =
            -(beforeMiddle - parentBeforeMiddle) / parentBeforeBounds.height + 0.5;
        offsetMin.y = -beforeBounds.height / 2;
        offsetMax.y = +beforeBounds.height / 2;
    }
    // pivotの設定 固定されている方向にあわせる
    if (styleFixLeft === true && styleFixRight !== true) {
        pivot.x = 0;
    }
    else if (styleFixLeft !== true && styleFixRight === true) {
        pivot.x = 1;
    }
    if (styleFixTop === true && styleFixBottom !== true) {
        pivot.y = 1;
    }
    else if (styleFixTop !== true && styleFixBottom === true) {
        pivot.y = 0;
    }
    return {
        fix: {
            left: styleFixLeft,
            right: styleFixRight,
            top: styleFixTop,
            bottom: styleFixBottom,
            width: styleFixWidth,
            height: styleFixHeight,
        },
        pivot,
        anchor_min: anchorMin,
        anchor_max: anchorMax,
        offset_min: offsetMin,
        offset_max: offsetMax,
    };
}
/**
 * 本当に正確なレスポンシブパラメータは、シャドウなどエフェクトを考慮し、どれだけ元サイズより
 大きくなるか最終アウトプットのサイズを踏まえて計算する必要がある
 calcResonsiveParameter内で、判断する必要があると思われる
 * 自動で取得されたレスポンシブパラメータは､optionの @Pivot @StretchXで上書きされる
 fix: {
      // ロック true or ピクセル数
      left: fixOptionLeft,
      right: fixOptionRight,
      top: fixOptionTop,
      bottom: fixOptionBottom,
      width: fixOptionWidth,
      height: fixOptionHeight,
    },
 anchor_min: anchorMin,
 anchor_max: anchorMax,
 offset_min: offsetMin,
 offset_max: offsetMax,
 * @param {SceneNode|SceneNodeClass} node
 * @param calcDrawBounds
 * @return {null}
 */
function calcDrawRectTransform(node, calcDrawBounds = true) {
    if (!node || !node.parent)
        return null;
    const bounds = globals_1.GlobalVars.responsiveBounds[node.guid];
    if (!bounds || !bounds.before || !bounds.after)
        return null;
    const parentBounds = globals_1.GlobalVars.responsiveBounds[node.parent.guid];
    if (!parentBounds || !parentBounds.before || !parentBounds.after)
        return null;
    const beforeGlobalBounds = bounds.before.global_bounds;
    const beforeGlobalDrawBounds = bounds.before.global_draw_bounds;
    const parentBeforeGlobalBounds = parentBounds.before.content_global_bounds ||
        parentBounds.before.global_bounds;
    const parentBeforeGlobalDrawBounds = parentBounds.before.content_global_draw_bounds ||
        parentBounds.before.global_draw_bounds;
    const afterGlobalBounds = bounds.after.global_bounds;
    const afterGlobalDrawBounds = bounds.after.global_draw_bounds;
    const parentAfterGlobalBounds = parentBounds.after.content_global_bounds || parentBounds.after.global_bounds;
    const parentAfterGlobalDrawBounds = parentBounds.after.content_global_draw_bounds ||
        parentBounds.after.global_draw_bounds;
    const beforeBounds = calcDrawBounds
        ? beforeGlobalDrawBounds
        : beforeGlobalBounds;
    const afterBounds = calcDrawBounds ? afterGlobalDrawBounds : afterGlobalBounds;
    //content_global_boundsは、親がマスク持ちグループである場合、グループ全体のBoundsになる
    const parentBeforeBounds = calcDrawBounds
        ? parentBeforeGlobalDrawBounds
        : parentBeforeGlobalBounds;
    const parentAfterBounds = calcDrawBounds
        ? parentAfterGlobalDrawBounds
        : parentAfterGlobalBounds;
    // fix を取得するため
    // TODO: anchor スタイルのパラメータはとるべきでは
    const style = node_1.getNodeNameAndStyle(node).style;
    const horizontalConstraints = node.horizontalConstraints;
    const verticalConstraints = node.verticalConstraints;
    return calcRect(parentBeforeBounds, beforeBounds, horizontalConstraints, verticalConstraints, style);
}
function calcRectTransform(node) {
    if (!node || !node.parent)
        return null;
    const bounds = globals_1.GlobalVars.responsiveBounds[node.guid];
    if (!bounds || !bounds.before || !bounds.after)
        return null;
    const parentBounds = globals_1.GlobalVars.responsiveBounds[node.parent.guid];
    if (!parentBounds || !parentBounds.before || !parentBounds.after)
        return null;
    const beforeGlobalBounds = bounds.before.global_bounds;
    const parentBeforeGlobalBounds = parentBounds.before.content_global_bounds ||
        parentBounds.before.global_bounds;
    // fix を取得するため
    // TODO: anchor スタイルのパラメータはとるべきでは
    const style = node_1.getNodeNameAndStyle(node).style;
    const horizontalConstraints = node.horizontalConstraints;
    const verticalConstraints = node.verticalConstraints;
    return calcRect(parentBeforeGlobalBounds, beforeGlobalBounds, horizontalConstraints, verticalConstraints, style);
}
class MinMaxSize {
    constructor() {
        this.minWidth = null;
        this.minHeight = null;
        this.maxWidth = null;
        this.maxHeight = null;
    }
    addSize(w, h) {
        if (this.minWidth == null || this.minWidth > w) {
            this.minWidth = w;
        }
        if (this.maxWidth == null || this.maxWidth < w) {
            this.maxWidth = w;
        }
        if (this.minHeight == null || this.minHeight > h) {
            this.minHeight = h;
        }
        if (this.maxHeight == null || this.maxHeight < h) {
            this.maxHeight = h;
        }
    }
}
class CalcBounds {
    constructor() {
        this.sx = null;
        this.sy = null;
        this.ex = null;
        this.ey = null;
    }
    addBoundsParam(x, y, w, h) {
        if (this.sx == null || this.sx > x) {
            this.sx = x;
        }
        if (this.sy == null || this.sy > y) {
            this.sy = y;
        }
        const ex = x + w;
        const ey = y + h;
        if (this.ex == null || this.ex < ex) {
            this.ex = ex;
        }
        if (this.ey == null || this.ey < ey) {
            this.ey = ey;
        }
    }
    /**
     * @param {Bounds} bounds
     */
    addBounds(bounds) {
        this.addBoundsParam(bounds.x, bounds.y, bounds.width, bounds.height);
    }
    /**
     * @returns {Bounds}
     */
    get bounds() {
        return {
            x: this.sx,
            y: this.sy,
            width: this.ex - this.sx,
            height: this.ey - this.sy,
            ex: this.ex,
            ey: this.ey,
        };
    }
}
/**
 * 子供(コンポーネント化するもの･withoutNodeを除く)の全体サイズと
 * 子供の中での最大Width、Heightを取得する
 * 注意：保存されたBounds情報をつかわず、現在のサイズを取得する
 * @param {SceneNode[]} nodes
 * @returns {{node_max_height: number, node_max_width: number, global_bounds: Bounds, global_draw_bounds: Bounds}}
 */
function calcGlobalBounds(nodes) {
    // console.log(`calcGlobalBounds(${nodes})`)
    if (!nodes || nodes.length === 0)
        return {
            global_bounds: null,
            global_draw_bounds: null,
            node_max_width: null,
            node_max_height: null,
        };
    let childrenCalcBounds = new CalcBounds();
    let childrenCalcDrawBounds = new CalcBounds();
    // セルサイズを決めるため最大サイズを取得する
    let childrenMinMaxSize = new MinMaxSize();
    function addNode(node) {
        /* 以下のコードは、nodeが、親のマスクにはいっているかどうかの判定のためのテストコード
        if (!testBounds(viewportBounds, childBounds)) {
          console.log(child.name + 'はViewportにはいっていない')
          return false // 処理しない
        }
         */
        const childBounds = node.globalDrawBounds;
        childrenCalcBounds.addBounds(childBounds);
        const childDrawBounds = node.globalDrawBounds;
        childrenCalcDrawBounds.addBounds(childDrawBounds);
        childrenMinMaxSize.addSize(childBounds.width, childBounds.height);
    }
    for (let node of nodes) {
        addNode(node);
    }
    return {
        global_bounds: childrenCalcBounds.bounds,
        global_draw_bounds: childrenCalcDrawBounds.bounds,
        node_max_width: childrenMinMaxSize.maxWidth * globals_1.GlobalVars.scale,
        node_max_height: childrenMinMaxSize.maxHeight * globals_1.GlobalVars.scale,
    };
}
class GlobalBounds {
    /**
     * @param {SceneNodeClass} node
     */
    constructor(node) {
        if (node == null)
            return;
        this.visible = node.visible;
        this.global_bounds = getGlobalBounds(node);
        this.global_draw_bounds = getGlobalDrawBounds(node);
        // console.log('node.constructor.name:' + node.constructor.name)
        if (node.constructor.name === 'Text') {
            this.text_font_size = node.fontSize;
        }
        if (node_1.hasContentBounds(node)) {
            // Mask（もしくはViewport）をふくむ、含まないで、それぞれのBoundsが必要
            //  マスクありでBoundsが欲しいとき → 全体コンテンツBoundsがほしいとき　とくに、Childrenが大幅にかたよっているときなど
            //  マスク抜きでBoundsが欲しいとき → List内コンテンツのPaddingの計算
            const { style } = node_1.getNodeNameAndStyle(node);
            const contents = node.children.filter(child => {
                return node_1.isContentChild(child);
            });
            const contentBounds = calcGlobalBounds(contents);
            this.content_global_bounds = contentBounds.global_bounds;
            this.content_global_draw_bounds = contentBounds.global_draw_bounds;
            const viewport = getViewport(node);
            if (viewport) {
                const viewportContents = contents.concat(viewport);
                const viewportContentsBounds = calcGlobalBounds(viewportContents);
                this.viewport_content_global_bounds =
                    viewportContentsBounds.global_bounds;
                this.viewport_content_global_draw_bounds =
                    viewportContentsBounds.global_draw_bounds;
            }
        }
    }
}
class BoundsToRectTransform {
    constructor(node) {
        this.node = node;
    }
    updateBeforeBounds() {
        // Before
        this.before = new GlobalBounds(this.node);
    }
    updateAfterBounds() {
        this.after = new GlobalBounds(this.node);
        {
            const beforeX = this.before.global_bounds.x;
            const beforeDrawX = this.before.global_draw_bounds.x;
            const beforeDrawSizeX = beforeDrawX - beforeX;
            const afterX = this.after.global_bounds.x;
            const afterDrawX = this.after.global_draw_bounds.x;
            const afterDrawSizeX = afterDrawX - afterX;
            // global
            if (!tools_1.approxEqual(beforeDrawSizeX, afterDrawSizeX)) {
                console.log(`${this.node.name} ${beforeDrawSizeX -
                    afterDrawSizeX}リサイズ後のBounds.x取得が正確ではないようです`);
                // beforeのサイズ差をもとに、afterを修正する
                this.after.global_draw_bounds.x =
                    this.after.global_bounds.x + beforeDrawSizeX;
            }
        }
        {
            const beforeY = this.before.global_bounds.y;
            const beforeDrawY = this.before.global_draw_bounds.y;
            const beforeDrawSizeY = beforeDrawY - beforeY;
            const afterY = this.after.global_bounds.y;
            const afterDrawY = this.after.global_draw_bounds.y;
            const afterDrawSizeY = afterDrawY - afterY;
            if (!tools_1.approxEqual(beforeDrawSizeY, afterDrawSizeY)) {
                console.log(`${this.node.name} ${beforeDrawSizeY -
                    afterDrawSizeY}リサイズ後のBounds.y取得がうまくいっていないようです`);
                // beforeのサイズ差をもとに、afterを修正する
                this.after.global_draw_bounds.y =
                    this.after.global_bounds.y + beforeDrawSizeY;
            }
        }
        {
            const beforeX = this.before.global_bounds.ex;
            const beforeDrawX = this.before.global_draw_bounds.ex;
            const beforeDrawSizeX = beforeDrawX - beforeX;
            const afterX = this.after.global_bounds.ex;
            const afterDrawX = this.after.global_draw_bounds.ex;
            const afterDrawSizeX = afterDrawX - afterX;
            if (!tools_1.approxEqual(beforeDrawSizeX, afterDrawSizeX)) {
                console.log(`${this.node.name} ${beforeDrawSizeX -
                    afterDrawSizeX}リサイズ後のBounds.ex取得がうまくいっていないようです`);
                // beforeのサイズ差をもとに、afterを修正する
                this.after.global_draw_bounds.ex =
                    this.after.global_bounds.ex + beforeDrawSizeX;
            }
        }
        {
            const beforeY = this.before.global_bounds.ey;
            const beforeDrawY = this.before.global_draw_bounds.ey;
            const beforeDrawSizeY = beforeDrawY - beforeY;
            const afterY = this.after.global_bounds.ey;
            const afterDrawY = this.after.global_draw_bounds.ey;
            const afterDrawSizeY = afterDrawY - afterY;
            if (!tools_1.approxEqual(beforeDrawSizeY, afterDrawSizeY)) {
                console.log(`${this.node.name} ${beforeDrawSizeY -
                    afterDrawSizeY}リサイズ後のBounds.ey取得がうまくいっていないようです`);
                // beforeのサイズ差をもとに、afterを修正する
                this.after.global_draw_bounds.ey =
                    this.after.global_bounds.ey + beforeDrawSizeY;
            }
        }
        this.after.global_draw_bounds.width =
            this.after.global_draw_bounds.ex - this.after.global_draw_bounds.x;
        this.after.global_draw_bounds.height =
            this.after.global_draw_bounds.ey - this.after.global_draw_bounds.y;
    }
    updateRestoreBounds() {
        this.restore = new GlobalBounds(this.node);
    }
    calcRectTransform() {
        // DrawBoundsでのレスポンシブパラメータ(場合によっては不正確)
        this.responsiveParameter = calcDrawRectTransform(this.node);
        // GlobalBoundsでのレスポンシブパラメータ(場合によっては不正確)
        this.responsiveParameterGlobal = calcDrawRectTransform(this.node, false);
    }
}
exports.BoundsToRectTransform = BoundsToRectTransform;
async function makeGlobalBoundsRectTransform(root) {
    // 現在のboundsを取得する
    node_1.traverseNode(root, node => {
        let param = new BoundsToRectTransform(node);
        param.updateBeforeBounds();
        globals_1.GlobalVars.responsiveBounds[node.guid] = param;
    });
    const rootWidth = root.globalBounds.width;
    const rootHeight = root.globalBounds.height;
    // リサイズは大きくなるほうでする
    // リピートグリッドが小さくなったとき、みえなくなるものがでてくる可能性がある
    // そうなると、リサイズ前後での比較ができなくなる
    const resizePlusWidth = 0;
    const resizePlusHeight = 0;
    // rootのリサイズ
    const viewportHeight = root.viewportHeight; // viewportの高さの保存
    // root.resize(rootWidth + resizePlusWidth, rootHeight + resizePlusHeight)
    if (viewportHeight) {
        // viewportの高さを高さが変わった分の変化に合わせる
        root.viewportHeight = viewportHeight + resizePlusHeight;
    }
    // ここでダイアログをだすと、Artboradをひきのばしたところで、どう変化したか見ることができる
    // await fs.getFileForSaving('txt', { types: ['txt'] })
    // 変更されたboundsを取得する
    node_1.traverseNode(root, node => {
        let bounds = globals_1.GlobalVars.responsiveBounds[node.guid] ||
            (globals_1.GlobalVars.responsiveBounds[node.guid] = new BoundsToRectTransform(node));
        bounds.updateAfterBounds();
    });
    // Artboardのサイズを元に戻す
    // root.resize(rootWidth, rootHeight)
    if (viewportHeight) {
        root.viewportHeight = viewportHeight;
    }
    // 元に戻ったときのbounds
    node_1.traverseNode(root, node => {
        globals_1.GlobalVars.responsiveBounds[node.guid].updateRestoreBounds();
    });
    // レスポンシブパラメータの生成
    for (let key in globals_1.GlobalVars.responsiveBounds) {
        globals_1.GlobalVars.responsiveBounds[key].calcRectTransform(); // ここまでに生成されたデータが必要
    }
}
exports.makeGlobalBoundsRectTransform = makeGlobalBoundsRectTransform;
/**
 * Viewportの役割をもつノードを返す
 * Maskをもっている場合はMask
 * @param node
 * @return {null|SceneNode|{show_mask_graphic: boolean}|string|*}
 */
function getViewport(node) {
    const { style } = node_1.getNodeNameAndStyle(node);
    const styleViewport = style.first(consts.STYLE_CREATE_CONTENT);
    if (tools_1.asBool(styleViewport))
        return node;
    if (node.mask)
        return node.mask;
    if (node.constructor.name === 'RepeatGrid')
        return node;
    return null;
}
