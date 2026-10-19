// sameNameImplementRecords_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// Two packages of ONE NAME, the shape Go 1.24's fips140 split made common: crypto/sha256 wraps
// crypto/internal/fips140/sha256. The public package converts the internal one's *Digest to hash.Hash, so its
// package_info records `GoImplement<go.crypto.@internal.fips140.sha256_package.Digest, hash_package.Hash>(Pointer
// = true)`: a LOCAL adapter of ITS assembly for a FOREIGN struct. The import reader keyed records by (declaring
// package NAME, struct SIMPLE name, interface), so a consumer that loaded the public package read it as
// `sha256|Digest|hash.Hash`, the internal package's own adapter, and named `sha256.DigestжHash` in the internal
// package, where nothing declares it: crypto/internal/fips140test at go1.26.9, CS0426 x206.
//
// Here: inner/x declares D, outer/x (also package x) converts *inner/x.D to hashlike.Hash, and the consumer imports
// both and converts *inner/x.D itself. A record whose struct is qualified to another package's class proves nothing
// about that package, so the consumer must take the local-adapter arm (`x_DжHash`, recorded here), never name an
// adapter in inner/x. Control: inner2/y converts its OWN *E to Hash, so its record is real proof, and the consumer
// names y's published adapter.

func TestSameNamedPackageForeignStructRecordIsNotTheOtherPackagesAdapter(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/snr\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "hashlike", "hashlike.go"), `package hashlike

type Hash interface{ Sum() int }
`)
	writeModuleFile(t, filepath.Join(appDir, "inner", "x", "x.go"), `package x

type D struct{ n int }

func (d *D) Sum() int { return d.n }

func New() *D { return &D{n: 1} }
`)
	writeModuleFile(t, filepath.Join(appDir, "outer", "x", "x.go"), `package x

import (
	"example.com/snr/hashlike"
	ix "example.com/snr/inner/x"
)

func New() hashlike.Hash { return ix.New() }
`)
	writeModuleFile(t, filepath.Join(appDir, "inner2", "y", "y.go"), `package y

import "example.com/snr/hashlike"

type E struct{ n int }

func (e *E) Sum() int { return e.n }

func New() hashlike.Hash { return &E{n: 2} }

func Raw() *E { return &E{n: 3} }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/snr/hashlike"
	"example.com/snr/inner2/y"
	ix "example.com/snr/inner/x"
	ox "example.com/snr/outer/x"
)

func main() {
	var h hashlike.Hash = ix.New()
	var g hashlike.Hash = y.Raw()
	fmt.Println(h.Sum(), g.Sum(), ox.New().Sum(), y.New().Sum())
}
`)

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

	dir := filepath.Join(options.go2csPath, "src", "example.com", "snr")
	outerInfo := readGenerated(t, filepath.Join(dir, "outer", "x", "package_info.cs"))
	mainCS := readGenerated(t, filepath.Join(dir, "main.cs"))
	mainInfo := readGenerated(t, filepath.Join(dir, "package_info.cs"))

	// The premise: outer/x records a local adapter for inner/x's struct, qualified to inner/x's class.
	if !strings.Contains(outerInfo, "x_package.D, ") || !strings.Contains(outerInfo, "(Pointer = true)") {
		t.Fatalf("premise: outer/x must record its local adapter for inner/x's *D:\n%s", outerInfo)
	}

	for _, arm := range []struct {
		name, text, want string
	}{
		{"the consumer builds its own adapter for inner/x's *D", mainCS, "new x_DжHash("},
		{"and records it", mainInfo, "[assembly: GoImplement<go.example.com.snr.inner.x_package.D, go.example.com.snr.hashlike_package.Hash>(Pointer = true)]"},
		{"control: y's own record still names y's published adapter", mainCS, "new y.EжHash("},
	} {
		if !strings.Contains(arm.text, arm.want) {
			t.Errorf("%s: missing %s; the records:\n%s", arm.name, arm.want, implementRecordLines(arm.text))
		}
	}

	if strings.Contains(mainCS, "ix.DжHash") {
		t.Errorf("the consumer names an adapter in inner/x that only outer/x's record implied:\n%s", mainCS)
	}
}

// implementRecordLines is the GoImplement records of a package_info.cs, for a failure message.
func implementRecordLines(text string) string {
	var records []string

	for _, line := range strings.Split(text, "\n") {
		if strings.Contains(line, "GoImplement<") {
			records = append(records, strings.TrimSpace(line))
		}
	}

	return strings.Join(records, "\n")
}
