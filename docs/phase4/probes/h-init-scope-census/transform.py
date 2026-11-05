# Apply candidate (a), strict, to a corpus copy: blank the `{` and `}` lines of every init block the
# predicate admits (no other occurrence of a declared name in the enclosing space, no closure in the
# block, no label + closure), keeping every other line and every line number. Whitespace only.
#   py -I -X utf8 transform.py <joined.jsonl> <census repo root> <target repo root>
import json, sys, collections

joined, src_root, dst_root = sys.argv[1], sys.argv[2].rstrip('/') + '/', sys.argv[3].rstrip('/') + '/'
rows = [json.loads(l) for l in open(joined, encoding='utf-8')]


def strict(r):
    c = r['cs_item']
    return (r['go']['item'] in ('if', 'switch', 'typeswitch') and not c['hits'] and not c['lambdaInBlock']
            and not ((c['labelInSpace'] or c['labeledParent']) and c['lambdaInBlock']))


by_file = collections.defaultdict(list)
for r in rows:
    if strict(r) and r['targets'] != ['golden']:
        by_file[r['cs']].append((r['cs_item']['line'], r['cs_item']['endLine']))
done = skipped = 0
for f, spans in by_file.items():
    dst = dst_root + f[len(src_root):]
    raw = open(dst, 'rb').read()
    bom = raw.startswith(b'\xef\xbb\xbf')
    text = raw.decode('utf-8-sig')
    lines = text.split('\n')
    for a, b in spans:
        if lines[a - 1].strip() == '{' and lines[b - 1].strip() == '}':
            for n in (a, b):
                lines[n - 1] = '\r' if lines[n - 1].endswith('\r') else ''
            done += 1
        else:
            skipped += 1
    out = '\n'.join(lines).encode('utf-8')
    open(dst, 'wb').write((b'\xef\xbb\xbf' if bom else b'') + out)
print(f'transformed {done} blocks in {len(by_file)} files; skipped (brace not alone on its line) {skipped}')
