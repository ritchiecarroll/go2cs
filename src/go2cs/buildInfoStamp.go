// buildInfoStamp.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"strings"

	"golang.org/x/mod/modfile"
	"golang.org/x/mod/module"
	"golang.org/x/tools/go/packages"
)

// The module half of a program's build info: the `path`, `mod` and `dep` lines cmd/go links into a Go binary
// for runtime/debug.ReadBuildInfo, stamped on a converted program's entry assembly as
// [assembly: go.GoModInfo("...")] through its project file -- DefaultGODEBUG's project route
// (godebugDefault.go), and not package_info.cs, whose content a -tests run seeds into the test assembly.
//
// Only the module half is converter-known. The `build` lines (GOOS, GOARCH, CGO_ENABLED, DefaultGODEBUG) are
// composed at run time from the binary itself (golib GoModInfoAttribute.Compose), and the settings with no
// honest managed value (-compiler, -buildmode, GOAMD64, vcs.*) are never stamped. A converted TEST host
// carries no stamp at all: its TestRegistry values are registered at start-up instead.

// programModInfo is a main package's stamp in Go's own line form: `path <import path>`, then `mod <module>
// (devel)` -- the version cmd/go reports for a main module built outside version control -- then one
// `dep <path> <version> <sum>` per third-party module, its sum the main module's go.sum `h1:` hash (read by
// modulesLock.go's goSumHashes, the same reading go2cs.modules.lock records). "" outside a
// module: a GOPATH-mode Go binary embeds no module info, and its import path can be a machine path, which must
// never reach an emitted project.
//
// A REPLACED dependency is written as cmd/go writes its dep line -- path and version, no sum column, since Go puts
// the sum on the `=>` line that follows -- and that `=>` line is NOT stamped: a replacement can be a local
// directory, an absolute machine path, and no machine path is emitted. ReadBuildInfo therefore reports such a dep
// with no Replace (a BOARD-recorded divergence, 2026-10-22).
func programModInfo(importPath, modulePath string, dependencies []thirdPartyModule, sums map[string]string) string {
	if modulePath == "" {
		return ""
	}

	var lines strings.Builder

	lines.WriteString("path\t" + importPath + "\n")
	lines.WriteString("mod\t" + modulePath + "\t(devel)\t\n")

	for _, dependency := range dependencies {
		if dependency.replaced {
			lines.WriteString("dep\t" + dependency.path + "\t" + dependency.version + "\n")
			continue
		}

		lines.WriteString("dep\t" + dependency.path + "\t" + dependency.version + "\t" + sums[dependency.path+" "+dependency.version] + "\n")
	}

	return lines.String()
}

// directoryModulePath is the path of the module a package directory belongs to, or "" when it has no go.mod
// above it (or the go.mod declares no module).
func directoryModulePath(packageDir string) string {
	path, ok := goModFile(moduleRootDir(packageDir))

	if !ok {
		return ""
	}

	data, err := os.ReadFile(path)

	if err != nil {
		return ""
	}

	return modfile.ModulePath(data)
}

// modInfoXMLEscapes writes the stamp's tabs and newlines as character references, so the item stays on one
// line of the project file; MSBuild hands the decoded value to the generated [assembly: ...] line unchanged.
var modInfoXMLEscapes = strings.NewReplacer(
	"&", "&amp;",
	"<", "&lt;",
	">", "&gt;",
	"\t", "&#9;",
	"\n", "&#10;",
)

// stampModInfoProject adds a program's module build info to its project file as an MSBuild
// AssemblyAttribute item (the SDK generates the [assembly: ...] line from it). A no-op for "".
func stampModInfoProject(projectFileName string, value string) error {
	if value == "" {
		return nil
	}

	item := "  <ItemGroup>\r\n" +
		"    <AssemblyAttribute Include=\"go.GoModInfoAttribute\">\r\n" +
		"      <_Parameter1>" + modInfoXMLEscapes.Replace(value) + "</_Parameter1>\r\n" +
		"    </AssemblyAttribute>\r\n" +
		"  </ItemGroup>\r\n\r\n"

	return insertProjectItem(projectFileName, item)
}

// testBinaryDependencies is a converted test host's `dep` lines, as cmd/go links them into a test binary. A test of a
// MAIN package copies that package's own build info (cmd/go/internal/load/test.go TestPackagesAndErrors), whose deps
// setBuildInfo collects by walking the package's imports and recording each module once, outside the main module. A
// test of any other package gets build info re-derived for the test main before its imports are known, so it lists
// NO deps (measured: go1.26.10, linux/amd64, a module package importing golang.org/x/sync). The walk is therefore the
// production package's import closure, test-only imports excluded, and nothing for a non-main package. A
// standard-library package carries no module. Each entry is a dep line's fields joined by tabs -- path, version and
// the go.sum `h1:` hash, sorted as cmd/go sorts them -- and a REPLACED module keeps only path and version, as
// programModInfo writes it.
func testBinaryDependencies(production *packages.Package, mainModulePath string, sums map[string]string) []string {
	if production == nil || production.Name != "main" {
		return nil
	}

	visited := map[*packages.Package]bool{}
	modules := map[module.Version]*packages.Module{}
	queue := []*packages.Package{production}

	for len(queue) > 0 {
		pkg := queue[0]
		queue = queue[1:]

		if visited[pkg] {
			continue
		}

		visited[pkg] = true

		if pkg.Module != nil && pkg.Module.Path != mainModulePath {
			modules[module.Version{Path: pkg.Module.Path, Version: pkg.Module.Version}] = pkg.Module
		}

		for _, imported := range pkg.Imports {
			queue = append(queue, imported)
		}
	}

	sorted := make([]module.Version, 0, len(modules))

	for version := range modules {
		sorted = append(sorted, version)
	}

	module.Sort(sorted)

	lines := make([]string, 0, len(sorted))

	for _, version := range sorted {
		path, revision := version.Path, version.Version

		if revision == "" {
			revision = "(devel)"
		}

		if modules[version].Replace != nil {
			lines = append(lines, path+"\t"+revision)
			continue
		}

		lines = append(lines, path+"\t"+revision+"\t"+sums[path+" "+version.Version])
	}

	return lines
}
