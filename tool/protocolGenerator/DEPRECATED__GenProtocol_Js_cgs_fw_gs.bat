@echo off
rem
rem python GenProtocolCpp.py srcDir outDir
rem
rem   srcDir: folder of protocol definition files(*.pd)
rem   outDir: folder of protocol source codes
rem

python2 GenProtocolTs.py ../../game/common/protocoldef ../../../../gs/trunk/gs_a9j/src/protocol/file ../../../../gs/trunk/gs_a9j/src/protocol/index.js GameCommon.pd ErrorCodes.pd ServiceProtocol.pd

python2 GenProtocolTs.py ../../game/common/protocoldef ../../../../fw/trunk/fw_a9j/src/protocol/file ../../../../fw/trunk/fw_a9j/src/protocol/index.js GameCommon.pd ErrorCodes.pd ServiceProtocol.pd

pause
