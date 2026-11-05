import sys, statistics as st
for lab in sys.argv[1:]:
    rows=[l.split(' ',5) for l in open(f"{__import__('os').environ.get('KIT','.')}/launch-{lab}/shim.log")]
    rows=rows[2:]  # drop the warm-up invocation (its probe and its handshake)
    for kind,pred in (('probe',lambda a:'-is-handshaker-supported' in a),('handshake',lambda a:'-port' in a)):
        ms=[int(r[2]) for r in rows if pred(r[5])]
        bad=[r for r in rows if pred(r[5]) and r[3]!='0']
        q=st.quantiles(ms,n=4)
        print(f'{lab:3} {kind:9} n={len(ms)} median={st.median(ms)/1000:.2f}s min={min(ms)/1000:.2f} p25={q[0]/1000:.2f} p75={q[2]/1000:.2f} max={max(ms)/1000:.2f} nonzero-rc={len(bad)} tier={rows[0][4]}' )
