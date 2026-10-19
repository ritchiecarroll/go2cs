// foreignConstraintProxy.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/types"
	"os"
	"regexp"
	"strings"
)

// A self-referential constraint over a FOREIGN interface (`lib.Ord2[*elem.Node]` under `Ord2[T Lesser[T]]`, from a
// package that declares neither) takes ONE proxy, because the proxy is the type argument and so part of the
// instantiated type's identity: every importer must name the same one, and none may declare its own.
//
//  1. A CONCRETE proxy the interface's package published for this element (`ecdh.P224PointжPoint`) wins. That package
//     records the pair where it closes the constraint itself, and every instantiator imports it, so every importer reads
//     the same record. The element's package cannot publish one for a foreign interface: it would be a consumer-local
//     proxy, which this rule exists to forbid.
//  2. Otherwise the interface package's GENERIC proxy closed over the element (`lib.жLesser<elem.Node>`), which
//     go2cs-gen publishes beside every self-referential constraint interface, and THIS package records a bind:
//     go2cs-gen emits a module initializer binding the proxy's slots to the element's Go methods.

// publishedConstraintProxyPattern matches a concrete constraint-proxy record. The interface side of a CONCRETE record
// is the publishing package's own interface, so it is bare (`Point<…>`); a qualified one is a bind record (rule 2).
var publishedConstraintProxyPattern = regexp.MustCompile(`^\[assembly: GoImplement<(.+?), ([^<>.]+)<.+>>\(ConstraintProxy = true\)\]$`)

// publishedConstraintProxyElements returns the element of every concrete constraint proxy named proxyName that the
// package pkg publishes, as its records spell them. Read from whichever source the conversion has for the package
// (importPackageInfoLines), once per package.
func (v *Visitor) publishedConstraintProxyElements(pkg *types.Package, proxyName string) []string {
	packageLock.Lock()
	proxies, loaded := importedConstraintProxies[pkg.Path()]
	packageLock.Unlock()

	if !loaded {
		proxies = make(map[string][]string)

		if info, ok := getImportPackageInfo([]string{pkg.Path()}, v.options)[pkg.Path()]; ok && info.Err == nil {
			if lines, ok := importPackageInfoLines(info, v.options); ok {
				for _, line := range lines {
					if matches := publishedConstraintProxyPattern.FindStringSubmatch(strings.TrimSpace(line)); matches != nil {
						name := simpleCSTypeName(matches[1]) + PointerPrefix + matches[2]
						proxies[name] = append(proxies[name], matches[1])
					}
				}
			}
		}

		packageLock.Lock()
		importedConstraintProxies[pkg.Path()] = proxies
		packageLock.Unlock()
	}

	return proxies[proxyName]
}

// importPackageInfoLines reads an imported package's package_info.cs records from the source loadImportedTypeAliases
// would read them from: a NuGet substitution's self-description, the converted file on disk, or the converter's
// embedded record of a published standard-library package. Reading here applies nothing, so a package this file reaches
// only through a third one is read without importing its aliases.
func importPackageInfoLines(info PackageInfo, options Options) ([]string, bool) {
	goos := goosOfTarget(options.targetPlatform)

	if info.NuGetSubstitution != nil {
		return info.NuGetSubstitution.metadataLines(info.ImportPath, goos)
	}

	packageInfoFile := platformPackageInfoPath(info.TargetDir, goos)

	if _, err := os.Stat(packageInfoFile); err == nil {
		lines, err := readPackageInfoLines(packageInfoFile)
		return lines, err == nil
	}

	if info.PublishedStdLib {
		return stdLibExportedMetadata(info.PackageName, goos)
	}

	return nil, false
}

// foreignConstraintProxy names the proxy for a self-referential constraint over the FOREIGN interface interfaceOrigin at
// the pointer to elemNamed, by the rule above. qualifier is the interface package's C# qualifier as this file spells it
// (`lib.`), or an imported type alias's (`ecdhꓸ`) when aliasQualifier is set. Returns false, refusing by name, when the
// interface's package published two concrete proxies of the one name.
func (v *Visitor) foreignConstraintProxy(interfaceOrigin, elemNamed *types.Named, proxyName, qualifier string, aliasQualifier bool) (string, bool) {
	switch published := v.publishedConstraintProxyElements(interfaceOrigin.Obj().Pkg(), proxyName); len(published) {
	case 0:
	case 1:
		return qualifier + proxyName, true
	default:
		v.showWarning("@constraintProxyFor - package %s publishes %d constraint proxies named `%s` (%s); refusing to choose one, the box is left in place (emission will not compile)",
			interfaceOrigin.Obj().Pkg().Path(), len(published), proxyName, strings.Join(published, ", "))
		return "", false
	}

	elementFullName := v.getFullyQualifiedTypeName(elemNamed, false)

	if elemNamed.Obj().Pkg() != v.pkg {
		elementFullName = convertToCSFullTypeName(elementFullName)
	}

	interfaceFullName := v.getFullyQualifiedTypeName(interfaceOrigin, false)

	if idx := strings.Index(interfaceFullName, "["); idx >= 0 {
		interfaceFullName = interfaceFullName[:idx]
	}

	interfaceFullName = convertToCSFullTypeName(interfaceFullName)
	genericName := PointerPrefix + interfaceOrigin.Obj().Name()

	// The record's interface argument is a placeholder the generator reads only the open definition of; the element
	// serves, unless the interface constrains itself, where only the closed generic proxy satisfies its bound.
	placeholder := elementFullName

	if interfaceConstrainsItself(interfaceOrigin) {
		placeholder = interfaceFullName[:strings.LastIndex(interfaceFullName, ".")+1] + genericName + "<" + elementFullName + ">"
	}

	// Keyed apart from the concrete records: a bind is written even where a concrete proxy's record defers to the
	// production assembly's (packageInfoWriter), since binding again binds the same methods.
	packageLock.Lock()
	constraintProxies[constraintProxyBindKey+elementFullName+"|"+interfaceFullName] = [3]string{elementFullName, interfaceFullName, placeholder}
	packageLock.Unlock()

	// A type alias names one closed type, and no alias names the generic proxy: qualify it by its package class instead.
	if aliasQualifier {
		qualifier = interfaceFullName[:strings.LastIndex(interfaceFullName, ".")+1]
	}

	return qualifier + genericName + "<" + convertToCSTypeName(v.getAliasQualifiedTypeName(elemNamed, false)) + ">", true
}

// constraintProxyBindKey prefixes the constraintProxies key of a bind record (rule 2 above).
const constraintProxyBindKey = "bind|"
