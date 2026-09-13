// licensing.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"encoding/xml"
	"fmt"
	"go/ast"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"sync"
)

// go2csCopyrightLine is the second line of the converted standard library's LICENSE: the upstream
// Go BSD-3-Clause text with the go2cs Authors added as a copyright holder for the project-owned
// (hand-written) BSD-licensed additions that share the file. copyRootAttributionFiles inserts it
// on every -stdlib regeneration, so a regen REPRODUCES the committed file: the file is copied
// from GOROOT verbatim otherwise, and every corpus package packs it as its NuGet license file, so
// a line that lived only in the committed copy would vanish at the next regeneration.
const go2csCopyrightLine = "Copyright © 2026 The go2cs Authors. All rights reserved."

// withGo2csCopyright returns the upstream LICENSE text with go2csCopyrightLine inserted after its
// first line (the upstream copyright line), preserving the file's line endings. Idempotent: a file
// already carrying the line comes back unchanged, which is what keeps the corpus copy comparable
// across regenerations and what the seeded-reconvert control relies on.
func withGo2csCopyright(data []byte) []byte {
	newline := "\n"

	if bytes.Contains(data, []byte("\r\n")) {
		newline = "\r\n"
	}

	first := bytes.Index(data, []byte(newline))

	if first < 0 {
		return data
	}

	rest := data[first+len(newline):]
	line := []byte(go2csCopyrightLine + newline)

	if bytes.HasPrefix(rest, line) {
		return data
	}

	out := make([]byte, 0, len(data)+len(line))
	out = append(out, data[:first+len(newline)]...)
	out = append(out, line...)
	out = append(out, rest...)

	return out
}

// Preserve recognized leading notice groups verbatim, including multiple holders and
// block comments. Never infer a license for an unknown input. With -comments the
// ordinary comment emitter already preserves these groups.
func sourceLicenseNotices(file *ast.File, newline string) string {
	var out strings.Builder
	for _, group := range file.Comments {
		if group.Pos() >= file.Package {
			break
		}
		text := strings.ToLower(group.Text())
		if !strings.Contains(text, "copyright") && !strings.Contains(text, "license") && !strings.Contains(text, "spdx-") {
			continue
		}
		for _, comment := range group.List {
			out.WriteString(strings.ReplaceAll(strings.ReplaceAll(comment.Text, "\r\n", "\n"), "\n", newline))
			out.WriteString(newline)
		}
		out.WriteString(newline)
	}
	return out.String()
}

// A GOROOT-relative path identifies stdlib files; a module-relative path identifies
// recursive module inputs. Otherwise the basename is the only portable fact known.
func sourceProvenance(source string, options Options, newline string) string {
	if !options.provenance {
		return ""
	}
	name := filepath.Base(source)
	for _, root := range []string{options.goRoot, options.mainModuleDir} {
		if root == "" {
			continue
		}
		rel, err := filepath.Rel(root, source)
		if err == nil && rel != ".." && !strings.HasPrefix(rel, ".."+string(filepath.Separator)) && !filepath.IsAbs(rel) {
			name = filepath.ToSlash(rel)
			break
		}
	}
	// Quoting escapes newlines and other control characters in unusual filenames.
	return fmt.Sprintf("// Converted from Go source: %q%s", name, newline)
}

// packageLicenseMarker is the placeholder csproj-template.xml carries where the package's license
// metadata goes. EVERY project-file emission resolves it -- library and application alike -- so
// the marker never survives verbatim into an emitted project file; a resolver keyed on one output
// type would leave the raw comment in every project of the other (TestLicensingEmissionCompatibility
// asserts the application side).
const packageLicenseMarker = "<!-- GO2CS:PACKAGE-LICENSE -->"

// localLicenseExists is the MSBuild condition under which a LICENSE beside the project is packed.
const localLicenseExists = "Exists('$(MSBuildProjectDirectory)/LICENSE')"

// spdxExpression bounds what -license accepts: an SPDX expression, never MSBuild markup or a
// property expansion. NuGet validates the expression itself when packing.
var spdxExpression = regexp.MustCompile(`^[A-Za-z0-9().+ :_-]+$`)

func validatedLicenseExpression(options Options) (string, error) {
	expression := strings.TrimSpace(options.licenseExpression)

	if expression != "" && !spdxExpression.MatchString(expression) {
		return "", fmt.Errorf("-license must be an SPDX expression, for example MIT or BSD-3-Clause")
	}

	return expression, nil
}

// localLicenseProperty is the metadata a project gets when nothing better is known: a local
// LICENSE beside the project is packed as the license file when it exists, otherwise licensing
// is left unspecified rather than guessed. An explicit -license expression replaces it.
func localLicenseProperty(expression string) string {
	if expression != "" {
		return "<PackageLicenseExpression>" + xmlLicenseText(expression) + "</PackageLicenseExpression>"
	}

	return "<PackageLicenseFile Condition=\"" + localLicenseExists + "\">LICENSE</PackageLicenseFile>"
}

// licenseExecutableProject resolves the template's license marker for an APPLICATION project. An
// application is not packed by default, so the marker takes the local-LICENSE form (or the -license
// override) with no warning and no packed items; the point is that the marker is resolved at all.
func licenseExecutableProject(contents []byte, options Options) ([]byte, error) {
	expression, err := validatedLicenseExpression(options)

	if err != nil {
		return nil, err
	}

	return bytes.ReplaceAll(contents, []byte(packageLicenseMarker), []byte(localLicenseProperty(expression))), nil
}

// Package licensing is metadata only: no license, notice or author text is ever AUTHORED by
// conversion. Local files are optional and shared stdlib terms are packed directly from their
// existing location. The one file conversion copies is a dependency module's OWN license, verbatim,
// to the converted module's root (thirdPartyModuleLicense) -- preserving an upstream notice, not
// guessing one.
func licenseConvertedProject(contents []byte, projectFile string, options Options) ([]byte, error) {
	return licenseConvertedProjectFor(contents, projectFile, packageSourceDir, options)
}

// licenseConvertedProjectFor is licenseConvertedProject with the package's Go source directory
// passed explicitly (the production caller reads the per-package global; tests pass a fixture).
func licenseConvertedProjectFor(contents []byte, projectFile string, sourceDir string, options Options) ([]byte, error) {
	text := string(contents)
	expression, err := validatedLicenseExpression(options)

	if err != nil {
		return nil, err
	}

	if !strings.Contains(text, packageLicenseMarker) {
		// Respect explicit custom-template metadata unless the CLI overrides it.
		if expression != "" {
			if !strings.Contains(text, "</Project>") {
				return nil, fmt.Errorf("custom project template has no closing Project element")
			}
			text = regexp.MustCompile(`(?s)<PackageLicense(?:Expression|File)\b[^>]*>.*?</PackageLicense(?:Expression|File)>`).ReplaceAllString(text, "")
			block := "  <PropertyGroup>\n    <PackageLicenseExpression>" + xmlLicenseText(expression) + "</PackageLicenseExpression>\n  </PropertyGroup>\n"
			if strings.Contains(text, "\r\n") {
				block = strings.ReplaceAll(block, "\n", "\r\n")
			}
			return []byte(strings.Replace(text, "</Project>", block+"</Project>", 1)), nil
		}
		return contents, nil
	}

	props := localLicenseProperty("")
	var items strings.Builder

	if emitsPackageReadme(projectFile, options) {
		// A converted standard-library package packs the shared upstream license by relative path.
		shared := filepath.Join(options.go2csPath, "core", "LICENSE")
		if options.go2csPath == "" {
			if options.goRoot == "" {
				return nil, fmt.Errorf("standard-library licensing requires a source or runtime root")
			}
			shared = filepath.Join(options.goRoot, "LICENSE")
		}
		relative, err := packageLicensePath(projectFile, shared)
		if err != nil {
			return nil, err
		}
		props = "<PackageLicenseFile>LICENSE</PackageLicenseFile>"
		fmt.Fprintf(&items, "    <None Include=\"%s\" Pack=\"true\" PackagePath=\"\" Condition=\"!%s\" />\n", xmlLicenseText(relative), localLicenseExists)
	} else if expression == "" {
		if _, err := os.Stat(filepath.Join(filepath.Dir(projectFile), "LICENSE")); os.IsNotExist(err) {
			name, relative, err := thirdPartyModuleLicense(projectFile, sourceDir, options)
			if err != nil {
				return nil, err
			}
			if name != "" {
				props = "<PackageLicenseFile>" + xmlLicenseText(name) + "</PackageLicenseFile>"
				fmt.Fprintf(&items, "    <None Include=\"%s\" Pack=\"true\" PackagePath=\"\" Condition=\"!%s\" />\n", xmlLicenseText(relative), localLicenseExists)
			} else {
				warnUnspecifiedLicense(projectFile, sourceDir)
			}
		}
	}

	if expression != "" {
		props = localLicenseProperty(expression)
	}

	text = strings.ReplaceAll(text, packageLicenseMarker, props)

	if items.Len() > 0 {
		block := "  <ItemGroup>\n" + items.String() + "  </ItemGroup>\n"
		if strings.Contains(text, "\r\n") {
			block = strings.ReplaceAll(block, "\n", "\r\n")
		}
		text = strings.Replace(text, "</Project>", block+"</Project>", 1)
	}

	return []byte(text), nil
}

// moduleLicenseNames are the license file names NuGet accepts as a package license file (a .txt,
// a .md or no extension), in preference order. A module's file is matched case-insensitively.
var moduleLicenseNames = []string{
	"LICENSE", "LICENSE.md", "LICENSE.txt",
	"LICENCE", "LICENCE.md", "LICENCE.txt",
	"COPYING", "COPYING.md", "COPYING.txt",
}

// thirdPartyModuleLicense mirrors the standard-library shape for a converted DEPENDENCY module of a
// -recurse conversion: the module's own license file is copied verbatim to the converted module's
// root (once per module, never into each package folder) and every package project packs it by
// relative path. It returns empty names when the package is not such a dependency (the app's own
// module is the user's to license), when no module root is found, when the module ships no license
// file, or when the output layout is not the pkg\<module path>\<package> shape the copy relies on.
func thirdPartyModuleLicense(projectFile string, sourceDir string, options Options) (name string, relative string, err error) {
	if options.mainModuleDir == "" || sourceDir == "" {
		return "", "", nil
	}

	moduleRoot := licenseModuleRoot(sourceDir)

	if moduleRoot == "" || licenseSamePath(moduleRoot, options.mainModuleDir) {
		return "", "", nil
	}

	name = licenseModuleFile(moduleRoot)

	if name == "" {
		return "", "", nil
	}

	rel, err := filepath.Rel(moduleRoot, sourceDir)

	if err != nil {
		return "", "", nil
	}

	// The converted module root is the project directory with the package's module-relative path
	// stripped: pkg\<module path>\<rel> -> pkg\<module path>. A layout that does not match is left
	// unspecified rather than guessed at.
	outputRoot := filepath.Dir(projectFile)

	if rel != "." {
		parts := strings.Split(filepath.ToSlash(rel), "/")

		for i := len(parts) - 1; i >= 0; i-- {
			if !strings.EqualFold(filepath.Base(outputRoot), parts[i]) {
				return "", "", nil
			}

			outputRoot = filepath.Dir(outputRoot)
		}
	}

	data, err := os.ReadFile(filepath.Join(moduleRoot, name))

	if err != nil {
		return "", "", err
	}

	destination := filepath.Join(outputRoot, name)

	// Verbatim, and refreshed when the upstream text changes (a version bump), never rewritten when
	// it has not -- the file is converter output under the pkg\ tree, not a user's own file.
	if existing, err := os.ReadFile(destination); err != nil || !bytes.Equal(existing, data) {
		if err := os.WriteFile(destination, data, 0644); err != nil {
			return "", "", fmt.Errorf("failed to copy module license to %s: %w", destination, err)
		}
	}

	relative, err = packageLicensePath(projectFile, destination)

	if err != nil {
		return "", "", err
	}

	return name, relative, nil
}

// licenseModuleRoot walks up from a package directory to the nearest directory holding a go.mod
// (a module-cache entry, a `replace` target and the app module all carry one), or "" if none.
func licenseModuleRoot(dir string) string {
	dir = filepath.Clean(dir)

	for {
		if info, err := os.Stat(filepath.Join(dir, "go.mod")); err == nil && !info.IsDir() {
			return dir
		}

		parent := filepath.Dir(dir)

		if parent == dir {
			return ""
		}

		dir = parent
	}
}

// licenseModuleFile returns the module root's license file name in moduleLicenseNames preference
// order, spelled as the directory spells it, or "" when the module ships none.
func licenseModuleFile(moduleRoot string) string {
	entries, err := os.ReadDir(moduleRoot)

	if err != nil {
		return ""
	}

	for _, want := range moduleLicenseNames {
		for _, entry := range entries {
			if !entry.IsDir() && strings.EqualFold(entry.Name(), want) {
				return entry.Name()
			}
		}
	}

	return ""
}

func licenseSamePath(a string, b string) bool {
	absA, errA := filepath.Abs(a)
	absB, errB := filepath.Abs(b)

	if errA != nil || errB != nil {
		return false
	}

	return strings.EqualFold(filepath.Clean(absA), filepath.Clean(absB))
}

// unspecifiedLicenseWarned keys the once-per-module warning below: a -recurse app with forty
// library packages gets one line on stderr, not forty.
var unspecifiedLicenseWarned sync.Map

func warnUnspecifiedLicense(projectFile string, sourceDir string) {
	key := filepath.Dir(projectFile)

	if root := licenseModuleRoot(sourceDir); root != "" {
		key = root
	}

	if _, seen := unspecifiedLicenseWarned.LoadOrStore(key, true); seen {
		return
	}

	showWarning("Package license is unspecified for %s (reported once per module); add a LICENSE beside the project or pass -license with an SPDX expression before packing.", projectFile)
}

func packageLicensePath(projectFile, licenseFile string) (string, error) {
	projectDir, err := filepath.Abs(filepath.Dir(projectFile))
	if err != nil {
		return "", err
	}
	source, err := filepath.Abs(licenseFile)
	if err != nil {
		return "", err
	}
	relative, err := filepath.Rel(projectDir, source)
	if err != nil {
		// MSBuild can also pack an absolute source across Windows drive boundaries.
		return filepath.ToSlash(source), nil
	}
	return filepath.ToSlash(relative), nil
}

func xmlLicenseText(value string) string {
	var escaped bytes.Buffer
	_ = xml.EscapeText(&escaped, []byte(value))
	return escaped.String()
}
