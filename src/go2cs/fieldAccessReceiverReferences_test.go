// fieldAccessReceiverReferences_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// net on darwin (re-bank discovery pass, run 41477450611 at f6666d98a2): net.tests failed to build with
// CS0012 on internal.routebsd, an assembly no net test file imports. The production package's darwin
// package_info.cs records five routebsd structs, `[assembly: GoFieldAccess(typeof(go.@internal.routebsd_package.X))]`,
// so go2cs-gen gives net.dll's internal GoFieldAccess class an extension block on ж<routebsd_package.X>. A
// white-box test assembly sees that class through InternalsVisibleTo, and every `p.f` it reads through a pointer
// binds by extension lookup: `ifi.Name` makes the compiler examine net's ж<routebsd.LinkAddr> block, which also
// declares Name, and that needs internal.routebsd's metadata. DisableTransitiveProjectReferences hides it, so the
// test project has to reference the record's declaring package itself.

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// writeForeignReceiverFixture writes a white-box suite (an internal test file, so the production project grants
// InternalsVisibleTo) whose production package reads a field of another package's struct through a pointer, and
// whose test reads a same-named field of its own struct through a pointer. The test imports only `testing`, so
// nothing in its import list names the foreign package.
func writeForeignReceiverFixture(t *testing.T) string {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":         "module example/foreignrecv\n\ngo 1.23\n",
		"link/link.go":   "package link\n\ntype Addr struct {\n\tIndex int\n\tName  string\n}\n",
		"other/other.go": "package other\n\ntype Thing struct{ N int }\n",
		"widget/widget.go": "package widget\n\nimport (\n\t\"example/foreignrecv/link\"\n\t\"example/foreignrecv/other\"\n)\n\n" +
			"func NameOf(a *link.Addr) string { return a.Name }\n\n" +
			"func Count(t other.Thing) int { return t.N }\n",
		"widget/widget_test.go": "package widget\n\nimport \"testing\"\n\n" +
			"type item struct{ Name string }\n\n" +
			"func TestItemName(t *testing.T) {\n" +
			"\tp := &item{Name: \"x\"}\n" +
			"\tif p.Name != \"x\" {\n" +
			"\t\tt.Fatal(p.Name)\n" +
			"\t}\n" +
			"}\n",
	})

	return filepath.Join(dir, "widget")
}

// The production half of a -tests run is already on disk when the test half reads it; this is the package_info.cs
// the production conversion writes for the fixture, reduced to the one record the reference set must read and one
// alias that must NOT become a reference: `other` is in the package's closure, so the alias scan WOULD resolve
// it, and the scan of this file is for field-access records only.
const foreignReceiverProductionInfo = "// <ImportedTypeAliases>\r\n" +
	"global using otherꓸThing = go.example.foreignrecv.other_package.Thing;\r\n" +
	"// </ImportedTypeAliases>\r\n\r\n" +
	"[assembly: GoFieldAccess(typeof(go.example.foreignrecv.link_package.Addr))]\r\n"

func TestWhiteboxTestProjectReferencesProductionFieldAccessReceivers(t *testing.T) {
	isolateConverterPathGlobals(t)

	inputPath := writeForeignReceiverFixture(t)

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: inputPath, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, inputPath)
	if production == nil {
		t.Fatal("production package was not loaded")
	}

	internal, external := findTestVariants(loaded, production)

	// Vacuity: the fixture must take the white-box model, the one whose production project grants the test
	// assembly InternalsVisibleTo, and the test files must not import the foreign package themselves.
	if model := selectTestProjectModel(internal, external); model != testProjectWhiteboxReference {
		t.Fatalf("fixture model = %v, want whitebox-reference", model)
	}

	outputPath := t.TempDir()

	if err := os.WriteFile(filepath.Join(outputPath, PackageInfoFileName), []byte(foreignReceiverProductionInfo), 0644); err != nil {
		t.Fatal(err)
	}

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		convertTests:        true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
	}

	if err := processTestConversion(inputPath, outputPath, options); err != nil {
		t.Fatalf("processTestConversion: %v", err)
	}

	projects, err := filepath.Glob(filepath.Join(outputPath, "*.tests.csproj"))
	if err != nil || len(projects) != 1 {
		t.Fatalf("want one test project in the output, got %v (%v)", projects, err)
	}

	body, err := os.ReadFile(projects[0])
	if err != nil {
		t.Fatal(err)
	}

	var references []string

	for _, line := range strings.Split(string(body), "\n") {
		if strings.Contains(line, "<ProjectReference ") {
			references = append(references, strings.TrimSpace(line))
		}
	}

	t.Logf("%s references: %v", filepath.Base(projects[0]), references)

	// Positive control: the colocated production project is referenced, so the list above is the real one.
	if !containsReferenceTo(references, strings.TrimSuffix(filepath.Base(projects[0]), ".tests.csproj")+".csproj\"") {
		t.Fatalf("%s must reference its production project: %v", filepath.Base(projects[0]), references)
	}

	if !containsReferenceTo(references, "link") {
		t.Errorf("the test project must reference the declaring package of a production [GoFieldAccess] record "+
			"(example/foreignrecv/link): the test's `p.Name` examines production's ж<link.Addr> extension block, so "+
			"without it the build is CS0012: %v", references)
	}

	if containsReferenceTo(references, "other") {
		t.Errorf("the production package_info.cs is read for its field-access records only; its aliases are the "+
			"production project's concern: %v", references)
	}

	resetPackageState(&packages.Package{})
}

// golang-jwt/jwt/v5 on go.* 1.26.10.2 (the nugetgo rebuild wave, 2026-11-04): jwt's production package_info.cs records
// `[assembly: GoFieldAccess(typeof(go.crypto.x509_package.Certificate))]`, and jwt.tests failed CS0012 ×3 on
// `claims.Issuer` (example_test.cs), "'pkix_package.Name' is defined in an assembly that is not referenced". The
// receiver's package, crypto/x509, IS referenced (the receiver edge above), but the extension block on
// ж<x509.Certificate> also declares Issuer, of type pkix.Name, and ranking that candidate binds its type. So the
// packages of the receiver's FIELD types are references too, for the fields a test selects by name: a field no test
// selects by name is never a candidate, and its type's package stays out.
func writeForeignReceiverFieldTypeFixture(t *testing.T) string {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":         "module example/foreignfield\n\ngo 1.23\n",
		"name/name.go":   "package name\n\ntype Name struct{ CommonName string }\n",
		"spare/spare.go": "package spare\n\ntype Part struct{ N int }\n",
		"cert/cert.go": "package cert\n\nimport (\n\t\"example/foreignfield/name\"\n\t\"example/foreignfield/spare\"\n)\n\n" +
			"type Certificate struct {\n\tIssuer name.Name\n\tSpare  spare.Part\n\tSerial int\n}\n",
		"widget/widget.go": "package widget\n\nimport \"example/foreignfield/cert\"\n\n" +
			"func SerialOf(c *cert.Certificate) int { return c.Serial }\n",
		"widget/widget_test.go": "package widget\n\nimport \"testing\"\n\n" +
			"type claims struct{ Issuer string }\n\n" +
			"func TestIssuer(t *testing.T) {\n" +
			"\tp := &claims{Issuer: \"x\"}\n" +
			"\tif p.Issuer != \"x\" {\n" +
			"\t\tt.Fatal(p.Issuer)\n" +
			"\t}\n" +
			"}\n",
	})

	return filepath.Join(dir, "widget")
}

const foreignReceiverFieldTypeProductionInfo = "[assembly: GoFieldAccess(typeof(go.example.foreignfield.cert_package.Certificate))]\r\n"

func TestWhiteboxTestProjectReferencesProductionFieldAccessFieldTypes(t *testing.T) {
	isolateConverterPathGlobals(t)

	inputPath := writeForeignReceiverFieldTypeFixture(t)

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: inputPath, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, inputPath)
	if production == nil {
		t.Fatal("production package was not loaded")
	}

	internal, external := findTestVariants(loaded, production)

	if model := selectTestProjectModel(internal, external); model != testProjectWhiteboxReference {
		t.Fatalf("fixture model = %v, want whitebox-reference", model)
	}

	outputPath := t.TempDir()

	if err := os.WriteFile(filepath.Join(outputPath, PackageInfoFileName), []byte(foreignReceiverFieldTypeProductionInfo), 0644); err != nil {
		t.Fatal(err)
	}

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		convertTests:        true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
	}

	if err := processTestConversion(inputPath, outputPath, options); err != nil {
		t.Fatalf("processTestConversion: %v", err)
	}

	projects, err := filepath.Glob(filepath.Join(outputPath, "*.tests.csproj"))
	if err != nil || len(projects) != 1 {
		t.Fatalf("want one test project in the output, got %v (%v)", projects, err)
	}

	body, err := os.ReadFile(projects[0])
	if err != nil {
		t.Fatal(err)
	}

	var references []string

	for _, line := range strings.Split(string(body), "\n") {
		if strings.Contains(line, "<ProjectReference ") {
			references = append(references, strings.TrimSpace(line))
		}
	}

	t.Logf("%s references: %v", filepath.Base(projects[0]), references)

	// Positive control: the receiver edge still holds, so the reference list read below is the real one.
	if !containsReferenceTo(references, "cert") {
		t.Fatalf("the receiver's own package (example/foreignfield/cert) must be referenced: %v", references)
	}

	if !containsReferenceTo(references, "/name/") {
		t.Errorf("the test project must reference the package of a field a test selects by name on a production "+
			"[GoFieldAccess] receiver (example/foreignfield/name, Certificate.Issuer): `p.Issuer` ranks the "+
			"extension block's Issuer, of type name.Name, so without it the build is CS0012: %v", references)
	}

	if containsReferenceTo(references, "spare") {
		t.Errorf("a receiver field no test selects by name (Certificate.Spare) is never a candidate; its package "+
			"(example/foreignfield/spare) must stay out: %v", references)
	}

	resetPackageState(&packages.Package{})
}

func containsReferenceTo(references []string, fragment string) bool {
	for _, reference := range references {
		if strings.Contains(reference, fragment) {
			return true
		}
	}

	return false
}

// The record reader's shapes, as the corpus spells them: rooted, `global::`-rooted, bare single-segment (emitted from
// inside `namespace go`) and a generic open definition. Any other line, a record of another kind included, is no
// target: the production package_info.cs is read for field-access receivers only.
func TestFieldAccessRecordTargetsReadsTheReceiverQualifier(t *testing.T) {
	cases := []struct {
		line string
		want []string
	}{
		{"[assembly: GoFieldAccess(typeof(go.@internal.routebsd_package.InetAddr))]", []string{"go.@internal.routebsd_package"}},
		{"[assembly: GoFieldAccess(typeof(global::go.net_package.TCPConn))]", []string{"go.net_package"}},
		{"[assembly: GoFieldAccess(typeof(syscall_package.Stat_t))]", []string{"syscall_package"}},
		{"[assembly: GoFieldAccess(typeof(go.container.list_package.Pair<,>))]", []string{"go.container.list_package"}},
		{"global using routebsdꓸInetAddr = go.@internal.routebsd_package.InetAddr;", nil},
		{"[assembly: GoImplement<go.io.fs_package.PathError, error>(Pointer = true)]", nil},
		{"[assembly: GoFieldAccess(typeof(Local))]", nil},
	}

	for _, c := range cases {
		got := fieldAccessRecordTargets(c.line)

		if strings.Join(got, "|") != strings.Join(c.want, "|") {
			t.Errorf("fieldAccessRecordTargets(%q) = %v, want %v", c.line, got, c.want)
		}
	}
}
