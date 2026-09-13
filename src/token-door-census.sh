#!/usr/bin/env bash
# token-door-census.sh -- which syscall/windows wrappers hand the trampoline a TAGGED TOKEN?
#
# THE PREDICATE, stated before it is run (measurement-discipline: a census states what it asks):
#
#   A wrapper is a MEMBER iff it passes, as a DIRECT (uintptr) argument to the syscall trampoline,
#   a `ж<T>` whose T is REFERENCE-BEARING -- because `operator uintptr` answers such a box with
#   AllocationBase(identityHash), a tagged order token, and `refuseManagedPointerTokens` (the FIRST
#   statement of syscalln, dll_windows.cs:159) throws panic on any tagged argument.
#
# T is reference-bearing iff RuntimeHelpers.IsReferenceOrContainsReferences<T>() is true. In the
# converted corpus that means the struct declares a field of a REFERENCE type. The types that are
# classes in golib: ж<...> (abstract class), array<...>, slice<...>, map<...>, @string, and any
# named struct that itself carries one (checked TRANSITIVELY below -- a one-level scan would
# under-report, and under-reporting is the wrong direction for a census whose finding is a count).
#
# ⚠ SCOPE, stated because it bounds the answer: DIRECT arguments only, exactly as the door's own
# scope note says. A token reached through a structure the trampoline does not decode is invisible
# to the door AND to this census. This narrows the class; it does not close it.
#
# POSITIVE CONTROLS (a census that cannot name a known member is not a census -- floor #13):
#   - StartupInfo         MUST be flagged reference-bearing (4 x ж fields) -- the row under study.
#   - Timezoneinformation MUST be flagged (array<uint16> name fields) -- the known, ALREADY-remedied
#                         member; if it is absent the type predicate is broken.
# NEGATIVE CONTROL:
#   - Timeval / Timespec  MUST NOT be flagged (plain integer fields).
set -u

CORE=${1:-/home/user/go2cs/src/core}
W="$CORE/syscall/windows"
TYPES="$W/types_windows.cs"
ZSYS="$W/zsyscall_windows.cs"

for f in "$TYPES" "$ZSYS"; do
  [ -r "$f" ] || { echo "REFUSE: cannot read $f" >&2; exit 2; }
done

work=$(mktemp -d); trap 'rm -rf "$work"' EXIT

# ⚠ THE CORPUS IS CRLF. Read from CR-stripped copies, once, here -- not with a `\r?` sprinkled
# through each pattern. The first cut of this script anchored field detection on /;$/, which cannot
# match a line ending in ";\r", and the census reported ZERO reference-bearing structs: a clean,
# confident, entirely wrong answer that only the StartupInfo control caught. That is the THIRD tool
# today to meet this exact trap (the H5 applier's `grep -x`, the door guard's brace match, this).
tr -d '\r' < "$TYPES" > "$work/types.cs"; TYPES="$work/types.cs"
tr -d '\r' < "$ZSYS"  > "$work/zsys.cs";  ZSYS="$work/zsys.cs"

# ---- pass 1: every [GoType] struct and the field TYPES it declares -------------------------
# Emitted as "Struct<TAB>fieldtype" rows so pass 2 can close transitively.
awk '
  /^\[GoType\][ \t]*partial struct [A-Za-z_][A-Za-z0-9_]*[ \t]*\{/ {
    name = $0
    sub(/^.*partial struct[ \t]*/, "", name)
    sub(/[ \t]*\{.*$/, "", name)
    cur = name; depth = 1; next
  }
  cur != "" {
    n = gsub(/\{/, "{"); depth += n
    n = gsub(/\}/, "}"); depth -= n
    if (depth <= 0) { cur = ""; next }
    line = $0
    sub(/\/\/.*$/, "", line)                      # a field scan must not read a COMMENT
    gsub(/^[ \t]+|[ \t]+$/, "", line)
    if (line !~ /;$/) next
    sub(/^public[ \t]+/, "", line); sub(/^internal[ \t]+/, "", line)
    sub(/^private[ \t]+/, "", line); sub(/^readonly[ \t]+/, "", line)
    type = line
    sub(/[ \t]+[A-Za-z_ᏑᎮж@][^ \t]*;$/, "", type)  # strip the field NAME, keep the type
    if (type == "" || type == line) next
    print cur "\t" type
  }
' "$TYPES" > "$work/fields"

# ---- pass 2: reference-bearing, closed transitively ----------------------------------------
awk -F'\t' '
  { fields[$1] = fields[$1] "\t" $2; if (!($1 in seen)) { seen[$1]=1; order[++n]=$1 } }
  END {
    # seed: a field whose type IS a golib reference type
    for (s in fields) {
      if (fields[s] ~ /(^|\t)(ж<|array<|slice<|map<|@string|channel<)/) ref[s] = 1
    }
    # close: a struct carrying a reference-bearing struct is reference-bearing too
    changed = 1
    while (changed) {
      changed = 0
      for (s in fields) {
        if (s in ref) continue
        split(fields[s], ft, "\t")
        for (i in ft) { t = ft[i]; if (t != "" && (t in ref)) { ref[s] = 1; changed = 1 } }
      }
    }
    for (i = 1; i <= n; i++) if (order[i] in ref) print order[i]
  }
' "$work/fields" | sort -u > "$work/refbearing"

echo "== reference-bearing [GoType] structs in syscall/windows: $(wc -l < "$work/refbearing")"

# controls, asserted not eyeballed
for must in StartupInfo Timezoneinformation; do
  grep -qx "$must" "$work/refbearing" || { echo "CONTROL FAILED: $must not flagged -- the type predicate is broken" >&2; exit 1; }
done
for mustnot in Timeval Timespec; do
  grep -qx "$mustnot" "$work/refbearing" && { echo "CONTROL FAILED: $mustnot flagged -- the predicate over-reports" >&2; exit 1; }
done
echo "   controls OK: StartupInfo + Timezoneinformation flagged; Timeval + Timespec not"
echo

# ---- pass 3: wrappers passing one of those DIRECTLY to the trampoline -----------------------
# A member is a function whose body has `(uintptr)ᴋN` for a temp assigned from a parameter whose
# declared type is ж<RefBearingStruct>. Read from the emission, per function body.
awk -v reflist="$work/refbearing" -v cntfile="$work/parsed" '
  BEGIN { while ((getline s < reflist) > 0) isref[s] = 1 }
  # ⚠ THE NAME PARSE, rewritten after the first cut emitted a member called "CertContext".
  # These signatures RETURN TUPLES -- `public static (ж<CertContext> context, error err)
  # CertEnumCertificatesInStore(...) {` -- so cutting at the FIRST "(" cuts at the RETURN tuple and
  # names the wrong thing. Parameter lists here nest no parens, so the LAST "(...)" before "{" IS the
  # parameter list; anchor on that. Under-reporting is the dangerous direction for a census whose
  # finding is a count, so `parsed` below is the denominator that makes a blind run visible.
  /^(public|internal) static / && /\{[ \t]*$/ {
    sig = $0
    if (!match(sig, /[A-Za-z_][A-Za-z0-9_]*\([^()]*\)[ \t]*\{[ \t]*$/)) next
    d = substr(sig, RSTART, RLENGTH)
    fn = d; sub(/\(.*$/, "", fn)
    params = d; sub(/^[^(]*\(/, "", params); sub(/\)[ \t]*\{[ \t]*$/, "", params)
    parsed++
    body = ""; depth = 1
    delete pnames
    n2 = split(params, plist, ",")
    for (j = 1; j <= n2; j++) {
      pd = plist[j]; gsub(/^[ \t]+|[ \t]+$/, "", pd)
      if (pd !~ /^ж<[A-Za-z_][A-Za-z0-9_]*>[ \t]/) continue
      t = pd; sub(/^ж</, "", t); sub(/>.*$/, "", t)
      p = pd; sub(/^[^ \t]+[ \t]+/, "", p); gsub(/[ \t]/, "", p)
      if (t in isref) pnames[p] = t
    }
    if (length(pnames) == 0) next
    infn = 1; next
  }
  END { print parsed > cntfile }
  infn {
    n = gsub(/\{/, "{"); depth += n
    n = gsub(/\}/, "}"); depth -= n
    body = body "\n" $0
    if (depth > 0) next
    infn = 0
    # temp <- parameter, then (uintptr)temp handed to a Syscall*/ SyscallN call
    for (p in pnames) {
      if (body ~ ("= *" p " *;") || body ~ ("\\(uintptr\\)" p)) {
        # find the temp assigned from p
        tmp = ""
        if (match(body, "var [^ ]+ = " p " *;")) {
          d = substr(body, RSTART, RLENGTH); tmp = d
          sub(/^var /, "", tmp); sub(/ =.*$/, "", tmp)
        }
        probe = (tmp != "") ? ("\\(uintptr\\)" tmp) : ("\\(uintptr\\)" p)
        if (body ~ probe && body ~ /Syscall/) print fn "\t" pnames[p]
      }
    }
  }
' "$ZSYS" | sort -u > "$work/members"

# The DENOMINATOR. A census reading a small number is either "few members" or "blind", and only the
# count of functions it actually PARSED tells them apart (harness lesson: a guard reading zero on a
# target is either no sites or no vision).
parsedn=$(cat "$work/parsed" 2>/dev/null || echo 0)
# DERIVED, not invented. The first cut asserted `> 200` -- a number with nothing behind it, which
# refused a parse that was in fact COMPLETE. The sound gate compares the parse against an
# INDEPENDENTLY counted population: every bodied signature in the file. (The other 162 `static`
# lines are LazyDLL/LazyProc FIELDS, not functions -- measured, not assumed.)
bodied=$(grep '^\(public\|internal\) static ' "$ZSYS" | grep -c '{$')
echo "== functions parsed in zsyscall_windows.cs: $parsedn of $bodied bodied signatures"
[ "${parsedn:-0}" = "$bodied" ] || { echo "REFUSE: parsed $parsedn of $bodied bodied signatures -- the parse is blind to $((bodied - parsedn))" >&2; exit 1; }

# pass-3 controls: a known member must appear, a known non-member must not.
grep -q "^getStartupInfo	" "$work/members" || { echo "CONTROL FAILED: getStartupInfo absent -- pass 3 is blind" >&2; exit 1; }
grep -q "^GetStdHandle	"   "$work/members" && { echo "CONTROL FAILED: GetStdHandle flagged -- pass 3 over-reports" >&2; exit 1; }
echo "   controls OK: getStartupInfo present; GetStdHandle absent"
echo

echo "== wrappers handing the trampoline a reference-bearing pointer DIRECTLY: $(wc -l < "$work/members")"
echo
printf '   %-30s %-20s %s\n' "WRAPPER" "POINTEE" "STATUS IN zsyscall"
while IFS=$'\t' read -r fn ty; do
  if grep -q "func $fn is hand-converted" "$ZSYS"; then st="DISPLACED"; else st="LIVE -- token reaches the door"; fi
  printf '   %-30s %-20s %s\n' "$fn" "$ty" "$st"
done < "$work/members"
