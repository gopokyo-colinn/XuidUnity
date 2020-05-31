#!/bin/sh
FFMPEG=/mnt/c/iola-user/local/ffmpeg-20200306-cfd9a65-win64-static/bin/ffmpeg.exe

for FILE in *.mp4
do
	FILENAME=`echo ${FILE} | sed 's/\.[^\.]*$//'`
	echo ${FILENAME}
	${FFMPEG} -i ${FILENAME}.mp4 -filter_complex "[0:v] fps=10,scale=640:-1,split [a][b];[a] palettegen [p];[b][p] paletteuse" ${FILENAME}.gif -y
done

