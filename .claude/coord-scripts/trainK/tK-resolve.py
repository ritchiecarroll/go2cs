# tK-resolve.py <kind> <repo> -- scripted resolutions for TRAIN K's three proven conflicts. Exit non-zero on any
# assumption that does not hold (the union is not what the resolution was proven against).
import sys, subprocess, re
kind, repo = sys.argv[1], sys.argv[2]
def git(*a): return subprocess.run(['git','-C',repo]+list(a),capture_output=True,text=True,encoding='utf-8',errors='surrogateescape')
def blob(ref,path):
    r=git('show',f'{ref}:{path}'); assert r.returncode==0, (ref,path,r.stderr); return r.stdout
def rd(p): return open(f'{repo}/{p}',encoding='utf-8',newline='',errors='surrogateescape').read()
def wr(p,s): open(f'{repo}/{p}','w',encoding='utf-8',newline='',errors='surrogateescape').write(s)
def die(m): print('RESOLVE-FAIL',kind,m); sys.exit(3)
if kind=='K8':
    p='src/core/golib/runtime/Goroutine.cs'
    if blob('HEAD',p)!=blob('346b26c81f',p): die('union Goroutine.cs is not P2\'s blob -- the pair proof does not apply')
    s=open(sys.argv[3],encoding='utf-8',newline='').read()
    if s.count('m_profileLabels = ')!=1 or s.count('s_live[goroutine.Id] = goroutine;')!=1 or '<<<<<<<' in s: die('resolved-file asserts')
    wr(p,s); print('K8 applied: m_profileLabels= x1, s_live x1')
elif kind=='K9':
    p='src/tests/GolibTests/ThreadStateCensusTests.cs'
    s=rd(p); nl='\r\n' if '\r\n' in s else '\n'; out=[]; L=s.split(nl); i=0; blocks=0
    while i<len(L):
        if L[i].startswith('<<<<<<< '):
            m=next(j for j in range(i,len(L)) if L[j]=='======='); b=next(j for j in range(m,len(L)) if L[j].startswith('>>>>>>> '))
            ours,theirs=L[i+1:m],L[m+1:b]
            if set(ours)&set(theirs): die('census block sides share lines -- not a pure adjacent insert')
            out+=ours+theirs; i=b+1; blocks+=1
        else: out.append(L[i]); i+=1
    r=nl.join(out); rows=lambda t: len(re.findall(r'^\s*\("[^"]+\|[^"]+", Disposition\.',t,re.M))
    base=rows(blob('f819887fa3',p)); got=rows(r)
    if got!=base+2: die(f'census rows {got} != base {base} + 2 (blocks {blocks}; 0 blocks = already resolved, verified by count)')
    if 't_chunk' not in r or 't_runtimeLockProfilePending' not in r: die('a census row is missing')
    wr(p,r)
    for os_ in ('windows','linux','darwin'):
        pi=f'src/core/runtime/{os_}/package_info.cs'; u=rd(pi)
        for src,ref in (('print.go','ed7859d20e'),('runtime.go','ed7859d20e'),('chan.go','b9c8948630'),('lock_spinbit.go','b9c8948630'),('lockrank_off.go','b9c8948630')):
            pat=re.compile(r'^\[assembly: go\.GoPositionMap\("runtime/'+re.escape(src)+r'".*$',re.M)
            a=[x.rstrip(chr(13)) for x in pat.findall(u)]; want=[x.rstrip(chr(13)) for x in pat.findall(blob(ref,pi))]  # run 3: the blob comes back newline-translated, the worktree raw (CRLF)
            if len(want)!=1: continue
            if a!=want: die(f'{pi} map line for {src} is not {ref}\'s')
    print(f'K9 applied: census rows {base} -> {got}; package_info map lines match their seats x3')
elif kind=='ROSTER':
    p='docs/ConversionStrategies-Reference/manual-conversions.md'
    s=rd(p); nl='\r\n' if '\r\n' in s else '\n'
    if s.count('<<<<<<< ')!=1: die('expected exactly one conflict block')
    a=s.index('<<<<<<< '); b=s.index(nl,s.index('>>>>>>> '))+len(nl)
    para=open(sys.argv[3],encoding='utf-8').read().rstrip('\n').replace('\n',nl)+nl
    s=s[:a]+para+s[b:]
    if '<<<<<<<' in s or '>>>>>>>' in s or '=======' + nl in s[a:a+len(para)+10]: die('markers remain')
    wr(p,s); print('ROSTER paragraph applied')
else: die('unknown kind')
