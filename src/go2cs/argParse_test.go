// argParse_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"flag"
	"io"
	"reflect"
	"testing"
)

// TestParseArgsInterspersed verifies flags are honored even when they appear AFTER positional
// arguments — Go's flag package otherwise stops parsing at the first non-flag token. This is the fix
// for `go2cs -recurse . -go2cspath dir` silently dropping -go2cspath (and writing to the default
// output root instead of the one the user passed).
func TestParseArgsInterspersed(t *testing.T) {
	cases := []struct {
		name    string
		args    []string
		wantPos []string
		wantRec bool
		wantOut string
	}{
		{"flag after positional", []string{"-recurse", ".", "-go2cspath", "out"}, []string{"."}, true, "out"},
		{"flags first (unchanged)", []string{"-recurse", "-go2cspath", "out", "."}, []string{"."}, true, "out"},
		{"two positionals then flag", []string{"a.go", "b.cs", "-go2cspath", "out"}, []string{"a.go", "b.cs"}, false, "out"},
		{"stdlib-style filter after flag", []string{"-recurse", "fmt", "io", "strings"}, []string{"fmt", "io", "strings"}, true, ""},
		{"equals form after positional", []string{".", "-go2cspath=out"}, []string{"."}, false, "out"},
		{"no positionals", []string{"-recurse"}, nil, true, ""},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			fs := flag.NewFlagSet("test", flag.ContinueOnError)
			rec := fs.Bool("recurse", false, "")
			out := fs.String("go2cspath", "", "")

			pos, err := parseArgsInterspersed(fs, tc.args)

			if err != nil {
				t.Fatalf("unexpected error: %v", err)
			}

			if !reflect.DeepEqual(pos, tc.wantPos) {
				t.Errorf("positionals = %#v, want %#v", pos, tc.wantPos)
			}

			if *rec != tc.wantRec {
				t.Errorf("recurse = %v, want %v", *rec, tc.wantRec)
			}

			if *out != tc.wantOut {
				t.Errorf("go2cspath = %q, want %q", *out, tc.wantOut)
			}
		})
	}
}

// TestRecurseModeFlag locks the -recurse flag contract after it became a custom optional-value flag.Value.
// A bare `-recurse` (and `-recurse .`) must still enable LOCAL project references without consuming the
// following positional (backward compatibility), while `-recurse=nuget` selects NuGet PackageReferences,
// `-recurse=module` narrows the SCOPE to the input module's own packages, the two compose in either order
// as a comma-separated list, and `-recurse=false` disables it. An unrecognized value is an error, not a
// silent enable.
func TestRecurseModeFlag(t *testing.T) {
	cases := []struct {
		name        string
		args        []string
		wantPos     []string
		wantEnabled bool
		wantNuget   bool
		wantModule  bool
		wantErr     bool
	}{
		{"bare recurse then positional", []string{"-recurse", "."}, []string{"."}, true, false, false, false},
		{"recurse=nuget then positional", []string{"-recurse=nuget", "."}, []string{"."}, true, true, false, false},
		{"recurse=nuget flag only", []string{"-recurse=nuget"}, nil, true, true, false, false},
		{"recurse=true", []string{"-recurse=true", "."}, []string{"."}, true, false, false, false},
		{"recurse=false", []string{"-recurse=false", "."}, []string{"."}, false, false, false, false},
		{"nuget after positional", []string{".", "-recurse=nuget"}, []string{"."}, true, true, false, false},
		{"recurse=module", []string{"-recurse=module", "."}, []string{"."}, true, false, true, false},
		{"recurse=module,nuget", []string{"-recurse=module,nuget", "."}, []string{"."}, true, true, true, false},
		{"recurse=nuget,module", []string{"-recurse=nuget,module", "."}, []string{"."}, true, true, true, false},
		{"module then false clears", []string{"-recurse=module,false", "."}, []string{"."}, false, false, false, false},
		{"invalid value errors", []string{"-recurse=bogus"}, nil, false, false, false, true},
		{"invalid value in a valid list errors", []string{"-recurse=module,bogus"}, nil, false, false, false, true},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			fs := flag.NewFlagSet("test", flag.ContinueOnError)
			fs.SetOutput(io.Discard)

			var rec recurseMode
			fs.Var(&rec, "recurse", "")

			pos, err := parseArgsInterspersed(fs, tc.args)

			if tc.wantErr {
				if err == nil {
					t.Fatalf("expected an error for %v, got none (enabled=%v nuget=%v)", tc.args, rec.enabled, rec.nuget)
				}
				return
			}

			if err != nil {
				t.Fatalf("unexpected error: %v", err)
			}

			if !reflect.DeepEqual(pos, tc.wantPos) {
				t.Errorf("positionals = %#v, want %#v", pos, tc.wantPos)
			}

			if rec.enabled != tc.wantEnabled {
				t.Errorf("enabled = %v, want %v", rec.enabled, tc.wantEnabled)
			}

			if rec.nuget != tc.wantNuget {
				t.Errorf("nuget = %v, want %v", rec.nuget, tc.wantNuget)
			}

			if rec.moduleOnly != tc.wantModule {
				t.Errorf("moduleOnly = %v, want %v", rec.moduleOnly, tc.wantModule)
			}
		})
	}
}
