// inlineArrayLiftChain.go - Gbtc
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
	"os"
	"path/filepath"
	"sort"
	"strings"
)

// AN IMPORTER FOLLOWS ITS IMPORT'S RESOLUTION (W18 ruling C, item 3, shape A; a76c8bf2eb)
//
// An importer reuses the lift an import's code uses for an anonymous struct, so that a value of that Go type crosses
// between the two by value. An import does not always DECLARE that lift: when it reuses one itself, its code names a
// third package's lift, and it publishes no lift of its own. The -recurse importer seed made that common (libc reuses
// libc/uuid/uuid's Timespecᴛ1 once uuid is seeded), and sqlite/lib, which imports libc but not uuid, then found no lift
// in libc and kept its own: 5 reuses lost per linux target. So an import offers the lift its code USES: one it declares
// (its public lift record), or one an exported alias of the identical struct names (its GoTypeAlias record), read at the
// package that declares it.
//
// Termination: the alias record names the type the import's code uses, which is the DECLARING package's lift, because
// the import resolved it by this same rule, one record deep. So one record read reaches a declaration, and that
// declaration is checked in the declaring package's own lift records; nothing is followed further. The declaring
// package is one of the import's transitive imports (the only packages its code can name), and the import graph is
// acyclic, so the chain cannot return to the importer or the import.

// liftUsedBy returns the lift pkg's code uses for st: the package that declares it, its name, and that package's own
// spelling of the struct (its lift record is keyed by it). nil when pkg uses no lift another package can name: it
// declares none publicly and no exported alias of the identical struct names one.
func (v *Visitor) liftUsedBy(pkg *types.Package, st *types.Struct) (*types.Package, string, *types.Struct) {
	meta, ok := importPackageDirs[pkg.Path()]

	if !ok {
		return nil, "", nil
	}

	if name, spelled := declaredPublicLift(pkg, filepath.Clean(meta.Dir), st); name != "" {
		return pkg, name, spelled
	}

	return v.namedLift(pkg, meta, st)
}

// declaredPublicLift returns the public lift pkg declares for st and pkg's spelling of the struct, or "". pkg's record
// is keyed by pkg's spelling, which can differ from this package's (W18): a record found by this package's spelling must
// still be a struct pkg declares identically, and otherwise each identical struct pkg declares is tried by its own
// spelling, in pkg's scope order.
func declaredPublicLift(pkg *types.Package, key string, st *types.Struct) (string, *types.Struct) {
	publicLift := func(spelling string) string {
		packageLock.Lock()
		defer packageLock.Unlock()

		if name := importedDynamicTypeNames[key][spelling]; name != "" && importedPublicLiftNames[key][name] {
			return name
		}

		return ""
	}

	if name := publicLift(st.String()); name != "" {
		if packageDeclaresIdenticalStruct(pkg, st) {
			return name, st
		}

		return "", nil
	}

	for _, identical := range packageIdenticalStructs(pkg, st) {
		if name := publicLift(identical.String()); name != "" {
			return name, identical
		}
	}

	return "", nil
}

// namedLift returns the lift pkg's code names from another package for st: the first exported alias of pkg (by name)
// whose target is identical to st and whose GoTypeAlias record names a lift that one of pkg's transitive imports
// declares publicly for the identical struct.
func (v *Visitor) namedLift(pkg *types.Package, meta importedPackageMeta, st *types.Struct) (*types.Package, string, *types.Struct) {
	scope := pkg.Scope()
	names := scope.Names()
	sort.Strings(names)

	for _, aliasName := range names {
		obj, ok := scope.Lookup(aliasName).(*types.TypeName)

		if !ok || !obj.IsAlias() || !obj.Exported() {
			continue
		}

		if target, ok := types.Unalias(obj.Type()).(*types.Struct); !ok || !types.Identical(target, st) {
			continue
		}

		recordKey := filepath.Clean(meta.Dir) + "\x00" + getSanitizedIdentifier(meta.Name) + "." + getCoreSanitizedIdentifier(aliasName)

		packageLock.Lock()
		target, recorded := importedTypeAliasTargetsByDir[recordKey]
		packageLock.Unlock()

		if !recorded {
			continue
		}

		owner, name := liftDeclaringPackage(pkg, target)

		if owner == nil || owner == pkg {
			continue
		}

		ownerMeta, ok := importPackageDirs[owner.Path()]

		if !ok || !v.loadImportLiftRecords(owner, ownerMeta) {
			continue
		}

		if declared, spelled := declaredPublicLift(owner, filepath.Clean(ownerMeta.Dir), st); declared == name {
			return owner, name, spelled
		}
	}

	return nil, "", nil
}

// liftDeclaringPackage returns the transitive import of pkg whose lift the alias record target names, and the lift's
// name: the target is that package's class, qualified as importedStructLift qualifies a reused lift, then the name.
func liftDeclaringPackage(pkg *types.Package, target string) (*types.Package, string) {
	dot := strings.LastIndex(target, ".")

	if dot < 0 {
		return nil, ""
	}

	name := target[dot+1:]
	seen := map[*types.Package]bool{}
	var found *types.Package
	var visit func(p *types.Package)

	visit = func(p *types.Package) {
		for _, imported := range p.Imports() {
			if found != nil || seen[imported] {
				continue
			}

			seen[imported] = true

			if code := liftCodeName(imported, name); code == target || RootNamespace+"."+code == target {
				found = imported
				return
			}

			visit(imported)
		}
	}

	visit(pkg)

	if found == nil {
		return nil, ""
	}

	return found, name
}

// liftCodeName is the code-body name importedStructLift gives pkg's lift name.
func liftCodeName(pkg *types.Package, name string) string {
	return convertToCSTypeName(getSanitizedImport(packageClassPath(pkg.Path(), pkg.Name())+PackageSuffix) + "." + name)
}

// loadImportLiftRecords loads the lift records of pkg, a transitive import this package does not import directly (a
// direct import's records are loaded with its aliases), from wherever pkg's records live (importPackageInfoLines). It
// reports whether pkg's records are loaded. Only the lift records are read: pkg's aliases are not this package's to use.
func (v *Visitor) loadImportLiftRecords(pkg *types.Package, meta importedPackageMeta) bool {
	key := filepath.Clean(meta.Dir)

	// One lock for the check and the mark, so two goroutines cannot both read the records (review 2.4).
	packageLock.Lock()
	_, loaded := importedDynamicTypeNames[key]
	tried := liftRecordsRead[key]

	if !loaded && !tried {
		if liftRecordsRead == nil {
			liftRecordsRead = map[string]bool{}
		}

		liftRecordsRead[key] = true
	}

	packageLock.Unlock()

	if loaded || tried {
		return loaded
	}

	info, ok := getImportPackageInfo([]string{pkg.Path()}, v.options)[pkg.Path()]

	if !ok || info.Err != nil {
		return false
	}

	// The one reader that knows every source of a package's records: a NuGet-substituted package's self-description, the
	// package_info.cs in the root, a published std package's embedded record (review 2.1).
	lines, ok := importPackageInfoLines(info, v.options)

	if !ok {
		return false
	}

	loadImportedDynamicTypeLiftLines(lines, meta.Dir)

	packageLock.Lock()
	_, loaded = importedDynamicTypeNames[key]
	packageLock.Unlock()

	return loaded
}

// traceLiftChain writes one line per lift taken through an import that names it from the declaring package, under
// GO2CS_TRACE_LIFT_REUSE=1: the importer, the import it followed, and the lift.
func traceLiftChain(importer, through, owner *types.Package, name string) {
	if os.Getenv(liftReuseTraceVariable) != "1" || importer == nil {
		return
	}

	fmt.Fprintf(os.Stderr, "liftchain\t%s\t%s\t%s.%s\n", importer.Path(), through.Path(), owner.Path(), name)
}
