// liftedTypeNames.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the naming of LIFTED types — the generated C# names given to Go types that have
// no name of their own (anonymous structs and interfaces) or whose own name cannot be used as-is.
//
// The whole problem is uniqueness under concurrency. A package's files are visited in parallel,
// each may need to lift a type, and two files must never claim the same name for different types
// nor different names for the same one. So a name is CLAIMED (claimLiftedTypeName) under the
// package lock rather than merely generated, and the taken-name check spans the whole package
// rather than the current file.
//
// Deciding a name is separate from RESOLVING a reference to one another file claimed — that is
// dynamicTypeOperations.go's deferred-marker pass.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

func (v *Visitor) typeExists(name string) bool {
	// Look in the package scope
	obj := v.pkg.Scope().Lookup(name)

	if obj != nil && (obj.Type() != nil || obj.Type().Underlying() != nil) {
		return true
	}

	// Or search through all definitions
	for _, obj := range v.info.Defs {
		if obj != nil && obj.Name() == name && (obj.Type() != nil || obj.Type().Underlying() != nil) {
			return true
		}
	}

	return false
}

func getGlobalTempVarName(varPrefix string) string {
	if globalTempVarCount == nil {
		globalTempVarCount = make(map[string]int)
	}

	count := globalTempVarCount[varPrefix]
	count++
	globalTempVarCount[varPrefix] = count

	return fmt.Sprintf("%s%s%d", varPrefix, TempVarMarker, count)
}

func (v *Visitor) getUniqueLiftedTypeName(typeName string) string {
	// Recover the original Go name by stripping BOTH sanitization markers ('@' and the Δ collision
	// rename) so the typeExists check below hits the real package scope (which holds the unsanitized
	// name). The lift is often called with the already-sanitized name (e.g. `Δtrace`).
	//
	// The '@' strip is stripSanitizationMarkers (EVERY marker) rather than the leading-only form,
	// and that is load-bearing: every caller COMPOSES `<enclosing>_<name>` before arriving here, so a
	// keyword-escaped component lands mid-identifier where '@' is illegal C# — it is legal only as an
	// identifier's FIRST character. `visitValueSpec` hands the lift its csIDName (the ESCAPED form,
	// `getSanitizedIdentifier(goIDName)`), so `var params struct{…}` inside `vgetrandomInit` composed
	// `vgetrandomInit_@params` and emitted uncompilable C# (CS1513/CS1514/CS1519). Stripping only the
	// LEADING marker could not see it. Re-sanitizing the stripped result below restores the escape
	// when the WHOLE composed identifier is itself a keyword, which is the only case that needs one.
	//
	// Measured 2026-09-07: 3 occurrences in the Go 1.24.13 emission (runtime/vgetrandom_linux.go,
	// a file absent from 1.23.12) and ZERO in the 1.23.12 corpus — this is latent here and reachable
	// TODAY through -recurse on end-user Go, since `params`, `ref`, `out` and `fixed` are all
	// ordinary Go identifiers. All five lift callers funnel through this one helper, so the fix sits
	// where the class is closed rather than at the composition sites, which are individually blameless.
	originalName := strings.TrimPrefix(stripSanitizationMarkers(typeName), ShadowVarMarker)
	typeName = getSanitizedIdentifier(originalName)
	uniqueTypeName := typeName
	count := 0

	// typeExists looks names up in the Go package scope, which holds UNSANITIZED names. A lifted type
	// named after a global var that is Δ-renamed (e.g. a `var trace struct{…}` whose anon type lifts
	// to `trace`→`Δtrace`) would check the sanitized `Δtrace`, miss the `trace` var, and collide with
	// it (a nested type + a property both named `Δtrace`, CS0102). Also test the original name so the
	// first iteration forces a `ᴛ1` suffix in that case.
	for v.liftedTypeNameTaken(uniqueTypeName) || v.typeExists(uniqueTypeName) || (count == 0 && v.typeExists(originalName)) {
		count++
		uniqueTypeName = fmt.Sprintf("%s%s%d", typeName, TempVarMarker, count)
	}

	v.claimLiftedTypeName(uniqueTypeName)

	return uniqueTypeName
}

// liftedTypeNameTaken reports whether a lifted type name is already claimed. The scope is the whole
// PACKAGE — every lifted type nests in the one `<pkg>_package` class (see packageLiftedTypeNames) —
// plus, on the `-tests` internal variant, the production names that class already declares on disk.
// A hand-owned file's visitor sees only its own per-file claims: its emission lands in a
// non-compiled `.cs.auto` sibling, so it neither collides with nor constrains the real declarations.
func (v *Visitor) liftedTypeNameTaken(name string) bool {
	if v.liftedTypeNames.Contains(name) {
		return true
	}

	if v.manualConversion {
		return false
	}

	packageLock.Lock()
	defer packageLock.Unlock()

	return packageLiftedTypeNames.Contains(name) || productionLiftedTypeNames.Contains(name)
}

// liftedNameFor resolves a type to the C# name it was LIFTED under: this visitor's own per-file
// claim first, then the PRODUCTION conversion's package-scope ALIAS lifts.
//
// The second source exists for the `-tests` REFERENCE model alone. `type CorpusEntry =
// struct{Parent string; Path string; …}` (internal/fuzz) is lifted in production to a real nested
// type and reached through a compilation-scoped `global using CorpusEntry = …CorpusEntryᴛ1;`. A
// reference-model test project does not recompile the production sources, so nothing visits that
// declaration and nothing claims the lift — every test-side reference to the alias then fell
// through to `t.String()` and emitted RAW GO SYNTAX into a C# file
// (`Func<struct{Parent string; …}, error>`: CS1031/CS1525/CS1003 cascades in minimize_test.cs and
// worker_test.cs, with all 52 of that package's verdicts behind it). The map is seeded from the
// production package's own PUBLISHED aliases, alongside the `global using` that makes the name
// resolvable in the test compilation — see seedProductionAliasLifts. Empty for a production
// conversion, so this is a pure no-op there.
func (v *Visitor) liftedNameFor(t types.Type) (string, bool) {
	if name, ok := v.liftedTypeMap[t]; ok {
		return name, true
	}

	if len(productionAliasLiftedTypes) == 0 {
		return "", false
	}

	packageLock.Lock()
	name, ok := productionAliasLiftedTypes[t]
	packageLock.Unlock()

	return name, ok
}

// claimLiftedTypeName records a lifted type name against this file and — unless the file is
// hand-owned — against the package (see liftedTypeNameTaken).
func (v *Visitor) claimLiftedTypeName(name string) {
	v.liftedTypeNames.Add(name)

	if v.manualConversion {
		return
	}

	packageLock.Lock()

	if packageLiftedTypeNames == nil {
		packageLiftedTypeNames = HashSet[string]{}
	}

	packageLiftedTypeNames.Add(name)
	packageLock.Unlock()
}

func (v *Visitor) liftedTypeExists(expr ast.Expr) bool {
	if expr == nil {
		return false
	}

	exprType := v.getType(expr, false)

	if exprType == nil {
		return false
	}

	if _, ok := v.liftedTypeMap[exprType]; ok {
		return true
	}

	if named, ok := exprType.(*types.Named); ok {
		if _, ok := v.liftedTypeMap[named.Underlying()]; ok {
			return true
		}
	}

	return false
}
