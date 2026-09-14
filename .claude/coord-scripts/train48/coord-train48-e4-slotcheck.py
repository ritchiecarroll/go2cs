#!/usr/bin/env python
# POST-CONDITION CHECKER for one union-resolved slot file (COORD ruling E4, 2026-09-13).
#   argv: base ours theirs resolved
#
# MEASURED FIRST, THEN ASSERTED.  The BOARD is append-only but it is NOT appended to at EOF: its last
# line is a SENTINEL ("keep this the FINAL line ... every append must land INSIDE the raw guard"), so
# every side INSERTS above it.  A naive `lines(resolved) == lines(ours) + lines(theirs) - lines(base)`
# is therefore wrong by construction -- it double-counts nothing, but it assumes a tail append.  What
# is asserted here is the structural identity instead, and any line the union COLLAPSED at the seam
# between the two insertions is named and required to be BLANK.
#
#   P1  the seat's diff base->theirs REMOVES and MODIFIES nothing (insertion only)
#   P2  resolved == base[:lead] + oursInsert + theirsInsert + baseTail, up to a deficit whose every
#       collapsed line is EMPTY (the union aligned a blank line shared by the two insertions)
#   P3  every line the seat INSERTED is present in resolved, VERBATIM and IN ORDER
#   P4  every line OURS holds is present in resolved, VERBATIM and IN ORDER
#   P5  zero conflict markers
import io
import sys
import difflib

BASE, OURS, THEIRS, RES = sys.argv[1:5]


def rd(p):
    with io.open(p, encoding='utf-8', newline='') as f:
        return f.read().splitlines()


b, o, t, r = rd(BASE), rd(OURS), rd(THEIRS), rd(RES)
bad = 0


def insert_of(x):
    lead = 0
    while lead < len(b) and lead < len(x) and x[lead] == b[lead]:
        lead += 1
    tail = 0
    while (tail < len(b) - lead and tail < len(x) - lead
           and x[len(x) - 1 - tail] == b[len(b) - 1 - tail]):
        tail += 1
    return lead, tail, x[lead:len(x) - tail]


# --- P1
removed = []
for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, b, t, autojunk=False).get_opcodes():
    if tag in ('delete', 'replace'):
        removed.extend(b[i1:i2])
lo, to, oins = insert_of(o)
lt, tt, tins = insert_of(t)
if removed:
    print('   FAIL P1 base->theirs REMOVES or MODIFIES %d existing ledger line(s) -- REFUSE this slot, '
          'a union merge may not decide that.  First: [%s]' % (len(removed), removed[0][:110]))
    bad = 1
else:
    print('   ok   P1 base->theirs is INSERTION ONLY :: removed=0 modified=0 :: seat inserts %d line(s) '
          'at offset %d, keeping the last %d base line(s) (the append-only SENTINEL) below it'
          % (len(tins), lt, tt))
    print('        ours inserts %d line(s) at offset %d, keeping the same %d base tail line(s)'
          % (len(oins), lo, to))

# --- P2
want = b[:min(lo, lt)] + oins + tins + b[len(b) - min(to, tt):]
if want == r:
    print('   ok   P2 resolved IS base[:%d] + oursInsert(%d) + theirsInsert(%d) + baseTail(%d) = %d lines '
          'EXACTLY -- nothing added, nothing dropped, merge order preserved'
          % (min(lo, lt), len(oins), len(tins), min(to, tt), len(want)))
else:
    coll = []
    extra = []
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, want, r, autojunk=False).get_opcodes():
        if tag in ('delete', 'replace'):
            coll.extend(want[i1:i2])
        if tag in ('insert', 'replace'):
            extra.extend(r[j1:j2])
    if extra:
        print('   FAIL P2 the resolved file carries %d line(s) NEITHER side wrote.  First: [%s]'
              % (len(extra), extra[0][:110]))
        bad = 1
    elif coll and all(x.strip() == '' for x in coll):
        print('   ok   P2 resolved = base[:%d] + oursInsert(%d) + theirsInsert(%d) + baseTail(%d) MINUS '
              '%d BLANK line(s) the union aligned at the seam between the two insertions :: '
              'expected %d, resolved %d.  Every collapsed line is EMPTY -- no ledger entry was merged, '
              'shortened or re-ordered.'
              % (min(lo, lt), len(oins), len(tins), min(to, tt), len(coll), len(want), len(r)))
    else:
        print('   FAIL P2 the union collapsed %d line(s) and at least one is NOT blank.  First: [%s]'
              % (len(coll), (coll[0][:110] if coll else '')))
        bad = 1
print('        line arithmetic :: base=%d ours=%d theirs=%d resolved=%d :: '
      'ours + seatInsert = %d + %d = %d' % (len(b), len(o), len(t), len(r), len(o), len(tins),
                                            len(o) + len(tins)))


def subseq(needles, hay, label):
    global bad
    it = iter(hay)
    miss = None
    for nl in needles:
        for h in it:
            if h == nl:
                break
        else:
            miss = nl
            break
    if miss is None:
        print('   ok   %s :: all %d line(s) present VERBATIM and IN ORDER' % (label, len(needles)))
    else:
        print('   FAIL %s :: line not found in order: [%s]' % (label, miss[:110]))
        bad = 1


subseq(tins, r, 'P3 the seat INSERTED lines survive')
subseq(o, r, 'P4 the union-so-far (ours) lines survive')

mk = [l for l in r if l.startswith('<<<<<<<') or l.startswith('=======') or l.startswith('>>>>>>>')]
if mk:
    print('   FAIL P5 the resolved file carries %d conflict-marker line(s)' % len(mk))
    bad = 1
else:
    print('   ok   P5 conflictMarkers=0')

sys.exit(bad)
