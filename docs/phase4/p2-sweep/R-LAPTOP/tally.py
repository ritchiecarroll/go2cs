import csv, re
from collections import Counter

E = r'<evidence-root>'
rec = {}
with open(E + r'\recompute-disclosed.tsv', encoding='utf-8') as f:
    for x in csv.DictReader(f, delimiter='\t'):
        rec[x['row']] = x
with open(E + r'\ledger.tsv', encoding='utf-8') as f:
    led = list(csv.DictReader(f, delimiter='\t'))

print('ledger', len(led), 'recomputed', len(rec))
mm = ['%s got=%s banked=%s' % (l['package'], rec[l['package']]['got_disclosed'], l['banked_disclosed'])
      for l in led if rec[l['package']]['got_disclosed'].strip() != l['banked_disclosed']]
print('gotDisclosed != banked Disclosed:', mm or 'NONE')
print('sum gotDisclosed', sum(int(rec[l['package']]['got_disclosed']) for l in led),
      'sum banked Disclosed', sum(int(l['banked_disclosed']) for l in led))
print('sum got', sum(int(l['got']) for l in led), 'sum banked Tests', sum(int(l['banked_tests']) for l in led))
print('orphanedDisclosures field:', dict(Counter(r['orphaned'] for r in rec.values())))
bad = []
for l in led:
    m = re.search(r'page (\d+)\|(\d+)', l['page_reading'])
    if not m or m.group(1) != l['got'] or m.group(2) != rec[l['package']]['got_disclosed'].strip():
        bad.append(l['package'])
print('P-2(a) page N|D == run got|gotDisclosed:', ('mismatch: ' + ','.join(bad)) if bad else 'NONE, 89/89')
bad2 = [l['package'] for l in led if not re.search(r'page %s\|%s ' % (l['banked_tests'], l['banked_disclosed']), l['page_reading'])]
print('P-2(b) banked Tests|Disclosed == page N|D:', bad2 or 'NONE, 89/89')
print('pages:', dict(Counter(l['page_reading'].split(' P2a')[0] for l in led)))
print('words:', dict(Counter(l['word'] for l in led)), 'rc:', dict(Counter(l['rc'] for l in led)))
