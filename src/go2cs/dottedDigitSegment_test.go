// dottedDigitSegment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// Guards joinDigitLeadingParts (identifierNaming.go): a path element whose dot-separated parts start
// with a digit -- Go 1.26's crypto/internal/entropy/v1.0.0 -- is one namespace level, `v1_0_0`, on the
// DECLARATION side and the IMPORT side alike. Split on its dots it emitted `namespace ….v1.0.0;`, where
// `0` is not an identifier (13 syntax errors on linux and darwin), and every importer's `using` named
// the same unparseable namespace. The fixture keeps the stdlib shape: the directory is the version, the
// package is named for its parent, and a second package imports it.

func TestDottedDigitPathSegmentIsOneNamespaceLevel(t *testing.T) {
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/dotseg\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "ent", "v1.0.0", "ent.go"), "package ent\n\nvar Seed = 21\n")
	writeModuleFile(t, filepath.Join(appDir, "use", "use.go"), "package use\n\nimport ent \"example.com/dotseg/ent/v1.0.0\"\n\nfunc Twice() int { return ent.Seed * 2 }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/dotseg/use\"\n)\n\nfunc main() { fmt.Println(use.Twice()) }\n")

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
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	moduleOut := filepath.Join(options.go2csPath, "src", "example.com", "dotseg")
	producer := readGenerated(t, filepath.Join(moduleOut, "ent", "v1.0.0", "ent.cs"))
	consumer := readGenerated(t, filepath.Join(moduleOut, "use", "use.cs"))

	declared := regexp.MustCompile(`(?m)^namespace ([^;]+);`).FindStringSubmatch(producer)

	if declared == nil {
		t.Fatalf("the versioned package declares no namespace:\n%s", producer)
	}

	namespace := declared[1]
	digitLevel := regexp.MustCompile(`\.[0-9]`)

	if digitLevel.MatchString(namespace) {
		t.Errorf("the declared namespace has a level that starts with a digit (not a C# identifier): %s", namespace)
	}

	if !strings.HasSuffix(namespace, ".v1_0_0") {
		t.Errorf("the version element is not one namespace level `v1_0_0`: %s", namespace)
	}

	// The importer names exactly the namespace the package declares.
	if !strings.Contains(consumer, namespace+".ent_package") {
		t.Errorf("the importer does not name the declared namespace %s.ent_package:\n%s", namespace, consumer)
	}

	for _, line := range strings.Split(consumer, "\n") {
		if strings.HasPrefix(strings.TrimSpace(line), "using ") && digitLevel.MatchString(line) {
			t.Errorf("an importer's using names a digit-led namespace level: %s", strings.TrimSpace(line))
		}
	}

	if _, err := os.Stat(filepath.Join(moduleOut, "ent", "v1.0.0")); err != nil {
		t.Fatalf("the versioned package's output directory moved: %v", err)
	}
}
