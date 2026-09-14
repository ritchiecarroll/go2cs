#!/usr/bin/env python3
"""coord-merge-disclosures.py <repo-relative path> -- resolve an append-append conflict on a go2cs_test_disclosures.json
manifest by UNION: ours' entries in ours' order, then every theirs entry whose (name, signature) is not already present.
Reads the three index stages (:1: base, :2: ours, :3: theirs) from git, writes the union to the working-tree path with
the file's own indentation (2 spaces) and CRLF endings if ours used them, and prints the counts. Exit 1 on any surprise
(an entry with the same name but a different signature on both sides is a real conflict, not an append-append)."""
import json, subprocess, sys

path = sys.argv[1]

def stage(n):
    out = subprocess.run(["git", "show", f":{n}:{path}"], capture_output=True)
    if out.returncode != 0:
        sys.exit(f"stage {n} of {path} unreadable: {out.stderr.decode(errors='replace').strip()}")
    return out.stdout

base_b, ours_b, theirs_b = stage(1), stage(2), stage(3)
crlf = b"\r\n" in ours_b
base, ours, theirs = (json.loads(b.decode("utf-8-sig")) for b in (base_b, ours_b, theirs_b))
for d in (base, ours, theirs):
    if set(d.keys()) != {"schemaVersion", "disclosures"} or d["schemaVersion"] != 1:
        sys.exit(f"unexpected manifest schema: {list(d.keys())}")

def key(e):
    return (e["name"], e["signature"])

merged = list(ours["disclosures"])
seen = {key(e) for e in merged}
names = {e["name"]: e for e in merged}
added = 0
for e in theirs["disclosures"]:
    k = key(e)
    if k in seen:
        continue
    if e["name"] in names and names[e["name"]] != e:
        sys.exit(f"REAL CONFLICT: {e['name']} present on both sides with different content")
    merged.append(e)
    seen.add(k)
    added += 1

# entries removed on either side relative to base are removals to honour, not additions to keep
base_keys = {key(e) for e in base["disclosures"]}
ours_keys = {key(e) for e in ours["disclosures"]}
theirs_keys = {key(e) for e in theirs["disclosures"]}
removed = (base_keys - ours_keys) | (base_keys - theirs_keys)
if removed:
    merged = [e for e in merged if key(e) not in removed]

text = json.dumps({"schemaVersion": 1, "disclosures": merged}, indent=2, ensure_ascii=False) + "\n"
data = text.replace("\n", "\r\n").encode("utf-8") if crlf else text.encode("utf-8")
with open(path, "wb") as f:
    f.write(data)
print(f"disclosures union: base={len(base['disclosures'])} ours={len(ours['disclosures'])} theirs={len(theirs['disclosures'])} "
      f"-> merged={len(merged)} (added from theirs {added}, removed {len(removed)}) crlf={crlf}")
