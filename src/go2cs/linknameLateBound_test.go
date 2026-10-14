// linknameLateBound_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the LATE-BOUND linkname var pull (varLinknamePull's lateBound, golib's LinknameVar). A two-arg
// `//go:linkname` pull whose direct forwarding reference would close a project cycle -- the target's
// package already imports the puller -- used to keep a plain field of its own, which is never Go's value
// (Go aliases one storage). At go1.26.9 that is math/bits pulling runtime.overflowError / divideError,
// because runtime now imports math/bits: bits.Div64 would panic with nil where Go's tests assert a
// runtime.Error. The pull is now reached by name through golib. This drives a real module conversion and
// reads the EMITTED source and project file, never the helper alone.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertLateBoundPullFixture(t *testing.T) (pullerCs string, pullerCsproj string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/lbcycle\n\ngo 1.23\n")

	// puller pulls lib.secret; lib imports puller. So the pull points UPWARD: a forwarding property in
	// puller would need puller -> lib, against Go's own lib -> puller.
	writeModuleFile(t, filepath.Join(appDir, "puller", "puller.go"), "package puller\n\n"+
		"import _ \"unsafe\" // for go:linkname\n\n"+
		"//go:linkname secret example.com/lbcycle/lib.secret\n"+
		"var secret string\n\n"+
		"// Secret reads the pulled var.\n"+
		"func Secret() string { return secret }\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), "package lib\n\n"+
		"import (\n\t_ \"unsafe\" // for go:linkname\n\n\t\"example.com/lbcycle/puller\"\n)\n\n"+
		"//go:linkname secret\n"+
		"var secret = \"held by lib\"\n\n"+
		"// Read returns what puller sees.\n"+
		"func Read() string { return puller.Secret() }\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\n"+
		"import (\n\t\"fmt\"\n\n\t\"example.com/lbcycle/lib\"\n)\n\n"+
		"func main() { fmt.Println(lib.Read()) }\n")

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

	pullerDir := filepath.Join(options.go2csPath, "src", "example.com", "lbcycle", "puller")
	source, err := os.ReadFile(filepath.Join(pullerDir, "puller.cs"))

	if err != nil {
		t.Fatalf("reading the emitted puller.cs: %v", err)
	}

	projects, _ := filepath.Glob(filepath.Join(pullerDir, "*.csproj"))

	if len(projects) != 1 {
		t.Fatalf("expected one project in %s, found %v", pullerDir, projects)
	}

	project, err := os.ReadFile(projects[0])

	if err != nil {
		t.Fatalf("reading the emitted puller project: %v", err)
	}

	return string(source), string(project)
}

func TestUpwardLinknamePullIsReachedLateBound(t *testing.T) {
	source, project := convertLateBoundPullFixture(t)

	var declaration string

	for _, line := range strings.Split(source, "\n") {
		if strings.Contains(line, " secret") && !strings.HasPrefix(strings.TrimSpace(line), "//") {
			declaration = strings.TrimSpace(line)
			break
		}
	}

	for _, want := range []string{
		"static @string secret {",
		"DynamicDependency(global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields, \"go.example.com.lbcycle.lib_package\", \"example.com.lbcycle.lib\")",
		"get => global::go.LinknameVar<@string>.Get(\"go.example.com.lbcycle.lib_package, example.com.lbcycle.lib\", \"secret\")",
		"set => global::go.LinknameVar<@string>.Set(\"go.example.com.lbcycle.lib_package, example.com.lbcycle.lib\", \"secret\", value)",
	} {
		if !strings.Contains(declaration, want) {
			t.Errorf("the upward pull's declaration does not hold %q -- it must be the late-bound property, never a plain field:\n%s", want, declaration)
		}
	}

	// No compile-time reference to the target: that edge, against Go's own lib -> puller, is the cycle.
	// (golib.csproj also ends in "lib.csproj", so only references into the fixture's module count.)
	for _, line := range strings.Split(project, "\n") {
		if strings.Contains(line, "<ProjectReference") && strings.Contains(line, "lbcycle") {
			t.Errorf("the puller's project references the pull target, closing the cycle the late-bound pull exists to avoid: %s", strings.TrimSpace(line))
		}
	}
}
