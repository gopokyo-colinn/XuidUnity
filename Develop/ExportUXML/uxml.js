"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const scenegraph_1 = require("scenegraph");
const uxp_1 = require("uxp");
const consts = require("./consts");
const css_1 = require("./css");
const geometry_1 = require("./geometry");
const node_1 = require("./node");
const globals_1 = require("./globals");
const fs = uxp_1.storage.localFileSystem;
function assignAttributeStyle(json, style) {
    if (!json.hasOwnProperty('attributes')) {
        Object.assign(json, { attributes: { style: {} } });
    }
    else {
        if (!json.attributes.hasOwnProperty('style')) {
            Object.assign(json.attributes, { style: {} });
        }
    }
    Object.assign(json.attributes.style, style);
}
// json内、styleを探し出し、文字列に変換する
function convertJsonAttributeStyleToString(key, json) {
    if (json !== null && typeof json == 'object') {
        // styleをみつけた
        if (key === 'style') {
            let styleString = '';
            Object.entries(json).forEach(([key, value]) => {
                styleString += ` ${key}:${value};`;
            });
            return styleString;
        }
        const overwrite = {};
        Object.entries(json).forEach(([key, value]) => {
            // key is either an array index or object key
            const retValue = convertJsonAttributeStyleToString(key, value);
            if (retValue) {
                overwrite[key] = retValue;
            }
        });
        Object.assign(json, overwrite);
    }
    else {
        // jsonObj is a number or string
    }
    return null;
}
class UXMLGenerator {
    constructor() {
        this.renditions = [];
    }
    async traverseNodeChildren(json, node) {
        let numChildren = node.children.length;
        const elements = [];
        for (let i = 0; i < numChildren; i++) {
            let child = node.children.at(i);
            const childJson = {};
            await this.traverseNode(childJson, child);
            elements.push(childJson);
        }
        json.elements = elements;
    }
    async traverseNode(json, node) {
        const result = true;
        // if (result === false) return // 明確なFalseの場合、子供へはいかない
        this.addCustomProperty(node);
        Object.assign(json, {
            type: 'element',
            name: 'ui:VisualElement',
        });
        this.addRoot(json, node);
        this.addButton(json, node);
        this.addBounds(json, node);
        this.addRectangle(json, node);
        await this.traverseNodeChildren(json, node);
    }
    async generate(rootNode) {
        globals_1.GlobalVars.reset();
        globals_1.GlobalVars.rootNode = rootNode;
        console.log('- load css files.');
        globals_1.GlobalVars.cssRules = await css_1.loadCssRules(await fs.getPluginFolder(), 'index.css');
        console.log('- make RectTransform');
        await geometry_1.makeGlobalBoundsRectTransform(rootNode);
        console.log('- traverse node.');
        this.json = {};
        await this.traverseNode(this.json, rootNode);
        convertJsonAttributeStyleToString(null, this.json);
    }
    /**
     * --ではじまる変数定期を登録する
     * RootNodeのときのみ
     */
    addCustomProperty(node) {
        // ToDo: rootノードの場合のみ登録できる
        if (node_1.isRootNode(node))
            return;
        const { style } = node_1.getNodeNameAndStyle(node);
        style.forEach((key, value) => {
            if (key.startsWith('--')) {
                globals_1.GlobalVars.cssVars[key] = value[0];
            }
        });
    }
    addRoot(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool('root')) {
            return;
        }
        Object.assign(json, {
            name: "ui:UXML",
            attributes: {
                'xmlns:ui': 'UnityEngine.UIElements',
                'xmlns:uie': 'UnityEditor.UIElements',
                engine: 'UnityEngine.UIElements',
                editor: 'UnityEditor.UIElements',
                noNamespaceSchemaLocation: '../../UIElementsSchema/UIElements.xsd',
                'editor-extension-mode': 'False',
            },
        });
    }
    addButton(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool(consts.STYLE_BUTTON))
            return;
        Object.assign(json, {
            name: 'ui:Button',
        });
    }
    addBounds(json, node) {
        const rect = geometry_1.getRectTransform(node);
        const bounds = geometry_1.getGlobalBounds(node);
        const parentBounds = geometry_1.getGlobalBounds(node.parent);
        if (rect && rect.fix) {
            assignAttributeStyle(json, {
                position: `absolute`,
            });
            if (rect.fix.width) {
                assignAttributeStyle(json, {
                    width: `${bounds.width}px`,
                });
            }
            if (rect.fix.height) {
                assignAttributeStyle(json, {
                    height: `${bounds.height}px`,
                });
            }
            const left = rect.fix.left;
            if (left === true) {
                assignAttributeStyle(json, {
                    left: `${rect.offset_min.x}px`,
                });
            }
            else if (typeof left == 'number') {
                assignAttributeStyle(json, {
                    left: `${left * 100}%`,
                });
            }
            const right = rect.fix.right;
            if (right === true) {
                assignAttributeStyle(json, {
                    right: `${-rect.offset_max.x}px`,
                });
            }
            else if (typeof right == 'number') {
                assignAttributeStyle(json, {
                    right: `${right * 100}%`,
                });
            }
            const top = rect.fix.top;
            if (top === true) {
                assignAttributeStyle(json, {
                    top: `${-rect.offset_max.y}px`,
                });
            }
            else if (typeof top == 'number') {
                assignAttributeStyle(json, {
                    top: `${top * 100}%`,
                });
            }
            const bottom = rect.fix.bottom;
            if (bottom === true) {
                assignAttributeStyle(json, {
                    bottom: `${rect.offset_min.y}px`,
                });
            }
            else if (typeof bottom == 'number') {
                assignAttributeStyle(json, {
                    bottom: `${bottom * 100}%`,
                });
            }
        }
    }
    addRectangle(json, node) {
        if (node instanceof scenegraph_1.Rectangle) {
            const rect = node;
            if (rect.fillEnabled) {
                if (rect.fill instanceof scenegraph_1.Color) {
                    const color = rect.fill;
                    // console.log('- fill color')
                    assignAttributeStyle(json, {
                        'background-color': `rgba(${color.r},${color.g},${color.b},${color.a})`,
                    });
                }
            }
            if (rect.strokeEnabled) {
                // console.log('- stroke')
                const borderWidth = rect.strokeWidth;
                const widthPixel = borderWidth + 'px';
                assignAttributeStyle(json, {
                    'border-left-width': widthPixel,
                    'border-right-width': widthPixel,
                    'border-top-width': widthPixel,
                    'border-bottom-width': widthPixel,
                });
                const borderColor = rect.stroke;
                const color = `rgb(${borderColor.r},${borderColor.g},${borderColor.b})`;
                assignAttributeStyle(json, {
                    'border-left-color': color,
                    'border-right-color': color,
                    'border-top-color': color,
                    'border-bottom-color': color,
                });
                const radii = rect.cornerRadii;
                if (radii) {
                    assignAttributeStyle(json, {
                        'border-top-left-radius': `${radii.topRight}px`,
                        'border-bottom-left-radius': `${radii.bottomLeft}px`,
                        'border-top-right-radius': `${radii.topRight}px`,
                        'border-bottom-right-radius': `${radii.bottomRight}px`,
                    });
                }
            }
        }
    }
}
exports.UXMLGenerator = UXMLGenerator;
