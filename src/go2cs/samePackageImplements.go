// samePackageImplements.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file closes the one place where "the declaring assembly implements this pair" is TRUE in Go
// and FALSE in the emitted C#.
//
// Every [assembly: GoImplement<T, Iface>] record the converter writes comes from a CAST it converted
// — convertToInterfaceType records the pair it just emitted. Go, though, satisfies an interface
// structurally: `encoding/binary` declares `type bigEndian struct{}` with the whole ByteOrder method
// set and exports `var BigEndian bigEndian`, and it never once writes `var _ ByteOrder = BigEndian`.
// No cast, no record — so `binary_package.bigEndian` was emitted as a partial struct that does NOT
// implement `ByteOrder`, and every consumer minted its own `binary_bigEndianᴠByteOrder` adapter.
//
// That adapter is a SECOND IDENTITY for one Go value: reflect and fmt see the wrapper where the
// value's own type belongs, and a direct-boxed value compares unequal to an adapter-wrapped one.
// The fix is to record what Go already says is true, so the declaring assembly realizes the pair
// itself and the consumer hands over the bare value.
//
// The POINTER method set has the same disease and needs a different cure. `*T → Iface` is never a
// bare value in C#: it is always the generated `<T>ж<Iface>` adapter, which aliases the receiver box.
// A pointer record therefore answers a different question than a value record — not "does the value
// convert implicitly" but "does the DECLARING assembly already carry that adapter class, so a
// consumer can reference it instead of minting its own" (implementRecordKey names this the
// adapter-class EXISTENCE signal). Sourcing that answer from cast sites alone is as fragile as the
// value form was, and in one way worse: the answer can be UNRECORDED by an edit that has nothing to
// do with the pair. syscall's three `SockaddrInet4 / SockaddrInet6 / SockaddrUnix → Sockaddr` pairs
// are witnessed by exactly ONE method body — `(*RawSockaddrAny).Sockaddr` — so hand-owning that one
// file (which the blittable-struct work has every reason to do) drops all three records with no
// diagnostic, and `net` then mints `syscall_SockaddrInet4жΔSockaddr` beside syscall's own. Recording
// from the method set instead of from the witnessing cast is what makes a pair independent of which
// bodies a run happens to convert.

package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"path/filepath"
	"strings"
)

// recordSamePackageImplements records the GoImplement pairs a package SATISFIES but never WITNESSES:
// a defined type and an interface, both declared in the package being converted, where the type's
// method set already implements the interface. Both forms are recorded — the VALUE form when `T`
// implements, the POINTER form when `*T` does (the larger set, and a superset of the first: a pair
// satisfied on the value set is satisfied on the pointer set too, and both records are wanted,
// because Go's `T` and `*T` are two dynamic types and each needs its own C# realization).
//
// It records through convertToInterfaceType with an EMPTY expression — the same record-only probe
// path convCompositeLit / convTypeAssertExpr / visitValueSpec already use — so a synthesized pair is
// composed, keyed and pruned exactly as a real cast would compose, key and prune it. That is the
// whole design: no second naming path can drift from the cast site's (the divergence that made the
// FOREIGN lookup miss for six weeks — see implementRecordKey).
//
// Four gates bound it, and each one is load-bearing:
//
//   - The interface is EXPORTED. A record is a CROSS-ASSEMBLY contract — it exists so another
//     assembly's cast can drop its local adapter — and no other assembly can name an unexported
//     interface, so a record for one could never be consulted. The package's own casts already
//     record whatever it needs internally.
//   - The type's underlying is NOT a *types.Signature. A named FUNC type is a C# DELEGATE, which
//     cannot be a partial struct, so ImplementGenerator emits an adapter CLASS for it instead; a
//     consumer trusting THAT record hands a bare delegate to an interface slot (CS0029 — net/http's
//     HandlerFunc → ΔHandler). This is the declaring-side half of valueRecordRealizesAsPartialStruct.
//   - Neither side is GENERIC. A type argument cannot appear in an assembly-attribute type argument
//     (CS0246), the same exclusion convertToInterfaceType's targetIsOpenGeneric makes.
//   - Both sides are declared in a file this run actually CONVERTS. go/packages and the converter's
//     own build-constraint evaluator agree on nearly every file, but a record naming a type no
//     emitted file declares is CS0246, and a package scope holds every file's declarations.
//   - Every interface method is REALIZABLE by the generator: it resolves on the type itself or
//     through at most ONE embedded field. ImplementGenerator forwards a promoted member through a
//     single embed hop and says so ("Go's promotion ambiguity rules make multi-embed satisfaction
//     rare; extend when needed"), so a deeper promotion emits a forwarder that passes the wrong
//     hop — CrossPkgUser's `rig` embeds `CrossPkgLib.Device`, which embeds `Sensor`, where Label
//     lives: `CrossPkgLib_package.Label(this.Device)` is CS1503, wanting `this.Device.Sensor`.
//     This is the same reasoning as valueRecordRealizesAsPartialStruct — record only what the
//     generator can realize — and it binds only the SPECULATIVE recorder here. A pair the source
//     actually casts is DEMANDED and still records at its cast site, promotion depth and all;
//     extending the generator is a separate increment that would relax this gate, not remove it.
//
// The POINTER form adds ONE gate to those five, and it comes straight from the different trust rule.
// A value record is consumable without naming anything: the consumer already holds the value and the
// declaring assembly's `partial struct T : Iface` makes the conversion implicit, so an UNEXPORTED `T`
// is no obstacle and only the interface must be exported. A pointer record is consumed by NAMING the
// generated adapter class (`new pkg.TжIface(x)`), and ImplementGenerator scopes that class `public`
// only when BOTH sides are exported — otherwise `internal`, invisible across the assembly boundary.
// So a speculative pointer record for an unexported type would be an existence signal for a class no
// consumer can reference (CS0122/CS0246), which is why pointerRecordIsPubliclyRealizable gates on the
// TARGET's exportedness as well as the interface's. See recordSamePackagePointerImplements.
func recordSamePackageImplements(fset *token.FileSet, packageTypes *types.Package, info *types.Info, options Options, globalIdentNames map[*ast.Ident]string, globalScope map[string]*types.Var, convertedFiles []FileEntry) {
	if packageTypes == nil || fset == nil || len(convertedFiles) == 0 {
		return
	}

	convertedPaths := HashSet[string]{}

	for _, entry := range convertedFiles {
		if entry.filePath != "" {
			convertedPaths.Add(strings.ToLower(filepath.Clean(entry.filePath)))
		}
	}

	declaredInConvertedFile := func(obj types.Object) bool {
		position := fset.Position(obj.Pos())

		if !position.IsValid() {
			return false
		}

		return convertedPaths.Contains(strings.ToLower(filepath.Clean(position.Filename)))
	}

	var targets, interfaces []*types.Named

	// Scope names arrive sorted, so the record order — and therefore any state a record claims — is
	// deterministic across runs.
	scope := packageTypes.Scope()

	for _, name := range scope.Names() {
		typeName, ok := scope.Lookup(name).(*types.TypeName)

		if !ok || typeName.IsAlias() || !declaredInConvertedFile(typeName) {
			continue
		}

		named, ok := typeName.Type().(*types.Named)

		if !ok || (named.TypeParams() != nil && named.TypeParams().Len() > 0) {
			continue
		}

		if iface, isInterface := named.Underlying().(*types.Interface); isInterface {
			// A CONSTRAINT interface (one carrying type terms) describes a type SET, not a method
			// set — nothing can implement it at run time, and go2cs-gen emits no interface for it.
			if typeName.Exported() && iface.NumMethods() > 0 && iface.IsMethodSet() {
				interfaces = append(interfaces, named)
			}

			continue
		}

		if _, isSignature := named.Underlying().(*types.Signature); isSignature {
			continue
		}

		targets = append(targets, named)
	}

	if len(targets) == 0 || len(interfaces) == 0 {
		return
	}

	// A scratch visitor: the record path needs the package's type/name resolution, not any one
	// file's emission state. It never visits a file and its output builders stay empty.
	visitor := newFileVisitor(fset, packageTypes, info, options, globalIdentNames, globalScope, map[types.Object]bool{}, newFileEntry(nil, "", false))

	for _, target := range targets {
		// One pointer instance per target: types.Implements and getFullyQualifiedTypeName both read
		// it structurally, so the pair (and therefore the record's ж<T> name) is identical whichever
		// interface it is asked about.
		pointerTarget := types.NewPointer(target)

		for _, ifaceNamed := range interfaces {
			iface, ok := ifaceNamed.Underlying().(*types.Interface)

			if !ok {
				continue
			}

			if types.Implements(target, iface) && generatorCanForwardMethodSet(target, iface, packageTypes) {
				visitor.convertToInterfaceType(ifaceNamed, target, "")
			}

			// The pointer method set INCLUDES the value method set, so a value-satisfied pair reaches
			// here too and is recorded in BOTH forms. That is deliberate: the value record realizes
			// `partial struct T : Iface` (Go's `T` in an interface) and the pointer record realizes
			// `TжIface` (Go's `*T`), which are two distinct dynamic types in Go and must stay two
			// distinct C# identities. Without the pointer record a consumer's `var i Iface = &t` has
			// nothing to reference and mints a local adapter — the very second identity this file
			// exists to eliminate.
			if !types.Implements(pointerTarget, iface) {
				continue
			}

			if !pointerRecordIsPubliclyRealizable(target, ifaceNamed) {
				continue
			}

			// The pointer depth gate's whole justification is that withholding a speculative
			// record is SAFE — "the consumer keeps the local adapter it had before". That is
			// true of an all-exported interface and FALSE of one carrying an unexported method,
			// which is why this arm exists. Go scopes such an interface's implementations to its
			// declaring package, so a consuming assembly cannot even NAME the unexported member
			// to forward it: ImplementGenerator emits that arm as `=> default!` (or an empty
			// body) and the adapter answers a plausible zero forever. The declaring package is
			// the ONLY place the pair can be realized, so here the record is not an optimization
			// and withholding it is not conservative — it is the defect.
			//
			// text/template/parse is the corpus instance. `*BranchNode` implements `Node`, but
			// `Type`/`Position` are promoted from its embedded `NodeType`/`Pos`, so the strict
			// gate withheld the record; `parse` itself never casts a `*BranchNode` (it casts the
			// If/Range/With wrappers), so nothing demanded it either. html/template's escaper
			// takes `&n.BranchNode` and hands it to `join` as a `Node`, minting a local adapter
			// whose `tree()` returns null — and `parse.ErrorContext`'s `if tree == nil` then
			// substitutes its own receiver, which at html/template's `(*parse.Tree)(nil)` call
			// site is also nil, so `~tree` nil-dereferences. One withheld record, one silent
			// stub, one crash three packages away.
			//
			// The relaxation is bounded, not removed: the VALUE form's depth-2 bound still
			// applies, so a promotion deeper than one embed hop is withheld exactly as before.
			// The shape the strict gate was written for — a speculative record whose promoted
			// member resolves through the wrong embedded POINTER hop
			// (StructPointerPromotionWithInterface's `MyCustomError`) — is an all-exported
			// interface and never reaches this arm.
			if !generatorCanForwardPointerMethodSet(pointerTarget, iface, packageTypes) &&
				!(interfaceHasUnexportedMethod(iface) && generatorCanForwardMethodSet(pointerTarget, iface, packageTypes)) {
				continue
			}

			visitor.convertToInterfaceType(ifaceNamed, pointerTarget, "")
		}
	}
}

// pointerRecordIsPubliclyRealizable reports whether a speculative POINTER record would name an
// adapter class a consuming assembly can actually reference.
//
// ImplementGenerator composes the adapter's accessibility from BOTH participants — `public` only when
// the struct and the interface are each public (its adapterScope is the Go-exportedness test in C#
// terms), `internal` otherwise. A record is a cross-assembly contract and the pointer form's contract
// is "this class exists and you may name it": the consuming converter reads the record and emits
// `new pkg.TжIface(x)` with nothing further to ask (implementRecordKey). An internal adapter would
// make that reference CS0122, and an unexported type never reaches a consumer's cast anyway, so the
// record would be pure cost.
//
// This is strictly stronger than the value form's interface-only export gate, and deliberately so:
// the value contract is realized by an implicit conversion that needs no name, so an unexported `T`
// with an exported interface is still consumable there. A pair the source actually CASTS is
// unaffected — a demanded pair still records at its cast site, exported or not, exactly as before.
func pointerRecordIsPubliclyRealizable(target *types.Named, ifaceNamed *types.Named) bool {
	return target.Obj().Exported() && ifaceNamed.Obj().Exported()
}

// generatorCanForwardPointerMethodSet is the realizability gate for the POINTER form, and it is
// STRICTER than the value form's: every interface method must resolve DIRECTLY on the type
// (types.LookupFieldOrMethod index length 1), with no promotion at all.
//
// The value form can afford promotion because ImplementGenerator realizes it as `partial struct T :
// Iface`, whose explicit implementations resolve a promoted member the same way the converter's own
// call sites do. The ж ADAPTER resolves it differently: its promoted-member arms are keyed on
// embedded POINTER fields (GetEmbeddedPointerHopNames), and with exactly ONE such field the single-hop
// arm takes every unbound member UNCONDITIONALLY — "that member's promotion is what type-checked the
// cast, so there is nothing to decide". That reasoning holds for a DEMANDED record, where the cast
// really did type-check through that hop. It does not hold for a SPECULATIVE one, where the member's
// true source may be a different embed entirely.
//
// The corpus instance is StructPointerPromotionWithInterface's `MyCustomError`, which embeds BOTH the
// `Abser` interface and `*MyError`. `Abs` is promoted from the INTERFACE, but the adapter's lone
// pointer embed is `*MyError`, so the forwarder emitted `Abs` against `MyError` — where the only
// candidate in scope was `time.Abs(Duration)`: CS1929, from a generated file, naming `time`. Depth is
// not the discriminator (that promotion is index length 2, which the value bound admits); the KIND of
// hop is, and modelling the generator's exact hop selection here would duplicate its internals in a
// second place — precisely the drift this file's design exists to avoid.
//
// So the bound is deliberately conservative, exactly as the value form's is: withholding a speculative
// record is always safe, because the consumer keeps the local adapter it had before. A pair the source
// actually CASTS is unaffected — it is demanded, records at its cast site, and keeps the promotion
// support the generator was built for (which is what StructPointerPromotionWithInterface guards).
func generatorCanForwardPointerMethodSet(target types.Type, iface *types.Interface, pkg *types.Package) bool {
	for i := range iface.NumMethods() {
		method := iface.Method(i)

		obj, index, _ := types.LookupFieldOrMethod(target, false, pkg, method.Name())

		if obj == nil || len(index) != 1 {
			return false
		}
	}

	return true
}

// generatorCanForwardMethodSet reports whether ImplementGenerator can emit a forwarder for every
// method of iface on target. It is the VALUE form's bound; the pointer form uses the stricter
// generatorCanForwardPointerMethodSet below, because the ж adapter resolves promotion through a
// different mechanism than a partial struct does.
// types.LookupFieldOrMethod returns one index element per embedded-field
// hop plus the final selection, so a directly declared method indexes at length 1 and a
// single-embed promotion at length 2 — the depth the generator's promoted-member arms resolve. A
// deeper path (a value embed of a value embed) forwards through the WRONG hop, so the speculative
// record is withheld rather than emitted as code that cannot compile. Promotion through an embedded
// INTERFACE is the common shape this admits (sort's `reverse` embeds `Interface`; debug/macho's
// segment types embed `LoadBytes`), and it stays admitted at depth 1.
func generatorCanForwardMethodSet(target types.Type, iface *types.Interface, pkg *types.Package) bool {
	for i := range iface.NumMethods() {
		method := iface.Method(i)

		obj, index, _ := types.LookupFieldOrMethod(target, false, pkg, method.Name())

		if obj == nil || len(index) > 2 {
			return false
		}
	}

	return true
}

// interfaceHasUnexportedMethod reports whether iface carries a method Go scopes to its declaring
// package. Such an interface can only ever be implemented THERE (the language rule), which is what
// makes a consumer's locally minted ж adapter unrealizable rather than merely redundant: the member
// is `internal` in the declaring assembly, so ImplementGenerator has nothing to forward to and emits
// a default-answering arm. See the pointer-record gate above for the consequence.
func interfaceHasUnexportedMethod(iface *types.Interface) bool {
	for i := range iface.NumMethods() {
		if !iface.Method(i).Exported() {
			return true
		}
	}

	return false
}
