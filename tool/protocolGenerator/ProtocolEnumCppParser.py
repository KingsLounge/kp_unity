# -*- coding: utf-8 -*-
import os
import re


class Codes:
	def __init__(self):
		self.groups = []

	def findProtocol(self, pname):
		for g, l in self.groups:
			for n, v in l:
				if n == pname:
					return g, v
		return "", 0

	def getGroup(self, n):
		for g, l in self.groups:
			if g == n: return l

	def getProtocol(self, gname, pname):
		g = self.getGroup(gname)
		if g:
			for n, v in g:
				if n == pname: return v

	def hasProtocol(self, gname, pname):
		v = self.getProtocol(gname, pname)
		return v is not None

	def addProtocol(self, groupName, name, curCode):
		g = self.getGroup(groupName)
		if g is None:
			g = []
			self.groups.append((groupName, g))

		g.append((name, curCode))


class Parser:
	def __init__(self, info):
		self.info = info
		self.eof = False
		self.lineNum = 0

	def readline(self):
		if self.eof: return ''

		s = self.file.readline()
		if not s: self.eof = True

		self.lineNum += 1
		return s.strip()

	def error(self, msg):
		raise RuntimeError('%s:%s: %s' % (self.filename, self.lineNum, msg))

	def parseFile(self, fn):
		self.filename = fn
		with open(fn, 'rb') as f:
			self.file = f
			self.parseMain()

	def parseMain(self):
		while not self.eof:
			s = self.readline()

			m = re.match(r'BEGIN_ENUM[(](\w+)[)]', s)
			if m:
				n = m.group(1)
				self.parseGroup(n)
				continue

	def parseGroup(self, groupName):

		endLine = 'END_ENUM(%s)' % groupName

		curCode = -1

		while True:
			s = self.readline()
			if s == endLine: break

			s = s.split('//')[0]
			s = s.strip()
			if not s: continue

			m = re.match(r'//(.*)', s)
			if m:
				self.writePdFile('\t#' + m.group(1))
				continue

			m = re.match(r'(DECL_ENUM_COMMENT|DECL_ENUM_ELEMENT)[(](.*)[)]', s)
			if not m: self.error('parse error!')

			ty, s = m.groups()
			s = re.sub(r'\s+', '', s)

			m = re.match(r'(\w+)=PROTOCOL_CODE[(](\w+),(\w+)[)]', s)
			if m:
				name, v1, v2 = m.groups()
				v1 = eval(v1)
				v2 = eval(v2)
				if v1 < 0 or v1 > 0xffff: self.error('code number out of range.')
				if v2 < 0 or v2 > 0xffff: self.error('code number out of range.')
				curCode = (v1 << 16) + v2
			else:
				name = s
				curCode += 1

			if self.info.hasProtocol(groupName, name):
				self.error('duplicate name!')

			self.info.addProtocol(groupName, name, curCode)



if __name__ == '__main__':
	import sys
	fn = sys.argv[1]
	info = Codes()
	Parser(info).parseFile(fn)

	for g, l in info.groups:
		print g
		for n, v in l:
			print '\t %-30s = 0x%08x' % (n, v)

