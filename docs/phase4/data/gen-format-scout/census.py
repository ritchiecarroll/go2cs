import csv,collections,re,json,sys
rows=list(csv.DictReader(open(sys.argv[1] if len(sys.argv)>1 else 'files.tsv'),delimiter='\t'))
rep=json.load(open(sys.argv[2] if len(sys.argv)>2 else 'report.json'))
fmt=collections.Counter()
for d in rep:
    p=re.sub(r'^.*?/(?=(core|tests)/)','',d['FileName'])
    fmt[p]+=len(d['FileChanges'])
kinds=['kr','misalign','depth','body','tabs','trailing','mixed_eol','blank_runs','normalize_changed']
agg=collections.defaultdict(collections.Counter)
def gen(p): return re.search(r'go2cs-gen/go2cs\.([A-Za-z]+)/',p).group(1)
for r in rows:
    g=gen(r['path']); a=agg[g]; a['files']+=1; a['lines']+=int(r['lines'])
    for k in kinds:
        v=int(r[k]); a[k]+=v; a[k+'_f']+= v>0
    f=fmt.get(r['path'],0); a['fmt']+=f; a['fmt_f']+= f>0
    a['any_f']+= (f>0 or any(int(r[k])>0 for k in kinds if k!='normalize_changed'))
tot=collections.Counter()
hdr=['generator','files','lines','fmt_files','fmt_diags','kr','misalign','depth','body','trailing','mixed_eol','blank_runs','tabs','normalize_files']
print('\t'.join(hdr))
for g,a in sorted(agg.items(), key=lambda x:-x[1]['files']):
    vals=[g,a['files'],a['lines'],a['fmt_f'],a['fmt'],f"{a['kr']}/{a['kr_f']}",f"{a['misalign']}/{a['misalign_f']}",f"{a['depth']}/{a['depth_f']}",f"{a['body']}/{a['body_f']}",f"{a['trailing']}/{a['trailing_f']}",f"{a['mixed_eol']}/{a['mixed_eol_f']}",f"{a['blank_runs']}/{a['blank_runs_f']}",f"{a['tabs']}/{a['tabs_f']}",a['normalize_changed_f']]
    print('\t'.join(map(str,vals)))
    tot.update(a)
print('\t'.join(map(str,['TOTAL',tot['files'],tot['lines'],tot['fmt_f'],tot['fmt'],f"{tot['kr']}/{tot['kr_f']}",f"{tot['misalign']}/{tot['misalign_f']}",f"{tot['depth']}/{tot['depth_f']}",f"{tot['body']}/{tot['body_f']}",f"{tot['trailing']}/{tot['trailing_f']}",f"{tot['mixed_eol']}/{tot['mixed_eol_f']}",f"{tot['blank_runs']}/{tot['blank_runs_f']}",f"{tot['tabs']}/{tot['tabs_f']}",tot['normalize_changed_f']])))
print('files flagged by dotnet format:',tot['fmt_f'],' by any genfmt layout arm or format:',tot['any_f'])
