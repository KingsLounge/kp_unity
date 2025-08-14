# -*- coding: utf-8 -*-
import os
import re
import sys
import glob
import PdFile


def getArraySizeExprOf(t, delta=0):
	if t.arraySizeExpr:
		if delta == 0:
			s = t.arraySizeExpr
			if '+' in s: s = '(%s)' % s
			return '(int)' + s
		elif delta == -1 and t.arraySizeExpr.endswith('+1'):
			s = t.arraySizeExpr[:-2]
			if '+' in s: s = '(%s)' % s
			return '(int)' + s

	return str(t.arraySize + delta)

def getCsTypeName(t, attr=None):
	n = t.name

	m = {
		'bool':'bool',
		'char':'SByte',
		'wchar':'Char',
		'byte':'Byte',
		'int':'Int32',
		'uint':'UInt32',
		'short':'Int16',
		'ushort':'UInt16',
		'long':'Int64',
		'ulong':'UInt64',
		'float':'float',
		'double':'double',
		}

	if n in m:
		n = m[n]

	if t.typeArg:
		if n == 'list':
			n = 'List'
		n = '%s<%s>' % (n, getCsTypeName(t.typeArg))
	
	if t.arraySize:
		n = n + '[]'

	if n == 'SByte[]' or n == 'Char[]':
		n = 'String'

	return n

def appendLine(l, t, s):
	if type(s) == list:
		for i in s:
			appendLine(l, t, i)
		return

	for i in s.split('\n'):
		if i:
			l.append(t + i)
		else:
			l.append(i)


def getEncodingOfPrimitiveType(tname):
	m = { 'bool':'Boolean',  'byte':'Byte',    'sbyte':'SByte',
	      'short':'Int16', 'ushort':'UInt16', 'int':'Int32',  'uint':'UInt32',
	      'long':'Int64', 'ulong':'UInt64', 'float':'Float', 'double':'Double',}
	return m.get(tname, ''), tname in m

# ---------------------------------------------------------------------------------


class CsGenerator(object):
	def __init__(self, pd, name):
		self.pd = pd
		self.name = name

	def isPrimitiveType(self, t):
		_, isPritimive = self.getDefaultEncodingOfType(t)
		return isPritimive

	def getDefaultEncodingOfType(self, t):
		if t.isPtr or t.arraySize:
			return '', False
		else:
			tn = self.pd.resolveTypedefs(t.name)
			return getEncodingOfPrimitiveType(tn)

	def generateType(self, vname, t, attr=PdFile.Attribute()):
		n = vname
		tn = getCsTypeName(t, attr)

		enc = attr.getEncoding()

		if t.arraySize:
			d = '%s %s;' % (tn, n)

			lenVar = attr.get('length')

			if lenVar:
				arrElementType = t.getArrayElementType()
				_, rr, ww = self.generateType('%s[i]' % n, arrElementType)

				r = ('for(int i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (lenVar, rr)
				w = ('for(int i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (lenVar, ww)

			elif tn == 'String':
				if t.name == 'char':
					d = 'String %s;' % n
					r = '%s = decoder.ReadString(%s);' % (n, getArraySizeExprOf(t,-1))
					w = 'encoder.WriteString(%s, %s);' % (n, getArraySizeExprOf(t,-1))
				elif t.name == 'wchar':
					d = 'String %s;' % n
					if enc == 'zero_terminated':
						r = '%s = decoder.ReadWStringz(%s);' % (n, getArraySizeExprOf(t,-1))
						w = 'encoder.WriteWStringz(%s, %s);' % (n, getArraySizeExprOf(t,-1))
					else:
						r = '%s = decoder.ReadWString(%s);' % (n, getArraySizeExprOf(t,-1))
						w = 'encoder.WriteWString(%s, %s);' % (n, getArraySizeExprOf(t,-1))
				else:
					raise RuntimeError('unknown string type: ' + t.name)
			else:
				arrElementType = t.getArrayElementType()
				_, rr, ww = self.generateType('%s[i]' % n, arrElementType)

				r = ('for(int i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (getArraySizeExprOf(t), rr)
				w = ('for(int i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (getArraySizeExprOf(t), ww)
		else:
			d = '%s %s;' % (tn, n)
		
			if t.name == 'list':

				_, typeArgIsPrimitive = self.getDefaultEncodingOfType(t.typeArg)
				if typeArgIsPrimitive:
					typeArgInit = '%s c;' % getCsTypeName(t.typeArg)
				else:
					typeArgInit = 'var c = new %s();' % getCsTypeName(t.typeArg)

				_, rr, _ = self.generateType('c', t.typeArg)
				r = ('{{\n'
					'\t{vname} = new {tn}();\n'
				    '\tint temp_cnt = decoder.ReadInt32();\n'
				    '\tfor(int i = 0; i < temp_cnt; ++i)\n'
				    '\t{{\n'
				    '\t\t{typeArgInit}\n'
				    '\t\t{rr}\n'
				    '\t\t{vname}.Insert(i, c);\n'
				    '\t}}\n'
				    '}}\n').format(vname=vname, tn=tn, typeArgInit=typeArgInit, rr=rr)

				_, _, ww = self.generateType(vname + '[i]', t.typeArg)
				w =('encoder.WriteInt32({vname}.Count);\n'
				    'for(int i = 0; i < {vname}.Count; ++i)\n'
				    '{{\n'
				    '\t{ww}\n'
				    '}}\n').format(vname=vname, tn=tn, ww=ww)

			else:
				e, _ = self.getDefaultEncodingOfType(t)

				if e:
					r = '%s = decoder.Read%s();' % (n, e)
					w = 'encoder.Write%s(%s);' % (e, n)
				else:
					r = '%s.OnDecode(decoder);' % (n,)
					w = '%s.OnEncode(encoder);' % (n,)

		return d, r, w

	def generateVar(self, varInfo):
		v = varInfo
		return self.generateType(v.name, v.type, v.attr)

	def generateStruct(self, st):

		for v in st.variables:
			if v.attr.getEncoding() == 'B':
				v.type = PdFile.TypeInfo('bool')

		p = self.pd.getProtocolForStruct(st)
		flagNoSend = (p and p.hasFlag('cs', 'NoSend'))
		flagNoReceive = (p and p.hasFlag('cs', 'NoReceive'))

		dl = []
		rl = []
		wl = []

		vl = [self.generateVar(v) for v in st.variables]

		l = []
		l.append('public class %s : Packet' % st.name)
		l.append('{')

		if st.variables:
			l.append('')

		t = '    '
		for d, r, w in vl:
			appendLine(l, t, 'public ' + d)

		if not flagNoReceive:
			l.append('')
			appendLine(l, t, self.generateStructDefaultCons(st))

		if (st.variables or flagNoReceive) and not flagNoSend:
			l.append('')
			appendLine(l, t, self.generateStructCons(st))

		# h1 = 'public override void OnDecode(NetMessageDecoder decoder)'
		# h2 = 'public override void OnEncode(NetMessageEncoder encoder)'

		# if not flagNoReceive:
		# 	l.append('')
		# 	appendLine(l, t, h1)
		# 	appendLine(l, t, '{')
		# 	for d, r, w in vl:
		# 		appendLine(l, t * 2, r)
		# 	appendLine(l, t, '}')

		# if not flagNoSend:
		# 	l.append('')
		# 	appendLine(l, t, h2)
		# 	appendLine(l, t, '{')
		# 	for d, r, w in vl:
		# 		appendLine(l, t * 2, w)
		# 	appendLine(l, t, '}')

		l.append('}')

		return '\n'.join(l)


	def generateStructDefaultCons(self, st):
		t = '    '
		l = []
		l.append('public %s()' % st.name)
		l.append('{')

		for v in st.variables:
			if v.init is not None:
				appendLine(l, t, "%s = %s;" % (v.name, v.init))
			else:
				tn = getCsTypeName(v.type, v.attr)
			
				if tn == 'String':
					appendLine(l, t, "%s = \"\";" % (v.name,))
				elif v.type.arraySize:
					t2 = v.type.clone()
					t2.arraySize = 0
					appendLine(l, t, "%s = new %s[%s];" % (v.name, getCsTypeName(t2), getArraySizeExprOf(v.type)))

				elif not self.isPrimitiveType(v.type):
						appendLine(l, t, "%s = new %s();" % (v.name, tn))

		l.append('}')
		return l

	def generateStructCons(self, st):
		t = '    '
		l = []

		argList = getConsParamsOf(st, namePrefix='_')
		l.append('public %s(%s)' % (st.name, argList))
		l.append('{')

		for v in st.variables:
			appendLine(l, t, "%s = _%s;" % (v.name, v.name))

		if not st.variables:
			l.append('')

		l.append('}')
		return l


	def generateFile(self):

		pd = self.pd

		l = []
		t = '    '

		l.append('//')
		l.append('// generated from "%s"' %(os.path.basename(pd.filename),))
		l.append('//')
		l.append('')

		for lang, s in pd.insertCodes:
			if lang == 'cs':
				appendLine(l, t, s)

		self.generateEnums(l)
		self.generateCodes(l)
		# self.generateMessageFactory(l)
		# self.generateStructs(l)

		return '\n'.join(l)

	def generateEnums(self, l):
		t = '    '

		for en in self.pd.enums:
			l.append('')
			appendLine(l, t, 'public enum %s : int\n{' % en.name)

			for k, v in en.items:
				appendLine(l, t * 2, '%s = %s,' % (k, v))

			appendLine(l, t, '}')

	def generateCodes(self, l):
		t = '    '
		pd = self.pd
		for p in pd.protocols:
			l.append('')
			appendLine(l, t, 'public enum %s : uint' % (p.name,))
			appendLine(l, t, '{')
			for n, v in p.codes:
				appendLine(l, t * 2, '%-30s = %d,' % (n, v))
			appendLine(l, t, '}')

	def generateMessageFactory(self, l):
		t = '    '
		pd = self.pd
		if pd.protocols:
			l.append('')
			appendLine(l, t, 'public static class makePacket')
			appendLine(l, t, '{')
			for p in pd.protocols:
				if p.hasFlag('cs', 'NoReceive'):
					continue

				appendLine(l, t * 2, 'public static Packet create%s(uint type)' % (p.name,))
				appendLine(l, t * 2, '{')
				appendLine(l, t * 3, 'switch (type)')
				appendLine(l, t * 3, '{')
				for st in p.structs:
					code = p.getCode(st.name)
					appendLine(l, t * 4, 'case (int)%s.%-30s return new %s();' % (p.name, st.name + ':', st.name))
				appendLine(l, t * 3, '}')
				appendLine(l, t * 3, 'return null;')
				appendLine(l, t * 2, '}')
			appendLine(l, t, '}')

	def generateStructs(self, l):
		t = '    '
		pd = self.pd
		for st in pd.structs:
			l.append('')
			s = self.generateStruct(st)
			appendLine(l, t, s)


def getConsParamsOf(st, namePrefix = ''):
	return ', '.join(["%s %s" % (getCsTypeName(v.type), namePrefix + v.name) for v in st.variables])

def getPascalCaseNameOf(s):
	if s[2] == '_':
		s = s[3:]

	return ''.join([i.capitalize() for i in s.split('_')])

# --------------------------------------------------------------------------------------------

def getCsFileNamesOf(outDir, n):
	l = n + '.cs'

	# m = re.match(r'^(\w+)Protocol$', n)
	# if m:
	# 	n = m.group(1)
	# 	l = 'NetProtocol%s.cs' % n


	return os.path.join(outDir, l)

def generateCsFiles(l, outDir):
	pdFiles = []

	for fn in l:
		pdDir, n = os.path.split(fn)
		n = os.path.splitext(n)[0]
		csFile = getCsFileNamesOf(outDir, n)

		print fn, '->', csFile
		pd = PdFile.readFile(fn)

		with open(csFile, 'wb') as f:
			s = CsGenerator(pd, n).generateFile()
			f.write(s)

		pdFiles.append(pd)

	return pdFiles

# --------------------------------------------------------------------------------------------

class PacketInfo:
	def __init__(self, st, code):
		self.st = st
		self.code = code
		self.name = st.name
		self.pascalCaseName = getPascalCaseNameOf(st.name)
		self.params = getConsParamsOf(st)

	def callParams(self, prefix=''):
		return ', '.join([prefix + v.name for v in self.st.variables])

class SourceCodeTemplate(object):
	def __init__(self, fn, text, pdFiles):
		self.filename = fn
		self.text = text
		self.pdFiles = pdFiles
		self.fb = []
		self.execEnv = {
			'add':self.addToBuf,
			'getPacketsOf':self.getPacketsOf,
			'getPacketOf':self.getPacketOf,
			'addInsertCodes':self.addInsertCodes,
			}
			
	def addInsertCodes(self, pdfilename):
		for pd in self.pdFiles:
			if pdfilename in pd.filename:
				for lang, s in pd.insertCodes:
					if lang == 'cs':
						appendLine(self.fb, '\n', s)
					
		self.fb.append('\n');

	def generate(self):
		pos = 0
		while pos < len(self.text):
			i = self.text.find('{{{', pos)
			if i < 0:
				self.fb.append(self.text[pos:])
				break
			else:
				self.fb.append(self.text[pos:i])

			j = self.text.find('}}}', i)
			if i < 0:
				print 'unmatched {{{', self.filename, ':', pos
				self.fb.append(self.text[pos:])
				break

			s = self.getStrOf(self.text[i+3:j])
			self.fb.append(s)

			pos = j+3

		return ''.join(self.fb)

	def getStrOf(self, s):
		self.buf = []

		try:
		 	s2 = 'if True:' + '  ' + '\n  '.join(s.split('\n'))
                        exec(s2, self.execEnv)
		except IndentationError:
			pass
			exec(s, self.execEnv)

		return ''.join(self.buf)

	def addToBuf(self, s):
		self.buf.append(s)

	def getPacketsOf(self, pn):
		for pd in self.pdFiles:
			for p in pd.protocols:
				if p.name == pn:
					l = []
					for st in p.structs:
						l.append(PacketInfo(st, p.getCode(st.name)))

					return l
		return []

	def getPacketOf(self, n):
		for pd in self.pdFiles:
			for p in pd.protocols:
				for st in p.structs:
					if st.name == n:
						return PacketInfo(st, p.getCode(st.name))

def generateCsFileFromTemplate(fn, fn2, pdFiles):
	print fn, '->', fn2

	with open(fn, 'rb') as f:
		tmpl = SourceCodeTemplate(fn, f.read(), pdFiles)

		s = tmpl.generate()

		# windows line ending
		s = re.sub('(?<!\r)\n', '\r\n', s)

		with open(fn2, 'wb') as f:
			f.write(s)

# --------------------------------------------------------------------------------------------

if __name__ == "__main__":
	if len(sys.argv) < 3:
		print "Command Line: python GenProtocolCs.py srcDir outDir"
		sys.exit(-1)

	#srcDir = '../../server/common/protocoldef/'
	srcDir = sys.argv[1]
	#outDir = '../../../../client/trunk/TrumpCardGame/Assets/Scripts/Network/'
	outDir = sys.argv[2]

	files = [
		'GameCommon.pd',
		'ErrorCodes.pd',
		'ServiceProtocol.pd',
	]

	pdFiles = generateCsFiles([os.path.join(srcDir, fn) for fn in files], outDir)

	# for fn in glob.glob(os.path.join(srcDir, "template_*.cs")):
	# 	name = re.match('^template_(.*).cs$', os.path.basename(fn)).group(1)
	# 	fn2 = os.path.join(outDir, name + '.cs')
	# 	generateCsFileFromTemplate(fn, fn2, pdFiles)

