#!/bin/bash

# python GenProtocolCs.py srcDir outDir
#   srcDir: folder of protocol definition files(*.pd)
#   outDir: folder of protocol source codes

python2 GenProtocolCs.py ../../game/common/protocoldef ~/work/kingslounge/git/kings/KingsPoker_unity/Assets/Script/Protocol

read -p "Press [Enter] key to continue..."