#!/usr/bin/env bash
# =================================================================================================
# h8-comparand.sh -- the H8 gate's two missing arms, as one instrument.
#
# H8's gate reads: "the platform manifest's marker gate is zero per target, and the default-flavor
# build reproduces the single-target build byte-for-byte." The 2026-09-13 amendment stopped short of
# scoring it, because at this hop NO outgoing manifest is committed (verified: no platform-manifest
# file has ever been tracked on any ref) and the byte-identity arm had no procedure. This script is
# the procedure for both, plus the package-delta derivation the class-count predictions come from.
#
#   classify   four-way census partition over three per-target manifests   (the comparand's engine)
#   compare    two classifications, ONE predicate, and the class-count delta
#   identity   the default-flavour byte-identity arm
#   pkgdelta   the std package delta per target at two releases            (the prediction's source)
#   selftest   every arm above MADE TO FAIL, then restored
#
# ------------------------------------------------------------------------------------------------
# WHY A SEEDED-ROOT MANIFEST IS NOT A CENSUS, AND THE TELL THAT REFUSES ONE
#
# The preserved H6 half-A / half-B artifacts are per-file manifests of SEEDED staging roots. A
# seeded root's path set is (seed union emitted) and all three targets share one seed, so such a
# manifest carries no emitted-vs-seeded discriminator -- which is exactly why the converter's own
# census uses a sentinel MTIME for that question rather than content. Classify three seeded-root
# manifests and `partial` and `exclusive` read from the SEED's path set, not from any emission.
#
# The tell is sharp and cheap: in a true per-target emission census a `*_windows.*` artifact CANNOT
# be emitted by the linux or darwin target. If a platform-suffixed artifact appears in a foreign
# target's manifest, the manifests cover a seeded root and `classify` REFUSES rather than returning
# a number that looks like a census. `--seeded-content-only` accepts them for the one question they
# CAN answer -- which shared paths differ in content across targets -- and says so in its output.
#
# ------------------------------------------------------------------------------------------------
# MANIFEST FORMAT (the form the fleet's preserved artifacts already use)
#
#   "<sha256><two spaces><relpath>", LC_ALL=C sorted, one artifact per line -- i.e. `sha256sum`
#   output over a stable relative walk. The TREE HASH of a manifest is sha256 of the manifest file
#   itself, which is order-independent only because the lines are sorted; this script re-sorts every
#   input before use so a differently-ordered manifest cannot read as a difference.
# =================================================================================================
set -u
PROG=${0##*/}

die(){ echo "$PROG: $*" >&2; exit 2; }

# ---- the layout key folders -----------------------------------------------------------------------
# The corpus's (GOOS, GOARCH) universe: the converter's platformUniverse (src/go2cs/platformLattice.go).
# The repoguard test TestLayoutKeyListsAreTheUniverses holds this line to it; keep it on ONE line, in
# this spelling. Layout L3 lays a package's files into a key folder per target: its GOOS, its GOARCH
# or the pair (windows/, amd64/, linux-arm64/), and every folder list below is derived from this one.
LAYOUT_UNIVERSE=(windows/amd64 linux/amd64 darwin/amd64 linux/arm64)
is_layout_key(){ # $1 = folder name -> 0 when the universe can lay a file into a folder of that name
  local t; for t in "${LAYOUT_UNIVERSE[@]}"; do
    case "$1" in "${t%/*}"|"${t#*/}"|"${t%/*}-${t#*/}") return 0 ;; esac
  done; return 1
}
layout_key_serves(){ # $1 = folder name, $2 = goos/arch -> 0 when that target's build compiles the folder
  case "$1" in "${2%/*}"|"${2#*/}"|"${2%/*}-${2#*/}") return 0 ;; esac; return 1
}

# ---- input hygiene: every compare asserts BOTH sides non-empty BEFORE reporting a difference -----
norm(){ # $1 = manifest path -> normalized "sha<TAB>path" on stdout
  local f="$1"
  [ -r "$f" ] || die "cannot read manifest '$f'"
  [ -s "$f" ] || die "manifest '$f' is EMPTY -- an empty baseline reports total disagreement, never a difference"
  local bad
  bad=$(grep -v '^# h8-comparand ' "$f" | grep -cvE '^[0-9a-fA-F]{64}[[:space:]]+[^[:space:]]')
  [ "$bad" -eq 0 ] || die "manifest '$f' has $bad line(s) not in '<sha256>  <relpath>' form"
  grep -v '^# h8-comparand ' "$f" | sed -E 's/^([0-9a-fA-F]{64})[[:space:]]+/\1\t/' | LC_ALL=C sort -t"$(printf '\t')" -k2,2
}
paths(){ cut -f2; }
treehash(){ LC_ALL=C sort "$1" | sha256sum | cut -d' ' -f1; }

# ---- the seed tell --------------------------------------------------------------------------------
# A platform-suffixed artifact in a FOREIGN target's manifest. Suffix set is the converter's own
# GOOS suffix vocabulary for the three H8 targets, matched on the stem before any extension.
seed_tell(){ # $1=win-norm $2=lin-norm $3=dar-norm ; prints offenders, returns 1 if the tell fires
  local hits=0 out
  out=$(
    paths < "$2" | grep -E '(^|/|_)[^/]*_windows(_test)?\.' | sed 's/^/  linux  manifest holds a windows artifact: /'
    paths < "$3" | grep -E '(^|/|_)[^/]*_windows(_test)?\.' | sed 's/^/  darwin manifest holds a windows artifact: /'
    paths < "$1" | grep -E '(^|/|_)[^/]*_(darwin|linux)(_test)?\.' | sed 's/^/  windows manifest holds a foreign artifact: /'
  )
  if [ -n "$out" ]; then printf '%s\n' "$out" | head -8; hits=$(printf '%s\n' "$out" | wc -l); fi
  [ "$hits" -eq 0 ]
}

# ---- classify -------------------------------------------------------------------------------------
# The partition is platformManifest.go's, verbatim, and in ITS order: a name emitted by every target
# is identical or variant by content; by exactly two, partial; by exactly one, exclusive.
do_classify(){
  local SEEDOK=0 ASSUMEFLAT=0
  while :; do
    case "${1:-}" in
      --seeded-content-only) SEEDOK=1; shift ;;
      --assume-flat)         ASSUMEFLAT=1; shift ;;
      *) break ;;
    esac
  done
  [ $# -eq 3 ] || die "usage: $PROG classify [--seeded-content-only] [--assume-flat] <win> <lin> <dar>"
  # ⚠ THE KEYING GATE. classify's answer is only a class census if its inputs are keyed on the FLAT
  # artifact path. A raw-path-keyed manifest yields the L3 TREE partition, which sums, passes the
  # partition check, clears the seed tell, and is wrong (see `manifest` above). Shape cannot
  # distinguish the two, so the format identifies itself: `manifest` stamps what it builds and this
  # refuses what carries no stamp. --assume-flat is for a manifest produced elsewhere (the preserved
  # H6 half-A/half-B artifacts), and the caller owns the claim.
  if [ "$ASSUMEFLAT" -eq 0 ]; then
    local f miss=0
    for f in "$1" "$2" "$3"; do
      head -1 "$f" 2>/dev/null | grep -q '^# h8-comparand manifest v1 key=flat-artifact-path' || { echo "  unstamped: $f" >&2; miss=1; }
    done
    if [ "$miss" -ne 0 ]; then
      echo "REFUSED: the manifest(s) above carry no flat-artifact-path stamp." >&2
      echo "         Build them with: $PROG manifest <census-target-root>   (it strips L3 layout folders" >&2
      echo "         structurally and stamps the result). A raw-path-keyed manifest scores variant 0 and" >&2
      echo "         every platform-varying artifact as exclusive -- a well-formed WRONG answer." >&2
      echo "         Pass --assume-flat only for a manifest built elsewhere that you know is flat-keyed." >&2
      exit 6
    fi
  fi
  local W L D T; T=$(mktemp -d)
  norm "$1" > "$T/w"; norm "$2" > "$T/l"; norm "$3" > "$T/d"

  # three byte-identical manifests are not three targets' emissions
  if cmp -s "$T/w" "$T/l" && cmp -s "$T/l" "$T/d"; then
    echo "REFUSED: all three manifests are identical after normalization -- this is one emission counted three times, not a census." >&2
    rm -rf "$T"; exit 3
  fi
  if ! seed_tell "$T/w" "$T/l" "$T/d"; then
    if [ "$SEEDOK" -eq 0 ]; then
      echo "REFUSED: SEED TELL FIRED (above). These manifests cover seeded staging roots, so they carry no" >&2
      echo "         emitted-vs-seeded discriminator and their partial/exclusive counts would be the SEED's" >&2
      echo "         path set, not an emission's. Re-run -platform-census under the release's own pin, or pass" >&2
      echo "         --seeded-content-only to get the one question a seeded pair CAN answer." >&2
      rm -rf "$T"; exit 3
    fi
    echo "  ⚠ SEEDED-CONTENT-ONLY: the seed tell fired; partial/exclusive below are NOT emission classes."
  fi

  paths < "$T/w" > "$T/pw"; paths < "$T/l" > "$T/pl"; paths < "$T/d" > "$T/pd"
  LC_ALL=C sort -u "$T/pw" "$T/pl" "$T/pd" > "$T/union"
  local ident=0 variant=0 partial=0 excl=0 n
  while IFS= read -r p; do
    local hw hl hd c; c=0
    hw=$(awk -F'\t' -v p="$p" '$2==p{print $1; exit}' "$T/w"); [ -n "$hw" ] && c=$((c+1))
    hl=$(awk -F'\t' -v p="$p" '$2==p{print $1; exit}' "$T/l"); [ -n "$hl" ] && c=$((c+1))
    hd=$(awk -F'\t' -v p="$p" '$2==p{print $1; exit}' "$T/d"); [ -n "$hd" ] && c=$((c+1))
    case "$c" in
      3) if [ "$hw" = "$hl" ] && [ "$hl" = "$hd" ]; then ident=$((ident+1)); else variant=$((variant+1)); fi ;;
      2) partial=$((partial+1)) ;;
      1) excl=$((excl+1)) ;;
    esac
  done < "$T/union"
  n=$(wc -l < "$T/union")
  # the L3 pricing identity the manifest computes rather than multiplies
  echo "  identical                 $ident"
  echo "  variant                   $variant"
  echo "  partial                   $partial"
  echo "  exclusive                 $excl"
  echo "  union total               $n"
  [ $((ident+variant+partial+excl)) -eq "$n" ] || { echo "REFUSED: partition does not sum to the union ($ident+$variant+$partial+$excl != $n)" >&2; rm -rf "$T"; exit 4; }
  echo "  partition sums to union   yes"
  printf 'CLASSCOUNTS %s %s %s %s %s\n' "$ident" "$variant" "$partial" "$excl" "$n"
  rm -rf "$T"
}

# ---- compare: one predicate, both sides, then the delta -------------------------------------------
do_compare(){
  local out=() in=() seen=0 FLAGS=()
  while :; do
    case "${1:-}" in --assume-flat|--seeded-content-only) FLAGS+=("$1"); shift ;; *) break ;; esac
  done
  for a in "$@"; do
    if [ "$a" = "--" ]; then seen=1; continue; fi
    if [ "$seen" -eq 0 ]; then out+=("$a"); else in+=("$a"); fi
  done
  [ "${#out[@]}" -eq 3 ] && [ "${#in[@]}" -eq 3 ] || die "usage: $PROG compare <out-win> <out-lin> <out-dar> -- <in-win> <in-lin> <in-dar>"
  echo "OUTGOING (the comparand):"
  local o; o=$("$0" classify "${FLAGS[@]}" "${out[@]}") || exit $?
  printf '%s\n' "$o" | grep -v '^CLASSCOUNTS'
  echo "INCOMING:"
  local i; i=$("$0" classify "${FLAGS[@]}" "${in[@]}") || exit $?
  printf '%s\n' "$i" | grep -v '^CLASSCOUNTS'
  local OV IV; OV=$(printf '%s\n' "$o" | grep '^CLASSCOUNTS'); IV=$(printf '%s\n' "$i" | grep '^CLASSCOUNTS')
  set -- $OV; local oi=$2 ov=$3 op=$4 oe=$5 ot=$6
  set -- $IV; local ii=$2 iv=$3 ip=$4 ie=$5 it=$6
  echo "DELTA (incoming - outgoing):"
  printf '  identical   %+d\n  variant     %+d\n  partial     %+d\n  exclusive   %+d\n  union       %+d\n' \
    $((ii-oi)) $((iv-ov)) $((ip-op)) $((ie-oe)) $((it-ot))
}

# ---- identity: the default-flavour byte-identity arm ----------------------------------------------
# manifest_minus: a normalized manifest with every listed PATH dropped, so a residual tree hash
# compares like with like. $1=manifest $2=path list to drop -> stdout
manifest_minus(){
  awk -F'	' -v drop="$2" 'BEGIN{while((getline p < drop)>0) d[p]=1} !($2 in d)' "$1"
}

# split_exempt: partition a path list by the seed-absent package set.
# $1=exempt package file  $2=path list  $3=exempt out  $4=residual out
# A path is exempt when it IS a listed package or lies under one, matched on a path-COMPONENT
# boundary -- never a bare string prefix, which would swallow a sibling such as runtime/cgotest.
split_exempt(){
  awk -v exf="$1" -v exout="$3" -v resout="$4" '
    BEGIN { n=0; while ((getline p < exf) > 0) if (p != "") pk[++n]=p }
    {
      ex=0
      for (i=1; i<=n; i++) if ($0 == pk[i] || index($0, pk[i] "/") == 1) { ex=1; break }
      if (ex) print > exout; else print > resout
    }
  ' "$2"
  [ -f "$3" ] || : > "$3"
  [ -f "$4" ] || : > "$4"
}

# ---- seedabsent: the exemption's population, derived from the SEED rather than asserted ---------
# Ruled 381577a8a: the byte-identity arm's clause names the BUILD, so a package the seed at the base
# does not carry has no committed layout to honour -- only a merged run has anything to place it by,
# and a placement difference there is not a build difference. The exemption is therefore legitimate
# and it is also the arm's biggest hole, so the set is DERIVED (never a literal) and REPORTED EVERY
# RUN including when it is empty: an exemption nobody counts is an exemption nobody can audit.
#
# A PACKAGE is a directory carrying a .csproj -- the same unit the emission writes.
do_seedabsent(){
  [ $# -eq 2 ] || die "usage: $PROG seedabsent <emission-root> <seed-root>"
  local EM="$1" SEED="$2"
  [ -d "$EM" ] || die "emission root not found: $EM"
  [ -d "$SEED" ] || die "seed root not found: $SEED"

  local T; T=$(mktemp -d)
  ( cd "$EM" && find . -name '*.csproj' ) | sed 's#/[^/]*\.csproj$##; s#^\./##' \
    | grep -v '^\.$' | LC_ALL=C sort -u > "$T/em"

  local nem; nem=$(wc -l < "$T/em")
  # An empty emission would report an empty seed-absent set and read exactly like a clean one.
  [ "$nem" -gt 0 ] || { rm -rf "$T"; die "no packages under $EM -- refusing to report a seed-absent set from an empty emission"; }

  local nseed; nseed=$( ( cd "$SEED" && find . -name '*.csproj' ) | wc -l )
  [ "$nseed" -gt 0 ] || { rm -rf "$T"; die "no packages under $SEED -- a seed with no packages would exempt EVERY package"; }

  local pkg n=0
  while IFS= read -r pkg; do
    [ -d "$SEED/$pkg" ] || { echo "$pkg"; n=$((n+1)); }
  done < "$T/em"

  echo "# emission packages $nem, seed packages $nseed, seed-absent $n" >&2
  rm -rf "$T"
}

do_identity(){
  [ $# -ge 2 ] && [ $# -le 3 ] || die "usage: $PROG identity <manifest-A> <manifest-B> [<seed-absent-package-file>]"
  local EX="${3:-}"
  local T; T=$(mktemp -d)
  norm "$1" > "$T/a"; norm "$2" > "$T/b"
  local na nb; na=$(wc -l < "$T/a"); nb=$(wc -l < "$T/b")
  paths < "$T/a" > "$T/pa"; paths < "$T/b" > "$T/pb"

  # The seed-absent exemption. Empty unless a package file is given, and counted either way.
  : > "$T/exempt"
  if [ -n "$EX" ]; then
    [ -f "$EX" ] || { rm -rf "$T"; die "seed-absent package file not found: $EX"; }
    grep -vE '^[[:space:]]*(#|$)' "$EX" | LC_ALL=C sort -u > "$T/exempt"
  fi
  local nexpkg; nexpkg=$(wc -l < "$T/exempt")

  comm -23 "$T/pa" "$T/pb" > "$T/oa"
  comm -13 "$T/pa" "$T/pb" > "$T/ob"
  split_exempt "$T/exempt" "$T/oa" "$T/oa.ex" "$T/oa.res"
  split_exempt "$T/exempt" "$T/ob" "$T/ob.ex" "$T/ob.res"

  local onlya onlyb differ exa exb
  onlya=$(wc -l < "$T/oa.res"); exa=$(wc -l < "$T/oa.ex")
  onlyb=$(wc -l < "$T/ob.res"); exb=$(wc -l < "$T/ob.ex")
  # ⚠ STRUCTURALLY DEAD UNTIL 2026-09-20, and it read a clean 0 the whole time. This was
  # `join -t... -j0 -o 0,1.1,2.1 ... 2>/dev/null`; GNU join REJECTS `-j0` ("invalid field
  # number: '0'"), the redirect ate the message, and the count came back 0 on every run.
  #
  # It was never WRONG, because the FULL tree-hash comparison caught every content change on its
  # behalf -- the selftest's "one CONTENT change goes red" arm goes red through the hash, not
  # through this counter, which is why a green selftest never exposed it. That is the shape of a
  # dead gate with a live neighbour: correct verdicts, and one number that was never a reading.
  #
  # The seed-absent exemption makes it LOAD-BEARING for the first time: exempt paths leave the
  # residual hash, so a content change inside a seed-absent package is invisible to the hash and
  # this counter is the only thing between it and a PASS -- exactly the hole the ruling's
  # "a content difference anywhere still fails" forbids. Rewritten without join and without a
  # swallowed stderr, and given selftest arms that isolate it from the hash.
  differ=$(awk -F'\t' 'NR==FNR{h[$2]=$1; next} ($2 in h) && h[$2]!=$1' "$T/a" "$T/b" | wc -l)
  # The tree hashes are taken over the NON-EXEMPT subset, because an exempt placement difference
  # moves the full-tree hash and would otherwise fail the run through the back door. Both are
  # printed: the residual is what the verdict reads, the full pair is what makes the exemption's
  # effect visible instead of implicit.
  cat "$T/pa" "$T/pb" | LC_ALL=C sort -u > "$T/allpaths"
  split_exempt "$T/exempt" "$T/allpaths" "$T/exempt.paths" "$T/residual.paths"
  manifest_minus "$T/a" "$T/exempt.paths" > "$T/a.res"
  manifest_minus "$T/b" "$T/exempt.paths" > "$T/b.res"
  local ha hb hra hrb
  ha=$(treehash "$T/a"); hb=$(treehash "$T/b")
  hra=$(treehash "$T/a.res"); hrb=$(treehash "$T/b.res")
  echo "  A artifacts               $na"
  echo "  B artifacts               $nb"
  echo "  seed-absent packages      $nexpkg (exempt paths: A $exa, B $exb)"
  echo "  only in A  (residual)     $onlya"
  echo "  only in B  (residual)     $onlyb"
  echo "  same path, content DIFFER $differ"
  echo "  A tree hash (residual)    $hra"
  echo "  B tree hash (residual)    $hrb"
  echo "  A tree hash (full)        $ha"
  echo "  B tree hash (full)        $hb"
  if [ "$na" -gt 0 ] && [ "$nb" -gt 0 ] && [ "$onlya" -eq 0 ] && [ "$onlyb" -eq 0 ] && [ "$differ" -eq 0 ] && [ "$hra" = "$hrb" ]; then
    if [ "$((exa+exb))" -gt 0 ]; then
      echo "BYTE-IDENTITY ARM: PASS WITH EXEMPTION ($na artifacts; $((exa+exb)) exempt paths in $nexpkg seed-absent package(s), listed below)"
      cat "$T/oa.ex" "$T/ob.ex" | sed 's/^/    exempt  /'
    else
      echo "BYTE-IDENTITY ARM: PASS ($na artifacts, both sides non-empty)"
    fi
    rm -rf "$T"; return 0
  fi
  echo "BYTE-IDENTITY ARM: FAIL"
  if [ "$na" -eq 0 ] || [ "$nb" -eq 0 ]; then echo "    a side is EMPTY -- an empty manifest cannot pass"; fi
  [ "$onlya" -gt 0 ] && { echo "  --- only in A, NOT exempt (first 10) ---"; head -10 "$T/oa.res" | sed 's/^/    /'; }
  [ "$onlyb" -gt 0 ] && { echo "  --- only in B, NOT exempt (first 10) ---"; head -10 "$T/ob.res" | sed 's/^/    /'; }
  rm -rf "$T"; return 1
}

# ---- pkgdelta: the prediction's source, with the H1 pin assertion built in ------------------------
# ⚠ MEASURED 2026-09-19 on a linux box: GO111MODULE=off SILENTLY CANCELS a GOTOOLCHAIN redirect --
# `GOTOOLCHAIN=go1.23.12 GO111MODULE=off go version` printed the AMBIENT toolchain at exit 0. The
# census instrument is specified WITH GO111MODULE=off, so that combination reads the wrong release
# and looks perfect. This mode therefore drives each release by its own GOROOT and asserts the
# release from `go version` OUTPUT before listing anything, per H1.
# ⚠ AND CGO_ENABLED is a real axis: it moves the LINUX count by exactly one package (runtime/cgo) at
# both releases. CGO_ENABLED=0 is pinned here, which is what reproduces the recorded census.
do_pkgdelta(){
  [ $# -eq 2 ] || die "usage: $PROG pkgdelta <goroot-outgoing> <goroot-incoming>"
  local T; T=$(mktemp -d)
  local names=(outgoing incoming) roots=("$1" "$2") i=0
  for R in "${roots[@]}"; do
    local which=${names[$i]}; i=$((i+1))
    [ -x "$R/bin/go" ] || die "no executable go at '$R/bin/go'"
    local v; v=$(GOTOOLCHAIN=local GOROOT="$R" GO111MODULE=off "$R/bin/go" version 2>/dev/null)
    case "$v" in *"go version go"*) ;; *) die "'$R/bin/go version' did not name a release: '$v'";; esac
    echo "  $which  $v"
    printf '%s\n' "$v" > "$T/$which.ver"
    for goos in windows linux darwin; do
      GOTOOLCHAIN=local GOROOT="$R" GO111MODULE=off GOOS=$goos GOARCH=amd64 CGO_ENABLED=0 \
        "$R/bin/go" list -tags purego,math_big_pure_go std 2>/dev/null | LC_ALL=C sort > "$T/$which-$goos.pkgs"
      [ -s "$T/$which-$goos.pkgs" ] || die "$which/$goos listed ZERO packages"
    done
  done
  cmp -s "$T/outgoing.ver" "$T/incoming.ver" && die "both GOROOTs run the SAME release -- the delta would be vacuous"
  echo
  echo "  per-target package counts"
  for goos in windows linux darwin; do
    printf '    %-8s outgoing %-4s incoming %-4s net %+d\n' "$goos" \
      "$(wc -l < "$T/outgoing-$goos.pkgs")" "$(wc -l < "$T/incoming-$goos.pkgs")" \
      "$(( $(wc -l < "$T/incoming-$goos.pkgs") - $(wc -l < "$T/outgoing-$goos.pkgs") ))"
  done
  echo
  echo "  added / removed per target"
  for goos in windows linux darwin; do
    comm -13 "$T/outgoing-$goos.pkgs" "$T/incoming-$goos.pkgs" > "$T/added-$goos"
    comm -23 "$T/outgoing-$goos.pkgs" "$T/incoming-$goos.pkgs" > "$T/removed-$goos"
    printf '    %-8s added %-4s removed %s\n' "$goos" "$(wc -l < "$T/added-$goos")" "$(wc -l < "$T/removed-$goos")"
  done
  local samea=yes samer=yes
  cmp -s "$T/added-windows" "$T/added-linux" && cmp -s "$T/added-linux" "$T/added-darwin" || samea=no
  cmp -s "$T/removed-windows" "$T/removed-linux" && cmp -s "$T/removed-linux" "$T/removed-darwin" || samer=no
  echo "    added set identical on all three targets:   $samea"
  echo "    removed set identical on all three targets: $samer"
  echo
  echo "  FILE-LEVEL classification of the added and removed sets (this is what moves the classes)"
  for side in added removed; do
    local R; [ "$side" = added ] && R="${roots[1]}" || R="${roots[0]}"
    for goos in windows linux darwin; do
      GOTOOLCHAIN=local GOROOT="$R" GO111MODULE=off GOOS=$goos GOARCH=amd64 CGO_ENABLED=0 \
        "$R/bin/go" list -tags purego,math_big_pure_go -f '{{range .GoFiles}}{{$.ImportPath}}|{{.}}
{{end}}' $(cat "$T/$side-$goos") 2>/dev/null | grep . | LC_ALL=C sort > "$T/$side-f-$goos"
    done
    LC_ALL=C sort -u "$T/$side-f-windows" "$T/$side-f-linux" "$T/$side-f-darwin" > "$T/$side-union"
    local on3=0 on2=0 on1=0
    while IFS= read -r n; do
      local c=0
      grep -Fxq "$n" "$T/$side-f-windows" && c=$((c+1))
      grep -Fxq "$n" "$T/$side-f-linux"   && c=$((c+1))
      grep -Fxq "$n" "$T/$side-f-darwin"  && c=$((c+1))
      case $c in 3) on3=$((on3+1));; 2) on2=$((on2+1));; 1) on1=$((on1+1));; esac
    done < "$T/$side-union"
    printf '    %-8s union %-4s on-all-3 %-4s on-2 (-> partial) %-4s on-1 (-> exclusive) %s\n' \
      "$side" "$(wc -l < "$T/$side-union")" "$on3" "$on2" "$on1"
    [ "$on1" -gt 0 ] && { echo "      the on-1 rows:"; while IFS= read -r n; do
        local c=0
        grep -Fxq "$n" "$T/$side-f-windows" && c=$((c+1)); grep -Fxq "$n" "$T/$side-f-linux" && c=$((c+1)); grep -Fxq "$n" "$T/$side-f-darwin" && c=$((c+1))
        [ "$c" -eq 1 ] && echo "        $n"
      done < "$T/$side-union"; }
  done
  rm -rf "$T"
}

# ---- manifest: build classify's input from a census root, KEYED ON THE FLAT ARTIFACT PATH ---------
# ⚠ THIS MODE EXISTS BECAUSE THE OBVIOUS HAND-BUILT MANIFEST IS SILENTLY WRONG (G, 2026-09-19,
# measured on the real incoming census). A census staging root is SEEDED from an L3 corpus, so a
# platform-varying artifact sits under a per-GOOS layout folder: `os/windows/file.cs` in the windows
# root, `os/linux/file.cs` in the linux one. Keyed on the RAW relative path those are two different
# names, so EVERY platform-varying artifact scores `exclusive` and `variant` collapses to exactly 0:
#
#     raw relative path   identical 1631 · variant 0  · partial 0  · exclusive 718 · union 2349
#     flat artifact path  identical 1631 · variant 83 · partial 93 · exclusive 283 · union 2090
#     the converter                 1631 ·         83 ·         93 ·           283 ·       2090
#
# The raw row SUMS TO ITS OWN UNION, PASSES the partition check and CLEARS the seed tell — it is the
# L3 TREE partition (1631 + 718 = l3UnionTreeTotal), a true answer to a different question. Only a
# reader noticing `variant 0` unaided would catch it. So the flat key is not a convention here: it is
# produced by this mode, the mode STAMPS the manifest, and `classify` REFUSES an unstamped one.
#
# platformCensus.go keys an artifact by its flat package-relative path (`variantFiles` reads
# `os/file.cs`, never `os/windows/file.cs`), and stripping to that reproduces its four classes exactly.
# Stripping uses the STRUCTURAL discriminator, never the directory names: measured on the incoming
# emission each target holds 100 GOOS-named directories, of which 99 are layout folders and ONE is a
# real package (`internal/syscall/windows`, which carries its own .csproj). A name filter deletes that
# package from two of the three views.
MANIFEST_STAMP='# h8-comparand manifest v1 key=flat-artifact-path'
# ⚠ SECOND GAP, closed 2026-09-20 (G, measured on the real census root): a census TARGET ROOT is
# seeded from an L3 corpus AND emitted into, so it holds BOTH `archive/tar/package_info.cs` (flat, from
# the seed) and `archive/tar/<goos>/package_info.cs` (the layout folder). Stripping maps the second onto
# the first and the duplicate-key guard REFUSES, rc 5 — loudly and correctly, but the sanctioned builder
# could not then consume the artifact the gate is about.
#
# What disambiguates them is the EMITTED-SET restriction the census itself uses: platformCensus.go
# stamps every SEEDED file with the sentinel mtime 2000-01-01T00:00:00Z, so a file whose mtime has moved
# was written BY this run. --emitted-only applies that filter, which is what G's own hand-built manifest
# did and the only reason it produced no collisions; it reproduces the manifest's `emittedCs` exactly.
SEED_SENTINEL_EPOCH=946684800   # 2000-01-01T00:00:00Z, platformCensus.go's censusSeedSentinel
do_manifest(){
  local EMITTED_ONLY=0
  [ "${1:-}" = "--emitted-only" ] && { EMITTED_ONLY=1; shift; }
  [ $# -eq 1 ] || die "usage: $PROG manifest [--emitted-only] <census-target-root>"
  local ROOT="$1"
  [ -d "$ROOT" ] || die "no such census root '$ROOT'"
  cd "$ROOT" || die "cannot enter '$ROOT'"
  local n=0 T; T=$(mktemp)
  local skipped_seed=0
  while IFS= read -r f; do
    local rel=${f#./} out="" d base parent
    if [ "$EMITTED_ONLY" -eq 1 ]; then
      # a file still carrying the seed sentinel was NOT written by this run
      local mt; mt=$(stat -c %Y "$rel" 2>/dev/null || echo 0)
      if [ "$mt" -eq "$SEED_SENTINEL_EPOCH" ]; then skipped_seed=$((skipped_seed+1)); continue; fi
    fi
    d=$(dirname "$rel"); base=$(basename "$rel")
    # strip EVERY layout-folder segment from the directory chain, innermost out
    local parts="" seg
    while [ "$d" != "." ] && [ -n "$d" ]; do
      seg=${d##*/}; parent=$(dirname "$d")
      if is_layout_key "$seg"; then
        if compgen -G "$d/*.csproj" >/dev/null 2>&1 || ! compgen -G "$parent/*.csproj" >/dev/null 2>&1; then
          parts="$seg${parts:+/$parts}"        # a real package directory: KEEP it
        fi                                      # else: a layout folder -- drop it
      else
        parts="$seg${parts:+/$parts}"
      fi
      d="$parent"
    done
    out="${parts:+$parts/}$base"
    printf '%s  %s\n' "$(sha256sum "$rel" | cut -d' ' -f1)" "$out" >> "$T"
    n=$((n+1))
  done < <(find . -type f -name '*.cs' -not -name '*.cs.auto' | LC_ALL=C sort)
  [ "$n" -gt 0 ] || { rm -f "$T"; die "manifest kept ZERO .cs files${EMITTED_ONLY:+ (--emitted-only: $skipped_seed still carried the seed sentinel)} -- an empty manifest is not a census"; }
  # stripping must not merge two distinct artifacts onto one key
  local dup; dup=$(cut -d' ' -f3- "$T" | LC_ALL=C sort | uniq -d | head -5)
  if [ -n "$dup" ]; then
    echo "REFUSED: stripping produced DUPLICATE keys -- two artifacts merged onto one name:" >&2
    printf '%s\n' "$dup" | sed 's/^/  /' >&2
    if [ "$EMITTED_ONLY" -eq 0 ]; then
      echo "         A census TARGET ROOT holds the SEED's flat copy and this run's per-GOOS copy of the" >&2
      echo "         same artifact, and stripping merges them. Re-run with --emitted-only, which keeps" >&2
      echo "         only files this run actually wrote (mtime moved off the census seed sentinel)." >&2
    fi
    rm -f "$T"; exit 5
  fi
  printf '%s\n' "$MANIFEST_STAMP"
  LC_ALL=C sort -k2,2 "$T"
  rm -f "$T"
}

# ---- view: the DEFAULT-FLAVOUR manifest of a corpus root -------------------------------------------
# The default flavour of an L3 corpus is what a build for <host> compiles: the FLAT files in each
# package directory plus that package's key folders for the host target (<goos>/, <arch>/ and
# <goos>-<arch>/), and nothing from another target's key folder. <host> is a target of the universe,
# <goos>/<arch>; a bare <goos> means <goos>/amd64, the arch every GOOS of the universe has.
#
# ⚠ A GOOS-NAMED DIRECTORY IS NOT AUTOMATICALLY A LAYOUT FOLDER. `internal/syscall/windows` is a
# PACKAGE whose directory is named `windows`, and it exists on every target's view. A filter that
# excludes any path component in {windows,linux,darwin} silently drops that whole package from the
# linux and darwin manifests -- and dropping the same paths from BOTH sides makes the arm agree
# about files it never looked at, which is the vacuous green this arm exists to avoid.
#
# The discriminator is structural and exact: a directory is a LAYOUT folder iff its name is a layout
# key of the universe (is_layout_key), it holds no .csproj of its own, and its PARENT holds a .csproj. A package directory named
# `windows` carries its own .csproj and is therefore kept.
do_view(){
  [ $# -eq 2 ] || die "usage: $PROG view <corpus-root> <host-goos>[/<arch>]"
  local ROOT="$1" HOST="$2" t known=0
  [ -d "$ROOT" ] || die "no such corpus root '$ROOT'"
  case "$HOST" in */*) ;; *) HOST="$HOST/amd64" ;; esac
  for t in "${LAYOUT_UNIVERSE[@]}"; do [ "$t" = "$HOST" ] && known=1; done
  [ "$known" -eq 1 ] || die "host '$HOST' is not a target of the universe (${LAYOUT_UNIVERSE[*]})"
  local n=0
  cd "$ROOT" || die "cannot enter '$ROOT'"
  while IFS= read -r f; do
    local rel=${f#./} keep=1 d
    d=$(dirname "$rel")
    # walk every ancestor directory of the file, looking for a LAYOUT folder that is not the host's
    while [ "$d" != "." ] && [ -n "$d" ]; do
      local base=${d##*/} parent; parent=$(dirname "$d")
      # layout folder iff a key name, no .csproj here AND the parent has one
      if is_layout_key "$base" && ! compgen -G "$d/*.csproj" >/dev/null 2>&1 && compgen -G "$parent/*.csproj" >/dev/null 2>&1; then
        layout_key_serves "$base" "$HOST" || { keep=0; break; }
      fi
      d="$parent"
    done
    [ "$keep" -eq 1 ] || continue
    printf '%s  %s\n' "$(sha256sum "$rel" | cut -d' ' -f1)" "$rel"
    n=$((n+1))
  done < <(find . -type f -name '*.cs' -not -name '*.cs.auto' | LC_ALL=C sort)
  [ "$n" -gt 0 ] || die "view produced ZERO artifacts -- a filter that selects nothing cannot be compared"
} 

# ---- selftest: every arm MADE TO FAIL, then restored ----------------------------------------------
st_pass=0; st_fail=0
st(){ # $1=label $2=expected-rc ; rest = command
  local label="$1" want="$2"; shift 2
  "$@" >/dev/null 2>&1; local got=$?
  if [ "$got" -eq "$want" ]; then st_pass=$((st_pass+1)); printf '  PASS  %-58s rc=%s\n' "$label" "$got"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s rc=%s want=%s\n' "$label" "$got" "$want"; fi
}
do_selftest(){
  local T; T=$(mktemp -d)
  h(){ printf '%064d' "$1"; }
  # a clean three-target EMISSION: same paths, one content differs, plus one exclusive per target
  { echo "$(h 1)  a/x.cs"; echo "$(h 2)  a/y.cs"; echo "$(h 7)  a/only_windows.cs"; } | LC_ALL=C sort > "$T/w"
  { echo "$(h 1)  a/x.cs"; echo "$(h 3)  a/y.cs"; echo "$(h 8)  a/only_linux.cs";   } | LC_ALL=C sort > "$T/l"
  { echo "$(h 1)  a/x.cs"; echo "$(h 3)  a/y.cs"; echo "$(h 9)  a/only_darwin.cs";  } | LC_ALL=C sort > "$T/d"
  echo "A. classify -- the healthy case and its arithmetic"
  local out; out=$("$0" classify --assume-flat "$T/w" "$T/l" "$T/d")
  st "clean three-target emission classifies"        0 "$0" classify --assume-flat "$T/w" "$T/l" "$T/d"
  local got; got=$(printf '%s\n' "$out" | grep '^CLASSCOUNTS')
  if [ "$got" = "CLASSCOUNTS 1 1 0 3 5" ]; then st_pass=$((st_pass+1)); printf '  PASS  %-58s %s\n' "counts are identical1/variant1/partial0/exclusive3" "$got"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s %s\n' "expected CLASSCOUNTS 1 1 0 3 5" "$got"; fi
  echo "B. classify -- every refusal MADE TO FAIL"
  : > "$T/empty"
  st "an EMPTY manifest refuses (never 'total disagreement')" 2 "$0" classify --assume-flat "$T/empty" "$T/l" "$T/d"
  printf 'not-a-manifest\n' > "$T/junk"
  st "a malformed manifest refuses"                 2 "$0" classify --assume-flat "$T/junk" "$T/l" "$T/d"
  st "three IDENTICAL manifests refuse"             3 "$0" classify --assume-flat "$T/w" "$T/w" "$T/w"
  # the seed tell: put the windows-only artifact into the linux and darwin roots (a seeded root)
  { cat "$T/l"; echo "$(h 7)  a/only_windows.cs"; } | LC_ALL=C sort > "$T/l_seeded"
  { cat "$T/d"; echo "$(h 7)  a/only_windows.cs"; } | LC_ALL=C sort > "$T/d_seeded"
  st "SEED TELL refuses a seeded-root triple"       3 "$0" classify --assume-flat "$T/w" "$T/l_seeded" "$T/d_seeded"
  st "  and --seeded-content-only admits it"        0 "$0" classify --seeded-content-only --assume-flat "$T/w" "$T/l_seeded" "$T/d_seeded"
  echo "C. identity -- the arm, and the three ways it must go red"
  cp "$T/w" "$T/w2"
  st "identical manifests PASS"                     0 "$0" identity "$T/w" "$T/w2"
  sed "s|$(h 2)  a/y.cs|$(h 5)  a/y.cs|" "$T/w" > "$T/w_content"
  st "one CONTENT change goes red"                  1 "$0" identity "$T/w" "$T/w_content"
  sed 's|a/y.cs|a/y_renamed.cs|' "$T/w" > "$T/w_path"
  st "one PATH change goes red (normalization is not eating it)" 1 "$0" identity "$T/w" "$T/w_path"
  st "an EMPTY side refuses rather than 'agreeing'" 2 "$0" identity "$T/empty" "$T/w"
  # order-independence must NOT read as a difference
  LC_ALL=C sort -r "$T/w" > "$T/w_rev"
  st "a reordered manifest still PASSES"            0 "$0" identity "$T/w" "$T/w_rev"
  # ⚠ The CONTENT arm above goes red through the TREE HASH. Under an exemption the exempt
  # paths leave the residual hash, so these arms are the ones that isolate the content counter
  # itself -- the counter that was dead from this instrument's first cut until 2026-09-20.
  sed 's|a/y.cs|a/linux/y.cs|' "$T/w" > "$T/w_moved"
  printf 'a\n'   > "$T/ex_a"
  printf 'zzz\n' > "$T/ex_none"
  st "a CONTENT change inside an EXEMPT package STILL goes red" 1 "$0" identity "$T/w" "$T/w_content" "$T/ex_a"
  st "a PLACEMENT-only change inside it is EXEMPT"              0 "$0" identity "$T/w" "$T/w_moved"   "$T/ex_a"
  st "the same move OUTSIDE the set still goes red"             1 "$0" identity "$T/w" "$T/w_moved"   "$T/ex_none"
  # the component boundary: runtime/cgo must not swallow runtime/cgotest
  printf '%s  %s\n' "$(h 1)" "runtime/cgotest/t.cs"       > "$T/sib_a"
  printf '%s  %s\n' "$(h 1)" "runtime/cgotest/linux/t.cs" > "$T/sib_b"
  printf 'runtime/cgo\n' > "$T/ex_cgo"
  st "a SIBLING package is not exempted by a name prefix"       1 "$0" identity "$T/sib_a" "$T/sib_b" "$T/ex_cgo"
  st "a MISSING exemption file refuses, never exempts nothing"  2 "$0" identity "$T/w" "$T/w2" "$T/no_such_file"

  echo "C2. seedabsent -- the exemption's population, DERIVED and refusing the vacuous cases"
  mkdir -p "$T/em/a" "$T/em/b" "$T/em/runtime/cgo" "$T/em/runtime/cgotest" "$T/seed/a" "$T/seed/b" "$T/void"
  : > "$T/em/a/a.csproj"; : > "$T/em/b/b.csproj"
  : > "$T/em/runtime/cgo/runtime.cgo.csproj"; : > "$T/em/runtime/cgotest/runtime.cgotest.csproj"
  : > "$T/seed/a/a.csproj"; : > "$T/seed/b/b.csproj"
  st "seedabsent runs"                                          0 "$0" seedabsent "$T/em" "$T/seed"
  st "an EMPTY emission refuses (it would report a clean 0)"     2 "$0" seedabsent "$T/void" "$T/seed"
  st "an EMPTY seed refuses (it would exempt EVERY package)"     2 "$0" seedabsent "$T/em" "$T/void"
  "$0" seedabsent "$T/em" "$T/seed" 2>/dev/null | LC_ALL=C sort | tr '\n' ' ' > "$T/absent"
  if [ "$(cat "$T/absent")" = "runtime/cgo runtime/cgotest " ]; then
    st "the derived set is exactly the seed-absent packages"      0 true
  else
    st "the derived set is exactly the seed-absent packages"      0 false
  fi

  echo "D. compare -- one predicate both sides, and a delta that is MEASURED, not assumed"
  # the incoming triple gains one shared artifact and loses the darwin-exclusive one, so the delta
  # must be identical +1 and exclusive -1. A control that compares a triple with ITSELF proves only
  # that the code runs; it cannot tell a working delta from a hardcoded row of zeros.
  { cat "$T/w"; echo "$(h 4)  a/z.cs"; } | LC_ALL=C sort > "$T/w2i"
  { cat "$T/l"; echo "$(h 4)  a/z.cs"; } | LC_ALL=C sort > "$T/l2i"
  { grep -v only_darwin "$T/d"; echo "$(h 4)  a/z.cs"; } | LC_ALL=C sort > "$T/d2i"
  st "compare runs on two DISTINCT triples"         0 "$0" compare --assume-flat "$T/w" "$T/l" "$T/d" -- "$T/w2i" "$T/l2i" "$T/d2i"
  local dlt; dlt=$("$0" compare --assume-flat "$T/w" "$T/l" "$T/d" -- "$T/w2i" "$T/l2i" "$T/d2i" | sed -n '/^DELTA/,$p')
  if printf '%s\n' "$dlt" | grep -q 'identical   +1' && printf '%s\n' "$dlt" | grep -q 'exclusive   -1'; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s %s\n' "the delta is non-zero and correct (+1 identical, -1 exclusive)" "measured"
  else
    st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "delta wrong:"; printf '%s\n' "$dlt" | sed 's/^/        /'
  fi
  st "compare refuses an empty side"                2 "$0" compare --assume-flat "$T/empty" "$T/l" "$T/d" -- "$T/w" "$T/l" "$T/d"
  echo "E. view -- the default-flavour filter, and the package-vs-layout trap"
  local C="$T/corpus"
  mkdir -p "$C/pkg/windows" "$C/pkg/linux" "$C/pkg/darwin" "$C/internal/syscall/windows"
  : > "$C/pkg/pkg.csproj"
  echo flat    > "$C/pkg/shared.cs"
  echo win     > "$C/pkg/windows/only.cs"
  echo lin     > "$C/pkg/linux/only.cs"
  echo dar     > "$C/pkg/darwin/only.cs"
  # a PACKAGE directory named windows: it carries its own .csproj and must survive every host view
  : > "$C/internal/syscall/windows/internal.syscall.windows.csproj"
  echo pkgwin  > "$C/internal/syscall/windows/zsyscall.cs"
  local vl; vl=$("$0" view "$C" linux)
  st "view runs on a corpus root"                   0 "$0" view "$C" linux
  if printf '%s\n' "$vl" | grep -q 'pkg/shared.cs' \
     && printf '%s\n' "$vl" | grep -q 'pkg/linux/only.cs' \
     && ! printf '%s\n' "$vl" | grep -q 'pkg/windows/only.cs' \
     && ! printf '%s\n' "$vl" | grep -q 'pkg/darwin/only.cs'; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "linux view keeps flat+linux, drops windows/darwin layout"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "linux view filter wrong:"; printf '%s\n' "$vl" | sed 's/^/        /'; fi
  if printf '%s\n' "$vl" | grep -q 'internal/syscall/windows/zsyscall.cs'; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "a PACKAGE dir named windows survives the linux view"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "the package-vs-layout discriminator dropped a real package"; fi
  # and the arm must SEE a difference that lives in the host folder
  local v1="$T/v1.man" v2="$T/v2.man"
  "$0" view "$C" linux > "$v1"
  echo changed > "$C/pkg/linux/only.cs"
  "$0" view "$C" linux > "$v2"
  st "a change INSIDE the host folder is seen by identity" 1 "$0" identity "$v1" "$v2"
  # a change in a FOREIGN folder must NOT move the host view (that is the view's whole job)
  echo changed2 > "$C/pkg/windows/only.cs"
  "$0" view "$C" linux > "$T/v3.man"
  st "a change in a FOREIGN folder leaves the host view alone" 0 "$0" identity "$v2" "$T/v3.man"
  echo "F. manifest + the KEYING GATE -- the trap G measured, built as a control"
  # a synthetic L3 census root per target: one flat shared file, one per-GOOS layout folder holding
  # the SAME artifact name, and a real package directory named `windows` that must survive.
  local C2R="$T/census"
  for g in windows linux darwin; do
    mkdir -p "$C2R/$g/pkg/$g" "$C2R/$g/internal/syscall/windows"
    : > "$C2R/$g/pkg/pkg.csproj"
    echo shared                       > "$C2R/$g/pkg/shared.cs"
    echo "variant-body-for-$g"        > "$C2R/$g/pkg/$g/file.cs"
    : > "$C2R/$g/internal/syscall/windows/internal.syscall.windows.csproj"
    echo pkgwin                       > "$C2R/$g/internal/syscall/windows/zsyscall.cs"
  done
  st "manifest builds from a census root"           0 "$0" manifest "$C2R/windows"
  for g in windows linux darwin; do "$0" manifest "$C2R/$g" > "$T/m-$g" 2>/dev/null; done
  # the real package survives the strip; the layout folder does not
  if grep -q 'internal/syscall/windows/zsyscall.cs' "$T/m-linux" && ! grep -qE '(^|[[:space:]])pkg/(windows|linux|darwin)/' "$T/m-linux"; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "flat key keeps a real package dir, drops the layout folder"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "flat-key stripping wrong:"; sed 's/^/        /' "$T/m-linux"; fi
  # THE POINT: flat keying finds the variant; raw keying calls it exclusive and reads variant 0
  local flat raw
  flat=$("$0" classify "$T/m-windows" "$T/m-linux" "$T/m-darwin" | grep '^CLASSCOUNTS')
  for g in windows linux darwin; do ( cd "$C2R/$g" && find . -type f -name '*.cs' | LC_ALL=C sort | while IFS= read -r f; do printf '%s  %s\n' "$(sha256sum "$f" | cut -d' ' -f1)" "${f#./}"; done ) > "$T/r-$g"; done
  raw=$("$0" classify --assume-flat "$T/r-windows" "$T/r-linux" "$T/r-darwin" | grep '^CLASSCOUNTS')
  # flat: shared.cs + zsyscall.cs identical, pkg/file.cs variant  -> identical 2 variant 1 exclusive 0
  # raw:  pkg/<goos>/file.cs are three DIFFERENT names            -> variant 0, exclusive 3
  if [ "$flat" = "CLASSCOUNTS 2 1 0 0 3" ] && [ "$raw" = "CLASSCOUNTS 2 0 0 3 5" ]; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "flat finds the variant; raw reads variant 0 (the trap reproduced)"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s flat=%s raw=%s\n' "trap control" "$flat" "$raw"; fi
  st "an UNSTAMPED manifest triple is REFUSED"      6 "$0" classify "$T/r-windows" "$T/r-linux" "$T/r-darwin"
  st "  and --assume-flat admits it (caller owns it)" 0 "$0" classify --assume-flat "$T/r-windows" "$T/r-linux" "$T/r-darwin"
  mkdir -p "$T/emptyroot"
  st "manifest refuses a root with no .cs"          2 "$0" manifest "$T/emptyroot"
  # duplicate-key refusal: two layout folders holding the same artifact name collapse onto one key
  mkdir -p "$C2R/dup/pkg/windows" "$C2R/dup/pkg/linux"; : > "$C2R/dup/pkg/pkg.csproj"
  echo a > "$C2R/dup/pkg/windows/same.cs"; echo b > "$C2R/dup/pkg/linux/same.cs"
  st "stripping that MERGES two artifacts is REFUSED" 5 "$0" manifest "$C2R/dup"

  echo "G. --emitted-only -- G's REAL census-root shape, built as an arm"
  # a census TARGET root: seeded flat copies (stamped with the census sentinel) PLUS this run's
  # per-GOOS emission of the same artifact names. Exactly what refused at rc 5 on the real artifact.
  local CR="$T/censusroot"
  mkdir -p "$CR/archive/tar/windows" "$CR/crypto/x509/windows"
  : > "$CR/archive/tar/archive.tar.csproj"; : > "$CR/crypto/x509/crypto.x509.csproj"
  echo seeded > "$CR/archive/tar/package_info.cs"          # seed, flat
  echo seeded > "$CR/archive/tar/stat_unix.cs"             # seed, flat
  echo seeded > "$CR/crypto/x509/package_info.cs"          # seed, flat
  echo emitted > "$CR/archive/tar/windows/package_info.cs" # this run
  echo emitted > "$CR/crypto/x509/windows/package_info.cs" # this run
  touch -d @946684800 "$CR/archive/tar/package_info.cs" "$CR/archive/tar/stat_unix.cs" "$CR/crypto/x509/package_info.cs"
  st "a real census-root shape REFUSES without the flag"  5 "$0" manifest "$CR"
  # the refusal must NAME the remedy, not just fail
  if "$0" manifest "$CR" 2>&1 | grep -q -- '--emitted-only'; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "the refusal names --emitted-only as the remedy"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "refusal does not name the remedy"; fi
  st "  and WITH --emitted-only it succeeds"             0 "$0" manifest --emitted-only "$CR"
  local em; em=$("$0" manifest --emitted-only "$CR" | grep -v '^# ')
  # only the two emitted artifacts survive, both keyed flat
  if [ "$(printf '%s\n' "$em" | wc -l)" -eq 2 ] \
     && printf '%s\n' "$em" | grep -q 'archive/tar/package_info.cs' \
     && printf '%s\n' "$em" | grep -q 'crypto/x509/package_info.cs' \
     && ! printf '%s\n' "$em" | grep -q 'stat_unix.cs'; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "--emitted-only keeps this run's 2, drops the 3 seeded"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "--emitted-only kept the wrong set:"; printf '%s\n' "$em" | sed 's/^/        /'; fi
  # and a root where EVERYTHING is seeded must refuse, not return an empty manifest
  local CR2="$T/allseed"; mkdir -p "$CR2/pkg"; : > "$CR2/pkg/pkg.csproj"
  echo seeded > "$CR2/pkg/a.cs"; touch -d @946684800 "$CR2/pkg/a.cs"
  st "a root with nothing emitted REFUSES (never empty)"  2 "$0" manifest --emitted-only "$CR2"

  echo "H. the layout key folders -- GOARCH and pair folders beside the GOOS ones"
  # the corpus lays a file shared by every amd64 target into amd64/, and one target's file into its
  # pair folder; a tool that knows only windows|linux|darwin reads both as packages of their own.
  local CK="$T/keyed"
  mkdir -p "$CK/pkg/linux" "$CK/pkg/amd64" "$CK/pkg/arm64" "$CK/pkg/linux-amd64" "$CK/pkg/linux-arm64"
  : > "$CK/pkg/pkg.csproj"
  echo flat > "$CK/pkg/shared.cs"; echo lin > "$CK/pkg/linux/only.cs"
  echo a64 > "$CK/pkg/amd64/arch.cs"; echo r64 > "$CK/pkg/arm64/archarm.cs"
  echo la > "$CK/pkg/linux-amd64/pair.cs"; echo lr > "$CK/pkg/linux-arm64/pairarm.cs"
  local kv; kv=$("$0" view "$CK" linux 2>&1)
  if printf '%s\n' "$kv" | grep -q 'pkg/amd64/arch.cs' && printf '%s\n' "$kv" | grep -q 'pkg/linux-amd64/pair.cs' \
     && ! printf '%s\n' "$kv" | grep -q 'pkg/linux-arm64/' && ! printf '%s\n' "$kv" | grep -q 'pkg/arm64/'; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "linux view keeps amd64/ and linux-amd64/, drops arm64 folders"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "linux view read the key folders wrong:"; printf '%s\n' "$kv" | sed 's/^/        /'; fi
  st "view takes a <goos>/<arch> host"                0 "$0" view "$CK" linux/arm64
  kv=$("$0" view "$CK" linux/arm64 2>&1)
  if printf '%s\n' "$kv" | grep -q 'pkg/linux/only.cs' && printf '%s\n' "$kv" | grep -q 'pkg/linux-arm64/pairarm.cs' \
     && printf '%s\n' "$kv" | grep -q 'pkg/arm64/archarm.cs' && ! printf '%s\n' "$kv" | grep -q 'pkg/amd64/' \
     && ! printf '%s\n' "$kv" | grep -q 'pkg/linux-amd64/'; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "linux/arm64 view keeps its folders, drops the amd64 ones"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "linux/arm64 view read the key folders wrong:"; printf '%s\n' "$kv" | sed 's/^/        /'; fi
  st "view refuses a host outside the universe"       2 "$0" view "$CK" darwin/arm64
  local km
  mkdir -p "$T/keyedcensus/pkg/linux-amd64" "$T/keyedcensus/pkg/amd64"; : > "$T/keyedcensus/pkg/pkg.csproj"
  echo la > "$T/keyedcensus/pkg/linux-amd64/pairfile.cs"; echo a64 > "$T/keyedcensus/pkg/amd64/archfile.cs"
  km=$("$0" manifest "$T/keyedcensus" 2>&1)
  if printf '%s\n' "$km" | grep -qE '[[:space:]]pkg/pairfile\.cs$' && printf '%s\n' "$km" | grep -qE '[[:space:]]pkg/archfile\.cs$'; then
    st_pass=$((st_pass+1)); printf '  PASS  %-58s\n' "manifest strips GOARCH and pair folders to the flat key"
  else st_fail=$((st_fail+1)); printf '  FAIL  %-58s\n' "manifest kept a key folder in the key:"; printf '%s\n' "$km" | sed 's/^/        /'; fi
  rm -rf "$T"
  echo
  echo "SELF-TEST: pass=$st_pass fail=$st_fail"
  [ "$st_fail" -eq 0 ] || { echo "SELF-TEST FAILED"; return 1; }
  echo "SELF-TEST PASSED"
}

case "${1:-}" in
  classify) shift; do_classify "$@" ;;
  compare)  shift; do_compare "$@" ;;
  identity) shift; do_identity "$@" ;;
  seedabsent) shift; do_seedabsent "$@" ;;
  view)     shift; do_view "$@" ;;
  manifest) shift; do_manifest "$@" ;;
  pkgdelta) shift; do_pkgdelta "$@" ;;
  selftest) shift; do_selftest ;;
  *) echo "usage: $PROG classify [--seeded-content-only] <win> <lin> <dar>"
     echo "       $PROG compare <out-win> <out-lin> <out-dar> -- <in-win> <in-lin> <in-dar>"
     echo "       $PROG identity <manifest-A> <manifest-B>"
     echo "       $PROG view <corpus-root> <host-goos>[/<arch>]"
     echo "       $PROG manifest [--emitted-only] <census-target-root>   (classify's input, flat-keyed + stamped)"
     echo "       $PROG pkgdelta <goroot-outgoing> <goroot-incoming>"
     echo "       $PROG selftest"; exit 2 ;;
esac
