# -*- coding: utf-8 -*-
import os
import re
import sys
import glob
import PdFile
import logging

indexFile = '';

def getArraySizeExprOf(t, delta=0):
	if t.arraySizeExpr:

		if delta == 0:
			s = t.arraySizeExpr
			if '+' in s: s = '(%s)' % s
			return 'GameCommon.' + s
		elif delta == -1 and t.arraySizeExpr.endswith('+1'):
			s = t.arraySizeExpr[:-2]
			if '+' in s: s = '(%s)' % s
			return 'GameCommon.' + s

	return str(t.arraySize + delta)

def getJsTypeName(t, attr=None):
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
		'long':'BigInteger',
		'ulong':'BigInteger',
		'float':'float',
		'double':'double',
		}

	if n in m:
		n = m[n]

	if t.typeArg:
		if n == 'list':
			n = 'List'
		n = '%s<%s>' % (n, getJsTypeName(t.typeArg))
	
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
	m = { 'bool':'Boolean',  'byte':'UInt8',    'sbyte':'SByte',
	      'short':'Int16', 'ushort':'UInt16', 'int':'Int32',  'uint':'UInt32',
	      'long':'BigInteger', 'ulong':'BigInteger', 'float':'Float', 'double':'Double',}
	return m.get(tname, ''), tname in m

class JsGenerator(object):
	def __init__(self, pd):
		self.pd = pd

	def getDefaultEncodingOfType(self, t):
		if t.isPtr or t.arraySize:
			return '', False
		else:
			tn = self.pd.resolveTypedefs(t.name)
			return getEncodingOfPrimitiveType(tn)

	def generateType(self, vname, t, attr=PdFile.Attribute()):
		n = vname
		tn = getJsTypeName(t, attr)
		enc = attr.getEncoding()

		if t.arraySize:
			if tn == 'Int32[]':
				d = '%s : %s' % (n, '[]')
			else: 
				d = '%s : %s' % (n, '\'\'')

			lenVar = attr.get('length')

			if lenVar:
				arrElementType = t.getArrayElementType()
				_, rr, ww = self.generateType('%s[i]' % n, arrElementType)

				r = ('for(var i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (lenVar, rr)
				w = ('for(var i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (lenVar, ww)

			elif tn == 'String':
				if t.name == 'char':
					d = '%s : %s' % (n, '\'\'')
					r = 'this.%s = decoder.ReadString(%s);' % (n, getArraySizeExprOf(t,-1))
					w = 'encoder.WriteString(this.%s, %s);' % (n, getArraySizeExprOf(t,-1))
				elif t.name == 'wchar':
					d = '%s : %s' % (n, '\'\'')
					if enc == 'zero_terminated':
						r = 'this.%s = decoder.ReadWStringz(%s);' % (n, getArraySizeExprOf(t,-1))
						w = 'encoder.WriteWStringz(this.%s, %s);' % (n, getArraySizeExprOf(t,-1))
					else:
						r = 'this.%s = decoder.ReadWString(%s);' % (n, getArraySizeExprOf(t,-1))
						w = 'encoder.WriteWString(this.%s, %s);' % (n, getArraySizeExprOf(t,-1))
				else:
					raise RuntimeError('unknown string type: ' + t.name)
			else:
				arrElementType = t.getArrayElementType()
				_, rr, ww = self.generateType('%s[i]' % n, arrElementType)

				r = ('for(var i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (getArraySizeExprOf(t), rr)
				w = ('for(var i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (getArraySizeExprOf(t), ww)
		else:
			e, _ = self.getDefaultEncodingOfType(t)

			if e:
				if e == 'BigInteger':
					d = '%s : %s' % (n, 'bigint.ZERO')
				elif e == 'Boolean':
					d = '%s : %s' % (n, 'false')
				else:
					d = '%s : %s' % (n, '0')

				

				r = 'this.%s = decoder.Read%s();' % (n, e)
				w = 'encoder.Write%s(this.%s);' % (e, n)
			else:
				d = '%s : NetData.%s()' % (n, tn)
				r = 'this.%s.OnDecode(decoder);' % (n)
				w = 'this.%s.OnEncode(encoder);' % (n)

		return d, r, w

	def generateVar(self, varInfo):
		v = varInfo
		return self.generateType(v.name, v.type, v.attr)

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

	def generateStruct(self, st):
		for v in st.variables:
			if v.attr.getEncoding() == 'B':
				v.type = PdFile.TypeInfo('bool')

		p = self.pd.getProtocolForStruct(st)
		
		t = '    '

		dl = []
		rl = []
		wl = []

		vl = [self.generateVar(v) for v in st.variables]

		l = []
		
		l.append('const %s = ()=> {' % st.name)
		appendLine(l, t, 'var Class:any = {')

		for d, r, w in vl:
			appendLine(l, t * 2, '%s,' % d)

		appendLine(l, t, '};')
		
		h1 = 'Class.OnDecode = function(decoder:any)'
		h2 = 'Class.OnEncode = function(encoder:any)'

		l.append('')
		appendLine(l, t, h1)
		appendLine(l, t, '{')
		for d, r, w in vl:
			appendLine(l, t * 2, '%s' % r)
		appendLine(l, t, '};')

		l.append('')
		appendLine(l, t, h2)
		appendLine(l, t, '{')
		for d, r, w in vl:
			appendLine(l, t * 2, '%s' % w)
		appendLine(l, t, '};')

		l.append('')
		appendLine(l, t, 'return Class;')
		l.append('};')

		return '\n'.join(l)

	def generateFile(self):
		pd = self.pd

		l = []
		t = '    '

		l.append('//')
		l.append('// generated from "%s"' %(os.path.basename(pd.filename),))
		l.append('//')
		l.append('')

		for lang, s in pd.insertCodes:
			if lang == 'js':
				l.append(s)

		# l.append('')
		# l.append('module.exports = new function()')
		# l.append('exports ')
		# l.append('{')

		self.generateEnums(l)
		self.generateCodes(l)
		self.generateMessageFactory(l)
		self.generateStructs(l)

		# l.append('')
		# l.append('}')

		self.generateLastExport(l)

		return '\n'.join(l)

	def generateIndexFile(self, jsFile):
		# 추후에 한단계 아래로 가도록 수정하거나 아니면 protocol파일들을 하위폴더에 넣도록 수정해야됨
		pdDir, n = os.path.split(jsFile)
		n = os.path.splitext(n)[0]

		# var n = require('./file/n');
		
		s = "module.exports." + n + " = require('./file/" + n + "');"

		with open(indexFile, 'a') as f:
			f.write(s)
			f.write('\r')
	
	def generateEnums(self, l):
		t = '    '

		for en in self.pd.enums:
			l.append('')
			appendLine(l, t, 'enum %s {' % en.name)

			for k, v in en.items:
				appendLine(l, t * 2, '%s = %s,' % (k, v))

			appendLine(l, t, '};')
	

	def generateLastExport(self, l):
		l.append('')
		tt = 'export { '

		for en in self.pd.protocols:
			# l.append('')
			tt += en.name
			tt += ', '

		for en in self.pd.structs:
			# l.append('')
			tt += en.name
			tt += ', '

		for en in self.pd.enums:
			# l.append('')
			tt += en.name
			tt += ', '

		tt += '};'			
		l.append(tt)

	def generateCodes(self, l):
		t = '    '
		pd = self.pd
		for p in pd.protocols:
			l.append('')
			appendLine(l, t, 'enum %s ' % (p.name,))
			appendLine(l, t, '{')
			for n, v in p.codes:
				appendLine(l, t * 2, '%-30s = %d,' % (n, v))

			appendLine(l, t, '};')

	def generateMessageFactory(self, l):
		print "Call generateMessageFactory"

	def generateStructs(self, l):
		t = '    '
		pd = self.pd
		for st in pd.structs:
			l.append('')
			s = self.generateStruct(st)
			appendLine(l, t, s)


# --------------------------------------------------------------------------------------------

def generateJsFiles(l, outDir):
	for fn in l:
		pdDir, n = os.path.split(fn)
		n = os.path.splitext(n)[0]
		jsFile = os.path.join(outDir, n + '.ts')
		print fn, '->', jsFile
		pd = PdFile.readFile(fn)
		jsGen = JsGenerator(pd)

		with open(jsFile, 'wb') as f:
			s = jsGen.generateFile()
			f.write(s)

		jsGen.generateIndexFile(jsFile)
			

# --------------------------------------------------------------------------------------------

def initIndexfile():
	with open(indexFile, "wb") as f:
		f.write("// make from generator")
		f.write("\r")

# --------------------------------------------------------------------------------------------

if __name__ == "__main__":
	if len(sys.argv) < 4:
		print "Command Line: python GenProtocolJs.py srcDir outDir indexFile [.pd files]"
		sys.exit(-1)

	srcDir = sys.argv[1]
	outDir = sys.argv[2]	
	indexFile = sys.argv[3]
		
	initIndexfile()

	files = []
	
	for i in sys.argv:
		if i.endswith('.pd'):
			files.append(i)
	
	pdFiles = generateJsFiles([os.path.join(srcDir, fn) for fn in files], outDir)
	#generateJsFiles(glob.glob( os.path.join(srcDir, '*.pd') ), outDir)