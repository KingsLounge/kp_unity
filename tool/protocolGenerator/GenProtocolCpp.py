# -*- coding: utf-8 -*-
import os
import re
import sys
import glob
import PdFile


def getEncodingOfPrimitiveType(tname):
	m = { 'bool':'B',  'byte':'C',    'char':'C', 'wchar':'H',
	      'short':'H', 'ushort':'UH', 'int':'I',  'uint':'U',
	      'long':'LL', 'ulong':'ULL', 'float':'', 'double':'',}
	return m.get(tname, ''), tname in m

# ------------------------------------------------------------------------------------------------------

def getCppTypeName(t, attr=None):
	n = t.name

	m = {
		'bool':'bool',
		'char':'char',
		'wchar':'wchar_t',
		'byte':'BYTE',
		'int':'int',
		'uint':'UINT',
		'short':'short',
		'ushort':'USHORT',
		'long':'LONGLONG',
		'ulong':'ULONGLONG',
		'float':'float',
		'double':'double',
		}

	if n in m:
		n = m[n]

	if t.typeArg:
		n = '%s<%s>' % (n, getCppTypeName(t.typeArg))
	
	if n == 'int' and attr and attr.getEncoding() == 'B':
		n = 'BOOL'

	return n


def appendCppLine(l, h, s):
	for i in s.split('\n'):
		if l: h = ''
		v = ('%-40s' % h) + i
		l.append(v.rstrip())


# ---------------------------------------------------------------------------------


class CppGenerator(object):
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
		tn = getCppTypeName(t, attr)

		enc = attr.getEncoding()

		if t.arraySize:
			d = '%s %s[%s];' % (tn, n, t.arraySize)

			lenVar = attr.get('length')

			if lenVar:
				arrElementType = t.getArrayElementType()
				_, rr, ww = self.generateType('%s[i]' % n, arrElementType)

				r = ('for(int i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (lenVar, rr)
				w = ('for(int i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (lenVar, ww)

			elif enc == 'copy':
				r = 'P_READ_D(%s, sizeof(%s));' % (n, n)
				w = 'P_WRITE_D(%s, sizeof(%s));' % (n, n)
			elif tn == 'char':
				r = 'P_READ_S(%s, %s);' % (n, t.arraySize-1)
				w = 'P_WRITE_S(%s, %s);' % (n, t.arraySize-1)
			elif tn == 'wchar_t':
				if enc == 'zero_terminated':
					r = 'UNPACK_WSTRINGZ(data, %s, %s);' % (n, t.arraySize-1)
					w = 'PACK_WSTRINGZ(buffer, %s, %s);' % (n, t.arraySize-1)
				else:
					r = 'P_READ_WS(%s, %s);' % (n, t.arraySize-1)
					w = 'P_WRITE_WS(%s, %s);' % (n, t.arraySize-1)
			else:
				arrElementType = t.getArrayElementType()
				_, rr, ww = self.generateType('%s[i]' % n, arrElementType)

				r = ('for(int i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (t.arraySize, rr)
				w = ('for(int i = 0; i < %s; ++i)\n{\n\t%s\n}\n') % (t.arraySize, ww)
		else:
			if t.isPtr:
				d = '%s* %s;' % (tn, n)
			else:
				d = '%s %s;' % (tn, n)
		
			if t.isPtr:
				n = '*' + vname
				p = vname
			else:
				n = vname
				p = '&' + vname

			
			if enc == 'assign':
				r = '%s = UNPACK_TYPE(data, %s);' % (n, tn)
				w = 'PACK_TYPE(buffer, %s, %s);' % (n, tn)

			elif enc == 'copy':
				r = 'P_READ_D(%s, sizeof(%s));' % (p, n)
				w = 'P_WRITE_D(%s, sizeof(%s));' % (p, n)

			elif enc == 'serializeInternal':
				r = 'P_READ_STX(%s, SerializeInternalFrom);' % (n,)
				w = 'P_WRITE_STX(%s, SerializeInternalTo);' % (n,)

			elif enc:
				r = 'P_READ_%s(%s);' % (enc, n,)
				w = 'P_WRITE_%s(%s);' % (enc, n,)
			elif t.name == 'list':
				_, rr, _ = self.generateType('c', t.typeArg)
				r = ('{{\n'
					'{vname}.clear();\n'
				    'int temp_cnt;\n'
				    'P_READ_I(temp_cnt);\n'
				    'for(int i = 0; i < temp_cnt; ++i)\n'
				    '{{\n'
				    '\t{typeArg} c;\n'
				    '\t{rr}\n'
				    '\t{vname}.push_back(c);\n'
				    '}}\n'
				    '}}\n').format(vname=vname, typeArg=getCppTypeName(t.typeArg), rr=rr)

				_, _, ww = self.generateType('*it', t.typeArg)
				w =('P_WRITE_I({vname}.size());\n'
				    'for({tn}::iterator it = {vname}.begin(); it != {vname}.end(); ++it)\n'
				    '{{\n'
				    '\t{ww};\n'
				    '}}\n').format(vname=vname, tn=tn, ww=ww)

			else:
				encodingName, isPrimitive = self.getDefaultEncodingOfType(t)

				e = encodingName
				if e:
					r = 'P_READ_%s(%s);' % (e, n)
					w = 'P_WRITE_%s(%s);' % (e, n)
				elif isPrimitive:
					r = '%s = UNPACK_TYPE(data, %s);' % (n, tn)
					w = 'PACK_TYPE(buffer, %s, %s);' % (n, tn)
				else:
					r = 'P_READ_ST(%s);' % (n,)
					w = 'P_WRITE_ST(%s);' % (n,)

		return d, r, w

	def generateVar(self, varInfo):
		v = varInfo
		return self.generateType(v.name, v.type, v.attr)

	def generatePacket(self, st):
		dl = []
		rl = []
		wl = []

		vl = [self.generateVar(v) for v in st.variables]
		if not vl: vl.append(('', '', ''))

		for d, r, w in vl:
			appendCppLine(dl, 'PACKET_BEGIN(%s)' % st.name, d)
			appendCppLine(rl, '_PACKET_IMPLEMENT_READ()',   r)
			appendCppLine(wl, '_PACKET_IMPLEMENT_WRITE()',  w)

		l = dl + rl + wl
		l.append('PACKET_END()')

		return '\n'.join(l)

	def generateStruct(self, st):
		t = '    '

		dl = []
		rl = []
		wl = []

		vl = [self.generateVar(v) for v in st.variables]

		l = []
		l.append('struct %s' % st.name)
		l.append('{')

		l += [t + d for d, _, _ in vl]

		cons = [t + '%s = %s;' % (v.name, v.init) for v in st.variables if v.init is not None]

		if cons:
			l.append('')
			cons = [st.name + '()', '{'] + cons + ['}']
			l += [t + i for i in cons]

		l.append('')
		l.append(t + 'int SerializeFrom(const char* data, int dataLen)')
		l.append(t + '{')
		l.append(t * 2 + 'PACK_INIT();')
		for _, r, _ in vl:
			l.append(t * 2 + r)
		l.append(t * 2 + 'return PACK_LEN();')
		l.append(t + '}')

		l.append('')
		l.append(t + 'int SerializeTo(char* buffer, int bufferLen)')
		l.append(t + '{')
		l.append(t * 2 + 'PACK_INIT();')
		for _, _, w in vl:
			l.append(t * 2 + w)
		l.append(t * 2 + 'return PACK_LEN();')
		l.append(t + '}')

		l.append('};')

		return '\n'.join(l)

	def generateEnums(self):

		l = []

		for en in self.pd.enums:
			l.append('')
			l.append('enum %s\n{' % en.name)

			if en.attr.contains('NoPrefix'):
				prefix = ''
			elif en.name.endswith('_TYPE'):
				prefix = en.name[:-4]
			else:
				prefix = en.name + "_"

			for k, v in en.items:
				l.append('\t%s%s = %s,' % (prefix, k, v))

			l.append('};')

		return l

	def generateFile(self):

		pd = self.pd

		l = []

		l.append('//')
		l.append('// generated source file (%s)' %(os.path.basename(pd.filename),))
		l.append('//')

		for lang, s in pd.insertCodes:
			if lang == 'cpp':
				l.append(s)

		for name, varType in pd.typedefs:
			l.append('typedef %s %s;' % (getCppTypeName(varType), name))

		if pd.typedefs: l.append('\n')

		l += self.generateEnums()

		for st in pd.structs:
			if pd.isPacket(st):
				s = self.generatePacket(st)
			else:
				s = self.generateStruct(st)

			l.append('\n%s\n' % s)

		l.append('')

		return '\n'.join(l)


# --------------------------------------------------------------------------------------------


def generateProtocolEnumInl(pd, pdDir, outDir):

	for p in pd.protocols:
		fn = os.path.join(outDir, "enum_%s.h" % p.name)

		print fn, '..'

		with open(fn, 'wb') as f:

			f.write('//\n')
			f.write('// generated source file (%s)\n' % (os.path.basename(pd.filename),))
			f.write('//\n')
			f.write('\n')
			f.write('#pragma once\n')
			f.write('\n')

			f.write('enum %s\n' % (p.name,))
			f.write('{\n')

			for n, v in p.codes:
				f.write('\t%-30s = 0x%x,\n' % (n, v))

			f.write('};\n')

			f.write('\n')

			f.write('inline const char * EnumToString%s(int index)\n' % (p.name,))
			f.write('{\n')
			f.write('	switch(index)\n\t{\n')

			for n, v in p.codes:
				f.write('\t\tcase %s: return "%s";\n' % (n, n))

			f.write('	}\n')
			f.write('	return NULL;\n')
			f.write('}\n')

			f.write('\n')

			f.write('inline int StringToEnum%s(const char * str)\n' % (p.name,))
			f.write('{\n')

			for n, v in p.codes:
				f.write('\tif (stricmp(str, "%s") == 0) return %s;\n' % (n, n))

			f.write('	return 0;\n')
			f.write('}\n')


protocol_inl_fmt = '''
	//
	// generated source file ({sourcefile} / {templatefile})
	//

protected:
	template<typename THIS_TYPE> void AddProtocolHandlers()
	{{
{add_handlers}
	}}

public:

	// handlers
{handlers}

'''

class HandlerTemplate(object):
	def __init__(self, fn, text):
		self.filename = fn
		self.text = text

	def format(self, st):
		self.struct = st
		self.fail = False
		s = re.sub(r'(?<!{){([^{}]*)}', self.getStrOf, self.text)
		s = s.replace('{{', '{')
		s = s.replace('}}', '}')

		if self.fail: return ''
		return s

	def evalLambda(self, n):
		s = 'lambda name: ' + n
		s = s.replace('\n', '')
		return eval(s)(self.struct.name)

	def getStrOf(self, m):
		n = m.group(1)
		if n == 'name':
			return self.struct.name

		if re.match(r'^\w+$', n):
			raise RuntimeError('%s: unknown key(%s)' % (self.filename, n))

		if n.startswith('condition:'):
			if not self.evalLambda( n.split(':', 1)[1] ):
				self.fail = True
				return
			else:
				return ''

		return str( self.evalLambda(n) )

def readHandlerTemplate(pdDir, p):
	fn = os.path.join(pdDir, "handler_template_%s.h" % p.name)
	tmpl = None
	if os.path.exists(fn):
		with open(fn, 'rb') as f:
			tmpl = HandlerTemplate( fn, f.read().replace('\r', '') )

	return fn, tmpl


def generateProtocolHandlerInl(pd, pdDir, outDir):

	protocol_add_fmt = '\t\tAddProtocolHandler(0x{code:x}, static_cast<handler_type>(&THIS_TYPE::Handler_{name}));'

	for p in pd.protocols:
		fn = os.path.join(outDir, "handler_%s.inl" % p.name)

		tmpl_name, handler_def_fmt = readHandlerTemplate(pdDir, p)
		if not handler_def_fmt: continue

		print fn, '..'

		with open(fn, 'wb') as f:
			add_handlers = []
			handlers = []

			for st in p.structs:
				code = p.getCode(st.name)
				if code is None: raise RuntimeError('Protocol code not found! (%s,%s)' % (p.name, st.name))

				h = handler_def_fmt.format(st)

				if h:
					add_handlers.append(protocol_add_fmt.format(code=code, name=st.name))
					handlers.append(h)

			s = protocol_inl_fmt.format(
					sourcefile=os.path.basename(pd.filename),
					templatefile=os.path.basename(tmpl_name),
					add_handlers='\n'.join(add_handlers),
					handlers='\n'.join(handlers))

			f.write(s)


def generateCppFiles(l, outDir):
	for fn in l:
		pdDir, n = os.path.split(fn)
		n = os.path.splitext(n)[0]
		cppFile = os.path.join(outDir, n + '.inl')

		print fn, '->', cppFile
		pd = PdFile.readFile(fn)
		cppGen = CppGenerator(pd)

		with open(cppFile, 'wb') as f:
			s = cppGen.generateFile()
			f.write(s)

		generateProtocolEnumInl(pd, pdDir, outDir)
		generateProtocolHandlerInl(pd, pdDir, outDir)


if __name__ == "__main__":
	if len(sys.argv) < 3:
		print "Command Line: python GenProtocolCpp.py srcDir outDir [Files]"
		sys.exit(-1)

	#srcDir = '../../server/common/protocoldef/'
	srcDir = sys.argv[1]
	#outDir = '../../server/common/protocol/'
	outDir = sys.argv[2]
	args = sys.argv[3:]

	if args:
		generateCppFiles(args, outDir)

	else:
		generateCppFiles(glob.glob( os.path.join(srcDir, '*.pd') ), outDir)

