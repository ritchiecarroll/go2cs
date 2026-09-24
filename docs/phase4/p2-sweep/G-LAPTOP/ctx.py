import json
c=json.load(open('/mnt/c/h10/p2/G-LAPTOP/rows/context/go2cs_test_comparison.json'))
print('environment:', c.get('environment'))
print('TestAllocs go/cs:', c['go'].get('TestAllocs'), c['csharp'].get('TestAllocs'), '| disclosed:', c['disclosed'], '| orphaned:', c.get('orphanedDisclosures'))
for e in json.load(open('/mnt/c/h10/p2/G-LAPTOP/rows/context/go2cs_test_results.json'))['events']:
    if e['test'].startswith('TestAllocs') and (e.get('output') or e['action'] in ('pass','fail')):
        print('CS', e['test'], e['action'], repr((e.get('output') or '')[:700]))
