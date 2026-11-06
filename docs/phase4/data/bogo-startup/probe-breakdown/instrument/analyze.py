#!/usr/bin/env python3
"""Per-assembly attribution of one probe launch's start-up, from the hook TSV (+ the init TSV when present).
Timeline on the main thread, innermost interval wins: LOAD(asm) inside JIT(method) inside INIT(pkg).
usage: analyze.py <run dir> [first] ; prints per-run totals and writes per-assembly medians to <run dir>/asm.tsv"""
import sys, os, glob, statistics as st, collections, re

def short(n): return n.split(',')[0].strip()

def one(hookf, initf):
    rows=[l.rstrip('\n').split('\t') for l in open(hookf)]
    sync=[r for r in rows if r[0]=='SYNC'][0]; sw0,ns0,t0,freq=map(float,sync[1:5])
    us_sw=lambda sw:(ns0-t0)/1000.0+(sw-sw0)*1e6/freq
    hook=float([r for r in rows if r[0]=='HOOK'][0][1]); exit_=float([r for r in rows if r[0]=='EXIT'][0][1])
    iv=[]  # (start,end,kind,name,tid)
    open_load=collections.defaultdict(list); jit_start={}; type_start={}
    n_jit=0; jit_ns=collections.Counter(); jit_t=collections.Counter(); asmloads=0
    for r in rows:
        k=r[0]
        if k=='EV290': open_load[r[2]].append((float(r[1]),short(r[4])))
        elif k=='EV291' and open_load[r[2]]:
            s,n=open_load[r[2]].pop(); iv.append((s,float(r[1]),'LOAD',n,r[2])); asmloads+=1
        elif k=='EV73': type_start[(r[2],r[4])]=float(r[1])
        elif k=='EV74' and (r[2],r[4]) in type_start:
            iv.append((type_start.pop((r[2],r[4])),float(r[1]),'TYPE',r[5],r[2]))
        elif k=='EV145': jit_start[r[6]]=(float(r[1]),r[4],r[5],r[2])
        elif k=='EV143' and r[7] in jit_start:
            s,ns,mn,tid=jit_start.pop(r[7]); iv.append((s,float(r[1]),'JIT',ns+'::'+mn,tid)); n_jit+=1
    asm=[r for r in rows if r[0]=='ASM']
    if initf and os.path.exists(initf):
        for l in open(initf):
            _,d,name,a,b,tid=l.rstrip('\n').split('\t')
            iv.append((us_sw(float(a)),us_sw(float(b)),'INIT',name,'init'))
    # main thread = the thread with most LOAD time
    lt=collections.Counter()
    for s,e,k,n,t in iv:
        if k=='LOAD': lt[t]+=e-s
    main=lt.most_common(1)[0][0] if lt else None
    jt=collections.Counter(t for s,e,k,n,t in iv if k=='JIT')
    seq=sorted([x for x in iv if x[4] in (main,'init')], key=lambda x:(x[0],-x[1]))
    # sweep: innermost (latest started still open) wins; heap of open intervals keyed by -start, lazy delete
    import heapq
    evs=[]
    for idx,x in enumerate(seq):
        evs.append((x[0],1,idx)); evs.append((x[1],0,idx))
    evs.sort()
    excl=collections.Counter(); cause=collections.Counter()
    heap=[]; iheap=[]; closed=set(); prev=hook
    def top(h):
        while h and h[0][1] in closed: heapq.heappop(h)
        return seq[h[0][1]] if h else None
    def account(a,b):
        a=max(a,hook); b=min(b,exit_)
        if b<=a: return
        best=top(heap); key=('OUT','outside') if best is None else (best[2],best[3])
        excl[key]+=b-a
        ini=top(iheap)
        if ini is not None: cause[ini[3]]+=b-a
    for t,kind,idx in evs:
        account(prev,t); prev=max(prev,t)
        if kind==1:
            heapq.heappush(heap,(-seq[idx][0],idx))
            if seq[idx][2]=='INIT': heapq.heappush(iheap,(-seq[idx][0],idx))
        else: closed.add(idx)
    account(prev,exit_)
    return dict(hook=hook,exit=exit_,excl=excl,cause=cause,n_jit=n_jit,asmloads=asmloads,nasm=len(asm),
                jit_threads=dict(jt),main=main,iv=iv)

def main():
    d=sys.argv[1]; res=[]
    for hf in sorted(glob.glob(f'{d}/hook-*.tsv'), key=lambda p:int(re.findall(r'(\d+)\.tsv',p)[0])):
        i=re.findall(r'(\d+)\.tsv',hf)[0]
        if i=='0': continue
        res.append(one(hf, f'{d}/init-{i}.tsv'))
    if not res: print('no runs'); return
    med=lambda xs: st.median(xs) if xs else 0
    print(f'runs={len(res)} hook_us={med([r["hook"] for r in res]):.0f} exit_us={med([r["exit"] for r in res]):.0f} '
          f'asm(ASM events)={med([r["nasm"] for r in res])} loads(290/291)={med([r["asmloads"] for r in res])} jitted={med([r["n_jit"] for r in res])}')
    print('jit threads (run 1):', res[0]['jit_threads'], 'main', res[0]['main'])
    kinds=collections.defaultdict(list)
    for r in res:
        kk=collections.Counter()
        for (k,n),v in r['excl'].items(): kk[k]+=v
        for k in ('OUT','LOAD','JIT','TYPE','INIT'): kinds[k].append(kk[k])
    for k,v in kinds.items(): print(f'  exclusive {k:5} median {med(v)/1000:8.1f} ms')
    # per-assembly medians: load, init-code, jit-inside-init, total caused
    names=set()
    for r in res:
        for (k,n) in r['excl']:
            if k in('LOAD','INIT'): names.add(n)
        names|=set(r['cause'])
    out=[]
    for n in names:
        load=med([r['excl'].get(('LOAD',n),0) for r in res])
        init=med([r['excl'].get(('INIT',n),0) for r in res])
        caused=med([r['cause'].get(n,0) for r in res])
        out.append((n,load,init,caused))
    with open(f'{d}/asm.tsv','w') as f:
        f.write('assembly\tload_us\tinit_code_us\tinside_init_us\n')
        for n,l,i,c in sorted(out,key=lambda x:-(x[1]+x[3])): f.write(f'{n}\t{l:.0f}\t{i:.0f}\t{c:.0f}\n')
    # JIT by namespace, run-summed over median run? use run 1..: aggregate across runs / n
    jc=collections.Counter(); jn=collections.Counter()
    for r in res:
        for s,e,k,n,t in r['iv']:
            if k=='JIT': jc[n.split('::')[0]]+=(e-s)/len(res); jn[n.split('::')[0]]+=1/len(res)
    tc=collections.Counter(); tn=collections.Counter()
    for r in res:
        for (k,n),v in r['excl'].items():
            if k=='TYPE':
                root=re.sub(r'[`\[+].*','',n); tc[root]+=v/len(res); tn[root]+=1/len(res)
    if tc:
        with open(f'{d}/types.tsv','w') as f:
            f.write('type_root\texcl_us_per_run\tintervals_per_run\n')
            for n,v in tc.most_common(): f.write(f'{n}\t{v:.0f}\t{tn[n]:.1f}\n')
    with open(f'{d}/jit.tsv','w') as f:
        f.write('type\tjit_us_per_run\tmethods_per_run\n')
        for n,v in jc.most_common(): f.write(f'{n}\t{v:.0f}\t{jn[n]:.1f}\n')
main()
