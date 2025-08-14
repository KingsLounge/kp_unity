# -*- coding: utf-8 -*-
import os
import re

class Constants:
	def __init__(self, svrDir):
		self.kvmap = {}
		self.list = []
		self.typedefs = []

		fn = os.path.join(svrDir, 'common/GameCommon.h')
		GameCommonParser(self).parseFile(fn)

	def hasEnumValue(self, n):
		return n in self.kvmap

	def addEnumValue(self, n, v):
		self.kvmap[n] = v

	def processNumbers(self, s):
		s = s.replace(' ', '')

		def f(m):
			n = m.group(0)
			if n in self.kvmap:
				return str(self.kvmap[n])
			return n

		s = re.sub(r'\w+', f, s)
		
		def g(m):
			a, b = m.groups()
			return str(int(a)+int(b))

		s = re.sub(r'(\d+)[+](\d+)', g, s)

		return s

	def toNumber(self, s):
		return int(self.processNumbers(s))

	def resolveTypedefs(self, name):
		for n, v in self.typedefs:
			if name == n:
				return v
		return name

	def generatePdFile(self, outDir):
		fn = os.path.join(outDir, 'GameCommon.pd')
		with open(fn, 'wb') as f:
			f.write('\xef\xbb\xbf')
			s = '\r\n'.join(self.list)
			f.write(unicode(s, 'mbcs').encode('utf8'))



class GameCommonParser:
	def __init__(self, constants):
		self.constants = constants
		self.eof = False
		self.lineNum = 0

	def writePdFile(self, s):
		self.constants.list.append(s)

	def readline(self):
		if self.eof: return ''

		s = self.file.readline()
		if not s: self.eof = True

		self.lineNum += 1
		return s.strip()

	def error(self, msg):
		raise RuntimeError('GameCommon.h:%s: %s' % (self.lineNum, msg))

	def parseFile(self, fn):
		
		emptyLine = 0

		with open(fn, 'rb') as f:
			self.file = f
			while not self.eof:
				s = self.readline()

				if not s:
					if not emptyLine: self.writePdFile('')
					emptyLine += 1
					continue

				if s == '/*' or s == '*/': continue
				if s.startswith('#include'): continue

				emptyLine = 0
				if s == '#pragma once': continue

				m = re.match(r'//(.*)', s)
				if m:
					v = m.group(1)
					self.writePdFile('#' + v)
					continue

				m = re.match(r'enum(?:\s+(\w+))?', s)
				if m:
					n = m.group(1)
					if not n: n = ''
					self.parseEnum(n)
					continue

				m = re.match(r'typedef ([\s\w]+)\s+(\w+)\s*;', s)
				if m:
					v, n = m.groups()
					v = v.strip()
					self.constants.typedefs.append((n, v))
					self.writePdFile('typedef %s = %s;' % (n, v))
					continue

				m = re.match(r'#define\s+(\w+)\s+"(.*)"', s)
				if m:
					n, v = m.groups()
					self.writePdFile('# const %s = "%s";' % (n, v))
					continue

				self.error('parse error!')

	def parseEnum(self, n):

		self.writePdFile('enum ' + n)
		self.writePdFile('{')

		while True:
			s = self.readline()
			if s == '{': break
			if not s: self.error('parseEnum error!')

		curValue = -1

		while True:
			s = self.readline()
			if s == '};': break

			m = re.match(r'//(.*)', s)
			if m:
				self.writePdFile('\t#' + m.group(1))
				continue

			m = re.match(r'(\w+)\s*(?:=\s*([^\n,]+))?[,]?\s*(?://(.*))?', s)
			if m:
				n, v, cmt = m.groups()
				
				if not v: v = ''
				v = v.strip()

				if not cmt: cmt = ''
				
				if self.constants.hasEnumValue(n): self.error('duplicate name!')

				if v:
					curValue = eval(self.constants.processNumbers(v))
				else:
					curValue += 1

				self.constants.addEnumValue(n, curValue)

				t = '\t' + n
				if v: t = '%s = %s' % (t, v)
				t = t + ','
				if cmt: t = '%s # %s' % (t, cmt)

				self.writePdFile(t)

		self.writePdFile('}')
