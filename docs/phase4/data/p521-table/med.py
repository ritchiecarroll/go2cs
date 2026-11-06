import re,statistics as st,sys
rows={}
text=open(sys.argv[1],newline='').read().replace('\r',' ')
for l in text.split('\n'):
    m=re.match(r'round (\d+) (\w+) (.*)',l.strip())
    if not m or m.group(1)=='0': continue
    t=m.group(3).split()
    kv={t[i]:t[i+1] for i in range(0,len(t)-1,2)}
    rows.setdefault(m.group(2),[]).append(kv)
for arm,rs in rows.items():
    out=[arm, f"n={len(rs)}"]
    for k in ['p521_first_us','p521_second_us','p521_steady_median_us','jit_first_us','jit_first_methods','p256_first_us']:
        v=[int(r[k]) for r in rs if k in r]
        if v: out.append(f"{k}={st.median(v):.0f} [{min(v)}..{max(v)}]")
    fs=[int(r['p521_first_us'])-int(r['p521_second_us'])-int(r.get('jit_first_us',0)) for r in rs]
    out.append(f"first-second-jit={st.median(fs):.0f} [{min(fs)}..{max(fs)}]")
    print('\n  '.join(out))
