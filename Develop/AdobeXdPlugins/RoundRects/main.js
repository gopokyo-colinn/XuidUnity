const { selection } = require('scenegraph')
const { Rectangle } = require('scenegraph')

/**
 * func : node => {}  nodeを引数とした関数
 * @param {SceneNode|SceneNodeClass} node
 * @param {*} func
 */
function traverseNode(node, func) {
  let result = func(node)
  if (result === false) return // 明確なFalseの場合、子供へはいかない
  node.children.forEach(child => {
    traverseNode(child, func)
  })
}

/**
 * @param {} node
 * @return {Rectangle[]}
 */
function getChildRectangles(node) {
  const children = []
  traverseNode(node, child => {
    if (child instanceof Rectangle && selection.isInEditContext(child)) {
      children.push(child)
    }
  })
  return children
}

/**
 * @type {{rect:Rectangle,cornerRadii:{}}[]}
 */
let editRects = []

function applyValue(scaleSliderValue, additionalSliderValue) {
  for (let editRect of editRects) {
    const scale = scaleSliderValue / 100.0
    const add = additionalSliderValue
    const srcRadii = editRect.cornerRadii

    let topLeft = srcRadii.topLeft * scale + add
    topLeft = topLeft < 0 ? 0 : topLeft
    let topRight = srcRadii.topRight * scale + add
    topRight = topRight < 0 ? 0 : topRight
    let bottomLeft = srcRadii.bottomLeft * scale + add
    bottomLeft = bottomLeft < 0 ? 0 : bottomLeft
    let bottomRight = srcRadii.bottomRight * scale + add
    bottomRight = bottomRight < 0 ? 0 : bottomRight

    const newRadii = {
      topLeft,
      topRight,
      bottomLeft,
      bottomRight,
    }
    // console.log(newRadii)
    editRect.rect.cornerRadii = newRadii
  }
}

let panel
let scaleSlider
let additionalSlider

function resetSliders() {
  if (scaleSlider) scaleSlider.value = 100
  if (additionalSlider) additionalSlider.value = 0
}

function create() {
  const HTML = `<style>
            .break {
                flex-wrap: wrap;
            }
            .spread > span {
                color: #8E8E8E;
                width: 20px;
                text-align: right;
                font-size: 9px;
            }
            label.row input {
                flex: 1 1 auto;
            }
            .show {
                display: block;
            }
            .hide {
                display: none;
            }
            .spread { 
              justify-content: space-between; 
            }
        </style>
        <form method="dialog" id="main">
            <div>
                <h2>Corner Radius</h2>
                <div class="row">
                    <span>Scale:</span>
                    <span class="scale_value" >100%</span>
                </div>
                <div class="row spread">
                    <span>0%</span>
                    <span>100%</span>
                    <span>200%</span>
                </div>
                <label class="row spread">
                  <input class="scale_slider" type="range" min=0 max=200 step=1 value=100 />
                </label>
                <div class="row">
                    <span>Addition:</span>
                    <span class="additional_value">0</span>
                </div>
                <div class="row spread">
                    <span>-100</span>
                    <span>100</span>
                </div>
                <label class="row spread">
                  <input class="additional_slider" type="range" min=-100 max=100 step=1 value=0 />
                </label>
            </div>
            <footer>
            <button class="reset_button" uxp-variant="secondary">Reset Values</button>
            <button id="ok" type="submit" uxp-variant="cta">Apply</button>
            </footer>
        </form>
        <p id="warning">This plugin requires you to select a rectangle or group include rectangles.</p>
        `

  function increaseRectangleSize() {
    const { editDocument } = require('application')

    editDocument({ editLabel: 'Round Rects' }, function(selection) {
      const scale = parseFloat(scaleSlider.value)
      const add = parseFloat(additionalSlider.value)
      applyValue(scale, add)
    })
  }

  panel = document.createElement('div')
  panel.innerHTML = HTML
  panel.querySelector('form').addEventListener('submit', increaseRectangleSize)

  scaleSlider = panel.getElementsByClassName('scale_slider')[0]
  const scaleValue = panel.getElementsByClassName('scale_value')[0]
  scaleSlider.addEventListener('change', function() {
    const scale = parseFloat(scaleSlider.value)
    scaleValue.innerText = Math.round(scale * 100) / 100 + '%'
  })

  additionalSlider = panel.getElementsByClassName('additional_slider')[0]
  const additionalValue = panel.getElementsByClassName('additional_value')[0]
  additionalSlider.addEventListener('change', function() {
    const add = parseFloat(additionalSlider.value)
    additionalValue.innerText = (Math.round(add * 100) / 100).toString()
  })

  const resetButton = panel.getElementsByClassName('reset_button')[0]
  resetButton.addEventListener('click', function() {
    console.log("reset values")
    resetSliders()
  })

  return panel
}

function show(event) {
  if (!panel) event.node.appendChild(create())
}

/**
 * update時に、違うものが選択されたか、そのままか判定する用の文字列
 * @type {null}
 */
let selectionString = null

function update() {
  let form = document.querySelector('form')
  let warning = document.querySelector('#warning')

  /**
   * @type {Rectangle[]}
   */
  let rects = []
  if (selection && selection.items.length > 0) {
    for (let item of selection.items) {
      rects = rects.concat(getChildRectangles(item))
    }
  }

  if (rects.length === 0) {
    form.className = 'hide'
    warning.className = 'show'
    selectionString = null
    editRects = null
    resetSliders()
    console.log('reset editRects')
  } else {
    form.className = 'show'
    warning.className = 'hide'

    const newEditRects = []
    for (let rect of rects) {
      const info = {
        rect,
        cornerRadii: {},
      }
      Object.assign(info.cornerRadii, rect.cornerRadii)
      newEditRects.push(info)
    }

    let newSelectionString = ""
    for (let newEditRect of newEditRects) {
      newSelectionString += newEditRect.rect.guid
    }
    // console.log(newSelectionString)

    if (selectionString === newSelectionString) {
      console.log('same editRects')
    } else {
      editRects = newEditRects
      selectionString = newSelectionString
      resetSliders()
      console.log('new editRects')
    }
  }
}

module.exports = {
  panels: {
    enlargeRectangle: {
      show,
      update,
    },
  },
}
