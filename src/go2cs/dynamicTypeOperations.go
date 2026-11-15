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
	"path/filepath"
	"slices"
	"sort"
	"strings"
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

	// Deterministic winner when one signature registers from multiple files/functions
	// (file visits are concurrent, so last-wins would vary run to run — the converter is
	// byte-deterministic): keep the lexically smallest name. Every registrant is a lifted
	// file-level type of the same package, so any winner resolves.
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
	if iface.NumExplicitMethods() == 0 {
		return ""
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
func (v *Visitor) forwardImportedStructLift(t types.Type) string {
	st, ok := t.(*types.Struct)

	if !ok {
		return ""
	}

	return v.importedStructLift(st)
}

// importedStructLift returns the qualified C# name of an anonymous struct that a package this one
// imports directly has already lifted, when that struct is IDENTICAL to st (types.Identical) and its
// lift is public; otherwise "". Go calls the two one type, so C# must too: sqlite's TLARGE_INTEGER and
// libc's largeInteger alias one anonymous struct, and two lifts made libc's function unbindable through
// sqlite's delegate type (DESIGN-sqlite-cs-funcptr.md section 3.3). An internal lift is never reused:
// it is CS0122 from this assembly. When two imports qualify, the lexically smallest import path wins.
func (v *Visitor) importedStructLift(st *types.Struct) string {
	if st == nil || isEmptyStructType(st) || v.pkg == nil {
		return ""
	}

	imports := slices.Clone(v.pkg.Imports())
	slices.SortFunc(imports, func(a, b *types.Package) int { return strings.Compare(a.Path(), b.Path()) })
	signature := st.String()

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

		if !public || !packageDeclaresIdenticalStruct(pkg, st) {
			continue
		}

		qualified := getSanitizedImport(packageClassPath(pkg.Path(), pkg.Name())+PackageSuffix) + "." + name
		codeName := convertToCSTypeName(qualified)

		packageLock.Lock()
		importedStructLiftTargets[codeName] = qualified
		packageLock.Unlock()

		return codeName
	}

	return ""
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
	seen := map[types.Type]bool{}
	var found func(t types.Type, depth int) bool

	found = func(t types.Type, depth int) bool {
		if t == nil || depth > 8 || seen[t] {
			return false
		}

		seen[t] = true

		switch x := t.(type) {
		case *types.Alias:
			return found(types.Unalias(x), depth)
		case *types.Struct:
			if types.Identical(x, st) {
				return true
			}

			for i := range x.NumFields() {
				if found(x.Field(i).Type(), depth+1) {
					return true
				}
			}
		case *types.Pointer:
			return found(x.Elem(), depth+1)
		case *types.Slice:
			return found(x.Elem(), depth+1)
		case *types.Array:
			return found(x.Elem(), depth+1)
		case *types.Map:
			return found(x.Key(), depth+1) || found(x.Elem(), depth+1)
		case *types.Chan:
			return found(x.Elem(), depth+1)
		case *types.Signature:
			for i := range x.Params().Len() {
				if found(x.Params().At(i).Type(), depth+1) {
					return true
				}
			}

			for i := range x.Results().Len() {
				if found(x.Results().At(i).Type(), depth+1) {
					return true
				}
			}
		}

		return false
	}

	scope := pkg.Scope()

	for _, name := range scope.Names() {
		switch obj := scope.Lookup(name).(type) {
		case *types.TypeName:
			if named, ok := obj.Type().(*types.Named); ok {
				if found(named.Underlying(), 0) {
					return true
				}
			} else if found(obj.Type(), 0) {
				return true
			}
		case *types.Var, *types.Func:
			if found(obj.Type(), 0) {
				return true
			}
		}
	}

	return false
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
	// Go-shaped name, which names no member of the package class (CS0426, sqlite-cs S12).
	if t != nil && v.isForeignAliasOfNamed(t) {
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
				showWarning("Undecodable dynamic-type marker payload \"%s\" in \"%s\"", payload, fileName)
				// Same gate class as the unresolved case below: the payload replacing the marker is
				// not valid C# either, so the file is just as certainly unbuildable.
				recordUnresolvedDynamicType("«undecodable marker payload: "+payload+"»", fileName, line)
				return payload, false
			}

			replacement := lookupDynamicTypeName(signature)

			if replacement == "" {
				replacement = lookupProductionDynamicTypeName(signature)
			}

			if replacement == "" {
				replacement = lookupConstraintLift(signature)
			}

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
