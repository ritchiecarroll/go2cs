// cgoTargetCheck_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"io/fs"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// A6, END TO END (cgo against go.* INC 3; P3's read and COORD's rulings). A cgo package's bindings are converted for one
// target and, on linux, against glibc (the converter's R3 binds gnu only). golib's GoCgoLibraries.Register refuses them,
// by name, in the package's module initializer, in any other process: before any of the package's code runs, since its
// managed code carries C layouts folded for the target (LongBytes below calls no C and still answers the target's width).
//
// The fixture is a module with a cgo package (cptr), a main whose argv picks the first touch (a plain call, a type
// assertion on a cptr value, a call under defer recover(), a C call), and a second main that reaches cptr only through
// another package's initializer. It is converted with -recurse -cgo for this linux/amd64 host, built against this tree,
// and run three ways:
//   - here, on glibc: every arm prints what go prints (a correct-target process sees no change);
//   - in Microsoft's Alpine .NET image (musl): every arm dies by name;
//   - in Alpine with Alpine's packaged dotnet (musl): every arm dies by name, and the RID that runtime reports, and
//     whether gcompat is present, are READINGS (P3: a distribution-built runtime's RID was not measured).
//
// Opt-in (GO2CS_CGO_BUILD=1, the cgo-e2e stage): the build takes the converted standard library's closure. linux/amd64
// only (the images are amd64), and the musl arms need docker; where docker does not answer they are NOT MEASURED, said so.

const cgoTargetCheckCptr = `package cptr

// #include <stdlib.h>
import "C"

import "unsafe"

// Val is a value of the cgo package's own type.
type Val struct{ N int }

// String makes Val a fmt.Stringer.
func (v Val) String() string { return "val" }

// LongBytes is the width of C's long on the target the package was converted for. It calls no C.
func LongBytes() int { return int(unsafe.Sizeof(C.long(0))) }

// Alloc allocates one byte of C memory.
func Alloc() unsafe.Pointer { return C.malloc(C.size_t(1)) }

// Free releases what Alloc returned.
func Free(p unsafe.Pointer) { C.free(p) }
`

const cgoTargetCheckNest = `package nest

import "example.com/cgorecurse/cptr"

// Width is set by the package's initializer, from the cgo package.
var Width = cptr.LongBytes()
`

const cgoTargetCheckMain = `package main

import (
	"fmt"
	"os"

	"example.com/cgorecurse/cptr"
)

func main() {
	switch os.Args[1] {
	case "call":
		fmt.Println("long", cptr.LongBytes())
	case "assert":
		var v any = cptr.Val{N: 1}
		_, ok := v.(fmt.Stringer)
		fmt.Println("stringer", ok)
	case "recover":
		func() {
			defer func() { fmt.Println("recovered:", recover()) }()
			fmt.Println("long", cptr.LongBytes())
		}()
		fmt.Println("after")
	case "malloc":
		p := cptr.Alloc()
		fmt.Println("alloc", p != nil)
		cptr.Free(p)
	}
}
`

const cgoTargetCheckNested = `package main

import (
	"fmt"

	"example.com/cgorecurse/nest"
)

func main() { fmt.Println("width", nest.Width) }
`

// cgoTargetCheckArm is one run: the program (main or nested) and its argument.
type cgoTargetCheckArm struct{ program, arg string }

func (a cgoTargetCheckArm) label() string {
	if a.arg == "" {
		return a.program
	}

	return a.program + " " + a.arg
}

var cgoTargetCheckArms = []cgoTargetCheckArm{{"main", "call"}, {"main", "assert"}, {"main", "recover"}, {"main", "malloc"}, {"nested", ""}}

// cgoTargetCheckAssembly finds a built program's assembly under the -recurse output's artifacts: the .dll beside a
// runtimeconfig of the same name.
func cgoTargetCheckAssembly(t *testing.T, output, name string) string {
	t.Helper()

	var found []string

	_ = filepath.WalkDir(filepath.Join(output, ".artifacts", "bin"), func(path string, entry fs.DirEntry, err error) error {
		if err == nil && !entry.IsDir() && entry.Name() == name+".runtimeconfig.json" {
			found = append(found, strings.TrimSuffix(path, ".runtimeconfig.json")+".dll")
		}

		return nil
	})

	if len(found) != 1 {
		t.Fatalf("want one built %s under %s, found %v", name, output, found)
	}

	return found[0]
}

func TestACgoPackageIsRefusedByNameInAProcessItWasNotConvertedFor(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to convert, build and run the A6 programs (Inconclusive, not a pass)", cgoBuildEnv)
	}

	if runtime.GOOS != "linux" || runtime.GOARCH != "amd64" {
		t.Skipf("the musl arms run linux/amd64 images, and the host control is this linux/amd64 build: NOT MEASURED on %s/%s", runtime.GOOS, runtime.GOARCH)
	}

	requireHostCC(t)

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	files := map[string]string{"cptr/cptr.go": cgoTargetCheckCptr, "nest/nest.go": cgoTargetCheckNest, "main.go": cgoTargetCheckMain, "cmd/nested/main.go": cgoTargetCheckNested}
	combined, output, err := convertCgoRecurse(t, files, "-recurse", "-cgo=true")

	if err != nil {
		t.Fatalf("the A6 fixture did not convert: %v\n%s", err, lastLines(combined, 30))
	}

	output = cgoBuildDir(t, output)
	module := filepath.Join(output, "src", "example.com", "cgorecurse")
	assemblies := map[string]string{}

	for program, project := range map[string]string{"main": filepath.Join(module, "example.com.cgorecurse.csproj"), "nested": filepath.Join(module, "cmd", "nested", "nested.csproj")} {
		build := exec.Command(dotnet, cgoBuildArgs(t, project, "linux", "amd64")...)
		build.Env = cgoBuildEnvironment()

		if built, err := build.CombinedOutput(); err != nil {
			t.Fatalf("the %s program does not build: %v\n%s", program, err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 30))
		}

		name := strings.TrimSuffix(filepath.Base(project), ".csproj")

		if program == "main" {
			name = "cgorecurse"
		}

		assemblies[program] = cgoTargetCheckAssembly(t, output, name)
	}

	// The glibc control: every arm prints what go prints for the same sources.
	source := t.TempDir()
	writeCgoFixture(t, source, "go.mod", "module example.com/cgorecurse\n\ngo 1.26\n")

	for name, contents := range files {
		if err := os.MkdirAll(filepath.Dir(filepath.Join(source, name)), 0o755); err != nil {
			t.Fatal(err)
		}

		writeCgoFixture(t, source, name, contents)
	}

	for _, arm := range cgoTargetCheckArms {
		t.Run("glibc "+arm.label(), func(t *testing.T) {
			goArgs := []string{"run", "."}

			if arm.program == "nested" {
				goArgs = []string{"run", "./cmd/nested"}
			}

			if arm.arg != "" {
				goArgs = append(goArgs, arm.arg)
			}

			goRun := exec.Command("go", goArgs...)
			goRun.Dir = source
			goRun.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=")
			want, err := goRun.CombinedOutput()

			if err != nil {
				t.Fatalf("go did not run %s: %v\n%s", arm.label(), err, want)
			}

			csArgs := []string{assemblies[arm.program]}

			if arm.arg != "" {
				csArgs = append(csArgs, arm.arg)
			}

			got, err := exec.Command(dotnet, csArgs...).CombinedOutput()
			t.Logf("READING glibc %s: go %q; go2cs %q (%v)", arm.label(), strings.TrimSpace(string(want)), strings.TrimSpace(string(got)), err)

			if err != nil || string(got) != string(want) {
				t.Errorf("on glibc, the converted %s differs from go or failed (%v):\n   go: %q\ngo2cs: %q", arm.label(), err, want, got)
			}
		})
	}

	// The musl arms, in containers. The two programs' build directory is mounted read-only.
	docker, err := exec.LookPath("docker")

	if err != nil || exec.Command(docker, "info").Run() != nil {
		// A leg that requires the musl arms (GO2CS_REQUIRE_DOCKER=1: the hosted linux cgo-e2e legs) fails here rather
		// than reading green with them unmeasured (P3's read of INC 3, SHOULD 1).
		if os.Getenv("GO2CS_REQUIRE_DOCKER") == "1" {
			t.Fatal("docker does not answer, and GO2CS_REQUIRE_DOCKER=1 requires the musl arms")
		}

		t.Logf("READING NOT MEASURED: docker does not answer here, so the musl arms did not run")
		return
	}

	bin := filepath.Dir(assemblies["main"])

	if filepath.Dir(assemblies["nested"]) != bin {
		// Each program's artifacts directory is its own; both are mounted.
		t.Logf("READING the two programs build into %s and %s", filepath.Dir(assemblies["main"]), filepath.Dir(assemblies["nested"]))
	}

	images := []struct {
		name, image, setup string
	}{
		{"Microsoft's Alpine .NET image", "mcr.microsoft.com/dotnet/runtime:10.0-alpine", "true"},
		{"Alpine's packaged dotnet", "alpine:3.23", "apk add --no-cache dotnet10-runtime >/dev/null"},
	}

	for _, image := range images {
		t.Run(image.name, func(t *testing.T) {
			var script strings.Builder
			script.WriteString(image.setup + " || exit 97\n")
			script.WriteString("echo \"RELEASE $(cat /etc/alpine-release)\"\n")
			script.WriteString("echo \"RID $(dotnet --info 2>/dev/null | awk '/RID:/ {print $2; exit}')\"\n")
			script.WriteString("if apk info -e gcompat >/dev/null 2>&1; then echo GCOMPAT yes; else echo GCOMPAT no; fi\n")

			for _, arm := range cgoTargetCheckArms {
				program := "/main/" + filepath.Base(assemblies["main"]) + " " + arm.arg

				if arm.program == "nested" {
					program = "/nested/" + filepath.Base(assemblies["nested"])
				}

				script.WriteString("echo \"ARM " + arm.label() + "\"\ndotnet " + program + " 2>&1\necho \"EXIT $?\"\n")
			}

			run := exec.Command(docker, "run", "--rm", "-v", filepath.Dir(assemblies["main"])+":/main:ro", "-v", filepath.Dir(assemblies["nested"])+":/nested:ro",
				image.image, "sh", "-c", script.String())
			out, err := run.CombinedOutput()
			text := strings.ReplaceAll(string(out), "\r\n", "\n")

			if code, ok := err.(*exec.ExitError); ok && code.ExitCode() == 97 {
				if os.Getenv("GO2CS_REQUIRE_DOCKER") == "1" {
					t.Fatalf("%s could not be set up (%s), and GO2CS_REQUIRE_DOCKER=1 requires its arm:\n%s", image.name, image.setup, lastLines(text, 10))
				}

				t.Skipf("NOT MEASURED: %s could not be set up (%s):\n%s", image.name, image.setup, lastLines(text, 10))
			}

			for _, line := range strings.Split(text, "\n") {
				if strings.HasPrefix(line, "RELEASE ") || strings.HasPrefix(line, "RID ") || strings.HasPrefix(line, "GCOMPAT ") {
					t.Logf("READING %s: %s", image.name, line)
				}
			}

			sections := strings.Split(text, "ARM ")

			for _, arm := range cgoTargetCheckArms {
				var section string

				for _, candidate := range sections {
					if strings.HasPrefix(candidate, arm.label()+"\n") {
						section = candidate
					}
				}

				refused := strings.Contains(section, "GoCgoTargetMismatchException") && strings.Contains(section, "converted for linux/amd64 against glibc") &&
					!strings.Contains(section, "\nEXIT 0")
				t.Logf("READING %s %s: refused by name %v; %s", image.name, arm.label(), refused, cgoTargetCheckFirstLines(section))

				if !refused {
					t.Errorf("%s: %s was not refused by name:\n%s", image.name, arm.label(), lastLines(section, 12))
				}
			}
		})
	}
}

// cgoTargetCheckFirstLines is a section's first two exception lines and its exit, for a READING.
func cgoTargetCheckFirstLines(section string) string {
	var picked []string

	for _, line := range strings.Split(section, "\n") {
		line = strings.TrimSpace(line)

		if strings.Contains(line, "Exception") && len(picked) < 2 {
			if len(line) > 160 {
				line = line[:160]
			}

			picked = append(picked, line)
		}

		if strings.HasPrefix(line, "EXIT ") {
			picked = append(picked, line)
		}
	}

	return strings.Join(picked, " | ")
}
