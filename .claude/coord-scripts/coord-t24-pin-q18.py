# coord-t24-pin-q18.py -- the train-24 UNION fix: C1's train-23 amendment made TestDeclarationKeyedCapabilityEntries per-entry
# (an unpinned test-named key fails outright); SUB-Q18's twelve declaration-keyed entries (testing_test.<Name>, cut before that
# rule existed) must be PINNED in the test's map with their own capability strings. Usage: python coord-t24-pin-q18.py <worktree>
import io, re, sys
D = sys.argv[1]
mp = io.open(D + r"\src\go2cs\testConversion.go", encoding="utf-8", newline="").read()
# entries of the capability map keyed on testing_test.<Name>: a tab, the quoted key, a colon, the quoted value, a comma
key_re = re.compile(r'^\t"(testing_test\.[A-Za-z0-9_]+)":\s*"((?:[^"\\]|\\.)*)",', re.M)
ents = key_re.findall(mp)
print("testing_test entries in the map:", len(ents))
for k, _ in ents:
    print("  ", k)
tf = D + r"\src\go2cs\testConversion_test.go"
s = io.open(tf, encoding="utf-8", newline="").read()
anchor = '"runtime/pprof.TestFakeMapping": {capability:'
i = s.find(anchor)
assert i >= 0, "anchor entry not found"
e = s.find("\n", i)
nl = "\r\n" if "\r\n" in s[:4000] else "\n"
if "testing_test." in s[i:i + 6000]:
    print("already pinned"); sys.exit(0)
block = nl + "\t\t// testing/*_test.go: `package testing_test` -> external -> testing_test.<Name> (SUB-Q18's twelve, pinned at the train-24 union under this per-entry rule)."
for k, v in ents:
    block += nl + '\t\t"%s": {capability: "%s", internal: false},' % (k, v)
s = s[:e] + block + s[e:]
io.open(tf, "w", encoding="utf-8", newline="").write(s)
print("pinned %d entries" % len(ents))
