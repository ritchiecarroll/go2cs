# tK-s1census.py -- TRAIN K fixup helper for P1's S1 csproj regeneration (inbox COORD 20260930T102227Z-P1,
# ruled in tK-seats-draft.txt '# S1 REGENERATION'). Read-only except where a mode says otherwise (none writes).
#
#   python tK-s1census.py eolsnap  <repo> <list.z> <snap.json>  # per listed file: (#LF, #CRLF) BEFORE the sed
#   python tK-s1census.py eolcheck <repo> <list.z> <snap.json>  # AFTER the sed: line endings unchanged per file,
#                                                               # old token 0x, new token exactly 1x in every file
#   python tK-s1census.py census   <repo> [<seats-file>]        # the (1,0,1) completeness census
#
# The census counts three tokens per tracked csproj outside src/archived/ and docs/ (P1's recipe, verbatim):
#   c = <LangVersion Condition="'$(LangVersion)'==''">14</LangVersion>
#   l = <LangVersion>latest</LangVersion>
#   a = any '<LangVersion'
# Every file must read (1,0,1) except the named ones below. (2,*,*) = DOUBLE EDIT, (0,1,1) = MISS; both abort, and
# so does ANY other unexpected tuple (the ruling states the whole shape, so anything outside it is unruled).
# Exit: 0 pass, 3 fail. Prints a tally line 'CENSUS tally ...' the fixup parses.
import json, os, subprocess, sys
from collections import Counter

TOK_C = "<LangVersion Condition=\"'$(LangVersion)'==''\">14</LangVersion>"
TOK_L = "<LangVersion>latest</LangVersion>"
TOK_A = "<LangVersion"

# P1's ruled exceptions, measured at f819887fa3 + S1.
EXCEPT = {
    "src/gen/go2cs-gen/go2cs-gen.csproj": (0, 0, 1),                    # its own unconditional 14 (netstandard2.0)
    "src/tests/ElemAliasProbe/ElemAliasProbe.csproj": (0, 0, 0),         # sets none, inherits src/Directory.Build.props
    "src/tests/PackageTests/RidCompileAsset/RidCompileAsset.csproj": (0, 0, 0),
    "src/utilities/UpdateTestTargets/UpdateTestTargets.csproj": (0, 0, 0),
}
# K-DERIVED, NOT in P1's ruling (the ruling predates the seat): i9-j0-uuid-rehearsal 86508936a3 adds a hand-written
# consumer project that sets no LangVersion. j0-consume.ps1 copies it to a scratch dir before building, so it takes the
# SDK's net10.0 default (C# 14), not src/Directory.Build.props. A gate fails CLOSED on what its ruling does not name,
# so this candidate is admitted ONLY when (1) a '# S1 REGENERATION' line of the seat file (the optional third argument
# to 'census') names its path -- COORD's amendment of the ruling -- or (2) K_J0_EXCEPTION=1 is set for the run.
# Otherwise it reads as UNEXPECTED (0,0,0) and the census fails. Which authority admitted it is printed by name.
K_EXCEPT = {
    "src/tools/j0-uuid-rehearsal/consumer/J0UuidConsumer.csproj": (0, 0, 0),
}
RULING_PREFIX = "# S1 REGENERATION"


def k_authority(seats, p):
    """How candidate p is admitted, or None: 'ruling' (named on a '# S1 REGENERATION' line of the seat file) or 'env'."""
    if seats:
        with open(seats, encoding="utf-8") as f:
            if any(ln.startswith(RULING_PREFIX) and p in ln for ln in f):
                return "ruling"
    if os.environ.get("K_J0_EXCEPTION", "0") == "1":
        return "env K_J0_EXCEPTION=1"
    return None


def read(repo, p):
    with open(f"{repo}/{p}", "rb") as f:
        return f.read()


def listed(path):
    with open(path, "rb") as f:
        return [p.decode("utf-8") for p in f.read().split(b"\0") if p]


def eolsnap(repo, lst, out):
    snap = {}
    for p in listed(lst):
        b = read(repo, p)
        snap[p] = [b.count(b"\n"), b.count(b"\r\n")]
    with open(out, "w", encoding="utf-8") as f:
        json.dump(snap, f)
    crlf = sum(1 for v in snap.values() if v[0] == v[1])
    print(f"EOLSNAP files={len(snap)} all-CRLF={crlf} other={len(snap) - crlf}")
    return 0


def eolcheck(repo, lst, snapf):
    with open(snapf, encoding="utf-8") as f:
        snap = json.load(f)
    files = listed(lst)
    bad = []
    if sorted(files) != sorted(snap):
        bad.append("the list and the snapshot name different files")
    for p in files:
        b = read(repo, p)
        now = [b.count(b"\n"), b.count(b"\r\n")]
        if now != snap.get(p):
            bad.append(f"{p}: line endings changed {snap.get(p)} -> {now}")
        t = b.decode("utf-8", "surrogateescape")
        if t.count(TOK_L) != 0 or t.count(TOK_C) != 1:
            bad.append(f"{p}: after the sed old={t.count(TOK_L)} new={t.count(TOK_C)} (want 0/1)")
    for x in bad[:20]:
        print("  FAIL", x)
    print(f"EOLCHECK files={len(files)} problems={len(bad)}")
    return 0 if not bad else 3


def census(repo, seats=None):
    out = subprocess.run(["git", "-C", repo, "ls-files", "-z", "*.csproj"], capture_output=True, check=True).stdout
    files = [p for p in out.decode("utf-8").split("\0") if p and not p.startswith(("src/archived/", "docs/"))]
    expect = dict(EXCEPT)
    admitted = {}
    for p, tup in K_EXCEPT.items():
        how = k_authority(seats, p)
        if how:
            expect[p] = tup
            admitted[p] = how
    tally, bad, seen = Counter(), [], set()
    for p in files:
        t = read(repo, p).decode("utf-8", "surrogateescape")
        tup = (t.count(TOK_C), t.count(TOK_L), t.count(TOK_A))
        tally[tup] += 1
        want = expect.get(p, (1, 0, 1))
        if p in expect:
            seen.add(p)
        if tup == want:
            continue
        if tup[0] >= 2:
            bad.append(f"DOUBLE EDIT {tup} {p}")
        elif tup == (0, 1, 1):
            bad.append(f"MISS {tup} {p}")
        else:
            bad.append(f"UNEXPECTED {tup} (want {want}) {p}")
    for p in sorted(set(expect) - seen):
        bad.append(f"NAMED EXCEPTION ABSENT from the tree: {p}")
    for p in sorted(K_EXCEPT):
        if p in admitted and p in seen:
            print(f"  K-EXCEPTION admitted by {admitted[p]} {expect[p]} {p}")
        elif p not in admitted:
            print(f"  K-EXCEPTION NOT ADMITTED (named by no '{RULING_PREFIX}' line and K_J0_EXCEPTION!=1): {p}")
    for x in bad[:40]:
        print("  FAIL", x)
    t = " ".join(f"({a},{b},{c})={n}" for (a, b, c), n in sorted(tally.items(), key=lambda kv: -kv[1]))
    print(f"CENSUS tally files={len(files)} {t}")
    print("CENSUS PASS" if not bad else f"CENSUS FAIL: {len(bad)} problem(s)")
    return 0 if not bad else 3


if __name__ == "__main__":
    mode = sys.argv[1]
    if mode == "eolsnap":
        sys.exit(eolsnap(*sys.argv[2:5]))
    if mode == "eolcheck":
        sys.exit(eolcheck(*sys.argv[2:5]))
    if mode == "census":
        sys.exit(census(sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else None))
    print("usage: eolsnap|eolcheck|census ...")
    sys.exit(2)
