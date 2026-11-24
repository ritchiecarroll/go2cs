// inlineArrayImporterSeed.go - Gbtc
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
	"maps"
	"os"
	"slices"
	"sort"
	"strings"

	"golang.org/x/tools/go/packages"
)

// THE -recurse IMPORTER SEED (W18 ruling C, item 3, scope b; e190d6da30)
//
// An importer that admits an anonymous struct identical to an alias a direct import declares, while the import does not
// admit it, keeps its own lift (W18 ruling A), and a value of that Go type cannot cross between the two packages by value.
// Ruling C's -inline-arrays=<import>#<alias> seed makes the import admit the alias, so the two agree on one lift. In a
// -recurse run the converter has the whole closure before it converts any package, so it derives those seeds itself.
//
// Admission reads the stamps of the packages a struct's closure reaches (foreignAdmits), while seeds flow from
// importers to imports, so the derivation is a fixed point over the closure: compute every package's admission in
// dependency order against IN-MEMORY stamps of its convert-set imports (any other import's, std's, is read where the
// conversion reads it), collect the demands, add them as seeds, and repeat until the
// seed set stops growing. It only grows, so it ends. The conversion then runs as before, each package with its derived
// seeds beside the flag's (a union: a type named by both is one seed), and records them as GoInlineArraySeed, so a
// -tests run repeats them. "-<pattern>" turns a package's admission off, which beats any seed; a forced package admits
// every candidate and needs none.
//
// Scope (b) only: a FOREIGN NAMED struct an importer converts whole stays its declaring package's decision (ruling C,
// item 3, scope a, a separate .5 row).

// importerSeedMaxPasses bounds the fixed point; each pass that changes anything adds at least one seed, and the
// closure's alias count bounds those, so this is a guard against a defect, not a limit a real closure reaches. A
// variable only so an arm can lower it.
var importerSeedMaxPasses = 16

// importerInlineArraySeeds derives the importer seeds for the convert-set queue (dependency order) from the closure's
// syntax and types: import path -> the alias names to seed, sorted. A package the closure did not load with syntax (a
// -recurse=module dependency) neither demands nor receives a seed.
func importerInlineArraySeeds(queue []string, closure map[string]*packages.Package, options Options) map[string][]string {
	inSet := make(map[string]bool, len(queue))

	for _, path := range queue {
		inSet[path] = true
	}

	derived := map[string]map[string]bool{}
	converged := false
	var lastAdded []string

	for pass := 0; pass < importerSeedMaxPasses; pass++ {
		added := importerSeedPass(pass, queue, closure, options, inSet, derived)

		if len(added) == 0 {
			converged = true
			break
		}

		lastAdded = added
	}

	// The cap ended the loop while a pass was still adding seeds: these are not the fixed point, and any demand a
	// further pass would make is dropped (review 2.2). Said by name rather than used silently.
	if !converged && len(lastAdded) > 0 {
		showWarning("@importerSeeds - the -recurse importer-seed derivation stopped at its cap of %d passes while pass %d was still adding seeds (%s); the seeds are not the fixed point, and any demand a further pass would make is dropped",
			importerSeedMaxPasses, importerSeedMaxPasses, strings.Join(lastAdded, ", "))
	}

	seeds := make(map[string][]string, len(derived))

	for path, names := range derived {
		seeds[path] = slices.Sorted(maps.Keys(names))
	}

	return seeds
}

// importerSeedPass computes every convert-set package's admission once, in dependency order, against this pass's
// in-memory stamps of the convert-set (and the conversion's own reader for any other import), adds each new demand to
// derived, and returns the seeds it added as path#name. The stamp lookup is swapped for the pass and restored by a
// deferred call, so a panic inside an admission cannot leave the pass's closure in the global (review 2.3).
func importerSeedPass(pass int, queue []string, closure map[string]*packages.Package, options Options, inSet map[string]bool, derived map[string]map[string]bool) []string {
	stamps := map[string]inlineArrayStamp{}
	var added []string

	restore := lookupInlineArrayStamp
	defer func() { lookupInlineArrayStamp = restore }()

	// A convert-set package's stamp is the one this pass computed (its record on disk, if any, is stale); any other
	// import's (std's) is read where the conversion reads it.
	lookupInlineArrayStamp = func(importPath string, options Options) (inlineArrayStamp, bool) {
		if inSet[importPath] {
			stamp, ok := stamps[importPath]
			return stamp, ok
		}

		return restore(importPath, options)
	}

	for _, path := range queue {
		pkg := closure[path]

		if pkg == nil || pkg.Types == nil || pkg.TypesInfo == nil || len(pkg.Syntax) == 0 {
			continue
		}

		packageOptions := options
		packageOptions.inlineArrays = withImporterSeeds(options.inlineArrays, path, derived[path])
		admission := computeInlineArrayAdmission(pkg.Syntax, pkg.Fset, pkg.Types, pkg.TypesInfo, pkg.Dir, packageOptions)
		stamps[path] = inlineArrayStampOf(admission, inlineArrayMode(path, pkg.Dir, packageOptions))

		for _, demand := range importerSeedDemands(admission, pkg.Types, inSet, stamps) {
			if derived[demand.pkgPath] == nil {
				derived[demand.pkgPath] = map[string]bool{}
			}

			if !derived[demand.pkgPath][demand.name] {
				derived[demand.pkgPath][demand.name] = true
				added = append(added, demand.pkgPath+"#"+demand.name)
				traceImporterSeed(pass+1, path, demand)
			}
		}
	}

	return added
}

// importerSeedDemand is one alias an importer needs its declaring package to admit.
type importerSeedDemand struct {
	pkgPath string
	name    string
}

// importerSeedDemands returns, for each anonymous struct the admission admits, the alias of the first direct import
// (in import-path order, importedStructLift's order) in the convert-set that declares an identical struct and whose
// stamp does not admit it.
func importerSeedDemands(admission *inlineArrayAdmission, pkg *types.Package, inSet map[string]bool, stamps map[string]inlineArrayStamp) []importerSeedDemand {
	if admission == nil {
		return nil
	}

	imports := slices.Clone(pkg.Imports())
	slices.SortFunc(imports, func(a, b *types.Package) int { return strings.Compare(a.Path(), b.Path()) })

	var demands []importerSeedDemand

	for _, key := range admission.keyOrder {
		if !strings.HasPrefix(key, "anon:") {
			continue
		}

		st, ok := admission.typeOf(key)

		if !ok {
			continue
		}

		for _, imported := range imports {
			if !inSet[imported.Path()] {
				continue
			}

			name, spelled := identicalAliasIn(imported, st)

			if name == "" {
				continue
			}

			stamp, known := stamps[imported.Path()]
			importKey, _ := inlineTypeKey(spelled)

			if admitted, decided := stamp.admitsAnonymous(importKey); known && decided && admitted {
				break
			}

			demands = append(demands, importerSeedDemand{imported.Path(), name})
			break
		}
	}

	return demands
}

// identicalAliasIn returns the first package-level alias (by name) of pkg whose target is a struct identical to st, and
// that struct as pkg spells it; "" when pkg declares none.
func identicalAliasIn(pkg *types.Package, st *types.Struct) (string, *types.Struct) {
	scope := pkg.Scope()
	names := scope.Names()
	sort.Strings(names)

	for _, name := range names {
		obj, ok := scope.Lookup(name).(*types.TypeName)

		if !ok || !obj.IsAlias() {
			continue
		}

		if target, ok := types.Unalias(obj.Type()).(*types.Struct); ok && types.Identical(target, st) {
			return name, target
		}
	}

	return "", nil
}

// withImporterSeeds returns override with the derived seeds for pkgPath added to its own (the flag's), as a copy.
func withImporterSeeds(override inlineArrayOverride, pkgPath string, derived map[string]bool) inlineArrayOverride {
	if len(derived) == 0 {
		return override
	}

	seeds := make(map[string][]string, len(override.seeds)+1)

	for path, names := range override.seeds {
		seeds[path] = slices.Clone(names)
	}

	for name := range derived {
		if !slices.Contains(seeds[pkgPath], name) {
			seeds[pkgPath] = append(seeds[pkgPath], name)
		}
	}

	slices.Sort(seeds[pkgPath])
	override.seeds = seeds

	return override
}

// inlineArrayStampOf is the record a package's admission would write (inlineArrayStampRecords), as its readers parse it.
func inlineArrayStampOf(a *inlineArrayAdmission, mode string) inlineArrayStamp {
	stamp := inlineArrayStamp{mode: mode, types: a.namedTypes(), anonymous: a.anonymousDigests()}

	if stamp.mode == "" {
		stamp.mode = inlineArraysModeAuto
	}

	if a != nil {
		stamp.keyCount = len(a.keyOrder)
		stamp.keyDigest = inlineArrayKeyDigest(a.keyOrder)
		stamp.seeds = slices.Clone(a.seeded)
	}

	if stamp.anonymous == nil {
		stamp.anonymous = []string{}
	}

	return stamp
}

// traceImporterSeed writes one line per derived seed to stderr when GO2CS_TRACE_LIFT_REUSE=1 (liftReuseTraceVariable):
// the pass that found it, the importer that demanded it, and the seed. The census reads the seeds and the pass count.
func traceImporterSeed(pass int, importer string, demand importerSeedDemand) {
	if os.Getenv(liftReuseTraceVariable) != "1" {
		return
	}

	fmt.Fprintf(os.Stderr, "importerseed\tpass=%d\t%s\t%s#%s\n", pass, importer, demand.pkgPath, demand.name)
}
