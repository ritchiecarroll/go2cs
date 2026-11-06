#!/usr/bin/env bash
# Build the probe at the tree's HEAD: JIT (Release build, copied out) and Native AOT (PerfAot publish). usage: build.sh <label>
L=$1; set -u
source $KIT0/env.sh >/dev/null 2>&1
W=$TREE; P=$W/src/tests/Performance/_p1aot_Probe
cd $P
echo "== build $L at $(git -C $W rev-parse --short=10 HEAD) $(date -u +%T)"
rm -rf bin obj
dotnet build P1AotProbe.csproj -nologo -clp:ErrorsOnly -c Release -p:go2csPath=$W/src/ > $KIT/jitbuild-$L.log 2>&1; rc=$?
echo "jit build rc=$rc"; [ $rc = 0 ] || { tail -20 $KIT/jitbuild-$L.log; exit 2; }
rm -rf $KIT/jit-$L; cp -r bin/Release/net10.0 $KIT/jit-$L
t0=$(date +%s)
dotnet publish P1AotProbe.csproj -nologo -clp:ErrorsOnly -c Release -p:PerfAot=true -p:go2csPath=$W/src/ -o $KIT/aot-$L > $KIT/aotpub-$L.log 2>&1; rc=$?
echo "aot publish rc=$rc wall $(( $(date +%s)-t0 ))s"
[ $rc = 0 ] || tail -20 $KIT/aotpub-$L.log
rm -rf bin obj
ls -la $KIT/aot-$L/P1AotProbe | awk '{print "aot exe bytes", $5}'
echo "== done $(date -u +%T)"
