# -*- coding: utf-8 -*-
"""Apply the re-derived corrections to docs/phase4/TRACKER-100-percent.md.

Every edit is an EXACT-STRING replacement. Nothing is written unless every anchor
is found exactly once, so a tracker that has moved under you fails loudly instead
of half-applying.

  python coord-tracker-rederive-apply.py                      # dry run (default)
  python coord-tracker-rederive-apply.py --apply              # write the 8 stale fixes
  python coord-tracker-rederive-apply.py --apply --include-optional
  python coord-tracker-rederive-apply.py --tracker <path> ...

I/O is encoding='utf-8', newline='' on BOTH sides: the file is CRLF except for one
bare LF ending the Linux-parity paragraph, and that mixed ending is preserved
byte-for-byte. Do not re-save the file with a tool that normalizes line endings.

Companion report: coord-tracker-rederive.md (sections A/B/C, with the roster line
number behind every re-derived value).
"""

import argparse
import io
import os
import sys

DEFAULT_TRACKER = os.path.join(
    r"C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c",
    "docs", "phase4", "TRACKER-100-percent.md",
)

# (id, why, anchor, replacement) -- anchors quoted verbatim from the tracker at master e0dcdb4f5.
EDITS = [
    (
        "S1",
        "line 17: raw percentage -- the roster's naive denominator is 215 (roster L139), "
        "so 201/215 = 93.5%, not 201/216 = 93.1%.",
        u"raw 201/216 = 93.1%",
        u"raw 201/215 = 93.5%",
    ),
    (
        "S2",
        "line 17: reflect's headline distance predates TRAIN 2; the tracker's own re-derived "
        "figure is 41 (L18, L30).",
        u"reflect 115 \u2192 48 real mismatches",
        u"reflect 115 \u2192 41 real mismatches",
    ),
    (
        "S3",
        "line 21: implementable remainder is 209 - 201 = 8 straight off roster L141.",
        u"**Rows remaining (implementable)** | **7** (os/user BANKED",
        u"**Rows remaining (implementable)** | **8** (os/user BANKED",
    ),
    (
        "S4",
        "line 21: reflect's parenthetical distance, same root as S2.",
        u"\u2014 reflect (48: the re-mapped tail above), os (682/685",
        u"\u2014 reflect (41: the re-mapped tail above), os (682/685",
    ),
    (
        "S5",
        "line 21: the named list is one package short of the arithmetic -- "
        "crypto/internal/boring/bcache is unruled, stays inside the naive denominator "
        "(roster L458-460) and is not among the six excluded (roster L449-456), so it "
        "counts against the implementable 209.",
        u"runtime/pprof, runtime/trace, testing (Option 1 ruled, sequenced) |",
        u"runtime/pprof, runtime/trace, testing (Option 1 ruled, sequenced), "
        u"crypto/internal/boring/bcache (the eighth, and the one the named list kept losing: "
        u"unruled, so the roster keeps it INSIDE the naive denominator and it counts against "
        u"the implementable 209 until its own measurement rules it) |",
    ),
    (
        "S6",
        "line 21: runtime's -tests distance is 0 on master per the same table's L17 and L33; "
        "only this cell still says 1.",
        u"runtime (-tests at **1** \u2014 the lone CS8175, i9+G paired on the capture-coordination "
        u"fix; at ZERO the SEMANTIC BILL prints and Stage A closes)",
        u"runtime (-tests at **0** compile errors on master at `9a5462091` \u2014 the CS8175 "
        u"receiver-snapshot family landed; the SEMANTIC BILL prints at i9's next run, Stage A "
        u"closes with commit 3)",
    ),
    (
        "S7",
        "line 30: the distance table's re-measure sentence still names 45.",
        u"at that landing the 45 re-measures at master.",
        u"at that landing the 41 re-measures at master.",
    ),
    (
        "S8",
        "line 51: present-tense remainder inside the History note; 209 - 201 = 8 today, and the "
        "replacement keeps the opening-day figure so both readings survive.",
        u"The remaining distance is 17 named rows, none a mystery.",
        u"The remaining distance is 8 named rows, none a mystery \u2014 17 when this note was "
        u"written.",
    ),
]

# Not a stale number -- an omission the re-derivation exposed. Off by default.
# The seven package names are pure roster derivation; the per-row dispositions are
# tonight's rulings as relayed by the coordinator and are labelled as rulings.
EDITS_OPTIONAL = [
    (
        "O1",
        "line 37: name the seven applicable rows that carry no linux annotation "
        "(199 applicable - 192 annotated). Anchor stops before the line ending, which on "
        "this ONE line is a bare LF -- it survives untouched.",
        u"both green on the i7 at the merge result).",
        u"both green on the i7 at the merge result). The seven applicable rows still "
        u"unannotated, from the roster itself: internal/poll, net, net/http, os/exec, "
        u"runtime/debug, sync/atomic, syscall \u2014 os/exec and runtime/debug are ONE ruling "
        u"from banking (TestExtraFiles ruled host-limit \u2192 linux: 86 + 2; TestPanicOnFault "
        u"ruled per-OS runtime-capability, excluded via the execution config \u2192 linux: 4 + 5), "
        u"net/http is a HOST-CAPACITY finding and takes no annotation, and syscall is the "
        u"converter/layout arc (L3 routes the production emission per-GOOS and leaves the "
        u"-tests artifacts flat \u2014 G's).",
    ),
]


def out(s):
    """Print without dying on a cp1252 console (the tracker text is not ASCII)."""
    enc = getattr(sys.stdout, "encoding", None) or "ascii"
    sys.stdout.write(s.encode(enc, "backslashreplace").decode(enc, "replace") + "\n")


def snippet(s, width=100):
    s = s.replace("\r", "\\r").replace("\n", "\\n")
    return s if len(s) <= width else s[:width - 1] + "\u2026"


def main(argv=None):
    ap = argparse.ArgumentParser(description="Apply re-derived TRACKER-100-percent.md corrections.")
    ap.add_argument("--tracker", default=DEFAULT_TRACKER, help="path to TRACKER-100-percent.md")
    ap.add_argument("--apply", action="store_true", help="write the file (default is a dry run)")
    ap.add_argument("--dry-run", action="store_true", default=False,
                    help="explicitly request the default behaviour")
    ap.add_argument("--include-optional", action="store_true",
                    help="also apply the optional additions (O*)")
    args = ap.parse_args(argv)

    if args.apply and args.dry_run:
        out("REFUSED: --apply and --dry-run are contradictory.")
        return 2

    edits = list(EDITS) + (list(EDITS_OPTIONAL) if args.include_optional else [])

    if not os.path.isfile(args.tracker):
        out("ABORT: tracker not found: %s" % args.tracker)
        return 2

    with io.open(args.tracker, "r", encoding="utf-8", newline="") as f:
        original = f.read()

    out("tracker : %s" % args.tracker)
    out("bytes   : %d chars  (CRLF %d, bare LF %d)"
        % (len(original), original.count("\r\n"),
           original.count("\n") - original.count("\r\n")))
    out("edits   : %d%s" % (len(edits), "  (incl. optional)" if args.include_optional else ""))
    out("")

    # --- Gate 1: every anchor must occur EXACTLY once, checked before anything is changed.
    failures = []
    for eid, why, anchor, _repl in edits:
        n = original.count(anchor)
        if n != 1:
            failures.append((eid, n, anchor))
    if failures:
        out("ABORT: %d anchor(s) not found exactly once. NOTHING was written." % len(failures))
        for eid, n, anchor in failures:
            out("  %s: found %d occurrence(s) of anchor:" % (eid, n))
            out("      %s" % snippet(anchor))
        return 1

    # --- Gate 2: no anchor may be a substring of another edit's replacement (ordering hazard).
    for i, (eid_a, _w, anchor_a, _r) in enumerate(edits):
        for j, (eid_b, _w2, _a2, repl_b) in enumerate(edits):
            if i != j and anchor_a in repl_b:
                out("ABORT: %s's anchor appears inside %s's replacement; edits are order-dependent."
                    % (eid_a, eid_b))
                return 1

    # --- Apply in memory.
    updated = original
    for eid, why, anchor, repl in edits:
        before = updated
        updated = updated.replace(anchor, repl, 1)
        if updated == before:
            out("ABORT: %s made no change after passing the count gate (unexpected)." % eid)
            return 1
        out("%s  %s" % (eid, why))
        out("    -  %s" % snippet(anchor))
        out("    +  %s" % snippet(repl))
        out("")

    # --- Gate 3: no anchor may survive (guards a replacement that quietly re-introduced one).
    for eid, _w, anchor, repl in edits:
        if anchor != repl and anchor in updated:
            out("ABORT: %s's anchor still present after replacement. NOTHING was written." % eid)
            return 1

    delta = len(updated) - len(original)
    out("net change: %+d chars  (CRLF %d, bare LF %d after)"
        % (delta, updated.count("\r\n"), updated.count("\n") - updated.count("\r\n")))

    if updated.count("\r\n") != original.count("\r\n") or \
       (updated.count("\n") - updated.count("\r\n")) != (original.count("\n") - original.count("\r\n")):
        out("ABORT: line-ending profile changed; refusing to write.")
        return 1

    if not args.apply:
        out("")
        out("DRY RUN - nothing written. Re-run with --apply to write.")
        return 0

    with io.open(args.tracker, "w", encoding="utf-8", newline="") as f:
        f.write(updated)
    out("")
    out("WROTE %s" % args.tracker)
    return 0


if __name__ == "__main__":
    sys.exit(main())
