"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @param r
 * @returns {boolean}
 */
function asBool(r) {
    if (typeof r == 'string') {
        const val = r.toLowerCase();
        if (val === 'false' || val === '0' || val === 'null')
            return false;
    }
    return !!r;
}
exports.asBool = asBool;
/**
 * ファイル名につかえる文字列に変換する
 * @param {string} name
 * @param {boolean} convertDot ドットも変換対象にするか
 * @return {string}
 */
function replaceToFileName(name, convertDot = false) {
    if (convertDot) {
        return name.replace(/[\\/:*?"<>|#\x00-\x1F\x7F\.]/g, '_');
    }
    return name.replace(/[\\/:*?"<>|#\x00-\x1F\x7F]/g, '_');
}
exports.replaceToFileName = replaceToFileName;
/**
 * 誤差範囲での差があるか
 * epsの値はこのアプリケーション内では共通にする
 * after-bounds before-boundsの変形で誤差が許容範囲と判定したにもかかわらず、
 * 後のcalcRectTransformで許容範囲外と判定してまうなどの事故を防ぐため
 * @param {number} a
 * @param {number} b
 */
function approxEqual(a, b) {
    const eps = 0.001; // リサイズして元にもどしたとき､これぐらいの誤差がでる
    return Math.abs(a - b) < eps;
}
exports.approxEqual = approxEqual;
/**
 * @param {[]} values
 * @param {*} checkValues
 * @return {boolean}
 */
function hasAnyValue(values, ...checkValues) {
    if (!values) {
        return false;
    }
    for (let value of values) {
        for (let checkValue of checkValues) {
            if (value === checkValue)
                return true;
        }
    }
    return false;
}
exports.hasAnyValue = hasAnyValue;
