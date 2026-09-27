import json, sys, re
res = sys.argv[1]; man = sys.argv[2]; out = sys.argv[3]
m = json.load(open(man, encoding='utf-8-sig'))['disclosures']
alloc = {d['name']: d for d in m if d.get('class') == 'alloc-profile'}
# the results file: a list or dict of test records with output text; collect output per test name
data = json.load(open(res, encoding='utf-8-sig'))
recs = data.get('events') if isinstance(data, dict) and 'events' in data else (data if isinstance(data, list) else [])
outputs = {}
for r in recs:
    name = r.get('test') or r.get('name')
    if not name:
        continue
    text = r.get('output') or ''
    if isinstance(text, list):
        text = ''.join(text)
    outputs.setdefault(name, []).append(text)
rows = []
for name in sorted(alloc):
    text = '\n'.join(outputs.get(name, []))
    note = re.findall(r'go2cs: testing\.AllocsPerRun[^\n]*', text)
    fail = [l for l in text.split('\n') if 'got' in l and ('alloc' in l.lower() or 'want' in l.lower())]
    rows.append((name, alloc[name].get('signature', '')[:80].replace('\t', ' '), (note[0] if note else 'NO-UNIT-NOTE')[:260], (fail[0].strip() if fail else '')[:200]))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('test\tsignature\tunit_note\tfailure_line\n')
    for r in rows:
        f.write('\t'.join(x.replace('\t', ' ').replace('\n', ' ') for x in r) + '\n')
print(len(alloc), 'alloc-profile entries;', sum(1 for r in rows if r[2] != 'NO-UNIT-NOTE'), 'with a unit note; records', len(recs))
