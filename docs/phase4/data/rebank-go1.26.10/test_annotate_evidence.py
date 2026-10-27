# Self-test of the annotators' evidence directory (COORD, 2026-10-27, PLAN-release-1-26-10-2.md, Rulings 1).
# Usage: py -3 -I test_annotate_evidence.py [--scripts <dir>] [--guard <check-roster-format.ps1>]
#   --scripts: where annotate_linux.py and annotate_darwin.py are (default: beside this file);
#   --guard: the roster guard annotate_darwin.py reads its waves, rulings and evidence directory from (default: this
#   tree's src/check-roster-format.ps1). Pointing both at an older commit's copies runs this test against them.
# A fixture worktree holds two readings of one row at one pin: the hop's (h10-evidence, 5) and a later re-bank's
# (rebank-<pin>.2, 6). The roster row carries the hop's 5. Each annotator must keep 5 by default and write 6 when the
# re-bank's directory is named (the linux one by --evidence; the darwin one by --evidence or by the guard's declared
# $DarwinEvidenceDirectory), and must refuse a named directory that does not exist. Exit code = the number of failed
# cases.
import argparse, os, re, shutil, subprocess, sys, tempfile

here = os.path.dirname(os.path.abspath(__file__))
ap = argparse.ArgumentParser()
ap.add_argument('--scripts', default=here)
ap.add_argument('--guard', default=os.path.join(here, '..', '..', '..', '..', 'src', 'check-roster-format.ps1'))
args = ap.parse_args()

PIN = '9.9.9'
OLD = f'docs/phase4/h10-evidence/go{PIN}'
NEW = f'docs/phase4/rebank-{PIN}.2'
ROW = '| [`f/pkg`](https://example.invalid/f/pkg) | 5 |  | A fixture row. · linux: 5 · darwin: 5 · [proof](p.md) |'
LINUX_HEADER = 'row\tword\tverdicts\tsweep_s\tfirst_in_list\trc\tdiverged\tplatform\ttree\twall_s\tpost_s\tmatched\tdisclosed'
DARWIN_HEADER = 'shard\trow\tRID\tverdicts\tmatched\tdisclosed\tdivergent\tnew\tstatus\tagree'


def write(path, text):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8', newline='') as f:
        f.write(text)


def fixture(root, guard_text):
    write(os.path.join(root, 'src/version.props'), f'<Project><PropertyGroup><GoStdLibVersion>{PIN}</GoStdLibVersion></PropertyGroup></Project>\n')
    write(os.path.join(root, 'src/check-roster-format.ps1'), guard_text)
    write(os.path.join(root, 'docs/ValidatedTestPackages.md'), '# Roster\n\n| a | b | c | d |\n|:--|--:|--:|:--|\n' + ROW + '\n')
    for base, shard, n in ((OLD, 'P1-1', 5), (NEW, 'RWSL-1', 6)):
        write(os.path.join(root, base, 'linux', f'{shard}.tsv'),
              LINUX_HEADER + '\n' + '\t'.join(['f/pkg', 'PASS', str(n), '1', 'no', '0', 'no', 'linux', 'x', '1', '0', str(n), '0']) + '\n')
        write(os.path.join(root, base, 'darwin', 'C1-1.tsv'), DARWIN_HEADER + '\n' + ''.join(
            '\t'.join(['C1-1', 'f/pkg', rid, str(n), str(n), '0', '0', '0', 'validated', 'yes']) + '\n' for rid in ('osx-arm64', 'osx-x64')))


def run(script, root, *extra):
    p = subprocess.run([sys.executable, '-I', os.path.join(args.scripts, script), root, *extra], capture_output=True, text=True)
    return p.returncode, p.stdout, p.stderr


failed = 0


def case(name, ok, detail):
    global failed
    print(f'  {"ok  " if ok else "FAIL"}  {name}' + ('' if ok else f' -- {detail}'))
    failed += 0 if ok else 1


guard_text = open(args.guard, encoding='utf-8', newline='').read()
declared_line = re.compile(r"^\$DarwinEvidenceDirectory = '[^']*'", re.M)
work = tempfile.mkdtemp(prefix='g2cs-annotate-evidence-')
try:
    root = os.path.join(work, 'tree')
    fixture(root, guard_text)

    rc, out, err = run('annotate_linux.py', root)
    case('linux: by default the hop\'s reading (5) is read, so nothing changes', rc == 0 and '-> linux:' not in out, f'rc {rc}, out {out!r}, err {err.strip()!r}')
    rc, out, err = run('annotate_linux.py', root, '--evidence', f'{NEW}/linux')
    case('linux: --evidence names the re-bank, and its newer reading (6) is written', rc == 0 and '-> linux: 6' in out, f'rc {rc}, out {out!r}, err {err.strip()!r}')
    case('linux: the directory read is printed', f'{NEW}/linux' in err, f'err {err.strip()!r}')
    rc, out, err = run('annotate_linux.py', root, '--evidence', f'{NEW}/nowhere')
    case('linux: a named directory that does not exist is refused', rc != 0 and 'does not exist' in err, f'rc {rc}, err {err.strip()!r}')

    rc, out, err = run('annotate_darwin.py', root)
    case('darwin: by default the hop\'s reading (5) is read, so nothing changes', rc == 0 and '-> darwin:' not in out, f'rc {rc}, out {out!r}, err {err.strip()!r}')
    rc, out, err = run('annotate_darwin.py', root, '--evidence', f'{NEW}/darwin')
    case('darwin: --evidence names the re-bank, and its newer reading (6) is written', rc == 0 and '-> darwin: 6' in out, f'rc {rc}, out {out!r}, err {err.strip()!r}')
    case('darwin: the directory read is printed', f'{NEW}/darwin' in err, f'err {err.strip()!r}')
    rc, out, err = run('annotate_darwin.py', root, '--evidence', f'{NEW}/nowhere')
    case('darwin: a named directory that does not exist is refused', rc != 0 and 'does not exist' in err, f'rc {rc}, err {err.strip()!r}')

    # The guard's declared directory is the darwin annotator's default, so the annotation and the guard read the same
    # evidence once the annotation seat points that line at the re-bank.
    if declared_line.search(guard_text):
        write(os.path.join(root, 'src/check-roster-format.ps1'), declared_line.sub(f"$DarwinEvidenceDirectory = 'docs/phase4/rebank-{{pin}}.2/darwin'", guard_text, count=1))
        rc, out, err = run('annotate_darwin.py', root)
        case('darwin: with the guard declaring the re-bank, the default reads it (6)', rc == 0 and '-> darwin: 6' in out, f'rc {rc}, out {out!r}, err {err.strip()!r}')
    else:
        case('darwin: with the guard declaring the re-bank, the default reads it (6)', False, f'{args.guard} declares no $DarwinEvidenceDirectory')
finally:
    shutil.rmtree(work, ignore_errors=True)

print(f'failed {failed}')
sys.exit(failed)
