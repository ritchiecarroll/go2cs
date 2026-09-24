import os, re, json, shutil
EV='/mnt/c/h10/p2/G-LAPTOP'; OUT='/mnt/c/h10/g-p2ev/docs/phase4/p2-sweep/G-LAPTOP'
os.makedirs(OUT, exist_ok=True)
# ordered: most specific first; every spelling the host can emit
BS=chr(92)
def _forms(path):
    parts=path.split("/")
    return [BS.join(parts), (BS+BS).join(parts), "/".join(parts)]
_lit=[]
for _p,_rep in [("<goroot>","<goroot>"),("<dotnet10>","<dotnet10>"),("<profile>","<profile>")]:
    for _f in _forms(_p): _lit.append((re.escape(_f),_rep))
_lit += [(re.escape("<goroot>"),"<goroot>"),(re.escape("<dotnet10>"),"<dotnet10>"),(re.escape("<profile>"),"<profile>"),("/home/[a-z]+","<home>")]
_sep="(?:"+re.escape(BS+BS)+"|"+re.escape(BS)+"|/)"
_lit.append((_sep+"Users"+_sep+"Admin","<profile>"))
SUBS=_lit
def scrub(t):
    for a,b in SUBS: t=re.sub(a,b,t,flags=re.I)
    return t
def put(rel, text):
    p=os.path.join(OUT,rel); os.makedirs(os.path.dirname(p),exist_ok=True)
    with open(p,'w',encoding='utf-8',newline='\n') as f: f.write(scrub(text))
def rd(p):
    b=open(p,'rb').read()
    if b.count(b'\x00'): raise SystemExit(f'NUL bytes in {p}')
    return b.decode('utf-8', errors='replace').replace('\r\n','\n')
top=['ledger.tsv','driver.log','driver.attempt1.log','selftest.log','analysis.txt','analyze.py','p2-driver.ps1','q2.ps1','ff-roster.ps1','env.sh','canary.ps1','canary.log','plant-refusal.log','q-symlink.log','cleanup-purge.log','cleanup-ignored.txt','ctx.py','build-evidence.py']
for f in top:
    if os.path.exists(f'{EV}/{f}'): put(f, rd(f'{EV}/{f}'))
put('REPORT.md', rd(f'{EV}/REPORT.txt'))
for f in sorted(os.listdir(f'{EV}/netqual-pre')): put(f'netqual-pre/{f}', rd(f'{EV}/netqual-pre/{f}'))
for f in sorted(os.listdir(EV)):
    if f.startswith('purge-') and f.endswith('.log'): put(f'purges/{f}', rd(f'{EV}/{f}'))
deferred=set(open(f'{EV}/deferred-rows.txt').read().split())
n=0
for d in sorted(os.listdir(f'{EV}/rows')):
    rdir=f'{EV}/rows/{d}'; pkg=d.replace('__','/')
    for f in sorted(os.listdir(rdir)):
        p=f'{rdir}/{f}'
        if os.path.isdir(p): continue
        if f=='go2cs_test_comparison.json':
            c=json.load(open(p,encoding='utf-8'))
            keep={k:c[k] for k in ['package','status','go','csharp','matched','skipped','disclosed','excluded','orphanedDisclosures','environment'] if k in c}
            put(f'rows/{d}/verdicts.json', json.dumps(keep,indent=1,ensure_ascii=False)+'\n'); n+=1
        elif f=='go2cs_test_results.json':
            if pkg in deferred: put(f'rows/{d}/go2cs_test_results.json', rd(p)); n+=1
        elif f=='go2cs_test_results.xml': continue
        else: put(f'rows/{d}/{f}', rd(p)); n+=1
print('files written:', sum(len(fs) for _,_,fs in os.walk(OUT)), 'row files:', n)
