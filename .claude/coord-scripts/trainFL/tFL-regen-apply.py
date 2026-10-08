# tFL-regen-apply.py -- TRAIN Q fixup step 4: copy the union converter's own emission over the corpus files the
# assembled union does not carry. DERIVED 2026-10-06 from trainP/tP-regen-apply.py with ONE change, rule 4's evidence
# (trainQ/tQ-CHANGES.md Q10, below P's note); P's was DERIVED 2026-10-04 from trainO/tO-regen-apply.py with its own
# change to rule 4 (trainP/tP-CHANGES.md P10); O's from trainN/, from trainM/tM-regen-apply.py (NEW in M; TRAIN K's
# tK-k6check.py did this for two named files; this takes its file list from the emission check instead of from a name).
# Q's rule 4 was RUN by its author on synthetic trees only (tFL-controls.sh, arm RG): no conversion.
# Q (Q10): WHO EMITS A PATH, WITHOUT A WRITTEN LIST. P's rule 4 named 'the targets that emit this path' from the two
# arms' written lists, and stated its lifetime: it held while the BASE arm's converter wrote every source. At Q the base
# is P's landed converter, which writes incrementally too, so NEITHER list names the targets that emit an unchanged
# path. The i9's ruling (COORD's notes, 2026-10-04 20:38): no always-write flag; the set a conversion produced comes
# from the converter's own output list. The converter at TRAIN P's landed line writes NO such list (read with git: no
# flag, no manifest, and a -stdlib log names packages, not files), so the evidence is a ladder, most direct first:
#   (a) emitted-U-<os>.txt in the scratch, when all three exist: one repo-relative path a line, the converter's own
#       output list for that target, from whatever provides one (a future converter flag; a list COORD derives). When
#       present it is the ONLY evidence read.
#   (b) the path sits under a GOOS folder (a path component windows, linux or darwin): layout L3 writes such a file
#       from that target's run only (rule 4's own words since M: 'a GOOS-foldered file is written by one target only').
#   (c) a FLAT path: every target whose kept root holds the file is a peer. A peer whose root holds the new bytes
#       agrees. A peer whose root holds anything else is REFUSED: either that target emits other bytes (an L3 split is
#       owed) or it does not emit the path at all, and without (a) nothing here can tell which. Fail closed, by name.
# At Q's table every footprint is committed in-seat (75 tokens, two rows), so the emission check is EXPECTED to list
# nothing and this file to print 'listed=0': the ladder is reached only if a seat x seat interaction moves emission.
# O (trainO/tO-CHANGES.md O9): a per-file .editorconfig is ordinary emission now, listed by the emission check like any file and
# admitted by the composed REGEN_ALLOW (p2-converter-warning-clears' footprint). It is neither a sibling nor hand-owned,
# so every rule below applies unchanged; rule 4 is the one that bites it: the three targets share the file and each run
# rewrites only its own GOOS's facts, so a union whose facts for one GOOS moved writes three different files and the path
# is REFUSED (the fixup stops for COORD; never hand-merged). Review round 1: a committed .editorconfig a target's run
# DELETED (emitdrift.py's DELETED class) is absent from that target's kept root and is REFUSED by name (MS24).
# P (P10): RULE 4 UNDER INCREMENTAL WRITES. i9-incremental-cs-writes-r2 is a P row: the union converter writes a
# converted source only when its bytes differ from the file on disk, and the emission check converts one target per run
# (not the census path, which alone forces every write). Rule 4 asked 'did every target whose run WROTE the path emit
# the same bytes', reading written-U-<os>.txt as 'the targets that emit this path'. Under the i9's row a target whose
# emission EQUALS the union tree's file writes nothing, so it left that list: a flat file that only ONE target now
# emits differently would have passed rule 4 and been copied over the shared location (leg E would then read it
# union-attributable on the other two targets, after the fixup's commit). The base arm's lists restore the reading:
# at P the base arm is TRAIN O's converter, which still writes every source it emits, so written-M-<os>.txt (kept in
# the same scratch) is 'the targets that emit this path' for every path the base holds. A target in EITHER list is a
# peer; a peer whose kept root still holds the union tree's bytes is refused exactly as a peer that wrote different
# bytes was (the refusal names which case). A path the union ADDS is in no base list: for it the old reading stands
# (stated). LIFETIME: this holds while the BASE arm's converter writes every source; at the next train both arms carry
# the i9's row and the check needs the converter to write unconditionally (a tQ derive item, tFL-README.md section 7).
# Measured on synthetic trees at P's derive (trainP/tP-DERIVE-REPORT.md): the one-target case was refused, the
# three-target case and the per-GOOS case apply as before.
#
#   python tFL-regen-apply.py check|apply <worktree> <scratch> <out-list> <siblings 0|1> <allow ERE>
#
# <scratch> is tFL-emitcheck.sh's scratch after a KEEP_U_ROOTS=1 run at the ASSEMBLED union: it holds, per target
# (windows, linux, darwin), the seeded root U-<os> the union's converter wrote into, attr-<os>.txt (the union-attributable
# drift: written by the union arm, different from the union's own tree, and not standing drift of the base), sa-<os>
# (the *.cs.auto review siblings only the union arm rewrites), written-U-<os>.txt (everything that target's union-arm run
# wrote) and, since P, written-M-<os>.txt (everything that target's BASE-arm run wrote: rule 4).
#
# For every listed path the rules are checked for ALL paths before ONE byte is written:
#   1. it matches <allow> (the ruled set; a path outside it is a seat's uncommitted footprint nobody ruled on);
#   2. it exists in the worktree (a path the converter would ADD is a registration question, not a refresh);
#   3. it is not hand-owned (golib, the files directly in testing/, unsafe, *_impl.cs, a line-anchored
#      [module: GoManualConversion]);
#   3b. (verify round 1) the BASE arm does not drift on it too (dm-<os>, the base arm's drift list the check keeps in
#      the scratch): for that class the unit is the union's own HUNKS, and a whole-file copy would carry the base's
#      standing drift into a commit that says it carries none. Refused, for a hand-applied hunk;
#   3c. (verify round 1) the emitted bytes hold no unresolved deferred marker (a raced conversion: floor 1);
#   4. every target that EMITS the path (Q10's ladder above: an output list, the GOOS folder, or every kept root that
#      holds a flat path) holds the same bytes in its kept root, CR-stripped (a flat file that differs per target wants an
#      L3 split; a GOOS-foldered file is written by one target only);
#   5. the emission really differs from the worktree's file, CR-stripped (else there is nothing to apply).
# check = rules only. apply = rules, then the emitted bytes are copied VERBATIM (the converter's own line endings) and
# <out-list> gets one repo-relative path per line. Exit 1 on any refused path (nothing written), 2 on a usage or
# evidence problem. 0 listed paths is a valid reading: it prints so and writes an empty list.
import os, re, sys

TARGETS = ('windows', 'linux', 'darwin')
MANUAL = re.compile(rb'^\[module: (go\.)?GoManualConversion\]', re.M)
# Verify round 1. The hand-owned testing package is the files DIRECTLY in src/core/testing/; its sub-packages (fstest,
# iotest, quick, slogtest, internal/testdeps) are converted (TRAIN L's windows arm wrote 13 files there and not testing.cs).
HANDOWN_TESTING = re.compile(r'^src/core/testing/[^/]+$')
# The converter's two deferred-marker prefixes, as UTF-8 bytes (dynamicTypeOperations.go:25, adapterNameCollisions.go:41):
# a raced conversion exits 0 and leaves one file holding an unresolved one (floor 1's signature).
MARKERS = (b'\xc2\xabDYNTYPE:', b'\xc2\xabADAPTER:')   # the left guillemet as its two UTF-8 bytes (this file stays ASCII)

def readlist(p):
    if not os.path.exists(p): return None
    return [l.split(' ', 1)[0].strip() for l in open(p, encoding='utf-8', errors='surrogateescape') if l.strip()]

def nocr(p):
    with open(p, 'rb') as f: return f.read().replace(b'\r', b'')

GOOS_SEG = re.compile(r'(?:^|/)(windows|linux|darwin)(?:/|$)')

def emitters(p, listed, emitted, written, scratch):
    # Q (Q10): -> (the targets that EMIT path p, how that was read). See the header's ladder (a) (b) (c).
    if all(emitted[o] is not None for o in TARGETS):
        return [o for o in TARGETS if p in emitted[o] or o in listed], 'output-list'
    m = GOOS_SEG.search(p)
    if m:
        return sorted(set([m.group(1)]) | set(listed), key=TARGETS.index), 'goos-folder'
    return [o for o in TARGETS if os.path.isfile(os.path.join(scratch, f'U-{o}', *p.split('/'))) or o in listed], 'flat'

def main(argv):
    if len(argv) != 6 or argv[0] not in ('check', 'apply'):
        print('REGEN usage: check|apply <worktree> <scratch> <out-list> <siblings 0|1> <allow ERE>'); return 2
    mode, wt, scratch, outp, sib, allow = argv
    allow_rx = re.compile(allow)
    where, written, base_drift, emitted = {}, {}, {}, {}
    for os_ in TARGETS:
        a = readlist(os.path.join(scratch, f'attr-{os_}.txt'))
        w = readlist(os.path.join(scratch, f'written-U-{os_}.txt'))
        dm = readlist(os.path.join(scratch, f'dm-{os_}'))
        if a is None or w is None or dm is None or not os.path.isdir(os.path.join(scratch, f'U-{os_}')):
            print(f'REGEN evidence incomplete for {os_} under {scratch} (attr list, written list, the base arm\'s drift list dm-{os_} or the kept root is missing)'); return 2
        s = (readlist(os.path.join(scratch, f'sa-{os_}')) or []) if sib == '1' else []
        written[os_] = set(w)
        base_drift[os_] = set(dm)
        e = readlist(os.path.join(scratch, f'emitted-U-{os_}.txt'))   # Q (Q10): the converter's own output list, when one is given
        emitted[os_] = set(e) if e is not None else None
        for p in a + s: where.setdefault(p, []).append(os_)
    print('REGEN emitters read from: ' + ('the converter output lists emitted-U-<os>.txt' if all(emitted[o] is not None for o in TARGETS)
                                          else 'no output list in the scratch: a GOOS-foldered path from its folder, a flat path from every kept root that holds it (a disagreeing root is refused)'))
    plan, bad = [], []
    for p in sorted(where):
        issibling = p.endswith('.cs.auto')
        src = os.path.join(scratch, f'U-{where[p][0]}', *p.split('/'))
        dst = os.path.join(wt, *p.split('/'))
        why = None
        if not allow_rx.search(p): why = 'outside the allowed set'
        elif not os.path.isfile(dst): why = 'absent from the worktree (a NEW emitted path is a registration question)'
        elif not os.path.isfile(src):
            # Review round 1 (F1-2): emitdrift.py lists a committed .editorconfig the conversion DELETED (an empty per-file
            # warning model). The fixup never applies a deletion (floor 8: step 7 dies on any tracked deletion), so it is
            # refused by name and routed: a row that commits the deletion (tFL-README.md MS24), or the owning seat re-cut.
            why = (f'the {where[p][0]} run DELETED this committed per-file warning-entries file (its model came out empty: '
                   f'warningEntries.go); the fixup never applies a deletion (floor 8): a row commits it (tFL-README.md MS24)'
                   if p.endswith('/.editorconfig') else f'listed for {where[p][0]} but absent from its kept root')
        elif not issibling and (p.startswith(('src/core/golib/', 'src/core/unsafe/')) or HANDOWN_TESTING.match(p) or p.endswith('_impl.cs')
                                or MANUAL.search(nocr(dst))): why = 'hand-owned: a conversion must not have written it'
        elif not issibling and any(p in base_drift[o] for o in where[p]):
            why = ('the base arm drifts on this file too (standing drift): a whole-file copy would carry the base\'s drift into the fixup; '
                   'apply the union\'s own hunks by hand, never the whole file (corpus-reconvert skill, section 3)')
        elif not issibling and any(m in nocr(src) for m in MARKERS):
            why = 'unresolved deferred marker in the emission (a raced or interrupted conversion, floor 1): re-run the emission check alone on the box'
        else:
            new = nocr(src)
            # A review sibling is not in any written list (emitdrift keeps *.cs.auto out of it): compare it across the
            # A review sibling is not in any written list (emitdrift keeps *.cs.auto out of it): compare it across the
            # targets that LIST it. Every other path: across every target that EMITS it (Q10: the emitters() ladder).
            peers, how = (where[p], 'sibling') if issibling else emitters(p, where[p], emitted, written, scratch)
            for o in peers:
                q = os.path.join(scratch, f'U-{o}', *p.split('/'))
                if not os.path.isfile(q) or nocr(q) != new:
                    if issibling or p in written[o]:
                        why = f'the {o} run wrote different bytes than the {where[p][0]} run (per-target emission of one path)'
                    elif how == 'output-list':
                        why = (f'the {o} run emits this path (the converter\'s output list, emitted-U-{o}.txt) and left the union tree\'s bytes in place, while the '
                               f'{where[p][0]} run emitted different bytes (per-target emission of one path: only some targets moved)')
                    else:
                        why = (f'the {o} root holds other bytes than the {where[p][0]} emission, and whether the {o} target EMITS this flat path is NOT KNOWN: at Q both arms write '
                               f'incrementally, so no written list says which targets emit a path. Refused (a flat file one target emits differently wants an L3 split): give the '
                               f'converter\'s output list as emitted-U-<os>.txt in the scratch, or apply by COORD\'s ruling')
                    break
                    break
            if why is None and nocr(dst) == new: why = 'the emission equals the worktree file (nothing to apply)'
        if why: bad.append((p, why))
        else: plan.append((p, src, dst, issibling))
    for p, why in bad: print(f'REGEN REFUSED {p}: {why}')
    nsib = sum(1 for x in plan if x[3])
    print(f'REGEN {mode}: listed={len(where)} applicable={len(plan)} (review siblings among them: {nsib}) refused={len(bad)} allow=/{allow}/ siblings={"on" if sib == "1" else "off"}')
    if bad: return 1
    for p, src, dst, issibling in plan:
        print(f'REGEN {"would apply" if mode == "check" else "applied"} {p} (targets: {",".join(where[p])}{"; review sibling" if issibling else ""})')
        if mode == 'apply':
            with open(src, 'rb') as f: data = f.read()
            with open(dst, 'wb') as f: f.write(data)
    if mode == 'apply':
        with open(outp, 'w', encoding='utf-8', newline='\n') as f: f.write(''.join(x[0] + '\n' for x in plan))
    return 0

if __name__ == '__main__':
    try: sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    except Exception: pass
    sys.exit(main(sys.argv[1:]))
