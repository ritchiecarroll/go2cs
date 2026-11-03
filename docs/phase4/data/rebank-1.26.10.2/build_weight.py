# Measures each linux row's BUILD WEIGHT for the re-pass rebalance (repass_cut.py --p2-to-rwsl): the number of distinct
# standard packages in the row's linux test binary (`go list -test`, the `<row>.test` entry's Deps, with the
# " [<row>.test]" test variants folded into their package). A -tests run converts and builds a project for every one
# of them, and that dependency output under src/core is what filled P2's disk at f6666d98a2 (the P2-1 ledger's two
# floor aborts), so the count is the weight. It is a proxy, stated: no ledger records per-row disk.
# Usage: py -3 -I build_weight.py <repo> <out.tsv>
#   `go` on PATH must answer go1.26.10; it runs in its own GOROOT/src as GOOS=linux GOARCH=amd64 CGO_ENABLED=0.
# The rows are legs.tsv's linux rows. A row go list cannot load is an error, never a skipped row.
import csv, os, subprocess, sys

repo, out = sys.argv[1], sys.argv[2]
env = dict(os.environ, GOOS='linux', GOARCH='amd64', CGO_ENABLED='0', GOTOOLCHAIN='local', GOFLAGS='')
env.pop('GOROOT', None)
ver = subprocess.run(['go', 'env', 'GOVERSION'], capture_output=True, check=True, env=env).stdout.decode().strip()
assert ver == 'go1.26.10', f'go answers {ver}, not go1.26.10'
goroot = subprocess.run(['go', 'env', 'GOROOT'], capture_output=True, check=True, env=env).stdout.decode().strip()

legs = os.path.join(repo, 'docs/phase4/data/rebank-1.26.10.2/legs.tsv')
rows = sorted({r['row'] for r in csv.DictReader(open(legs, encoding='utf-8', newline=''), delimiter='\t') if r['os'] == 'linux'})
res = subprocess.run(['go', 'list', '-e', '-test', '-f', '{{.ImportPath}}\t{{if .Error}}ERROR{{end}}\t{{join .Deps " "}}'] + rows,
                     capture_output=True, check=True, env=env, cwd=os.path.join(goroot, 'src'))
weight = {}
for line in res.stdout.decode('utf-8').splitlines():
    path, err, deps = line.split('\t')
    assert not err, f'go list cannot load {path}'
    if path.endswith('.test') and path[:-len('.test')] in rows:
        weight[path[:-len('.test')]] = len({d.split(' [', 1)[0] for d in deps.split()})
missing = [r for r in rows if r not in weight]
assert not missing, f'no test binary for {missing}'
with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('row\tbuild_weight\n')
    for r in rows:
        fh.write(f'{r}\t{weight[r]}\n')
print(f'{len(rows)} linux rows weighed with {ver} (GOOS=linux GOARCH=amd64 CGO_ENABLED=0)', file=sys.stderr)
