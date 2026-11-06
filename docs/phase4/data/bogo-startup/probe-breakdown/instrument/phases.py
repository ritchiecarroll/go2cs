import statistics as st, collections, sys
med=st.median
D='run-F-M-all'
def jitns(n):
    ns=n.split('::')[0]
    if ns.startswith(('Listener','StartupHook','dynamicClass')): return 'hook/stub'
    if ns.startswith('System.'): return 'System'
    if ns.startswith('go.crypto.tls_internal_test') or 'tls_internal_test' in ns: return 'host'
    if ns.startswith(('go.ж`1','go.StandardBox','go.FieldRefBox','go.slice`1','go.array`1','go.BoxShape','go.GoTypeOps','go.StringLiteralCache','go.builtin','go.ISupportMake','go.IGoReflect','go.SlottedStandardBox','go.map`2')): return 'golib generic/runtime'
    if ns.startswith('go.'): return 'converted std'
    return 'other'
P=collections.defaultdict(list); J=collections.defaultdict(list); L=collections.defaultdict(list); JN=collections.defaultdict(list)
for i in range(1,25):
    rows=[l.rstrip('\n').split('\t') for l in open(f'{D}/hook-{i}.tsv')]
    sync=[r for r in rows if r[0]=='SYNC'][0]; sw0,ns0,t0,freq=map(float,sync[1:5])
    us=lambda sw:((ns0-t0)/1000.0+(sw-sw0)*1e6/freq)/1000
    mk=collections.defaultdict(list)
    for l in open(f'{D}/init-{i}.tsv'):
        _,d,n,a,b,tid=l.rstrip('\n').split('\t')
        if d=='-1': mk[n].append((us(float(a)),us(float(b))))
    t=lambda k: float([r for r in rows if r[0]==k][0][1])/1000
    golib=[float(r[1])/1000 for r in rows if r[0]=='ASM' and r[3]=='golib'][0]
    ms=sorted(mk['MSET']); b=[0, t('HOOK'), golib, ms[0][0], ms[0][1]]
    pk=[float(r[1])/1000 for r in rows if r[0]=='EV145' and 'pkcs1PrivateKey' in r[4]][0]
    b+=[mk['RUN-ENTER'][0][0], mk['TESTMAIN-CALL'][0][0], t('EXIT')]
    names=['exec->hook','hook->golib load','init burst (to 1st method-set)','1st method-set (ext scan)','after scan -> Main (test pkg vars)','Main -> TestMain (preload, rescan)','TestMain -> exit']
    jit={}; open_={}
    ints=[]
    for r in rows:
        if r[0]=='EV145': open_[r[6]]=(float(r[1])/1000,r[4]+'::'+r[5],r[2])
        elif r[0]=='EV143' and r[7] in open_: s,n,tid=open_.pop(r[7]); ints.append((s,float(r[1])/1000,n,tid))
    lds=[]; ol=collections.defaultdict(list)
    for r in rows:
        if r[0]=='EV290': ol[r[2]].append(float(r[1])/1000)
        elif r[0]=='EV291' and ol[r[2]]: lds.append((ol[r[2]].pop(),float(r[1])/1000))
    for k,(a,c) in enumerate(zip(b,b[1:])):
        P[names[k]].append(c-a)
        J[names[k]].append(sum(e-s for s,e,n,tid in ints if a<=s<c and not n.startswith(('Listener','StartupHook'))))
        L[names[k]].append(sum(e-s for s,e in lds if a<=s<c))
    for k,v in collections.Counter({}).items(): pass
    c=collections.Counter()
    for s,e,n,tid in ints: c[jitns(n)]+=e-s
    for k in ['hook/stub','System','host','golib generic/runtime','converted std','other']: JN[k].append(c[k])
    P['(pkcs1 JIT at)'].append(pk); P['(exit)'].append(t('EXIT'))
for k in P: print('%-38s median %6.1f ms  jit %5.1f  loads %5.1f'%(k,med(P[k]),med(J[k]) if k in J else 0,med(L[k]) if k in L else 0))
print('JIT by owner (all threads):', {k:round(med(v),1) for k,v in JN.items()})
