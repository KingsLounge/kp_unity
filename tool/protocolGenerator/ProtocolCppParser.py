# -*- coding: utf-8 -*-
import os
import re



class TypeInfo:
	def __init__(self, constants, t, a, p):
		#print 'typeinfo', (t, a, p)

		t = preprocessTypeString(t)
		self.t = t
		self.name = t

		if a:
			a = a.strip()
			self.isArray = True
			self.arrayElementType = t
			self.arraySizeS = a
			self.arraySize = constants.toNumber(a)
			self.name = '%s[%s]' % (t, a)
		else:
			self.isArray = False
			self.arrayElementType = None
			self.arraySizeS = ''
			self.arraySize = 0

		self.isPointer = p
		self.pointee = None

		if self.isPointer:
			self.name = t + '*'
			self.pointee = t


def parseDeclLine(s):
	s = re.sub(r'\s+', ' ', s)

	name_arr = r'(\w+) *(?:\[([^\]]+)\])?'

	m = re.match(r'([\w ]+) +' + name_arr, s)
	if m:
		t, n, a = m.groups()
		return n, (t, a, False)

	m = re.match(r'([\w]+ *< *\w+ *>) *' + name_arr, s)
	if m:
		t, n, a = m.groups()
		return n, (t, a, False)

	m = re.match(r'([\w ]+)[*] *' + name_arr, s)
	if m:
		t, n, a = m.groups()
		return n, (t, a, True)



class CppParser(object):
	def __init__(self, constants):
		self.constants = constants

	def parse(self, src):
		unknownTypes = []
		err = 0
		l = []

		for s in src.split('\n'):
			s = s.strip()
			if not s: continue

			k = s.split('//', 1)
			s = k[0]
			cmt = ''

			if len(k) == 2: cmt = k[1]

			s = s.strip(';')
			if not s: continue

			m = parseDeclLine(s)
			if not m:
				print 'parse error:', repr(s)
				err += 1
				continue

			n, t = m

			l.append((n, TypeInfo(self.constants, *t), cmt))

		return l, err


def preprocessTypeString(s):
	s = s.strip()
	s = re.sub(r'\s+', ' ', s)

	m = {
		'bool':'bool',
		'char':'char',
		'wchar_t':'wchar',
		'BYTE':'byte',
		'unsigned char':'byte',
		'short':'short',
		'USHORT':'ushort',
		'unsigned short':'ushort',
		'int':'int',
		'INT':'int',
		'UINT':'uint',
		'DWORD':'uint',
		'BOOL':'int',
		'size_t':'int',
		'long':'int',
		'LONG':'int',
		'unsigned':'uint',
		'unsigned int':'uint',
		'long long':'long',
		'LONGLONG':'long',
		'unsigned long long':'ulong',
		'ULONGLONG':'ulong',
		'float':'float',
		'double':'double',
		}

	def f(n):
		s = n.group(0)
		if s in m: return m[s]
		return s

	s = re.sub(r'\w+(?: \w+)*', f, s)

	return s


def preprocessComment(s):
	s = s.rstrip()
	if not s: return s

	l = s.split('\n')
	l2 = []
	for i in l:
		i = i.strip()
		if i.startswith('//'): i = i[2:]
		l2.append(i)
	return '\n'.join(l2)



def listPacketBlockWithComment(srcFile):
	with open(srcFile, 'rb') as f:
		comments = []

		while True:
			s = f.readline()
			if not s: return

			s = s.strip()

			if s.startswith('PACKET_BEGIN'):

				lines = [s]
				while True:
					s = f.readline()
					if not s: return

					s = s.strip()
					lines.append(s)
					if s.startswith('PACKET_END()'): break

				yield '\n'.join(comments), '\n'.join(lines)

				comments = []

			elif s.startswith('//'):
				comments.append(s)
			elif not s and comments:
				comments.append(s)
			else:
				comments = []


def enumeratePacketBlocks(srcFile):

	l = []

	for cmt, blk in listPacketBlockWithComment(srcFile):
		m = re.match(
				r'^PACKET_BEGIN[(]([^)]+)[)]' +
				r'((?:.|\r|\n)*?)' +
				r'_PACKET_IMPLEMENT_READ[(][)]' +
				r'((?:.|\r|\n)*?)' +
				r'_PACKET_IMPLEMENT_WRITE[(][)]' +
				r'((?:.|\r|\n)+?)' +
				r'^PACKET_END[(][)]'
				,
				blk,
				re.M)

		name, s1, s2, s3 = m.groups()
		cmt = preprocessComment(cmt)

		l.append((cmt.replace('\r','').rstrip(), name, s1, s2, s3))

	with open(srcFile, 'rb') as f:
		src = f.read()
		if len(l) != len(re.findall('^PACKET_BEGIN', src, re.M)):
			raise RuntimeError('incorrect #packets')

	return l
		

def compactSerializeCodeList(constants, s):
	l = []
	for i in s.split('\n'):
		i = i.split('//', 1)[0]
		i = compactSerializeCode(constants, i)
		if not i: continue
		l.append(i)
	return l

def compactSerializeCode(constants, s):
	s = s.strip()
	s = s.strip(';')

	s = re.sub(r'\bDWORD\b', r'UINT', s)
	s = re.sub(r'\bunsigned short\b', r'USHORT', s)

	s = re.sub(r'\s+', '', s)
	s = s.replace('UNPACK_STRING(data,', 'P_READ_S(')
	s = s.replace('PACK_STRING(buffer,', 'P_WRITE_S(')

	s = constants.processNumbers(s)

	s = re.sub(r'(\w+)=UNPACK_TYPE[(]data,bool[)]', r'P_READ_B(\1)', s)
	s = re.sub(r'PACK_TYPE[(]buffer,(\w+),bool[)]', r'P_WRITE_B(\1)', s)

	s = re.sub(r'(\w+)=UNPACK_TYPE[(]data,LONGLONG[)]', r'P_READ_LL(\1)', s)
	s = re.sub(r'PACK_TYPE[(]buffer,(\w+),LONGLONG[)]', r'P_WRITE_LL(\1)', s)

	s = re.sub(r'(\w+)=UNPACK_TYPE[(]data,ULONGLONG[)]', r'P_READ_ULL(\1)', s)
	s = re.sub(r'PACK_TYPE[(]buffer,(\w+),ULONGLONG[)]', r'P_WRITE_ULL(\1)', s)

	s = re.sub(r'(\w+)=UNPACK_SHORT[(]data[)]', r'P_READ_H(\1)', s)
	s = re.sub(r'PACK_SHORT[(]buffer,(\w+)[)]', r'P_WRITE_H(\1)', s)
	s = re.sub(r'(\w+)=UNPACK_TYPE[(]data,short[)]', r'P_READ_H(\1)', s)
	s = re.sub(r'PACK_TYPE[(]buffer,(\w+),short[)]', r'P_WRITE_H(\1)', s)

	s = re.sub(r'(\w+)=UNPACK_INT[(]data[)]', r'P_READ_I(\1)', s)
	s = re.sub(r'PACK_INT[(]buffer,(\w+)[)]', r'P_WRITE_I(\1)', s)
	s = re.sub(r'(\w+)=UNPACK_TYPE[(]data,int[)]', r'P_READ_I(\1)', s)
	s = re.sub(r'PACK_TYPE[(]buffer,(\w+),int[)]', r'P_WRITE_I(\1)', s)

	s = re.sub(r'(\w+)=UNPACK_UINT[(]data[)]', r'P_READ_U(\1)', s)
	s = re.sub(r'PACK_UINT[(]buffer,(\w+)[)]', r'P_WRITE_U(\1)', s)

	s = re.sub(r'UNPACK_WSTRING[(]data,(\w+),(\d+)[)]', r'P_READ_WS(\1,\2)', s)
	s = re.sub(r'PACK_WSTRING[(]buffer,(\w+),(\d+)[)]', r'P_WRITE_WS(\1,\2)', s)

	s = re.sub(r'P_READ_D[(][&](\w+),sizeof[(](\w+)[)][)]', r'P_READ_D(&\1,sizeof(\1))', s)
	s = re.sub(r'P_WRITE_D[(][&](\w+),sizeof[(](\w+)[)][)]', r'P_WRITE_D(&\1,sizeof(\1))', s)

	s = re.sub(r'UNPACK_DATA[(]data,[&](\w+),sizeof[(]\1[)][)]', r'P_READ_D(&\1,sizeof(\1))', s)
	s = re.sub(r'PACK_DATA[(]buffer,[&](\w+),sizeof[(]\1[)][)]', r'P_WRITE_D(&\1,sizeof(\1))', s)

	s = re.sub(r'PACK_SEEKADD[(](\w+)[.]SerializeFrom[(]PACK_TELL[(]data[)],dataLen[)][)]', r'P_READ_ST(\1)', s)
	s = re.sub(r'PACK_SEEKADD[(](\w+)[.]SerializeTo[(]PACK_TELL[(]buffer[)],bufferLen[)][)]', r'P_WRITE_ST(\1)', s)

	s = re.sub(r'PACK_SEEKADD[(](\w+)->SerializeFrom[(]PACK_TELL[(]data[)],dataLen[)][)]', r'P_READ_ST(*\1)', s)
	s = re.sub(r'PACK_SEEKADD[(](\w+)->SerializeTo[(]PACK_TELL[(]buffer[)],bufferLen[)][)]', r'P_WRITE_ST(*\1)', s)

	s = s.replace('(UUID)', '(_GUID)')

	return s


