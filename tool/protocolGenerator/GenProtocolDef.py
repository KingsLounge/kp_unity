# -*- coding: utf-8 -*-
import sys
import os
import re
import glob
from ProtocolCppParser import CppParser, compactSerializeCodeList, enumeratePacketBlocks
from GameCommonCppParser import Constants
from GenProtocolCpp import getEncodingOfPrimitiveType
import ProtocolEnumCppParser



class VarInfo(object):
	def __init__(self, name, t):
		self.name = name
		self.t = t
		self.typename = t.name
		self.comment = ''
		self.serialize_type = ''

	def printInfo(self):
		print self, self.name, self.typename
		print '\t serialize_type:', self.serialize_type
		self.t.printInfo()

	def checkIfSerializeTypeCopy(self, s):
		m = re.match(r'P_READ_D[(]([&]?)(\w+),sizeof[(](\w+)[)][)]', s)
		if not m: return False

		p, n, t = m.groups()

		if (self.t.isPointer or self.t.isArray) and p: return False
		if not (self.t.isPointer or self.t.isArray) and not p: return False

		if n == self.name and (t == self.typename or t == self.name):
			return True

		return False

	def checkIfSerializeTypeAssign(self, s):
		m = re.match(r'(\w+)\s*[=]\s*UNPACK_TYPE[(]data,\s*(\w+)[)]', s)
		if not m: return False

		n, t = m.groups()
		
		if n == self.name and t == self.typename:
			return True

		return False
	
	def checkIfSerializeTypeSerializeInternal(self, s):
		m = re.match(r'P_READ_STX[(](\w+),SerializeInternalFrom[)]', s)
		if not m: return False

		n = m.group(1)
		
		if n == self.name:
			return True

		return False

	def checkIfSerializeType_P_READ_XXX(self, s):
		m = re.match(r'P_READ_(\w+)[(][*]?(\w+)[)]', s)
		if not m: return

		t, n = m.groups()
		
		if n == self.name:
			return t

	def processRWCode(self, s):

		if self.comment:
			m = re.match(r'\[([^\]]+)\]', self.comment)
			if m:
				self.serialize_type = m.group(1)
				self.comment = ''
				return

		if self.checkIfSerializeTypeAssign(s):
			self.serialize_type = 'assign'

		elif self.checkIfSerializeTypeCopy(s):
			self.serialize_type = 'copy'

		elif self.checkIfSerializeTypeSerializeInternal(s):
			self.serialize_type = 'serializeInternal'

		else:
			t = self.checkIfSerializeType_P_READ_XXX(s)
			if t:
				self.serialize_type = t


class VarInfo_Array(VarInfo):
	def __init__(self, name, t, cmt):
		self.name = name
		self.t = t
		self.comment = cmt
		self.typename = t.name
		self.serialize_type = ''

	def processRWCode(self, s):
		super(VarInfo_Array, self).processRWCode(s)

		t = self.t

		if t.arrayElementType == 'wchar':
			m = re.match(r'UNPACK_WSTRINGZ[(]data,(\w+),(\d+)[)]', s)
			if m:
				en, ec = m.groups()
				ec = int(ec)
				if en == self.name and ec == t.arraySize-1:
					self.serialize_type = 'zero_terminated'

	def generate(self):
		t = self.t
		d = '%s %s;' % (t.name, self.name)

		if self.serialize_type:
			d = '[%s] %s' % (self.serialize_type, d)

		if self.comment:
			d = '%s #%s' % (d, self.comment)

		return d


class VarInfo_Record(VarInfo):
	def __init__(self, name, t, cmt):
		self.name = name
		self.t = t
		self.comment = cmt
		self.typename = t.name
		self.serialize_type = ''

	def canSkipSerializeType(self):
		if not self.serialize_type: return True

		enc, isPrim = getEncodingOfPrimitiveType(constants.resolveTypedefs(self.typename))
		if self.serialize_type == enc: return True
		if self.serialize_type == 'assign' and isPrim: return True
		if self.serialize_type == 'ST' and not isPrim: return True

		return False

	def generate(self):
		
		d = '%s %s;' % (self.typename, self.name)
		
		if not self.canSkipSerializeType():
			d = '[%s] %s' % (self.serialize_type, d)

		if self.comment:
			d = '%s #%s' % (d, self.comment)

		return d


class PacketInfo:
	def __init__(self, name, comment):
		self.name = name
		self.comment = comment
		self.variables = []

	def addVariable(self, n, t, cmt):

		if t.isArray:
			v = VarInfo_Array(n, t, cmt)
		else:
			v = VarInfo_Record(n, t, cmt)

		self.variables.append(v)

	def processRWCode(self, s2, s3):

		l1 = compactSerializeCodeList(constants, s2)

		for i in range(len(self.variables)):

			rs0 = ''
			if i < len(l1): rs0 = l1[i]

			v = self.variables[i]
			v.processRWCode(rs0)


	def generate(self):
		d = []

		for v in self.variables:
			s1 = v.generate()
			if s1: d.append('\t' + s1)

		l = []

		if self.comment:
			cmt = self.comment.rstrip()
			cmt = '#' + '\n#'.join(cmt.split('\n'))
			l.append(cmt)
			l.append('\n')

		l.append('struct ' + self.name)

		if d:
			l.append('\n{\n%s\n}\n' % '\n'.join(d))
		else:
			l.append(' {}\n')

		return ''.join(l)

class ProtocolInlParser:
	def __init__(self, name, codes):
		self.name = name
		self.codes = codes
		self.includes = []
		self.packets = []
		self.failed = []

	def parse(self, srcFile):
		print srcFile, '...'

		with open(srcFile, 'rb') as f:
			for m in re.finditer(r'#include\s+"([^\n]*)"', f.read()):
				self.includes.append(m.group(1))

		for i in enumeratePacketBlocks(srcFile):
			self.parsePacket(*i)

		#print '\t', len(self.packets), 'packets'

	def parsePacket(self, comment, name, s1, s2, s3):
		#if name != 'DB_SERVER_CUR_USER': return

		p = PacketInfo(name, comment)

		l, err = cppParser.parse(s1)

		if err:
			self.failed.append((self.name, p.name))

		for n, t, cmt in l:
			p.addVariable(n, t, cmt)

		p.processRWCode(s2, s3)

		self.packets.append(p)
	
	def generate(self, fn):
		lines = []

		lines.append('#\n')
		lines.append('# PD file\n')
		lines.append('#\n')
		lines.append('import GameCommon\n\n')

		for i in self.includes:
			lines.append('''insert_code cpp '#include "%s"'\n''' % i)

		lines.append('\n')

		for p, l in self.codes.groups:
			if l:
				_, code = l[0]
				code = code >> 16
				lines.append('protocol %s(0x%x)\n{\n' % (p, code))
				for n, v in l:
					lines.append('\tcode %-30s = %d;\n' % (n, v & 0xffff))
				lines.append('}\n')

		lines.append('\n')

		curGroup = ''

		for p in self.packets:

			group, code = self.codes.findProtocol(p.name)

			if group and group != curGroup:
				#print 'protocol:', group
				if curGroup: lines.append('}\n\n')
				lines.append('protocol %s\n' % (group))
				lines.append('{\n')

			s = p.generate()

			if group:
				s = '\t' + s.replace('\n', '\n\t')

			lines.append(s)
			lines.append('\n')

			curGroup = group

		if curGroup:
			lines.append('}\n')

		with open(fn, 'wb') as f:
			s = ''.join(lines).replace('\n', '\r\n')
			f.write('\xef\xbb\xbf')
			f.write(unicode(s, 'mbcs').encode('utf8'))


def genProtocolDef(fn, outDir):
	dn, n = os.path.split(fn)
	name, _ = os.path.splitext(n)

	codes = ProtocolEnumCppParser.Codes()

	hFile = os.path.join(dn, name + '.h')
	if os.path.exists(hFile):
		ProtocolEnumCppParser.Parser(codes).parseFile(hFile)

	p = ProtocolInlParser(name, codes)
	p.parse(fn)

	p.generate(os.path.join(outDir, name + ".pd"))
	return p


# --------------------------------------------------------------------------------


if __name__ == "__main__":
	if len(sys.argv) < 4:
		print "Command Line: python GenProtocolDef.py svrDir protocolDir outDir"
		sys.exit(-1)

	#svrDir = '../../server/'
	#srcDir = '../../server/common/protocol/'
	#outDir = '../../server/common/protocoldef/'
	svrDir = sys.argv[1]
	srcDir = sys.argv[2]
	outDir = sys.argv[3]
	args = sys.argv[4:]

	constants = Constants(svrDir)
	constants.generatePdFile(outDir)

	cppParser = CppParser(constants)

	numPackets = 0
	failed = []

	if args:
		p = genProtocolDef(os.path.join(srcDir, args[0]), outDir)
		numPackets += len(p.packets)
		failed += p.failed

	else:
		for fn in glob.glob( os.path.join(srcDir, '*Protocol.inl') ):
			_, n = os.path.split(fn)
			if n.startswith('handler_'): continue

			p = genProtocolDef(fn, outDir)
			numPackets += len(p.packets)
			failed += p.failed
	
	print '#packets:', numPackets
	print '#fail:', len(failed)
	for i in failed:
		print '\tfailed!', i


