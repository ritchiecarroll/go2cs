#!/usr/bin/env bash
# TRAIN F: emit runtime (three targets) with the union converter and compare the committed runtime files.
set -u
W=${W:-/h/go2cs-tmp-coord/tF}
T=/h/go2cs-tmp-coord/tFemit
S=${COORD_SCRATCH:?set COORD_SCRATCH to the COORD scratch directory}
export PATH="$HOME/sdk/go1.24.13/bin:$PATH" GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
cd "$W" || exit 2
rm -rf "$T"; mkdir -p "$T"
( cd src/go2cs && go build -o "$T/go2cs-u.exe" . ) || { echo "ABORT build"; exit 3; }
mkdir -p "$T/seed" && git archive HEAD src/core | tar -x -C "$T/seed"
for os in windows linux darwin; do
  R="$T/$os"; mkdir -p "$R"; git archive HEAD src/core src/version.props docs/validation | tar -x -C "$R"
  touch "$T/sent-$os"; sleep 1
  "$T/go2cs-u.exe" -stdlib -comments -platforms "$os/amd64" -go2cspath "$R/src" runtime > "$T/emit-$os.log" 2>&1; echo "EMIT $os rc=$?"
  python "$(cygpath -w "$S")/emitdrift.py" "$(cygpath -w "$R")" "$(cygpath -w "$T/seed")" "$(cygpath -w "$T/sent-$os")" "$(cygpath -w "$T/w-$os.txt")" "$(cygpath -w "$T/d-$os.txt")" > /dev/null
  echo "  $os written=$(wc -l < "$T/w-$os.txt") drift=$(wc -l < "$T/d-$os.txt"): $(tr '\n' ' ' < "$T/d-$os.txt" | cut -c1-400)"
done
echo DONE
