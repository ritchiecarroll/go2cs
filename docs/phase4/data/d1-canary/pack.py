import csv, os
S=os.path.dirname(os.path.abspath(__file__))
t=list(csv.DictReader(open(S+'/table.tsv'),delimiter='\t'))
warm={r[0]:int(r[2]) for r in csv.reader(open(S+'/timings-warm.tsv'),delimiter='\t') if len(r)>2}
F=1.26; BUDGET=2700; CAP=600
EXC={'internal/runtime/gc/scan':'host-dependent red at master here (AVX-512 path), score 1'}
WIN_OVERRIDE={'internal/poll'}  # linux-only cost: 1844 s here vs 17 s i9 windows
OVERCATCH={'plugin','internal/testenv','image/jpeg','internal/strconv','hash/crc32','encoding/binary','internal/zstd'}
used=0; chosen=[]; dec={}; est={}; note={}; info={}
NAMED=['sync','runtime-adj','time','context','net/http','io','fmt','net']
rows=sorted(t,key=lambda r:int(r['rank']))
for r in rows:
    p=r['package']; lin=warm.get(p,int(r['linux_s']))
    e=round(int(r['i9_go12610_s'])/0.415) if p in WIN_OVERRIDE else round(lin*F)
    n=''
    if p in warm: n='warm re-measure (first read %s s cold)'%r['linux_s']
    elif r['cold']: n='read cold after a disk purge'
    if p in WIN_OVERRIDE: n='estimate from i9 windows 17 s / 0.415: linux-only cost here'
    if r['rc']!='0': n=(n+'; ' if n else '')+'RED at master on this box'
    est[p]=e; note[p]=n; info[p]=(r['rank'],r['family'],int(r['score']),lin)
    if p in EXC: dec[p]='out: '+EXC[p]
    elif e>CAP: dec[p]='out: over 10 i7-min alone'
fam_top={}
for r in rows:
    p=r['package']
    if r['family'] in NAMED and p not in dec and r['family'] not in fam_top: fam_top[r['family']]=p
def take(p,tag):
    global used
    if p in dec: return
    if used+est[p]<=BUDGET: used+=est[p]; chosen.append(p); dec[p]='IN %s (cum %d)'%(tag,used)
p1=[r['package'] for r in rows if r['family'] in NAMED and (int(r['score'])>=20 or fam_top.get(r['family'])==r['package'])]
p2=[r['package'] for r in rows if r['family'] in NAMED or 'judgement' in r['family']]
p3=[r['package'] for r in rows if r['package'] not in OVERCATCH]
for p in p1: take(p,'p1')
for p in p2: take(p,'p2')
for p in p3: take(p,'p3')
for p in [r['package'] for r in rows]: take(p,'p4')
out=[]
for r in rows:
    p=r['package']; rk,fa,sc,lin=info[p]
    out.append((rk,p,fa,sc,lin,est[p],dec.get(p,'out: budget'),note[p]))
with open(S+'/plan.tsv','w') as o:
    o.write('rank\tpackage\tfamily\tscore\tlinux_s\ti7_est_s\tdecision\tnote\n')
    for x in out: o.write('\t'.join(map(str,x))+'\n')
open(os.path.join(S,'..','..','..','..','src','d1-canary-set.txt'),'w').write('\n'.join(sorted(chosen, key=lambda p: int(info[p][0])))+'\n')
print(len(chosen),'rows',used,'i7-s',round(used/60,1),'min')
for x in out: print('\t'.join(map(str,x)))
