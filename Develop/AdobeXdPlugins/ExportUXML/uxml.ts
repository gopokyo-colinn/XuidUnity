import {
  RenditionSettings,
  RenditionSettingsPNG,
  RenditionType,
} from 'application'
import { Color, Rectangle, SceneNode, Text } from 'scenegraph'
import { storage } from 'uxp'
import * as consts from './consts'
import { loadCssRules } from './css'
import {
  getGlobalBounds,
  getRectTransform,
  makeGlobalBoundsRectTransform,
} from './geometry'
import { GlobalVars } from './globals'
import { getNodeNameAndStyle, getUnityName, isRootNode } from './node'
import { replaceToFileName } from './tools'

const application = require('application')

const fs = storage.localFileSystem

function assignAttribute(json: ElementJson, attribute: {}) {
  if (!json.hasOwnProperty('attributes')) {
    Object.assign(json, { attributes: { style: {} } })
  }
  Object.assign(json.attributes, attribute)
}

function assignAttributeStyle(json: ElementJson, style: {}) {
  if (!json.hasOwnProperty('attributes')) {
    Object.assign(json, { attributes: { style: {} } })
  } else {
    if (!json.attributes.hasOwnProperty('style')) {
      Object.assign(json.attributes, { style: {} })
    }
  }
  Object.assign(json.attributes.style, style)
}

// json内、styleを探し出し、文字列に変換する
function convertJsonAttributeStyleToString(key: string, json: {}): string {
  if (json !== null && typeof json == 'object') {
    // styleをみつけた
    if (key === 'style') {
      let styleString = ''
      Object.entries(json).forEach(([key, value]) => {
        styleString += ` ${key}:${value};`
      })
      return styleString
    }
    const overwrite = {}
    Object.entries(json).forEach(([key, value]) => {
      // key is either an array index or object key
      const retValue = convertJsonAttributeStyleToString(key, value)
      if (retValue) {
        overwrite[key] = retValue
      }
    })
    Object.assign(json, overwrite)
  } else {
    // jsonObj is a number or string
  }
  return null
}

function getRgba(color: Color) {
  return `rgba(${color.r},${color.g},${color.b},${color.a})`
}

function getRgb(color: Color) {
  return `rgb(${color.r},${color.g},${color.b})`
}

interface ElementJson {
  type: string
  name: string
  attributes: Object & { style: Object }
}

interface AddTypeResult {
  noTraverseChildren?: boolean
}

export class UXMLGenerator {
  public json: Object
  public outputFolder: storage.Folder
  public renditions: RenditionSettings[]

  private async traverseNodeChildren(json, node: SceneNode) {
    let numChildren = node.children.length
    const elements = []
    for (let i = 0; i < numChildren; i++) {
      let child = node.children.at(i)
      const childJson = {}
      await this.traverseNode(childJson, child)
      elements.push(childJson)
    }
    json.elements = elements
  }

  private async traverseNode(json, node: SceneNode) {
    const result = true
    // if (result === false) return // 明確なFalseの場合、子供へはいかない

    this.addCustomProperty(node)

    Object.assign(json, {
      type: 'element',
      name: 'ui:VisualElement',
      attributes: {},
    })
    // Base情報
    this.addName(json, node)
    this.addBounds(json, node)
    this.addRectangle(json, node)

    // Type情報
    this.addRoot(json, node)
    this.addButton(json, node)
    this.addToggle(json, node)
    this.addLabel(json, node)
    this.addSlider(json, node)
    this.addMinMaxSlider(json, node)
    this.addTextField(json, node)
    this.addFoldout(json, node)

    const r = await this.addImage(json, node)
    const noTraverseChildren = r.noTraverseChildren === true

    if (!noTraverseChildren) {
      await this.traverseNodeChildren(json, node)
    }
  }

  public async generate(rootNode: SceneNode) {
    GlobalVars.reset()

    GlobalVars.rootNode = rootNode

    console.log('- load css files.')
    GlobalVars.cssRules = await loadCssRules(
      await fs.getPluginFolder(),
      'index.css',
    )

    console.log('- make RectTransform')
    await makeGlobalBoundsRectTransform(rootNode)

    console.log('- traverse node.')
    this.json = {}
    await this.traverseNode(this.json, rootNode)

    convertJsonAttributeStyleToString(null, this.json)
  }

  /**
   * --ではじまる変数定期を登録する
   * RootNodeのときのみ
   */
  addCustomProperty(node: SceneNode) {
    // ToDo: rootノードの場合のみ登録できる
    if (isRootNode(node)) return
    const { style } = getNodeNameAndStyle(node)
    style.forEach((key, value) => {
      if (key.startsWith('--')) {
        GlobalVars.cssVars[key] = value[0]
      }
    })
  }

  addRoot(json: ElementJson, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool('root')) {
      return
    }
    Object.assign(json, {
      name: 'ui:UXML',
      attributes: {
        'xmlns:ui': 'UnityEngine.UIElements',
        'xmlns:uie': 'UnityEditor.UIElements',
        engine: 'UnityEngine.UIElements',
        editor: 'UnityEditor.UIElements',
        noNamespaceSchemaLocation: '../../UIElementsSchema/UIElements.xsd',
        'editor-extension-mode': 'False',
      },
    })
  }

  addName(json: ElementJson, node: SceneNode) {
    const name = getUnityName(node)
    Object.assign(json.attributes, {
      name,
    })
  }

  addLabel(json: ElementJson, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool(consts.STYLE_LABEL)) return
    Object.assign(json, {
      name: 'ui:Label',
    })
    let textNode: Text = node as Text
    if (textNode.text) {
      assignAttribute(json, {
        text: textNode.text.replace('\n', '&#10;'),
      })
      assignAttributeStyle(json, {
        'white-space': 'nowrap',
        'font-size': textNode.fontSize + 'px',
        color: getRgba(textNode.fill),
      })
      let horizontalAlign: string = ''
      switch (textNode.textAlign) {
        case Text.ALIGN_CENTER:
          horizontalAlign = 'center'
          break
        case Text.ALIGN_LEFT:
          horizontalAlign = 'left'
          break
        case Text.ALIGN_RIGHT:
          horizontalAlign = 'right'
          break
      }
      let verticalAlign = 'middle'
      assignAttributeStyle(json, {
        '-unity-text-align': verticalAlign + '-' + horizontalAlign,
      })
    }
  }

  addButton(json: ElementJson, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool(consts.STYLE_BUTTON)) return
    Object.assign(json, {
      name: 'ui:Button',
    })
  }

  addToggle(json: ElementJson, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool(consts.STYLE_TOGGLE)) return
    Object.assign(json, {
      name: 'ui:Toggle',
    })
  }

  addSlider(json: ElementJson, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool(consts.STYLE_SLIDER)) return
    Object.assign(json, {
      name: 'ui:Slider',
    })
  }

  addMinMaxSlider(json: ElementJson, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool(consts.STYLE_MIN_MAX_SLIDER)) return
    Object.assign(json, {
      name: 'ui:MinMaxSlider',
    })
  }

  addTextField(json: ElementJson, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool(consts.STYLE_TEXT_FIELD)) return
    Object.assign(json, {
      name: 'ui:TextField',
    })
  }

  addFoldout(json: ElementJson, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool(consts.STYLE_FOLDOUT)) return
    Object.assign(json, {
      name: 'ui:Foldout',
    })
  }

  addBounds(json: ElementJson, node: SceneNode) {
    const rect = getRectTransform(node)
    const bounds = getGlobalBounds(node)
    const parentBounds = getGlobalBounds(node.parent)
    if (rect && rect.fix) {
      assignAttributeStyle(json, {
        position: `absolute`,
      })

      // サイズが固定されていて、左右どちらかが固定されているときにサイズ情報出力する
      // 左右どちらも固定されていないときは、margin マイナスハーフサイズ値でサイズ調整する
      let outputWidth: boolean = false
      if (rect.fix.width && (rect.fix.left || rect.fix.right)) {
        assignAttributeStyle(json, {
          width: `${bounds.width}px`,
        })
        outputWidth = true
      }
      let outputHeight: boolean = false
      if (rect.fix.height && (rect.fix.top || rect.fix.bottom)) {
        assignAttributeStyle(json, {
          height: `${bounds.height}px`,
        })
        outputHeight = true
      }

      if (rect.fix.left === true) {
        assignAttributeStyle(json, {
          left: `${rect.offset_min.x}px`,
        })
      } else {
        assignAttributeStyle(json, {
          left: `${rect.anchor_min.x * 100}%`,
        })
        assignAttributeStyle(json, {
          'margin-left': `${rect.offset_min.x}px`,
        })
      }
      if (rect.fix.right === true) {
        assignAttributeStyle(json, {
          right: `${-rect.offset_max.x}px`,
        })
      } else {
        if (!outputWidth) {
          // まだWidthが出力されていない
          // Rightが固定されていない&&高さが固定されていないときのみ
          assignAttributeStyle(json, {
            right: `${(1 - rect.anchor_max.x) * 100}%`,
          })
          assignAttributeStyle(json, {
            'margin-right': `${-rect.offset_max.x}px`,
          })
        }
      }
      if (rect.fix.top === true) {
        assignAttributeStyle(json, {
          top: `${-rect.offset_max.y}px`,
        })
      } else {
        assignAttributeStyle(json, {
          top: `${(1 - rect.anchor_max.y) * 100}%`,
        })
        assignAttributeStyle(json, {
          'margin-top': `${-rect.offset_max.y}px`,
        })
      }
      if (rect.fix.bottom === true) {
        assignAttributeStyle(json, {
          bottom: `${rect.offset_min.y}px`,
        })
      } else if (!outputHeight) {
        // まだHeightが出力されていない
        // Bottomが固定されていない&&高さが固定されていないときのみ
        assignAttributeStyle(json, {
          bottom: `${rect.anchor_min.y * 100}%`,
        })
        assignAttributeStyle(json, {
          'margin-bottom': `${rect.offset_min.y}px`,
        })
      }
    }
  }

  addRectangle(json: ElementJson, node: SceneNode) {
    if (node instanceof Rectangle) {
      const rect: Rectangle = node as Rectangle
      if (rect.fillEnabled) {
        if (rect.fill instanceof Color) {
          const color = rect.fill as Color
          // console.log('- fill color')
          assignAttributeStyle(json, {
            'background-color': getRgba(color),
          })
        }
      }
      if (rect.strokeEnabled) {
        // console.log('- stroke')
        const borderWidth = rect.strokeWidth
        const widthPixel = borderWidth + 'px'
        assignAttributeStyle(json, {
          'border-left-width': widthPixel,
          'border-right-width': widthPixel,
          'border-top-width': widthPixel,
          'border-bottom-width': widthPixel,
        })
        const borderColor: Color = rect.stroke
        const color = getRgb(borderColor)
        assignAttributeStyle(json, {
          'border-left-color': color,
          'border-right-color': color,
          'border-top-color': color,
          'border-bottom-color': color,
        })
        const radii = rect.cornerRadii
        if (radii) {
          assignAttributeStyle(json, {
            'border-top-left-radius': `${radii.topRight}px`,
            'border-bottom-left-radius': `${radii.bottomLeft}px`,
            'border-top-right-radius': `${radii.topRight}px`,
            'border-bottom-right-radius': `${radii.bottomRight}px`,
          })
        }
      }
    }
  }

  async addImage(json: ElementJson, node: SceneNode): Promise<AddTypeResult> {
    // 画像のみのローカルスケール
    // CSSによるコンテンツ書き換えのため、子供を先に処理することもある
    // 親Boundsに合わせて画像出力機能
    // sliceパラメータに対応
    // これ以下のChildTraverseを止める
    const fileName = replaceToFileName(getUnityName(node))

    if (this.outputFolder) {
      const outputFile = await this.outputFolder.createFile(fileName + '.png', {
        overwrite: true,
      })

      let rendition: RenditionSettingsPNG = {
        outputFile,
        scale: 1,
        type: RenditionType.PNG,
        node,
      }

      //TODO:ユニークな名前かチェックする
      this.renditions.push(rendition)
    }

    return {
      noTraverseChildren: true,
    }
  }
}
