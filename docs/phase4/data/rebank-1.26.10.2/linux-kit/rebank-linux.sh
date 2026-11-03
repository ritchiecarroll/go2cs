#!/usr/bin/env bash
# THE LINUX RE-BANK KIT for the go1.26.10 freeze (frozen after its smoke; COORD 2026-11-02 04:15Z). rehearse.sh (the
# 64d145b2c4 rehearsal) plus the four ruled kit changes: (1) SHARD and LIST are parameters; (2) the derived argv is
# argv.txt, so the run directory holds no *.tsv but <SHARD>.tsv and its ledger; (3) a final `git clean -fdXq -- src`
# at the shard's end, then refuse on any tracked deletion (safety floor 8); (4) classify_row.py calls a PASS with a
# non-zero rc PASS?rc, which does not bank. Its three files live together in $KIT: rebank-linux.sh, classify_row.py,
# derive-argv.ps1.
#   TIP=<full sha> SHARD=<wave-shaped name> LIST=<file> bash $KIT/rebank-linux.sh
# Origin: RWSL-1 DRY RUN at the tip (COORD 2026-10-30 23:30Z, item B): docs/PLAN-rebank-go126.md §12 R block / §3.2 linux act,
# R-LAPTOP's WSL, ONE battery on the box. One -tests invocation per row, never the sweep; every row's argv DERIVED at
# the tip by derive-argv.ps1 (the wrapper's and the sweep's own functions, by AST, and the roster reader), never typed.
#   TIP=<full sha> bash /root/rwsl-dry2.sh
# Evidence lands in $R, outside the clone. Nothing is committed or pushed by this script.
set -u
TIP=${TIP:?set TIP to the full tip SHA}
W=${W:-/root/go2cs}
GOROOT=${GOROOT:-/root/sdk/go1.26.10}
KIT=${KIT:-/root/rebank-kit}
for f in rebank-linux.sh classify_row.py derive-argv.ps1; do [ -s "$KIT/$f" ] || { echo "kit file $KIT/$f missing"; exit 2; }; done
SHARD=${SHARD:?set SHARD (a wave-shaped name, e.g. RHR-1)}
LIST=${LIST:?set LIST to the row list file}
R=${R:-/root/h10-$SHARD}
FLOOR_GB=${FLOOR_GB:-25}
mkdir -p "$R" || exit 2
SUM="$R/SUMMARY.txt"
stamp(){ echo "$(date -u '+%Y-%m-%d %H:%M:%SZ') $*" | tee -a "$SUM"; }
abort(){ stamp "ABORT: $*"; echo "$SHARD-DONE rc=2" >> "$SUM"; exit 2; }
freegb(){ df -BG --output=avail "$W" | tail -n 1 | tr -dc '0-9'; }

# --- toolchain: go1.26.10, the four overrides, GoTargetOS NOT exported -----------------------------------------
case "$GOROOT" in /*) ;; *) abort "GOROOT is not absolute: $GOROOT";; esac
head -n 1 "$GOROOT/VERSION" | grep -qx 'go1\.26\.10' || abort "GOROOT's VERSION is not go1.26.10"
export GOROOT GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export PATH="$GOROOT/bin:/root/dotnet10:$PATH"
export DOTNET_ROOT=/root/dotnet10 DOTNET_CLI_TELEMETRY_OPTOUT=1 MSBUILDDISABLENODEREUSE=1
unset GoTargetOS GO2CS_MODULE_ROOT
[ "$(go env GOROOT)" = "$GOROOT" ] || abort "go env GOROOT reads $(go env GOROOT), not $GOROOT (floor 6)"
go version | grep -q 'go1\.26\.10 ' || abort "go is $(go version), not go1.26.10"
[ "$(command -v go)" = "$GOROOT/bin/go" ] || abort "go resolves to $(command -v go)"
[ "$(env | grep -c '^GoTargetOS=')" = 0 ] || abort "GoTargetOS is exported"

# --- §12 host checks ---------------------------------------------------------------------------------------------
TMPV=${TMPDIR:-/tmp}
[ "${#TMPV}" -le 10 ] || abort "TMPDIR '$TMPV' is ${#TMPV} bytes (> 10)"
GTEST=$(find "$GOROOT/test" -type f 2>/dev/null | wc -l)
[ "$GTEST" -gt 0 ] || abort "$GOROOT carries no test/"
NSL=$(nslookup invalid.invalid 2>&1)
echo "$NSL" > "$R/nslookup.txt"
echo "$NSL" | grep -q 'NXDOMAIN' || abort "nslookup invalid.invalid did not answer NXDOMAIN (nslookup.txt)"
[ "$(freegb)" -ge "$FLOOR_GB" ] || abort "free space $(freegb)G below ${FLOOR_GB}G"
SDKV=$(dotnet --version 2>&1) || abort "dotnet --version failed: $SDKV"
RTV=$(dotnet --list-runtimes 2>&1 | awk '/Microsoft.NETCore.App/ {print $2}' | tr '\n' ',' | sed 's/,$//')
stamp "HOST uid=$(id -u) TMPDIR=$TMPV(${#TMPV} bytes) GOROOT=$GOROOT test/=$GTEST files toolchain: $(go version | cut -d' ' -f3-4) go=$(command -v go) VERSION=$(head -n 1 "$GOROOT/VERSION") .NET sdk=$SDKV runtime=$RTV nslookup=NXDOMAIN free=$(freegb)G nproc=$(nproc) mem=$(free -g | awk '/Mem:/ {print $2}')G"

# --- the clone: clean (tracked, and untracked under tracked paths), then detached at the tip --------------------
cd "$W" || abort "no clone at $W"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || abort "tracked changes in $W before checkout"
UT=$(git ls-files --others --exclude-standard | head -5)
[ -z "$UT" ] || abort "untracked, non-ignored files in $W before checkout: $UT"
git fetch -q origin master || abort "fetch failed"
git cat-file -e "$TIP^{commit}" || abort "tip $TIP not in the clone after fetch"
git checkout -q --detach "$TIP" || abort "checkout $TIP failed"
[ "$(git rev-parse HEAD)" = "$TIP" ] || abort "HEAD is not $TIP"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || abort "tracked changes after checkout"
UT=$(git ls-files --others --exclude-standard | head -5)
[ -z "$UT" ] || abort "untracked, non-ignored files after checkout: $UT"
git status --ignored=matching --porcelain > "$R/residue-before.txt"
stamp "PRE lane=R shard=$SHARD head=$(git rev-parse --short=10 HEAD) clean: tracked 0, untracked-under-tracked 0; residue=$(wc -l < "$R/residue-before.txt")"

# --- the row list: the LIST parameter, in its order ----------------------------------------------------------------
[ -s "$LIST" ] || abort "no row list at $LIST"
[ "$LIST" = "$R/$SHARD.txt" ] || cp "$LIST" "$R/$SHARD.txt"
stamp "rows: $(grep -c . "$R/$SHARD.txt") from $LIST: $(tr '\n' ' ' < "$R/$SHARD.txt")"

# --- the converter, built at the tip -----------------------------------------------------------------------------
( cd src/go2cs && go build -o go2cs . ) > "$R/build-converter.log" 2>&1 || abort "converter build failed (build-converter.log)"
CONV="$W/src/go2cs/go2cs"
CONV_MTIME=$(stat -c '%Y' "$CONV")
stamp "converter built: $CONV mtime=$CONV_MTIME"

# --- the argv, DERIVED at the tip ---------------------------------------------------------------------------------
pwsh -NoProfile -File "$KIT/derive-argv.ps1" -Tree "$W" -GoRoot "$GOROOT" -List "$R/$SHARD.txt" -Out "$R/argv.txt" 2> "$R/derive.log"
drc=$?
cat "$R/derive.log" >> "$SUM"
[ "$drc" -eq 0 ] || abort "argv derivation failed rc=$drc (derive.log)"
[ "$(grep -c . "$R/argv.txt")" = "$(grep -c . "$R/$SHARD.txt")" ] || abort "argv.txt row count differs from the list"

TSV="$R/$SHARD.tsv"
LEDGER="$R/$SHARD.ledger.tsv"
[ -f "$TSV" ] || printf 'row\tword\tverdicts\tmatched\tdisclosed\tdiverged\tsweep_s\twall_s\trc\ttimeout\tdeadline\n' > "$TSV"
[ -f "$LEDGER" ] || printf 'row\tcorpus_commit\tconverter_commit\tconverter_mtime\tword\trc\tended_utc\n' > "$LEDGER"

n=0
while IFS=$'\t' read -r -a A; do
  [ "${#A[@]}" -lt 3 ] && continue
  row=${A[0]}; ARGS=("${A[@]:1}")
  n=$((n+1))
  if awk -F'\t' -v r="$row" -v f="$TIP" -v m="$CONV_MTIME" '$1==r && $2==f && $4==m {found=1} END {exit !found}' "$LEDGER"; then
    stamp "row $n $row: already in the ledger at this tip and converter; skipped (resume)"
    continue
  fi
  [ "$(freegb)" -ge "$FLOOR_GB" ] || abort "free space $(freegb)G below ${FLOOR_GB}G before $row"
  tag=$(echo "$row" | tr '/' '.')
  E="$R/$tag"; mkdir -p "$E"
  T=$(printf '%s\n' "${ARGS[@]}" | awk 'p {print; exit} $0=="-test-timeout" {p=1}')
  D=$(printf '%s\n' "${ARGS[@]}" | awk 'p {print; exit} $0=="-test-deadline" {p=1}')
  OUT="${ARGS[${#ARGS[@]}-1]}"
  printf '%s\n' "$CONV" "${ARGS[@]}" > "$E/argv.txt"
  stamp "row $n $row: start (timeout $T, deadline ${D:-none}) argv: ${ARGS[*]}"
  started=$(date +%s)
  "$CONV" "${ARGS[@]}" < /dev/null > "$E/run.log" 2>&1
  rc=$?
  ended=$(date +%s)
  cp -p "$OUT/go2cs_test_comparison.json" "$OUT/go2cs_test_results.json" "$E/" 2>/dev/null
  tail -n 3 "$OUT/go2cs_test_results.json" > "$E/results-tail.txt" 2>/dev/null
  python3 -I "$KIT/classify_row.py" "$row" "$rc" "$started" "$E" "$OUT" $((ended-started)) "$T" "${D:-}" >> "$TSV" 2>> "$E/classify.err"
  word=$(awk -F'\t' -v r="$row" '$1==r {w=$2} END {print w}' "$TSV")
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\n' "$row" "$TIP" "$TIP" "$CONV_MTIME" "$word" "$rc" "$(date -u '+%Y-%m-%dT%H:%M:%SZ')" >> "$LEDGER"
  stamp "row $n $row: rc=$rc word=$word wall=$((ended-started))s ($(tail -n 1 "$TSV" | cut -f3-6))"
  # restore the row's dirt BY STATUS, then purge the row's build output
  git status --porcelain | grep -v '^??' | awk '{print $2}' > "$E/moved.txt"
  [ -s "$E/moved.txt" ] && xargs -a "$E/moved.txt" git checkout -q --
  git ls-files --others --exclude-standard > "$E/untracked.txt"
  [ -s "$E/untracked.txt" ] && xargs -a "$E/untracked.txt" rm -f --
  git clean -fdXq -- "src/core/$row"
  stamp "row $n $row: restored moved=$(wc -l < "$E/moved.txt") untracked=$(wc -l < "$E/untracked.txt") tracked-dirty=$(git status --porcelain | grep -vc '^??') deleted=$(git status --porcelain | grep -c '^ D') free=$(freegb)G"
done < "$R/argv.txt"

# Kit change 3: the shard's own build output, outside src/core/<row>, purged at the end; then the safety floor 8 check
# that the purge deleted nothing tracked.
git status --ignored=matching --porcelain > "$R/residue-before-purge.txt"
git clean -fdXq -- src
DEL=$(git status --porcelain | grep '^ D')
[ -z "$DEL" ] || abort "the final purge left tracked deletions: $(echo "$DEL" | head -5 | tr '\n' ' ')"
git status --ignored=matching --porcelain > "$R/residue-after.txt"
stamp "END rows=$(($(wc -l < "$TSV")-1)) of $(grep -c . "$R/$SHARD.txt") residue-before-purge=$(wc -l < "$R/residue-before-purge.txt") residue-after=$(wc -l < "$R/residue-after.txt") tracked-deleted=0"
echo "$SHARD-DONE rc=0" >> "$SUM"
