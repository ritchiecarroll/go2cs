// cgoFlagSecurity_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
//
// This file is derived from Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE:
// - The flag tables and TestCgoSecCheckCompilerFlags, TestCgoSecCheckLinkerFlags and TestCgoSecCheckFlagAllowDisallow:
//   DERIVED FROM src/cmd/go/internal/work/security_test.go, Go 1.26.11, goodCompilerFlags through TestCheckLinkerFlags
//   and TestCheckFlagAllowDisallow, names prefixed cgoSec, the environment a map read through cgoSecWith.

package main

import (
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// cgoSecTestCheck runs a check with cmd/go's state as its own tests see it: the host's GOOS and, in place of the
// process environment, env (nil reads nothing).
func cgoSecTestCheck(env map[string]string, check func() error) error {
	return cgoSecWith(func(key string) string { return env[key] }, runtime.GOOS, "", "", check)
}

var cgoSecGoodCompilerFlags = [][]string{
	{"-DFOO"},
	{"-Dfoo=bar"},
	{"-Ufoo"},
	{"-Ufoo1"},
	{"-F/Qt"},
	{"-F", "/Qt"},
	{"-I/"},
	{"-I/etc/passwd"},
	{"-I."},
	{"-O"},
	{"-O2"},
	{"-Osmall"},
	{"-W"},
	{"-Wall"},
	{"-Wp,-Dfoo=bar"},
	{"-Wp,-Ufoo"},
	{"-Wp,-Dfoo1"},
	{"-Wp,-Ufoo1"},
	{"-flto"},
	{"-fobjc-arc"},
	{"-fno-objc-arc"},
	{"-fomit-frame-pointer"},
	{"-fno-omit-frame-pointer"},
	{"-fpic"},
	{"-fno-pic"},
	{"-fPIC"},
	{"-fno-PIC"},
	{"-fpie"},
	{"-fno-pie"},
	{"-fPIE"},
	{"-fno-PIE"},
	{"-fsplit-stack"},
	{"-fno-split-stack"},
	{"-fstack-xxx"},
	{"-fno-stack-xxx"},
	{"-fsanitize=hands"},
	{"-ftls-model=local-dynamic"},
	{"-g"},
	{"-ggdb"},
	{"-mabi=lp64d"},
	{"-march=souza"},
	{"-mcmodel=medium"},
	{"-mcpu=123"},
	{"-mfpu=123"},
	{"-mtls-dialect=gnu"},
	{"-mtls-dialect=gnu2"},
	{"-mtls-dialect=trad"},
	{"-mtls-dialect=desc"},
	{"-mtls-dialect=xyz"},
	{"-msimd=lasx"},
	{"-msimd=xyz"},
	{"-mdouble-float"},
	{"-mrelax"},
	{"-mstrict-align"},
	{"-mlsx"},
	{"-mlasx"},
	{"-mfrecipe"},
	{"-mlam-bh"},
	{"-mlamcas"},
	{"-mld-seq-sa"},
	{"-mno-relax"},
	{"-mno-strict-align"},
	{"-mno-lsx"},
	{"-mno-lasx"},
	{"-mno-frecipe"},
	{"-mno-lam-bh"},
	{"-mno-lamcas"},
	{"-mno-ld-seq-sa"},
	{"-mlarge-data-threshold=16"},
	{"-mtune=happybirthday"},
	{"-mstack-overflow"},
	{"-mno-stack-overflow"},
	{"-mmacosx-version"},
	{"-mnop-fun-dllimport"},
	{"-pthread"},
	{"-std=c99"},
	{"-xc"},
	{"-D", "FOO"},
	{"-D", "foo=bar"},
	{"-I", "."},
	{"-I", "/etc/passwd"},
	{"-I", "世界"},
	{"-I", "=/usr/include/libxml2"},
	{"-I", "dir"},
	{"-I", "$SYSROOT/dir"},
	{"-isystem", "/usr/include/mozjs-68"},
	{"-include", "/usr/include/mozjs-68/RequiredDefines.h"},
	{"-framework", "Chocolate"},
	{"-isysroot", "/tmp"},
	{"-x", "c"},
	{"-v"},
}

var cgoSecBadCompilerFlags = [][]string{
	{"-D@X"},
	{"-D-X"},
	{"-Ufoo=bar"},
	{"-F@dir"},
	{"-F-dir"},
	{"-I@dir"},
	{"-I-dir"},
	{"-O@1"},
	{"-Wa,-foo"},
	{"-W@foo"},
	{"-Wp,-DX,-D@X"},
	{"-Wp,-UX,-U@X"},
	{"-g@gdb"},
	{"-g-gdb"},
	{"-march=@dawn"},
	{"-march=-dawn"},
	{"-mcmodel=@model"},
	{"-mfpu=@0"},
	{"-mfpu=-0"},
	{"-mlarge-data-threshold=@12"},
	{"-mtls-dialect=@gnu"},
	{"-mtls-dialect=-gnu"},
	{"-msimd=@none"},
	{"-msimd=-none"},
	{"-std=@c99"},
	{"-std=-c99"},
	{"-x@c"},
	{"-x-c"},
	{"-D", "@foo"},
	{"-D", "-foo"},
	{"-I", "@foo"},
	{"-I", "-foo"},
	{"-I", "=@obj"},
	{"-include", "@foo"},
	{"-framework", "-Caffeine"},
	{"-framework", "@Home"},
	{"-x", "--c"},
	{"-x", "@obj"},
	{"-xada"},
	{"-x", "ada"},
	{"-x", "none"},
	{"--sysroot=/tmp"},
	{"--sysroot", "/tmp"},
	{"-target", "x86_64-scei-ps4"},
	{"-target", "x86_64-linux-gnu"},
	{"-fsanitize=@foo"},
	{"-fsanitize=-foo"},
	{"-gnatO"},
	{"-gnatO", "foo.o"},
	{"-gen-cdb-fragment-path"},
	{"-gen-cdb-fragment-path", "."},
}

func TestCgoSecCheckCompilerFlags(t *testing.T) {
	for _, f := range cgoSecGoodCompilerFlags {
		if err := cgoSecTestCheck(nil, func() error { return cgoSecCheckCompilerFlags("test", "test", f) }); err != nil {
			t.Errorf("unexpected error for %q: %v", f, err)
		}
	}
	for _, f := range cgoSecBadCompilerFlags {
		if err := cgoSecTestCheck(nil, func() error { return cgoSecCheckCompilerFlags("test", "test", f) }); err == nil {
			t.Errorf("missing error for %q", f)
		}
	}
}

var cgoSecGoodLinkerFlags = [][]string{
	{"-Fbar"},
	{"-lbar"},
	{"-Lbar"},
	{"-fpic"},
	{"-fno-pic"},
	{"-fPIC"},
	{"-fno-PIC"},
	{"-fpie"},
	{"-fno-pie"},
	{"-fPIE"},
	{"-fno-PIE"},
	{"-fsanitize=hands"},
	{"-g"},
	{"-ggdb"},
	{"-march=souza"},
	{"-mcpu=123"},
	{"-mfpu=123"},
	{"-mtune=happybirthday"},
	{"-pic"},
	{"-pthread"},
	{"-Wl,--hash-style=both"},
	{"-Wl,-rpath,foo"},
	{"-Wl,-rpath,$ORIGIN/foo"},
	{"-Wl,-R", "/foo"},
	{"-Wl,-R", "foo"},
	{"-Wl,-R,foo"},
	{"-Wl,-R=foo"},
	{"-Wl,--just-symbols=foo"},
	{"-Wl,--just-symbols,foo"},
	{"-Wl,--warn-error"},
	{"-Wl,--no-warn-error"},
	{"-Wl,--warn-backrefs-exclude=foo*"},
	{"-Wl,-subsystem,console"},
	{"-Wl,--subsystem,console"},
	{"foo.so"},
	{"_世界.dll"},
	{"./x.o"},
	{"libcgosotest.dylib"},
	{"-F", "framework"},
	{"-l", "."},
	{"-l", "世界"},
	{"-L", "framework"},
	{"-framework", "Chocolate"},
	{"-v"},
	{"-Wl,-sectcreate,__TEXT,__info_plist,${SRCDIR}/Info.plist"},
	{"-Wl,-framework", "-Wl,Chocolate"},
	{"-Wl,-framework,Chocolate"},
	{"-Wl,-unresolved-symbols=ignore-all"},
	{"-Wl,-z,relro"},
	{"-Wl,-z,relro,-z,now"},
	{"-Wl,-z,now"},
	{"-Wl,-z,noexecstack"},
	{"libcgotbdtest.tbd"},
	{"./libcgotbdtest.tbd"},
	{"-Wl,--push-state"},
	{"-Wl,--pop-state"},
	{"-Wl,--push-state,--as-needed"},
	{"-Wl,--push-state,--no-as-needed,-Bstatic"},
	{"-Wl,--just-symbols,."},
	{"-Wl,-framework,."},
	{"-Wl,-rpath,."},
	{"-Wl,-rpath-link,."},
	{"-Wl,-sectcreate,.,.,."},
	{"-Wl,-syslibroot,."},
	{"-Wl,-undefined,."},
}

var cgoSecBadLinkerFlags = [][]string{
	{"-DFOO"},
	{"-Dfoo=bar"},
	{"-W"},
	{"-Wall"},
	{"-fobjc-arc"},
	{"-fno-objc-arc"},
	{"-fomit-frame-pointer"},
	{"-fno-omit-frame-pointer"},
	{"-fsplit-stack"},
	{"-fno-split-stack"},
	{"-fstack-xxx"},
	{"-fno-stack-xxx"},
	{"-mstack-overflow"},
	{"-mno-stack-overflow"},
	{"-mnop-fun-dllimport"},
	{"-std=c99"},
	{"-xc"},
	{"-D", "FOO"},
	{"-D", "foo=bar"},
	{"-I", "FOO"},
	{"-L", "@foo"},
	{"-L", "-foo"},
	{"-x", "c"},
	{"-D@X"},
	{"-D-X"},
	{"-I@dir"},
	{"-I-dir"},
	{"-O@1"},
	{"-Wa,-foo"},
	{"-W@foo"},
	{"-g@gdb"},
	{"-g-gdb"},
	{"-march=@dawn"},
	{"-march=-dawn"},
	{"-std=@c99"},
	{"-std=-c99"},
	{"-x@c"},
	{"-x-c"},
	{"-D", "@foo"},
	{"-D", "-foo"},
	{"-I", "@foo"},
	{"-I", "-foo"},
	{"-l", "@foo"},
	{"-l", "-foo"},
	{"-l", "to_library"},
	{"-l", "azy_library"},
	{"-l", "azy_framework"},
	{"-l", "oad_hidden"},
	{"-l", "ibrary"},
	{"-l", "ibrary=:pwn.ld"},
	{"-l", "ibrary-path"},
	{"-l", "ibrary-path=/tmp"},
	{"-lazy_library"},
	{"-lazy_framework"},
	{"-load_hidden"},
	{"-library"},
	{"-library=:pwn.ld"},
	{"-library-path"},
	{"-library-path=/tmp"},
	{"-framework", "-Caffeine"},
	{"-framework", "@Home"},
	{"-Wl,-framework,-Caffeine"},
	{"-Wl,-framework", "-Wl,@Home"},
	{"-Wl,-framework", "@Home"},
	{"-Wl,-framework", "Chocolate"},
	{"-Wl,-framework,/tmp/pwn.ld"},
	{"-Wl,-framework", "-Wl,/tmp/pwn.ld"},
	{"-framework", "./pwn.ld"},
	{"-framework", "/tmp/pwn.ld"},
	{"-Wl,-framework,Chocolate,@Home"},
	{"-Wl,--hash-style=foo"},
	{"-x", "--c"},
	{"-x", "@obj"},
	{"-Wl,-rpath,@foo"},
	{"-Wl,-rpath", "foo"},
	{"-Wl,-R,foo,bar"},
	{"-Wl,-R,@foo"},
	{"-Wl,--just-symbols,@foo"},
	{"-Wl,--just-symbols", "foo"},
	{"-Wl,-undefined", "foo"},
	{"-Wl,-undefined,/tmp/pwn.ld"},
	{"-Wl,-undefined", "-Wl,/tmp/pwn.ld"},
	{"-Wl,--push-state,-undefined=/tmp/pwn.ld"},
	{"../x.o"},
	{"-Wl,-R,"},
	{"-Wl,-R="},
	{"-Wl,-O"},
	{"-Wl,-e="},
	{"-Wl,-e,"},
	{"-Wl,-R,-flag"},
	{"-Wl,-R=-flag"},
	{"-Wl,-R=@foo"},
	{"-Wl,-R==foo"},
	{"-Wl,-R,=foo"},
	{"-Wl,--warn-backrefs-exclude"},
	{"-Wl,--push-state,--warn-backrefs-exclude"},
	{"-Wl,--push-state,"},
	{"-Wl,--push-state,@foo"},
	{"-fplugin=./-Wl,--push-state,-R.so"},
	{"./-Wl,--push-state,-R.c"},
	{"--sysroot=/tmp"},
	{"--sysroot", "/tmp"},
	{"-target", "x86_64-scei-ps4"},
	{"-target", "x86_64-linux-gnu"},
	{"-fsanitize=@foo"},
	{"-fsanitize=-foo"},
	{"-gnatO"},
	{"-gnatO", "-L", "foo.ld"},
	{"-gen-cdb-fragment-path"},
	{"-gen-cdb-fragment-path", "-Wl,-framework", "-Wl,foo.so"},
	{"-l:pwn.ld"},
	{"-l:libc.a"},
	{"-lfoo:bar"},
	{"-lfoo=bar"},
	{"-lsub/foo"},
	{`-lsub\foo`},
	{"-l", ":pwn.ld"},
	{"-l", "foo:bar"},
	{"-l", "foo=bar"},
	{"-l", "/etc/passwd"},
	{"-l", "sub/foo"},
	{"-l", `sub\foo`},
}

func TestCgoSecCheckLinkerFlags(t *testing.T) {
	for _, f := range cgoSecGoodLinkerFlags {
		if err := cgoSecTestCheck(nil, func() error { return cgoSecCheckLinkerFlags("test", "test", f) }); err != nil {
			t.Errorf("unexpected error for %q: %v", f, err)
		}
	}
	for _, f := range cgoSecBadLinkerFlags {
		if err := cgoSecTestCheck(nil, func() error { return cgoSecCheckLinkerFlags("test", "test", f) }); err == nil {
			t.Errorf("missing error for %q", f)
		}
	}
}

func TestCgoSecCheckFlagAllowDisallow(t *testing.T) {
	env := map[string]string{}
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckCompilerFlags("TEST", "test", []string{"-disallow"}) }); err == nil {
		t.Fatalf("missing error for -disallow")
	}
	env["CGO_TEST_ALLOW"] = "-disallo"
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckCompilerFlags("TEST", "test", []string{"-disallow"}) }); err == nil {
		t.Fatalf("missing error for -disallow with CGO_TEST_ALLOW=-disallo")
	}
	env["CGO_TEST_ALLOW"] = "-disallow"
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckCompilerFlags("TEST", "test", []string{"-disallow"}) }); err != nil {
		t.Fatalf("unexpected error for -disallow with CGO_TEST_ALLOW=-disallow: %v", err)
	}
	delete(env, "CGO_TEST_ALLOW")

	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckCompilerFlags("TEST", "test", []string{"-Wall"}) }); err != nil {
		t.Fatalf("unexpected error for -Wall: %v", err)
	}
	env["CGO_TEST_DISALLOW"] = "-Wall"
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckCompilerFlags("TEST", "test", []string{"-Wall"}) }); err == nil {
		t.Fatalf("missing error for -Wall with CGO_TEST_DISALLOW=-Wall")
	}
	env["CGO_TEST_ALLOW"] = "-Wall" // disallow wins
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckCompilerFlags("TEST", "test", []string{"-Wall"}) }); err == nil {
		t.Fatalf("missing error for -Wall with CGO_TEST_DISALLOW=-Wall and CGO_TEST_ALLOW=-Wall")
	}

	env["CGO_TEST_ALLOW"] = "-fplugin.*"
	env["CGO_TEST_DISALLOW"] = "-fplugin=lint.so"
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckCompilerFlags("TEST", "test", []string{"-fplugin=faster.so"}) }); err != nil {
		t.Fatalf("unexpected error for -fplugin=faster.so: %v", err)
	}
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckCompilerFlags("TEST", "test", []string{"-fplugin=lint.so"}) }); err == nil {
		t.Fatalf("missing error for -fplugin=lint.so: %v", err)
	}

	env["CGO_TEST_DISALLOW"] = "-lfoo"
	delete(env, "CGO_TEST_ALLOW")
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckLinkerFlags("TEST", "test", []string{"-l", "foo"}) }); err == nil {
		t.Fatalf("missing error for -l foo with CGO_TEST_DISALLOW=-lfoo")
	}
	env["CGO_TEST_ALLOW"] = "-lto_library"
	delete(env, "CGO_TEST_DISALLOW")
	if err := cgoSecTestCheck(env, func() error { return cgoSecCheckLinkerFlags("TEST", "test", []string{"-l", "to_library"}) }); err != nil {
		t.Fatalf("unexpected error for -l to_library with CGO_TEST_ALLOW=-lto_library: %v", err)
	}
	delete(env, "CGO_TEST_ALLOW")
}

// A package's own #cgo flags are held to cmd/go's allowlist before the route compiles or links with any (COORD's ruling,
// 2026-11-28): what cmd/go refuses is refused in cmd/go's words, what it allows passes, its ALLOW override is honoured,
// and a flag from the environment is exempt, as cmd/go exempts it.
func TestCgoFlagsAllowedHoldsAPackagesFlagsToCmdGosAllowlist(t *testing.T) {
	for _, row := range []struct {
		name, preamble string
		env            []string
		want           string // "" for allowed
	}{
		{"fplugin", "#cgo CFLAGS: -fplugin=./evil.so\n", nil, "invalid flag in #cgo CFLAGS: -fplugin=./evil.so"},
		{"sysroot", "#cgo CFLAGS: --sysroot=/tmp\n", nil, "invalid flag in #cgo CFLAGS: --sysroot=/tmp"},
		{"include", "#cgo CFLAGS: -Isub/dir -DGO2CS=1\n", nil, ""},
		{"ldscript", "#cgo LDFLAGS: -L${SRCDIR}/evil -levil\n", nil, "invalid linker input file in #cgo LDFLAGS"},
		{"allowed by override", "#cgo CFLAGS: -fplugin=./evil.so\n", []string{"CGO_CFLAGS_ALLOW=-fplugin.*"}, ""},
		{"environment exempt", "", []string{"CGO_CFLAGS=-fplugin=./evil.so"}, ""},
	} {
		t.Run(row.name, func(t *testing.T) {
			dir := t.TempDir()
			files := map[string]string{
				"go.mod":  "module example.com/flags\n\ngo 1.26\n",
				"main.go": "package main\n\n/*\n" + row.preamble + "static int x(void) { return 1; }\n*/\nimport \"C\"\n\nfunc main() { _ = C.x() }\n",
			}

			if row.name == "ldscript" {
				files["evil/libevil.a"] = "/* a GNU ld script, not an archive */\nINPUT(-lc)\n"
			}

			if row.name == "include" {
				files["sub/dir/go2cs_flags.h"] = "#define GO2CS_FLAGS 1\n"
			}

			for name, contents := range files {
				full := filepath.Join(dir, filepath.FromSlash(name))

				if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
					t.Fatal(err)
				}

				if err := os.WriteFile(full, []byte(contents), 0o644); err != nil {
					t.Fatal(err)
				}
			}

			env := append(append(os.Environ(), "GOWORK=off", "GOFLAGS=", "GOTOOLCHAIN=local", "CGO_ENABLED=1"), row.env...)
			err := cgoFlagsAllowed(cgoLoad{env: env, dir: dir}, Options{}, &packages.Package{Dir: dir, PkgPath: "example.com/flags"}, runtime.GOOS)

			switch {
			case row.want == "" && err != nil:
				t.Errorf("refused: %v", err)
			case row.want != "" && (err == nil || !strings.Contains(err.Error(), row.want)):
				t.Errorf("got %v, want an error containing %q", err, row.want)
			}
		})
	}
}

// The route refuses a #cgo flag cmd/go refuses on its own, not through the load (COORD's ruling, 2026-11-28: the
// converter must not rely on the loader for flag safety). The offline conversion reads cmd/cgo's trees as written and
// runs no go list -compiled, so no cgo action checks the flags first: the refusal can only be the route's.
func TestCgoTheRouteRefusesAFlagCmdGoRefusesWithoutTheLoad(t *testing.T) {
	files := cgoFixtureFiles(t, cgoPhase3FixtureDir)

	if !strings.Contains(files["fixture.go"], "/*\n#include <stdlib.h>\n") {
		t.Fatal("the fixture's preamble does not open as this row expects")
	}

	files["fixture.go"] = strings.Replace(files["fixture.go"], "/*\n#include <stdlib.h>\n", "/*\n#cgo CFLAGS: -fplugin=./evil.so\n#include <stdlib.h>\n", 1)
	stderr, output, err := convertCgoOffline(t, files)

	if err == nil || !strings.Contains(stderr, "invalid flag in #cgo CFLAGS: -fplugin=./evil.so") {
		t.Errorf("the route did not refuse -fplugin in cmd/go's words (%v):\n%s", err, lastLines(stderr, 12))
	}

	requireNothingWritten(t, output, err, stderr)
}
