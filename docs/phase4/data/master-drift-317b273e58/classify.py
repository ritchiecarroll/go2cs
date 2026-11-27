import subprocess,collections,sys
import os; S=os.environ['DRIFT_DIR']  # holds code.lst; table.tsv is written there
R=os.environ['DRIFT_ROOT']  # the emitted core, with a trailing slash
rows=[]; cnt=collections.Counter()
for f in open(S+'/code.lst').read().split():
    d=subprocess.run(['diff','core/'+f,R+f],capture_output=True,text=True).stdout.split('\n')
    m=collections.Counter();p=collections.Counter();cl=set()
    for l in d:
        if not l[:2] in ('< ','> '): continue
        b=l[2:].rstrip('\r')
        if 'GoPositionMap' in b: cl.add('posmap'); continue
        if 'blank import:' in b or 'import _ "' in b: cl.add('blank-import'); continue
        n=b.replace('builtin.','')
        if n!=b: cl.add('builtin')
        (m if l[0]=='<' else p)[n]+=1
    res=(m-p)+(p-m)
    if res:
        txt=' '.join(res)
        if 'chᴛ' in txt or 'selᴛ13' in txt: cl.add('select-temp')
        elif '([' in txt or '.array()' in txt or ']);' in txt or '}.array();' in txt: cl.add('array-literal')
        else: cl.add('UNEXPLAINED')
    k='+'.join(sorted(cl)); cnt[k]+=1; rows.append((f,k))
for k,v in sorted(cnt.items(),key=lambda x:-x[1]): print(v,k)
open(S+'/table.tsv','w').write('\n'.join(f+'\t'+k for f,k in rows)+'\n')
