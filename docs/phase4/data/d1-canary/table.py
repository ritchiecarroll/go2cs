import csv, os, statistics
S=os.path.dirname(os.path.abspath(__file__))
i9={}
for r in csv.reader(open(S+'/i9-go12610.tsv'),delimiter='\t'): i9[r[0]]=min(int(r[1]), i9.get(r[0],10**9))
res={}; order=[]
for r in csv.reader(open(S+'/timings.tsv'),delimiter='\t'):
    if r[0]=='DONE' or len(r)<3: continue
    res[r[0]]=(int(r[1]),int(r[2]),r[3],r[4].strip() if len(r)>4 else ''); order.append(r[0])
cold={'sync'}
prev=None
purged=[l.split()[3] for l in open(S+'/purges.txt')]
for i,p in enumerate(order):
    if i and order[i-1] in purged: cold.add(p)
rank={}
for l in open(S+'/ranked.tsv'): n,p,f,s=l.rstrip('\n').split('\t'); rank[p]=(int(n),f,int(s))
for l in open(S+'/adds.tsv'): n,p,f,s=l.rstrip('\n').split('\t'); rank[p]=(int(n),f,int(s))
F=1.26
rows=[]
for p in order:
    rc,s,fam,v=res[p]
    est_lin=round(s*F)
    est_w=round(i9[p]/0.415) if p in i9 else None
    rows.append((rank[p][0],p,fam,rank[p][2],rc,s,'cold' if p in cold else '',i9.get(p,''),est_lin,est_w if est_w else ''))
with open(S+'/table.tsv','w') as o:
    o.write('rank\tpackage\tfamily\tscore\trc\tlinux_s\tcold\ti9_go12610_s\ti7_est_from_linux\ti7_est_from_i9\n')
    for r in sorted(rows): o.write('\t'.join(map(str,r))+'\n')
print(open(S+'/table.tsv').read())
