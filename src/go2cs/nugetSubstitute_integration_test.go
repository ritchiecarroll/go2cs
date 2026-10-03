// nugetSubstitute_integration_test.go - Gbtc
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
	"io/fs"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"go2cs/internal/sourcemeta"

	"golang.org/x/mod/module"
	"golang.org/x/mod/sumdb/dirhash"
	modzip "golang.org/x/mod/zip"
)

const shapeModulePath = "example.test/shape"

// newShapeModuleProxy serves example.test/shape v1.0.0 -- a type and a function returning it -- from a
// file GOPROXY into an empty module cache, and returns an app module importing it with a complete go.sum.
func newShapeModuleProxy(t *testing.T) (appDir string, options Options) {
	t.Helper()

	root := t.TempDir()
	proxy := filepath.Join(root, "proxy")
	cache := filepath.Join(root, "modcache")
	version := "v1.0.0"
	src := filepath.Join(root, "shapesrc")
	goMod := "module " + shapeModulePath + "\n\ngo 1.23\n"

	writeModuleFile(t, filepath.Join(src, "go.mod"), goMod)
	writeModuleFile(t, filepath.Join(src, "shape.go"), "package shape\n\n// Domain is a small named type.\ntype Domain int\n\n// Get returns a Domain.\nfunc Get() Domain {\n\treturn 7\n}\n")

	versionDir := filepath.Join(proxy, shapeModulePath, "@v")
	writeModuleFile(t, filepath.Join(versionDir, version+".mod"), goMod)
	writeModuleFile(t, filepath.Join(versionDir, version+".info"), fmt.Sprintf(`{"Version":%q,"Time":"2026-10-02T00:00:00Z"}`, version))
	writeModuleFile(t, filepath.Join(versionDir, "list"), version+"\n")

	zipPath := filepath.Join(versionDir, version+".zip")
	zipFile, err := os.Create(zipPath)

	if err != nil {
		t.Fatal(err)
	}

	if err := modzip.CreateFromDir(zipFile, module.Version{Path: shapeModulePath, Version: version}, src); err != nil {
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

	proxyURL := "file://" + filepath.ToSlash(proxy)

	if runtime.GOOS == "windows" {
		proxyURL = "file:///" + filepath.ToSlash(proxy)
	}

	t.Setenv("GOPROXY", proxyURL)
	t.Setenv("GOSUMDB", "off")
	t.Setenv("GOMODCACHE", cache)
	t.Setenv("GOFLAGS", "-mod=readonly")
	t.Setenv("GOWORK", "off")
	t.Setenv("GOTOOLCHAIN", "local")

	savedModCache := goModCache
	goModCache = cache
	t.Cleanup(func() { goModCache = savedModCache })

	appDir = filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), fmt.Sprintf("module example.test/app\n\ngo 1.23\n\nrequire %s %s\n", shapeModulePath, version))
	writeModuleFile(t, filepath.Join(appDir, "go.sum"), fmt.Sprintf("%s %s %s\n%s %s/go.mod %s\n", shapeModulePath, version, zipHash, shapeModulePath, version, modHash))
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\""+shapeModulePath+"\"\n)\n\nfunc main() {\n\tvar d shape.Domain = shape.Get()\n\tfmt.Println(int(d))\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	build.Default.GOROOT = goRoot

	return appDir, Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		recurse:             true,
		nugetRefs:           true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}
}

// generatedText concatenates every generated .cs and .csproj under dir.
func generatedText(t *testing.T, dir string) string {
	t.Helper()
	var text strings.Builder

	_ = filepath.WalkDir(dir, func(path string, entry fs.DirEntry, err error) error {
		if err == nil && !entry.IsDir() && (strings.HasSuffix(path, ".cs") || strings.HasSuffix(path, ".csproj")) {
			data, _ := os.ReadFile(path)
			text.Write(data)
		}

		return nil
	})

	return text.String()
}

// S3b end to end: under -recurse=nuget with no -nuget-map flag (the default is now ON), a module the registry
// maps and whose published package carries a matching self-description is REFERENCED, not converted: one
// exact-pinned PackageReference, nothing under pkg/, no go2cs.modules.lock line, the package version and its
// SHA-512 in go2cs.nuget.lock -- and the importer reads the package's METADATA: the self-description records a
// Domain -> ΔDomain alias a local conversion would never produce, and the app's emission follows it. The control
// run (-nuget-map off) converts the module locally and never says ΔDomain.
func TestRecurseNuGetSubstitutesAMappedModule(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: go mod download + go/packages over a file-proxy module cache")
	}

	f := newNuGetMapFixture(t)
	f.set("/registry", mapRow(shapeModulePath, "nugetgo.example.test.shape", "canonical"))
	nupkg := buildNupkg(t, map[string]string{sourcemeta.EntryPath: selfDescription(t, shapeModulePath, "v1.0.0", corpusRelease(), nil,
		"// <ExportedTypeAliases>", `[assembly: GoTypeAlias("Domain", "ΔDomain")]`, "// </ExportedTypeAliases>")})
	f.publish("nugetgo.example.test.shape", map[string][]byte{"1.0.0": nupkg})

	appDir, options := newShapeModuleProxy(t)
	root := filepath.Dir(appDir)

	mapped := options
	mapped.recurseOutputRoot = filepath.Join(root, "out-mapped")

	if err := NewModuleConverter(mapped).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule (mapped): %v", err)
	}

	app := generatedText(t, filepath.Join(mapped.recurseOutputRoot, "src", "example.test", "app"))

	if !strings.Contains(app, `<PackageReference Include="nugetgo.example.test.shape" Version="[1.0.0]" />`) {
		t.Errorf("the app project does not reference the package exact-pinned:\n%s", app)
	}

	if strings.Contains(app, "example.test/shape/") || strings.Contains(app, `example.test\shape\`) {
		t.Errorf("the app project still references the module as a project")
	}

	if !strings.Contains(app, "ΔDomain") {
		t.Errorf("the importer did not read the package's self-description (no ΔDomain in its emission)")
	}

	if _, err := os.Stat(filepath.Join(mapped.recurseOutputRoot, "pkg", "example.test", "shape")); !os.IsNotExist(err) {
		t.Errorf("the substituted module was converted under pkg/ anyway")
	}

	if data, err := os.ReadFile(modulesLockPath(mapped.recurseOutputRoot)); err == nil && strings.Contains(string(data), shapeModulePath) {
		t.Errorf("the substituted module is in go2cs.modules.lock:\n%s", data)
	}

	lock, err := readNuGetLock(mapped.recurseOutputRoot)

	if entry := lock[shapeModulePath]; err != nil || entry.packageVersion != "1.0.0" || entry.contentHash != nupkgHash(nupkg) {
		t.Errorf("go2cs.nuget.lock holds %+v (err %v); want package 1.0.0 with its SHA-512", entry, err)
	}

	control := options
	control.recurseOutputRoot = filepath.Join(root, "out-control")
	control.nugetMap = nugetMapOptions{off: true}

	if err := NewModuleConverter(control).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule (control): %v", err)
	}

	if _, err := os.Stat(filepath.Join(control.recurseOutputRoot, "pkg", "example.test", "shape")); err != nil {
		t.Errorf("the control run did not convert the module locally: %v", err)
	}

	if strings.Contains(generatedText(t, filepath.Join(control.recurseOutputRoot, "src", "example.test", "app")), "ΔDomain") {
		t.Errorf("the control run's emission says ΔDomain without the package's metadata, so the mapped run's ΔDomain proves nothing")
	}
}
