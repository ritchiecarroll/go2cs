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
//   DERIVED FROM src/cmd/go/internal/work/security_test.go, Go 1.26.11, cgoSecGoodCompilerFlags through TestCheckLinkerFlags
//   and TestCheckFlagAllowDisallow, names prefixed cgoSec, the environment a map read through cgoSecWith.
// - TestCgoSecCheckLinkerSysrootFlags, TestCgoSecCheckGCCSpecFiles, TestCgoSecCheckLinkerWlFlags,
//   TestCgoSecCheckPkgConfigFlags, TestCgoSecCheckCompilerFlagsForInternalLink, cgoSecMakeTestCOFFSection,
//   cgoSecMakeTestCOFFLongSection, TestCgoSecCheckLinkerInputFiles, TestCgoSecCheckLinkerCOFFDrectve and
//   TestCgoSecCheckLinkerObjectSections: DERIVED FROM the same file, the functions of those names, names prefixed cgoSec
//   and each test's body run inside cgoSecTestRun (cmd/go's state as its own tests see it).

package main

import (
	"archive/zip"
	"bytes"
	"encoding/binary"
	"fmt"
	"go/ast"
	"go/format"
	"go/parser"
	"go/token"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"slices"
	"strings"
	"testing"

	"golang.org/x/tools/go/ast/astutil"
	"golang.org/x/tools/go/packages"
)

// cgoSecTestRun runs a ported test's body inside cgoSecWith, with the host's GOOS and no environment overrides, as
// cmd/go's own tests run with its configuration. A t.Fatal inside the body ends the goroutine, and cgoSecWith's deferred
// unlock still runs.
func cgoSecTestRun(t *testing.T, body func()) {
	t.Helper()

	_ = cgoSecWith(func(string) string { return "" }, runtime.GOOS, "", "", func() error {
		body()
		return nil
	})
}

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
			load := cgoLoad{env: env, dir: dir}
			listed, err := cgoSideList(load, Options{}, &packages.Package{Dir: dir, PkgPath: "example.com/flags"})

			if err != nil {
				t.Fatal(err)
			}

			err = cgoFlagsAllowed(load, listed, runtime.GOOS)

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

// The check reads the listing the side build compiles with (P3's MUST on 4b, 2026-11-28): the package listed once, by
// import path, in the load's directory. A dependency fetched from a module proxy with no go.mod of its own sits in the
// module cache with no go.mod above it; listed from there (`go list .` in its own directory, the old check's shape) it
// is refused, "go.mod file not found", while the load's listing reads its #cgo flags, and so the check refuses its
// -fplugin in cmd/go's words. The proxy is a local directory, so nothing is fetched from the network.
func TestCgoFlagsAllowedChecksTheListingTheSideBuildCompilesWith(t *testing.T) {
	root := t.TempDir()
	proxy := filepath.Join(root, "proxy", "example.com", "dep", "@v")
	main := filepath.Join(root, "main")

	for _, dir := range []string{proxy, main} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	var zipped bytes.Buffer
	archive := zip.NewWriter(&zipped)
	entry, err := archive.Create("example.com/dep@v1.0.0/dep.go")

	if err != nil {
		t.Fatal(err)
	}

	if _, err := entry.Write([]byte("package dep\n\n/*\n#cgo CFLAGS: -fplugin=./evil.so\nstatic int x(void) { return 1; }\n*/\nimport \"C\"\n\nfunc X() int { return int(C.x()) }\n")); err != nil {
		t.Fatal(err)
	}

	if err := archive.Close(); err != nil {
		t.Fatal(err)
	}

	for name, contents := range map[string][]byte{
		filepath.Join(proxy, "list"):        []byte("v1.0.0\n"),
		filepath.Join(proxy, "v1.0.0.info"): []byte(`{"Version":"v1.0.0"}`),
		filepath.Join(proxy, "v1.0.0.mod"):  []byte("module example.com/dep\n"),
		filepath.Join(proxy, "v1.0.0.zip"):  zipped.Bytes(),
		filepath.Join(main, "go.mod"):       []byte("module example.com/main\n\ngo 1.26\n\nrequire example.com/dep v1.0.0\n"),
		filepath.Join(main, "main.go"):      []byte("package main\n\nimport _ \"example.com/dep\"\n\nfunc main() {}\n"),
	} {
		if err := os.WriteFile(name, contents, 0o644); err != nil {
			t.Fatal(err)
		}
	}

	proxyURL := "file://" + filepath.ToSlash(filepath.Join(root, "proxy"))

	if !strings.HasPrefix(filepath.ToSlash(root), "/") {
		proxyURL = "file:///" + filepath.ToSlash(filepath.Join(root, "proxy"))
	}

	// -modcacherw: the module cache is otherwise written read-only, and t.TempDir cannot remove it on unix.
	env := append(os.Environ(), "GOWORK=off", "GOFLAGS=-mod=mod -modcacherw", "GOTOOLCHAIN=local", "CGO_ENABLED=1",
		"GOPROXY="+proxyURL, "GOSUMDB=off", "GONOSUMCHECK=1", "GOMODCACHE="+filepath.Join(root, "modcache"))
	load := cgoLoad{env: env, dir: main}
	listed, err := cgoSideList(load, Options{}, &packages.Package{PkgPath: "example.com/dep"})

	if err != nil {
		t.Fatal(err)
	}

	if !slices.Contains(listed.CgoCFLAGS, "-fplugin=./evil.so") {
		t.Fatalf("the load's listing does not carry the dependency's #cgo CFLAGS: %q (dir %s)", listed.CgoCFLAGS, listed.Dir)
	}

	// The old shape, a second listing from the package's own directory, answers nothing usable there.
	own := exec.Command("go", "list", "-json=CgoCFLAGS", ".")
	own.Env = env
	own.Dir = listed.Dir

	if out, ownErr := own.CombinedOutput(); ownErr == nil {
		t.Errorf("a listing from the dependency's own directory succeeded, where it has no go.mod above it:\n%s", out)
	}

	if err := cgoFlagsAllowed(load, listed, runtime.GOOS); err == nil || !strings.Contains(err.Error(), "invalid flag in #cgo CFLAGS: -fplugin=./evil.so") {
		t.Errorf("got %v, want the dependency's -fplugin refused in cmd/go's words", err)
	}
}

func TestCgoSecCheckLinkerSysrootFlags(t *testing.T) {
	cgoSecTestRun(t, func() {
		dir := t.TempDir()
		script := filepath.Join(dir, "script.ld")
		if err := os.WriteFile(script, []byte("PLUGIN(\"/tmp/pwn.so\")\n"), 0644); err != nil {
			t.Fatal(err)
		}
		for _, f := range [][]string{
			{"-isysroot", dir},
			{"-Wl,-syslibroot," + dir},
			{"-Wl,-syslibroot=" + dir},
		} {
			if err := cgoSecCheckLinkerFlags("test", "test", f); err != nil {
				t.Errorf("unexpected error for %q: %v", f, err)
			}
		}
		for _, f := range [][]string{
			{"-isysroot", script},
			{"-Wl,-syslibroot," + script},
			{"-Wl,-syslibroot=" + script},
			{"-Wl,--push-state,-syslibroot=" + script},
		} {
			if err := cgoSecCheckLinkerFlags("test", "test", f); err == nil {
				t.Errorf("missing error for %q", f)
			}
		}
	})
}

func TestCgoSecCheckGCCSpecFiles(t *testing.T) {
	cgoSecTestRun(t, func() {
		// Compiler invocations always pass -c, which never evaluates GCC's
		// *link_command spec (%:include(libsanitizer.spec) / %:include(libgomp.spec)).
		cwdSpecDir := t.TempDir()
		if err := os.WriteFile(filepath.Join(cwdSpecDir, "libsanitizer.spec"), []byte("*link_libubsan:\n"), 0644); err != nil {
			t.Fatal(err)
		}
		t.Chdir(cwdSpecDir)
		for _, f := range [][]string{
			{"-fsanitize=memory"},
			{"-fsanitize=undefined"},
			{"-fopenmp"},
		} {
			if err := cgoSecCheckCompilerFlags("test", "test", f); err != nil {
				t.Errorf("unexpected error for compiler flag %q with libsanitizer.spec in CWD: %v", f, err)
			}
		}

		// Verify -L, -isysroot, and -Wl,-syslibroot reject directories containing
		// GCC spec files (directly, in subdirectories, or in multilib sibling dirs).
		specDir := t.TempDir()
		if err := os.WriteFile(filepath.Join(specDir, "libgomp.spec"), []byte("*link_gomp: %{!static:}\n"), 0644); err != nil {
			t.Fatal(err)
		}
		for _, f := range [][]string{
			{"-L" + specDir, "-fopenmp"},
			{"-L", specDir, "-fopenmp"},
			{"-L=" + specDir, "-fopenmp"},
			{"-L$SYSROOT" + specDir, "-fopenmp"},
			{"-isysroot", specDir, "-fopenmp"},
			{"-Wl,-syslibroot," + specDir, "-fopenmp"},
			{"-Wl,--push-state,-syslibroot=" + specDir, "-fopenmp"},
		} {
			if err := cgoSecCheckLinkerFlags("test", "test", f); err == nil {
				t.Errorf("expected error for GCC spec file in %q, got nil", f)
			}
		}

		// Subdirectory spec file inside sysroot or -L directory.
		nestedRoot := t.TempDir()
		nestedLib := filepath.Join(nestedRoot, "usr", "lib")
		if err := os.MkdirAll(nestedLib, 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(filepath.Join(nestedLib, "libsanitizer.spec"), []byte("*link_libasan: -lasan\n"), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("test", "test", []string{"-L" + nestedRoot, "-fsanitize=address"}); err == nil {
			t.Errorf("expected error for nested spec file in -L directory, got nil")
		}

		// Multilib sibling directory (../lib64/libgomp.spec relative to -L<dir>/sub,
		// both when <dir>/sub exists and when <dir>/nonexistent does not exist).
		multiRoot := t.TempDir()
		subDir := filepath.Join(multiRoot, "sub")
		lib64Dir := filepath.Join(multiRoot, "lib64")
		if err := os.MkdirAll(subDir, 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.MkdirAll(lib64Dir, 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(filepath.Join(lib64Dir, "libgomp.spec"), []byte("*link_gomp:\n"), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("test", "test", []string{"-L" + subDir, "-fopenmp"}); err == nil {
			t.Errorf("expected error for multilib sibling spec file in ../lib64, got nil")
		}
		if err := cgoSecCheckLinkerFlags("test", "test", []string{"-L" + filepath.Join(multiRoot, "nonexistent"), "-fopenmp"}); err == nil {
			t.Errorf("expected error for multilib sibling spec file via nonexistent child dir, got nil")
		}

		// Symlinked directory pointing to a directory with a spec file, and
		// subdirectory symlink inside a -L directory (N-2).
		linkParent := t.TempDir()
		symDir := filepath.Join(linkParent, "symlink-lib")
		if err := os.Symlink(specDir, symDir); err == nil {
			if err := cgoSecCheckLinkerFlags("test", "test", []string{"-L" + symDir, "-fopenmp"}); err == nil {
				t.Errorf("expected error for symlinked -L directory containing spec file, got nil")
			}
			if err := cgoSecCheckLinkerFlags("test", "test", []string{"-L" + linkParent, "-fopenmp"}); err == nil {
				t.Errorf("expected error for -L directory with subdirectory symlink to spec file, got nil")
			}
		}
		// Symlinked directory pointing to a child directory whose real parent has ../lib64/libgomp.spec.
		symSub := filepath.Join(linkParent, "symlink-sub")
		if err := os.Symlink(subDir, symSub); err == nil {
			if err := cgoSecCheckLinkerFlags("test", "test", []string{"-L" + symSub, "-fopenmp"}); err == nil {
				t.Errorf("expected error for symlinked -L directory with multilib sibling spec file, got nil")
			}
		}

		// CWD fallback when -fsanitize= is combined with -static/-static-lib* or
		// when -fopenmp is used without -L (N-3), while preserving standalone
		// -fsanitize=memory, -fsanitize=address, -fno-openmp, -fopenmp-simd, and -static.
		t.Chdir(specDir)
		for _, f := range [][]string{
			{"-fsanitize=memory"},
			{"-fsanitize=address"},
			{"-fsanitize=undefined"},
			{"-fno-openmp"},
			{"-fopenmp-simd"},
			{"-static"},
		} {
			if err := cgoSecCheckLinkerFlags("test", "test", f); err != nil {
				t.Errorf("unexpected error for safe flag %q with spec file in CWD: %v", f, err)
			}
		}
		for _, f := range [][]string{
			{"-fsanitize=undefined", "-static-libubsan"},
			{"-fsanitize=address", "-static"},
			{"-fopenmp"},
		} {
			if err := cgoSecCheckLinkerFlags("test", "test", f); err == nil {
				t.Errorf("expected error for %q with spec file in CWD and no -L, got nil", f)
			}
		}

		// CWD/package-dir spec checks must not walk ../lib* multilib siblings
		// (e.g. ./../lib -> host /lib when CWD is a depth-1 directory).
		cwdParent := t.TempDir()
		cwdChild := filepath.Join(cwdParent, "app")
		cwdSiblingLib := filepath.Join(cwdParent, "lib")
		if err := os.MkdirAll(cwdChild, 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.MkdirAll(cwdSiblingLib, 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(filepath.Join(cwdSiblingLib, "libgomp.spec"), []byte("*link_gomp: -lgomp\n"), 0644); err != nil {
			t.Fatal(err)
		}
		t.Chdir(cwdChild)
		if err := cgoSecCheckLinkerFlags("test", "test", []string{"-fopenmp"}); err != nil {
			t.Errorf("unexpected error for CWD check with ../lib/libgomp.spec sibling: %v", err)
		}

		// Host default library trees (/lib*, /usr/lib*) must be skipped when passed
		// without a sysroot prefix (e.g. -L/usr/lib or -L/usr/local/../lib), while
		// sysroot-prefixed -L= paths are still scanned.
		if cgoSecGoos != "windows" {
			for _, sysFlag := range [][]string{
				{"-L/usr/lib", "-fopenmp"},
				{"-L/usr/lib64", "-fopenmp"},
			} {
				if err := cgoSecCheckLinkerFlags("test", "test", sysFlag); err != nil {
					t.Errorf("unexpected error for host system %q: %v", sysFlag, err)
				}
			}
		}
	})
}

func TestCgoSecCheckLinkerWlFlags(t *testing.T) {
	cgoSecTestRun(t, func() {
		dir := t.TempDir()
		script := filepath.Join(dir, "script.ld")
		if err := os.WriteFile(script, []byte("PLUGIN(\"/tmp/pwn.so\")\n"), 0644); err != nil {
			t.Fatal(err)
		}
		elfObj := filepath.Join(dir, "valid.o")
		if err := os.WriteFile(elfObj, cgoSecMakeTestCOFFSection(".text", []byte{0x90}), 0644); err != nil {
			t.Fatal(err)
		}
		for _, f := range [][]string{
			{"-Wl,-rpath", "-Wl,foo"},
			{"-Wl,-undefined", "-Wl,dynamic_lookup"},
			{"-Wl,--just-symbols", "-Wl," + elfObj},
			{"-Wl,--just-symbols=" + elfObj},
			{"-Wl,-R", dir},
			{"-Wl,-R," + dir},
			{"-Wl,-R=" + dir},
			{"-Wl,-R=" + elfObj},
		} {
			if err := cgoSecCheckLinkerFlags("test", "test", f); err != nil {
				t.Errorf("unexpected error for %q: %v", f, err)
			}
		}
		for _, f := range [][]string{
			{"-Wl,--just-symbols=" + script},
			{"-Wl,--just-symbols," + script},
			{"-Wl,--just-symbols", "-Wl," + script},
			{"-Wl,--push-state,--just-symbols=" + script},
			{"-Wl,-R," + script},
			{"-Wl,-R=" + script},
			{"-Wl,-R" + script},
			{"-Wl,-R", script},
			{"-Wl,-R", "-Wl," + script},
			{"-Wl,--push-state,-R" + script},
			{"-Wl,--push-state,-R=" + script},
			{"-Wl,-sectcreate," + script + ",__info_plist,foo"},
			{"-Wl,-sectcreate,__TEXT," + script + ",foo"},
		} {
			if err := cgoSecCheckLinkerFlags("test", "test", f); err == nil {
				t.Errorf("missing error for %q", f)
			}
		}
		if cgoSecGoos != "darwin" && cgoSecGoos != "ios" {
			if err := cgoSecCheckLinkerFlags("test", "test", []string{"-Wl,-sectcreate,__TEXT,__info_plist," + script}); err == nil {
				t.Errorf("missing error for -Wl,-sectcreate with existing file on %s", cgoSecGoos)
			}
		}
		if cgoSecGoos != "windows" {
			if err := os.WriteFile(filepath.Join(dir, "console"), []byte("OUTPUT(/tmp/pwn)\n"), 0644); err != nil {
				t.Fatal(err)
			}
			t.Chdir(dir)
			if err := cgoSecCheckLinkerFlags("test", "test", []string{"-Wl,-subsystem,console"}); err == nil {
				t.Errorf("missing error for -Wl,-subsystem,console with existing file in CWD on %s", cgoSecGoos)
			}
			if err := cgoSecCheckLinkerFlags("test", "test", []string{"-Wl,-subsystem,windows"}); err != nil {
				t.Errorf("unexpected error for -Wl,-subsystem,windows without matching file in CWD: %v", err)
			}
			if err := cgoSecCheckLinkerFlags("test", "test", []string{"-Wl,--subsystem,console"}); err != nil {
				t.Errorf("unexpected error for -Wl,--subsystem,console: %v", err)
			}
		}
	})
}

func TestCgoSecCheckPkgConfigFlags(t *testing.T) {
	cgoSecTestRun(t, func() {
		good := [][]string{
			{"--define-variable=prefix=/opt/mypkg/lib"},
			{"--prefix-variable=prefix"},
		}
		for _, f := range good {
			if err := cgoSecCheckPkgConfigFlags("test", "test", f); err != nil {
				t.Errorf("unexpected error for %q: %v", f, err)
			}
		}

		bad := [][]string{
			{"--define-variable=A=-b"},
			{"--define-variable=A=@b"},
			{"--define-variable=A=b-c"},
			{"--define-variable=A=b@c"},
			{"--define-variable=libdir==/tmp/sub"},
			{"--define-variable=libdir=$SYSROOT/tmp/sub"},
			{"--define-variable=A=b$c"},
			{"--define-variable=A="},
			{"--define-variable=1A=b"},
			{"--define-variable=A"},
			{"--log-file=/tmp/log"},
			{"--with-path=."},
			{"--with-path=/tmp"},
			{"--personality=triplet"},
			{"--personality=filename"},
		}
		for _, f := range bad {
			if err := cgoSecCheckPkgConfigFlags("test", "test", f); err == nil {
				t.Errorf("missing error for %q", f)
			}
		}
	})
}

func TestCgoSecCheckCompilerFlagsForInternalLink(t *testing.T) {
	cgoSecTestRun(t, func() {
		// Any "bad" compiler flag should trigger external linking.
		for _, f := range cgoSecBadCompilerFlags {
			if err := cgoSecCheckCompilerFlagsForInternalLink("test", "test", f); err == nil {
				t.Errorf("missing error for %q", f)
			}
		}

		// All "good" compiler flags should not trigger external linking,
		// except for anything that begins with "-flto".
		for _, f := range cgoSecGoodCompilerFlags {
			foundLTO := false
			for _, s := range f {
				if strings.Contains(s, "-flto") {
					foundLTO = true
				}
			}
			if err := cgoSecCheckCompilerFlagsForInternalLink("test", "test", f); err != nil {
				// expect error for -flto
				if !foundLTO {
					t.Errorf("unexpected error for %q: %v", f, err)
				}
			} else {
				// expect no error for everything else
				if foundLTO {
					t.Errorf("missing error for %q: %v", f, err)
				}
			}
		}
	})
}

func cgoSecMakeTestCOFFSection(secName string, dataBytes []byte) []byte {
	hdrAndSec := 20 + 40
	buf := make([]byte, hdrAndSec+len(dataBytes))
	binary.LittleEndian.PutUint16(buf[0:2], 0x8664) // IMAGE_FILE_MACHINE_AMD64
	binary.LittleEndian.PutUint16(buf[2:4], 1)      // NumberOfSections = 1
	copy(buf[20:28], secName)
	binary.LittleEndian.PutUint32(buf[20+16:20+20], uint32(len(dataBytes))) // SizeOfRawData
	binary.LittleEndian.PutUint32(buf[20+20:20+24], uint32(hdrAndSec))      // PointerToRawData
	copy(buf[hdrAndSec:], dataBytes)
	return buf
}

func cgoSecMakeTestCOFFLongSection(longName string) []byte {
	strTab := append(make([]byte, 4), append([]byte(longName), 0)...)
	binary.LittleEndian.PutUint32(strTab[0:4], uint32(len(strTab)))
	buf := make([]byte, 20+40+len(strTab))
	binary.LittleEndian.PutUint16(buf[0:2], 0x8664)
	binary.LittleEndian.PutUint16(buf[2:4], 1)
	binary.LittleEndian.PutUint32(buf[8:12], 60) // PointerToSymbolTable = 60
	binary.LittleEndian.PutUint32(buf[12:16], 0) // NumberOfSymbols = 0
	copy(buf[20:28], "/4")
	copy(buf[60:], strTab)
	return buf
}

func TestCgoSecCheckLinkerInputFiles(t *testing.T) {
	cgoSecTestRun(t, func() {
		dir := t.TempDir()

		// 1. GNU ld implicit linker script disguised as a direct .so input (A-4),
		// or passed via -Wl, flags ending in an object/library extension.
		evilSo := filepath.Join(dir, "evil.so")
		if err := os.WriteFile(evilSo, []byte("PLUGIN(\"/tmp/pwn.so\")\n"), 0644); err != nil {
			t.Fatal(err)
		}
		for _, f := range [][]string{
			{evilSo},
			{"-Wl,--just-symbols=" + evilSo},
			{"-Wl,--just-symbols," + evilSo},
			{"-Wl,-R," + evilSo},
			{"-Wl,-R" + evilSo},
			{"-Wl,-syslibroot=" + evilSo},
			{"-Wl,--push-state,-R," + evilSo},
		} {
			if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", f); err == nil {
				t.Errorf("expected error for GNU ld script disguised as .so in %q, got nil", f)
			}
		}
		t.Chdir(dir)
		for _, f := range [][]string{
			{"-Wl,-framework,evil.so"},
			{"-Wl,-undefined=evil.so"},
		} {
			if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", f); err == nil {
				t.Errorf("expected error for CWD file with .so suffix in %q, got nil", f)
			}
		}
		if cgoSecGoos != "darwin" && cgoSecGoos != "ios" {
			if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{"-Wl,-sectcreate,__TEXT,__text,evil.so"}); err == nil {
				t.Errorf("expected error for -Wl,-sectcreate with existing .so file on %s, got nil", cgoSecGoos)
			}
		}

		// 2. GNU ld implicit linker script named libc.so, foo.lib, or ..dll inside a -L directory (A-3c, N-2, N-5).
		libDir := filepath.Join(dir, "lib")
		if err := os.MkdirAll(libDir, 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(filepath.Join(libDir, "libc.so"), []byte("GROUP(\"/tmp/pwn.so\")\n"), 0644); err != nil {
			t.Fatal(err)
		}
		for _, f := range [][]string{
			{"-L" + libDir},
			{"-L", libDir},
			{"-L=" + libDir},
			{"-L$SYSROOT" + libDir},
		} {
			if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", f); err == nil {
				t.Errorf("expected error for -L directory %q containing linker script libc.so, got nil", f)
			}
		}
		// Subdirectory symlink inside -L directory pointing to a directory with a linker script (N-2).
		symSearchDir := t.TempDir()
		if err := os.Symlink(libDir, filepath.Join(symSearchDir, "sub")); err == nil {
			if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{"-L" + symSearchDir}); err == nil {
				t.Errorf("expected error for -L directory containing symlinked subdirectory with linker script, got nil")
			}
		}

		winLibDir := filepath.Join(dir, "winlib")
		if err := os.MkdirAll(winLibDir, 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(filepath.Join(winLibDir, "foo.lib"), []byte("INPUT(\"/tmp/pwn.dll\")\n"), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{"-L" + winLibDir}); err == nil {
			t.Errorf("expected error for -L directory containing linker script foo.lib, got nil")
		}
		dotDllDir := filepath.Join(dir, "dotdll")
		if err := os.MkdirAll(dotDllDir, 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(filepath.Join(dotDllDir, "..dll"), []byte("INPUT(\"/tmp/pwn.dll\")\n"), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{"-L" + dotDllDir}); err == nil {
			t.Errorf("expected error for -L directory containing linker script ..dll, got nil")
		}

		// 3. ar archive containing a PE/COFF member with dangerous .drectve,
		// including N-6 bypass variants (BSD #1/<len> __.SYMDEF.o prefix, >64-byte
		// #1/<len> name, non-first SVR4 __.SYMDEF member, and truncated !<arch>\n).
		peBuf := cgoSecMakeTestCOFFSection(".drectve", []byte(`--plugin /tmp/pwn.so`))
		arPath := filepath.Join(dir, "libevil.a")
		memberSize := fmt.Sprintf("%-10d", len(peBuf))
		arHdr := []byte("!<arch>\nevil.o          0           0     0     644     " + memberSize + "`\n")
		if err := os.WriteFile(arPath, append(arHdr, peBuf...), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{arPath}); err == nil {
			t.Errorf("expected error for archive containing dangerous .drectve member, got nil")
		}
		if err := cgoSecCheckLinkerInputFile(".syso", arPath); err == nil {
			t.Errorf("expected error for archive used as .syso, got nil")
		}

		// N-6a: BSD #1/12 member named "__.SYMDEF.o\x00".
		bsdSymdefPrefix := filepath.Join(dir, "libbsd_symdef_prefix.a")
		bsdName := []byte("__.SYMDEF.o\x00")
		bsdSize := fmt.Sprintf("%-10d", len(bsdName)+len(peBuf))
		bsdHdr := []byte("!<arch>\n#1/12           0           0     0     644     " + bsdSize + "`\n")
		if err := os.WriteFile(bsdSymdefPrefix, append(append(bsdHdr, bsdName...), peBuf...), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{bsdSymdefPrefix}); err == nil {
			t.Errorf("expected error for BSD archive member named __.SYMDEF.o, got nil")
		}

		// N-6b: BSD #1/68 member named "__.SYMDEF" + 55*'A' + ".o\x00\x00" (>64 bytes).
		bsdLongName := append([]byte("__.SYMDEF"+strings.Repeat("A", 55)+".o"), 0, 0)
		bsdLongPath := filepath.Join(dir, "libbsd_long.a")
		bsdLongSize := fmt.Sprintf("%-10d", len(bsdLongName)+len(peBuf))
		bsdLongHdr := []byte(fmt.Sprintf("!<arch>\n#1/%-13d0           0     0     644     %s`\n", len(bsdLongName), bsdLongSize))
		if err := os.WriteFile(bsdLongPath, append(append(bsdLongHdr, bsdLongName...), peBuf...), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{bsdLongPath}); err == nil {
			t.Errorf("expected error for BSD archive member with >64-byte __.SYMDEF* name, got nil")
		}

		// N-6c: Non-first archive member named "__.SYMDEF" containing dangerous COFF object.
		svr4SecondSymdef := filepath.Join(dir, "libsvr4_second.a")
		firstHdr := []byte("!<arch>\n/               0           0     0     0       4         `\n\x00\x00\x00\x00")
		secondHdr := []byte("__.SYMDEF       0           0     0     644     " + memberSize + "`\n")
		if err := os.WriteFile(svr4SecondSymdef, append(append(firstHdr, secondHdr...), peBuf...), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{svr4SecondSymdef}); err == nil {
			t.Errorf("expected error for second archive member named __.SYMDEF, got nil")
		}

		// N-6d: Truncated archive ("!<arch>\nPLUGIN(...)" < 68 bytes).
		truncAr := filepath.Join(dir, "libtrunc.a")
		if err := os.WriteFile(truncAr, []byte("!<arch>\nPLUGIN(\"/tmp/pwn.so\")\n"), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{truncAr}); err == nil {
			t.Errorf("expected error for truncated !<arch> file, got nil")
		}
	})
}

func TestCgoSecCheckLinkerCOFFDrectve(t *testing.T) {
	cgoSecTestRun(t, func() {
		dir := t.TempDir()

		// 1. PE/COFF object with safe .drectve directives should be allowed.
		safeObj := filepath.Join(dir, "safe_drectve.o")
		if err := os.WriteFile(safeObj, cgoSecMakeTestCOFFSection(".drectve", []byte(`/DEFAULTLIB:msvcrt /EXPORT:MyFunc`)), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{safeObj}); err != nil {
			t.Errorf("unexpected error for PE/COFF with safe .drectve: %v", err)
		}

		safeMinGW := filepath.Join(dir, "safe_mingw.o")
		if err := os.WriteFile(safeMinGW, cgoSecMakeTestCOFFSection(".drectve", []byte("\xef\xbb\xbf"+`-lkernel32 --enable-auto-import "/DEFAULTLIB:uuid.lib" /MANIFESTDEPENDENCY:"type='win32' name='Microsoft.Windows.Common-Controls'"`+"\x00\x00"+`-export:Bar /CETCOMPAT /GUARD:EHCONT --exclude-all-symbols`)), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{safeMinGW}); err != nil {
			t.Errorf("unexpected error for PE/COFF with safe MinGW/MSVC .drectve: %v", err)
		}

		// 2. PE/COFF object with dangerous .drectve content must be rejected.
		drectveObj := filepath.Join(dir, "drectve.o")
		if err := os.WriteFile(drectveObj, cgoSecMakeTestCOFFSection(".drectve", []byte(`--plugin /tmp/pwn.so`)), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{drectveObj}); err == nil {
			t.Errorf("expected error for PE/COFF object with dangerous .drectve, got nil")
		}

		evilDrectve := filepath.Join(dir, "evil_drectve.o")
		if err := os.WriteFile(evilDrectve, cgoSecMakeTestCOFFSection(".drectve", []byte(`-L/tmp/evil -T/tmp/evil.ld`)), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{evilDrectve}); err == nil {
			t.Errorf("expected error for PE/COFF object with -L/-T .drectve, got nil")
		}

		driveRelDrectve := filepath.Join(dir, "drive_rel_drectve.o")
		if err := os.WriteFile(driveRelDrectve, cgoSecMakeTestCOFFSection(".drectve", []byte(`/DEFAULTLIB:C:evil.lib`)), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{driveRelDrectve}); err == nil {
			t.Errorf("expected error for PE/COFF object with drive-relative /DEFAULTLIB:C:evil.lib, got nil")
		}

		// 3. PE/COFF object with a valid long DWARF section name (/4 -> .debug_info) in string table.
		dwarfObj := filepath.Join(dir, "dwarf.o")
		if err := os.WriteFile(dwarfObj, cgoSecMakeTestCOFFLongSection(".debug_info"), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{dwarfObj}); err != nil {
			t.Errorf("unexpected error for PE/COFF object with /4 .debug_info section: %v", err)
		}
	})
}

func TestCgoSecCheckLinkerObjectSections(t *testing.T) {
	cgoSecTestRun(t, func() {
		dir := t.TempDir()

		// 1. PE/COFF object with GCC/LLVM LTO sections (.llvmbc or /4 -> .gnu.lto_.symtab.1234) must be rejected.
		coffLtoShort := filepath.Join(dir, "coff_lto_short.o")
		if err := os.WriteFile(coffLtoShort, cgoSecMakeTestCOFFSection(".llvmbc", []byte("BC\xc0\xde")), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{coffLtoShort}); err == nil {
			t.Errorf("expected error for PE/COFF object with .llvmbc section, got nil")
		}
		coffLtoLong := filepath.Join(dir, "coff_lto_long.o")
		if err := os.WriteFile(coffLtoLong, cgoSecMakeTestCOFFLongSection(".gnu.lto_.symtab.12345678"), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{coffLtoLong}); err == nil {
			t.Errorf("expected error for PE/COFF object with /4 .gnu.lto_.symtab.* section, got nil")
		}

		// 2. Mach-O 64-bit object containing LC_LINKER_OPTION (0x2d) (B-1 macOS):
		// dangerous/malformed options must be rejected, while safe -l and -framework options are allowed.
		makeMachOLinkerOpt := func(count uint32, payload []byte) []byte {
			cmdSize := 12 + len(payload)
			if cmdSize%8 != 0 {
				cmdSize += 8 - (cmdSize % 8)
			}
			buf := make([]byte, 32+cmdSize)
			binary.LittleEndian.PutUint32(buf[0:4], 0xfeedfacf) // MH_MAGIC_64
			binary.LittleEndian.PutUint32(buf[12:16], 1)        // MH_OBJECT
			binary.LittleEndian.PutUint32(buf[16:20], 1)        // ncmds = 1
			binary.LittleEndian.PutUint32(buf[20:24], uint32(cmdSize))
			binary.LittleEndian.PutUint32(buf[32:36], 0x2d) // LC_LINKER_OPTION
			binary.LittleEndian.PutUint32(buf[36:40], uint32(cmdSize))
			binary.LittleEndian.PutUint32(buf[40:44], count)
			copy(buf[44:], payload)
			return buf
		}
		for _, badOpt := range [][]byte{
			makeMachOLinkerOpt(0, nil),
			makeMachOLinkerOpt(2, []byte("-lto_library\x00/tmp/pwn.dylib\x00")),
			makeMachOLinkerOpt(1, []byte("-l:evil.ld\x00")),
			makeMachOLinkerOpt(2, []byte("-framework\x00/tmp/pwn.ld\x00")),
		} {
			machoObj := filepath.Join(dir, "macho_opt.o")
			if err := os.WriteFile(machoObj, badOpt, 0644); err != nil {
				t.Fatal(err)
			}
			if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{machoObj}); err == nil {
				t.Errorf("expected error for Mach-O object with disallowed LC_LINKER_OPTION, got nil")
			}
		}
		for _, goodOpt := range [][]byte{
			makeMachOLinkerOpt(1, []byte("-lSystem\x00")),
			makeMachOLinkerOpt(2, []byte("-framework\x00CoreFoundation\x00")),
		} {
			machoGood := filepath.Join(dir, "macho_good_opt.o")
			if err := os.WriteFile(machoGood, goodOpt, 0644); err != nil {
				t.Fatal(err)
			}
			if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{machoGood}); err != nil {
				t.Errorf("unexpected error for Mach-O object with safe LC_LINKER_OPTION: %v", err)
			}
		}

		// 3. ELF 64-bit relocatable object containing GCC/LLVM LTO sections (.gnu.lto_.opts, SHT_LLVM_LTO)
		// must be rejected, while normal ELF objects are allowed.
		makeELF64 := func(eType uint16, secName string, shType uint32) []byte {
			shstrtab := append([]byte{0}, append([]byte(secName), 0)...)
			shOff := 64 + len(shstrtab)
			if shOff%8 != 0 {
				shOff += 8 - (shOff % 8)
			}
			buf := make([]byte, shOff+3*64)
			copy(buf[0:4], "\x7fELF")
			buf[4] = 2                                       // ELFCLASS64
			buf[5] = 1                                       // ELFDATA2LSB
			buf[6] = 1                                       // EV_CURRENT
			binary.LittleEndian.PutUint16(buf[16:18], eType) // e_type
			binary.LittleEndian.PutUint16(buf[18:20], 0x3e)  // EM_X86_64
			binary.LittleEndian.PutUint32(buf[20:24], 1)     // EV_CURRENT
			binary.LittleEndian.PutUint64(buf[40:48], uint64(shOff))
			binary.LittleEndian.PutUint16(buf[52:54], 64) // e_ehsize
			binary.LittleEndian.PutUint16(buf[58:60], 64) // e_shentsize
			binary.LittleEndian.PutUint16(buf[60:62], 3)  // e_shnum (NULL, target sec, .shstrtab)
			binary.LittleEndian.PutUint16(buf[62:64], 2)  // e_shstrndx = 2
			copy(buf[64:], shstrtab)
			// Section 1: target section
			sec1 := buf[shOff+64 : shOff+128]
			binary.LittleEndian.PutUint32(sec1[0:4], 1)      // sh_name = 1
			binary.LittleEndian.PutUint32(sec1[4:8], shType) // sh_type
			// Section 2: .shstrtab
			sec2 := buf[shOff+128 : shOff+192]
			binary.LittleEndian.PutUint32(sec2[4:8], 3)                       // SHT_STRTAB
			binary.LittleEndian.PutUint64(sec2[24:32], 64)                    // sh_offset = 64
			binary.LittleEndian.PutUint64(sec2[32:40], uint64(len(shstrtab))) // sh_size
			return buf
		}

		normalELF := filepath.Join(dir, "normal.o")
		if err := os.WriteFile(normalELF, makeELF64(1, ".text", 1), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{normalELF}); err != nil {
			t.Errorf("unexpected error for normal ELF64 .o: %v", err)
		}

		gccLtoELF := filepath.Join(dir, "gcc_lto.o")
		if err := os.WriteFile(gccLtoELF, makeELF64(1, ".gnu.lto_.opts", 1), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{gccLtoELF}); err == nil {
			t.Errorf("expected error for ELF64 object with .gnu.lto_.opts section, got nil")
		}
		if err := cgoSecCheckLinkerInputFile(".syso", gccLtoELF); err == nil {
			t.Errorf("expected error for ELF64 .syso with .gnu.lto_.opts section, got nil")
		}

		llvmLtoELF := filepath.Join(dir, "llvm_lto.o")
		if err := os.WriteFile(llvmLtoELF, makeELF64(1, ".text", 0x6fff4c0c), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{llvmLtoELF}); err == nil {
			t.Errorf("expected error for ELF64 object with SHT_LLVM_LTO section, got nil")
		}

		llvmLinkerOptsELF := filepath.Join(dir, "llvm_linker_opts.o")
		if err := os.WriteFile(llvmLinkerOptsELF, makeELF64(1, ".linker-options", 0x6fff4c01), 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{llvmLinkerOptsELF}); err == nil {
			t.Errorf("expected error for ELF64 object with SHT_LLVM_LINKER_OPTIONS section, got nil")
		}

		// Polyglot linker script starting with "MZ=0;/*..." and fake PE header at offset 0x80.
		mzScript := make([]byte, 256)
		copy(mzScript, "MZ=0; PLUGIN(\"/tmp/pwn.so\") /*")
		binary.LittleEndian.PutUint32(mzScript[0x3c:0x40], 0x80)
		copy(mzScript[0x80:], "PE\x00\x00")
		binary.LittleEndian.PutUint16(mzScript[0x84:0x86], 0x8664)
		mzScriptPath := filepath.Join(dir, "mz_script.dll")
		if err := os.WriteFile(mzScriptPath, mzScript, 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{mzScriptPath}); err == nil {
			t.Errorf("expected error for MZ polyglot linker script, got nil")
		}

		// AIX big archive ("<bigaf>\n") containing an invalid/script member.
		bigAr := make([]byte, 128+112+4+2+16)
		copy(bigAr[0:8], "<bigaf>\n")
		copy(bigAr[68:88], fmt.Sprintf("%-20d", 128))
		copy(bigAr[88:108], fmt.Sprintf("%-20d", 128))
		copy(bigAr[128+0:128+20], fmt.Sprintf("%-20d", 16))
		copy(bigAr[128+108:128+112], fmt.Sprintf("%-4d", 4))
		copy(bigAr[128+112:128+116], "a.o\x00")
		copy(bigAr[128+116:128+118], "`\n")
		copy(bigAr[128+118:], "INPUT(/tmp/pwn)\n")
		bigArPath := filepath.Join(dir, "libbig.a")
		if err := os.WriteFile(bigArPath, bigAr, 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{bigArPath}); err == nil {
			t.Errorf("expected error for AIX big archive with linker script member, got nil")
		}

		// ASCII polyglot linker script starting with COFF LOONGARCH64 magic ("db") and >=256 sections must fail.
		coffPoly := make([]byte, 20+0x3020*40)
		copy(coffPoly, "db /*               ")
		coffPolyPath := filepath.Join(dir, "coff_poly.o")
		if err := os.WriteFile(coffPolyPath, coffPoly, 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{coffPolyPath}); err == nil {
			t.Errorf("expected error for ASCII COFF polyglot linker script, got nil")
		}

		// GNU ar archive whose "//" long-filename table starts with "db" (0x6264 == LOONGARCH64) must pass,
		// while a binary COFF object disguised as "//" must fail.
		arMember := func(name string, body []byte) []byte {
			hdr := fmt.Sprintf("%-16s%-12d%-6d%-6d%-8s%-10d`\n", name, 0, 0, 0, "644", len(body))
			out := append([]byte(hdr), body...)
			if len(out)%2 != 0 {
				out = append(out, '\n')
			}
			return out
		}
		validMemberObj := cgoSecMakeTestCOFFSection(".text", []byte{0xc3})
		gnuStrTab := []byte("db_impl_write.cc.o/\n")
		gnuArBytes := append([]byte("!<arch>\n"), arMember("//", gnuStrTab)...)
		gnuArBytes = append(gnuArBytes, arMember("/0", validMemberObj)...)
		gnuArPath := filepath.Join(dir, "librocksdb_like.a")
		if err := os.WriteFile(gnuArPath, gnuArBytes, 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{gnuArPath}); err != nil {
			t.Errorf("unexpected error for GNU archive with 'db*' long-name string table: %v", err)
		}

		disguisedStrTabAr := append([]byte("!<arch>\n"), arMember("//", cgoSecMakeTestCOFFSection(".drectve", []byte("--plugin /tmp/pwn.so")))...)
		disguisedPath := filepath.Join(dir, "libdisguised_strtab.a")
		if err := os.WriteFile(disguisedPath, disguisedStrTabAr, 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{disguisedPath}); err == nil {
			t.Errorf("expected error for COFF object disguised as '//' archive string table, got nil")
		}

		// Windows PE/COFF import library (.dll.a / .lib): dual "/" linker members + "//" at index 2 +
		// short import member (IMPORT_OBJECT_HEADER: 00 00 FF FF Version=0) must pass.
		impPayload := []byte("SymName\x00foo.dll\x00")
		impObj := make([]byte, 20+len(impPayload))
		binary.LittleEndian.PutUint16(impObj[0:2], 0x0000)
		binary.LittleEndian.PutUint16(impObj[2:4], 0xffff)
		binary.LittleEndian.PutUint16(impObj[4:6], 0)
		binary.LittleEndian.PutUint16(impObj[6:8], 0x8664)
		binary.LittleEndian.PutUint32(impObj[12:16], uint32(len(impPayload)))
		binary.LittleEndian.PutUint16(impObj[18:20], 0x0004) // IMPORT_CODE | IMPORT_NAME
		copy(impObj[20:], impPayload)

		firstSlash := make([]byte, 4) // 0 symbols (BE)
		secondSlash := make([]byte, 12)
		binary.LittleEndian.PutUint32(secondSlash[0:4], 1) // 1 member offset
		binary.LittleEndian.PutUint32(secondSlash[8:12], 0)
		peStrTab := []byte("long_import_member.dll\x00")
		impLibBytes := []byte("!<arch>\n")
		impLibBytes = append(impLibBytes, arMember("/", firstSlash)...)
		impLibBytes = append(impLibBytes, arMember("/", secondSlash)...)
		impLibBytes = append(impLibBytes, arMember("//", peStrTab)...)
		impLibBytes = append(impLibBytes, arMember("/0", impObj)...)
		impLibPath := filepath.Join(dir, "foo.dll.a")
		if err := os.WriteFile(impLibPath, impLibBytes, 0644); err != nil {
			t.Fatal(err)
		}
		if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{impLibPath}); err != nil {
			t.Errorf("unexpected error for valid PE import library (.dll.a): %v", err)
		}
		if err := cgoSecCheckLinkerInputFile(".syso", impLibPath); err == nil {
			t.Errorf("expected error for import archive used as .syso, got nil")
		}

		// Host default library directory (-L/usr/lib) must not fail on GNU ld stub scripts (libc.so, libm.so).
		if cgoSecGoos != "windows" {
			if err := cgoSecCheckLinkerFlags("LDFLAGS", "#cgo LDFLAGS", []string{"-L/usr/lib"}); err != nil {
				t.Errorf("unexpected error for host -L/usr/lib: %v", err)
			}
		}
	})
}

// cmd/cgo's //go:cgo_ldflag record carries pkg-config --libs output beside the package's #cgo LDFLAGS, and the capture
// and side link take their -L directories from it. A record -L directory that is not the environment's is scanned as
// cmd/go 1.26.11 scans a linker search directory, so a GNU ld script posing as a library there is refused by the route
// itself, not only by the loader (P3's SHOULD 4 on 4b, 2026-11-28). The offline conversion runs no go list -compiled.
func TestCgoTheRouteScansTheRecordsLinkerDirectories(t *testing.T) {
	files := cgoFixtureFiles(t, cgoPhase3FixtureDir)
	evil := filepath.Join(t.TempDir(), "evil")

	if err := os.MkdirAll(evil, 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(evil, "libevil.a"), []byte("/* a GNU ld script, not an archive */\nINPUT(-lc)\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	files["cgo_gotypes.go"] += "\n//go:cgo_ldflag \"-L" + filepath.ToSlash(evil) + "\"\n"
	stderr, output, err := convertCgoOffline(t, files)

	if err == nil || !strings.Contains(stderr, "invalid linker input file in //go:cgo_ldflag") {
		t.Errorf("the route did not refuse the record's -L directory (%v):\n%s", err, lastLines(stderr, 12))
	}

	requireNothingWritten(t, output, err, stderr)
}

// The port is held to the pinned GOROOT's cmd/go/internal/work/security.go, declaration by declaration (P3's SHOULD 2 on
// 4b, 2026-11-28): each of Go's top-level declarations, renamed and with cmd/go's state substituted as the port does it,
// must print exactly as the port's declaration of that name prints, comments aside. The one known hunk, the XCOFF
// statement go2cs refuses where cmd/go reads the object, is left out of both. A Go patch release that changes a rule
// turns this red until the port is re-taken.
func TestCgoFlagSecurityMatchesThePinnedGoFile(t *testing.T) {
	out, err := exec.Command("go", "env", "GOROOT").Output()

	if err != nil {
		t.Fatalf("go env GOROOT: %v", err)
	}

	goFile := filepath.Join(strings.TrimSpace(string(out)), "src", "cmd", "go", "internal", "work", "security.go")
	fset := token.NewFileSet()
	upstream, err := parser.ParseFile(fset, goFile, nil, 0)

	if err != nil {
		t.Fatalf("the pinned GOROOT's security.go: %v", err)
	}

	port, err := parser.ParseFile(fset, "cgoFlagSecurity.go", nil, 0)

	if err != nil {
		t.Fatal(err)
	}

	// The port's renames: every top-level name of Go's file takes the cgoSec prefix.
	renames := map[string]string{}

	for _, decl := range upstream.Decls {
		for _, name := range cgoSecDeclNames(decl) {
			renames[name] = "cgoSec" + strings.ToUpper(name[:1]) + name[1:]
		}
	}

	state := map[string]string{"cfg.Getenv": "cgoSecGetenv", "cfg.Goos": "cgoSecGoos", "cfg.GOMODCACHE": "cgoSecModCache",
		"base.Cwd": "cgoSecCwd", "str.HasFilePathPrefix": "cgoSecHasFilePathPrefix", "load.SafeArg": "cgoSecSafeArg",
		"lazyregexp.New": "regexp.MustCompile", "lazyregexp.Regexp": "regexp.Regexp"}

	transform := func(node ast.Node) ast.Node {
		return astutil.Apply(node, func(c *astutil.Cursor) bool {
			switch n := c.Node().(type) {
			case *ast.CallExpr:
				// base.Cwd() is a value in the port.
				if sel, ok := n.Fun.(*ast.SelectorExpr); ok && cgoSecSelector(sel) == "base.Cwd" && len(n.Args) == 0 {
					c.Replace(ast.NewIdent("cgoSecCwd"))
					return false
				}
			case *ast.SelectorExpr:
				if to, ok := state[cgoSecSelector(n)]; ok {
					c.Replace(cgoSecExpr(to))
					return false
				}
			case *ast.Ident:
				if to, ok := renames[n.Name]; ok {
					n.Name = to
				}
			}

			return true
		}, nil)
	}

	printNode := func(node ast.Node) string {
		var buf bytes.Buffer

		if err := format.Node(&buf, token.NewFileSet(), cgoSecWithoutXCOFF(node)); err != nil {
			t.Fatal(err)
		}

		return buf.String()
	}

	ported := map[string]ast.Node{}

	for _, decl := range port.Decls {
		for _, name := range cgoSecDeclNames(decl) {
			ported[name] = cgoSecDeclNode(decl, name)
		}
	}

	checked := 0

	for _, decl := range upstream.Decls {
		for _, name := range cgoSecDeclNames(decl) {
			want := printNode(transform(cgoSecDeclNode(decl, name)))
			mine, found := ported[renames[name]]

			if !found {
				t.Errorf("the port has no %s (Go's %s)", renames[name], name)
				continue
			}

			if got := printNode(mine); got != want {
				t.Errorf("the port's %s differs from Go's %s at the pin:\n--- Go, transformed ---\n%s\n--- port ---\n%s", renames[name], name, want, got)
			}

			checked++
		}
	}

	if checked < 40 {
		t.Errorf("only %d declarations compared; the pinned file holds more", checked)
	}
}

// cgoSecDeclNames names a top-level declaration: a function (a method as Type.Method) or each spec of a var, const or
// type declaration.
func cgoSecDeclNames(decl ast.Decl) []string {
	switch d := decl.(type) {
	case *ast.FuncDecl:
		if d.Recv != nil && len(d.Recv.List) == 1 {
			if ident, ok := d.Recv.List[0].Type.(*ast.Ident); ok {
				return []string{ident.Name + "." + d.Name.Name}
			}
		}

		return []string{d.Name.Name}
	case *ast.GenDecl:
		var names []string

		for _, spec := range d.Specs {
			switch s := spec.(type) {
			case *ast.ValueSpec:
				for _, name := range s.Names {
					names = append(names, name.Name)
				}
			case *ast.TypeSpec:
				names = append(names, s.Name.Name)
			}
		}

		return names
	}

	return nil
}

// cgoSecDeclNode is the node a name declares: the function, or the one spec, without its comments.
func cgoSecDeclNode(decl ast.Decl, name string) ast.Node {
	switch d := decl.(type) {
	case *ast.FuncDecl:
		clone := *d
		clone.Doc = nil
		return &clone
	case *ast.GenDecl:
		for _, spec := range d.Specs {
			for _, n := range cgoSecDeclNames(&ast.GenDecl{Tok: d.Tok, Specs: []ast.Spec{spec}}) {
				if n == name {
					return &ast.GenDecl{Tok: d.Tok, Specs: []ast.Spec{spec}}
				}
			}
		}
	}

	return nil
}

// cgoSecWithoutXCOFF drops the one statement the port is known to differ in: the XCOFF check (an if whose condition names
// U802TOCMAGIC, or the port's 0x01DF).
func cgoSecWithoutXCOFF(node ast.Node) ast.Node {
	return astutil.Apply(node, func(c *astutil.Cursor) bool {
		if stmt, ok := c.Node().(*ast.IfStmt); ok && c.Index() >= 0 {
			text := ""

			ast.Inspect(stmt.Cond, func(n ast.Node) bool {
				switch v := n.(type) {
				case *ast.Ident:
					text += v.Name + " "
				case *ast.BasicLit:
					text += v.Value + " "
				}

				return true
			})

			if strings.Contains(text, "U802TOCMAGIC") || strings.Contains(text, "0x01DF") {
				c.Delete()
				return false
			}
		}

		return true
	}, nil)
}

// cgoSecSelector spells a pkg.Name selector, or "" for any other.
func cgoSecSelector(sel *ast.SelectorExpr) string {
	if x, ok := sel.X.(*ast.Ident); ok {
		return x.Name + "." + sel.Sel.Name
	}

	return ""
}

// cgoSecExpr is the expression a dotted or bare name spells.
func cgoSecExpr(name string) ast.Expr {
	if pkg, member, ok := strings.Cut(name, "."); ok {
		return &ast.SelectorExpr{X: ast.NewIdent(pkg), Sel: ast.NewIdent(member)}
	}

	return ast.NewIdent(name)
}
