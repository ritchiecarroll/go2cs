---
paths:
  - "docs/**"
---

# Document authority, conventions and the security order

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 5463-5684.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens.

     PHASE 2 DISTILLATION, 2026-09-12. Every visible rule traces to the Phase 1 text; every date, SHA, measured number
     and named incident from that text is retained in the comment attached to the rule it justifies. Nothing was
     deleted, only moved. This file resisted the campaign's 5-10x target by its nature: almost all of it is the owner's
     standing SECURITY ORDER and the gate procedure implementing it, which is normative and stays visible. Only the
     incident narratives were moved. Visible characters fell 21,152 -> ~9,500.

     BATCH19 MERGE, 2026-09-12. The items routed here from origin/claude/coord-doctrine-batch19
     (commits 24bfc8304 / c5e17217b / e9e56b657, CLAUDE.md@44f858717 anchors 5507, 5519, 5604, 5662) were
     integrated: one new visible census rule (the line-break wrap and its join-class discriminator, which is
     normative for the security order), three amendments to existing rules (guard-fixture template class,
     foreign-name probe, allowlist fail-open), and their narratives into the comments beside them. -->

<!-- Integration note, batch19: none of the four items CONTRADICTED standing doctrine. The allowlist item
     (anchor 5662) narrows "Audit a widened gate ... controlled both ways" without reversing it, and its own
     author retracted the general "do not allowlist a security gate" form within the hour; the retracted broad
     form is recorded beside that rule so it is not re-derived. -->

## The security order
- ⚠ **No real machine names or other internal-infrastructure identifiers on ANY pushed surface** — standing owner order.
  Committed files, mailbox entries, commit messages, ref names and staged diffs name fleet machines ONLY by nickname:
  `R-LAPTOP`, `G-LAPTOP`, `i9`, `i7`/`coordinator`. Real hostnames, UNC paths carrying them, share names and non-public
  usernames stay off GitHub entirely. <!-- Owner order 2026-09-01; that scrub replaced every occurrence at both public tips, master and the mailbox branch. -->
- **Forbidden token classes, one census arm each**: both profile-root spellings, `/home/`, the doubled-separator network
  prefix, the account name, and the owner's GIVEN NAME and surname in ordinary prose. <!-- The name class is the one that gets
  missed. 2026-09-08: six of seven scrub hits were the owner's given name used as a form of address; a seventh was a network path
  whose host was ALREADY the prescribed nickname, firing only because the nicknames were absent from the guard's placeholder list.
  One lane's pre-post census had eleven path-and-host arms and none for names, and passed hundreds of posts only because its author
  never typed the name — it passed by luck, not by gate; its new arm was red-tested BEFORE the fix (a body naming the owner PASSED).
  A second lane's four path-and-account arms read CLEAN on the tree that held all six name hits: its "every post censused clean" was
  TRUE and measured far less than it sounded. A disclosure of one's own broken gate is what made the second lane look at its own; a
  clean report would have moved nobody. -->
- **Git HISTORY retains the pre-scrub originals, so never reintroduce one by quoting a pre-scrub record verbatim.**
  Re-census case-insensitively before banking any doc that copies old text. <!-- History retention is owner-accepted. The
  reintroduce-by-quoting mode is real: five of the seven hits found on the mailbox tree 2026-09-08 were in posts dated AT OR AFTER the scrub. -->
- **A scratch-directory transpile's emission is not postable**: it carries an ABSOLUTE source path in `GoPositionMap`
  and drops the hand-added `[GoTestMatchingConsoleOutput]`. Post from a repo-relative run or redact first, and never
  copy such a `package_info.cs` into the corpus. <!-- The committed file carries the relative main.go. Measured 2026-09-02: one
  was posted carrying a profile path plus worktree and session layout, and had to be scrubbed off the mailbox. -->
- **A post quotes the PATTERN it checked and never a value that matches one** — the pre-post grep covers the patterns
  you quote, not only your prose. Prove a toolchain pin with the bare `go version` line, which carries no path, and NAME
  a security grep's patterns rather than spelling them. "Spell GOROOT exactly as `go env GOROOT` prints it" governs the
  ARGUMENT, never the POST. <!-- 2026-09-02: a post describing its own census as <name>|<profile-root>|/home/ spelled the real
  account name onto the pushed surface. 2026-09-04: a dispatch repeating the GOROOT instruction invited a lane to quote its GOROOT
  verbatim onto the mailbox, profile root and account name included; scrubbed by a follow-up commit within minutes. -->
- **A code branch is a pushed surface too**: the commit message, the staged diff AND the ref name each take the census,
  exit-gated, and the posting tool HOLDS after announcing the SHA.
- **A surface with no gate on it is route #6 one layer over**, and a composition that cannot find its gate correctly
  REFUSES to push. **A lane does not scrub a shared transport branch piecemeal** — the remedy is the coordinator's, as
  one act at the tip, plus the gate placed where the branch can reach it. <!-- 2026-09-08: the mailbox branch forked BEFORE
  the fleet-identifier guard existed, so a safe-push composition could not find its test file there and refused — meaning every mailbox
  push to that date went out UNGATED. Master's guard run by hand against the mailbox tree read seven hits in one file. Scoping preceded
  routing: none from the reporting post; one the reporter's own, named; master GREEN on the same guard; and a clearance-liveness failure
  named as a SCOPE ARTIFACT of running master's guard on an older branch rather than counted. -->

## Running a census
- ⚠ **Diff-scoped and EXIT-GATED before EVERY push, or it is decoration** — a census whose exit code does not gate the
  push is a guard built and not armed. **Ordering is part of the property**: a census in the SAME command chain as the
  push lets the push run on whatever the census printed. <!-- Ruling 2026-09-04. 2026-09-07: one lane printed "census: 0" above
  eight entries in a session and for one pattern that 0 meant the instrument had ERRORED; another lane's census returned DIRTY / exit 1
  and the commit and push ran anyway. 2026-09-08, this file's own author: a coordinator composed its security census into the same chain
  as the push and the branch was on the remote before the number was read. It was clean, which is exactly why the shape survives — the
  log shows the gate running and reading, and the ORDERING makes its verdict inert. -->
- **DELETE a decoration census; never repair it** — a duplicate adds a way to be wrong and no way to be right, and the
  fix is the one-line wiring (`|| exit`), never a new census. What protects a push is ONE gate that EXITS non-zero on any
  hit: a bracketed case-insensitive alternation over the forbidden classes, one planted identifier per class.
- **A census takes TWO PASSES.** A path-anchored pattern cannot see an identifier used OUTSIDE a path BY CONSTRUCTION (a
  listing's owner column, an "account X" parenthetical, a machine name in a roster row), so a LITERAL pass over the
  identifiers the first pass surfaced is NOT redundant. Enumerate every hit outside the scrub scope with a per-line
  REASON into an allowlist the instrument consults; close the arithmetic in BOTH directions (substituted class up by
  exactly the substituted spans, allowlisted class unmoved); substitute the identifier ALONE in the tree's existing
  placeholder spelling; verify at the SAME LAYER — the working-tree form under the `eol=crlf` pin, never the LF blob;
  and REFUSE an empty file list. Durable form: a **standing census guard in the converter's own test suite**, allowlist
  and positive control inside it. <!-- The order landed 2026-09-01 and was BREACHED in pushed docs by 2026-09-04 with no gate that
  could see it; the outside-a-path case is the order's own headline clause. The same-layer verification was shown RED on one extra byte
  and made to refuse an empty file list — a verification that passed vacuously over an empty list had already happened once. -->
- ⚠ **A LINE-ANCHORED census cannot see a token WRAPPED ACROSS A LINE BREAK**, so append a whitespace-joined copy
  of the input to the buffer EVERY arm searches — structural, never per-arm, so an arm added tomorrow is covered —
  collapsing whitespace only ADJACENT TO THE BREAK, which closes the bare-newline, indented-continuation and
  trailing-space-at-the-break shapes at once without fusing arbitrary words. **The join class discriminates by
  CONTINUATION, never by LENGTH**: a leaked path CONTINUES past the account (the next byte is a separator) where
  fused prose ENDS at a space or EOL, and a minimum-segment-length rule fails in BOTH directions. Probe with
  realistic variants and STATE any residual shape the discriminator still misses. <!-- 2026-09-08, i9:
  of six planted shapes five fired and the SPLIT token was missed — an account name broken mid-token across the wrap, with a sibling
  lane's SHA wrapped the same way. The first fix, joining on a bare newline, was PARTIAL and its own re-probe caught it: an INDENTED
  continuation and a TRAILING SPACE at the break, the two commonest real shapes, were still missed. A red arm on ONE historical plant
  shape proves the census catches that shape and nothing else.
  2026-09-08, C2, the discriminator: length fails both ways (fused 9/10/16-character vocabulary against 4-character real accounts);
  continuation kept 8 of 8 wrap positions, freed 6 prose shapes, and left one residual STATED (a break exactly at the separator AND a
  path ending at an unknown account). Third instance of "a mechanism does not transfer by resemblance" — a sibling's redundancy was
  real on its own gate and absent on this two-arm one. Its red control was INVALID the first time, run against a tree not yet carrying
  the prose arms (0 red where 4 were required), and was fixed by asserting that the variant DIFFERS in the intended way AND that the
  arms it must fail are PRESENT; a duplicate arm that fired two kinds was DELETED rather than the attributability test loosened. -->
- **An unclassified total is a number, never a finding** — classify a count-only census before reading it. <!-- 2026-09-08:
  904 hits over a tree that legitimately carries a public URL on every page, every one the repository's own public URL, with the
  profile-path, home-prefix and network-prefix arms all at zero. A number nobody classified is exactly what a gate wired after the push
  cannot stop. -->
- **Census the mailbox at `origin/claude/mailbox` after a VERIFIED fetch.** <!-- 2026-09-02: an already-scrubbed line was
  re-reported from a stale copy. -->
- **Census the ref a push creates**: a rev-range census is structurally blind to it, and path-shaped patterns are NOT
  inert on ref-shaped input — git rejects a backslash in a ref name, but every separator class admits the forward slash
  and refs permit dots and underscores. The branch-name arm runs BEFORE the push, with a sentinel the instrument derives
  from the pattern file at runtime. <!-- 2026-09-08: 6 of 6 identifier patterns FIRED on ref-legal probes — a gate author reasoning
  "refs cannot hold paths" ships four inert arms believing them live. Load-bearing-ness proven by a NEUTER: exactly one arm red, its
  neighbours green, the restore byte-identical. -->
- **Two censuses answer different questions, and a clean reading from one does not certify the other**: an
  IDENTIFIER-keyed guard (hashed denylist plus clearance allowlist) and a SHAPE-keyed pre-post census disagree
  legitimately, and a tree gate and a delta gate are COMPLEMENTARY rather than duplicates. **Any document quoting
  captured `-json` output trips a doubled-separator detector on escaping alone.** Put the strict instrument where a
  false refusal costs its owner one rewrite — its own pushes. <!-- 2026-09-08: master's fleet guard read 0 on the scrubbed
  mailbox tree while a lane's shape-keyed census REFUSED with 18 — a quoted pattern-plus-ellipsis in prose, the profile ENVIRONMENT
  VARIABLE's name, bare generic home prefixes, and seven quotations of captured -json test output whose every separator is DOUBLED by
  JSON escaping and so is indistinguishable from a network prefix. The account-name class, the only real leak, read clean everywhere.
  The strict instrument earned its placement by refusing the very post that first quoted the three shapes literally. The red arm proving
  a tree gate covers a PENDING entry is a line appended and not committed. -->
- **Re-guard a scrub commit AFTER its rebase**, so the claim holds for the exact tree pushed.
- **To run master's guard against another branch's tree, place the guard's package at a NEW UNTRACKED path inside the
  clone** — its root resolver still finds the clone root. Never copy master's converter tree OVER the branch's: that
  substitutes master's content across hundreds of tracked files and changes the thing being measured. **Classify hits
  with the guard's OWN functions, never by eye.** <!-- 2026-09-08: classifying by eye gave "all false positives, stop" as the first
  conclusion, overturned by MEASURING the live post tool with four controls (it refuses that token, a planted network path and a planted
  profile path, and admits a clean entry), which made the operative policy DENY and the six lines pre-census residue. -->

## Building and controlling a security gate
- ⚠ **POSITIVE-CONTROL THE INSTRUMENT ON THE BOX THAT WILL RUN IT.** The security-census false clean has at least two
  doors on GNU grep 3.0 and both read as a clean ZERO, so the rule is NOT "avoid `-F`": a case-insensitive LITERAL
  match — exactly how the pre-post identifier census is specified — is one of the dead combinations, so the one
  instrument the owner's standing order rests on can read NOTHING and report clean. A lane gaining a LINUX host
  inherits the `/home/` exposure and re-controls that arm THERE; a Windows reading does not carry. <!-- The Linux clause: 2026-09-08, G — narrative beside the EXACTLY-ONE-ARM rule below. Measured independently on two lanes
  at the same grep version, 2026-09-07: on one, -i combined with -F returns EMPTY (-c -F gives 1, -ic gives 1, -ic -F gives nothing); on
  the other, a case-insensitive count whose pattern ENDS IN A BACKSLASH returns EMPTY with "Trailing backslash" while the same pattern
  BRACKETED returns 1. Neither door is portable: a lane that "avoided -F" on one box walks into the other. Joins grep -P, the UTF-16
  redirect, the LF anchor and the leading-slash path conversion. -->
- **Positive-control on a planted line of EACH class the pattern claims to detect — and prefer the PRE-SCRUB TREE to
  anything planted.** Discipline is not a gate. <!-- 2026-09-08: the fixed arm derives its tokens from the configured user name
  (split on non-letters, pieces under three characters dropped, so the script never carries what it forbids) and is controlled three
  ways: the real pre-scrub tree refuses with exactly the six, the post-scrub tree is clean, its own posts unchanged. -->
- **DERIVE forbidden tokens, never spell them** — the surname from the account name, the given name from the configured
  email's local part — and ABORT a derivation shorter than four characters rather than install a two-character detector.
  Store a denylist as HASHES, with each token's LENGTH in the SAME struct as its hash. <!-- A length kept in a second list can
  silently disagree and disarm the entry while every test passes (2026-09-04). Name derivation red/red/green controlled, 2026-09-08. -->
- **A control must be catchable by EXACTLY ONE ARM, or it cannot tell you that arm works** — each probe uses a token no
  other arm matches, verified end to end through the real script; a HOME-PATH arm probed with the REAL account name is
  caught by the owner-token arm too and proves nothing, so probe it with a FOREIGN name. **Plant the instrument's own
  sentinel into the input copy** so the control coincides with the subject on NO input. <!-- 2026-09-08, G: the real account
  name COINCIDED with the owner-token arm, so "all refused at 2 hits each" proved NOTHING about the path arm — per-arm isolation defeated
  by a probe whose token TWO arms can match. Re-probed with a FOREIGN name the arm stands alone (home path REFUSED, bare foreign name
  CLEAN, windows path REFUSED). Every lane gaining a Linux host inherits the exposure, so that arm is re-controlled there rather than
  carried from the Windows reading — visible as a clause on the box rule above. 2026-09-07: a lane ran SIX green controls over a gate whose
  user-path arm was COMPLETELY DEAD (a heredoc had collapsed its doubled backslashes into the literal drive-colon-Users text) because the
  probe meant to prove that arm ALSO carried the account name, and the account arm caught it. 2026-09-08, 99a1f5e: a census anchored its
  "the grep works" control on a token every mailbox post happens to carry — the lane's own name — so on a CODE COMMIT MESSAGE it ABORTED
  as unprovable. The abort was correct; proceeding past it was the defect. The sentinel form is proven by four arms: a clean commit-shaped
  file passes; the OLD census aborts on it; a planted network-share path refuses with the control at 1; a real post still passes. -->
- **A widening that must reach ONE arm travels as a per-arm parameter from the caller that knows the arm**, never
  through a list every arm shares. Prove an arm load-bearing by NEUTERING its mechanism, not by planting. <!-- 2026-09-08:
  the fleet nicknames had to be admitted as network-path HOST segments and NOT as profile segments, and the shared placeholder list both
  arms consult would have widened BOTH — so the network arm passes its own admit set and the profile arm passes none, with no kind-string
  comparison a literal could drift against. The red-before arm neutered the MECHANISM: 12 of 12 spellings fired, the only red arm, restore
  byte-identical. A denied token beside a nickname host is still caught; a nickname used as a profile segment is still refused. The security
  census over that diff was positive-controlled only after its FIRST planting silently failed on a capital-letter escape in a printf — the
  initial zero was a broken control, not a clean tree. -->
- **Never exempt a guard's own source** — it is a tracked file the guard scans and the one most likely to be edited by
  whoever adds the next entry. Assemble planted fixtures through `Sprintf` so the source reads as a placeholder while
  the runtime string is real-looking. A coarse battery census then reads those fixtures as HITS: exclude them as a
  TEMPLATE class keyed on the SEGMENT after the root and **never on the path** — so a fixture carrying a REAL segment
  still counts — PRINT the excluded lines beside the residual, and control the class both ways (fixture shapes
  excluded, a real segment and a placeholder-elsewhere line SURVIVING) before any merge. <!-- 2026-09-04: the green arm
  going RED first because the guard found ITSELF is the control working. 2026-09-08, coordinator: one battery run read residual = 11,
  ALL inside the guard's own census test file — the detector's quoted literals and its `Sprintf` fixtures, whose SEGMENT after the root
  is a placeholder — with the owner-account alternative at 0 and the authoritative repo guard green inside the same battery leg at the
  union. The template class was applied to hard hits too and given a six-arm control: four fixture shapes excluded, while a real segment
  and a placeholder-elsewhere line SURVIVE — the two arms that keep the class from laundering a name. Proved from the INSTALLED bytes on
  the union delta (raw 21 / template 11 / residual 0), after a first patch that collapsed its backslashes in transit and broke the parse,
  rebuilt from `chr(92)` per the Write-patterns rule below. -->
- **Key clearances by (path, segment) with a liveness test, NEVER by line** — a line-numbered clearance goes stale on the
  next edit above it, route #8. A structural pass skipped over fixture-heavy trees stays honest only if a second,
  token-keyed pass still runs there. A guard RED at master BY CONSTRUCTION names its merge-order constraint (with or
  after the cure) and is its own full-scale positive control.
- **A post tool censuses BOTH surfaces — the entry BODY and the COMMIT SUBJECT — with one planted control per class per
  surface.** A gate that refuses the redaction placeholder is a gate people route around, so fire the profile-path arm
  only when the prefix is followed by something that is NOT the placeholder bracket. <!-- 2026-09-07: one lane's body-only gap,
  a second lane's identical gap, and the coordinator's own tool carrying NO identifier census at all. -->
- **A positive control of an ADMIT arm on a state-advancing tool PUBLISHES** — run such controls behind a dry-run switch
  that stops after the guards and before any side effect (controlled by the remote tip unchanged before and after), and
  NAME a published artifact as such in a follow-up rather than rewriting it away. <!-- One posted a junk entry, 2026-09-05. -->
- **Audit a widened gate for false positives** on documented-allowed forms (kernel constants, pattern descriptions,
  placeholders) with a match-then-subtract allowlist controlled both ways. **An arm requiring a separator on BOTH sides
  misses a path ENDING in the token.** An allowlist on a REFUSAL arm is controlled in BOTH directions or it cannot
  fail — **an admit-only control reads GREEN on a dead arm** — and needs three properties: admit per OCCURRENCE (set
  membership on the captured segment) and **never per LINE**, which fails open on a line carrying a nickname AND a real
  host; a per-ARM scope (see the per-arm-parameter rule above); and a control of the mixed-line case. **Never write a
  negative lookahead into a `grep -E` arm**: ERE has no lookaround and `grep -P` cannot run on this box, so the arm
  matches NOTHING and fails OPEN silently — confirm the engine (Go's RE2 refuses lookaround at COMPILE, loudly). Where
  an instrument cannot provide per-occurrence matching and per-arm scope, **refusing BROADLY is the honest fallback** —
  a limit of that instrument, not a general prohibition on allowlisting a security gate. <!-- 2026-09-08: the lookahead arm
  shipped, a planted real-looking host read CLEAN, and it was caught only because the control ran the REFUSE direction too. The lane
  reverted to the broad arm on its own tradeoff (a false refusal costs one rewrite, a false pass costs a scrub), sent the warning BEFORE
  the coordinator's seat pushed, and RETRACTED the general form of its own warning — "do not allowlist a security gate" — within the hour
  rather than leave it on the record as a reason not to do what had just been done correctly. The correctly-done thing is the per-arm
  widening recorded above (network arm admits the nicknames, profile arm admits nothing, shared placeholder set untouched), same day. -->
- **Write patterns that must survive verbatim into a FILE with the Write tool**, never spliced through an interpolating
  shell. <!-- Instrument traps paid for in this class: (a) a coarse census whose network-share alternative is spelled with FOUR consecutive
  backslashes in the ERE — the bash-quoted eight-backslash form — matches only a four-backslash run and is BLIND to the ordinary
  two-backslash prefix; the coordinator's pre-post pre-checks carried that spelling all evening, the post tool's own arm was the gate and
  the pre-check was decoration (found by a train-41 derive sub-agent's control, 2026-09-07). (b) A python heredoc carrying a literal
  backslash-U sequence dies at parse, truncated unicode escape: build backslashes from chr(92). (c) `grep -c … || echo 0` prints TWO zeros,
  since grep -c prints its own 0 AND exits 1 (2026-09-08). (d) A sed-built probe rewrote the very pattern it was meant to exercise
  (2026-09-08). (e) A column reading uniformly empty is a DEAD instrument — a -prune that excluded exactly what it measured — and a killed
  run's temp files contaminated the next run's tallies, so the PRINTED table is the authority over the merged counts (2026-09-08). -->

## Completeness is a separate guard
⚠ **The security census is a CONTENT check and never a COMPLETENESS check** — two guards, two questions. An announce is
read for its GATE READINGS, not its prose: a seat is wired at the SHA and FILLED only when the readings are on the
record, so the APPENDER refuses a body still carrying a template placeholder token and ABORTS before the commit. Six
mechanics: <!-- 2026-09-05, three lanes in one evening: a gate-readings placeholder went out unfilled and the seat could not be filled
from it. The six mechanics below are what three independent derivations of that guard cost. -->
1. The predicate is a **whole-word, case-SENSITIVE TOKEN LIST**, not a bare-uppercase pattern, which would refuse
   ordinary words a gate line legitimately contains. **A guard's DESCRIPTION is not the guard**: state the mechanism you
   IMPLEMENTED and run it on real input before quoting it. <!-- An unanchored uppercase pattern also over-matched a bare
   three-letter word inside a longer token, found only by running the guard against the author's own honest post. -->
2. **The gate's PLACE is part of the property** — it runs BEFORE the fetch/checkout, with controls asserting the side
   effect did NOT happen.
3. **A guard over marker TEXT cannot tell a QUOTATION from an unfilled marker**, so write about a marker form in prose
   and never by spelling it; cure a legitimate upper-case TYPE spelled inside angle brackets (the converter's `ж<…>`
   forms) by naming the type OUTSIDE the brackets, never by relaxing case sensitivity.
4. **A pre-check and the guard it fronts must be ONE predicate** — pass `-CaseSensitive` to PowerShell's `Select-String`
   and carry two controls (an uppercase token refused, a lowercase generic admitted). <!-- A case-sensitive grep cleared an
   entry that Select-String, case-INSENSITIVE by default, then refused. -->
5. **Interpolate with a QUOTED heredoc plus a `sed`/python substitution, and read the body BACK between fill and
   append**, since no placeholder guard can see an empty substitution. <!-- An UNQUOTED heredoc executes every BACKTICK PAIR as a
   command substitution: a SHA in a heading became an empty string ("command not found" on stderr) and the post published with the SHA
   missing while the commit subject carried it. -->
6. **An ADMIT-arm control on a state-advancing tool publishes** — see the dry-run rule above.

## Other conventions
- C# style: see [`docs/coding-style.md`](docs/coding-style.md) (Allman braces, 4 spaces, `m_`/`s_`/`t_` field prefixes,
  explicit types over `var`, language keywords over BCL types, `\uXXXX` for non-ASCII).
- Conversion strategy: [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) — a high-level, example-driven
  **summary** of how each Go construct maps to C#; each section links into the exhaustive
  [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) for the full detail.
- Process/gate terminology as used in commit messages and reviews (CNR, A/B footprint, census, chip, guard, golden,
  overlay, banked…): [`docs/Glossary.md`](docs/Glossary.md).
- Generated C# intentionally targets Go-like *behavior first* (no implicit async), and Go-like *appearance* second
  (extra machinery hidden in partial classes / generated files).
