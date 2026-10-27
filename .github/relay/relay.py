# relay.py <run-id> <artifact-dir>
#
# Reads one os-matrix sweep-shard run's two mac artifacts (osx-arm64, osx-x64), already downloaded under
# <artifact-dir>/<rid>/, and prints each swept row's reading as GitHub annotations, so a lane whose egress denies the
# artifact blob host can read the counts through the check-runs API alone. Nothing is written back to the run.
#
# One line per row per RID: row, RID, verdicts, matched, disclosed, divergent, new, status, agree, record-sha256-12.
#   verdicts   the verdict names on either side (go and csharp keys, united)
#   matched    names whose go and csharp verdicts are equal, identical skips included
#   disclosed  len(comparison.disclosed), the disclosure list
#   divergent  verdicts - matched
#   new        divergent names absent from the disclosure list (each entry's name, before its " (class)")
#   agree      osx-arm64 == osx-x64 verdict for verdict, on both the go and the csharp side
import hashlib, json, os, sys

run, root = sys.argv[1], sys.argv[2]
RIDS = ('osx-arm64', 'osx-x64')

def esc(text):
    return text.replace('%', '%25').replace('\r', '%0D').replace('\n', '%0A')

def esc_prop(text):
    return esc(text).replace(':', '%3A').replace(',', '%2C')

def load(rid):
    rows = {}
    base = os.path.join(root, rid)
    for d, _, files in os.walk(base):
        if 'go2cs_test_comparison.json' not in files:
            continue
        path = os.path.join(d, 'go2cs_test_comparison.json')
        raw = open(path, 'rb').read()
        rec = json.loads(raw.decode('utf-8-sig'))
        pkg = rec.get('package') or os.path.relpath(d, base)
        if pkg in rows:
            sys.exit(f'{rid}: two comparison records for {pkg}')
        rows[pkg] = (rec, hashlib.sha256(raw).hexdigest()[:12])
    return rows

def reading(rec):
    go = rec.get('go') or {}
    cs = rec.get('csharp') or {}
    names = set(go) | set(cs)
    matched = sum(1 for n in names if n in go and n in cs and go[n] == cs[n])
    disclosed = list(rec.get('disclosed') or [])
    # An entry reads "Name (class): text"; Go spells a space in a test name as '_', so the name ends at the first " (".
    disclosed_names = {entry.split(' (', 1)[0] for entry in disclosed}
    divergent = sorted(n for n in names if not (n in go and n in cs and go[n] == cs[n]))
    new = [n for n in divergent if n not in disclosed_names]
    return len(names), matched, len(disclosed), len(divergent), new, rec.get('status', '?')

by_rid = {rid: load(rid) for rid in RIDS}
pkgs = sorted(set(by_rid['osx-arm64']) | set(by_rid['osx-x64']))

table, details = [], []
for pkg in pkgs:
    a = by_rid['osx-arm64'].get(pkg)
    x = by_rid['osx-x64'].get(pkg)
    agree = '-'
    if a and x:
        same = (a[0].get('go') or {}) == (x[0].get('go') or {}) and (a[0].get('csharp') or {}) == (x[0].get('csharp') or {})
        agree = 'yes' if same else 'no'
        if not same:
            diff = []
            for side in ('go', 'csharp'):
                ma, mx = a[0].get(side) or {}, x[0].get(side) or {}
                for n in sorted(set(ma) | set(mx)):
                    if ma.get(n) != mx.get(n):
                        diff.append(f'{side}:{n}={ma.get(n, "-")}/{mx.get(n, "-")}')
            details.append(f'{pkg} RIDs differ (arm64/x64), {len(diff)}: ' + '; '.join(diff[:15]) + (' ...' if len(diff) > 15 else ''))
    for rid, got in (('osx-arm64', a), ('osx-x64', x)):
        if not got:
            table.append(f'{pkg}\t{rid}\t-\t-\t-\t-\t-\tMISSING\t-\t-')
            continue
        verdicts, matched, disclosed, divergent, new, status = reading(got[0])
        table.append(f'{pkg}\t{rid}\t{verdicts}\t{matched}\t{disclosed}\t{divergent}\t{len(new)}\t{status}\t{agree}\t{got[1]}')
        if new:
            details.append(f'{pkg} {rid} new {len(new)}: ' + ', '.join(new[:15]) + (' ...' if len(new) > 15 else ''))

def emit(level, title, lines, cap=8, budget=3000):
    chunks, cur = [], ''
    for line in lines:
        if cur and len(cur) + len(line) + 1 > budget:
            chunks.append(cur)
            cur = ''
        cur += line + '\n'
    if cur:
        chunks.append(cur)
    if len(chunks) > cap:
        dropped = len(chunks) - cap + 1
        chunks = chunks[:cap - 1] + [f'{dropped} chunk(s) not shown; the annotation cap is {cap} per level\n']
    for i, chunk in enumerate(chunks, 1):
        print(f'::{level} title={esc_prop(f"{title} {i}/{len(chunks)}")}::{esc(chunk.rstrip())}')

summary = f'run {run}: {len(pkgs)} row(s); records arm64 {len(by_rid["osx-arm64"])}, x64 {len(by_rid["osx-x64"])}'
print(summary)
emit('notice', f'relay {run} table', [summary] + table)
if details:
    emit('warning', f'relay {run} details', details)
