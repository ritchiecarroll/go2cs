import glob,statistics,sys
runs=[]
for f in sorted(glob.glob(sys.argv[1])):
    runs.append({l.split('\t')[1]:l.rstrip('\n').split('\t') for l in open(f) if l.startswith('init')})
names=list(runs[0])
med=lambda n,i:statistics.median(float(r[n][i]) for r in runs)
tot=lambda i:statistics.median(sum(float(v[i]) for v in r.values()) for r in runs)
print(f"runs {len(runs)} packages {len(names)}  total wall {tot(2):.1f}  jit {tot(3):.1f}  load {tot(7):.1f}  methods {tot(4):.0f}  il {tot(5):.0f}  allocKB {tot(6)/1024:.0f}")
rows=sorted(names,key=lambda n:-med(n,2))
print(f"{'assembly':34}{'wall':>8}{'jit':>8}{'rest':>7}{'meth':>6}{'il':>8}{'allocKB':>9}{'load':>7}")
cum=0
for n in rows[:int(sys.argv[2])]:
    w=med(n,2); j=med(n,3); cum+=w
    print(f"{n:34}{w:8.1f}{j:8.1f}{w-j:7.1f}{med(n,4):6.0f}{med(n,5):8.0f}{med(n,6)/1024:9.0f}{med(n,7):7.1f}   cum {100*cum/tot(2):.0f}%")
