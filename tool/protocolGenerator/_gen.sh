#!/bin/bash
# 
# python GenProtocolCpp.py srcDir outDir
# 
#   srcDir: folder of protocol definition files(*.pd)
#   outDir: folder of protocol source codes
# 

python2 GenProtocolTs.py ../../game/common/protocoldef ../../../../gs/trunk/gs_a9j/src/protocol/file ../../../../gs/trunk/gs_a9j/src/protocol/index.js GameCommon.pd ErrorCodes.pd ServiceProtocol.pd
python2 GenProtocolTs.py ../../game/common/protocoldef ../../../../fw/trunk/fw_a9j/src/protocol/file ../../../../fw/trunk/fw_a9j/src/protocol/index.js GameCommon.pd ErrorCodes.pd ServiceProtocol.pd

python2 GenProtocolTs.py ../../game/common/protocoldef ../../../../botmanager_api/src/bot/src/casino_core/protocol/file ../../../../botmanager_api/src/bot/src/casino_core/protocol/index.js GameCommon.pd ErrorCodes.pd ServiceProtocol.pd

read -p "Press enter to continue"
