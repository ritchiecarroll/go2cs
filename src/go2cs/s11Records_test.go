// s11Records_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the section 11 rows of docs/PLAN-marker-comment-parity.md: the inline attributes that leave converted
// code for a record the converter writes on the type's accessibility declaration in package_info.cs.
// testdata/s11records/main.go is converted as a module and both files are read.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertS11Fixture converts testdata/s11records as a module and returns its main.cs and package_info.cs.
func convertS11Fixture(t *testing.T) (mainCs string, packageInfo string) {
	t.Helper()

	source, err := os.ReadFile(filepath.Join("testdata", "s11records", "main.go"))

	if err != nil {
		t.Fatalf("read fixture: %v", err)
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/s11\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), string(source))

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		includeComments:     true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "s11")
	mainCs = strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "main.cs")), "\r\n", "\n")
	packageInfo = strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "package_info.cs")), "\r\n", "\n")

	return mainCs, packageInfo
}

// TestDescriptorCarriersAreRecordedOnTheType holds row 5: a field whose Go type is a defined type over an
// interface carries no [GoDescriptorType]; its carrier is recorded on the struct's accessibility declaration,
// naming the field as reflection sees it, for a local carrier, an imported one, each name of a grouped field
// and an embedded field.
func TestDescriptorCarriersAreRecordedOnTheType(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs, packageInfo := convertS11Fixture(t)

	if strings.Contains(mainCs, "[GoDescriptorType") {
		t.Errorf("converted code still carries [GoDescriptorType]:\n%s", mainCs)
	}

	for _, want := range []string{
		"public Token Tok;",
		"public cryptoꓸPublicKey Key;",
		"public Named A, B;",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in:\n%s", want, mainCs)
		}
	}

	var record string

	for _, line := range strings.Split(packageInfo, "\n") {
		if strings.HasSuffix(line, "partial struct holder {}") {
			record = line
		}
	}

	if record == "" {
		t.Fatalf("no accessibility declaration for holder in:\n%s", packageInfo)
	}

	for _, want := range []string{
		`[GoMemberRecord("Tok", GoMemberFact.Descriptor, typeof(Tokenᴅ))]`,
		`[GoMemberRecord("Key", GoMemberFact.Descriptor, typeof(go.crypto_package.PublicKeyᴅ))]`,
		`[GoMemberRecord("A", GoMemberFact.Descriptor, typeof(Namedᴅ))]`,
		`[GoMemberRecord("B", GoMemberFact.Descriptor, typeof(Namedᴅ))]`,
		`[GoMemberRecord("Token", GoMemberFact.Descriptor, typeof(Tokenᴅ))]`,
	} {
		if !strings.Contains(record, want) {
			t.Errorf("want %s in holder's record:\n%s", want, record)
		}
	}

	if strings.Contains(record, `"Plain"`) {
		t.Errorf("a field of a plain interface type has no carrier to record:\n%s", record)
	}
}
