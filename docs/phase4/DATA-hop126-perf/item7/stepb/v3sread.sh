#!/usr/bin/env bash
# v3sread.sh <arms.tsv>... -- §11.6's reading for a two-image window (default arms D26 vs V3; set NUM/DEN to read other ids).
# Per program: each arm's value is the mean of its A and AA medians (Startup: wall_ms, column 4; else inner_ms, column 5).
# Floor = the largest of the two arms' A/A gaps and their four IQRs; MOVED only if |ratio-1| > floor, either direction.
# Flags DISTURBED when an arm's A/A gap is 3% or more (validity). One line per program.
NUM=${NUM:-V3} DEN=${DEN:-D26}
cat "$@" | awk -F'\t' -v N="$NUM" -v D="$DEN" '$1 != "program" {v=($1=="PerfStartup")?$4:$5; x[$1"|"$2]=x[$1"|"$2]" "v; p[$1]=1}
  function med(s, a, m) { m=split(substr(s,2),a," "); asort(a); return (m%2)?a[(m+1)/2]:(a[m/2]+a[m/2+1])/2 }
  function iqr(s, a, m) { m=split(substr(s,2),a," "); asort(a); return (a[int(m*0.75)+1]-a[int(m*0.25)+1])/med(s)*100 }
  function arm(q, a,   ma, mb, s, t) { ma=med(x[q"|"a]); mb=med(x[q"|"a"-AA"]); val=(ma+mb)/2; gap=(ma>mb?ma/mb-1:mb/ma-1)*100
    s=iqr(x[q"|"a]); t=iqr(x[q"|"a"-AA"]); sp=(s>t?s:t) }
  END { for (q in p) { if (!((q"|"N) in x) || !((q"|"D) in x)) continue
      arm(q, N); vn=val; gn=gap; sn=sp; arm(q, D); vd=val; gd=gap; sd=sp
      f=gn; if (gd>f) f=gd; if (sn>f) f=sn; if (sd>f) f=sd; r=vn/vd; d=(r-1)*100
      if (gn>=3 || gd>=3) printf "DISTURBED %s A/A %s %.2f%% %s %.2f%%\n", q, N, gn, D, gd
      printf "%s\t%s/%s\t%.1f\t%.1f\t%.3f\t%+.1f%%\tfloor %.1f%%\t%s\n", q, N, D, vn, vd, r, d, f, ((d>f||-d>f)?"MOVED":"within") } }' | sort
