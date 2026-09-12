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
     context, so provenance kept this way costs ZERO tokens. -->

## Conventions

- **⚠ SECURITY — no real machine names or other internal-infrastructure identifiers on ANY pushed
  surface (owner order, 2026-09-01).** Every committed file, mailbox entry, commit message and branch
  name refers to fleet machines ONLY by their nicknames — `R-LAPTOP`, `G-LAPTOP`, `i9`,
  `i7`/`coordinator`. Real hostnames, UNC paths carrying them, and any other detail that exposes the
  owner's internal network (share names, non-public usernames) stay off GitHub entirely. The
  2026-09-01 scrub replaced every occurrence at both public tips (master and the mailbox branch); git
  HISTORY retains the originals (owner-accepted) — so never reintroduce one by quoting a pre-scrub
  record verbatim: re-census with a case-insensitive grep before banking any doc that copies old text.
  ⚠ **Two scrub rules paid for on 2026-09-02.** (1) **A SCRATCH-directory transpile's emission is not
  postable**: it records an ABSOLUTE source path in `GoPositionMap` (the committed file carries the
  relative `main.go`) and drops the hand-added `[GoTestMatchingConsoleOutput]`, so such a
  `package_info.cs` is never copied into the corpus and never pasted onto a pushed surface — one was,
  carrying a profile path plus worktree and session layout, and had to be scrubbed off the mailbox.
  Post emissions from a repo-relative run, or redact before posting. (2) **The pre-post grep covers
  the PATTERNS you quote, not only your prose** — a post that described its own census as
  `<name>|<profile-root>|/home/` spelled the real account name onto the pushed surface — and a
  security census of the mailbox reads `origin/claude/mailbox` after a VERIFIED fetch (an
  already-scrubbed line was re-reported from a stale copy). Census case-insensitively over BOTH
  profile-root spellings and `/home/`.
  ⚠ **RUNNING MASTER'S GUARD AGAINST ANOTHER BRANCH'S TREE** (2026-09-08): place the guard's package
  at a NEW UNTRACKED path inside the clone — its root resolver still finds the clone root — rather
  than copying master's converter tree OVER the branch's, which substitutes master's content for the
  branch's across hundreds of tracked files and changes the thing being measured. **Classify hits
  with the guard's OWN functions, never by eye**: six were the owner's given name as a form of
  address and one a network path whose host was ALREADY the prescribed nickname, firing only because
  the nicknames are absent from the guard's placeholder list — so "all false positives, stop" was the
  first conclusion, overturned by MEASURING the live post tool (four controls: it refuses that token,
  a planted network path and a planted profile path, and admits a clean entry), which made the
  operative policy DENY and the six lines pre-census residue. A tree gate and a delta gate are
  COMPLEMENTARY, not the duplicate census this file warns against; the red arm proving a tree gate
  covers a PENDING entry is a line appended and not committed; and **a scrub commit is re-guarded
  AFTER its rebase**, so the claim holds for the exact tree pushed.
  ⚠ **THE CENSUS THAT CLOSES THE CLASS — the order landed 2026-09-01 and was BREACHED in pushed docs
  by 2026-09-04, with no gate that could see it.** A security census takes **TWO PASSES**: a
  path-anchored pattern cannot see an identifier used OUTSIDE a path BY CONSTRUCTION (a directory
  listing's owner column, an "account X" parenthetical, a machine name in a roster row — the order's
  own headline clause), so a LITERAL pass over the identifiers the first pass surfaced is not
  redundant. Every hit outside the scrub scope is enumerated with a per-line REASON into an allowlist
  the instrument consults; the arithmetic closes in BOTH directions (the substituted class rising by
  exactly the substituted spans, the allowlisted class unmoved); the substitution is the identifier
  ALONE, in the tree's existing placeholder spelling; and the verification compares at the SAME LAYER
  (the working-tree form under the `eol=crlf` pin, never the LF blob) after being shown RED on one
  extra byte and after REFUSING an empty file list — a verification that passed vacuously over an
  empty list had already happened once. The durable form is a **standing census guard in the
  converter's own test suite**, allowlist and positive control inside it.
  ⚠ **A SURFACE WITH NO GATE ON IT IS ROUTE #6 ONE LAYER OVER** (2026-09-08): the mailbox branch
  forked BEFORE the fleet-identifier guard existed, so a safe-push composition could not find its
  test file there and correctly REFUSED — "a composition that cannot find its gate does not push" —
  which means every mailbox push to that date went out UNGATED, and master's guard run by hand
  against the mailbox tree read seven hits in one file, five of them in posts dated AT OR AFTER the
  scrub (the reintroduce-by-quoting mode). Scoping was done before routing: none from the reporting
  post; one the reporter's own, named; master GREEN on the same guard; and a clearance-liveness
  failure named as a SCOPE ARTIFACT of running master's guard on an older branch rather than counted.
  **A lane does not scrub a shared transport branch piecemeal** — the remedy is the coordinator's, as
  one act at the tip, plus the gate placed where the branch can reach it.
  ⚠ **Four mechanics that guard carries** (2026-09-04). A guard's OWN source is a tracked file the
  guard scans — and the file most likely to be edited by whoever adds the next entry — so it is NEVER
  exempted: planted fixtures are assembled through `Sprintf` so the source reads as a placeholder
  while the runtime string is real-looking, and the green arm going RED first because the guard found
  ITSELF is the control working. A denylist that would otherwise put the identifiers it forbids on the
  pushed surface is stored as HASHES, with each token's LENGTH in the SAME struct as its hash (a
  length kept in a second list can silently disagree and disarm the entry while every test passes).
  Clearances are keyed by **(path, segment)** with a liveness test, NEVER by line — a line-numbered
  clearance goes stale on the next edit above it, route #8. A structural pass skipped over
  fixture-heavy trees stays honest only if a second, token-keyed pass still runs there. And a guard
  that is RED at master BY CONSTRUCTION names its merge-order constraint (with or after the cure) and
  is its own full-scale positive control.
  ⚠ **A WIDENING THAT MUST REACH ONE ARM TRAVELS AS A PER-ARM PARAMETER FROM THE CALLER THAT KNOWS
  THE ARM, never through a list every arm shares** (2026-09-08): the fleet nicknames had to be
  admitted as network-path HOST segments and NOT as profile segments, and the shared placeholder list
  both arms consult would have widened BOTH — so the network arm passes its own admit set and the
  profile arm passes none, with no kind-string comparison a literal could drift against. The
  red-before arm neutered the MECHANISM (12 of 12 spellings fired, the only red arm, restore
  byte-identical); a denied token beside a nickname host is still caught; a nickname used as a profile
  segment is still refused; and the security census over the diff was positive-controlled only after
  its FIRST planting silently failed on a capital-letter escape in a `printf` — **the initial zero
  was a broken control, not a clean tree.**
  ⚠ **What a POST may say, and when the census runs** (2026-09-04). "Spell GOROOT exactly as
  `go env GOROOT` prints it" is an instruction about the ARGUMENT, never about the POST — a dispatch
  that said it invited a lane to quote its GOROOT verbatim onto the mailbox, profile root and account
  name included, scrubbed by a follow-up commit within minutes. **A post quotes the PATTERN it
  checked** (profile root, home prefix, doubled-backslash network prefix) **and never a value that
  matches one**; a toolchain pin is proven on a post by the bare `go version` line ALONE, which
  carries no path; and a docs seat's security grep NAMES its patterns rather than spelling them.
  **The pre-post census runs before EVERY push — diff-scoped and EXIT-GATED, or it is decoration**: a
  census whose exit code does not gate the push is a guard built and not armed.
  ⚠ **A POST TOOL CENSUSES BOTH SURFACES — the entry BODY and the COMMIT SUBJECT — with one planted
  control per class per surface, and its ADMIT-arm control runs behind a dry run that stops before any
  side effect** (2026-09-07: one lane's body-only gap, a second lane's identical gap, and the
  coordinator's own tool carrying NO identifier census at all). **A gate that refuses the redaction
  placeholder is a gate people route around**, so the profile-path arm fires only when the prefix is
  followed by something that is NOT the placeholder bracket. ⚠ **And a coarse identifier census whose
  network-share alternative is spelled with FOUR consecutive backslashes in the ERE — the bash-quoted
  eight-backslash form — matches only a four-backslash run and is BLIND to the ordinary two-backslash
  prefix.** The coordinator's pre-post pre-checks carried that spelling all evening; the post tool's
  own arm was the gate and the pre-check was decoration (found by a train-41 derive sub-agent's
  control, 2026-09-07). **A census pattern is positive-controlled on a planted line of EACH class it
  claims to detect.** Companion: a python heredoc carrying a literal backslash-U sequence dies at parse
  — truncated unicode escape — so build backslashes from `chr(92)`.
  ⚠ **A CENSUS WITH ELEVEN PATH-AND-HOST ARMS AND NONE FOR THE CLASS THAT FIRED PASSES BY LUCK, NOT
  BY GATE** (2026-09-08): six of seven scrub hits were the owner's GIVEN NAME in prose, and a lane's
  pre-post census passed hundreds of posts only because its author never typed the name. The arm was
  red-tested BEFORE the fix (a body naming the owner PASSED), then fixed with the tokens DERIVED and
  never spelled — the surname from the account name, the given name from the configured email's local
  part — with a derivation shorter than four characters ABORTING rather than installing a
  two-character detector, red/red/green controlled. ⚠ **And DISCIPLINE IS NOT A GATE: the PRE-SCRUB
  TREE is a better positive control than anything planted.** A second lane's four path-and-account
  arms read CLEAN on the tree that held all six name hits — its "every post censused clean" was TRUE
  and measured less than it sounded — so its new arm derives its tokens from the configured user name
  (split on non-letters, under-three-character pieces dropped, so the script never carries what it
  forbids) and is controlled three ways: the real pre-scrub tree refuses with exactly the six, the
  post-scrub tree is clean, its own posts unchanged. **A disclosure of one's own broken gate is what
  made the second lane look at its own; a clean report would have moved nobody.** Two instrument
  notes: a column reading uniformly empty is a DEAD instrument (a `-prune` that excluded exactly what
  it measured), and a killed run's temp files contaminated the next run's tallies, so the PRINTED
  table is the authority over the merged counts.
  ⚠ **THE SECURITY-CENSUS FALSE CLEAN HAS AT LEAST TWO DOORS ON GNU grep 3.0, AND BOTH READ AS A CLEAN
  ZERO — so the rule is NOT "avoid `-F`", it is POSITIVE-CONTROL THE INSTRUMENT ON THE BOX THAT WILL RUN
  IT.** Measured independently on two lanes at the same grep version (2026-09-07): on one, `-i` combined
  with `-F` returns EMPTY (`-c -F` gives 1, `-ic` gives 1, `-ic -F` gives nothing); on the other, a
  case-insensitive count whose pattern ENDS IN A BACKSLASH returns EMPTY with *Trailing backslash* while
  the same pattern BRACKETED returns 1. **Our pre-post identifier census is specified as a
  case-insensitive LITERAL match — precisely the dead combination** — so the one instrument the owner's
  standing security order rests on can read NOTHING and report clean, and neither door is portable: a
  lane that "avoided `-F`" on one box walks into the other. Joins `grep -P`, the UTF-16 redirect, the LF
  anchor and the leading-slash path conversion. ⚠ **And a census NOT WIRED TO AN EXIT is a DECORATION,
  more dangerous than no census because it LOOKS like the check** — one lane printed `census: 0` above
  eight entries in a session, and for one pattern that `0` meant the instrument had ERRORED; another
  lane's census returned DIRTY / exit 1 and the commit and push ran anyway. What protected the posts was
  a SEPARATE gate that EXITS non-zero on any hit, one bracketed case-insensitive alternation over the
  forbidden token classes, positive-controlled with one planted identifier per class. **The remedy is to
  DELETE the decoration, not repair it** — a second census duplicating a gate adds a way to be wrong and
  no way to be right — and the fix is the one-line wiring (`|| exit`), never a new census.
  ⚠ **THE SAME CLASS, PAID BY THIS FILE'S OWN AUTHOR** (2026-09-08): a coordinator composed its
  security census into the SAME command chain as the push, and the push ran whatever the census
  printed — the branch was on the remote before the number was read. It was clean, which is exactly
  why the shape survives: the log shows the gate running and reading, and the ORDERING makes its
  verdict inert. ⚠ **And a COUNT-ONLY census over a tree that legitimately carries a public URL on
  every page is CLASSIFIED before it is read as a number**: 904 hits, every one the repository's own
  public URL, with the profile-path, home-prefix and network-prefix arms all at zero. **An
  unclassified total is a number, never a finding** — and a number nobody classified is exactly what
  a gate wired after the push cannot stop.
  ⚠ **A SECURITY-GATE CONTROL MUST BE CATCHABLE BY EXACTLY ONE ARM, or it cannot tell you that arm
  works** (2026-09-07): a lane ran SIX green controls over a gate whose user-path arm was COMPLETELY
  DEAD — a heredoc had collapsed its doubled backslashes into the literal drive-colon-Users text —
  because the probe meant to prove that arm ALSO carried the account name, and the account arm caught
  it. **Per-arm isolation**: each probe uses a token no other arm matches, verified end to end through
  the real script; patterns that must survive verbatim live in a FILE written by the Write tool, never
  spliced through an interpolating shell; and a widened gate is AUDITED for false positives on
  documented-allowed forms (kernel constants, pattern descriptions, placeholders) with a
  match-then-subtract allowlist controlled both ways. Its trailing sibling: **an arm requiring a
  separator on BOTH sides misses a path ENDING in the token.**
  ⚠ **TWO SECURITY CENSUSES ANSWER DIFFERENT QUESTIONS, AND A CLEAN READING FROM ONE DOES NOT CERTIFY
  THE OTHER** (2026-09-08): master's fleet guard keys on actual IDENTIFIERS (a hashed denylist plus a
  clearance allowlist) and read 0 on the scrubbed mailbox tree, while a lane's SHAPE-keyed pre-post
  census REFUSED with 18 — a quoted pattern-plus-ellipsis in prose, the profile ENVIRONMENT
  VARIABLE's name, bare generic home prefixes, and seven quotations of captured `-json` test output
  whose every separator is DOUBLED by JSON escaping and so is indistinguishable from a network prefix
  to a doubled-separator detector. The account-name class — the only real leak — read clean
  everywhere. Reusable: **any document quoting captured `-json` output trips a doubled-separator
  detector on escaping alone.** The strict instrument belongs where a false refusal costs its owner
  one rewrite (its own pushes), and it earned that placement by refusing the very post that first
  quoted the three shapes literally.
  ⚠ **ITS COMPLETENESS SIBLING — a post that ships with a TEMPLATE PLACEHOLDER still in it, and the
  guard that catches one (2026-09-05, three lanes in one evening).** A gate-readings placeholder went
  out unfilled and the seat could not be filled from it: **an announce is read for its GATE READINGS,
  not its prose** — a seat is wired at the SHA and FILLED only when the readings are on the record.
  So the APPENDER refuses a body still carrying a placeholder token and ABORTS before the commit,
  because **the security census is a CONTENT check and never a COMPLETENESS check** — two guards, two
  questions. Six mechanics the three independent derivations of that guard cost. (1) The predicate is
  a **whole-word, case-SENSITIVE TOKEN LIST**, not a bare-uppercase pattern, which refuses ordinary
  words a gate line legitimately contains; an unanchored uppercase pattern also over-matched a bare
  three-letter word inside a longer token, found only by running the guard against the author's own
  honest post — **a guard's DESCRIPTION is not the guard, so state the mechanism you IMPLEMENTED and
  run it on real input before quoting it.** (2) The gate's PLACE is part of the property: it runs
  BEFORE the fetch/checkout, with controls asserting the side effect did NOT happen. (3) A guard over
  marker TEXT cannot tell a QUOTATION from an unfilled marker, so **write about a marker form in prose
  and never by spelling it** — and by the same token a legitimate upper-case TYPE spelled inside angle
  brackets (the converter's `ж<…>` forms) trips it, which is cured by naming the type OUTSIDE the
  brackets rather than by relaxing the case sensitivity. (4) **A pre-check and the guard it fronts must
  be ONE predicate**: a case-sensitive `grep` cleared an entry that PowerShell's `Select-String` —
  case-INSENSITIVE by default — then refused, so pass `-CaseSensitive` and carry two controls (an
  uppercase token refused, a lowercase generic admitted). (5) An UNQUOTED heredoc executes every
  BACKTICK PAIR as a command substitution: a SHA in a heading became an empty string ("command not
  found" on stderr) and the post published with the SHA missing while the commit subject carried it —
  interpolate with a QUOTED heredoc plus a `sed`/python substitution, and **read the body BACK between
  fill and append**, since no placeholder guard can see an empty substitution. (6) **A positive
  control of an ADMIT arm on a state-advancing tool PUBLISHES** — one posted a junk entry — so such
  controls run behind a dry-run switch that stops after the guards (controlled: the remote tip
  unchanged before and after), and a published artifact is NAMED as such in a follow-up, never
  rewritten away.
  ⚠ **A NEGATIVE CONTROL THAT PROVES THE SUBJECT IS NOT A CONTROL OF THE INSTRUMENT** (2026-09-08,
  `99a1f5e`): a security census anchored its "the grep works" control on a token every mailbox post
  happens to carry — the lane's own name — so on a CODE COMMIT MESSAGE, which carries no such token,
  the census ABORTED as unprovable. **The abort was correct; proceeding past it was the defect.** The
  instrument PLANTS its own sentinel into the input copy and greps for THAT, so the control coincides
  with the subject on NO input — proven by four arms (a clean commit-shaped file passes; the OLD census
  aborts on it; a planted network-share path refuses with the control at 1; a real post still passes).
  Companion: **a code branch is a pushed surface too** — the commit message, the staged diff AND the
  REF NAME all take the census, exit-gated, and the tool HOLDS after announcing the SHA.
  ⚠ **A REV-RANGE CENSUS IS STRUCTURALLY BLIND TO THE REF A PUSH CREATES, and path-shaped patterns
  are NOT inert on ref-shaped input** (2026-09-08): git rejects a backslash in a ref name, but every
  separator class admits the forward slash and refs permit dots and underscores, so **6 of 6
  identifier patterns FIRED on ref-legal probes** — a gate author reasoning "refs cannot hold paths"
  ships four inert arms believing them live. The branch-name arm runs BEFORE the push, with a
  sentinel the instrument derives from the pattern file at runtime, and its load-bearing-ness is
  proven by a NEUTER (exactly one arm red, its neighbours green, the restore byte-identical). Two
  instrument faults met on the way, both UNDER-reporting coverage: `grep -c … || echo 0` prints TWO
  zeros (`grep -c` prints its own 0 AND exits 1), and a `sed`-built probe rewrote the very pattern it
  was meant to exercise.
- C# style: see [`docs/coding-style.md`](docs/coding-style.md) (Allman braces, 4 spaces, `m_`/`s_`/`t_`
  field prefixes, explicit types over `var`, language keywords over BCL types, `\uXXXX` for non-ASCII).
- Conversion strategy: [`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) — a high-level,
  example-driven **summary** of how each Go construct maps to C#; each section links into the exhaustive
  [`docs/ConversionStrategies-Reference.md`](docs/ConversionStrategies-Reference.md) for the full detail.
- Process/gate terminology as used in commit messages and reviews (CNR, A/B footprint, census,
  chip, guard, golden, overlay, banked…): [`docs/Glossary.md`](docs/Glossary.md).
- Generated C# intentionally targets Go-like *behavior first* (no implicit async), and Go-like *appearance*
  second (extra machinery hidden in partial classes / generated files).

