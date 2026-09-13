// moduleConverter_integration_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"log"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// TestRecurseSyntheticModule is the P5 synthetic integration test: it runs the real -recurse
// conversion over a two-module fixture — an `app` main package importing a co-located `lib` module
// via `replace`, which imports the standard library — and confirms the recurse loop end to end: the
// closure is partitioned, the lib converts before the app (topological order), each converts into the
// parallel tree under an isolated output root (the app to src\, the dependency lib to pkg\ — keeping
// the Go source pure), the cross-package call resolves, references wire relatively to the lib and via
// $(go2csPath) to the stdlib, and a folder-grouped recurse solution is emitted. Deterministic and
// network-free.
func TestRecurseSyntheticModule(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/lib v0.0.0\n\nreplace example.com/lib => ../lib\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/lib\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
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

	// getImportPackageInfo resolves stdlib references through build.Default; pin it as main() does.
	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	// The convert-set is exactly {app, lib}, ordered least-dependencies-first (lib before app).
	queue := converter.graph.sortedQueue

	if len(queue) != 2 || queue[0] != "example.com/lib" || queue[1] != "example.com/app" {
		t.Fatalf("convert order = %v, want [example.com/lib example.com/app]", queue)
	}

	// Each package converts into the parallel tree under the deploy root — the app to src\<import>,
	// the dependency lib to pkg\<import> — leaving the original Go source directories pure.
	mainCs := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "main.cs"))
	greetingCs := readGenerated(t, filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib", "greeting.cs"))

	if !strings.Contains(mainCs, "lib.Greeting") {
		t.Errorf("app main.cs missing the cross-package call lib.Greeting:\n%s", mainCs)
	}

	if !strings.Contains(greetingCs, "strings.TrimSpace") {
		t.Errorf("lib greeting.cs missing strings.TrimSpace:\n%s", greetingCs)
	}

	// The original Go source dirs must stay pure — no C# artifacts written in place.
	if _, err := os.Stat(filepath.Join(appDir, "main.cs")); !os.IsNotExist(err) {
		t.Errorf("app Go source dir was polluted with in-place C# output (main.cs)")
	}

	// The app csproj references the converted lib relatively within the isolated output tree and
	// the stdlib fmt via the independently selectable $(go2csPath) runtime root.
	appProjectDir := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	libProject := filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib", "example.com.lib.csproj")
	appCsproj := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.csproj"))
	libReference, err := filepath.Rel(appProjectDir, libProject)
	libReference = filepath.ToSlash(libReference)

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(appCsproj, `<ProjectReference Include="`+libReference+`" />`) {
		t.Errorf("app csproj missing the relative lib ProjectReference %q:\n%s", libReference, appCsproj)
	}

	if !strings.Contains(appCsproj, "$(go2csPath)core/fmt/fmt.csproj") {
		t.Errorf("app csproj missing the stdlib fmt reference:\n%s", appCsproj)
	}

	// A per-project solution sits next to the app csproj, over the app + its transitive converted
	// dependency (lib) + the shared runtime (golib) + the analyzer (go2cs-gen) — no stdlib listed. This
	// is the build-everything solution for the app; no separate flat deploy-root solution is written.
	appSlnx := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "example.com.app.slnx"))

	for _, want := range []string{"example.com.app.csproj", "example.com.lib.csproj", "golib.csproj", "go2cs-gen.csproj"} {
		if !strings.Contains(appSlnx, want) {
			t.Errorf("app per-project solution missing %q:\n%s", want, appSlnx)
		}
	}

	// Projects are grouped into the %GOPATH%-mirroring solution folders, emitted in the enforced
	// src → pkg → core order (deliberately NOT alphabetic): the app under /src/, its converted dependency
	// (lib) under /pkg/, and the runtime + analyzer under /core/.
	for _, want := range []string{`<Folder Name="/src/">`, `<Folder Name="/pkg/">`, `<Folder Name="/core/">`} {
		if !strings.Contains(appSlnx, want) {
			t.Errorf("app per-project solution missing folder %q:\n%s", want, appSlnx)
		}
	}

	srcIdx := strings.Index(appSlnx, `<Folder Name="/src/">`)
	pkgIdx := strings.Index(appSlnx, `<Folder Name="/pkg/">`)
	coreIdx := strings.Index(appSlnx, `<Folder Name="/core/">`)

	if !(srcIdx < pkgIdx && pkgIdx < coreIdx) {
		t.Errorf("solution folders not in enforced src→pkg→core order (src=%d pkg=%d core=%d):\n%s", srcIdx, pkgIdx, coreIdx, appSlnx)
	}

	// The app project is marked the Visual Studio default startup project.
	if !strings.Contains(appSlnx, `example.com.app.csproj" DefaultStartup="true"`) {
		t.Errorf("app per-project solution does not mark the app as the startup project:\n%s", appSlnx)
	}

	// The retired flat deploy-root solution must no longer be generated.
	if _, err := os.Stat(filepath.Join(options.recurseOutputRoot, "go2cs-recurse.slnx")); !os.IsNotExist(err) {
		t.Errorf("unexpected flat deploy-root solution go2cs-recurse.slnx was written")
	}

	if strings.Contains(appSlnx, "fmt.csproj") {
		t.Errorf("app per-project solution should not list stdlib projects (found fmt.csproj):\n%s", appSlnx)
	}

	// Issue #36: the emitted output-root Directory.Build.props defaults go2csPath to the runtime
	// root the conversion resolved against, so the $(go2csPath)core/... references resolve without
	// an environment variable or a -p:go2csPath global. The output root here is isolated from the
	// runtime root, so the pin is the absolute resolved path (a re-conversion refreshes it); the
	// Condition keeps every consumer override winning.
	props := readGenerated(t, filepath.Join(options.recurseOutputRoot, "Directory.Build.props"))

	if !strings.Contains(props, `<PropertyGroup Condition="'$(go2csPath)' == ''">`) {
		t.Errorf("output-root Directory.Build.props missing the conditional go2csPath default:\n%s", props)
	}

	runtimeRoot, err := filepath.Abs(options.go2csPath)

	if err != nil {
		t.Fatal(err)
	}

	if want := "<go2csPath>" + filepath.ToSlash(runtimeRoot) + "/</go2csPath>"; !strings.Contains(props, want) {
		t.Errorf("output-root Directory.Build.props missing the runtime-root pin %q:\n%s", want, props)
	}
}

// TestRecurseBuildFilesRelativePinWhenRootsCoincide covers the established one-positional CLI form,
// where the runtime root doubles as the output root: the go2csPath default then stays relative
// ($(MSBuildThisFileDirectory), the deploy-core form) so the tree can move as a unit. No module
// conversion is needed — the build files are a pure function of the two roots.
func TestRecurseBuildFilesRelativePinWhenRootsCoincide(t *testing.T) {
	root := t.TempDir()
	m := NewModuleConverter(Options{go2csPath: root, recurse: true})

	m.generateRecurseBuildFiles()

	props := readGenerated(t, filepath.Join(root, "Directory.Build.props"))

	if !strings.Contains(props, "<go2csPath>$(MSBuildThisFileDirectory)</go2csPath>") {
		t.Errorf("coinciding roots should pin go2csPath relatively:\n%s", props)
	}

	if !strings.Contains(props, `<PropertyGroup Condition="'$(go2csPath)' == ''">`) {
		t.Errorf("go2csPath pin is not condition-guarded:\n%s", props)
	}
}

// TestRecurseNuGetReferences is the -recurse=nuget counterpart to TestRecurseSyntheticModule: it runs the
// same two-module fixture (app importing a co-located lib via replace, each importing the standard library)
// but with nugetRefs enabled, and confirms the reference rewrite. The go2cs standard library (fmt),
// runtime (golib) and analyzer (go2cs-gen) become NuGet PackageReferences (go.fmt / go.lib / go.gen), while
// the app's own converted dependency (lib) stays a LOCAL ProjectReference; an output-root
// Directory.Build.props defaults GoStdLibVersion; and the per-project solution drops golib/analyzer and
// the /core/ folder.
// Deterministic and network-free (asserts the emitted .csproj/.props/.slnx text; no dotnet restore).
func TestRecurseNuGetReferences(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/lib v0.0.0\n\nreplace example.com/lib => ../lib\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/lib\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		nugetRefs:           true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	// getImportPackageInfo resolves stdlib references through build.Default; pin it as main() does.
	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	appProjectDir := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	libProject := filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib", "example.com.lib.csproj")
	appCsproj := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.csproj"))

	// The go2cs standard library, runtime and analyzer are referenced from NuGet.
	for _, want := range []string{
		`<PackageReference Include="go.fmt" Version="$(GoStdLibVersion)" />`,
		`<PackageReference Include="go.lib" Version="$(GoStdLibVersion)" />`,
		`<PackageReference Include="go.gen" Version="$(GoStdLibVersion)" PrivateAssets="all" />`,
	} {
		if !strings.Contains(appCsproj, want) {
			t.Errorf("app csproj missing NuGet reference %q:\n%s", want, appCsproj)
		}
	}

	// No local $(go2csPath) references remain for the stdlib or the runtime.
	for _, notWant := range []string{
		`$(go2csPath)core/fmt/fmt.csproj`,
		`$(go2csPath)core/golib/golib.csproj`,
		`$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj`,
	} {
		if strings.Contains(appCsproj, notWant) {
			t.Errorf("app csproj still has local reference %q under -recurse=nuget:\n%s", notWant, appCsproj)
		}
	}

	// The app's OWN converted dependency stays a relative LOCAL ProjectReference.
	libReference, err := filepath.Rel(appProjectDir, libProject)
	libReference = filepath.ToSlash(libReference)

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(appCsproj, `<ProjectReference Include="`+libReference+`" />`) {
		t.Errorf("app csproj missing the relative local lib ProjectReference %q:\n%s", libReference, appCsproj)
	}

	// The Directory.Build.props is emitted at the isolated output root and defaults GoStdLibVersion.
	props := readGenerated(t, filepath.Join(options.recurseOutputRoot, "Directory.Build.props"))

	if strings.Contains(props, `<go2csPath>`) {
		t.Errorf("output-root Directory.Build.props should not couple go2csPath to converted output:\n%s", props)
	}

	if base := goVersion(); base != "" {
		if !strings.Contains(props, `<PropertyGroup Condition="'$(GoStdLibVersion)' == ''">`) {
			t.Errorf("Directory.Build.props missing the conditional GoStdLibVersion default:\n%s", props)
		}

		if want := "<GoStdLibVersion>" + base + ".*</GoStdLibVersion>"; !strings.Contains(props, want) {
			t.Errorf("Directory.Build.props missing floating version default %q:\n%s", want, props)
		}
	}

	// The per-project solution drops golib/analyzer (now NuGet) and the /core/ folder, but still lists the
	// app (src) and its converted dependency lib (pkg).
	appSlnx := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "example.com.app.slnx"))

	for _, notWant := range []string{"golib.csproj", "go2cs-gen.csproj", `<Folder Name="/core/">`} {
		if strings.Contains(appSlnx, notWant) {
			t.Errorf("app per-project solution should not list %q under -recurse=nuget:\n%s", notWant, appSlnx)
		}
	}

	for _, want := range []string{"example.com.app.csproj", "example.com.lib.csproj", `<Folder Name="/src/">`, `<Folder Name="/pkg/">`} {
		if !strings.Contains(appSlnx, want) {
			t.Errorf("app per-project solution missing %q:\n%s", want, appSlnx)
		}
	}
}

// TestRecurseModuleOnly is the -recurse=module counterpart to TestRecurseSyntheticModule: the same
// two-module fixture (app importing a co-located lib via replace, each importing the standard
// library), converted with the scope narrowed to the input module's own packages. The app converts;
// the dependency does NOT — no pkg\ output is written for it and it never enters the convert-set —
// while the app's reference to it is still emitted into pkg\ (from the import path alone), so
// converting it later into the same output root resolves that reference. This is the mode that lets
// a module whose third-party closure cannot be converted still convert its own code.
// Deterministic and network-free.
func TestRecurseModuleOnly(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/lib v0.0.0\n\nreplace example.com/lib => ../lib\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/lib\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		moduleOnly:          true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	// getImportPackageInfo resolves stdlib references through build.Default; pin it as main() does.
	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	// The convert-set is the module's own package alone; the dependency is recorded as referenced.
	if queue := converter.graph.sortedQueue; len(queue) != 1 || queue[0] != "example.com/app" {
		t.Fatalf("convert order = %v, want [example.com/app]", queue)
	}

	if refs := converter.referencedThirdParty; len(refs) != 1 || refs[0] != "example.com/lib" {
		t.Fatalf("referenced-only third-party = %v, want [example.com/lib]", refs)
	}

	// The app converts into src\<import-path> exactly as it does under the full recurse.
	mainCs := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "main.cs"))

	if !strings.Contains(mainCs, "lib.Greeting") {
		t.Errorf("app main.cs missing the cross-package call lib.Greeting:\n%s", mainCs)
	}

	// The dependency is NOT converted — nothing at all is written under pkg\.
	if _, err := os.Stat(filepath.Join(options.recurseOutputRoot, "pkg")); !os.IsNotExist(err) {
		t.Errorf("dependency tree pkg\\ was written under -recurse=module (want nothing converted there)")
	}

	// The app csproj still carries the relative reference to where that dependency WOULD be
	// converted, so converting it into the same output root later resolves the reference.
	appProjectDir := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	libProject := filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib", "example.com.lib.csproj")
	appCsproj := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.csproj"))
	libReference, err := filepath.Rel(appProjectDir, libProject)
	libReference = filepath.ToSlash(libReference)

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(appCsproj, `<ProjectReference Include="`+libReference+`" />`) {
		t.Errorf("app csproj missing the relative lib ProjectReference %q:\n%s", libReference, appCsproj)
	}

	// The standard library is referenced exactly as before — the scope narrows the CONVERT-set only.
	if !strings.Contains(appCsproj, "$(go2csPath)core/fmt/fmt.csproj") {
		t.Errorf("app csproj missing the stdlib fmt reference:\n%s", appCsproj)
	}

	// The per-project solution lists the app + runtime + analyzer; the unconverted dependency has no
	// project to list, so the /pkg/ folder is empty and skipped.
	appSlnx := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.slnx"))

	for _, want := range []string{"example.com.app.csproj", "golib.csproj", "go2cs-gen.csproj", `<Folder Name="/src/">`} {
		if !strings.Contains(appSlnx, want) {
			t.Errorf("app per-project solution missing %q:\n%s", want, appSlnx)
		}
	}

	for _, notWant := range []string{"example.com.lib.csproj", `<Folder Name="/pkg/">`} {
		if strings.Contains(appSlnx, notWant) {
			t.Errorf("app per-project solution should not list %q under -recurse=module:\n%s", notWant, appSlnx)
		}
	}
}

// TestModuleCachePoisonedGoWorkLoad guards the issue-#32 module-cache load fix: a dependency
// module's zip can ship its repository's go.work file (the cloud.google.com/go monorepo lists
// ~200 sibling modules that are never in the cache), and processConversion's per-package load —
// running the go command with the module-cache directory as its working directory — would enter
// workspace mode and fail with "cannot load module ../<sibling> listed in go.work file". The fix
// disables workspace mode (GOWORK=off) for loads whose input dir is under the module cache, and
// ONLY there, so an end-user module resolving its own packages through a real workspace keeps the
// ambient behavior. Both sides are asserted here against the same poisoned fixture: outside the
// (test-pinned) cache root the go.work still aborts the load, under it the package converts.
// Deterministic and network-free.
func TestModuleCachePoisonedGoWorkLoad(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	root := t.TempDir()

	// A fake module-cache entry: a self-contained module whose go.work names a sibling module
	// that does not exist beside it — exactly the vestigial state an unpacked module zip leaves.
	depDir := filepath.Join(root, "modcache", "example.com", "dep@v1.0.0")

	writeModuleFile(t, filepath.Join(depDir, "go.mod"), "module example.com/dep\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(depDir, "go.work"), "go 1.23\n\nuse (\n\t.\n\t../missing\n)\n")
	writeModuleFile(t, filepath.Join(depDir, "value.go"),
		"package dep\n\nfunc Value() string {\n\treturn \"dep\"\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
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

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	outDir := filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "dep")

	// Control: with the fixture NOT under the module cache, the ambient workspace behavior stands,
	// the go command finds the poisoned go.work, and the load fails. If this side ever passes, the
	// go toolchain's own behavior changed and the GOWORK gate below is no longer proving anything.
	savedModCache := goModCache
	goModCache = filepath.Join(root, "elsewhere")

	defer func() { goModCache = savedModCache }()

	if err := processConversion(depDir, true, outDir, options); err == nil {
		t.Fatalf("control conversion unexpectedly succeeded with ambient GOWORK against a poisoned go.work")
	} else if !strings.Contains(err.Error(), "go.work") {
		t.Fatalf("control conversion failed, but not through the poisoned go.work: %v", err)
	}

	// The fix: with the fixture under the module-cache root, the load runs with GOWORK=off, the
	// vestigial go.work is ignored, and the package converts.
	goModCache = filepath.Join(root, "modcache")

	if err := processConversion(depDir, true, outDir, options); err != nil {
		t.Fatalf("conversion under the module cache still failed — GOWORK=off not applied: %v", err)
	}

	valueCs := readGenerated(t, filepath.Join(outDir, "value.cs"))

	if !strings.Contains(valueCs, "Value") {
		t.Errorf("converted value.cs missing func Value:\n%s", valueCs)
	}
}

// TestModuleCacheVestigialReplaceLoad guards the load SHAPE for a module-cache package (issue #33
// follow-up). A published module zip routinely carries directives that were written for the source
// repo and are vestigial once unpacked — here a monorepo's `replace example.com/sub => ./sub`, whose
// target is a separate module and so is excluded from the zip. Loading the package from ITS OWN
// directory makes the go command treat that go.mod as the MAIN module, where a `replace` IS
// authoritative, and the dependency dies: `replacement directory ./sub does not exist`, an
// empty-named types.Package, and `could not import example.com/sub (invalid package name: "")` at
// every use site. Loading the same package from the APP's directory by import path ignores the
// dependency's replaces, exactly as the app's own build does.
//
// Both sides are asserted from one fixture, so the control proves the fixture still reproduces the
// failure rather than the guard passing vacuously. Network-free: the "module cache" is a temp
// directory that goModCache is pinned to, and the app reaches both modules through its own replaces.
func TestModuleCacheVestigialReplaceLoad(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture packages via go/packages")
	}

	root := t.TempDir()
	modCache := filepath.Join(root, "modcache")
	depDir := filepath.Join(modCache, "example.com", "dep@v1.0.0")
	subDir := filepath.Join(modCache, "example.com", "sub@v1.0.0")
	appDir := filepath.Join(root, "app")

	// The dependency, as unpacked from its zip: it requires a sibling module and replaces that module
	// with a RELATIVE directory the zip does not contain.
	writeModuleFile(t, filepath.Join(depDir, "go.mod"),
		"module example.com/dep\n\ngo 1.23\n\nrequire example.com/sub v1.0.0\n\nreplace example.com/sub => ./sub\n")
	writeModuleFile(t, filepath.Join(depDir, "value.go"),
		"package dep\n\nimport \"example.com/sub\"\n\nfunc Value() string {\n\treturn sub.Name()\n}\n")

	// The sibling module, present in the cache at its own path — which is how the app resolves it.
	writeModuleFile(t, filepath.Join(subDir, "go.mod"), "module example.com/sub\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(subDir, "name.go"),
		"package sub\n\nfunc Name() string {\n\treturn \"sub\"\n}\n")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire (\n\texample.com/dep v1.0.0\n\texample.com/sub v1.0.0\n)\n\n"+
			"replace example.com/dep => "+filepath.ToSlash(depDir)+"\n\nreplace example.com/sub => "+filepath.ToSlash(subDir)+"\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport \"example.com/dep\"\n\nfunc main() {\n\tprintln(dep.Value())\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
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

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	// Point the module-cache classification at the fixture, as the go.work guard does.
	savedModCache := goModCache
	goModCache = modCache

	defer func() { goModCache = savedModCache }()

	// The loader reports type errors through the standard logger, and processConversion converts
	// best-effort either way — so the log is where the two shapes actually differ.
	convert := func(opts Options, outDir string) string {
		var captured strings.Builder

		savedOutput := log.Writer()
		log.SetOutput(&captured)

		defer log.SetOutput(savedOutput)

		if err := processConversion(depDir, true, outDir, opts); err != nil {
			t.Fatalf("conversion failed: %v", err)
		}

		return captured.String()
	}

	// Control — the old shape: no main module to borrow a context from, so the load runs from inside
	// the cache and the vestigial replace fires. If this side ever goes quiet, the fixture has stopped
	// reproducing and the guard below proves nothing.
	controlLog := convert(options, filepath.Join(root, "out-control"))

	if !strings.Contains(controlLog, "invalid package name") {
		t.Fatalf("control did not reproduce the vestigial-replace failure — the guard below is vacuous:\n%s", controlLog)
	}

	// The fix: name the main module and the package's import path, and the same package loads from
	// the app's context, where the dependency's replaces are correctly ignored.
	fixed := options
	fixed.mainModuleDir = appDir
	fixed.packageImportPath = "example.com/dep"

	fixedLog := convert(fixed, filepath.Join(root, "out-fixed"))

	if strings.Contains(fixedLog, "invalid package name") {
		t.Errorf("main-module load still hit the dependency's vestigial replace:\n%s", fixedLog)
	}

	if strings.Contains(fixedLog, "resolved away from") {
		t.Errorf("import path did not resolve to the convert-set directory:\n%s", fixedLog)
	}

	// The conversion is real on the fixed side, not merely quiet.
	if valueCs := readGenerated(t, filepath.Join(root, "out-fixed", "value.cs")); !strings.Contains(valueCs, "sub.Name()") {
		t.Errorf("converted value.cs lost the cross-module call:\n%s", valueCs)
	}
}

// TestRecurseQuotedModulePath guards issue #33's csproj-filename failure end to end. A go.mod module
// path is a token and may be QUOTED — gopkg.in/yaml.v3 declares itself `module "gopkg.in/yaml.v3"`,
// and the gopkg.in family generally does — so reading the directive's raw remainder carried the
// quotes into the project name and from there into the file the project writer opens:
//
//	Error while writing project file "…\pkg\gopkg.in\yaml.v3\"gopkg.in.yaml.v3".csproj":
//	The filename, directory name, or volume label syntax is incorrect.
//
// github.com/… paths never showed it because their go.mod writes the path bare; the quoting is the
// whole discriminator. The fixture reproduces the shape exactly — a versioned, dotted-host dependency
// whose go.mod quotes both its module path and its requires — resolved through a local replace so the
// test stays deterministic and network-free.
func TestRecurseQuotedModulePath(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib.v3")

	// Byte-for-byte the gopkg.in shape: quoted module path, quoted require paths.
	writeModuleFile(t, filepath.Join(libDir, "go.mod"),
		"module \"gopkg.in/lib.v3\"\n\nrequire (\n\t\"gopkg.in/check.v1\" v0.0.0-20161208181325-20d25e280405\n)\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire gopkg.in/lib.v3 v3.0.0\n\nreplace gopkg.in/lib.v3 => ../lib.v3\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\tlib \"gopkg.in/lib.v3\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
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

	// getImportPackageInfo resolves stdlib references through build.Default; pin it as main() does.
	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	if queue := converter.graph.sortedQueue; len(queue) != 2 || queue[0] != "gopkg.in/lib.v3" || queue[1] != "example.com/app" {
		t.Fatalf("convert order = %v, want [gopkg.in/lib.v3 example.com/app]", queue)
	}

	// The dependency's project file exists under the unquoted, dotted import path. Before the fix the
	// write failed outright (logged as a per-package WARNING, so the run itself still returned nil) and
	// nothing was here at all.
	libProjectDir := filepath.Join(options.recurseOutputRoot, "pkg", "gopkg.in", "lib.v3")
	libProject := filepath.Join(libProjectDir, "gopkg.in.lib.v3.csproj")

	if _, err := os.Stat(libProject); err != nil {
		t.Fatalf("dependency project file was not written at %s: %v", libProject, err)
	}

	// …and nothing quoted was written beside it under some other name.
	entries, err := os.ReadDir(libProjectDir)

	if err != nil {
		t.Fatal(err)
	}

	for _, entry := range entries {
		if strings.ContainsAny(entry.Name(), `"'`) {
			t.Errorf("generated file name %q carries go.mod quoting", entry.Name())
		}
	}

	// The converted dependency is real, and the app references it by that unquoted name.
	if greetingCs := readGenerated(t, filepath.Join(libProjectDir, "greeting.cs")); !strings.Contains(greetingCs, "strings.TrimSpace") {
		t.Errorf("lib greeting.cs missing strings.TrimSpace:\n%s", greetingCs)
	}

	appProjectDir := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	appCsproj := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.csproj"))
	libReference, err := filepath.Rel(appProjectDir, libProject)
	libReference = filepath.ToSlash(libReference)

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(appCsproj, `<ProjectReference Include="`+libReference+`" />`) {
		t.Errorf("app csproj missing the relative lib ProjectReference %q:\n%s", libReference, appCsproj)
	}
}

// TestRecurseKeywordNamespaceSegment guards issue #33's wall AFTER the csproj-filename one above: a
// dependency whose import path embeds a C# keyword in a dotted path ELEMENT (the `in` of gopkg.in)
// converted fine and then would not compile, because the two sides of the same namespace disagreed.
// The dependency's own declaration escaped it — `namespace go.gopkg.@in;`, composed through
// getCoreSanitizedIdentifier, which splits an element on its dots — while every importer rendered
// through getSanitizedImport, which measured the element whole, so `gopkg.in` was not a keyword and
// emitted bare: `using yaml = gopkg.in.yaml_package;` and `using gopkg.in;`, neither of which parses
// (CS1001/CS1002/CS1022 at the `in`).
//
// Same fixture shape as TestRecurseQuotedModulePath — a versioned, dotted-host dependency resolved
// through a local replace, so this stays deterministic and network-free — but the import is
// UNALIASED, which is what makes the consumer emit BOTH forms (the alias and the enclosing-namespace
// `using`); an aliased import emits only the first. The final assertion is deliberately stated as a
// property over the converter's own `keywords` set rather than a list of literals, so a keyword this
// test never names is covered too.
func TestRecurseKeywordNamespaceSegment(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib.v3")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module gopkg.in/lib.v3\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "greeting.go"),
		"package lib\n\nimport \"strings\"\n\nfunc Greeting(name string) string {\n\treturn strings.TrimSpace(\"Hello, \"+name+\"!\")\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire gopkg.in/lib.v3 v3.0.0\n\nreplace gopkg.in/lib.v3 => ../lib.v3\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"gopkg.in/lib.v3\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Greeting(\"go2cs\"))\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
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

	// getImportPackageInfo resolves stdlib references through build.Default; pin it as main() does.
	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	// The PRODUCER side — unchanged by the fix, and the reference the consumer has to match.
	greetingCs := readGenerated(t, filepath.Join(options.recurseOutputRoot, "pkg", "gopkg.in", "lib.v3", "greeting.cs"))
	wantNamespace := RootNamespace + ".gopkg.@in"

	if !strings.Contains(greetingCs, "namespace "+wantNamespace+";") {
		t.Fatalf("dependency did not declare %s:\n%s", wantNamespace, greetingCs)
	}

	// The CONSUMER side — both emissions an unaliased import produces, naming that same namespace.
	mainCs := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", "main.cs"))

	for _, want := range []string{"using lib = gopkg.@in.lib" + PackageSuffix + ";", "using gopkg.@in;"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("app main.cs missing %q:\n%s", want, mainCs)
		}
	}

	// …and the property behind those two literals, over every using the file emits: no level of a
	// rendered namespace may be a bare C# keyword. Read from the converter's own keyword set, so a
	// keyword this fixture never exercises is guarded by the same assertion.
	for _, line := range strings.Split(mainCs, "\n") {
		line = strings.TrimSpace(strings.TrimSuffix(strings.TrimSpace(line), ";"))

		if !strings.HasPrefix(line, "using ") {
			continue
		}

		target := strings.TrimPrefix(strings.TrimPrefix(line, "using "), "static ")

		if equals := strings.Index(target, " = "); equals != -1 {
			target = target[equals+len(" = "):]
		}

		for _, level := range strings.Split(target, ".") {
			if keywords.Contains(level) {
				t.Errorf("using target %q names the C# keyword %q unescaped (line %q)", target, level, line)
			}
		}
	}
}

// TestRecurseGoFileFreeContainerDirsKeepDistinctProjectNames is issue #35 end to end: a generated .slnx
// carried two projects named `apiv1.csproj`, and Visual Studio refuses to open a solution holding two
// projects of the same name — silently from a file dialog, with an undiagnosed failure from the recent
// list. The reporter's conversion had 175 such projects across 49 colliding names.
//
// The cause was upstream of the solution writer: getProjectName walked up from the package directory
// looking for the module root and treated the first ancestor holding no .go files as the boundary, so
// cloud.google.com/go/bigquery's datatransfer/apiv1 and storage/apiv1 both collapsed to the leaf segment
// `apiv1`. Go modules are made of such container directories, which is why the collision was routine
// rather than exotic. Nothing in the solution writer could recover from it — deduplicating there would
// only have renamed one of two projects whose namespaces had ALSO collapsed onto each other.
//
// The fixture is the reporter's shape, deterministic and network-free: a dependency module with two
// go-file-free container directories over same-named leaf packages, plus the same shape inside the app's
// OWN tree (internal/web/api), imported so both sides of every emitted name are exercised.
func TestRecurseGoFileFreeContainerDirsKeepDistinctProjectNames(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "cloud")

	// The dependency: datatransfer/ and storage/ hold no .go files of their own, only an apiv1 package.
	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/cloud\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "datatransfer", "apiv1", "client.go"),
		"package apiv1\n\nfunc Name() string {\n\treturn \"datatransfer\"\n}\n")
	writeModuleFile(t, filepath.Join(libDir, "storage", "apiv1", "client.go"),
		"package apiv1\n\nfunc Name() string {\n\treturn \"storage\"\n}\n")

	// The app: internal/web/ holds no .go files either, so its own package collapsed the same way.
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/cloud v1.0.0\n\nreplace example.com/cloud => ../cloud\n")
	writeModuleFile(t, filepath.Join(appDir, "internal", "web", "api", "api.go"),
		"package api\n\nfunc Route() string {\n\treturn \"/api\"\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\tdt \"example.com/cloud/datatransfer/apiv1\"\n"+
			"\tst \"example.com/cloud/storage/apiv1\"\n\t\"example.com/app/internal/web/api\"\n)\n\n"+
			"func main() {\n\tfmt.Println(dt.Name(), st.Name(), api.Route())\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
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

	// getImportPackageInfo resolves stdlib references through build.Default; pin it as main() does.
	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	// Each package's project is named for its full import path. Before the fix the two apiv1 projects
	// were both `apiv1.csproj` and the app's own package was `api.csproj`.
	wantProjects := map[string]string{
		filepath.Join("pkg", "example.com", "cloud", "datatransfer", "apiv1"): "example.com.cloud.datatransfer.apiv1.csproj",
		filepath.Join("pkg", "example.com", "cloud", "storage", "apiv1"):      "example.com.cloud.storage.apiv1.csproj",
		filepath.Join("src", "example.com", "app", "internal", "web", "api"):  "example.com.app.internal.web.api.csproj",
	}

	for dir, name := range wantProjects {
		if _, err := os.Stat(filepath.Join(options.recurseOutputRoot, dir, name)); err != nil {
			t.Errorf("project file not written at %s: %v", filepath.Join(dir, name), err)
		}
	}

	appProjectDir := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	appSlnx := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.slnx"))

	// The invariant the issue is actually about: no two projects in the solution share a name.
	seen := make(map[string]string)

	for _, match := range regexp.MustCompile(`<Project Path="([^"]+)"`).FindAllStringSubmatch(appSlnx, -1) {
		project := match[1]
		name := path.Base(project)

		if prior, dup := seen[name]; dup {
			t.Errorf("solution lists two projects named %q (%q and %q) — Visual Studio will not open it:\n%s",
				name, prior, project, appSlnx)
		}

		seen[name] = project
	}

	for _, name := range wantProjects {
		if _, ok := seen[name]; !ok {
			t.Errorf("app per-project solution missing %q:\n%s", name, appSlnx)
		}
	}

	// The reference side agrees with the declaration side: the app's csproj names both dependencies by
	// their qualified project names, and its converted code qualifies the two same-named Go packages
	// onto DISTINCT C# classes. When the names collapsed, both rendered as go.apiv1_package.
	appCsproj := readGenerated(t, filepath.Join(appProjectDir, "example.com.app.csproj"))

	for _, name := range wantProjects {
		if !strings.Contains(appCsproj, "/"+name+`" />`) {
			t.Errorf("app csproj missing a ProjectReference to %q:\n%s", name, appCsproj)
		}
	}

	mainCs := readGenerated(t, filepath.Join(appProjectDir, "main.cs"))

	for _, want := range []string{
		"go.example.com.cloud.datatransfer.apiv1_package",
		"go.example.com.cloud.storage.apiv1_package",
		// `internal` is a C# keyword, escaped by getCoreSanitizedIdentifier on BOTH the declaration
		// and reference sides (issue #33's second wall). Worth asserting in the escaped form rather
		// than working around: recovering the full import path is what puts Go's most common
		// directory name into a namespace at all, so this fix is what exposes that path broadly.
		"go.example.com.app.@internal.web.api_package",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("main.cs does not qualify against %q:\n%s", want, mainCs)
		}
	}

	// Issue #35's SECOND wall, at the same altitude. Recovering the full import path made every name
	// correct and, on a deep tree, long: the path is spelled once by the directory and again by the
	// file name, so the reporter's deepest package emitted a 242-character project path and Visual
	// Studio then refused to LOAD it — MAX_PATH, which its project loader enforces even where
	// LongPathsEnabled is on. Nothing emitted anywhere in the output tree may exceed the budget.
	assertEmittedPathsInsideBudget(t, options.recurseOutputRoot)

	// And the artifact redirection that keeps the DERIVED paths bounded too — obj, bin and the
	// generator output all multiply the import path rather than merely repeating it, and on the
	// reporter's tree 1,570 of 1,725 projects produced a generator path over MAX_PATH.
	props := readGenerated(t, filepath.Join(options.recurseOutputRoot, "Directory.Build.props"))

	for _, want := range []string{"<BaseIntermediateOutputPath>", "<BaseOutputPath>", "Go2csArtifactToken"} {
		if !strings.Contains(props, want) {
			t.Errorf("output-root Directory.Build.props does not redirect %s:\n%s", want, props)
		}
	}

	targets := readGenerated(t, filepath.Join(options.recurseOutputRoot, "Directory.Build.targets"))

	// The .targets, not the .props: the generated csproj sets CompilerGeneratedFilesOutputPath in the
	// project body, which beats any .props value. And with NO trailing separator — csc receives it as
	// /generatedfilesout:"<path>", where a trailing backslash escapes the closing quote and the
	// compiler is handed the directory as a file name (CS2021).
	if !strings.Contains(targets, "<CompilerGeneratedFilesOutputPath>$(Go2csArtifactsRoot)gen\\$(Go2csArtifactToken)</CompilerGeneratedFilesOutputPath>") {
		t.Errorf("output-root Directory.Build.targets does not redirect generator output correctly:\n%s", targets)
	}
}

// assertEmittedPathsInsideBudget fails if anything the conversion wrote under root exceeds
// maxRelativeProjectPath, which is the whole point of the budget: a bound that holds for the TREE, not
// merely for the one file whose name the derivation controls.
func assertEmittedPathsInsideBudget(t *testing.T, root string) {
	t.Helper()

	var longest string

	if err := filepath.Walk(root, func(p string, info os.FileInfo, err error) error {
		if err != nil || info.IsDir() {
			return err
		}

		rel, relErr := filepath.Rel(root, p)

		if relErr != nil {
			return relErr
		}

		if len(rel) > len(longest) {
			longest = rel
		}

		return nil
	}); err != nil {
		t.Fatalf("walking the output root: %v", err)
	}

	if len(longest) > maxRelativeProjectPath {
		t.Errorf("emitted path %q is %d characters, over the %d budget", longest, len(longest), maxRelativeProjectPath)
	}
}

// TestRecurseDeepImportPathStaysInsideMaxPath is the issue-#35 second-wall regression at full depth,
// on the reporter's actual shape: a 115-character import path (envoyproxy/go-control-plane's
// client_side_weighted_round_robin/v3) whose package implements an interface from a SECOND deep
// module, which is what drives the source generators to compose hint names carrying a namespace
// twice. Before the budget this emitted a 242-character project path Visual Studio would not load,
// and a 314-character generated FILE NAME that no Windows can write at any output root — that build
// died with CS0016 from a nine-character root, with every long-path setting turned on.
func TestRecurseDeepImportPathStaysInsideMaxPath(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	pbDir := filepath.Join(root, "pb")
	gcpDir := filepath.Join(root, "gcp")

	deep := filepath.Join("envoy", "extensions", "load_balancing_policies", "client_side_weighted_round_robin", "v3")

	writeModuleFile(t, filepath.Join(pbDir, "go.mod"), "module google.golang.org/protobuf\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(pbDir, "reflect", "protoreflect", "protoreflect.go"),
		"package protoreflect\n\ntype Message interface {\n\tDescriptor() string\n}\n\n"+
			"type ProtoMessage interface {\n\tProtoReflect() Message\n}\n")

	writeModuleFile(t, filepath.Join(gcpDir, "go.mod"),
		"module github.com/envoyproxy/go-control-plane\n\ngo 1.23\n\n"+
			"require google.golang.org/protobuf v1.0.0\n\nreplace google.golang.org/protobuf => ../pb\n")
	writeModuleFile(t, filepath.Join(gcpDir, deep, "config.go"),
		"package v3\n\nimport pr \"google.golang.org/protobuf/reflect/protoreflect\"\n\n"+
			"type ClientSideWeightedRoundRobinConfig struct {\n\tName string\n}\n\n"+
			"func (x *ClientSideWeightedRoundRobinConfig) ProtoReflect() pr.Message {\n\treturn nil\n}\n\n"+
			"var _ pr.ProtoMessage = (*ClientSideWeightedRoundRobinConfig)(nil)\n")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module renart\n\ngo 1.23\n\nrequire github.com/envoyproxy/go-control-plane v1.0.0\n\n"+
			// The transitive dependency needs its own require in the MAIN module's go.mod: module
			// graph pruning (go 1.17+) will not otherwise resolve a replaced indirect dependency.
			"require google.golang.org/protobuf v1.0.0 // indirect\n\n"+
			"replace github.com/envoyproxy/go-control-plane => ../gcp\n\n"+
			"replace google.golang.org/protobuf => ../pb\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\tv3 \"github.com/envoyproxy/go-control-plane/"+
			"envoy/extensions/load_balancing_policies/client_side_weighted_round_robin/v3\"\n)\n\n"+
			"func main() {\n\tc := &v3.ClientSideWeightedRoundRobinConfig{Name: \"wrr\"}\n\tfmt.Println(c.Name)\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
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

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	const canonical = "github.com.envoyproxy.go-control-plane.envoy.extensions.load_balancing_policies." +
		"client_side_weighted_round_robin.v3"

	deepDir := filepath.Join(options.recurseOutputRoot, "pkg", "github.com", "envoyproxy",
		"go-control-plane", deep)

	// The name is COMPRESSED, and the file is written where the reference side will look for it.
	emitted := projectFileBaseName(canonical)

	if emitted == canonical {
		t.Fatalf("expected the deep project name to be compressed, got the full %q", emitted)
	}

	if _, err := os.Stat(filepath.Join(deepDir, emitted+".csproj")); err != nil {
		t.Errorf("deep project file not written at %s: %v", filepath.Join(deepDir, emitted+".csproj"), err)
	}

	// IDENTITY is untouched by the file-name compression: the assembly — hence the NuGet package id,
	// which the template derives from it — still spells the full import path.
	deepCsproj := readGenerated(t, filepath.Join(deepDir, emitted+".csproj"))

	if !strings.Contains(deepCsproj, "<AssemblyName>"+canonical+"</AssemblyName>") {
		t.Errorf("deep project's AssemblyName is not the full canonical path:\n%s", deepCsproj)
	}

	assertEmittedPathsInsideBudget(t, options.recurseOutputRoot)

	// The app references the dependency by its compressed name. Declaration and reference sides derive
	// it independently, from the import path alone — they must agree or the reference dangles.
	appCsproj := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "renart", "renart.csproj"))

	if !strings.Contains(appCsproj, emitted+".csproj") {
		t.Errorf("app csproj does not reference the deep dependency as %q:\n%s", emitted+".csproj", appCsproj)
	}
}

func writeModuleFile(t *testing.T, path, content string) {
	t.Helper()

	if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
		t.Fatalf("mkdir %s: %v", filepath.Dir(path), err)
	}

	if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
		t.Fatalf("write %s: %v", path, err)
	}
}

// TestRecurseChannelOfHyphenatedModulePath is issue #33's THIRD report at the recurse altitude: a
// `-recurse` over a project depending on go.mongodb.org/mongo-driver died with `fatal error: stack
// overflow` while converting x/mongo/driver/session, on nothing more exotic than
//
//	type Pool struct { descChan <-chan description.Topology }
//
// convertToCSFullTypeName rewrites an import path to a C# namespace BEFORE the branches that peel a
// `<-chan ` off the front, and it measured the path from index 0 — so the sanitizer's legal
// hyphen-to-underscore mapping (`mongo-driver`) also rewrote the `-` of `<-chan`. `<_chan …` matches
// no channel branch, fell into the array branch, found no `]`-turned-`>` to slice past, and re-entered
// on the identical string forever. See typeNameResolution.go's importPathStart.
//
// The unit half (typeNameResolution_test.go) pins the renderer directly; this pins that a real Go
// declaration of that shape reaches it and converts. The fixture mirrors the reported path exactly —
// a HYPHENATED first segment and a MULTI-segment tail, which is the whole trigger: a single-segment
// path has no slash to enter the rewrite, which is why the standard library's own `<-chan time.Time`
// fields were always fine. Network-free, via a local replace, like its neighbors.
func TestRecurseChannelOfHyphenatedModulePath(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "mongo-driver")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/mongo-driver\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "mongo", "description", "description.go"),
		"package description\n\ntype Topology struct {\n\tKind int\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/mongo-driver v1.0.0\n\nreplace example.com/mongo-driver => ../mongo-driver\n")

	// The struct FIELD is the reported shape — it is what reaches the fully-qualified renderer
	// through visitStructType, and where the run died. The type ALIAS beside it is what makes the
	// result assertable: a field DECLARATION emits the readable file-local alias
	// (`description.Topology`), so the fully-qualified render is computed but never written, and a
	// test that only reads the field cannot tell a correct render from a mangled one. An exported
	// type alias emits the fully-qualified string verbatim, into both `main.cs` and the
	// `[GoTypeAlias]` record — the same reason the NestedAliasUser golden is a good sentinel.
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/mongo-driver/mongo/description\"\n)\n\n"+
			"type TopoChan = <-chan description.Topology\n\n"+
			"type Pool struct {\n\tdescChan <-chan description.Topology\n}\n\n"+
			"func NewPool(descChan <-chan description.Topology) *Pool {\n\treturn &Pool{descChan: descChan}\n}\n\n"+
			"func main() {\n\tch := make(chan description.Topology, 1)\n\tch <- description.Topology{Kind: 7}\n\tfmt.Println((<-NewPool(ch).descChan).Kind)\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
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

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	// Before the fix this call did not return — it exhausted the goroutine stack, and a Go stack
	// overflow is FATAL, so there was nothing for the driver's per-file recover to catch.
	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	appDirOut := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	mainCs := readGenerated(t, filepath.Join(appDirOut, "main.cs"))
	packageInfoCs := readGenerated(t, filepath.Join(appDirOut, "package_info.cs"))

	// The fully-qualified render, where the crash lived — the channel constructor intact and the
	// hyphenated multi-segment path resolved to its namespace.
	//
	// The element carries the ROOT namespace as well, and that is load-bearing rather than
	// decorative: a using alias resolves at COMPILATION scope, where `example.com…` is not a
	// namespace of anything (CS0246). This assertion read the unrooted form until the alias
	// renderer was given its rooted-nesting mode, i.e. it was pinning the defect — a `-recurse`
	// module conversion is exactly the audience for it, since an end-user alias over a
	// third-party type is ordinary code (guard: PackageAliasRootedTypeArgs).
	wantAlias := "global using TopoChan = " + RootNamespace + "./*<-*/channel<" + RootNamespace +
		".example.com.mongo_driver.mongo.description" + PackageSuffix + ".Topology>;"

	if !strings.Contains(mainCs, wantAlias) {
		t.Errorf("app main.cs missing %q:\n%s", wantAlias, mainCs)
	}

	if !strings.Contains(packageInfoCs, `[assembly: GoTypeAlias("TopoChan", "`+RootNamespace+"./*<-*/channel<") {
		t.Errorf("package_info.cs did not record the channel alias with its constructor:\n%s", packageInfoCs)
	}

	// The mangled arrow, stated directly: `-` sanitized to `_` inside the constructor. Naming the
	// failure mode keeps the diagnosis in the test rather than in whoever reads the diff.
	for name, generated := range map[string]string{"main.cs": mainCs, "package_info.cs": packageInfoCs} {
		if strings.Contains(generated, "<_chan") {
			t.Errorf("%s: the `<-chan` constructor was sanitized as import-path text:\n%s", name, generated)
		}
	}

	// The readable form at the declaration site, which is what a reader of the converted code sees.
	if want := "/*<-*/channel<description.Topology> descChan"; !strings.Contains(mainCs, want) {
		t.Errorf("app main.cs missing %q:\n%s", want, mainCs)
	}
}

func readGenerated(t *testing.T, path string) string {
	t.Helper()

	data, err := os.ReadFile(path)

	if err != nil {
		t.Fatalf("read generated %s: %v", path, err)
	}

	return string(data)
}
