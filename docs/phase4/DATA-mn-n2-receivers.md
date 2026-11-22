# N2, the box twin for by-ref receivers: the cut, measured (record)

**Read 2026-11-16 by G.** Design DESIGN-mn-box-twin.md (revision 1, section 9; the re-review's changes and PN-7 in
section 10, written before any arm was read). COORD's GO after the re-review. Branch `claude/g-mn-proto-b0`, the cut
on 3cd9db89a8. Closure: stage (c)'s MnHttpClosure (DATA-mn-stage-c.md), 186 packages, arm B's interface rule (every
member marked) in all three arms. GOMAXPROCS=12, 12 cores, 5 runs an arm.

## Arms

| arm | switches | build | twinned of 13,502 | await sites | box twins | named-plain | sync fwd | go.*.dll bytes |
|---|---|---|---|---|---|---|---|---|
| O | GoTwinsBoxRecv=false (the base) | 405 s | 2,999 (22.2%) | 7,167 | 0 | 0 | 658 | 82,780,672 |
| N | N2 as cut | 449 s | 3,806 (28.2%) | 9,495 | 634 | 24 | 502 | 83,961,344 (+1.43%) |
| NF | N2, GoTwinsBoxForwarders=false | 376 s | 3,806 (28.2%) | 9,495 | 634 | 24* | 646 | 83,945,984 (+1.41%) |

\* NF read 37 under the report before its last fix (below); the emission does not depend on it.

**Arm O is the base.** Its twinned, await-site and sync-forwarder counts, and its go.*.dll bytes, equal stage (c)'s
arm B exactly. Every run of every arm printed Go's 9 lines (15 of 15).

| arm | hand-offs (5 runs) | median | parks | peak carriers | peak OS threads | steps over 10 ms |
|---|---|---|---|---|---|---|
| O | 113 114 114 114 113 | 114 | 4-5 | 24 | 58-60 | 4-5 |
| N | 135 137 134 131 136 | 135 | 63-71 | 20 | 39-40 | 37-42 |
| NF | 134 131 133 131 133 | 133 | 64-69 | 20-22 | 38-39 | 32-37 |

Refused parkers, N: 175 (byref-param 63, unsafe 55, reflike 46, native 5, await-in-lock 3, host-not-partial 2,
ref-unrederivable 1); byref-recv 2,377 -> 2 (neither parks). The "blocked" count is a floor in every arm: a refused
function parks only through callees that twin.

Named-plain, N, 24: stack locals 19 (named-plain, as ruled: `sΔ1.sweep`, `a.GoString`, `lc.Listen` ...), an element
under a package variable 3 (the re-review's nit, `mheap_.central[i].mcentral`), a field of a value parameter 1, a
`.Value` chain over a ж value 1. 14 are inside twins, 10 in unrefused functions that park nowhere else.

## Scores (DESIGN-mn-box-twin.md 9.6 and 10)

- PN-1 MISS: four rules beyond the signature move, the call's box and the guard (DESIGN 10, "found by the first
  closure arms"), each with a GenTests unit red first.
- PN-2 HIT: twinned 3,806, +2.4% on the projected 3,718 (within 3%); refused parkers 175 (at most 180).
- PN-3 HIT: +1,180,672 bytes (+0.8 to +1.5 MB predicted).
- PN-4 MISS: hand-offs ROSE, median 114 -> 135 (at most 105 predicted). The counter is a goroutine ON A CARRIER
  blocking its thread (GoScheduler.EnterBlocked). N2 puts many more goroutines on carriers (parks 5 -> 65, steps
  25 -> 105), so their remaining plain blocks now count where they did not before; peak OS threads fell 59 -> 39.
  Steps over 10 ms rose 5 -> 40 with them (not read further here). The prediction's premise, that hand-offs measure
  held carriers alone, was wrong.
- PN-5 HIT: the 30-program twin sweep (stage (c)'s 29 and MnBoxAlias) 30/30 on the cut; plain emission unchanged (the
  base arm's bytes are identical, and no converter or non-twin generator file is in the change).
- PN-6 HIT: named-plain 24 (at most 25).
- PN-7 SPLIT: sync forwarders 658 -> 502 with the forwarders on (450-600 predicted: HIT), 646 off; hand-offs on 135,
  off 133 (on at most off predicted: MISS by 2, inside the arms' own spread, 131-137).

Stop lines: SN-1 none (no copy; every form bound); SN-2 none (CS9217 is the known ref-across-await family, cured);
SN-3 none; SN-4 not reached (no foreign element arm on the closure); SN-5 none; SN-6 GREEN with N2 and RED with the
copy control; SN-7 read 37 once, 12 and 1 of them inside REFUSED callers (fmt's `ss` methods and bidi's
`resolvePairedBrackets`, reflike), which the census never counted; the report now leaves them under their caller's
refusal, 24; SN-8 none; SN-9 green (581).

## The aliasing probe (SN-6)

MnBoxAlias (behavioral fixture): a pointer method parks while another goroutine writes through the same pointer, and
reads the write after the park, through a root pointer, a field (the field token), an element after an append (the
old backing array), an element of the own receiver's slice field, a value method through a pointer (the write NOT
seen, its own mutation NOT reaching `*p`), a nil receiver and a nil pointer's field (Go's panics). Go prints 9 lines,
stable over 3 runs. N2: equal. GoTwinsBoxCopy=true (each box twin copies its receiver): differs.

## COORD's rulings (dated block, 2026-11-16)

**SN-7: NOT FIRED**, on the census's definition: a refused caller never gets a twin, so its plain call sites are
outside the population N2 acts on. The three numbers, as the ruling requires:

- **37 raw**: arm N's report before its last fix.
- **13 under refused callers**: fmt's `ss.advance` 7 (`sstring format`), `ss.consume` 2 and `ss.peek` 2 (`sstring
  ok`), `ss.scanNumber` 1 (`sstring digits`), refused `reflike` (a ref-struct parameter); and bidi's
  `resolvePairedBrackets(ref isolatingRunSequence s)` 1, refused `byref-param` (the post to COORD said reflike for
  it: wrong; it is in the next step's population). None parks, so none has an R row in its report.
- **24 counted**: stack locals 19, the package-variable element 3, a field of a value parameter 1, a `.Value` chain
  over a ж value 1. The exclusion's GenTests unit (red first) is in the cut.

**PN-1 and PN-4 stand as scored.** Arm O was NOT the base until rule (2) of PN-1 landed (a method with a twin of its
own shape keeps the base form; the switch off knows no referenced box twin). Every arm-O number read before rule (2)
is void: the first chain's arm O read 2,998 twinned, 7,159 await sites, 125 box calls and 5 named-plain with the
switch off. The table above is the reading after it. For byref-param, the hand-off counter is split BEFORE any
prediction: carrier-mounted against non-carrier blocks, with peak OS threads as the headline.

## The code review and its re-gates (dated block, 2026-11-16)

COORD's Fable code review of 3cd9db89a8..adf81cf40f: APPROVE WITH CHANGES; the seven changes landed in cb38d22455,
each with a unit red first (DESIGN-mn-box-twin.md 10 and the commit say which). Re-gated from a per-run CLONE at
cb38d22455: the probe green with N2 and red with GoTwinsBoxCopy; the sweep 30/30; repoguard green; arm N 3,806
twinned, named-plain 24 (no `generic-unfollowed` row), Go 5/5; arm O 2,999 / 7,167 / 658 and 82,780,672 bytes,
stage (c)'s arm B byte for byte.

**Measurement rule (found here).** Every converted dll embeds its PDB path, so a tree whose path is longer than the
reference tree's grows dlls by 512-byte alignment units: the same arm O read 82,782,208 (+1,536 = 3 x 512) from
`g-run-rev`, 4 characters longer than `g-mn3`, and 82,780,672 from `g-rv3`, as long. Compare go.*.dll byte totals
only between trees whose paths are as long as each other, or strip the PDB path first. The split clone's +2,048
(`g-run-split`, 6 characters longer) is that effect, not the golib counter it was first credited to; the counter's
own size is UNREAD.

**PN-4, settled by the split counter** (56baaf2de4, 5 runs each): base O 110-115 carrier hand-offs + 81-86 thread
blocks (about 197 plain blocks), peak OS threads 57-61; N2 130-136 + 0 (about 133), threads 39-40. Every off-carrier
block moved onto a carrier and the total fell by a third. byref-param's headline is that total.
