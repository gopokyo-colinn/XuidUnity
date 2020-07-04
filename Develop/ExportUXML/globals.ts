// 全体にかけるスケール
import { SceneNode } from 'scenegraph'
import { BoundsToRectTransform } from './geometry'

export class GlobalVars {
  public static scale: number = 1.0

  public static cssRules: any = null

  public static cssVars = {}

  // コンバート中のRoot
  public static rootNode: SceneNode

  public static cacheNodeNameAndStyle = {}

  // レスポンシブパラメータを保存する
  public static responsiveBounds: {[key:string]:BoundsToRectTransform} = {}

  public static cacheParseNodeName = {}

  public static reset() {
    this.scale = 1.0
    this.cssRules = null
    this.cssVars = {}
    this.rootNode = null
    this.cacheNodeNameAndStyle = {}
    this.responsiveBounds = {}
    this.cacheParseNodeName = {}
  }
}

