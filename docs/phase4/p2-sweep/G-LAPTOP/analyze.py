# analyze.py -- S-G report inputs, OFFLINE from the preserved evidence (page readings with UTF-8 HEAD bytes)
import csv, json, os, re, sys, collections
EV='/mnt/c/h10/p2/G-LAPTOP'
FLOORS={'hash/maphash':60,'index/suffixarray':120,'crypto/dsa':120,'archive/zip':60,'go/parser':90,'crypto/internal/fips140/mlkem':30,'crypto/mlkem':30,'time':40,'crypto/tls':60,'sync/atomic':150,'net':120,'net/http':60}
VROW=re.compile(r'^\|\s*`([^`]+)`\s*\|(.*)$')
def read_page(text):
    rows={}; inv=False; n=d=None
    for line in text.replace('\r','').split('\n'):
        if re.match(r'^##\s+Verdicts\b',line): inv=True; continue
        if inv and re.match(r'^##\s',line): inv=False
        m=VROW.match(line)
        if inv and m: rows[m.group(1)]=re.sub(r'\s+',' ',m.group(2)).strip()
        if n is None:
            m=re.search(r'\*\*(\d+) matched',line)
            if m:
                n=int(m.group(1)); m2=re.search(r'(\d+) disclosed',line); d=int(m2.group(1)) if m2 else None
    return rows,n,d
def compare(head,new):
    h,hn,hd=read_page(head); w,wn,wd=read_page(new)
    moved=[k for k in h if w.get(k)!=h[k]]+[k for k in w if k not in h]
    return moved,(hn,hd),(wn,wd)
# planted non-ASCII control: a UTF-8 name that must read EQUAL against itself and MOVED when its cell changes
ctl="**2 matched · 0 disclosed**\n\n## Verdicts\n\n| `TestReadStdin/c1/r1/äöü` | pass | pass |\n| `TestPlain` | pass | pass |\n"
assert compare(ctl,ctl)[0]==[], 'control EQUAL failed'
assert compare(ctl,ctl.replace('| `TestPlain` | pass | pass |','| `TestPlain` | pass | fail |'))[0]==['TestPlain'], 'control MOVED failed'
assert compare(ctl.encode('utf-8').decode('cp437'),ctl)[0]!=[], 'control: a cp437-decoded HEAD must NOT read equal (the live-driver defect)'
print('PLANTED CONTROLS: non-ASCII EQUAL ok; cell move names TestPlain; cp437-decoded HEAD reads MOVED (defect reproduced)')
rows=list(csv.DictReader(open(f'{EV}/ledger.tsv',encoding='utf-8'),delimiter='\t'))
out=[]; sum_got=sum_banked=0; notes=collections.defaultdict(list)
for r in rows:
    pkg=r['package']; rd=f"{EV}/rows/{pkg.replace('/','__')}"; dotted=pkg.replace('/','.')
    newp=f'{rd}/page-{dotted}.md'; headp=f'{EV}/headpages/{dotted}.md'
    head=open(headp,encoding='utf-8').read() if os.path.exists(headp) else ''
    if r['page'].startswith('NOT REWRITTEN'): reading='NOT REWRITTEN (=EQUAL, COORD ruling)'; moved=[]; wn=wd=None
    elif os.path.exists(newp):
        moved,(hn,hd),(wn,wd)=compare(head,open(newp,encoding='utf-8').read())
        reading='MOVED' if moved or (hn,hd)!=(wn,wd) else 'EQUAL(rewritten)'
    else: reading='ABSENT'; moved=[]; wn=wd=None
    rec={}
    cp=f'{rd}/go2cs_test_comparison.json'
    if os.path.exists(cp):
        c=json.load(open(cp,encoding='utf-8')); rec={'orph':[o['name'] for o in (c.get('orphanedDisclosures') or [])],'env':c.get('environment',{})}
    got=int(r['got']) if r['got'] else 0; banked=int(r['bankedTests']); sum_got+=got; sum_banked+=banked
    wall=int(r['sweep_s']) if r['sweep_s'] else None
    fl=FLOORS.get(pkg)
    wallflag=''
    if fl and wall is not None and wall>=0.75*fl*60: wallflag=f'FLOOR-BREACH {wall}s >= 0.75x{fl}m'
    elif fl: wallflag=f'floor {fl}m: {wall}s = {wall/(fl*60):.2f}x'
    elif wall is not None and wall>=450: wallflag=f'UNFLOORED >=450s: {wall}s'
    out.append((pkg,r['word'],got,r['gotDisclosed'],banked,r['bankedDisclosed'],r['absorption'],reading,moved,wn,wd,r['drift'],wall,wallflag,rec.get('orph',[]),rec.get('env',{})))
print(f'ROWS {len(out)}  words={dict(collections.Counter(o[1] for o in out))}')
print(f'SUM got={sum_got}  SUM banked Tests={sum_banked}  diff={sum_got-sum_banked}')
for o in out:
    if o[2]!=o[4]: print(f'  got!=banked: {o[0]} got={o[2]} banked={o[4]} word={o[1]}')
print('ABSORPTIONS:', [(o[0],o[6]) for o in out if o[6]!='none'] or 'none')
print('PAGES:', dict(collections.Counter(o[7] for o in out)))
for o in out:
    if o[7]=='MOVED': print(f'  MOVED {o[0]}: names={o[8]} page N/D={o[9]}/{o[10]} run got/D={o[2]}/{o[3]}')
print('DRIFT:', dict(collections.Counter(o[11] for o in out)))
for o in out:
    if o[11]!='clean': print(f'  drift {o[0]}: {o[11]}')
print('WALLS (floored + unfloored>=450s):')
for o in out:
    if o[13] and (o[0] in FLOORS or 'UNFLOORED' in o[13]): print(f'  {o[0]}: {o[13]}')
print('ORPHANED DISCLOSURES:', [(o[0],o[14]) for o in out if o[14]] or 'none')
envs=collections.Counter(json.dumps(o[15],sort_keys=True) for o in out); print('ENVIRONMENTS:', dict(envs))
