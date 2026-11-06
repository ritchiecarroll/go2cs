#!/usr/bin/env bash
# r2ruread.sh <tsv>... -- section 10.x: R2Ru (GoReadyToRunUserCode=true) against R2R, window 1's samples (w1-r2ru-*.tsv).
# No per-arm A/A was taken, so a cell's floor is the larger of its two arms' IQRs and window 1's JIT A/A null (2.8%).
# Value = median (Startup: wall_ms; else inner_ms). MOVED only if |ratio-1| > floor, either direction.
cat "$@" | awk -F'\t' -v null=2.8 '$1 != "program" {v=($1=="PerfStartup")?$4:$5; x[$1"|"$2]=x[$1"|"$2]" "v; p[$1]=1}
  function med(s, a, m) { m=split(substr(s,2),a," "); asort(a); return (m%2)?a[(m+1)/2]:(a[m/2]+a[m/2+1])/2 }
  function iqr(s, a, m) { m=split(substr(s,2),a," "); asort(a); return (a[int(m*0.75)+1]-a[int(m*0.25)+1])/med(s)*100 }
  END { for (q in p) { a=med(x[q"|R2Ru"]); b=med(x[q"|R2R"]); r=a/b; f=null; i1=iqr(x[q"|R2Ru"]); i2=iqr(x[q"|R2R"]); if (i1>f) f=i1; if (i2>f) f=i2
        n=split(substr(x[q"|R2R"],2),t," "); d=(r-1)*100
        printf "%s\tR2Ru/R2R\t%.1f\t%.1f\t%.3f\t%+.1f%%\tfloor %.1f%%\tn=%d\t%s\n", q, a, b, r, d, f, n, ((d>f||-d>f)?"MOVED":"within") } }' | sort
