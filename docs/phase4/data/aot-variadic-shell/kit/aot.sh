#!/usr/bin/env bash
# Paths: $KIT is the lane box's kit root and $TREES its worktree root (both box-local; set them to rerun).
# Native AOT read of one program in one tree. usage: aot.sh <tree> <label> <B:Behavioral dir | P:Performance dir> ...
# Behavioral: a scratch copy _p1avAot_<dir> with PublishAot added (source from the tip tree p1av), Go vs AOT diff.
# Performance: PerfAot publish of the program as is. Prints the AOT exe size. Removes the scratch copy and bin/obj.
source $KIT/p1es/env.sh >/dev/null 2>&1
W=$1; L=$2; shift 2; O=$KIT/p1av/aot-$L; mkdir -p $O
echo "== $L at $(git -C $W rev-parse --short=10 HEAD) $(date -u +%T)"
for spec in "$@"; do
  kind=${spec%%:*}; d=${spec#*:}
  if [ $kind = B ]; then
    S=$W/src/tests/Behavioral/_p1avAot_$d; rm -rf $S; cp -r $TREES/p1av/src/tests/Behavioral/$d $S; rm -rf $S/bin $S/obj
    sed -i '0,/<OutputType>Exe<\/OutputType>/s//<OutputType>Exe<\/OutputType>\n    <PublishAot>true<\/PublishAot>/' $S/$d.csproj
    grep -c "<PublishAot>true" $S/$d.csproj | sed 's/^/publishaot lines /'
    cd $S; go run . > $O/$d-go.txt 2>&1; echo "$d go rc=$?"
    t0=$(date +%s)
    dotnet publish $d.csproj -nologo -clp:ErrorsOnly -c Release -p:go2csPath=$W/src/ -o $O/$d > $O/$d-pub.log 2>&1; rc=$?
  else
    S=$W/src/tests/Performance/$d; cd $S; rm -rf bin obj; t0=$(date +%s)
    dotnet publish $d.csproj -nologo -clp:ErrorsOnly -c Release -p:PerfAot=true -p:go2csPath=$W/src/ -o $O/$d > $O/$d-pub.log 2>&1; rc=$?
  fi
  echo "$d aot publish rc=$rc wall $(( $(date +%s)-t0 ))s"; [ $rc = 0 ] || tail -20 $O/$d-pub.log
  ls -l $O/$d/$d | awk -v d=$d '{print d, "aot exe bytes", $5}'
  if [ $kind = B ]; then
    $O/$d/$d > $O/$d-aot.txt 2>&1; echo "$d aot run rc=$?"
    diff $O/$d-go.txt $O/$d-aot.txt > $O/$d-diff.txt; echo "$d diff rc=$? lines go=$(wc -l < $O/$d-go.txt) aot=$(wc -l < $O/$d-aot.txt) differing=$(grep -c '^<' $O/$d-diff.txt)"
    cd /; rm -rf $S
  else
    rm -rf $S/bin $S/obj
  fi
  rm -rf $O/$d/*.dbg; df -h / | tail -1 | awk '{print "free", $4}'
done
echo "== $L done $(date -u +%T)"
