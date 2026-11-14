# multi.py ROUNDS -- label=ENV1=V,ENV2=V:cmd ... ; interleaves the arms round-robin, reports median/min wall and cpu per arm
import os,statistics,subprocess,sys,time
rounds=int(sys.argv[1]); arms=[]
for a in sys.argv[3:]:
    head,cmd=a.split(':',1); label,_,envs=head.partition('=')
    env=dict(os.environ)
    for kv in filter(None,envs.split(',')):
        k,v=kv.split('=',1); env[k]=v
    arms.append((label,cmd,env,[],[]))
for i in range(rounds+2):
    order=arms if i%2==0 else arms[::-1]
    for label,cmd,env,w,c in order:
        t0=time.perf_counter(); p=subprocess.Popen([cmd],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,env=env); _,st,ru=os.wait4(p.pid,0); el=(time.perf_counter()-t0)*1000
        assert st==0,(label,st)
        if i>=2: w.append(el); c.append((ru.ru_utime+ru.ru_stime)*1000)
base=statistics.median(arms[0][3])
for label,cmd,env,w,c in arms:
    print(f"{label:28} wall median {statistics.median(w):6.0f} (min {min(w):4.0f})  delta {statistics.median(w)-base:+6.0f}  cpu {statistics.median(c):5.0f}")
