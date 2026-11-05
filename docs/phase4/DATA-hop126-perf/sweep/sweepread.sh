#!/usr/bin/env bash
# sweepread.sh <arms.tsv>... -- the sweep's section 8 reading. Per program: each arm's value is the mean of its A and AA
# medians (Startup: wall_ms, column 4; else inner_ms, column 5). For each ratio (JH/JP, RH/RP, JD/JP, RD/RP, JG/JP) the floor
# is the largest of its two arms' A/A gaps and their four IQRs; the cell MOVES only if |ratio-1| > floor, either direction.
# Also flags any arm whose A/A gap is 3% or more (disturbed, protocol v3's bound). Prints one line per program x ratio.
cat "$@" | awk -F'\t' '$1 != "program" {v=($1=="PerfStartup")?$4:$5; x[$1"|"$2]=x[$1"|"$2]" "v; p[$1]=1}
  function med(s, a, m) { m=split(substr(s,2),a," "); asort(a); return (m%2)?a[(m+1)/2]:(a[m/2]+a[m/2+1])/2 }
  function iqr(s, a, m) { m=split(substr(s,2),a," "); asort(a); return (a[int(m*0.75)+1]-a[int(m*0.25)+1])/med(s)*100 }
  END {
    n=split("JP JH RP RH JD RD JG", ARM, " ")
    for (q in p) {
      for (i=1;i<=n;i++) { a=ARM[i]; k=q"|"a; ka=q"|"a"-AA"; if (!(k in x) || !(ka in x)) continue
        ma=med(x[k]); mb=med(x[ka]); val[q,a]=(ma+mb)/2; gap[q,a]=(ma>mb?ma/mb-1:mb/ma-1)*100
        s=iqr(x[k]); t=iqr(x[ka]); sp[q,a]=(s>t?s:t)
        if (gap[q,a] >= 3) printf "DISTURBED %s %s A/A %.2f%%\n", q, a, gap[q,a] }
      m=split("JH/JP RH/RP JD/JP RD/RP JG/JP", R, " ")
      for (j=1;j<=m;j++) { split(R[j], z, "/"); A1=z[1]; B1=z[2]; if (!((q,A1) in val) || !((q,B1) in val)) continue
        r=val[q,A1]/val[q,B1]; f=gap[q,A1]; if (gap[q,B1]>f) f=gap[q,B1]; if (sp[q,A1]>f) f=sp[q,A1]; if (sp[q,B1]>f) f=sp[q,B1]
        d=(r-1)*100; printf "%s\t%s\t%.1f\t%.1f\t%.3f\t%+.1f%%\tfloor %.1f%%\t%s\n", q, R[j], val[q,A1], val[q,B1], r, d, f, ((d>f||-d>f)?"MOVED":"within") }
    }
  }' | sort -t$'\t' -k2,2 -k1,1
