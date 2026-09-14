import io, re, sys

p = sys.argv[1]
s = io.open(p, encoding='utf-8', newline='').read()
n0 = len(s)

# 1. Profile paths: C:\Users\<name>\..., /c/Users/<name>/..., /Users/<name>/... -> <user>
prof = re.compile(r'(Users[\\/])([A-Za-z0-9._-]+)')
def rep_prof(m):
    if m.group(2) == '<user>':
        return m.group(0)
    return m.group(1) + '<user>'
s, c1 = prof.subn(rep_prof, s)

# 2. DOMAIN\user spellings (a Windows domain-qualified account), excluding drive letters and well-known principals.
keep = {'BUILTIN', 'NT', 'AUTHORITY', 'HKLM', 'HKCU', 'HKCR', 'HKU'}
dom = re.compile(r'\b([A-Z][A-Z0-9-]{2,})\\([A-Za-z][A-Za-z0-9._-]+)\b')
def rep_dom(m):
    d, u = m.group(1), m.group(2)
    if d in keep or u == '<user>':
        return m.group(0)
    return '<DOMAIN>\\<user>'
s, c2 = dom.subn(rep_dom, s)

io.open(p, 'w', encoding='utf-8', newline='').write(s)
print('profile-path replacements:', c1, 'domain-user replacements:', c2, 'bytes', n0, '->', len(s))
