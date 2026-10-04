#!/usr/bin/env bash
# check-warning-entries.sh -- do the per-file warning entries match the warnings, both ways?
#
# The converter writes a `.editorconfig` beside a package's project file that turns off CS0219,
# CS0649 or CS0675 in the files whose Go source holds the fact (src/go2cs/warningEntries.go). This
# gate checks the entries against the compiler, per flavour, in the two directions a stale entry
# set can be wrong:
#
#   A  MISSING: build the whole standard library with the entries in place. Any CS0219, CS0649 or
#      CS0675 in a converted source file (src/core, not Generated/) is a warning with no entry: FAIL.
#   B  STALE: move every marked entry file aside and build the packages that held one. Every entry
#      must warn on at least one of the flavours its comment line names that this run builds (a
#      flat file's entry may be inert on the others); an entry for a file under `<goos>/` must warn
#      on THAT flavour. Otherwise FAIL. An entry none of whose flavours this run builds is reported
#      UNMEASURED, not passed. The entry files are put back byte for byte, and the tree is checked
#      for deletions.
#
# It is a TRAIN and PRE-RELEASE gate, not a per-lane one: it costs one standard-library build and
# one entry-package build per flavour. The Go unit tests per fact run under `go test ./...`.
#
# POSITIVE CONTROL (a gate that has never failed proves nothing): `--control` plants a file with an
# unread `int zz = 1;` in a package that holds no entry. A must then fail and NAME that file; the run
# reports CONTROL CAUGHT and exits 0 only if it did. The plant is removed on every exit the shell
# sees; a host killed mid-run leaves it (zz_warning_entries_control.cs), and the next A names it.
#
# usage: check-warning-entries.sh [--flavours linux,windows] [--control] [--only-a|--only-b] [--logs DIR]
#   Run from a clean tree (no uncommitted .editorconfig). Build output is left in place; purge it after.
set -u

SRC="$(cd "$(dirname "$0")" && pwd)"
CORE="$SRC/core"
MARKER='# go2cs: per-file warning entries, written by every conversion of this package -- do not edit'
CODES='CS0219|CS0649|CS0675'

FLAVOURS="linux,windows"
CONTROL=0
RUN_A=1
RUN_B=1
LOGS=""

while [ $# -gt 0 ]; do
  case "$1" in
    --flavours) FLAVOURS="$2"; shift 2 ;;
    --control) CONTROL=1; RUN_B=0; shift ;;
    --only-a) RUN_B=0; shift ;;
    --only-b) RUN_A=0; shift ;;
    --logs) LOGS="$2"; shift 2 ;;
    *) echo "usage: $0 [--flavours linux,windows] [--control] [--only-a|--only-b] [--logs DIR]" >&2; exit 2 ;;
  esac
done

[ -d "$CORE" ] || { echo "ABORT: no core tree at $CORE" >&2; exit 2; }
command -v dotnet >/dev/null 2>&1 || { echo "ABORT: dotnet is not on PATH" >&2; exit 2; }

if [ -z "$LOGS" ]; then LOGS="$(mktemp -d)"; fi
mkdir -p "$LOGS"

dirty="$(git -C "$SRC" status --porcelain -- 'core/**/.editorconfig' 'core/.editorconfig')"
if [ -n "$dirty" ]; then
  echo "ABORT: uncommitted entry files; the gate measures the committed set:" >&2
  echo "$dirty" >&2
  exit 2
fi

IFS=',' read -r -a FLAVOUR_LIST <<< "$FLAVOURS"
fail=0

# warnings LOG -> "core-relative-file|line|col|code" per converted-source warning, unique.
warnings() {
  tr '\\' '/' < "$1" | grep -aoE "[^ ]*/src/core/[^ ]*\.cs\([0-9]+,[0-9]+\): warning ($CODES)" |
    grep -av '/Generated/' |
    sed -E 's#^.*/src/core/##; s#\(([0-9]+),([0-9]+)\): warning #|\1|\2|#' | sort -u
}

# ---- marker files and their entries --------------------------------------------------------------
MARKED=()
while IFS= read -r -d '' f; do
  if [ "$(head -n 1 "$f" | tr -d '\r')" = "$MARKER" ]; then MARKED+=("$f"); fi
done < <(find "$CORE" -name .editorconfig -not -path '*/bin/*' -not -path '*/obj/*' -print0 | sort -z)

# entries -> "pkg|section|code|flavours" (pkg core-relative, section package-relative, flavours from
# the comment line above the code: `# CS0219: darwin linux windows`)
ENTRIES="$LOGS/entries.txt"
: > "$ENTRIES"
for f in "${MARKED[@]}"; do
  pkg="${f#"$CORE"/}"; pkg="${pkg%/.editorconfig}"
  tr -d '\r' < "$f" | awk -v pkg="$pkg" '
    /^\[\// { section = substr($0, 3, length($0) - 3); flavours = ""; next }
    /^# CS[0-9]+: / { flavours = substr($0, index($0, ": ") + 2); next }
    /^dotnet_diagnostic\.CS[0-9]+\.severity = none$/ { split($0, p, "."); print pkg "|" section "|" p[2] "|" flavours }
  ' >> "$ENTRIES"
done
echo "entry files: ${#MARKED[@]}  entries: $(wc -l < "$ENTRIES")  flavours: $FLAVOURS  logs: $LOGS"

# ---- control plant -------------------------------------------------------------------------------
CONTROL_PKG=unicode/utf8
PLANT="$CORE/$CONTROL_PKG/zz_warning_entries_control.cs"
cleanup_plant() { rm -f "$PLANT"; }
if [ "$CONTROL" = 1 ]; then
  if grep -q "^$CONTROL_PKG|" "$ENTRIES"; then
    echo "ABORT: the control package ($CONTROL_PKG) holds entries; pick another" >&2; exit 2
  fi
  trap cleanup_plant EXIT
  printf 'namespace go;\r\n\r\ninternal static class zz_warning_entries_control {\r\n    internal static void F() {\r\n        int zz = 1;\r\n    }\r\n}\r\n' > "$PLANT"
  echo "CONTROL planted core/$CONTROL_PKG/zz_warning_entries_control.cs"
fi

# ---- A: missing entries --------------------------------------------------------------------------
if [ "$RUN_A" = 1 ]; then
  for flavour in "${FLAVOUR_LIST[@]}"; do
    log="$LOGS/a-$flavour.log"
    echo "A [$flavour] building go2cs-stdlib.slnx (Release) ..."
    # --no-incremental: an up-to-date project is not compiled, so it reports NO warnings -- an
    # incremental A would pass on a stale build without having looked.
    dotnet build "$SRC/go2cs-stdlib.slnx" -c Release -p:GoTargetOS="$flavour" --no-incremental -clp:NoSummary > "$log" 2>&1
    rc=$?
    found="$(warnings "$log")"
    count=0; [ -n "$found" ] && count=$(printf '%s\n' "$found" | wc -l)
    echo "A [$flavour] build rc=$rc  warnings with no entry: $count"
    if [ "$rc" != 0 ]; then
      echo "  FAIL: the build itself failed (errors: $(grep -ac ' error ' "$log")); see $log"; fail=1
    fi
    if [ -n "$found" ]; then
      printf '%s\n' "$found" | sed 's/^/  MISSING /'; fail=1
    fi
  done
fi

if [ "$CONTROL" = 1 ]; then
  if for log in "$LOGS"/a-*.log; do warnings "$log"; done | grep -qF "$CONTROL_PKG/zz_warning_entries_control.cs|"; then
    echo "CONTROL CAUGHT: A failed and named the planted file"; exit 0
  fi
  echo "CONTROL MISSED: A did not name the planted file -- the gate is blind"; exit 1
fi

# ---- B: stale entries ----------------------------------------------------------------------------
if [ "$RUN_B" = 1 ] && [ "${#MARKED[@]}" -gt 0 ]; then
  ASIDE="$LOGS/aside"
  mkdir -p "$ASIDE"
  restore() {
    for f in "${MARKED[@]}"; do
      rel="${f#"$CORE"/}"
      [ -f "$ASIDE/$rel" ] && mv "$ASIDE/$rel" "$f"
    done
  }
  trap restore EXIT

  for f in "${MARKED[@]}"; do
    rel="${f#"$CORE"/}"
    mkdir -p "$ASIDE/$(dirname "$rel")"
    cp -p "$f" "$ASIDE/$rel.orig"
    mv "$f" "$ASIDE/$rel"
  done

  : > "$LOGS/b-warnings.txt"
  for flavour in "${FLAVOUR_LIST[@]}"; do
    for f in "${MARKED[@]}"; do
      dir="$(dirname "$f")"
      pkg="${dir#"$CORE"/}"
      proj="$(find "$dir" -maxdepth 1 -name '*.csproj' -not -name '*.tests.csproj' | head -n 1)"
      [ -n "$proj" ] || { echo "  FAIL: no project file beside core/$pkg/.editorconfig"; fail=1; continue; }
      log="$LOGS/b-$flavour-$(printf '%s' "$pkg" | tr '/' '_').log"
      dotnet build "$proj" -c Release -p:GoTargetOS="$flavour" --no-incremental -clp:NoSummary > "$log" 2>&1
      rc=$?
      echo "B [$flavour] core/$pkg rc=$rc"
      warnings "$log" | sed "s/^/$flavour|/" >> "$LOGS/b-warnings.txt"
    done
  done

  restore
  trap - EXIT

  for f in "${MARKED[@]}"; do
    rel="${f#"$CORE"/}"
    if ! cmp -s "$f" "$ASIDE/$rel.orig"; then echo "  FAIL: core/$rel did not come back byte for byte"; fail=1; fi
  done

  deleted="$(git -C "$SRC" status --porcelain | grep '^ D' || true)"
  if [ -n "$deleted" ]; then echo "  FAIL: tracked files deleted:"; echo "$deleted"; fail=1; fi

  # Each entry: warned on which flavours?
  while IFS='|' read -r pkg section code listed; do
    warned=""; measured=""
    for flavour in "${FLAVOUR_LIST[@]}"; do
      [[ " $listed " == *" $flavour "* ]] || continue
      measured="$measured $flavour"
      if grep -q "^$flavour|$pkg/$section|[0-9]*|[0-9]*|$code\$" "$LOGS/b-warnings.txt"; then warned="$warned $flavour"; fi
    done
    folder="${section%%/*}"
    if [ -z "$measured" ]; then
      echo "  UNMEASURED core/$pkg [/$section] $code: named for $listed, none of which this run built"
    elif [ -z "$warned" ]; then
      echo "  STALE core/$pkg [/$section] $code: warns on none of$measured"; fail=1
    elif [ "$folder" != "$section" ] && [[ ",$FLAVOURS," == *",$folder,"* ]] && [[ " $warned " != *" $folder "* ]]; then
      echo "  STALE core/$pkg [/$section] $code: under $folder/ but does not warn on $folder (warns on:$warned)"; fail=1
    else
      echo "  ok   core/$pkg [/$section] $code: warns on$warned"
    fi
  done < "$ENTRIES"
fi

if [ "$fail" = 0 ]; then echo "PASS"; else echo "FAIL"; fi
exit "$fail"
