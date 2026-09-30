# tK-k6check.py -- TRAIN K fixup helper for PRERES-K6: refresh ONE committed file from the union converter's seeded
# emission, but only if the difference is exactly the one [StackTraceHidden] hunk the ruling names.
#
#   python tK-k6check.py <repo> <relpath> <emitted-root> <sentinel> [--apply]
#
#   <repo>          the union worktree (the committed/worktree copy of <relpath> is read there)
#   <relpath>       e.g. src/core/os/linux/file_unix.cs
#   <emitted-root>  the seeded temp root the conversion wrote into (it holds src/core/...)
#   <sentinel>      a file touched just before the conversion: the emitted copy must be NEWER (written by this run,
#                   not the seed's own copy left in place), and the package's FLAT twin must NOT be newer (layout L3
#                   adoption held: the single-target reconvert wrote the per-GOOS file, not a flat duplicate)
#
# The rule (CR-stripped, line-wise): exactly ONE changed line; the emitted line equals the committed line with
# '[global::System.Diagnostics.StackTraceHidden] ' prepended; the committed line declares sigpipe(). Anything else
# aborts and prints the difference. With --apply the emitted content is written over the worktree file in the
# worktree file's own line-ending convention. Exit: 0 pass (and applied), 3 fail, 2 usage.
import difflib, os, subprocess, sys

PREFIX = "[global::System.Diagnostics.StackTraceHidden] "

if len(sys.argv) < 5:
    print("usage: tK-k6check.py <repo> <relpath> <emitted-root> <sentinel> [--apply]")
    sys.exit(2)
repo, rel, root, sentinel = sys.argv[1:5]
apply = "--apply" in sys.argv[5:]


def die(m):
    print(f"K6 FAIL {rel}: {m}")
    sys.exit(3)


t0 = os.stat(sentinel).st_mtime
em = os.path.join(root, rel)
if not os.path.exists(em):
    die(f"the conversion wrote no {em}")
if os.stat(em).st_mtime <= t0:
    die("the emitted copy is not newer than the sentinel (the conversion did not write it)")
parts = rel.split("/")
flat = os.path.join(root, *parts[:-2], parts[-1])  # src/core/os/linux/file_unix.cs -> src/core/os/file_unix.cs
if os.path.exists(flat) and os.stat(flat).st_mtime > t0:
    die(f"the conversion wrote the FLAT twin {flat} (layout L3 adoption did not hold)")

wt_path = os.path.join(repo, rel)
wt_bytes = open(wt_path, "rb").read()
head = subprocess.run(["git", "-C", repo, "show", f"HEAD:{rel}"], capture_output=True)
if head.returncode != 0:
    die("not tracked at HEAD")
if wt_bytes.replace(b"\r", b"") != head.stdout.replace(b"\r", b""):
    die("the worktree copy already differs from HEAD (refusing to stack on an unknown edit)")

em_bytes = open(em, "rb").read()
a = wt_bytes.decode("utf-8").replace("\r\n", "\n").split("\n")
b = em_bytes.decode("utf-8").replace("\r\n", "\n").split("\n")
ops = [op for op in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes() if op[0] != "equal"]
if not ops:
    die("the emission equals the committed file: no [StackTraceHidden] hunk to take (the ruling's premise is gone)")
if len(ops) != 1:
    for line in list(difflib.unified_diff(a, b, "committed", "emitted", n=1, lineterm=""))[:60]:
        print("   ", line)
    die(f"{len(ops)} hunks, the ruling allows exactly one")
tag, i1, i2, j1, j2 = ops[0]
if not (tag == "replace" and i2 - i1 == 1 and j2 - j1 == 1):
    for line in list(difflib.unified_diff(a, b, "committed", "emitted", n=1, lineterm=""))[:60]:
        print("   ", line)
    die(f"the one hunk is '{tag}' {i2 - i1}->{j2 - j1} lines, the ruling allows one line changed in place")
old, new = a[i1], b[j1]
if new != PREFIX + old or "sigpipe()" not in old:
    print(f"    committed line {i1 + 1}: {old}")
    print(f"    emitted   line {j1 + 1}: {new}")
    die("the changed line is not the committed sigpipe() declaration with [StackTraceHidden] prepended")
print(f"K6 OK {rel}: one hunk at line {i1 + 1}: +[StackTraceHidden] on '{old.strip()}'")

if apply:
    nl = "\r\n" if b"\r\n" in wt_bytes else "\n"
    text = "\n".join(b).replace("\n", nl)
    with open(wt_path, "w", encoding="utf-8", newline="") as f:
        f.write(text)
    print(f"K6 APPLIED {rel} ({'CRLF' if nl == chr(13) + chr(10) else 'LF'}, {len(b)} lines)")
sys.exit(0)
