import { SceneNode } from "scenegraph";
import * as consts from "./consts";
import { globalCacheNodeNameAndStyle } from "./uxml";

/**
 * @param r
 * @returns {boolean}
 */
export function asBool(r) {
  if (typeof r == 'string') {
    const val = r.toLowerCase()
    if (val === 'false' || val === '0' || val === 'null') return false
  }
  return !!r
}

/**
 * ファイル名につかえる文字列に変換する
 * @param {string} name
 * @param {boolean} convertDot ドットも変換対象にするか
 * @return {string}
 */
export function replaceToFileName(name, convertDot = false) {
  if (convertDot) {
    return name.replace(/[\\/:*?"<>|#\x00-\x1F\x7F\.]/g, '_')
  }
  return name.replace(/[\\/:*?"<>|#\x00-\x1F\x7F]/g, '_')
}


/**
 * 誤差範囲での差があるか
 * epsの値はこのアプリケーション内では共通にする
 * after-bounds before-boundsの変形で誤差が許容範囲と判定したにもかかわらず、
 * 後のcalcRectTransformで許容範囲外と判定してまうなどの事故を防ぐため
 * @param {number} a
 * @param {number} b
 */
export function approxEqual(a, b) {
  const eps = 0.001 // リサイズして元にもどしたとき､これぐらいの誤差がでる
  return Math.abs(a - b) < eps
}


/**
 * @param {[]} values
 * @param {*} checkValues
 * @return {boolean}
 */
export function hasAnyValue(values, ...checkValues) {
  if (!values) {
    return false
  }
  for (let value of values) {
    for (let checkValue of checkValues) {
      if (value === checkValue) return true
    }
  }
  return false
}
