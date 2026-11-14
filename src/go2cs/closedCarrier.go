// closedCarrier.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/types"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// A generic type of the converting package can satisfy a NON-generic interface for ONE instantiation only, when a
// method the interface requires names the type's own type parameter:
//
//	type box[T any] struct{ v T }
//	func (b box[T]) val() T { return b.v }
//	type valuer interface{ val() point }
//	var v valuer = box[point]{}          // box[int] is not a valuer
//
// go2cs-gen writes a same-package generic's interface implementation over the type's OPEN definition
// (`partial struct box<T> : valuer`, or the generic adapter `boxжvaluer<T>`), where `val()` returns T and the
// interface wants `point` (CS0029). Such a pair takes a CLOSED CARRIER instead: an adapter class written for the one
// instantiation, `boxᴠvaluer` holding a `box<point>` for the value form and `boxжvaluer` holding its box for the
// pointer form. The record carries `Closed = true`, which is what go2cs-gen reads, and the conversion site wraps.
//
// The closed carrier is named from the type's BARE name, so one (type, interface) pair has room for one closed
// instantiation in a package. A second one (`pair[point, int]` and `pair[point, string]` to one interface) is
// refused by warning. Two more shapes are refused by the same warning, and keep the emission they had:
//
//   - the instantiation as a TYPE ARGUMENT whose constraint is the interface (`use[V valuer]` called with
//     `box[point]`): C# checks the constraint nominally, and `box<point>` does not implement `valuer`;
//   - a method the interface requires that the type PROMOTES from an embedded field while it names the type
//     parameter (`outer[T]{box[T]}`): the carrier forwards to members the type declares itself.

// closedCarrierRoute is closedCarrierRouteOf's answer for one (type, interface) pair.
type closedCarrierRoute int

const (
	// closedCarrierNone: the pair keeps the route it has; nothing here applies to it.
	closedCarrierNone closedCarrierRoute = iota

	// closedCarrierClosed: the pair is carried by a closed adapter class.
	closedCarrierClosed

	// closedCarrierRefused: the pair needs a closed carrier, and one of its required methods is promoted.
	closedCarrierRefused
)

// closedCarrierRouteOf decides whether converting `targetType` (a value or a pointer) to `interfaceType` takes a
// closed carrier. It answers closedCarrierNone, which leaves the emission exactly as it was, unless ALL of these hold:
// the target is an instantiation of a generic type `pkg` declares, with concrete type arguments; the interface is a
// named, non-generic interface; the generic definition has every method the interface requires; and at least one of
// those methods names a type parameter of the definition in its parameters or results. The signature read is the
// SELECTION's, so a method promoted from an embedded `box[T]` is seen with the outer type's parameter in it.
func closedCarrierRouteOf(targetType, interfaceType types.Type, pkg *types.Package) closedCarrierRoute {
	if targetType == nil || interfaceType == nil || pkg == nil {
		return closedCarrierNone
	}

	if ptr, ok := types.Unalias(targetType).(*types.Pointer); ok {
		targetType = ptr.Elem()
	}

	named, ok := types.Unalias(targetType).(*types.Named)

	if !ok || named.TypeArgs().Len() == 0 || named.Obj().Pkg() != pkg {
		return closedCarrierNone
	}

	// An instantiation over a type parameter in scope (`box[T]` inside a generic function) is an open target.
	for i := range named.TypeArgs().Len() {
		if mentionsTypeParam(named.TypeArgs().At(i), map[types.Type]bool{}) {
			return closedCarrierNone
		}
	}

	ifaceNamed, ok := types.Unalias(interfaceType).(*types.Named)

	if !ok || ifaceNamed.TypeArgs().Len() > 0 || ifaceNamed.TypeParams().Len() > 0 {
		return closedCarrierNone
	}

	iface, ok := ifaceNamed.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 {
		return closedCarrierNone
	}

	methods := types.NewMethodSet(types.NewPointer(named.Origin()))
	namesTypeParam, promoted := false, false

	for i := range iface.NumMethods() {
		required := iface.Method(i)
		selection := methods.Lookup(required.Pkg(), required.Name())

		if selection == nil {
			return closedCarrierNone
		}

		signature, ok := selection.Obj().Type().(*types.Signature)

		if !ok {
			return closedCarrierNone
		}

		if len(selection.Index()) != 1 {
			promoted = true
		}

		if mentionsTypeParam(signature, map[types.Type]bool{}) {
			namesTypeParam = true
		}
	}

	switch {
	case !namesTypeParam:
		return closedCarrierNone
	case promoted:
		return closedCarrierRefused
	default:
		return closedCarrierClosed
	}
}

// closedCarrierBareName drops a C# type reference's type-argument list: `box<main_package.point>` is `box`. An
// adapter class is named from the bare name, and a type argument's own qualifier must not be read as the type's.
func closedCarrierBareName(typeName string) string {
	if idx := strings.Index(typeName, "<"); idx >= 0 {
		return typeName[:idx]
	}

	return typeName
}

// closedCarrierRecordArgument is the named argument a closed pair's GoImplement record carries, after `Pointer = true`
// for the pointer form. importOperations.go's record patterns do NOT match a record that carries it, and that is the
// rule rather than an accident: an importing package must not read the record as "the type implements the interface"
// (it does not, in C#), so it records and generates a carrier of its own.
const closedCarrierRecordArgument = "Closed = true"

// closedCarrierPairKey is closedCarrierImplementations' key for a recorded pair.
func closedCarrierPairKey(interfaceTypeName, recordName string) string {
	return interfaceTypeName + "|" + recordName
}

// warnClosedCarrierPromoted reports, once per file and pair, a conversion whose closed carrier would have to forward
// a promoted method. The slot conversion has no node position, so the diagnostic names the file.
func (v *Visitor) warnClosedCarrierPromoted(targetType, interfaceType types.Type) {
	qualifier := types.RelativeTo(v.pkg)
	fileName := ""

	if v.file != nil {
		fileName = v.file.Name()
	}

	text := fmt.Sprintf("%s to %s needs a closed carrier, and %s promotes a method %s requires that names its type parameter; "+
		"the carrier forwards only methods the type declares itself, so this conversion is not carried and the C# build fails on it",
		types.TypeString(targetType, qualifier), types.TypeString(interfaceType, qualifier),
		closedCarrierDefinitionName(targetType, qualifier), types.TypeString(interfaceType, qualifier))

	packageLock.Lock()
	seen := closedCarrierWarnings.Contains(fileName + "|" + text)
	closedCarrierWarnings.Add(fileName + "|" + text)
	packageLock.Unlock()

	if !seen {
		reportWarningDiagnostic(fileName, diagnosticClosedCarrierRefused, text)
	}
}

// closedCarrierDefinitionName names the generic definition behind a value or pointer target.
func closedCarrierDefinitionName(targetType types.Type, qualifier types.Qualifier) string {
	if ptr, ok := types.Unalias(targetType).(*types.Pointer); ok {
		targetType = ptr.Elem()
	}

	if named, ok := types.Unalias(targetType).(*types.Named); ok {
		return named.Obj().Name()
	}

	return types.TypeString(targetType, qualifier)
}

// warnClosedCarrierTypeArguments reports every instantiation in the package whose type argument meets its constraint
// only as a closed pair. An adapter serves an interface conversion and never a nominal C# constraint, so no carrier
// can stand in that slot; the conversion goes on and the C# build fails on the site this names.
func warnClosedCarrierTypeArguments(pkg *packages.Package) {
	if pkg == nil || pkg.TypesInfo == nil || pkg.Types == nil {
		return
	}

	type site struct {
		position string
		offset   int
		text     string
	}

	var sites []site
	qualifier := types.RelativeTo(pkg.Types)

	for ident, instance := range pkg.TypesInfo.Instances {
		typeParams := genericTypeParams(pkg.TypesInfo.Uses[ident], instance.Type)

		if typeParams == nil || instance.TypeArgs == nil {
			continue
		}

		for i := range min(typeParams.Len(), instance.TypeArgs.Len()) {
			typeArg, constraint := instance.TypeArgs.At(i), typeParams.At(i).Constraint()

			if closedCarrierRouteOf(typeArg, constraint, pkg.Types) == closedCarrierNone {
				continue
			}

			position := pkg.Fset.Position(ident.Pos())

			sites = append(sites, site{
				position: position.String(),
				offset:   position.Offset,
				text: fmt.Sprintf("%s takes %s as a type argument constrained by %s, which it satisfies for this instantiation only, through a closed carrier; "+
					"a closed carrier cannot stand in a type-argument slot, so the C# constraint is not met and the C# build fails on it",
					ident.Name, types.TypeString(typeArg, qualifier), types.TypeString(constraint, qualifier)),
			})
		}
	}

	sort.Slice(sites, func(i, j int) bool {
		if sites[i].position != sites[j].position {
			return sites[i].position < sites[j].position
		}

		return sites[i].text < sites[j].text
	})

	for i, s := range sites {
		if i > 0 && sites[i-1] == s {
			continue
		}

		reportWarningDiagnostic(s.position, diagnosticClosedCarrierRefused, s.text)
	}
}

// warnClosedCarrierCollisions reports, at package-info write time, every closed pair whose carrier name another
// closed instantiation of the same generic type and interface already took. `records` are closedCarrierImplementations'
// keys, `interface|record`. Both records are still written, so the C# build fails on the duplicate class as well.
func warnClosedCarrierCollisions(records []string) {
	groups := map[string][]string{}

	for _, record := range records {
		interfaceName, implementation, ok := strings.Cut(record, "|")

		if !ok {
			continue
		}

		form := "value"

		if inner, isPointer := strings.CutPrefix(implementation, PointerPrefix+"<"); isPointer {
			implementation = strings.TrimSuffix(inner, ">")
			form = "pointer"
		}

		key := form + "|" + closedCarrierBareName(implementation) + "|" + interfaceName
		groups[key] = append(groups[key], implementation)
	}

	keys := make([]string, 0, len(groups))

	for key, members := range groups {
		if len(members) > 1 {
			keys = append(keys, key)
		}
	}

	sort.Strings(keys)

	for _, key := range keys {
		members := groups[key]
		sort.Strings(members)
		parts := strings.SplitN(key, "|", 3)

		reportWarningDiagnostic("", diagnosticClosedCarrierRefused, fmt.Sprintf(
			"%s each need a closed carrier to %s (%s form), and the carrier is named from the generic type alone, so a package has room for one; "+
				"the C# build fails on the duplicate class", strings.Join(members, " and "), parts[2], parts[0]))
	}
}
