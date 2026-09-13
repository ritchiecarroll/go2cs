// importOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bufio"
	"crypto/sha256"
	"encoding/hex"
	"errors"
	"fmt"
	"go/build"
	"go/types"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"

	"golang.org/x/mod/modfile"
)

// PackageInfo represents information about a package
type PackageInfo struct {
	IsStdLib bool
	// PublishedStdLib marks a standard-library import that this conversion references as a
	// PUBLISHED go.<pkg> NuGet assembly rather than as converted source (-recurse=nuget). Such a
	// dependency has no package_info.cs on disk to read its exported aliases and GoImplement
	// records from, so those come from the converter's embedded record of the tree those packages
	// are built from (stdlibMetadata.go). The flag is what makes that substitution SOUND: the
	// record describes src/core, which under this mode is exactly what is referenced.
	// It is deliberately NOT set for a $(go2csPath) source deployment, whose staged tree may be
	// the baseline core stub instead.
	PublishedStdLib  bool
	PackageName      string
	RootPackageName  string
	SourceDir        string
	TargetDir        string
	ProjectReference string
	Err              error
}

func getProjectName(importPath string, options Options) (string, string) {
	// isPathUnder, not strings.HasPrefix. The two operands are the SAME directory reached by two
	// routes — options.goRoot as the environment or the operator spelled it, importPath as go/build
	// or the command line spelled it — so a raw byte-prefix test promotes every difference in
	// SPELLING to a difference in MEANING. filepath.Rel (isPathUnder's engine) Cleans both sides, so
	// a forward-slash GOROOT on Windows, a trailing separator, and a doubled one all compare equal;
	// it folds case on Windows, where one directory legitimately has several spellings; and it is
	// boundary-correct, which the prefix test was not (`/usr/lib/go` prefix-matched `/usr/lib/gopher`).
	//
	// The HALF-recognized GOROOT is the expensive failure here, not the unrecognized one. When this
	// test wrongly says no for a package that IS under GOROOT/src, the else branch walks up looking
	// for a module root and finds $GOROOT/src/go.mod — which declares `module std`. Every
	// standard-library package is then named `std/<pkg>`, the whole emission lands in
	// `namespace go.std.*` with a 0 exit code and no warning, and the damage surfaces only in the
	// CONSUMER packages as `CS0117: 'utf8_package' does not contain a definition for …` — pointing
	// away from the cause and reading exactly like a converter regression that dropped public
	// members. Guarded by stdNamespaceGuard_test.go, which is where the spelling variants are
	// enumerated.
	if isPathUnder(importPath, options.goRoot) {
		// Clean the SUBJECT too. pathReplace's literal match still needs one separator spelling on
		// both sides, and a no-match at this step is not benign: the untrimmed absolute path becomes
		// the dotted project name (`C:.Users.…`), so the normalization the test above just did has
		// to reach the trim as well or it only moves the failure one line down.
		//
		// A no-match after that IS tolerable: the guard only proves the path is under GOROOT, not
		// under GOROOT/src, and the untrimmed value still names the package sensibly for the caller.
		importPath, _ = pathReplace(filepath.Clean(importPath), filepath.Join(options.goRoot, "src"), "")
	} else {
		// Check if current folder has go.mod or main.go
		if _, err := os.Stat(filepath.Join(importPath, "go.mod")); err == nil {
			// If we have a go.mod, try to read the module name from it
			if moduleName := readModuleFromGoMod(filepath.Join(importPath, "go.mod")); moduleName != "" {
				// Append remaining path segments if importPath has subdirectories
				relPath := ""
				if filepath.Base(importPath) != importPath {
					// Get the relative path from the directory containing go.mod to the importPath
					relPath = getRelativePath(importPath, importPath)
					if relPath != "" {
						moduleName = filepath.Join(moduleName, relPath)
					}
				}
				importPath = moduleName
			} else {
				importPath = filepath.Base(importPath)
			}
		} else if _, err := os.Stat(filepath.Join(importPath, "main.go")); err == nil {
			importPath = filepath.Base(importPath)
		} else {
			// Check if current folder has no go files
			if !hasGoFiles(importPath) {
				// If user provided path has no go files, we will assume current path
				// for project name and let parser fail since it is not a valid package
				importPath = filepath.Base(importPath)
			} else {
				// At this point, current folder has go files, but no go.mod or main.go
				// Keep traversing up the directory tree until we find go.mod or main.go
				// or we reach the root directory
				currentPath := importPath
				lastGoFilePath := currentPath // Keep track of the last path with Go files

				// An ancestor directory holding no .go files of its own is a FALLBACK name, never a
				// stop — issue #35. Go modules are full of pure container directories: `internal/`,
				// a service tree's `endpoints/` parent, a proto grouping like `xds/core/`, the
				// `datatransfer/` above an `apiv1/`. Treating the first one as the module boundary
				// truncated the name to the leaf segment alone, so cloud.google.com/go/bigquery's
				// datatransfer/apiv1 and storage/apiv1 both emitted `apiv1.csproj` — and Visual
				// Studio refuses to open a solution containing two projects of the same name,
				// without saying why, so the entire generated .slnx failed to load. That one
				// conversion had 175 such projects across 49 colliding names.
				//
				// The truncation also loses the namespace (the collapse joins its segments with "."
				// before the separator split that builds one), so a truncated `internal/errors`
				// lands on go.errors_package — the converted standard library's own class.
				//
				// Walking up to the module root instead is what the go command itself does to find
				// the main module, and it makes the name the package's import path by construction
				// (module path + relative path), which is what the GOROOT branch above already
				// produces for the standard library. The fallback still applies when there is
				// genuinely no module root anywhere above — a GOPATH-style tree — which is the only
				// case it was ever needed for.
				truncatedFallback := ""

				for {
					parentDir := filepath.Dir(currentPath)

					if parentDir == currentPath {
						// Reached the root directory with no module root above it, so there is no
						// import path to recover; the leaf-relative name composed on the way up is
						// the best available.
						if truncatedFallback != "" {
							importPath = truncatedFallback
						} else {
							importPath = filepath.Base(importPath)
						}

						break
					}

					currentPath = parentDir

					if _, err := os.Stat(filepath.Join(currentPath, "go.mod")); err == nil {
						// Found go.mod, use module name and append relative path
						if moduleName := readModuleFromGoMod(filepath.Join(currentPath, "go.mod")); moduleName != "" {
							// Get relative path from module root to import path
							relPath := getRelativePath(importPath, currentPath)
							if relPath != "" {
								importPath = filepath.Join(moduleName, relPath)
							} else {
								importPath = moduleName
							}
						} else {
							// Fallback if module name can't be read
							importPath = filepath.Base(currentPath) + "." + getRelativePath(importPath, currentPath)
						}

						break
					} else if _, err := os.Stat(filepath.Join(currentPath, "main.go")); err == nil {
						// Found main.go, get relative path from main.go directory to import path
						relPath := getRelativePath(importPath, currentPath)

						if relPath != "" {
							importPath = filepath.Base(currentPath) + "." + relPath
						} else {
							importPath = filepath.Base(currentPath)
						}

						break
					} else if !hasGoFiles(currentPath) {
						// No Go files in this directory: record the last directory that had them as
						// the fallback name and keep looking for the module root above. Frozen at
						// the FIRST crossing, because directories past it are not part of this
						// package's own chain and must not extend the fallback — so it is exactly
						// the name this arm produced before it stopped being a stop.
						if truncatedFallback == "" {
							if relPath := getRelativePath(importPath, lastGoFilePath); relPath != "" {
								truncatedFallback = filepath.Base(lastGoFilePath) + "." + relPath
							} else {
								truncatedFallback = filepath.Base(lastGoFilePath)
							}
						}

						continue
					}

					// Reached only when the directory HAS Go files (the arm above continues past the
					// ones that do not), so no second probe is needed to decide.
					lastGoFilePath = currentPath
				}
			}
		}
	}

	importPath = strings.ReplaceAll(importPath, "\\", "/")
	importPath = strings.TrimPrefix(importPath, "/")
	importPath = trimGo2CSModulePrefix(importPath)

	// Replace path separators with dots
	parts := strings.Split(importPath, "/")

	projectName := strings.Join(parts, ".")
	namespace := RootNamespace

	if len(parts) > 1 {

		for i := 0; i < len(parts)-1; i++ {
			namespace += "." + getCoreSanitizedIdentifier(parts[i])
		}
	}

	return projectName, namespace
}

// go2csModulePrefix is this repository's own module-path convention. The behavioral corpus and the
// package-test fixtures declare `module go2cs/<Name>`, and that leading segment is a REPOSITORY
// marker rather than a namespace segment: `module go2cs/<Name>` and a bare `module <Name>` have
// always emitted the same `namespace go;` + `<Name>_package`, which is the only reason the two
// spellings can coexist in one tree.
const go2csModulePrefix = "go2cs/"

// trimGo2CSModulePrefix elides that marker from an import path on its way to a namespace or a
// project name.
//
// It is ONE function because the elision has to happen on BOTH sides of a reference. It lived inline
// in getProjectName — the DECLARATION side, which decides the emitted `namespace`, the project name,
// and the <ProjectReference> file name derived from it — while convertImportPathToNamespace, the
// IMPORT side, which decides a `using <alias> = …` target, a bare `using <namespace>;` and a forcing
// hook's `typeof`, kept the marker as a namespace segment. Any reference to a `go2cs/…` path
// therefore named a namespace the declaration never emits, and a package could not be imported at
// all: `module go2cs/convertedtestharness` declares `go.convertedtestharness_package` while its own
// external test variant imported `go2cs.convertedtestharness_package` — CS0234 ×2, which is what
// left the end-to-end `-tests` fixture unbuildable from the day it was written.
//
// The DECLARATION side is the one made canonical, for three reasons.
//
//   - The committed corpus already rests on it. 618 behavioral modules are `go2cs/<Name>` and emit
//     `namespace go;`; keeping the marker instead would rename every one of them, their project
//     files and their solution entries, and buy nothing.
//   - The 46 BARE modules exist only to dodge this asymmetry. A behavioral test holding a nested
//     sub-package is the measured case: under `module IoLike` the parent imports `IoLike/FsLike`,
//     which has no marker to disagree about, while the same tree under `module go2cs/IoLike`
//     declares `go.IoLike.FsLike_package` and imports `go2cs.IoLike.FsLike_package`. Agreeing here
//     retires that constraint instead of entrenching it; no existing module is respelled.
//   - Nothing outside this repository can hold such a path. A fetchable module path's first element
//     must contain a dot, because it is a host name; `go2cs` has none, so the marker is repo-private
//     by construction and eliding it cannot swallow a real dependency's first segment.
//
// It is deliberately NOT applied to a path recovered from a DIRECTORY rather than declared by a
// go.mod (getImportPackageInfo's build.Import arm, whose canonical path is GOROOT/src- or
// GOPATH-relative). This is a module-path rule; a directory that merely happens to sit under a
// `go2cs` folder is not a module declaring that path.
func trimGo2CSModulePrefix(importPath string) string {
	return strings.TrimPrefix(importPath, go2csModulePrefix)
}

// maxRelativeProjectPath bounds, by construction, the length of a generated project's path RELATIVE
// to the conversion output root — `<tree-root>/<import-path>/<name>.csproj`. Absolute paths are what
// Windows measures, so the budget leaves room for the user's own output root: 200 characters here
// means any root up to 59 characters keeps every emitted project under Windows' 260-character MAX_PATH.
//
// The limit is real and it is NOT liftable — issue #35, second wall. Visual Studio's project loader
// refuses a .csproj whose path exceeds MAX_PATH *even on a machine with LongPathsEnabled=1* (measured:
// a 259-character project loads and builds; a 265-character one fails with "The project file could not
// be loaded. Could not find a part of the path", naming a file that is demonstrably there and that
// `dotnet build` reads without complaint). So the registry key is not an answer, and neither is
// flattening the tree: `pkg/<import-path>/<dotted>.csproj` and `pkg/<dotted>/<dotted>.csproj` measure
// IDENTICALLY, because a separator costs exactly what a dot costs. The reporter's real conversion put
// its deepest project at 242 characters before the output root.
//
// The import path is spelled TWICE on disk — once by the directory, which mirrors it, and once by this
// file name — and only the second spelling is redundant: the directory already says which package this
// is. It cannot simply be dropped, though. Visual Studio derives a project's NAME from its file name
// and refuses a solution holding two of the same name (issue #35's FIRST wall, fixed in 9c970b258 by
// recovering the full import path), and .slnx has no display-name override to separate the two
// (measured: a `DisplayName` attribute does not help — VS still reports "Project name 'v3' already
// exists in the '/pkg/' solution folder"). The name must stay unique; the only lever left is to make
// it shorter WHILE unique.
const maxRelativeProjectPath = 200

// minProjectFileBaseName floors the budget: an import path deep enough to exhaust the whole allowance
// on its directory alone cannot meet maxRelativeProjectPath however short the file name gets (the
// directory is not negotiable — it is what a Go developer navigates by). Rather than compress the name
// to nothing, stop at a length that still carries a readable head, a readable tail and the hash, and
// let the path run over. Nothing observed comes close: the deepest import path in the reporter's
// 1,725-package conversion is 115 characters, against the ~168 at which this floor starts to bind.
const minProjectFileBaseName = 24

// projectFileBaseName returns the FILE-NAME spelling of a project whose canonical name is projectName
// (the dotted import path getProjectName returns). It is the file name only — the C# namespace, the
// AssemblyName and the NuGet PackageId all keep the full canonical path, because those are identity:
// they must stay globally unique and legible in a stack trace. This is a label.
//
// Names that fit the budget are returned VERBATIM, which is every package in the standard library
// (longest emitted path: 101 characters) and every behavioral-test package (longest: 109) — so this
// function is a no-op for the entire committed corpus. Only a name whose emitted path would exceed
// maxRelativeProjectPath is compressed, to `head~tail.hash8`: a readable head (carrying the module), a
// readable tail (carrying the leaf package), and 8 hex characters of SHA-256 over the FULL name.
//
// Two properties matter more than the length:
//
//   - It is a pure function of the canonical name, so it is SET-INDEPENDENT. The reference side
//     (getRecurseDependencyInfo) derives a dependency's file name from the import path alone, without
//     knowing which other packages are in the conversion. A shortest-unique-SUFFIX scheme would have
//     been shorter still, but under it adding one dependency can rename unrelated projects — the same
//     coupling 9c970b258 removed, one level up.
//   - Uniqueness survives compression, because the hash covers the full name: two import paths that
//     share a head and a tail still differ in the hash.
func projectFileBaseName(projectName string) string {
	// The emitted layout spells the import path twice: as the directory `<tree-root>/<import-path>`
	// and as this file name. The dotted name and the slashed import path are the SAME length (one
	// separator per segment either way), so the directory costs len(projectName) plus the tree root
	// and its separator — `core/` at 5 is the longest of core, pkg and src, and taking the longest
	// keeps the bound conservative for all three.
	const treeRootCost = len("core") + 1
	const extensionCost = len(".csproj")

	budget := maxRelativeProjectPath - treeRootCost - len(projectName) - 1 - extensionCost

	if len(projectName) <= budget {
		return projectName
	}

	if budget < minProjectFileBaseName {
		budget = minProjectFileBaseName
	}

	sum := sha256.Sum256([]byte(projectName))
	hash := hex.EncodeToString(sum[:])[:8]

	// `head~tail.hash8`: the tilde marks the elision (a hyphen would not — module paths like
	// `go-control-plane` already contain hyphens), and the hash is set off by a dot so the whole name
	// still reads as dotted segments.
	visible := budget - len(hash) - 2
	head := visible * 2 / 3
	tail := visible - head

	return projectName[:head] + "~" + projectName[len(projectName)-tail:] + "." + hash
}

// readModuleFromGoMod reads the module path from a go.mod file.
//
// The module path is a go.mod TOKEN, and a token may be written as a quoted string — gopkg.in/yaml.v3
// declares itself `module "gopkg.in/yaml.v3"`, and the gopkg.in family generally does. A raw read of
// the line's remainder carries those quotes into the module path, hence into the project name, hence
// into the csproj FILENAME, which Windows rejects outright: issue #33's
// `"gopkg.in.yaml.v3".csproj` … "The filename, directory name, or volume label syntax is incorrect."
// The quotes are syntax, never part of the path, so the fix is to read the token rather than the line.
// modfile.ModulePath is the tolerant reader cmd/go itself uses for exactly this job: it drops `//`
// comments, requires `module` to begin the line, and unquotes both the interpreted and raw forms.
func readModuleFromGoMod(goModPath string) string {
	data, err := os.ReadFile(goModPath)

	if err != nil {
		return ""
	}

	return modfile.ModulePath(data)
}

// getRelativePath returns the relative path from basePath to targetPath
func getRelativePath(targetPath, basePath string) string {
	rel, err := filepath.Rel(basePath, targetPath)

	if err != nil {
		return ""
	}

	// If the paths are the same, return empty string
	if rel == "." {
		return ""
	}

	return rel
}

// hasGoFiles checks if the specified directory contains any .go files
func hasGoFiles(dirPath string) bool {
	// Pattern to match .go files in the specified directory
	pattern := filepath.Join(dirPath, "*.go")

	// Find all files matching the pattern
	matches, err := filepath.Glob(pattern)

	if err != nil {
		return false
	}

	// If we found at least one match, return true
	return len(matches) > 0
}

// getImportPackageInfo returns information about whether the packages are from the standard
// library and their physical directories.
//
// A stdlib import's exported-type-alias metadata (package_info.cs) is loaded from the SAME tree its
// converted assembly is compiled from, which since 2026-08-01 needs no mapping at all: the converted
// standard library lives at $(go2csPath)core\<pkg>, the one place every resolver here already points.
// (Until then a -tests conversion compiled against a separate converted stdlib tree while the alias
// load looked at the baseline core stub — which for most packages had no package_info.cs — so the
// alias map came back empty and a cross-package reference to a collision-renamed stdlib type rendered
// the raw, undefined name `runtime.Error` (CS0426) instead of `runtimeꓸError` =>
// `runtime_package.ΔError`. One tree, one path, and the whole remap is gone.)
func getImportPackageInfo(importPaths []string, options Options) map[string]PackageInfo {
	result := make(map[string]PackageInfo, len(importPaths))

	for _, importPath := range importPaths {
		// Under -recurse, resolve APP (main-module) and THIRD-PARTY dependency references via the
		// module-aware go/packages metadata, routing them to the parallel recurse-output src\ and
		// pkg\ trees (version-free). Stdlib falls through to the standard resolver below,
		// which emits $(go2csPath)core references.
		if options.recurse {
			if info, ok := getRecurseDependencyInfo(importPath, options); ok {
				result[importPath] = info
				continue
			}
		}

		pkg, err := build.Import(importPath, "", build.FindOnly)

		// go/build (GOPATH-based) cannot resolve a LOCAL/USER module reached via a `replace`
		// directive or otherwise outside GOPATH/GOROOT. Fall back to the module-aware go/packages
		// dir captured at load time, treating its converted output as in-place (co-located with the
		// Go source) — the common layout when converting a whole module tree.
		if err != nil {
			if info, ok := getLocalModulePackageInfo(importPath, options); ok {
				result[importPath] = info
			} else {
				result[importPath] = PackageInfo{Err: err}
			}
			continue
		}

		// Standard library packages are located in GOROOT
		isStdLib := pkg.Goroot && !build.IsLocalImport(importPath)

		sourceDir := pkg.Dir
		var targetDir string

		// The package's identity for naming purposes: its ON-DISK path, which for a GOROOT-vendored
		// package is the `vendor/`-prefixed one and for everything else is the import path itself
		// (resolved below, once the stdlib branch has produced the directory to read it from).
		canonicalImportPath := importPath

		if isStdLib {
			goRootSrc := filepath.Join(options.goRoot, "src")

			var rewritten bool
			targetDir, rewritten = pathReplace(sourceDir, goRootSrc, go2csCoreRoot)

			if !rewritten {
				warnGoRootPathReplace(sourceDir, goRootSrc)
			}

			if canonical, ok := stdLibImportPathFromTargetDir(targetDir); ok {
				canonicalImportPath = canonical
			}
		} else {
			// A no-match here is ROUTINE — a non-stdlib package resolved by go/build commonly sits
			// under GOPATH/src or a local module rather than GOPATH/pkg, and keeping the source dir
			// is the in-place convention for those. Nothing to warn about.
			targetDir, _ = pathReplace(sourceDir, filepath.Join(options.goPath, "pkg"), "$(go2csPath)pkg")
		}

		importPathParts := strings.Split(canonicalImportPath, "/")
		packageName := strings.Join(importPathParts, ".")
		projectReference := emittedProjectReference(targetDir, projectFileBaseName(packageName)+".csproj")

		targetDir = strings.ReplaceAll(targetDir, "$(go2csPath)", options.go2csPath+string(os.PathSeparator))

		result[importPath] = PackageInfo{
			IsStdLib: isStdLib,
			// Mirrors the emitNuGet gate in writeProjectFile: -recurse=nuget turns stdlib imports
			// into go.<pkg> PackageReferences, EXCEPT during the stdlib self-conversion, whose
			// packages must reference each other as source to build the assemblies being published.
			PublishedStdLib:  isStdLib && options.nugetRefs && !options.convertStdLib,
			PackageName:      packageName,
			RootPackageName:  rootPackageNameFromPathParts(importPathParts),
			SourceDir:        sourceDir,
			TargetDir:        targetDir,
			ProjectReference: projectReference,
		}
	}

	return result
}

// rootPackageNameFromPathParts derives the Go package NAME from a path-resolved import's
// segments. Normally the last segment, but a major-version tail (/vN) is a version marker,
// not the package — math/rand/v2 is `package rand`, named for the PARENT segment.
// RootPackageName is the code-facing qualifier (imported-alias keys, foreign-implement
// record keys keyed by the name cast sites use), so it must carry the package name;
// PackageName keeps the full path form (it names the referenced .csproj, which IS
// math.rand.v2.csproj).
func rootPackageNameFromPathParts(pathParts []string) string {
	last := pathParts[len(pathParts)-1]

	if len(pathParts) > 1 && majorVersionSegmentRegex.MatchString(last) {
		return pathParts[len(pathParts)-2]
	}

	return last
}

// getLocalModulePackageInfo resolves cross-package reference info for a LOCAL/USER module import that
// go/build could not find, using the module-aware go/packages dir captured in importPackageDirs. The
// converted output is treated as in-place (co-located with the Go source). The returned
// ProjectReference is an ABSOLUTE path to the imported package's generated .csproj; writeProjectFile
// rewrites it relative to the referencing project. RootPackageName/PackageName are the Go package name
// (the identifier used to qualify references in code, and — for a single-segment module — the C# class
// base `<name>_package`). Returns ok=false when the import is unknown to the loaded graph.
func getLocalModulePackageInfo(importPath string, options Options) (PackageInfo, bool) {
	meta, ok := importPackageDirs[importPath]

	if !ok || meta.Dir == "" {
		return PackageInfo{}, false
	}

	// Defense in depth: build.Import is pinned to the resolved GOROOT (see main), but if it still
	// fails for a STDLIB package, the dir is under GOROOT/src — apply the stdlib `$(go2csPath)core`
	// mapping rather than emitting a machine-specific absolute reference as if it were a user module.
	goRootSrc := filepath.Join(options.goRoot, "src")

	if isPathUnder(meta.Dir, goRootSrc) {
		targetDir, rewritten := pathReplace(meta.Dir, goRootSrc, go2csCoreRoot)

		if !rewritten {
			warnGoRootPathReplace(meta.Dir, goRootSrc)
		}

		// The ON-DISK spelling names the package (see stdLibImportPathFromTargetDir). This is the
		// branch a GOROOT-VENDORED import reaches: go/build cannot resolve `golang.org/x/crypto/…`
		// with no source dir to vendor-resolve against, so it falls through to the module-aware
		// loader's dir — which IS the vendored one — while the import path stays as written.
		canonicalImportPath := importPath

		if canonical, ok := stdLibImportPathFromTargetDir(targetDir); ok {
			canonicalImportPath = canonical
		}

		importPathParts := strings.Split(canonicalImportPath, "/")
		packageName := strings.Join(importPathParts, ".")
		projectReference := emittedProjectReference(targetDir, projectFileBaseName(packageName)+".csproj")

		return PackageInfo{
			IsStdLib:         true,
			PackageName:      packageName,
			RootPackageName:  rootPackageNameFromPathParts(importPathParts),
			SourceDir:        meta.Dir,
			TargetDir:        strings.ReplaceAll(targetDir, "$(go2csPath)", options.go2csPath+string(os.PathSeparator)),
			ProjectReference: projectReference,
		}, true
	}

	// A read-only, versioned module-cache dependency ($GOPATH/pkg/mod/<module>@<version>/...): its
	// converted output goes to a WRITABLE $(go2csPath)pkg\<import-path> location, referenced there via
	// the $(go2csPath) property (like the stdlib $(go2csPath)core refs — NOT rewritten relative, so it
	// resolves against the deploy root). The @version segment is stripped by deriving the path from the
	// version-free IMPORT PATH rather than from meta.Dir. ModuleConverter writes the package's output
	// to the matching $(go2csPath)pkg\<import-path> directory, so reference and output stay in agreement.
	if isPathUnder(meta.Dir, filepath.Join(options.goPath, "pkg", "mod")) {
		libProjectName, namespace := getProjectName(meta.Dir, options)
		targetDir := "$(go2csPath)pkg/" + importPath
		projectReference := emittedProjectReference(targetDir, projectFileBaseName(libProjectName)+".csproj")

		return PackageInfo{
			IsStdLib:         false,
			PackageName:      packageQualifiedName(namespace, meta.Name),
			RootPackageName:  meta.Name,
			SourceDir:        meta.Dir,
			TargetDir:        strings.ReplaceAll(targetDir, "$(go2csPath)", options.go2csPath+string(os.PathSeparator)),
			ProjectReference: projectReference,
		}, true
	}

	// A genuine LOCAL/USER module (a co-located `replace`): its converted output is in-place
	// (co-located with its Go source), and it generates `<projectName>.csproj` in its own directory.
	// The absolute ProjectReference is rewritten relative to the referencing project by writeProjectFile.
	libProjectName, namespace := getProjectName(meta.Dir, options)
	projectReference := filepath.Join(meta.Dir, projectFileBaseName(libProjectName)+".csproj")

	return PackageInfo{
		IsStdLib:         false,
		PackageName:      packageQualifiedName(namespace, meta.Name),
		RootPackageName:  meta.Name,
		SourceDir:        meta.Dir,
		TargetDir:        meta.Dir,
		ProjectReference: projectReference,
	}, true
}

// isMainModulePackage reports whether importPath belongs to the app (main) module identified by
// mainModulePath — the module path itself or any sub-package of it. Used under -recurse to route
// the app's own packages to the src\ tree while dependencies go to pkg\.
func isMainModulePackage(importPath, mainModulePath string) bool {
	return mainModulePath != "" &&
		(importPath == mainModulePath || strings.HasPrefix(importPath, mainModulePath+"/"))
}

// getRecurseDependencyInfo resolves cross-package reference info for an APP (main-module) or
// THIRD-PARTY import under -recurse, from the module-aware go/packages metadata captured in
// importPackageDirs. App packages live under <recurse-output>\src\<import-path>; dependencies live
// under <recurse-output>\pkg\<import-path> — both version-free (the path is derived from the
// version-free import path, not the possibly-@versioned module-cache source dir). The absolute
// generated-project path is returned so writeProjectFile rewrites it relative to the importing
// project. That keeps the converted graph portable and leaves $(go2csPath) dedicated to the
// separately-selectable stdlib/runtime root.
// Returns ok=false for the standard library (resolved by the default path as $(go2csPath)core refs)
// and for imports unknown to the loaded graph.
func getRecurseDependencyInfo(importPath string, options Options) (PackageInfo, bool) {
	meta, ok := importPackageDirs[importPath]

	if !ok || meta.Dir == "" {
		return PackageInfo{}, false
	}

	// Standard library is resolved by the default (build.Import) path as $(go2csPath)core refs.
	if isPathUnder(meta.Dir, filepath.Join(options.goRoot, "src")) {
		return PackageInfo{}, false
	}

	root := "pkg"

	if isMainModulePackage(importPath, options.mainModulePath) {
		root = "src"
	}

	outputRoot := options.recurseOutputRoot

	if outputRoot == "" {
		outputRoot = options.go2csPath
	}

	libProjectName, namespace := getProjectName(meta.Dir, options)
	targetDir := filepath.Join(outputRoot, root, filepath.FromSlash(importPath))
	projectReference := filepath.Join(targetDir, projectFileBaseName(libProjectName)+".csproj")

	return PackageInfo{
		IsStdLib:         false,
		PackageName:      packageQualifiedName(namespace, meta.Name),
		RootPackageName:  meta.Name,
		SourceDir:        meta.Dir,
		TargetDir:        targetDir,
		ProjectReference: projectReference,
	}, true
}

// packageQualifiedName returns the dotted name N for which go.<N>_package is the imported package's
// converted class: the namespace getProjectName produced (minus the `go.` root) joined with the Go
// package name. For a single-segment module (namespace == the root `go`) it is just the package name.
// An imported type alias must qualify against this — the bare package name targets a nonexistent
// top-level go.<name>_package for any multi-segment import path (CS0234, e.g. github.com/google/uuid's
// `uuid` -> go.uuid_package instead of go.github.com.google.uuid_package). Using the Go package name
// (meta.Name) as the last segment — not the import-path segment — keeps it correct when they differ
// (github.com/mattn/go-isatty is `package isatty` -> go.github.com.mattn.isatty_package).
func packageQualifiedName(namespace, packageName string) string {
	nsPrefix := strings.TrimPrefix(namespace, RootNamespace+".")

	if nsPrefix == namespace {
		return packageName
	}

	return nsPrefix + "." + packageName
}

// go2csCoreRoot is the emitted root of the ONE converted standard library — the prefix every stdlib
// reference is rewritten to, and the prefix stdLibImportPathFromTargetDir reads back off.
const go2csCoreRoot = "$(go2csPath)core"

// stdLibImportPathFromTargetDir recovers a standard-library package's canonical, ON-DISK import path
// from the $(go2csPath)core-rooted directory its reference already resolved to. Returns ok=false when
// the directory is not core-rooted (the GOROOT rewrite found no match — warnGoRootPathReplace's case),
// leaving the caller on the import path as written.
//
// The two spellings differ for exactly one class: a GOROOT-VENDORED package. `crypto/ecdh` imports
// `golang.org/x/crypto/chacha20`, but that package exists on disk — and therefore as a converted
// project — only at `vendor/golang.org/x/crypto/chacha20`. The DIRECTORY was always right (it is
// rewritten from the resolved source dir), while the project FILE NAME was composed from the import
// path as written, so the reference named a real directory and a file in it that exists nowhere:
// `…/vendor/golang.org/x/crypto/chacha20/golang.org.x.crypto.chacha20.csproj` against the real
// `vendor.golang.org.x.crypto.chacha20.csproj`.
//
// A missing <ProjectReference> sounds fatal and is not — MSBuild degrades it to warning MSB9008 and
// builds on (measured: the pre-fix crypto.ecdh.tests.csproj builds, 0 errors, and the correct sibling
// reference supplied the assembly anyway). The cost was downstream: the stale name was harvested into
// go2cs-stdlib.slnx as a phantom 308th project by the multi-platform merge's solution recovery
// (fixed on the solution side by TestCollectConvertedProjectsIgnoresTestProjectReferences).
//
// Deriving the name from the directory is what makes the two sides structurally agree rather than
// coincidentally: getProjectName — the PRODUCER, which names the .csproj the vendored package
// actually emits — has always derived it from that same GOROOT/src-relative directory. Anything that
// resolves the directory correctly now names the file correctly by construction.
//
// PackageName is not only the file name, so the correction reaches further: it keys the embedded
// standard-library metadata (stdlibMetadata.go records the vendored spelling,
// `##vendor.golang.org.x.crypto.chacha20`, so the unvendored lookup MISSED), and it composes the
// imported-alias class path (`go.vendor.golang.org.x.crypto.chacha20_package` — the unvendored
// `go.golang.org…_package` names a class that exists nowhere, the CS0234 family
// resolveGorootVendoredPath was added to prevent on the namespace side).
func stdLibImportPathFromTargetDir(targetDir string) (string, bool) {
	rest, found := strings.CutPrefix(normalizeEmittedPath(targetDir), go2csCoreRoot)

	// The separator is required, not merely trimmed: `core` must match the whole path SEGMENT, or a
	// sibling root that merely starts with it would be read as the core tree plus a truncated import
	// path. `$(go2csPath)core` itself (the root, no package) is likewise not a package path.
	if !found || !strings.HasPrefix(rest, "/") {
		return "", false
	}

	if rest = rest[1:]; rest == "" {
		return "", false
	}

	return rest, true
}

// emittedProjectReference joins a reference DIRECTORY (an $(go2csPath)-rooted or absolute path) and a
// .csproj file name into the exact string that lands in a <ProjectReference Include="…">.
//
// It emits FORWARD slashes on every host, because the emitted corpus must be one corpus: MSBuild
// accepts `/` in every path context on Windows too (FileUtilities.MaybeAdjustFilePath normalizes the
// other direction on Unix, which is why the historical `\` form still *builds* there), so `/` is the
// single form that is correct everywhere — no per-host emission, no second corpus shape to diff.
//
// The hand-rolled predecessor here was `filepath.Join(strings.ReplaceAll(dir, "/", "\\"), "\\"+name)`.
// On Windows filepath.Clean folded the injected separators back into a well-formed `\` path; on Unix
// filepath.Join treats `\` as an ordinary filename character, so it produced the malformed
// `$(go2csPath)core\fmt/\fmt.csproj` — F5 in docs/PLAN-linux-operation.md. path.Join (slash-only,
// host-independent) over a ToSlash'd directory has neither behavior.
func emittedProjectReference(dir string, csprojFileName string) string {
	return path.Join(filepath.ToSlash(dir), csprojFileName)
}

// normalizeEmittedPath rewrites a path READ BACK OUT of an MSBuild file to forward slashes on ANY
// host, so the readers of an emitted <ProjectReference> are separator-agnostic in both directions.
//
// filepath.ToSlash cannot do this job: it replaces the HOST's separator, so on Linux and macOS it is
// the identity and a `\`-spelled reference passes through unchanged — which is why, before this,
// parseCoreProjectRefs returned `core\golib\golib.csproj` on Linux and isSelfProjectReference's
// filepath.Base saw one long filename. Emission is forward-slash since F5, but a reference can still
// arrive backslashed from a corpus converted by an older binary, a deployed tree, or a hand-authored
// project — and reading those must not depend on which OS is doing the reading.
func normalizeEmittedPath(reference string) string {
	return strings.ReplaceAll(reference, "\\", "/")
}

// isPathUnder reports whether path is the directory dir or nested within it (case-insensitive on
// Windows), used to recognize a stdlib package by its location under GOROOT/src.
func isPathUnder(path, dir string) bool {
	rel, err := filepath.Rel(dir, path)
	if err != nil {
		return false
	}

	return rel != ".." && !strings.HasPrefix(rel, ".."+string(os.PathSeparator))
}

// pathReplace rewrites occurrences of the `search` path inside `subject` to `replace` — the mapping
// that turns a GOROOT/GOPATH source directory into the emitted $(go2csPath)core / $(go2csPath)pkg
// reference. It reports whether a replacement actually happened.
//
// The bool matters because a NO-MATCH is silent and consequential: the caller keeps the untouched
// absolute source path and emits it as a <ProjectReference>, producing a machine-specific reference
// to a .csproj under GOROOT that does not exist. On Windows the match is case-insensitive so the
// realistic failure is none; on Linux and macOS the exposure is a toolchain reached through a
// SYMLINK, where options.goRoot and the directory go/build reports differ by spelling alone
// (/usr/lib/go vs /usr/lib/go-1.23, $HOME/sdk/go1.23.1 vs a GOROOT env pointing at a symlink).
//
// The symlink fallback runs ONLY when the direct replace already failed, which is the whole reason
// it is safe: a run whose direct replace matches — every Windows run today — never reaches it, so
// emitted bytes cannot move. Both sides are resolved, because either one may be the symlinked
// spelling. See F6 in docs/PLAN-linux-operation.md.
func pathReplace(subject string, search string, replace string) (string, bool) {
	if result, ok := pathReplaceExact(subject, search, replace); ok {
		return result, true
	}

	// filepath.EvalSymlinks needs both paths to exist; when either does not, there is nothing to
	// resolve and the no-match stands.
	resolvedSubject, err := filepath.EvalSymlinks(subject)

	if err != nil {
		return subject, false
	}

	resolvedSearch, err := filepath.EvalSymlinks(search)

	if err != nil {
		return subject, false
	}

	return pathReplaceExact(resolvedSubject, resolvedSearch, replace)
}

// pathReplaceExact is pathReplace's literal half: case-insensitive on Windows (where the same
// directory is legitimately spelled several ways), exact elsewhere.
func pathReplaceExact(subject string, search string, replace string) (string, bool) {
	var result string

	if runtime.GOOS == "windows" {
		searchEscaped := regexp.QuoteMeta(search)
		searchRE := regexp.MustCompile("(?i)" + searchEscaped)
		result = searchRE.ReplaceAllString(subject, replace)
	} else {
		result = strings.ReplaceAll(subject, search, replace)
	}

	return result, result != subject
}

// goRootPathReplaceWarned pins the GOROOT-rewrite no-match warning to ONCE per run — a conversion
// resolves dozens of imports and every stdlib one would otherwise repeat the same advice. Follows
// go2csRootWarned's shape (package-level, test-pinnable: a test saves it, resets it, restores it).
var goRootPathReplaceWarned bool

// warnGoRootPathReplace reports a stdlib source directory that could NOT be rewritten to its
// $(go2csPath)core form. Called only where the directory is KNOWN to live under GOROOT/src, so a
// no-match is never the normal case — unlike the import-path and GOPATH call sites, where a
// non-match is routine and silence is correct.
func warnGoRootPathReplace(sourceDir string, goRootSrc string) {
	if goRootPathReplaceWarned {
		return
	}

	goRootPathReplaceWarned = true

	// %s, not %q: on Windows %q escapes every separator in the paths it is asking the reader to compare.
	showWarning("Standard-library source \"%s\" could not be rewritten relative to \"%s\".\n"+
		"         The emitted <ProjectReference> keeps that absolute path and will not resolve on\n"+
		"         another machine. This is the symlinked-toolchain case: pass -goroot with the same\n"+
		"         spelling `go env GOROOT` reports, or point it at the resolved directory.",
		sourceDir, goRootSrc)
}

func (v *Visitor) loadImportedTypeAliases(projectImport string) {
	packageInfoMap := getImportPackageInfo([]string{projectImport}, v.options)

	for _, info := range packageInfoMap {
		// Load imported type aliases for the target project import, if not already loaded
		loadImportedTypeAliases(info, v.options)
	}
}

// isCSharpBuiltinTypeName reports whether an exported type-alias TARGET is a C# built-in type
// keyword (object from a defined type over the empty interface, or a numeric/bool/string/char
// primitive) rather than a package-local named type. Such a target is imported BARE.
func isCSharpBuiltinTypeName(name string) bool {
	switch name {
	case "object", "string", "bool", "char", "decimal",
		"byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong",
		"float", "double", "nint", "nuint":
		return true
	}
	return false
}

func loadImportedTypeAliases(info PackageInfo, options Options) {
	// Layout L3 routes a package's platform-varying artifacts into per-GOOS folders, and
	// `package_info.cs` is one of them (design §4.3) — so ask for the copy that describes the
	// platform THIS conversion is emitting for. Flat wins when the dependency's metadata is shared,
	// which it is for the overwhelming majority of the corpus; see platformPackageInfoPath for what
	// asking flat unconditionally would silently cost.
	packageInfoFile := platformPackageInfoPath(info.TargetDir, goosOfTarget(options.targetPlatform))

	packageLock.Lock()

	// Check if this package info file has already been parsed
	if _, ok := parsedPackageInfoFiles[packageInfoFile]; ok {
		packageLock.Unlock()
		return
	}

	parsedPackageInfoFiles.Add(packageInfoFile)
	packageLock.Unlock()

	// The dependency has not been converted into this output root, so there is no package_info.cs
	// carrying its exported type aliases.
	if _, err := os.Stat(packageInfoFile); os.IsNotExist(err) {
		// A PUBLISHED standard-library dependency (-recurse=nuget: the assembly referenced is the
		// go.<pkg> package built from src/core) has no converted source anywhere on
		// disk, and never will — but the metadata that source would have carried is recorded in
		// the converter itself, captured from that same tree. Read it, so a NuGet-referencing
		// conversion resolves foreign aliases and foreign GoImplement records exactly as a
		// source-referencing one does. Without it the converter cannot see that the dependency's
		// OWN assembly already implements an interface on one of its types, and re-declares the
		// pair locally in this package — go2cs-gen then emits a second, duplicate adapter class
		// (syscall.Errno → error, consumed by golang.org/x/sys/windows: CS0102/CS0111/CS8646).
		if info.PublishedStdLib {
			if lines, ok := stdLibExportedMetadata(info.PackageName); ok {
				if results, parseErr := parseExportedTypeAliasLines(lines); parseErr == nil {
					applyExportedTypeAliases(results, info, false)
					loadPackageImplementLines(lines, info.RootPackageName)
					return
				}
			}
		}

		// Most exported aliases are still knowable — they are a function of the dependency's OWN
		// declarations — so derive and apply those, giving a single-package conversion the same
		// foreign spelling a whole-stdlib run produces: its name-collision renames (an unrenamed
		// `time.Second` binds the `Second(this Time)` method group and does not compile) and its
		// re-exported type aliases (`os.FileMode` is a using alias to `go.io.fs_package.FileMode`,
		// not a member of `os_package` — CS0426). See foreignNameCollisions.go and
		// foreignTypeAliases.go for the invariant and for what deliberately stays underivable.
		applyExportedTypeAliases(foreignDerivedTypeAliases(importedPackageSources[filepath.Clean(info.SourceDir)]), info, true)
		return
	}

	// Parse package info file for exported type aliases, these are used
	// as the imported type aliases in the current package
	results, err := parseExportedTypeAliases(packageInfoFile)

	if err == nil {
		applyExportedTypeAliases(results, info, false)
		loadPackageImplements(packageInfoFile, info.RootPackageName)
	} else {
		showWarning("Failed to parse exported type aliases from package info file \"%s\": %s", packageInfoFile, err)
	}
}

// applyExportedTypeAliases records one imported package's exported type aliases — parsed from its
// package_info.cs, or derived from its own declarations when that file does not exist — as this
// package's imported type aliases. Shared by both sources so a derived entry is qualified exactly
// like a parsed one. A derived entry is marked as such: its `global using` is emitted only if an
// emitted reference resolves through it (see derivedTypeAliases).
func applyExportedTypeAliases(results [][2]string, info PackageInfo, derived bool) {
	if len(results) == 0 {
		return
	}

	rootPackageName := getSanitizedIdentifier(info.RootPackageName)

	// The alias TARGET names the imported package's converted CLASS — `go.<N>_package` where N is
	// info.PackageName with its final segment replaced by the Go package NAME. The two agree except
	// for a major-version path tail: math/rand/v2's class path is math.rand.rand (class
	// rand_package), while the path-derived math.rand.v2 targets the nonexistent v2_package
	// (CS0426). info.PackageName itself stays path-formed — it also names the referenced .csproj.
	classPath := info.PackageName

	if idx := strings.LastIndex(classPath, "."); idx != -1 {
		classPath = classPath[:idx+1] + info.RootPackageName
	} else {
		classPath = info.RootPackageName
	}

	packageName := getCoreSanitizedIdentifier(classPath)

	// A collision-renamed type whose renamed form is ITSELF an exported alias produces a TWO-HOP
	// chain in the producer's package_info: encoding/json's `Token` type collides with
	// `(*Decoder).Token()`, so the TYPE is Δ-renamed (`GoTypeAlias("Token", "ΔToken")`), and — Token
	// being `type Token any` (an empty interface) — visitTypeSpec ALSO exports the renamed form's
	// concrete target (`GoTypeAlias("ΔToken", "object")`). A consumer that resolves only the FIRST
	// hop qualifies the intermediate Δ-name as a package member (`go.encoding.json_package.ΔToken`),
	// but ΔToken is an assembly-scoped `global using`, not a namespace member → CS0426 (html/template,
	// internal/coverage/cfile, expvar, internal/fuzz, log/slog, ...). Build a source→target map of
	// THIS package's exported aliases so the chain can be followed to its concrete target (`object`).
	localAliases := make(map[string]string, len(results))

	for _, result := range results {
		localAliases[result[0]] = result[1]
	}

	for _, result := range results {
		// Add the exported type alias to the imported type aliases map
		alias := fmt.Sprintf("%s.%s", rootPackageName, getCoreSanitizedIdentifier(result[0]))

		// Follow a chain of same-package aliases to the concrete target (Token → ΔToken → object).
		// Bounded by the alias count and short-circuited on a self-reference, so a degenerate cycle
		// cannot loop. A target that is NOT itself an exported source (a real Δ-renamed delegate/
		// struct member such as ΔFilter, or an already `go.`-qualified name) leaves it unchanged.
		target := result[1]

		for range results {
			next, ok := localAliases[target]

			if !ok || next == target {
				break
			}

			target = next
		}

		typeName := getCoreSanitizedIdentifier(target)

		if isCSharpBuiltinTypeName(target) {
			// A C# BUILT-IN type target (`object` from `type X any`, or a numeric/bool/string
			// primitive) is NOT a package member — import it BARE, never @-escaped or
			// go.<pkg>_package.-qualified. crypto's `type PublicKey any` exports "object"; an
			// importer qualified it to `go.crypto_package.@object`, a nonexistent nested type
			// (CS0426, crypto/md5 + crypto/internal/boring + every crypto importer).
			typeName = target
		} else if strings.HasPrefix(typeName, "const:") {
			typeName = strings.TrimPrefix(typeName, "const:")

			packageLock.Lock()
			constImportedTypeAliases.Add(alias)
			packageLock.Unlock()
		} else if !strings.HasPrefix(typeName, RootNamespace) {
			typeName = fmt.Sprintf("%s.%s%s.%s", RootNamespace, packageName, PackageSuffix, typeName)
		}

		packageLock.Lock()
		importedTypeAliases[alias] = typeName
		importedTypeAliasSourceDirs[alias] = filepath.Clean(info.SourceDir)

		if derived {
			derivedTypeAliases.Add(alias)
		}

		packageLock.Unlock()
	}
}

// loadPackageImplements records a converted package's exported GoImplement pairs from its
// package_info.cs into the imported-implements sets. Split out of loadImportedTypeAliases so
// the -tests path can load the PRODUCTION package's pairs without its alias load
// (visitImportSpec deliberately skips that for the package under test — its types bind
// locally): an EXTERNAL test file's cast of a production type must reference the seeded
// adapter through the aliased qualifier instead of re-recording the pair (B4/B5).
func loadPackageImplements(packageInfoFile string, rootPackageName string) {
	lines, err := readPackageInfoLines(packageInfoFile)

	if err != nil {
		return
	}

	loadPackageImplementLines(lines, rootPackageName)
}

// loadPackageImplementLines is loadPackageImplements over an already-read line set, so the
// embedded standard-library metadata record can seed the same sets as a package_info.cs read
// from disk (see stdlibMetadata.go).
func loadPackageImplementLines(lines []string, rootPackageName string) {
	// Record the package's POINTER-sourced GoImplement pairs: their generated adapter
	// classes (TжIface) are public members of the foreign package class, so a cross-package
	// pointer-to-interface conversion here can reference them by qualified name (io/fs's
	// PathErrorжerror consumed by os - CS0029 x38).
	{
		pairs := parseExportedPointerImplementLines(lines)

		packageLock.Lock()

		for _, pair := range pairs {
			importedPointerImplements.Add(implementRecordKey(rootPackageName, pair[0], pair[1], rootPackageName))
		}

		packageLock.Unlock()
	}

	// Record the package's VALUE-form GoImplement pairs (plain or Promoted): the foreign
	// struct's own assembly implements the interface, so a value cast here converts
	// implicitly and needs no local adapter (see the both-foreign value arm in
	// convertToInterfaceType).
	{
		pairs := parseExportedValueImplementLines(lines)

		packageLock.Lock()

		for _, pair := range pairs {
			importedValueImplements.Add(implementRecordKey(rootPackageName, pair[0], pair[1], rootPackageName))
		}

		packageLock.Unlock()
	}

	// Record the package's published `ref`-receiver primaries (the cross-package lowering
	// contract, refVerdictPublication.go) — read from the same line set, so a NuGet-referenced
	// dependency's embedded metadata seeds them exactly as an on-disk package_info.cs does.
	loadRefPrimaryLines(lines, rootPackageName)
}

// preloadImportedTypeAliases loads the exported type aliases of EVERY package imported by ANY file in
// the current package, BEFORE file conversion begins. importedTypeAliases is package-global but was
// otherwise populated INCREMENTALLY — visitImportSpec loads a package's aliases only when it visits an
// import of it, and files convert in sorted-filename order. So a foreign RENAMED type reached through a
// value whose package the current FILE does not itself import rendered its raw (nonexistent) name when
// that file converted before any file that DOES import the package: go/printer's comment.go
// (`slash := list[0].Slash`, a token.Pos read through ast.Comment, with only go/ast imported) sorts
// first, so its `slash` heap box emitted `heap<go.token_package.Pos>` instead of the alias
// `heap<tokenꓸPos>` (= go.go.token_package.ΔPos) — CS0426. Loading is deduped per imported package
// (parsedPackageInfoFiles), so this only FRONT-LOADS the work visitImportSpec would otherwise do
// incrementally; the resulting alias set is file-order-independent and only ADDS aliases that were
// previously missing for a transitive-use file, so it cannot change a render that already resolved.
func preloadImportedTypeAliases(files []FileEntry, options Options) {
	goroot := filepath.Clean(build.Default.GOROOT)

	for _, fileEntry := range files {
		underGoroot := strings.HasPrefix(filepath.Clean(fileEntry.filePath), goroot+string(filepath.Separator))

		for _, importSpec := range fileEntry.file.Imports {
			importPath := strings.Trim(importSpec.Path.Value, "\"")

			if importPath == "C" {
				continue
			}

			// Match visitImportSpec's GOROOT-vendored resolution so the loader reads the real
			// output location (a GOROOT-vendored golang.org/x import lands under vendor/).
			if underGoroot {
				importPath = resolveGorootVendoredPath(importPath)
			}

			for _, info := range getImportPackageInfo([]string{importPath}, options) {
				loadImportedTypeAliases(info, options)
			}
		}
	}
}

// readPackageInfoLines reads a package info file into its lines. Splitting the file read out
// of the three parsers below lets them run over an in-memory line set too — the embedded
// standard-library metadata record (stdlibMetadata.go), which carries the very same lines for
// a dependency whose converted source is not on disk (-recurse=nuget).
func readPackageInfoLines(packageInfoFile string) ([]string, error) {
	file, err := os.Open(packageInfoFile)

	if err != nil {
		return nil, err
	}

	defer file.Close()

	var lines []string

	scanner := bufio.NewScanner(file)

	for scanner.Scan() {
		lines = append(lines, scanner.Text())
	}

	if err := scanner.Err(); err != nil {
		return nil, err
	}

	return lines, nil
}

// parseExportedTypeAliases parses a package info file and extracts the GoTypeAlias
// entries as tuples of (source, destination) strings
func parseExportedTypeAliases(packageInfoFile string) ([][2]string, error) {
	lines, err := readPackageInfoLines(packageInfoFile)

	if err != nil {
		return nil, err
	}

	return parseExportedTypeAliasLines(lines)
}

// parseExportedTypeAliasLines extracts the GoTypeAlias entries from package-info lines.
func parseExportedTypeAliasLines(lines []string) ([][2]string, error) {
	// Look for the start of the ExportedTypeAliases section
	inSection := false
	var aliases [][2]string

	// Pattern to match: [assembly: GoTypeAlias("Source", "Destination")]
	pattern := regexp.MustCompile(`\[assembly: GoTypeAlias\("([^"]+)", "([^"]+)"\)\]`)

	for _, line := range lines {
		if strings.TrimSpace(line) == "// <ExportedTypeAliases>" {
			inSection = true
			continue
		}

		if strings.TrimSpace(line) == "// </ExportedTypeAliases>" {
			break // End of section reached
		}

		if inSection {
			matches := pattern.FindStringSubmatch(line)

			if len(matches) == 3 {
				// Extract the source and destination as tuple
				alias := [2]string{matches[1], matches[2]}
				aliases = append(aliases, alias)
			}
		}
	}

	if !inSection && len(aliases) == 0 {
		return nil, errors.New("exported type aliases section not found")
	}

	return aliases, nil
}

// parseDynamicTypeLifts parses a package info file and extracts the GoDynamicTypeLift entries as
// (structural signature, lifted C# name) tuples — production's own answer for a purely-anonymous
// struct/interface with no Go-level alias, the sibling of parseExportedTypeAliases for the
// registrations that attribute does not cover. See GoDynamicTypeLiftAttribute
// (src/core/golib/GoDynamicTypeLiftAttribute.cs) and seedProductionDynamicTypeLifts.
func parseDynamicTypeLifts(packageInfoFile string) ([][2]string, error) {
	lines, err := readPackageInfoLines(packageInfoFile)

	if err != nil {
		return nil, err
	}

	return parseDynamicTypeLiftLines(lines)
}

// parseDynamicTypeLiftLines extracts the GoDynamicTypeLift entries from package-info lines. Shares
// the ExportedTypeAliases section rather than owning a new one — a GoDynamicTypeLift line is simply
// a record parseExportedTypeAliasLines' own GoTypeAlias-only pattern does not match, so the two
// record kinds coexist in one block with neither parser seeing the other's lines.
func parseDynamicTypeLiftLines(lines []string) ([][2]string, error) {
	inSection := false
	var lifts [][2]string

	// Pattern to match: [assembly: GoDynamicTypeLift("<hex signature>", "TypeName")]
	pattern := regexp.MustCompile(`\[assembly: GoDynamicTypeLift\("([0-9a-f]+)", "([^"]+)"\)\]`)

	for _, line := range lines {
		if strings.TrimSpace(line) == "// <ExportedTypeAliases>" {
			inSection = true
			continue
		}

		if strings.TrimSpace(line) == "// </ExportedTypeAliases>" {
			break
		}

		if !inSection {
			continue
		}

		matches := pattern.FindStringSubmatch(line)

		if len(matches) != 3 {
			continue
		}

		signature, err := hex.DecodeString(matches[1])

		if err != nil {
			// Cannot happen for a line this converter wrote — the pattern's own charclass already
			// restricts the payload to hex digits — but a hand-edited or corrupted file must not
			// panic the reader that meant only to seed a `-tests` conversion.
			continue
		}

		lifts = append(lifts, [2]string{string(signature), matches[2]})
	}

	return lifts, nil
}

// implementRecordKey composes the ONE spelling of a GoImplement pair that BOTH sides of the lookup
// can compute — the LOAD side, reading a dependency's package_info.cs, and the USE side, converting
// a cast. It serves BOTH record sets, the VALUE form and the POINTER form, because the two sides of
// each set diverged in exactly the same two places and there is no reason for a second naming path.
// Before this existed the two sides composed from different alphabets and a match could only ever
// fire for a single-segment import path:
//
//	image/color   load `color|ΔRGBA|color_package.Color`   use `color|RGBA|image.color_package.Color`
//	encoding/bin. load `binary|bigEndian|binary_package.ByteOrder`
//	                                     use `binary|bigEndian|encoding.binary_package.ByteOrder`
//	io            load `io|noBody|io_package.ReadCloser`   use `io|noBody|io_package.ReadCloser`  (match)
//
// Two divergences, both closed here. (1) The TYPE side: the record carries the emitted C# name
// (image/color's `RGBA` is collision-renamed to `ΔRGBA`) while the use side named the GO type — so
// every collision-renamed type missed on top of the path miss. Both sides now reduce the C# name.
// (2) The INTERFACE side: see canonicalImplementRecordIfaceName.
//
// The DECLARING-package component is what keeps a record trustworthy: a package may record a pair
// for a type declared in a THIRD assembly (image re-declares image/color's models), and go2cs-gen
// realizes THAT as a local adapter class, not as the type implementing the interface. The use side
// names the TARGET's package, so such a record can never satisfy a cast — `image|Alpha|…` against
// `color|Alpha|…`.
//
// What the two sets do NOT share is what a matched record is trusted to MEAN. A VALUE record says
// only that the declaring assembly implements the pair somehow, so the use site must still ask HOW
// (valueRecordRealizesAsPartialStruct — a named FUNC type is a C# delegate and takes the adapter
// route). A POINTER record is already an adapter-class EXISTENCE signal: `Pointer = true` is exactly
// the shape go2cs-gen realizes as `<T>ж<Iface>`, so there is nothing further to ask.
func implementRecordKey(declaringPackageName string, csTypeName string, ifaceName string, ifacePackageName string) string {
	return fmt.Sprintf("%s|%s|%s", getSanitizedIdentifier(declaringPackageName),
		removeLeadingSanitizationMarker(simpleCSTypeName(csTypeName)),
		canonicalImplementRecordIfaceName(ifaceName, ifacePackageName))
}

// simpleCSTypeName reduces a rendered C# type name to its last dot-segment — the form both a
// package_info record's type argument and a cast site's rendered target agree on.
func simpleCSTypeName(name string) string {
	if idx := strings.LastIndex(name, "."); idx >= 0 {
		return name[idx+1:]
	}

	return name
}

// canonicalImplementRecordIfaceName reduces a record's INTERFACE side to the class-relative
// spelling. A record PARSED from a package_info file names an interface the recording package
// declares BARE (`Color`) and a foreign one whole (`go.image.color_package.Color`), while a CAST
// SITE always renders the whole namespace chain — so the two spellings of one pair only ever agreed
// when the chain was a single segment. Dropping everything ahead of the `<pkg>_package` segment
// makes them one spelling. Neither direction of the divergence is the "long" one: go/types records
// its OWN `Object` whole (`go.go.types_package.Object`) where the cast site renders it short, and
// text/template/parse records its own `Node` bare where the cast site renders it whole. Which
// spelling a file produces depends on its own using/alias context, which is exactly why neither
// side's raw text can be the key.
//
// The SIMPLE name alone is NOT enough — image's Paletted→image.Image record must not satisfy a
// Paletted→draw.Image cast (both reduce to "Image") — so the package class stays. A member path
// under the class (`y_package.Outer.Inner`) survives intact, which is why this keeps the tail rather
// than taking the last two segments.
//
// This deliberately reverses the earlier non-collapse ruling, which kept the mismatch because a
// FOREIGN pair matching the declaring package's own record suppresses the LOCAL record the consumer
// needs (expvar / net/http/cgi: `HandlerFunc` → `ΔHandler`, CS0029). That hazard is real but it is
// not about the KEY: it is about HOW the declaring assembly realized the pair, and it is now gated
// precisely, at the value use site, by valueRecordRealizesAsPartialStruct. The POINTER set needs no
// such gate — see implementRecordKey.
func canonicalImplementRecordIfaceName(ifaceName string, ifacePackageName string) string {
	ifaceName = strings.TrimPrefix(ifaceName, RootNamespace+".")

	if !strings.Contains(ifaceName, ".") {
		// The universe `error` is not a package member.
		if ifaceName == "error" {
			return ifaceName
		}

		return getSanitizedIdentifier(ifacePackageName) + PackageSuffix + "." + ifaceName
	}

	parts := strings.Split(ifaceName, ".")

	for i, part := range parts {
		if strings.HasSuffix(part, PackageSuffix) {
			return strings.Join(parts[i:], ".")
		}
	}

	return ifaceName
}

// valueRecordRealizesAsPartialStruct reports whether a dependency's VALUE-form GoImplement record
// can be trusted to mean "the declaring assembly's own type implements the interface" — which is
// what lets a cast site drop its local ᴠ value adapter and hand over the bare value.
//
// go2cs-gen realizes a value pair as `partial struct T : Iface` for EVERY named Go type — struct,
// slice (`[GoType("[]Color")] partial struct Palette`), map, channel, numeric
// (`[GoType("num:nint")] partial struct ΔSignal`) — with exactly ONE exception: a named FUNC type
// arrives as a C# DELEGATE, which cannot be a partial struct, so ImplementGenerator's
// TypeKind.Delegate arm emits an adapter CLASS in the declaring assembly instead. The record itself
// says nothing about which route was taken, so the target's Go underlying is the gate: trusting a
// *types.Signature record drops the adapter and emits a bare delegate into an interface slot —
// CS0029 for net/http's `HandlerFunc` → `ΔHandler` in expvar, net/http/cgi and three more.
// (An interface-underlying target never reaches a value arm; recordableBase excludes it.)
func valueRecordRealizesAsPartialStruct(targetType types.Type) bool {
	named, ok := types.Unalias(targetType).(*types.Named)

	if !ok {
		return false
	}

	_, isSignature := named.Underlying().(*types.Signature)

	return !isSignature
}

// parseExportedPointerImplementLines parses package-info lines for `GoImplement<T, Iface>(Pointer
// = true)` assembly attributes, returning (T-simple, Iface-qualified) pairs - the adapter-class
// existence records for cross-package pointer-to-interface conversions.
//
// Parsing cannot fail: a line that does not match the pattern is simply not a record and is
// skipped, so an unrecognized or malformed package_info yields an empty result rather than an
// error. There is no error to return, and none is.
func parseExportedPointerImplementLines(lines []string) [][2]string {
	var pairs [][2]string

	pattern := regexp.MustCompile(`\[assembly: GoImplement<(.+), (.+)>\(Pointer = true\)\]`)

	for _, line := range lines {
		matches := pattern.FindStringSubmatch(line)

		if matches == nil {
			continue
		}

		// The struct side reduces to its SIMPLE (last-dot-segment) name - the adapter class
		// name composes from exactly that (see adapterTypeRef / the ImplementGenerator). The
		// INTERFACE side keeps its qualifier (canonicalized at the populate site).
		pairs = append(pairs, [2]string{simpleCSTypeName(matches[1]), matches[2]})
	}

	return pairs
}

// parseExportedValueImplementLines parses package-info lines for VALUE-form `GoImplement<T, Iface>`
// assembly attributes (plain or `(Promoted = true)`), returning (T-simple, Iface-simple) pairs -
// records that the defining assembly itself implements the interface on the value type.
//
// Like its pointer-form sibling this cannot fail — a non-matching line is just not a record.
func parseExportedValueImplementLines(lines []string) [][2]string {
	var pairs [][2]string

	pattern := regexp.MustCompile(`\[assembly: GoImplement<(.+), (.+)>(?:\(Promoted = true\))?\]`)

	for _, line := range lines {
		matches := pattern.FindStringSubmatch(line)

		if matches == nil {
			continue
		}

		// A Pointer-form record is the ADAPTER existence signal, not a value implementation.
		if strings.Contains(line, "(Pointer = true)") {
			continue
		}

		// The struct side reduces to its SIMPLE (last-dot-segment) name; the INTERFACE side
		// keeps its qualifier (canonicalized at the populate site — the simple name collides
		// across same-named interfaces, see canonicalImplementRecordIfaceName).
		pairs = append(pairs, [2]string{simpleCSTypeName(matches[1]), matches[2]})
	}

	return pairs
}
