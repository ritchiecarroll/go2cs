# The go1.26.10 LINUX annotation leg (PLAN-rebank-go126.md 5.1 and 13.3 step 2): rewrite each roster row's `linux:`
# segment from the hop's linux evidence TSVs.
# Usage: py -3 -I annotate_linux.py <worktree> [--evidence <dir>] [--write]
#   Without --write it prints the change list and changes nothing.
#   --evidence names the evidence directory, relative to <worktree> (or absolute); it defaults to the hop's,
#   docs/phase4/h10-evidence/go<pin>/linux. A re-bank at an unchanged pin passes its own (1.26.10.2:
#   docs/phase4/rebank-1.26.10.2/linux). The directory read is printed with the summary.
# The rule, the batch-41 leg's (61fe31d90a), extended to the later legs:
#   - the evidence is <evidence>/*.tsv (ledgers and .untracked excluded), read in DECLARED
#     WAVES by shard name: pass 1 (`P1-3`), L1 (`P2-L1`), L2 (`P1-L2`, `P1-L2-poll`), the final re-read (`P1-R`).
#     A row's reading of record is its LATEST wave, and within it its LAST line (crypto/x509's SSL_CERT_FILE-unset
#     re-run is the precedent);
#   - that reading banks only on `PASS`, and the segment is "<matched>[ + <disclosed-divergent>]", matched being the count
#     column where the TSV has one and `verdicts` where `matched` is pass 1's boolean;
#   - the TSVs' `disclosed` column is len(comparison.disclosed), the disclosure LIST, which the converter builds as the
#     disclosed-divergent set PLUS every host-fatal name in scope (src/go2cs/testConversion.go, hostFatalNames, appended
#     after the comparison: those tests ran on neither side and have no verdict). The roster's disclosed count is the
#     summary's "K disclosed-divergent" (Get-DisclosedCount), so the row's host-fatal entries in scope on linux are
#     subtracted (COORD 2026-10-26, R's finding: runtime 11091 + 76 is 11091 + 70);
#   - a row whose reading of record is NOT PASS keeps its segment and is listed, for its re-read or a ruling;
#   - a segment whose note reads linux-arm64 pending keeps the note; a segment carrying a linux-arm64 READING is refused
#     (that row is re-banked with --rids), and nothing is written.
#
# TWO-RID MODE (--rids; S7-a, COORD 2026-11-10/11, DATA-la64-s7-bank-plan.md 2.4). The linux bank reads every row on
# linux-x64 AND linux-arm64, in the mac evidence form (shard, row, RID, verdicts, matched, disclosed, divergent, new,
# status, agree, absent, divergent_names), one line per row per RID. Each row's segment is DERIVED, by the rules of
# src/check-roster-format.ps1 section 2i, rule for rule:
#   - the evidence directory defaults to the guard's $LinuxRidEvidenceDirectory, read in its declared waves
#     ($LinuxRidEvidenceWaves), a row's reading of record being its latest wave (a re-dispatch stem supersedes its
#     original, as for darwin). Empty means no evidence is declared: nothing is read;
#   - the roster's linux-x64 reading is never rewritten here. Where the same dispatch's linux-x64 line is not that
#     reading (status validated, new 0, the same matched and disclosed-divergent), it is a HOST DIFFERENCE (COORD
#     2026-11-10: recorded, not written) and linux-arm64 stays pending;
#   - EQUAL: both validated, agree yes on both, the same verdicts, matched and disclosed-divergent:
#     "N + d (linux-x64; linux-arm64: N + d)", both RIDs written out;
#   - ARCH-STRICT (Get-DarwinArchStrictReading), ARCH-COUNT (Get-ArchCountReading) and COMBINED (Get-ArchCombinedReading:
#     verdicts AND disclosed-divergent both differ, both deltas named in the `absent` and `divergent_names` columns):
#     the guard's functions, as arch_strict, arch_count and arch_combined below;
#   - anything else: "<linux-x64 reading> (linux-x64; linux-arm64: pending)", listed with its reason;
#   - a shape a guard REFUSES (a difference not wholly named) exits non-zero naming every such row, and writes nothing,
#     unless the guard's UNMEASURED ledger ($LinuxArm64LedgerPath) has a line with a reason for that row: then the row is
#     written pending and listed (the guard's own ledger checks decide whether the line is well formed);
#   - a numeric linux segment the evidence does not name is written pending (an unread row has no linux-arm64 reading),
#     its linux-x64 reading kept.
import argparse, csv, json, os, re, sys

ap = argparse.ArgumentParser(description='Rewrite the roster rows\' linux: segments from the linux evidence TSVs.')
ap.add_argument('tree')
ap.add_argument('--evidence', help='the evidence directory, relative to the worktree or absolute (default: the hop\'s)')
ap.add_argument('--rids', action='store_true', help='two-RID mode: derive linux-x64 / linux-arm64 segments from the mac-form evidence')
ap.add_argument('--write', action='store_true')
args = ap.parse_args()
tree, write = args.tree, args.write
props = open(os.path.join(tree, 'src/version.props'), encoding='utf-8').read()
pin = re.search(r'<GoStdLibVersion>([^<]+)</GoStdLibVersion>', props).group(1)
table = os.path.join(tree, 'docs/ValidatedTestPackages.md')
row_re = re.compile(r'^\| \[`([^`]+)`\]\(')
# A linux segment, its per-RID note included: groups 1-2 the headline, 3-4 the two RIDs, 5-6 the note's reading or 7
# `pending`.
LRID = r'(linux-x64|linux-arm64)'
SEG = re.compile(r' · linux: (?:(\d+)(?: \+ (\d+))?(?: \(' + LRID + '; ' + LRID + r': (?:(\d+)(?: \+ (\d+))?|(pending))\))?|n/a)(?= · | \|$)')


def fmt(m, d):
    return f'{m}' + (f' + {d}' if d else '')


def x64_reading(have):
    # The roster's linux-x64 reading in a numeric segment: the headline when it is linux-x64's (or there is no note),
    # else the note's; None when the segment names none.
    if have is None or have.group(1) is None:
        return None
    if have.group(3) is None or have.group(3) == 'linux-x64':
        return int(have.group(1)), int(have.group(2) or 0)
    if have.group(4) == 'linux-x64' and have.group(7) is None:
        return int(have.group(5)), int(have.group(6) or 0)
    return None


class Refusal(Exception):
    pass


def disclosed_divergent(r):
    try:
        return int(r['divergent']) - int(r['new'])
    except (KeyError, ValueError, TypeError):
        return int(r['disclosed'])


def arch_strict(a, x):
    # The guard's Get-DarwinArchStrictReading, rule for rule (annotate_darwin.py's arch_strict).
    if a['status'] not in ('validated', 'COUNT', 'DISC') or x['status'] not in ('validated', 'COUNT', 'DISC'):
        return None
    try:
        va, vx, na, nx = int(a['verdicts']), int(x['verdicts']), int(a['new']), int(x['new'])
        ma, mx = int(a['matched']), int(x['matched'])
        da, dx = int(a['divergent']) - na, int(x['divergent']) - nx
    except (KeyError, ValueError, TypeError):
        return None
    if na or nx or va != vx or ma + da != mx + dx or (ma == mx and da == dx):
        return None
    (rb, mb, db), (ro, mo, do) = ((a['RID'], ma, da), (x['RID'], mx, dx)) if da > dx else ((x['RID'], mx, dx), (a['RID'], ma, da))
    return f'{fmt(mb, db)} ({rb}; {ro}: {fmt(mo, do)})'


def names(r, column):
    if r.get(column) is None:
        return None
    return [n.strip() for n in r[column].split(';') if n.strip() and n.strip() != '-']


def arch_count(a, x):
    # The guard's Get-ArchCountReading, rule for rule (annotate_darwin.py's arch_count).
    if a['status'] not in ('validated', 'COUNT') or x['status'] not in ('validated', 'COUNT'):
        return None
    try:
        va, vx, ma, mx = int(a['verdicts']), int(x['verdicts']), int(a['matched']), int(x['matched'])
        ga, gx, na, nx = int(a['divergent']), int(x['divergent']), int(a['new']), int(x['new'])
    except (KeyError, ValueError, TypeError):
        return None
    if na or nx or va == vx or ga != gx or ma + ga != va or mx + gx != vx:
        return None
    long_, short = (a, x) if va > vx else (x, a)
    gap, short_names, long_names = abs(va - vx), names(short, 'absent') or [], names(long_, 'absent') or []
    if long_names:
        raise Refusal(f"{a['row']}: arch-count: the evidence names absent verdicts on {long_['RID']}, the LONGER RID")
    if len(set(short_names)) != gap or len(short_names) != gap:
        raise Refusal(f"{a['row']}: arch-count: its verdict counts differ by RID ({long_['RID']} {long_['verdicts']}, {short['RID']} {short['verdicts']}) "
                      f"with nothing divergent, but the evidence names {len(set(short_names))} of the {gap} missing verdict(s) on {short['RID']} (column absent)")
    return f"{fmt(long_['matched'], ga)} ({long_['RID']}; {short['RID']}: {fmt(short['matched'], ga)})"


def arch_combined(a, x):
    # The guard's Get-ArchCombinedReading, rule for rule: verdicts AND disclosed-divergent both differ, new 0, matched +
    # divergent = verdicts on each; banks the LONGER RID only with both deltas named in full.
    if a['status'] not in ('validated', 'COUNT', 'DISC') or x['status'] not in ('validated', 'COUNT', 'DISC'):
        return None
    try:
        va, vx, ma, mx = int(a['verdicts']), int(x['verdicts']), int(a['matched']), int(x['matched'])
        ga, gx, na, nx = int(a['divergent']), int(x['divergent']), int(a['new']), int(x['new'])
    except (KeyError, ValueError, TypeError):
        return None
    if na or nx or va == vx or ga == gx or ma + ga != va or mx + gx != vx:
        return None
    long_, short = (a, x) if va > vx else (x, a)
    counts = f"{long_['RID']} {long_['verdicts']} verdicts + {long_['divergent']} disclosed, {short['RID']} {short['verdicts']} + {short['divergent']}"
    head = f"{a['row']}: combined: its verdicts and disclosed-divergent both differ by RID ({counts}), but the evidence names"
    gap, short_absent, long_absent = abs(va - vx), names(short, 'absent') or [], names(long_, 'absent') or []
    if short.get('absent') is None:
        raise Refusal(f"{a['row']}: combined: its verdicts and disclosed-divergent both differ by RID ({counts}), but the evidence for "
                      f"{short['RID']}, the shorter RID, has no absent column, so its {gap} missing verdict(s) are not named")
    if long_absent:
        raise Refusal(f"{head} absent verdicts on {long_['RID']}, the LONGER RID")
    if len(set(short_absent)) != gap or len(short_absent) != gap:
        raise Refusal(f"{head} {len(set(short_absent))} of the {gap} missing verdict(s) on {short['RID']} (column absent)")
    for side in (short, long_):
        listed = names(side, 'divergent_names')
        want = int(side['divergent']) - int(side['new'])
        if listed is None:
            raise Refusal(f"{head} no disclosed-divergent verdicts on {side['RID']} (column divergent_names)")
        if len(set(listed)) != want or len(listed) != want:
            raise Refusal(f"{head} {len(set(listed))} of the {want} disclosed-divergent verdict(s) on {side['RID']} (column divergent_names)")
    both = [n for n in names(short, 'divergent_names') if n in short_absent]
    if both:
        raise Refusal(f"{head} {', '.join(both)} as both absent and disclosed on {short['RID']}")
    return f"{fmt(long_['matched'], int(long_['divergent']))} ({long_['RID']}; {short['RID']}: {fmt(short['matched'], int(short['divergent']))})"


def latest(records):
    # The guard's Select-LatestDarwinWave: the latest declared wave; a re-dispatch (its stem extends the original's)
    # supersedes the original within it.
    top = max(w for w, _, _ in records)
    in_wave = [(s, r) for w, s, r in records if w == top]
    stems = sorted({s for s, _ in in_wave})
    if len(stems) > 1 and all(stems[k].startswith(stems[k - 1]) for k in range(1, len(stems))):
        in_wave = [(s, r) for s, r in in_wave if s == stems[-1]]
    return [r for _, r in in_wave]


def pair_reading(pkg, records, roster):
    # Get-LinuxPairReading: (form, segment text or None, reason). Raises Refusal for a shape a guard refuses.
    rs = latest(records)
    x64 = [r for r in rs if r['RID'] == 'linux-x64']
    arm = [r for r in rs if r['RID'] == 'linux-arm64']
    if len(x64) != 1 or len(arm) != 1:
        raise Refusal(f'{pkg}: the linux evidence holds {len(x64)} linux-x64 and {len(arm)} linux-arm64 line(s) for it, not one of each')
    if roster is None:
        raise Refusal(f'{pkg}: the linux evidence names it, but it carries no numeric linux: segment with a linux-x64 reading')
    x, a = x64[0], arm[0]
    # Every count cell the forms read must be a number: a malformed line is refused by name, never a crash.
    for line in (x, a):
        for column in ('verdicts', 'matched', 'divergent', 'new'):
            if not re.fullmatch(r'\d+', line.get(column) or ''):
                raise Refusal(f"{pkg}: the linux evidence's {line['RID']} line has {column} {line.get(column)!r}, not a number")
    xd, ad = disclosed_divergent(x), disclosed_divergent(a)
    if x['status'] != 'validated' or x.get('new') != '0' or int(x['matched']) != roster[0] or xd != roster[1]:
        return 'host-difference', None, (f"host difference: the same dispatch's linux-x64 line ({x['status']} {fmt(x['matched'], xd)}, new {x.get('new')}) "
                                         f"is not the roster's linux-x64 reading ({fmt(*roster)}), recorded and not written")
    if (x['status'] == a['status'] == 'validated' and x['agree'] == a['agree'] == 'yes' and a['new'] == '0' and x['verdicts'] == a['verdicts']
            and int(x['matched']) == int(a['matched']) and xd == ad):
        one = fmt(x['matched'], xd)
        return 'equal', f'{one} (linux-x64; linux-arm64: {one})', None
    strict = arch_strict(x, a)
    if strict is not None:
        return 'arch-strict', strict, None
    for form, fn in (('arch-count', arch_count), ('combined', arch_combined)):
        text = fn(x, a)
        if text is not None:
            return form, text, None
    return 'pending', None, (f"no bank form fits (linux-x64 {x['status']} {x['verdicts']} verdicts {fmt(x['matched'], xd)}; "
                             f"linux-arm64 {a['status']} {a['verdicts']} verdicts {fmt(a['matched'], ad)}, new {a.get('new')}, agree {x['agree']}/{a['agree']})")


if args.rids:
    guard = open(os.path.join(tree, 'src/check-roster-format.ps1'), encoding='utf-8').read()
    declared = re.search(r"^\$LinuxRidEvidenceDirectory = '([^']*)'", guard, re.M)
    waves_line = re.search(r'^\$LinuxRidEvidenceWaves = @\(([^)]*)\)', guard, re.M)
    if not declared or not waves_line:
        sys.exit('the guard declares no $LinuxRidEvidenceDirectory / $LinuxRidEvidenceWaves: --rids derives by the guard\'s section 2i and will not run against a guard that lacks it')
    waves = re.findall(r"'([^']*)'", waves_line.group(1))
    ledger_line = re.search(r"^\$LinuxArm64LedgerPath = '([^']*)'", guard, re.M)
    if not ledger_line:
        sys.exit('the guard declares no $LinuxArm64LedgerPath: --rids accepts a refused row only through the guard\'s ledger')
    ledgered = set()
    ledger_file = os.path.join(tree, ledger_line.group(1))
    if os.path.exists(ledger_file):
        for r in csv.DictReader(open(ledger_file, encoding='utf-8'), delimiter='\t'):
            if (r.get('reason') or '').strip():
                ledgered.add(r['row'])
    evrel = args.evidence or declared.group(1).replace('{pin}', pin)
    by_row = {}
    if evrel:
        evdir = os.path.join(tree, evrel)
        if not os.path.isdir(evdir):
            sys.exit(f'evidence directory {evrel} does not exist')
        for d, _, files in os.walk(evdir):
            rel = os.path.relpath(d, evdir).replace(os.sep, '/')
            rel = '' if rel == '.' else rel
            if rel not in waves:
                sys.exit(f'evidence subdirectory {rel!r} is not a declared wave {waves}')
            for f in sorted(files):
                if f.endswith('.tsv'):
                    for r in csv.DictReader(open(os.path.join(d, f), encoding='utf-8'), delimiter='\t'):
                        by_row.setdefault(r['row'], []).append((waves.index(rel), f[:-4], r))
    lines = open(table, encoding='utf-8', newline='').read().split('\n')
    changes, held, refused = [], [], []
    for i, line in enumerate(lines):
        m = row_re.match(line)
        if not m:
            continue
        pkg = m.group(1)
        have = SEG.search(line)
        roster = x64_reading(have)
        if pkg in by_row:
            try:
                form, text, reason = pair_reading(pkg, by_row[pkg], roster)
            except Refusal as refusal:
                if pkg not in ledgered or roster is None:
                    refused.append(str(refusal))
                    continue
                form, text, reason = 'refused', None, f'UNMEASURED (ledgered): {refusal}'
        elif roster is not None:
            form, text, reason = 'unread', None, 'not read on linux-arm64'
        else:
            continue
        want = text if text is not None else f'{fmt(*roster)} (linux-x64; linux-arm64: pending)'
        if text is None:
            held.append((pkg, reason))
        new = line[:have.start()] + f' · linux: {want}' + line[have.end():]
        if new != line:
            changes.append((pkg, have.group(0).strip(' ·'), f'linux: {want}', form))
            lines[i] = new
    if refused:
        sys.exit('REFUSED, nothing written:\n  ' + '\n  '.join(refused))
    for pkg, old, new, form in changes:
        print(f'{pkg:40} {old:40} -> {new} ({form})')
    for pkg, reason in held:
        if reason != 'not read on linux-arm64':
            print(f'{pkg:40} PENDING: {reason}')
    print(f'{len(changes)} row(s) change, {len(held)} pending; {len(by_row)} rows named by the evidence in ' +
          (evrel if evrel else 'no declared directory (no linux evidence declared)'), file=sys.stderr)
    if write:
        open(table, 'w', encoding='utf-8', newline='').write('\n'.join(lines))
    sys.exit(0)

evrel = args.evidence or f'docs/phase4/h10-evidence/go{pin}/linux'
evdir = os.path.join(tree, evrel)
if not os.path.isdir(evdir):
    sys.exit(f'evidence directory {evrel} does not exist')

WAVES = [('pass1', re.compile(r'^[A-Za-z0-9]+-\d+$')), ('L1', re.compile(r'^[A-Za-z0-9]+-L1$')),
         ('L2', re.compile(r'^[A-Za-z0-9]+-L2(-[a-z0-9]+)?$')), ('reread', re.compile(r'^[A-Za-z0-9]+-R$'))]

by_row = {}
for f in sorted(os.listdir(evdir)):
    if not f.endswith('.tsv') or '.ledger.' in f or '.untracked.' in f:
        continue
    stem = f[:-4]
    wave = [i for i, (_, rx) in enumerate(WAVES) if rx.match(stem)]
    if len(wave) != 1:
        sys.exit(f'{f}: matches {len(wave)} declared waves, not one')
    for n, r in enumerate(csv.DictReader(open(os.path.join(evdir, f), encoding='utf-8'), delimiter='\t')):
        by_row.setdefault(r['row'], []).append(((wave[0], f, n), r))

def host_fatal_in_scope(pkg, goos='linux'):
    path = os.path.join(tree, 'src/core', pkg, 'go2cs_test_disclosures.json')
    if not os.path.exists(path):
        return 0
    entries = json.load(open(path, encoding='utf-8')).get('disclosures', [])
    return sum(1 for e in entries if e.get('class') == 'host-fatal' and (not e.get('platforms') or goos in e['platforms']))

def reading(records):
    (_, f, _), r = max(records, key=lambda t: t[0])
    if r['word'] != 'PASS':
        return None, f'{r["word"]} in {f}'
    matched = r['matched'] if r['matched'].isdigit() else r['verdicts']
    divergent = int(r['disclosed']) - host_fatal_in_scope(r['row'])
    assert divergent >= 0, f"{r['row']}: disclosed {r['disclosed']} is less than its host-fatal entries in scope"
    return matched + (f' + {divergent}' if divergent else ''), f

lines = open(table, encoding='utf-8', newline='').read().split('\n')
changes, held = [], []
for i, line in enumerate(lines):
    m = row_re.match(line)
    if not m or m.group(1) not in by_row:
        continue
    pkg = m.group(1)
    want, src = reading(by_row[pkg])
    have = SEG.search(line)
    if want is None:
        held.append((pkg, have.group(0).strip(' ·') if have else '-', src))
        continue
    seg = f' · linux: {want}'
    if have and have.group(3) is not None:
        if have.group(7) is None:
            sys.exit(f'{pkg}: its {have.group(0).strip(" ·")} carries a linux-arm64 reading; re-bank this row with --rids (nothing written)')
        seg += ' (linux-x64; linux-arm64: pending)'
    if have:
        new = line[:have.start()] + seg + line[have.end():]
    else:
        proof = line.rfind(' · [proof](')
        assert proof > 0, f'{pkg}: no proof link to insert before'
        new = line[:proof] + seg + line[proof:]
    if new != line:
        changes.append((pkg, have.group(0).strip(' ·') if have else '-', f'linux: {want}', src))
        lines[i] = new

for pkg, old, new, src in changes:
    print(f'{pkg:40} {old:22} -> {new:22} ({src})')
for pkg, old, src in held:
    print(f'{pkg:40} {old:22} HELD: reading of record is {src}')
print(f'{len(changes)} row(s) change, {len(held)} held; {len(by_row)} rows named by the evidence in {evrel}', file=sys.stderr)
if write:
    open(table, 'w', encoding='utf-8', newline='').write('\n'.join(lines))
