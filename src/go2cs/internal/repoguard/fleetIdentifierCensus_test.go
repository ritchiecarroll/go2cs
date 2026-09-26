// fleetIdentifierCensus_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"fmt"
	"os"
	"os/exec"
	"path"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// The owner's standing security order (2026-09-01) keeps real machine names and other
// internal-infrastructure identifiers off every pushed surface: fleet machines are referred to by
// nickname only, and real hostnames, UNC paths carrying them, share names, non-public usernames and
// profile paths stay off GitHub entirely. Both public tips were scrubbed the day the order landed.
//
// It came back. A census on 2026-09-04 found the pattern REINTRODUCED in tracked, already-pushed
// records -- profile paths carrying real account names across a dozen docs, plus a real machine name
// in the fleet roster table -- three days after the scrub, and it was found only because a census
// happened to be commissioned. That is the shape this guard exists for: nothing in the tree could
// see it, so the failure mode is silent, it recurs on whoever writes the next provisioning table or
// pastes the next shell transcript, and the cost of noticing is a human remembering to look.
//
// This is the same invariant-in-the-cheapest-place move projitemsIntegrity_test.go makes: it runs in
// the converter's own `go test ./...`, which every lane already pays for, so a reintroduction is a
// red converter suite at the merge rather than a scrub weeks later.
//
// It runs there from THIS package rather than from package main, and so does contextBudget_test.go
// beside it. Neither guard tests converter behaviour. Both used to fail as `go2cs`, so a docs or
// record edit could redden a run made right after a converter change, and the first hypothesis was
// that the converter change broke something. `go test ./...` from src\go2cs walks internal\, so
// nothing about WHEN or WHERE they run changed. What changed is that a failure prints as
// `go2cs/internal/repoguard`, which says "not converter code" before anyone forms a hypothesis.
//
// TWO PASSES, because neither sees what the other does.
//
//	Pass 1, PATH-ANCHORED and structural: a profile directory or network prefix whose identifier
//	segment is not a placeholder. It needs no list of names -- it is the SHAPE that is forbidden --
//	so it catches an account nobody has told this guard about, including a new machine's.
//
//	Pass 2, DENIED TOKEN: a known fleet identifier used OUTSIDE any path -- a directory listing's
//	owner/group column, an "account X" parenthetical, a machine name in a roster row. A
//	path-anchored pattern cannot reach those by construction, and in the measured case five
//	line-hits in five files were visible ONLY to this pass, the roster's machine name among them.
//
// The denylist is stored as SALT-FREE SHA-256 of the lowercased token, never as plaintext: a guard
// that spelled the identifiers it forbids would put them on the pushed surface itself, which is the
// thing being prevented. Hashes are checked against whole tokens and against each
// dot/hyphen/underscore component, so a machine name and the account name inside it both match.
//
// What is deliberately NOT flagged: the owner's PUBLIC name, e-mail and GitHub handle. Those are
// published attribution, not infrastructure, and the order names the latter. They are cleared by
// FILE below rather than by line, because line numbers drift and a stale line number silently
// disarms a guard (route #8's shape).

// fleetFinding is one hit. It carries the path, the line and the KIND -- never the offending text,
// so a failing run's own output cannot put an identifier into a build log.
type fleetFinding struct {
	Path string
	Line int
	Kind string
}

func (f fleetFinding) String() string {
	return fmt.Sprintf("%s:%d [%s]", f.Path, f.Line, f.Kind)
}

// fleetDeniedToken is one forbidden identifier, stored as the SHA-256 of its lowercased form.
//
// Len is carried WITH the hash rather than derived beside it, because the scan hashes only tokens of
// a denied length -- that is what keeps this pass cheap over a corpus this size -- and a length kept
// in a second list is a length that can silently disagree with its hash, which would disarm the entry
// while every test still passed. One struct, one entry, no way to add half of it.
//
// Adding a machine or an account spells nothing:
//
//	echo -n "<token>" | tr A-Z a-z | sha256sum
//
// Before adding one, check it is not already SUBSUMED. Components are hashed as well as whole runs,
// so an entry whose own token carries a separator can fire only on a WHOLE run -- no component can
// ever equal it, components carrying no separator by construction -- and any run that equals it
// necessarily presents its components on the same line. A machine name that embeds an
// already-denied account name as one of its components is therefore fully covered by that account
// row, and adding it as its own row is a row that can never produce a hit the account row does not.
// Measured 2026-09-07: exactly that entry was proposed for one fleet box, measured strictly
// subsumed across four planted arms, and NOT added. Dead weight here reads as diligence.
//
// A new row's evidence is a ONE-TIME red-first at cut time, because the positive control below
// drives a SYNTHETIC denied index by construction: a control that exercised a REAL row would have to
// spell the identifier, which is the thing this list exists to keep out of the tree. The control can
// prove the PASS is live; it cannot prove any particular row is. So plant the token in a tracked
// file FROM THE ENVIRONMENT -- never as a literal -- run TestNoFleetIdentifiersInTrackedFiles with
// and without the row, restore byte-identically, and record both arms in the commit.
type fleetDeniedToken struct {
	Len  int
	Hash string
	What string
}

var fleetDeniedTokens = []fleetDeniedToken{
	{7, "20befabea93592064aad4d07e1af70c5d6859667e1edffdb591accf60e2993ee", "fleet account name"},
	{8, "deff430814c33ac000dbdf4bd1061321b8387df004594375c947fabf73d3acc1", "fleet account name"},
	{13, "1070b0f89514d6852350c53ac7682edcb2d41f38d66fb95c340cdec08802c74e", "fleet machine name"},
	{15, "64bcb3dc70c4e605e5f1f29e4e42af4ab20de3b69f6452e5d5444305f432ea6d", "fleet machine name"},
}

// fleetPublicHandle is one of the owner's PUBLIC handles, admitted as a known public exception and
// stored exactly as a denied token is -- (length, SHA-256 of the lowercased form, reason) -- because
// a guard that spelled ANY account-derived token would put it on the pushed surface itself. The
// handles are public; the consistency is the point, so no reader of this file has to learn which of
// two spellings of the same idea is the safe one.
//
// RULED 2026-09-22 at the console. Two handles, and only these two:
//
//	the GitHub organisation / NuGet author handle -- the same token the public mail handle uses,
//	which appears in every `github.com/<handle>/go2cs` URL at master and in the registry search URLs
//	the published README carries;
//
//	the work-mail account handle -- the shorter of the two.
//
// WHAT IS NOT ADMITTED, and the ruling says so in as many words: the Windows ACCOUNT name as a bare
// token, every profile path carrying any of these (`users\<name>`, `/home/<name>`), hostnames, share
// names and addresses. The account name is a SUBSTRING of the longer handle, which is the whole
// reason this set exists -- the shell census's reduced pass read ten hits on the published README
// from that containment alone -- and admitting a WORD is the narrowest thing that fixes it.
//
// SEMANTICS, and they are deliberately the narrowest that can work: the admit is consulted only when
// a denied token matches INSIDE a longer word, and it is keyed on the hash of the ENCLOSING WORD,
// whole and lowercased. A bare denied token standing alone as a word is refused without the set
// being consulted at all (fleetLineHasDeniedToken tests the whole run FIRST and returns), a word
// that merely CONTAINS an admitted handle is refused because the lookup is by exact hash and never
// by containment, and the STRUCTURAL passes are untouched -- a profile segment spelled with a public
// handle is still an account segment and still a hit.
type fleetPublicHandle struct {
	Len  int
	Hash string
	What string
}

var fleetPublicHandles = []fleetPublicHandle{
	{14, "6e20b08e8ef15c17552c9ba5ee62ff640ea39552bbdccc1f0f6c0db5e5c92818", "the GitHub organisation / NuGet author handle"},
	{8, "f73dc7a07d73876ae6a1e18eb95a99cc5d12a628644c33ea307397700ba6c1f3", "the work-mail account handle"},
}

// fleetAdmitIndex groups the admitted handles by length, exactly as fleetDeniedIndex does, so a
// candidate word is hashed only when its length can possibly match.
func fleetAdmitIndex(hs []fleetPublicHandle) map[int]map[string]string {
	idx := map[int]map[string]string{}
	for _, h := range hs {
		if idx[h.Len] == nil {
			idx[h.Len] = map[string]string{}
		}
		idx[h.Len][h.Hash] = h.What
	}
	return idx
}

// fleetDeniedIndex groups the denylist by token length, so a line's tokens are hashed only when
// their length can possibly match.
func fleetDeniedIndex(toks []fleetDeniedToken) map[int]map[string]string {
	idx := map[int]map[string]string{}
	for _, t := range toks {
		if idx[t.Len] == nil {
			idx[t.Len] = map[string]string{}
		}
		idx[t.Len][t.Hash] = t.What
	}
	return idx
}

// fleetPlaceholderSegments are the redacted or generic segments a profile/network path is ALLOWED to
// carry. Anything opening with a substitution sigil (<, %, $, {, [, () is accepted too.
var fleetPlaceholderSegments = map[string]bool{
	"user": true, "users": true, "username": true, "user-name": true, "youruser": true,
	"profile": true, "profile-root": true, "home": true, "root": true, "name": true,
	"host": true, "hostname": true, "server": true, "share": true, "machine": true,
	"public": true, "default": true, "all users": true, "programdata": true,
	"userprofile": true, "%userprofile%": true, "unc": true, "...": true,
	"foo": true, "bar": true, "baz": true, "example": true, "redacted": true, "placeholder": true,
	"go": true, "gopher": true, "runner": true, "agent": true, "ubuntu": true, "vagrant": true,
	"administrator": true, "admin": true, "ci": true, "build": true, "dev": true,
}

// fleetNicknameHostSegments are the four fleet nicknames the owner's security order PRESCRIBES for
// pushed surfaces. They are admitted as UNC HOST segments -- and only there.
//
// Measured 2026-09-08: a mailbox line whose UNC host segment was ALREADY a nickname fired as
// [network-path], and the only way to satisfy this guard was to scrub a line that already complied
// into a generic placeholder. That is the guard teaching the next writer to delete exactly the
// information the order asks to be kept, so the spelling the order prescribes has to be a spelling
// the structural pass accepts.
//
// Deliberately NOT folded into fleetPlaceholderSegments, which the PROFILE arm shares: a nickname
// names a HOST, so an account segment spelled with one is still an account segment and still a hit.
//
// This costs the denied-token pass nothing. Admission here is STRUCTURAL only -- the denied-token
// pass runs over every line whatever the structural pass admitted -- so a denied real host inside a
// UNC still fires, and no nickname can ever clear a denied token.
var fleetNicknameHostSegments = map[string]bool{
	"r-laptop": true, "g-laptop": true, "i9": true, "i7": true,
}

// fleetClearedSegment clears one inspected, non-fleet segment in one file. Keyed by path AND
// segment rather than by line, so an edit above it cannot silently disarm the entry. Every entry
// below was read by eye during the 2026-09-04 census; the reason is the record of that reading.
//
// These twenty-two hits are every path-anchored hit in the tree outside docs/** at the time of the
// scrub, and they collapse to the pairs below because the generic UNC placeholders repeat verbatim
// across the converted platform flavours of the same upstream file.
var fleetClearedSegments = map[string]string{
	// A deliberately fictitious illustrative account in an emitted-XML example.
	"docs/ConversionStrategies-Reference.md|mason": "fictitious name in a documentation example",
	// Upstream Go doc comments and fixtures, carried verbatim into the converted corpus.
	"src/core/runtime/traceback.cs|rsc":           "upstream Go doc comment: an example traceback path",
	"src/core/os/user/linux/lookup_unix.cs|kevin": "upstream Go fixture: a passwd-format line in a doc comment",
}

// fleetClearedTokenFiles clears the DENIED-TOKEN pass for files that legitimately carry the owner's
// PUBLIC name or handle. Cleared by file, with the reason; pass 1 still applies to every one of them,
// so a profile path appearing in these files is still caught.
var fleetClearedTokenFiles = map[string]string{
	"AUTHORS":                                        "published attribution: the owner's public name and e-mail",
	"docs/_config.yml":                               "published site configuration: the owner's public name",
	"src/go2cs/winres/winres.json":                   "published copyright string: the owner's public name",
	"src/go2cs/.vscode/settings.json":                "spell-check dictionary word: the owner's public given name",
	"docs/PLAN-nugetgo.md":                           "a package-registry organisation name that is public on the registry",
	"docs/phase4/SESSION-ROLL-2026-09-01-EVENING.md": "address-style norms using the owner's public given name",
}

var (
	// A profile root followed by its identifier segment, either separator, either platform.
	fleetProfileRe = regexp.MustCompile("(?i)(?:users[\\\\/]+|/home/)([^\\\\/\\s\"'`,;:)\\]}>*|]+)")
	// A UNC prefix followed by a host segment, two characters minimum.
	//
	// The leading group is a hand-rolled lookbehind: RE2 has none, and without it every ESCAPED
	// backslash in a Go or C# string literal reads as a UNC prefix -- "core\\testing\\x",
	// "%SystemRoot%\\system32\\" and a captured "stderr:\\n0x7f..." all matched before this was
	// added, and a guard whose steady state is dozens of false positives is a guard nobody leaves
	// switched on.
	//
	// The rule is positive rather than subtractive, because subtracting only word characters still
	// admitted the `%`- and `:`-preceded cases above: a UNC prefix BEGINS a path token, so it must
	// sit at the start of the line or after whitespace, a quote or an opening delimiter. Submatch 2
	// is the host.
	fleetNetworkRe = regexp.MustCompile("(^|[\\s\"'`(\\[=,])\\\\\\\\([A-Za-z][A-Za-z0-9._-]+)\\\\")
)

// fleetIsUpstreamFixture reports whether a path is converted-upstream or captured test DATA, where a
// generic profile path or UNC example is the content itself: Go's own suites are full of them, and
// the converted corpus carries them verbatim.
//
// Only the STRUCTURAL pass is skipped for these. The denied-token pass still runs, so a real fleet
// identifier reaching a fixture -- an absolute source path baked into an emission, say -- is still
// caught. Skipping both would be the blind spot; skipping neither is 2,500 false positives.
func fleetIsUpstreamFixture(path string) bool {
	p := strings.ToLower(path)
	return strings.Contains(p, "/testdata/") ||
		strings.HasSuffix(p, "_test.cs") ||
		strings.HasSuffix(p, "_test.cs.auto") ||
		strings.HasSuffix(p, ".test")
}

// fleetEmbedResourceRe matches one //go:embed payload item exactly as the converter mints it
// (embedResourceItemLines in src/go2cs/embedDirective.go): Include first, then a LogicalName carrying
// the `go.embed/` prefix nothing but that emitter writes. Submatch 1 is the Include value, relative
// to the project file's directory, slash-separated and XML-escaped.
var fleetEmbedResourceRe = regexp.MustCompile(`<EmbeddedResource Include="([^"]+)" LogicalName="go\.embed/[^"]+" />`)

var fleetXMLAttributeUnescaper = strings.NewReplacer("&lt;", "<", "&gt;", ">", "&quot;", `"`, "&apos;", "'", "&amp;", "&")

// fleetEmbedPayloads returns the //go:embed PAYLOADS among the tracked paths: every file a tracked
// project file under src/core/ names as a converter-minted `<EmbeddedResource>` item, outside
// testdata/ (which fleetIsUpstreamFixture already admits).
//
// A payload is Go's own bytes carried verbatim, which is exactly what a testdata fixture is. It is the
// one way upstream bytes reach the corpus OUTSIDE a testdata directory: Go 1.24's
// internal/trace/traceviewer embeds static/trace_viewer_full.html, whose JavaScript regex source
// reads, escaped, as a UNC host (2 structural hits the day the H10 close's regen staged it). So it is
// admitted the way a fixture is: the STRUCTURAL pass is skipped, and the denied-token pass still runs.
//
// DERIVED FROM THE TRACKED PROJECT FILES, NOT COMPARED TO GOROOT. A byte-equality check would make a
// tree guard depend on a toolchain: this runs under the plain `go test ./...` on every lane, including
// boxes where the pinned GOROOT is not materialised, and across a hop the committed payload and the
// pinned GOROOT legitimately differ until the corpus is regenerated. The project files are in the
// tree being scanned, and they are the same derivation the close's -text pins read.
//
// THE TEETH, each one a way a hand-edit cannot widen this:
//   - the project file must be tracked and under src/core/ (the converter's output tree);
//   - the item must carry the converter's `go.embed/` LogicalName, in the order it emits;
//   - the Include must resolve INSIDE the project file's own directory (no "..", no absolute path);
//   - only paths in the scanned list are ever consulted, so an item naming an untracked file admits
//     nothing.
//
// It fails CLOSED: an emitter format change that this regex stops matching derives nothing, and the
// payload is then scanned like any other file and refused -- a red guard, never a silent admit.
func fleetEmbedPayloads(root string, rel []string) map[string]bool {
	payloads := map[string]bool{}

	for _, p := range rel {
		if !strings.HasPrefix(p, "src/core/") || !strings.HasSuffix(p, ".csproj") {
			continue
		}

		content, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))
		if err != nil {
			continue
		}

		dir := path.Dir(p)

		for _, m := range fleetEmbedResourceRe.FindAllSubmatch(content, -1) {
			include := fleetXMLAttributeUnescaper.Replace(string(m[1]))

			if strings.Contains(include, "\\") || path.IsAbs(include) {
				continue
			}

			clean := path.Clean(include)

			if clean == "." || clean == ".." || strings.HasPrefix(clean, "../") {
				continue
			}

			payload := path.Join(dir, clean)

			if fleetIsUpstreamFixture(payload) {
				continue
			}

			payloads[payload] = true
		}
	}

	return payloads
}

func fleetHash(s string) string {
	sum := sha256.Sum256([]byte(strings.ToLower(s)))
	return hex.EncodeToString(sum[:])
}

// fleetIsUnicodeEscapeSegment reports whether a captured UNC host segment is really a JSON unicode
// escape of a character rather than a machine name. PARITY with the census's `unc_escape` admit
// (coord-identifier-patterns.txt), ruled 2026-09-20.
//
// The converter's own glyphs -- the box, the generic-arity backtick, the address-of marker -- appear
// escaped inside every .NET stack trace that names a converted type, and a comparison document is
// full of them. Escaped, they satisfy fleetNetworkRe's host class exactly.
//
// WHY IT IS SOUND rather than a convenience: inside a JSON string a literal backslash is DOUBLED, so
// a real UNC prefix that survives JSON encoding carries FOUR backslashes while an escape carries TWO.
// Measured 2026-09-20 on one committed line that carries both shapes side by side.
//
// COST, stated: a UNC in RAW text whose host BEGINS with that shape is admitted here. The
// denied-token pass is the mitigation and is untouched -- it runs over every line whatever the
// STRUCTURAL pass admitted, so a denied real host inside such a UNC still fires AS A WHOLE RUN OR AS
// A DOT/HYPHEN/UNDERSCORE COMPONENT of one. It does NOT reach a denied token concatenated onto the
// escape with no separator, because fleetLineHasDeniedToken has no substring pass -- measured, and
// the same bound the nickname admit has always carried without naming it.
//
// Scoped to the NETWORK-PATH kinds at its one call site: a profile segment of that shape is not an
// escape of anything and is still an account segment.
func fleetIsUnicodeEscapeSegment(seg string) bool {
	if len(seg) < 5 || (seg[0] != 'u' && seg[0] != 'U') {
		return false
	}

	for i := 1; i < 5; i++ {
		c := seg[i]
		if !(c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F') {
			return false
		}
	}

	return true
}

func fleetIsPlaceholder(seg string) bool {
	// A single character is a stand-in, not an account: the converter's own spelling guards write
	// `C:\Users\u\sdk\...` precisely to avoid naming one. The denied-token pass covers real names
	// whatever their length, so nothing is lost here.
	if len(seg) < 2 {
		return true
	}
	if strings.ContainsAny(seg[:1], "<%${[(") {
		return true
	}
	return fleetPlaceholderSegments[strings.ToLower(seg)]
}

// scanFleetIdentifiers runs both passes over one file's bytes. denied is a parameter rather than a
// package global so the positive control can drive the denied-token pass with a synthetic entry,
// exercising this exact code path without any test spelling a real identifier.
func scanFleetIdentifiers(path string, content []byte, denied, admit map[int]map[string]string) []fleetFinding {
	return scanFleetContent(path, content, denied, admit, fleetIsUpstreamFixture(path))
}

// scanFleetContent is scanFleetIdentifiers with the upstream-data decision made by the caller:
// scanFleetTree also admits the //go:embed payloads its tracked project files name (see
// fleetEmbedPayloads), which no predicate on the path alone can know.
func scanFleetContent(path string, content []byte, denied, admit map[int]map[string]string, upstream bool) []fleetFinding {
	if bytes.IndexByte(content, 0) >= 0 {
		return nil // binary
	}
	var out []fleetFinding
	clearedTokens := fleetClearedTokenFiles[path] != ""
	structural := !upstream

	// What the PER-LINE pass actually recorded, keyed by arm and segment. The joined pass consults it
	// so that a hit living on ONE line is reported once rather than twice -- see the suppression note
	// above that pass.
	reportedInline := map[string]bool{}
	inlineDeniedToken := false

	// Lines are walked in place rather than through strings.Split: this runs over every tracked
	// file in the repository, and materialising a slice of every line of the converted corpus cost
	// more than the matching did.
	rest := content
	for n := 1; len(rest) > 0 || n == 1; n++ {
		var line []byte
		if i := bytes.IndexByte(rest, '\n'); i >= 0 {
			line, rest = rest[:i], rest[i+1:]
		} else {
			line, rest = rest, nil
		}
		if len(line) == 0 {
			if rest == nil {
				break
			}
			continue
		}

		if structural {
			// Cheap prefilters. The regexes are the expensive part and almost no line can match
			// either, so ask a substring question first: a profile path needs a profile root, and a
			// network path needs a doubled backslash.
			if fleetHasFold(line, "users") || bytes.Contains(line, []byte("/home/")) {
				for _, m := range fleetProfileRe.FindAllSubmatch(line, -1) {
					// No admit set: a nickname names a host, never an account.
					if fleetConsiderSegment(&out, path, n, "profile-path", string(m[1]), nil) {
						reportedInline["profile-path|"+strings.ToLower(string(m[1]))] = true
					}
				}
			}
			if bytes.Contains(line, []byte(`\\`)) {
				for _, m := range fleetNetworkRe.FindAllSubmatch(line, -1) {
					if fleetConsiderSegment(&out, path, n, "network-path", string(m[2]), fleetNicknameHostSegments) {
						reportedInline["network-path|"+strings.ToLower(string(m[2]))] = true
					}
				}
			}
		}
		if clearedTokens {
			continue
		}
		if fleetLineHasDeniedToken(line, denied, admit) {
			out = append(out, fleetFinding{path, n, "denied-token"})
			inlineDeniedToken = true
		}
	}

	// ⚠ THE SECOND PASS, over the same content with LINE BREAKS CLOSED. Every arm above is
	// line-based, so a token that exists on NO SINGLE LINE is invisible to all of them at once --
	// measured on this guard: the inline shape FIRES while the same token split across a break, with
	// an indented continuation, or with a trailing space before the break, all read zero findings.
	// The hole was found in three independent gates on one day (a lane's post census, a second
	// lane's pre-post census, and this one), which is why the remedy is placed HERE rather than in
	// each arm: a per-arm patch protects today's arms and silently misses the one added tomorrow.
	//
	// Whitespace is collapsed only where it ABUTS the break. Stripping all whitespace would close
	// the same shapes and fuse arbitrary adjacent words, so the short arms would start firing on
	// ordinary prose; restricting the fusion to line boundaries keeps the false-positive surface to
	// word pairs that a break separates. Findings carry line 0 and a "-split" kind, because a line
	// number means nothing in joined text and a reader must not be sent to a line that reads clean.
	//
	// ⚠ SUPPRESSION: THIS PASS REPORTS ONLY WHAT THE PER-LINE PASS DID NOT (coordinator ruling,
	// mailbox 1dd7d4671 §1, 2026-09-13). An INLINE hit survives the break-collapsing unchanged, so
	// until that ruling every single-line hit matched in BOTH passes and was recorded twice --
	// measured 4 of 4 classes: one leaked path printed `2 fleet-identifier hit(s)`, the second being
	// the same hit at line 0. Over-report was never a SAFETY defect and the file list was always
	// right; the cost was legibility, and in precisely the shape the paragraph above says line 0
	// exists to avoid -- a reader sent to a non-line for a hit already named one line down.
	//
	// It was invisible for as long as it existed because every plant asserted that a hit of the
	// expected kind was PRESENT and none asserted what ELSE was in the result: the clean arms are
	// two-sided (`len(got) != 0`), the firing arms were one-sided. The plants now declare their whole
	// finding SET, which is what red-proves this suppression -- an inline plant must read EXACTLY one
	// finding of the per-line kind and a wrapped one EXACTLY one of the split kind, both in the same
	// subtest, so a suppression that removed too much goes red beside one that removed too little.
	//
	// The structural arms suppress by (arm, SEGMENT), which is the finest span this scanner carries:
	// a file holding an inline leak AND a differently-wrapped one still reports both. The DENIED-TOKEN
	// arm is coarser and says so -- the joined side of it is a file-level boolean by construction
	// (`fleetLineHasDeniedToken` over the whole joined text, one finding per file however many splits
	// there are), so there is no finer span to compare and it suppresses when the per-line pass
	// reported a denied token anywhere in the same file. Nothing actionable is lost: the file is
	// already refused and already named, and a wrapped token still fires once the inline one is gone.
	if joined := fleetJoinLineBreaks(content); joined != nil {
		if structural {
			if fleetHasFold(joined, "users") || bytes.Contains(joined, []byte("/home/")) {
				// ⚠ ON THE JOINED SURFACE ONLY, the path must CONTINUE past the segment. Measured
				// 2026-09-08, after i9 (0a1e7a0f8d) found that collapsing whitespace at a break fuses
				// ORDINARY PROSE into what short structural arms match, and R (6fc8978fa8) measured the
				// opposite constraint -- that dropping these arms from the joined pass is the
				// FALSE-PASS direction, because a genuinely wrapped path then goes clean.
				//
				// Both are true of THIS gate, so neither lane's remedy was taken. Unqualified, this arm
				// gave FOUR false refusals on prose this project writes constantly (a line ending in a
				// profile-root word, the next opening with a separator and a short path word); removing
				// it from the joined surface lost THREE of four wrap positions for an account not yet on
				// the denylist. The discriminator that separates them is not length -- a fused word of
				// nine or ten characters is ordinary here -- it is that A LEAKED PATH CONTINUES past the
				// account and FUSED PROSE DOES NOT: the next byte is a separator in the first case and a
				// space or end-of-line in the second.
				//
				// Measured both ways with the arm set this gate actually has: EIGHT of eight wrap
				// positions still refuse (posix and windows spellings, long and SHORT accounts, wrapped
				// inside the account, at the separator, inside the root word, and before it), and all
				// five prose shapes go clean including the long fused words. The residual is ONE shape
				// -- a break falling exactly at the separator AND the path ending at the account AND the
				// account unknown -- which the unqualified arm did catch; every other ending, and every
				// denied account, is still refused. Stated because it is a real if narrow loss.
				for _, ix := range fleetProfileRe.FindAllSubmatchIndex(joined, -1) {
					end := ix[3]
					if end < len(joined) && (joined[end] == '/' || joined[end] == '\\') {
						seg := string(joined[ix[2]:ix[3]])
						if reportedInline["profile-path|"+strings.ToLower(seg)] {
							continue
						}
						fleetConsiderSegment(&out, path, 0, "profile-path-split", seg, nil)
					}
				}
			}
			if bytes.Contains(joined, []byte(`\\`)) {
				for _, m := range fleetNetworkRe.FindAllSubmatch(joined, -1) {
					seg := string(m[2])
					if reportedInline["network-path|"+strings.ToLower(seg)] {
						continue
					}
					fleetConsiderSegment(&out, path, 0, "network-path-split", seg, fleetNicknameHostSegments)
				}
			}
		}
		if !clearedTokens && !inlineDeniedToken && fleetLineHasDeniedToken(joined, denied, admit) {
			out = append(out, fleetFinding{path, 0, "denied-token-split"})
		}
	}

	return out
}

// fleetJoinLineBreaks returns content with every line break -- and the whitespace immediately
// abutting it -- removed, so a token split across a break becomes contiguous for the line-based
// arms above. It returns nil when there is nothing to join, so a single-line file costs one scan
// and no allocation.
func fleetJoinLineBreaks(content []byte) []byte {
	if bytes.IndexByte(content, '\n') < 0 {
		return nil
	}

	out := make([]byte, 0, len(content))
	i := 0
	for i < len(content) {
		c := content[i]
		if c != '\n' && c != '\r' {
			out = append(out, c)
			i++
			continue
		}
		// Drop the break, the whitespace before it (already appended), and the whitespace after.
		for len(out) > 0 && (out[len(out)-1] == ' ' || out[len(out)-1] == '\t') {
			out = out[:len(out)-1]
		}
		for i < len(content) && (content[i] == '\n' || content[i] == '\r' || content[i] == ' ' || content[i] == '\t') {
			i++
		}
	}
	return out
}

// fleetConsiderSegment records one inspected segment unless something admits it. admitted is the
// per-ARM allow set -- the network arm passes the fleet nicknames, the profile arm passes nothing --
// so the SCOPE of an admission is carried by the caller that knows which arm it is, rather than
// re-derived here from the kind string, where a drifting literal could widen it silently.
// It reports whether it RECORDED a finding, which the joined pass needs: a segment the per-line pass
// already reported must not be reported a second time from the joined surface (see the suppression
// note there), and "admitted" and "recorded" are not the same answer.
func fleetConsiderSegment(out *[]fleetFinding, path string, line int, kind, seg string, admitted map[string]bool) bool {
	if fleetIsPlaceholder(seg) {
		return false
	}
	if admitted[strings.ToLower(seg)] {
		return false
	}

	// PER KIND, and only here: the escape admit reaches the NETWORK-PATH arms and nothing else.
	if strings.HasPrefix(kind, "network-path") && fleetIsUnicodeEscapeSegment(seg) {
		return false
	}
	if _, ok := fleetClearedSegments[path+"|"+strings.ToLower(seg)]; ok {
		return false
	}
	*out = append(*out, fleetFinding{path, line, kind})
	return true
}

// fleetHasFold is a case-insensitive substring test that does not allocate a lowered copy of the
// line; needle must already be lowercase ASCII.
func fleetHasFold(hay []byte, needle string) bool {
	n := len(needle)
	for i := 0; i+n <= len(hay); i++ {
		ok := true
		for j := 0; j < n; j++ {
			c := hay[i+j]
			if c >= 'A' && c <= 'Z' {
				c += 'a' - 'A'
			}
			if c != needle[j] {
				ok = false
				break
			}
		}
		if ok {
			return true
		}
	}
	return false
}

// fleetLineHasDeniedToken walks identifier-shaped runs by hand -- a regexp tokenizer over every line
// of the corpus was the single most expensive thing this guard did -- and hashes a run only when its
// length is one a denied token could have. Each hyphen/dot/underscore component is tested as well as
// the whole run, so a machine name and the account name inside it both match.
//
// '_' joined the separators on 2026-09-07, by measurement rather than by reading: it is a token
// CHARACTER but was not a SPLIT character, so `x_<denied>_y` passed this guard while `x-<denied>-y`
// was caught -- a gap with no principle behind it, since an owner column, a share name, a Windows
// account and an environment variable all join with '_' exactly as readily as with '-'. The
// widening cost the corpus nothing: the whole-tree run is green before and after.
//
// ⚠ THE PUBLIC-HANDLE ADMIT (2026-09-22) LIVES HERE, AND ITS PLACEMENT IS THE WHOLE OF ITS SCOPE.
// The WHOLE RUN is tested against the denylist FIRST and returns without the admit set being read at
// all, so a bare denied token standing alone as a word can never be admitted by anything. The admit
// is consulted only between that test and the COMPONENT walk -- that is, only where a denied token
// would be found INSIDE a longer word -- and it is keyed on the hash of the enclosing run, whole and
// lowercased. Three properties follow from the placement rather than from a rule anyone has to
// remember: a bare denied token still refuses, a word that merely CONTAINS an admitted handle still
// refuses (the lookup is an exact hash, never containment), and the STRUCTURAL passes never reach
// this function, so `users\<handle>\x` is still a profile-path hit.
func fleetLineHasDeniedToken(line []byte, denied, admit map[int]map[string]string) bool {
	isTok := func(c byte) bool {
		return c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9' ||
			c == '.' || c == '_' || c == '-'
	}
	for i := 0; i < len(line); {
		if !isTok(line[i]) {
			i++
			continue
		}
		j := i
		for j < len(line) && isTok(line[j]) {
			j++
		}
		tok := line[i:j]
		if fleetTokenDenied(tok, denied) {
			return true
		}
		if !fleetWordAdmitted(tok, admit) {
			for k, p := 0, 0; k <= len(tok); k++ {
				if k == len(tok) || tok[k] == '-' || tok[k] == '.' || tok[k] == '_' {
					if k > p && fleetTokenDenied(tok[p:k], denied) {
						return true
					}
					p = k + 1
				}
			}
		}
		i = j
	}
	return false
}

func fleetTokenDenied(tok []byte, denied map[int]map[string]string) bool {
	byHash, ok := denied[len(tok)]
	if !ok {
		return false
	}
	_, hit := byHash[fleetHash(string(tok))]
	return hit
}

// fleetWordAdmitted reports whether one whole identifier run is one of the owner's admitted public
// handles. Same shape as fleetTokenDenied on purpose -- length-bucketed, exact hash of the lowercased
// form -- so the two lists are read the same way and neither can grow a containment rule the other
// does not have.
func fleetWordAdmitted(tok []byte, admit map[int]map[string]string) bool {
	byHash, ok := admit[len(tok)]
	if !ok {
		return false
	}
	_, hit := byHash[fleetHash(string(tok))]
	return hit
}

// scanFleetTree scans an explicit list of paths relative to root. Factored out so the positive
// control drives the SAME walk -- binary skip, line splitting and both passes -- over a temporary
// tree, rather than testing a reimplementation of it.
func scanFleetTree(root string, rel []string, denied, admit map[int]map[string]string) ([]fleetFinding, int) {
	var out []fleetFinding
	read := 0
	payloads := fleetEmbedPayloads(root, rel)
	for _, p := range rel {
		content, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))
		if err != nil {
			continue // unreadable or a submodule entry; never a pass by omission, see the count assert
		}
		read++
		out = append(out, scanFleetContent(p, content, denied, admit, fleetIsUpstreamFixture(p) || payloads[p])...)
	}
	sort.Slice(out, func(i, j int) bool {
		if out[i].Path != out[j].Path {
			return out[i].Path < out[j].Path
		}
		return out[i].Line < out[j].Line
	})
	return out, read
}

// repoRootFromPackageDir walks up from the package directory to the first ancestor holding a .git
// entry, which is the repository root.
//
// It SEARCHES rather than counting levels. It used to be a fixed filepath.Dir(filepath.Dir(wd)), which
// was right only while these guards lived in src\go2cs, and moving them to src\go2cs\internal\repoguard
// would have silently pointed it at src\go2cs instead of the root. A fixed depth breaks on every move,
// and a search does not.
//
// .git is tested with os.Stat and never IsDir: in a git WORKTREE it is a FILE naming the real git
// directory, and every lane in this fleet works from worktrees. The NEAREST .git wins, which is git's
// own discovery rule, so the root found here is the tree `git -C <root> ls-files` then enumerates. No
// .git anywhere above is a loud failure, because a guard that read a wrong root would scan the wrong
// tree and pass.
func repoRootFromPackageDir(t *testing.T) string {
	t.Helper()
	wd, err := os.Getwd()
	if err != nil {
		t.Fatalf("cannot determine the working directory: %v", err)
	}
	for dir := wd; ; {
		if _, err := os.Stat(filepath.Join(dir, ".git")); err == nil {
			return dir
		}
		parent := filepath.Dir(dir)
		if parent == dir {
			t.Fatalf("repository root not found: no .git entry in %s or any ancestor", wd)
		}
		dir = parent
	}
}

// TestNoFleetIdentifiersInTrackedFiles is the guard. It enumerates TRACKED files only -- an
// untracked scratch file is nobody's pushed surface -- and reads them from disk.
//
// The files it reads live outside this module (docs\, src\core\, the repository root), and cmd/go
// drops out-of-module files from the test input hash, so a cached PASS here would survive a
// reintroduction: run the converter suite with -count=1, which is what every gate in this repo
// already does after a harness-only change.
func TestNoFleetIdentifiersInTrackedFiles(t *testing.T) {
	root := repoRootFromPackageDir(t)

	cmd := exec.Command("git", "-C", root, "ls-files", "-z")
	out, err := cmd.Output()
	if err != nil {
		// An instrument that cannot enumerate must not report success.
		t.Fatalf("git ls-files failed in %s: %v", root, err)
	}
	var files []string
	for _, p := range strings.Split(string(out), "\x00") {
		if p != "" {
			files = append(files, p)
		}
	}
	if len(files) < 1000 {
		t.Fatalf("git ls-files returned %d paths, which is too few to be this repository -- "+
			"the enumeration is broken, and a guard that scans nothing passes everything", len(files))
	}

	findings, read := scanFleetTree(root, files, fleetDeniedIndex(fleetDeniedTokens), fleetAdmitIndex(fleetPublicHandles))
	if read < len(files)*9/10 {
		t.Fatalf("read only %d of %d tracked files; the scan has a hole", read, len(files))
	}
	if len(findings) == 0 {
		return
	}

	byFile := map[string]int{}
	for _, f := range findings {
		byFile[f.Path]++
	}
	names := make([]string, 0, len(byFile))
	for p := range byFile {
		names = append(names, p)
	}
	sort.Strings(names)

	var b strings.Builder
	fmt.Fprintf(&b, "%d fleet-identifier hit(s) in %d tracked file(s).\n", len(findings), len(byFile))
	b.WriteString("The owner's 2026-09-01 security order keeps real machine names, non-public\n")
	b.WriteString("usernames and profile paths off every pushed surface. Substitute the identifier\n")
	b.WriteString("ALONE -- <user> for an account segment, the machine's fleet nickname for a host --\n")
	b.WriteString("leaving the rest of the line untouched, so the record still says what it said.\n")
	b.WriteString("A genuinely generic or fictitious segment belongs in fleetPlaceholderSegments or\n")
	b.WriteString("fleetClearedSegments, with the reason, NOT scrubbed.\n")
	for _, p := range names {
		fmt.Fprintf(&b, "  %4d  %s\n", byFile[p], p)
	}
	b.WriteString("Sites (text withheld deliberately -- a failing build log is a pushed surface too):\n")
	for _, f := range findings {
		fmt.Fprintf(&b, "  %s\n", f)
	}
	t.Fatal(b.String())
}

// TestFleetIdentifierScannerFiresAndRestores is the positive control. A guard that has never been
// made to fail proves nothing, and this one's steady state is silence, so its greens are worthless
// until it has been shown to go red on demand.
//
// It plants into a REAL file in a temporary tree and drives the real walk, then restores and asserts
// the restore is byte-identical -- so the control also demonstrates that a clean tree reads clean
// through the same path that reported the hit.
func TestFleetIdentifierScannerFiresAndRestores(t *testing.T) {
	// A synthetic denied token, so the control exercises the denied-token pass without any test
	// source spelling a real fleet identifier.
	const controlToken = "zzcontrolaccount"
	denied := fleetDeniedIndex([]fleetDeniedToken{{len(controlToken), fleetHash(controlToken), "control token"}})

	clean := "toolchain root at C:\\Users\\<user>\\sdk and /home/<user>/go\n" +
		"a UNC share at \\\\host\\share\\path and \\\\server\\share\n" +
		"the roster row names the machine by its nickname i9\n"

	// The planted paths are ASSEMBLED through Sprintf rather than written as literals, because this
	// file is itself a tracked file and the guard scans it: written out, the two plants below were
	// found by the guard reading its own source, which is exactly what the first green-arm run
	// reported. Exempting this file would have been the wrong fix -- it is the file most likely to
	// be edited by whoever adds the next denylist entry, so an exemption here is a permanent hole.
	// A verb leaves `%s` where the segment goes, and `%` is a substitution sigil, so the source text
	// reads as a placeholder while the RUNTIME string carries a real-looking identifier.
	const seg = "zzexampleaccount"
	const host = "zzexamplehost"

	plants := []struct {
		name string
		line string
		kind string
		// split marks a plant whose token exists on NO SINGLE LINE. The joined pass reports those
		// at line 0, because a line number means nothing in joined text.
		split bool
	}{
		{"windows profile path", fmt.Sprintf("root at C:\\Users\\%s\\sdk\n", seg), "profile-path", false},
		{"posix home path", fmt.Sprintf("root at /home/%s/go\n", seg), "profile-path", false},
		{"unc host", fmt.Sprintf("share at \\\\%s\\public\\x\n", host), "network-path", false},
		{"bare denied token", "owner column reads " + controlToken + " here\n", "denied-token", false},
		{"denied token inside a machine name", "row names " + controlToken + "-desk2\n", "denied-token", false},
		// The arm that pays for the 2026-09-07 widening: '_' is a token character, so without it in
		// the split set this line's whole run is one 20-character token that matches no bucket and
		// the plant goes UNDETECTED. Remove '_' from fleetLineHasDeniedToken and this arm goes red.
		{"denied token joined by underscores", "owner column reads x_" + controlToken + "_y\n", "denied-token", false},

		// ⚠ THE SPLIT ARMS. Every plant above sits on ONE line, and for a long time so did every arm
		// of this control -- six shapes, one geometry -- which is exactly why the line-break hole
		// survived in this guard and in two other gates until three lanes probed for it on the same
		// day. A token that exists on no single line was invisible to every line-based arm at once.
		// Remove the joined pass in scanFleetIdentifiers and these three go red; the six above stay
		// green, which is the whole point of adding them.
		{"profile path split across a line break", fmt.Sprintf("root at /home/\n%s/go\n", seg), "profile-path-split", true},
		{"profile path split with an indented continuation", fmt.Sprintf("root at /home/\n    %s/go\n", seg), "profile-path-split", true},
		{"denied token split across a line break", "owner column reads " + controlToken[:6] + "\n" + controlToken[6:] + " here\n", "denied-token-split", true},

		// Shapes contributed by other lanes' probes, added because each was found by RUNNING a
		// neighbour's control rather than reasoning that this one covered it. G named the
		// trailing-space break; R named the BLANK LINE -- a paragraph break, the commonest break in
		// prose, which none of the three gates had tested. Both pass here, and that is knowable only
		// because they were run: R's own fix covered the bare split and left the indented case open
		// on exactly the reasoning that "should cover it".
		{"profile path split with a trailing space", fmt.Sprintf("root at /home/ \n%s/go\n", seg), "profile-path-split", true},
		{"profile path split across a BLANK LINE", fmt.Sprintf("root at /home/\n\n%s/go\n", seg), "profile-path-split", true},
		{"profile path split across a blank line with indent", fmt.Sprintf("root at /home/\n   \n   %s/go\n", seg), "profile-path-split", true},

		// ⚠ network-path-split -- the class that had NO arm here at all until 2026-09-13, while every
		// other kind this scanner emits had one. Its kind string appeared only at its own emit site,
		// because every UNC fixture in this file is single-line.
		//
		// It was never DEAD, which is the part worth recording: measured through this same scan path,
		// it fires on five of six wrap positions, and it had been firing all along inside
		// TestFleetIdentifierNicknameHostsAreAdmitted on every INLINE UNC fixture -- with nothing
		// asserting on it, because the assertions there ask only whether the expected kind is present.
		// A class can be alive and invisible, and the gap was in the assertions rather than the arm.
		//
		// The sixth position is left OUT deliberately rather than silently: a break falling BEFORE the
		// two backslashes puts the whole UNC on one line, and the per-line arm correctly reports
		// `network-path` there. That is the reading, not a miss.
		{"unc host split right after the two backslashes", fmt.Sprintf("share at \\\\\n%s\\public\\x\n", host), "network-path-split", true},
		{"unc host split inside the host segment", fmt.Sprintf("share at \\\\%s\n%s\\public\\x\n", host[:6], host[6:]), "network-path-split", true},
		{"unc host split before its own separator", fmt.Sprintf("share at \\\\%s\n\\public\\x\n", host), "network-path-split", true},
		{"unc host split with an indented continuation", fmt.Sprintf("share at \\\\\n    %s\\public\\x\n", host), "network-path-split", true},
		{"unc host split across a BLANK LINE", fmt.Sprintf("share at \\\\\n\n%s\\public\\x\n", host), "network-path-split", true},
	}

	for _, p := range plants {
		t.Run(p.name, func(t *testing.T) {
			dir := t.TempDir()
			rel := "docs/phase4/CONTROL-record.md"
			full := filepath.Join(dir, filepath.FromSlash(rel))
			if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
				t.Fatal(err)
			}
			if err := os.WriteFile(full, []byte(clean), 0o644); err != nil {
				t.Fatal(err)
			}
			original, err := os.ReadFile(full)
			if err != nil {
				t.Fatal(err)
			}

			// GREEN before: the clean record, whose placeholders and nickname are exactly what a
			// scrubbed file looks like, must not fire. Without this arm a scanner that flagged
			// everything would pass the red arm below.
			if got, _ := scanFleetTree(dir, []string{rel}, denied, nil); len(got) != 0 {
				t.Fatalf("clean record fired: %v", got)
			}

			// RED: plant, and require a hit of the RIGHT kind on the RIGHT line.
			if err := os.WriteFile(full, append(append([]byte{}, original...), []byte(p.line)...), 0o644); err != nil {
				t.Fatal(err)
			}
			got, _ := scanFleetTree(dir, []string{rel}, denied, nil)
			if len(got) == 0 {
				t.Fatalf("planted %s was NOT detected -- this guard cannot go red", p.name)
			}
			wantLine := strings.Count(clean, "\n") + 1
			if p.split {
				wantLine = 0 // the joined pass has no meaningful line number
			}

			// ⚠ THE DECLARED SET, not merely membership in it (coordinator ruling, mailbox 1dd7d4671
			// §1 item 2). Every plant below carries exactly ONE identifier, so the whole expected
			// result is one finding of one kind -- and asserting that, rather than "the kind I want is
			// somewhere in here", is what makes this arm able to see what it did not expect.
			//
			// It is here because the one-sided version hid a real defect for as long as it existed:
			// until the joined pass gained its suppression, EVERY single-line hit was reported twice
			// (once per pass, the second at line 0), measured on all four classes, and no arm could
			// say so -- each looked only for the kind it planted. The clean arms were two-sided all
			// along (`len(got) != 0`), which is why the zero direction was covered and this one was
			// not. AN ASSERTION THAT ONLY LOOKS FOR WHAT IT EXPECTS CANNOT SEE WHAT IT DID NOT.
			//
			// It is also the red-proof of that suppression, in both directions at once: a suppression
			// that removed too much drops the count to zero at the arm above, and one that removed too
			// little puts a second finding here. i9's cross-arm clause (mailbox 1ae48b10b §2) in the
			// form that lane measured to be satisfiable -- a set the plant DECLARES, not a blanket
			// zero over arms that overlap by construction.
			if len(got) != 1 {
				t.Fatalf("planted %s produced %d findings %v, want EXACTLY one (%q at line %d) -- "+
					"a plant carrying one identifier that reads as more than one finding is the scanner "+
					"reporting the same hit twice, which no arm could see while they asserted presence only",
					p.name, len(got), got, p.kind, wantLine)
			}
			found := false
			for _, f := range got {
				if f.Kind == p.kind && f.Line == wantLine && f.Path == rel {
					found = true
				}
			}
			if !found {
				t.Fatalf("planted %s detected as %v, want kind %q on line %d", p.name, got, p.kind, wantLine)
			}

			// GREEN after restore, and the restore is byte-identical.
			if err := os.WriteFile(full, original, 0o644); err != nil {
				t.Fatal(err)
			}
			back, err := os.ReadFile(full)
			if err != nil {
				t.Fatal(err)
			}
			if !bytes.Equal(back, original) {
				t.Fatal("restore is not byte-identical")
			}
			if got, _ := scanFleetTree(dir, []string{rel}, denied, nil); len(got) != 0 {
				t.Fatalf("restored record still fires: %v", got)
			}
		})
	}
}

// TestFleetIdentifierNicknameHostsAreAdmitted pins the 2026-09-08 widening, and its arms are what
// keep that widening narrow.
//
// The trigger was measured, not reasoned: a mailbox line whose UNC host segment was already one of
// the four prescribed nicknames fired as [network-path], and the only way to satisfy this guard was
// to scrub a line that already complied into a generic placeholder.
//
// Every arm below exists because the structural admit could otherwise buy a hole. A nickname clears
// the NETWORK arm only: it does not clear a profile segment, it does not clear the denied-token pass
// (which runs on every line whatever the structural pass admitted), and it is matched as a WHOLE
// segment, so a host that merely CONTAINS a nickname is still a hit. The admitted SET is pinned too,
// enumerated with a count, so a fifth name arriving here without the order naming it goes red.
func TestFleetIdentifierNicknameHostsAreAdmitted(t *testing.T) {
	// A synthetic denied token, for the reason the positive control gives: an arm that exercised a
	// REAL row would have to spell the identifier the denylist exists to keep out of the tree.
	const controlToken = "zzcontrolaccount"
	denied := fleetDeniedIndex([]fleetDeniedToken{{len(controlToken), fleetHash(controlToken), "control token"}})

	// Nicknames may be spelled out here: they ARE the compliant surface form, so an arm printing one
	// into a build log puts nothing there the order does not already prescribe.
	nicknames := []string{"R-LAPTOP", "G-LAPTOP", "i9", "i7"}

	// The planted lines are ASSEMBLED at run time, for the reason the positive control gives: this
	// file is itself tracked and scanned, so a UNC or a profile path written out as a literal here is
	// content the guard reads. A verb leaves `%s` where the segment goes, and `%` is a substitution
	// sigil, so the source text reads as a placeholder while the RUNTIME string carries a real path.
	const uncFmt = "the share sits at \\\\%s\\build\\artifacts\n"
	const profileFmt = "toolchain root at C:\\Users\\%s\\sdk\n"

	// scan drives the REAL walk over a temporary tree, exactly as the positive control does, rather
	// than a reimplementation of it -- and asserts the file was read, so no arm can pass by having
	// measured nothing.
	scan := func(t *testing.T, line string) []fleetFinding {
		t.Helper()
		dir := t.TempDir()
		rel := "docs/phase4/CONTROL-record.md"
		full := filepath.Join(dir, filepath.FromSlash(rel))
		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(full, []byte(line), 0o644); err != nil {
			t.Fatal(err)
		}
		got, read := scanFleetTree(dir, []string{rel}, denied, nil)
		if read != 1 {
			t.Fatalf("the arm scanned %d files, want 1 -- it measured nothing", read)
		}
		return got
	}
	hasKind := func(got []fleetFinding, kind string) bool {
		for _, f := range got {
			if f.Kind == kind {
				return true
			}
		}
		return false
	}

	// (a) ADMITTED. This is the arm the widening buys, and the one that was RED before it: on the
	// unmodified guard every spelling below fired as [network-path].
	t.Run("a nickname host is admitted", func(t *testing.T) {
		for _, nick := range nicknames {
			for _, spelling := range []string{nick, strings.ToLower(nick), strings.ToUpper(nick)} {
				if got := scan(t, fmt.Sprintf(uncFmt, spelling)); len(got) != 0 {
					t.Errorf("a UNC whose host is the prescribed nickname %q fired: %v", spelling, got)
				}
			}
		}
	})

	// (b) REFUSED, unchanged. A host that is not a nickname is still a hit, and one that is a denied
	// token is a hit twice over -- structurally, and through the pass the widening does not touch.
	t.Run("a denied host inside a UNC is still refused", func(t *testing.T) {
		got := scan(t, fmt.Sprintf(uncFmt, controlToken))
		if !hasKind(got, "network-path") {
			t.Errorf("a UNC with a non-nickname host did not fire structurally: %v", got)
		}
		if !hasKind(got, "denied-token") {
			t.Errorf("a UNC carrying a denied host did not fire the denied-token pass: %v", got)
		}
	})

	// The property the widening must not cost: the admit is structural, so a denied token sitting on
	// a line whose UNC host IS a nickname is still caught.
	t.Run("a nickname host does not clear a denied token on its line", func(t *testing.T) {
		for _, nick := range nicknames {
			line := fmt.Sprintf("owner column reads %s and ", controlToken) + fmt.Sprintf(uncFmt, nick)
			if got := scan(t, line); !hasKind(got, "denied-token") {
				t.Errorf("a denied token beside nickname host %q was not caught: %v", nick, got)
			}
		}
	})

	// Whole segment, never substring: a glyph-substring over-match is how an admit list quietly
	// becomes a hole, and a real host that merely embeds a nickname is exactly what would slip.
	t.Run("a host that merely contains a nickname is still refused", func(t *testing.T) {
		for _, nick := range nicknames {
			for _, host := range []string{"zz" + nick, nick + "zz", "zz" + nick + "zz"} {
				if got := scan(t, fmt.Sprintf(uncFmt, host)); !hasKind(got, "network-path") {
					t.Errorf("host %q, which only CONTAINS a nickname, was admitted: %v", host, got)
				}
			}
		}
	})

	// (c) A nickname in prose was never a hit, and stays one. The only pass that can reach a bare
	// token is the denied-token pass, and the arm below pins that against the REAL denylist.
	t.Run("a nickname in prose is not a hit", func(t *testing.T) {
		for _, nick := range nicknames {
			line := fmt.Sprintf("the roster row names the machine by its nickname %s\n", nick)
			if got := scan(t, line); len(got) != 0 {
				t.Errorf("nickname %q in prose fired: %v", nick, got)
			}
		}
	})

	// Driven with the package's OWN index, so it spells nothing: the two lists must not disagree
	// about one string, and an admitted nickname that were also denied would be exactly that.
	t.Run("no nickname is a denied token", func(t *testing.T) {
		live := fleetDeniedIndex(fleetDeniedTokens)
		for nick := range fleetNicknameHostSegments {
			if fleetLineHasDeniedToken([]byte(nick), live, nil) {
				t.Errorf("nickname %q is also a denied token -- the two lists disagree about one string", nick)
			}
		}
	})

	// The admitted set, enumerated with a count: a claim about a set is derived from the whole
	// construct, never from the members a reader happens to check.
	t.Run("exactly the four prescribed nicknames are admitted", func(t *testing.T) {
		want := map[string]bool{}
		for _, nick := range nicknames {
			want[strings.ToLower(nick)] = true
		}
		if len(fleetNicknameHostSegments) != len(want) {
			t.Fatalf("%d host nicknames are admitted, want %d -- the order names four machines",
				len(fleetNicknameHostSegments), len(want))
		}
		for nick := range fleetNicknameHostSegments {
			if !want[nick] {
				t.Errorf("%q is admitted as a UNC host but is not one of the prescribed nicknames", nick)
			}
		}
	})

	// The SCOPE of the widening. The profile arm is untouched, because a nickname names a host: an
	// account segment spelled with one is still an account segment.
	t.Run("a nickname is not admitted as a profile segment", func(t *testing.T) {
		for _, nick := range nicknames {
			if got := scan(t, fmt.Sprintf(profileFmt, nick)); !hasKind(got, "profile-path") {
				t.Errorf("nickname %q was admitted as a profile segment: %v", nick, got)
			}
		}
	})
}

// TestFleetIdentifierClearancesAreLive keeps the two allowlists honest. An entry whose file has gone
// (a record renamed, a corpus file relocated by a layout change) is dead weight that reads as
// diligence, and a cleared SEGMENT that no longer appears in its file is a clearance covering
// nothing -- the shape that lets a guard go quietly vacuous.
// TestSplitRefusalIsAttributableToTheToken is i9's contributed control (mailbox 5e7e71a063), and it
// closes a gap every shape probe in this fleet shared on 2026-09-08 -- four lanes, twenty-seven shapes
// between them, all measuring the same one thing: THAT THE PLANT FIRES. None measured WHY.
//
// A refusal is evidence the gate caught the TOKEN only if an identically-shaped plant carrying a
// HARMLESS token comes back CLEAN. Without that arm, a joiner that fused text too aggressively would
// refuse everything split across a line break, and every "it FIRES" any of us posted would still read
// PASS. The committed control here had clean-INLINE (the baseline record) and plant-SPLIT; it never had
// clean-SPLIT, so the failure mode that the split FIX itself introduces was the one shape untested.
//
// Two properties, and the second is the one a red exit code cannot give: the plants fire, and they fire
// THE NAMED ARM and nothing else.
// TestFleetIdentifierUnicodeEscapeHostsAreAdmitted is the PARITY arm for the census's `unc_escape`
// admit, BOTH DIRECTIONS. The refuse sibling is the one that carries the weight: the admit is bounded
// to the HEX body, so a segment of the same shape whose body is not hex must still fire, and without
// that case the admit could widen to "any host starting with that letter" with every other arm green.
//
// ⚠ NOTHING BELOW SPELLS THE SHAPE. This file is tracked and is scanned by the guard it tests, so the
// planted lines are assembled at run time through a `%s` verb exactly as the nickname arm does, and
// the escape bodies are ordinary strings until the format runs.
func TestFleetIdentifierUnicodeEscapeHostsAreAdmitted(t *testing.T) {
	const controlToken = "zzcontrolaccount"
	denied := fleetDeniedIndex([]fleetDeniedToken{{len(controlToken), fleetHash(controlToken), "control token"}})

	const uncFmt = "at go.fmt_package.printArg(\\\\%s\\build p)\n"
	const profileFmt = "toolchain root at C:\\Users\\%s\\sdk\n"

	scan := func(t *testing.T, line string) []fleetFinding {
		t.Helper()
		dir := t.TempDir()
		rel := "docs/phase4/CONTROL-record.md"
		full := filepath.Join(dir, filepath.FromSlash(rel))
		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(full, []byte(line), 0o644); err != nil {
			t.Fatal(err)
		}
		got, read := scanFleetTree(dir, []string{rel}, denied, nil)
		if read != 1 {
			t.Fatalf("the arm scanned %d files, want 1 -- it measured nothing", read)
		}
		return got
	}
	hasKind := func(got []fleetFinding, kind string) bool {
		for _, f := range got {
			if f.Kind == kind {
				return true
			}
		}
		return false
	}

	// The three glyphs the corpus actually escapes, in both letter cases the encoder emits, plus a
	// body with trailing token characters -- the shape measured in the wild where the escape is
	// followed by the rest of the name.
	escapes := []string{"u0436", "u0060", "u13D1", "u13d1", "u0022idle"}

	t.Run("an escaped glyph is not a network path", func(t *testing.T) {
		for _, esc := range escapes {
			if got := scan(t, fmt.Sprintf(uncFmt, esc)); len(got) != 0 {
				t.Errorf("a unicode escape %q fired as a path: %v", esc, got)
			}
		}
	})

	// ⚠ THE BOUND, and the reason this test is not admit-only. Same leading letter, same length, a
	// body that is NOT hex: still a structural hit.
	t.Run("a same-shaped non-hex body is still refused", func(t *testing.T) {
		for _, seg := range []string{"uzzzz", "u123", "uz0436"} {
			if got := scan(t, fmt.Sprintf(uncFmt, seg)); !hasKind(got, "network-path") {
				t.Errorf("%q was admitted although its body is not a four-digit hex escape: %v", seg, got)
			}
		}
	})

	// The denied-token pass is the stated mitigation for the cost this admit takes, so it must be
	// measurably untouched: a denied host that happens to wear the shape still fires through it.
	// ⚠ AND ITS BOUND, measured rather than assumed. fleetLineHasDeniedToken walks whole
	// identifier runs PLUS each dot/hyphen/underscore COMPONENT -- there is no substring pass -- so
	// the mitigation covers a denied host that is a component of the segment and NOT one
	// concatenated onto the escape with no separator. That bound is not new and is not this admit's:
	// it is the same bound the nickname admit's "a denied real host inside a UNC still fires" has
	// always had. Stated here rather than asserted, because a future substring pass would be an
	// improvement and must not make this arm red.
	t.Run("the denied-token pass is untouched", func(t *testing.T) {
		got := scan(t, fmt.Sprintf(uncFmt, "u0436-"+controlToken))
		if !hasKind(got, "denied-token") {
			t.Errorf("a denied token as a component of an escape-shaped host did not fire the denied-token pass: %v", got)
		}
	})

	// SCOPE. The admit is per KIND: a profile segment of that shape is not an escape of anything.
	t.Run("the shape is not admitted as a profile segment", func(t *testing.T) {
		if got := scan(t, fmt.Sprintf(profileFmt, "u0436")); !hasKind(got, "profile-path") {
			t.Errorf("an escape-shaped PROFILE segment was admitted: %v", got)
		}
	})
}

func TestSplitRefusalIsAttributableToTheToken(t *testing.T) {
	const controlToken = "zzcontrolaccount"
	denied := fleetDeniedIndex([]fleetDeniedToken{{len(controlToken), fleetHash(controlToken), "control token"}})

	// Both are token-shaped and split identically. Only one is denied.
	const harmless = "zzharmlessword"
	const seg = "zzexampleaccount"

	// Pieces for the prose arms below. ASSEMBLED rather than spelled, for the same reason the plants
	// are: this file is a tracked file the guard scans, and a profile-root word followed by a
	// separator written out here would make the guard refuse its own source. proseTail stands for an
	// ordinary directory word; what matters is that it is NOT in fleetPlaceholderSegments, or the arm
	// would pass for free. proseLong is the length case that defeats a minimum-length discriminator.
	const proseRoot = "sources live under /home"
	const proseWin = "Users"
	const proseSep = "/"
	const proseBS = "\\"
	const proseTail = "zzprosetail"
	const proseLong = "zzgeneratedfiles"

	cases := []struct {
		name     string
		content  string
		wantKind string // "" means the arm must stay CLEAN
	}{
		{"denied token split across a break", "owner reads " + controlToken[:6] + "\n" + controlToken[6:] + " here\n", "denied-token-split"},
		{"denied token split across a BLANK LINE", "owner reads " + controlToken[:6] + "\n\n" + controlToken[6:] + " here\n", "denied-token-split"},
		{"profile path split across a break", fmt.Sprintf("root at /home/\n%s/go\n", seg), "profile-path-split"},

		// ⚠ THE ARMS THAT MAKE THE ONES ABOVE MEAN SOMETHING. Same geometry, harmless content.
		{"HARMLESS token, the same split geometry", fmt.Sprintf("a note about \n%s and things\n", harmless), ""},
		{"HARMLESS token, blank-line geometry", fmt.Sprintf("a note about \n\n%s and things\n", harmless), ""},
		{"PLACEHOLDER path split (must stay cleared)", "root at /home/\nuser/go\n", ""},
		{"ordinary indented prose over three lines", "the census reads\n    every tracked file\n    and reports\n", ""},

		// ⚠ THE FALSE-POSITIVE SURFACE, ASSERTED RATHER THAN ACCEPTED IN A COMMENT. Collapsing
		// whitespace at a break also fuses the last word of one line to the first of the next, so two
		// INNOCENT words can spell a denied token together. This arm requires that to REFUSE: the gate
		// cannot distinguish it from a genuinely wrapped token, and refusing is the direction chosen --
		// a false refusal costs one rewrite, a false pass costs the fleet a scrub. It is here so that
		// nobody narrows the joiner to "fix" this and silently reopens the split hole; if this arm ever
		// goes green, the split arms above are about to stop working.
		{"ACCEPTED false positive: two innocent words fusing at a break", "the " + controlToken[:9] + "\n" + controlToken[9:] + " is a ledger column\n", "denied-token-split"},

		// And the measurement that bounds it: the SAME pair not at a break stays clean, which is what
		// collapsing only at the break buys over stripping all whitespace.
		{"the same pair NOT at a break stays clean", "the " + controlToken[:9] + " " + controlToken[9:] + " is a ledger column\n", ""},

		// ⚠ THE PROSE SHAPES THE JOINED STRUCTURAL ARM REFUSED UNTIL 2026-09-08. Each is ordinary
		// project prose: a line ending in a profile-root word, the next opening with a separator and a
		// path word. All four REFUSED with profile-path-split before the continuation requirement went
		// in; the same words INLINE were clean, which is what proved it was the JOIN and not the
		// content. Remove that requirement and these four go red -- they are the arm that keeps it.
		{"prose: posix profile word, next line opens with a separator", proseRoot + "\n" + proseSep + proseTail + " is where they land\n", ""},
		{"prose: profile word, next line opens with a separator", "shared by all " + proseWin + "\n" + proseSep + proseTail + " resolves per lane\n", ""},
		{"prose: windows profile root, next line opens with a backslash", "sits under C:" + proseBS + proseWin + "\n" + proseBS + proseTail + " on that host\n", ""},
		{"prose: profile word and a path word across a break", "a note about " + proseWin + "\n" + proseSep + proseTail + " spellings\n", ""},

		// The LENGTH case, and it is why the discriminator is continuation rather than segment length:
		// a sixteen-character fused word is ordinary vocabulary in this project, so any minimum-length
		// rule would refuse this line while this one stays clean.
		{"prose: a LONG fused word after the separator", proseRoot + "\n" + proseSep + proseLong + " land under obj\n", ""},
		{"prose: fused word at END of line", proseRoot + "\n" + proseSep + proseLong + "\n", ""},

		// The INLINE twins: same words, one line. Clean before the change too, which is what makes the
		// six above a statement about the JOIN rather than about the words.
		{"prose INLINE twin: posix", proseRoot + " " + proseSep + proseTail + " is where they land\n", ""},
		{"prose INLINE twin: profile word", "shared by all " + proseWin + " " + proseSep + proseTail + " resolves per lane\n", ""},

		// ⚠ THE OTHER DIRECTION is already asserted, by the `profile path split across a break` arm
		// above: a wrapped path that CONTINUES past the account still refuses. No arm is added for it
		// here -- a duplicate at a different wrap position fires the UNJOINED arm too (on the fragment
		// left after the separator) and this test requires every finding to be the one named kind, so
		// the honest place for multi-arm shapes is the plants control. R measured that dropping these
		// arms takes every wrap position clean on their gate (6fc8978fa8); that existing arm is what
		// keeps this gate from going the same way.
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			got := scanFleetIdentifiers("docs/phase4/CONTROL-record.md", []byte(c.content), denied, nil)

			if c.wantKind == "" {
				if len(got) != 0 {
					t.Fatalf("must stay CLEAN, fired %v -- a joiner that refuses harmless split text makes every "+
						"\"it fires\" reading in this file meaningless", got)
				}
				return
			}

			if len(got) == 0 {
				t.Fatalf("planted %s was NOT detected -- this arm cannot go red", c.name)
			}
			for _, f := range got {
				if f.Kind != c.wantKind {
					t.Fatalf("fired %q, want %q -- the refusal must be attributable to the arm claimed, "+
						"and an exit code alone cannot tell those apart", f.Kind, c.wantKind)
				}
			}
		})
	}
}

// TestFleetIdentifierEmbedPayloadsAreAdmittedAsFixtures controls the //go:embed payload admit
// (fleetEmbedPayloads). The payload line is the SHAPE Go 1.24's traceviewer ships at
// static/trace_viewer_full.html:6244 -- a JavaScript regex whose escaped character class reads as a
// UNC host -- assembled at run time so this file carries no hit of its own.
//
// RED at 47e088d3d7: the payload arm fires [network-path] and [network-path-split], exactly the two
// hits the H10 close's regen reported. Every other arm is a refusal that must SURVIVE the admit: the
// same bytes anywhere the tracked project files do not name as a converter-minted payload.
func TestFleetIdentifierEmbedPayloadsAreAdmittedAsFixtures(t *testing.T) {
	const controlToken = "zzcontrolaccount"
	denied := fleetDeniedIndex([]fleetDeniedToken{{len(controlToken), fleetHash(controlToken), "control token"}})

	bs := `\`
	regexLine := "const r=new RegExp('addr=(" + bs + bs + "[" + bs + bs + "da-fA-F" + bs + bs + "-]+" + bs + bs + "])');\n"

	const pkg = "src/core/internal/trace/traceviewer"
	item := func(include, logical string) string {
		return "<Project>\r\n  <ItemGroup Label=\"GoEmbeddedResources\">\r\n    <EmbeddedResource Include=\"" + include +
			"\" LogicalName=\"" + logical + "\" />\r\n  </ItemGroup>\r\n</Project>\r\n"
	}
	minted := item("static/viewer.html", "go.embed/internal/trace/traceviewer/static/viewer.html")

	scan := func(t *testing.T, files map[string]string) []fleetFinding {
		t.Helper()
		dir := t.TempDir()
		var rel []string
		for p, content := range files {
			full := filepath.Join(dir, filepath.FromSlash(p))
			if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
				t.Fatal(err)
			}
			if err := os.WriteFile(full, []byte(content), 0o644); err != nil {
				t.Fatal(err)
			}
			rel = append(rel, p)
		}
		got, read := scanFleetTree(dir, rel, denied, nil)
		if read != len(files) {
			t.Fatalf("the arm scanned %d of %d files -- it measured nothing", read, len(files))
		}
		return got
	}
	hitsIn := func(got []fleetFinding, p string) []fleetFinding {
		var out []fleetFinding
		for _, f := range got {
			if f.Path == p {
				out = append(out, f)
			}
		}
		return out
	}
	hasKind := func(got []fleetFinding, kind string) bool {
		for _, f := range got {
			if f.Kind == kind {
				return true
			}
		}
		return false
	}

	// The control that makes every admit below mean something: the line IS a structural hit.
	//
	// ONE hit, not two. The payload line carries one UNC-shaped segment, and before the joined pass
	// gained its suppression it read as `network-path` AND `network-path-split` -- the same segment
	// found once per pass, the double report that ruling (mailbox 1dd7d4671 §1) removed. So the
	// control now asserts the whole set: exactly one finding, of the per-line kind.
	t.Run("the regex shape is a structural hit on its own", func(t *testing.T) {
		got := scan(t, map[string]string{"docs/phase4/CONTROL-record.md": regexLine})
		if len(got) != 1 || !hasKind(got, "network-path") {
			t.Fatalf("the fixture line must read as exactly one network-path hit: %v", got)
		}
	})

	t.Run("a converter-minted payload is admitted", func(t *testing.T) {
		got := scan(t, map[string]string{pkg + "/internal.trace.traceviewer.csproj": minted, pkg + "/static/viewer.html": regexLine})
		if len(got) != 0 {
			t.Errorf("a //go:embed payload named by its tracked project file was refused: %v", got)
		}
	})

	t.Run("the same bytes at a path no item names are refused", func(t *testing.T) {
		stray := pkg + "/static/other.html"
		got := scan(t, map[string]string{pkg + "/internal.trace.traceviewer.csproj": minted, pkg + "/static/viewer.html": regexLine, stray: regexLine})
		if !hasKind(hitsIn(got, stray), "network-path") {
			t.Errorf("a NON-payload file carrying the payload's bytes was admitted: %v", got)
		}
		if len(hitsIn(got, pkg+"/static/viewer.html")) != 0 {
			t.Errorf("the named payload beside it was refused: %v", got)
		}
	})

	t.Run("an item without the converter's go.embed LogicalName admits nothing", func(t *testing.T) {
		got := scan(t, map[string]string{pkg + "/x.csproj": item("static/viewer.html", "viewer.html"), pkg + "/static/viewer.html": regexLine})
		if !hasKind(got, "network-path") {
			t.Errorf("a hand-shaped EmbeddedResource item admitted a file: %v", got)
		}
	})

	t.Run("an item reaching outside its project directory admits nothing", func(t *testing.T) {
		outside := "src/core/internal/trace/viewer.html"
		got := scan(t, map[string]string{
			pkg + "/x.csproj": item("../viewer.html", "go.embed/internal/trace/traceviewer/../viewer.html"),
			outside:           regexLine,
		})
		if !hasKind(hitsIn(got, outside), "network-path") {
			t.Errorf("an item escaping its package directory admitted a file: %v", got)
		}
	})

	t.Run("a project file outside src/core admits nothing", func(t *testing.T) {
		probe := "docs/phase4/probes/p"
		got := scan(t, map[string]string{
			probe + "/p.csproj":           item("static/viewer.html", "go.embed/p/static/viewer.html"),
			probe + "/static/viewer.html": regexLine,
		})
		if !hasKind(got, "network-path") {
			t.Errorf("a project file outside the converted tree admitted a file: %v", got)
		}
	})

	// THE SAME TEETH AS TESTDATA: only the structural pass is skipped.
	t.Run("the denied-token pass still runs over a payload", func(t *testing.T) {
		got := scan(t, map[string]string{pkg + "/internal.trace.traceviewer.csproj": minted, pkg + "/static/viewer.html": "owner " + controlToken + "\n" + regexLine})
		if !hasKind(got, "denied-token") {
			t.Errorf("a denied token inside an admitted payload did not fire: %v", got)
		}
		if hasKind(got, "network-path") {
			t.Errorf("the structural pass ran over an admitted payload: %v", got)
		}
	})
}

// TestFleetEmbedPayloadDerivationReadsTheTree keeps the derivation honest against the REAL emitter
// format: the two payloads tracked since the //go:embed shape was ruled (2026-09-22) must be derived
// from their project files. If the emitter's item format drifts, the admit derives nothing and fails
// closed; this names that as the cause instead of leaving a payload refusal to be read as a leak.
func TestFleetEmbedPayloadDerivationReadsTheTree(t *testing.T) {
	root := repoRootFromPackageDir(t)

	out, err := exec.Command("git", "-C", root, "ls-files", "-z", "src/core").Output()
	if err != nil {
		t.Fatalf("git ls-files failed in %s: %v", root, err)
	}
	var files []string
	for _, p := range strings.Split(string(out), "\x00") {
		if p != "" {
			files = append(files, p)
		}
	}

	payloads := fleetEmbedPayloads(root, files)
	for _, want := range []string{
		"src/core/embed/internal/embedtest/concurrency.txt",
		"src/core/crypto/internal/fips140test/acvp_capabilities.json",
	} {
		if !payloads[want] {
			t.Errorf("the //go:embed payload %s was not derived from its project file (derived %d) -- "+
				"has the emitter's <EmbeddedResource> format moved?", want, len(payloads))
		}
	}
}

func TestFleetIdentifierClearancesAreLive(t *testing.T) {
	root := repoRootFromPackageDir(t)
	for key, reason := range fleetClearedSegments {
		parts := strings.SplitN(key, "|", 2)
		if len(parts) != 2 || reason == "" {
			t.Fatalf("malformed clearance %q (want \"path|segment\" and a reason)", key)
		}
		content, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(parts[0])))
		if err != nil {
			t.Errorf("cleared segment names a file that is gone: %s (%v) -- retire the entry", parts[0], err)
			continue
		}
		if !bytes.Contains(bytes.ToLower(content), []byte(strings.ToLower(parts[1]))) {
			t.Errorf("cleared segment %q no longer appears in %s -- retire the entry", parts[1], parts[0])
		}
	}
	for path, reason := range fleetClearedTokenFiles {
		if reason == "" {
			t.Fatalf("clearance for %s has no reason", path)
		}
		if _, err := os.Stat(filepath.Join(root, filepath.FromSlash(path))); err != nil {
			t.Errorf("cleared file is gone: %s (%v) -- retire the entry", path, err)
		}
	}
}

// TestFleetPublicHandleAdmitIsBounded is the control for the 2026-09-22 public-handle admit, and,
// as with the nickname widening, the arms that matter are the REFUSALS. An admit-only control is
// green on a set that admits everything.
//
// ⚠ THE SET IT DRIVES IS SYNTHETIC, for the reason fleetDeniedToken's comment gives: an arm that
// exercised a REAL row would have to spell the token, which is the thing the hashes exist to keep
// out of the tree. What the synthetic set proves is that the MECHANISM is live and bounded. What it
// cannot prove is that a particular row is live -- that is the one-time red-first at cut time, and
// for the two rows added on 2026-09-22 it was measured on the shell census, where the hits are.
//
// ⚠ AND THE READING THAT MUST BE RECORDED RATHER THAN ASSUMED: under THIS guard's tokenizer the two
// real rows are INERT, and that is not a defect. fleetLineHasDeniedToken has no substring pass -- it
// matches a whole run or a dot/hyphen/underscore COMPONENT -- so the longer public handle never
// produced a denied-token hit here in the first place; the containment that costs ten hits on the
// published README is visible only to the shell census's reduced pass. The rows live here because
// this file is the AUTHORITY the shell list is generated from, and one authority is the property
// worth having. Measured at cut time: TestNoFleetIdentifiersInTrackedFiles is green on master's
// docs/README.md both before and after this change.
func TestFleetPublicHandleAdmitIsBounded(t *testing.T) {
	// Denied, and admitted: the admitted word CONTAINS the denied token as a component, which is
	// exactly the shape the ruling is about -- the account name inside the public handle.
	const controlToken = "zzcontrolaccount"
	const handle = "zzcontrolaccount-public"
	denied := fleetDeniedIndex([]fleetDeniedToken{{len(controlToken), fleetHash(controlToken), "control token"}})
	admit := fleetAdmitIndex([]fleetPublicHandle{{len(handle), fleetHash(handle), "control handle"}})

	// ASSEMBLED, never spelled: this file is tracked and is scanned by the guard it tests, so a
	// profile path written out as a literal here is content the guard reads. `%` is a substitution
	// sigil, so the source text reads as a placeholder while the runtime string carries a real path.
	const profileFmt = "toolchain root at C:\\Users\\%s\\sdk\n"
	const homeFmt = "sources under /home/%s/go\n"

	scan := func(content string) []fleetFinding {
		return scanFleetIdentifiers("docs/phase4/CONTROL-record.md", []byte(content), denied, admit)
	}
	hasKind := func(got []fleetFinding, kind string) bool {
		for _, f := range got {
			if f.Kind == kind {
				return true
			}
		}
		return false
	}

	// (a) THE ADMIT. The one thing the widening buys: the denied token is a component of an admitted
	// word, and the line is clean. Remove the admit consult in fleetLineHasDeniedToken and this is
	// the arm that goes red.
	t.Run("a denied token inside an admitted handle is admitted", func(t *testing.T) {
		if got := scan("see the " + handle + " page for the published packages\n"); len(got) != 0 {
			t.Errorf("an admitted public handle fired: %v", got)
		}
	})

	// (b) THE BARE TOKEN, refused, and refused WITHOUT the admit set being consulted at all -- the
	// whole run is tested first and returns. This is the arm that keeps the admit from ever becoming
	// a way to clear the account name itself.
	t.Run("a bare denied token in prose is still refused", func(t *testing.T) {
		if got := scan("owner column reads " + controlToken + " here\n"); !hasKind(got, "denied-token") {
			t.Errorf("a bare denied token was not refused: %v", got)
		}
	})

	// (c) CONTAINMENT IS NOT ADMISSION. The lookup is an exact hash of the whole enclosing run, so a
	// word that merely contains an admitted handle -- or merely contains the denied token with a
	// separator -- is still a hit. This is how an admit list quietly becomes a hole, and it is the
	// same bound the nickname admit carries.
	t.Run("a word that merely contains an admitted handle is refused", func(t *testing.T) {
		for _, w := range []string{handle + "xyz", controlToken + "-notpublic", "x_" + controlToken + "_public"} {
			if got := scan("owner column reads " + w + " here\n"); !hasKind(got, "denied-token") {
				t.Errorf("%q was admitted although it is not the admitted handle: %v", w, got)
			}
		}
	})

	// ⚠ THE BOUND, MEASURED RATHER THAN ASSUMED, and it is NOT this widening's. A denied token glued
	// into a longer run with NO separator was never reachable by this guard -- fleetLineHasDeniedToken
	// walks whole runs plus dot/hyphen/underscore components and has no substring pass -- so a word
	// like "zz" + the handle reads clean here whatever the admit set says. Written first as a REFUSE
	// arm and measured red, which is how the bound came to be stated rather than assumed. The arm
	// drives it BOTH ways and requires the readings to AGREE: attributable to the tokenizer, not to
	// the admit. Same bound the nickname and unicode-escape admits carry, and stated rather than
	// asserted as an admit property so that a future substring pass is an improvement, not a red arm.
	t.Run("a no-separator prefix is invisible to the tokenizer, admit or not", func(t *testing.T) {
		line := "owner column reads zz" + handle + " here\n"
		withAdmit := scan(line)
		withoutAdmit := scanFleetIdentifiers("docs/phase4/CONTROL-record.md", []byte(line), denied, nil)
		if len(withAdmit) != 0 || len(withoutAdmit) != 0 {
			t.Fatalf("expected both readings clean (the tokenizer's bound): with=%v without=%v",
				withAdmit, withoutAdmit)
		}
	})

	// (d) SCOPE: THE STRUCTURAL PASSES ARE UNTOUCHED. The admit lives in the denied-token walker,
	// which the profile and home arms never reach, so a profile path spelled with the PUBLIC HANDLE
	// is still an account segment and still a hit. The ruling says this in as many words.
	t.Run("a profile path carrying the admitted handle is still refused", func(t *testing.T) {
		for _, seg := range []string{handle, controlToken} {
			if got := scan(fmt.Sprintf(profileFmt, seg)); !hasKind(got, "profile-path") {
				t.Errorf("a profile path with segment %q was admitted: %v", seg, got)
			}
			if got := scan(fmt.Sprintf(homeFmt, seg)); !hasKind(got, "profile-path") {
				t.Errorf("a home path with segment %q was admitted: %v", seg, got)
			}
		}
	})

	// (e) The admit reaches the JOINED surface too, because the joined pass calls the same walker.
	// Without this the gate would refuse a handle that a line break happens to fall inside, which is
	// the false-refusal direction the split arms above were added to avoid creating.
	t.Run("an admitted handle split across a line break is admitted", func(t *testing.T) {
		if got := scan("see the " + handle[:8] + "\n" + handle[8:] + " page\n"); len(got) != 0 {
			t.Errorf("an admitted handle wrapped at a line break fired: %v", got)
		}
	})

	// (f) A nil admit set is the pre-ruling behaviour EXACTLY -- the property every other arm in this
	// file depends on, since they all pass nil.
	t.Run("a nil admit set refuses what it always refused", func(t *testing.T) {
		got := scanFleetIdentifiers("docs/phase4/CONTROL-record.md",
			[]byte("see the "+handle+" page\n"), denied, nil)
		if !hasKind(got, "denied-token") {
			t.Errorf("with no admit set the handle must refuse exactly as before: %v", got)
		}
	})

	// THE TWO LIVE LISTS MUST NOT DISAGREE ABOUT ONE STRING. Driven with the package's own indexes,
	// so it spells nothing: an admitted handle that were also a denied token would be a row that
	// cannot be reached (the whole-run denial returns first) and a contradiction in the ruling.
	t.Run("no admitted handle is a denied token", func(t *testing.T) {
		liveDenied := fleetDeniedIndex(fleetDeniedTokens)
		for _, h := range fleetPublicHandles {
			if byHash, ok := liveDenied[h.Len]; ok {
				if _, clash := byHash[h.Hash]; clash {
					t.Errorf("an admitted handle (%s) is also a denied token -- the two lists disagree", h.What)
				}
			}
		}
	})

	// The admitted SET, enumerated with a count and with reasons: a claim about a set is derived from
	// the whole construct, never from the members a reader happens to check. A third row arriving
	// here without a ruling naming it goes red.
	t.Run("exactly the two ruled handles are admitted", func(t *testing.T) {
		if len(fleetPublicHandles) != 2 {
			t.Fatalf("%d public handles are admitted, want 2 -- the 2026-09-22 ruling names two",
				len(fleetPublicHandles))
		}
		seen := map[string]bool{}
		for _, h := range fleetPublicHandles {
			if h.What == "" {
				t.Errorf("an admitted handle of length %d carries no reason", h.Len)
			}
			if h.Len < 5 {
				t.Errorf("an admitted handle of length %d is too short to be a handle", h.Len)
			}
			if len(h.Hash) != 64 {
				t.Errorf("an admitted handle's hash is %d characters, want 64", len(h.Hash))
			}
			if seen[h.Hash] {
				t.Errorf("an admitted handle is listed twice")
			}
			seen[h.Hash] = true
		}
	})
}

// TestFleetIdentifierHashFileMatchesTheGoLists makes the shell census's hash list's own header
// claim -- "GENERATED FROM ... fleetDeniedTokens" -- something the tree ENFORCES rather than
// something a commit message asserts.
//
// The two instruments cannot disagree about WHICH names are denied or WHICH handles are admitted,
// and until this arm existed the only thing stopping them was whoever remembered to edit both files
// in one commit. That is exactly the silent-drift shape this whole guard was built for: the shell
// census would go on measuring an older order, every test would stay green, and the gap would be
// found the way the 2026-09-04 reintroduction was -- by someone happening to look.
//
// It compares HASHES, so it spells nothing, and it is an EQUALITY in both directions: a row in
// either file with no partner in the other is a failure that names the side it is missing from.
func TestFleetIdentifierHashFileMatchesTheGoLists(t *testing.T) {
	root := repoRootFromPackageDir(t)
	rel := filepath.Join(".claude", "coord-scripts", "coord-identifier-hashes.txt")
	content, err := os.ReadFile(filepath.Join(root, rel))
	if err != nil {
		t.Fatalf("the shared hash list is unreadable at %s: %v -- "+
			"an instrument that cannot read the other instrument's list must not report agreement", rel, err)
	}

	fileDenied := map[string]bool{}
	fileAdmit := map[string]bool{}
	for n, raw := range strings.Split(string(content), "\n") {
		line := strings.TrimRight(raw, "\r")
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}
		f := strings.Split(line, "\t")
		switch {
		case f[0] == "ADMIT":
			if len(f) < 4 {
				t.Fatalf("%s:%d: an ADMIT row needs ADMIT<TAB>LEN<TAB>HASH<TAB>WHAT", rel, n+1)
			}
			fileAdmit[f[1]+"\t"+f[2]] = true
		case len(f) >= 3:
			fileDenied[f[0]+"\t"+f[1]] = true
		default:
			t.Fatalf("%s:%d: a row that is neither a denied row nor an ADMIT row", rel, n+1)
		}
	}

	goDenied := map[string]bool{}
	for _, d := range fleetDeniedTokens {
		goDenied[fmt.Sprintf("%d\t%s", d.Len, d.Hash)] = true
	}
	goAdmit := map[string]bool{}
	for _, h := range fleetPublicHandles {
		goAdmit[fmt.Sprintf("%d\t%s", h.Len, h.Hash)] = true
	}

	cmp := func(kind string, inGo, inFile map[string]bool) {
		for k := range inGo {
			if !inFile[k] {
				t.Errorf("a %s row is in the Go list but NOT in %s -- the shell census is measuring "+
					"an older order (len %s)", kind, rel, strings.SplitN(k, "\t", 2)[0])
			}
		}
		for k := range inFile {
			if !inGo[k] {
				t.Errorf("a %s row is in %s but NOT in the Go list, which is the authority -- "+
					"add it there or retire it here (len %s)", kind, rel, strings.SplitN(k, "\t", 2)[0])
			}
		}
		if len(inGo) != len(inFile) {
			t.Errorf("%s rows: %d in the Go list, %d in %s", kind, len(inGo), len(inFile), rel)
		}
	}
	cmp("denied", goDenied, fileDenied)
	cmp("admitted-handle", goAdmit, fileAdmit)
}
