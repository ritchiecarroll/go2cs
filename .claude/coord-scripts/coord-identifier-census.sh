#!/usr/bin/env bash
# =================================================================================================
# coord-identifier-census.sh -- the fleet's ONE identifier census. Reference consumer of
# coord-identifier-patterns.txt (the ONE definition of the arms) and coord-identifier-hashes.txt
# (the ONE list of denied names, as hashes, generated from the repository's own Go guard).
#
# -------------------------------------------------------------------------------------------------
# HOW A POST TOOL ADOPTS IT
#
#   C="$(dirname "$0")/coord-identifier-census.sh"
#   "$C" entry   "$ENTRYFILE"        || exit $?      # THE GATE
#   "$C" subject "$COMMIT_SUBJECT"   || exit $?      # THE GATE
#   git fetch origin claude/mailbox                  # then, and only then:
#   TIP="$(git rev-parse FETCH_HEAD)"
#   "$C" tree docs/phase4/MAILBOX.md "$TIP"          # A READING. Do not gate the push on it.
#   # ... append, commit, push.
#
# Each gate call exits non-zero on a refusal, so `|| exit $?` is the whole wiring. THE CENSUS RUNS IN
# ITS OWN COMMAND, BEFORE the push and NOT in the same chain as it: a census composed into the push
# chain lets the push run on whatever the census printed. A census whose exit code does not gate the
# push is a guard built and not armed.
#
# ⚠ TREE MODE IS A READING, NOT THE GATE, AND ITS BASELINE IS THE FETCHED TIP.
#
# `entry` and `subject` are what decides whether a post may go out: they read the bytes this lane
# wrote, in STRICT mode, and nothing else. `tree` answers a different question -- what does the
# shared surface hold, and what would this append add to it -- and its answer is only as good as its
# baseline.
#
# THE BASELINE MUST BE THE TIP BLOB AS FETCHED IMMEDIATELY BEFORE THE APPEND. Never a lane's
# last-read sha, never a stored anchor, never "the sha I posted last time". Everything other lanes
# landed in between is otherwise attributed to THIS post. C2 measured both arms on one clean entry:
# baseline = its own last-read sha gave added=4 and REFUSED; baseline = the freshly fetched tip gave
# added=0 and CLEAN. The four were six other lanes' entries that had landed in the interval. Same
# post, same bytes, opposite verdicts -- the difference was entirely the baseline.
#
# So a post tool calls tree AFTER its fetch, with the sha that fetch produced, and treats a non-zero
# exit as something to READ rather than something to obey. A pre-existing hit on a shared surface is
# not this post's to fix and not this post's to be blocked by.
#
# NO POST TOOL EVER PASSES --unmask. See MASKING below.
#
# THE RULE: NO TOOL CARRIES A PRIVATE COPY OF ANY ARM. If an arm is wrong, it is wrong HERE, for
# everyone, and it is fixed HERE. A tool that adds a local pattern has re-created the three-tools-
# three-definitions state this file exists to end. A tool that needs an arm WIDENED for one case
# passes that widening as a per-arm admit set in the patterns file -- never as a list every arm
# shares, which widens all of them.
#
# -------------------------------------------------------------------------------------------------
# RELATION TO THE REPOSITORY'S OWN GO GUARD
#
# src/go2cs/internal/repoguard/fleetIdentifierCensus_test.go (TestNoFleetIdentifiersInTrackedFiles)
# is the TRACKED-TREE gate: it runs under the plain `go test ./...`, scans every tracked file, and
# is the authority on the structural classes and on which names are denied. This census is the
# PRE-POST gate: it runs before anything becomes tracked or pushed, over an entry body, a commit
# subject, or a shared transport file.
#
#   same classes  the profile/home arm is that file's fleetProfileRe, split into its two
#                 alternatives; the backslash share arm is its fleetNetworkRe; the placeholder admit
#                 set is its fleetIsPlaceholder plus fleetPlaceholderSegments, consulted by the
#                 profile, home AND unc arms exactly as fleetConsiderSegment consults it for every
#                 kind; the UNC host admit set is its fleetNicknameHostSegments, exactly those four
#                 and no more.
#   same names    coord-identifier-hashes.txt is generated from its fleetDeniedTokens. Neither file
#                 carries plaintext.
#   same tokens   candidates are maximal [a-z0-9._-] runs PLUS each dot/hyphen/underscore component,
#                 which is what makes a machine name and the account name inside it both match.
#   what is new   the IPv4 arm, the forward-slash share arm, the host-assignment arm and the
#                 owner-name-in-prose arm have no counterpart there and are marked as additions.
#   what differs  the Go guard hashes every token of every tracked file in process. This census
#                 does BOTH halves, and they are different instruments:
#                 (1) THE RUN-TIME ARMS invert the comparison: they hash the handful of literals the
#                 LOCAL BOX can derive and report which hashed rows those matched (hashes=N
#                 matched=M). A denied row this box cannot supply plaintext for is not one of THOSE
#                 arms -- which is how a post naming the owner read CLEAN on a box whose account is a
#                 generic one (2026-10-06) and REFUSED, same bytes, on a box whose account is a
#                 denied row. A verdict that depends on who is asking is not a gate.
#                 (2) THE TEXT_DENIED ARM (2026-10-06) is the Go guard's own comparison, and ANY box
#                 can run it: in the STRICT modes (entry, subject, converted) the candidate tokens
#                 of the TEXT ITSELF are hashed -- and, beyond what the Go guard does, every WINDOW
#                 of every denied length over the buffers the passes read -- and a (length, hash)
#                 pair equal to a denied row refuses. The text supplies the plaintext the box could
#                 not. It needs no account name, no machine name, no git identity and no token
#                 file. See TEXT_DENIED below for what it reads, what it costs, and exactly what it
#                 still does not find.
#                 `tree` mode does NOT run it -- delta is a reading over a long shared surface and
#                 is not the gate -- and says so on its own line rather than printing a zero.
#
# The two are COMPLEMENTARY and neither certifies the other.
#
# -------------------------------------------------------------------------------------------------
# NAMING CONVENTION -- USE A CLASS NAME WITH PROVENANCE, NEVER AN INSTANCE
#
# The fleet's own discussion of this class is the fastest way the class GROWS: arguing about the
# IPv4 arm put seven new matchable strings on the shared surface in forty minutes, and every one is
# indistinguishable to every arm from the thing being guarded. When a post, a commit message, a
# board row or a census report must refer to one of these, it names the CLASS AND ITS PROVENANCE:
#
#     the «ipv4» in <lane>'s <item> line          the «unc» in <file>:<line>
#     the «profile-path» in <lane>'s <item>       the «account» in <file>:<line>
#     the «version-quad» in <lane>'s <item>       the «host» in <lane>'s <item>
#
# The class alone is not enough -- a reader cannot tell a known negative from a leak from a bare
# «ipv4». The provenance is what makes the sentence decidable without the value.
#
# -------------------------------------------------------------------------------------------------
# MASKING -- THE REPORT AND THE REFUSAL ARE MASKED IDENTICALLY
#
# The REFUSAL path is the only path that runs when a real identifier is actually present, so it is
# the path that must not print one. It does not get its own renderer: refusal and report are the
# same lines from the same function. Every hit prints ARM, PASS, LINE NUMBER, a MASKED rendering and
# a short fingerprint -- and never the matched line and never the matched value.
#
#   «ipv4»            N.x.x.x                first octet only
#   everything else   <*REDACTED-<len>*>     COORD's rule: length only, no fragment
#   fingerprint       fp=<8 hex>             FNV-1a of the lower-cased value, so two different hits
#                                            stay distinguishable while neither is readable
#
# --unmask is for a LOCAL CONSOLE ONLY and is OFF by default. It adds the matched VALUE in the clear
# and the matched LINE rendered through COORD's line rule -- longest literal first, so a contained
# literal is never masked ahead of the literal that contains it (masking a 7-character account
# before the 13-character machine name that contains it renders the machine as
# `<*REDACTED-7*>-...`, which discloses exactly the infrastructure detail the order forbids), then
# the profile-root, home-prefix and share-prefix shape masks, then truncation at 160 characters. NO
# POST TOOL EVER PASSES IT, and nothing --unmask prints may be pasted into a post.
#
# -------------------------------------------------------------------------------------------------
# MODES
#   entry <file>              whole-entry census. Refuses on ANY hit. STRICT.
#   converted <file>...       STRICT census over a tracked CONVERTED GO TEST SOURCE, in which the
#                             arms the definition marks [CONVERTED-CONTEXT] -- the LITERAL-shaped
#                             ones -- are REPORTED with their counts and cannot refuse, while every
#                             other arm refuses exactly as in entry. REFUSES TO RUN (rc 2) on a path
#                             that is not one. See CONVERTED SOURCES below.
#   subject <string>          the same census over one string (a commit subject). STRICT.
#   tree <file> <baseline>    DELTA census over a tree-wide surface: hits in <file> at the worktree
#                             (or at HEAD if it is not on disk) MINUS hits in `git show
#                             <baseline>:<file>`. Refuses ONLY on hits this post ADDS. Prints the
#                             pre-existing count per arm loudly -- a pre-existing hit is not this
#                             post's to fix and not this post's to be blocked by, but it is never
#                             silent.
#   selftest                  the triad: PLANTS that must refuse, KNOWN NEGATIVES that must pass,
#                             and the DECLARED SET. Plants live only in the test's temp files and
#                             are never echoed; the output names arms and PASS/FAIL only. A case
#                             whose premise is a fact about the BOX (it derives a denied name) is
#                             counted NOT APPLICABLE where that is false: a third count, on its own
#                             line above the tally, named with its reason, never a pass.
#
# RULE 4 IS ADJACENCY, NEVER A WINDOW. A version word is allowed to excuse a quad only from the
# quad's OWN whitespace-delimited word or from the word immediately before or immediately after it.
# The first draft read a 56/32-character window and C2's A/B showed what that costs: a routable-
# shaped quad ALONE was refused, and the same quad with any version word loose in the window read
# CLEAN. A window is a laundering surface -- it lets a sentence exonerate an address that has nothing
# to do with it, and a lane that wanted to spell one only had to mention a version nearby.
#
# RUN-TIME TOKEN ARMS ARE BOUNDED BY THE DENIED SET. A derived token is a GUESS about what this box
# is called; on a container it is an ordinary English word. C2 measured 4348 pre-existing hits from
# RUNTIME_ACCOUNT and RUNTIME_OWNERNAME on its own box from exactly that. A derivation is now used
# only if it is 5+ characters, is not a stop-listed generic account name, AND is in the denied set --
# its hash a row in coord-identifier-hashes.txt, or its literal a line in the local never-push token
# file. The denied set is the authority on which names are forbidden; a derivation that is not in it
# is a name this fleet never denied. An arm that fails those bars prints
# `<ARM>: token not in the denied set, arm inert` and does not fire -- reported, never silent.
#
# STRICT vs DELTA. In STRICT mode the IPv4 arm runs with NO context exclusion at all: a lane must
# not be able to spell an identifier by making the sentence around it sound like documentation, and
# the cost of the strict reading falls on the writer, as one rewrite of their own post. The version,
# branch, assembly and documentation-constant context rules apply ONLY in tree mode, where their job
# is to CLASSIFY the pre-existing hits of a long shared surface -- an unclassified total is a number,
# never a finding. So does rule 6, the POSITION-MAP FUNCTION-LITERAL rule (2026-09-28): a quad that is
# exactly the closure-counter suffix of one funcLits entry, inside the FOURTH argument of a
# GoPositionMap record, is excluded and counted as `position-map-funclit`. It reads the record around
# the quad, so it is a context rule, and STRICT refuses it for the reason it refuses the others -- a
# lane can write its sentence as a record. The reasoning is on the rule, in scanIpv4.
# THE ONE EXCLUSION STRICT MODE TAKES is the release_literal ADMIT SET (rule 5): it
# is read on the QUAD'S OWN CHARACTERS and on nothing around it, so unlike a context rule it cannot
# be arranged by the sentence a lane writes -- which is exactly why a context rule is refused here
# and a shape admit is not. What it costs is stated on that arm's own line in the definition.
# Every other arm is identical in both modes.
#
# `entry`/`subject` ask "does what I am about to write carry one?". `tree` asks "does what I am
# about to write ADD one to a surface that already holds some?". Two questions; a clean reading from
# one does not certify the other.
#
# -------------------------------------------------------------------------------------------------
# CONVERTED SOURCES -- WHY THE LITERAL ARMS COME OFF, AND ONLY THERE (2026-09-22)
#
# A converted Go test source carries GO'S OWN TEST DATA, converted verbatim. STRICT's whole rationale
# is that "the cost of the strict reading falls on the writer, as one rewrite of their own post" --
# and that is FALSE of a file the lane did not author, exactly as it was false of the evidence record
# that moved the documentation-constant admit into strict. MEASURED on two lanes' H10 re-bank shards
# on the day this mode was added: 13 refusals on one and 25 on the other, over staged `*_test.cs`
# under src/core, and every value was found verbatim in the same package's own `*_test.go` under the
# pinned GOROOT -- a dnsName «ipv4» in a certificate test, policy-test names, a base64 position map
# whose characters satisfy the quad arm, a loopback-name-and-port literal satisfying host_ctx. The lanes ran repoguard
# by hand and explained the count in prose on every shard. The tool says it instead.
#
# WHAT MOVES: only the arms the DEFINITION marks [CONVERTED-CONTEXT]. Nothing is listed in this
# script -- an arm is declared once, in the patterns file, with every other property of it. A run
# whose downgrade set reads EMPTY is an instrument failure (rc 2), not a run that happens to equal
# entry.
#
# WHAT DOES NOT MOVE: profile_root, home_unix, the unc_ arms and the run-time token arms refuse in
# converted mode exactly as in entry. A REAL identifier reaching a converted file is not Go's data --
# it is an absolute source path baked into an emission, or an account name in a captured line -- and
# that is precisely the class the repository's own guard keeps its denied-token pass running for over
# the very fixtures whose STRUCTURAL pass it skips (fleetIsUpstreamFixture). A unc_ hit here is
# therefore either the JSON/C#/Go escape the unc_escape admit already recognises IN BOTH MODES, or it
# refuses and the lane reads it against the guard. STATED RESIDUAL: an escaped literal whose host
# token is not that recognised escape shape still refuses in this mode.
#
# THE GATE OF RECORD for the TRACKED TREE is repoguard's TestNoFleetIdentifiersInTrackedFiles, which
# already admits this class by file. This mode is the PRE-POST reading of the same file, and it is
# STRICTER than that guard by construction: the guard skips the whole structural pass for a
# `*_test.cs`; this keeps every structural arm but the marked ones. `entry` remains the gate for
# messages, docs and scripts, and a file that is not a converted source is refused INTO it.
#
# -------------------------------------------------------------------------------------------------
# TEXT_DENIED -- THE DENIED ROWS, MATCHED AGAINST THE TEXT ITSELF (2026-10-06)
#
# THE INVARIANT. On ANY box this arm DISCOVERS a denied word wherever the three passes would hit or
# admit it if it were installed as a literal. A box that derives the word and a box that derives
# nothing then read one text to one verdict. WHAT IS STILL NOT FOUND, below, is the whole of what
# that sentence does not cover.
#
# WHAT IT READS -- TWO ROUTES, ONE PROCESS EACH.
# (1) THE WINDOW SCAN. The census awk writes, through its own tokenBuffer, the buffer its token
# passes read for each line and for each adjacent line pair joined with the whitespace at the break
# removed (the PASS 2 join; the CR of a CRLF line end is whitespace at the break, on every awk). That
# buffer is the lower-cased text WITH THE PUBLIC-URL SPAN BLANKED -- it is what PASS 3 reduces, and
# PASS 3 is the authority; it is NOT the bare lower-cased line. One scanner process then hashes EVERY
# WINDOW of every denied row's length over (a) each maximal [a-z0-9._-] run of each buffer and (b)
# the alphanumerics-only reduction of each buffer -- each distinct window once -- and looks every
# digest up in the denied rows itself. So a word is found standing alone, GLUED inside a longer word
# (a plural s, a digit, any prefix or suffix, a CamelCase neighbour, the last letter or digit of a
# percent-escape or a backslash-escape), or BROKEN by characters that are not its own (a space
# between two of its letters, a zero-width space, a form feed or a no-break space or a lone CR at a
# wrap). The scanner is python, python3 or perl with Digest::SHA -- the first that answers its probe.
# (2) THE CANDIDATES, kept from the first cut of this arm, and what the Go guard reads: every maximal
# [a-z0-9._-] run of the lower-cased line, each dot/hyphen/underscore component of a run, and -- the
# one thing the Go guard does not read -- each SPAN of adjacent components, separators kept, no longer
# than the longest denied row; over each line and each joined pair. Only a candidate whose LENGTH is
# some row's length is hashed, all of them by one process: python where it answers, else one
# sha256sum over one file per candidate. This is also the route that finds the ADMIT words (below).
# EACH ROUTE CARRIES A KNOWN-ANSWER PROBE ON EVERY CENSUS, so a hasher or a scanner that prints
# nothing, or the wrong thing, is not one. The scan's probe exercises hashing, windowing over a run,
# windowing over the reduction and the set lookup at once; see idc_window_scan.
#
# WHAT A MATCH DOES. The matched word is installed as a literal of the TEXT_DENIED arm FOR THAT ONE
# CENSUS, and the ordinary three passes then read the text for it exactly as they read it for a
# run-time literal: PASS 1 per line, PASS 2 across the join, PASS 3 over the alphanumerics-only
# reduction. So the arm is counted, masked, printed and delta-keyed by the code every other token arm
# uses, and there is no second renderer that could spell a value.
#
# SUBSTRINGS, EXACTLY. A denied word is FOUND wherever its bytes are a window of a run or of the
# alphanumerics-only reduction of a buffer the passes read -- whole, glued or broken -- and, once
# found, PASS 1, 2 and 3 read the text for it as they read it for a run-time literal. Until
# 2026-10-06 it was found only as a whole run, a whole component or a whole span of components, and
# a text that carried it ONLY glued or ONLY broken read CLEAN on a box that derives nothing and
# REFUSED on a box that derives the word: the incident's own asymmetry, by another door. That is
# closed, not stated. The Go guard still has that limit, and the span limit too.
#
# WHAT IS STILL NOT FOUND, EXACTLY -- four shapes, each pinned by a self-test case (D1 and D2):
#   (1) A DENIED WORD THAT ITSELF CONTAINS A SEPARATOR (a dot, a hyphen or an underscore), spelled
#       any way but its exact bytes: its separator dropped or re-spelled, or the word broken by a
#       character that is not its own. Only the exact bytes of a row are hashed, so only they are
#       recognised; such a word IS found whole, glued inside a longer run, and wrapped across one
#       line break. PASS 3 reduces an installed LITERAL as well as the line, so a box that DERIVES
#       that word -- a machine name, on that machine -- still refuses those spellings: on that text
#       the verdict still depends on the box. Closing it needs the digest of each row's reduction in
#       the hash list, which is the Go guard's list to extend. A denied word of letters and digits
#       only has no such case.
#   (2) A word wrapped across THREE OR MORE LINES. No pass joins more than an adjacent pair, so no
#       box finds it, derived or discovered: the same on every box, and outside the invariant.
#   (3) A word spelled with a BYTE THAT IS A LETTER ONLY TO ONE LOCALE. Every program this tool runs
#       reads BYTES (LC_ALL=C, set at the top whatever the caller exported), and its letters are the
#       ASCII ones. So a word broken by a double-byte character whose trail byte is an ASCII letter
#       or digit (possible in Shift-JIS, GBK and Big5; never in UTF-8, where no byte of a multi-byte
#       character is ASCII) keeps that byte inside it and is found by no box; and a capital letter
#       outside ASCII is lower-cased by no box. Until 2026-10-06 the awks read CHARACTERS in the box's
#       own locale and the scanner read bytes, and both of these depended on the box. MEASURED before
#       the change: the double-byte break REFUSED on a box holding the word under ja_JP.SJIS, zh_CN.GBK
#       and zh_TW.BIG5 and read CLEAN on a box deriving nothing (gawk 5.0.0, Git Bash); a word spelled
#       with the dotted capital I (U+0130) for its i REFUSED under gawk 5.0.1 on Linux in C.UTF-8 and
#       en_US.utf8 -- on a box holding the word and on one deriving nothing -- and read CLEAN under
#       mawk, under that gawk in the C locale and under gawk 5.0.0 on Git Bash. Both now read CLEAN
#       on every box: the same everywhere, and outside the invariant. THAT SECOND ONE IS A NARROWING
#       ON ONE CLASS OF BOX, stated as one: the Linux gawk in a UTF-8 locale used to refuse it.
#       A break by a byte pair neither of which is a letter or digit is found everywhere, as before.
#   (4) A BROKEN SPELLING BESIDE AN ADMITTED HANDLE. A denied word spelled with characters that are
#       not its own -- its letters spaced out, a hyphen or a zero-width space inside it, or the word
#       hyphen-wrapped across one line break -- reads CLEAN when an admitted public handle THAT
#       CONTAINS THE WORD stands on the same line; for a spelling broken across a line break, on
#       either of its two lines. The handle may be bare, in its dotted module-path spelling, or
#       inside the public URL. Only PASS 3 finds a broken spelling, and its admit (admitWord) asks
#       one question of the text it fired on: is every WORD that contains the literal an admitted
#       handle, and is there at least one? A broken spelling is inside no word, so the handle
#       answers for it. WHAT STILL REFUSES: each of these without the handle; each of these with
#       the handle on a line the broken spelling does not touch; and a word wrapped WITHOUT a
#       hyphen, beside the handle or not (PASS 2 reads the joined word as a word of its own).
#       IT IS THE SAME ON EVERY BOX -- the box that holds the word, the box that derives nothing
#       -- AND IT IS OLDER THAN THIS ARM: the tool before TEXT_DENIED reads these texts CLEAN on a
#       box that holds the word. So it is outside the invariant, and it is an evasion shape, not
#       an accident shape. A FIRST CUT AT CLOSING IT WAS MADE AND TAKEN BACK ON 2026-10-06: an admit
#       that counted occurrences REFUSED the handle, or its dotted module path, FIRST on a line after
#       a line ending in a word character (an unbulleted list of repositories or project files), and
#       read the handle glued to the public-URL span CLEAN on one kind of box and REFUSED on the
#       other. Both are pinned beside the residual (D1 i; D2 h1 and o). QUEUED for a design of its
#       own.
#
# THE PUBLIC-HANDLE ADMIT applies to it as to RUNTIME_ACCOUNT and RUNTIME_OWNERNAME, by the same
# marker in the definition: an occurrence INSIDE a longer word takes that word whole, and a word that
# is an ADMIT row is reported as public-handle and not counted. A bare denied word is never admitted.
# (A BROKEN one beside a handle is residual 4 above: the admit reads WORDS, and it is in none.)
# So that this is true on a box that cannot derive the handle either, the words of the text (and
# their dot segments, for the dotted module-path spelling) whose (length, hash) is an ADMIT row join
# the admit set for that census. The ADMIT section is the authority in both directions; a word that
# merely contains a handle is not an ADMIT row and admits nothing. A HANDLE THAT CONTAINS A DENIED
# WORD is therefore discovered by the scan wherever it is written outside the blanked public-URL
# span, and cleared by this admit: the count appears as PUBLIC-HANDLE ADMITS, on a box that derives
# nothing exactly as on the owner's.
#
# IT COUNTS ON ITS OWN. On a box whose run-time arm already holds the same literal, one occurrence is
# a hit under BOTH arms. Each arm reports what IT saw; a zero under TEXT_DENIED must mean the text
# carries no denied word, never that another arm was left to say so.
#
# IT FAILS CLOSED. No usable hasher; NO USABLE WINDOW SCANNER; a buffer awk that fails, reports no
# count, or wrote NO buffer from a text that is not empty (and, in every mode, a census awk that read
# no line of one); a scanner that did not account for every buffer written; a word the scanner named that the
# hash list does not confirm; a hash list that is missing or has a row that does not parse (a length
# written with a leading zero is such a row); or zero denied rows: REFUSED(2) with the arm and the
# reason named. THERE IS NO NARROWED MODE -- a box without python and without perl does not fall back
# to the candidates alone, because that is the verdict that depends on the box. Before this arm a
# missing hash list only made the run-time arms inert; in a STRICT mode it now refuses.
#
# COST. Every strict census prints its own: candidates=, hashed= (the hasher's own count of digests),
# scan-windows= (the scanner's own count of windows hashed), the hasher and scanner it used, and
# lookups= (how many times the authoritative row readers were asked: one per prefilter positive).
# MEASURED 2026-10-06 on the i7 (Windows, Git Bash, Python 3.11.0), wall seconds, the fastest of
# three interleaved runs, the tool WITHOUT this arm -> this tool:
#                                               this box              a box that derives nothing
#   entry      docs/README.md, 47 KB           4.6 ->  5.4  1.19x      2.6 ->  3.9  1.49x
#   converted  net/http/serve_test.cs, 408 KB 20.5 -> 23.8  1.16x     16.7 -> 20.2  1.21x
#   entry      3,000 short lines, 108 KB       9.8 -> 11.8  1.21x      7.0 ->  9.7  1.38x
#   entry      one 20,000-character line       3.8 ->  4.5  1.18x      1.9 ->  2.7  1.42x
# AND ON A REAL GENERIC LINUX BOX -- Ubuntu 20.04 under WSL, gawk 5.0.1, Python 3.8.5 (its `python` is
# 2.7 and fails the probe, so python3 is the scanner), the fastest of three runs, a box that derives
# nothing:
#   entry      docs/README.md                  1.0 ->  1.3  1.34x   (15.7 before `ws = ws - seen`)
#   converted  net/http/serve_test.cs          9.3 ->  7.1  0.76x   (perl forced: 7.2)
# That last ratio is under one because of LC_ALL=C: the tool without this arm ran that gawk in the
# box's UTF-8 locale. THE ADDED SECONDS on Git Bash are about the same on both boxes -- 0.7 to 3.4
# here, 0.8 to 3.5 there. The generic box's RATIO on a small input is the larger one because its
# seconds WITHOUT this arm are the smaller: holding no literal, that tool never entered the token
# passes at all (they run only when a literal is installed). THE SCAN'S OWN SHARE, read from a
# phase-stamped copy on serve_test.cs: the buffer awk 0.2 s, the scanner 1.3 s (python) or 1.6 s
# (perl) for 320,749 distinct windows. The rest is the candidate route and -- where the text itself
# supplies a word, as the README's handle does -- the token passes, which then have a literal to
# read for. WHAT PAID FOR PART OF IT, so the numbers above are not read as the arm's cost alone: the
# same day the tool stopped running a chmod after each temp file (umask), stopped asking the hash
# list twice about each derived literal, and built two characters without a fork. Those are savings
# in the tool around the arm, not in the arm.
#
# -------------------------------------------------------------------------------------------------
# THE AWK PROBE -- THE TOOL ASKS ITS OWN AWK A KNOWN-ANSWER QUESTION FIRST (2026-10-06)
#
# Every arm is an ERE read by WHATEVER awk the box has, and an awk that does not implement a construct
# an arm is spelled with does not fail: the arm matches nothing, prints occ=0 exactly as it does on a
# text that carries none, and the census reads CLEAN. MEASURED 2026-10-06: mawk 1.3.4 20200120 -- the
# mawk an Ubuntu 20.04 image ships -- reads a braced interval expression as LITERAL TEXT, and the ipv4
# candidate matcher was spelled with one. On that awk no address was ever a hit: of the 81 quads in 78
# real posts it counted none, `entry` over a bare private-range address exited 0, and 54 self-test
# cases were red there and green on gawk. A gate inert on a generic box -- by its awk this time, not by
# its account. The arm is respelled (PORTABILITY, in the definition); this probe is what makes the
# NEXT such spelling a refusal instead of a clean read.
#
# WHAT IT ASKS. Before any mode reads a text -- entry, subject, converted, tree -- the census awk is run
# once over a PROBE TEXT, with this tool's own definition: for every REFUSING structural arm one line
# it must hit and one it must not (for ipv4: an address; and three octets, and four with the last out
# of range), a token arm through each of the three passes, each admit set the strict modes consult,
# and -- read in delta -- each ipv4 context rule. The table is beside idc_awk_probe. THE ANSWER IS
# EXACT: arm, pass, line and the fingerprint of the decision token for every hit, and every exclusion
# with its count -- so an awk whose match() reports another EXTENT answers wrong as surely as one
# whose arm is dead.
#
# WHAT A WRONG ANSWER DOES. REFUSED(2), naming the awk by ITS OWN VERSION LINE -- never by its path,
# which on this fleet is a «profile-path» -- and the first arm that answered wrong. An awk that cannot
# compile the definition at all is the same refusal. idc_census asks again if nothing has asked in its
# process, so a mode added later cannot reach a census around the probe. Every census prints the
# probe's line beside the others. The self-test (section P) asks it of this box's awk and then, mode
# by mode, of a stand-in whose address arm is dead: the same bytes, refused by the one, and by the
# other not read at all.
#
# WHAT IT CANNOT KNOW. It certifies the awk against the constructs the arms use TODAY. An arm added to
# the definition gets a line in the probe in the same change, or the probe is silent about that arm.
# What it does not ask at all is listed beside idc_awk_probe.
# AND IT ASKS NO LINE THAT CARRIES A NUL BYTE, because no awk is handed one. MEASURED 2026-10-06: mawk
# 1.3.4 20200120 passed this probe and read a line with a NUL in it as memory that was not the line's
# -- a whole denied word after the NUL, UTF-16 text and an address after a NUL read CLEAN there and
# REFUSED under gawk. The answer is not a probe line that refuses mawk; it is that idc_census gives
# every awk a copy of the text in which a NUL is \001 (see there), and the self-test reads the three
# shapes on whatever awk the box has (D2, p1): green on gawk before that copy and red on that mawk.
#
# MEASURED 2026-10-06, the same answer from all three: gawk 5.0.0 (Git Bash on Windows), and gawk 5.0.1
# and mawk 1.3.4 20200120 (Ubuntu 20.04). A busybox awk (1.30.1) cannot compile the share arm's
# expression; the census awk's exit code already refused it, and the probe now refuses it by name. No
# other awk has been asked: a box with another one learns its answer from the self-test's first case.
#
# EXIT CODES
#   0  clean
#   1  refused -- one or more hits (for `tree`, one or more ADDED hits)
#   2  misuse, or the instrument could not measure. A gate that cannot measure does not pass.
#   3  self-test failed
#
# -------------------------------------------------------------------------------------------------
# THREE PASSES (R's shape, kept)
#   PASS 1  every arm, per line. Token candidates are tokenised as the Go guard tokenises them.
#   PASS 2  every arm, over each ADJACENT LINE PAIR joined with the whitespace at the break removed
#           (trailing on the left, indentation on the right). A line-anchored census cannot see a
#           token WRAPPED ACROSS A LINE BREAK, and joining on a bare newline alone still misses the
#           two commonest real shapes -- an indented continuation and a trailing space at the break.
#           Reported only when the match SPANS the join, so PASS 2 never re-reports PASS 1.
#   PASS 3  the run-time token arms ONLY, as a bare substring over an ALPHANUMERICS-ONLY reduction
#           of the line and of the joined pair. This catches a token broken by separators INSIDE a
#           component, which run-and-component tokenising cannot see. It is sound for pure
#           alphanumeric tokens and would over-fire wildly on the path arms, so it is scoped to the
#           token arms and to tokens of 4+ characters, and it skips what PASS 1 or 2 already found.
#
# INSTRUMENT FAILURE IS A REFUSAL, NEVER A CLEAN READ. R's tool gets this from `grep rc > 1`; there
# is no grep in this pipeline, so the same property is carried by four checks: the patterns file must
# be readable and yield 1+ arms; the input must be readable; awk must exit 0; and the arm count awk
# reports must equal the arm count this script counts independently from the same file. Any of them
# failing exits 2. A guard that cannot read its input PASSES it -- that is how a placeholder arm once
# failed open -- so none of these is allowed to be silent. AND A FIFTH, AHEAD OF THEM ALL (2026-10-06):
# THE AWK PROBE above. Each of the four notices an awk that FAILED; none of them notices an awk that
# ran, exited 0, counted every arm and matched nothing. AND A SIXTH, the same day: the census awk
# reports how many LINES it was given, and a census that read none of an input that is not empty is
# refused -- an awk handed a file name it took for an assignment exits 0 with every arm at zero.
#
# No `set -o pipefail` and no `| grep -q` anywhere: awk writes its report, its keys and its status to
# FILES and this script reads them with redirects, so there is no pipeline whose exit code could be
# read from the wrong end. Every count is tested on its RAW value.
# =================================================================================================

set -u

# BYTES, NEVER CHARACTERS, ON EVERY BOX (2026-10-06). Every program this tool runs -- each awk, tr, the
# hasher, the scanner, and the shell's own ${#word} -- reads the text in the C locale, whatever locale
# the caller exported. The window scanner always did; the awks read CHARACTERS in whatever locale the
# box had, and where the two differ the verdict depended on the box. MEASURED: a synthetic denied word
# broken by one double-byte character whose trail byte is an ASCII letter REFUSED on a box holding the
# word under a Shift-JIS, GBK or Big5 locale (its awk dropped the pair whole, PASS 3 hit) and read
# CLEAN on a box that holds nothing (the scanner kept the trail byte as a letter). See WHAT IS STILL
# NOT FOUND (3) in the header. It also makes a length the BYTE length the Go guard's len() is.
LC_ALL=C
export LC_ALL
# Every file this tool creates is its own to read and nobody else's, from the moment it exists. One
# setting, and no chmod process after each file: one chmod measured 38 ms on Git Bash (the i7), and the
# steps of TEXT_DENIED and the awk probe ran six of them on every census.
umask 077

IDC_PROG="coord-identifier-census.sh"
IDC_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
IDC_SELF="$IDC_DIR/$(basename -- "$0")"
IDC_PATTERNS="${IDC_PATTERNS:-$IDC_DIR/coord-identifier-patterns.txt}"
IDC_HASHES="${IDC_HASHES:-$IDC_DIR/coord-identifier-hashes.txt}"
IDC_UNMASK=0
IDC_CONVERTED=0               # set by `converted` mode and by nothing else.
IDC_FIXTURE=0                 # set PER FILE by `converted` mode from idc_is_upstream_fixture; 0 everywhere else.
IDC_SHORT="${IDC_SHORT:-0}"   # self-test forcing hook only; see ipv4Extent. Never set in normal use.
IDC_TMP=""

idc_cleanup() {
    if [ -n "$IDC_TMP" ] && [ -d "$IDC_TMP" ]; then rm -rf -- "$IDC_TMP"; fi
}
trap idc_cleanup EXIT INT TERM

IDC_TMP="$(mktemp -d 2>/dev/null)"
if [ -z "$IDC_TMP" ] || [ ! -d "$IDC_TMP" ]; then
    echo "REFUSED(2): $IDC_PROG could not create a temp directory -- it cannot measure, so it does not pass"
    exit 2
fi
chmod 700 -- "$IDC_TMP" 2>/dev/null

idc_misuse() {
    echo "REFUSED(2): $1"
    echo "usage: $IDC_PROG [--unmask] entry <file> | converted <file>... | subject <string> | tree <file> <baseline-sha> | selftest"
    exit 2
}

# -------------------------------------------------------------------------------------------------
# RUN-TIME TOKEN SET. Read from the local box, written to a 600 temp file, never printed, never
# committed. The summary reports SOURCE COUNTS and HASH MATCHES, never a value.
# -------------------------------------------------------------------------------------------------
IDC_TOKFILE="$IDC_TMP/tok"
IDC_TOKSUMMARY=""
IDC_TOKFILE_PRESENT="no"
IDC_HASHSUMMARY=""
# The ADMITTED PUBLIC HANDLES, resolved to literals by the same INVERTED comparison the denied rows
# use: this box derives a handful of candidate handles, hashes those, and keeps the ones the shared
# list already admits. A handle this box cannot derive is not an admit here -- a number on the record
# (admits=N matched=M) and never an assumption. Written to the same 600 temp file class, never
# printed, never committed.
IDC_ADMITFILE="$IDC_TMP/admit"
IDC_ADMITSUMMARY=""

idc_resolve_tokenfile() {
    # The never-push token file. Located, never printed.
    if [ -n "${IDC_TOKEN_FILE:-}" ] && [ -f "${IDC_TOKEN_FILE:-}" ]; then echo "$IDC_TOKEN_FILE"; return 0; fi
    if [ -f "$IDC_DIR/coord-identifier-tokens.local" ]; then echo "$IDC_DIR/coord-identifier-tokens.local"; return 0; fi
    if [ -n "${HOME:-}" ] && [ -f "${HOME:-}/.claude/coord-identifier-tokens" ]; then echo "$HOME/.claude/coord-identifier-tokens"; return 0; fi
    echo ""
}

# Hash one literal exactly as the Go guard does: SHA-256 of the LOWER-CASED token, hex, salt-free.
idc_hash() {
    local low="" out=""
    low="$(printf '%s' "$1" | tr 'A-Z' 'a-z')"
    out="$(printf '%s' "$low" | sha256sum 2>/dev/null)"
    printf '%s' "${out%% *}"
}

IDC_HASH_HITS=0
# How many times the two authoritative readers below were ASKED, counted where they are entered and
# nowhere else. TEXT_DENIED reports its own share as `lookups=` -- one per prefilter positive -- so a
# reader that is no longer asked, or one asked about every candidate again (22 ms each: the cost R5
# of the ruling removed), is a number on the census line and a red self-test case, not a slow day.
IDC_ROW_ASKS=0
idc_denied_row() {
    # 0 if the (len, hash) pair is a DENIED row in the shared hash list. The ONE reader of that
    # section: idc_hash_known asks it about a literal it hashed itself, and TEXT_DENIED asks it about
    # a digest that came back from the batch hasher. Two askers, one definition of a row.
    local len="$1" h="$2" f1="" f2=""
    IDC_ROW_ASKS=$((IDC_ROW_ASKS + 1))
    [ -f "$IDC_HASHES" ] || return 1
    [ -n "$h" ] || return 1
    while IFS=$'\t' read -r f1 f2 _rest || [ -n "$f1" ]; do
        f1="${f1%$'\r'}"; f2="${f2%$'\r'}"
        case "$f1" in '#'*) continue ;; '') continue ;; esac
        case "$f1" in 'ADMIT') continue ;; esac
        if [ "$f1" = "$len" ] && [ "$f2" = "$h" ]; then return 0; fi
    done < "$IDC_HASHES"
    return 1
}
idc_hash_known() {
    # 0 if the literal's (len, hash) pair is a row in the shared hash list.
    idc_denied_row "${#1}" "$(idc_hash "$1")"
}

idc_admit_row() {
    # 0 if the (len, hash) pair is an ADMIT row in the shared hash list. Separate reader from
    # idc_denied_row on purpose: one function that answered both questions from one row shape is one
    # typo away from admitting a denied token.
    local len="$1" h="$2" f1="" f2="" f3=""
    IDC_ROW_ASKS=$((IDC_ROW_ASKS + 1))
    [ -f "$IDC_HASHES" ] || return 1
    [ -n "$h" ] || return 1
    while IFS=$'\t' read -r f1 f2 f3 _rest || [ -n "$f1" ]; do
        f1="${f1%$'\r'}"; f2="${f2%$'\r'}"; f3="${f3%$'\r'}"
        case "$f1" in 'ADMIT') : ;; *) continue ;; esac
        if [ "$f2" = "$len" ] && [ "$f3" = "$h" ]; then return 0; fi
    done < "$IDC_HASHES"
    return 1
}
idc_admit_known() {
    # 0 if the literal's (len, hash) pair is an ADMIT row in the shared hash list.
    idc_admit_row "${#1}" "$(idc_hash "$1")"
}

# A DERIVED token is a GUESS about what this box is called. On a container it is an ordinary word --
# `root`, `user`, `ubuntu` -- and C2 measured 4348 pre-existing hits from RUNTIME_ACCOUNT and
# RUNTIME_OWNERNAME on its box from exactly that. An arm that fires on an English word is not a
# security arm, it is a denial of service against its own operator.
#
# So a derived token is admitted only if it clears THREE bars, and the third is the real one:
#   length >= 5          a four-character derivation is a word more often than a name
#   not stop-listed      the container and CI account names, named rather than inferred
#   IN THE DENIED SET    its hash is a row in coord-identifier-hashes.txt, or the literal is in the
#                        local never-push token file. THIS is the discriminator: the denied set is
#                        the authority on which names are forbidden, and a derivation that is not in
#                        it is a name this fleet never denied.
# Otherwise the arm is INERT and says so by name. An inert arm is reported, never silent -- a zero
# that nobody can tell from an arm that was never wired is the shape this whole instrument exists to
# avoid.
#
# The TOKENFILE source is exempt from the third bar: those literals ARE the local denied set.
IDC_STOPLIST=" root user users admin home ubuntu debian guest default runner agent claude coord coordinator "
IDC_INERT=""

idc_token_in_file() {
    local lit="$1" tf="$2" line=""
    [ -n "$tf" ] || return 1
    [ -f "$tf" ] || return 1
    while IFS= read -r line || [ -n "$line" ]; do
        line="${line%$'\r'}"
        case "$line" in '#'*) continue ;; '') continue ;; esac
        if [ "$line" = "$lit" ]; then return 0; fi
    done < "$tf"
    return 1
}

idc_add_token() {
    # $1 = source arm name, $2 = literal, $3 = minimum length
    # known: the literal's hash IS a denied row -- asked ONCE and kept. The third bar below used to ask
    # the same question again: two forks, a tr and a sha256sum for an answer already in hand, measured
    # 0.18 s a derived literal on Git Bash (the i7), on every census.
    local lit="$2" low="" known=1
    if [ -z "$lit" ]; then return 1; fi
    if [ "${#lit}" -lt "$3" ]; then return 1; fi
    if idc_hash_known "$lit"; then known=0; IDC_HASH_HITS=$((IDC_HASH_HITS + 1)); fi
    case "$1" in
        TOKENFILE) : ;;                     # the local denied set itself; the bars below do not apply
        *)
            low="$(printf '%s' "$lit" | tr 'A-Z' 'a-z')"
            if [ "${#lit}" -lt 5 ]; then
                IDC_INERT="$IDC_INERT  $1: derived token under 5 characters, arm inert\n"; return 1
            fi
            case "$IDC_STOPLIST" in
                *" $low "*)
                    IDC_INERT="$IDC_INERT  $1: derived token is a stop-listed generic account name, arm inert\n"; return 1 ;;
            esac
            if [ "$known" -ne 0 ] && ! idc_token_in_file "$lit" "$IDC_TF_PATH"; then
                IDC_INERT="$IDC_INERT  $1: token not in the denied set, arm inert\n"; return 1
            fi
            ;;
    esac
    printf '%s\t%s\n' "$1" "$lit" >> "$IDC_TOKFILE"
    return 0
}

# THE PUBLIC-HANDLE ADMIT SET, derived and then FILTERED BY THE SHARED LIST -- the same inversion
# idc_add_token's third bar uses, and for the same reason: a derivation is a GUESS about what this
# box is called, and the authority on which words are the owner's ruled public handles is the ADMIT
# section of coord-identifier-hashes.txt, generated from the Go guard's fleetPublicHandles.
#
# A candidate that is not an ADMIT row is DROPPED, silently and by construction -- it is not that the
# admit is inert, it is that the word was never ruled a handle. The count is reported either way, so
# `admits=N matched=0` on a box that cannot derive them reads as "the arms refuse exactly as they did
# before", which is the safe direction and is a number rather than an inference.
#
# The candidates, and there are only three shapes because a wider guess is a wider admit:
#   the local part of `git config --get user.email`               -- the published address form
#   every alphabetic piece of `git config --get user.name`, joined -- the organisation-handle shape
#   the first letter of the FIRST piece + the LAST piece           -- the work-mail handle shape
idc_build_admits() {
    : > "$IDC_ADMITFILE"
    chmod 600 -- "$IDC_ADMITFILE" 2>/dev/null
    local nMatch=0 nRows=0 cand="" nm="" piece="" first="" last="" joined="" line="" low=""

    if [ -f "$IDC_HASHES" ]; then
        while IFS= read -r line || [ -n "$line" ]; do
            line="${line%$'\r'}"
            case "$line" in 'ADMIT	'*) nRows=$((nRows + 1)) ;; esac
        done < "$IDC_HASHES"
    fi

    if [ -n "${IDC_TEST_ADMITS:-}" ]; then
        # SELF-TEST OVERRIDE: synthetic handles, so a control never depends on this box's real name
        # and never coincides with an arm it was not meant to prove.
        for cand in $IDC_TEST_ADMITS; do
            printf '%s\n' "$(printf '%s' "$cand" | tr 'A-Z' 'a-z')" >> "$IDC_ADMITFILE"
            nMatch=$((nMatch + 1))
        done
        IDC_ADMITSUMMARY="admits=$nRows matched=$nMatch (synthetic admit set)"
        return 0
    fi

    idc_admit_try() {
        local lit="" low2=""
        lit="$1"
        [ -n "$lit" ] || return 0
        [ "${#lit}" -ge 5 ] || return 0
        low2="$(printf '%s' "$lit" | tr 'A-Z' 'a-z')"
        idc_admit_known "$low2" || return 0
        if ! grep -qxF -- "$low2" "$IDC_ADMITFILE" 2>/dev/null; then
            printf '%s\n' "$low2" >> "$IDC_ADMITFILE"
            nMatch=$((nMatch + 1))
        fi
        return 0
    }

    cand="$(git config --get user.email 2>/dev/null)"
    cand="${cand%%@*}"
    idc_admit_try "$cand"

    nm="$(git config --get user.name 2>/dev/null)"
    if [ -n "$nm" ]; then
        nm="$(printf '%s' "$nm" | tr -c 'A-Za-z' ' ')"
        first=""; last=""; joined=""
        for piece in $nm; do
            if [ -z "$first" ]; then first="$piece"; fi
            last="$piece"
            if [ "${#piece}" -ge 2 ]; then joined="$joined$piece"; fi
        done
        idc_admit_try "$joined"
        if [ -n "$first" ] && [ -n "$last" ] && [ "$first" != "$last" ]; then
            idc_admit_try "$(printf '%s' "$first" | cut -c1)$last"
        fi
    fi

    IDC_ADMITSUMMARY="admits=$nRows matched=$nMatch unmatched=$((nRows - nMatch))"
    return 0
}

idc_build_tokens() {
    : > "$IDC_TOKFILE"
    chmod 600 -- "$IDC_TOKFILE" 2>/dev/null
    IDC_HASH_HITS=0
    IDC_TOKFILE_PRESENT="no"
    IDC_INERT=""
    IDC_TF_PATH="$(idc_resolve_tokenfile)"
    local nTF=0 nAC=0 nMA=0 nOW=0 nRows=0 skipped="" tf="" line="" nm="" piece=""

    nRows=0
    if [ -f "$IDC_HASHES" ]; then
        while IFS= read -r line || [ -n "$line" ]; do
            case "$line" in '#'*) continue ;; '') continue ;; esac
            # The ADMIT section is a DIFFERENT list, counted by idc_build_admits. Folding it into
            # `hashes=N` would inflate the denied denominator and make `unmatched` unreadable.
            case "$line" in 'ADMIT	'*) continue ;; esac
            case "$line" in *"	"*) nRows=$((nRows + 1)) ;; esac
        done < "$IDC_HASHES"
    fi

    if [ -n "${IDC_TEST_TOKENS:-}" ]; then
        # SELF-TEST OVERRIDE: synthetic tokens, so a control never depends on this box's real name
        # and never coincides with an arm it was not meant to prove.
        #
        # ARM=literal installs a synthetic literal UNDER A NAMED ARM, bypassing the three bars exactly
        # as the bare form does. It is what lets a control prove an arm-SCOPED property -- the
        # public-handle admit reaches RUNTIME_ACCOUNT and RUNTIME_OWNERNAME and NOT TOKENFILE --
        # without the control depending on this box's real account name. The bare form is unchanged,
        # so every control written before this still installs under TOKENFILE and reads as it did.
        local nSA=0 nSM=0 nSO=0
        for piece in $IDC_TEST_TOKENS; do
            case "$piece" in
                RUNTIME_ACCOUNT=*|RUNTIME_MACHINE=*|RUNTIME_OWNERNAME=*|TOKENFILE=*)
                    printf '%s\t%s\n' "${piece%%=*}" "$(printf '%s' "${piece#*=}" | tr 'A-Z' 'a-z')" >> "$IDC_TOKFILE"
                    case "${piece%%=*}" in
                        RUNTIME_ACCOUNT)   nSA=$((nSA + 1)) ;;
                        RUNTIME_MACHINE)   nSM=$((nSM + 1)) ;;
                        RUNTIME_OWNERNAME) nSO=$((nSO + 1)) ;;
                        *)                 nTF=$((nTF + 1)) ;;
                    esac ;;
                *)
                    if idc_add_token "TOKENFILE" "$piece" 4; then nTF=$((nTF + 1)); fi ;;
            esac
        done
        IDC_TOKFILE_PRESENT="synthetic"
        IDC_TOKSUMMARY="TOKENFILE=$nTF(synthetic) RUNTIME_ACCOUNT=$nSA RUNTIME_MACHINE=$nSM RUNTIME_OWNERNAME=$nSO"
        IDC_HASHSUMMARY="hashes=$nRows matched=0 (synthetic token set)"
        idc_build_admits
        return 0
    fi

    tf="$IDC_TF_PATH"
    if [ -n "$tf" ]; then
        IDC_TOKFILE_PRESENT="yes"
        while IFS= read -r line || [ -n "$line" ]; do
            line="${line%$'\r'}"
            case "$line" in '#'*) continue ;; '') continue ;; esac
            if idc_add_token "TOKENFILE" "$line" 4; then nTF=$((nTF + 1)); fi
        done < "$tf"
    fi

    # account: basename $HOME, then $USERNAME, then $USER. Floor 4 -- a shorter derivation is
    # ABORTED and NAMED, never installed as a two-character detector.
    nm=""
    if [ -n "${IDC_TEST_ACCOUNT:-}" ]; then nm="$IDC_TEST_ACCOUNT"     # self-test only
    elif [ -n "${HOME:-}" ]; then nm="$(basename -- "$HOME")"; fi
    if [ -z "$nm" ]; then nm="${USERNAME:-}"; fi
    if [ -z "$nm" ]; then nm="${USER:-}"; fi
    if [ -n "$nm" ] && [ "${#nm}" -ge 4 ]; then
        if idc_add_token "RUNTIME_ACCOUNT" "$nm" 4; then nAC=$((nAC + 1)); fi
    else
        skipped="$skipped RUNTIME_ACCOUNT(derivation empty or under 4 chars)"
    fi
    if [ -z "${IDC_TEST_ACCOUNT:-}" ] && [ -n "${USERNAME:-}" ] && [ "${USERNAME:-}" != "$nm" ]; then
        if idc_add_token "RUNTIME_ACCOUNT" "$USERNAME" 4; then nAC=$((nAC + 1)); fi
    fi

    # machine: $COMPUTERNAME, then hostname. Same floor.
    nm="${COMPUTERNAME:-}"
    if [ -z "$nm" ]; then nm="$(hostname 2>/dev/null)"; fi
    if [ -n "$nm" ] && [ "${#nm}" -ge 4 ]; then
        if idc_add_token "RUNTIME_MACHINE" "$nm" 4; then nMA=$((nMA + 1)); fi
    else
        skipped="$skipped RUNTIME_MACHINE(derivation empty or under 4 chars)"
    fi

    # owner name in ordinary prose: git user.name split on non-alphabetics, pieces of 3+ kept.
    nm="$(git config --get user.name 2>/dev/null)"
    if [ -n "$nm" ]; then
        nm="$(printf '%s' "$nm" | tr -c 'A-Za-z' ' ')"
        for piece in $nm; do
            if idc_add_token "RUNTIME_OWNERNAME" "$piece" 3; then nOW=$((nOW + 1)); fi
        done
    fi
    if [ "$nOW" -eq 0 ]; then skipped="$skipped RUNTIME_OWNERNAME(git user.name empty or no piece of 3+ chars)"; fi

    IDC_TOKSUMMARY="TOKENFILE=$nTF RUNTIME_ACCOUNT=$nAC RUNTIME_MACHINE=$nMA RUNTIME_OWNERNAME=$nOW"
    if [ -n "$skipped" ]; then IDC_TOKSUMMARY="$IDC_TOKSUMMARY  SKIPPED:$skipped"; fi
    IDC_HASHSUMMARY="hashes=$nRows matched=$IDC_HASH_HITS unmatched=$((nRows - IDC_HASH_HITS))"
    idc_build_admits
    return 0
}

# -------------------------------------------------------------------------------------------------
# TEXT_DENIED. The header says what it is; this is the mechanism. Four steps, each of which can
# refuse and none of which can pass by having read nothing:
#   1. the hash list is read ONCE: every row is checked for shape on the way, and the denied and the
#      admit rows are kept in memory as two length lists and two (length:digest) sets;
#   2. THE WINDOW SCAN (idc_scan_text, below): the census awk writes the buffers its three passes
#      search, and one scanner process hashes every window of every denied length over them and
#      names the words it found;
#   3. one awk writes the de-duplicated candidates of the input (the tokenisation is scanTokens'
#      plus the spans of adjacent components, the join is PASS 2's, the admit words are admitWord's
#      and dottedHandleOK's) and takes the scan's words in beside them, so a word found twice is one
#      candidate and is installed once;
#   4. one process hashes them all, the known-answer probe FIRST; each digest is looked up in the
#      in-memory sets, and only a POSITIVE is asked of idc_denied_row / idc_admit_row -- the readers
#      the run-time arms use, and still the authority on what a row is. (Asking them about every
#      candidate re-read the hash list twice per candidate, about 22 ms each on Git Bash: 3,000
#      short lines took 144 s against 12.6 s for the tool without this arm.)
# What it finds goes to two 600 temp files the census awk reads beside the run-time ones, rebuilt for
# EVERY census, so a literal found in one file of a `converted` run is never carried into the next.
# -------------------------------------------------------------------------------------------------
IDC_TXTARM="TEXT_DENIED"
IDC_TXTTOK="$IDC_TMP/tok.text"
IDC_TXTADM="$IDC_TMP/admit.text"
IDC_TXTSUMMARY=""
IDC_TXTN=0
IDC_HB_COUNT=0
# SHA-256 of the three bytes `abc` -- the standard's own first test vector. It is the first candidate
# of every batch, so "the hasher ran and is a SHA-256" is measured on every census, never assumed.
IDC_KAT_IN="abc"
IDC_KAT_OUT="ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"

idc_hash_batch() {
    # $1 hasher, $2 candidates (one lowercased literal per line), $3 digests out (one per line, same
    # order). ONE hashing process for the whole file -- a spawn per candidate is ~60 ms on Windows.
    # Returns 0 only if it produced exactly one well-formed digest per candidate AND the probe's is
    # the known answer. IDC_HB_COUNT is the number of digests THE HASHER returned, counted from its
    # own output and never from the candidate file: it is what `hashed=` on the summary line reports.
    local hasher="$1" in="$2" out="$3" d="" n=0 m=0 line="" first=""
    IDC_HB_COUNT=0
    : > "$out"
    case "$hasher" in
        python|python3)
            command -v "$hasher" >/dev/null 2>&1 || return 1
            "$hasher" -c 'import sys, hashlib
for w in sys.stdin.buffer.read().split(b"\n"):
    w = w.rstrip(b"\r")
    if w: sys.stdout.write(hashlib.sha256(w).hexdigest() + "\n")' < "$in" > "$out" 2>/dev/null || return 1
            ;;
        sha256sum)
            # The shell route: no interpreter, only the sha256sum idc_hash already needs. One file per
            # candidate, named by its line number so the glob order IS the candidate order.
            command -v sha256sum >/dev/null 2>&1 || return 1
            IDC_TXTN=$((IDC_TXTN + 1))
            d="$IDC_TMP/hb.$IDC_TXTN"
            mkdir -- "$d" 2>/dev/null || return 1
            chmod 700 -- "$d" 2>/dev/null
            awk -v D="$d" '{ f = sprintf("%s/%07d", D, NR); printf "%s", $0 > f; close(f) }' "$in" || return 1
            ( cd -- "$d" && printf '%s\n' [0-9]* | xargs sha256sum -- ) > "$out.raw" 2>/dev/null
            while IFS= read -r line || [ -n "$line" ]; do
                line="${line#\\}"; printf '%s\n' "${line%% *}"
            done < "$out.raw" > "$out"
            ;;
        *) return 1 ;;
    esac
    while IFS= read -r line || [ -n "$line" ]; do n=$((n + 1)); done < "$in"
    while IFS= read -r line || [ -n "$line" ]; do
        line="${line%$'\r'}"
        m=$((m + 1))
        if [ "$m" -eq 1 ]; then first="$line"; fi
        if [ "${#line}" -ne 64 ]; then return 1; fi
        case "$line" in *[!0-9a-f]*) return 1 ;; esac
    done < "$out"
    IDC_HB_COUNT="$m"
    [ "$n" -ge 1 ] && [ "$n" -eq "$m" ] && [ "$first" = "$IDC_KAT_OUT" ]
}

# -------------------------------------------------------------------------------------------------
# THE WINDOW SCAN (2026-10-06). The candidate route above finds a denied word only where it STANDS
# ALONE -- a whole run, a whole component, a span of components. Glued inside a longer word or broken
# by a character that is not its own, it is no candidate, and such a text read CLEAN on a box that
# derives nothing and REFUSED on a box that derives the word: the verdict depended on who was asking.
# The scan closes that from the text's side. It hashes EVERY WINDOW of every denied length over the
# buffers the three passes search, so a word is DISCOVERED wherever PASS 1, 2 or 3 would hit it if
# it were installed as a literal -- and it is then installed, and they do.
#
# THE BUFFERS ARE NOT RE-DERIVED HERE. The census awk writes them itself, in its dump mode, through
# tokenBuffer -- the one function its token passes read -- so the scanner cannot be reading a line
# the passes do not. Each buffer line is prefixed `=`, each row line `#`, so no text can pass itself
# off as a row line.
#
# ONE PROCESS, TWO SPELLINGS OF ONE ALGORITHM (python, perl with Digest::SHA). Bytes, never
# characters; ASCII lower-casing only (a no-op on a buffer awk already lower-cased, and what makes
# the scanner right on one that was not). For each `=` line under the rows of the last `#` line:
# every maximal [a-z0-9._-] run, then the alphanumerics-only reduction; every window of every row
# length; each DISTINCT window hashed once; the digest looked up IN the process. A buffer longer than
# 64 KiB is walked in pieces that overlap by the longest row length less one, so no window is lost at
# a seam, and the seen-set is dropped past 500,000 windows, so memory is bounded and the only cost of
# dropping it is hashing a window twice. It prints, per `#` section, its own count of windows hashed
# and buffers read, then the distinct words it found, sorted. Nothing else leaves it.
#
# THE PROBE IS A SECTION OF ITS OWN, FIRST, ON EVERY SCAN -- a probe line and two probe rows the
# tool holds in memory, never in the hash list. One probe word carries separators and stands glued
# inside a longer run, so only a window over a RUN finds it; the other is broken by a space, a slash
# and punctuation, so only a window over the REDUCTION finds it. The known answer is the exact window
# count and both words: a scanner that cannot hash, cannot window (either way), cannot look a digest
# up, or miscounts by one is not a scanner. And the second section's buffer count must equal the
# count the census awk says it wrote, so a scanner that stopped reading early is not one either.
# -------------------------------------------------------------------------------------------------
IDC_WPROBE_ROWS="11:ae1a1a58e2afcceea1a6e239df83f205cc3200805b496d26768279f9d417a833 9:f1bf08dde263b4f87cc60f028b10f3afdc68d50c551d43dde6f26f5561cf394d"
IDC_WPROBE_LINE="xidc-probe.ax ~id!c pr/ob,eb~"
IDC_WPROBE_HEAD="windows=29 buffers=1"
IDC_WPROBE_A="idc-probe.a"
IDC_WPROBE_B="idcprobeb"
IDC_WS_WINDOWS=""
IDC_WS_BUFFERS=""

# No backslash and no apostrophe in the python: it travels as one -c argument through a Windows
# command line on Git Bash. `read -d ""` and not $(cat <<...): bash 3.2 mis-parses a here-document
# inside a command substitution. read returns non-zero at end of input by design; the text is set.
#
# `ws = ws - seen`, NEVER `ws -= seen`. A CPython before 3.9 walks the RIGHT operand of an in-place
# set difference, so each call cost the size of the whole seen-set and the scan was quadratic in the
# text. MEASURED 2026-10-06 on Python 3.8.5 (Ubuntu 20.04, the first scanner that answers there):
# 2,000 calls against a 300,000-entry set took 3.5 s in place and under a millisecond spelled as a
# difference; `entry docs/README.md` took 15.7 s where the tool without this arm takes 1.0 s, and
# takes 1.2 to 1.3 s respelled, the same 109,786 windows. Python 3.11 hid it (0.000 s for both spellings).
# The binary difference walks the LEFT operand on every version.
IFS= read -r -d '' IDC_SCAN_PY <<'IDCPYEOF'
import sys, hashlib
NL = bytes([10])
RUNCH = b"abcdefghijklmnopqrstuvwxyz0123456789._-"
ALNUM = RUNCH[:36]
TOSP = bytes([c if c in RUNCH else 32 for c in range(256)])
DROP = bytes([c for c in range(256) if c not in ALNUM])
CHUNK = 65536
CAP = 500000
sha = hashlib.sha256
out = sys.stdout.buffer
want = {}; lens = []; seen = set(); found = set(); nwin = 0; nbuf = 0; opened = False
def flush():
    out.write(b"windows=%d buffers=%d" % (nwin, nbuf) + NL)
    for w in sorted(found): out.write(w + NL)
def scan(s):
    global nwin
    L = len(s)
    if not lens or L < lens[0]: return
    top = lens[-1]
    for o in range(0, L, CHUNK):
        piece = s[o:o + CHUNK + top - 1]
        P = len(piece)
        for n in lens:
            if n > P: break
            ws = {piece[i:i + n] for i in range(P - n + 1)}
            ws = ws - seen
            if not ws: continue
            seen.update(ws); nwin += len(ws)
            hit = want[n]
            for w in ws:
                if sha(w).digest() in hit: found.add(w)
        if len(seen) > CAP: seen.clear()
data = sys.stdin.buffer.read().split(NL)
if data and data[-1] == b"": data.pop()
for line in data:
    tag = line[:1]
    if tag == b"#":
        if opened: flush()
        opened = True
        want = {}; seen = set(); found = set(); nwin = 0; nbuf = 0
        for p in line[1:].split():
            n, sep, h = p.partition(b":")
            if len(h) != 64: sys.exit(3)
            want.setdefault(int(n), set()).add(bytes.fromhex(h.decode("ascii")))
        lens = sorted(want)
    elif tag == b"=" and opened:
        nbuf += 1
        s = line[1:].lower()
        for run in s.translate(TOSP).split(): scan(run)
        scan(s.translate(None, DROP))
    else:
        sys.exit(3)
if opened: flush()
IDCPYEOF

IFS= read -r -d '' IDC_SCAN_PL <<'IDCPLEOF'
use strict; use warnings; use Digest::SHA qw(sha256_hex);
binmode(STDIN); binmode(STDOUT);
my (%want, @lens, %seen, %found);
my ($nwin, $nbuf, $opened) = (0, 0, 0);
my ($CHUNK, $CAP) = (65536, 500000);
sub flush_section {
    print "windows=$nwin buffers=$nbuf\n";
    print "$_\n" for sort keys %found;
}
sub scan {
    my $L = length($_[0]);
    return if !@lens || $L < $lens[0];
    my $top = $lens[-1];
    for (my $o = 0; $o < $L; $o += $CHUNK) {
        my $piece = substr($_[0], $o, $CHUNK + $top - 1);
        my $P = length($piece);
        for my $n (@lens) {
            last if $n > $P;
            for (my $i = 0; $i + $n <= $P; $i++) {
                my $w = substr($piece, $i, $n);
                next if $seen{$w}++;
                $nwin++;
                $found{$w} = 1 if $want{$n . ":" . sha256_hex($w)};
            }
        }
        %seen = () if keys(%seen) > $CAP;
    }
}
while (defined(my $line = <STDIN>)) {
    chomp $line;
    my $tag = substr($line, 0, 1);
    if ($tag eq "#") {
        flush_section() if $opened;
        $opened = 1; %want = (); %seen = (); %found = (); $nwin = 0; $nbuf = 0;
        my %ls;
        for my $p (split " ", substr($line, 1)) {
            my ($n, $h) = split /:/, $p, 2;
            exit 3 unless defined $h && $n =~ /^[0-9]+$/ && $h =~ /^[0-9a-f]{64}$/;
            $want{($n + 0) . ":" . $h} = 1; $ls{$n + 0} = 1;
        }
        @lens = sort { $a <=> $b } keys %ls;
    } elsif ($tag eq "=" && $opened) {
        $nbuf++;
        my $s = substr($line, 1);
        $s =~ tr/A-Z/a-z/;
        for my $run (split /[^a-z0-9._-]+/, $s) { scan($run); }
        (my $red = $s) =~ tr/a-z0-9//cd;
        scan($red);
    } else {
        exit 3;
    }
}
flush_section() if $opened;
IDCPLEOF

idc_window_scan() {
    # $1 scanner, $2 the scan input (probe section, then the rows and the buffers), $3 words out (one
    # found word per line). Returns 0 only if the scanner exited 0, its output OPENS with the probe's
    # exact known answer, and what follows is one well-formed count line and then nothing but words.
    # Sets IDC_WS_WINDOWS and IDC_WS_BUFFERS from the scanner's own count line; the caller compares
    # the buffer count with the census awk's.
    local scanner="$1" in="$2" words="$3" raw="$3.raw" line="" n=0
    IDC_WS_WINDOWS=""; IDC_WS_BUFFERS=""
    : > "$raw"; : > "$words"
    case "$scanner" in
        python|python3)
            command -v "$scanner" >/dev/null 2>&1 || return 1
            "$scanner" -c "$IDC_SCAN_PY" < "$in" > "$raw" 2>/dev/null || return 1
            ;;
        perl)
            command -v perl >/dev/null 2>&1 || return 1
            perl -e "$IDC_SCAN_PL" < "$in" > "$raw" 2>/dev/null || return 1
            ;;
        *) return 1 ;;
    esac
    while IFS= read -r line || [ -n "$line" ]; do
        line="${line%$'\r'}"
        n=$((n + 1))
        case "$n" in
            1) [ "$line" = "$IDC_WPROBE_HEAD" ] || return 1 ;;
            2) [ "$line" = "$IDC_WPROBE_A" ] || return 1 ;;
            3) [ "$line" = "$IDC_WPROBE_B" ] || return 1 ;;
            4)
                case "$line" in "windows="*" buffers="*) : ;; *) return 1 ;; esac
                IDC_WS_WINDOWS="${line#windows=}"; IDC_WS_WINDOWS="${IDC_WS_WINDOWS%% *}"
                IDC_WS_BUFFERS="${line##* buffers=}"
                case "$IDC_WS_WINDOWS" in ''|*[!0-9]*) return 1 ;; esac
                case "$IDC_WS_BUFFERS" in ''|*[!0-9]*) return 1 ;; esac
                ;;
            *)
                case "$line" in ''|*[!a-z0-9._-]*) return 1 ;; esac
                printf '%s\n' "$line" >> "$words"
                ;;
        esac
    done < "$raw"
    [ "$n" -ge 4 ]
}

IDC_WS_SCANNER=""
IDC_WS_WORDS="$IDC_TMP/scan.words"
idc_scan_text() {
    # $1 = the input, $2 = the denied rows as "LEN:DIGEST LEN:DIGEST ...". THE WHOLE SCAN, and the
    # one place it is run from: idc_text_denied for a census, and the self-test for one shape on its
    # own -- a census proves a shape REFUSES, and only the scan alone proves the scan FOUND it.
    # Returns 0 with the found words in IDC_WS_WORDS (one per line, possibly none) and
    # IDC_WS_SCANNER, IDC_WS_WINDOWS set. Otherwise it PRINTS the refusal, arm and reason named, and
    # returns 1: the caller exits 2 on that. It never returns 0 having read nothing.
    local input="$1" rows="$2" scanin="$IDC_TMP/scan.in" dstat="$IDC_TMP/scan.status" f1="" f2="" nBuf="" try=""
    IDC_WS_SCANNER=""
    # The probe section first, then the denied rows from memory, then the buffers -- appended by the
    # census awk in its dump mode, which also reports how many it wrote.
    : > "$scanin"; : > "$dstat"; : > "$IDC_WS_WORDS"
    printf '#%s\n=%s\n#%s\n' "$IDC_WPROBE_ROWS" "$IDC_WPROBE_LINE" "$rows" > "$scanin"
    awk -v PATFILE="$IDC_PATTERNS" -v TOKFILE="" -v ADMITFILE="" -v TXTTOK="" -v TXTADM="" \
        -v REPORT="$IDC_TMP/scan.report" -v KEYS="$IDC_TMP/scan.keys" -v STATUS="$dstat" -v STRICT=1 -v UNMASK=0 \
        -v SHORT=0 -v CONVERTED=0 -v FIXTURE=0 -v DUMP="$scanin" -f "$IDC_AWK" -- "$input"
    if [ "$?" -ne 0 ]; then
        echo "REFUSED(2): $IDC_TXTARM: the buffer awk failed -- the window scan had nothing to read, and an instrument failure is not a clean read"
        return 1
    fi
    while IFS='=' read -r f1 f2 || [ -n "$f1" ]; do
        f2="${f2%$'\r'}"
        case "$f1" in buffers) nBuf="$f2" ;; esac
    done < "$dstat"
    case "$nBuf" in
        ''|*[!0-9]*)
            echo "REFUSED(2): $IDC_TXTARM: the buffer awk reported no buffer count -- the window scan cannot be checked against what was written, so it does not pass"
            return 1 ;;
    esac
    # A text that is not empty has at least one line, and a line is a buffer. Zero buffers from a
    # non-empty input is an awk that was handed the text and READ NONE OF IT -- and the scanner then
    # accounts for every one of those zero buffers, so the count check below cannot see it. MEASURED
    # 2026-10-06: `entry w=1.md`, a relative name shaped like an awk assignment, was taken by awk as
    # one; it read no file, 0 = 0 held, and the census read CLEAN over a line that carried a denied
    # word. idc_census now hands every awk a path that cannot be read that way; this is the same
    # property asked of the RESULT, for whatever the next way of reading nothing turns out to be.
    if [ "$nBuf" -eq 0 ] && [ -s "$input" ]; then
        echo "REFUSED(2): $IDC_TXTARM: the buffer awk wrote 0 buffers from an input that is not empty -- it read none of the text, so the window scan had nothing to read and it does not pass"
        return 1
    fi
    # IDC_WINDOWER is a self-test forcing hook only, like IDC_HASHER: it NAMES the one scanner to try.
    # It cannot widen anything -- a name that is not a working scanner refuses below. THERE IS NO
    # NARROWED MODE: a box with no scanner does not fall back to the candidates alone, because that
    # is the verdict that depends on the box.
    for try in ${IDC_WINDOWER:-python python3 perl}; do
        if idc_window_scan "$try" "$scanin" "$IDC_WS_WORDS" && [ "$IDC_WS_BUFFERS" = "$nBuf" ]; then IDC_WS_SCANNER="$try"; break; fi
    done
    if [ -z "$IDC_WS_SCANNER" ]; then
        echo "REFUSED(2): $IDC_TXTARM: no usable window scanner (tried: ${IDC_WINDOWER:-python python3 perl}; each must return the known answer for its probe and account for every buffer) -- the windows of the text were not hashed, so it does not pass"
        return 1
    fi
    return 0
}

idc_text_denied() {
    # $1 = the input. Fills IDC_TXTTOK, IDC_TXTADM and IDC_TXTSUMMARY -- or EXITS 2, reason named.
    local input="$1" cand="$IDC_TMP/cand" kinds="$IDC_TMP/cand.kind" sums="$IDC_TMP/cand.sha"
    local f1="" f2="" f3="" len="" h="" row=0 dl=" " al=" " nRows=0 try="" hasher=""
    local tok="" kind="" n=0 nDen=0 nAdm=0
    # dset / aset: the DENIED and the ADMIT rows as " LEN:DIGEST LEN:DIGEST ... " -- one string each,
    # tested with `case`, because bash 3.2 has no associative array. They are a PREFILTER and nothing
    # more: a positive is still put to idc_denied_row / idc_admit_row before anything is installed.
    local dset=" " aset=" " nScan=0 nConf=0 line="" asks0=0
    : > "$IDC_TXTTOK"; : > "$IDC_TXTADM"

    if [ ! -f "$IDC_HASHES" ] || [ ! -r "$IDC_HASHES" ]; then
        echo "REFUSED(2): $IDC_TXTARM: the hash list $(basename -- "$IDC_HASHES") is missing or unreadable -- an arm with no denied rows has nothing to refuse with, so it does not pass"
        exit 2
    fi
    while IFS=$'\t' read -r f1 f2 f3 _rest || [ -n "$f1" ]; do
        row=$((row + 1))
        f1="${f1%$'\r'}"; f2="${f2%$'\r'}"; f3="${f3%$'\r'}"
        case "$f1" in '#'*|'') continue ;; esac
        if [ "$f1" = "ADMIT" ]; then len="$f2"; h="$f3"; else len="$f1"; h="$f2"; fi
        # 0*: a length written with a leading zero is a NUMBER to awk and to the scanner and a
        # different STRING to idc_denied_row, which compares it with a word's length as text. A row
        # the readers disagree about is a row that does not parse.
        case "$len" in ''|*[!0-9]*|0*) h="" ;; esac
        case "$h" in *[!0-9a-f]*) h="" ;; esac
        if [ "${#h}" -ne 64 ]; then
            echo "REFUSED(2): $IDC_TXTARM: row $row of $(basename -- "$IDC_HASHES") is not LEN<TAB>SHA256 (or ADMIT<TAB>LEN<TAB>SHA256) -- a hash list that does not parse is not a list, so it does not pass"
            exit 2
        fi
        if [ "$f1" = "ADMIT" ]; then al="$al$len "; aset="$aset$len:$h "
        else dl="$dl$len "; dset="$dset$len:$h "; nRows=$((nRows + 1)); fi
    done < "$IDC_HASHES"
    if [ "$nRows" -eq 0 ]; then
        echo "REFUSED(2): $IDC_TXTARM: $(basename -- "$IDC_HASHES") yielded 0 denied rows -- a census that reads nothing does not pass"
        exit 2
    fi

    # 2. THE WINDOW SCAN, over the denied rows held in memory. It names its own refusals.
    idc_scan_text "$input" "${dset# }" || exit 2
    while IFS= read -r line || [ -n "$line" ]; do nScan=$((nScan + 1)); done < "$IDC_WS_WORDS"

    : > "$cand"; : > "$kinds"
    awk -v DL="$dl" -v AL="$al" -v TOK="$cand" -v KIND="$kinds" -v KAT="$IDC_KAT_IN" -v SCANW="$IDC_WS_WORDS" '
        # The CR of a CRLF line end is whitespace at the break. An awk off Windows keeps it on the
        # record, and left there it is a separator in the middle of the joined word.
        function rtrim(s) { sub(/[ \t\r]+$/, "", s); return s }
        function ltrim(s) { sub(/^[ \t]+/, "", s); return s }
        function note(t, k) {
            if (!(t in K)) { nC++; ORD[nC] = t; K[t] = "" }
            if (index(K[t], k) == 0) K[t] = K[t] k
        }
        # scanTokens: maximal [a-z0-9._-] runs, plus each dot/hyphen/underscore component -- AND each
        # SPAN of adjacent components with the separators between them kept. A denied word that itself
        # carries a separator is otherwise cut in two by the component walk, and is the whole run only
        # while nothing touches it: the full stop that ends a sentence is a run character. P[j] is where
        # component j starts; a span is abandoned as soon as it is longer than the longest denied row,
        # so the walk is bounded by that length and not by the length of the run. (No apostrophe may
        # appear in this program: it is a single-quoted shell word.)
        function denied(lo,   m, R, i, c, C, P, j, k, n) {
            m = split(lo, R, /[^a-z0-9._-]+/)
            for (i = 1; i <= m; i++) {
                if (length(R[i]) in DLEN) note(R[i], "D")
                c = split(R[i], C, /[._-]/)
                if (c < 2) continue
                P[1] = 1
                for (j = 1; j <= c; j++) {
                    if (length(C[j]) in DLEN) note(C[j], "D")
                    P[j + 1] = P[j] + length(C[j]) + 1
                }
                for (j = 1; j < c; j++)
                    for (k = j + 1; k <= c; k++) {
                        n = P[k] + length(C[k]) - P[j]
                        if (n > DMAX) break
                        if ((n in DLEN) && n < length(R[i])) note(substr(R[i], P[j], n), "D")
                    }
            }
        }
        # admitWord: percent-escapes are separators, the word is the [a-z0-9_.-] run; dottedHandleOK
        # reads a word by its dot segments.
        function admits(lo,   m, R, i, c, C, j) {
            gsub(/%[0-9a-f][0-9a-f]/, " ", lo)
            m = split(lo, R, /[^a-z0-9_.-]+/)
            for (i = 1; i <= m; i++) {
                if (length(R[i]) in ALEN) note(R[i], "A")
                c = split(R[i], C, /[.]/)
                if (c > 1) for (j = 1; j <= c; j++) if (length(C[j]) in ALEN) note(C[j], "A")
            }
        }
        BEGIN {
            nC = 0
            DMAX = 0
            m = split(DL, T, " "); for (i = 1; i <= m; i++) { DLEN[T[i] + 0] = 1; if (T[i] + 0 > DMAX) DMAX = T[i] + 0 }
            m = split(AL, T, " "); for (i = 1; i <= m; i++) ALEN[T[i] + 0] = 1
            delete DLEN[0]; delete ALEN[0]
            print KAT > TOK; print "K" > KIND
        }
        {
            lo = tolower($0); denied(lo); admits(lo)
            if (NR > 1) {
                left = rtrim(prev); right = ltrim($0)
                if (length(left) > 0 && length(right) > 0) { lo = tolower(left right); denied(lo); admits(lo) }
            }
            prev = $0
        }
        END {
            # The words the WINDOW SCAN named join the candidates here: kind D, so each is hashed
            # again by the batch hasher and confirmed by the authoritative reader like any other, and
            # kind S, so the shell can require that every one of them WAS confirmed. A word that was
            # already a candidate stays ONE candidate -- note() merges the kinds.
            if (SCANW != "") {
                while ((getline w < SCANW) > 0) { sub(/\r$/, "", w); if (w != "") { note(w, "D"); note(w, "S") } }
                close(SCANW)
            }
            for (i = 1; i <= nC; i++) { print ORD[i] > TOK; print K[ORD[i]] > KIND }
            close(TOK); close(KIND)
        }
    ' < "$input"
    if [ "$?" -ne 0 ]; then
        echo "REFUSED(2): $IDC_TXTARM: the candidate awk failed -- an instrument failure is not a clean read"
        exit 2
    fi

    # IDC_HASHER is a self-test forcing hook only, like IDC_SHORT: it NAMES the one hasher to try. It
    # cannot widen anything -- a name that is not a working hasher refuses below.
    for try in ${IDC_HASHER:-python python3 sha256sum}; do
        if idc_hash_batch "$try" "$cand" "$sums"; then hasher="$try"; break; fi
    done
    if [ -z "$hasher" ]; then
        echo "REFUSED(2): $IDC_TXTARM: no usable hasher (tried: ${IDC_HASHER:-python python3 sha256sum}; each must return the known answer for its probe) -- the words of the text were not hashed, so it does not pass"
        exit 2
    fi

    asks0="$IDC_ROW_ASKS"
    exec 7< "$cand" 8< "$kinds" 9< "$sums"
    while IFS= read -r tok <&7; do
        IFS= read -r kind <&8; IFS= read -r h <&9; h="${h%$'\r'}"
        n=$((n + 1))
        # The in-memory set first; the authoritative reader only on a positive. A candidate is one
        # line of the candidate file and is met here once, so a word is installed once.
        case "$kind" in
            *D*)
                case "$dset" in
                    *" ${#tok}:$h "*)
                        if idc_denied_row "${#tok}" "$h"; then
                            printf '%s\t%s\n' "$IDC_TXTARM" "$tok" >> "$IDC_TXTTOK"; nDen=$((nDen + 1))
                            case "$kind" in *S*) nConf=$((nConf + 1)) ;; esac
                        fi ;;
                esac ;;
        esac
        case "$kind" in
            *A*)
                case "$aset" in
                    *" ${#tok}:$h "*)
                        if idc_admit_row "${#tok}" "$h"; then printf '%s\n' "$tok" >> "$IDC_TXTADM"; nAdm=$((nAdm + 1)); fi ;;
                esac ;;
        esac
    done
    exec 7<&- 8<&- 9<&-
    # EVERY word the scanner named must have come back confirmed. The scanner looked each one up in
    # the rows it was handed; the batch hasher and idc_denied_row have now looked the same words up in
    # the list itself. If the two counts differ, one of them is wrong, and which one is not something
    # a census can pass on.
    if [ "$nConf" -ne "$nScan" ]; then
        echo "REFUSED(2): $IDC_TXTARM: the window scan named $nScan word(s) and the hash list confirmed $nConf of them -- the scanner and the list disagree, so it does not pass"
        exit 2
    fi
    # candidates= is what the candidate awk wrote (the scan's words among them); hashed= is the
    # HASHER'S OWN count of digests, less its probe; scan-windows= is the SCANNER'S OWN count of
    # windows hashed, its probe section not included. None of the three is derived from another.
    # lookups= is how many times the authoritative readers were ASKED in the loop above, counted
    # inside the readers: one per prefilter positive. A text with no denied and no admit word asks
    # them nothing, however many candidates it has.
    IDC_TXTSUMMARY="$IDC_TXTARM denied-rows=$nRows candidates=$((n - 1)) hashed=$((IDC_HB_COUNT - 1)) denied-words=$nDen admit-words=$nAdm hasher=$hasher scan-windows=$IDC_WS_WINDOWS scan-words=$nScan scanner=$IDC_WS_SCANNER lookups=$((IDC_ROW_ASKS - asks0))"
    return 0
}

# -------------------------------------------------------------------------------------------------
# The awk program. Written to a temp file from a QUOTED heredoc, so nothing in it is interpolated,
# expanded or collapsed on the way through the shell.
# -------------------------------------------------------------------------------------------------
IDC_AWK="$IDC_TMP/census.awk"
cat > "$IDC_AWK" <<'IDCAWKEOF'
# [ \t\r]: the CR of a CRLF line end is whitespace at the break (an awk off Windows keeps it on the
# record), so PASS 2 joins the same two halves on every box. The candidate awk of TEXT_DENIED trims
# the same way; the two must not differ, or a word found there is not found here.
function rtrim(s) { sub(/[ \t\r]+$/, "", s); return s }
function ltrim(s) { sub(/^[ \t]+/, "", s); return s }

function pathSeg(mt,   t, i) {
    t = mt; gsub(/\\/, "/", t)
    i = length(t)
    while (i > 0 && substr(t, i, 1) != "/") i--
    return substr(t, i + 1)
}
function uncHost(mt,   t, i, rest, j) {
    t = mt; gsub(/\\/, "/", t)
    i = index(t, "//")
    if (i == 0) return ""
    rest = substr(t, i + 2)
    j = index(rest, "/")
    if (j > 0) rest = substr(rest, 1, j - 1)
    return rest
}
function hostVal(mt,   i, j, v) {
    i = index(mt, "="); j = index(mt, ":")
    if (i == 0 || (j > 0 && j < i)) i = j
    if (i == 0) return ""
    v = substr(mt, i + 1); sub(/^[ \t]+/, "", v)
    return v
}
# CONVERTED MODE -- the one place, other than STRICT/DELTA, where the consumer mode moves an arm.
# An arm whose NOTE carries [CONVERTED-CONTEXT] in the DEFINITION is read as `context` here: its
# occurrences are counted and REPORTED and it cannot refuse. The set is read from the patterns file
# and is NOT a list in this script -- an arm is declared once, where every other property of it is
# declared. Everything else keeps its mode, so the profile, home, share and run-time token arms refuse
# in converted mode exactly as they do in entry.
#
# ⚠ THE RUN-TIME TOKEN ARMS CANNOT BE DOWNGRADED AT ALL, and that is structural rather than a rule
# written down anywhere: their literals are @RUNTIME@, scanAll skips them in the arm loop, and
# checkTok calls record() directly without passing through here. MEASURED while red-firsting this
# change -- marking TOKENFILE [CONVERTED-CONTEXT] in the definition left its case GREEN, where the
# same marking on profile_root reds its case immediately. The property is the one that matters (a
# denied name can never be admitted by a mode), but it is not something the marker enforces, so a
# future arm that DOES route through here and must never be downgradable needs its own control.
function armRefuses(arm) {
    if (MD[arm] != "refuse") return 0
    if (CONVERTED == 1 && DOWNARM[arm] == 1) return 0
    # ON AN UPSTREAM FIXTURE ONLY: the Go gate's scanFleetIdentifiers sets structural :=
    # !fleetIsUpstreamFixture(path) and runs its profile/home and network-path regexes only when it is
    # true. Mirrored, never exceeded -- a converted-door path the Go gate does NOT call a fixture (a
    # validation page, package_test_info.cs, go2cs_test_host.cs, a tests csproj) keeps the two-arm
    # downgrade above and nothing more. The run-time and denied-token arms never reach this function
    # (see the note above it), so no marker here can downgrade them.
    if (CONVERTED == 1 && FIXTURE == 1 && FIXARM[arm] == 1) return 0
    return 1
}
# Anchored-whole membership in a CONTEXT arm used as an admit set. Per OCCURRENCE, on the decision
# token -- never on the line.
function admitted(setName, tok) {
    if (!(setName in RE)) return 0
    return (tolower(tok) ~ ("^(" RE[setName] ")$"))
}
# FNV-1a, 32-bit, over the lower-cased value. A DELTA KEY and a DISTINGUISHER, never a disclosure:
# it lets tree mode tell "the hit the baseline already had" from "a hit this post ADDED", and lets a
# reader tell two masked hits apart, without the instrument printing a value.
function fnv(s,   i, h, c) {
    s = tolower(s); h = 2166136261
    for (i = 1; i <= length(s); i++) {
        c = index(CHARS, substr(s, i, 1))
        h = xor32(h, c)
        h = (h * 16777619) % 4294967296
    }
    return sprintf("%08x", h)
}
function xor32(a, b,   i, r, p, x, y) {
    r = 0; p = 1
    for (i = 0; i < 32; i++) {
        x = a % 2; y = b % 2
        if (x != y) r = r + p
        a = int(a / 2); b = int(b / 2); p = p * 2
        if (a == 0 && b == 0) break
    }
    return r
}
# Literal, case-insensitive replace-all. Never gsub with a literal built into a regex: a name can
# carry '.' and '-', and '.' in a dynamic regex matches anything.
function replAll(s, find, repl,   out, i) {
    if (find == "") return s
    out = ""
    while (1) {
        i = index(tolower(s), tolower(find))
        if (i == 0) break
        out = out substr(s, 1, i - 1) repl
        s = substr(s, i + length(find))
    }
    return out s
}
# COORD's line rule, reproduced: LONGEST LITERAL FIRST, then the three shape masks, then 160 chars.
# Masking a contained literal ahead of the literal that contains it leaves the container's suffix
# standing, which discloses exactly the infrastructure detail the order forbids. Only --unmask
# reaches this; the default report prints no line at all.
function maskLine(line,   m, i) {
    m = line
    for (i = 1; i <= nT; i++) m = replAll(m, TSORT[i], "<*REDACTED-" length(TSORT[i]) "*>")
    m = shapeMask(m, "users")
    m = shapeMask(m, "home")
    m = shapeMask(m, "unc")
    if (length(m) > 160) m = substr(m, 1, 160) " ..."
    return m
}
function shapeMask(s, kind,   out, rest, re, i, pre, seg, c, j) {
    if (kind == "users") re = "users[\\\\/]+"
    else if (kind == "home") re = "/home/"
    else re = "[\\\\][\\\\]"
    out = ""; rest = s
    while (match(tolower(rest), re)) {
        pre = substr(rest, 1, RSTART + RLENGTH - 1)
        rest = substr(rest, RSTART + RLENGTH)
        j = 0
        while (j < length(rest)) {
            c = substr(rest, j + 1, 1)
            if (c ~ /[\\\/ \t"'`,;:)\]}>*|]/) break
            j++
        }
        seg = substr(rest, 1, j)
        rest = substr(rest, j + 1)
        if (length(seg) >= 2) out = out pre "<*REDACTED*>"
        else out = out pre seg
    }
    return out rest
}
function maskIpv4(q,   p) { p = index(q, "."); if (p < 2) return "x.x.x.x"; return substr(q, 1, p - 1) ".x.x.x" }
function maskValue(arm, v) {
    if (UNMASK == 1) return v
    if (arm == "ipv4") return maskIpv4(v)
    return "<*REDACTED-" length(v) "*>"
}

function record(arm, pass, lineno, value, line,   shown) {
    HITS[arm]++
    nH++
    shown = maskValue(arm, value)
    HL[nH] = sprintf("    %-18s pass%-2d line %-7d %-22s fp=%s", arm, pass, lineno, shown, fnv(value))
    if (UNMASK == 1) HL[nH] = HL[nH] "\n        line: " maskLine(line)
    print arm "\t" fnv(value) "\t1" > KEYS
    # THE AWK PROBE's reading of a hit, and nothing else's: which arm, which pass, which line, and
    # the fingerprint of the value -- so the extent the engine reported is part of the answer.
    if (PROBE == 1) HAT[nH] = arm "|" pass "|" lineno "|" fnv(value)
}

# ---- structural arms -----------------------------------------------------------------------------
function scanArm(arm, lineno, text, lo, pass, joinAt,   pos, s, e, mt, tok, ok) {
    pos = 0
    while (1) {
        if (match(substr(lo, pos + 1), RE[arm]) == 0) break
        s = pos + RSTART; e = s + RLENGTH - 1
        if (RLENGTH < 1) break
        pos = e
        if (pass == 2 && !(s <= joinAt && e > joinAt)) continue
        OCC[arm]++
        if (MD[arm] != "refuse") continue
        mt = substr(text, s, RLENGTH)
        ok = 1
        if (arm ~ /^profile_/ || arm ~ /^home_/) {
            tok = pathSeg(mt)
            if (admitted("profile_placeholder", tok)) { EXC[arm "\t" "placeholder-segment"]++; ok = 0 }
        } else if (arm ~ /^unc_/) {
            tok = uncHost(mt)
            # THE PLACEHOLDER ADMIT REACHES THE unc_ ARMS TOO (COORD d30c9d36a, ruling (a)). The Go
            # guard's fleetConsiderSegment applies fleetIsPlaceholder to EVERY kind, network-path
            # included; the header above already claimed that set for every arm, which was true of
            # the PATTERN and, until this line, false of the DECISION. Consulted BEFORE the nickname
            # map, in the guard's own order, and on the HOST SEGMENT -- per OCCURRENCE, never per
            # line, so a placeholder host never clears a real host beside it.
            if (tok == "") { EXC[arm "\t" "no-host"]++; ok = 0 }
            # A JSON unicode escape of a converter glyph satisfies this arm's host-token class, and
            # the corpus escapes constantly. Inside a JSON string a literal backslash is DOUBLED, so a
            # real UNC prefix carries FOUR and an escape carries TWO -- measured on one committed line
            # carrying both. Consulted FIRST because it is the most mechanical of the three, so the
            # reason a hit is admitted reads as what it is (COORD 3a680658f (4), C1's A/B 48b7d578).
            else if (admitted("unc_escape", tok)) { EXC[arm "\t" "escape-sequence"]++; ok = 0 }
            else if (admitted("profile_placeholder", tok)) { EXC[arm "\t" "placeholder-segment"]++; ok = 0 }
            else if (admitted("nickname_host", tok)) { EXC[arm "\t" "nickname-host"]++; ok = 0 }
        } else if (arm ~ /^host_/) {
            tok = hostVal(mt)
            # A nickname-PREFIXED value is admitted here and NOWHERE ELSE: the measured shape on the
            # live surface is a fleet host named by its prescribed nickname plus a qualifier, and
            # refusing that teaches the next writer to delete exactly the spelling the order asks
            # for. Per ARM, per OCCURRENCE, and the profile and unc arms do not get it.
            if (tok == "") { EXC[arm "\t" "no-value"]++; ok = 0 }
            else if (tolower(tok) ~ ("^(" RE["nickname_fleet"] ")([._-][a-z0-9._-]*)?$")) { EXC[arm "\t" "nickname-host"]++; ok = 0 }
        } else {
            tok = mt
        }
        # The admit sets have already had their say; what is left is what entry mode WOULD refuse.
        # In converted mode a downgraded arm records that number instead of a hit, so the reader sees
        # exactly what was admitted and by which arm.
        if (ok) { if (armRefuses(arm)) record(arm, pass, lineno, tok, text); else DOWN[arm]++ }
    }
}

# ---- the IPv4 arm --------------------------------------------------------------------------------
# STRICT (entry/subject): no CONTEXT exclusion of any kind; the release_literal ADMIT SET (rule 5),
# read on the quad's own shape, is the one exclusion it takes. DELTA (tree): rules 1-4, then rule 5,
# then rule 6 (a GoPositionMap record's function-literal suffix), which is a context rule and so
# DELTA ONLY.
# ipv4ParseAt walks a four-octet quad BY HAND from position p and sets IPV4E to the position of its
# last digit. It returns 0 unless all four octets are there, 0-255, unpadded, and not followed by a
# fifth digit -- the same acceptance the ipv4 ERE has, derived without the ERE.
function ipv4ParseAt(lo, p,   i, j, c, n, e) {
    e = p - 1
    for (i = 1; i <= 4; i++) {
        if (i > 1) {
            if (substr(lo, e + 1, 1) != ".") return 0
            e = e + 1
        }
        j = e + 1; n = 0
        while (j <= length(lo) && substr(lo, j, 1) ~ /[0-9]/ && n < 3) { j++; n++ }
        if (n == 0) return 0
        if (n > 1 && substr(lo, e + 1, 1) == "0") return 0   # a padded octet is not one, as the ERE has it
        c = substr(lo, e + 1, n) + 0
        if (c > 255) return 0
        e = j - 1
    }
    if (substr(lo, e + 1, 1) ~ /[0-9]/) return 0             # a fifth digit means this was never an octet
    IPV4E = e
    return 1
}
# ipv4Extent derives the candidate's TRUE start and end from RSTART alone, setting IPV4S/IPV4E.
#
# ⚠ RLENGTH IS NOT USABLE HERE. mawk 1.3.4's match() is not leftmost-longest: on a four-octet quad it
# returns RLENGTH=7 -- three octets' worth -- so `e = RSTART + RLENGTH` lands MID-QUAD, and every rule
# that reads from the quad's end (the token run, the 32-character after-window) is then computed from
# the wrong place. Measured on C1's box: the version-context exclusion never fired and honest version
# quads in prose were REFUSED, self-test 60/62 under mawk while gawk read 62/62. A regex engine's
# submatch extent is an ASSUMPTION; the characters on the line are the measurement.
#
# So: scan BACK over [0-9.] in case the engine's start is itself short, then parse forward from each
# candidate position up to RSTART -- a quad must begin at or before the position the engine matched.
# The three whitespace-delimited words rule 4 is allowed to read: the quad's OWN word, the one
# immediately before it, and the one immediately after. A WINDOW is a laundering surface -- C2's A/B
# measured a routable-shaped quad refused when alone and CLEAN when any version word sat loose in the
# same 56/32 characters. Adjacency cannot be arranged by a sentence that has nothing to do with the
# address.
function wordOwn(lo, s, e,   b, t, L) {
    L = length(lo)
    b = s; while (b > 1 && substr(lo, b - 1, 1) !~ /[ \t]/) b--
    t = e; while (t < L && substr(lo, t + 1, 1) !~ /[ \t]/) t++
    return substr(lo, b, t - b + 1)
}
function wordBefore(lo, s,   i, e2, b) {
    i = s; while (i > 1 && substr(lo, i - 1, 1) !~ /[ \t]/) i--
    e2 = i - 1
    while (e2 > 0 && substr(lo, e2, 1) ~ /[ \t]/) e2--
    if (e2 < 1) return ""
    b = e2; while (b > 1 && substr(lo, b - 1, 1) !~ /[ \t]/) b--
    return substr(lo, b, e2 - b + 1)
}
function wordAfter(lo, e,   i, b, e2, L) {
    L = length(lo)
    i = e; while (i < L && substr(lo, i + 1, 1) !~ /[ \t]/) i++
    b = i + 1
    while (b <= L && substr(lo, b, 1) ~ /[ \t]/) b++
    if (b > L) return ""
    e2 = b; while (e2 < L && substr(lo, e2 + 1, 1) !~ /[ \t]/) e2++
    return substr(lo, b, e2 - b + 1)
}
#
# The back-scan stops at FLOOR + 1, where FLOOR is the end of the span this line already decided. A quad
# glued to the one before it by a dot (`<decided quad>.<next quad>`) otherwise scans back over the dot
# into the decided quad, parses IT first, and hands back its span, which scanIpv4 skips as already
# decided; the trailing quad was then never examined and read CLEAN in STRICT, whatever it was, after
# a loopback, broadcast, unspecified or release-literal quad (COORD 2026-09-29, found as pre-existing).
function ipv4Extent(lo, rstart, floor,   s) {
    s = rstart
    while (s > floor + 1 && substr(lo, s - 1, 1) ~ /[0-9.]/) s--
    while (s <= rstart) {
        if (substr(lo, s, 1) ~ /[0-9]/ && ipv4ParseAt(lo, s)) { IPV4S = s; return 1 }
        s++
    }
    return 0
}
# csStrEnd walks ONE C# regular string literal whose opening quote is at p and returns the position
# of its closing quote, or 0 if there is none on the line. A backslash escapes the character after
# it, which is exactly how the converter's csharpStringLiteral writes the two it escapes (a quote and
# a backslash), so an escaped quote inside an earlier argument can never be read as that argument's
# end and shift the fourth argument's span.
function csStrEnd(lo, p,   L, c) {
    L = length(lo)
    if (substr(lo, p, 1) != "\"") return 0
    p++
    while (p <= L) {
        c = substr(lo, p, 1)
        if (c == "\\") { p += 2; continue }
        if (c == "\"") return p
        p++
    }
    return 0
}
# posmapFunclit is RULE 6's whole decision, per OCCURRENCE: 1 only when ALL THREE hold.
#   (1) the line IS a GoPositionMap record -- ipv4_posmap, the recogniser the DEFINITION carries,
#       anchored at the line's start, and its argument list parses as exactly FOUR C# string
#       literals joined by ", " and closed by ")]", as positionMapOperations.go emits it;
#   (2) the candidate lies wholly INSIDE THE FOURTH argument -- the funcLits list -- and not in the
#       Go file identity, the C# file name, the encoded table, or anything after the ")]";
#   (3) it is EXACTLY THE SUFFIX of one well-formed entry: immediately preceded by
#       <digits>-<digits>: whose first digit opens the argument or follows a ';', and immediately
#       followed by a ';' or by the argument's closing quote.
# The opening parenthesis is found by index() on the characters, never from RLENGTH: this file's
# doctrine is that an engine's reported extent is an assumption (see ipv4Extent), and the prefix the
# recogniser matched is one literal the line has to spell anyway.
function posmapFunclit(lo, s, e,   p, i, q, open4, k) {
    if (!("ipv4_posmap" in RE)) return 0
    if (lo !~ RE["ipv4_posmap"]) return 0
    p = index(lo, "gopositionmap(")
    if (p == 0) return 0
    p = p + length("gopositionmap(")
    open4 = 0; q = 0
    for (i = 1; i <= 4; i++) {
        if (i > 1) { if (substr(lo, p, 2) != ", ") return 0; p = p + 2 }
        q = csStrEnd(lo, p)
        if (q == 0) return 0
        if (i == 4) open4 = p
        p = q + 1
    }
    if (substr(lo, p, 2) != ")]") return 0
    # (2) q is now the FOURTH argument's closing quote and open4 its opening one.
    if (s <= open4 || e >= q) return 0
    # (3) the terminator, then the entry prefix read backwards from the candidate's own start.
    if (!(substr(lo, e + 1, 1) == ";" || e + 1 == q)) return 0
    k = s - 1
    if (substr(lo, k, 1) != ":") return 0
    k--
    if (substr(lo, k, 1) !~ /[0-9]/) return 0
    while (substr(lo, k, 1) ~ /[0-9]/) k--
    if (substr(lo, k, 1) != "-") return 0
    k--
    if (substr(lo, k, 1) !~ /[0-9]/) return 0
    while (substr(lo, k, 1) ~ /[0-9]/) k--
    return (k == open4 || substr(lo, k, 1) == ";")
}
function scanIpv4(lineno, text, lo, pass, joinAt,   pos, s, e, quad, lq, k, b, cand, rs, rr, run, own, before, after, rstart, lastEnd) {
    pos = 0; lastEnd = 0
    while (1) {
        if (match(substr(lo, pos + 1), RE["ipv4"]) == 0) break
        rstart = pos + RSTART
        # SHORT is the self-test's forcing hook: it perturbs the engine's reported start FORWARD, which
        # is a strictly harder version of what a short match does, so the extent derivation is proven
        # on gawk too and cannot regress silently back onto RLENGTH.
        if (SHORT > 0 && rstart + SHORT <= length(lo)) rstart = rstart + SHORT
        if (ipv4Extent(lo, rstart, lastEnd) == 0) { pos = rstart; continue }
        s = IPV4S; e = IPV4E
        pos = (e > rstart) ? e : rstart     # rstart > old pos, so this always advances
        if (e <= lastEnd) continue          # the back-scan re-found a span already decided
        lastEnd = e
        if (pass == 2 && !(s <= joinAt && e > joinAt)) continue
        OCC["ipv4"]++
        quad = substr(text, s, e - s + 1); lq = tolower(quad)

        if (STRICT == 0) {
            # RULE 1 -- C2's prefix exemption, read against the reconstructed prefix-plus-quad.
            k = s
            if (k > 1 && substr(lo, k - 1, 1) == "=") k = k - 1
            while (k > 1 && substr(lo, k - 1, 1) ~ /[a-z_]/) k--
            b = (k > 1) ? substr(lo, k - 1, 1) : ""
            cand = b substr(lo, k, s - k) lq
            if (cand ~ RE["ipv4_prefix_ex"]) { EXC["ipv4\tprefix-ex"]++; continue }

            # RULE 2 -- token run. The maximal identifier run around the quad, less one trailing dot.
            rs = s; while (rs > 1 && substr(lo, rs - 1, 1) ~ /[a-z0-9._-]/) rs--
            rr = e; while (rr < length(lo) && substr(lo, rr + 1, 1) ~ /[a-z0-9._-]/) rr++
            run = substr(lo, rs, rr - rs + 1); sub(/\.$/, "", run)
            if (run != lq) { EXC["ipv4\ttoken-run"]++; continue }

            # RULE 4 -- version context by ADJACENCY, never by a window. Three words only: the quad's
            # own whitespace-delimited word, the one immediately before, the one immediately after.
            own = wordOwn(lo, s, e); before = wordBefore(lo, s); after = wordAfter(lo, e)
            if (own ~ RE["ipv4_vercontext"] || before ~ RE["ipv4_vercontext"] || after ~ RE["ipv4_vercontext"]) { EXC["ipv4\tversion-context"]++; continue }
        }

        # RULE 3 -- the three DECLARED DOCUMENTATION CONSTANTS (loopback, unspecified, broadcast).
        # MOVED OUT of the STRICT==0 block 2026-09-20 (COORD fc4edcd8c) and consulted in BOTH modes,
        # for rule 5's reason and by rule 5's own test: it is read ANCHORED WHOLE on the quad's own
        # characters and on nothing around it, so it is a SHAPE and not a context. Rules 1, 2 and 4
        # read a prefix, a token run and neighbouring words and every one of them can be ARRANGED by
        # the sentence a lane writes; no sentence can arrange for a quad to BE one of these three.
        #
        # ⚠ WHY IT HAD TO MOVE, and it is not tidiness. STRICT's rationale is that the cost of the
        # strict reading "falls on the writer, as one rewrite of their own post" -- true of a post
        # BODY and FALSE of an EVIDENCE RECORD, which the lane did not author and cannot rewrite
        # without corrupting the field the re-classification reads. A results tail whose quads are
        # all loopback refused every one of them at `entry`: measured 12 of 12 before this change.
        #
        # ⚠ ORDER, and its cost measured rather than argued: rule 3 now runs AFTER rules 1, 2 and 4
        # in delta instead of between 2 and 4, so an occurrence those rules dispose of first keeps
        # THEIR reason. Measured on the shared surface across the move, the delta tally is unchanged
        # (release-literal 48, token-run 2, version-context 9, doc-constant 2, prefix-ex 18), so
        # nothing was re-attributed here -- which is the property rule 5's comment below asks for.
        if (lq ~ ("^(" RE["ipv4_doc"] ")$")) { EXC["ipv4\tdoc-constant"]++; OCC["ipv4_doc"]++; continue }

        # RULE 5 -- the Go RELEASE LITERAL, and the ONE exclusion the STRICT reading takes. It is a
        # per-arm ADMIT SET read on the DECISION TOKEN -- the quad itself, anchored whole, per
        # OCCURRENCE -- and on nothing around it. That is what separates it from rules 1-4: a
        # context rule can be arranged by the sentence a lane writes, and a SHAPE cannot, so this
        # one is safe in the mode where the others are refused. It is consulted LAST in delta mode
        # on purpose: every occurrence rules 1-4 already dispose of keeps ITS OWN reason in the
        # EXCLUSIONS block, so adding a rule cannot silently re-attribute what the others were
        # measured on. Per ARM: no other arm consults this set, and none of them gains an admit.
        if (admitted("release_literal", lq)) { EXC["ipv4\trelease-literal"]++; continue }

        # RULE 6 -- a GoPositionMap record's FUNCTION-LITERAL SUFFIX, and IN DELTA MODE ONLY. The
        # fourth argument of every record the converter emits into an info file is its funcLits list,
        # "<startLine>-<endLine>:<suffix>" entries joined by ';', and the suffix is Go's own closure
        # counter: a literal nested four deep carries four dotted integers, which is a quad to the
        # candidate matcher and was refused as one. MEASURED 2026-09-28 at 15da8805b2: the aead.go record in
        # crypto/internal/cryptotest's package_info.cs read four ipv4 hits in BOTH modes, all four of
        # them depth-four suffixes; corpus-wide, six records carry such suffixes (two production
        # package_info.cs, four package_test_info.cs). Every condition is per OCCURRENCE, never per
        # line -- see posmapFunclit -- so a quad anywhere else on the same record still refuses.
        #
        # ⚠ WHY NOT IN STRICT, and it is rule 5's own test applied rather than a preference. This rule
        # reads the LINE AROUND the quad -- the record's prefix, which argument it sits in, and the
        # characters either side of it -- and a lane can arrange every one of those by writing its
        # sentence AS A RECORD. That is a CONTEXT rule, exactly as rules 1, 2 and 4 are, and STRICT
        # refuses context rules by design. The authorship argument is real -- nobody writes these by
        # hand, and a rewritten suffix corrupts the frame names the runtime derives from it -- but this
        # file's answer to "the lane did not author these bytes" is a MODE whose door is a property of
        # the INPUT'S PATH (`converted`), never a context rule inside `entry`: package_test_info.cs and
        # package_info_internal_test.cs are through that door already, where the quad arm is reported
        # and cannot refuse. A production package_info.cs is not, and widening the door to it is a
        # ruling, not this rule.
        #
        # ⚠ CONSULTED LAST, after rule 5, for rule 5's stated reason: an occurrence an older rule
        # already disposes of keeps ITS reason in the EXCLUSIONS block, so adding this rule cannot
        # re-attribute anything the others were measured on.
        #
        # A suffix nested FIVE or more deep is NOT rule 6's, and needs no rule: its leading four
        # components are the candidate and are followed by a '.', so condition (3) fails -- but rule 2
        # has already excluded it, because the token run is the whole dotted suffix and not exactly
        # the quad. MEASURED at the same tip: the four such entries corpus-wide (three in crypto/tls's
        # package_info.cs, one in runtime's package_test_info.cs) read token-run, and the twenty
        # four-deep suffixes read position-map-funclit, in delta. A self-test case pins both.
        if (STRICT == 0 && posmapFunclit(lo, s, e)) { EXC["ipv4\tposition-map-funclit"]++; continue }

        if (armRefuses("ipv4")) record("ipv4", pass, lineno, quad, text); else DOWN["ipv4"]++
    }
}

# ---- the run-time token arms ---------------------------------------------------------------------
# Tokenised exactly as the Go guard tokenises: maximal [a-z0-9._-] runs, plus each dot/hyphen/
# underscore component. That is what makes a machine name and the account name inside it both match,
# and what makes a path ENDING in the token fire -- the shape a both-sides separator rule misses.
#
# ⚠ THE PUBLIC-HANDLE ADMIT (ruled 2026-09-22) is consulted HERE and in scanTokensReduced, and ONLY
# for the arms the DEFINITION marks [PUBLIC-HANDLE-ADMIT]. It reads the ENCLOSING WORD in the
# original text -- never the token, never the line -- and it is an EXACT hash of a whole word, so
# containment can never admit anything.
#
# WHY IT MUST READ THE ORIGINAL TEXT rather than the surface the arm fired on: the hits this ruling
# is about are pass-3 hits, and the reduced surface has NO word boundaries left by construction, so
# "the enclosing word" does not exist there. Going back to the text is what makes the decision a
# word decision in both passes.
#
# EVERY occurrence must be inside an admitted word, and at least one must exist. Both halves are
# load-bearing and both are controlled: a line carrying the handle AND the bare account name still
# refuses (some occurrence is not admitted), and a token that exists only across a fused line break
# -- inside no single word -- finds zero enclosing words and refuses, which is the safe direction.
#
# PERCENT-ESCAPES ARE SEPARATORS for the word split. The five README URLs this ruling was measured
# on spell the handle after `%20`, an encoded SPACE, so without this the enclosing word would be the
# two hex digits glued to the handle and the admit would miss the only surface it was ruled for.
# COST, stated: a percent-encoded PATH whose segment is an admitted handle is admitted by this arm.
# The structural profile/home arms are untouched and are the mitigation, exactly as the denied-token
# pass is the mitigation for the unicode-escape admit above.
#
# THE DOTTED MODULE-PATH SPELLING (COORD ruling 2026-10-03). A Go module path under an admitted
# handle reaches C# and NuGet with its slashes turned into DOTS: the namespace `go.github.com.<handle>`,
# the ID `nugetgo.github.com.<handle>.<module>`, the project file `github.com.<handle>.<module>.csproj`.
# The dot is a WORD character for the split above, so the enclosing word is the whole dotted path and
# never equals the handle. dottedHandleOK admits exactly that shape and nothing wider: the word is cut on
# dots, an admitted handle must be a WHOLE segment directly after a `github` `com` segment pair, and every
# segment that contains the denied literal must be such a segment. A different host, a segment that only
# CONTAINS the handle, or the bare literal in another segment of the same word still refuses.
function dottedHandleOK(w, tok,   n, S, j, mark, k) {
    if (index(w, ".") == 0) return 0
    n = split(w, S, ".")
    for (j = 1; j + 2 <= n; j++)
        if (S[j] == "github" && S[j + 1] == "com" && (S[j + 2] in ADM)) mark[j + 2] = 1
    for (k = 1; k <= n; k++)
        if (index(S[k], tok) > 0 && !(k in mark)) return 0
    return 1
}
# STATED RESIDUAL (the header, WHAT IS STILL NOT FOUND, 4). The walk below only ever meets an
# occurrence that stands INSIDE A WORD of the bare text. A spelling BROKEN by characters that are not
# its own is inside no word, so one admitted handle on the same text gives seen=1 ok=1 and answers
# for it -- on every box, and on the tool as it was before TEXT_DENIED. An admit that COUNTED
# occurrences here, read its words from tokenBuffer and kept admissions out of SEEN was made and
# taken back on 2026-10-06: it refused the handle first on a line after a line ending in a word
# character, and split the two kinds of box on the handle glued to the public-URL span. The self-test
# pins the residual and both of those (D1 h and i; D2 h1 and o). Queued for a design of its own.
function admitWord(tok, text,   src, n, W, i, w, seen, ok) {
    if (nADM == 0 || tok == "") return 0
    src = tolower(text)
    gsub(/%[0-9a-f][0-9a-f]/, " ", src)
    n = split(src, W, /[^a-z0-9_.-]+/)
    seen = 0; ok = 1
    for (i = 1; i <= n; i++) {
        w = W[i]
        if (w == "" || index(w, tok) == 0) continue
        seen++
        if (!(w in ADM) && !dottedHandleOK(w, tok)) ok = 0
    }
    return (seen > 0 && ok)
}
function checkTok(t, lineno, pass, text,   i) {
    if (t == "") return
    for (i = 1; i <= nT; i++) {
        if (t == TVAL[i]) {
            OCC[TSRC[i]]++
            if (ADMITARM[TSRC[i]] == 1 && admitWord(TVAL[i], text)) {
                EXC[TSRC[i] "\t" "public-handle"]++
                nPH++
                SEEN[lineno "\t" i] = 1
                if (pass == 2) SEEN[(lineno - 1) "\t" i] = 1
                continue
            }
            record(TSRC[i], pass, lineno, TVAL[i], text)
            SEEN[lineno "\t" i] = 1
            if (pass == 2) SEEN[(lineno - 1) "\t" i] = 1
        }
    }
}
function scanTokens(lineno, text, lo, pass, joinAt,   i, L, s, e, c, run, nc, k) {
    L = length(lo); i = 1
    while (i <= L) {
        c = substr(lo, i, 1)
        if (c ~ /[a-z0-9._-]/) {
            s = i
            while (i <= L && substr(lo, i, 1) ~ /[a-z0-9._-]/) i++
            e = i - 1
            if (!(pass == 2 && !(s <= joinAt && e > joinAt))) {
                run = substr(lo, s, e - s + 1)
                checkTok(run, lineno, pass, text)
                nc = split(run, CMP, /[._-]/)
                if (nc > 1) for (k = 1; k <= nc; k++) checkTok(CMP[k], lineno, pass, text)
            }
        } else i++
    }
}
# ⚠⚠ THE ALPHANUMERIC REDUCTION IS NOT `gsub`, AND THAT IS A MEASUREMENT ABOUT THE ENGINE RATHER
# THAN A STYLE CHOICE. `gsub(/[^a-z0-9]/, "", s)` is QUADRATIC IN length(s) under gawk 5.0.0 --
# roughly half the characters of any real line match, so the match count is O(L) and each one
# rebuilds the buffer. Timed directly, one string, nothing else in the program:
#
#     50,000 chars   3,615 ms        200,000 chars  61,446 ms
#    100,000 chars  13,856 ms        (2x the length, 4.4x the time)
#
# That is the whole of the hang this tool showed on JSON. MEASURED 2026-09-22 on the i7, on
# single-LINE fixtures, `entry` mode end to end, against a ~2.7 s fixed overhead:
#
#       5 KB  2.8 s      87 KB   9.4 s      362 KB  104.0 s
#      20 KB  2.9 s     177 KB  26.1 s      3.06 MB never returned (~2 h by extrapolation)
#
# ⚠ AND IT IS LINE LENGTH, NOT BYTES, NOT JSON. One-axis A/B on the SAME 177 KB: one line 27.8 s,
# folded to 1,769 lines 7.5 s. JSON is merely where a 3 MB single line occurs -- the pipeline's own
# `go2cs_test_results.json` is one line of 2.9 MB, which is how the leg met this. A 46 KB manifest
# with a longest line of 1,896 characters reads in 4.8 s, so ordinary committed JSON was never slow.
# Bisected by disabling one stage at a time on the 177 KB line: whole program 27.4 s, without the
# arm loop 27.8 s, without the ipv4 scan 28.4 s, WITHOUT THE TOKEN PASS 0.2 s; and inside that pass,
# with the reduction kept and its scan dropped 24.9 s, with the reduction dropped and its scan kept
# 0.5 s. One stage, named by subtraction.
#
# `split` on the COMPLEMENT class does the same work in ONE scan, and the pieces are then joined
# PAIRWISE -- `out = out piece` over O(L) pieces is the same quadratic by another route, while a
# balanced merge copies O(L log n). The two spellings are EXACTLY equivalent, and the reason is that
# the class is a SINGLE CHARACTER: `split(s, P, /[^a-z0-9]+/)` yields precisely the maximal
# alphanumeric runs, so concatenating them is the string with every non-alphanumeric removed.
# Controlled on a gnarly literal (separators, quotes, digits, a backslash) against the gsub it
# replaces -- identical output -- and on the three edges: empty, all-separator, all-alphanumeric.
function alnumOnly(s,   n, i, P, m, j) {
    n = split(s, P, /[^a-z0-9]+/)
    while (n > 1) {
        m = 0
        for (i = 1; i <= n; i += 2) { m++; P[m] = (i + 1 <= n) ? P[i] P[i + 1] : P[i] }
        for (j = m + 1; j <= n; j++) delete P[j]
        n = m
    }
    return (n == 1) ? P[1] : ""
}
# PASS 3 -- token arms only, over an alphanumerics-only reduction. Tokens of 4+ characters only: the
# reduction has no boundaries left, so a 3-character detector here would fire on ordinary prose.
function scanTokensReduced(lineno, red, keyline, text,   i, t) {
    for (i = 1; i <= nT; i++) {
        t = TVAL[i]
        if (length(t) < 4) continue
        if ((keyline "\t" i) in SEEN) continue
        gsub(/[^a-z0-9]/, "", t)
        if (t == "") continue
        if (index(red, t) > 0) {
            OCC[TSRC[i]]++
            # The admit is asked about the ORIGINAL literal, not the alnum-reduced one: the reduction
            # is how the arm FOUND the occurrence, and the word it sits in is a fact about the text.
            if (ADMITARM[TSRC[i]] == 1 && admitWord(TVAL[i], text)) {
                EXC[TSRC[i] "\t" "public-handle"]++
                nPH++
                SEEN[keyline "\t" i] = 1
                continue
            }
            record(TSRC[i], 3, lineno, TVAL[i], text)
            SEEN[keyline "\t" i] = 1
        }
    }
}
# Length-preserving blank, so blanking the public URL cannot shift a PASS-2 join offset.
function blankSpans(s, re,   out, rest) {
    out = ""; rest = s
    while (match(rest, re)) {
        out = out substr(rest, 1, RSTART - 1) sprintf("%*s", RLENGTH, "")
        rest = substr(rest, RSTART + RLENGTH)
    }
    return out rest
}
# THE ONE BUFFER THE TOKEN PASSES READ, and so the one buffer the WINDOW SCAN of TEXT_DENIED reads.
# The public URL is PUBLISHED ATTRIBUTION, not infrastructure: blanked before the token arms run, as
# a SPAN and never as a line, so a denied token elsewhere on that line still fires. PASS 1 and PASS 2
# tokenise this buffer and PASS 3 reduces it -- the lower-cased line or joined pair WITH THAT SPAN
# BLANKED, not the bare lower-cased text -- and the dump rule below writes exactly it. One function,
# three callers: a scan that read anything else would discover a word the passes then cannot see, or
# miss one they can.
function tokenBuffer(text,   b) {
    b = tolower(text)
    if ("public_url" in RE) b = blankSpans(b, RE["public_url"])
    return b
}

function scanAll(lineno, text, pass, joinAt,   a, arm, lo, blanked, red) {
    lo = tolower(text)
    for (a = 1; a <= nA; a++) {
        arm = ARM[a]
        if (RE[arm] == "@RUNTIME@") continue
        if (arm == "ipv4") { scanIpv4(lineno, text, lo, pass, joinAt); continue }
        # An arm marked [CONSULTED-ONLY] in the definition is an ADMIT SET or a decision input, not
        # a pattern with a standalone occurrence count. Scanning one directly is meaningless: the
        # placeholder set matches ANY SINGLE CHARACTER by its own "a single character is a stand-in,
        # not an account" rule, and read 32 occurrences on a clean line before this was added.
        if (CONSULTED[arm] == 1) continue
        scanArm(arm, lineno, text, lo, pass, joinAt)
    }
    if (nT > 0) {
        blanked = tokenBuffer(text)
        scanTokens(lineno, text, blanked, pass, joinAt)
        red = alnumOnly(blanked)
        scanTokensReduced(lineno, red, lineno, text)
    }
}

BEGIN {
    CHARS = " !\"#$%&()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[\\]^_`{|}~" "'"
    nA = 0; nT = 0; nH = 0; nADM = 0; nPH = 0
    if (PATFILE == "" || KEYS == "" || REPORT == "" || STATUS == "") { print "awk: missing -v" > "/dev/stderr"; exit 9 }
    while ((getline L < PATFILE) > 0) {
        # The definition is a .txt with no eol pin, so a Windows checkout materialises CRLF while a
        # Linux one materialises LF. Strip the CR here rather than relying on either: verify at the
        # layer the instrument actually reads, never at the one you assume it reads.
        sub(/\r$/, "", L)
        if (L ~ /^#/) continue
        if (L ~ /^[ \t]*$/) continue
        n = split(L, F, "\t")
        if (n < 3) continue
        nA++; ARM[nA] = F[1]; RE[F[1]] = F[2]; MD[F[1]] = F[3]; OCC[F[1]] = 0; HITS[F[1]] = 0
        CONSULTED[F[1]] = ((n >= 4) && (F[4] ~ /^\[CONSULTED-ONLY\]/)) ? 1 : 0
        # The CONVERTED-mode downgrade set, read from the DEFINITION rather than listed here. Not
        # anchored to the start of the note, because an arm may already carry another marker there.
        DOWNARM[F[1]] = ((n >= 4) && (F[4] ~ /\[CONVERTED-CONTEXT\]/)) ? 1 : 0
        # The UPSTREAM-FIXTURE downgrade set (COORD ruling 1, 2026-09-22), read from the DEFINITION for
        # the same reason: the four STRUCTURAL arms, which the Go gate skips on an upstream fixture.
        # Applied only when the shell classified THIS file as one -- see idc_is_upstream_fixture.
        FIXARM[F[1]] = ((n >= 4) && (F[4] ~ /\[FIXTURE-CONTEXT\]/)) ? 1 : 0
        # The PUBLIC-HANDLE ADMIT set, read from the DEFINITION rather than listed here, for the
        # reason the CONVERTED-CONTEXT set is: an arm is declared once, where every other property of
        # it is declared, and a widening that must reach TWO arms travels as a per-arm marker rather
        # than through a list every arm shares.
        ADMITARM[F[1]] = ((n >= 4) && (F[4] ~ /\[PUBLIC-HANDLE-ADMIT\]/)) ? 1 : 0
        DOWN[F[1]] = 0
    }
    close(PATFILE)
    if (nA < 1) { print "awk: patterns file yielded 0 arms" > "/dev/stderr"; exit 9 }
    # TWO files, one shape: the run-time literals this box derived, then the denied words the TEXT
    # itself supplied (the TEXT_DENIED arm; empty outside the strict modes).
    for (fi = 1; fi <= 2; fi++) {
        TF = (fi == 1) ? TOKFILE : TXTTOK
        if (TF == "") continue
        while ((getline L < TF) > 0) {
            sub(/\r$/, "", L)
            if (L ~ /^[ \t]*$/) continue
            n = split(L, F, "\t")
            if (n < 2) continue
            nT++; TSRC[nT] = F[1]; TVAL[nT] = tolower(F[2])
        }
        close(TF)
    }
    # The admitted public handles, one lowercased literal per line. Kept as a SET keyed by the whole
    # word, never as a list walked with index(): the membership test is the bound.
    # TWO files here too: the handles this box derived, then the words of the text that ARE ADMIT rows.
    for (fi = 1; fi <= 2; fi++) {
        AF = (fi == 1) ? ADMITFILE : TXTADM
        if (AF == "") continue
        while ((getline L < AF) > 0) {
            sub(/\r$/, "", L)
            if (L ~ /^[ \t]*$/) continue
            if (!(tolower(L) in ADM)) nADM++
            ADM[tolower(L)] = 1
        }
        close(AF)
    }
    # LONGEST FIRST, once, for maskLine. A contained literal must never be masked ahead of the
    # literal that contains it.
    for (i = 1; i <= nT; i++) TSORT[i] = TVAL[i]
    for (i = 1; i < nT; i++)
        for (j = i + 1; j <= nT; j++)
            if (length(TSORT[j]) > length(TSORT[i])) { t = TSORT[i]; TSORT[i] = TSORT[j]; TSORT[j] = t }
    # Reached only when the definition was read: an `exit` above skips it, and the dump rule's END
    # then writes no buffer count, which the shell refuses on.
    DUMPOK = 1
}

# DUMP MODE -- THE WINDOW SCAN'S BUFFERS (TEXT_DENIED, 2026-10-06). With -v DUMP=<file> this program
# censuses nothing: for each line it APPENDS the buffer the token passes would read for it, and for
# each adjacent pair the buffer they would read for the join -- the same rtrim, the same ltrim, the
# same both-halves-non-empty condition and the same tokenBuffer as the rule below, in this one file,
# so the two cannot drift apart. Each line is prefixed `=`; the scanner strips it. The count goes to
# STATUS so the shell can require that the scanner read every buffer written.
DUMP != "" {
    print "=" tokenBuffer($0) >> DUMP; nDump++
    if (NR > 1) {
        left = rtrim(prev); right = ltrim($0)
        if (length(left) > 0 && length(right) > 0) { print "=" tokenBuffer(left right) >> DUMP; nDump++ }
    }
    prev = $0
    next
}

{
    scanAll(NR, $0, 1, 0)
    if (NR > 1) {
        left = rtrim(prev); right = ltrim($0)
        if (length(left) > 0 && length(right) > 0) {
            joined = left right
            # scanAll reads the pair through all three passes: PASS 2 over the join and PASS 3 over
            # its reduction. (Until 2026-10-06 a second PASS 3 call followed here over the same
            # reduction of the same pair under the same line number -- it could find only what the
            # first had already recorded or admitted, and it cost a tokenBuffer and a reduction a
            # pair on every box that holds a literal.)
            scanAll(NR, joined, 2, length(left))
        }
    }
    prev = $0
}

END {
    if (DUMP != "") {
        close(DUMP)
        if (DUMPOK == 1) print "buffers=" (nDump + 0) > STATUS
        close(STATUS)
        exit
    }
    printf "  DECLARED SET (arms=%d, strict=%d, converted=%d -- every arm prints, so a zero is still looking):\n", nA, STRICT, CONVERTED > REPORT
    nDown = 0; nFix = 0
    for (a = 1; a <= nA; a++) if (MD[ARM[a]] == "refuse" && FIXARM[ARM[a]] == 1) nFix++
    for (a = 1; a <= nA; a++) {
        arm = ARM[a]
        if (CONSULTED[arm] == 1)
            printf "    %-7s %-20s occ=(consulted by another arm)\n", MD[arm], arm > REPORT
        else if (MD[arm] == "refuse" && !armRefuses(arm)) {
            # A DOWNGRADED arm prints its own mode word, so the declared set never reads as though a
            # refusing arm found nothing when what happened is that it was not allowed to refuse.
            # nDown counts the [CONVERTED-CONTEXT] set ONLY, so its meaning does not move on a fixture.
            if (DOWNARM[arm] == 1) nDown++
            printf "    %-7s %-20s occ=%-6d down=%d\n", "down", arm, OCC[arm], DOWN[arm] > REPORT
        }
        else if (MD[arm] == "refuse")
            printf "    %-7s %-20s occ=%-6d hits=%d\n", MD[arm], arm, OCC[arm], HITS[arm] > REPORT
        else
            printf "    %-7s %-20s occ=%-6d hits=-\n", MD[arm], arm, OCC[arm] > REPORT
    }
    if (CONVERTED == 1) {
        print "  DOWNGRADED IN CONVERTED MODE (Go's own test literals -- reported, never refusing):" > REPORT
        if (FIXTURE == 1) print "    (this file is an UPSTREAM FIXTURE by the Go gate's own predicate, so its structural arms are here too)" > REPORT
        nAny = 0
        for (a = 1; a <= nA; a++) {
            arm = ARM[a]
            if (MD[arm] == "refuse" && !armRefuses(arm)) {
                printf "    %s occ=%d would-have-refused=%d\n", arm, OCC[arm], DOWN[arm] > REPORT
                nAny++
            }
        }
        if (nAny == 0) print "    (none)" > REPORT
    }
    # The admitted count, stated on its own line as well as in the EXCLUSIONS tally, because it is
    # the one number that says "this run cleared something it would otherwise have refused". A zero
    # prints nothing: the arms' own hits=0 rows already say the arms found nothing to clear.
    if (nPH > 0)
        printf "  PUBLIC-HANDLE ADMITS: %d occurrence(s) cleared as the owner's ruled public handles (admit set size %d)\n", nPH, nADM > REPORT
    ne = 0
    for (k in EXC) ne++
    if (ne > 0) {
        print "  EXCLUSIONS (arm / reason / count -- what the arms saw and decided against):" > REPORT
        for (k in EXC) {
            split(k, KP, "\t")
            printf "    %-18s %-18s %d\n", KP[1], KP[2], EXC[k] > REPORT
        }
    }
    if (nH > 0) {
        print "  HITS (arm, pass, line, MASKED, fingerprint -- never a value, never the line):" > REPORT
        for (i = 1; i <= nH; i++) print HL[i] > REPORT
    }
    print "arms=" nA > STATUS
    print "hits=" nH > STATUS
    # How many lines this program was given to read. The shell refuses a census that read none of an
    # input that is not empty: hits=0 over no lines is not a reading.
    print "lines=" (NR + 0) > STATUS
    # The size of the downgrade set AS READ FROM THE DEFINITION. The shell refuses converted mode on
    # a zero: a mode whose whole content is a marker the definition no longer carries would otherwise
    # read exactly like entry and call itself converted.
    print "downarms=" nDown > STATUS
    print "fixarms=" nFix > STATUS
    for (a = 1; a <= nA; a++) if (DOWN[ARM[a]] > 0) print "down=" ARM[a] "|" DOWN[ARM[a]] > STATUS
    # Machine-readable, so a consumer (and the self-test) asserts on ARMS rather than on printed
    # prose. Arm names and counts only -- never a value.
    for (a = 1; a <= nA; a++) if (HITS[ARM[a]] > 0) print "hitarm=" ARM[a] > STATUS
    for (k in EXC) { split(k, KP, "\t"); print "exc=" KP[1] "|" KP[2] "|" EXC[k] > STATUS }
    # In hit order, for the awk probe only (-v PROBE=1); a census never writes these.
    if (PROBE == 1) for (i = 1; i <= nH; i++) print "hitat=" HAT[i] > STATUS
    close(REPORT); close(KEYS); close(STATUS)
}
IDCAWKEOF

# -------------------------------------------------------------------------------------------------
IDC_HITS=""
IDC_ARMS=""

idc_count_arms_independently() {
    # A SECOND reader of the same file, so "awk read the patterns" is proven rather than assumed.
    local n=0 line=""
    while IFS= read -r line || [ -n "$line" ]; do
        case "$line" in '#'*) continue ;; esac
        case "$line" in '') continue ;; esac
        case "$line" in *"	"*) n=$((n + 1)) ;; esac
    done < "$IDC_PATTERNS"
    echo "$n"
}

idc_run_awk() {
    # $1 input, $2 keys, $3 report, $4 status, $5 strict
    awk -v PATFILE="$IDC_PATTERNS" -v TOKFILE="$IDC_TOKFILE" -v ADMITFILE="$IDC_ADMITFILE" -v REPORT="$3" \
        -v TXTTOK="$IDC_TXTTOK" -v TXTADM="$IDC_TXTADM" \
        -v KEYS="$2" -v STATUS="$4" -v STRICT="$5" -v UNMASK="$IDC_UNMASK" \
        -v SHORT="${IDC_SHORT:-0}" -v CONVERTED="$IDC_CONVERTED" -v FIXTURE="${IDC_FIXTURE:-0}" \
        -f "$IDC_AWK" -- "$1"
}

# -------------------------------------------------------------------------------------------------
# THE AWK PROBE. The header says why; this is the mechanism. ONE run of the census awk -- the program
# above, this tool's own definition -- over a probe text of two files: the first read STRICT, the
# second in DELTA (the `STRICT=0` between them is awk's own operand assignment, so the mode changes
# where the second file starts and the line numbers run on). Every probe line stands between blank
# lines, so nothing joins but the one pair that is there to be joined.
#
#   line  the text carries                                   the known answer
#   1     an address                                         ipv4 hits, pass 1
#   3     three octets, and four with the last out of range  nothing
#   5     the loopback constant and a release literal        nothing; doc-constant, release-literal
#   7     a profile root with an account segment             profile_root hits
#   9     the same with a placeholder segment                nothing; placeholder-segment
#   11    a home prefix with an account segment              home_unix hits
#   13    a backslash share on a host                        unc_backslash hits
#   15    the same on a fleet nickname                       nothing; nickname-host
#   17    a unicode escape in the share's shape              nothing; escape-sequence
#   19    a forward-slash share on a host                    unc_slash hits
#   21    a URL with a scheme                                nothing
#   23    a host assignment                                  host_ctx hits
#   25    a prose colon after the same word                  nothing
#   27    the probe token as a word                          the token arm hits, pass 1
#   29-30 the token wrapped across an indented break         the token arm hits, pass 2
#   32    the token broken by a separator                    the token arm hits, pass 3
#   34    the token inside an admitted handle                nothing; public-handle
#   36    (delta) a package-prefixed version quad            nothing; prefix-ex
#   38    (delta) a toolchain-prefixed version quad          nothing; token-run
#   40    (delta) a quad beside a version word               nothing; version-context
#   42    (delta) an address with no context                 ipv4 hits
#   44    (delta) a position-map record's closure suffix     nothing; position-map-funclit
#
# THE ANSWER IS EXACT. IDC_AP_HITS is every hit in order -- arm|pass|line|fingerprint -- and the
# fingerprint is of the DECISION TOKEN, so an engine that reports another extent for a match answers
# wrong even where the arm fires. IDC_AP_EXC is every exclusion with its count, and no other may
# appear. The fingerprints are the reference awk's (gawk 5.0), measured equal on mawk 1.3.4 20200120.
# The token and the handle are the self-test's synthetic ones; nothing here is a real name, and the
# text is assembled from parts so that this file, which is itself censused, spells no shape.
#
# WHAT IT DOES NOT ASK, stated: the public_url span blank (asking it would mean spelling the span
# here; the SELF-TEST asks it, of a copy of the definition whose span is a synthetic path -- D2, o),
# the context arms that are only reported, and the two awk programs of TEXT_DENIED's own -- whose
# words the scanner and the hasher confirm, each under its own probe.
# -------------------------------------------------------------------------------------------------
IDC_AP_HITS="ipv4|1|1|7fe58600 profile_root|1|7|492a7c80 home_unix|1|11|492a7c80 unc_backslash|1|13|aff58aa4 unc_slash|1|19|aff58aa4 host_ctx|1|23|684e4170 RUNTIME_ACCOUNT|1|27|6e722118 RUNTIME_ACCOUNT|2|30|6e722118 RUNTIME_ACCOUNT|3|32|6e722118 ipv4|1|42|7fe58600"
IDC_AP_EXC="ipv4|doc-constant|1 ipv4|release-literal|1 profile_root|placeholder-segment|1 unc_backslash|nickname-host|1 unc_backslash|escape-sequence|1 RUNTIME_ACCOUNT|public-handle|1 ipv4|prefix-ex|1 ipv4|token-run|1 ipv4|version-context|1 ipv4|position-map-funclit|1"
IDC_AP_SUMMARY=""

idc_awk_banner() {
    # The awk's own first line about itself, for a refusal that must NAME it -- and never its path: a
    # path on this fleet is a «profile-path». -W version is mawk's spelling and gawk takes it too;
    # --version is the GNU one. The first line that is not the awk complaining about the flag is the
    # banner (a busybox awk says who it is on its second). stdin is /dev/null, so an awk that reads
    # the word as a program reads nothing and returns. Run on the way to a refusal and by the
    # self-test; a census that passes its probe never pays for it.
    local v="" line="" flag="" f="$IDC_TMP/awk.banner"
    for flag in "-W version" "--version"; do
        awk $flag > "$f" 2>&1 < /dev/null
        while IFS= read -r line || [ -n "$line" ]; do
            line="${line%$'\r'}"
            case "$line" in ''|awk:*|[Uu]sage*) continue ;; esac
            v="$line"; break
        done < "$f"
        if [ -n "$v" ]; then break; fi
    done
    v="$(printf '%s' "$v" | tr -cd 'A-Za-z0-9 .,:()_-')"
    v="${v:0:80}"
    if [ -z "$v" ]; then v="an awk that states no version"; fi
    printf '%s' "$v"
}

idc_awk_probe() {
    # Returns 0 with IDC_AP_SUMMARY set, or PRINTS the refusal -- the awk named, the first wrong
    # answer named by ARM -- and returns 1: the caller exits 2 on that. Nothing of the probe text is
    # ever printed.
    local ps="$IDC_TMP/probe.strict" pd="$IDC_TMP/probe.delta" tok="$IDC_TMP/probe.tok" adm="$IDC_TMP/probe.admit"
    local st="$IDC_TMP/probe.status" bs="" sl="/" pc="" rc=0 line="" got="" excs=" " nh=0 ne=0 nw=0 w="" why=""
    IDC_AP_SUMMARY=""
    if [ ! -f "$IDC_PATTERNS" ]; then
        echo "REFUSED(2): the patterns file is not beside this tool -- there is nothing to census with"
        return 1
    fi
    printf -v bs '\134'        # one backslash, built rather than written -- and built in this shell:
    printf -v pc '%%'          # a command substitution is a fork, measured 27 ms each on Git Bash
    : > "$ps"; : > "$pd"; : > "$tok"; : > "$adm"; : > "$st"
    printf 'RUNTIME_ACCOUNT\t%s\n' "zorbulax" > "$tok"
    printf '%s\n' "zorbulaxqueen" > "$adm"
    {
        printf 'the address %d.%d.%d.%d answered\n\n' 10 20 30 40
        printf 'not one: %d.%d.%d nor %d.%d.%d.%d here\n\n' 10 20 30 10 20 30 256
        printf 'the loopback %d.%d.%d.%d and the release %d.%d.%d.%d\n\n' 127 0 0 1 1 24 13 4
        printf 'built at C:%sUsers%s%s%sx\n\n' "$bs" "$bs" "sylvandeep" "$bs"
        printf 'built at C:%sUsers%s<user>%sx\n\n' "$bs" "$bs" "$bs"
        printf 'built at %shome%s%s%sx\n\n' "$sl" "$sl" "sylvandeep" "$sl"
        printf 'copied from %s%s%s%sshare%sx\n\n' "$bs" "$bs" "box7" "$bs" "$bs"
        printf 'from %s%sR-LAPTOP%sshare only\n\n' "$bs" "$bs" "$bs"
        printf 'stack: at go.fmt_package.printArg(%s%su0436%s%su0060 p)\n\n' "$bs" "$bs" "$bs" "$bs"
        printf 'see %s%s%s%sshare%sx for the log\n\n' "$sl" "$sl" "box7" "$sl" "$sl"
        printf 'see https:%s%sexample.invalid%sshare%sx for it\n\n' "$sl" "$sl" "$sl" "$sl"
        printf 'HOST=%s\n\n' "box7lab"
        printf 'the host: a fleet box\n\n'
        printf 'owner column reads %s here\n\n' "zorbulax"
        printf 'owner column reads zorb\n    ulax here\n\n'
        printf 'row names ab%s-%s2 here\n\n' "zorbul" "ax"
        printf 'browse at https:%s%sexample.invalid%spackages?q=go2cs%s20%s today\n\n' "$sl" "$sl" "$sl" "$pc" "zorbulaxqueen"
    } > "$ps"
    {
        printf 'older re-creations of nuget-%d.%d.%d.%d, their targets on origin.\n\n' 1 23 1 7
        printf 'the pinned toolchain is go%d.%d.%d.%d on this box\n\n' 1 24 13 3
        printf 'linking the %d.%d.%d.%d validation page that a reader follows\n\n' 2 24 13 3
        printf 'the box answered on %d.%d.%d.%d last night\n\n' 10 20 30 40
        printf '[assembly: go.GoPositionMap("x.go", "x.cs", "abc", "3-9:%d.%d.%d.%d")]\n' 1 2 3 4
    } > "$pd"
    awk -v PATFILE="$IDC_PATTERNS" -v TOKFILE="$tok" -v ADMITFILE="$adm" -v TXTTOK="" -v TXTADM="" \
        -v REPORT="$IDC_TMP/probe.report" -v KEYS="$IDC_TMP/probe.keys" -v STATUS="$st" -v STRICT=1 -v UNMASK=0 \
        -v SHORT=0 -v CONVERTED=0 -v FIXTURE=0 -v PROBE=1 -f "$IDC_AWK" -- "$ps" STRICT=0 "$pd" > /dev/null 2>&1
    rc=$?
    if [ "$rc" -ne 0 ]; then
        echo "REFUSED(2): the awk probe: this box's awk ($(idc_awk_banner)) exited $rc on the probe text -- it cannot run the census program or compile the definition's expressions, so no arm was asked anything and nothing was censused"
        return 1
    fi
    while IFS= read -r line || [ -n "$line" ]; do
        line="${line%$'\r'}"
        case "$line" in
            hitat=*) got="$got ${line#hitat=}"; nh=$((nh + 1)) ;;
            exc=*)   excs="$excs${line#exc=} "; ne=$((ne + 1)) ;;
        esac
    done < "$st"
    got="${got# }"
    if [ "$got" != "$IDC_AP_HITS" ]; then
        for w in $IDC_AP_HITS; do
            case " $got " in *" $w "*) : ;; *) why="the ${w%%|*} arm did not fire on its probe line, or fired on another extent"; break ;; esac
        done
        if [ -z "$why" ]; then
            for w in $got; do
                case " $IDC_AP_HITS " in *" $w "*) : ;; *) why="the ${w%%|*} arm fired where it must not"; break ;; esac
            done
        fi
        if [ -z "$why" ]; then why="the hits came back in another order, or more than once"; fi
    fi
    if [ -z "$why" ]; then
        for w in $IDC_AP_EXC; do
            nw=$((nw + 1))
            case "$excs" in *" $w "*) : ;; *) why="the ${w%|*} exclusion did not fire exactly once on its probe line"; break ;; esac
        done
        if [ -z "$why" ] && [ "$ne" -ne "$nw" ]; then why="an exclusion fired that the probe text does not carry"; fi
    fi
    if [ -n "$why" ]; then
        echo "REFUSED(2): the awk probe: this box's awk ($(idc_awk_banner)) did not give the known answer -- $why. An awk that cannot run an arm reads every text CLEAN on that arm, so nothing was censused. (The same refusal means the definition this tool read is not the one it was written against.)"
        return 1
    fi
    IDC_AP_SUMMARY="known answer given -- $nh hits and $ne exclusions over the probe text"
    return 0
}

idc_census() {
    local input="$1" label="$2" keys="$3" strict="$4"
    local report="$IDC_TMP/report.out" status="$IDC_TMP/status.out" rc=0 k="" v="" expect=""

    if [ ! -f "$IDC_PATTERNS" ]; then
        echo "REFUSED(2): the patterns file is not beside this tool -- there is nothing to census with"
        exit 2
    fi
    if [ ! -r "$input" ]; then
        echo "REFUSED(2): $IDC_PROG cannot read the input for '$label' -- it cannot know, so it does not pass"
        exit 2
    fi
    # NO CENSUS WITHOUT THE AWK PROBE. The dispatcher asks it first for every mode it knows; this is
    # the same question asked here if nothing has asked it yet in this process, so a mode added later
    # cannot reach a census around it. Once per process.
    if [ -z "$IDC_AP_SUMMARY" ]; then idc_awk_probe || exit 2; fi

    # ONE COPY, AND EVERY STEP READS THAT COPY (2026-10-06) -- the buffer awk, the candidate awk and
    # the census awk. It is the caller's bytes with one change, and it closes two ways a text was
    # read differently by different readers:
    #   (1) A NUL BYTE BECOMES \001. mawk (1.3.4 20200120, Ubuntu 20.04) reads a line that carries a
    #       NUL as memory that is not the line's: tolower() returned the right LENGTH and a wrong
    #       TAIL, so a whole denied word after a NUL, UTF-16 text and an address after a NUL read
    #       CLEAN there (rc 0) and REFUSED (rc 1) under gawk -- with the awk probe passed. \001 is to
    #       every expression here what \000 is to gawk: a control byte that is no letter, no digit, no
    #       separator of a run and no white space. So the verdict gawk gave is kept, and every awk
    #       now gives it. An input with no NUL is copied unchanged.
    #   (2) THE PATH IS THIS TOOL'S OWN. awk takes an operand shaped name=value as an ASSIGNMENT and
    #       reads no file: `entry w=1.md` read CLEAN over a denied word while `entry ./w=1.md`, the
    #       same bytes, refused. A path under the temp directory is absolute and is never that shape.
    local work="$IDC_TMP/input.read" nlines=""
    if ! tr '\000' '\001' < "$input" > "$work"; then
        echo "REFUSED(2): $IDC_PROG could not copy the input for '$label' to read it -- it cannot know, so it does not pass"
        exit 2
    fi
    input="$work"

    # STRICT ONLY: entry, subject and converted all arrive here with strict=1, and `tree` with 0. In
    # delta the arm is NOT RUN and its line says so -- its occ=0 in the declared set below is then not
    # a reading, and must not look like one.
    if [ "$strict" = "1" ]; then
        idc_text_denied "$input"
    else
        : > "$IDC_TXTTOK"; : > "$IDC_TXTADM"
        IDC_TXTSUMMARY="$IDC_TXTARM NOT RUN -- delta mode is a reading, not the gate; the strict modes (entry, subject, converted) run it"
    fi

    : > "$report"; : > "$keys"; : > "$status"
    idc_run_awk "$input" "$keys" "$report" "$status" "$strict"
    rc=$?
    if [ "$rc" -ne 0 ]; then
        echo "REFUSED(2): the census awk exited $rc on '$label' -- an instrument failure is not a clean read"
        exit 2
    fi

    IDC_ARMS=""; IDC_HITS=""; IDC_DOWNARMS=""; IDC_FIXARMS=""
    while IFS='=' read -r k v; do
        case "$k" in
            arms) IDC_ARMS="$v" ;;
            hits) IDC_HITS="$v" ;;
            lines) nlines="${v%$'\r'}" ;;
            downarms) IDC_DOWNARMS="$v" ;;
            fixarms) IDC_FIXARMS="$v" ;;
        esac
    done < "$status"

    # THE CENSUS AWK MUST HAVE READ THE TEXT. An awk that exits 0 having read no line of an input
    # that is not empty reports hits=0 exactly as a clean text does -- the same property the buffer
    # awk is held to in idc_scan_text, asked here of every mode, delta included.
    case "$nlines" in
        ''|*[!0-9]*|0)
            if [ -s "$input" ]; then
                echo "REFUSED(2): the census awk read no line of '$label', and the input is not empty -- a census that read nothing is not a clean one"
                exit 2
            fi ;;
    esac

    # A converted-mode run whose downgrade set is EMPTY has read a definition that no longer declares
    # one. It would behave exactly like entry while printing "MODE converted" over it, which is the
    # false-green shape this instrument exists to refuse -- so it is an instrument failure, not a run.
    if [ "$IDC_CONVERTED" = "1" ]; then
        case "${IDC_DOWNARMS:-0}" in
            ''|0) echo "REFUSED(2): converted mode read 0 arms marked [CONVERTED-CONTEXT] in $(basename -- "$IDC_PATTERNS") -- the mode and its definition disagree, so it does not pass"; exit 2 ;;
        esac
        # The same refusal for the fixture set: a file classified as an upstream fixture against a
        # definition that marks no structural arm would read exactly like the pre-ruling mode.
        case "${IDC_FIXARMS:-0}" in
            ''|0) echo "REFUSED(2): converted mode read 0 arms marked [FIXTURE-CONTEXT] in $(basename -- "$IDC_PATTERNS") -- the mode and its definition disagree, so it does not pass"; exit 2 ;;
        esac
    fi

    if [ -z "$IDC_ARMS" ] || [ -z "$IDC_HITS" ]; then
        echo "REFUSED(2): the census produced no arm/hit count for '$label' -- an empty reading is not a clean one"
        exit 2
    fi
    case "$IDC_ARMS" in ''|0) echo "REFUSED(2): the census declared $IDC_ARMS arms"; exit 2 ;; esac
    expect="$(idc_count_arms_independently)"
    if [ "$IDC_ARMS" != "$expect" ]; then
        echo "REFUSED(2): the census read $IDC_ARMS arms where this script counts $expect in the same file -- the instrument and its definition disagree"
        exit 2
    fi

    echo "IDENTIFIER CENSUS -- $label"
    if [ "$IDC_CONVERTED" = "1" ]; then
        echo "  MODE converted -- a tracked CONVERTED GO TEST SOURCE. The $IDC_DOWNARMS literal-shaped arm(s)"
        echo "    marked [CONVERTED-CONTEXT] are REPORTED with their counts and cannot refuse."
        if [ "${IDC_FIXTURE:-0}" = "1" ]; then
            echo "    UPSTREAM FIXTURE by the Go gate's own predicate (fleetIsUpstreamFixture, or fleetEmbedPayloads"
            echo "    for a //go:embed payload): the $IDC_FIXARMS structural arm(s) marked [FIXTURE-CONTEXT] are reported"
            echo "    too and cannot refuse, as the gate skips its structural pass here. The denied-token and run-time"
            echo "    arms refuse exactly as in entry."
        else
            echo "    NOT an upstream fixture by the Go gate's predicate: every other arm refuses exactly as in entry."
        fi
        echo "    THE GATE OF RECORD for the tracked tree is repoguard's"
        echo "    TestNoFleetIdentifiersInTrackedFiles (go test ./internal/repoguard/ under src/go2cs)."
    fi
    echo "  patterns: $(basename -- "$IDC_PATTERNS")   $IDC_HASHSUMMARY"
    echo "  token file: $IDC_TOKFILE_PRESENT   run-time arms: $IDC_TOKSUMMARY"
    echo "  public handles: $IDC_ADMITSUMMARY"
    echo "  text-hash arm: $IDC_TXTSUMMARY"
    echo "  awk probe: $IDC_AP_SUMMARY"
    if [ -n "${IDC_INERT:-}" ]; then printf '%b' "$IDC_INERT"; fi
    if [ "$IDC_UNMASK" = "1" ]; then echo "  *** --unmask IS ON. LOCAL CONSOLE ONLY. NOTHING BELOW MAY BE PASTED INTO A POST. ***"; fi
    cat -- "$report"
    echo "  hits=$IDC_HITS"
    return 0
}

# -------------------------------------------------------------------------------------------------
idc_mode_entry() {
    [ "$#" -eq 1 ] || idc_misuse "entry takes exactly one argument, the entry file"
    idc_build_tokens
    idc_census "$1" "entry $(basename -- "$1")" "$IDC_TMP/keys.entry" 1
    case "$IDC_HITS" in
        ''|0) echo "CLEAN: no identifier arm fired."; return 0 ;;
        *)    echo "REFUSED(1): $IDC_HITS hit(s). The arms, passes and line numbers are above; the values are not."; return 1 ;;
    esac
}

# -------------------------------------------------------------------------------------------------
# CONVERTED MODE -- ELIGIBILITY IS PART OF THE GATE, NOT A CONVENIENCE.
#
# The downgrade is justified by ONE property of the input: the literals are Go's own test data, which
# the lane did not author and cannot rewrite. A mode that took that property on the caller's word
# would be a general "please do not refuse" switch, and the first hand-written file passed through it
# would carry a real identifier past a gate that printed CLEAN. So the file must BE one, by its path,
# and anything else is refused with rc 2 and sent to `entry`.
#
# The set is the `-tests` pipeline's own artifacts under src/core/, plus the proof pages. It is
# deliberately NARROWER than the Go guard's fleetIsUpstreamFixture (which admits any /testdata/ and
# any .test): this instrument runs before a push, and a narrow door is re-widened by a ruling.
#
# THE REFUSAL PRINTS A BASENAME AND NEVER THE PATH. A caller can pass an absolute path, and an
# absolute path on this fleet is exactly a «profile-path» -- a refusal that echoed its argument would
# spell into the terminal, and into whatever log the caller keeps, the class this tool exists to keep
# out of them.
idc_converted_eligible() {
    local p="" n=""
    p="$(printf '%s' "$1" | tr '\\' '/')"
    case "$p" in
        docs/validation/current/*.md|*/docs/validation/current/*.md) return 0 ;;
    esac
    case "$p" in
        src/core/*|*/src/core/*) : ;;
        *) return 2 ;;
    esac
    n="${p##*/}"
    case "$n" in
        *_test.cs)                     return 0 ;;
        package_test_info.cs)          return 0 ;;
        package_info_internal_test.cs) return 0 ;;
        go2cs_test_host.cs)            return 0 ;;
        *.tests.csproj)                return 0 ;;
    esac
    # A //go:embed payload is Go's own bytes by the same argument, and its PATH cannot say so -- its
    # project file does. See idc_is_embed_payload.
    idc_is_embed_payload "$1" && return 0
    return 3
}

# idc_is_upstream_fixture PATH -- fleetIsUpstreamFixture, MIRRORED EXACTLY (COORD ruling 1,
# 2026-09-22). The Go gate of record for the tracked tree (src/go2cs/internal/repoguard/
# fleetIdentifierCensus_test.go) lower-cases the path and treats it as converted-upstream or captured
# test DATA when it contains "/testdata/" or ends in "_test.cs", "_test.cs.auto" or ".test"; on those
# it skips its STRUCTURAL pass (profile/home and network-path) and still runs the denied-token pass.
# Its own reason, verbatim: "Skipping both would be the blind spot; skipping neither is 2,500 false
# positives." This census refused there while the gate passed, which made the pre-push census
# STRICTER than the gate on exactly the files batches re-emit; the mirror is what closes that.
#
# ⚠ MIRROR, NEVER WIDEN. All four tests are carried even though converted mode's own door admits
# only two of them (an .auto or .test name never reaches here): a copy reduced to "the cases that can
# occur" is a copy that silently diverges the day the door widens. And every change here must move
# with the Go function, or the two gates disagree again -- which is the defect this replaced.
#
# One stated gap: tr folds ASCII only, where strings.ToLower is Unicode-aware. They differ only on a
# path with a non-ASCII capital, and the tracked tree carries none.
idc_is_upstream_fixture() {
    local p
    p="$(printf '%s' "$1" | tr '[:upper:]' '[:lower:]')"
    case "$p" in
        */testdata/*)                      return 0 ;;
        *_test.cs|*_test.cs.auto|*.test)   return 0 ;;
    esac
    return 1
}

# idc_is_embed_payload PATH -- fleetEmbedPayloads, MIRRORED (COORD, H10 close, 2026-09-23). The Go
# gate admits a //go:embed PAYLOAD the way it admits testdata: Go 1.24's
# internal/trace/traceviewer/static/trace_viewer_full.html carries a JavaScript regex whose escaped
# character class reads as a UNC host, and a payload is the one way Go's own bytes reach the corpus
# outside a testdata directory. The Go side is src/go2cs/internal/repoguard/fleetIdentifierCensus_test.go
# (fleetEmbedPayloads); every change here moves with it.
#
# THE SAME DERIVATION, NOT A PATH LIST: the file is a payload when a project file in one of its own
# ancestor directories, at or below src/core/, carries the item the converter mints for it --
#     <EmbeddedResource Include="<path relative to that csproj, XML-escaped>" LogicalName="go.embed/..." />
# -- which is exactly an Include that resolves INSIDE the csproj's directory. A testdata path is not
# counted here (the fixture predicate above already admits it), as the Go derivation does not count it.
#
# TRACKED, where that can be asked: inside a git work tree, the project file AND the payload must both
# be tracked, as the Go gate reads only tracked files. Outside one (the self-test's temp tree) there is
# no index to ask and the files on disk are read.
#
# STRICTER THAN THE GO SIDE, stated and allowed (mirror, never widen): a path carrying `.` or `..`
# segments is never a payload here, and an Include must equal the path byte for byte after unescaping
# where Go compares it after path.Clean.
idc_is_embed_payload() {
    local p="" dir="" rel="" proj="" needle="" found=1
    p="$(printf '%s' "$1" | tr '\\' '/')"
    case "$p" in
        src/core/*|*/src/core/*) : ;;
        *) return 1 ;;
    esac
    case "/$p/" in
        */./*|*/../*) return 1 ;;
    esac
    idc_is_upstream_fixture "$p" && return 1
    [ -f "$p" ] || return 1
    idc_is_tracked "$p" || return 1
    dir="${p%/*}"
    rel="${p##*/}"
    while :; do
        for proj in "$dir"/*.csproj; do
            [ -f "$proj" ] || continue
            idc_is_tracked "$proj" || continue
            # The Include as the emitter writes it: XML-escaped, & first.
            needle="$(printf '%s' "$rel" | sed -e 's/&/\&amp;/g' -e 's/</\&lt;/g' -e 's/>/\&gt;/g' -e 's/"/\&quot;/g' -e "s/'/\&apos;/g")"
            needle="<EmbeddedResource Include=\"$needle\" LogicalName=\"go.embed/"
            if awk -v n="$needle" '{ i = index($0, n); if (i) { r = substr($0, i + length(n)); if (r ~ /^[^"]+" \/>/) { f = 1; exit } } } END { exit f ? 0 : 1 }' "$proj"; then
                found=0
                break 2
            fi
        done
        case "$dir" in
            src/core|*/src/core) break ;;
        esac
        rel="${dir##*/}/$rel"
        dir="${dir%/*}"
    done
    return "$found"
}

# idc_is_tracked PATH -- 0 when PATH is tracked or there is no work tree to ask (see above), 1 when a
# work tree holds it untracked.
idc_is_tracked() {
    local d="" b=""
    d="${1%/*}"; b="${1##*/}"
    [ "$(git -C "$d" rev-parse --is-inside-work-tree 2>/dev/null)" = "true" ] || return 0
    git -C "$d" ls-files --error-unmatch -- "$b" >/dev/null 2>&1
}

idc_mode_converted() {
    [ "$#" -ge 1 ] || idc_misuse "converted takes one or more converted-test-source files"
    local f="" bad=0 total=0 rc=0
    for f in "$@"; do
        idc_converted_eligible "$f"; rc=$?
        case "$rc" in
            2) echo "REFUSED(2): '$(basename -- "$f")' is not under src/core/ -- converted mode reads only tracked converted test sources; census it with \`entry\`."; bad=1 ;;
            3) echo "REFUSED(2): '$(basename -- "$f")' is not a converted test artifact (*_test.cs, package_test_info.cs, go2cs_test_host.cs, *.tests.csproj, docs/validation/current/*.md, or a //go:embed payload its csproj names) -- census it with \`entry\`."; bad=1 ;;
        esac
    done
    # Every path is judged before any is censused: a mode that refused halfway would leave the caller
    # reading a clean verdict for the files that happened to sort first.
    [ "$bad" -eq 0 ] || exit 2

    IDC_CONVERTED=1
    idc_build_tokens
    for f in "$@"; do
        # PER FILE, because the Go gate decides per path: one run can mix fixtures and pages.
        if idc_is_upstream_fixture "$f" || idc_is_embed_payload "$f"; then IDC_FIXTURE=1; else IDC_FIXTURE=0; fi
        idc_census "$f" "converted $(basename -- "$f")" "$IDC_TMP/keys.converted" 1
        total=$((total + ${IDC_HITS:-0}))
    done
    case "$total" in
        0) echo "CLEAN: no refusing arm fired. The downgraded arms' counts are above and are NOT a refusal."; return 0 ;;
        *) echo "REFUSED(1): $total hit(s) from arms that refuse in converted mode too. The arms, passes and line numbers are above; the values are not."; return 1 ;;
    esac
}

idc_mode_subject() {
    [ "$#" -eq 1 ] || idc_misuse "subject takes exactly one argument, the subject string"
    idc_build_tokens
    printf '%s\n' "$1" > "$IDC_TMP/subject"
    idc_census "$IDC_TMP/subject" "subject" "$IDC_TMP/keys.subject" 1
    case "$IDC_HITS" in
        ''|0) echo "CLEAN: no identifier arm fired."; return 0 ;;
        *)    echo "REFUSED(1): $IDC_HITS hit(s) in the COMMIT SUBJECT. A post censuses BOTH surfaces."; return 1 ;;
    esac
}

idc_mode_tree() {
    [ "$#" -eq 2 ] || idc_misuse "tree takes two arguments, <file> and <baseline-sha>"
    local file="$1" base="$2" cur="$IDC_TMP/cur" old="$IDC_TMP/old" curhits="" basehits="" added="" k="" v=""

    if ! git rev-parse --git-dir >/dev/null 2>&1; then
        idc_misuse "tree mode must run inside a git repository"
    fi
    if [ -f "$file" ]; then
        cp -- "$file" "$cur" || idc_misuse "could not read $file from the worktree"
    else
        git show "HEAD:$file" > "$cur" 2>/dev/null || idc_misuse "$file is neither on disk nor at HEAD"
    fi
    if ! git show "$base:$file" > "$old" 2>/dev/null; then
        echo "REFUSED(2): the baseline $base:$file could not be read -- an unreadable baseline is not 'no pre-existing hits'"
        exit 2
    fi

    idc_build_tokens
    echo "DELTA CENSUS -- $file"
    echo "  baseline: $base"
    echo "  ⚠ THIS IS A READING, NOT THE GATE. The pre-push gate is entry + subject (strict). The"
    echo "    baseline above MUST be the tip blob as fetched IMMEDIATELY BEFORE the append -- never a"
    echo "    lane's last-read sha. Anything landed by another lane in between is counted as ADDED by"
    echo "    this post, and a clean post is refused for someone else's entries."
    echo
    idc_census "$old" "BASELINE $base:$file" "$IDC_TMP/keys.base" 0
    basehits="$IDC_HITS"
    echo
    idc_census "$cur" "CURRENT $file" "$IDC_TMP/keys.cur" 0
    curhits="$IDC_HITS"
    echo

    awk -v BK="$IDC_TMP/keys.base" -v STATUSF="$IDC_TMP/delta.status" '
        BEGIN { while ((getline L < BK) > 0) { n = split(L, F, "\t"); if (n >= 3) B[F[1] "\t" F[2]] += F[3] } close(BK) }
        { n = split($0, F, "\t"); if (n < 3) next; C[F[1] "\t" F[2]] += F[3] }
        END {
            added = 0
            for (k in C) {
                split(k, KP, "\t")
                pre = (k in B) ? B[k] : 0
                PRE[KP[1]] += (pre < C[k]) ? pre : C[k]
                d = C[k] - pre
                if (d > 0) { added += d; ADD[KP[1]] += d }
            }
            print "  PRE-EXISTING ON THIS SURFACE (not this post to fix, and not silent):"
            any = 0
            for (a in PRE) if (PRE[a] > 0) { printf "    %-18s %d\n", a, PRE[a]; any = 1 }
            if (any == 0) print "    (none)"
            print "  ADDED BY WHAT IS BEING POSTED:"
            any = 0
            for (a in ADD) if (ADD[a] > 0) { printf "    %-18s %d\n", a, ADD[a]; any = 1 }
            if (any == 0) print "    (none)"
            print "added=" added > STATUSF
            close(STATUSF)
        }
    ' "$IDC_TMP/keys.cur" > "$IDC_TMP/delta.report"
    if [ "$?" -ne 0 ]; then
        echo "REFUSED(2): the delta awk failed -- an instrument failure is not a clean read"
        exit 2
    fi
    cat -- "$IDC_TMP/delta.report"

    while IFS='=' read -r k v; do
        case "$k" in added) added="$v" ;; esac
    done < "$IDC_TMP/delta.status"
    if [ -z "$added" ]; then
        echo "REFUSED(2): the delta produced no count -- an empty reading is not a clean one"
        exit 2
    fi
    echo "  baseline hits=$basehits  current hits=$curhits  added=$added"
    case "$added" in
        ''|0) echo "CLEAN: this surface adds no identifier. Pre-existing hits are reported above and are NOT a refusal."; return 0 ;;
        *)    echo "REFUSED(1): $added hit(s) ADDED to $file by what is being posted."; return 1 ;;
    esac
}

# =================================================================================================
# SELF-TEST. The triad: PLANTS, KNOWN NEGATIVES, DECLARED SET.
#
# Every plant is assembled through printf from synthetic parts, so this SOURCE reads as a template
# while the runtime string is real-looking; every plant is written to a temp file and NEVER echoed;
# and the output names the ARM and PASS/FAIL only.
#
# Each case declares the EXACT arm set it must fire, and the test asserts the fired set EQUALS it.
# That is stronger than "catchable by exactly one arm" and gives the same property: a control that
# two arms can catch cannot tell you either one works. Where a plant legitimately fires two arms, it
# says so and both are required.
#
# The admit sets are controlled in BOTH DIRECTIONS. An admit-only control reads GREEN on a dead arm,
# so for every nickname/placeholder case that must PASS there is a sibling that must REFUSE.
# =================================================================================================
IDC_ST_PASS=0
IDC_ST_FAIL=0
# NOT APPLICABLE ON THIS BOX -- a third count, and never either of the other two. A case whose PREMISE
# is a fact about the box (it derives a name the fleet has denied) cannot be run on a box where that
# is false: counting it a FAIL calls a sound census broken on every generic box, for ever, and teaches
# each lane's tool to wave one red through; counting it a PASS is a skip read as a green. So it is
# counted on its own line, printed with its reason every time, and the tally line keeps its exact
# shape (`SELF-TEST: pass=N fail=M`), which every lane's tool parses.
IDC_ST_NA=0

idc_st_na() {
    # $1 name, $2 reason.
    printf '  N/A   %-48s %s\n' "$1" "$2"; IDC_ST_NA=$((IDC_ST_NA + 1))
}

idc_st_fired() {
    # The REFUSE-arm hit set, read from the status file the census writes -- not from printed prose,
    # and not by eye. A context arm is not in it by construction: a context arm cannot refuse.
    local status="$1" out="" line=""
    while IFS= read -r line; do
        case "$line" in hitarm=*) out="$out ${line#hitarm=}" ;; esac
    done < "$status"
    printf '%s' "${out# }"
}

idc_st_exc() {
    # $1 name, $2 "arm|reason", $3 status file. Asserts the exclusion FIRED at least once -- the
    # admit direction of an admit set, which an admit-only control cannot distinguish from a dead arm.
    local name="$1" want="$2" status="$3" got="0" line=""
    while IFS= read -r line; do
        case "$line" in
            "exc=$want|"*) got="${line##*|}" ;;
        esac
    done < "$status"
    case "$got" in
        ''|0) printf '  FAIL  %-48s exclusion %s never fired\n' "$name" "$want"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
        *)    printf '  PASS  %-48s exclusion %s fired %s\n' "$name" "$want" "$got"; IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
    esac
}

idc_st_case() {
    # $1 name, $2 expected arm set, $3 file, $4 strict
    local name="$1" expect="$2" f="$3" strict="$4" got="" rc=0
    local report="$IDC_TMP/st.report" status="$IDC_TMP/st.status" keys="$IDC_TMP/st.keys"
    : > "$report"; : > "$status"; : > "$keys"
    idc_run_awk "$f" "$keys" "$report" "$status" "$strict"
    rc=$?
    if [ "$rc" -ne 0 ]; then
        printf '  FAIL  %-48s instrument exited %d\n' "$name" "$rc"
        IDC_ST_FAIL=$((IDC_ST_FAIL + 1)); return 0
    fi
    got="$(idc_st_fired "$status")"
    if [ "$got" = "$expect" ]; then
        printf '  PASS  %-48s arms{%s}\n' "$name" "$got"
        IDC_ST_PASS=$((IDC_ST_PASS + 1))
    else
        printf '  FAIL  %-48s expected arms{%s} got arms{%s}\n' "$name" "$expect" "$got"
        IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi
    return 0
}

idc_st_assert_absent() {
    # $1 name, $2 needle, $3 file. The needle is NEVER echoed -- only the count is.
    local name="$1" needle="$2" f="$3" c=""
    c="$(grep -c -F -- "$needle" "$f" 2>/dev/null)"
    case "$c" in
        ''|0) printf '  PASS  %-48s occurrences=0\n' "$name"; IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
        *)    printf '  FAIL  %-48s occurrences=%s -- the output SPELLED it\n' "$name" "$c"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
    esac
}
idc_st_rc() {
    # $1 name, $2 expected rc, $3 actual rc. An exit code is the only thing a caller wires `|| exit`
    # to, so a mode's verdict is asserted on IT and never on the prose above it.
    local name="$1" want="$2" got="$3"
    if [ "$got" = "$want" ]; then
        printf '  PASS  %-48s exit=%s\n' "$name" "$got"; IDC_ST_PASS=$((IDC_ST_PASS + 1))
    else
        printf '  FAIL  %-48s exit=%s (expected %s)\n' "$name" "$got" "$want"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi
}
idc_st_assert_present() {
    local name="$1" needle="$2" f="$3" c=""
    c="$(grep -c -F -- "$needle" "$f" 2>/dev/null)"
    case "$c" in
        ''|0) printf '  FAIL  %-48s occurrences=0 -- the check itself is dead\n' "$name"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
        *)    printf '  PASS  %-48s occurrences=%s\n' "$name" "$c"; IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
    esac
}
idc_st_scan() {
    # $1 name, $2 scanner, $3 file, $4 rows ("LEN:DIGEST ..."), $5 the ONE word the scan must name,
    # or "" if it must name nothing. THE SCAN ALONE -- idc_scan_text, no census and no candidate
    # route -- so a pass is the scan having found THIS shape, not a literal another line supplied.
    # The word is compared and never printed.
    local name="$1" want="$5" n=0 got="" line="" IDC_WINDOWER="$2"
    if ! idc_scan_text "$3" "$4" > /dev/null 2>&1; then
        printf '  FAIL  %-48s the scan did not run\n' "$name"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)); return 0
    fi
    while IFS= read -r line || [ -n "$line" ]; do n=$((n + 1)); got="$line"; done < "$IDC_WS_WORDS"
    if [ -z "$want" ] && [ "$n" -eq 0 ] && [ "${IDC_WS_WINDOWS:-0}" -gt 0 ]; then
        printf '  PASS  %-48s words=0 windows=%s\n' "$name" "$IDC_WS_WINDOWS"; IDC_ST_PASS=$((IDC_ST_PASS + 1))
    elif [ -n "$want" ] && [ "$n" -eq 1 ] && [ "$got" = "$want" ]; then
        printf '  PASS  %-48s words=1, and it is the row\n' "$name"; IDC_ST_PASS=$((IDC_ST_PASS + 1))
    else
        printf '  FAIL  %-48s words=%s windows=%s\n' "$name" "$n" "${IDC_WS_WINDOWS:-0}"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi
    return 0
}
idc_st_lookups() {
    # $1 name, $2 the EXACT count, $3 a census output. Asserts the `lookups=` of the text-hash line:
    # how many times the authoritative readers were asked. Compared whole -- a substring test would
    # read 1 in 10 -- and it is what pins BOTH halves of the prefilter: a reader no longer asked on a
    # positive reads one too few, and a reader asked about every candidate again reads far too many.
    local name="$1" want="$2" got="" line=""
    while IFS= read -r line || [ -n "$line" ]; do
        case "$line" in *"text-hash arm:"*" lookups="*) got="${line##* lookups=}"; got="${got%$'\r'}" ;; esac
    done < "$3"
    if [ "$got" = "$want" ]; then
        printf '  PASS  %-48s lookups=%s\n' "$name" "$got"; IDC_ST_PASS=$((IDC_ST_PASS + 1))
    else
        printf '  FAIL  %-48s lookups=%s (expected %s)\n' "$name" "${got:-none}" "$want"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi
}
idc_st_can_scan() {
    # 0 if this box HAS what the named scanner is written for -- asked WITHOUT the scanner program: a
    # Python 3 that imports hashlib, or a perl that loads Digest::SHA. It is what tells a scanner that
    # is ABSENT (its cases are not run, and that is named) from one that is BROKEN (a failure). Before
    # 2026-10-06 the self-test could not tell them apart: with the python scanner hashing only the
    # first window of each run it printed one more NOTE, 36 fewer passes, and PASSED.
    case "$1" in
        python|python3)
            command -v "$1" >/dev/null 2>&1 || return 1
            "$1" -c 'import sys, hashlib
sys.exit(0 if sys.version_info[0] >= 3 else 1)' >/dev/null 2>&1 < /dev/null ;;
        perl)
            command -v perl >/dev/null 2>&1 || return 1
            perl -MDigest::SHA -e 'exit 0' >/dev/null 2>&1 < /dev/null ;;
        *) return 1 ;;
    esac
}

idc_mode_selftest() {
    local d="$IDC_TMP/st" bs sl pc out="" rc=0 probe="" c=""
    # The per-case runner calls awk directly and does NOT go through idc_census, so a missing
    # definition file surfaces as "instrument exited 9" on every case instead of once, clearly. It
    # still fails CLOSED, which is right, but an unreadable red is a red nobody can act on -- it cost
    # two runs of a neuter control before the cause was read. One check, up front.
    if [ ! -f "$IDC_PATTERNS" ]; then
        echo "REFUSED(2): the patterns file is not beside this tool -- the self-test has nothing to test with"
        return 2
    fi
    mkdir -p -- "$d"; chmod 700 -- "$d" 2>/dev/null
    bs="$(printf '\134')"      # one backslash, built rather than written
    sl="/"
    pc="$(printf '%%')"

    IDC_TEST_TOKENS="zorbulax quennelbee zorbulaxqueen"
    export IDC_TEST_TOKENS
    idc_build_tokens

    echo "SELF-TEST -- coord-identifier-census.sh"
    echo "  patterns: $IDC_PATTERNS"
    echo "  token set: synthetic (3 literals, not printed)"
    echo
    echo "  P. THE AWK PROBE -- asked of this box's awk before any mode reads a text. An awk that does not"
    echo "     implement a construct an arm is spelled with does not fail: the arm matches nothing and the"
    echo "     text reads CLEAN. So, mode by mode and over the SAME BYTES: this box's awk reads the plant as"
    echo "     it must, and a stand-in awk whose address arm is dead REFUSES TO RUN."
    local apd="$d/ap" apawk="" appat="" aptree="" apline="" apneedle="" APTOK="TOKENFILE=qqunspelledqq"
    local apoct='(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])'
    mkdir -p -- "$apd/bin" "$apd/src/core/apk" "$apd/repo"
    out="$apd/p0.out"; idc_awk_probe > "$out" 2>&1; rc=$?
    idc_st_rc             "this box's awk gives the known answer"          0 "$rc"
    echo "        this box's awk: $(idc_awk_banner)"
    # THE STAND-IN: the real awk, handed a copy of the definition in which the address arm is spelled
    # as an awk WITHOUT interval expressions read its spelling before 2026-10-06 -- the braces literal
    # text. That is the measured defect (mawk 1.3.4 20200120), replayed on whatever awk this box has:
    # the arm compiles, is counted as an arm, and matches no address. Every other line of the
    # definition is this tool's own, and nothing but the PATFILE argument is touched on the way to
    # the real awk.
    apawk="$(command -v awk)"
    appat="$apd/patterns.dead"
    while IFS= read -r apline || [ -n "$apline" ]; do
        case "$apline" in
            "ipv4"$'\t'*) printf 'ipv4\t%s(%s.%s)%s{3%s}\trefuse\t[CONVERTED-CONTEXT] stand-in: the braces are literal text\n' "$apoct" "$bs" "$apoct" "$bs" "$bs" ;;
            *) printf '%s\n' "$apline" ;;
        esac
    done < "$IDC_PATTERNS" > "$appat"
    printf '#!/bin/sh\nn=$#\nwhile [ "$n" -gt 0 ]; do\n    a="$1"; shift; n=$((n - 1))\n    case "$a" in PATFILE=*) a="PATFILE=%s" ;; esac\n    set -- "$@" "$a"\ndone\nexec "%s" "$@"\n' "$appat" "$apawk" > "$apd/bin/awk"
    chmod 700 -- "$apd/bin/awk" 2>/dev/null
    printf 'the address %d.%d.%d.%d answered\n' 192 168 4 20 > "$apd/plant"
    cp -- "$apd/plant" "$apd/src/core/apk/x_test.cs"
    cp -- "$apd/plant" "$apd/repo/plant.txt"

    out="$apd/e1.out"; IDC_TEST_TOKENS="$APTOK" "$IDC_SELF" entry "$apd/plant" > "$out" 2>&1; rc=$?
    idc_st_rc             "ENTRY: this box's awk refuses the address"      1 "$rc"
    out="$apd/e2.out"; PATH="$apd/bin:$PATH" IDC_TEST_TOKENS="$APTOK" "$IDC_SELF" entry "$apd/plant" > "$out" 2>&1; rc=$?
    idc_st_rc             "  the dead-arm awk REFUSES TO RUN"              2 "$rc"
    idc_st_assert_present "  naming the probe"                             "REFUSED(2): the awk probe:" "$out"
    idc_st_assert_present "  and the arm that could not fire"              "the ipv4 arm did not fire" "$out"
    idc_st_assert_absent  "  and it never reads CLEAN"                     "CLEAN:" "$out"

    out="$apd/s1.out"; IDC_TEST_TOKENS="$APTOK" "$IDC_SELF" subject "$(cat -- "$apd/plant")" > "$out" 2>&1; rc=$?
    idc_st_rc             "SUBJECT: this box's awk refuses the address"    1 "$rc"
    out="$apd/s2.out"; PATH="$apd/bin:$PATH" IDC_TEST_TOKENS="$APTOK" "$IDC_SELF" subject "$(cat -- "$apd/plant")" > "$out" 2>&1; rc=$?
    idc_st_rc             "  the dead-arm awk REFUSES TO RUN"              2 "$rc"
    idc_st_assert_present "  naming the probe"                             "REFUSED(2): the awk probe:" "$out"

    # CONVERTED reports the address arm and cannot refuse on it, so the healthy reading is exit 0 WITH
    # the arm counted -- and a dead arm there is a zero nobody could tell from a file with no address.
    out="$apd/c1.out"; IDC_TEST_TOKENS="$APTOK" "$IDC_SELF" converted "$apd/src/core/apk/x_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc             "CONVERTED: this box's awk reads the file"       0 "$rc"
    idc_st_assert_present "  and COUNTS the address it may not refuse"     "ipv4 occ=1 would-have-refused=1" "$out"
    out="$apd/c2.out"; PATH="$apd/bin:$PATH" IDC_TEST_TOKENS="$APTOK" "$IDC_SELF" converted "$apd/src/core/apk/x_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc             "  the dead-arm awk REFUSES TO RUN"              2 "$rc"
    idc_st_assert_present "  naming the probe"                             "REFUSED(2): the awk probe:" "$out"

    # TREE reads a surface against a baseline. The baseline here is a TREE object written from the
    # index -- no commit, so no identity, no signing and no hook is asked of this box.
    if git init -q "$apd/repo" >/dev/null 2>&1 && git -C "$apd/repo" add -- plant.txt >/dev/null 2>&1 \
        && aptree="$(git -C "$apd/repo" write-tree 2>/dev/null)" && [ -n "$aptree" ]; then
        apneedle="$(printf '%-18s %d' "ipv4" 1)"
        out="$apd/t1.out"; ( cd -- "$apd/repo" && IDC_TEST_TOKENS="$APTOK" "$IDC_SELF" tree plant.txt "$aptree" ) > "$out" 2>&1; rc=$?
        idc_st_rc             "TREE: this box's awk reads the surface"       0 "$rc"
        idc_st_assert_present "  and COUNTS the address as pre-existing"     "$apneedle" "$out"
        out="$apd/t2.out"; ( cd -- "$apd/repo" && PATH="$apd/bin:$PATH" IDC_TEST_TOKENS="$APTOK" "$IDC_SELF" tree plant.txt "$aptree" ) > "$out" 2>&1; rc=$?
        idc_st_rc             "  the dead-arm awk REFUSES TO RUN"            2 "$rc"
        idc_st_assert_present "  naming the probe"                           "REFUSED(2): the awk probe:" "$out"
    else
        printf '  FAIL  %-48s git could not build the fixture -- the tree arm measured nothing\n' "TREE: the awk probe"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi

    echo
    echo "  A. PLANTS -- each MUST fire exactly the declared arm set"

    printf 'HOST=%s\n' "box7lab"                                           > "$d/p01"; idc_st_case "host assignment context"                  "host_ctx" "$d/p01" 1
    printf 'the address %d.%d.%d.%d answered\n' 192 168 4 20               > "$d/p02"; idc_st_case "bare private-range quad"                  "ipv4"     "$d/p02" 1
    printf 'HOST=%d.%d.%d.%d\n' 10 7 7 7                                   > "$d/p03"; idc_st_case "assignment context carrying a quad"       "host_ctx ipv4" "$d/p03" 1
    printf 'copied from %s%s%s%sshare%sx\n' "$bs" "$bs" "box7" "$bs" "$bs" > "$d/p04"; idc_st_case "backslash share path"                     "unc_backslash" "$d/p04" 1
    printf 'see %s%s%s%sshare%sx for the log\n' "$sl" "$sl" "box7" "$sl" "$sl" > "$d/p05"; idc_st_case "forward-slash share path"             "unc_slash" "$d/p05" 1
    printf 'built at C:%sUsers%s%s%sx\n' "$bs" "$bs" "sylvandeep" "$bs"    > "$d/p06"; idc_st_case "windows profile root"                     "profile_root" "$d/p06" 1
    printf 'built at %sc%sUsers%s%s%sx\n' "$sl" "$sl" "$sl" "sylvandeep" "$sl" > "$d/p07"; idc_st_case "msys profile root"                    "profile_root" "$d/p07" 1
    printf 'built at %shome%s%s%sx\n' "$sl" "$sl" "sylvandeep" "$sl"       > "$d/p08"; idc_st_case "linux home prefix"                        "home_unix" "$d/p08" 1
    printf 'built at %sUsers%s%s%sx\n' "$sl" "$sl" "sylvandeep" "$sl"      > "$d/p09"; idc_st_case "darwin home prefix"                       "profile_root" "$d/p09" 1

    # BOTH DIRECTIONS on the nickname admit set: admitted as a HOST, refused as a PROFILE SEGMENT.
    printf 'built at C:%sUsers%sR-LAPTOP%sx\n' "$bs" "$bs" "$bs"           > "$d/p10"; idc_st_case "nickname AS A PROFILE SEGMENT refuses"    "profile_root" "$d/p10" 1
    # PER OCCURRENCE, never per line: a nickname host and a real host on ONE line still refuses.
    printf 'from %s%sR-LAPTOP%sshare and %s%s%s%sshare\n' "$bs" "$bs" "$bs" "$bs" "$bs" "box7" "$bs" > "$d/p11"; idc_st_case "mixed line: nickname host + real host" "unc_backslash" "$d/p11" 1
    idc_st_exc "  and the nickname on that SAME LINE was admitted" "unc_backslash|nickname-host" "$IDC_TMP/st.status"
    # A PLACEHOLDER IS EXCLUDED AS A SEGMENT, NEVER AS A LINE.
    printf 'C:%sUsers%s<user>%sa and C:%sUsers%s%s%sb\n' "$bs" "$bs" "$bs" "$bs" "$bs" "sylvandeep" "$bs" > "$d/p12"; idc_st_case "placeholder segment does not clear the line" "profile_root" "$d/p12" 1
    idc_st_exc "  and the placeholder on that SAME LINE was admitted" "profile_root|placeholder-segment" "$IDC_TMP/st.status"

    # THE PLACEHOLDER ADMIT ON THE unc_ ARMS -- BOTH DIRECTIONS, IN THE GATE'S OWN MODE (STRICT).
    # An admit-only battery reads GREEN on an arm that admits every host, so every case that must
    # PASS here has p04/p05 above as its REFUSE sibling: those hosts are in NEITHER admit set and
    # still fire. Each pass also asserts that the PLACEHOLDER ADMIT is what admitted it, so the
    # case cannot go green because the arm stopped matching.
    printf 'upstream doc comment: %s%sserver%sshare%spath\n' "$bs" "$bs" "$bs" "$bs" > "$d/p19"; idc_st_case "STRICT admits a placeholder host in a UNC" "" "$d/p19" 1
    idc_st_exc "  and the PLACEHOLDER ADMIT is what admitted it" "unc_backslash|placeholder-segment" "$IDC_TMP/st.status"
    printf 'and the slash flavour: %s%sserver%sshare%spath\n' "$sl" "$sl" "$sl" "$sl" > "$d/p20"; idc_st_case "STRICT admits a placeholder host in a slash share" "" "$d/p20" 1
    idc_st_exc "  and the PLACEHOLDER ADMIT is what admitted it" "unc_slash|placeholder-segment" "$IDC_TMP/st.status"
    # PER OCCURRENCE, NEVER PER LINE -- the property p11 proves for the nickname admit, proved again
    # for this one: a placeholder host and a real host on ONE line still refuses.
    printf 'from %s%sserver%sshare and %s%s%s%sshare\n' "$bs" "$bs" "$bs" "$bs" "$bs" "box7" "$bs" > "$d/p21"; idc_st_case "mixed line: placeholder host + real host" "unc_backslash" "$d/p21" 1
    idc_st_exc "  and the placeholder on that SAME LINE was admitted" "unc_backslash|placeholder-segment" "$IDC_TMP/st.status"
    # AND IN DELTA MODE TOO: this admit is read on the DECISION TOKEN, not on the sentence, so it is
    # identical in both modes -- unlike a context rule, which strict refuses on purpose.
    printf 'upstream doc comment: %s%sserver%sshare%spath\n' "$bs" "$bs" "$bs" "$bs" > "$d/p22"; idc_st_case "DELTA admits the same placeholder host" "" "$d/p22" 0
    idc_st_exc "  and by the PLACEHOLDER ADMIT in delta as well" "unc_backslash|placeholder-segment" "$IDC_TMP/st.status"

    # THE UNICODE-ESCAPE ADMIT (COORD 3a680658f (4), C1's A/B 48b7d578). ⚠ NOTHING HERE SPELLS THE
    # SHAPE: every backslash comes from $bs and the escape body is a separate argument, so this file
    # -- which is itself tracked and censused -- carries no literal network path. BOTH DIRECTIONS,
    # and the REFUSE sibling is the one that matters: the admit is bounded to the HEX form, so a
    # token of the same length whose body is NOT hex must still fire.
    printf 'stack: at go.fmt_package.printArg(%s%su0436%s%su0060 p)\n' "$bs" "$bs" "$bs" "$bs" > "$d/p23"; idc_st_case "STRICT admits a unicode-escape host" "" "$d/p23" 1
    idc_st_exc "  and the ESCAPE ADMIT is what admitted it"      "unc_backslash|escape-sequence" "$IDC_TMP/st.status"
    printf 'and the slash flavour %s%su0436%sshare%sx\n' "$sl" "$sl" "$sl" "$sl" > "$d/p24"; idc_st_case "STRICT admits it on the slash arm too (ruled scope: unc_*)" "" "$d/p24" 1
    idc_st_exc "  and the ESCAPE ADMIT is what admitted it"      "unc_slash|escape-sequence" "$IDC_TMP/st.status"
    # ⚠ THE BOUND. Same leading letter, same length, body NOT hex -> still a hit. Without this case
    # the admit could widen to "any host starting with that letter" and every other case stays green.
    printf 'copied from %s%suzzzz%sshare%sx\n' "$bs" "$bs" "$bs" "$bs" > "$d/p25"; idc_st_case "a non-hex body of the same shape STILL REFUSES" "unc_backslash" "$d/p25" 1
    # PER OCCURRENCE, never per line: an escape and a real host on ONE line still refuses.
    printf 'from %s%su0436%sshare and %s%s%s%sshare\n' "$bs" "$bs" "$bs" "$bs" "$bs" "box7" "$bs" > "$d/p26"; idc_st_case "mixed line: escape host + real host" "unc_backslash" "$d/p26" 1
    idc_st_exc "  and the escape on that SAME LINE was admitted" "unc_backslash|escape-sequence" "$IDC_TMP/st.status"
    # DELTA too: read on the decision token, so identical in both modes.
    printf 'stack: at go.fmt_package.printArg(%s%su13d1%s%su0060 p)\n' "$bs" "$bs" "$bs" "$bs" > "$d/p27"; idc_st_case "DELTA admits a unicode-escape host as well" "" "$d/p27" 0
    idc_st_exc "  and by the ESCAPE ADMIT in delta as well"      "unc_backslash|escape-sequence" "$IDC_TMP/st.status"

    # PASS 2 -- a token split across a line break, with an INDENTED continuation.
    printf 'owner column reads zorb\n    ulax here\n'                      > "$d/p13"; idc_st_case "token split across a line break (PASS 2)" "TOKENFILE" "$d/p13" 1
    # Go-guard tokenising: a token as a dot/hyphen/underscore COMPONENT of a larger run.
    printf 'row names x_%s_y and more\n' "quennelbee"                      > "$d/p14"; idc_st_case "token as an underscore component (PASS 1)" "TOKENFILE" "$d/p14" 1
    # PASS 3 -- a token broken by separators INSIDE a component, which tokenising cannot see.
    printf 'row names ab%s-%s2 here\n' "zorbul" "ax"                       > "$d/p15"; idc_st_case "token broken inside a component (PASS 3)"  "TOKENFILE" "$d/p15" 1
    # PASS 3 AT THE END OF THE LINE, and this case exists because its absence was MEASURED. The
    # reduction the pass reads is `alnumOnly`, and a reduction that loses the line's TAIL -- the
    # likeliest way to get a piecewise reducer wrong -- is INVISIBLE to the case above, whose token
    # sits mid-line with `2 here` after it: regressing the reducer to drop its last character read
    # 116/0 and green. Neutering it entirely DOES red that case (115/1), so the stage was covered and
    # its BOUNDARY was not. One case, at the boundary, and the same plant otherwise.
    printf 'row names ab%s-%s\n' "zorbul" "ax"                             > "$d/p15b"; idc_st_case "token broken, at END of line (PASS 3)"     "TOKENFILE" "$d/p15b" 1
    # A path ENDING in the token -- the shape a both-sides separator rule misses.
    printf 'built at C:%sUsers%s%s\n' "$bs" "$bs" "zorbulax"               > "$d/p16"; idc_st_case "path ENDING in a denied token" "profile_root TOKENFILE" "$d/p16" 1
    # STRICT: the IPv4 arm takes NO CONTEXT exclusion in entry/subject mode. The quad planted here
    # is version-SHAPED but OFF the release shape (a first component of 2), because the release
    # shape itself is admitted in strict by rule 5 from this change forward -- left as it was, this
    # plant would have gone on reading green while proving the opposite of what its name says. The
    # release shape's own both-directions battery is A2 below; this case keeps its own question.
    printf 'the toolchain is go%d.%d.%d.%d here\n' 2 24 13 3               > "$d/p17"; idc_st_case "STRICT refuses a version quad off the release shape" "ipv4" "$d/p17" 1
    # ⚠ RE-RULED 2026-09-20 (COORD fc4edcd8c). Rule 3 moved OUT of the STRICT-only block, so a
    # DECLARED documentation constant is now admitted in strict exactly as rule 5's release literal
    # is. What this arm pinned before -- the strict reading refusing one -- is retired ON PURPOSE,
    # and it is retired because the strict rationale ("the cost falls on the writer, as one rewrite
    # of their own post") is false of an EVIDENCE RECORD the lane did not author. The REFUSE
    # direction is not weakened and is not carried by this case: p02, p17 and q04-q07 below are
    # quads rule 3 does NOT name, and every one of them still fires in STRICT.
    printf 'the loopback %d.%d.%d.%d appears\n' 127 0 0 1                  > "$d/p18"; idc_st_case "STRICT admits a declared doc constant"               ""     "$d/p18" 1
    idc_st_exc "  and the DOC-CONSTANT ADMIT is what admitted it" "ipv4|doc-constant" "$IDC_TMP/st.status"
    # The other two declared constants, in the GATE'S OWN MODE. An admit proven on one member of a
    # three-member set is an admit proven on one member.
    printf 'the unspecified %d.%d.%d.%d and the broadcast %d.%d.%d.%d appear\n' 0 0 0 0 255 255 255 255 > "$d/p18b"; idc_st_case "STRICT admits the other two declared constants" "" "$d/p18b" 1
    idc_st_exc "  and BOTH were admitted AS DOC CONSTANTS"        "ipv4|doc-constant" "$IDC_TMP/st.status"
    # ⚠ THE BOUND, in the gate's own mode: a quad ONE COMPONENT off a declared constant is not one.
    printf 'the address %d.%d.%d.%d answered\n' 127 0 0 2                 > "$d/p18c"; idc_st_case "STRICT still refuses a quad one component off" "ipv4" "$d/p18c" 1

    echo
    echo "  A2. THE RELEASE-LITERAL ADMIT -- BOTH DIRECTIONS, IN THE GATE'S OWN MODE (STRICT)"
    echo "      (an admit-only battery reads GREEN on an arm that admits every quad, so each case"
    echo "       that must PASS has a sibling one digit off the shape that must still REFUSE, and"
    echo "       every pass asserts the RELEASE-LITERAL exclusion actually fired)"
    printf 'the hop landed go%d.%d.%d.%d on every lane\n' 1 24 13 3        > "$d/q01"; idc_st_case "STRICT admits a Go release literal"         "" "$d/q01" 1
    idc_st_exc "  and the RELEASE ADMIT is what admitted it"      "ipv4|release-literal" "$IDC_TMP/st.status"
    printf 'the package nuget-%d.%d.%d.%d is on the feed\n' 1 23 12 1      > "$d/q02"; idc_st_case "STRICT admits a package-prefixed release literal" "" "$d/q02" 1
    idc_st_exc "  and the RELEASE ADMIT is what admitted it"      "ipv4|release-literal" "$IDC_TMP/st.status"
    printf 'see docs%svalidation%s%d.%d.%d.%d%s for the roster\n' "$sl" "$sl" 1 24 13 0 "$sl" > "$d/q03"; idc_st_case "STRICT admits a release literal inside a path" "" "$d/q03" 1
    idc_st_exc "  and the RELEASE ADMIT is what admitted it"      "ipv4|release-literal" "$IDC_TMP/st.status"
    # THE REFUSE DIRECTION. The admit is bounded to ONE shape, so a private-LAN quad and a quad a
    # single component off the shape must both still be hits -- in STRICT mode, where nothing in the
    # sentence around them can help either way.
    printf 'the box answered on %d.%d.%d.%d last night\n' 10 0 0 1         > "$d/q04"; idc_st_case "a 10/8 quad is not the release shape"       "ipv4" "$d/q04" 1
    printf 'the box answered on %d.%d.%d.%d last night\n' 192 168 1 20     > "$d/q05"; idc_st_case "a private-range quad is not the release shape" "ipv4" "$d/q05" 1
    printf 'the build stamped %d.%d.%d.%d into the assembly\n' 1 3 4 5     > "$d/q06"; idc_st_case "second component off the shape still refuses" "ipv4" "$d/q06" 1
    printf 'the build stamped %d.%d.%d.%d into the assembly\n' 2 24 13 3   > "$d/q07"; idc_st_case "first component off the shape still refuses"  "ipv4" "$d/q07" 1
    # GLUED BY A DOT to an admitted quad. The back-scan used to cross the dot into the quad before it,
    # re-find that span and skip it as decided, so the trailing quad read CLEAN in STRICT whatever it
    # was (COORD 2026-09-29, pre-existing). One plant per admitted shape, since each admit disposes of
    # the first quad by a different rule, and one control that a glued pair of ADMITTED quads stays
    # admitted, so the floor cannot be satisfied by refusing everything after a dot.
    printf 'the box answered on %d.%d.%d.%d.%d.%d.%d.%d last night\n' 127 0 0 1 192 168 1 20 > "$d/q09"; idc_st_case "a quad glued after a loopback still refuses"    "ipv4" "$d/q09" 1
    printf 'the box answered on %d.%d.%d.%d.%d.%d.%d.%d last night\n' 255 255 255 255 10 0 0 1 > "$d/q10"; idc_st_case "a quad glued after a broadcast still refuses"   "ipv4" "$d/q10" 1
    printf 'the box answered on %d.%d.%d.%d.%d.%d.%d.%d last night\n' 0 0 0 0 172 16 0 9 > "$d/q11"; idc_st_case "a quad glued after an unspecified still refuses" "ipv4" "$d/q11" 1
    printf 'the hop landed go%d.%d.%d.%d.%d.%d.%d.%d on every lane\n' 1 24 13 3 192 168 1 20 > "$d/q12"; idc_st_case "a quad glued after a release literal still refuses" "ipv4" "$d/q12" 1
    printf 'the loopback %d.%d.%d.%d.%d.%d.%d.%d is linked from the roster\n' 127 0 0 1 1 24 13 3 > "$d/q13"; idc_st_case "a release literal glued after a loopback is admitted" "" "$d/q13" 1
    idc_st_exc "  and the RELEASE ADMIT is what admitted it"      "ipv4|release-literal" "$IDC_TMP/st.status"
    # AND IN DELTA MODE TOO, by the release admit and not by rules 1-4: this quad carries no prefix,
    # its token run IS the quad, it is no doc constant, and neither neighbouring word is a context
    # word -- so under rules 1-4 alone it was a hit, and the reason printed is the discriminator.
    printf 'the page %d.%d.%d.%d is linked from the roster\n' 1 24 13 3    > "$d/q08"; idc_st_case "the admit is consulted in DELTA mode as well" "" "$d/q08" 0
    idc_st_exc "  and by the RELEASE ADMIT, not by rules 1-4"     "ipv4|release-literal" "$IDC_TMP/st.status"

    echo
    echo "  B. KNOWN NEGATIVES -- in DELTA mode, each MUST fire the arm set declared beside it"
    echo "     (mostly the empty set; the few that declare an arm are the REFUSE-direction siblings"
    echo "      of an admit rule, because an admit-only control reads GREEN on a dead arm)"
    printf 'older re-creations of nuget-%d.%d.%d.%d, their targets on origin.\n' 1 23 1 7 > "$d/n01"; idc_st_case "package-prefixed version quad" "" "$d/n01" 0
    idc_st_exc "  and it was excluded BY C2 PREFIX-EX"            "ipv4|prefix-ex" "$IDC_TMP/st.status"
    printf 'the pinned toolchain is go%d.%d.%d.%d on this box\n' 1 24 13 3 > "$d/n02"; idc_st_case "toolchain-prefixed version quad" "" "$d/n02" 0
    idc_st_exc "  and it was excluded BY THE TOKEN RUN"           "ipv4|token-run" "$IDC_TMP/st.status"
    printf 'linking the %d.%d.%d.%d validation page that a reader follows\n' 1 24 13 3 > "$d/n03"; idc_st_case "version quad, space-separated context" "" "$d/n03" 0
    idc_st_exc "  and it was excluded BY THE WINDOW (C2 gap)"     "ipv4|version-context" "$IDC_TMP/st.status"
    printf 'assembly Version=%d.%d.%d.%d inside the test host\n' 1 24 13 3 > "$d/n04"; idc_st_case "version quad after an assignment" "" "$d/n04" 0
    idc_st_exc "  and it was excluded BY C2 PREFIX-EX"            "ipv4|prefix-ex" "$IDC_TMP/st.status"
    # ADJACENCY, not a window: the nearest context word here is THREE words back, so this quad is no
    # longer excused. It was excluded by the 56-character window the first draft read, and that is
    # exactly the laundering surface C2's A/B found. The refusal is the intended cost.
    # The quad is OFF the release shape (first component 2) so that rule 5 cannot dispose of it:
    # with the release shape here, this case would still have read green -- for the wrong reason --
    # and the adjacency control it exists to be would have been dead without ever going red.
    printf -- '-> FileNotFoundException for internal/itoa %d.%d.%d.%d. The error\n' 2 24 13 3 > "$d/n05"; idc_st_case "version word THREE words back no longer excuses" "ipv4" "$d/n05" 0
    printf 'the loopback %d.%d.%d.%d is a documentation constant\n' 127 0 0 1 > "$d/n06"; idc_st_case "loopback constant" "" "$d/n06" 0
    idc_st_exc "  and it was excluded AS A DOC CONSTANT"          "ipv4|doc-constant" "$IDC_TMP/st.status"
    printf 'the unspecified %d.%d.%d.%d and broadcast %d.%d.%d.%d addresses\n' 0 0 0 0 255 255 255 255 > "$d/n07"; idc_st_case "unspecified and broadcast constants" "" "$d/n07" 0
    idc_st_exc "  and BOTH were excluded AS DOC CONSTANTS"        "ipv4|doc-constant" "$IDC_TMP/st.status"
    printf 'lanes R-LAPTOP G-LAPTOP i9 i7 C1 C2 reported in\n'             > "$d/n08"; idc_st_case "bare nicknames" "" "$d/n08" 0
    printf 'from %s%sR-LAPTOP%sshare only\n' "$bs" "$bs" "$bs"             > "$d/n09"; idc_st_case "nickname AS A NETWORK HOST is admitted" "" "$d/n09" 0
    idc_st_exc "  and the admit set is what admitted it"          "unc_backslash|nickname-host" "$IDC_TMP/st.status"
    printf 'HOST=C1 reported the leg\n'                                    > "$d/n09b"; idc_st_case "nickname AS AN ASSIGNED HOST is admitted" "" "$d/n09b" 0
    idc_st_exc "  and the WIDER fleet set is what admitted it"    "host_ctx|nickname-host" "$IDC_TMP/st.status"
    printf 'HOST=i9-runner took the leg\n'                                 > "$d/n09c"; idc_st_case "a nickname-PREFIXED assigned host is admitted" "" "$d/n09c" 0
    idc_st_exc "  and the prefix rule is what admitted it"        "host_ctx|nickname-host" "$IDC_TMP/st.status"
    # The narrowing that took host_ctx from 25 false hits to 0 on the live surface: a PROSE COLON is
    # not an assignment. Both directions -- prose admitted, the assignment shape still refused.
    printf 'the host: a fleet box, and the server = the one we use\n'      > "$d/n09d"; idc_st_case "a prose colon is not an assignment" "" "$d/n09d" 0
    printf 'hostname:%s in the pasted env dump\n' "box7lab"                > "$d/n09e"; idc_st_case "the unspaced assignment shape still refuses" "host_ctx" "$d/n09e" 0
    printf 'import golang.org/x/sys for the syscall shim\n'                > "$d/n10"; idc_st_case "a go import path" "" "$d/n10" 0
    printf 'see https://github.com/ritchiecarroll/go2cs for the tree\n'    > "$d/n11"; idc_st_case "the repository public URL" "" "$d/n11" 0
    # The ESCAPED spelling, which is how this arm's own line in the definition carries it, and the
    # printf-template spelling, which is how this very file carries it. Their absence from the
    # first draft's prefix-keyed exception is what made the census refuse its own diff.
    printf 'the arm reads github%s.com/ritchiecarroll/go2cs here\n' "$bs"  > "$d/n11b"; idc_st_case "the public URL in its ERE-ESCAPED spelling" "" "$d/n11b" 0
    # The REFUSE direction of this exception is in section D, not here, for two reasons: these
    # cases run on the SYNTHETIC token set, where the account arm is not live and the control would
    # be vacuous; and writing a bare handle into this source would itself put a matchable string on
    # the pushed surface. Section D plants the REAL literal from the run-time set into a temp file
    # and asserts it fires -- the refuse direction, spelling nothing here.
    printf 'the toolchain lives under %sUSERPROFILE%s%ssdk\n' "$pc" "$pc" "$bs" > "$d/n12"; idc_st_case "the profile ENVIRONMENT VARIABLE" "" "$d/n12" 0
    printf 'built at C:%sUsers%s<user>%ssdk\n' "$bs" "$bs" "$bs"           > "$d/n13"; idc_st_case "a redacted profile segment" "" "$d/n13" 0
    idc_st_exc "  and the placeholder set is what admitted it"    "profile_root|placeholder-segment" "$IDC_TMP/st.status"
    printf 'the tree built internal.itoa.dll v%d.%d.%d.%d today\n' 1 24 13 3 > "$d/n14"; idc_st_case "v-prefixed version quad" "" "$d/n14" 0
    printf 'a C++ // comment and a ratio 3/4 in prose\n'                   > "$d/n15"; idc_st_case "a doubled slash that is not a share" "" "$d/n15" 0
    printf 'the escaped literal C:%s%sUsers is quoted -json output\n' "$bs" "$bs" > "$d/n16"; idc_st_case "a doubled backslash from json escaping" "" "$d/n16" 0

    echo
    echo "  B3. C2's A/B -- ADJACENCY, NOT A WINDOW. A version word must be NEXT TO the quad."
    # ARM A: a routable-shaped quad with a context word three words away on the same line. Under a
    # 56/32 window this read CLEAN; a window that can be arranged is a laundering surface.
    printf 'HOST=%d.%d.%d.%d was the release build target\n' 203 0 113 7 > "$d/ab1"; idc_st_case "context word three words away STILL HITS" "host_ctx ipv4" "$d/ab1" 0
    # ARM B: the same quad with nothing around it -- the control that arm A is not just always-hit.
    printf 'HOST=%d.%d.%d.%d answered\n' 203 0 113 7                     > "$d/ab2"; idc_st_case "  the same quad with no context at all"   "host_ctx ipv4" "$d/ab2" 0
    # And the two shapes that MUST still be excused, so the narrowing is not simply a dead rule 4.
    printf 'assembly Version=%d.%d.%d.%d shipped\n' 1 24 13 3            > "$d/ab3"; idc_st_case "Version=<quad> is still excused"          "" "$d/ab3" 0
    printf 'the assembly %d.%d.%d.%d validation page\n' 1 24 13 3        > "$d/ab4"; idc_st_case "<context> <quad> <context> is still excused" "" "$d/ab4" 0
    idc_st_exc "  by ADJACENCY on the neighbouring words"         "ipv4|version-context" "$IDC_TMP/st.status"

    echo
    echo "  B4. THE POSITION-MAP FUNCLIT RULE (rule 6) -- BOTH DIRECTIONS, IN THE MODE IT LIVES IN (DELTA)"
    echo "      (every PASS asserts rule 6 is what excluded it; every REFUSE plant carries a genuine"
    echo "       four-deep suffix too and asserts rule 6 fired on THAT, so each plant proves per"
    echo "       OCCURRENCE -- the suffix beside it admitted, the planted quad still a hit)"
    # The record exactly as positionMapOperations.go emits one: four C# string literals joined by
    # ", ", the fourth the funcLits list. The file names and the table are synthetic and carry no
    # context word, so no older rule can dispose of an occurrence here and read green for rule 6.
    # The suffix is mid-list here (terminated by ';') and LAST in m02 (terminated by the closing
    # quote), and m02 carries the global::-qualified spelling -- the two terminators and the two
    # spellings, one case each.
    printf '[assembly: go.GoPositionMap("x/lits/lits.go", "lits.cs", "AB8wwoKClqaC", "30-54:1;35-51:1.1;62-79:2.1.1;116-140:%d.%d.%d.%d;142-175:3.1.2")]\n' 3 1 1 1 > "$d/m01"
    idc_st_case "DELTA admits a four-deep funclit suffix (mid-list)" "" "$d/m01" 0
    idc_st_exc "  and RULE 6 is what admitted it"                 "ipv4|position-map-funclit" "$IDC_TMP/st.status"
    printf '[assembly: global::go.GoPositionMap("x/lits/lits.go", "lits.cs", "AB8wwoKClqaC", "30-54:1;134-134:%d.%d.%d.%d")]\n' 3 1 1 2 > "$d/m02"
    idc_st_case "DELTA admits it LAST in the list (global::)" "" "$d/m02" 0
    idc_st_exc "  and RULE 6 is what admitted it"                 "ipv4|position-map-funclit" "$IDC_TMP/st.status"
    # STRICT KEEPS ITS SEMANTICS, pinned: the same bytes at `entry` still refuse. Rule 6 reads the
    # record around the quad, so it is a context rule, and STRICT takes none.
    idc_st_case "STRICT still refuses the SAME record"          "ipv4" "$d/m01" 1
    # THE REFUSE DIRECTION. Each plant is the same record shape with ONE real-looking quad placed
    # where rule 6 must not reach, beside a genuine suffix that it must.
    # 1st argument. It carries a SPACE on purpose, so the quad is not in the record's FIRST WORD: the
    # word before that one is `[assembly:`, which rule 4 reads as a version context (it is how an
    # assembly-version attribute is excused), and a plant an older rule disposes of first proves
    # nothing about rule 6.
    printf '[assembly: go.GoPositionMap("x/go work/%d.%d.%d.%d/lits.go", "lits.cs", "AB8wwoKClqaC", "30-54:1;134-134:%d.%d.%d.%d")]\n' 192 168 4 20 3 1 1 1 > "$d/m03"
    idc_st_case "a quad in the 1st argument STILL REFUSES"      "ipv4" "$d/m03" 0
    idc_st_exc "  while the suffix beside it was admitted"        "ipv4|position-map-funclit" "$IDC_TMP/st.status"
    # 3rd argument, and the SHARPEST of the four: its content IS a well-formed entry -- the <digits>-
    # <digits>: prefix opening the argument, the closing quote right after the quad -- so condition
    # (2), WHICH ARGUMENT, is the only thing that can refuse it. RED-CONTROLLED 2026-09-28: removing
    # the range test ALONE leaves this case green, because the follower and anchor tests compare
    # against the fourth argument's own quotes too; removing all three reds this case and only it.
    printf '[assembly: go.GoPositionMap("x/lits/lits.go", "lits.cs", "30-54:%d.%d.%d.%d", "30-54:1;134-134:%d.%d.%d.%d")]\n' 10 7 7 7 3 1 1 1 > "$d/m04"
    idc_st_case "an entry-shaped quad in the 3rd argument REFUSES" "ipv4" "$d/m04" 0
    idc_st_exc "  while the suffix beside it was admitted"        "ipv4|position-map-funclit" "$IDC_TMP/st.status"
    # After the record, as a trailing comment: outside the argument list altogether.
    printf '[assembly: go.GoPositionMap("x/lits/lits.go", "lits.cs", "AB8wwoKClqaC", "30-54:1;134-134:%d.%d.%d.%d")] // %d.%d.%d.%d\n' 3 1 1 1 192 168 1 20 > "$d/m05"
    idc_st_case "a quad in a trailing comment STILL REFUSES"    "ipv4" "$d/m05" 0
    idc_st_exc "  while the suffix beside it was admitted"        "ipv4|position-map-funclit" "$IDC_TMP/st.status"
    # INSIDE the 4th argument but not preceded by <digits>-<digits>: -- a bare element of the list.
    printf '[assembly: go.GoPositionMap("x/lits/lits.go", "lits.cs", "AB8wwoKClqaC", "30-54:1;%d.%d.%d.%d;134-134:%d.%d.%d.%d")]\n' 10 20 30 40 3 1 1 1 > "$d/m06"
    idc_st_case "a 4th-argument quad without the entry prefix REFUSES" "ipv4" "$d/m06" 0
    idc_st_exc "  while the suffix beside it was admitted"        "ipv4|position-map-funclit" "$IDC_TMP/st.status"
    # ⚠ THE BOUND ON THE OTHER SIDE: preceded by the entry prefix, but followed by neither a ';' nor
    # the closing quote. The follower is a ':' so that rule 2 cannot take it first -- a follower in
    # the token-run class would be disposed of there, and this case would read green for rule 2.
    printf '[assembly: go.GoPositionMap("x/lits/lits.go", "lits.cs", "AB8wwoKClqaC", "30-54:1;134-134:%d.%d.%d.%d:9;142-175:%d.%d.%d.%d")]\n' 10 20 30 40 3 1 1 2 > "$d/m07"
    idc_st_case "an entry-prefixed quad with a wrong follower REFUSES" "ipv4" "$d/m07" 0
    idc_st_exc "  while the suffix beside it was admitted"        "ipv4|position-map-funclit" "$IDC_TMP/st.status"
    # A suffix nested FIVE deep is NOT rule 6's (its candidate is followed by a '.'), and it needs no
    # rule: rule 2 disposes of it first, since the token run is the whole suffix, and it keeps that
    # reason. Four such entries exist corpus-wide; this is the case that says delta covers them.
    printf '[assembly: go.GoPositionMap("x/lits/lits.go", "lits.cs", "AB8wwoKClqaC", "30-54:1;139-139:%d.%d.%d.%d.%d")]\n' 3 1 1 1 2 > "$d/m08"
    idc_st_case "a five-deep suffix is excused by rule 2 first" "" "$d/m08" 0
    idc_st_exc "  and keeps rule 2's reason"                      "ipv4|token-run" "$IDC_TMP/st.status"

    echo
    echo "  B2. THE SHORT-MATCH PATH -- the same cases with the engine's reported start PERTURBED"
    echo "      (mawk 1.3.4's match() is not leftmost-longest and returns RLENGTH=7 on a four-octet"
    echo "       quad; this forces a strictly harder perturbation so gawk exercises the same code)"
    IDC_SHORT=3
    idc_st_case "version quad, space-separated context (short match)" "" "$d/n03" 0
    idc_st_exc "  still excluded BY THE WINDOW, from the right end" "ipv4|version-context" "$IDC_TMP/st.status"
    idc_st_case "version word three words back (short match)"        "ipv4" "$d/n05" 0
    idc_st_case "package-prefixed version quad (short match)"        "" "$d/n01" 0
    idc_st_case "toolchain-prefixed version quad (short match)"      "" "$d/n02" 0
    idc_st_exc "  still excluded BY THE TOKEN RUN, from the right end" "ipv4|token-run" "$IDC_TMP/st.status"
    idc_st_case "loopback constant (short match)"                    "" "$d/n06" 0
    idc_st_exc "  still excluded AS A DOC CONSTANT (exact quad)"    "ipv4|doc-constant" "$IDC_TMP/st.status"
    # Rule 6 reads the quad's start and end from both sides of it, so it is exactly the kind of rule
    # a short extent would break.
    idc_st_case "position-map funclit suffix (short match)"         "" "$d/m01" 0
    idc_st_exc "  still excluded BY RULE 6, from the derived extent" "ipv4|position-map-funclit" "$IDC_TMP/st.status"
    idc_st_case "a 1st-argument quad (short match)"                  "ipv4" "$d/m03" 0
    # The refuse direction: the perturbation must not make the arm go BLIND, which an all-negative
    # short-match battery would read as green.
    idc_st_case "a real quad is STILL a hit under a short match"     "ipv4" "$d/p02" 1
    idc_st_case "assignment context + quad under a short match"      "host_ctx ipv4" "$d/p03" 1
    IDC_SHORT=0

    echo
    echo "  C. THE REFUSAL PATH MUST MASK -- entry mode end to end, output checked for the plant"
    printf 'built at C:%sUsers%s%s%ssdk\n' "$bs" "$bs" "sylvandeep" "$bs"  > "$d/r01"
    out="$d/r01.out"
    "$IDC_SELF" entry "$d/r01" > "$out" 2>&1
    rc=$?
    if [ "$rc" -eq 1 ]; then
        printf '  PASS  %-48s exit=1\n' "entry mode REFUSES a profile-path plant"
        IDC_ST_PASS=$((IDC_ST_PASS + 1))
    else
        printf '  FAIL  %-48s exit=%d (expected 1)\n' "entry mode REFUSES a profile-path plant" "$rc"
        IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi
    idc_st_assert_absent  "refusal output does not spell the plant segment" "sylvandeep" "$out"
    idc_st_assert_present "refusal output names the arm"                    "profile_root" "$out"
    idc_st_assert_present "refusal output carries a masked rendering"       "<*REDACTED-10*>" "$out"
    idc_st_assert_present "refusal output carries the hit count"            "hits=1" "$out"
    # The longest-literal-first property: a literal that CONTAINS another must not render as the
    # contained literal's mask plus a surviving suffix.
    printf 'owner column reads %s here\n' "zorbulaxqueen"                   > "$d/r02"
    out="$d/r02.out"
    IDC_TEST_TOKENS="zorbulax quennelbee zorbulaxqueen" "$IDC_SELF" entry "$d/r02" > "$out" 2>&1
    idc_st_assert_absent  "container literal leaves no surviving suffix"    "queen" "$out"
    idc_st_assert_present "container literal masked at its own length"      "<*REDACTED-13*>" "$out"

    echo
    echo "  C3. CONVERTED MODE -- THE SAME FILE BOTH WAYS, AND THE DOOR IT REFUSES TO OPEN"
    echo "      (the pair IS the test: one downgrade can only be read against the reading that"
    echo "       refuses, and every case that must PASS has a sibling here that must still REFUSE)"
    local cdir="$d/src/core/x"
    mkdir -p -- "$cdir"
    # A converted test source, as the -tests pipeline emits one: Go's own test literals, carried
    # across verbatim. A quad OFF the release shape and off every declared constant, so nothing but
    # the downgrade can dispose of it, and the loopback-name-and-port literal that matched host_ctx on the live shard.
    {
        printf '// converted from the package own x_test.go -- Go test data, not this lane\n'
        printf '    @string dnsName = "%d.%d.%d.%d";\n' 1 2 3 4
        printf '    @string addr = "localhost:%d";\n' 0
    } > "$cdir/x_test.cs"

    out="$d/c3a.out"; "$IDC_SELF" converted "$cdir/x_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "converted mode is CLEAN on a converted test source" 0 "$rc"
    idc_st_assert_present "  and it says which mode it read in"            "MODE converted" "$out"
    # THE ADMIT DIRECTION, named: the case must not be able to go green because an arm stopped
    # matching. The arm MATCHED, and the downgrade is what kept it from refusing.
    idc_st_assert_present "  and the quad arm MATCHED and was downgraded"  "ipv4 occ=1 would-have-refused=1" "$out"
    idc_st_assert_present "  and the host arm MATCHED and was downgraded"  "host_ctx occ=1 would-have-refused=1" "$out"
    # THE SIBLING. The same bytes, the gate for messages and docs, and it must still refuse both.
    out="$d/c3a2.out"; "$IDC_SELF" entry "$cdir/x_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "  and ENTRY on the SAME FILE still REFUSES"        1 "$rc"
    idc_st_assert_present "  with both literal arms counted as hits"       "hits=2" "$out"

    # ⚠ THIS CASE CHANGED ITS VERDICT BY RULING (COORD ruling 1, 2026-09-22), and the old form is kept
    # in the history rather than lost: it asserted "a profile path in a converted source is not Go's
    # test data" and REFUSED. The Go gate of record never agreed -- fleetIsUpstreamFixture skips the
    # structural pass on every *_test.cs -- so the census was refusing what the gate passes. The
    # protection that old sentence wanted is real, and it now lives where BOTH gates put it: a
    # converting box's own profile path is caught by the RUN-TIME account arms, and a fleet name by
    # the DENIED-TOKEN arms, neither of which any marker can downgrade. The sibling below holds that.
    cp -- "$cdir/x_test.cs" "$cdir/y_test.cs"
    printf 'built at C:%sUsers%s%s%sx\n' "$bs" "$bs" "sylvandeep" "$bs" >> "$cdir/y_test.cs"
    out="$d/c3b.out"; "$IDC_SELF" converted "$cdir/y_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "an UPSTREAM FIXTURE's profile path REPORTS, never refuses" 0 "$rc"
    # THE ADMIT DIRECTION, named: the arm MATCHED and the fixture downgrade is what disposed of it.
    idc_st_assert_present "  and the profile arm MATCHED and was downgraded" "profile_root occ=1 would-have-refused=1" "$out"
    idc_st_assert_present "  and it says why"                                "UPSTREAM FIXTURE" "$out"
    idc_st_assert_absent  "  without spelling the plant segment"            "sylvandeep" "$out"
    idc_st_assert_present "  while the literal arms are still downgraded"   "ipv4 occ=1 would-have-refused=1" "$out"
    # THE SIBLING THAT HOLDS THE PROTECTION: the same file, the gate for messages and docs.
    out="$d/c3b2.out"; "$IDC_SELF" entry "$cdir/y_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "  and ENTRY on the SAME FILE still REFUSES"        1 "$rc"
    idc_st_assert_present "  naming the profile arm"                        "profile_root" "$out"

    # THE OTHER THREE STRUCTURAL ARMS, one file each so a case cannot pass on another arm's match.
    printf 'fixture home: /home/%s/x\n' "sylvandeep" > "$cdir/h_test.cs"
    out="$d/c3f1.out"; "$IDC_SELF" converted "$cdir/h_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "an UPSTREAM FIXTURE's home path REPORTS"             0 "$rc"
    idc_st_assert_present "  and the home arm MATCHED and was downgraded"  "home_unix occ=1 would-have-refused=1" "$out"
    printf 'fixture share: %s%s%s%sshare%sx\n' "$bs" "$bs" "sylvandeep" "$bs" "$bs" > "$cdir/u_test.cs"
    out="$d/c3f2.out"; "$IDC_SELF" converted "$cdir/u_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "an UPSTREAM FIXTURE's backslash share REPORTS"       0 "$rc"
    idc_st_assert_present "  and the share arm MATCHED and was downgraded" "unc_backslash occ=1 would-have-refused=1" "$out"
    printf 'fixture share: %s%s%s%sshare%sx\n' "$sl" "$sl" "sylvandeep" "$sl" "$sl" > "$cdir/s_test.cs"
    out="$d/c3f3.out"; "$IDC_SELF" converted "$cdir/s_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "an UPSTREAM FIXTURE's slash share REPORTS"           0 "$rc"
    idc_st_assert_present "  and the slash arm MATCHED and was downgraded" "unc_slash occ=1 would-have-refused=1" "$out"

    # ⚠ THE PROTECTION, both shapes the ruling names: a DENIED token INSIDE a UNC and INSIDE a profile
    # path, in an upstream fixture, still refuses -- by its token arm, which no marker reaches.
    printf 'fixture share: %s%s%s%sshare%sx\n' "$bs" "$bs" "zorbulax" "$bs" "$bs" > "$cdir/t1_test.cs"
    out="$d/c3g1.out"
    IDC_TEST_TOKENS="zorbulax quennelbee zorbulaxqueen" "$IDC_SELF" converted "$cdir/t1_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "a DENIED token inside a UNC in a fixture REFUSES"    1 "$rc"
    idc_st_assert_present "  and the TOKEN arm is what refused"             "TOKENFILE" "$out"
    printf 'built at C:%sUsers%s%s%sx\n' "$bs" "$bs" "zorbulax" "$bs" > "$cdir/t2_test.cs"
    out="$d/c3g2.out"
    IDC_TEST_TOKENS="zorbulax quennelbee zorbulaxqueen" "$IDC_SELF" converted "$cdir/t2_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "a DENIED token inside a profile path in a fixture REFUSES" 1 "$rc"
    idc_st_assert_present "  and the TOKEN arm is what refused"             "TOKENFILE" "$out"

    # MIRROR, NEVER EXCEED: a converted-door path the Go gate does NOT call a fixture keeps refusing
    # on a structural hit, exactly as the gate would. package_test_info.cs ends in _info.cs, not
    # _test.cs; a validation page is a page.
    printf 'built at C:%sUsers%s%s%sx\n' "$bs" "$bs" "sylvandeep" "$bs" > "$cdir/package_test_info.cs"
    out="$d/c3h1.out"; "$IDC_SELF" converted "$cdir/package_test_info.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "a NON-fixture door path (test info) still REFUSES"   1 "$rc"
    idc_st_assert_present "  and says it is not a fixture"                  "NOT an upstream fixture" "$out"
    mkdir -p -- "$d/docs/validation/current"
    printf 'page cites %s%s%s%sshare%sx\n' "$bs" "$bs" "sylvandeep" "$bs" "$bs" > "$d/docs/validation/current/x.md"
    out="$d/c3h2.out"; "$IDC_SELF" converted "$d/docs/validation/current/x.md" > "$out" 2>&1; rc=$?
    idc_st_rc      "a NON-fixture door path (a page) still REFUSES"      1 "$rc"
    idc_st_assert_present "  on the share arm"                               "unc_backslash" "$out"
    # ...while a name that ENDS in _test.cs is a fixture by the Go predicate, whatever it is called.
    printf 'built at C:%sUsers%s%s%sx\n' "$bs" "$bs" "sylvandeep" "$bs" > "$cdir/package_info_internal_test.cs"
    out="$d/c3h3.out"; "$IDC_SELF" converted "$cdir/package_info_internal_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "package_info_internal_test.cs IS a fixture (suffix)" 0 "$rc"

    # PER FILE, NEVER PER RUN: a fixture and a page in ONE call, each read by its own path. The page's
    # hit refuses and the fixture's does not -- one hit total, and a flag hoisted out of the loop would
    # read both the same way.
    out="$d/c3i.out"; "$IDC_SELF" converted "$cdir/y_test.cs" "$cdir/package_test_info.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "a MIXED run refuses on the page alone"               1 "$rc"
    idc_st_assert_present "  with exactly ONE hit across both files"        "REFUSED(1): 1 hit(s)" "$out"

    # AND NEITHER DOES A DENIED TOKEN -- the class the tracked-tree guard keeps running over exactly
    # these files, and the one an emission can actually carry into one.
    printf 'owner column reads %s here\n' "zorbulax" > "$cdir/z_test.cs"
    out="$d/c3c.out"
    IDC_TEST_TOKENS="zorbulax quennelbee zorbulaxqueen" "$IDC_SELF" converted "$cdir/z_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "converted mode REFUSES a denied token all the same"  1 "$rc"
    idc_st_assert_present "  and names the arm that refused"                "TOKENFILE" "$out"

    # THE DOOR. The downgrade is justified by ONE property of the INPUT, so a caller cannot assert
    # that property -- the path must carry it. A production .cs beside the tests is the near miss.
    printf 'a converted production source, not a test artifact\n' > "$cdir/x.cs"
    out="$d/c3d.out"; "$IDC_SELF" converted "$cdir/x.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "converted mode REFUSES TO RUN on a production .cs"  2 "$rc"
    idc_st_assert_present "  and the refusal points at the mode that reads it" "census it with" "$out"
    c="$(awk 'END { print NR }' "$out")"
    if [ "$c" = "1" ]; then
        printf '  PASS  %-48s lines=1\n' "  in ONE line"; IDC_ST_PASS=$((IDC_ST_PASS + 1))
    else
        printf '  FAIL  %-48s lines=%s (expected 1)\n' "  in ONE line" "$c"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi
    # And the other half of the door: the right NAME in the wrong TREE.
    printf 'a test-shaped name outside the corpus\n' > "$d/x_test.cs"
    out="$d/c3e.out"; "$IDC_SELF" converted "$d/x_test.cs" > "$out" 2>&1; rc=$?
    idc_st_rc      "converted mode REFUSES a path outside src/core"     2 "$rc"
    idc_st_assert_present "  naming that reason"                            "not under src/core/" "$out"

    echo
    echo "  C3P. //go:embed PAYLOADS -- fleetEmbedPayloads MIRRORED: the csproj says so, and nothing else can"
    # The shape Go 1.24's traceviewer ships at static/trace_viewer_full.html:6244: a JavaScript regex
    # whose escaped character class reads as a UNC host. Assembled here so this file carries no hit.
    local pdir="$d/src/core/tv" regexline=""
    mkdir -p -- "$pdir/static" "$pdir/sub"
    regexline="$(printf "const r=new RegExp('addr=(%s%s[%s%sda-fA-F%s%s-]+%s%s])');" "$bs" "$bs" "$bs" "$bs" "$bs" "$bs" "$bs" "$bs")"
    printf '%s\n' "$regexline" > "$pdir/static/viewer.html"
    printf '<Project>\r\n  <ItemGroup Label="GoEmbeddedResources">\r\n    <EmbeddedResource Include="static/viewer.html" LogicalName="go.embed/tv/static/viewer.html" />\r\n  </ItemGroup>\r\n</Project>\r\n' > "$pdir/tv.csproj"
    out="$d/c3p1.out"; "$IDC_SELF" converted "$pdir/static/viewer.html" > "$out" 2>&1; rc=$?
    idc_st_rc      "a payload its csproj NAMES is admitted"            0 "$rc"
    # THE ADMIT DIRECTION, named: the share arm MATCHED and the fixture downgrade disposed of it.
    idc_st_assert_present "  and the share arm MATCHED and was downgraded" "unc_backslash occ=1 would-have-refused=1" "$out"
    idc_st_assert_present "  and it says why"                                "UPSTREAM FIXTURE" "$out"
    out="$d/c3p2.out"; "$IDC_SELF" entry "$pdir/static/viewer.html" > "$out" 2>&1; rc=$?
    idc_st_rc      "  and ENTRY on the SAME FILE still REFUSES"        1 "$rc"
    # THE SAME BYTES WHERE NO ITEM NAMES THEM: not a payload, so not through the door at all.
    printf '%s\n' "$regexline" > "$pdir/static/other.html"
    out="$d/c3p3.out"; "$IDC_SELF" converted "$pdir/static/other.html" > "$out" 2>&1; rc=$?
    idc_st_rc      "the SAME BYTES at an unnamed path are REFUSED"     2 "$rc"
    idc_st_assert_present "  at the door"                                   "not a converted test artifact" "$out"
    # An item without the converter's go.embed LogicalName admits nothing.
    mkdir -p -- "$d/src/core/tw/static"
    printf '%s\n' "$regexline" > "$d/src/core/tw/static/viewer.html"
    printf '    <EmbeddedResource Include="static/viewer.html" LogicalName="viewer.html" />\n' > "$d/src/core/tw/tw.csproj"
    out="$d/c3p4.out"; "$IDC_SELF" converted "$d/src/core/tw/static/viewer.html" > "$out" 2>&1; rc=$?
    idc_st_rc      "an item WITHOUT go.embed/ admits nothing"          2 "$rc"
    # An item reaching OUT of its own directory admits nothing: a csproj one level DOWN naming ../x.
    printf '%s\n' "$regexline" > "$pdir/up.html"
    printf '    <EmbeddedResource Include="../up.html" LogicalName="go.embed/tv/up.html" />\n' > "$pdir/sub/sub.csproj"
    out="$d/c3p5.out"; "$IDC_SELF" converted "$pdir/up.html" > "$out" 2>&1; rc=$?
    idc_st_rc      "an item reaching outside its directory admits nothing" 2 "$rc"
    # THE SAME TEETH AS TESTDATA: a denied token in an admitted payload still refuses.
    printf 'owner %s\n%s\n' "zorbulax" "$regexline" > "$pdir/static/viewer.html"
    out="$d/c3p6.out"
    IDC_TEST_TOKENS="zorbulax quennelbee zorbulaxqueen" "$IDC_SELF" converted "$pdir/static/viewer.html" > "$out" 2>&1; rc=$?
    idc_st_rc      "a DENIED token in an admitted payload REFUSES"     1 "$rc"
    idc_st_assert_present "  and the TOKEN arm is what refused"             "TOKENFILE" "$out"
    # TRACKED, where it can be asked: in a work tree, an untracked csproj and payload admit nothing,
    # and the same two files admit once tracked.
    local gdir="$d/wt"
    mkdir -p -- "$gdir/src/core/tv/static"
    cp -- "$pdir/tv.csproj" "$gdir/src/core/tv/tv.csproj"
    printf '%s\n' "$regexline" > "$gdir/src/core/tv/static/viewer.html"
    if git init -q "$gdir" >/dev/null 2>&1; then
        out="$d/c3p7.out"; "$IDC_SELF" converted "$gdir/src/core/tv/static/viewer.html" > "$out" 2>&1; rc=$?
        idc_st_rc      "an UNTRACKED payload in a work tree admits nothing" 2 "$rc"
        git -C "$gdir" add -- src/core/tv/tv.csproj src/core/tv/static/viewer.html >/dev/null 2>&1
        out="$d/c3p8.out"; "$IDC_SELF" converted "$gdir/src/core/tv/static/viewer.html" > "$out" 2>&1; rc=$?
        idc_st_rc      "  and the same files admit once TRACKED"         0 "$rc"
    else
        printf '  FAIL  %-48s git init failed -- the tracked arm measured nothing\n' "the tracked-payload arm"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi

    echo
    echo "  D0. THE RUN-TIME ARMS ARE BOUNDED BY THE DENIED SET -- three bars, one control each"
    unset IDC_TEST_TOKENS
    for probe in "root:under 5 characters" "ubuntu:stop-listed generic account name" "buildbox7:not in the denied set"; do
        IDC_TEST_ACCOUNT="${probe%%:*}"; export IDC_TEST_ACCOUNT
        idc_build_tokens
        case "$IDC_INERT" in
            *"RUNTIME_ACCOUNT: "*"${probe#*:}"*)
                printf '  PASS  %-48s inert: %s\n' "a derived account probe is INERT" "${probe#*:}"
                IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
            *)
                printf '  FAIL  %-48s expected inert(%s), got: %s\n' "a derived account probe is INERT" "${probe#*:}" "$(printf '%b' "$IDC_INERT" | tr -d '\n')"
                IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
        esac
        # and it must not be in the live token set at all
        c="$(awk -F'\t' '$1 == "RUNTIME_ACCOUNT" { n++ } END { print n + 0 }' "$IDC_TOKFILE")"
        case "$c" in
            ''|0) printf '  PASS  %-48s live RUNTIME_ACCOUNT literals=0\n' "  and it is not a live arm"; IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
            *)    printf '  FAIL  %-48s live RUNTIME_ACCOUNT literals=%s\n' "  and it is not a live arm" "$c"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
        esac
    done
    # THE FIRE DIRECTION OF THE SAME BARS, ON ANY BOX (2026-10-06). A derivation whose (length, hash)
    # IS a denied row clears all three and becomes a live literal. Synthetic on BOTH sides -- the
    # account, and the list that holds its row -- so it asks nothing of what this box is called. The
    # three controls above are an all-inert battery, green on an arm that can never fire; the control
    # that fired them used to be section D's alone, and D needs a box the fleet has denied.
    local barsacct="quennelbox7" barslist="$d/bars.hashes" barskeep="$IDC_HASHES"
    printf '# synthetic denied row, built at run time\n%s\t%s\tsynthetic account\n' "${#barsacct}" "$(idc_hash "$barsacct")" > "$barslist"
    IDC_HASHES="$barslist"; IDC_TEST_ACCOUNT="$barsacct"; export IDC_TEST_ACCOUNT
    idc_build_tokens
    IDC_HASHES="$barskeep"
    c="$(awk -F'\t' '$1 == "RUNTIME_ACCOUNT" { n++ } END { print n + 0 }' "$IDC_TOKFILE")"
    if [ "${IDC_HASH_HITS:-0}" -ge 1 ] && [ "$c" = "1" ]; then
        printf '  PASS  %-48s hash matches=%s, live RUNTIME_ACCOUNT literals=1 (synthetic row)\n' "a derived account that IS a denied row CLEARS" "$IDC_HASH_HITS"; IDC_ST_PASS=$((IDC_ST_PASS + 1))
    else
        printf '  FAIL  %-48s hash matches=%s, live RUNTIME_ACCOUNT literals=%s (synthetic row)\n' "a derived account that IS a denied row CLEARS" "${IDC_HASH_HITS:-0}" "$c"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
    fi
    unset IDC_TEST_ACCOUNT

    echo
    echo "  D. THE RUN-TIME ARMS ON THIS BOX -- fired yes/no only, never a value"
    idc_build_tokens
    local tfpresent="$IDC_TOKFILE_PRESENT" nlit=0 fired="no" nawhy=""
    nlit="$(awk 'END { print NR }' "$IDC_TOKFILE")"
    # THIS SECTION'S PREMISE IS A FACT ABOUT THE BOX: that it derives a name the fleet has denied, or
    # holds the local token file. On a box that does neither -- a generic container -- no run-time arm
    # has a literal, and these four cases are NOT APPLICABLE: counted as that, named, never a pass.
    # Until 2026-10-06 the first of them was a FAIL there (`no run-time arm can fire here`) and the
    # other three an uncounted SKIP, so the self-test could not be green on the box class the incident
    # was on, and each lane's tool had learned to let one red through. What that FAIL stood for is now
    # held by cases every box runs: D0 fires the bars on a synthetic row, and D2's TEXT_DENIED reads
    # the denied rows from the text. A box that DOES hold a run-time literal is judged exactly as before.
    nawhy="this box holds no run-time literal (nothing it derives is a denied row; token file present=$tfpresent) -- inert BY ENVIRONMENT. D0 fires the bars on a synthetic row; D2 reads the denied rows from the text"
    # The FIRE direction of the same bars ON THIS BOX: a derivation whose hash IS a row in the shared
    # hashes file clears them.
    case "$IDC_HASH_HITS" in
        ''|0)
            case "$nlit" in
                ''|0) idc_st_na "a denied-set token clears the bars" "$nawhy" ;;
                *)    printf '  FAIL  %-48s hash matches=0 -- no run-time arm can fire here\n' "a denied-set token clears the bars" ; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
            esac ;;
        *)    printf '  PASS  %-48s hash matches=%s\n' "a denied-set token clears the bars" "$IDC_HASH_HITS"; IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
    esac
    case "$nlit" in
        ''|0)
            idc_st_na "run-time token arm fired"                  "the same: the run-time token set is EMPTY on this box, so there is no arm to control"
            idc_st_na "run-time token arm silent on a clean body" "the same"
            idc_st_na "public handle admitted in BOTH spellings"  "the same"
            ;;
        *)
            : > "$d/rt"; chmod 600 -- "$d/rt" 2>/dev/null
            awk -F'\t' 'NR == 1 { print "owner column reads " $2 " here" }' "$IDC_TOKFILE" > "$d/rt"
            : > "$IDC_TMP/st.keys"; : > "$IDC_TMP/st.status"
            idc_run_awk "$d/rt" "$IDC_TMP/st.keys" "$IDC_TMP/st.report" "$IDC_TMP/st.status" 1
            rc=$?
            fired="$(idc_st_fired "$IDC_TMP/st.status")"
            rm -f -- "$d/rt"
            if [ "$rc" -eq 0 ] && [ -n "$fired" ]; then
                printf '  PASS  %-48s token file present=%s, literals=%s\n' "run-time token arm fired=yes" "$tfpresent" "$nlit"
                IDC_ST_PASS=$((IDC_ST_PASS + 1))
            else
                printf '  FAIL  %-48s token file present=%s, literals=%s\n' "run-time token arm fired=no" "$tfpresent" "$nlit"
                IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
            fi
            printf 'an ordinary line with nothing of the kind on it\n' > "$d/rt2"
            : > "$IDC_TMP/st.keys"; : > "$IDC_TMP/st.status"
            idc_run_awk "$d/rt2" "$IDC_TMP/st.keys" "$IDC_TMP/st.report" "$IDC_TMP/st.status" 1
            rc=$?
            fired="$(idc_st_fired "$IDC_TMP/st.status")"
            if [ "$rc" -eq 0 ] && [ -z "$fired" ]; then
                printf '  PASS  %-48s\n' "run-time token arm silent on a clean body"
                IDC_ST_PASS=$((IDC_ST_PASS + 1))
            else
                printf '  FAIL  %-48s arms{%s}\n' "run-time token arm fired on a clean body" "$fired"
                IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
            fi
            # THE PUBLISHED-ATTRIBUTION EXCEPTION, controlled with the REAL token set -- the only
            # set it can possibly matter for. Both spellings: the URL as written in prose, and the
            # ERE-ESCAPED form the definition file itself carries. The refuse direction is the two
            # checks above; this is the admit direction, and an admit-only control would be green
            # on a dead arm, which is why it is not the only one here.
            printf 'see https://github.com/ritchiecarroll/go2cs and the arm reads github%s.com/ritchiecarroll/go2cs\n' "$bs" > "$d/rt3"
            : > "$IDC_TMP/st.keys"; : > "$IDC_TMP/st.status"
            idc_run_awk "$d/rt3" "$IDC_TMP/st.keys" "$IDC_TMP/st.report" "$IDC_TMP/st.status" 1
            rc=$?
            fired="$(idc_st_fired "$IDC_TMP/st.status")"
            if [ "$rc" -eq 0 ] && [ -z "$fired" ]; then
                printf '  PASS  %-48s\n' "public handle admitted in BOTH spellings"
                IDC_ST_PASS=$((IDC_ST_PASS + 1))
            else
                printf '  FAIL  %-48s arms{%s}\n' "public handle refused in some spelling" "$fired"
                IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
            fi
            ;;
    esac

    echo
    echo "  D1. THE PUBLIC-HANDLE ADMIT -- ruled 2026-09-22, and every admitted case has a"
    echo "      refusing sibling. The literals are SYNTHETIC, so no case depends on this box's real"
    echo "      account name, and the synthetic handle CONTAINS the synthetic denied literal, which"
    echo "      is the entire shape the ruling is about."
    local ADMARM="RUNTIME_ACCOUNT=zorbulax" ADMTF="TOKENFILE=zorbulax" ADMSET="zorbulaxqueen"
    unset IDC_TEST_TOKENS IDC_TEST_ADMITS

    # (a) THE ADMIT, on the exact surface the ruling was measured on: a registry-search URL, where
    # the handle follows a PERCENT-ESCAPED SPACE. Remove the escape rule from admitWord and the
    # enclosing word becomes the two hex digits glued to the handle, and this case reds.
    printf 'browse at https://example.invalid/packages?q=go2cs%s20%s today\n' "$pc" "$ADMSET" > "$d/h01"
    out="$d/h01.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a handle after a percent-escape is ADMITTED"   0 "$rc"
    idc_st_assert_present "  and the arm MATCHED and was ADMITTED, not missed" "public-handle" "$out"
    idc_st_assert_present "  and the admitted count is on the record"     "PUBLIC-HANDLE ADMITS: 1" "$out"
    idc_st_assert_absent  "  without spelling the handle"                 "$ADMSET" "$out"

    # ⚠ THE ONE-AXIS SIBLING, and it is what makes the case above mean anything. SAME BYTES, the one
    # difference being that the synthetic handle is no longer in the admit set. An admit control
    # without this arm is green on an arm that simply stopped matching.
    out="$d/h01b.out"
    IDC_TEST_TOKENS="$ADMARM" "$IDC_SELF" entry "$d/h01" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and the SAME BYTES REFUSE off the admit set" 1 "$rc"
    idc_st_assert_present "  naming the arm that refused"                 "RUNTIME_ACCOUNT" "$out"

    # (e) The handle as an ordinary whole word, which is the work-mail handle's shape in prose.
    printf 'the %s packages are published under that name\n' "$ADMSET" > "$d/h02"
    out="$d/h02.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h02" > "$out" 2>&1; rc=$?
    idc_st_rc             "a handle as a bare WORD is ADMITTED"           0 "$rc"

    # (d) THE BARE DENIED LITERAL. The admit is keyed on the enclosing word, and when the token IS
    # the whole word the word is not an admitted handle. This is the arm that keeps the admit from
    # ever becoming a way to clear the account name itself.
    printf 'owner column reads %s here\n' "zorbulax" > "$d/h03"
    out="$d/h03.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h03" > "$out" 2>&1; rc=$?
    idc_st_rc             "a BARE account literal still REFUSES"          1 "$rc"
    idc_st_assert_present "  naming the arm that refused"                 "RUNTIME_ACCOUNT" "$out"

    # (b) A PROFILE PATH carrying the ACCOUNT name. A different arm entirely, never consulted by this
    # admit, and the ruling says so in as many words.
    printf 'built at C:%sUsers%s%s%sx\n' "$bs" "$bs" "zorbulax" "$bs" > "$d/h04"
    out="$d/h04.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h04" > "$out" 2>&1; rc=$?
    idc_st_rc             "a profile path with the ACCOUNT name REFUSES"  1 "$rc"
    idc_st_assert_present "  naming the structural arm"                   "profile_root" "$out"

    # (c) A PROFILE PATH carrying the PUBLIC HANDLE. The handle is published attribution; a profile
    # path is infrastructure whatever its segment spells, and this is the case an admit that reached
    # the structural arms would quietly open.
    printf 'built at C:%sUsers%s%s%sx\n' "$bs" "$bs" "$ADMSET" "$bs" > "$d/h05"
    out="$d/h05.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h05" > "$out" 2>&1; rc=$?
    idc_st_rc             "a profile path with the HANDLE still REFUSES"  1 "$rc"
    idc_st_assert_present "  naming the structural arm"                   "profile_root" "$out"

    # (f) CONTAINMENT IS NOT ADMISSION. The lookup is an exact hash of the WHOLE enclosing word, so a
    # word that merely contains the account literal -- or merely contains the admitted handle -- is
    # still a hit. This is the shape in which an admit list quietly becomes a hole.
    printf 'owner column reads %sxyz and %sxyz here\n' "zorbulax" "$ADMSET" > "$d/h06"
    out="$d/h06.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h06" > "$out" 2>&1; rc=$?
    idc_st_rc             "a word CONTAINING the handle still REFUSES"    1 "$rc"

    # THE MIXED LINE. Every occurrence must be inside an admitted word: a line that carries the
    # handle AND the bare account literal is still a refusal, so an admitted span cannot launder the
    # rest of its own line. Same property blankSpans has for public_url, asserted rather than assumed.
    printf 'see the %s page, account %s\n' "$ADMSET" "zorbulax" > "$d/h07"
    out="$d/h07.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h07" > "$out" 2>&1; rc=$?
    idc_st_rc             "handle AND bare literal on one line REFUSES"   1 "$rc"

    # (g) THE DOTTED MODULE-PATH SPELLING, ruled 2026-10-03: a module under the handle as C# spells it
    # (namespace), as NuGet spells it (ID) and as a project path spells it. Remove dottedHandleOK from
    # admitWord and the enclosing word is the whole dotted path, and this case reds.
    printf 'using static go.github.com.%s.hashset_package; id nugetgo.github.com.%s.hashset in ../pkg/github.com.%s.hashset.csproj\n' "$ADMSET" "$ADMSET" "$ADMSET" > "$d/h09"
    out="$d/h09.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h09" > "$out" 2>&1; rc=$?
    idc_st_rc             "the DOTTED module-path handle is ADMITTED"     0 "$rc"
    # One count per line and token; admitWord requires EVERY word on the line that holds the literal to
    # pass, so this one line proves all three spellings at once.
    idc_st_assert_present "  and the arm MATCHED and was ADMITTED, not missed" "PUBLIC-HANDLE ADMITS: 1" "$out"
    # Its one-axis sibling: the same bytes off the admit set refuse.
    out="$d/h09b.out"
    IDC_TEST_TOKENS="$ADMARM" "$IDC_SELF" entry "$d/h09" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and the SAME BYTES REFUSE off the admit set" 1 "$rc"
    # Only the github host: the same shape on another host refuses.
    printf 'using go.gitlab.com.%s.thing;\n' "$ADMSET" > "$d/h10"
    out="$d/h10.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h10" > "$out" 2>&1; rc=$?
    idc_st_rc             "a dotted handle on ANOTHER host REFUSES"       1 "$rc"
    # Containment is still not admission inside a dotted word.
    printf 'using go.github.com.%sxyz.thing;\n' "$ADMSET" > "$d/h11"
    out="$d/h11.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h11" > "$out" 2>&1; rc=$?
    idc_st_rc             "a dotted segment CONTAINING the handle REFUSES" 1 "$rc"
    # The admitted segment cannot launder the bare literal in another segment of the same word.
    printf 'using go.github.com.%s.%s;\n' "$ADMSET" "zorbulax" > "$d/h12"
    out="$d/h12.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h12" > "$out" 2>&1; rc=$?
    idc_st_rc             "a dotted word with the bare literal REFUSES"   1 "$rc"

    # (h) RESIDUAL 4, PINNED: A BROKEN SPELLING BESIDE THE HANDLE READS CLEAN. The literal spelled
    # with characters that are not its own is inside NO word, so the word walk of the admit never
    # meets it, and one admitted handle on the line -- or on the line joined to it -- answers for it.
    # Each of these reads CLEAN on a box holding the literal (here), on one holding nothing (D2, h1)
    # and on the tool as it was before TEXT_DENIED; without the handle each refuses (p15 above, and
    # D2 k). It is stated in the header (WHAT IS STILL NOT FOUND, 4) and queued for a design of its
    # own. THE CASES ASSERT TODAY'S VERDICT, so the residual cannot change silently in EITHER
    # direction: the change that closes it reds them and turns them over in the same commit. A first
    # cut at closing it -- an admit that counted occurrences -- was taken back on 2026-10-06, the day
    # it was made, for what it broke: (i) below, and the glued span in D2 (o).
    printf 'the handle %s said z o r b u l a x here\n' "$ADMSET" > "$d/h13"
    out="$d/h13.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h13" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 4: handle + literal SPACED OUT is CLEAN" 0 "$rc"
    idc_st_assert_present "  the reduced pass MET it and the admit cleared it" "PUBLIC-HANDLE ADMITS: 1 " "$out"
    printf 'the handle %s said zorb-ulax here\n' "$ADMSET" > "$d/h14"
    out="$d/h14.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h14" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 4: handle + literal HYPHENATED is CLEAN" 0 "$rc"
    printf 'the handle %s said the row was zorb-\n    ulax here\n' "$ADMSET" > "$d/h15"
    out="$d/h15.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h15" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 4: handle, literal WRAPPED below, is CLEAN" 0 "$rc"
    # Two admissions: the handle on its own line, and the PAIR -- which is where the wrapped literal
    # is, and where the handle answered for it.
    idc_st_assert_present "  the joined pair was read, and admitted"      "PUBLIC-HANDLE ADMITS: 2 " "$out"
    # The handle on the SECOND line: its admission there also switches PASS 3 off for the pair that
    # ends on that line.
    printf 'the row was zorb-\n    ulax under the handle %s here\n' "$ADMSET" > "$d/h16"
    out="$d/h16.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h16" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 4: literal WRAPPED onto the handle's line" 0 "$rc"
    # The handle on two wrapped lines with nothing broken beside it: CLEAN, and counted once a line.
    printf 'the handle %s is here\n    and %s again there\n' "$ADMSET" "$ADMSET" > "$d/h17"
    out="$d/h17.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h17" > "$out" 2>&1; rc=$?
    idc_st_rc             "the handle on two wrapped lines is ADMITTED"   0 "$rc"
    idc_st_assert_present "  once a line, the pair not counted again"     "PUBLIC-HANDLE ADMITS: 2 " "$out"

    # (i) THE HANDLE FIRST ON A LINE, AFTER A LINE THAT ENDS IN A WORD CHARACTER (2026-10-06). An
    # unbulleted list of the owner's repositories or project files, one a line, is this shape. The
    # joined pair glues the last word of one line to the handle on the next, so the handle is no
    # WORD of the pair -- but it is a word of its own line, that line admitted it, and the admission
    # is what tells PASS 3 not to read the pair for the same literal again. The admit that counted
    # occurrences (h, above) kept its admissions apart from its hits, read the pair again, and
    # REFUSED all four of these on every box. The tool before it reads them CLEAN, and so does the
    # tool before TEXT_DENIED. Its sibling on the box that derives nothing is D2 (h1).
    printf 'the row ends with a word\n%s starts this line\n' "$ADMSET" > "$d/h18"
    out="$d/h18.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h18" > "$out" 2>&1; rc=$?
    idc_st_rc             "the handle FIRST on a line, after a word, is CLEAN" 0 "$rc"
    printf 'files in the list\n    github.com.%s.hashset.csproj\n' "$ADMSET" > "$d/h19"
    out="$d/h19.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h19" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and so is its DOTTED module path, indented"  0 "$rc"
    printf '%s/hashset\n%s/uuid\n%s/jwt\n' "$ADMSET" "$ADMSET" "$ADMSET" > "$d/h20"
    out="$d/h20.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h20" > "$out" 2>&1; rc=$?
    idc_st_rc             "  a list of repositories, one a line, is CLEAN" 0 "$rc"
    printf 'github.com.%s.hashset.csproj\ngithub.com.%s.uuid.csproj\n' "$ADMSET" "$ADMSET" > "$d/h21"
    out="$d/h21.out"
    IDC_TEST_TOKENS="$ADMARM" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h21" > "$out" 2>&1; rc=$?
    idc_st_rc             "  a list of project files, one a line, is CLEAN" 0 "$rc"

    # SCOPE, and it is the reason the admit travels as a per-arm marker in the DEFINITION rather than
    # as a list every arm shares. The SAME bytes and the SAME admit set, with the literal installed
    # under TOKENFILE -- an arm the definition does not mark -- must refuse. A widening that reached
    # every token arm would clear a locally denied literal, which no ruling has ever asked for.
    out="$d/h08.out"
    IDC_TEST_TOKENS="$ADMTF" IDC_TEST_ADMITS="$ADMSET" "$IDC_SELF" entry "$d/h01" > "$out" 2>&1; rc=$?
    idc_st_rc             "an UNMARKED arm does not consult the admit"    1 "$rc"
    idc_st_assert_present "  naming the unmarked arm that refused"        "TOKENFILE" "$out"

    # THE DEFINITION IS THE DECLARATION. The marked arms -- RUNTIME_ACCOUNT, RUNTIME_OWNERNAME and,
    # since 2026-10-06, TEXT_DENIED -- are read from the patterns file, so the
    # count is derived from the whole construct rather than from the arms a reader happens to check.
    c="$(awk -F'\t' 'NF >= 4 && index($4, "[PUBLIC-HANDLE-ADMIT]") > 0 { n++ } END { print n + 0 }' "$IDC_PATTERNS")"
    case "$c" in
        3) printf '  PASS  %-48s marked arms=3\n' "exactly three arms consult the admit"; IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
        *) printf '  FAIL  %-48s marked arms=%s (expected 3)\n' "exactly three arms consult the admit" "$c"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
    esac

    # AND THE TWO INSTRUMENTS AGREE ABOUT THE SET. The Go guard's fleetPublicHandles is the authority
    # and the ADMIT rows are generated from it; the tracked-tree side of that equality is asserted by
    # TestFleetIdentifierHashFileMatchesTheGoLists. What this arm adds is that THIS tool can read the
    # section at all -- a malformed row would otherwise leave the admit silently empty.
    c="$(awk -F'\t' '$1 == "ADMIT" && NF >= 4 && length($3) == 64 { n++ } END { print n + 0 }' "$IDC_HASHES")"
    case "$c" in
        ''|0) printf '  FAIL  %-48s readable ADMIT rows=0\n' "the shared ADMIT section is readable"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
        *)    printf '  PASS  %-48s readable ADMIT rows=%s\n' "the shared ADMIT section is readable" "$c"; IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
    esac

    echo
    echo "  D2. TEXT_DENIED -- a denied row matched against the WORDS OF THE TEXT. The denied row, the"
    echo "      admitted handle and the hash list holding them are SYNTHETIC and built here, so no case"
    echo "      depends on this box's name. Against that list no derivation of this box is a denied"
    echo "      row, so every run-time arm is inert: it is the box the arm exists for."
    unset IDC_TEST_TOKENS IDC_TEST_ADMITS
    local TXW="vexomarrin" TXH="vexomarrinqueen" TXHIT="TEXT_DENIED        pass"
    local hl="$d/tx.hashes" hlna="$d/tx.hashes.noadmit" hlbad="$d/tx.hashes.bad" hlzero="$d/tx.hashes.zero" fb="$d/fakebin"
    # A literal the bare IDC_TEST_TOKENS form installs under TOKENFILE and no case below spells: the
    # run-time side of every child is then synthetic, fixed, and silent.
    local TXTOK="TOKENFILE=qqunspelledqq"
    printf '# synthetic denied row, built at run time\n%s\t%s\tsynthetic denied word\n' "${#TXW}" "$(idc_hash "$TXW")" > "$hlna"
    { cat -- "$hlna"; printf 'ADMIT\t%s\t%s\tsynthetic public handle\n' "${#TXH}" "$(idc_hash "$TXH")"; } > "$hl"
    printf '# no denied row at all\nADMIT\t%s\t%s\tsynthetic public handle\n' "${#TXH}" "$(idc_hash "$TXH")" > "$hlzero"
    { cat -- "$hlna"; printf '%s\t%s\ta row that is not a digest\n' "${#TXW}" "not-a-sha256"; } > "$hlbad"

    # (a) THE INCIDENT'S SHAPE: the word in ordinary prose, on a box that can derive nothing.
    printf 'thanks %s, that reading is noted\n' "$TXW" > "$d/x01"
    out="$d/x01.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a denied word in PROSE refuses on any box"     1 "$rc"
    idc_st_assert_present "  and TEXT_DENIED is the arm that refused"     "${TXHIT}1" "$out"
    idc_st_assert_present "  alone -- no run-time arm was live to help"   "REFUSED(1): 1 hit(s)" "$out"
    idc_st_assert_absent  "  and the refusal never spells the word"       "$TXW" "$out"
    # (b) THE SHELL ROUTE gives the same verdict as whatever (a) used. A fallback that is only ever
    # reached on the box that lacks the interpreter is a fallback nobody has run.
    out="$d/x01s.out"
    IDC_HASHER="sha256sum" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "the sha256sum route refuses the same bytes"    1 "$rc"
    idc_st_assert_present "  and it is the route that ran"                "hasher=sha256sum" "$out"
    # (c) THE SUBJECT is censused by the same arm.
    out="$d/x02.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" subject "inbox: for $TXW -- a reading" > "$out" 2>&1; rc=$?
    idc_st_rc             "a denied word in the SUBJECT refuses"          1 "$rc"
    idc_st_assert_present "  naming the arm"                              "${TXHIT}1" "$out"
    # (d) A COMPONENT of a longer run -- the Go guard's component walk.
    printf 'the share lab-%s-7 answered\n' "$TXW" > "$d/x03"
    out="$d/x03.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x03" > "$out" 2>&1; rc=$?
    idc_st_rc             "a denied word as a COMPONENT refuses"          1 "$rc"
    idc_st_assert_present "  naming the arm"                              "${TXHIT}1" "$out"
    # (e) WRAPPED across a line break, with indentation -- the joined copy is a candidate source.
    printf 'the name on that row was vexo\n    marrin and nothing else\n' > "$d/x04"
    out="$d/x04.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x04" > "$out" 2>&1; rc=$?
    idc_st_rc             "a denied word WRAPPED across a break refuses"  1 "$rc"
    idc_st_assert_present "  found in the joined pair (PASS 2)"           "${TXHIT}2" "$out"
    # (f) THE KNOWN NEGATIVE, and the length bound read off the same run: a word of a denied row's
    # LENGTH that is not the row is hashed and passes, and it is the ONLY word hashed on that line.
    printf 'the name vexomarrim was used\n' > "$d/x05"
    out="$d/x05.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x05" > "$out" 2>&1; rc=$?
    idc_st_rc             "a same-LENGTH word that is not a row passes"   0 "$rc"
    idc_st_assert_present "  only the candidate of that length was hashed" "candidates=1 hashed=1 denied-words=0" "$out"
    idc_st_assert_present "  and the arm is in the DECLARED SET at zero"  "refuse  TEXT_DENIED          occ=0      hits=0" "$out"
    # (g) THE PUBLIC-HANDLE ADMIT, on a box that cannot derive the handle. The dotted spelling of the
    # handle and the bare word in one text: the bare word refuses, the handle is admitted because the
    # text's own dot segment is an ADMIT row -- and the SAME BYTES against a list without that row
    # count both.
    printf 'using go.github.com.%s.thing;\nand %s signed it\n' "$TXH" "$TXW" > "$d/x06"
    out="$d/x06.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x06" > "$out" 2>&1; rc=$?
    idc_st_rc             "handle + bare word: the bare word REFUSES"     1 "$rc"
    idc_st_assert_present "  the handle occurrence is ADMITTED from text" "PUBLIC-HANDLE ADMITS: 1 " "$out"
    idc_st_assert_present "  so exactly one hit is counted"               "REFUSED(1): 1 hit(s)" "$out"
    out="$d/x06n.out"
    IDC_HASHES="$hlna" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x06" > "$out" 2>&1; rc=$?
    idc_st_rc             "  the SAME BYTES off the ADMIT row refuse"     1 "$rc"
    idc_st_assert_present "  and count both occurrences"                  "REFUSED(1): 2 hit(s)" "$out"
    # (h) THE HANDLE ALONE. The denied word stands only INSIDE a longer word here, never as a run or
    # a component. Until the window scan that was a stated residual -- the word was not found, and
    # the text read CLEAN for the wrong reason. It is now FOUND, by the scan, and cleared by the
    # ADMIT row the text itself supplies: CLEAN for the right one. Its one-axis sibling is the same
    # bytes against the list without that row, which must refuse.
    printf 'using go.github.com.%s.thing;\n' "$TXH" > "$d/x07"
    out="$d/x07.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x07" > "$out" 2>&1; rc=$?
    idc_st_rc             "the handle alone is CLEAN"                     0 "$rc"
    idc_st_assert_present "  the denied word inside it WAS found (scan)"  "denied-words=1 admit-words=1" "$out"
    idc_st_assert_present "  and the ADMIT row is what cleared it"        "PUBLIC-HANDLE ADMITS: 1 " "$out"
    out="$d/x07n.out"
    IDC_HASHES="$hlna" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x07" > "$out" 2>&1; rc=$?
    idc_st_rc             "  the SAME BYTES off the ADMIT row REFUSE"     1 "$rc"
    # (h1) RESIDUAL 4, PINNED ON THE BOX THAT DERIVES NOTHING -- D1 (h) where the scan discovers the
    # word and the ADMIT row comes from the text. The word IS found (the admission is counted, and
    # only TEXT_DENIED holds a literal the text spells), and the handle beside it answers for it:
    # CLEAN, as on the box that holds the word. One verdict on both kinds of box is all this arm
    # promises, and the residual keeps that promise; closing it is the queued design, not this arm.
    printf 'the handle %s said %s %s here\n' "$TXH" "${TXW:0:1}" "${TXW:1}" > "$d/x15"
    out="$d/x15.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x15" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 4: handle + word BROKEN by a space, CLEAN" 0 "$rc"
    idc_st_assert_present "  the scan FOUND it and the admit cleared it"  "PUBLIC-HANDLE ADMITS: 1 " "$out"
    printf 'the handle %s said the row was %s-\n    %s here\n' "$TXH" "${TXW:0:4}" "${TXW:4}" > "$d/x16"
    out="$d/x16.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x16" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 4: handle, word WRAPPED below, is CLEAN" 0 "$rc"
    idc_st_assert_present "  the joined pair was read, and admitted"      "PUBLIC-HANDLE ADMITS: 2 " "$out"
    printf 'the row was %s-\n    %s under the handle %s here\n' "${TXW:0:4}" "${TXW:4}" "$TXH" > "$d/x17"
    out="$d/x17.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x17" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 4: word WRAPPED onto the handle's line" 0 "$rc"
    # THE HANDLE FIRST ON A LINE, AFTER A LINE THAT ENDS IN A WORD CHARACTER -- D1 (i) on the box
    # that derives nothing. The admit that counted occurrences refused these here too.
    printf 'the row ends with a word\n%s starts this line\n' "$TXH" > "$d/x18"
    out="$d/x18.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x18" > "$out" 2>&1; rc=$?
    idc_st_rc             "the handle FIRST on a line, after a word, is CLEAN" 0 "$rc"
    printf 'files in the list\n    github.com.%s.hashset.csproj\n' "$TXH" > "$d/x19"
    out="$d/x19.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x19" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and so is its DOTTED module path, indented"  0 "$rc"
    printf '%s/hashset\n%s/uuid\n%s/jwt\n' "$TXH" "$TXH" "$TXH" > "$d/x20"
    out="$d/x20.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x20" > "$out" 2>&1; rc=$?
    idc_st_rc             "  a list of repositories, one a line, is CLEAN" 0 "$rc"
    printf 'github.com.%s.hashset.csproj\ngithub.com.%s.uuid.csproj\n' "$TXH" "$TXH" > "$d/x21"
    out="$d/x21.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x21" > "$out" 2>&1; rc=$?
    idc_st_rc             "  a list of project files, one a line, is CLEAN" 0 "$rc"
    # (h2) A DENIED WORD THAT ITSELF CARRIES A SEPARATOR, at the end of a sentence. The run is the
    # word PLUS the full stop, and the components cut the word in two, so neither is the row: the
    # SPANS of the run are what find it. Its own synthetic list; the near-miss of the same length and
    # the same shape is the known negative, and the shell route is run over the same bytes.
    local TXY="vexo-marrin12" hly="$d/tx.hashes.hyphen"
    printf '# synthetic denied row that contains a hyphen\n%s\t%s\tsynthetic machine name\n' "${#TXY}" "$(idc_hash "$TXY")" > "$hly"
    printf 'the box is %s.\n' "$TXY" > "$d/x08"
    out="$d/x08.out"
    IDC_HASHES="$hly" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x08" > "$out" 2>&1; rc=$?
    idc_st_rc             "a HYPHENATED denied word + full stop refuses"  1 "$rc"
    idc_st_assert_present "  found as a SPAN of the run"                  "denied-words=1" "$out"
    idc_st_assert_present "  naming the arm"                              "$TXHIT" "$out"
    idc_st_assert_absent  "  and the refusal never spells the word"       "$TXY" "$out"
    out="$d/x08s.out"
    IDC_HASHER="sha256sum" IDC_HASHES="$hly" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x08" > "$out" 2>&1; rc=$?
    idc_st_rc             "  the sha256sum route refuses it too"          1 "$rc"
    printf 'the box _%s_ and backup_%s_0930.zip\n' "$TXY" "$TXY" > "$d/x08b"
    out="$d/x08b.out"
    IDC_HASHES="$hly" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x08b" > "$out" 2>&1; rc=$?
    idc_st_rc             "  in _emphasis_ and inside a file name too"    1 "$rc"
    out="$d/x08c.out"
    IDC_HASHES="$hly" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" subject "ssh $TXY.example.com, then $TXY-2..." > "$out" 2>&1; rc=$?
    idc_st_rc             "  with a .suffix and a -suffix, in a SUBJECT"  1 "$rc"
    printf 'the box is vexo-marrin13.\n' > "$d/x09"
    out="$d/x09.out"
    IDC_HASHES="$hly" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x09" > "$out" 2>&1; rc=$?
    idc_st_rc             "a same-shape span that is not a row passes"    0 "$rc"
    idc_st_assert_present "  and it WAS hashed -- one span of that length" "candidates=1 hashed=1 denied-words=0" "$out"
    # (h3) A CRLF FILE, read by an awk that keeps the CR on the record -- every awk off Windows. The
    # wrapped word of case (e), same bytes but for the line ends. The stand-in `awk` sets BINMODE so
    # this box's awk reads the bytes as a Linux awk does; where awk already does, it changes nothing.
    local fa="$d/fakeawk" realawk=""
    realawk="$(command -v awk)"
    mkdir -p -- "$fa"
    printf '#!/bin/sh\nexec "%s" -v BINMODE=3 "$@"\n' "$realawk" > "$fa/awk"
    chmod 700 -- "$fa/awk" 2>/dev/null
    printf 'the name on that row was vexo\r\n    marrin and nothing else\r\n' > "$d/x04r"
    out="$d/x04r.out"
    PATH="$fa:$PATH" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x04r" > "$out" 2>&1; rc=$?
    idc_st_rc             "a CRLF-WRAPPED denied word refuses (CR kept)"  1 "$rc"
    idc_st_assert_present "  found in the joined pair (PASS 2)"           "${TXHIT}2" "$out"
    out="$d/x04rs.out"
    PATH="$fa:$PATH" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" subject "$(cat -- "$d/x04r")" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and in a SUBJECT carrying the same bytes"    1 "$rc"

    # (i) IT FAILS CLOSED. Each of these is a census that would otherwise read nothing and pass, over
    # the bytes case (a) refuses.
    out="$d/x10.out"
    IDC_HASHES="$d/tx.hashes.absent" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a MISSING hash list refuses to run"            2 "$rc"
    idc_st_assert_present "  naming the arm and the reason"               "REFUSED(2): TEXT_DENIED: the hash list" "$out"
    out="$d/x11.out"
    IDC_HASHES="$hlzero" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a list with ZERO denied rows refuses to run"   2 "$rc"
    idc_st_assert_present "  naming the arm and the reason"               "yielded 0 denied rows" "$out"
    out="$d/x12.out"
    IDC_HASHES="$hlbad" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a row that does not PARSE refuses to run"      2 "$rc"
    idc_st_assert_present "  naming the row by NUMBER, never by content"  "row 3 of" "$out"
    out="$d/x13.out"
    IDC_HASHER="none" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "NO usable hasher refuses to run"               2 "$rc"
    idc_st_assert_present "  naming the arm and the reason"               "REFUSED(2): TEXT_DENIED: no usable hasher" "$out"
    # A hasher that RUNS and answers WRONG is the dangerous one: every digest it returns matches no
    # row, so without the known-answer probe it reads as a clean text. A stand-in named `python`,
    # first on PATH, that prints a well-formed digest of nothing for every candidate.
    mkdir -p -- "$fb"
    printf '#!/bin/sh\nwhile IFS= read -r l; do echo e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855; done\n' > "$fb/python"
    chmod 700 -- "$fb/python" 2>/dev/null
    out="$d/x14.out"
    PATH="$fb:$PATH" IDC_HASHER="python" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a hasher that answers WRONG refuses to run"    2 "$rc"
    idc_st_assert_present "  naming the arm and the reason"               "REFUSED(2): TEXT_DENIED: no usable hasher" "$out"

    # (k) THE WINDOW SCAN. Every shape below carries the denied word ONLY glued inside a longer word
    # or ONLY broken by characters that are not its own -- never as a run, a component or a span, so
    # the candidate route alone reads each of them CLEAN.
    #
    # TWO ASSERTIONS A SHAPE, AND THEY ARE NOT THE SAME ONE. (1) THE SCAN ALONE, over that shape in a
    # file of its own, names the word -- idc_scan_text, no census. (2) The census of all thirteen in
    # one file, an empty line between shapes so no joined pair crosses two of them, hits each shape
    # BY ITS OWN LINE NUMBER. The second cannot stand for the first, and this was MEASURED: with the
    # dump's joined-pair buffers switched off, the one-file census stayed green on every line,
    # because a word discovered on ANY line is then a literal on EVERY line. Only (1) says the scan
    # found THIS shape.
    #
    # THROUGH EACH SCANNER THIS BOX HAS. A scanner that does not answer its probe here is NAMED and
    # its cases are NOT RUN -- they are not counted as passes -- and at least one must answer.
    local TXC="V${TXW#v}" TXN="vexomarrim" zw nb ff cr sc="" nsc=0 ln="" wk="$d/wk" k=0 id="" rest="" txrows="" hlnm="$d/tx.hashes.nearmiss"
    zw="$(printf '\342\200\213')"; nb="$(printf '\302\240')"; ff="$(printf '\014')"; cr="$(printf '\015')"
    txrows="${#TXW}:$(idc_hash "$TXW")"
    printf '# the near-miss word AS the row\n%s\t%s\tsynthetic denied word\n' "${#TXN}" "$(idc_hash "$TXN")" > "$hlnm"
    printf 'the boxes of the %ss were moved\n' "$TXW"                            > "$d/ws01"   # a plural s
    printf 'the account %s2 exists\n' "$TXW"                                     > "$d/ws02"   # a digit suffix
    printf 'the word %sxyz here\n' "$TXW"                                        > "$d/ws03"   # suffix letters
    printf 'the word x%s here\n' "$TXW"                                          > "$d/ws04"   # a prefix letter
    printf 'class %sBox here\n' "$TXC"                                           > "$d/ws05"   # a CamelCase neighbour
    printf 'see docs/first%s20%s/page\n' "$pc" "$TXW"                            > "$d/ws06"   # a percent-escape before it
    printf '{"msg":"hello%sn%s"}\n' "$bs" "$TXW"                                 > "$d/ws07"   # a backslash-n before it
    printf 'the owner %s %s said\n' "${TXW:0:1}" "${TXW:1}"                      > "$d/ws08"   # a space between two letters
    printf 'the owner %s%s%s said\n' "${TXW:0:4}" "$zw" "${TXW:4}"               > "$d/ws09"   # a zero-width space inside it
    printf 'mail j%s@example.com now\n' "$TXW"                                   > "$d/ws10"   # glued in an e-mail local part
    printf 'that row was %s%s\n%s and no more\n' "${TXW:0:4}" "$ff" "${TXW:4}"   > "$d/ws11"   # a form feed at a wrap
    printf 'that row was %s%s\n%s and no more\n' "${TXW:0:4}" "$nb" "${TXW:4}"   > "$d/ws12"   # a no-break space at a wrap
    printf 'that row was %s%s%s and no more\n' "${TXW:0:4}" "$cr" "${TXW:4}"     > "$d/ws13"   # a lone CR at a wrap
    # id : the line of the one-file census its hit is reported on : what it is
    local shapes="01:1:a plural s|02:3:a digit suffix|03:5:suffix letters|04:7:a prefix letter|05:9:a CamelCase neighbour|06:11:a percent-escape before it|07:13:a backslash-n before it|08:15:a space between two letters|09:17:a zero-width space inside it|10:19:glued in an e-mail local part|11:22:a form feed at a wrap|12:25:a no-break space at a wrap|13:27:a lone CR at a wrap"
    : > "$d/w01"
    for id in 01 02 03 04 05 06 07 08 09 10 11 12 13; do cat -- "$d/ws$id" >> "$d/w01"; printf '\n' >> "$d/w01"; done
    # The near-misses: the same shapes around a word one letter off the row. Every one of them puts
    # windows of the row's length through the hasher, and none is the row.
    {
        printf 'the boxes of the %ss were moved\n\n' "$TXN"
        printf 'class V%sBox and x%s2 here\n\n' "${TXN#v}" "$TXN"
        printf 'the owner %s %s said and vexomarri x too\n\n' "${TXN:0:1}" "${TXN:1}"
        printf 'that row was %s%s\n%s and no more\n' "${TXN:0:4}" "$ff" "${TXN:4}"
    } > "$d/w02"
    printf '#%s\n=%s\n#%s\n' "$IDC_WPROBE_ROWS" "$IDC_WPROBE_LINE" "$IDC_WPROBE_ROWS" > "$d/wprobe.in"
    # THE 64 KiB SEAM. A scanner walks a long buffer in pieces, and a window that straddles the end
    # of a piece is hashed only because the pieces OVERLAP. Two buffers longer than one piece, the
    # word across the seam in each: (1) broken by a space, 65,531 alphanumerics in, so only the
    # REDUCTION holds it and it lies over offset 65,536 of that; (2) the hyphenated word, 65,530
    # characters into one RUN. And a public-URL stand-in for (o) below.
    local txrowsy="" psyn="$d/patterns.synurl" SYNP="quillhaven/synrepo" opat=""
    txrowsy="${#TXY}:$(idc_hash "$TXY")"
    awk -v a="${TXW:0:5}" -v b="${TXW:5}" 'BEGIN { for (i = 0; i < 32765; i++) printf "ab "; printf "c %s %s and no more\n", a, b }' > "$d/wseam1"
    awk -v w="$TXY" 'BEGIN { printf "the name "; for (i = 0; i < 6553; i++) printf "xxxxxxxxxx"; printf "%syy was used\n", w }' > "$d/wseam2"
    for sc in python python3 perl; do
        if ! { idc_window_scan "$sc" "$d/wprobe.in" "$d/wprobe.words" && [ "$IDC_WS_BUFFERS" = "0" ]; }; then
            # ABSENT is named and its cases are not run. BROKEN -- the box has the interpreter and the
            # scanner written for it does not answer -- is a FAILURE: production would fall back to
            # another scanner here, and refuse every census on a box that has only this one.
            if idc_st_can_scan "$sc"; then
                printf '  FAIL  %-48s this box CAN run it and it does not answer its probe -- broken, not absent\n' "scanner $sc"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1))
            else
                printf '  NOTE  %-48s is not on this box (no such command, or not a Python 3 / a perl with Digest::SHA) -- its cases are NOT RUN\n' "scanner $sc"
            fi
            continue
        fi
        nsc=$((nsc + 1))
        idc_st_scan "[$sc] a word across the 64 KiB seam of a REDUCTION" "$sc" "$d/wseam1" "$txrows" "$TXW"
        idc_st_scan "[$sc] a word across the 64 KiB seam of a RUN"       "$sc" "$d/wseam2" "$txrowsy" "$TXY"
        rest="$shapes|"
        while [ -n "$rest" ]; do
            ln="${rest%%|*}"; rest="${rest#*|}"
            idc_st_scan "[$sc] the scan ALONE finds: ${ln#*:*:}" "$sc" "$d/ws${ln%%:*}" "$txrows" "$TXW"
        done
        out="$d/w01.$sc.out"
        IDC_WINDOWER="$sc" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w01" > "$out" 2>&1; rc=$?
        idc_st_rc             "[$sc] glued and broken shapes REFUSE"        1 "$rc"
        idc_st_assert_present "  and it is the scanner that ran"              "scan-words=1 scanner=$sc" "$out"
        k=0; rest="$shapes|"
        while [ -n "$rest" ]; do
            ln="${rest%%|*}"; rest="${rest#*|}"; ln="${ln#*:}"
            idc_st_assert_present "  [$sc] hit on its line: ${ln#*:}" "${TXHIT}3  line ${ln%%:*} " "$out"
            k=$((k + 1))
        done
        idc_st_assert_present "  [$sc] one hit a shape, and no other"        "REFUSED(1): $k hit(s)" "$out"
        idc_st_assert_absent  "  [$sc] and the refusal never spells the word" "$TXW" "$out"
        idc_st_scan           "[$sc] the scan ALONE names no near-miss"       "$sc" "$d/w02" "$txrows" ""
        out="$d/w02.$sc.out"
        IDC_WINDOWER="$sc" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w02" > "$out" 2>&1; rc=$?
        idc_st_rc             "[$sc] the near-misses of every shape are CLEAN" 0 "$rc"
        idc_st_assert_present "  [$sc] with no word found"                   "scan-words=0 scanner=$sc" "$out"
        idc_st_assert_absent  "  [$sc] and windows WERE hashed, not zero"    "scan-windows=0 " "$out"
        # THE ONE-AXIS SIBLING of the near-misses, so their CLEAN is not a census that never read
        # those lines: the SAME BYTES against a list whose row IS that word refuse, once a shape.
        out="$d/w02f.$sc.out"
        IDC_WINDOWER="$sc" IDC_HASHES="$hlnm" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w02" > "$out" 2>&1; rc=$?
        idc_st_rc             "  [$sc] and REFUSE when that word IS the row"  1 "$rc"
        idc_st_assert_present "  [$sc] once a shape"                         "REFUSED(1): 4 hit(s)" "$out"
    done
    case "$nsc" in
        0) printf '  FAIL  %-48s usable scanners=0\n' "at least one window scanner answers here"; IDC_ST_FAIL=$((IDC_ST_FAIL + 1)) ;;
        *) printf '  PASS  %-48s usable scanners=%s\n' "at least one window scanner answers here" "$nsc"; IDC_ST_PASS=$((IDC_ST_PASS + 1)) ;;
    esac
    # THE SUBJECT is scanned by the same arm, with whichever scanner answers first.
    out="$d/w03.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" subject "inbox: the ${TXW}s reading" > "$out" 2>&1; rc=$?
    idc_st_rc             "a GLUED denied word in the SUBJECT refuses"    1 "$rc"
    idc_st_assert_present "  naming the arm"                              "${TXHIT}3" "$out"

    # (l) WHAT IS STILL NOT FOUND, PINNED so no residual can change silently. Residuals 1 and 2 are
    # here; residual 3 is (p3) below; residual 4 is D1 (h), (h1) above and the URL case in (o).
    # (l1) A denied word that ITSELF CONTAINS A SEPARATOR. Found glued and found wrapped across one
    # break -- its exact bytes are a window of a run -- and NOT found with its separator re-spelled,
    # because only the exact bytes are hashed. The last case is the asymmetry itself: the same bytes
    # REFUSE on a box that holds the literal, where PASS 3 reduces the literal too.
    printf 'the box x%ss is up\n' "$TXY" > "$d/w04"
    out="$d/w04.out"
    IDC_HASHES="$hly" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w04" > "$out" 2>&1; rc=$?
    idc_st_rc             "a HYPHENATED denied word, GLUED, refuses"      1 "$rc"
    printf 'the box is %s-\n    %s today\n' "${TXY%%-*}" "${TXY#*-}" > "$d/w05"
    out="$d/w05.out"
    IDC_HASHES="$hly" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w05" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and WRAPPED across one break, refuses"       1 "$rc"
    printf 'the box %s_%s is up\n' "${TXY%%-*}" "${TXY#*-}" > "$d/w06"
    out="$d/w06.out"
    IDC_HASHES="$hly" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w06" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 1: its separator RE-SPELLED is CLEAN" 0 "$rc"
    idc_st_assert_present "  the scan ran and named nothing"              "scan-words=0 scanner=" "$out"
    out="$d/w06t.out"
    IDC_HASHES="$hly" IDC_TEST_TOKENS="TOKENFILE=$TXY" "$IDC_SELF" entry "$d/w06" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and a box HOLDING the literal REFUSES it"    1 "$rc"
    # (l2) A word wrapped across THREE lines. No pass joins more than an adjacent pair, so no box
    # finds it -- discovered or held as a literal. The same on every box, and stated.
    printf 'that row was %s\n%s\n%s and no more\n' "${TXW:0:3}" "${TXW:3:4}" "${TXW:7}" > "$d/w07"
    out="$d/w07.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w07" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 2: a THREE-LINE wrap is CLEAN"        0 "$rc"
    out="$d/w07t.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="TOKENFILE=$TXW" "$IDC_SELF" entry "$d/w07" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and CLEAN on a box holding the literal too"  0 "$rc"

    # (m) THE SCAN FAILS CLOSED, over the bytes case (a) refuses. There is no narrowed mode: with no
    # scanner the candidate route WOULD have found this word, and the census still does not pass.
    out="$d/w10.out"
    IDC_WINDOWER="none" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "NO usable window scanner refuses to run"       2 "$rc"
    idc_st_assert_present "  naming the arm and the reason"               "REFUSED(2): TEXT_DENIED: no usable window scanner" "$out"
    # A scanner that RUNS and answers its probe WRONG. This stand-in windows the runs and not the
    # reduction: it returns one probe word of two, a right-looking count line, and no word. Without
    # the known answer it is a scanner that reads every broken word CLEAN.
    mkdir -p -- "$wk/a" "$wk/b" "$wk/c"
    printf '#!/bin/sh\ns=0; n=0\nwhile IFS= read -r l; do case "$l" in "#"*) s=$((s + 1)) ;; "="*) [ "$s" -eq 2 ] && n=$((n + 1)) ;; esac; done\necho "%s"; echo "%s"; echo "windows=1 buffers=$n"\n' "$IDC_WPROBE_HEAD" "$IDC_WPROBE_A" > "$wk/a/perl"
    chmod 700 -- "$wk/a/perl" 2>/dev/null
    out="$d/w11.out"
    PATH="$wk/a:$PATH" IDC_WINDOWER="perl" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a scanner that answers its probe WRONG refuses" 2 "$rc"
    idc_st_assert_present "  naming the arm and the reason"               "REFUSED(2): TEXT_DENIED: no usable window scanner" "$out"
    # A scanner that answers its probe RIGHT and then stops reading: zero buffers accounted for.
    printf '#!/bin/sh\ncat > /dev/null\necho "%s"; echo "%s"; echo "%s"; echo "windows=0 buffers=0"\n' "$IDC_WPROBE_HEAD" "$IDC_WPROBE_A" "$IDC_WPROBE_B" > "$wk/b/perl"
    chmod 700 -- "$wk/b/perl" 2>/dev/null
    out="$d/w12.out"
    PATH="$wk/b:$PATH" IDC_WINDOWER="perl" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a scanner that SKIPS the buffers refuses"      2 "$rc"
    idc_st_assert_present "  naming the arm and the reason"               "REFUSED(2): TEXT_DENIED: no usable window scanner" "$out"
    # A scanner that answers its probe right, accounts for every buffer, and NAMES A WORD THE LIST
    # DOES NOT HOLD. The confirmation through the batch hasher and idc_denied_row is what stands
    # between that and a literal installed on a scanner's say-so.
    printf '#!/bin/sh\ns=0; n=0\nwhile IFS= read -r l; do case "$l" in "#"*) s=$((s + 1)) ;; "="*) [ "$s" -eq 2 ] && n=$((n + 1)) ;; esac; done\necho "%s"; echo "%s"; echo "%s"; echo "windows=1 buffers=$n"; echo "thanks"\n' "$IDC_WPROBE_HEAD" "$IDC_WPROBE_A" "$IDC_WPROBE_B" > "$wk/c/perl"
    chmod 700 -- "$wk/c/perl" 2>/dev/null
    out="$d/w13.out"
    PATH="$wk/c:$PATH" IDC_WINDOWER="perl" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a scanner naming a NON-ROW refuses to run"     2 "$rc"
    idc_st_assert_present "  naming the disagreement"                     "the scanner and the list disagree" "$out"
    # THE THREE LINES OF THE KNOWN ANSWER, ONE AT A TIME. The stand-in above returns three lines and
    # is refused for being short; it never reaches a comparison. These return a WELL-FORMED answer --
    # four lines, every buffer accounted for, no word -- that is wrong in exactly one place: the count
    # off by one, the first probe word, the second. Over a text that carries the word only GLUED, so
    # a scanner accepted here reads it CLEAN.
    local wrong="" wl1="" wl2="" wl3=""
    for wrong in head worda wordb; do
        wl1="$IDC_WPROBE_HEAD"; wl2="$IDC_WPROBE_A"; wl3="$IDC_WPROBE_B"
        case "$wrong" in
            head)  wl1="windows=30 buffers=1" ;;
            worda) wl2="idc-probe.b" ;;
            wordb) wl3="idcprobea" ;;
        esac
        mkdir -p -- "$wk/p$wrong"
        printf '#!/bin/sh\ns=0; n=0\nwhile IFS= read -r l; do case "$l" in "#"*) s=$((s + 1)) ;; "="*) [ "$s" -eq 2 ] && n=$((n + 1)) ;; esac; done\necho "%s"; echo "%s"; echo "%s"; echo "windows=1 buffers=$n"\n' "$wl1" "$wl2" "$wl3" > "$wk/p$wrong/perl"
        chmod 700 -- "$wk/p$wrong/perl" 2>/dev/null
        out="$d/w14.$wrong.out"
        PATH="$wk/p$wrong:$PATH" IDC_WINDOWER="perl" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/ws01" > "$out" 2>&1; rc=$?
        idc_st_rc             "a 4-line probe answer wrong in its $wrong refuses" 2 "$rc"
        idc_st_assert_present "  naming the arm and the reason"               "REFUSED(2): TEXT_DENIED: no usable window scanner" "$out"
    done

    # (n) THE PREFILTER, AND THE READERS BEHIND IT. Only a prefilter positive is put to idc_denied_row
    # / idc_admit_row, and every positive is: `lookups=` is counted inside the two readers. A reader
    # that is skipped reads one too few; a prefilter that lets every candidate through reads one for
    # every candidate, which is the 22 ms a candidate this arm once cost. STATED: the reader's ANSWER
    # cannot differ from the prefilter's -- both are read from the same rows by the same grammar -- so
    # what is pinned is that it is ASKED, exactly once a positive; no list a census can be handed
    # makes the two disagree.
    idc_st_lookups        "one denied word: the reader is asked ONCE"     1 "$d/x01.out"
    idc_st_lookups        "a denied and an ADMIT word: asked TWICE"       2 "$d/x07.out"
    idc_st_lookups        "a candidate that is no row: NEVER asked"       0 "$d/x05.out"

    # (o) THE SCAN READS THE BUFFER THE PASSES READ -- the public-URL span BLANKED. A denied word
    # broken by that span: the token passes blank the span, PASS 3 reduces what is left, and the two
    # halves meet. A scan of the bare lower-cased line would not find it, and the verdict would again
    # depend on the box. The span is a STAND-IN, in a copy of the definition whose public_url row is a
    # synthetic path -- so the case spells no real one, and this box's own definition is not what is
    # under test. The control is the same bytes around a path that is NOT the span.
    while IFS= read -r ln || [ -n "$ln" ]; do
        case "$ln" in
            "public_url"$'\t'*) printf 'public_url\t%s\tcontext\tstand-in: a synthetic published path\n' "$SYNP" ;;
            *) printf '%s\n' "$ln" ;;
        esac
    done < "$IDC_PATTERNS" > "$psyn"
    printf 'the owner %s%s%s said\n' "${TXW:0:4}" "$SYNP" "${TXW:4}" > "$d/w20"
    printf 'the owner %s%s%s said\n' "${TXW:0:4}" "quillhaven/otherrepo" "${TXW:4}" > "$d/w21"
    opat="$IDC_PATTERNS"; IDC_PATTERNS="$psyn"
    for sc in python python3 perl; do
        idc_st_can_scan "$sc" || continue
        idc_st_scan "[$sc] the scan ALONE finds: broken by the URL span" "$sc" "$d/w20" "$txrows" "$TXW"
    done
    IDC_PATTERNS="$opat"
    out="$d/w20.out"
    IDC_PATTERNS="$psyn" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w20" > "$out" 2>&1; rc=$?
    idc_st_rc             "a word broken by the PUBLIC-URL SPAN refuses"  1 "$rc"
    idc_st_assert_present "  discovered by the scan, hit in PASS 3"       "${TXHIT}3" "$out"
    out="$d/w20t.out"
    IDC_PATTERNS="$psyn" IDC_HASHES="$hl" IDC_TEST_TOKENS="TOKENFILE=$TXW" "$IDC_SELF" entry "$d/w20" > "$out" 2>&1; rc=$?
    idc_st_rc             "  as it does on a box HOLDING the literal"     1 "$rc"
    out="$d/w21.out"
    IDC_PATTERNS="$psyn" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w21" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and broken by ANOTHER path it is CLEAN"      0 "$rc"
    out="$d/w21t.out"
    IDC_PATTERNS="$psyn" IDC_HASHES="$hl" IDC_TEST_TOKENS="TOKENFILE=$TXW" "$IDC_SELF" entry "$d/w21" > "$out" 2>&1; rc=$?
    idc_st_rc             "  on a box HOLDING the literal too"            0 "$rc"
    # RESIDUAL 4 AGAIN, BY THE URL: the stand-in span here is the synthetic handle's own path, the
    # handle is an ADMIT row, and the word is spaced out on the same line. The passes blank the span,
    # but the admit reads its words from the BARE text, finds the handle inside the span, and clears
    # the line -- on the box that derives nothing and on the box that holds the word alike. `held`
    # below is that second box: the word under RUNTIME_ACCOUNT, the handle in its admit set.
    local held="RUNTIME_ACCOUNT=$TXW" rch=0
    printf 'public_url\t%s/synrepo\tcontext\tstand-in: the synthetic handle and a repository\n' "$TXH" > "$d/purl.row"
    while IFS= read -r ln || [ -n "$ln" ]; do
        case "$ln" in "public_url"$'\t'*) cat -- "$d/purl.row" ;; *) printf '%s\n' "$ln" ;; esac
    done < "$IDC_PATTERNS" > "$d/patterns.synhandle"
    printf 'see https://example.invalid/%s/synrepo as %s %s said\n' "$TXH" "${TXW:0:1}" "${TXW:1}" > "$d/w22"
    out="$d/w22.out"
    IDC_PATTERNS="$d/patterns.synhandle" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w22" > "$out" 2>&1; rc=$?
    idc_st_rc             "RESIDUAL 4: the URL + the word BROKEN beside it" 0 "$rc"
    out="$d/w22t.out"
    IDC_PATTERNS="$d/patterns.synhandle" IDC_HASHES="$hl" IDC_TEST_TOKENS="$held" IDC_TEST_ADMITS="$TXH" "$IDC_SELF" entry "$d/w22" > "$out" 2>&1; rch=$?
    idc_st_rc             "  CLEAN on a box HOLDING the word too"         0 "$rch"
    # The URL alone, against the list WITHOUT the ADMIT row: CLEAN because the span is BLANKED, not
    # because anything admitted it. With the blank dead the scan finds the word inside the handle
    # and nothing clears it.
    printf 'see https://example.invalid/%s/synrepo for the tree\n' "$TXH" > "$d/w23"
    out="$d/w23.out"
    IDC_PATTERNS="$d/patterns.synhandle" IDC_HASHES="$hlna" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w23" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and the URL alone is CLEAN -- blanked, not admitted" 0 "$rc"
    # THE HANDLE GLUED TO THE SPAN (2026-10-06). Blanking the span leaves the glued handle standing as
    # a word of the BUFFER, while in the bare text it is half of a longer word that is no ADMIT row.
    # The admit reads the bare text, so both shapes REFUSE, on both kinds of box. An admit that read
    # the buffer instead (the count that was taken back) read the handle glued IN FRONT of the span
    # CLEAN on the box that holds the handle and REFUSED on the box that derives nothing -- one text,
    # two verdicts, which is the one thing this arm exists to end -- and read the handle glued BEHIND
    # the span CLEAN on every box, where the tool before TEXT_DENIED refuses it.
    printf 'see %s%s/synrepo here\n' "$TXH" "$TXH" > "$d/w24"
    out="$d/w24t.out"
    IDC_PATTERNS="$d/patterns.synhandle" IDC_HASHES="$hl" IDC_TEST_TOKENS="$held" IDC_TEST_ADMITS="$TXH" "$IDC_SELF" entry "$d/w24" > "$out" 2>&1; rch=$?
    idc_st_rc             "the handle glued IN FRONT of the span refuses, held" 1 "$rch"
    out="$d/w24.out"
    IDC_PATTERNS="$d/patterns.synhandle" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w24" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and the box that derives nothing AGREES"     "$rch" "$rc"
    printf 'see %s/synrepo%s here\n' "$TXH" "$TXH" > "$d/w25"
    out="$d/w25t.out"
    IDC_PATTERNS="$d/patterns.synhandle" IDC_HASHES="$hl" IDC_TEST_TOKENS="$held" IDC_TEST_ADMITS="$TXH" "$IDC_SELF" entry "$d/w25" > "$out" 2>&1; rch=$?
    idc_st_rc             "the handle glued BEHIND the span refuses, held" 1 "$rch"
    out="$d/w25.out"
    IDC_PATTERNS="$d/patterns.synhandle" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/w25" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and on a box that derives nothing"           1 "$rc"

    # (p) EVERY BOX READS ONE TEXT TO ONE VERDICT -- three ways a reader used to read another text.
    # (p1) A NUL BYTE. mawk reads the rest of such a line as memory that is not the line's: a WHOLE
    # denied word after a NUL, UTF-16 text and an address after a NUL read CLEAN there and refused
    # under gawk. idc_census now hands every awk a copy with the NULs made \001. On gawk these were
    # green before that change; on mawk 1.3.4 20200120 all three were red.
    printf 'abc\000 the owner %s said\n' "$TXW" > "$d/y01"
    out="$d/y01.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/y01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a denied word AFTER A NUL refuses"             1 "$rc"
    idc_st_assert_present "  whole, in PASS 1"                            "${TXHIT}1" "$out"
    : > "$d/y02"; rest="the owner $TXW said"; k=0
    while [ "$k" -lt "${#rest}" ]; do printf '%s\000' "${rest:$k:1}" >> "$d/y02"; k=$((k + 1)); done
    printf '\n\000' >> "$d/y02"
    out="$d/y02.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/y02" > "$out" 2>&1; rc=$?
    idc_st_rc             "a denied word in UTF-16 text refuses"          1 "$rc"
    idc_st_assert_present "  each letter parted by a NUL: PASS 3"         "${TXHIT}3" "$out"
    printf 'abc\000 the box at %d.%d.%d.%d answered\n' 10 20 30 40 > "$d/y03"
    out="$d/y03.out"
    IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/y03" > "$out" 2>&1; rc=$?
    idc_st_rc             "an address AFTER A NUL refuses"                1 "$rc"
    idc_st_assert_present "  the structural arm, not a token arm"         "ipv4               pass1" "$out"
    # (p2) A FILE NAME SHAPED LIKE AN AWK ASSIGNMENT. awk took `name=value` as one and read no file;
    # the census read CLEAN with denied-words=1 printed above the verdict. Relative names, so the
    # child is run from their directory.
    mkdir -p -- "$d/eq"
    cp -- "$d/x01" "$d/eq/idcw=1.md"; cp -- "$d/ws01" "$d/eq/idcg=1.md"; cp -- "$d/y03" "$d/eq/idcv=1.md"
    out="$d/y04.out"
    ( cd -- "$d/eq" && IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "idcw=1.md" ) > "$out" 2>&1; rc=$?
    idc_st_rc             "a file NAMED like an assignment is still read" 1 "$rc"
    idc_st_assert_present "  the word found whole, PASS 1"                "${TXHIT}1" "$out"
    out="$d/y05.out"
    ( cd -- "$d/eq" && IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "idcg=1.md" ) > "$out" 2>&1; rc=$?
    idc_st_rc             "  and the word only GLUED in such a file"      1 "$rc"
    out="$d/y06.out"
    ( cd -- "$d/eq" && IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "idcv=1.md" ) > "$out" 2>&1; rc=$?
    idc_st_rc             "  and an address in such a file"               1 "$rc"
    # The same property asked of the RESULT: an awk that is handed the text and reads none of it. Two
    # stand-ins, each the real awk with the text operand swapped for the empty device -- the first in
    # the buffer awk only (the window scan would account for all of zero buffers), the second in the
    # census awk only (hits=0 over no lines). The awk probe is untouched by both and passes.
    mkdir -p -- "$d/nr1" "$d/nr2"
    printf '#!/bin/sh\nn=$#; dump=0\nfor a in "$@"; do case "$a" in DUMP=?*) dump=1 ;; esac; done\nwhile [ "$n" -gt 0 ]; do\n    a="$1"; shift; n=$((n - 1))\n    if [ "$dump" -eq %s ]; then case "$a" in */input.read) a=/dev/null ;; esac; fi\n    set -- "$@" "$a"\ndone\nexec "%s" "$@"\n' 1 "$realawk" > "$d/nr1/awk"
    printf '#!/bin/sh\nn=$#; dump=0\nfor a in "$@"; do case "$a" in DUMP=?*) dump=1 ;; esac; done\nwhile [ "$n" -gt 0 ]; do\n    a="$1"; shift; n=$((n - 1))\n    if [ "$dump" -eq %s ]; then case "$a" in */input.read) a=/dev/null ;; esac; fi\n    set -- "$@" "$a"\ndone\nexec "%s" "$@"\n' 0 "$realawk" > "$d/nr2/awk"
    chmod 700 -- "$d/nr1/awk" "$d/nr2/awk" 2>/dev/null
    out="$d/y07.out"
    PATH="$d/nr1:$PATH" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a buffer awk that READS NOTHING refuses to run" 2 "$rc"
    idc_st_assert_present "  naming the arm and the reason"               "wrote 0 buffers from an input that is not empty" "$out"
    out="$d/y08.out"
    PATH="$d/nr2:$PATH" IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/x01" > "$out" 2>&1; rc=$?
    idc_st_rc             "a census awk that READS NOTHING refuses to run" 2 "$rc"
    idc_st_assert_present "  naming the reason"                           "the census awk read no line of" "$out"
    # (p3) RESIDUAL 3, PINNED: A DOUBLE-BYTE CHARACTER WHOSE TRAIL BYTE IS AN ASCII LETTER, inside the
    # word. Every program here reads BYTES (LC_ALL=C, set by this tool whatever the caller exported),
    # so the trail byte is a letter to every reader and NO box finds the word -- the box that holds
    # the literal as little as the box that derives nothing. Before that, under a Shift-JIS locale the
    # holding box's awk dropped the pair whole and REFUSED, and the scanner's box read CLEAN. The
    # premise is a fact about the box -- its awk must know that locale for the old reading to exist --
    # so where it does not, the case that was red is named NOT APPLICABLE. The sibling, whose trail
    # byte is no letter, is found on every box.
    printf 'the owner %s\203\162%s said\n' "${TXW:0:4}" "${TXW:4}" > "$d/y09"
    printf 'the owner %s\203\100%s said\n' "${TXW:0:4}" "${TXW:4}" > "$d/y10"
    c="$(printf 'ab\203\162cd\n' | LC_ALL=ja_JP.SJIS awk '{ print length($0) }' 2>/dev/null)"
    local rcheld=0
    out="$d/y09t.out"
    LC_ALL=ja_JP.SJIS IDC_HASHES="$hl" IDC_TEST_TOKENS="TOKENFILE=$TXW" "$IDC_SELF" entry "$d/y09" > "$out" 2>&1; rcheld=$?
    out="$d/y09.out"
    LC_ALL=ja_JP.SJIS IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/y09" > "$out" 2>&1; rc=$?
    if [ "$c" = "5" ]; then
        idc_st_rc         "RESIDUAL 3: a double-byte break is CLEAN, held" 0 "$rcheld"
        # THE INVARIANT ITSELF, as one assertion: the two boxes give ONE verdict. Before the change
        # this was 1 against 0.
        idc_st_rc         "  and the box that derives nothing AGREES"     "$rcheld" "$rc"
    else
        idc_st_na         "RESIDUAL 3: a double-byte break is CLEAN, held" "this box's awk does not read ja_JP.SJIS as a double-byte locale, so it never read the pair as one character (exit=$rcheld here)"
        idc_st_na         "  and the box that derives nothing AGREES"     "the same (exit=$rc here)"
    fi
    out="$d/y10t.out"
    LC_ALL=ja_JP.SJIS IDC_HASHES="$hl" IDC_TEST_TOKENS="TOKENFILE=$TXW" "$IDC_SELF" entry "$d/y10" > "$out" 2>&1; rc=$?
    idc_st_rc             "  a trail byte that is NO letter refuses, held" 1 "$rc"
    out="$d/y10.out"
    LC_ALL=ja_JP.SJIS IDC_HASHES="$hl" IDC_TEST_TOKENS="$TXTOK" "$IDC_SELF" entry "$d/y10" > "$out" 2>&1; rc=$?
    idc_st_rc             "  and on a box that derives nothing"           1 "$rc"

    # (j) AND THE REAL LIST IS ONE THIS ARM CAN READ: section E below runs the arm over the shared
    # list and exits 2 if it cannot. What it does NOT prove is a real row firing -- no plaintext for
    # one may exist in this file. Section D's `a denied-set token clears the bars` is that control
    # on a box that can derive one; on a box that cannot, that case is NOT APPLICABLE and says so,
    # and the control is the Go guard's parity arm (TestFleetIdentifierHashFileMatchesTheGoLists).

    unset IDC_TEST_TOKENS IDC_TEST_ADMITS
    idc_build_tokens

    echo
    echo "  E. DECLARED SET"
    printf 'nothing of interest on this line\n' > "$d/decl"
    idc_census "$d/decl" "declared set" "$IDC_TMP/keys.decl" 1

    echo
    # The not-applicable count has a line of its own, FIRST, and the tally line keeps its exact shape:
    # every lane's tool parses `SELF-TEST: pass=N fail=M`, one of them anchored at both ends, and one
    # takes the first count of failures it meets anywhere in this output -- so no line above the tally
    # may spell that count's key. A zero prints: it is a reading too.
    echo "SELF-TEST: not-applicable=$IDC_ST_NA (cases whose premise is a fact about this box; each is named above with its reason, and none is counted as passed)"
    echo "SELF-TEST: pass=$IDC_ST_PASS fail=$IDC_ST_FAIL"
    case "$IDC_ST_FAIL" in
        ''|0) echo "SELF-TEST PASSED"; return 0 ;;
        *)    echo "SELF-TEST FAILED"; return 3 ;;
    esac
}

# -------------------------------------------------------------------------------------------------
while [ "$#" -gt 0 ]; do
    case "$1" in
        --unmask) IDC_UNMASK=1; shift ;;
        --) shift; break ;;
        *) break ;;
    esac
done
if [ "$#" -lt 1 ]; then idc_misuse "no mode given"; fi
IDC_MODE="$1"; shift
# THE AWK PROBE, FIRST, in every mode that reads a text. See the header.
case "$IDC_MODE" in
    entry|converted|subject|tree) idc_awk_probe || exit 2 ;;
esac
case "$IDC_MODE" in
    entry)     idc_mode_entry "$@";     exit $? ;;
    converted) idc_mode_converted "$@"; exit $? ;;
    subject)  idc_mode_subject "$@";  exit $? ;;
    tree)     idc_mode_tree "$@";     exit $? ;;
    selftest) idc_mode_selftest "$@"; exit $? ;;
    *)        idc_misuse "unknown mode '$IDC_MODE'" ;;
esac
