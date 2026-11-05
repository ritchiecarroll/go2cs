#!/usr/bin/env bash
# v3read.sh <sm-aot.tsv> <label> -- protocol v3's reading: arm = mean of A and AA medians; floor per ratio = max(two arms'
# A/A gaps, their four IQRs); a ratio is MOVED only if |r-1| > floor. Also the v3 validity bound (every A/A < 3%).
awk -F'\t' -v lab="$2" 'NR>1 {x[$2]=x[$2]" "$5}
  function med(s, a, m) { m=split(substr(s,2),a," "); asort(a); return (m%2)?a[(m+1)/2]:(a[m/2]+a[m/2+1])/2 }
  function iqr(s, a, m) { m=split(substr(s,2),a," "); asort(a); return (a[int(m*0.75)+1]-a[int(m*0.25)+1])/med(s)*100 }
  END {
    split("P H C0 C", A, " ")
    for (i=1;i<=4;i++) { k=A[i]; ma=med(x[k]); mb=med(x[k"-AA"]); v[k]=(ma+mb)/2; aa[k]=(ma>mb?ma/mb:mb/ma-0)*100-100; if (ma<mb) aa[k]=(mb/ma-1)*100; else aa[k]=(ma/mb-1)*100
      q[k]=iqr(x[k]); if (iqr(x[k"-AA"])>q[k]) q[k]=iqr(x[k"-AA"]); ok=(aa[k]<3)?"ok":"VOID"
      printf "%s %-2s value %.1f  A/A %.2f%% (%s)  maxIQR %.1f%%\n", lab, k, v[k], aa[k], ok, q[k] }
    split("H/P C/C0 C0/P", R, " ")
    for (j=1;j<=3;j++) { split(R[j], p, "/"); r=v[p[1]]/v[p[2]]; f=aa[p[1]]; if (aa[p[2]]>f) f=aa[p[2]]; if (q[p[1]]>f) f=q[p[1]]; if (q[p[2]]>f) f=q[p[2]]
      d=(r-1)*100; printf "%s %-5s %.3f  floor %.1f%%  %s\n", lab, R[j], r, f, ((d>f||-d>f)?"MOVED":"within") }
  }' "$1"
