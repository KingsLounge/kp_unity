# -*- coding: utf-8 -*-
import os
import token
import tokenize
import StringIO

class TypeInfo:
	def __init__(self, name, typeArg=None, arraySize=0, isPtr=False):
		self.name = name
		self.typeArg = typeArg
		self.arraySize = arraySize
		self.arraySizeExpr = ''
		self.isPtr = isPtr 

	def __str__(self):
		n = self.getNameWithTypeArg()
		if self.arraySize: n = '%s[%s]' % (n, self.arraySize)
		if self.isPtr: n = n + '*'
		return 'Type(%s)' % (n,)

	def clone(self):
		return TypeInfo(self.name, self.typeArg, self.arraySize, self.isPtr)

	def getNameWithTypeArg(self):
		if self.typeArg:
			return '%s<%s>' % (self.name, self.typeArg.getNameWithTypeArg())
		else:
			return self.name

	def getArrayElementType(self):
		return TypeInfo(self.name, self.typeArg)


class Attribute:
	def __init__(self):
		self.values = {}

	def isEmpty(self):
		return not self.values

	def getEncoding(self): return self.values.get('encoding', '')
	def setEncoding(self, n): self.values['encoding'] = n
	def set(self, n, v):
		if n in ['B', 'C', 'H', 'UH', 'I', 'U', 'LL', 'ULL', 'zero_terminated', 'copy', 'assign', 'serializeInternal', 'ST']:
			self.setEncoding(n)
		else:
			if n not in ['code', 'length', 'NoPrefix']: raise RuntimeError('unknown Variable enc:' + n)
			self.values[n] = v

	def get(self, n):
		return self.values.get(n, '')

	def contains(self, n):
		return n in self.values
	


class VariableInfo:
	def __init__(self, name, varType, attr, init):
		self.name = name
		self.type = varType
		self.attr = attr
		self.init = init
	
	def __str__(self):
		return 'Var([%s] %s: %s)' % (self.attr, self.name, self.type)


class StructInfo:
	def __init__(self, name, l, attr, protocol=None):
		self.name = name
		self.variables = l
		self.attr = attr
		self.protocol = protocol

	def findVariableByName(self, n):
		for v in self.variables:
			if v.name == n:
				return v


class ProtocolInfo:
	def __init__(self, name, code, attr):
		self.name = name
		self.code = code
		self.attr = attr
		self.structs = []
		self.codes = []
		self.flags = []

	def addCode(self, n, v):
		self.codes.append((n, (self.code << 16) + v))

	def getCode(self, name):
		for n, code in self.codes:
			if n == name: return code

	def addFlag(self, lang, name):
		self.flags.append((lang, name))

	def hasFlag(self, lang, name):
		for l, n in self.flags:
			if l == lang and n == name:
				return True

class EnumInfo:
	def __init__(self, name, attr):
		self.name = name
		self.items = []
		self.attr = attr

	def add(self, k, v):
		self.items.append((k, v))


class PdFileInfo:
	def __init__(self, filename):
		self.filename = filename
		self.imports = []
		self.insertCodes = []
		self.enums = []
		self.typedefs = []
		self.structs = []
		self.protocols = []
	
	def addImport(self, v):
		self.imports.append(v)

	def addInsertCode(self, v):
		self.insertCodes.append(v)

	def addEnum(self, enum):
		self.enums.append(enum)

	def addTypedef(self, n, v):
		self.typedefs.append((n, v))

	def addStruct(self, v):
		self.structs.append(v)

	def newProtocol(self, name, code, attr):
		for p in self.protocols:
			if p.name == name: return p

		p = ProtocolInfo(name, code, attr)
		self.protocols.append(p)
		return p

	def getSize(self):
		return len(self.structs)

	def getStruct(self, name):
		for i in self.structs:
			if i.name == name:
				return i

	def getProtocolForStruct(self, st):
		for p in self.protocols:
			if p.getCode(st.name) is not None:
				return p

	def isPacket(self, st):
		return self.getProtocolForStruct(st)
	
	def getConstant(self, n):

		curVal = (None, None)
		numVal = 0

		for e in self.enums:
			for k, v in e.items:
				if ('%s.%s' % (e.name, k)) == n:
					return v, e
				if k == n:
					curVal = v, e
					numVal += 1

		for i in self.imports:
			v, e = i.getConstant(n)
			if v is not None:
				curVal = (v, e)
				numVal += 1

		if numVal == 1:
			return curVal

		return None, None

	def enumConstants(self):
		for e in self.enums:
			for n,v in e.items:
				yield e,n,v

		for i in self.imports:
			for e, n, v in i.enumConstants():
				yield e,n,v

	def resolveTypedefs(self, name):
		for n, t in self.typedefs:
			if name == n:
				return t.getNameWithTypeArg()

		for pd in self.imports:
			v = pd.resolveTypedefs(name)
			if v != name: return v

		return name
	
# ------------------------------------------------------------------------------------------------------

class PdFileParser:
	def __init__(self, fn, tokens):
		self.filename = fn
		self.tokens = tokens
		self.nextToken()

	def nextToken(self, n=None, t=None):
		if n and n != self.cur[0]: self.error()
		if t and t != self.cur[1]: self.error()

		while True:
			n, _ = self._nextToken()
			if n in [token.INDENT, token.DEDENT, token.NEWLINE]: continue
			if n >= token.N_TOKENS: continue
			break

		return self.cur


	def _nextToken(self):
		tk = next(self.tokens)
		self.curToken = tk
		self.cur = (tk[0], tk[1])
		return self.cur

	def assertType(self, n):
		if n != self.cur[0]:
			self.error()

	def assertToken(self, t):
		if t != self.cur[1]:
			self.error()

	def error(self, msg=''):
		if not msg: msg = 'error!'
		raise RuntimeError('%s!%s' % (msg, self.curToken))

	def pdFile(self):
		self.pd = PdFileInfo(self.filename)

		while True:
			attr = self.attribute()

			n, t = self.cur
			if t == 'protocol':
				self.protocol(attr)
			elif t == 'struct':
				self.pd.addStruct(self.struct(attr))
			elif t == 'import':
				self.pd.addImport(self.importFile())
			elif t == 'insert_code':
				self.pd.addInsertCode(self.insertCode())
			elif t == 'typedef':
				self.typedef()
			elif t == 'enum':
				self.parseEnum(attr)
			elif n == token.ENDMARKER:
				break
			else:
				self.error()
		
		return self.pd

	def protocol(self, attr):

		val = None

		_, name = self.nextToken(t='protocol')
		self.nextToken(n=token.NAME)
		if self.cur[1] == '(':
			self.nextToken(t='(')
			val, _ = self.numbers()
			self.nextToken(t=')')

		p = self.pd.newProtocol(name, val, attr)

		self.nextToken(t='{')

		if p.code is None:
			self.error('protocol code is none!')

		if val is not None and val != p.code:
			self.error('incorrect protocol code!')

		curCode = 0

		while True:
			attr = self.attribute()

			n, t = self.cur
			if t == 'struct':
				s = self.struct(attr)
				s.protocol = p
				p.structs.append(s)
				self.pd.addStruct(s)
			elif t == 'code':
				self.nextToken()
				_, codeName = self.cur
				self.nextToken()
				if self.cur[1] == '=':
					self.assertToken('=')
					self.nextToken()
					curCode, _ = self.numbers()
				else:
					curCode += 1
				self.assertToken(';')
				self.nextToken()

				p.addCode(codeName, curCode)
			elif t == 'flag':
				self.nextToken()
				_, lang = self.cur
				self.assertType(token.NAME)
				self.nextToken()
				_, flagName = self.cur
				self.assertType(token.NAME)
				self.nextToken()
				self.assertToken(';')
				self.nextToken()

				p.addFlag(lang, flagName)

			else:
				break

		self.nextToken(t='}')

	def parseEnum(self, attr):
		self.nextToken(t='enum')

		n, t = self.cur
		if n == token.NAME:
			enumName = t
			self.nextToken()
		else:
			enumName  = ''

		enum = EnumInfo(enumName, attr)
		self.pd.addEnum(enum)

		self.nextToken(t='{')
		val = -1

		while True:
			n, t = self.cur

			if t == '}':
				self.nextToken()
				break

			if n != token.NAME: self.error()

			name = t

			self.nextToken()
			n, t = self.cur
			if t == '=':
				self.nextToken()
				val, _ = self.numbers(enumGroup=enumName)
			else:
				val += 1

			enum.add(name, val)

			if self.cur[1] == ',':
				self.nextToken()

	def importFile(self):
		_, name = self.nextToken(t='import')
		self.assertType(token.NAME)
		self.nextToken()

		fn = os.path.join(os.path.dirname(self.filename), name + '.pd')
		return readFile(fn)

	def insertCode(self):
		_, name = self.nextToken(t='insert_code')
		_, codes = self.nextToken(n=token.NAME)
		self.assertType(token.STRING)
		self.nextToken()

		return name, eval(codes)

	def typedef(self):
		_, name = self.nextToken(t='typedef')
		self.nextToken(n=token.NAME)
		self.nextToken(t='=')
		varType = self.varType()
		self.nextToken(t=';')

		self.pd.addTypedef(name, varType)

	def struct(self, attr):
		n, t = self.nextToken(t='struct')

		if n == token.NAME:
			name = t
		else:
			self.error()

		self.nextToken()
		self.nextToken(t='{')

		#print 'struct', n, t

		l = []

		while True:
			if self.cur[1] == '}':
				self.nextToken()
				break
			l.append(self.variable())

		return StructInfo(name, l, attr)

	def variable(self):
		attr = self.attribute()
		varType = self.varType()
		initialVal = None

		self.assertType(token.NAME)
		name = self.cur[1]

		self.nextToken()

		_, t = self.cur

		if t == '=':
			self.nextToken()
			n, initialVal = self.cur
			if n != token.NUMBER and n != token.STRING:
				self.error('invalid initial value: ' + initialVal)

			self.nextToken()

		self.assertToken(';')
		self.nextToken()

		#print '  variable:', attr, name, varType

		return VariableInfo(name, varType, attr, initialVal)

	def attribute(self):

		attr = Attribute()

		while True:
			_, t = self.cur
			if t != '[': break

			self.nextToken()
			self.assertType(token.NAME)

			name = self.cur[1]
			val = ''

			self.nextToken()

			if self.cur[1] == '(':
				self.nextToken()
				val = self.cur[1]
				self.nextToken()
				self.assertToken(')')
				self.nextToken()

			self.assertToken(']')
			self.nextToken()

			attr.set(name, val)

		return attr

	def varType(self):
		self.assertType(token.NAME)
		name = self.cur[1]
		self.nextToken()

		while self.cur[1] in ['int', 'short', 'char', 'long']:
			name = '%s %s' % (name, self.cur[1])
			self.nextToken()

		typeArg = None

		if self.cur[1] == '<':
			self.nextToken()
			self.assertType(token.NAME)
			typeArg = TypeInfo(self.cur[1])
			self.nextToken()
			self.assertToken('>')
			self.nextToken()

		arraySize = 0
		arraySizeExpr = ''

		if self.cur[1] == '[':
			self.nextToken()
			arraySize, arraySizeExpr = self.numbers()
			self.assertToken(']')
			self.nextToken()

		isPtr = False
		if self.cur[1] == '*':
			self.nextToken()
			isPtr = True

		ty = TypeInfo(name, typeArg, arraySize, isPtr)
		ty.arraySizeExpr = arraySizeExpr
		return ty

	def numbers(self, enumGroup=''):

		l = []

		v, s = self.number(enumGroup)

		sumVal = v
		l.append(s)

		while self.cur[1] == '+':
			l.append('+')

			self.nextToken()
			v, s = self.number(enumGroup)

			sumVal += v
			l.append(s)

		return sumVal, ''.join(l)

	def number(self, enumGroup=''):
		n, t = self.cur
		self.nextToken()

		if n == token.NUMBER:
			return eval(t), t
		elif n == token.NAME:
			if self.cur[1] == '.':
				self.nextToken()
				_, t2 = self.cur
				self.nextToken()

				name = '%s.%s' % (t, t2)
				c, _ = self.pd.getConstant(name)
			else:
				name = t
				c, enumInfo = self.pd.getConstant(name)
				name = '%s.%s' % (enumInfo.name, t)

				if c is None:
					name = '%s.%s' % (enumGroup, t)
					c, _ = self.pd.getConstant(name)

			if c is None: self.error('constant not found: (%s)' % name)
			return c, name
		else:
			self.error()


def readFile(fn):
	with open(fn, 'rb') as f:
		s = f.read()

	if s.startswith('\xef\xbb\xbf'):
		s = s[3:]

	f = StringIO.StringIO(s)

	tokens = tokenize.generate_tokens(f.readline)
	pd = PdFileParser(fn, tokens).pdFile()
	return pd
