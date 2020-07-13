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
function assignAttribute(json, attribute) {
    if (!json.hasOwnProperty('attributes')) {
        Object.assign(json, { attributes: { style: {} } });
    }
    Object.assign(json.attributes, attribute);
}
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
function getRgba(color) {
    return `rgba(${color.r},${color.g},${color.b},${color.a})`;
}
function getRgb(color) {
    return `rgb(${color.r},${color.g},${color.b})`;
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
            attributes: {},
        });
        this.addRoot(json, node);
        this.addName(json, node);
        this.addButton(json, node);
        this.addToggle(json, node);
        this.addLabel(json, node);
        this.addSlider(json, node);
        this.addMinMaxSlider(json, node);
        this.addTextField(json, node);
        this.addFoldout(json, node);
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
            name: 'ui:UXML',
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
    addName(json, node) {
        const name = node_1.getUnityName(node);
        Object.assign(json.attributes, {
            name,
        });
    }
    addLabel(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool(consts.STYLE_LABEL))
            return;
        Object.assign(json, {
            name: 'ui:Label',
        });
        let textNode = node;
        if (textNode.text) {
            assignAttribute(json, {
                text: textNode.text.replace('\n', '&#10;'),
            });
            assignAttributeStyle(json, {
                'white-space': 'nowrap',
                'font-size': textNode.fontSize + 'px',
                color: getRgba(textNode.fill),
            });
            let horizontalAlign = '';
            switch (textNode.textAlign) {
                case scenegraph_1.Text.ALIGN_CENTER:
                    horizontalAlign = 'center';
                    break;
                case scenegraph_1.Text.ALIGN_LEFT:
                    horizontalAlign = 'left';
                    break;
                case scenegraph_1.Text.ALIGN_RIGHT:
                    horizontalAlign = 'right';
                    break;
            }
            let verticalAlign = 'middle';
            assignAttributeStyle(json, {
                '-unity-text-align': verticalAlign + '-' + horizontalAlign,
            });
        }
    }
    addButton(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool(consts.STYLE_BUTTON))
            return;
        Object.assign(json, {
            name: 'ui:Button',
        });
    }
    addToggle(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool(consts.STYLE_TOGGLE))
            return;
        Object.assign(json, {
            name: 'ui:Toggle',
        });
    }
    addSlider(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool(consts.STYLE_SLIDER))
            return;
        Object.assign(json, {
            name: 'ui:Slider',
        });
    }
    addMinMaxSlider(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool(consts.STYLE_MIN_MAX_SLIDER))
            return;
        Object.assign(json, {
            name: 'ui:MinMaxSlider',
        });
    }
    addTextField(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool(consts.STYLE_TEXT_FIELD))
            return;
        Object.assign(json, {
            name: 'ui:TextField',
        });
    }
    addFoldout(json, node) {
        const { style } = node_1.getNodeNameAndStyle(node);
        if (!style.firstAsBool(consts.STYLE_FOLDOUT))
            return;
        Object.assign(json, {
            name: 'ui:Foldout',
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
            // サイズが固定されていて、左右どちらかが固定されているときにサイズ情報出力する
            // 左右どちらも固定されていないときは、margin マイナスハーフサイズ値でサイズ調整する
            let outputWidth = false;
            if (rect.fix.width && (rect.fix.left || rect.fix.right)) {
                assignAttributeStyle(json, {
                    width: `${bounds.width}px`,
                });
                outputWidth = true;
            }
            let outputHeight = false;
            if (rect.fix.height && (rect.fix.top || rect.fix.bottom)) {
                assignAttributeStyle(json, {
                    height: `${bounds.height}px`,
                });
                outputHeight = true;
            }
            if (rect.fix.left === true) {
                assignAttributeStyle(json, {
                    left: `${rect.offset_min.x}px`,
                });
            }
            else {
                assignAttributeStyle(json, {
                    left: `${rect.anchor_min.x * 100}%`,
                });
                assignAttributeStyle(json, {
                    'margin-left': `${rect.offset_min.x}px`,
                });
            }
            if (rect.fix.right === true) {
                assignAttributeStyle(json, {
                    right: `${-rect.offset_max.x}px`,
                });
            }
            else {
                if (!outputWidth) {
                    // まだWidthが出力されていない
                    // Rightが固定されていない&&高さが固定されていないときのみ
                    assignAttributeStyle(json, {
                        right: `${(1 - rect.anchor_max.x) * 100}%`,
                    });
                    assignAttributeStyle(json, {
                        'margin-right': `${-rect.offset_max.x}px`,
                    });
                }
            }
            if (rect.fix.top === true) {
                assignAttributeStyle(json, {
                    top: `${-rect.offset_max.y}px`,
                });
            }
            else {
                assignAttributeStyle(json, {
                    top: `${(1 - rect.anchor_max.y) * 100}%`,
                });
                assignAttributeStyle(json, {
                    'margin-top': `${-rect.offset_max.y}px`,
                });
            }
            if (rect.fix.bottom === true) {
                assignAttributeStyle(json, {
                    bottom: `${rect.offset_min.y}px`,
                });
            }
            else if (!outputHeight) {
                // まだHeightが出力されていない
                // Bottomが固定されていない&&高さが固定されていないときのみ
                assignAttributeStyle(json, {
                    bottom: `${rect.anchor_min.y * 100}%`,
                });
                assignAttributeStyle(json, {
                    'margin-bottom': `${rect.offset_min.y}px`,
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
                        'background-color': getRgba(color),
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
                const color = getRgb(borderColor);
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
