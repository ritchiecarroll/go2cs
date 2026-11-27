// cpuStartupFlavours_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"sort"
	"strings"
	"testing"
)

// EVERY FLAVOUR FILLS ITS CPU FLAGS AT START-UP. In Go the runtime fills internal/cpu before any package init reads a
// flag: schedinit calls cpuinit, which calls cpu.Initialize(GODEBUG). go2cs never runs schedinit, so each flavour of
// internal/cpu carries a [ModuleInitializer] that stands in for that slot (amd64/cpu_x86_impl.cs explains it). A flavour
// whose compile set holds none builds and runs green with every flag of its GOARCH at the zero value, so the runtime's
// AES hash, crypto and bytealg take the fallbacks Go does not take on that machine, and nothing reports it.
// darwin/arm64 is where it was found: D2 gave that flavour Go's osInit but no initializer to reach it (P6's D5
// golib-tests reading on osx-arm64, 2026-11-25; COORD ruled the fold into D2 the same day).
//
// The rule: for every target of platformUniverse, internal/cpu's compile set (the package's own files plus the layout
// source folders that target compiles) holds a [ModuleInitializer] whose body fills that target's GOARCH flag set, as
// cpuStartupFill names it. A GOARCH with no entry fails, so a new architecture in the universe has to say how its
// flags are filled before it ships.
var cpuStartupFill = map[string]*regexp.Regexp{
	// No Go-shaped detection exists for x86 here: the initializer maps System.Runtime.Intrinsics.X86 to cpu.X86.
	"amd64": regexp.MustCompile(`\bX86\.Has\w+\s*=`),
	// The converted Initialize: doinit's option table, Go's own osInit for the GOOS, then processOptions.
	"arm64": regexp.MustCompile(`\bInitialize\s*\(`),
}

var moduleInitializerAttribute = regexp.MustCompile(`\[(?:global::System\.Runtime\.CompilerServices\.)?ModuleInitializer(?:Attribute)?\]`)

// moduleInitializerBodies returns the body of each method marked [ModuleInitializer] in text: the brace-balanced block
// that follows the attribute's method signature. A file whose braces do not balance after an attribute yields what it
// has read so far, which a guard reads as a body that fills nothing.
func moduleInitializerBodies(text string) []string {
	var bodies []string

	for _, location := range moduleInitializerAttribute.FindAllStringIndex(text, -1) {
		rest := text[location[1]:]
		open := strings.IndexByte(rest, '{')

		if open < 0 {
			continue
		}

		depth := 0
		end := len(rest)

		for i := open; i < len(rest); i++ {
			switch rest[i] {
			case '{':
				depth++
			case '}':
				depth--
			}

			if depth == 0 {
				end = i + 1
				break
			}
		}

		bodies = append(bodies, rest[open:end])
	}

	return bodies
}

// cpuStartupGaps returns, sorted, each target of universe whose compile set of the package at pkgDir holds no
// [ModuleInitializer] that fills the target's GOARCH flags (or whose GOARCH cpuStartupFill does not name).
func cpuStartupGaps(pkgDir string, universe []string) ([]string, error) {
	sources := map[string]string{}

	entries, err := os.ReadDir(pkgDir)

	if err != nil {
		return nil, err
	}

	read := func(relative string) error {
		data, err := os.ReadFile(filepath.Join(pkgDir, relative))

		if err == nil {
			sources[filepath.ToSlash(relative)] = string(data)
		}

		return err
	}

	for _, child := range entries {
		switch {
		case child.IsDir() && isPlatformSourceFolder(pkgDir, child.Name()):
			inner, err := os.ReadDir(filepath.Join(pkgDir, child.Name()))

			if err != nil {
				return nil, err
			}

			for _, file := range inner {
				if !file.IsDir() && isProductionSource(file.Name()) {
					if err := read(filepath.Join(child.Name(), file.Name())); err != nil {
						return nil, err
					}
				}
			}
		case !child.IsDir() && isProductionSource(child.Name()):
			if err := read(child.Name()); err != nil {
				return nil, err
			}
		}
	}

	var gaps []string

	for _, target := range universe {
		_, goarch, _ := strings.Cut(target, "/")
		fill, named := cpuStartupFill[goarch]
		filled := false

		for relative, text := range sources {
			if !named || filled {
				break
			}

			folder, _, nested := strings.Cut(relative, "/")

			if nested && !layoutFolderServes(folder, target) {
				continue
			}

			for _, body := range moduleInitializerBodies(text) {
				if fill.MatchString(body) {
					filled = true
					break
				}
			}
		}

		if !filled {
			gaps = append(gaps, target)
		}
	}

	sort.Strings(gaps)

	return gaps, nil
}

func TestInternalCpuFillsItsFlagsInEveryFlavour(t *testing.T) {
	gaps, err := cpuStartupGaps(filepath.Join("..", "core", "internal", "cpu"), platformUniverse)

	if err != nil {
		t.Fatalf("reading src/core/internal/cpu: %v", err)
	}

	for _, target := range gaps {
		_, goarch, _ := strings.Cut(target, "/")

		if _, named := cpuStartupFill[goarch]; !named {
			t.Errorf("platformUniverse carries %s, and cpuStartupFill does not say how a %s flavour fills internal/cpu's "+
				"flags at start-up: add the entry with the initializer that does it", target, goarch)
			continue
		}

		t.Errorf("the %s build of internal/cpu compiles no [ModuleInitializer] that fills its %s flags (%s): nothing "+
			"stands in for schedinit's cpuinit there, so every flag reads false and the runtime takes Go's fallbacks. "+
			"Add the initializer to that flavour's compile set, as linux-arm64/cpu_arm64_hwcap_impl.cs does",
			target, goarch, cpuStartupFill[goarch])
	}
}

// The guard against the shape it exists for, on a fixture: a darwin-arm64/ folder with Go's osInit and no initializer
// is reported for darwin/arm64 alone; the same tree with the initializer reads clean; an initializer elsewhere in the
// tree that does not fill the flags, or one that sits in a folder the target does not compile, does not count.
func TestCpuStartupGapsNamesAFlavourWithNoInitializer(t *testing.T) {
	five := []string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64", "darwin/arm64"}
	x86 := "partial class cpu_package {\n    [ModuleInitializer]\n    internal static void initX86() {\n" +
		"        if (!X86Base.IsSupported) { return; }\n        X86.HasAES = Aes.IsSupported;\n    }\n}\n"
	linux := "partial class cpu_package {\n[ModuleInitializer]\ninternal static void initLinuxArm64() {\n" +
		"    HWCap = libcGetauxval(AT_HWCAP);\n    Initialize(Environment.GetEnvironmentVariable(\"GODEBUG\") ?? \"\");\n}\n}\n"
	osInit := "partial class cpu_package {\ninternal static void osInit() {\n    ARM64.HasAES = true;\n}\n}\n"
	darwin := osInit + "partial class cpu_package {\n[ModuleInitializer]\ninternal static void initDarwinArm64() {\n" +
		"    Initialize(Environment.GetEnvironmentVariable(\"GODEBUG\") ?? \"\");\n}\n}\n"
	idle := osInit + "partial class cpu_package {\n[ModuleInitializer]\ninternal static void initDarwinArm64() {\n" +
		"    { }\n}\n}\n"

	cases := []struct {
		name  string
		files map[string]string
		want  []string
	}{
		{
			name: "D2 before the fold: darwin-arm64/ has osInit and no initializer; a test source does not count",
			files: map[string]string{
				"cpu.csproj":                            "<Project />",
				"amd64/cpu_x86_impl.cs":                 x86,
				"linux-arm64/cpu_arm64_hwcap_impl.cs":   linux,
				"darwin-arm64/cpu_arm64_darwin_impl.cs": osInit,
				"darwin-arm64/cpu_arm64_darwin_test.cs": darwin,
				"darwin/cpu_darwin.cs":                  "partial class cpu_package { }\n",
			},
			want: []string{"darwin/arm64"},
		},
		{
			name: "the fold: darwin-arm64/ carries the initializer",
			files: map[string]string{
				"cpu.csproj":                            "<Project />",
				"amd64/cpu_x86_impl.cs":                 x86,
				"linux-arm64/cpu_arm64_hwcap_impl.cs":   linux,
				"darwin-arm64/cpu_arm64_darwin_impl.cs": darwin,
			},
			want: nil,
		},
		{
			name: "an initializer that fills nothing does not count",
			files: map[string]string{
				"cpu.csproj":                            "<Project />",
				"amd64/cpu_x86_impl.cs":                 x86,
				"linux-arm64/cpu_arm64_hwcap_impl.cs":   linux,
				"darwin-arm64/cpu_arm64_darwin_impl.cs": idle,
			},
			want: []string{"darwin/arm64"},
		},
		{
			name: "the arm64 initializer homed only under linux-arm64/ serves neither darwin flavour nor the amd64 ones",
			files: map[string]string{
				"cpu.csproj":                          "<Project />",
				"linux-arm64/cpu_arm64_hwcap_impl.cs": linux,
				"darwin-arm64/cpu_arm64_darwin.cs":    osInit,
			},
			want: []string{"darwin/amd64", "darwin/arm64", "linux/amd64", "windows/amd64"},
		},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			root := t.TempDir()

			for relative, text := range c.files {
				path := filepath.Join(root, filepath.FromSlash(relative))

				if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
					t.Fatal(err)
				}

				if err := os.WriteFile(path, []byte(text), 0o644); err != nil {
					t.Fatal(err)
				}
			}

			gaps, err := cpuStartupGaps(root, five)

			if err != nil {
				t.Fatal(err)
			}

			if !slices.Equal(gaps, c.want) {
				t.Errorf("gaps = %s, want %s", fmt.Sprint(gaps), fmt.Sprint(c.want))
			}
		})
	}
}
