// interceptorBuildGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// Outside GOROOT the converter passes Go's `fmt.Println("hello")` as a bare C# literal, and go2cs-gen's
// LiteralInterceptorGenerator intercepts the call so the callee receives a Go string. If the interceptors are not
// admitted, the build fails loudly (CS9137); if go2cs-gen does not load, the build would go on WITHOUT them and the
// program would see System.String where Go has a string. So every route a converted project is built by must admit
// the interceptors' namespace and raise the generator-load warnings to errors:
//
//   - src/Directory.Build.props, for every project in this tree (TestGeneratorLoadFailuresAreBuildErrors pins its
//     codes; this pins the namespace);
//   - golib's buildTransitive/go.lib.targets, for a NuGet consumer of the go.* packages;
//   - the two csproj templates, for a converted project outside the tree that references golib and go2cs-gen as
//     projects (a project reference never imports buildTransitive).
//
// go.lib.targets also refuses, as GO2CS3001, a compiler below the Roslyn version go2cs-gen is built against.
// TestTheCompilerFloorRefusesAnOlderCompiler reads that refusal by building against the working-tree targets with
// $(CompilerApiVersion) forced: a PROPERTY-LEVEL red, since this box's SDK runs only its own Roslyn 5.0 (a pinned
// Microsoft.Net.Compilers.Toolset 4.12.0 restored but did not replace it, measured 2026-10-19).
const interceptorsNamespace = "go2cs.Interceptors"

func TestEveryBuildRouteAdmitsTheInterceptorsAndFailsOnAnUnloadedGenerator(t *testing.T) {
	root := repoRootFromPackageDir(t)

	routes := map[string]bool{
		"src/Directory.Build.props":                     false,
		"src/core/golib/buildTransitive/go.lib.targets": true,
		"src/go2cs/csproj-template.xml":                 true,
		"src/go2cs/test-csproj-template.xml":            true,
	}

	for rel, checkCodes := range routes {
		text, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		content := string(text)

		if !strings.Contains(content, "<InterceptorsNamespaces>$(InterceptorsNamespaces);"+interceptorsNamespace+"</InterceptorsNamespaces>") {
			t.Errorf("%s does not append %s to $(InterceptorsNamespaces): every intercepted call it builds would be CS9137", rel, interceptorsNamespace)
		}

		if !checkCodes {
			continue
		}

		matches := warningsAsErrorsElement.FindAllStringSubmatch(content, -1)

		if len(matches) != 1 {
			t.Errorf("%s sets <WarningsAsErrors> %d time(s); want exactly 1 carrying %s", rel, len(matches), strings.Join(generatorLoadCodes, ";"))
			continue
		}

		for _, code := range generatorLoadCodes {
			if !strings.Contains(matches[0][1], code) {
				t.Errorf("%s does not raise %s to an error: a go2cs-gen that fails that way would build on without its interceptors", rel, code)
			}
		}
	}
}

func TestTheCompilerFloorRefusesAnOlderCompiler(t *testing.T) {
	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Skip("dotnet is not on PATH; the compiler-floor arm needs MSBuild")
	}

	root := repoRootFromPackageDir(t)
	targets := filepath.Join(root, "src", "core", "golib", "buildTransitive", "go.lib.targets")
	dir := t.TempDir()
	project := filepath.Join(dir, "floor.proj")

	// The smallest project that imports the targets: Language is what the floor target keys on (the SDK sets it),
	// and the target is run alone, so nothing needs a restore.
	content := `<Project>
  <PropertyGroup><Language>C#</Language></PropertyGroup>
  <Import Project="` + targets + `" />
</Project>
`

	if err := os.WriteFile(project, []byte(content), 0o644); err != nil {
		t.Fatalf("cannot write the probe project: %v", err)
	}

	for _, c := range []struct {
		version string
		refused bool
	}{
		{"roslyn4.13", true},
		{"roslyn4.9", true},
		{"roslyn4.14", false},
		{"roslyn5.0", false},
	} {
		cmd := exec.Command(dotnet, "msbuild", project, "-t:Go2csCompilerFloor", "-p:CompilerApiVersion="+c.version, "-nologo", "-v:q")
		cmd.Dir = dir
		cmd.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1")
		out, runErr := cmd.CombinedOutput()
		refused := strings.Contains(string(out), "error GO2CS3001")

		if refused != c.refused || (runErr != nil) != c.refused {
			t.Errorf("CompilerApiVersion=%s: refused=%v (exit error %v), want refused=%v; output:\n%s", c.version, refused, runErr, c.refused, out)
		}
	}
}
