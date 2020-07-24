"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class GlobalVars {
    static reset() {
        this.scale = 1.0;
        this.cssRules = null;
        this.cssVars = {};
        this.rootNode = null;
        this.cacheNodeNameAndStyle = {};
        this.responsiveBounds = {};
        this.cacheParseNodeName = {};
    }
}
exports.GlobalVars = GlobalVars;
GlobalVars.scale = 1.0;
GlobalVars.cssRules = null;
GlobalVars.cssVars = {};
GlobalVars.cacheNodeNameAndStyle = {};
// レスポンシブパラメータを保存する
GlobalVars.responsiveBounds = {};
GlobalVars.cacheParseNodeName = {};
