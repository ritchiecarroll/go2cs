// moduleCacheLock_integration_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).


package main

import (
	"fmt"
	"go/build"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"golang.org/x/mod/module"
	"golang.org/x/mod/sumdb/dirhash"
	modzip "golang.org/x/mod/zip"
)

func nonCommentLines(text string) []string {
	var lines []string

	for _, line := range strings.Split(text, "\n") {
		if line != "" && !strings.HasPrefix(line, "#") {
			lines = append(lines, line)
		}
	}

	return lines
}

// moduleProxyFixture is a hermetic, network-free module source: a file-based GOPROXY holding
// example.test/dep at two versions, and an EMPTY module cache outside GOPATH that `go mod download`
// fills from it -- so every test below exercises a REAL `<module>@<version>` cache entry, unpacked
// by the go command itself, with no third-party code and no network.
type moduleProxyFixture struct {
	root    string
	cache   string
	goSums  map[string]string // "version" -> the two go.sum lines for example.test/dep at that version
	options Options
}

const fixtureDepPath = "example.test/dep"

func newModuleProxyFixture(t *testing.T) *moduleProxyFixture {
	t.Helper()

	root := t.TempDir()
	proxy := filepath.Join(root, "proxy")
	fixture := &moduleProxyFixture{root: root, cache: filepath.Join(root, "modcache"), goSums: make(map[string]string)}

	for _, version := range []string{"v1.0.0", "v1.1.0"} {
		src := filepath.Join(root, "depsrc", version)
		goMod := "module " + fixtureDepPath + "\n\ngo 1.23\n"

		writeModuleFile(t, filepath.Join(src, "go.mod"), goMod)
		writeModuleFile(t, filepath.Join(src, "dep.go"),
			fmt.Sprintf("package dep\n\nfunc Version() string {\n\treturn %q\n}\n", version))

		versionDir := filepath.Join(proxy, fixtureDepPath, "@v")
		writeModuleFile(t, filepath.Join(versionDir, version+".mod"), goMod)
		writeModuleFile(t, filepath.Join(versionDir, version+".info"), fmt.Sprintf(`{"Version":%q,"Time":"2026-09-30T00:00:00Z"}`, version))

		zipPath := filepath.Join(versionDir, version+".zip")
		zipFile, err := os.Create(zipPath)

		if err != nil {
			t.Fatal(err)
		}

		if err := modzip.CreateFromDir(zipFile, module.Version{Path: fixtureDepPath, Version: version}, src); err != nil {
			zipFile.Close()
			t.Fatal(err)
		}

		zipFile.Close()

		zipHash, err := dirhash.HashZip(zipPath, dirhash.Hash1)

		if err != nil {
			t.Fatal(err)
		}

		modHash, err := dirhash.Hash1([]string{"go.mod"}, func(string) (io.ReadCloser, error) {
			return io.NopCloser(strings.NewReader(goMod)), nil
		})

		if err != nil {
			t.Fatal(err)
		}

		fixture.goSums[version] = fmt.Sprintf("%s %s %s\n%s %s/go.mod %s\n", fixtureDepPath, version, zipHash, fixtureDepPath, version, modHash)
	}

	writeModuleFile(t, filepath.Join(proxy, fixtureDepPath, "@v", "list"), "v1.0.0\nv1.1.0\n")

	proxyURL := "file://" + filepath.ToSlash(proxy)

	if runtime.GOOS == "windows" {
		proxyURL = "file:///" + filepath.ToSlash(proxy)
	}

	t.Setenv("GOPROXY", proxyURL)
	t.Setenv("GOSUMDB", "off")
	t.Setenv("GOMODCACHE", fixture.cache)
	t.Setenv("GOFLAGS", "-mod=readonly")
	t.Setenv("GOWORK", "off")
	t.Setenv("GOTOOLCHAIN", "local")

	savedModCache := goModCache
	goModCache = fixture.cache
	t.Cleanup(func() { goModCache = savedModCache })

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	fixture.options = Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = fixture.options.goRoot
	build.Default.GOPATH = fixture.options.goPath

	return fixture
}

// app writes a main module requiring example.test/dep at version, with a complete go.sum.
func (f *moduleProxyFixture) app(t *testing.T, name, version string) string {
	t.Helper()

	appDir := filepath.Join(f.root, name)
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		fmt.Sprintf("module example.test/%s\n\ngo 1.23\n\nrequire %s %s\n", name, fixtureDepPath, version))
	writeModuleFile(t, filepath.Join(appDir, "go.sum"), f.goSums[version])
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\""+fixtureDepPath+"\"\n)\n\nfunc main() {\n\tfmt.Println(dep.Version())\n}\n")

	return appDir
}

// TestModuleCacheEndToEndLockAndClash converts a main module whose dependency comes from a REAL
// module-cache entry, then refuses a second module needing another version of that dependency in the
// same output root. RED at master: no lock was written, and the second conversion silently overwrote
// pkg/example.test/dep with v1.1.0 under the first module's feet.
func TestModuleCacheEndToEndLockAndClash(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: go mod download + go/packages over a file-proxy module cache")
	}

	fixture := newModuleProxyFixture(t)
	outRoot := fixture.options.recurseOutputRoot

	if err := NewModuleConverter(fixture.options).ConvertModule(fixture.app(t, "first", "v1.0.0")); err != nil {
		t.Fatalf("ConvertModule(first): %v", err)
	}

	// The dependency came from the cache, not a replace: it is under the fixture's GOMODCACHE.
	if _, err := os.Stat(filepath.Join(fixture.cache, fixtureDepPath+"@v1.0.0", "dep.go")); err != nil {
		t.Fatalf("the preflight did not place %s@v1.0.0 in the module cache: %v", fixtureDepPath, err)
	}

	depCs := readGenerated(t, filepath.Join(outRoot, "pkg", "example.test", "dep", "dep.cs"))

	if !strings.Contains(depCs, `"v1.0.0"`) {
		t.Errorf("pkg/%s/dep.cs is not v1.0.0's:\n%s", fixtureDepPath, depCs)
	}

	// Read as TEXT, so this guard stands on master's API alone: `<module> <version> <h1 hash> <converter>`.
	lockData, err := os.ReadFile(filepath.Join(outRoot, "go2cs.modules.lock"))

	if err != nil {
		t.Fatalf("no go2cs.modules.lock in the output root: %v", err)
	}

	lock := nonCommentLines(strings.ReplaceAll(string(lockData), "\r\n", "\n"))
	var entry []string

	if len(lock) == 1 {
		entry = strings.Fields(lock[0])
	}

	if len(entry) != 4 || entry[0] != fixtureDepPath || entry[1] != "v1.0.0" || !strings.HasPrefix(entry[2], "h1:") {
		t.Fatalf("lock = %q, want exactly one line: %s v1.0.0 <go.sum h1 hash> <converter> (the main module is not locked)", lock, fixtureDepPath)
	}

	// The same version into the same root is fine.
	if err := NewModuleConverter(fixture.options).ConvertModule(fixture.app(t, "second", "v1.0.0")); err != nil {
		t.Fatalf("ConvertModule(second, same version): %v", err)
	}

	// Another version into the same root is refused, and nothing of it is written.
	err = NewModuleConverter(fixture.options).ConvertModule(fixture.app(t, "third", "v1.1.0"))

	if err == nil {
		t.Fatalf("a module needing %s@v1.1.0 converted into a root locked at v1.0.0", fixtureDepPath)
	}

	for _, want := range []string{fixtureDepPath, "locked at v1.0.0", "needs v1.1.0"} {
		if !strings.Contains(err.Error(), want) {
			t.Errorf("refusal does not name %q:\n%v", want, err)
		}
	}

	if depCs = readGenerated(t, filepath.Join(outRoot, "pkg", "example.test", "dep", "dep.cs")); !strings.Contains(depCs, `"v1.0.0"`) {
		t.Errorf("the refused conversion still overwrote pkg/%s:\n%s", fixtureDepPath, depCs)
	}

	if _, err := os.Stat(filepath.Join(outRoot, "src", "example.test", "third")); !os.IsNotExist(err) {
		t.Errorf("the refused conversion wrote its app output")
	}
}

// TestModuleCachePreflightNamesMissingDependency: a dependency that is in neither the cache nor the
// proxy is ONE refusal from the `go mod download` preflight, naming it, before anything loads.
func TestModuleCachePreflightNamesMissingDependency(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: go mod download over a file-proxy module cache")
	}

	fixture := newModuleProxyFixture(t)
	appDir := filepath.Join(fixture.root, "missing")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.test/missing\n\ngo 1.23\n\nrequire example.test/absent v1.0.0\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport _ \"example.test/absent\"\n\nfunc main() {}\n")

	err := NewModuleConverter(fixture.options).ConvertModule(appDir)

	if err == nil {
		t.Fatalf("a module requiring an absent dependency converted")
	}

	if !strings.Contains(err.Error(), "preflight") || !strings.Contains(err.Error(), "example.test/absent") {
		t.Errorf("the refusal is not the preflight naming example.test/absent:\n%v", err)
	}

	if _, statErr := os.Stat(fixture.options.recurseOutputRoot); !os.IsNotExist(statErr) {
		t.Errorf("the refused conversion created the output root")
	}
}

// TestModuleCacheOutsideGopathResolvesAsCacheDependency guards the stale resolver: a SINGLE-package
// conversion (not -recurse) importing a module-cache dependency when GOMODCACHE is not
// $GOPATH/pkg/mod. getLocalModulePackageInfo compared against the hardcoded GOPATH path, so the dep
// was taken for a LOCAL module: an in-place output inside the read-only cache and a relative
// reference to it. It must reference $(go2csPath)pkg/<import-path>.
func TestModuleCacheOutsideGopathResolvesAsCacheDependency(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: go mod download + go/packages over a file-proxy module cache")
	}

	fixture := newModuleProxyFixture(t)
	appDir := fixture.app(t, "single", "v1.0.0")

	// Populate the cache the way the go command does (a single-package conversion runs no preflight).
	download := exec.Command("go", "mod", "download")
	download.Dir = appDir

	if output, err := download.CombinedOutput(); err != nil {
		t.Fatalf("go mod download: %v\n%s", err, output)
	}

	options := fixture.options
	options.recurse = false
	options.recurseOutputRoot = ""
	outDir := filepath.Join(fixture.root, "singleout")

	if err := processConversion(appDir, false, outDir, options); err != nil {
		t.Fatalf("processConversion: %v", err)
	}

	matches, _ := filepath.Glob(filepath.Join(outDir, "*.csproj"))

	if len(matches) != 1 {
		t.Fatalf("want one csproj in %s, got %v", outDir, matches)
	}

	csproj := readGenerated(t, matches[0])

	if !strings.Contains(csproj, "$(go2csPath)pkg/"+fixtureDepPath) {
		t.Errorf("the cache dependency is not referenced at $(go2csPath)pkg/%s:\n%s", fixtureDepPath, csproj)
	}
}
