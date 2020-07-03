export const STR_CONTENT = "content";
export const STR_VERTICAL = "vertical";
export const STR_HORIZONTAL = "horizontal";
export const STR_PREFERRED = "preferred";
export const STR_GRID = "grid";

export const FIXED_TOP = "fixed-top"
export const FIXED_LEFT = "fixed-left"
export const FIXED_RIGHT = "fixed-right"
export const FIXED_BOTTOM = "fixed-bottom"
export const FIXED_BOTH = "fixed-both"
export const SIZE_FIXED = "size-fixed"

// オプション文字列　全て小文字 数字を含まない
// OPTION名に H V　X Yといった、高さ方向をしめすものはできるだけ出さないようにする
export const STYLE_ALIGN = "align"; // テキストの縦横のアライメントの設定が可能　XDの設定に上書き
export const STYLE_BLANK = "blank";
export const STYLE_BUTTON = "button";
export const STYLE_BUTTON_TRANSITION = "button-transition";
export const STYLE_BUTTON_TRANSITION_TARGET_GRAPHIC =
  "button-transition-target-graphic";
export const STYLE_BUTTON_TRANSITION_HIGHLIGHTED_SPRITE_TARGET =
  "button-transition-highlighted-sprite-target";
export const STYLE_BUTTON_TRANSITION_PRESSED_SPRITE_TARGET =
  "button-transition-pressed-sprite-target";
export const STYLE_BUTTON_TRANSITION_SELECTED_SPRITE_TARGET =
  "button-transition-selected-sprite-target";
export const STYLE_BUTTON_TRANSITION_DISABLED_SPRITE_TARGET =
  "button-transition-disabled-sprite-target";
export const STYLE_CANVAS_GROUP = "canvas-group"; // 削除予定
export const STYLE_COMMENT_OUT = "comment-out";
export const STYLE_COMPONENT = "component";
export const STYLE_CONTENT_SIZE_FITTER = "content-size-fitter"; //自身のSizeFitterオプション
export const STYLE_CONTENT_SIZE_FITTER_HORIZONTAL_FIT =
  "content-size-fitter-horizontal-fit";
export const STYLE_CONTENT_SIZE_FITTER_VERTICAL_FIT =
  "content-size-fitter-vertical-fit";
export const STYLE_MARGIN_FIX = "fix";
export const STYLE_IMAGE = "image";
export const STYLE_IMAGE_SCALE = "image-scale";
export const STYLE_IMAGE_SLICE = "image-slice"; // 9スライス ドット数を指定する
export const STYLE_IMAGE_TYPE = "image-type"; // sliced/tiled/simple/filled
export const STYLE_IMAGE_FIT_PARENT_BOUNDS = "image-fit-parent-bounds"; // 親と同じ大きさで画像を作成する
export const STYLE_LAYER = "layer";
export const STYLE_LAYOUT_ELEMENT = "layout-element";
export const STYLE_LAYOUT_ELEMENT_IGNORE_LAYOUT = "layout-element-ignore-layout";
export const STYLE_LAYOUT_ELEMENT_PREFERRED_WIDTH = "layout-element-preferred-width";
export const STYLE_LAYOUT_ELEMENT_PREFERRED_HEIGHT = "layout-element-preferred-height";
export const STYLE_LAYOUT_GROUP = "layout-group"; //子供を自動的にどうならべるかのオプション
export const STYLE_LAYOUT_GROUP_CHILD_ALIGNMENT = "layout-group-child-alignment";
export const STYLE_LAYOUT_GROUP_CHILD_FORCE_EXPAND = "layout-group-child-force-expand";
export const STYLE_LAYOUT_GROUP_CHILD_CONTROL_SIZE = "layout-group-child-control-size";
export const STYLE_LAYOUT_GROUP_SPACING_X = "layout-group-spacing-x";
export const STYLE_LAYOUT_GROUP_SPACING_Y = "layout-group-spacing-y";
export const STYLE_LAYOUT_GROUP_START_AXIS = "layout-group-start-axis";
export const STYLE_LAYOUT_GROUP_USE_CHILD_SCALE = "layout-group-use-child-scale";
export const STYLE_MATCH_LOG = "match-log";
export const STYLE_PRESERVE_ASPECT = "preserve-aspect";
export const STYLE_LOCK_ASPECT = "lock-aspect"; // preserve-aspectと同じ動作にする　アスペクト比を維持する
export const STYLE_RAYCAST_TARGET = "raycast-target"; // 削除予定
export const STYLE_RECT_MASK_2D = "rect-mask-twod";
export const STYLE_RECT_TRANSFORM_ANCHOR_OFFSET_X = "rect-transform-anchor-offset-x"; // offset-min offset-max anchors-min anchors-maxの順
export const STYLE_RECT_TRANSFORM_ANCHOR_OFFSET_Y = "rect-transform-anchor-offset-y"; // offset-min offset-max anchors-min anchors-maxの順
export const STYLE_RECT_TRANSFORM_ANCHORS_X = "rect-transform-anchors-x"; // anchors-min anchors-maxの順
export const STYLE_RECT_TRANSFORM_ANCHORS_Y = "rect-transform-anchors-y"; // anchors-min anchors-maxの順
export const STYLE_REPEATGRID_ATTACH_TEXT_DATA_SERIES =
  "repeatgrid-attach-text-data-series";
export const STYLE_REPEATGRID_ATTACH_IMAGE_DATA_SERIES =
  "repeatgrid-attach-image-data-series";
export const STYLE_SCROLLBAR = "scrollbar";
export const STYLE_SCROLLBAR_DIRECTION = "scrollbar-direction";
export const STYLE_SCROLLBAR_HANDLE_TARGET = "scrollbar-handle-target";
export const STYLE_SCROLL_RECT = "scroll-rect";
export const STYLE_SCROLL_RECT_CONTENT_NAME = "scroll-rect-content-target";
export const STYLE_SCROLL_RECT_HORIZONTAL_SCROLLBAR_TARGET =
  "scroll-rect-horizontal-scrollbar-target";
export const STYLE_SCROLL_RECT_VERTICAL_SCROLLBAR_TARGET =
  "scroll-rect-vertical-scrollbar-target";
export const STYLE_SLIDER = "slider";
export const STYLE_SLIDER_DIRECTION = "slider-direction";
export const STYLE_SLIDER_FILL_RECT_TARGET = "slider-fill-rect-target";
export const STYLE_SLIDER_HANDLE_RECT_TARGET = "slider-handle-rect-target";
export const STYLE_TEXT = "text";
export const STYLE_TEXTMP = "textmp"; // textmeshpro
export const STYLE_TEXT_STRING = "text-string";
export const STYLE_TOGGLE = "toggle";
export const STYLE_TOGGLE_TRANSITION = "toggle-transition";
export const STYLE_TOGGLE_GRAPHIC_TARGET = "toggle-graphic-target";
export const STYLE_TOGGLE_TRANSITION_TARGET_GRAPHIC_TARGET =
  "toggle-transition-target-graphic-target";
export const STYLE_TOGGLE_TRANSITION_HIGHLIGHTED_SPRITE_TARGET =
  "toggle-transition-highlighted-sprite-target";
export const STYLE_TOGGLE_TRANSITION_PRESSED_SPRITE_TARGET =
  "toggle-transition-pressed-sprite-target";
export const STYLE_TOGGLE_TRANSITION_SELECTED_SPRITE_TARGET =
  "toggle-transition-selected-sprite-target";
export const STYLE_TOGGLE_TRANSITION_DISABLED_SPRITE_TARGET =
  "toggle-transition-disabled-sprite-target";
export const STYLE_TOGGLE_GROUP = "toggle-group";
export const STYLE_INPUT = "input";
export const STYLE_INPUT_TRANSITION = "input-transition";
export const STYLE_INPUT_GRAPHIC_NAME = "input-graphic-target";
export const STYLE_INPUT_TARGET_GRAPHIC_NAME = "input-transition-target-graphic-target";
export const STYLE_INPUT_TRANSITION_HIGHLIGHTED_SPRITE_TARGET =
  "input-transition-highlighted-sprite-target";
export const STYLE_INPUT_TRANSITION_PRESSED_SPRITE_TARGET =
  "input-transition-pressed-sprite-target";
export const STYLE_INPUT_TRANSITION_SELECTED_SPRITE_TARGET =
  "input-transition-selected-sprite-target";
export const STYLE_INPUT_TRANSITION_DISABLED_SPRITE_TARGET =
  "input-transition-disabled-sprite-target";
export const STYLE_INPUT_TEXT_TARGET = "input-text-target";
export const STYLE_INPUT_PLACEHOLDER_TARGET = "input-placeholder-target";
export const STYLE_CREATE_CONTENT = "create-content";
export const STYLE_CREATE_CONTENT_BOUNDS = "create-content-bounds";
export const STYLE_CREATE_CONTENT_NAME = "create-content-name";
export const STYLE_V_ALIGN = "v-align"; //テキストの縦方向のアライメント XDの設定に追記される
export const STYLE_ADD_COMPONENT = "add-component";
export const STYLE_MASK = "mask";
export const STYLE_UNITY_NAME = "unity-name";
export const STYLE_CHECK_LOG = "check-log";
