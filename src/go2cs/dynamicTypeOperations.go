// dynamicTypeOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/hex"
	"fmt"
	"go/ast"
	"go/types"
	"os"
	"path/filepath"
	"slices"
	"sort"
	"strings"
	"sync"
)

// Sentinels wrapping a deferred dynamic-type-name reference, resolved to the lifted
// C# type name after all files in the package have been visited (see
// resolveDynamicTypeMarkers). The guillemets do not occur in generated C#, so the
// marker is unambiguous.
const (
	dynamicTypeMarkerPrefix = "«DYNTYPE:"
	dynamicTypeMarkerSuffix = ":DYNTYPE»"
)

// dynamicTypeMarker returns the deferred marker for a signature. The payload is the
// HEX-ENCODED signature, not the raw text: a rendered type name flows through string
// transformation passes before it reaches the output file (convertToCSTypeName rewrites
// every `[`/`]` to `<`/`>`, getAliasedTypeName splits on `.`, …), and raw Go type text
// like `struct{a []byte; …}` would be corrupted in transit so the post-barrier
// resolution could never match it back to the registry. Hex digits pass through every
// transform untouched. The encoding is a pure function, so equal signatures yield the
// identical marker — string comparisons on rendered names behave exactly as signature
// comparisons.
func dynamicTypeMarker(signature string) string {
	return dynamicTypeMarkerPrefix + hex.EncodeToString([]byte(signature)) + dynamicTypeMarkerSuffix
}

// dynamicTypeMarkerSignature decodes a marker payload back to the structural signature. ok is false
// when the payload is not valid hex.
//
// Hex is chosen precisely so the payload survives every string transform between emission and
// resolution (see dynamicTypeMarker), so ok=false does not mean "unknown type" — it means the text
// between the sentinels was never produced by dynamicTypeMarker: a transform corrupted it, or a Go
// string literal in the source happens to spell the sentinels. Both callers must distinguish that
// from an ordinary unresolved type, because on failure there is no signature to name and the ""
// this returns would be reported as if it were one.
func dynamicTypeMarkerSignature(payload string) (string, bool) {
	signature, err := hex.DecodeString(payload)

	if err != nil {
		return "", false
	}

	return string(signature), true
}

// deferredDynamicTypeName renders a NON-EMPTY anonymous struct/interface type that no
// per-file lifted name resolved: this visitor's own claim or the PRODUCTION conversion's
// seeded lift (both via liftedNameFor — see its own comment for why a -tests reference-model
// conversion needs the second source), then the shared package registry's lifted name when
// the declaring file has already been visited (file visits run in deterministic sorted-file
// order), else a deferred marker resolved after the file-visit barrier. Returns "" for
// any other type — including the EMPTY struct/interface, whose raw `struct{}`/`interface{}`
// signatures already map to `EmptyStruct`/`any` downstream.
func (v *Visitor) deferredDynamicTypeName(t types.Type) string {
	switch typ := t.(type) {
	case *types.Struct:
		if isEmptyStructType(typ) {
			return ""
		}
	case *types.Interface:
		if typ.Empty() {
			return ""
		}
	default:
		return ""
	}

	if name, ok := v.liftedNameFor(t); ok {
		return name
	}

	signature := t.String()

	if name := lookupDynamicTypeName(signature); name != "" {
		return name
	}

	if name := lookupProductionDynamicTypeName(signature); name != "" {
		return name
	}

	return dynamicTypeMarker(signature)
}

// unresolvedDynamicType is one deferred dynamic-type marker the post-barrier pass could not
// resolve, recorded at the emitted site so a gate can name it. Its emission is the raw Go type
// text — braces, semicolons, slash-bearing import paths — which is never valid C#, so every
// record is a GUARANTEED compile failure in the file it names.
type unresolvedDynamicType struct {
	signature string
	fileName  string
	line      int
}

// unresolvedDynamicTypes accumulates those records for the life of the process. It is written by
// resolveDynamicTypeMarkers (which runs after each package's file-visit barrier, and concurrently
// across packages under -stdlib) and drained by takeUnresolvedDynamicTypes.
//
// Deliberately NOT reset per package: the one reader is the -tests gate, and a -tests run is a
// single conversion per process whose production half, `.cs.auto` review siblings and test
// variants must all be covered by one verdict. Under -stdlib nothing reads it.
var unresolvedDynamicTypes []unresolvedDynamicType

// recordUnresolvedDynamicType records one unresolvable marker site.
func recordUnresolvedDynamicType(signature, fileName string, line int) {
	packageLock.Lock()
	unresolvedDynamicTypes = append(unresolvedDynamicTypes, unresolvedDynamicType{signature: signature, fileName: fileName, line: line})
	packageLock.Unlock()
}

// takeUnresolvedDynamicTypes returns the recorded sites in a deterministic order and clears the
// record, so a second conversion in the same process starts clean.
func takeUnresolvedDynamicTypes() []unresolvedDynamicType {
	packageLock.Lock()
	taken := unresolvedDynamicTypes
	unresolvedDynamicTypes = nil
	packageLock.Unlock()

	// Files are appended to outputFileNames from concurrent per-file goroutines, so the walk order
	// — and therefore the record order — varies run to run. Sort so the reported summary does not.
	sort.Slice(taken, func(i, j int) bool {
		if taken[i].fileName != taken[j].fileName {
			return taken[i].fileName < taken[j].fileName
		}

		if taken[i].line != taken[j].line {
			return taken[i].line < taken[j].line
		}

		return taken[i].signature < taken[j].signature
	})

	return taken
}

// unresolvedDynamicTypeError is the W2b GATE: it turns any recorded unresolvable dynamic-type
// marker into a hard error naming every site, and clears the record either way.
//
// Why this is fatal rather than advisory. The fallback emission
// is the raw Go type signature, which CANNOT compile — so the warning is a free, already-correct
// prediction of a broken artifact. A `-tests` conversion is one link in a pipeline whose very next
// step builds that artifact, and the build's own diagnostics point AWAY from the cause: three
// unresolved types in runtime's export_test produced 202 errors across 106 distinct lines, of
// which 5 were real sites, with the parse cascade burying them (docs/phase4/
// CENSUS-runtime-first-contact.md, W2). Exiting 0 there is the same false green CNR's NOT MEASURED
// rule closes: an emission the converter could not fully regenerate must never read as success.
//
// Every driver fails on it since 2026-10-19 (COORD's ruling on C1's pointer-conversion seat, D4): the -tests path
// returns it early from conversionDriver.go, before its build step, and main checks it once more at exit after
// whichever driver ran. Before that the -stdlib and plain single-package paths warned and continued by design:
// -stdlib has a downstream gate that names an uncompilable emission (the go2cs-stdlib.slnx build), and a
// single-package conversion of arbitrary Go is best-effort. The ruling is that no run exits 0 after writing text
// that cannot compile, whatever gate follows it; the files are still written.
func unresolvedDynamicTypeError() error {
	unresolved := takeUnresolvedDynamicTypes()

	if len(unresolved) == 0 {
		return nil
	}

	var summary strings.Builder

	summary.WriteString(fmt.Sprintf("%d unresolved dynamic type(s) were emitted as raw Go source, which cannot compile:", len(unresolved)))

	for _, entry := range unresolved {
		summary.WriteString(fmt.Sprintf("\n  %s(%d): %s", entry.fileName, entry.line, entry.signature))
	}

	summary.WriteString("\n  The emitted C# will not build, and its compiler errors will name the parse")
	summary.WriteString("\n  cascade rather than these types. Failing here so the cause is what gets reported.")

	return fmt.Errorf("%s", summary.String())
}

// registerDynamicTypeName records the lifted C# name for a package-level
// anonymous struct/interface type, keyed by its structural signature, so other
// files in the same package can resolve cross-file references to it.
func registerDynamicTypeName(signature, csTypeName string) {
	packageLock.Lock()

	// Deterministic winner when one signature registers from multiple files/functions: keep
	// the lexically smallest name. File visits are sequential, in sorted file order
	// (conversionDriver), so the winner does not depend on the visit order either way. Every
	// registrant is a lifted file-level type of the same package, so any winner resolves.
	if existing, ok := packageDynamicTypeNames[signature]; !ok || csTypeName < existing {
		packageDynamicTypeNames[signature] = csTypeName
	}

	packageLock.Unlock()
}

// constraintLiftKey names a generic TYPE's index'th type parameter's inline constraint by its
// DECLARATION, the key the type and each method's receiver copy share (liftTypeParamConstraints). It
// is no Go type's text, so it can never match a signature lookup or a published GoDynamicTypeLift.
func constraintLiftKey(owner *types.Named, index int) string {
	obj := owner.Origin().Obj()
	path := ""

	if obj.Pkg() != nil {
		path = obj.Pkg().Path()
	}

	return fmt.Sprintf("constraint %s.%s#%d", path, obj.Name(), index)
}

// registerConstraintLift records a generic type's inline-constraint lift by declaration. Not
// published: a lift is referenced only by its own package's declarations and methods.
func registerConstraintLift(key, csTypeName string) {
	packageLock.Lock()
	packageConstraintLiftNames[key] = csTypeName
	packageLock.Unlock()
}

// lookupConstraintLift returns the lift registered for a declaration key, or "" if its declaring
// file has not been visited yet (the deferred marker then resolves it at the barrier).
func lookupConstraintLift(key string) string {
	packageLock.Lock()
	name := packageConstraintLiftNames[key]
	packageLock.Unlock()
	return name
}

// lookupDynamicTypeName returns the lifted C# name registered for a signature, or
// "" if none is registered yet (the declaring file may not have been visited).
func lookupDynamicTypeName(signature string) string {
	packageLock.Lock()
	name := packageDynamicTypeNames[signature]
	packageLock.Unlock()
	return name
}

// lookupProductionDynamicTypeName is lookupDynamicTypeName's `-tests` sibling: the name PRODUCTION
// already lifted a purely-anonymous struct/interface under, for a signature this run's own
// packageDynamicTypeNames has no entry for at all (production's registrations live in a map
// resetPackageState already replaced for this pass — see productionDynamicTypeNames). "" when unset
// (a production conversion, or simply no match), the same not-yet-resolved contract
// lookupDynamicTypeName's own callers already handle.
//
// It answers for BOTH test variants, and this comment said INTERNAL-variant-only until 2026-09-04,
// when an instrumented `-tests` convert of runtime read the external variant resolving `ifaceHash_i`
// through it on both targets. Two sources fill the map and only the first is internal-only:
// convertTestVariant seeds productionSeed.dynamicTypeNames (zero for an external variant, by that
// type's own documented contract), and then seedProductionDynamicTypeLifts REPOPULATES it from the
// production package_info.cs's published GoDynamicTypeLift records for EVERY variant whose model
// names a production half. Whether a hit may be ADOPTED is a separate question with a separate
// answer — productionLiftReuseReachable, which is where the variant/assembly distinction lives.
func lookupProductionDynamicTypeName(signature string) string {
	packageLock.Lock()
	name := productionDynamicTypeNames[signature]
	packageLock.Unlock()
	return name
}

// importedDynamicInterfaceName returns the qualified C# name of an anonymous interface that ANOTHER
// package declared and lifted, from that package's published GoDynamicTypeLift records, or "" when
// the interface is this package's own or the declaring package published no lift for it. The
// declaring package is read off the interface's explicit methods: an interface literal's methods
// belong to the package whose source spells it. Without this a call into `func Fail(n interface{
// K() string })` from another package found no lift of its own and emitted the raw Go text
// (catalogue C1, EXPERIMENT-ts-readiness-2026-11-07.md).
func (v *Visitor) importedDynamicInterfaceName(iface *types.Interface) string {
	// An interface made only of embedded interfaces names no package of its own (goldmark's html.WithWriter
	// returns `interface{ renderer.Option; Option }`), so the imports' published lifts are searched instead.
	if iface.NumExplicitMethods() == 0 {
		return v.importedInterfaceLift(iface)
	}

	pkg := iface.ExplicitMethod(0).Pkg()

	if pkg == nil || pkg == v.pkg {
		return ""
	}

	meta, ok := importPackageDirs[pkg.Path()]

	if !ok {
		return ""
	}

	packageLock.Lock()
	name := importedDynamicTypeNames[filepath.Clean(meta.Dir)][iface.String()]
	packageLock.Unlock()

	if name == "" {
		return ""
	}

	// The qualified form a foreign named interface takes (getFullyQualifiedTypeName, then convertToCSTypeName).
	return convertToCSTypeName(getSanitizedImport(packageClassPath(pkg.Path(), pkg.Name())+PackageSuffix) + "." + name)
}

// forwardImportedStructLift answers a reference to an anonymous struct that would otherwise defer to a marker:
// the lift an import already made of an identical struct (importedStructLift), the one the struct's own
// declaration reuses when it is visited. A reference reached BEFORE that declaration -- modernc.org/sqlite/lib's
// `type LC_ID = TLC_ID` ahead of `type TLC_ID = struct{...}` in one file, the struct libc's tagLC_ID -- deferred to
// a marker the post-barrier pass could not resolve, since a reused lift is another package's and never enters
// this package's registry: raw Go text (sqlite-cs W2). "" when t is not such a struct.
//
// An anonymous INTERFACE another package lifted takes that lift the same way (importedInterfaceLift): a value
// whose static type is one -- the result of goldmark's html.WithWriter, `interface{ renderer.Option; Option }`,
// passed on as a renderer.Option -- named its type through the marker, which only this package's registry
// answers, and the run refused the raw Go text (board ii).
func (v *Visitor) forwardImportedStructLift(t types.Type) string {
	switch typ := t.(type) {
	case *types.Struct:
		return v.importedStructLift(typ)
	case *types.Interface:
		return v.importedInterfaceLift(typ)
	}

	return ""
}

// importedInterfaceLift returns the qualified C# name of the public lift that a package this one imports
// directly published for an anonymous interface with iface's signature, or "". When two imports publish one,
// the lexically smallest import path wins, as for importedStructLift; an interface with an explicit method has
// a declaring package, and importedDynamicInterfaceName asks that package first.
//
// The signature is the match key, so the lookup is made only where the signature identifies the type: go/types
// prints an unexported method and a type parameter by bare name, so `interface{ run() }` in this package and the
// same spelling in an import are two Go types with one signature (as are two generic functions' `interface{ M() T }`).
// Such an interface is never matched here and keeps its own package's lift. With exported method names and
// path-qualified named types, equal signatures are identical interfaces.
func (v *Visitor) importedInterfaceLift(iface *types.Interface) string {
	if iface == nil || iface.Empty() || v.pkg == nil || !interfaceSignatureIdentifiesIt(iface) {
		return ""
	}

	imports := slices.Clone(v.pkg.Imports())
	slices.SortFunc(imports, func(a, b *types.Package) int { return strings.Compare(a.Path(), b.Path()) })
	signature := iface.String()

	for _, pkg := range imports {
		meta, ok := importPackageDirs[pkg.Path()]

		if !ok {
			continue
		}

		key := filepath.Clean(meta.Dir)

		packageLock.Lock()
		name := importedDynamicTypeNames[key][signature]
		public := name != "" && importedPublicLiftNames[key][name]
		packageLock.Unlock()

		if public {
			return convertToCSTypeName(getSanitizedImport(packageClassPath(pkg.Path(), pkg.Name())+PackageSuffix) + "." + name)
		}
	}

	return ""
}

// importedStructLift returns the qualified C# name of an anonymous struct that a package this one
// imports directly has already lifted, when that struct is IDENTICAL to st (types.Identical) and its
// lift is public; otherwise "". Go calls the two one type, so C# must too: sqlite's TLARGE_INTEGER and
// libc's largeInteger alias one anonymous struct, and two lifts made libc's function unbindable through
// sqlite's delegate type (DESIGN-sqlite-cs-funcptr.md section 3.3). An internal lift is never reused:
// it is CS0122 from this assembly. When two imports qualify, the lexically smallest import path wins.
//
// The import's lift record is keyed by the DECLARING package's spelling of the struct, and an identical struct can be
// spelled differently here: sqlite/lib writes TLARGE_INTEGER's fields through its own aliases (`LowPart DWORD`) where
// libc writes uint32, and types.Struct.String() names the alias (W18). So when this package's spelling finds no
// record, each identical struct the import declares is tried by ITS spelling, in the import's scope order.
func (v *Visitor) importedStructLift(st *types.Struct) string {
	pkg, name := v.importedStructLiftSource(st)

	if pkg == nil {
		return ""
	}

	qualified := getSanitizedImport(packageClassPath(pkg.Path(), pkg.Name())+PackageSuffix) + "." + name
	codeName := convertToCSTypeName(qualified)

	packageLock.Lock()
	importedStructLiftTargets[codeName] = qualified
	packageLock.Unlock()

	return codeName
}

// importedStructLiftSource returns the package whose lift importedStructLift names for st, and that lift's name, or nil.
// Each direct import, in import-path order, offers the lift ITS code uses for st (liftUsedBy): one it declares, or one
// it names from the package that declares it (W18 ruling C item 3, shape A). The lift is taken only when its inline
// admission matches this package's (liftAdmissionMatches, read at the declaring package) and every struct it holds
// resolves here to the lift the declaring package's lift holds (nestedLiftsMatch).
func (v *Visitor) importedStructLiftSource(st *types.Struct) (*types.Package, string) {
	if st == nil || isEmptyStructType(st) || v.pkg == nil {
		return nil, ""
	}

	imports := slices.Clone(v.pkg.Imports())
	slices.SortFunc(imports, func(a, b *types.Package) int { return strings.Compare(a.Path(), b.Path()) })

	for _, pkg := range imports {
		owner, name, spelled := v.liftUsedBy(pkg, st)

		if owner == nil {
			continue
		}

		ownerDir := importPackageDirs[owner.Path()].Dir

		if !v.liftAdmissionMatches(owner, ownerDir, filepath.Clean(ownerDir), name, st, spelled) || !v.nestedLiftsMatch(owner, name, st) {
			continue
		}

		if owner != pkg {
			traceLiftChain(v.pkg, pkg, owner, name)
		}

		return owner, name
	}

	return nil, ""
}

// nestedLiftsMatch reports whether every anonymous struct that st holds by value (through fields, embedded ones included,
// and array elements) resolves here to the lift that pkg's lift of st holds for it (liftUsedBy, asked of pkg). A reused
// lift fixes its fields' C# types to pkg's choices, while this package types its own values of those fields by ITS
// choices: libc's Titimerval reused libc/uuid/uuid's Itimervalᴛ1 (neither admits it) while its field type Ttimeval kept
// libc's own admitted lift, and libc's literal `new Titimerval(Fit_value: new Ttimeval(...))` was CS1503 (a linux-x64
// build, 2026-11-17; a76c8bf2eb). The match is transitive: each nested struct is resolved by importedStructLiftSource,
// which checks that struct's own nested structs. pkg's lift of a nested struct may itself be named from a third package
// (libc's Tstat holds libc/uuid/uuid's Timespecᴛ1 once libc reuses it), so the two are compared as lifts, package and
// name. A struct held through a NAMED type is that type's declaration, the same C# type on both sides, and an empty
// struct is never lifted, so neither is checked.
func (v *Visitor) nestedLiftsMatch(pkg *types.Package, name string, st *types.Struct) bool {
	for _, nested := range heldAnonymousStructs(st) {
		from, nestedName := v.importedStructLiftSource(nested)
		heldBy, heldName, _ := v.liftUsedBy(pkg, nested)

		if from == nil || from != heldBy || nestedName != heldName {
			resolved := "own"

			if from != nil {
				resolved = from.Path() + "." + nestedName
			}

			traceLiftReuse(v.pkg, pkg, name, false, false, true, "nested:"+resolved, false)

			return false
		}
	}

	return true
}

// heldAnonymousStructs returns the anonymous, non-empty struct types st holds directly by value: its fields' types and,
// through arrays, their element types. It does not descend into those structs (their own check does) nor into a named
// type.
func heldAnonymousStructs(st *types.Struct) []*types.Struct {
	var held []*types.Struct

	var visit func(t types.Type)

	visit = func(t types.Type) {
		switch t := types.Unalias(t).(type) {
		case *types.Struct:
			if !isEmptyStructType(t) {
				held = append(held, t)
			}
		case *types.Array:
			visit(t.Elem())
		}
	}

	for field := range st.Fields() {
		visit(field.Type())
	}

	return held
}

// liftAdmissionMatches reports whether naming pkg's lift of the struct (spelled, pkg's own spelling of st) keeps st's
// inline admission. An admitted struct's lift is [GoInlineLayout] with go.arrays fields, an unadmitted one holds golib's
// array<T>, and an admitted holder needs its field's lift admitted too: libc's admitted Tstatfs over libc/uuid/uuid's
// unadmitted lift of Tfsid_t was CS8377 (W18's linux wall, G 2026-11-16). A struct this package does not admit and that
// holds no array at any depth is the same C# whichever lift it names, so it reuses as before. Otherwise the import's
// admission of its lift must be KNOWN and equal this package's; an unknown one keeps this package's own lift.
// liftKey is the import's cleaned source directory, the key its lift records are loaded under, and name its lift.
func (v *Visitor) liftAdmissionMatches(pkg *types.Package, pkgDir, liftKey, name string, st, spelled *types.Struct) bool {
	need := packageInlineAdmission.admits(st)

	if !need && !structHoldsArray(st, 0) {
		traceLiftReuse(v.pkg, pkg, name, need, false, true, "array-free", true)
		return true
	}

	have, known, source := foreignLiftAdmitted(pkg, pkgDir, liftKey, name, spelled, v.options)
	reuse := known && have == need
	traceLiftReuse(v.pkg, pkg, name, need, have, known, source, reuse)

	return reuse
}

// liftReuseTraceVariable names the environment variable that, set to 1, makes importedStructLift write one line per
// admission decision to stderr: the importer, the import's lift, this package's need, the import's admission, whether
// it is known and which source decided it, and the verdict. It is the census instrument for W18's admission rule (COORD
// 2026-11-17: count each source's decisions per target, and name any it decides wrongly).
const liftReuseTraceVariable = "GO2CS_TRACE_LIFT_REUSE"

func traceLiftReuse(importer, pkg *types.Package, name string, need, have, known bool, source string, reuse bool) {
	if os.Getenv(liftReuseTraceVariable) != "1" || importer == nil {
		return
	}

	fmt.Fprintf(os.Stderr, "liftreuse\t%s\t%s.%s\tneed=%t\thave=%t\tknown=%t\tsource=%s\treuse=%t\n",
		importer.Path(), pkg.Path(), name, need, have, known, source, reuse)
}

// foreignLiftAdmitted reports whether the package pkg (source directory pkgDir) admitted its lift name of the
// anonymous struct st, spelled as pkg spells it, and whether that can be known. A package I0 never admits (std) admits
// nothing, and a lift whose public record carries [GoValueClone] is not admitted: an admitted struct copies whole and
// never takes it. Otherwise pkg's admission record says: GoInlineArrayKeys counts every admitted key and digests them,
// and GoInlineArrayType names the admitted NAMED types, so the anonymous structs (and generic instantiations) are the
// difference. A record that lists its anonymous keys (GoInlineArrayAnonymous, ruling C) answers by membership. Before
// it, none means st is not admitted, exactly one is st exactly when the digest of the named keys plus st's key
// reproduces the record's, and more than one cannot be told apart, nor can a record with no key count. source names the
// rule that answered, for the census trace (traceLiftReuse).
func foreignLiftAdmitted(pkg *types.Package, pkgDir, liftKey, name string, st *types.Struct, options Options) (admitted, known bool, source string) {
	if inlineArraysExcluded(pkgDir, options) {
		return false, true, "std"
	}

	packageLock.Lock()
	valueClone := importedValueCloneLiftNames[liftKey][name]
	packageLock.Unlock()

	if valueClone {
		return false, true, "valueclone"
	}

	packageLock.Lock()
	stamp, cached := importedLiftAdmissionStamps[pkg.Path()]
	packageLock.Unlock()

	if !cached {
		if read, ok := lookupInlineArrayStamp(pkg.Path(), options); ok {
			stamp = &read
		}

		packageLock.Lock()

		if importedLiftAdmissionStamps == nil {
			importedLiftAdmissionStamps = make(map[string]*inlineArrayStamp)
		}

		importedLiftAdmissionStamps[pkg.Path()] = stamp
		packageLock.Unlock()
	}

	if stamp == nil {
		return false, false, "unknown-no-record"
	}

	if stamp.keyCount == 0 {
		// No admitted key at all, unless an older record named types without counting its keys.
		if len(stamp.types) > 0 {
			return false, false, "unknown-uncounted"
		}

		return false, true, "stamp-none"
	}

	unnamed := stamp.keyCount - len(stamp.types)

	if unnamed <= 0 {
		return false, true, "stamp-named-only"
	}

	key, ok := inlineTypeKey(st)

	if !ok {
		return false, false, "unknown-no-key"
	}

	// A stamp that records its admitted anonymous keys (ruling C, COORD 2026-11-17 12:14Z) answers by membership.
	if admitted, known := stamp.admitsAnonymous(key); known {
		return admitted, true, "stamp-anon"
	}

	if unnamed > 1 {
		return false, false, "unknown-several-anonymous"
	}

	keys := make([]string, 0, stamp.keyCount)

	for _, name := range stamp.types {
		keys = append(keys, pkg.Path()+"."+name)
	}

	return inlineArrayKeyDigest(append(keys, key)) == stamp.keyDigest, true, "stamp-digest"
}

// importedStructLiftTarget returns the `global using` spelling of a lift importedStructLift reused, given its
// code-body name, below the root namespace (the RHS renderer roots it). A reused lift is another package's
// type, so usingAliasTypeQualifier, which qualifies THIS package's lifts, must not prefix it.
func importedStructLiftTarget(codeName string) (string, bool) {
	packageLock.Lock()
	target, ok := importedStructLiftTargets[codeName]
	packageLock.Unlock()

	return target, ok
}

// packageDeclaresIdenticalStruct reports whether an anonymous struct identical to st appears in pkg's
// package-level declarations: an alias target, a variable, a function signature, or a field of a
// defined struct, through pointers, slices, arrays, maps and channels. Two structs with the same text
// and an unexported field are different Go types, so text alone is not identity.
func packageDeclaresIdenticalStruct(pkg *types.Package, st *types.Struct) bool {
	return len(packageIdenticalStructs(pkg, st)) > 0
}

// packageIdenticalStructs returns the anonymous structs identical to st that pkg's package-level declarations spell
// (packageDeclaredStructs), one per distinct types.Struct.String() spelling, in the order the declarations are walked.
func packageIdenticalStructs(pkg *types.Package, st *types.Struct) []*types.Struct {
	var identical []*types.Struct
	spellings := map[string]bool{}

	for _, declared := range packageDeclaredStructs(pkg) {
		if !types.Identical(declared, st) {
			continue
		}

		if spelling := declared.String(); !spellings[spelling] {
			spellings[spelling] = true
			identical = append(identical, declared)
		}
	}

	return identical
}

var (
	declaredStructsLock      sync.Mutex
	declaredStructsByPackage = map[*types.Package][]*types.Struct{}
)

// packageDeclaredStructs returns every anonymous struct that appears in pkg's package-level declarations: an alias
// target, a variable, a function signature, or a field of a defined struct, through pointers, slices, arrays, maps and
// channels, in scope order (scope.Names is sorted). It is walked once per package and kept: an importer asks it once
// per anonymous struct it lifts, and libc's scope is large.
func packageDeclaredStructs(pkg *types.Package) []*types.Struct {
	declaredStructsLock.Lock()
	cached, ok := declaredStructsByPackage[pkg]
	declaredStructsLock.Unlock()

	if ok {
		return cached
	}

	seen := map[types.Type]bool{}
	var structs []*types.Struct
	var walk func(t types.Type, depth int)

	walk = func(t types.Type, depth int) {
		if t == nil || depth > 8 || seen[t] {
			return
		}

		seen[t] = true

		switch x := t.(type) {
		case *types.Alias:
			walk(types.Unalias(x), depth)
		case *types.Struct:
			structs = append(structs, x)

			for i := range x.NumFields() {
				walk(x.Field(i).Type(), depth+1)
			}
		case *types.Pointer:
			walk(x.Elem(), depth+1)
		case *types.Slice:
			walk(x.Elem(), depth+1)
		case *types.Array:
			walk(x.Elem(), depth+1)
		case *types.Map:
			walk(x.Key(), depth+1)
			walk(x.Elem(), depth+1)
		case *types.Chan:
			walk(x.Elem(), depth+1)
		case *types.Signature:
			for i := range x.Params().Len() {
				walk(x.Params().At(i).Type(), depth+1)
			}

			for i := range x.Results().Len() {
				walk(x.Results().At(i).Type(), depth+1)
			}
		}
	}

	scope := pkg.Scope()

	for _, name := range scope.Names() {
		switch obj := scope.Lookup(name).(type) {
		case *types.TypeName:
			if named, ok := obj.Type().(*types.Named); ok {
				walk(named.Underlying(), 0)
			} else {
				walk(obj.Type(), 0)
			}
		case *types.Var, *types.Func:
			walk(obj.Type(), 0)
		}
	}

	declaredStructsLock.Lock()
	declaredStructsByPackage[pkg] = structs
	declaredStructsLock.Unlock()

	return structs
}

// dynamicStructTypeName resolves the C# type name of an expression whose type is
// a (possibly anonymous) struct, for use where a concrete type name is required —
// e.g. the `ж.of(StructType.ᏑField)` address-of-field form. It prefers this
// visitor's per-file lifted name or the PRODUCTION conversion's seeded lift (both via
// liftedNameFor), falls back to the shared package registry, and otherwise emits a
// marker resolved after the file-visit barrier.
func (v *Visitor) dynamicStructTypeName(expr ast.Expr) string {
	t := v.getType(expr, false)

	if t != nil {
		if name, ok := v.liftedNameFor(t); ok {
			return name
		}

		signature := t.String()

		if name := lookupDynamicTypeName(signature); name != "" {
			return name
		}

		if name := lookupProductionDynamicTypeName(signature); name != "" {
			return name
		}

		// A non-empty anonymous struct lifted in another file of this package:
		// defer resolution until the shared registry is fully populated.
		if structType, ok := t.(*types.Struct); ok && !isEmptyStructType(structType) {
			return dynamicTypeMarker(signature)
		}
	}

	// Another package's alias of a defined type (`stdio.FILE`, `type FILE = X_iobuf`) is that defined type, spelled
	// through the importer's alias table (`stdioꓸFILE`) as its declarations are. The normal path below returns its
	// Go-shaped name, which names no member of the package class (CS0426, sqlite-cs S12). The same holds where the
	// alias reaches an ANONYMOUS struct its package lifted and published (libc's `type X_iobuf = struct{...}`,
	// `type FILE = X_iobuf`): `&X_iob[i]` took `.at<stdio.FILE>` (CS0426, sqlite-cs W5a).
	if t != nil && (v.isForeignAliasOfNamed(t) || v.isPublishedForeignAnonStructAlias(t)) {
		return convertToCSTypeName(v.getScopeCheckedTypeName(t))
	}

	// Concrete/named or otherwise resolvable type: use the normal path.
	return v.getExpressionTypeName(expr, false)
}

// resolveDynamicTypeMarkers rewrites any deferred dynamic-type markers in the
// given output files using the now-complete package registry. Called once after
// the concurrent file-visit barrier. Unresolved markers (genuinely unknown types)
// are replaced with the raw signature and a warning.
//
// The file walking is shared with the adapter-name pass — see rewriteDeferredMarkers
// (deferredMarkerOperations.go); only the lookup below is specific to dynamic types.
func resolveDynamicTypeMarkers(outputFileNames []string) {
	rewriteDeferredMarkers(outputFileNames, "dynamic type", dynamicTypeMarkerPrefix, dynamicTypeMarkerSuffix,
		func(fileName string, line int, payload string) (string, bool) {
			signature, decoded := dynamicTypeMarkerSignature(payload)

			if !decoded {
				// No signature to look up or to name, so report the raw payload instead — it is the
				// only evidence of what the corrupted text was. The payload also becomes the
				// replacement: something must replace the marker (leaving it would re-match on the
				// next pass of the rewrite loop), and this keeps the evidence in the file where a
				// human or a grep will find it. Substituting only this occurrence, so a second
				// corrupted marker gets its own report rather than being collapsed into this one.
				// Same gate class as the unresolved case below: the payload replacing the marker is not valid C#
				// either, so the file is just as certainly unbuildable. A corrupt payload is a converter defect,
				// not an unlifted type, so it refuses at once (GO2CS2014; review of the godebug change,
				// 2026-11-19) rather than waiting for unresolvedDynamicTypeError at the end of the run.
				recordUnresolvedDynamicType("«undecodable marker payload: "+payload+"»", fileName, line)
				refuseMarkerPass(fmt.Sprintf("%s:%d", fileName, line), fmt.Sprintf("a dynamic-type marker's payload %q does not decode", payload))
				return payload, false
			}

			replacement := lookupAnyDynamicTypeName(signature)

			if replacement == "" {
				showWarning("Unresolved dynamic struct type: %s in \"%s\"(%d)", signature, fileName, line)
				// Fall back to the raw Go signature: it will not compile, but it names the exact
				// type that went unresolved, which is far easier to act on than a leftover marker.
				replacement = signature
				// Record the site for the -tests gate (unresolvedDynamicTypeError). The warning
				// alone was a free, always-correct prediction of a broken build that the run then
				// discarded by exiting 0.
				recordUnresolvedDynamicType(signature, fileName, line)
			}

			// Every occurrence of this marker resolves to the same lifted name, so substitute
			// them all at once and warn about the signature only once.
			return replacement, true
		})
}

// lookupAnyDynamicTypeName names a structural signature's lift from the registries the marker resolver consults, in
// its order: this package's lifts, the production package's (a -tests conversion), then inline-constraint lifts.
// It answers "" when none holds the signature.
func lookupAnyDynamicTypeName(signature string) string {
	if name := lookupDynamicTypeName(signature); name != "" {
		return name
	}

	if name := lookupProductionDynamicTypeName(signature); name != "" {
		return name
	}

	return lookupConstraintLift(signature)
}

// resolvedDynamicTypeText answers text with each dynamic-type marker replaced by the lift the registries name for its
// signature (lookupAnyDynamicTypeName). A marker no registry answers stays as written, and its signature (or, for an
// undecodable payload, the payload itself) is returned in unresolved, for its caller to report.
//
// package_info.cs's GoTypeAlias records are written from values captured while files were visited, so a forward alias
// chain to an anonymous struct (modernc.org/sqlite/lib's `Sqlite3_vtab_cursor = sqlite3_vtab_cursor =
// Tsqlite3_vtab_cursor`, the first declared above the struct) recorded the marker resolveDynamicTypeMarkers later
// rewrote in the .cs files only. Importers copy a record into a `global using`, where the marker does not parse
// (sqlite-cs W7: 3,080 of lib's 15,499 records). A prefix with no suffix after it is not a marker and stays as text.
func resolvedDynamicTypeText(text string) (result string, unresolved []string) {
	var resolved strings.Builder

	for {
		start := strings.Index(text, dynamicTypeMarkerPrefix)

		if start < 0 {
			break
		}

		payloadStart := start + len(dynamicTypeMarkerPrefix)
		length := strings.Index(text[payloadStart:], dynamicTypeMarkerSuffix)

		if length < 0 {
			break
		}

		markerEnd := payloadStart + length + len(dynamicTypeMarkerSuffix)
		payload := text[payloadStart : payloadStart+length]
		name := ""

		if signature, decoded := dynamicTypeMarkerSignature(payload); decoded {
			if name = lookupAnyDynamicTypeName(signature); name == "" {
				unresolved = append(unresolved, signature)
			}
		} else {
			unresolved = append(unresolved, "«undecodable marker payload: "+payload+"»")
		}

		if name == "" {
			name = text[start:markerEnd]
		}

		resolved.WriteString(text[:start])
		resolved.WriteString(name)
		text = text[markerEnd:]
	}

	resolved.WriteString(text)

	return resolved.String(), unresolved
}

// interfaceSignatureIdentifiesIt reports whether iface's printed signature names one Go type wherever it is read:
// a method set (no type terms) whose every method, embedded ones included, is exported and whose signature mentions
// no type parameter anywhere (mentionsTypeParam; typeContainsTypeParams does not look inside a signature).
func interfaceSignatureIdentifiesIt(iface *types.Interface) bool {
	if !iface.IsMethodSet() {
		return false
	}

	for i := range iface.NumMethods() {
		if method := iface.Method(i); !method.Exported() || mentionsTypeParam(method.Type(), map[types.Type]bool{}) {
			return false
		}
	}

	return true
}

// recordLiftDeclaredPublic notes that a lift this package declares is public, so a later site whose
// own lift would need public may reuse it (board (jj)): one Go type, one lift. access is the modifier
// the declaration carries: "public " (publicized), "internal " (function-local, localTypeAccess), or
// "" (go2cs-gen scopes the type by its name, as generatedTypeScope does). The record is keyed by the
// lift name alone, which is sound because a lift name is unique within a package
// (getUniqueLiftedTypeName claims each name package-wide).
func recordLiftDeclaredPublic(csTypeName, access string) {
	if csTypeName == "" || !(access == "public " || access == "" && generatedTypeScope(csTypeName) == "public") {
		return
	}

	packageLock.Lock()
	packagePublicLiftNames[csTypeName] = true
	packageLock.Unlock()
}

// liftDeclaredPublic reports whether a lift registered for reuse was declared public by
// recordLiftDeclaredPublic. This is the declaration's real accessibility, not a guess from its
// name, which is what the removed name-based disjunct got wrong for function-local lifts.
func liftDeclaredPublic(csTypeName string) bool {
	packageLock.Lock()
	public := packagePublicLiftNames[csTypeName]
	packageLock.Unlock()
	return public
}
