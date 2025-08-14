# -*- coding: utf-8 -*-
'''

python2.7.x (win32)
install llvm-xxx-win32.exe (http://llvm.org/builds/)
pip install clang

'''

import sys
import os
import re
import glob
from ProtocolCppParser import CppParser, compactSerializeCode, compactSerializeCodeList, enumeratePacketBlocks, readConstants



class VarInfo(object):
	def __init__(self, name, t):
		self.name = name
		self.t = t
		self.typename = t.name
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
		
	def checkIfSerializeTypeFunc(self, s):
		m = re.match(r'P_READ_ST[(](\w+)[)]', s)
		if not m: return False

		n = m.group(1)
		
		if n == self.name:
			return True

		return False

	def checkIfSerializeTypeSerializeInternal(self, s):
		m = re.match(r'P_READ_STX[(](\w+),SerializeInternalFrom[)]', s)
		if not m: return False

		n = m.group(1)
		
		if n == self.name:
			return True

		return False

	def checkIfSerializeTypeBool(self, s):
		m = re.match(r'P_READ_B[(](\w+)[)]', s)
		if not m: return False

		n = m.group(1)
		
		if n == self.name:
			return True

		return False

	def processRWCode(self, s):

		if self.checkIfSerializeTypeAssign(s):
			self.serialize_type = 'assign'

		elif self.checkIfSerializeTypeCopy(s):
			self.serialize_type = 'copy'

		elif self.checkIfSerializeTypeBool(s):
			self.serialize_type = 'bool'

		elif self.checkIfSerializeTypeFunc(s):
			self.serialize_type = 'func'

		elif self.checkIfSerializeTypeSerializeInternal(s):
			self.serialize_type = 'serializeInternal'

	def generate(self):
		d = '%s %s;' % (self.typename, self.name)
		return d, "", ""

class VarInfo_Array(VarInfo):
	def __init__(self, name, t):
		self.name = name
		self.t = t
		self.typename = t.name
		self.serialize_type = ''

	def processRWCode(self, s):
		super(VarInfo_Array, self).processRWCode(s)

		t = self.t

		if t.arrayElementType.name == 'wchar_t':
			m = re.match(r'UNPACK_WSTRINGZ[(]data,(\w+),(\d+)[)]', s)
			if m:
				en, ec = m.groups()
				ec = int(ec)
				if en == self.name and ec == t.arraySize-1:
					self.serialize_type = 'zero_terminated'

	def generate(self):
		t = self.t
		n = self.t.arraySize
		d = '%s[%s] %s;' % (t.arrayElementType.name, n, self.name)

		if self.serialize_type:
			d = '[%s] %s' % (self.serialize_type, d)

		if self.serialize_type == 'copy':
			r = 'P_READ_D(%s, sizeof(%s));' % (self.name, self.name)
			w = 'P_WRITE_D(%s, sizeof(%s));' % (self.name, self.name)

		elif t.arrayElementType.name == 'char':
			assert t.byteSize == n
			r = 'P_READ_S(%s, %s);' % (self.name, n-1)
			w = 'P_WRITE_S(%s, %s);' % (self.name, n-1)
		elif t.arrayElementType.name == 'wchar_t':
			assert t.byteSize == n * 2

			if self.serialize_type == 'zero_terminated':
				r = 'UNPACK_WSTRINGZ(data, %s, %s);' % (self.name, n-1)
				w = 'PACK_WSTRINGZ(buffer, %s, %s);' % (self.name, n-1)
			else:
				r = 'P_READ_WS(%s, %s);' % (self.name, n-1)
				w = 'P_WRITE_WS(%s, %s);' % (self.name, n-1)

		else:
			r = "!!no implementation!!"
			w = "!!no implementation!!"

		return d, r, w



class VarInfo_Pod(VarInfo):
	def __init__(self, name, t):
		self.name = name
		self.t = t
		self.typename = t.name
		self.serialize_type = ''

	def generate(self):
		
		t = self.t
		tn = self.typename
		#if not tn: raise RuntimeError('typename is None!' + self.name);

		if self.serialize_type == 'assign':
			d = '%s %s;' % (tn, self.name)
			r = '%s = UNPACK_TYPE(data, %s);' % (self.name, tn)
			w = 'PACK_TYPE(buffer, %s, %s);' % (self.name, tn)

		elif self.serialize_type == 'copy':
			d = '%s %s;' % (tn, self.name)
			r = 'P_READ_D(&%s, sizeof(%s));' % (self.name, self.name)
			w = 'P_WRITE_D(&%s, sizeof(%s));' % (self.name, self.name)

		elif self.serialize_type == 'bool':
			d = '%s %s;' % (tn, self.name)
			r = 'P_READ_B(%s);' % (self.name,)
			w = 'P_WRITE_B(%s);' % (self.name,)

		elif self.serialize_type == 'func':
			d = '%s %s;' % (tn, self.name)
			r = 'P_READ_ST(%s);' % (self.name,)
			w = 'P_WRITE_ST(%s);' % (self.name,)

		elif t.isPrimitive:
			d = '%s %s;' % (self.typename, self.name)

			if t.encodingName:
				r = 'P_READ_%s(%s);' % (t.encodingName, self.name)
				w = 'P_WRITE_%s(%s);' % (t.encodingName, self.name)
			else:
				r = '%s = UNPACK_TYPE(data, %s);' % (self.name, self.typename)
				w = 'PACK_TYPE(buffer, %s, %s);' % (self.name, self.typename)

		else:
			d = '%s %s;' % (tn, self.name)
			r = '%s = UNPACK_TYPE(data, %s);' % (self.name, tn)
			w = 'PACK_TYPE(buffer, %s, %s);' % (self.name, tn)

		if self.serialize_type:
			d = '[%s] %s' % (self.serialize_type, d)

		return d, r, w

class VarInfo_Record(VarInfo):
	def __init__(self, name, t):
		self.name = name
		self.t = t
		self.typename = t.name
		self.serialize_type = ''

	def generate(self):
		
		tn = self.typename

		d = '%s %s;' % (tn, self.name)
		
		if self.t.isPointer:
			n = '*' + self.name
			p = self.name
		else:
			n = self.name
			p = '&' + self.name

		if self.serialize_type == 'assign':
			r = '%s = UNPACK_TYPE(data, %s);' % (n, tn)
			w = 'PACK_TYPE(buffer, %s, %s);' % (n, tn)

		elif self.serialize_type == 'copy':
			r = 'P_READ_D(%s, sizeof(%s));' % (p, n)
			w = 'P_WRITE_D(%s, sizeof(%s));' % (p, n)

		elif self.serialize_type == 'serializeInternal':
			r = 'P_READ_STX(%s, SerializeInternalFrom);' % (n,)
			w = 'P_WRITE_STX(%s, SerializeInternalTo);' % (n,)

		else:
			r = 'P_READ_ST(%s);' % (n,)
			w = 'P_WRITE_ST(%s);' % (n,)

		if self.serialize_type:
			d = '[%s] %s' % (self.serialize_type, d)

		return d, r, w


class PacketInfo:
	def __init__(self, name):
		self.name = name
		self.variables = []

	def addVariable(self, n, t):

		if t.isArray:
			v = VarInfo_Array(n, t)
		elif t.isRecord or t.isPointer and t.pointee.isRecord:
			v = VarInfo_Record(n, t)
		elif t.isPod:
			v = VarInfo_Pod(n, t)
		else:
			v = VarInfo(n, t)

		self.variables.append(v)

	def processRWCode(self, s2, s3):

		l1 = compactSerializeCodeList(constants, s2)
		l2 = compactSerializeCodeList(constants, s3)

		failCount = 0

		for i in range(len(self.variables)):

			rs0 = ''
			ws0 = ''

			if i < len(l1): rs0 = l1[i]
			if i < len(l2): ws0 = l2[i]

			v = self.variables[i]
			v.processRWCode(rs0)

			_, rs, ws = v.generate()
			rs = compactSerializeCode(constants, rs)
			ws = compactSerializeCode(constants, ws)

			if rs0 != rs or ws0 != ws:
				#print rs0, ws0
				#print rs, ws
				#v.printInfo()
				failCount += 1

		return failCount == 0

	def generate(self):
		d = []
		r = []
		w = []

		for v in self.variables:
			s1, s2, s3 = v.generate()
			if s1: d.append('\t' + s1)
			if s2: r.append('\t\t' + s2)
			if s3: w.append('\t\t' + s3)

		l = []
		l.append('struct ' + self.name)

		if d:
			l.append('\n{\n%s\n}\n' % '\n'.join(d))
		else:
			l.append(' {}\n')

		return ''.join(l)

class ProtocolInlParser:
	def __init__(self, name):
		self.name = name
		self.packets = []
		self.failed = []

	def parse(self, srcFile):
		print srcFile, '...'

		for i in enumeratePacketBlocks(srcFile):
			self.parsePacket(*i)

		#print '\t', len(self.packets), 'packets'

	def parsePacket(self, name, s1, s2, s3):
		#if name != 'DB_SERVER_CUR_USER': return

		p = PacketInfo(name)

		tu = cppParser.parse(s1)

		for n, t, err in tu:
			p.addVariable(n, t)
			if err:
				self.failed.append((self.name, p.name))

		if not p.processRWCode(s2, s3):
			self.failed.append((self.name, p.name))

		self.packets.append(p)
	
	def generate(self, fn):
		with open(fn, 'wb') as f:
			for p in self.packets:
				f.write(p.generate())
				f.write('\n')

def parseProtocolInl(fn, outDir):
	_, n = os.path.split(fn)
	n, _ = os.path.splitext(n)
	p = ProtocolInlParser(n)
	p.parse(fn)
	p.generate(os.path.join(outDir, n + ".pd"))
	return p



if __name__ == "__main__":
	svrDir = '../../server/'
	srcDir = '../../server/common/protocol/'
	varFile = '../../server/common/GameCommon.h'
	outDir = '.'

	cppParser = CppParser(svrDir)

	constants = readConstants(svrDir)

	numPackets = 0
	failed = []

	if len(sys.argv) > 1:
		p = parseProtocolInl(os.path.join(srcDir, sys.argv[1]), outDir)
		numPackets += len(p.packets)
		failed += p.failed

	else:
		for fn in glob.glob( os.path.join(srcDir, '*Protocol.inl') ):
			p = parseProtocolInl(fn, outDir)
			numPackets += len(p.packets)
			failed += p.failed
	
	print '#packets:', numPackets
	print '#fail:', len(failed)
	for i in failed:
		print '\tfailed!', i


