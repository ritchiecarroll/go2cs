// classicPartialFlavours_test.go - Gbtc
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

// A CLASSIC PARTIAL METHOD THAT LOSES ITS BODY LOSES IT SILENTLY. A hand-own in src/core may declare a classic partial
// method (`static partial void name();`, void and with no accessibility keyword) and let another file of the same
// package implement it: internal/cpu's osInitArm64 is declared in amd64/cpu_arm64_process_impl.cs and implemented per
// GOOS. C# erases a classic partial that has no implementing part TOGETHER WITH EVERY CALL TO IT, and go2cs-gen's
// PartialStubGenerator leaves classic partials to the compiler on purpose (an erased call is what such a hook means
// where only some flavours implement it), so a layout move that separates the two halves builds green and the flavour
// simply stops running the body. The darwin/arm64 flip is where this was found: its census moves the darwin body to
// darwin-arm64/, away from the darwin/amd64 build that keeps the call (docs/phase4/DESIGN-darwin-arm64-flavour.md
// section 5; P3's revision 2 finding 1 asked for this standing guard).
//
// The rule: for every target of platformUniverse, every classic partial DEFINITION in a package's compile set has an
// implementing part in that same compile set, or the (package, name, target) is named below with the reason the
// erased call is the intended behaviour there. A named exception that is no longer a gap fails too, so the list
// cannot outlive its reason.
var classicPartialErasedOnPurpose = map[string]string{
	"internal/cpu osInitArm64 windows/amd64": "the arm64 option table runs only on an arm64 process, and " +
		"win-arm64 is not a shipped RID: no windows osInit exists to call (cpu_arm64_process_impl.cs)",
	"internal/poll prepayPlatformDeadlinePath linux/amd64":  "only Windows has a platform deadline path to prepay (deadline_prepay_impl.cs)",
	"internal/poll prepayPlatformDeadlinePath darwin/amd64": "only Windows has a platform deadline path to prepay (deadline_prepay_impl.cs)",
	"internal/poll prepayPlatformDeadlinePath linux/arm64":  "only Windows has a platform deadline path to prepay (deadline_prepay_impl.cs)",
	"internal/poll prepayPlatformDeadlinePath darwin/arm64": "only Windows has a platform deadline path to prepay (deadline_prepay_impl.cs)",
}

var (
	// A classic partial DEFINITION: void, no accessibility keyword, no body. Modifiers C# admits on one are matched.
	classicPartialDefinition = regexp.MustCompile(`(?m)^[ \t]*(?:(?:static|unsafe|new)[ \t]+)*partial[ \t]+void[ \t]+(\w+)[ \t]*\([^)]*\)[ \t]*;`)
	// Any partial void member named N whose parameter list is not followed by `;`: an implementing part.
	partialVoidMember = regexp.MustCompile(`(?m)^[ \t]*(?:(?:static|unsafe|new)[ \t]+)*partial[ \t]+void[ \t]+(\w+)[ \t]*\([^)]*\)[ \t]*([;]?)`)
)

// classicPartialGap is one (package, method, target) whose compile set declares a classic partial and holds no body.
type classicPartialGap struct {
	pkg, name, target string
}

func (gap classicPartialGap) key() string {
	return gap.pkg + " " + gap.name + " " + gap.target
}

// classicPartialGaps walks every converted package under coreDir and returns, per target of universe, each classic
// partial definition with no implementing part in that target's compile set (the package directory's own .cs files
// plus those of each layout source folder the target's build compiles), sorted.
func classicPartialGaps(coreDir string, universe []string) ([]classicPartialGap, int, error) {
	var gaps []classicPartialGap
	definitions := 0

	err := filepath.WalkDir(coreDir, func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if !entry.IsDir() {
			return nil
		}

		if name := entry.Name(); name == "bin" || name == "obj" {
			return filepath.SkipDir
		}

		projects, _ := filepath.Glob(filepath.Join(path, "*.csproj"))

		if len(projects) == 0 {
			return nil
		}

		pkg, err := filepath.Rel(coreDir, path)

		if err != nil {
			return err
		}

		pkg = filepath.ToSlash(pkg)

		// file name (relative to the package) -> its text, for the package's own files and each source folder's.
		sources := map[string]string{}
		var folders []string

		entries, err := os.ReadDir(path)

		if err != nil {
			return err
		}

		read := func(relative string) error {
			data, err := os.ReadFile(filepath.Join(path, relative))

			if err == nil {
				sources[filepath.ToSlash(relative)] = string(data)
			}

			return err
		}

		for _, child := range entries {
			switch {
			case child.IsDir() && isPlatformSourceFolder(path, child.Name()):
				folders = append(folders, child.Name())

				inner, err := os.ReadDir(filepath.Join(path, child.Name()))

				if err != nil {
					return err
				}

				for _, file := range inner {
					if !file.IsDir() && isProductionSource(file.Name()) {
						if err := read(filepath.Join(child.Name(), file.Name())); err != nil {
							return err
						}
					}
				}
			case !child.IsDir() && isProductionSource(child.Name()):
				if err := read(child.Name()); err != nil {
					return err
				}
			}
		}

		for _, target := range universe {
			defined := map[string]bool{}
			implemented := map[string]bool{}

			for relative, text := range sources {
				folder, _, nested := strings.Cut(relative, "/")

				if nested && !layoutFolderServes(folder, target) {
					continue
				}

				for _, match := range classicPartialDefinition.FindAllStringSubmatch(text, -1) {
					defined[match[1]] = true
				}

				for _, match := range partialVoidMember.FindAllStringSubmatch(text, -1) {
					if match[2] == "" {
						implemented[match[1]] = true
					}
				}
			}

			definitions += len(defined)

			for name := range defined {
				if !implemented[name] {
					gaps = append(gaps, classicPartialGap{pkg: pkg, name: name, target: target})
				}
			}
		}

		return nil
	})

	sort.Slice(gaps, func(i, j int) bool { return gaps[i].key() < gaps[j].key() })

	return gaps, definitions, err
}

// isProductionSource reports whether a file name is a C# source the package's production project compiles: the
// test sources a -tests conversion adds beside it belong to the tests project.
func isProductionSource(name string) bool {
	if !strings.EqualFold(filepath.Ext(name), ".cs") {
		return false
	}

	return !strings.HasSuffix(name, "_test.cs") && name != "go2cs_test_host.cs" && name != "package_test_info.cs"
}

func TestClassicPartialsKeepTheirBodyInEveryFlavour(t *testing.T) {
	gaps, definitions, err := classicPartialGaps(filepath.Join("..", "core"), platformUniverse)

	if err != nil {
		t.Fatalf("walking src/core: %v", err)
	}

	// The walk has to have found something to judge, or a broken pattern reads as a clean corpus.
	if definitions == 0 {
		t.Fatalf("no classic partial definition found under src/core across %v: the pattern is broken", platformUniverse)
	}

	seen := map[string]bool{}

	for _, gap := range gaps {
		seen[gap.key()] = true

		if _, named := classicPartialErasedOnPurpose[gap.key()]; !named {
			t.Errorf("%s declares classic partial %s, and the %s build compiles no body for it: C# erases the call "+
				"there with no diagnostic. Move the body (or the definition) so both halves share that flavour's compile "+
				"set, or name it in classicPartialErasedOnPurpose with the reason the erased call is right there",
				gap.pkg, gap.name, gap.target)
		}
	}

	for key, reason := range classicPartialErasedOnPurpose {
		if !seen[key] {
			t.Errorf("classicPartialErasedOnPurpose names %q (%s), but that flavour now compiles a body for it, or no "+
				"longer declares it: remove the entry", key, reason)
		}
	}
}

// The guard against the shape it exists for, on a fixture: the darwin/arm64 census's move of the darwin body into
// darwin-arm64/ is reported for darwin/amd64 (the erasure) and nothing else, and the same tree with the body homed
// beside the darwin/amd64 build reads clean. A windows-only hook with no body elsewhere is reported per target.
func TestClassicPartialGapsNamesASeparatedPair(t *testing.T) {
	five := []string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64", "darwin/arm64"}
	definition := "partial class cpu_package\n{\n    static partial void osInitArm64();\n}\n"
	body := "partial class cpu_package\n{\n    static partial void osInitArm64()\n    {\n    }\n}\n"

	write := func(t *testing.T, root string, files map[string]string) {
		t.Helper()

		for relative, text := range files {
			path := filepath.Join(root, filepath.FromSlash(relative))

			if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
				t.Fatal(err)
			}

			if err := os.WriteFile(path, []byte(text), 0o644); err != nil {
				t.Fatal(err)
			}
		}
	}

	keys := func(gaps []classicPartialGap) []string {
		var out []string

		for _, gap := range gaps {
			out = append(out, gap.key())
		}

		return out
	}

	cases := []struct {
		name  string
		files map[string]string
		want  []string
	}{
		{
			name: "the census move: body in darwin-arm64/, definition in amd64/",
			files: map[string]string{
				"cpu/cpu.csproj":                                  "<Project />",
				"cpu/amd64/cpu_arm64_process_impl.cs":             definition,
				"cpu/darwin-arm64/cpu_arm64_darwin_impl.cs":       body,
				"cpu/linux-amd64/cpu_arm64_process_linux_impl.cs": body,
			},
			want: []string{"cpu osInitArm64 darwin/amd64", "cpu osInitArm64 windows/amd64"},
		},
		{
			name: "D2's placement: the darwin/amd64 body renamed into darwin-amd64/",
			files: map[string]string{
				"cpu/cpu.csproj":                                    "<Project />",
				"cpu/amd64/cpu_arm64_process_impl.cs":               definition,
				"cpu/darwin-amd64/cpu_arm64_process_darwin_impl.cs": body,
				"cpu/linux-amd64/cpu_arm64_process_linux_impl.cs":   body,
			},
			want: []string{"cpu osInitArm64 windows/amd64"},
		},
		{
			name: "a nested package named like a key folder is its own package, never a source folder",
			files: map[string]string{
				"syscall/syscall.csproj":         "<Project />",
				"syscall/hook_impl.cs":           "partial class syscall_package\n{\n    static partial void hook();\n}\n",
				"syscall/windows/windows.csproj": "<Project />",
				"syscall/windows/hook_impl.cs":   "partial class windows_package\n{\n    static partial void hook()\n    {\n    }\n}\n",
			},
			// want is filled below: one gap per target.
		},
	}

	// The nested-package case: syscall's definition has no body in ANY flavour (the body belongs to the nested
	// package syscall/windows, whose own definition is absent), so every target is a gap for syscall and none for
	// syscall/windows, which declares nothing.
	cases[2].want = nil
	for _, target := range five {
		cases[2].want = append(cases[2].want, "syscall hook "+target)
	}
	sort.Strings(cases[2].want)

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			root := t.TempDir()
			write(t, root, c.files)

			gaps, _, err := classicPartialGaps(root, five)

			if err != nil {
				t.Fatal(err)
			}

			if got := keys(gaps); !slices.Equal(got, c.want) {
				t.Errorf("gaps = %s, want %s", fmt.Sprint(got), fmt.Sprint(c.want))
			}
		})
	}
}
