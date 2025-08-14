# -*- coding: utf-8 -*-
import os
import re
import token
import tokenize
import sys
import glob


class TypeInfo:
	def __init__(self, name, typeArg, arraySize, isPtr):
		self.name = name
		self.typeArg = typeArg
		self.arraySize = arraySize
		self.isPtr = isPtr

class VariableInfo:
	def __init__(self, name, varType, attr):
		self.name = name
		self.type = varType
		self.attr = attr

class StructInfo:
	def __init__(self, name, l):
		self.name = name
		self.variables = l

class PdFileInfo:
	def __init__(self, l):
		self.structList = l


class PdFileParser:
	def __init__(self, tokens):
		self.tokens = tokens
		self.nextToken()

	def nextToken(self, n=None, t=None):
		if n and n != self.cur[0]: self.error()
		if t and t != self.cur[1]: self.error()

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

	def error(self):
		raise RuntimeError('parse error!' + str(self.curToken))

	def pdFile(self):

		l = []

		while True:
			n, t = self.cur
			if n == token.NAME and t == 'struct':
				l.append(self.struct())
			elif n == token.ENDMARKER:
				break
			else:
				self.error()
		
		return PdFileInfo(l)

	def struct(self):
		n, t = self.nextToken(t='struct')

		if n == token.NAME:
			name = t
		else:
			self.error()

		self.nextToken()
		self.nextToken(t='{')

		l = []

		while True:
			if self.cur[1] == '}':
				self.nextToken()
				break
			l.append(self.variable())

		return StructInfo(name, l)

	def variable(self):
		attr = self.attribute()
		varType = self.varType()

		self.assertType(token.NAME)
		name = self.cur[1]

		self.nextToken()
		self.assertToken(';')
		self.nextToken()

		return VariableInfo(name, varType, attr)

	def attribute(self):
		n, t = self.cur

		name = None

		if t == '[':
			self.nextToken()
			self.assertType(token.NAME)
			name = self.cur[1]
			self.nextToken()
			self.assertToken(']')
			self.nextToken()

		return name

	def varType(self):
		self.assertType(token.NAME)
		name = self.cur[1]
		self.nextToken()

		typeArg = None

		if self.cur[1] == '<':
			self.nextToken()
			self.assertType(token.NAME)
			typeArg = self.cur[1]
			self.nextToken()
			self.assertToken('>')
			self.nextToken()

		arraySize = 0

		if self.cur[1] == '[':
			self.nextToken()
			self.assertType(token.NUMBER)
			arraySize = int(self.cur[1])
			self.nextToken()
			self.assertToken(']')
			self.nextToken()

		isPtr = False
		if self.cur[1] == '*':
			self.nextToken()
			isPtr = True

		return TypeInfo(name, typeArg, arraySize, isPtr)
			


def skipWhitespaces(tokens):
	for tk in tokens:
		n = tk[0]
		if n in [token.NEWLINE, token.INDENT, token.DEDENT]:
			#print 'skip', tk
			continue

		t = tk[1]
		if n != token.ENDMARKER and not t.strip():
			#print 'skip', tk
			continue

		yield tk


def processFile(fn):
	print fn

	with open(fn, 'rb') as f:
		tokens = tokenize.generate_tokens(f.readline)
		tokens = skipWhitespaces(tokens)
		v = PdFileParser(tokens).pdFile()
		print v


if __name__ == "__main__":
	srcDir = '.'

	if len(sys.argv) > 1:
		for fn in sys.argv[1:]:
			processFile(fn)

	else:
		for fn in glob.glob( os.path.join(srcDir, '*.pd') ):
			processFile(fn)



