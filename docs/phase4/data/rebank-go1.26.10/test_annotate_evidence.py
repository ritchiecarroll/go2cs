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
    # The defaults are tested against a guard that declares the HOP's directory, whatever this tree's guard declares
    # (an annotation seat points that line at its re-bank; the last case below tests that form explicitly).
    fixture(root, declared_line.sub("$DarwinEvidenceDirectory = 'docs/phase4/h10-evidence/go{pin}/darwin'", guard_text, count=1))

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

    # THE RULED-COUNT PATH (COORD, 2026-11-03; H's review). The re-bank's darwin reading is COUNT 6 on both RIDs, not
    # validated. It banks only through a $DarwinRuledCounts entry at exactly 6/0; a different ruled count, or no entry,
    # leaves the row unbanked, so its segment is removed. The guard's own table is used, with the fixture row added.
    write(os.path.join(root, NEW, 'darwin', 'C1-1.tsv'), DARWIN_HEADER + '\n' + ''.join(
        '\t'.join(['C1-1', 'f/pkg', rid, '6', '6', '0', '0', '0', 'COUNT', 'yes']) + '\n' for rid in ('osx-arm64', 'osx-x64')))
    ruled_open = re.compile(r'^\$DarwinRuledCounts = @\{(\r?\n)', re.M)   # LF in the blob, CRLF in a Windows checkout
    if ruled_open.search(guard_text):
        def ruled_guard(entry):
            hop = declared_line.sub("$DarwinEvidenceDirectory = 'docs/phase4/h10-evidence/go{pin}/darwin'", guard_text, count=1)
            return hop if entry is None else ruled_open.sub(lambda m: m.group(0) + f"    'f/pkg' = '{entry} -- fixture ruling'" + m.group(1), hop, count=1)
        for entry, want, name in (('6/0', '-> darwin: 6', 'darwin: a COUNT reading at its ruled count (6/0) is written (6)'),
                                  ('7/0', '-> -', 'darwin: a COUNT reading off its ruled count (7/0) does not bank; the segment is removed'),
                                  (None, '-> -', 'darwin: a COUNT reading with no ruled-count entry does not bank; the segment is removed')):
            write(os.path.join(root, 'src/check-roster-format.ps1'), ruled_guard(entry))
            rc, out, err = run('annotate_darwin.py', root, '--evidence', f'{NEW}/darwin')
            case(name, rc == 0 and want in out, f'rc {rc}, out {out!r}, err {err.strip()!r}')
    else:
        case('darwin: the ruled-count path', False, f'{args.guard} declares no $DarwinRuledCounts table')

    # THE ARCH-COUNT FORM (COORD ruling 2026-11-10, gcimporter; the guard's Get-ArchCountReading). The re-bank's darwin
    # reading is 6 of 6 on osx-x64 and 5 of 5 on osx-arm64, nothing divergent, and osx-arm64 NAMES the verdict it lacks in
    # its `absent` column: the row banks "6 (osx-x64; osx-arm64: 5)". A difference the column does not wholly name is
    # REFUSED: the annotator exits non-zero, naming the row, and writes nothing.
    write(os.path.join(root, 'src/check-roster-format.ps1'), declared_line.sub("$DarwinEvidenceDirectory = 'docs/phase4/h10-evidence/go{pin}/darwin'", guard_text, count=1))
    def arch_count_evidence(absent_arm, absent_x64):
        write(os.path.join(root, NEW, 'darwin', 'C1-1.tsv'), DARWIN_HEADER + '\tabsent\n' +
              '\t'.join(['C1-1', 'f/pkg', 'osx-arm64', '5', '5', '0', '0', '0', 'validated', 'no', absent_arm]) + '\n' +
              '\t'.join(['C1-1', 'f/pkg', 'osx-x64', '6', '6', '0', '0', '0', 'validated', 'no', absent_x64]) + '\n')
    arch_count_evidence('TestOnlyOnAmd64', '-')
    rc, out, err = run('annotate_darwin.py', root, '--evidence', f'{NEW}/darwin')
    case('darwin: an arch-count reading with its missing verdict named banks "6 (osx-x64; osx-arm64: 5)"', rc == 0 and '-> darwin: 6 (osx-x64; osx-arm64: 5)' in out, f'rc {rc}, out {out!r}, err {err.strip()!r}')
    for absent_arm, absent_x64, name in (('-', '-', 'names none of it'), ('TestOnlyOnAmd64;TestOther', '-', 'names more than the difference'),
                                         ('TestOnlyOnAmd64', 'TestZ', 'names a verdict on the longer RID')):
        arch_count_evidence(absent_arm, absent_x64)
        rc, out, err = run('annotate_darwin.py', root, '--evidence', f'{NEW}/darwin', '--write')
        roster = open(os.path.join(root, 'docs/ValidatedTestPackages.md'), encoding='utf-8').read()
        case(f'darwin: an arch-count difference whose evidence {name} is refused by name, and nothing is written', rc != 0 and 'arch-count' in err and 'f/pkg' in err and '· darwin: 5 ·' in roster,
             f'rc {rc}, out {out!r}, err {err.strip()!r}')

    # THE TWO-RID LINUX MODE (S7-a; COORD 2026-11-10/11; DATA-la64-s7-bank-plan.md 2.4). annotate_linux.py --rids reads
    # the mac-form evidence of both linux RIDs and DERIVES the row's segment by the guard's section 2i rules: equal (both
    # RIDs written out), arch-strict, arch-count, combined, else "(linux-x64; linux-arm64: pending)". A refused shape
    # exits non-zero naming the row and writes nothing. The roster's linux-x64 reading is never rewritten from the
    # evidence: a hosted linux-x64 line that differs is a host difference, and the row stays pending.
    RIDS = f'{NEW}/linux-rids'
    RIDS_HEADER = DARWIN_HEADER + '\tabsent\tdivergent_names'
    write(os.path.join(root, 'src/check-roster-format.ps1'), declared_line.sub("$DarwinEvidenceDirectory = 'docs/phase4/h10-evidence/go{pin}/darwin'", guard_text, count=1))
    def rids_line(rid, status, verdicts, matched, divergent, new='0', agree='no', absent='-', names=None):
        cells = ['W1-1', 'f/pkg', rid, str(verdicts), str(matched), str(divergent), str(divergent), str(new), status, agree, absent]
        return '\t'.join(cells + ([names] if names is not None else [])) + '\n'
    def rids_case(name, linux, lines, want=None, refused=None, extra=()):
        write(os.path.join(root, 'docs/ValidatedTestPackages.md'), '# Roster\n\n| a | b | c | d |\n|:--|--:|--:|:--|\n' +
              ROW.replace('· linux: 5 ·', f'· linux: {linux} ·') + '\n')
        write(os.path.join(root, RIDS, 'W1-1.tsv'), RIDS_HEADER + '\n' + ''.join(lines))
        rc, out, err = run('annotate_linux.py', root, '--rids', '--evidence', RIDS, '--write', *extra)
        roster = open(os.path.join(root, 'docs/ValidatedTestPackages.md'), encoding='utf-8').read()
        if refused is None:
            case(name, rc == 0 and f'· linux: {want} ·' in roster, f'rc {rc}, out {out!r}, err {err.strip()!r}, roster {roster.splitlines()[-1]!r}')
        else:
            case(name, rc != 0 and refused in err and 'f/pkg' in err and f'· linux: {linux} ·' in roster,
                 f'rc {rc}, out {out!r}, err {err.strip()!r}, roster {roster.splitlines()[-1]!r}')
        return out, err
    rids_case('linux --rids: equal readings write both RIDs out', '5',
              [rids_line('linux-x64', 'validated', 5, 5, 0, agree='yes'), rids_line('linux-arm64', 'validated', 5, 5, 0, agree='yes')], '5 (linux-x64; linux-arm64: 5)')
    rids_case('linux --rids: arch-strict headlines the RID carrying the disclosures', '11',
              [rids_line('linux-x64', 'validated', 11, 11, 0), rids_line('linux-arm64', 'DISC', 11, 8, 3)], '8 + 3 (linux-arm64; linux-x64: 11)')
    rids_case('linux --rids: arch-count with its missing verdict named', '6',
              [rids_line('linux-x64', 'validated', 6, 6, 0), rids_line('linux-arm64', 'validated', 5, 5, 0, absent='TestOnlyOnAmd64')], '6 (linux-x64; linux-arm64: 5)')
    rids_case('linux --rids: the combined form, both deltas named', '6 + 2',
              [rids_line('linux-x64', 'validated', 8, 6, 2, names='d1;d2'), rids_line('linux-arm64', 'DISC', 6, 5, 1, absent='a1;a2', names='d1')], '6 + 2 (linux-x64; linux-arm64: 5 + 1)')
    for name, lines, refused in (
            ('names 1 of the 2 missing verdicts', [rids_line('linux-x64', 'validated', 8, 6, 2, names='d1;d2'), rids_line('linux-arm64', 'DISC', 6, 5, 1, absent='a1', names='d1')], 'names 1 of the 2 missing'),
            ('has no divergent_names column', [rids_line('linux-x64', 'validated', 8, 6, 2), rids_line('linux-arm64', 'DISC', 6, 5, 1, absent='a1;a2')], 'names no disclosed-divergent verdicts'),
            ('names none of its disclosed verdict', [rids_line('linux-x64', 'validated', 8, 6, 2, names='d1;d2'), rids_line('linux-arm64', 'DISC', 6, 5, 1, absent='a1;a2', names='-')], 'names 0 of the 1 disclosed-divergent'),
            ('names a verdict as both absent and disclosed', [rids_line('linux-x64', 'validated', 8, 6, 2, names='d1;d2'), rids_line('linux-arm64', 'DISC', 6, 5, 1, absent='a1;d1', names='d1')], 'both absent and disclosed'),
            ('names absent verdicts on the longer RID', [rids_line('linux-x64', 'validated', 8, 6, 2, absent='z', names='d1;d2'), rids_line('linux-arm64', 'DISC', 6, 5, 1, absent='a1;a2', names='d1')], 'LONGER')):
        rids_case(f'linux --rids: a combined shape that {name} is refused by name, and nothing is written', '6 + 2', lines, refused=refused)
    rids_case('linux --rids: an arch-count shape naming too little is refused by name too', '6',
              [rids_line('linux-x64', 'validated', 6, 6, 0), rids_line('linux-arm64', 'validated', 4, 4, 0, absent='TestOnlyOnAmd64')], refused='names 1 of the 2')
    rids_case('linux --rids: no form fits, so the row is written pending', '5',
              [rids_line('linux-x64', 'validated', 5, 5, 0), rids_line('linux-arm64', 'FAIL', 5, 4, 1, new='1')], '5 (linux-x64; linux-arm64: pending)')
    out, err = rids_case('linux --rids: a hosted linux-x64 reading that differs is a host difference, and the row stays pending', '5',
                         [rids_line('linux-x64', 'COUNT', 4, 4, 0, agree='yes'), rids_line('linux-arm64', 'COUNT', 4, 4, 0, agree='yes')], '5 (linux-x64; linux-arm64: pending)')
    case('linux --rids: the host difference is listed by name', 'host difference' in out, f'out {out!r}')
    rids_case('linux --rids: a pending row read and banked loses its pending', '5 (linux-x64; linux-arm64: pending)',
              [rids_line('linux-x64', 'validated', 5, 5, 0, agree='yes'), rids_line('linux-arm64', 'validated', 5, 5, 0, agree='yes')], '5 (linux-x64; linux-arm64: 5)')
    rids_case('linux --rids: a row the evidence does not name is written pending', '5',
              [rids_line('linux-x64', 'validated', 5, 5, 0, agree='yes').replace('f/pkg', 'g/other')], '5 (linux-x64; linux-arm64: pending)')
    rids_case('linux --rids: a linux-arm64 reading no evidence names is put back to pending, the linux-x64 reading kept', '7 + 1 (linux-arm64; linux-x64: 9)',
              [rids_line('linux-x64', 'validated', 5, 5, 0, agree='yes').replace('f/pkg', 'g/other')], '9 (linux-x64; linux-arm64: pending)')
    # The guard's declared $LinuxRidEvidenceDirectory is the default, as $DarwinEvidenceDirectory is darwin's; while it is
    # empty, no evidence is read and every numeric linux segment is written pending (the S7-a mechanical commit).
    rids_line_eq = [rids_line('linux-x64', 'validated', 5, 5, 0, agree='yes'), rids_line('linux-arm64', 'validated', 5, 5, 0, agree='yes')]
    write(os.path.join(root, RIDS, 'W1-1.tsv'), RIDS_HEADER + '\n' + ''.join(rids_line_eq))
    linux_declared = re.compile(r"^\$LinuxRidEvidenceDirectory = '[^']*'", re.M)
    for declared, want, name in ((RIDS.replace(PIN, '{pin}'), '5 (linux-x64; linux-arm64: 5)', 'linux --rids: the guard\'s declared directory is the default'),
                                 ('', '5 (linux-x64; linux-arm64: pending)', 'linux --rids: with no directory declared, every numeric segment is written pending')):
        write(os.path.join(root, 'docs/ValidatedTestPackages.md'), '# Roster\n\n| a | b | c | d |\n|:--|--:|--:|:--|\n' + ROW + '\n')
        if linux_declared.search(guard_text):
            write(os.path.join(root, 'src/check-roster-format.ps1'), linux_declared.sub(f"$LinuxRidEvidenceDirectory = '{declared}'", guard_text, count=1))
            rc, out, err = run('annotate_linux.py', root, '--rids', '--write')
            roster = open(os.path.join(root, 'docs/ValidatedTestPackages.md'), encoding='utf-8').read()
            case(name, rc == 0 and f'· linux: {want} ·' in roster, f'rc {rc}, out {out!r}, err {err.strip()!r}')
        else:
            case(name, False, f'{args.guard} declares no $LinuxRidEvidenceDirectory')
    # Single-RID mode (an x64-only re-bank) keeps a pending note and refuses a two-RID reading.
    write(os.path.join(root, 'src/check-roster-format.ps1'), declared_line.sub("$DarwinEvidenceDirectory = 'docs/phase4/h10-evidence/go{pin}/darwin'", guard_text, count=1))
    for linux, want, name in (('5 (linux-x64; linux-arm64: pending)', '6 (linux-x64; linux-arm64: pending)', 'linux single-RID: a pending note is kept while linux-x64 is re-banked'),
                              ('5 (linux-x64; linux-arm64: 5)', None, 'linux single-RID: a two-RID reading is refused (re-bank it with --rids), nothing written')):
        write(os.path.join(root, 'docs/ValidatedTestPackages.md'), '# Roster\n\n| a | b | c | d |\n|:--|--:|--:|:--|\n' + ROW.replace('· linux: 5 ·', f'· linux: {linux} ·') + '\n')
        rc, out, err = run('annotate_linux.py', root, '--evidence', f'{NEW}/linux', '--write')
        roster = open(os.path.join(root, 'docs/ValidatedTestPackages.md'), encoding='utf-8').read()
        ok = (rc == 0 and f'· linux: {want} ·' in roster) if want else (rc != 0 and '--rids' in err and f'· linux: {linux} ·' in roster)
        case(name, ok, f'rc {rc}, out {out!r}, err {err.strip()!r}, roster {roster.splitlines()[-1]!r}')
finally:
    shutil.rmtree(work, ignore_errors=True)

print(f'failed {failed}')
sys.exit(failed)
