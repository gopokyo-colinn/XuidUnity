import { Color, Rectangle, SceneNode } from 'scenegraph'
import { storage } from 'uxp'
import * as consts from './consts'
import { loadCssRules } from './css'
import {
  getGlobalBounds,
  getRectTransform,
  makeGlobalBoundsRectTransform,
} from './geometry'
import { getNodeNameAndStyle, isRootNode } from './node'
import { GlobalVars } from './globals'

const fs = storage.localFileSystem

function assignAttributeStyle(json, style: {}) {
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

export class UXMLGenerator {
  public json: Object
  public renditions = []

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
    })
    this.addRoot(json, node)
    this.addButton(json, node)

    this.addBounds(json, node)
    this.addRectangle(json, node)

    await this.traverseNodeChildren(json, node)
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

  addRoot(json: Object, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool('root')) {
      return
    }
    Object.assign(json, {
      name:"ui:UXML",
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

  addButton(json: Object, node: SceneNode) {
    const { style } = getNodeNameAndStyle(node)
    if (!style.firstAsBool(consts.STYLE_BUTTON)) return
    Object.assign(json, {
      name: 'ui:Button',
    })
  }

  addBounds(json, node: SceneNode) {
    const rect = getRectTransform(node)
    const bounds = getGlobalBounds(node)
    const parentBounds = getGlobalBounds(node.parent)
    if (rect && rect.fix) {
      assignAttributeStyle(json, {
        position: `absolute`,
      })
      if (rect.fix.width) {
        assignAttributeStyle(json, {
          width: `${bounds.width}px`,
        })
      }
      if (rect.fix.height) {
        assignAttributeStyle(json, {
          height: `${bounds.height}px`,
        })
      }
      const left = rect.fix.left
      if (left === true) {
        assignAttributeStyle(json, {
          left: `${rect.offset_min.x}px`,
        })
      } else if (typeof left == 'number') {
        assignAttributeStyle(json, {
          left: `${left * 100}%`,
        })
      }
      const right = rect.fix.right
      if (right === true) {
        assignAttributeStyle(json, {
          right: `${-rect.offset_max.x}px`,
        })
      } else if (typeof right == 'number') {
        assignAttributeStyle(json, {
          right: `${right * 100}%`,
        })
      }
      const top = rect.fix.top
      if (top === true) {
        assignAttributeStyle(json, {
          top: `${-rect.offset_max.y}px`,
        })
      } else if (typeof top == 'number') {
        assignAttributeStyle(json, {
          top: `${top * 100}%`,
        })
      }
      const bottom = rect.fix.bottom
      if (bottom === true) {
        assignAttributeStyle(json, {
          bottom: `${rect.offset_min.y}px`,
        })
      } else if (typeof bottom == 'number') {
        assignAttributeStyle(json, {
          bottom: `${bottom * 100}%`,
        })
      }
    }
  }

  addRectangle(json, node: SceneNode) {
    if (node instanceof Rectangle) {
      const rect: Rectangle = node as Rectangle
      if (rect.fillEnabled) {
        if (rect.fill instanceof Color) {
          const color = rect.fill as Color
          // console.log('- fill color')
          assignAttributeStyle(json, {
            'background-color': `rgba(${color.r},${color.g},${color.b},${color.a})`,
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
        const color = `rgb(${borderColor.r},${borderColor.g},${borderColor.b})`
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
}
