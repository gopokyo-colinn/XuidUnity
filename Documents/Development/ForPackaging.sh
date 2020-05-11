#!/bin/sh
# FOR PACKAGING

# Samples.xdの同期をする
echo "----- sync samples.xd -----"
SAMPLE1=./UnityProject/Assets/I0plus/XdUnityUI-ForAdobeXD/Samples/
SAMPLE2=./XdPlugins/Samples/
rsync --update -r --exclude=*.meta ${SAMPLE1} ${SAMPLE2}
rsync --update -r --exclude=*.meta ${SAMPLE2} ${SAMPLE1}
echo "done.\n"

# 以下の動作はSyncToolで行うこと
# AdobeXD developフォルダにあるプラグインソースをリポジトリに同期
#echo "----- sync AdobeXD develop plugin folder. -----"
#rsync -av --delete /mnt/c/Users/itouh2/AppData/Local/Packages/Adobe.CC.XD_adky2gkssdxte/LocalState/develop/XdUnityUIExport/ ./XdPlugins/XdUnityUIExport/
#rsync -av --delete /mnt/c/Users/itouh2/AppData/Local/Packages/Adobe.CC.XD_adky2gkssdxte/LocalState/develop/9SliceHelper/ ./XdPlugins/9SliceHelper/
#echo "done.\n"

# リポジトリ内から AdobeXDプラグインファイルを作成する
echo "----- make AdobeXD plugin .xdx file. -----"
(cd ./XdPlugins && zip -q -r XdUnityUIExport.xdx XdUnityUIExport -x \*/_SYNCAPP/*)
(cd ./XdPlugins && zip -q -r 9SliceHelper.xdx 9SliceHelper -x \*/_SYNCAPP/*)
echo "done.\n"

# AdobeXDプラグインをUnityプロジェクト内にコピーする
echo "----- copy .xdx file to Unity Assets. -----"
cp ./XdPlugins/XdUnityUIExport.xdx ./UnityProject/Assets/I0plus/XdUnityUI-ForAdobeXD/XdUnityUIExport.xdx
cp ./XdPlugins/9SliceHelper.xdx ./UnityProject/Assets/I0plus/XdUnityUI-ForAdobeXD/9SliceHelper.xdx
echo "done.\n"

