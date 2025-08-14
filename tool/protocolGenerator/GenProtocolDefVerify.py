# -*- coding: utf-8 -*-
import os
import glob
import sys
import PdFile
from GenProtocolCpp import CppGenerator
from ProtocolCppParser import enumeratePacketBlocks, compactSerializeCode, compactSerializeCodeList
from GameCommonCppParser import Constants

def verifyStruct(cppGen, st, s1, s2, s3):
	fail = 0

	l1 = compactSerializeCodeList(constants, s1)
	l2 = compactSerializeCodeList(constants, s2)
	l3 = compactSerializeCodeList(constants, s3)

	dl = []
	rl = []
	wl = []

	for i in range(len(st.variables)):

		v = st.variables[i]

		d, r, w = cppGen.generateVar(v)
		dl.append(compactSerializeCode(constants, d))
		rl.append(compactSerializeCode(constants, r))
		wl.append(compactSerializeCode(constants, w))

	d0 = ''.join(l1)
	r0 = ''.join(l2).replace(';','')
	w0 = ''.join(l3).replace(';','')

	d = ''.join(dl)
	r = ''.join(rl).replace(';','')
	w = ''.join(wl).replace(';','')
	
	if d0 != d or r0 != r or w0 != w:
		print os.path.basename(cppGen.pd.filename), st.name

		if d0 != d:
			print '\toriginal :', d0
			print '\tgenerated:', d

		if r0 != r:
			print '\toriginal :', r0
			print '\tgenerated:', r

		if w0 != w:
			print '\toriginal :', w0
			print '\tgenerated:', w

		fail += 1

	return fail



def verifyFile(filename, srcDir):

	_, n = os.path.split(filename)
	n, _ = os.path.splitext(n)

	inlFile = os.path.join(srcDir, n + '.inl')
	if not os.path.exists(inlFile):
		print 'file not found:', inlFile
		return 1

	pdFile = PdFile.readFile(filename)
	#print n, ':', pdFile.getSize(), 'structs'

	cppGen = CppGenerator(pdFile)

	count = 0
	fail = 0

	for cmt, name, s1, s2, s3 in enumeratePacketBlocks(inlFile):
		count += 1
		st = pdFile.getStruct(name)
		if not st:
			print n, ': struct not found! "%s"' % name
			continue

		f = verifyStruct(cppGen, st, s1, s2, s3)
		if f:
			fail += 1
	
	return fail
	


def verifyFiles(filenames, srcDir):
	fail = 0
	for fn in filenames:
		fail += verifyFile(fn, srcDir)

	print '#failed:', fail


if __name__ == "__main__":
	if len(sys.argv) < 4:
		print "Command Line: python GenProtocolDefVerify.py svrDir protocolDir outDir"
		sys.exit(-1)

	#svrDir = '../../server/'
	#srcDir = '../../server/common/protocol/'
	#outDir = '../../server/common/protocoldef/'
	svrDir = sys.argv[1]
	srcDir = sys.argv[2]
	outDir = sys.argv[3]
	args = sys.argv[4:]
	
	constants = Constants(svrDir)

	if args:
		verifyFiles(args, srcDir)
	else:
		verifyFiles(glob.glob( os.path.join(outDir, '*.pd') ), srcDir)



