// toolchainProvision_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"archive/tar"
	"archive/zip"
	"bytes"
	"compress/gzip"
	"crypto/sha256"
	"encoding/hex"
	"io"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"sync/atomic"
	"testing"

	"golang.org/x/mod/modfile"
	"golang.org/x/mod/sumdb/dirhash"
)

const fakeRelease = "go1.99.1"

// fakeToolchainFiles is the smallest tree checkRelease accepts, plus the two things a module zip needs
// finished: a command without its execute bit and a stored _go.mod.
func fakeToolchainFiles(goos string) map[string]string {
	return map[string]string{
		"VERSION":                  fakeRelease + "\ntime 2026-10-21T00:00:00Z\n",
		"bin/go" + exeSuffix(goos): "#!/bin/sh\necho fake\n",
		"pkg/tool/x_y/compile":     "tool",
		"src/_go.mod":              "module std\n",
		"src/cmd/_go.mod":          "module cmd\n",
		"src/fmt/print.go":         "package fmt\n",
	}
}

func buildZip(t *testing.T, prefix string, files map[string]string) []byte {
	t.Helper()

	var buffer bytes.Buffer
	writer := zip.NewWriter(&buffer)

	for name, body := range files {
		w, err := writer.Create(prefix + name)
		if err != nil {
			t.Fatal(err)
		}

		io.WriteString(w, body)
	}

	if err := writer.Close(); err != nil {
		t.Fatal(err)
	}

	return buffer.Bytes()
}

func buildTarGz(t *testing.T, prefix string, files map[string]string) []byte {
	t.Helper()

	var buffer bytes.Buffer
	gz := gzip.NewWriter(&buffer)
	writer := tar.NewWriter(gz)

	for name, body := range files {
		mode := int64(0o644)

		if strings.HasPrefix(name, "bin/") || strings.HasPrefix(name, "pkg/tool/") {
			mode = 0o755
		}

		header := &tar.Header{Name: prefix + name, Mode: mode, Size: int64(len(body)), Typeflag: tar.TypeReg}

		if err := writer.WriteHeader(header); err != nil {
			t.Fatal(err)
		}

		io.WriteString(writer, body)
	}

	if err := writer.Close(); err != nil {
		t.Fatal(err)
	}

	gz.Close()

	return buffer.Bytes()
}

func moduleZipHash(t *testing.T, data []byte) string {
	t.Helper()

	file := filepath.Join(t.TempDir(), "module.zip")

	if err := os.WriteFile(file, data, 0o644); err != nil {
		t.Fatal(err)
	}

	sum, err := dirhash.HashZip(file, dirhash.Hash1)
	if err != nil {
		t.Fatal(err)
	}

	return sum
}

// testProvisioner wires a provisioner to an in-memory environment and the given sources.
func testProvisioner(t *testing.T, env map[string]string, sources []toolchainSource) *toolchainProvisioner {
	t.Helper()

	return &toolchainProvisioner{
		goVersion: fakeRelease,
		goos:      runtime.GOOS,
		goarch:    runtime.GOARCH,
		cacheRoot: filepath.Join(t.TempDir(), "toolchains"),
		getenv:    func(name string) string { return env[name] },
		setenv:    func(name, value string) error { env[name] = value; return nil },
		pathGo:    func() string { return env["test.pathGo"] },
		buildRoot: func() string { return env["test.buildRoot"] },
		sources:   func() []toolchainSource { return sources },
		client:    http.DefaultClient,
		progress:  io.Discard,
	}
}

func serve(t *testing.T, routes map[string][]byte) (*httptest.Server, *atomic.Int32) {
	t.Helper()

	var hits atomic.Int32

	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		hits.Add(1)

		body, ok := routes[r.URL.Path]
		if !ok {
			http.NotFound(w, r)
			return
		}

		w.Write(body)
	}))

	t.Cleanup(server.Close)

	return server, &hits
}

func moduleSource(p *toolchainProvisioner, proxy, sum string) toolchainSource {
	saved := pinnedGoModules
	pinnedGoModules = map[string]string{p.goos + "-" + p.goarch: sum}
	defer func() { pinnedGoModules = saved }()

	savedArchives := pinnedGoArchives
	pinnedGoArchives = map[string]goArchivePin{}
	defer func() { pinnedGoArchives = savedArchives }()

	env := map[string]string{"GOPROXY": proxy}
	getenv := p.getenv
	p.getenv = func(name string) string { return env[name] }
	defer func() { p.getenv = getenv }()

	return p.defaultSources()[0]
}

func assertInstalled(t *testing.T, p *toolchainProvisioner, env map[string]string) {
	t.Helper()

	root := p.cachedRoot()

	if err := p.checkRelease(root); err != nil {
		t.Fatalf("no whole install at %s: %v", root, err)
	}

	bin := filepath.Join(root, "bin")

	if env["GOROOT"] != root || env["GOTOOLCHAIN"] != "local" || env["PATH"] != bin && !strings.HasPrefix(env["PATH"], bin+string(os.PathListSeparator)) {
		t.Fatalf("environment not exported: GOROOT=%q GOTOOLCHAIN=%q PATH=%q", env["GOROOT"], env["GOTOOLCHAIN"], env["PATH"])
	}

	if runtime.GOOS != "windows" {
		for _, command := range []string{"bin/go", "pkg/tool/x_y/compile"} {
			info, err := os.Stat(filepath.Join(root, filepath.FromSlash(command)))
			if err != nil || info.Mode()&0o111 == 0 {
				t.Fatalf("%s is not executable after install (%v)", command, err)
			}
		}
	}
}

// The module-proxy route: a module zip records no modes and stores _go.mod, so both are finished the
// way the go command finishes a toolchain it downloaded.
func TestToolchainProvisionFromModuleProxy(t *testing.T) {
	platform := runtime.GOOS + "-" + runtime.GOARCH
	module := toolchainModuleVersion(fakeRelease, platform)
	data := buildZip(t, "golang.org/toolchain@"+module+"/", fakeToolchainFiles(runtime.GOOS))
	server, _ := serve(t, map[string][]byte{"/golang.org/toolchain/@v/" + module + ".zip": data})

	env := map[string]string{"PATH": "/usr/bin"}
	p := testProvisioner(t, env, nil)
	source := moduleSource(p, server.URL, moduleZipHash(t, data))
	p.sources = func() []toolchainSource { return []toolchainSource{source} }

	if err := p.ensure(true); err != nil {
		t.Fatal(err)
	}

	assertInstalled(t, p, env)

	for _, dir := range []string{"src", "src/cmd"} {
		if _, err := os.Stat(filepath.Join(p.cachedRoot(), filepath.FromSlash(dir), "go.mod")); err != nil {
			t.Fatalf("%s/_go.mod was not copied to go.mod: %v", dir, err)
		}
	}
}

// The release-archive route keeps the archive's own modes.
func TestToolchainProvisionFromReleaseArchive(t *testing.T) {
	data := buildTarGz(t, "go/", fakeToolchainFiles(runtime.GOOS))
	if runtime.GOOS == "windows" {
		data = buildZip(t, "go/", fakeToolchainFiles(runtime.GOOS))
	}

	sum := sha256.Sum256(data)
	server, _ := serve(t, map[string][]byte{"/archive": data})

	env := map[string]string{}
	p := testProvisioner(t, env, nil)
	file := "archive.tar.gz"
	if runtime.GOOS == "windows" {
		file = "archive.zip"
	}

	source := toolchainSource{
		name:   "fake",
		url:    server.URL + "/archive",
		verify: func(path string) error { return verifySHA256(path, hex.EncodeToString(sum[:])) },
		unpack: func(path, dest string) error {
			if strings.HasSuffix(file, ".zip") {
				return unpackZip(path, dest, "go/")
			}

			return unpackTarGz(path, dest, "go/")
		},
	}

	p.sources = func() []toolchainSource { return []toolchainSource{source} }

	if err := p.ensure(true); err != nil {
		t.Fatal(err)
	}

	assertInstalled(t, p, env)
}

// A download that does not match its pin is refused, nothing is left where checkRelease would accept
// it, and the next source is tried.
func TestToolchainProvisionRefusesAMismatchedDownload(t *testing.T) {
	platform := runtime.GOOS + "-" + runtime.GOARCH
	module := toolchainModuleVersion(fakeRelease, platform)
	good := buildZip(t, "golang.org/toolchain@"+module+"/", fakeToolchainFiles(runtime.GOOS))
	tampered := map[string]string{}

	for name, body := range fakeToolchainFiles(runtime.GOOS) {
		tampered[name] = body
	}

	tampered["src/fmt/print.go"] = "package fmt // changed\n"
	bad := buildZip(t, "golang.org/toolchain@"+module+"/", tampered)

	badServer, _ := serve(t, map[string][]byte{"/golang.org/toolchain/@v/" + module + ".zip": bad})

	env := map[string]string{}
	p := testProvisioner(t, env, nil)
	sum := moduleZipHash(t, good)
	only := moduleSource(p, badServer.URL, sum)
	p.sources = func() []toolchainSource { return []toolchainSource{only} }

	err := p.ensure(true)
	if err == nil || !strings.Contains(err.Error(), "does not match the pinned") {
		t.Fatalf("a tampered toolchain was accepted or refused for the wrong reason: %v", err)
	}

	if p.checkRelease(p.cachedRoot()) == nil || env["GOROOT"] != "" {
		t.Fatal("a refused download left an install or an exported GOROOT behind")
	}

	entries, _ := os.ReadDir(p.cacheRoot)
	if len(entries) != 0 {
		t.Fatalf("a refused download left %d entries in the cache", len(entries))
	}

	goodServer, _ := serve(t, map[string][]byte{"/golang.org/toolchain/@v/" + module + ".zip": good})
	second := moduleSource(p, goodServer.URL, sum)
	p.sources = func() []toolchainSource { return []toolchainSource{only, second} }

	if err := p.ensure(true); err != nil {
		t.Fatalf("the second source was not tried after the first was refused: %v", err)
	}

	assertInstalled(t, p, env)
}

// Each earlier step of the choice wins without a download; -version and help never download.
func TestToolchainProvisionChoosesWithoutDownloading(t *testing.T) {
	server, hits := serve(t, map[string][]byte{})
	never := toolchainSource{name: "never", url: server.URL + "/x", verify: func(string) error { return nil }, unpack: func(string, string) error { return nil }}

	t.Run("operator GOROOT", func(t *testing.T) {
		env := map[string]string{"GOROOT": "/some/go"}
		p := testProvisioner(t, env, []toolchainSource{never})

		if err := p.ensure(true); err != nil || env["GOTOOLCHAIN"] != "" {
			t.Fatalf("an operator GOROOT was overridden: %v %v", err, env)
		}
	})

	t.Run("pinned go on PATH", func(t *testing.T) {
		env := map[string]string{"test.pathGo": fakeRelease}
		p := testProvisioner(t, env, []toolchainSource{never})

		if err := p.ensure(true); err != nil || env["GOROOT"] != "" {
			t.Fatalf("the pinned go on PATH was overridden: %v %v", err, env)
		}
	})

	t.Run("build GOROOT of the pinned release", func(t *testing.T) {
		root := t.TempDir()
		os.MkdirAll(filepath.Join(root, "bin"), 0o755)
		os.WriteFile(filepath.Join(root, "VERSION"), []byte(fakeRelease+"\n"), 0o644)
		os.WriteFile(filepath.Join(root, "bin", "go"+exeSuffix(runtime.GOOS)), nil, 0o755)

		env := map[string]string{"test.buildRoot": root}
		p := testProvisioner(t, env, []toolchainSource{never})

		if err := p.ensure(true); err != nil || env["GOROOT"] != "" {
			t.Fatalf("the build GOROOT was not used: %v %v", err, env)
		}
	})

	t.Run("version only", func(t *testing.T) {
		env := map[string]string{}
		p := testProvisioner(t, env, []toolchainSource{never})

		if err := p.ensure(false); err != nil || env["GOROOT"] != "" {
			t.Fatalf("a version-only run did something: %v %v", err, env)
		}
	})

	t.Run("GO2CS_GOROOT of another release", func(t *testing.T) {
		root := t.TempDir()
		os.WriteFile(filepath.Join(root, "VERSION"), []byte("go1.20.1\n"), 0o644)

		env := map[string]string{goRootOverrideEnv: root}
		p := testProvisioner(t, env, []toolchainSource{never})

		if err := p.ensure(true); err == nil || !strings.Contains(err.Error(), "go1.20.1") {
			t.Fatalf("a GO2CS_GOROOT of another release was accepted: %v", err)
		}
	})

	if hits.Load() != 0 {
		t.Fatalf("%d downloads were made where none was needed", hits.Load())
	}

	for _, args := range [][]string{nil, {"-version"}, {"--version"}, {"-h"}, {"-help"}} {
		if !argsNeedNoToolchain(args) {
			t.Fatalf("%q would download a toolchain", args)
		}
	}

	for _, args := range [][]string{{"-version", "x.go"}, {"x.go"}, {"-stdlib"}} {
		if argsNeedNoToolchain(args) {
			t.Fatalf("%q would convert without a toolchain", args)
		}
	}
}

// An entry naming a path outside the destination is never written.
func TestToolchainUnpackRefusesEscapingNames(t *testing.T) {
	data := buildZip(t, "go/", map[string]string{"../escape": "x", "ok": "y"})
	file := filepath.Join(t.TempDir(), "a.zip")
	os.WriteFile(file, data, 0o644)

	dest := filepath.Join(t.TempDir(), "dest")

	if err := unpackZip(file, dest, "go/"); err != nil {
		t.Fatal(err)
	}

	if _, err := os.Stat(filepath.Join(filepath.Dir(dest), "escape")); err == nil {
		t.Fatal("an entry escaped the destination")
	}

	if _, err := os.Stat(filepath.Join(dest, "ok")); err != nil {
		t.Fatal("an ordinary entry was not written")
	}
}

func TestGoProxyURLs(t *testing.T) {
	cases := map[string]string{
		"":                                "https://proxy.golang.org",
		"https://proxy.golang.org,direct": "https://proxy.golang.org",
		"https://a.example/|https://b.example,off": "https://a.example https://b.example",
		"direct": "",
	}

	for value, want := range cases {
		if got := strings.Join(goProxyURLs(value), " "); got != want {
			t.Errorf("goProxyURLs(%q) = %q, want %q", value, got, want)
		}
	}
}

// The pins describe the release this converter is built with, on all six release platforms.
func TestToolchainPinsMatchTheConverterGo(t *testing.T) {
	data, err := os.ReadFile("go.mod")
	if err != nil {
		t.Fatal(err)
	}

	parsed, err := modfile.Parse("go.mod", data, nil)
	if err != nil {
		t.Fatal(err)
	}

	if want := "go" + parsed.Go.Version; pinnedGoVersion != want {
		t.Fatalf("pinnedGoVersion is %s and go.mod's go line is %s: regenerate the pins", pinnedGoVersion, want)
	}

	for _, platform := range []string{"windows-amd64", "windows-arm64", "linux-amd64", "linux-arm64", "darwin-amd64", "darwin-arm64"} {
		if !strings.HasPrefix(pinnedGoModules[platform], "h1:") {
			t.Errorf("no toolchain module pin for %s", platform)
		}
	}
}
