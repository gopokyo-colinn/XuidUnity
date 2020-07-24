import { SceneNode } from 'scenegraph'
import { storage } from 'uxp'
import { UXMLGenerator } from './uxml'
import * as tools from './tools'
import { sax } from 'sax/lib/sax'
//const sax = require('./node_modules/sax/lib/sax')
//import {js2xml} from './node_modules/xml-js/lib/js2xml'
const js2xml = require('./node_modules/xml-js/lib/js2xml')
const h = require('./h')

const fs = storage.localFileSystem

function ExportRootsToString(roots: SceneNode[]) {
  // 出力するアートボートの名前リスト
  let artboards = []
  // 出力するレイヤーの名前リスト
  let layers = []

  for (let node of roots) {
    if (node.constructor.name === 'Artboard') {
      artboards.push(node.name)
    } else {
      layers.push(node.name)
    }
  }

  // 名前でソート
  artboards.sort()
  layers.sort()

  let message = ''

  message +=
    artboards.length > 0 ? '[ARTBOARD] ' + artboards.join('\n[ARTBOARD] ') : ''
  message += layers.length > 0 ? '[LAYER] ' + layers.join('\n[LAYER] ') : ''

  return message
}


export async function commandExportUXML(selection: any) {
  console.log('# Export UXML')
  const roots: SceneNode[] = selection.items.filter(item => {
    return true
  })
  const message = ExportRootsToString(roots)
  console.log(message)
  const divStyle = {
    style: {
      flexDirection: 'row',
      alignItems: 'center',
      paddingLeft: '2em',
    },
  }

  let outputFolder: storage.Folder
  let folderElement
  let dialog = h(
    'dialog',
    h(
      'label',
      divStyle,
      h('span', { width: '70' }, 'Folder'),
      (folderElement = h('input', {
        width: '250',
        readonly: true,
        border: 0,
      })),
      h(
        'button',
        {
          async onclick(e) {
            outputFolder = await fs.getFolder()
            if (outputFolder) {
              folderElement.value = outputFolder.nativePath
              console.log(`- output folder: ${folderElement.value}`)
            }
          },
        },
        '...',
      ),
    ),
    h(
      'footer',
      h(
        'button',
        {
          uxpVariant: 'primary',
          onclick(e) {
            dialog.close('Cancel')
          },
        },
        'Cancel',
      ),
      h(
        'button',
        {
          uxpVariant: 'cta',
          type: 'submit',
          onclick(e) {
            dialog.close('Export')
          },
        },
        'Export',
      ),
    ),
  )
  document.body.appendChild(dialog)
  let result = await dialog.showModal()
  if (result != 'Export') return
  console.log('## UXML generate')
  for (let root of roots) {
    console.log(`### ${root.name}`)
    let generator = new UXMLGenerator()
    await generator.generate(root)

    const uxmlJson = {
      declaration: {
        attributes: {
          version: '1.0',
          encoding: 'utf-8',
        },
      },
      elements: [
        {
          ...generator.json
        },
      ],
    }
    const xmlString = js2xml(uxmlJson, { compact: false, spaces: 4 })

    if (outputFolder) {
      console.log('- write file.')
      const fileName = tools.replaceToFileName(root.name + '.uxml')
      const uxmlFile = await outputFolder.createFile(fileName, {
        overwrite: true,
      })
      await uxmlFile.write(xmlString)
    } else {
      console.log(xmlString)
    }

    console.log('- done.')
  }
  console.log('## end process')
}

/*
declare var module: any
module.exports = {
  // コマンドIDとファンクションの紐付け
  commands: {
    commandExportUXML,
  },
}
*/