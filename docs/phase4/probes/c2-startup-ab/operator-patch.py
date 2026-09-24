# Arms B and L: the span operator (src/core/golib/string.cs:460) consults the table before copying.
# usage: operator-patch.py <scratch src root> [apply|revert]
import re, sys
p = sys.argv[1] + '/core/golib/string.cs'
b = open(p, 'rb').read().decode('utf-8-sig')
wired = 'return LiteralTable.Lookup(value) is { } literal ? new @string(literal) : new @string(value);'
pat = re.compile(r'(public static implicit operator @string\(ReadOnlySpan<byte> value\)\r?\n    \{\r?\n        )return (?:new @string\(value\)|LiteralTable\.Lookup\(value\) is \{ \} literal \? new @string\(literal\) : new @string\(value\));')
assert len(pat.findall(b)) == 1
b = pat.sub(lambda m: m.group(1) + (wired if sys.argv[2] == 'apply' else 'return new @string(value);'), b)
open(p, 'w', encoding='utf-8').write(b)
