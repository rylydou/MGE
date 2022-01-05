#!/bin/bash

# # [output] [file url] [line comment prefix] [license url]
# download-with-license() {
# 	wget -O $1 $4
# 	sed -i 's,^,'$3' ,' $1
# 	wget $2 -O - >>$1
# }

wget https://raw.githubusercontent.com/gabomdq/SDL_GameControllerDB/master/gamecontrollerdb.txt -O MGE/Assets/Mappings.csv
