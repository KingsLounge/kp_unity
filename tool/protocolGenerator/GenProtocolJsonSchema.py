# -*- coding: utf-8 -*-
import os
import re
import sys
import glob
import PdFile

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
				d = '"%s" : { %s }' % (n, '"type" : "array"')
				#d = '%s : %s' % (n, '[]')
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
					d = '"%s" : { "type": "%s" }' % (n, 'string')
					#d = '%s : %s' % (n, '\'\'')
					r = 'this.%s = decoder.ReadString(%s);' % (n, getArraySizeExprOf(t,-1))
					w = 'encoder.WriteString(this.%s, %s);' % (n, getArraySizeExprOf(t,-1))
				elif t.name == 'wchar':
					d = '"%s" : { "type": "%s" }' % (n, 'string')
                    #d = '%s : %s' % (n, '\'\'')
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

			if e: # 여기서 a: {"type": "string"}
				if e == 'BigInteger':
					d = '"%s" : { "type": "%s" }' % (n, 'number')
					#d = '%s : %s' % (n, 'bigint.ZERO')
				elif e == 'Boolean':
					d = '"%s" : { "type": "%s" }' % (n, 'boolean')
					#d = '%s : %s' % (n, 'false')
				elif e == 'Int32':
					d = '"%s" : { "type": "%s" }' % (n, 'integer')
				else:
					d = '"%s" : { "type": "%s" }' % (n, 'string')
                    #d = '%s : %s' % (n, '0')

				

				r = 'this.%s = decoder.Read%s();' % (n, e)
				w = 'encoder.Write%s(this.%s);' % (e, n)
			else:
				d = '"%s" : { "type": "object" }' % (n)
				#d = '%s : NetData.%s()' % (n, tn)
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

	def generateStruct(self, st): # 구현
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
		
		l.append('"%s": {' % st.name)
		l.append(t + '"title": "%s",' % st.name)
		l.append(t + '"description": "%s",' % st.name)
		l.append(t + '"type": "object",')
		#l.append('this.%s = function() {' % st.name)
		appendLine(l, t, '"properties": {')
		#appendLine(l, t, 'var Class = {')

		pi = 0

		for d, r, w in vl:
			pi += 1
			if len(vl) == pi:
				appendLine(l, t * 2, '%s' % d)
			else:
				appendLine(l, t * 2, '%s,' % d)


		appendLine(l, t, '}') # 프로퍼티 끝
		# appendLine(l, t, '};')

		# h1 = 'Class.OnDecode = function(decoder)'
		# h2 = 'Class.OnEncode = function(encoder)'

		# l.append('')
		# appendLine(l, t, h1)
		# appendLine(l, t, '{')
		# for d, r, w in vl:
		# 	appendLine(l, t * 2, '%s' % r)
		# appendLine(l, t, '};')

		# l.append('')
		# appendLine(l, t, h2)
		# appendLine(l, t, '{')
		# for d, r, w in vl:
		# 	appendLine(l, t * 2, '%s' % w)
		# appendLine(l, t, '};')

		# l.append('')
		# appendLine(l, t, 'return Class;')
		l.append('},')
		#l.append('};')

		return '\n'.join(l)

	def generateFile(self): # 여기가 코드변환의 시작인거같음
		pd = self.pd

		l = [] # 얘한테 값을 계속 추가함
		t = '    ' # 탭간격인듯함 스페이스바 4칸

		# l.append('//')
		# l.append('// generated from "%s"' %(os.path.basename(pd.filename),))
		# l.append('//')
		# l.append('') # 한칸 띄우는거로 추정

		# for lang, s in pd.insertCodes: # js라고 적혀있는 애들은 쌩으로 넣어주나봄
		# 	if lang == 'js':
		# 		l.append(s)

		# l.append('')
		# l.append('module.exports = new function()') # 이제부터 js 시작
		l.append('{')

		# 얘네처럼 함수를 JSON으로 만들어야할듯함
		# self.generateEnums(l) # 작동 안함
		# self.generateCodes(l) # 헤더
		# self.generateMessageFactory(l) # 걍 로그띄우는거
		self.generateStructs(l) # 소스

		l.append('') # 한칸 띄우는거로 추정
		l.append('}') # 종료

		return '\n'.join(l) # 개행으로 마무리

	def generateIndexFile(self, jsFile):
		# 추후에 한단계 아래로 가도록 수정하거나 아니면 protocol파일들을 하위폴더에 넣도록 수정해야됨
		#pdDir, n = os.path.split(jsFile)
		#n = os.path.splitext(n)[0]

		# var n = require('./file/n');
		
		#s = "module.exports." + n + " = require('./file/" + n + "');"

		#with open(indexFile, 'a') as f:
		#	f.write(s)
		#	f.write('\r')
		t = '    '
	
	def generateEnums(self, l):
		t = '    '

		for en in self.pd.enums:
			l.append('')
			appendLine(l, t, 'this.%s = {' % en.name)

			for k, v in en.items:
				appendLine(l, t * 2, '%s : %s,' % (k, v))

			appendLine(l, t, '};')

	def generateCodes(self, l):
		t = '    '
		pd = self.pd
		for p in pd.protocols:
			l.append('')
			appendLine(l, t, 'this.코드%s = ' % (p.name,))
			appendLine(l, t, '{')
			for n, v in p.codes:
				appendLine(l, t * 2, '%-30s : %d,' % (n, v))

			appendLine(l, t, '};')

	def generateMessageFactory(self, l):
		print "Call generateMessageFactory"

	def generateStructs(self, l):
		print "Call generateJsonSchema"
		t = '    '
		pd = self.pd
		i = 0
		for st in pd.structs:
			i += 1
			l.append('')
			s = self.generateStruct(st)
			if i == len(pd.structs):
				s = s[:-1]
			appendLine(l, t, s)


# --------------------------------------------------------------------------------------------

def generateJsonFiles(l, outDir):
	for fn in l:
		pdDir, n = os.path.split(fn)
		n = os.path.splitext(n)[0]
		jsFile = os.path.join(outDir, n + '.json')
		print fn, '->', jsFile
		pd = PdFile.readFile(fn)
		jsGen = JsGenerator(pd)

		with open(jsFile, 'wb') as f:
			s = jsGen.generateFile()
			f.write(s)

		# jsGen.generateIndexFile(jsFile);
			

# --------------------------------------------------------------------------------------------

def initIndexfile():
	with open(indexFile, "wb") as f:
		f.write("// make from generator");
		f.write("\r");

# --------------------------------------------------------------------------------------------

if __name__ == "__main__":
	if len(sys.argv) < 4:
		print "Command Line: python GenProtocolJs.py srcDir outDir indexFile [.pd files]"
		sys.exit(-1)

	srcDir = sys.argv[1]
	outDir = sys.argv[2]	
	#indexFile = sys.argv[3]
		
	#initIndexfile()

	files = []
	
	for i in sys.argv:
		if i.endswith('.pd'):
			files.append(i)
	
	pdFiles = generateJsonFiles([os.path.join(srcDir, fn) for fn in files], outDir)
	#generateJsFiles(glob.glob( os.path.join(srcDir, '*.pd') ), outDir)