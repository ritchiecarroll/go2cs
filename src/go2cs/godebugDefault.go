// godebugDefault.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"sort"
	"strconv"
	"strings"

	"golang.org/x/mod/modfile"
)

// DefaultGODEBUG (DESIGN-multi-package-modules D4, seat M6): the GODEBUG defaults cmd/go bakes into a Go
// binary as runtime.godebugDefault. A module whose `go` line is older than the toolchain gets, for every
// setting changed since, the OLD behavior -- Go's backward-compatibility promise -- and its go.mod
// `godebug` block and its //go:debug directives override that. Reproduced from cmd/go/internal/load/
// godebug.go (defaultGODEBUG / godebugForGoVersion) and stamped on the entry assembly as
// [assembly: go.GoDefaultGodebug("...")], which the runtime and internal/godebug read at start-up.
//
// Emitted only when non-empty: a module at the corpus release with no overrides emits nothing, and the
// standard library (`module std`, at the corpus release) never reaches this code at all.

// godebugTableEntry is one row of internal/godebugs.All that matters here.
type godebugTableEntry struct {
	name    string
	changed int
	old     string
}

// readGodebugsTable reads internal/godebugs.All from the converting GOROOT's own source, so the table is
// always the release being converted -- never a copy that can drift from it.
func readGodebugsTable(goRoot string) ([]godebugTableEntry, error) {
	path := filepath.Join(goRoot, "src", "internal", "godebugs", "table.go")
	file, err := parser.ParseFile(token.NewFileSet(), path, nil, 0)

	if err != nil {
		return nil, fmt.Errorf("reading the godebugs table: %w", err)
	}

	var entries []godebugTableEntry

	ast.Inspect(file, func(node ast.Node) bool {
		literal, ok := node.(*ast.CompositeLit)

		if !ok {
			return true
		}

		var entry godebugTableEntry
		named := false

		for _, element := range literal.Elts {
			pair, ok := element.(*ast.KeyValueExpr)

			if !ok {
				continue
			}

			key, ok := pair.Key.(*ast.Ident)
			value, isLiteral := pair.Value.(*ast.BasicLit)

			if !ok || !isLiteral {
				continue
			}

			switch key.Name {
			case "Name":
				entry.name, _ = strconv.Unquote(value.Value)
				named = true
			case "Changed":
				entry.changed, _ = strconv.Atoi(value.Value)
			case "Old":
				entry.old, _ = strconv.Unquote(value.Value)
			}
		}

		if named {
			entries = append(entries, entry)
			return false
		}

		return true
	})

	if len(entries) == 0 {
		return nil, fmt.Errorf("reading the godebugs table: no entries found in %s", path)
	}

	return entries, nil
}

// godebugSetting is one `key=value` override, in the order it was declared.
type godebugSetting struct {
	key   string
	value string
}

// goModDefaultGoVersion is the language version cmd/go gives a go.mod with no `go` line.
const goModDefaultGoVersion = "1.16"

// moduleGodebug reads a module's `go` line and its go.mod `godebug` block. STRICT modfile.Parse: ParseLax
// DROPS godebug directives, so the lax parser would silently lose exactly the overrides read here.
func moduleGodebug(moduleDir string) (string, []godebugSetting, error) {
	path, ok := goModFile(moduleDir)

	if !ok {
		path = filepath.Join(moduleDir, "go.mod")
	}

	data, err := os.ReadFile(path)

	if err != nil {
		return "", nil, err
	}

	parsed, err := modfile.Parse(path, data, nil)

	if err != nil {
		return "", nil, err
	}

	// A go.mod with no `go` line is a go 1.16 module to cmd/go (gover.DefaultGoModVersion) -- every
	// module-cache release that predates modules reads this way (github.com/pkg/errors v0.9.1: `go list`
	// reports GoVersion 1.16 and a DefaultGODEBUG carrying panicnil=1). Without the default the module
	// read as the corpus release and its program was stamped with nothing.
	goLine := goModDefaultGoVersion

	if parsed.Go != nil {
		goLine = parsed.Go.Version
	}

	var settings []godebugSetting

	for _, godebug := range parsed.Godebug {
		settings = append(settings, godebugSetting{key: godebug.Key, value: godebug.Value})
	}

	return goLine, settings, nil
}

// fileGoDebugDirectives reads the `//go:debug key=value` directives of Go source files -- the comments
// before each file's package clause, as go/build records them -- in file order.
func fileGoDebugDirectives(files []string) []godebugSetting {
	var settings []godebugSetting

	for _, path := range files {
		file, err := parser.ParseFile(token.NewFileSet(), path, nil, parser.PackageClauseOnly|parser.ParseComments)

		if err != nil {
			continue
		}

		for _, group := range file.Comments {
			if group.Pos() >= file.Package {
				break
			}

			for _, comment := range group.List {
				text, ok := strings.CutPrefix(comment.Text, "//go:debug")

				if !ok || (text != "" && text[0] != ' ' && text[0] != '\t') {
					continue
				}

				if key, value, ok := strings.Cut(strings.TrimSpace(text), "="); ok {
					settings = append(settings, godebugSetting{key: strings.TrimSpace(key), value: strings.TrimSpace(value)})
				}
			}
		}
	}

	return settings
}

// defaultGODEBUG reproduces cmd/go's algorithm: the overrides (module block first, then file directives,
// later winning), a `default=goX.Y` override of the go line, then every table entry changed AFTER the go
// line at its old value, with the overrides on top -- sorted `k=v` joined by commas.
func defaultGODEBUG(goLine string, overrides []godebugSetting, table []godebugTableEntry) string {
	explicit := make(map[string]string)

	for _, setting := range overrides {
		explicit[setting.key] = setting.value
	}

	if version, ok := explicit["default"]; ok {
		delete(explicit, "default")
		goLine = strings.TrimPrefix(version, "go")
	}

	defaults := make(map[string]string)

	if minor, ok := goMinorVersion(goLine); ok {
		for _, entry := range table {
			if minor < entry.changed {
				defaults[entry.name] = entry.old
			}
		}
	}

	for key, value := range explicit {
		defaults[key] = value
	}

	keys := make([]string, 0, len(defaults))

	for key := range defaults {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	var result strings.Builder

	for i, key := range keys {
		if i > 0 {
			result.WriteByte(',')
		}

		result.WriteString(key + "=" + defaults[key])
	}

	return result.String()
}

// goMinorVersion is the N of a `1.N[.P]` go line (cmd/go truncates to the language version).
func goMinorVersion(goLine string) (int, bool) {
	parts := strings.Split(strings.TrimPrefix(goLine, "go"), ".")

	if len(parts) < 2 || parts[0] != "1" {
		return 0, false
	}

	minor, err := strconv.Atoi(strings.TrimRightFunc(parts[1], func(r rune) bool { return r < '0' || r > '9' }))

	return minor, err == nil
}

// packageDefaultGODEBUG is a NON-standard-library package's DefaultGODEBUG: its module's go line and
// godebug block, plus the //go:debug directives of the given files (the package's, and for a test binary
// its test and xtest files too). "" for a standard-library package, a package with no go.mod above it,
// or a module at the corpus release with no overrides.
func packageDefaultGODEBUG(packageDir string, files []string, options Options) (string, error) {
	if stdLibImportPathOf(packageDir, options.goRoot) != "" {
		return "", nil
	}

	moduleDir := moduleRootDir(packageDir)

	if _, ok := goModFile(moduleDir); !ok {
		return "", nil
	}

	goLine, moduleSettings, err := moduleGodebug(moduleDir)

	if err != nil {
		return "", fmt.Errorf("DefaultGODEBUG: %w", err)
	}

	table, err := readGodebugsTable(options.goRoot)

	if err != nil {
		return "", fmt.Errorf("DefaultGODEBUG: %w", err)
	}

	return defaultGODEBUG(goLine, append(moduleSettings, fileGoDebugDirectives(files)...), table), nil
}

// stampDefaultGodebugProject adds the DefaultGODEBUG to a program's project file as an MSBuild
// AssemblyAttribute item (the SDK generates the [assembly: ...] line from it). A no-op for "".
func stampDefaultGodebugProject(projectFileName string, value string) error {
	if value == "" {
		return nil
	}

	data, err := os.ReadFile(projectFileName)

	if err != nil {
		return err
	}

	project := string(data)
	end := strings.LastIndex(project, "</Project>")

	if end < 0 {
		return fmt.Errorf("DefaultGODEBUG: no </Project> in %s", projectFileName)
	}

	item := "  <ItemGroup>\r\n" +
		"    <!-- The module's DefaultGODEBUG (cmd/go's runtime.godebugDefault), read by the runtime at start-up. -->\r\n" +
		"    <AssemblyAttribute Include=\"go.GoDefaultGodebugAttribute\">\r\n" +
		"      <_Parameter1>" + escapeXMLAttributeValue(value) + "</_Parameter1>\r\n" +
		"    </AssemblyAttribute>\r\n" +
		"  </ItemGroup>\r\n\r\n"

	return os.WriteFile(projectFileName, []byte(project[:end]+item+project[end:]), 0o644)
}

// defaultGodebugAttribute is the assembly attribute line carrying a DefaultGODEBUG, or "" for none.
func defaultGodebugAttribute(value string) string {
	if value == "" {
		return ""
	}

	return fmt.Sprintf("[assembly: go.GoDefaultGodebug(\"%s\")]", escapeCSharp(value))
}
