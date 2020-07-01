#!/bin/sh
# FOR PACKAGING

# 以下の動作はSyncToolで行うこと
# AdobeXD developフォルダにあるプラグインソースをリポジトリに同期
#echo "----- sync AdobeXD develop plugin folder. -----"
#rsync -av --delete /mnt/c/Users/itouh2/AppData/Local/Packages/Adobe.CC.XD_adky2gkssdxte/LocalState/develop/XdUnityUIExport/ ./XdPlugins/XdUnityUIExport/
#rsync -av --delete /mnt/c/Users/itouh2/AppData/Local/Packages/Adobe.CC.XD_adky2gkssdxte/LocalState/develop/9SliceHelper/ ./XdPlugins/9SliceHelper/
#echo "done.\n"

# リポジトリ内から AdobeXDプラグインファイルを作成する
echo "----- make AdobeXD plugin .xdx file. -----"
(cd ../Develop && zip -q -r ../Release/XdUnityUIExport.xdx ./XdUnityUIExport -x \*/types/*)
(cd ../Develop && zip -q -r ../Release/9SliceHelper.xdx ./9SliceHelper -x \*/types/*)
(cd ../Develop && zip -q -r ../Release/RoundRects.xdx ./RoundRects -x \*/types/*)
(cd .. && zip -q -r ./Release/Samples.zip ./Samples)
echo "done.\n"
