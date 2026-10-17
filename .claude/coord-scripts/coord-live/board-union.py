p='docs/phase4/BOARD-next-validation-candidates.md'
t=open(p,encoding='utf-8',newline='').read()
out=[];state=0;n=0
for line in t.splitlines(keepends=True):
    if line.startswith('<<<<<<< '): state=1; n+=1; continue
    if state==1 and line.startswith('======='): state=2; continue
    if state==2 and line.startswith('>>>>>>> '): state=0; continue
    out.append(line)
open(p,'w',encoding='utf-8',newline='').write(''.join(out)); print('board hunks unioned', n)
