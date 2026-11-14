// adapterNameCollisions.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/hex"
	"os"
	"strings"
)

// A pointer-interface adapter class is named `[<pkg>_]<structSimple>ж<ifaceSimple>` — the STRUCT
// side is package-qualified when foreign (two same-named foreign structs adapting to one interface
// would otherwise compose one class), but the INTERFACE side composes from its bare last-dot
// segment. That is only unambiguous while a given struct adapts to at most one interface of each
// simple name. compress/flate breaks it: its own `Reader` interface and `io.Reader` share a simple
// name, and its tests hand a *bufio.Reader and a *bytes.Reader to NewReader, which casts to both —
// so `bufio_ReaderжReader` and `bytes_ReaderжReader` were each composed TWICE (CS0102 + CS0111 ×6
// + CS8646 per pair). The whole 302-package production corpus has ZERO such collisions; it takes a
// test closure's extra casts to produce one, which is why this survived the Phase-3 milestone.
//
// The rule is COLLISION-CONDITIONAL: within a group of records that compose the same adapter name,
// every FOREIGN interface takes a package qualifier (`bufio_Readerжio_Reader`) and a LOCAL one stays
// bare. Unconditional qualification was measured and rejected — 3,688 construction sites across 644
// adapter names would churn. Because collisions do not occur in production, this is byte-neutral
// there by construction.
//
// The converter and go2cs-gen must agree on every name, and neither may guess: the authority is the
// FINAL set of `[assembly: GoImplement<…>(Pointer = true)]` lines, which is exactly what the
// generator reads and what writePackageInfoFile emits after its alias-covered skip and its
// interface-inheritance prune. Those lines are not known until the whole package has been visited,
// long after the cast sites were rendered — so a cast emits a deferred marker (mirroring
// dynamicTypeOperations' DYNTYPE marker) that resolveAdapterNameMarkers rewrites once the records
// are final.
const (
	adapterNameMarkerPrefix = "«ADAPTER:"
	adapterNameMarkerSuffix = ":ADAPTER»"
	adapterNameMarkerSep    = "|"
)

// THE PASS SEPARATION. A production adapter name is ALREADY COMPILED TEXT. Under the recompile
// model the production `.cs` files become COMPILE ITEMS of the test assembly, where go2cs-gen reads
// the UNION of both halves' records — but that production text was rendered in the production pass,
// against the PRODUCTION record set alone, and nothing can go back and re-render it. So a test-half
// record that lands in a production record's collision group must not be allowed to rename the
// production member: the generator would mint `SHA3жhash_Hash` where sha3.cs spells `SHA3жHash`
// (CS0246/CS0426 ×4, crypto/sha3 — the row's second blocker, the first being the split key
// adapterStructQualifierIsLocal fixes).
//
// The two halves are told apart by a FACET on the record itself. writePackageInfoFile renders every
// record the same way, and the recompile seed is a VERBATIM byte copy of the production
// package_info.cs (convertTestVariants), so the one place the halves are distinguishable is that
// copy: facetProductionPointerRecords stamps `Production = true` onto the seeded records as they
// cross into the test metadata file. Records the test pass then merges in carry no facet, and the
// union still tells the halves apart.
//
// THE RULE (COORD 2026-09-21 23:33), applied here and by the generator's ImplementGenerator:
//
//  1. Collisions are still computed over the UNION — the facet decides naming, never grouping.
//  2. In a colliding group with EXACTLY ONE faceted member, that member keeps the unprefixed name
//     the production text already spells; only the unfaceted members take the interface prefix.
//  3. TWO OR MORE faceted members are a PRODUCTION-pass collision: the production pass already
//     resolved it and the production text already spells the prefixed names, so the ordinary rule
//     applies to the whole group and reproduces exactly those names. Both stand.
//  4. A group with NO faceted member is the ordinary rule — which is every production conversion
//     and every reference model, so this is byte-neutral outside a recompile-model test project.
//
// Only the recompile model populates any of this: both reference models compile the production half
// as a separate ASSEMBLY and seed a test-class-only anchor, so no faceted record exists there, every
// group falls to case 4, and the corpus cannot move.
const (
	pointerRecordSuffix           = ">(Pointer = true)]"
	pointerProductionRecordSuffix = ">(Pointer = true, Production = true)]"

	// pointerClosedRecordSuffix ends a pointer record of a CLOSED CARRIER pair (closedCarrier.go). Its adapter takes
	// the same `<struct>ж<iface>` name an ordinary pointer adapter does, so it joins the same collision groups.
	pointerClosedRecordSuffix = ">(Pointer = true, " + closedCarrierRecordArgument + ")]"
)

// emittedPointerAdapterPairs holds the (structRef, interfaceRef) pairs of the pointer-form
// GoImplement records writePackageInfoFile actually emitted, in the spelling it wrote them. Reset
// per package-info write; consumed by resolveAdapterNameMarkers. Guarded by packageLock like the
// registries it is derived from.
var emittedPointerAdapterPairs [][2]string

// productionFacetedAdapterPairs holds, for the same capture, which of those pairs carried the
// PRODUCTION facet — keyed by the pair's spelling exactly as recorded, so it cannot drift from
// emittedPointerAdapterPairs: one parse populates both. A parallel set rather than a third element
// on the pair because [2]string is the shape seven functions and their arms already take, and
// widening it would touch every one of them for a fact only two naming sites read.
//
// A given pair SPELLING has one facet state by construction: the writer's HashSet dedupes identical
// record text, and the one way to hold both spellings of one pair — the seeded faceted record plus
// a freshly-rendered unfaceted twin — is collapsed at the merge (see packageInfoWriter.go), because
// two records for one pair make the generator compose the adapter twice whatever the facet says.
var productionFacetedAdapterPairs map[string]bool

// adapterPairFacetKey keys productionFacetedAdapterPairs. The RECORD spelling, not a normalized
// key: two members of one collision group share every normalized key there is (that is what makes
// them a group), so the facet has to be remembered against the text the record was written in.
func adapterPairFacetKey(structRef, interfaceRef string) string {
	return structRef + adapterNameMarkerSep + interfaceRef
}

// adapterPairIsProductionFaceted reports whether a recorded pair came from the production half.
func adapterPairIsProductionFaceted(pair [2]string) bool {
	packageLock.Lock()
	defer packageLock.Unlock()

	return productionFacetedAdapterPairs[adapterPairFacetKey(pair[0], pair[1])]
}

// facetProductionPointerRecords stamps the production facet onto every pointer-form GoImplement
// record as a production package_info.cs is seeded into a recompile-model package_test_info.cs.
//
// A whole-content replacement of an exact token, deliberately: the token contains no newline, so
// every line ending in the file is carried through untouched — the seed is the one file in the
// `-tests` flow that is copied byte-for-byte rather than rewritten, and a line-ending normalization
// slipped in here would be invisible until a CRLF golden moved. It is also idempotent by
// construction: an already-faceted record does not match the unfaceted token.
//
// Scoped to the RECORD text and nothing else — `(Pointer = true)]` appears in no comment any
// package_info writer emits (verified over the corpus's emitted files; the template carries no
// `Pointer` token at all), and the value-form and `Promoted`/`ConstraintProxy` records do not
// contain it.
func facetProductionPointerRecords(content []byte) []byte {
	return []byte(strings.ReplaceAll(string(content), pointerRecordSuffix, pointerProductionRecordSuffix))
}

// pointerRecordFacetedForm returns the FACETED spelling of a freshly-rendered pointer record — the
// text a recompile seed would already hold for the same pair. The merge in writePackageInfoFile
// asks for it by this one function rather than composing the suffix swap inline, so the two
// spellings of one pair are related in exactly one place and an arm can hold that relation.
// A record that is not a pointer record, or is already faceted, comes back unchanged.
func pointerRecordFacetedForm(record string) string {
	if !strings.HasSuffix(record, pointerRecordSuffix) {
		return record
	}

	return strings.TrimSuffix(record, pointerRecordSuffix) + pointerProductionRecordSuffix
}

// testAdapterResolveNames accumulates every -tests emission PATH across both variants, so the
// deferred adapter names can be resolved in one pass once the merged metadata file is final.
var testAdapterResolveNames []string

// recordEmittedPointerAdapterPairs captures the pointer-form GoImplement pairs from the FINAL
// package-info lines — the exact text go2cs-gen will read. Replaces (not appends to) the previous
// set: writePackageInfoFile is called once per emitted info file (production, then each -tests
// variant), and each variant's cast sites resolve against its OWN records.
func recordEmittedPointerAdapterPairs(lines []string) {
	pairs := [][2]string{}
	faceted := map[string]bool{}

	for _, line := range lines {
		inner, ok := strings.CutPrefix(strings.TrimSpace(line), "[assembly: GoImplement<")

		if !ok {
			continue
		}

		// The faceted suffix is tested FIRST and the two are mutually exclusive, so the ordinary
		// suffix cannot claim a faceted record. Getting this wrong is silent in the worst way: the
		// seeded production records simply fall out of the pair set, no collision is seen at all,
		// and every test-half cast takes the bare name the production half owns.
		isProduction := strings.HasSuffix(inner, pointerProductionRecordSuffix)

		switch {
		case isProduction:
			inner = strings.TrimSuffix(inner, pointerProductionRecordSuffix)
		case strings.HasSuffix(inner, pointerRecordSuffix):
			inner = strings.TrimSuffix(inner, pointerRecordSuffix)
		case strings.HasSuffix(inner, pointerClosedRecordSuffix):
			inner = strings.TrimSuffix(inner, pointerClosedRecordSuffix)
		default:
			continue
		}

		// Split on the record's separating comma. A GENERIC struct reference carries its own
		// commas inside `<…>` (`nistCurve<ж<P224Point>>`), so track angle-bracket depth rather
		// than taking the first comma.
		depth := 0
		split := -1

		for i, r := range inner {
			switch r {
			case '<':
				depth++
			case '>':
				depth--
			case ',':
				if depth == 0 && split < 0 {
					split = i
				}
			}
		}

		if split < 0 {
			continue
		}

		structRef, interfaceRef := strings.TrimSpace(inner[:split]), strings.TrimSpace(inner[split+1:])
		pairs = append(pairs, [2]string{structRef, interfaceRef})

		if isProduction {
			faceted[adapterPairFacetKey(structRef, interfaceRef)] = true
		}
	}

	packageLock.Lock()
	emittedPointerAdapterPairs = pairs
	productionFacetedAdapterPairs = faceted
	packageLock.Unlock()
}

// emittedAdapterPairAnchors maps a pair's collision-group key to the metadata anchor CLASS the
// pair's record file is anchored to — the class go2cs-gen generates that pair's adapter into.
// Populated per capture under the reference test models, where a test compilation can carry TWO
// anchor files (the test class and the white-box bridge); empty everywhere else.
var emittedAdapterPairAnchors map[string]string

// captureAdapterPairsFromInfoFile re-reads a written package-info file and captures its pointer
// records as the authoritative pair set. Used by the -tests flow, whose variants reach the metadata
// file through more than one writer. A non-empty anchorClass additionally records, per pair, the
// class the file anchors generated adapters to; successive captures ACCUMULATE pairs so a
// two-anchor test layout captures both files.
func captureAdapterPairsFromInfoFile(packageInfoFileName string, anchorClass ...string) {
	contentBytes, err := os.ReadFile(packageInfoFileName)

	if err != nil {
		showWarning("Failed to read \"%s\" for adapter-name resolution: %s", packageInfoFileName, err)
		return
	}

	previous := emittedPointerAdapterPairs
	previousFaceted := productionFacetedAdapterPairs
	recordEmittedPointerAdapterPairs(strings.Split(string(contentBytes), "\n"))

	if len(anchorClass) > 0 && anchorClass[0] != "" {
		if emittedAdapterPairAnchors == nil {
			emittedAdapterPairAnchors = map[string]string{}
		}
		packageLock.Lock()
		for _, pair := range emittedPointerAdapterPairs {
			emittedAdapterPairAnchors[adapterGroupKey(pair[0], pair[1])] = anchorClass[0]
		}
		packageLock.Unlock()
	}

	// recordEmittedPointerAdapterPairs REPLACES the pair set with this file's records; a
	// two-anchor capture needs the union, so fold the earlier capture back in.
	if len(previous) > 0 {
		packageLock.Lock()
		merged := make([][2]string, 0, len(previous)+len(emittedPointerAdapterPairs))
		seen := map[string]bool{}
		for _, pair := range append(append([][2]string{}, emittedPointerAdapterPairs...), previous...) {
			key := pair[0] + "|" + pair[1]
			if !seen[key] {
				seen[key] = true
				merged = append(merged, pair)
			}
		}
		emittedPointerAdapterPairs = merged

		// The facet set is folded back with the pairs it describes, or the earlier capture's
		// records would be in the union naming-blind — present as pairs, unfaceted as facts.
		for key, isProduction := range previousFaceted {
			if isProduction {
				productionFacetedAdapterPairs[key] = true
			}
		}
		packageLock.Unlock()
	}
}

// adapterNameMarker returns the deferred marker for a pointer-adapter reference. The payload is
// HEX-ENCODED for the same reason the DYNTYPE payload is: a rendered type name flows through string
// transformation passes (convertToCSTypeName, getAliasedTypeName, …) before reaching the output
// file, and raw type text would be corrupted in transit. Equal pairs yield an identical marker, so
// string comparisons on rendered references behave exactly as pair comparisons.
func adapterNameMarker(structBase string, interfaceTypeName string) string {
	payload := hex.EncodeToString([]byte(structBase + adapterNameMarkerSep + interfaceTypeName))
	return adapterNameMarkerPrefix + payload + adapterNameMarkerSuffix
}

// adapterNameMarkerPair decodes a marker payload back to its (structBase, interfaceTypeName) pair.
func adapterNameMarkerPair(payload string) (string, string, bool) {
	decoded, err := hex.DecodeString(payload)

	if err != nil {
		return "", "", false
	}

	structBase, interfaceTypeName, ok := strings.Cut(string(decoded), adapterNameMarkerSep)

	if !ok {
		return "", "", false
	}

	return structBase, interfaceTypeName, true
}

// adapterInterfaceSimpleName reduces an interface reference to the bare last-dot segment the
// adapter name composes from — the same reduction adapterTypeRef applied inline before markers,
// and the same one the generator's GetSimpleName performs.
func adapterInterfaceSimpleName(interfaceTypeName string) string {
	simple := interfaceTypeName

	if idx := strings.LastIndex(simple, "."); idx >= 0 {
		simple = simple[idx+1:]
	}

	return stripSanitizationMarkers(simple)
}

// stripAdapterInterfaceTypeArgs drops a CLOSED generic argument list from an interface REFERENCE,
// BEFORE any last-dot scan runs over it. It is the interface-side twin of splitAdapterStructReference's
// first step and exists for the same reason: the last-dot scan otherwise runs INSIDE the argument
// list and yields the ARGUMENT's tail segment — `keyedLike<go.p.named>` reducing to `named>`, and a
// nested `outer<inner<x>>` to `x>>` — which is not a name any class can carry.
//
// ⚠ APPLIED AT THE NAME-COMPOSING SITES ONLY, never inside adapterInterfaceSimpleName. That helper
// also serves the collision KEYS, and those garble a generic interface reference ON PURPOSE: the
// generator's keys garble identically, so the two halves agree, and stripping one side alone would
// manufacture the divergence the struct-side rule exists to prevent (COORD 8b1a284122 / 274b71f5a).
// A parity arm asserts the garbled value so a later reader cannot tidy the asymmetry away. ⚠ That
// parity is a property of the CALL SITE, not of this function: it holds where the key is taken from
// the record spelling (anchoredAdapterMemberName) and not where the name arrives pre-stripped from
// the marker (adapterResolvedName, whose own comment says so).
//
// ⚠ THE INPUT CONTRACT IS AN INTERFACE REFERENCE, NEVER A BOX FORM, and that is caller discipline
// rather than construction (C2, mailbox 767c73dd1 §4). Handed `ж<T>` this returns the bare marker
// glyph: the guard excludes only a name STARTING with '<', so a box form satisfies it. Every call
// site today passes an interface reference — a GoImplement's interface side is never a box — but
// the next caller is the one that finds out, and "correct through today's callers" is exactly the
// coupling adapterResolvedName strips its own input to avoid.
func stripAdapterInterfaceTypeArgs(interfaceTypeName string) string {
	if idx := strings.Index(interfaceTypeName, "<"); idx > 0 && strings.HasSuffix(interfaceTypeName, ">") {
		return interfaceTypeName[:idx]
	}

	return interfaceTypeName
}

// adapterInterfacePackagePrefix returns the disambiguating prefix ("io_") for a FOREIGN interface
// reference, derived from the package class segment that precedes the type ("io_package.Reader").
// Returns "" for a LOCAL interface (a bare, undotted name), which never takes a qualifier: at most
// one member of a colliding group can be local, so leaving it bare is always unambiguous and keeps
// the Go-like short form for the package's own interface. Mirrors the generator's
// ForeignPackagePrefix on the struct side, and the converter's own `getSanitizedIdentifier(pkg) +
// "_"` composition at foreign-struct cast sites.
func adapterInterfacePackagePrefix(interfaceTypeName string) string {
	idx := strings.LastIndex(interfaceTypeName, ".")

	if idx < 0 {
		return ""
	}

	return packageClassPrefix(interfaceTypeName[:idx])
}

// packageClassPrefix reduces a type reference's qualifier to the generator's flattened package
// prefix: "go.compress.flate_package" → "flate_". A qualifier that is not a package class (an
// enclosing type, say) yields "" — no qualification is possible or wanted there.
func packageClassPrefix(qualifier string) string {
	if dot := strings.LastIndex(qualifier, "."); dot >= 0 {
		qualifier = qualifier[dot+1:]
	}

	if !strings.HasSuffix(qualifier, PackageSuffix) {
		return ""
	}

	return strings.TrimSuffix(qualifier, PackageSuffix) + "_"
}

// adapterNameCollisionSet returns the composed adapter names that MORE THAN ONE distinct interface
// maps to, computed over the final emitted pointer records. Grouping keys on the composed name —
// struct side included — because two records sharing an interface simple name do NOT collide when
// their struct sides differ: compress/gzip records both `<Reader, io.Reader>` and
// `<bufio.Reader, flate.Reader>`, which compose `ReaderжReader` and `bufio_ReaderжReader`. Keying on
// the interface simple name alone would call that a collision and rename a validated package's
// adapters for nothing.
func adapterNameCollisionSet(pairs [][2]string) map[string]bool {
	groups := map[string]map[string]bool{}

	for _, pair := range pairs {
		key := adapterGroupKey(pair[0], pair[1])

		if groups[key] == nil {
			groups[key] = map[string]bool{}
		}

		groups[key][pair[1]] = true
	}

	colliding := map[string]bool{}

	for name, interfaces := range groups {
		if len(interfaces) > 1 {
			colliding[name] = true
		}
	}

	return colliding
}

// adapterProductionNameKeepers returns the collision-group keys in which a PRODUCTION-faceted
// record KEEPS the unprefixed name — rule 2 of the pass separation above: the group collides and
// exactly ONE of its members carries the facet, so that member's already-compiled production text
// is what the union must reproduce.
//
// Two or more faceted members (rule 3) deliberately yield NO keeper: that group collided in the
// production pass too, the production text therefore already spells the PREFIXED names, and the
// ordinary rule is what reproduces them. Exempting one of them would rename a production site in
// the opposite direction — the same defect, mirrored.
//
// ⚠ THE DEGENERATE CASE IS WARNED, NOT SILENTLY MIS-NAMED. Rule 2 tells the unfaceted members to
// "take the interface prefix", and a LOCAL interface's prefix is the empty string — so a group
// whose faceted member keeps the bare name and whose unfaceted member is local would compose ONE
// name twice, which the generator reports as CS0102 at a site that reads like a converter bug.
// No corpus package reaches it: every member of every recompile-model collision group today has a
// foreign interface (crypto/sha3's hash.Hash and fips140.Hash; crypto/ecdh, the in-set negative
// control, has no collision at all). The shape is stated and named rather than machinery built for
// it, so the package that reaches it first arrives with its own name in the warning instead of an
// unexplained duplicate class.
func adapterProductionNameKeepers(pairs [][2]string, colliding map[string]bool) map[string]bool {
	facetedCounts := map[string]int{}
	groupMembers := map[string][][2]string{}

	for _, pair := range pairs {
		key := adapterGroupKey(pair[0], pair[1])

		if !colliding[key] {
			continue
		}

		groupMembers[key] = append(groupMembers[key], pair)

		if adapterPairIsProductionFaceted(pair) {
			facetedCounts[key]++
		}
	}

	keepers := map[string]bool{}

	for key, count := range facetedCounts {
		if count != 1 {
			continue
		}

		keepers[key] = true

		for _, pair := range groupMembers[key] {
			if adapterPairIsProductionFaceted(pair) {
				continue
			}

			if adapterInterfacePackagePrefix(stripAdapterInterfaceTypeArgs(pair[1])) == "" {
				showWarning("Adapter name \"%s\" cannot be separated by pass: the production record keeps the unprefixed name and the test-half record for interface \"%s\" is LOCAL, so it has no prefix to take", key, pair[1])
			}
		}
	}

	return keepers
}

// adapterGroupKey is the collision-grouping key for a pair. ONLY a key — never emitted. The same
// struct reaches this code in three spellings that must all group together: a GoImplement record's
// package-class form ("bufio_package.Reader"), a cast site's flattened foreign form
// ("bufio_Reader"), and a cast site's NAMESPACE-qualified form ("os.File", naming an adapter class
// that lives in another assembly). All normalize to the generator's "<pkg>_<Simple>", which is what
// AdapterStructKey computes from the symbol on the other side.
func adapterGroupKey(structBase string, interfaceTypeName string) string {
	return adapterStructKey(structBase) + PointerPrefix + adapterInterfaceSimpleName(interfaceTypeName)
}

// splitAdapterStructReference breaks a struct spelling into its immediate qualifier and its simple
// name — the one piece of parsing every adapter-naming decision starts from.
//
//	"go.io_test_package.Buffer"  -> ("io_test_package", "Buffer")
//	"bytes_package.Reader<int>"  -> ("bytes_package",   "Reader")
//	"Buffer"                     -> ("",                "Buffer")
//
// Two details are load-bearing. A generic suffix is dropped first, because `Reader<T>` names the
// same adapter struct as `Reader` and the '<' would otherwise swallow the rest of the scan. And
// only the LAST qualifier segment is kept: a fully-qualified spelling like "go.io_test_package.X"
// carries the namespace too, but the generator compares against a package CLASS name, so
// "io_test_package" is the part that can match.
//
// The qualifier is returned as spelled — callers decide what it means. adapterStructKey strips the
// "_package" suffix to build the generator's "<pkg>_<Simple>" key; adapterStructQualifierClass
// instead REQUIRES that suffix and returns the class name whole.
func splitAdapterStructReference(structBase string) (qualifier, simpleName string) {
	base := structBase

	if idx := strings.Index(base, "<"); idx >= 0 {
		base = base[:idx]
	}

	idx := strings.LastIndex(base, ".")

	if idx < 0 {
		return "", base
	}

	qualifier = base[:idx]

	if dot := strings.LastIndex(qualifier, "."); dot >= 0 {
		qualifier = qualifier[dot+1:]
	}

	return qualifier, base[idx+1:]
}

// adapterStructKey normalizes any of those struct spellings to "<pkg>_<Simple>".
func adapterStructKey(structBase string) string {
	qualifier, simpleName := splitAdapterStructReference(structBase)

	// A bare spelling has no package to prefix with — it is already the key.
	if qualifier == "" {
		return stripSanitizationMarkers(simpleName)
	}

	// A qualifier naming the LOCAL package class is not foreign, however it is spelled.
	if adapterStructQualifierIsLocal(qualifier) {
		return stripSanitizationMarkers(simpleName)
	}

	return strings.TrimSuffix(qualifier, PackageSuffix) + "_" + stripSanitizationMarkers(simpleName)
}

// adapterStructQualifierIsLocal reports whether a struct spelling's QUALIFIER names the package
// class whose members compile into THIS assembly. It is the converter's half of the generator's
// own locality test — AdapterStructKey's `container == packageClassName`, which decides bare
// versus `<pkg>_<Simple>` on the other side of the SAME key — and the generator's doc states the
// contract the two share: "the two must agree or the collision groups diverge".
//
// They diverged for one spelling. A RECORD names the local class outright (`sha3_package.SHA3`,
// or bare after stripLocalTypeQualifier), but a CAST SITE in an external `<pkg>_test` variant
// reaches the package under test through the USING ALIAS the converter itself minted for it
// (`using sha3 = go.crypto.sha3_package;` — visitImportSpec's isPackageUnderTest arm), so its
// qualifier is the Go package NAME, carrying no PackageSuffix for the trim below to find. The key
// therefore composed the FOREIGN form `sha3_SHA3` while the generator, resolving the symbol,
// composed bare `SHA3`: the group SPLIT, `colliding` was keyed on a name no cast site asked about,
// and every reference took the unprefixed name for a class the generator never emits. crypto/sha3
// is the corpus instance — SHA3 reaches hash.Hash and fips140.Hash, so the generator prefixes
// BOTH members and no `SHA3жHash` exists at all (CS0246/CS0426).
//
// Both spellings are tested, against the SAME testLocalTypePrefixes the record side strips with
// (stripLocalTypeQualifier), so the two halves cannot drift apart again. That set is populated
// ONLY under the recompile model — both reference models clear testPackageName precisely so the
// production package binds as an ordinary import (see the model branch in convertTestPackage) —
// which is exactly the locality this test is asking about: under a reference model the package
// under test is a separate assembly and its structs ARE foreign, so the set is empty, this
// returns false, and compress/flate's `flate_WriterжWriter` and every other foreign key stand
// unchanged.
func adapterStructQualifierIsLocal(qualifier string) bool {
	if qualifier == "" || len(testLocalTypePrefixes) == 0 {
		return false
	}

	for _, prefix := range testLocalTypePrefixes {
		localClass := prefix

		if dot := strings.LastIndex(localClass, "."); dot >= 0 {
			localClass = localClass[dot+1:]
		}

		// The record's spelling names the class whole; the alias spelling is the class name minus
		// its PackageSuffix, which is what the alias is minted from.
		if qualifier == localClass || qualifier+PackageSuffix == localClass {
			return true
		}
	}

	return false
}

// emittedAdapterPair finds the RECORD pair a cast's (structBase, interfaceTypeName) spelling
// belongs to, or ok=false when the pair was never recorded in a test metadata anchor — imported
// adapters have markers too, but must keep pointing at their defining production assembly rather
// than being redirected into a test anchor. The record's spelling — not the cast's — is what the
// generator derives the emitted adapter class name from, so the caller composes the anchored
// reference from the returned pair. A dot-less cast spelling additionally matches on the bare
// simple name, because a cast in the record's own declaring scope legitimately spells the struct
// unqualified while the record carries the qualified form.
//
// The simple-name fallback is TWO-tiered, because a bare cast spelling is ambiguous the same way
// a bare C# name is: io's tests declare their own `Buffer` while `using static io_package` (and
// the record set also carries `bytes_package.Buffer`), and C# binds the bare name to the
// VARIANT'S OWN nested type before any import. So among simple-name candidates, a record the
// generator will treat as LOCAL to its anchor (struct's package class == the anchor class it
// generates into — the exact AdapterStructKey locality test) wins over a foreign record; taking
// the first candidate in record order instead handed every `*Buffer` cast to bytes_BufferжReader
// (CS1503 ×20, the io wall's reappearance). Exact-key matches run as a full pass FIRST so a
// fallback match on an early pair can never shadow an exact match on a later one.
func emittedAdapterPair(pairs [][2]string, structBase, interfaceTypeName string) ([2]string, bool) {
	structKey := strings.TrimPrefix(adapterStructKey(structBase), ShadowVarMarker)

	// ⚠⚠ BOTH SIDES OF THIS COMPARISON STRIP, and they must, because the two sides arrive in
	// DIFFERENT SPELLINGS: `interfaceTypeName` comes from the deferred marker — already stripped by
	// adapterTypeRef — while `pair[1]` comes from the emitted `[assembly: GoImplement<…>]` lines,
	// which carry the CLOSED instantiation. Strip one and not the other and the keys differ for every
	// generic interface, the lookup misses, and the anchored branch above it is SKIPPED — so the cast
	// site takes the bare resolved name and loses the anchor class it needed: the right identifier,
	// unqualified, CS0246 under the white-box model.
	//
	// ⚠ This is the PAIRING lookup, not a collision KEY. The keys stay unstripped on both halves on
	// purpose (see stripAdapterInterfaceTypeArgs); that parity argument is about grouping two records
	// that compose one name, and says nothing about matching a marker to the record it came from.
	// Conflating the two is what made this look like a decision already taken.
	lookupRef := stripAdapterInterfaceTypeArgs(interfaceTypeName)
	interfaceKey := strings.TrimPrefix(adapterInterfacePackagePrefix(lookupRef), ShadowVarMarker) + adapterInterfaceSimpleName(lookupRef)

	matchesInterface := func(pair [2]string) bool {
		pairRef := stripAdapterInterfaceTypeArgs(pair[1])

		return strings.TrimPrefix(adapterInterfacePackagePrefix(pairRef), ShadowVarMarker)+adapterInterfaceSimpleName(pairRef) == interfaceKey
	}

	for _, pair := range pairs {
		if matchesInterface(pair) && strings.TrimPrefix(adapterStructKey(pair[0]), ShadowVarMarker) == structKey {
			return pair, true
		}
	}

	if strings.Contains(structBase, ".") {
		return [2]string{}, false
	}

	fallback := [2]string{}
	haveFallback := false

	for _, pair := range pairs {
		if !matchesInterface(pair) {
			continue
		}

		pairSimple := pair[0]

		if dot := strings.LastIndex(pairSimple, "."); dot >= 0 {
			pairSimple = pairSimple[dot+1:]
		}

		if stripSanitizationMarkers(pairSimple) != stripSanitizationMarkers(structBase) {
			continue
		}

		if qualifier := adapterStructQualifierClass(pair[0]); qualifier != "" && qualifier == emittedAdapterPairAnchors[adapterGroupKey(pair[0], pair[1])] {
			return pair, true
		}

		if !haveFallback {
			fallback, haveFallback = pair, true
		}
	}

	return fallback, haveFallback
}

// adapterStructQualifierClass returns the package CLASS that qualifies a record's struct spelling
// ("go.io_test_package.Buffer" → "io_test_package", "bytes_package.Buffer" → "bytes_package"),
// or "" for a bare spelling or a non-package qualifier. This is the converter-side spelling of
// the generator's `structType.ContainingType.Name`, which AdapterStructKey compares against its
// anchor class to decide local (bare) vs foreign (`<pkg>_`-prefixed) naming.
func adapterStructQualifierClass(structBase string) string {
	qualifier, _ := splitAdapterStructReference(structBase)

	// A bare spelling has no qualifier, and a qualifier that is not a package class (some other
	// enclosing type) is not what the generator compares against — report neither.
	if qualifier == "" || !strings.HasSuffix(qualifier, PackageSuffix) {
		return ""
	}

	return qualifier
}

// anchoredAdapterMemberName composes the adapter class name go2cs-gen will emit for a
// test-anchored pair, from the RECORD's spellings: adapterStructKey normalizes a qualified
// production struct to the generator's foreign `<pkg>_<Simple>` form and leaves a variant-local
// bare name bare — exactly the generator's local-vs-foreign naming split. A record whose struct
// is QUALIFIED by the very anchor class it generates into (`go.io_test_package.Buffer` anchored
// at `io_test_package` — the record writer qualifies past bridge/using-static hiding) is LOCAL
// to the generator (`container == packageClassName` in its AdapterStructKey), so it composes the
// bare simple name, never `io_test_Buffer`. The interface side mirrors adapterResolvedName's
// collision handling on the same record spellings.
//
// ⚠ The shadow marker STAYS on the struct part. The generator names a local adapter from
// `adapterBaseName`, which is the C# type name verbatim (`Δhandler`), and a foreign one from
// `GetSimpleName(structName)` — neither strips the marker — so stripping it here composed a
// reference to a class that is never emitted. net/http's internal test variant declares
// `type handler struct{ i int }` (server_test.go), shadow-renamed to `Δhandler`: the generator
// minted `ΔhandlerжΔHandler` while every cast site referenced `handlerжΔHandler`, CS0426 ×9. The
// marker belongs to the C# IDENTITY of the type, not to a rendering convention — adapterStructKey
// strips it for GROUPING, which is right, and that key must not double as the emitted name.
// Reached only through the `-tests` metadata-anchored path (resolveAdapterNameMarkers takes an
// anchor only from testConversion; a production conversion resolves through adapterResolvedName,
// which never stripped), so the corpus cannot move.
func anchoredAdapterMemberName(pair [2]string, colliding map[string]bool, keepsProductionName bool) string {
	structPart := adapterStructKey(pair[0])

	// ⚠ THE ONE SITE THE MARKER'S OWN STRIP DOES NOT REACH. adapterTypeRef strips before writing the
	// deferred marker, so adapterResolvedName inherits a stripped name for free — but this function
	// takes pair[1] from the EMITTED `[assembly: GoImplement<…>]` lines, which carry the CLOSED
	// instantiation. Without the strip its last-dot lands inside the argument list and the member is
	// named for the ARGUMENT (`probe_digestжnamed>`), where the generator emits `digestжkeyedLike`.
	// Reached only under the white-box `-tests` model (testConversion.go:1331 is the sole caller that
	// passes an anchor), which is why no corpus row has shown it.
	interfaceRef := stripAdapterInterfaceTypeArgs(pair[1])
	ifaceSimple := adapterInterfaceSimpleName(interfaceRef)

	if qualifier := adapterStructQualifierClass(pair[0]); qualifier != "" && qualifier == emittedAdapterPairAnchors[adapterGroupKey(pair[0], pair[1])] {
		// The struct is qualified by the very anchor class the generator emits into, so it is
		// LOCAL there and takes the bare simple name — never the foreign "<pkg>_<Simple>" form.
		_, simpleName := splitAdapterStructReference(pair[0])
		structPart = stripSanitizationMarkers(simpleName)
	}

	// keepsProductionName is the pass separation (rule 2): a colliding group's sole
	// production-faceted member renders exactly as a non-colliding one would, because that is what
	// the production pass already wrote into the production .cs this assembly recompiles.
	if keepsProductionName || !colliding[adapterGroupKey(pair[0], pair[1])] {
		return structPart + PointerPrefix + ifaceSimple
	}

	// ⚠ The QUALIFIER takes the stripped reference too, and that is a second defect rather than a
	// tidiness: unstripped, adapterInterfacePackagePrefix reads its prefix out of the ARGUMENT's
	// package (`keyedLike<other_package.named>` yielding `other_`), where the generator derives it
	// from the INTERFACE symbol. The stopped sub-agent's arms found this one (COORD 938bb886f) and
	// it is adopted here rather than re-derived.
	return structPart + PointerPrefix + adapterInterfacePackagePrefix(interfaceRef) + ifaceSimple
}

// adapterResolvedName renders the final adapter class REFERENCE for a pair. The struct side is
// emitted VERBATIM — it is not merely a name fragment but the reference's path, and rewriting it
// broke `new os.FileжWriter(f)` (namespace `os`, adapter class `FileжWriter`, generated in os's own
// assembly) into a bare `FileжWriter` that resolves nowhere, CS0246. Only the interface side is
// ever rewritten, and only for a colliding group.
func adapterResolvedName(structBase string, interfaceTypeName string, colliding map[string]bool, keepsProductionName bool) string {
	// ⚠ STRIPPED HERE TOO, although in production this name arrives from the deferred marker and
	// adapterTypeRef has already stripped it. Relying on that would make this function correct only
	// through its one caller — and the sibling that DID rely on an upstream spelling
	// (emittedAdapterPair, whose two sides arrive stripped and unstripped) is precisely what broke.
	// A name-composing site strips its own input; the coupling is not worth the line it saves.
	//
	// ⚠ The colliding lookup keeps the name AS GIVEN — and unlike its sibling at
	// anchoredAdapterMemberName, the PARITY ARGUMENT DOES NOT HOLD HERE (C1, mailbox fe07b469e §2).
	// There, pair[1] is the RECORD spelling adapterNameCollisionSet builds the set from, so the key
	// matches. Here the name arrives from the MARKER, already stripped by adapterTypeRef, while the
	// set still holds the closed spelling — so for a generic interface this lookup MISSES.
	//
	// That miss is COORD's banked residual (938bb886f), at its exact site, and it costs the
	// QUALIFIER rather than the name: an unapplied prefix on a colliding group, never a wrong
	// identifier. Unifying the two is a PAIRED seat — the converter's keys and the generator's
	// garble alike today, and moving one alone manufactures the divergence AdapterStructKey exists
	// to prevent — so it is named here rather than fixed here.
	interfaceRef := stripAdapterInterfaceTypeArgs(interfaceTypeName)
	ifaceSimple := adapterInterfaceSimpleName(interfaceRef)

	// The pass separation (rule 2), as at anchoredAdapterMemberName. This is the arm the RECOMPILE
	// model takes — it resolves through the plain, unanchored path by design — so it is the one
	// that decides whether a test-half cast onto a production record's pair spells the production
	// name. It does when that pair is the group's sole faceted member, and the generator, reading
	// the same facet, mints exactly that class.
	if keepsProductionName || !colliding[adapterGroupKey(structBase, interfaceTypeName)] {
		return structBase + PointerPrefix + ifaceSimple
	}

	// The LOCAL member of a colliding group keeps the bare name (prefix is empty for it).
	return structBase + PointerPrefix + adapterInterfacePackagePrefix(interfaceRef) + ifaceSimple
}

// resolveAdapterNameMarkers rewrites deferred pointer-adapter markers in the given output files
// once the package's GoImplement records are final. Called after writePackageInfoFile (whose
// alias-covered skip and interface-inheritance prune decide the authoritative set), mirroring
// resolveDynamicTypeMarkers' post-barrier text pass. A marker whose pair never reached a record —
// possible when a cast is emitted for a pair the prune later drops — still resolves, to the
// unqualified name it would have had, so no marker can survive into the output.
func resolveAdapterNameMarkers(outputFileNames []string, metadataAnchor ...string) {
	packageLock.Lock()
	pairs := make([][2]string, len(emittedPointerAdapterPairs))
	copy(pairs, emittedPointerAdapterPairs)
	packageLock.Unlock()

	colliding := adapterNameCollisionSet(pairs)
	keepers := adapterProductionNameKeepers(pairs, colliding)
	defaultAnchor := ""
	if len(metadataAnchor) > 0 {
		defaultAnchor = metadataAnchor[0]
	}

	// The file walking is shared with the dynamic-type pass — see rewriteDeferredMarkers
	// (deferredMarkerOperations.go); only the pair resolution below is specific to adapters.
	rewriteDeferredMarkers(outputFileNames, "adapter-name", adapterNameMarkerPrefix, adapterNameMarkerSuffix,
		func(fileName string, line int, payload string) (string, bool) {
			structBase, interfaceTypeName, ok := adapterNameMarkerPair(payload)

			if !ok {
				// A payload that will not decode names no pair, so there is nothing to resolve to;
				// drop the marker so it cannot survive into the emitted C#. Substituting only THIS
				// occurrence means a file carrying several bad markers reports each of them.
				showWarning("Unresolved adapter-name marker in \"%s\"(%d)", fileName, line)
				return "", false
			}

			// The RECORD the cast belongs to, looked up for EVERY marker rather than only for the
			// anchored path: the facet is a property of the record, and the marker carries the
			// cast's own spelling (which is the whole reason adapterStructQualifierIsLocal exists).
			// A marker whose pair was never recorded keeps its old, unfaceted answer.
			pair, pairFound := emittedAdapterPair(pairs, structBase, interfaceTypeName)
			keepsProductionName := pairFound &&
				keepers[adapterGroupKey(pair[0], pair[1])] &&
				adapterPairIsProductionFaceted(pair)

			resolvedName := adapterResolvedName(structBase, interfaceTypeName, colliding, keepsProductionName)
			if defaultAnchor != "" && pairFound {
				anchorClass := emittedAdapterPairAnchors[adapterGroupKey(pair[0], pair[1])]
				if anchorClass == "" {
					anchorClass = defaultAnchor
				}
				resolvedName = anchorClass + "." + anchoredAdapterMemberName(pair, colliding, keepsProductionName)
			}

			// The pair resolves identically wherever it appears, so substitute every occurrence.
			return resolvedName, true
		})
}
