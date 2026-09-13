// licensing_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestLicensingEmissionCompatibility(t *testing.T) {
	root := t.TempDir()
	sourceDir := filepath.Join(root, "app")
	out := filepath.Join(root, "out")
	writeModuleFile(t, filepath.Join(sourceDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")
	const notice = "// Copyright 2020 Example Author.\n// SPDX-License-Identifier: BSD-3-Clause\n\n"
	writeModuleFile(t, filepath.Join(sourceDir, "main.go"), notice+"package main\nfunc main() {}\n")
	options := Options{goRoot: runtime.GOROOT(), goPath: build.Default.GOPATH,
		go2csPath: filepath.Join(root, "runtime"), targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}
	convert := func() string {
		t.Helper()
		if err := processConversion(sourceDir, true, out, options); err != nil {
			t.Fatal(err)
		}
		return strings.ReplaceAll(readGenerated(t, filepath.Join(out, "main.cs")), "\r\n", "\n")
	}
	baseline := convert()
	if strings.Contains(baseline, "Converted from Go source:") || strings.Count(baseline, "Copyright 2020 Example Author.") != 1 {
		t.Fatal(baseline)
	}
	// An APPLICATION project resolves the template's license marker too -- a resolver keyed on the
	// library output type alone leaves the raw placeholder comment in every Exe csproj -- and takes
	// the local-LICENSE form: no warning, nothing packed.
	projects, err := filepath.Glob(filepath.Join(out, "*.csproj"))
	if err != nil || len(projects) != 1 {
		t.Fatalf("expected one application project: %v (%v)", projects, err)
	}
	if project := readGenerated(t, projects[0]); strings.Contains(project, packageLicenseMarker) || !strings.Contains(project, `<PackageLicenseFile Condition="Exists('$(MSBuildProjectDirectory)/LICENSE')">LICENSE</PackageLicenseFile>`) {
		t.Fatalf("application project license metadata: %s", project)
	}
	if got := convert(); got != baseline {
		t.Fatal("reconversion changed default output")
	}
	options.provenance = true
	provenance := sourceProvenance(filepath.Join(sourceDir, "main.go"), options, "\n")
	if got := convert(); strings.Replace(got, provenance, "", 1) != baseline || strings.Count(got, provenance) != 1 {
		t.Fatal("provenance changed the converted body or duplicated the header")
	}
	options.includeComments = true
	if got := convert(); strings.Count(got, "Copyright 2020 Example Author.") != 1 {
		t.Fatalf("-comments duplicated/dropped notices: %s", got)
	}
}

func TestLicensingNoticePreservation(t *testing.T) {
	source := "// Copyright 2009 The Go Authors.\n// BSD-style license in LICENSE.\n\n/* Copyright 2020 Another Author.\nSPDX-License-Identifier: MIT */\n\npackage example\n"
	file, err := parser.ParseFile(token.NewFileSet(), "example.go", source, parser.ParseComments)
	if err != nil {
		t.Fatal(err)
	}
	got := sourceLicenseNotices(file, "\n")
	for _, want := range []string{"Copyright 2009 The Go Authors.", "BSD-style license in LICENSE.", "Copyright 2020 Another Author.", "SPDX-License-Identifier: MIT"} {
		if strings.Count(got, want) != 1 {
			t.Errorf("notice missing or duplicated: %q in %q", want, got)
		}
	}
	unknown, err := parser.ParseFile(token.NewFileSet(), "unknown.go", "// Package example demonstrates something.\npackage example\n", parser.ParseComments)
	if err != nil {
		t.Fatal(err)
	}
	if got := sourceLicenseNotices(unknown, "\n"); got != "" {
		t.Fatalf("unknown license was inferred: %q", got)
	}
}

func TestLicensingProvenanceDefaultAndPortable(t *testing.T) {
	root := t.TempDir()
	source := filepath.Join(root, "src", "sync", "mutex.go")
	if got := sourceProvenance(source, Options{}, "\n"); got != "" {
		t.Fatalf("default output changed: %q", got)
	}
	options := Options{goRoot: root, provenance: true}
	want := "// Converted from Go source: \"src/sync/mutex.go\"\n"
	if got := sourceProvenance(source, options, "\n"); got != want {
		t.Fatalf("got %q, want %q", got, want)
	}
	otherRoot := t.TempDir()
	options.goRoot = otherRoot
	if got := sourceProvenance(filepath.Join(otherRoot, "src", "sync", "mutex.go"), options, "\n"); got != want {
		t.Fatalf("machine-dependent provenance: %q", got)
	}
	if got := sourceProvenance(source, Options{provenance: true}, "\n"); got != "// Converted from Go source: \"mutex.go\"\n" {
		t.Fatal(got)
	}
}

func TestLicensingPackageAndReadme(t *testing.T) {
	root := t.TempDir()
	output := filepath.Join(root, "core", "sync")
	writeModuleFile(t, filepath.Join(root, "core", "LICENSE"), "Upstream BSD terms retained exactly\n")
	if err := os.MkdirAll(output, 0755); err != nil {
		t.Fatal(err)
	}
	options := Options{go2csPath: root, convertStdLib: true}
	template := []byte("<Project><PropertyGroup><!-- GO2CS:PACKAGE-LICENSE --></PropertyGroup></Project>")
	convert := func(o Options) string {
		t.Helper()
		got, err := licenseConvertedProject(template, filepath.Join(output, "sync.csproj"), o)
		if err != nil {
			t.Fatal(err)
		}
		return string(got)
	}
	got := convert(options)
	if !strings.Contains(got, `Include="../LICENSE"`) || !strings.Contains(got, "<PackageLicenseFile>LICENSE</PackageLicenseFile>") {
		t.Fatal(got)
	}
	if again := convert(options); again != got {
		t.Fatal("reconversion changed metadata")
	}
	for _, name := range []string{"LICENSE", "NOTICE", "AUTHORS"} {
		if _, err := os.Stat(filepath.Join(output, name)); !os.IsNotExist(err) {
			t.Fatalf("conversion created %s: %v", name, err)
		}
	}
	writeModuleFile(t, filepath.Join(output, "manual.cs"), "// SPDX-License-Identifier: BSD-3-Clause\n")
	withManual := convert(options)
	if withManual != got {
		t.Fatal("handwritten standard-library additions changed license packaging")
	}
	if strings.Contains(withManual, "Go2csAuthorsFile") || strings.Contains(withManual, "ValidateGo2csAuthors") {
		t.Fatal("author discovery returned")
	}
	writeModuleFile(t, filepath.Join(output, "LICENSE"), "Custom local terms")
	local := convert(options)
	if !strings.Contains(local, `Condition="!Exists('$(MSBuildProjectDirectory)/LICENSE')"`) {
		t.Fatal("shared license does not defer to local file")
	}
	contents, _ := os.ReadFile(filepath.Join(output, "LICENSE"))
	if string(contents) != "Custom local terms" {
		t.Fatal("local license was overwritten")
	}
	if err := writeReadmeFile(output, "sync", "", "", options); err != nil {
		t.Fatal(err)
	}
	readme, err := os.ReadFile(filepath.Join(output, "README.md"))
	if err != nil {
		t.Fatal(err)
	}
	for _, want := range []string{"Copyright 2009 The Go Authors.", "Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README)."} {
		if !strings.Contains(string(readme), want) {
			t.Errorf("README lacks %s", want)
		}
	}
	if strings.Contains(string(readme), "conversion itself") || strings.Contains(string(readme), "AGPL") {
		t.Fatal(string(readme))
	}
}

func TestLicensingExpressionAndUnknownInput(t *testing.T) {
	output := t.TempDir()
	template := []byte("<Project><PropertyGroup><!-- GO2CS:PACKAGE-LICENSE --></PropertyGroup></Project>")
	unknown, err := licenseConvertedProject(template, filepath.Join(output, "app.csproj"), Options{})
	if err != nil {
		t.Fatal(err)
	}
	if strings.Contains(string(unknown), "PackageLicenseExpression") || !strings.Contains(string(unknown), `Condition="Exists('$(MSBuildProjectDirectory)/LICENSE')"`) {
		t.Fatal(string(unknown))
	}
	for _, template := range [][]byte{template, []byte("<Project><PropertyGroup><PackageLicenseFile>LICENSE</PackageLicenseFile><PackageLicenseExpression>MIT</PackageLicenseExpression></PropertyGroup></Project>")} {
		got, err := licenseConvertedProject(template, filepath.Join(output, "app.csproj"), Options{licenseExpression: "BSD-3-Clause OR MIT"})
		if err != nil {
			t.Fatal(err)
		}
		if strings.Contains(string(got), "PackageLicenseFile") || strings.Count(string(got), "<PackageLicenseExpression>") != 1 || !strings.Contains(string(got), ">BSD-3-Clause OR MIT<") {
			t.Fatal(string(got))
		}
	}
	if _, err := licenseConvertedProject(template, filepath.Join(output, "app.csproj"), Options{licenseExpression: "$(UnsafeProperty)"}); err == nil {
		t.Fatal("accepted MSBuild expansion instead of SPDX expression")
	}
	custom := []byte("<Project><PackageLicenseExpression>Apache-2.0</PackageLicenseExpression></Project>")
	got, err := licenseConvertedProject(custom, filepath.Join(output, "app.csproj"), Options{})
	if err != nil || string(got) != string(custom) {
		t.Fatal("custom license changed without an override")
	}
}

func TestLicensingCoreLicenseCopyright(t *testing.T) {
	// A -stdlib regeneration copies GOROOT's LICENSE into core/ and inserts the go2cs Authors line
	// after the upstream copyright line; the committed file must be exactly what that produces, or
	// the next regeneration silently reverts a hand edit every corpus package packs.
	upstream := "Copyright (c) 2009 The Go Authors. All rights reserved.\n\nRedistribution and use ...\n"
	want := "Copyright (c) 2009 The Go Authors. All rights reserved.\n" + go2csCopyrightLine + "\n\nRedistribution and use ...\n"
	if got := string(withGo2csCopyright([]byte(upstream))); got != want {
		t.Fatalf("got %q, want %q", got, want)
	}
	if got := string(withGo2csCopyright([]byte(want))); got != want {
		t.Fatal("second application changed the file")
	}
	crlf := strings.ReplaceAll(upstream, "\n", "\r\n")
	if got := string(withGo2csCopyright([]byte(crlf))); got != strings.ReplaceAll(want, "\n", "\r\n") {
		t.Fatalf("CRLF input lost its line endings: %q", got)
	}
	if got := withGo2csCopyright([]byte("no newline at all")); string(got) != "no newline at all" {
		t.Fatalf("single-line input changed: %q", got)
	}
	committed, err := os.ReadFile("../core/LICENSE")
	if err != nil {
		t.Fatal(err)
	}
	if string(withGo2csCopyright(committed)) != string(committed) {
		t.Fatal("../core/LICENSE is not what a regeneration writes: its second line must be the go2cs copyright line")
	}
	lines := strings.SplitN(strings.ReplaceAll(string(committed), "\r\n", "\n"), "\n", 3)
	if len(lines) < 3 || !strings.HasPrefix(lines[0], "Copyright") || lines[1] != go2csCopyrightLine {
		t.Fatalf("../core/LICENSE head: %q", lines)
	}
}

func TestLicensingThirdPartyModuleLicense(t *testing.T) {
	root := t.TempDir()
	app := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(app, "go.mod"), "module example.com/app\n")
	writeModuleFile(t, filepath.Join(app, "LICENSE"), "App terms\n")
	writeModuleFile(t, filepath.Join(app, "lib", "lib.go"), "package lib\n")
	dep := filepath.Join(root, "mod", "example.com", "dep@v1.2.0")
	writeModuleFile(t, filepath.Join(dep, "go.mod"), "module example.com/dep\n")
	writeModuleFile(t, filepath.Join(dep, "License.md"), "Upstream terms\n")
	writeModuleFile(t, filepath.Join(dep, "sub", "sub.go"), "package sub\n")
	bare := filepath.Join(root, "mod", "example.com", "bare@v0.1.0")
	writeModuleFile(t, filepath.Join(bare, "go.mod"), "module example.com/bare\n")
	writeModuleFile(t, filepath.Join(bare, "bare.go"), "package bare\n")
	template := []byte("<Project><PropertyGroup><!-- GO2CS:PACKAGE-LICENSE --></PropertyGroup></Project>")
	options := Options{mainModuleDir: app}
	convert := func(project string, sourceDir string) string {
		t.Helper()
		if err := os.MkdirAll(filepath.Dir(project), 0755); err != nil {
			t.Fatal(err)
		}
		got, err := licenseConvertedProjectFor(template, project, sourceDir, options)
		if err != nil {
			t.Fatal(err)
		}
		return string(got)
	}

	// A dependency module's own license file is copied verbatim to the converted MODULE root (once,
	// spelled as upstream spells it) and packed by relative path from every package of the module.
	depOut := filepath.Join(root, "out", "pkg", "example.com", "dep")
	got := convert(filepath.Join(depOut, "sub", "example.com.dep.sub.csproj"), filepath.Join(dep, "sub"))
	copied, err := os.ReadFile(filepath.Join(depOut, "License.md"))
	if err != nil || string(copied) != "Upstream terms\n" {
		t.Fatalf("module license not copied verbatim: %v %q", err, copied)
	}
	if !strings.Contains(got, "<PackageLicenseFile>License.md</PackageLicenseFile>") || !strings.Contains(got, `Include="../License.md" Pack="true" PackagePath="" Condition="!Exists('$(MSBuildProjectDirectory)/LICENSE')"`) {
		t.Fatal(got)
	}
	if _, err := os.Stat(filepath.Join(depOut, "sub", "License.md")); !os.IsNotExist(err) {
		t.Fatal("license copied into the package directory instead of the module root")
	}
	if again := convert(filepath.Join(depOut, "sub", "example.com.dep.sub.csproj"), filepath.Join(dep, "sub")); again != got {
		t.Fatal("reconversion changed metadata")
	}
	rootPackage := convert(filepath.Join(depOut, "example.com.dep.csproj"), dep)
	if !strings.Contains(rootPackage, `Include="License.md" Pack="true"`) {
		t.Fatal(rootPackage)
	}

	// The app's own module is the user's to license: nothing is copied, the local-LICENSE form stands.
	appOut := filepath.Join(root, "out", "src", "example.com", "app", "lib")
	got = convert(filepath.Join(appOut, "example.com.app.lib.csproj"), filepath.Join(app, "lib"))
	if strings.Contains(got, "<PackageLicenseFile>LICENSE</PackageLicenseFile>") || !strings.Contains(got, `<PackageLicenseFile Condition="Exists('$(MSBuildProjectDirectory)/LICENSE')">LICENSE</PackageLicenseFile>`) {
		t.Fatal(got)
	}
	if _, err := os.Stat(filepath.Join(root, "out", "src", "example.com", "app", "LICENSE")); !os.IsNotExist(err) {
		t.Fatal("the app module's own license was copied")
	}

	// A dependency shipping no license file stays unspecified rather than guessed.
	got = convert(filepath.Join(root, "out", "pkg", "example.com", "bare", "example.com.bare.csproj"), bare)
	if strings.Contains(got, "<PackageLicenseFile>") || !strings.Contains(got, "Condition=\"Exists(") {
		t.Fatal(got)
	}

	// Outside a -recurse conversion (no main module) nothing is copied either, and an output layout
	// that is not pkg\<module path>\<package> is left unspecified rather than guessed at.
	solo := filepath.Join(root, "solo", "example.com.dep.sub.csproj")
	if err := os.MkdirAll(filepath.Dir(solo), 0755); err != nil {
		t.Fatal(err)
	}
	noModule, err := licenseConvertedProjectFor(template, solo, filepath.Join(dep, "sub"), Options{})
	if err != nil {
		t.Fatal(err)
	}
	wrongLayout := convert(solo, filepath.Join(dep, "sub"))
	if _, err := os.Stat(filepath.Join(root, "License.md")); !os.IsNotExist(err) || strings.Contains(string(noModule), "License.md") || strings.Contains(wrongLayout, "License.md") {
		t.Fatal("copied without a main module or into an unrecognized layout")
	}
}

func TestLicensingConverterHeaders(t *testing.T) {
	err := filepath.WalkDir(".", func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() || !strings.HasSuffix(path, ".go") {
			return nil
		}
		data, err := os.ReadFile(path)
		if err != nil {
			return err
		}
		header := string(data)
		if len(header) > 600 {
			header = header[:600]
		}
		if !strings.Contains(header, "// SPDX-License-Identifier: AGPL-3.0-only") {
			t.Errorf("missing converter header: %s", path)
		}
		// AGPL section 7: a file covered by an additional permission carries a notice saying where
		// to find it. Every converter source file is covered by the output exception.
		if !strings.Contains(header, "// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).") {
			t.Errorf("missing output-exception notice (AGPL section 7): %s", path)
		}
		return nil
	})
	if err != nil {
		t.Fatal(err)
	}
}

func TestLicensingPackageBoundaries(t *testing.T) {
	// The output exception the converter headers point at exists beside the AGPL text and says the
	// things the policy relies on: it is a section 7 permission, it names the embedded templates,
	// it carries the MIT grant for scaffolding, and it stops at the converter itself.
	exception, err := os.ReadFile("LICENSE-EXCEPTION")
	if err != nil {
		t.Fatalf("converter output exception missing: %v", err)
	}
	for _, want := range []string{"section 7", "embeddedTemplates.go", "MIT License", "does not apply to the Converter itself"} {
		if !strings.Contains(string(exception), want) {
			t.Errorf("LICENSE-EXCEPTION lacks %q", want)
		}
	}
	for _, dir := range []string{"../core/golib", "../core/go2cs", "../gen/go2cs-gen"} {
		data, err := os.ReadFile(filepath.Join(dir, "LICENSE"))
		if err != nil || !strings.Contains(string(data), "MIT License") {
			t.Errorf("permissive component %s: missing MIT license (%v)", dir, err)
		}
	}
	err = filepath.WalkDir("../core", func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if entry.IsDir() {
			switch entry.Name() {
			case "bin", "obj", "Generated", ".vs", "golib", "go2cs":
				return filepath.SkipDir
			}
			return nil
		}
		if strings.HasSuffix(path, ".cs") {
			data, err := os.ReadFile(path)
			if err != nil {
				return err
			}
			header := string(data[:min(len(data), 2048)])
			if strings.Contains(header, "MIT-style license") || strings.Contains(header, "SPDX-License-Identifier: MIT") {
				t.Errorf("standard-library source carries MIT instead of BSD: %s", path)
			}
			if strings.Contains(header, "The go2cs Authors") && !strings.Contains(header, "BSD-style license") && !strings.Contains(header, "SPDX-License-Identifier: BSD-3-Clause") {
				t.Errorf("project-owned standard-library source lacks BSD header: %s", path)
			}
			return nil
		}
		if !strings.HasSuffix(path, ".csproj") || strings.HasSuffix(path, ".tests.csproj") {
			return nil
		}
		data, err := os.ReadFile(path)
		if err != nil {
			return err
		}
		if !strings.Contains(string(data), "<PackageId>go.") {
			return nil
		}
		if !strings.Contains(string(data), "<PackageLicenseFile>LICENSE</PackageLicenseFile>") {
			t.Errorf("converted package lacks upstream license metadata: %s", path)
		}
		if strings.Contains(string(data), "LICENSES/MIT") || strings.Contains(string(data), "golib/LICENSE") {
			t.Errorf("standard-library package includes an additional MIT license: %s", path)
		}
		for _, name := range []string{"LICENSE", "NOTICE", "AUTHORS"} {
			want := `Include="` + name + `" Pack="true" PackagePath="" Condition="Exists('$(MSBuildProjectDirectory)/` + name + `')"`
			if !strings.Contains(string(data), want) {
				t.Errorf("%s lacks optional %s packing", path, name)
			}
		}
		if strings.Contains(string(data), "Go2csAuthorsFile") || strings.Contains(string(data), "ValidateGo2csAuthors") {
			t.Errorf("complex AUTHORS discovery remains in %s", path)
		}
		shared, err := filepath.Rel(filepath.Dir(path), "../core/LICENSE")
		if err != nil {
			return err
		}
		if !strings.Contains(string(data), `Include="`+filepath.ToSlash(shared)+`" Pack="true" PackagePath=""`) {
			t.Errorf("missing relative upstream license in %s", path)
		}
		// A packed standard-library README that mentions MIT at all contradicts the BSD license file
		// packed beside it. The check is a word match rather than a sentence match so that it reaches
		// the hand-owned READMEs (unsafe, testing) as well as the converter-emitted ones.
		if readme, err := os.ReadFile(filepath.Join(filepath.Dir(path), "README.md")); err == nil && regexp.MustCompile(`\bMIT\b`).MatchString(string(readme)) {
			t.Errorf("standard-library README claims MIT: %s", path)
		}
		return nil
	})
	if err != nil {
		t.Fatal(err)
	}
}

func TestLicensingLibraryConversion(t *testing.T) {
	root := t.TempDir()
	source := filepath.Join(root, "input")
	output := filepath.Join(root, "output")
	writeModuleFile(t, filepath.Join(source, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(source, "lib.go"), "package lib\nvar Answer = 42\n")
	options := Options{goRoot: runtime.GOROOT(), goPath: build.Default.GOPATH,
		go2csPath: filepath.Join(root, "runtime"), targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces: 4, preferVarDecl: true, useChannelOperators: true, licenseExpression: "Apache-2.0"}
	if err := processConversion(source, true, output, options); err != nil {
		t.Fatal(err)
	}
	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))
	if err != nil || len(projects) != 1 {
		t.Fatalf("expected one library project: %v (%v)", projects, err)
	}
	data, err := os.ReadFile(projects[0])
	if err != nil {
		t.Fatal(err)
	}
	if !strings.Contains(string(data), "<PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>") || strings.Contains(string(data), "<PackageLicenseFile") {
		t.Fatal(string(data))
	}
	for _, name := range []string{"LICENSE", "NOTICE", "AUTHORS"} {
		if !strings.Contains(string(data), `Condition="Exists('$(MSBuildProjectDirectory)/`+name+`')"`) {
			t.Errorf("missing optional packing for %s", name)
		}
		if _, err := os.Stat(filepath.Join(output, name)); !os.IsNotExist(err) {
			t.Errorf("conversion created %s: %v", name, err)
		}
	}
}
