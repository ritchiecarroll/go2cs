// sourcemeta.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Package sourcemeta is the self-description a published go2cs conversion of a Go module carries
// (docs/PLAN-nugetgo.md section 5, stage S2; format v1 as COORD ruled it on 2026-10-02).
//
// A consuming converter cannot read a NuGet-referenced package's package_info.cs off disk, so the package
// carries what the converter needs instead, at go2cs/source-metadata.txt inside the nupkg:
//
//	#go2cs-source-metadata v1
//	module <module path>
//	module-version <the Go module version converted, with its v>
//	go2cs-release <the go2cs corpus release it was built against, e.g. 1.24.13.3>
//	require <module> <version> <nuget-id>     one per third-party module the assemblies depend on
//	package <import path> <assembly name>     one per packed Go package
//	##<dotted package name>[@<goos>]          then that package's metadata lines, verbatim
//
// The sections are stdlib-metadata.txt's own format, taken from each package's package_info.cs by the same
// collector (internal/stdlibmeta), so the converter's existing record parsers read them unchanged. UTF-8, no
// byte-order mark, LF line endings; '#' lines are comments and blank lines are skipped. This package owns
// BOTH the writer (the pack, through gensourcemeta) and the strict parser the consumer reads it with, and
// the two share one validation, so a pack can never produce a file its consumer refuses.
package sourcemeta

import (
	"bytes"
	"fmt"
	"regexp"
	"sort"
	"strings"

	"go2cs/internal/stdlibmeta"

	"golang.org/x/mod/module"
	"golang.org/x/mod/semver"
)

// EntryPath is where the self-description sits inside a nupkg: a PackagePath, never contentFiles, so a
// consuming project never copies it into its own output.
const EntryPath = "go2cs/source-metadata.txt"

// Magic is the self-description's first line; its last word is the schema version.
const Magic = "#go2cs-source-metadata v1"

const magicPrefix = "#go2cs-source-metadata "

const comment = "# Written by go2cs (internal/gensourcemeta) when a converted Go module is packed; do not edit. Format: docs/PLAN-nugetgo.md section 5."

// nugetIDPattern is the nugetgo.net registry lint's rule for a NuGet ID (relayed by COORD, 2026-10-02).
var nugetIDPattern = regexp.MustCompile(`^[A-Za-z0-9_]+([.-][A-Za-z0-9_]+)*$`)

const nugetIDMaxLength = 100

// Require is one third-party module the packed assemblies depend on, at the version they were built against,
// and the NuGet ID that module is published under.
type Require struct {
	Module  string
	Version string
	NuGetID string
}

// Package is one packed Go package and the assembly it compiled to.
type Package struct {
	ImportPath string
	Assembly   string
}

// Description is a self-description's content. Sections is keyed by dotted package name, with
// "@<goos>" for a non-reference platform flavor, exactly as in stdlib-metadata.txt.
type Description struct {
	Module        string
	ModuleVersion string
	Go2csRelease  string
	Requires      []Require
	Packages      []Package
	Sections      map[string][]string
}

// sectionName is a package's section key: its import path dotted, the stdlib record's spelling.
func sectionName(importPath string) string {
	return strings.ReplaceAll(importPath, "/", ".")
}

// Collect returns the metadata sections of the packed packages, taken from their package_info.cs files under
// srcRoot (a -recurse output root's src/ tree) by the stdlib record's own collector. A packed package without
// a package_info.cs is refused by name: its consumer would otherwise read "no metadata" as "none exists".
func Collect(srcRoot string, packages []Package) (map[string][]string, error) {
	all, err := stdlibmeta.Collect(srcRoot)

	if err != nil {
		return nil, err
	}

	sections := make(map[string][]string)

	for _, pkg := range packages {
		name := sectionName(pkg.ImportPath)
		lines, found := all[name]

		if !found {
			return nil, fmt.Errorf("package %s has no %s under %s", pkg.ImportPath, stdlibmeta.PackageInfoFileName, srcRoot)
		}

		sections[name] = lines

		for key, flavor := range all {
			if strings.HasPrefix(key, name+stdlibmeta.FlavorSeparator) {
				sections[key] = flavor
			}
		}
	}

	return sections, nil
}

// Generate renders a self-description deterministically: requires sorted by module, packages by import path,
// sections by name. It refuses anything Parse would refuse.
func Generate(d Description) ([]byte, error) {
	if err := validate(d); err != nil {
		return nil, err
	}

	requires := append([]Require(nil), d.Requires...)
	sort.Slice(requires, func(i, j int) bool { return requires[i].Module < requires[j].Module })
	packages := append([]Package(nil), d.Packages...)
	sort.Slice(packages, func(i, j int) bool { return packages[i].ImportPath < packages[j].ImportPath })
	names := make([]string, 0, len(d.Sections))

	for name := range d.Sections {
		names = append(names, name)
	}

	sort.Strings(names)

	var out bytes.Buffer
	fmt.Fprintf(&out, "%s\n%s\n", Magic, comment)
	fmt.Fprintf(&out, "module %s\nmodule-version %s\ngo2cs-release %s\n", d.Module, d.ModuleVersion, d.Go2csRelease)

	for _, r := range requires {
		fmt.Fprintf(&out, "require %s %s %s\n", r.Module, r.Version, r.NuGetID)
	}

	for _, p := range packages {
		fmt.Fprintf(&out, "package %s %s\n", p.ImportPath, p.Assembly)
	}

	for _, name := range names {
		fmt.Fprintf(&out, "\n%s%s\n", stdlibmeta.SectionPrefix, name)

		for _, line := range d.Sections[name] {
			out.WriteString(line)
			out.WriteString("\n")
		}
	}

	return out.Bytes(), nil
}

// Parse reads a self-description strictly. Every refusal says what is malformed and, where it is one line's
// fault, which line: the consumer falls back to local conversion and its warning quotes this text.
func Parse(data []byte) (Description, error) {
	if bytes.HasPrefix(data, []byte{0xEF, 0xBB, 0xBF}) {
		return Description{}, fmt.Errorf("line 1: a byte-order mark (the file is UTF-8 without one)")
	}

	lines := strings.Split(string(data), "\n")

	if len(lines) > 0 && lines[len(lines)-1] == "" {
		lines = lines[:len(lines)-1]
	}

	for index, line := range lines {
		if strings.Contains(line, "\r") {
			return Description{}, fmt.Errorf("line %d: a carriage return (the file is written with LF line endings)", index+1)
		}
	}

	if len(lines) == 0 || lines[0] != Magic {
		first := ""

		if len(lines) > 0 {
			first = lines[0]
		}

		if strings.HasPrefix(first, magicPrefix) {
			return Description{}, fmt.Errorf("line 1: schema %q; this converter reads %q", strings.TrimPrefix(first, magicPrefix), strings.TrimPrefix(Magic, magicPrefix))
		}

		return Description{}, fmt.Errorf("line 1: not a go2cs self-description (want %q)", Magic)
	}

	d := Description{Sections: make(map[string][]string)}
	seen := make(map[string]bool)
	current := ""

	for index := 1; index < len(lines); index++ {
		line, number := lines[index], index+1

		if strings.HasPrefix(line, stdlibmeta.SectionPrefix) {
			current = strings.TrimPrefix(line, stdlibmeta.SectionPrefix)

			if _, duplicate := d.Sections[current]; duplicate || current == "" {
				return Description{}, fmt.Errorf("line %d: section %q is empty or repeated", number, current)
			}

			d.Sections[current] = nil
			continue
		}

		if strings.TrimSpace(line) == "" || strings.HasPrefix(line, "#") {
			continue
		}

		if current != "" {
			d.Sections[current] = append(d.Sections[current], line)
			continue
		}

		key, rest, _ := strings.Cut(line, " ")
		fields := strings.Fields(rest)

		switch key {
		case "module", "module-version", "go2cs-release":
			if seen[key] {
				return Description{}, fmt.Errorf("line %d: a second %s line", number, key)
			}

			if len(fields) != 1 {
				return Description{}, fmt.Errorf("line %d: %s needs exactly one value", number, key)
			}

			seen[key] = true

			switch key {
			case "module":
				d.Module = fields[0]
			case "module-version":
				d.ModuleVersion = fields[0]
			default:
				d.Go2csRelease = fields[0]
			}
		case "require":
			if len(fields) != 3 {
				return Description{}, fmt.Errorf("line %d: require needs <module> <version> <nuget-id>, got %q", number, rest)
			}

			r := Require{Module: fields[0], Version: fields[1], NuGetID: fields[2]}

			if err := validateRequire(r); err != nil {
				return Description{}, fmt.Errorf("line %d: %v", number, err)
			}

			d.Requires = append(d.Requires, r)
		case "package":
			if len(fields) != 2 {
				return Description{}, fmt.Errorf("line %d: package needs <import-path> <assembly>, got %q", number, rest)
			}

			d.Packages = append(d.Packages, Package{ImportPath: fields[0], Assembly: fields[1]})
		default:
			return Description{}, fmt.Errorf("line %d: unknown key %q (a v1 self-description has module, module-version, go2cs-release, require and package lines before its sections)", number, key)
		}
	}

	if err := validate(d); err != nil {
		return Description{}, err
	}

	return d, nil
}

// validate is the one rule set Generate and Parse share.
func validate(d Description) error {
	if err := module.CheckPath(d.Module); err != nil {
		return fmt.Errorf("module %q: %v", d.Module, err)
	}

	if !semver.IsValid(d.ModuleVersion) {
		return fmt.Errorf("module-version %q is not a semantic version with its leading v", d.ModuleVersion)
	}

	if d.Go2csRelease == "" || strings.ContainsAny(d.Go2csRelease, " \t") {
		return fmt.Errorf("go2cs-release %q is missing or not a single word", d.Go2csRelease)
	}

	seenRequire := make(map[string]bool)

	for _, r := range d.Requires {
		if err := validateRequire(r); err != nil {
			return err
		}

		if seenRequire[r.Module] {
			return fmt.Errorf("require %s listed twice", r.Module)
		}

		seenRequire[r.Module] = true
	}

	if len(d.Packages) == 0 {
		return fmt.Errorf("no package line: a self-description describes at least one packed package")
	}

	packageSections := make(map[string]bool)

	for _, p := range d.Packages {
		if p.ImportPath != d.Module && !strings.HasPrefix(p.ImportPath, d.Module+"/") {
			return fmt.Errorf("package %s is outside module %s", p.ImportPath, d.Module)
		}

		if err := module.CheckImportPath(p.ImportPath); err != nil {
			return fmt.Errorf("package %q: %v", p.ImportPath, err)
		}

		if p.Assembly == "" || strings.ContainsAny(p.Assembly, " \t") {
			return fmt.Errorf("package %s: assembly name %q is missing or not a single word", p.ImportPath, p.Assembly)
		}

		name := sectionName(p.ImportPath)

		if packageSections[name] {
			return fmt.Errorf("package %s listed twice", p.ImportPath)
		}

		packageSections[name] = true

		if _, found := d.Sections[name]; !found {
			return fmt.Errorf("package %s has no %s%s section", p.ImportPath, stdlibmeta.SectionPrefix, name)
		}
	}

	for name, lines := range d.Sections {
		base, flavor, flavored := strings.Cut(name, stdlibmeta.FlavorSeparator)

		if !packageSections[base] || (flavored && flavor == "") {
			return fmt.Errorf("section %s%s names no listed package", stdlibmeta.SectionPrefix, name)
		}

		for _, line := range lines {
			if strings.TrimSpace(line) == "" || strings.HasPrefix(line, "#") || strings.ContainsAny(line, "\r\n") {
				return fmt.Errorf("section %s%s: line %q would not read back as a record", stdlibmeta.SectionPrefix, name, line)
			}
		}
	}

	return nil
}

// CheckModuleNuGetID refuses a converted MODULE's NuGet ID that takes the "go." prefix, in any letter case (NuGet IDs
// are case-insensitive): "go." is the converted Go standard library, and a module's ID is the "nugetgo." form (owner
// ruling, 2026-10-02). It mirrors the registry's lint exactly (nugetgo cmd/sitegen/parse.go at 90cc7d7409): the PREFIX
// only, the dot included, so "golang.x", "gopher.x", "go-x.y" and the bare "go" pass; it requires nothing else; and a
// caller runs it AFTER its own length and validity checks, so a malformed ID is refused for that alone. A
// standard-library reference is never a module ID, so it never reaches this check.
func CheckModuleNuGetID(id string) error {
	if strings.HasPrefix(strings.ToLower(id), "go.") {
		return fmt.Errorf(`nuget-id "%s" uses the "go." prefix, which is the converted Go standard library; the ID of a converted module starts with "nugetgo."`, id)
	}

	return nil
}

// validateRequire checks one require line's three fields.
func validateRequire(r Require) error {
	if err := module.CheckPath(r.Module); err != nil {
		return fmt.Errorf("require module %q: %v", r.Module, err)
	}

	if !semver.IsValid(r.Version) {
		return fmt.Errorf("require %s: version %q is not a semantic version with its leading v", r.Module, r.Version)
	}

	if !nugetIDPattern.MatchString(r.NuGetID) || len(r.NuGetID) > nugetIDMaxLength {
		return fmt.Errorf("require %s: invalid nuget-id %q (the registry's rule: %s, at most %d characters)", r.Module, r.NuGetID, nugetIDPattern, nugetIDMaxLength)
	}

	if err := CheckModuleNuGetID(r.NuGetID); err != nil {
		return fmt.Errorf("require %s: %v", r.Module, err)
	}

	return nil
}
