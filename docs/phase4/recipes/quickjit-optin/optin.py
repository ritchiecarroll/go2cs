#!/usr/bin/env python3
# The OSR opt-in's corpus edit: insert, into every committed descendant of src/go2cs/csproj-template.xml, the
# commented TieredCompilationQuickJitForLoops block exactly as the template now renders it, and check the result.
#   python3 optin.py insert <repo>   # idempotent; keeps each file's line endings
#   python3 optin.py check  <repo>   # completeness: exit 0 only if every rule holds
# The inserted lines are READ FROM THE TEMPLATE (between the anchor and the group's </PropertyGroup>), so this
# script carries no second copy of them.
import subprocess, sys

ANCHOR = "    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>"
GROUP = "<PropertyGroup Condition=\"'$(OutputType)'!='Library'\">"
OPTIN = "<!-- <TieredCompilationQuickJitForLoops>false</TieredCompilationQuickJitForLoops> -->"
MAIN_SIGNATURE = "Enable native compiled output optimizations"
TEST_SIGNATURE = "The test project shares its directory with the production project"
HAND_OWNED = {"src/core/golib/golib.csproj"}  # carries the template's comment, is never emitted by the converter

def tracked_csproj(repo):
    out = subprocess.run(["git", "-C", repo, "ls-files", "-z", "*.csproj"], capture_output=True, check=True).stdout
    return [p for p in out.decode("utf-8").split("\0") if p]

def read(repo, path):
    with open(f"{repo}/{path}", encoding="utf-8", newline="") as f:
        return f.read()

def block_from_template(repo):
    t = read(repo, "src/go2cs/csproj-template.xml").replace("\r\n", "\n").split("\n")
    i = t.index(ANCHOR)
    j = t.index("  </PropertyGroup>", i)
    block = t[i + 1:j]
    assert block and any(OPTIN in l for l in block), "the template carries no opt-in block after the anchor"
    return block

def classify(repo):
    main, test = [], []
    for p in tracked_csproj(repo):
        text = read(repo, p)
        if MAIN_SIGNATURE in text and p not in HAND_OWNED:
            main.append(p)
        elif TEST_SIGNATURE in text:
            test.append(p)
    return main, test

def insert(repo):
    block = block_from_template(repo)
    main, _ = classify(repo)
    done = skipped = 0
    for p in main:
        text = read(repo, p)
        nl = "\r\n" if "\r\n" in text else "\n"
        if OPTIN in text:
            skipped += 1
            continue
        lines = text.split(nl)
        assert lines.count(ANCHOR) == 1, f"{p}: the anchor appears {lines.count(ANCHOR)} times"
        k = lines.index(ANCHOR)
        lines[k + 1:k + 1] = block
        with open(f"{repo}/{p}", "w", encoding="utf-8", newline="") as f:
            f.write(nl.join(lines))
        done += 1
    print(f"insert: {done} edited, {skipped} already carried it, {len(main)} descendants")

def check(repo):
    main, test = classify(repo)
    bad = []
    for p in main:
        text = read(repo, p).replace("\r\n", "\n")
        if text.count(OPTIN) != 1:
            bad.append(f"{p}: carries the opt-in {text.count(OPTIN)} times")
            continue
        g = text.find(GROUP)
        end = text.find("</PropertyGroup>", g)
        at = text.find(OPTIN)
        if g < 0 or not g < at < end:
            bad.append(f"{p}: the opt-in is outside the executable-only group")
    for p in test:
        if "TieredCompilationQuickJitForLoops" in read(repo, p):
            bad.append(f"{p}: a test-host project mentions the opt-in")
    for p in sorted(HAND_OWNED):
        if "TieredCompilationQuickJitForLoops" in read(repo, p):
            bad.append(f"{p}: the hand-owned project was edited")
    print(f"check: {len(main)} template descendants, {len(test)} test hosts, {len(HAND_OWNED)} hand-owned skipped")
    for b in bad[:20]:
        print("  FAIL", b)
    print("CHECK PASS" if not bad else f"CHECK FAIL: {len(bad)} problem(s)")
    return 0 if not bad else 1

if __name__ == "__main__":
    mode, repo = sys.argv[1], sys.argv[2]
    if mode == "insert":
        insert(repo)
    else:
        sys.exit(check(repo))
