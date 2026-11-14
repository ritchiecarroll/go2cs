import collections,sys
rows=[]; cur=None
for l in open(sys.argv[1]):
    f=l.rstrip('\n').split('\t')
    if f[0]=='mark': cur=f[1].replace('begin ',''); continue
    if f[0]=='jit' and len(f)>=6: rows.append((cur,f[1],f[2],float(f[3]),int(f[4]),f[5]))
th=collections.Counter(r[5] for r in rows)
main=th.most_common(1)[0][0]
print("threads:",[(t,n,round(sum(r[3] for r in rows if r[5]==t),1)) for t,n in th.most_common(4)])
def kind(ns,m):
    base=ns.split('`')[0].split('[')[0]
    if ns.startswith('System.Diagnostics.Tracing') or ns.startswith('StartupHook'): return 'z listener artifact'
    if ns=='<Module>': return 'a module initializer'
    if base.endswith('_package') and m=='.cctor': return 'b package static constructor'
    if '_package' in base: return 'c package functions and nested types'
    if base.startswith('go.'):
        if '`' in ns or '[' in ns or '[' in m:
            return 'd golib generic over a package type' if '_package' in ns+m else 'd golib generic over golib or framework types'
        return 'e golib non-generic'
    if base.startswith('System.Reflection') or base.startswith('System.RuntimeType') or 'CustomAttribute' in base: return 'f framework reflection'
    if '`' in ns or '[' in m: return 'g framework generic instantiation'
    return 'h framework other'
k=collections.defaultdict(lambda:[0,0.0,0])
for p,ns,m,ms,il,t in rows:
    if t!=main or p in (None,'end'): continue
    e=k[kind(ns,m)]; e[0]+=1; e[1]+=ms; e[2]+=il
tot=sum(e[1] for n,e in k.items() if not n.startswith('z'))
for n in sorted(k): e=k[n]; print(f"{n[2:]:38}{e[0]:6d} methods {e[1]:8.1f} ms {100*e[1]/tot:5.1f}%  il {e[2]:7d}")
# golib generic detail
g=collections.Counter(); gc=collections.Counter()
for p,ns,m,ms,il,t in rows:
    if t==main and kind(ns,m).startswith('d'): g[ns.split('`')[0].split('[')[0]]+=ms; gc[ns.split('`')[0].split('[')[0]]+=1
print("golib generic types:",[(n,gc[n],round(ms,1)) for n,ms in g.most_common(10)])
g=collections.Counter(); gc=collections.Counter()
for p,ns,m,ms,il,t in rows:
    if t==main and kind(ns,m)[0] in 'fgh': g[ns.split('`')[0].split('[')[0]]+=ms; gc[ns.split('`')[0].split('[')[0]]+=1
print("framework types:",[(n,gc[n],round(ms,1)) for n,ms in g.most_common(16)])
