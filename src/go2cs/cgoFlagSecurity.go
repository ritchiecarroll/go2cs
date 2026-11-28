// cgoFlagSecurity.go - Gbtc
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
// - The whole file below cgoSecWith: DERIVED FROM src/cmd/go/internal/work/security.go, Go 1.26.11, every rule and check
//   as there, its top-level names prefixed cgoSec. cmd/go state comes from the conversion's load through cgoSecWith,
//   and an XCOFF linker input is refused, where cmd/go reads it with internal/xcoff.
//   cgoSecCheckPkgConfigFlags and cgoSecCheckCompilerFlagsForInternalLink are carried whole and unused in production: go2cs
//   never runs pkg-config (its #cgo pkg-config names need no check here) and has no internal link. Their ported tests run.
// - cgoSecHasFilePathPrefix: DERIVED FROM src/cmd/go/internal/str/path.go, Go 1.26.11, func HasFilePathPrefix.
// - cgoSecSafeArg: DERIVED FROM src/cmd/go/internal/load/pkg.go, Go 1.26.11, func SafeArg.
//
// cgoLinkDirsAllowed's record scan is stricter than cmd/go in two ways, neither able to accept a directory cmd/go
// refuses: a relative environment -L directory resolves against the load's directory (the record resolves one against
// the package's), and no CGO_LDFLAGS_ALLOW applies to the scan.

package main

import (
	"bytes"
	"debug/elf"
	"debug/macho"
	"debug/pe"
	"debug/plan9obj"
	"encoding/binary"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"regexp"
	"slices"
	"strconv"
	"strings"
	"sync"
	"unicode"
	"unicode/utf8"
)

// cgo flag security (COORD's ruling, 2026-11-28): a package's #cgo flags reach the C compiler and linker in a -cgo
// conversion's side build and library capture, so they are held to cmd/go's own allowlist at the pinned GOROOT, as
// cmd/go holds them at build time. cmd/go's comment on the allowlist follows.
//
// Checking of compiler and linker flags.
// We must avoid flags like -fplugin=, which can allow
// arbitrary code execution during the build.
// Do not make changes here without carefully
// considering the implications.
// (That's why the code is isolated in a file named security.go.)
//
// Note that -Wl,foo means split foo on commas and pass to
// the linker, so that -Wl,-foo,bar means pass -foo bar to
// the linker. Similarly -Wa,foo for the assembler and so on.
// If any of these are permitted, the wildcard portion must
// disallow commas.
//
// Note also that GNU binutils accept any argument @foo
// as meaning "read more flags from the file foo", so we must
// guard against any command-line argument beginning with @,
// even things like "-I @foo".
// We use load.SafeArg (which is even more conservative)
// to reject these.
//
// Even worse, gcc -I@foo (one arg) turns into cc1 -I @foo (two args),
// so although gcc doesn't expand the @foo, cc1 will.
// So out of paranoia, we reject @ at the beginning of every
// flag argument that might be split into its own argument.

// The state cmd/go's checks read from its own configuration, set from the conversion's load for one check at a time.
var (
	cgoSecMutex    sync.Mutex
	cgoSecGetenv   = func(string) string { return "" }
	cgoSecGoos     string
	cgoSecModCache string
	cgoSecCwd      string
)

// cgoSecWith runs check with cmd/go's state taken from the load: getenv for CGO_*_ALLOW and CGO_*_DISALLOW, the target
// GOOS, the module cache and the working directory cmd/go would run in.
func cgoSecWith(getenv func(string) string, goos, modCache, cwd string, check func() error) error {
	cgoSecMutex.Lock()
	defer cgoSecMutex.Unlock()

	cgoSecGetenv, cgoSecGoos, cgoSecModCache, cgoSecCwd = getenv, goos, modCache, cwd

	defer func() {
		cgoSecGetenv, cgoSecGoos, cgoSecModCache, cgoSecCwd = func(string) string { return "" }, "", "", ""
	}()

	return check()
}

// cgoFlagsAllowed holds a package's own #cgo flags to cmd/go's allowlist (the checks below), as cmd/go's buildFlags
// does at build time (work/exec.go, CFlags): CPPFLAGS, CFLAGS, CXXFLAGS and FFLAGS through checkCompilerFlags, and
// LDFLAGS through checkLinkerFlags, each named "#cgo <NAME>". The environment's CGO_<NAME> flags are cmd/go's defaults
// and are exempt, as there. CGO_<NAME>_ALLOW and _DISALLOW are read through go env in the load's environment, so a go
// env -w setting counts, as it does for cmd/go. cmd/go's working directory is the load's.
//
// listed is the route's ONE listing of the package (cgoSideList: by import path, in the load's directory, the context
// cmd/go builds it in), the same one the side build compiles with, so the flags checked are the flags compiled (P3's
// MUST on 4b, 2026-11-28). A second listing from the package's own directory could answer differently (a dependency
// is its own main module there, so its go.mod picks the toolchain), and fails outright for a package with no go.mod
// above it.
func cgoFlagsAllowed(load cgoLoad, listed cgoSideListing, goos string) error {
	checks := []struct {
		name   string
		list   []string
		linker bool
	}{
		{"CPPFLAGS", listed.CgoCPPFLAGS, false},
		{"CFLAGS", listed.CgoCFLAGS, false},
		{"CXXFLAGS", listed.CgoCXXFLAGS, false},
		{"FFLAGS", listed.CgoFFLAGS, false},
		{"LDFLAGS", listed.CgoLDFLAGS, true},
	}

	var keys []string

	for _, check := range checks {
		keys = append(keys, "CGO_"+check.name+"_ALLOW", "CGO_"+check.name+"_DISALLOW")
	}

	// GOMODCACHE, which is always set, goes last: cgoGoEnv trims trailing blank lines, so unset keys at the end would
	// read as missing values.
	keys = append(keys, "GOMODCACHE")

	values, err := cgoGoEnv(load, keys...)

	if err != nil {
		return err
	}

	env := map[string]string{}

	for i, key := range keys {
		env[key] = values[i]
	}

	return cgoSecWith(func(key string) string { return env[key] }, goos, env["GOMODCACHE"], load.dir, func() error {
		for _, check := range checks {
			verify := cgoSecCheckCompilerFlags

			if check.linker {
				verify = cgoSecCheckLinkerFlags
			}

			if err := verify(check.name, "#cgo "+check.name, check.list); err != nil {
				return fmt.Errorf("cgo: %w", err)
			}
		}

		return nil
	})
}

// cgoLinkDirsAllowed scans the -L directories of cmd/cgo's //go:cgo_ldflag record that did not come from the
// environment, with cmd/go's 1.26.11 linker search-directory scan (checkLinkerLibDir: no GCC .spec file, and no
// "library" that is a GNU ld script or an object carrying linker options or LTO sections). The record holds the
// environment's CGO_LDFLAGS (exempt, as in cmd/go), the package's #cgo LDFLAGS (which cgoFlagsAllowed checks), and
// pkg-config --libs output, which cmd/go checks as "pkg-config --libs" before cmd/cgo runs and which go2cs never
// produces itself. Scanning every directory not from the environment covers that last share without resting on the
// loader (P3's SHOULD 4 on 4b, 2026-11-28).
func cgoLinkDirsAllowed(load cgoLoad, flags cgoLinkFlags, goos string) error {
	values, err := cgoGoEnv(load, "CGO_LDFLAGS", "GOMODCACHE")

	if err != nil {
		return err
	}

	fromEnv := map[string]bool{}

	if fields, err := cgoQuotedSplit(values[0]); err == nil {
		for i := 0; i < len(fields); i++ {
			dir, found := strings.CutPrefix(fields[i], "-L")

			if found && dir == "" && i+1 < len(fields) {
				i++
				dir = fields[i]
			}

			if !found || dir == "" {
				continue
			}

			if !filepath.IsAbs(dir) {
				dir = filepath.Join(load.dir, dir)
			}

			fromEnv[filepath.Clean(dir)] = true
		}
	}

	return cgoSecWith(func(string) string { return "" }, goos, values[1], load.dir, func() error {
		for _, dir := range flags.dirs {
			if fromEnv[filepath.Clean(dir)] {
				continue
			}

			if err := cgoSecCheckLinkerLibDir("//go:cgo_ldflag", dir); err != nil {
				return fmt.Errorf("cgo: %w", err)
			}
		}

		return nil
	})
}

// cgoSecSafeArg reports whether name is safe to pass on a command line: it does not begin with a character a tool
// could read as a flag or a response file (@).
func cgoSecSafeArg(name string) bool {
	if name == "" {
		return false
	}

	c := name[0]

	return '0' <= c && c <= '9' || 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || c == '.' || c == '_' || c == '/' || c >= utf8.RuneSelf
}

// cgoSecHasFilePathPrefix reports whether the filesystem path s
// begins with the elements in prefix.
//
// cgoSecHasFilePathPrefix is case-sensitive (except for volume names) even if the
// filesystem is not, does not apply Unicode normalization even if the
// filesystem does, and assumes that all path separators are canonicalized to
// filepath.Separator (as returned by filepath.Clean).
func cgoSecHasFilePathPrefix(s, prefix string) bool {
	sv := filepath.VolumeName(s)
	pv := filepath.VolumeName(prefix)

	// Strip the volume from both paths before canonicalizing sv and pv:
	// it's unlikely that strings.ToUpper will change the length of the string,
	// but doesn't seem impossible.
	s = s[len(sv):]
	prefix = prefix[len(pv):]

	// Always treat Windows volume names as case-insensitive, even though
	// we don't treat the rest of the path as such.
	//
	// TODO(bcmills): Why do we care about case only for the volume name? It's
	// been this way since https://go.dev/cl/11316, but I don't understand why
	// that problem doesn't apply to case differences in the entire path.
	if sv != pv {
		sv = strings.ToUpper(sv)
		pv = strings.ToUpper(pv)
	}

	switch {
	default:
		return false
	case sv != pv:
		return false
	case len(s) == len(prefix):
		return s == prefix
	case prefix == "":
		return true
	case len(s) > len(prefix):
		if prefix[len(prefix)-1] == filepath.Separator {
			return strings.HasPrefix(s, prefix)
		}
		return s[len(prefix)] == filepath.Separator && s[:len(prefix)] == prefix
	}
}

var cgoSecRe = regexp.MustCompile

var cgoSecValidCompilerFlags = []*regexp.Regexp{
	cgoSecRe(`-D([A-Za-z_][A-Za-z0-9_]*)(=[^@\-]*)?`),
	cgoSecRe(`-U([A-Za-z_][A-Za-z0-9_]*)`),
	cgoSecRe(`-F([^@\-].*)`),
	cgoSecRe(`-I([^@\-].*)`),
	cgoSecRe(`-O`),
	cgoSecRe(`-O([^@\-].*)`),
	cgoSecRe(`-W`),
	cgoSecRe(`-W([^@,]+)`), // -Wall but not -Wa,-foo.
	cgoSecRe(`-Wa,-mbig-obj`),
	cgoSecRe(`-Wp,-D([A-Za-z_][A-Za-z0-9_]*)(=[^@,\-]*)?`),
	cgoSecRe(`-Wp,-U([A-Za-z_][A-Za-z0-9_]*)`),
	cgoSecRe(`-ansi`),
	cgoSecRe(`-f(no-)?asynchronous-unwind-tables`),
	cgoSecRe(`-f(no-)?blocks`),
	cgoSecRe(`-f(no-)builtin-[a-zA-Z0-9_]*`),
	cgoSecRe(`-f(no-)?common`),
	cgoSecRe(`-f(no-)?constant-cfstrings`),
	cgoSecRe(`-fdebug-prefix-map=([^@]+)=([^@]+)`),
	cgoSecRe(`-fdiagnostics-show-note-include-stack`),
	cgoSecRe(`-ffile-prefix-map=([^@]+)=([^@]+)`),
	cgoSecRe(`-fno-canonical-system-headers`),
	cgoSecRe(`-f(no-)?eliminate-unused-debug-types`),
	cgoSecRe(`-f(no-)?exceptions`),
	cgoSecRe(`-f(no-)?fast-math`),
	cgoSecRe(`-f(no-)?inline-functions`),
	cgoSecRe(`-finput-charset=([^@\-].*)`),
	cgoSecRe(`-f(no-)?fat-lto-objects`),
	cgoSecRe(`-f(no-)?keep-inline-dllexport`),
	cgoSecRe(`-f(no-)?lto`),
	cgoSecRe(`-fmacro-backtrace-limit=(.+)`),
	cgoSecRe(`-fmessage-length=(.+)`),
	cgoSecRe(`-f(no-)?modules`),
	cgoSecRe(`-f(no-)?objc-arc`),
	cgoSecRe(`-f(no-)?objc-nonfragile-abi`),
	cgoSecRe(`-f(no-)?objc-legacy-dispatch`),
	cgoSecRe(`-f(no-)?omit-frame-pointer`),
	cgoSecRe(`-f(no-)?openmp(-simd)?`),
	cgoSecRe(`-f(no-)?permissive`),
	cgoSecRe(`-f(no-)?(pic|PIC|pie|PIE)`),
	cgoSecRe(`-f(no-)?plt`),
	cgoSecRe(`-f(no-)?rtti`),
	cgoSecRe(`-f(no-)?split-stack`),
	cgoSecRe(`-f(no-)?stack-(.+)`),
	cgoSecRe(`-f(no-)?strict-aliasing`),
	cgoSecRe(`-f(un)signed-char`),
	cgoSecRe(`-f(no-)?use-linker-plugin`), // safe if -B is not used; we don't permit -B
	cgoSecRe(`-f(no-)?visibility-inlines-hidden`),
	cgoSecRe(`-fsanitize=([^@\-].*)`),
	cgoSecRe(`-fsanitize-undefined-strip-path-components=(-)?[0-9]+`),
	cgoSecRe(`-ftemplate-depth-(.+)`),
	cgoSecRe(`-ftls-model=(global-dynamic|local-dynamic|initial-exec|local-exec)`),
	cgoSecRe(`-fvisibility=(.+)`),
	cgoSecRe(`-g([^@\-].*)?`),
	cgoSecRe(`-m32`),
	cgoSecRe(`-m64`),
	cgoSecRe(`-m(abi|arch|cpu|fpu|simd|tls-dialect|tune)=([^@\-].*)`),
	cgoSecRe(`-m(no-)?v?aes`),
	cgoSecRe(`-marm`),
	cgoSecRe(`-mcmodel=[0-9a-z-]+`),
	cgoSecRe(`-mfloat-abi=([^@\-].*)`),
	cgoSecRe(`-m(soft|single|double)-float`),
	cgoSecRe(`-mfpmath=[0-9a-z,+]*`),
	cgoSecRe(`-m(no-)?avx[0-9a-z.]*`),
	cgoSecRe(`-m(no-)?ms-bitfields`),
	cgoSecRe(`-m(no-)?stack-(.+)`),
	cgoSecRe(`-mmacosx-(.+)`),
	cgoSecRe(`-m(no-)?relax`),
	cgoSecRe(`-m(no-)?strict-align`),
	cgoSecRe(`-m(no-)?(lsx|lasx|frecipe|div32|lam-bh|lamcas|ld-seq-sa)`),
	cgoSecRe(`-mios-simulator-version-min=(.+)`),
	cgoSecRe(`-miphoneos-version-min=(.+)`),
	cgoSecRe(`-mlarge-data-threshold=[0-9]+`),
	cgoSecRe(`-mtvos-simulator-version-min=(.+)`),
	cgoSecRe(`-mtvos-version-min=(.+)`),
	cgoSecRe(`-mwatchos-simulator-version-min=(.+)`),
	cgoSecRe(`-mwatchos-version-min=(.+)`),
	cgoSecRe(`-mnop-fun-dllimport`),
	cgoSecRe(`-m(no-)?sse[0-9.]*`),
	cgoSecRe(`-m(no-)?ssse3`),
	cgoSecRe(`-mthumb(-interwork)?`),
	cgoSecRe(`-mthreads`),
	cgoSecRe(`-mwindows`),
	cgoSecRe(`-no-canonical-prefixes`),
	cgoSecRe(`--param=ssp-buffer-size=[0-9]*`),
	cgoSecRe(`-pedantic(-errors)?`),
	cgoSecRe(`-pipe`),
	cgoSecRe(`-pthread`),
	cgoSecRe(`--static`),
	cgoSecRe(`-?-std=([^@\-].*)`),
	cgoSecRe(`-?-stdlib=([^@\-].*)`),
	cgoSecRe(`-w`),
	cgoSecRe(`-x(c|c-header|cpp-output|c\+\+|c\+\+-header|c\+\+-cpp-output|objective-c|objective-c-header|objective-c-cpp-output|objective-c\+\+|objective-c\+\+-header|objective-c\+\+-cpp-output|assembler|assembler-with-cpp)`),
	cgoSecRe(`-v`),
}

var cgoSecValidCompilerXLang = cgoSecRe(`^(c|c-header|cpp-output|c\+\+|c\+\+-header|c\+\+-cpp-output|objective-c|objective-c-header|objective-c-cpp-output|objective-c\+\+|objective-c\+\+-header|objective-c\+\+-cpp-output|assembler|assembler-with-cpp)$`)

var cgoSecValidCompilerFlagsWithNextArg = []string{
	"-arch",
	"-D",
	"-U",
	"-I",
	"-F",
	"-framework",
	"-include",
	"-isysroot",
	"-isystem",
	"-x",
}

var cgoSecInvalidCompilerFlags = []*regexp.Regexp{
	// Clang's -gen-cdb-fragment-path takes a directory argument and writes
	// compilation database fragments there; have to exclude separately because
	// -g* is allowed in general. GCC's -gnatO similarly takes a separate
	// output filename argument in the GCC driver.
	cgoSecRe(`-gen-.*`),
	cgoSecRe(`-gnat.*`),
}

var cgoSecInvalidLinkerFlags = []*regexp.Regexp{
	// On macOS this means the linker loads and executes the next argument.
	// Have to exclude separately because -lfoo is allowed in general.
	cgoSecRe(`-lto_library`),
	cgoSecRe(`-lazy_library`),
	cgoSecRe(`-lazy_framework`),
	cgoSecRe(`-load_hidden`),
	// GNU gold and LLD accept single-dash -library and -library-path.
	cgoSecRe(`-library(-path)?(=.*)?`),
	cgoSecRe(`-gen-.*`),
	cgoSecRe(`-gnat.*`),
	// In lld, --warn-backrefs-exclude takes a separate <glob> argument when not
	// joined with '='; exclude the bare form because -Wl,--(no-)?warn-([^,]+) is
	// allowed in general.
	cgoSecRe(`-Wl,--warn-backrefs-exclude`),
}

var cgoSecValidLinkerFlags = []*regexp.Regexp{
	cgoSecRe(`-F([^@\-].*)`),
	cgoSecRe(`-l([^@\-:=/\\][^:=/\\]*)`),
	cgoSecRe(`-L([^@\-].*)`),
	cgoSecRe(`-O`),
	cgoSecRe(`-O([^@\-].*)`),
	cgoSecRe(`-f(no-)?(pic|PIC|pie|PIE)`),
	cgoSecRe(`-f(no-)?openmp(-simd)?`),
	cgoSecRe(`-fsanitize=([^@\-].*)`),
	cgoSecRe(`-flat_namespace`),
	cgoSecRe(`-g([^@\-].*)?`),
	cgoSecRe(`-headerpad_max_install_names`),
	cgoSecRe(`-m(abi|arch|cpu|fpu|simd|tls-dialect|tune)=([^@\-].*)`),
	cgoSecRe(`-mcmodel=[0-9a-z-]+`),
	cgoSecRe(`-mfloat-abi=([^@\-].*)`),
	cgoSecRe(`-m(soft|single|double)-float`),
	cgoSecRe(`-m(no-)?relax`),
	cgoSecRe(`-m(no-)?strict-align`),
	cgoSecRe(`-m(no-)?(lsx|lasx|frecipe|div32|lam-bh|lamcas|ld-seq-sa)`),
	cgoSecRe(`-mmacosx-(.+)`),
	cgoSecRe(`-mios-simulator-version-min=(.+)`),
	cgoSecRe(`-miphoneos-version-min=(.+)`),
	cgoSecRe(`-mthreads`),
	cgoSecRe(`-mwindows`),
	cgoSecRe(`-(pic|PIC|pie|PIE)`),
	cgoSecRe(`-pthread`),
	cgoSecRe(`-rdynamic`),
	cgoSecRe(`-shared`),
	cgoSecRe(`-?-static([-a-z0-9+]*)`),
	cgoSecRe(`-?-stdlib=([^@\-].*)`),
	cgoSecRe(`-v`),

	// Note that any wildcards in -Wl need to exclude comma,
	// since -Wl splits its argument at commas and passes
	// them all to the linker uninterpreted. Allowing comma
	// in a wildcard would allow tunneling arbitrary additional
	// linker arguments through one of these.
	cgoSecRe(`-Wl,--(no-)?allow-multiple-definition`),
	cgoSecRe(`-Wl,--(no-)?allow-shlib-undefined`),
	cgoSecRe(`-Wl,--(no-)?as-needed`),
	cgoSecRe(`-Wl,-Bdynamic`),
	cgoSecRe(`-Wl,-berok`),
	cgoSecRe(`-Wl,-Bstatic`),
	cgoSecRe(`-Wl,-Bsymbolic-functions`),
	cgoSecRe(`-Wl,-O[0-9]+`),
	cgoSecRe(`-Wl,-d[ny]`),
	cgoSecRe(`-Wl,--disable-new-dtags`),
	cgoSecRe(`-Wl,-e[=,][a-zA-Z0-9]+`),
	cgoSecRe(`-Wl,--enable-new-dtags`),
	cgoSecRe(`-Wl,--end-group`),
	cgoSecRe(`-Wl,--(no-)?export-dynamic`),
	cgoSecRe(`-Wl,-E`),
	cgoSecRe(`-Wl,-framework,[^,@\-][^,]*`),
	cgoSecRe(`-Wl,--hash-style=(sysv|gnu|both)`),
	cgoSecRe(`-Wl,-headerpad_max_install_names`),
	cgoSecRe(`-Wl,--no-undefined`),
	cgoSecRe(`-Wl,--pop-state`),
	cgoSecRe(`-Wl,--push-state`),
	cgoSecRe(`-Wl,-R[,=]?([^@\-,=][^,@]*$)`),
	cgoSecRe(`-Wl,--just-symbols[=,]([^,@\-][^,@]*)`),
	cgoSecRe(`-Wl,-rpath(-link)?[=,]([^,@\-][^,]*)`),
	cgoSecRe(`-Wl,-s`),
	cgoSecRe(`-Wl,-search_paths_first`),
	cgoSecRe(`-Wl,-sectcreate,([^,@\-][^,]*),([^,@\-][^,]*),([^,@\-][^,]*)`),
	cgoSecRe(`-Wl,--start-group`),
	cgoSecRe(`-Wl,-?-static`),
	cgoSecRe(`-Wl,-?-subsystem,(native|windows|console|posix|xbox)`),
	cgoSecRe(`-Wl,-syslibroot[=,]([^,@\-][^,]*)`),
	cgoSecRe(`-Wl,-undefined[=,]([^,@\-][^,]*)`),
	cgoSecRe(`-Wl,-?-unresolved-symbols=[^,]+`),
	cgoSecRe(`-Wl,--(no-)?warn-([^,]+)`),
	cgoSecRe(`-Wl,-?-wrap[=,][^,@\-][^,]*`),
	cgoSecRe(`-Wl(,-z,(relro|now|(no)?execstack))+`),

	cgoSecRe(`[a-zA-Z0-9_/].*\.(a|o|obj|dll|dylib|lib|so|tbd)`), // direct linker inputs: x.o or libfoo.so (but not -foo.o or @foo.o)
	cgoSecRe(`\./.*\.(a|o|obj|dll|dylib|lib|so|tbd)`),
}

var cgoSecValidLinkerFlagsWithNextArg = []string{
	"-arch",
	"-F",
	"-l",
	"-L",
	"-framework",
	"-isysroot",
	"-Wl,-framework",
	"-Wl,-rpath",
	"-Wl,-R",
	"-Wl,--just-symbols",
	"-Wl,-undefined",
}

// cgoSecRecombinedLinkerFlagsWithNextArg lists flags in cgoSecValidLinkerFlagsWithNextArg
// that GCC and Clang recombine with their following argument (e.g., "-l" "foo"
// becomes "-lfoo"), which must also be checked against cgoSecInvalidLinkerFlags.
var cgoSecRecombinedLinkerFlagsWithNextArg = []string{
	"-l",
}

var cgoSecValidPkgConfigFlags = []*regexp.Regexp{
	cgoSecRe(`--atleast-pkgconfig-version=\d+\.\d+\.\d+`),
	cgoSecRe(`--atleast-version=\d+\.\d+\.\d+`),
	cgoSecRe(`--cflags-only-I`),
	cgoSecRe(`--cflags`),
	cgoSecRe(`--define-prefix`),
	cgoSecRe(`--define-variable=[A-Za-z_][A-Za-z0-9_]*=[^@\-=$][^@\-$]*`),
	cgoSecRe(`--digraph`),
	cgoSecRe(`--dont-define-prefix`),
	cgoSecRe(`--dont-relocate-paths`),
	cgoSecRe(`--dump-personality`),
	cgoSecRe(`--env-only`),
	cgoSecRe(`--errors-to-stdout`),
	cgoSecRe(`--exact-version=\d+\.\d+\.\d+`),
	cgoSecRe(`--exists`),
	cgoSecRe(`--fragment-filter=[A-Za-z_][a-zA-Z0-9_]*`),
	cgoSecRe(`--ignore-conflicts`),
	cgoSecRe(`--internal-cflags`),
	cgoSecRe(`--keep-system-cflags`),
	cgoSecRe(`--keep-system-libs`),
	cgoSecRe(`--libs-only-l`),
	cgoSecRe(`--libs-only-L`),
	cgoSecRe(`--libs`),
	cgoSecRe(`--list-all`),
	cgoSecRe(`--list-package-names`),
	cgoSecRe(`--max-version=\d+\.\d+\.\d+`),
	cgoSecRe(`--maximum-traverse-depth=[0-9]+`),
	cgoSecRe(`--modversion`),
	cgoSecRe(`--msvc-syntax`),
	cgoSecRe(`--no-cache`),
	cgoSecRe(`--no-provides`),
	cgoSecRe(`--no-uninstalled`),
	cgoSecRe(`--path`),
	cgoSecRe(`--prefix-variable=[A-Za-z_][a-zA-Z0-9_]*`),
	cgoSecRe(`--print-errors`),
	cgoSecRe(`--print-provides`),
	cgoSecRe(`--print-requires-private`),
	cgoSecRe(`--print-requires`),
	cgoSecRe(`--print-variables`),
	cgoSecRe(`--pure`),
	cgoSecRe(`--shared`),
	cgoSecRe(`--short-errors`),
	cgoSecRe(`--silence-errors`),
	cgoSecRe(`--simulate`),
	cgoSecRe(`--static`),
	cgoSecRe(`--uninstalled`),
	cgoSecRe(`--validate`),
	cgoSecRe(`--variable=[A-Za-z_][a-zA-Z0-9_]*`),
}

// cgoSecHasGCCSpecFlags reports whether list contains linker flags that cause GCC's
// built-in *link_command spec to evaluate %:include(libgomp.spec) (-fopenmp or
// -fopenacc) or %:include(libsanitizer.spec) (-fsanitize=* combined with
// -static or -static-lib*).
func cgoSecHasGCCSpecFlags(list []string) bool {
	hasSanitize := false
	hasStatic := false
	for _, arg := range list {
		if arg == "-fopenmp" || arg == "-fopenacc" {
			return true
		}
		if strings.HasPrefix(arg, "-fsanitize=") {
			hasSanitize = true
		}
		if arg == "-static" || arg == "--static" || strings.HasPrefix(arg, "-static-") || strings.HasPrefix(arg, "--static-") {
			hasStatic = true
		}
	}
	return hasSanitize && hasStatic
}

func cgoSecCheckCompilerFlags(name, source string, list []string) error {
	checkOverrides := true
	return cgoSecCheckFlags(name, source, list, cgoSecInvalidCompilerFlags, cgoSecValidCompilerFlags, nil, cgoSecValidCompilerFlagsWithNextArg, checkOverrides)
}

func cgoSecCheckLinkerFlags(name, source string, list []string) error {
	checkOverrides := true
	if err := cgoSecCheckFlags(name, source, list, cgoSecInvalidLinkerFlags, cgoSecValidLinkerFlags, cgoSecRecombinedLinkerFlagsWithNextArg, cgoSecValidLinkerFlagsWithNextArg, checkOverrides); err != nil {
		return err
	}
	if cgoSecHasGCCSpecFlags(list) {
		if err := cgoSecCheckLinkerSpecDir(source, "."); err != nil {
			return err
		}
	}
	for i := 0; i < len(list); i++ {
		arg := list[i]
		switch {
		case strings.HasPrefix(arg, "-L"):
			dir := arg[2:]
			if dir == "" && i+1 < len(list) {
				i++
				dir = list[i]
			}
			if dir != "" {
				if err := cgoSecCheckLinkerLibDir(source, dir); err != nil {
					return err
				}
			}
		// Although -isysroot is a preprocessor/compiler option in GCC, clang on
		// Darwin also accepts it as a linker flag (forwarding it as -syslibroot)
		// and we include it in cgoSecValidLinkerFlagsWithNextArg on all platforms.
		case arg == "-isysroot" && i+1 < len(list):
			i++ // consume next argument
			if err := cgoSecCheckLinkerSysroot(source, arg+" "+list[i], list[i]); err != nil {
				return err
			}
		case arg == "-framework" && i+1 < len(list):
			i++
			if err := cgoSecCheckLinkerNonPathNonFile(source, arg+" "+list[i], list[i]); err != nil {
				return err
			}
		case strings.HasPrefix(arg, "-Wl,"):
			raw := arg
			warg := arg
			if slices.Contains(cgoSecValidLinkerFlagsWithNextArg, arg) && i+1 < len(list) {
				i++
				raw = arg + " " + list[i]
				warg = arg + "," + strings.TrimPrefix(list[i], "-Wl,")
			}
			if err := cgoSecCheckLinkerWlFlag(source, raw, warg); err != nil {
				return err
			}
		case cgoSecReDirectLinkerInput.MatchString(arg) && cgoSecIsLocalLinkerPath(arg):
			if err := cgoSecCheckLinkerInputFile(source, arg); err != nil {
				return err
			}
		}
	}
	return nil
}

// cgoSecCheckLinkerSysroot verifies that the target of -isysroot or -Wl,-syslibroot
// does not name a regular file on disk, which GNU ld.bfd would treat as a
// positional linker script.
func cgoSecCheckLinkerSysroot(source, flag, dir string) error {
	if fi, err := os.Stat(dir); err == nil && !fi.IsDir() {
		return fmt.Errorf("invalid flag in %s: %s (not a directory)", source, flag)
	}
	return cgoSecCheckLinkerSpecDir(source, dir)
}

// cgoSecCheckLinkerNonPathNonFile verifies that val (such as a framework or symbol
// name) is not a path and does not name a regular file in the current working
// directory. If a Darwin-only or symbol flag is misinterpreted by GNU ld.bfd
// (for example, -framework <val> parsed as -f ramework <val>, or -undefined
// <val> parsed as -u ndefined <val>), ld.bfd treats <val> as a positional
// input file and will execute it as a linker script if it is a regular file.
// Directories (such as "." in existing tests or subdirectories in the working
// directory that happen to share a framework or symbol name) are allowed
// because ld.bfd only parses regular files as linker scripts.
func cgoSecCheckLinkerNonPathNonFile(source, raw, val string) error {
	if strings.ContainsAny(val, `/\`) {
		return fmt.Errorf("invalid flag in %s: %s (see https://go.dev/s/invalidflag)", source, raw)
	}
	if fi, err := os.Stat(val); err == nil && !fi.IsDir() {
		return fmt.Errorf("invalid flag in %s: %s (see https://go.dev/s/invalidflag)", source, raw)
	}
	return nil
}

func cgoSecCheckLinkerObjOrDir(source, raw, val string) error {
	if !cgoSecIsLocalLinkerPath(val) {
		return nil
	}
	if fi, err := os.Stat(val); err == nil && !fi.IsDir() {
		if err := cgoSecCheckLinkerInputFile(source, val); err != nil {
			return fmt.Errorf("invalid flag in %s: %s (%v)", source, raw, err)
		}
	}
	return nil
}

func cgoSecCheckLinkerWlFlag(source, raw, arg string) error {
	wargs := []string{arg}
	if strings.HasPrefix(arg, "-Wl,--push-state,") {
		for _, sub := range strings.Split(arg, ",")[2:] {
			wargs = append(wargs, "-Wl,"+sub)
		}
	}
	for _, warg := range wargs {
		switch {
		case strings.HasPrefix(warg, "-Wl,-syslibroot,"), strings.HasPrefix(warg, "-Wl,-syslibroot="):
			dir := warg[len("-Wl,-syslibroot,"):]
			if err := cgoSecCheckLinkerSysroot(source, raw, dir); err != nil {
				return err
			}
		case strings.HasPrefix(warg, "-Wl,-framework,"):
			if err := cgoSecCheckLinkerNonPathNonFile(source, raw, warg[len("-Wl,-framework,"):]); err != nil {
				return err
			}
		case strings.HasPrefix(warg, "-Wl,-undefined,"), strings.HasPrefix(warg, "-Wl,-undefined="):
			if err := cgoSecCheckLinkerNonPathNonFile(source, raw, warg[len("-Wl,-undefined,"):]); err != nil {
				return err
			}
		case strings.HasPrefix(warg, "-Wl,--just-symbols,"), strings.HasPrefix(warg, "-Wl,--just-symbols="):
			if err := cgoSecCheckLinkerObjOrDir(source, raw, warg[len("-Wl,--just-symbols,"):]); err != nil {
				return err
			}
		case strings.HasPrefix(warg, "-Wl,-R"):
			val := warg[len("-Wl,-R"):]
			if after, ok := strings.CutPrefix(val, ","); ok {
				val = after
			} else if after, ok := strings.CutPrefix(val, "="); ok {
				val = after
			}
			if err := cgoSecCheckLinkerObjOrDir(source, raw, val); err != nil {
				return err
			}
		case strings.HasPrefix(warg, "-Wl,-subsystem,"):
			// On non-Windows targets, GNU ld.gold (and older non-PE ld.bfd) parses
			// single-dash -subsystem as -s -u bsystem and treats the following
			// comma-split argument as a positional input file / linker script.
			if cgoSecGoos != "windows" {
				if err := cgoSecCheckLinkerNonPathNonFile(source, raw, warg[len("-Wl,-subsystem,"):]); err != nil {
					return err
				}
			}
		case strings.HasPrefix(warg, "-Wl,-sectcreate,"):
			// -sectcreate <seg> <sect> <file> is an Apple linker flag whose
			// third argument is legitimately a regular file on Darwin/iOS.
			// On other platforms, GNU ld.bfd parses -sectcreate as -s -e ctcreate
			// and treats all three arguments as positional linker scripts; we still
			// allow non-existent file arguments on non-Darwin so cross-platform
			// flag validation tests continue to work.
			parts := strings.SplitN(warg[len("-Wl,-sectcreate,"):], ",", 3)
			if len(parts) == 3 {
				if err := cgoSecCheckLinkerNonPathNonFile(source, raw, parts[0]); err != nil {
					return err
				}
				if err := cgoSecCheckLinkerNonPathNonFile(source, raw, parts[1]); err != nil {
					return err
				}
				if cgoSecGoos != "darwin" && cgoSecGoos != "ios" {
					if fi, err := os.Stat(parts[2]); err == nil && !fi.IsDir() {
						return fmt.Errorf("invalid flag in %s: %s (see https://go.dev/s/invalidflag)", source, raw)
					}
				}
			}
		}
	}
	return nil
}

func cgoSecTrimLinkerSysrootPrefix(dir string) string {
	if strings.HasPrefix(dir, "=") {
		return dir[1:]
	}
	if strings.HasPrefix(dir, "$SYSROOT") {
		return dir[len("$SYSROOT"):]
	}
	return dir
}

var (
	cgoSecReDirectLinkerInput = cgoSecRe(`^(\./|[a-zA-Z0-9_/]).*\.(a|o|obj|dll|dylib|lib|so|tbd)$`)
	cgoSecReLinkerLibFile     = cgoSecRe(`(\.(a|o|obj|dll|dylib|lib|so|tbd)$|\.so\.)`)
)

// cgoSecIsLocalLinkerPath reports whether path is local to the build (a relative
// path, or an absolute path within the working directory, module cache, or
// temp/work directory, such as from ${SRCDIR} expansion). Non-local host
// paths are outside the cmd/go threat model.
func cgoSecIsLocalLinkerPath(path string) bool {
	if !filepath.IsAbs(path) {
		return true
	}
	clean := filepath.Clean(path)
	for _, root := range []string{cgoSecCwd, cgoSecModCache, os.TempDir()} {
		if root == "" {
			continue
		}
		if cgoSecHasFilePathPrefix(clean, filepath.Clean(root)) {
			return true
		}
		if realRoot, err := filepath.EvalSymlinks(root); err == nil {
			if realClean, err := filepath.EvalSymlinks(clean); err == nil && cgoSecHasFilePathPrefix(realClean, realRoot) {
				return true
			}
		}
	}
	return false
}

func cgoSecCheckLinkerSpecDir(source, dir string) error {
	return cgoSecScanLinkerSearchDir(source, dir, false)
}

func cgoSecCheckLinkerLibDir(source, dir string) error {
	return cgoSecScanLinkerSearchDir(source, dir, true)
}

// cgoSecScanLinkerSearchDir walks the directory rooted at dir and looks for .spec files,
// which can inject arbitrary linker flags, or cause execution of arbitrary
// commands. When checkLibs is true, it also validates library and object files in
// the directory via cgoSecCheckLinkerInputFile to reject GNU ld linker scripts and
// objects with embedded linker options or LTO sections.
//
// When GCC evaluates %:include(libgomp.spec) or %:include(libsanitizer.spec),
// which can be caused by -fopenmp, -fopenacc, or -fsanitize=..., it looks for the specific
// spec file in a number of places, including the current working directory and
// sysroot or -L directories. GCC uses a search strategy that looks for target-triple
// directories and GCC version directories inside of these directories, but since
// we don't want to hard code those values, we use a much less targeted approach,
// and just walk the directory looking for a spec file. In theory this can cause
// failure when we find a spec file that GCC wouldn't load, but it seems better to
// be safe here.
func cgoSecScanLinkerSearchDir(source, rawDir string, checkLibs bool) error {
	dir := cgoSecTrimLinkerSysrootPrefix(rawDir)
	if dir == "" || !cgoSecIsLocalLinkerPath(dir) {
		return nil
	}
	visited := make(map[string]bool)
	var checkTree func(root string) error
	checkTree = func(root string) error {
		if abs, err := filepath.Abs(root); err == nil {
			root = abs
		} else {
			root = filepath.Clean(root)
		}
		if !cgoSecIsLocalLinkerPath(root) {
			return nil
		}
		if resolved, err := filepath.EvalSymlinks(root); err == nil {
			root = resolved
		}
		if visited[root] || !cgoSecIsLocalLinkerPath(root) {
			return nil
		}
		visited[root] = true
		if fi, err := os.Stat(root); err != nil || !fi.IsDir() {
			return nil
		}
		return filepath.WalkDir(root, func(path string, d os.DirEntry, err error) error {
			if err != nil {
				return nil
			}
			if d.IsDir() {
				if path != root && !cgoSecIsLocalLinkerPath(path) {
					return filepath.SkipDir
				}
				return nil
			}
			if d.Type()&os.ModeSymlink != 0 {
				if fi, err := os.Stat(path); err == nil && fi.IsDir() {
					return checkTree(path)
				}
			}
			lower := strings.ToLower(d.Name())
			if lower == "specs" || strings.HasSuffix(lower, ".spec") {
				return fmt.Errorf("disallowed GCC spec file in %s: %s", source, path)
			}
			if checkLibs && cgoSecReLinkerLibFile.MatchString(lower) {
				if err := cgoSecCheckLinkerInputFile(source, path); err != nil {
					return err
				}
			}
			return nil
		})
	}
	if err := checkTree(dir); err != nil {
		return err
	}
	if !checkLibs {
		return nil
	}
	// GCC also searches multilib sibling directories (such as ../lib64)
	// relative to each -L directory. Because filepath.Join cleans ".."
	// lexically, if dir is a symlink its lexical parent and its resolved
	// parent on disk may differ; check both.
	absDir := filepath.Clean(dir)
	if abs, err := filepath.Abs(dir); err == nil {
		absDir = abs
	}
	bases := []string{absDir}
	if resolved, err := filepath.EvalSymlinks(absDir); err == nil {
		if abs, err := filepath.Abs(resolved); err == nil {
			resolved = abs
		}
		if resolved != absDir {
			bases = append(bases, resolved)
		}
	}
	for _, base := range bases {
		// Avoid walking host top-level /lib, /lib64, C:\lib, etc. when base is a
		// filesystem root or depth-1 directory such as /tmp, /app, or C:\app
		// (where filepath.Join(base, "..", "lib") would resolve to /lib or C:\lib).
		if parent := filepath.Dir(base); parent == base || filepath.Dir(parent) == parent {
			continue
		}
		for _, sub := range []string{"lib", "lib32", "lib64", "libx32"} {
			if err := checkTree(filepath.Join(base, "..", sub)); err != nil {
				return err
			}
		}
	}
	return nil
}

// cgoSecCheckLinkerInputFile verifies that an existing linker input or .syso file
// is a valid binary object/archive (or macOS .tbd stub) rather than a GNU ld
// linker script, and does not contain embedded linker flags (unallowlisted
// .drectve directives, LC_LINKER_OPTION, or LTO IR sections).
func cgoSecCheckLinkerInputFile(source, path string) error {
	f, err := os.Open(path)
	if err != nil {
		if os.IsNotExist(err) {
			return nil
		}
		return err
	}
	defer f.Close()

	fi, err := f.Stat()
	if err != nil {
		return err
	}

	if strings.HasSuffix(strings.ToLower(path), ".tbd") && source != ".syso" {
		var hdr [256]byte
		n, _ := f.Read(hdr[:])
		s := strings.TrimSpace(string(hdr[:n]))
		if !strings.HasPrefix(s, "---") && !strings.HasPrefix(s, "{") {
			return fmt.Errorf("invalid .tbd file in %s: %s", source, path)
		}
		return nil
	}

	return cgoSecCheckLinkerObjectRegion(source, path, io.NewSectionReader(f, 0, fi.Size()), 0)
}

func cgoSecIsDisallowedLTOSection(name string) bool {
	return strings.HasPrefix(name, ".gnu.lto_") ||
		strings.HasPrefix(name, ".gnu.debuglto_") ||
		strings.HasPrefix(name, ".llvm.lto") ||
		name == ".llvmbc" ||
		name == ".llvmcmd"
}

func cgoSecIsCOFFMachine(mach uint16) bool {
	switch mach {
	case pe.IMAGE_FILE_MACHINE_I386,
		pe.IMAGE_FILE_MACHINE_AMD64,
		pe.IMAGE_FILE_MACHINE_ARM,
		pe.IMAGE_FILE_MACHINE_ARMNT,
		pe.IMAGE_FILE_MACHINE_ARM64,
		pe.IMAGE_FILE_MACHINE_LOONGARCH32,
		pe.IMAGE_FILE_MACHINE_LOONGARCH64,
		pe.IMAGE_FILE_MACHINE_RISCV32,
		pe.IMAGE_FILE_MACHINE_RISCV64,
		pe.IMAGE_FILE_MACHINE_RISCV128:
		return true
	}
	return false
}

// cgoSecCoffReaderAt wraps an io.SectionReader so debug/pe.NewFile's initial 96-byte
// DOS-header probe succeeds on valid headerless COFF .o files smaller than 96 bytes.
type cgoSecCoffReaderAt struct {
	r *io.SectionReader
}

func (c cgoSecCoffReaderAt) ReadAt(p []byte, off int64) (int, error) {
	if off == 0 && len(p) == 96 && c.r.Size() >= 20 && c.r.Size() < 96 {
		clear(p)
		if _, err := c.r.ReadAt(p[:c.r.Size()], 0); err != nil {
			return 0, err
		}
		return len(p), nil
	}
	return c.r.ReadAt(p, off)
}

func cgoSecCheckLinkerArchive(source, path string, r *io.SectionReader, depth int) error {
	if source == ".syso" {
		return fmt.Errorf("archive not allowed as .syso in %s: %s", source, path)
	}
	off := int64(8)
	memberIdx := 0
	slashCount := 0
	sawLongNames := false
	var arhdr [60]byte
	for off+60 <= r.Size() {
		if _, err := r.ReadAt(arhdr[:], off); err != nil {
			return fmt.Errorf("malformed archive in %s: %s", source, path)
		}
		if arhdr[58] != '`' || arhdr[59] != '\n' {
			return fmt.Errorf("malformed archive header in %s: %s", source, path)
		}
		name := strings.TrimSpace(string(arhdr[0:16]))
		sizeStr := strings.TrimSpace(string(arhdr[48:58]))
		sz, err := strconv.ParseInt(sizeStr, 10, 64)
		if err != nil || sz < 0 || off+60+sz > r.Size() {
			return fmt.Errorf("malformed archive member size in %s: %s", source, path)
		}
		off += 60
		switch {
		case name == "__.SYMDEF" || name == "__.SYMDEF SORTED":
			if memberIdx != 0 {
				return fmt.Errorf("unexpected archive symbol table member %q in %s: %s", name, source, path)
			}
		case name == "/":
			if memberIdx != slashCount || slashCount >= 2 || sawLongNames {
				return fmt.Errorf("unexpected archive symbol table member %q in %s: %s", name, source, path)
			}
			if slashCount == 0 {
				if err := cgoSecCheckArchiveFirstLinkerMember(source, path, r, off, sz); err != nil {
					return err
				}
			} else {
				if err := cgoSecCheckPESecondLinkerMember(source, path, r, off, sz); err != nil {
					return err
				}
			}
			slashCount++
		case name == "//":
			if memberIdx != slashCount || sawLongNames {
				return fmt.Errorf("unexpected archive string table member in %s: %s", source, path)
			}
			sawLongNames = true
			// Reject binary objects disguised as the "//" string table, which a
			// symbol offset in "/" could otherwise target and cause the linker to load.
			if sz >= 4 {
				if err := cgoSecCheckLinkerObjectRegion(source, path, io.NewSectionReader(r, off, sz), depth+1); err == nil {
					return fmt.Errorf("binary object disguised as archive string table in %s: %s", source, path)
				} else if strings.Contains(err.Error(), "disallowed") {
					return err
				}
			}
		case strings.HasPrefix(name, "#1/"):
			nameLen, err := strconv.ParseInt(name[3:], 10, 64)
			if err != nil || nameLen <= 0 || nameLen > 4096 || nameLen > sz {
				return fmt.Errorf("malformed BSD archive member in %s: %s", source, path)
			}
			nameBytes := make([]byte, nameLen)
			if _, err := io.ReadFull(io.NewSectionReader(r, off, nameLen), nameBytes); err != nil {
				return fmt.Errorf("malformed BSD archive member name in %s: %s", source, path)
			}
			realName := strings.TrimRight(string(nameBytes), "\x00")
			isSymdef := memberIdx == 0 && (realName == "__.SYMDEF" || realName == "__.SYMDEF SORTED")
			if !isSymdef {
				if sz <= nameLen {
					return fmt.Errorf("empty BSD archive member in %s: %s", source, path)
				}
				if err := cgoSecCheckLinkerObjectRegion(source, path, io.NewSectionReader(r, off+nameLen, sz-nameLen), depth+1); err != nil {
					return err
				}
			}
		default:
			if sz <= 0 {
				return fmt.Errorf("empty archive member in %s: %s", source, path)
			}
			if err := cgoSecCheckLinkerObjectRegion(source, path, io.NewSectionReader(r, off, sz), depth+1); err != nil {
				return err
			}
		}
		off += sz
		if off&1 != 0 {
			off++
		}
		memberIdx++
	}
	if off != r.Size() {
		return fmt.Errorf("truncated or trailing data in archive in %s: %s", source, path)
	}
	return nil
}

// cgoSecCheckArchiveFirstLinkerMember validates the SVR4/GNU/PE first "/" archive
// symbol table member. When resolving a symbol from "/", linkers seek directly
// to the recorded file offset without checking the target member's header name.
// The big-endian symbol count check (4+4*numSyms <= sz) rejects binary object
// headers starting with a non-zero byte (\x7fELF, Mach-O, MZ, COFF); we also
// explicitly reject the PE short import header magic (00 00 ff ff, which would
// otherwise parse as numSyms = 65535).
func cgoSecCheckArchiveFirstLinkerMember(source, path string, r *io.SectionReader, off, sz int64) error {
	if sz == 0 {
		return nil
	}
	if sz < 4 {
		return fmt.Errorf("malformed archive symbol table in %s: %s", source, path)
	}
	var shdr [4]byte
	if _, err := r.ReadAt(shdr[:], off); err != nil {
		return fmt.Errorf("malformed archive symbol table in %s: %s", source, path)
	}
	if shdr == [4]byte{0, 0, 0xff, 0xff} {
		return fmt.Errorf("binary object disguised as archive symbol table in %s: %s", source, path)
	}
	numSyms := int64(binary.BigEndian.Uint32(shdr[:]))
	if 4+4*numSyms > sz {
		return fmt.Errorf("malformed archive symbol table in %s: %s", source, path)
	}
	return nil
}

// cgoSecCheckPESecondLinkerMember validates the optional second "/" linker member in
// a Microsoft PE/COFF archive (which lists little-endian member count, member
// file offsets, symbol count, and symbol indices). The numMembers <= r.Size()/60
// bound rejects binary object headers disguised as a second "/" member.
func cgoSecCheckPESecondLinkerMember(source, path string, r *io.SectionReader, off, sz int64) error {
	if sz < 8 {
		return fmt.Errorf("malformed PE archive second linker member in %s: %s", source, path)
	}
	var mhdr [8]byte
	if _, err := r.ReadAt(mhdr[:], off); err != nil {
		return fmt.Errorf("malformed PE archive second linker member in %s: %s", source, path)
	}
	numMembers := int64(binary.LittleEndian.Uint32(mhdr[0:4]))
	if numMembers > r.Size()/60 || 4+4*numMembers+4 > sz {
		return fmt.Errorf("malformed PE archive second linker member in %s: %s", source, path)
	}
	var symCountBuf [4]byte
	if _, err := r.ReadAt(symCountBuf[:], off+4+4*numMembers); err != nil {
		return fmt.Errorf("malformed PE archive second linker member in %s: %s", source, path)
	}
	numSyms := int64(binary.LittleEndian.Uint32(symCountBuf[:]))
	if 4+4*numMembers+4+2*numSyms > sz {
		return fmt.Errorf("malformed PE archive second linker member in %s: %s", source, path)
	}
	return nil
}

// cgoSecCheckPEImportObject validates a PE short import library member
// (IMPORT_OBJECT_HEADER: Sig1=0x0000, Sig2=0xFFFF, Version=0 followed by
// null-terminated symbol and DLL name strings, which cannot contain .drectve).
func cgoSecCheckPEImportObject(source, path string, r *io.SectionReader) error {
	if r.Size() < 20 {
		return fmt.Errorf("invalid PE import object header in %s: %s", source, path)
	}
	var hdr [20]byte
	if _, err := r.ReadAt(hdr[:], 0); err != nil {
		return fmt.Errorf("invalid PE import object header in %s: %s", source, path)
	}
	mach := binary.LittleEndian.Uint16(hdr[6:8])
	if !cgoSecIsCOFFMachine(mach) {
		return fmt.Errorf("invalid PE import object machine %#x in %s: %s", mach, source, path)
	}
	sizeOfData := binary.LittleEndian.Uint32(hdr[12:16])
	if sizeOfData < 4 || sizeOfData > 1<<20 || 20+int64(sizeOfData) != r.Size() {
		return fmt.Errorf("invalid PE import object size in %s: %s", source, path)
	}
	flags := binary.LittleEndian.Uint16(hdr[18:20])
	typ := flags & 0x3
	nameType := (flags >> 2) & 0x7
	if typ > 2 || nameType > 3 || flags>>5 != 0 {
		return fmt.Errorf("invalid PE import object type flags %#x in %s: %s", flags, source, path)
	}
	data := make([]byte, sizeOfData)
	if _, err := io.ReadFull(io.NewSectionReader(r, 20, int64(sizeOfData)), data); err != nil {
		return fmt.Errorf("cannot read PE import object data in %s: %s: %v", source, path, err)
	}
	sep := bytes.IndexByte(data, 0)
	if sep <= 0 || sep >= len(data)-2 || data[len(data)-1] != 0 || bytes.IndexByte(data[sep+1:len(data)-1], 0) >= 0 {
		return fmt.Errorf("invalid PE import object strings in %s: %s", source, path)
	}
	return nil
}

func cgoSecCheckLinkerBigArchive(source, path string, r *io.SectionReader, depth int) error {
	if source == ".syso" {
		return fmt.Errorf("archive not allowed as .syso in %s: %s", source, path)
	}
	if r.Size() < 128 {
		return fmt.Errorf("malformed AIX big archive header in %s: %s", source, path)
	}
	var fhdr [128]byte
	if _, err := r.ReadAt(fhdr[:], 0); err != nil {
		return fmt.Errorf("malformed AIX big archive header in %s: %s", source, path)
	}
	parseDec := func(b []byte) (int64, error) {
		return strconv.ParseInt(strings.TrimSpace(string(b)), 10, 64)
	}
	off, err := parseDec(fhdr[68:88])
	if err != nil || off < 0 || (off > 0 && off < 128) || off > r.Size() {
		return fmt.Errorf("malformed AIX big archive first member offset in %s: %s", source, path)
	}
	if off == 0 {
		return nil
	}
	lastoff, err := parseDec(fhdr[88:108])
	if err != nil || lastoff < off || lastoff > r.Size() {
		return fmt.Errorf("malformed AIX big archive last member offset in %s: %s", source, path)
	}
	var mhdr [112]byte
	for {
		if off+112 > r.Size() {
			return fmt.Errorf("malformed AIX big archive member header in %s: %s", source, path)
		}
		if _, err := r.ReadAt(mhdr[:], off); err != nil {
			return fmt.Errorf("malformed AIX big archive member header in %s: %s", source, path)
		}
		sz, err := parseDec(mhdr[0:20])
		if err != nil || sz < 0 {
			return fmt.Errorf("malformed AIX big archive member size in %s: %s", source, path)
		}
		namlen, err := parseDec(mhdr[108:112])
		if err != nil || namlen <= 0 || namlen > 4096 {
			return fmt.Errorf("malformed AIX big archive member name length in %s: %s", source, path)
		}
		fileoff := off + 112 + namlen
		if fileoff&1 != 0 {
			fileoff++
		}
		var fmag [2]byte
		if fileoff+2 > r.Size() || sz > r.Size()-(fileoff+2) {
			return fmt.Errorf("malformed AIX big archive member bounds in %s: %s", source, path)
		}
		if _, err := r.ReadAt(fmag[:], fileoff); err != nil || fmag[0] != '`' || fmag[1] != '\n' {
			return fmt.Errorf("malformed AIX big archive member magic in %s: %s", source, path)
		}
		fileoff += 2
		if namlen > 0 {
			if sz == 0 {
				return fmt.Errorf("empty AIX big archive member in %s: %s", source, path)
			}
			if err := cgoSecCheckLinkerObjectRegion(source, path, io.NewSectionReader(r, fileoff, sz), depth+1); err != nil {
				return err
			}
		}
		if off == lastoff {
			break
		}
		nextoff, err := parseDec(mhdr[20:40])
		if err != nil || nextoff < fileoff+sz || nextoff > lastoff {
			return fmt.Errorf("malformed AIX big archive next member offset in %s: %s", source, path)
		}
		off = nextoff
	}
	return nil
}

// cgoSecHasCOFFControlBeforeComment reports whether buf contains a non-whitespace C0
// control/null byte (or DEL) before any "/*" linker-script block comment can open.
// This rejects ASCII polyglot linker scripts starting with a COFF machine magic
// such as "db" (0x6264) or "dP" (0x5064) while allowing COFF objects with >= 256 sections.
func cgoSecHasCOFFControlBeforeComment(buf []byte) bool {
	for i := 0; i < len(buf); i++ {
		if i+1 < len(buf) && buf[i] == '/' && buf[i+1] == '*' {
			return false
		}
		if buf[i] < 0x09 || (buf[i] > 0x0d && buf[i] < 0x20) || buf[i] == 0x7f {
			return true
		}
	}
	return false
}

func cgoSecCheckLinkerObjectRegion(source, path string, r *io.SectionReader, depth int) error {
	if depth > 4 {
		return fmt.Errorf("nested archive/fat object too deep in %s: %s", source, path)
	}
	var hdr [64]byte
	n, _ := io.ReadFull(r, hdr[:])
	if n < 4 {
		return fmt.Errorf("invalid object file in %s: %s", source, path)
	}
	buf := hdr[:n]

	if bytes.HasPrefix(buf, []byte("!<arch>\n")) {
		return cgoSecCheckLinkerArchive(source, path, r, depth)
	}
	if bytes.HasPrefix(buf, []byte("<bigaf>\n")) {
		return cgoSecCheckLinkerBigArchive(source, path, r, depth)
	}

	// Check PE short import library member (IMPORT_OBJECT_HEADER: Sig1=0x0000, Sig2=0xFFFF, Version=0).
	// Leading 0x00 bytes at offset 0..1 prevent GNU ld ASCII linker-script polyglots.
	if source != ".syso" && n >= 6 && binary.LittleEndian.Uint16(buf[0:2]) == 0x0000 && binary.LittleEndian.Uint16(buf[2:4]) == 0xffff && binary.LittleEndian.Uint16(buf[4:6]) == 0 {
		return cgoSecCheckPEImportObject(source, path, r)
	}

	// Check COFF object or PE image and validate .drectve and LTO sections via debug/pe.
	// Require buf[3] <= 1 for MZ (DOS header e_cblp < 0x200) and a non-whitespace control/null
	// byte in the 20-byte COFF FileHeader before any "/*" comment can open (plus structural
	// section/symbol table bounds checks), preventing an ASCII polyglot linker script starting
	// with "MZ" or an ASCII COFF machine magic (such as "db" or "dP") while allowing >= 256 sections.
	mach := binary.LittleEndian.Uint16(buf[0:2])
	isMZ := bytes.HasPrefix(buf, []byte("MZ")) && source != ".syso" && binary.LittleEndian.Uint16(buf[2:4]) < 0x200
	isCOFF := !isMZ && n >= 20 && cgoSecIsCOFFMachine(mach) && cgoSecHasCOFFControlBeforeComment(buf[:20])
	if isCOFF || isMZ {
		var coffBase int64
		if isMZ {
			if n < 0x40 {
				return fmt.Errorf("invalid PE DOS header in %s: %s", source, path)
			}
			signoff := int64(binary.LittleEndian.Uint32(buf[0x3c:0x40]))
			if signoff < 0 || signoff+4 > r.Size() {
				return fmt.Errorf("invalid PE signature offset in %s: %s", source, path)
			}
			coffBase = signoff + 4
		}
		var rAt io.ReaderAt = r
		if !isMZ {
			rAt = cgoSecCoffReaderAt{r}
		}
		pf, err := pe.NewFile(rAt)
		if err != nil {
			return fmt.Errorf("invalid PE/COFF object in %s: %s: %v", source, path, err)
		}
		if !cgoSecIsCOFFMachine(pf.FileHeader.Machine) || (isMZ && pf.OptionalHeader == nil) || len(pf.Sections) == 0 {
			return fmt.Errorf("invalid PE/COFF header or missing sections in %s: %s", source, path)
		}
		hdrEnd := coffBase + 20 + int64(pf.FileHeader.SizeOfOptionalHeader) + int64(pf.FileHeader.NumberOfSections)*40
		if hdrEnd > r.Size() {
			return fmt.Errorf("truncated PE/COFF section table in %s: %s", source, path)
		}
		if pf.FileHeader.PointerToSymbolTable != 0 {
			symOff := int64(pf.FileHeader.PointerToSymbolTable)
			symEnd := symOff + int64(pf.FileHeader.NumberOfSymbols)*18
			if symOff < hdrEnd || symEnd > r.Size() {
				return fmt.Errorf("invalid PE/COFF symbol table bounds in %s: %s", source, path)
			}
		}
		for _, sec := range pf.Sections {
			if sec.Offset != 0 {
				secEnd := int64(sec.Offset) + int64(sec.Size)
				if secEnd > r.Size() || (sec.Size > 0 && int64(sec.Offset) < hdrEnd) {
					return fmt.Errorf("invalid PE/COFF section %q bounds in %s: %s", sec.Name, source, path)
				}
			}
			secName := strings.TrimSpace(sec.Name)
			if cgoSecIsDisallowedLTOSection(secName) {
				return fmt.Errorf("disallowed LTO section %q in %s: %s", secName, source, path)
			}
			if secName == ".drectve" {
				if sec.Size == 0 || sec.Size > 1<<20 {
					return fmt.Errorf("suspicious .drectve section size (%d) in %s: %s", sec.Size, source, path)
				}
				data, err := sec.Data()
				if err != nil {
					return fmt.Errorf("cannot read .drectve section in %s: %s: %v", source, path, err)
				}
				if err := cgoSecCheckDrectveDirectives(source, path, data); err != nil {
					return err
				}
			}
		}
		return nil
	}

	// Check Mach-O object via debug/macho and reject LC_LINKER_OPTION (0x2d) and LTO segments/sections.
	magicLE := binary.LittleEndian.Uint32(buf[0:4])
	magicBE := binary.BigEndian.Uint32(buf[0:4])
	if magicLE == macho.Magic32 || magicLE == macho.Magic64 || magicBE == macho.Magic32 || magicBE == macho.Magic64 {
		mf, err := macho.NewFile(r)
		if err != nil {
			return fmt.Errorf("invalid Mach-O object in %s: %s: %v", source, path, err)
		}
		for _, l := range mf.Loads {
			if raw := l.Raw(); len(raw) >= 4 && mf.ByteOrder.Uint32(raw[:4]) == 0x2d { // LC_LINKER_OPTION
				if err := cgoSecCheckMachOLinkerOption(source, path, mf.ByteOrder, raw); err != nil {
					return err
				}
			}
			if seg, ok := l.(*macho.Segment); ok {
				if seg.Name == "__LLVM" || seg.Name == "__GNU_LTO" {
					return fmt.Errorf("disallowed LTO segment %q in %s: %s", seg.Name, source, path)
				}
			}
		}
		for _, sec := range mf.Sections {
			if sec.Seg == "__LLVM" || sec.Seg == "__GNU_LTO" || cgoSecIsDisallowedLTOSection(sec.Name) {
				return fmt.Errorf("disallowed LTO section %s,%s in %s: %s", sec.Seg, sec.Name, source, path)
			}
		}
		return nil
	}

	// Check Universal (Fat) Mach-O (0xcafebabe) and validate each slice.
	// Note: slices inside a fat .a static library are !<arch>\n archives rather than
	// thin Mach-O files, so we read macho.FatArchHeader and recurse rather than calling macho.NewFatFile.
	if magicBE == macho.MagicFat {
		if n < 8 {
			return fmt.Errorf("truncated universal Mach-O header in %s: %s", source, path)
		}
		nfat := int(binary.BigEndian.Uint32(buf[4:8]))
		if nfat <= 0 || nfat > 32 {
			return fmt.Errorf("invalid universal Mach-O arch count in %s: %s", source, path)
		}
		sr := io.NewSectionReader(r, 8, int64(nfat)*20)
		for i := 0; i < nfat; i++ {
			var fa macho.FatArchHeader
			if err := binary.Read(sr, binary.BigEndian, &fa); err != nil {
				return fmt.Errorf("truncated universal Mach-O arch header in %s: %s", source, path)
			}
			sliceOff := int64(fa.Offset)
			sliceSz := int64(fa.Size)
			if sliceOff < 0 || sliceSz <= 0 || sliceOff+sliceSz > r.Size() {
				return fmt.Errorf("invalid universal Mach-O slice bounds in %s: %s", source, path)
			}
			if err := cgoSecCheckLinkerObjectRegion(source, path, io.NewSectionReader(r, sliceOff, sliceSz), depth+1); err != nil {
				return err
			}
		}
		return nil
	}

	// Check ELF object via debug/elf and reject GCC/LLVM LTO sections.
	if bytes.HasPrefix(buf, []byte(elf.ELFMAG)) {
		ef, err := elf.NewFile(r)
		if err != nil {
			return fmt.Errorf("invalid ELF object in %s: %s: %v", source, path, err)
		}
		if ef.Type == elf.ET_REL && len(ef.Sections) == 0 {
			return fmt.Errorf("ELF relocatable object missing section headers in %s: %s", source, path)
		}
		for _, sec := range ef.Sections {
			if sec.Type == 0x6fff4c01 || sec.Type == 0x6fff4c04 { // SHT_LLVM_LINKER_OPTIONS, SHT_LLVM_DEPENDENT_LIBRARIES
				return fmt.Errorf("disallowed ELF embedded linker options section in %s: %s", source, path)
			}
			if sec.Type == 0x6fff4c0c { // SHT_LLVM_LTO
				return fmt.Errorf("disallowed LLVM LTO section in %s: %s", source, path)
			}
			if cgoSecIsDisallowedLTOSection(sec.Name) {
				return fmt.Errorf("disallowed LTO section %q in %s: %s", sec.Name, source, path)
			}
		}
		return nil
	}

	// An XCOFF object. cmd/go parses it with internal/xcoff, which a program outside GOROOT cannot import; go2cs targets
	// no AIX, so the port refuses it rather than accept one it has not read (a deviation, stricter than cmd/go).
	magicBE16 := binary.BigEndian.Uint16(buf[0:2])
	if magicBE16 == 0x01DF || magicBE16 == 0x01F7 { // U802TOCMAGIC, U64_TOCMAGIC
		return fmt.Errorf("invalid linker input file in %s: %s (an XCOFF object, which go2cs does not read)", source, path)
	}

	// Check Plan 9 object via debug/plan9obj.
	if magicBE == plan9obj.Magic386 || magicBE == plan9obj.MagicAMD64 || magicBE == plan9obj.MagicARM {
		if _, err := plan9obj.NewFile(r); err != nil {
			return fmt.Errorf("invalid Plan 9 object in %s: %s: %v", source, path, err)
		}
		return nil
	}

	// Check WASM binary ("\x00asm\x01\x00\x00\x00").
	if bytes.HasPrefix(buf, []byte("\x00asm\x01\x00\x00\x00")) {
		return nil
	}
	return fmt.Errorf("invalid linker input file in %s: %s (unrecognized binary format)", source, path)
}

// cgoSecCheckMachOLinkerOption validates an LC_LINKER_OPTION (0x2d) load command,
// allowing safe library (-l<name>) and framework (-framework/-weak_framework <name>)
// auto-link directives emitted by Clang (-fmodules) and Rust/Swift staticlibs
// while rejecting arbitrary linker flags (such as -lto_library or -lazy_library).
func cgoSecCheckMachOLinkerOption(source, path string, bo binary.ByteOrder, raw []byte) error {
	if len(raw) < 12 {
		return fmt.Errorf("malformed LC_LINKER_OPTION in %s: %s", source, path)
	}
	count := bo.Uint32(raw[8:12])
	if count == 0 || count > 2 {
		return fmt.Errorf("disallowed LC_LINKER_OPTION in %s: %s", source, path)
	}
	rest := raw[12:]
	opts := make([]string, 0, count)
	for range count {
		nul := bytes.IndexByte(rest, 0)
		if nul <= 0 {
			return fmt.Errorf("disallowed LC_LINKER_OPTION in %s: %s", source, path)
		}
		opts = append(opts, string(rest[:nul]))
		rest = rest[nul+1:]
	}
	for _, b := range rest {
		if b != 0 {
			return fmt.Errorf("disallowed LC_LINKER_OPTION in %s: %s", source, path)
		}
	}
	switch len(opts) {
	case 1:
		arg := opts[0]
		if !strings.HasPrefix(arg, "-l") || len(arg) <= 2 || !cgoSecSafeArg(arg[2:]) || strings.ContainsAny(arg[2:], `:=/\`) {
			return fmt.Errorf("disallowed LC_LINKER_OPTION %q in %s: %s", arg, source, path)
		}
		for _, cgoSecRe := range cgoSecInvalidLinkerFlags {
			if cgoSecRe.FindString(arg) == arg {
				return fmt.Errorf("disallowed LC_LINKER_OPTION %q in %s: %s", arg, source, path)
			}
		}
		return nil
	case 2:
		if (opts[0] == "-framework" || opts[0] == "-weak_framework") && cgoSecSafeArg(opts[1]) {
			if err := cgoSecCheckLinkerNonPathNonFile(source, opts[0]+" "+opts[1], opts[1]); err == nil {
				return nil
			}
		}
		return fmt.Errorf("disallowed LC_LINKER_OPTION %q in %s: %s", strings.Join(opts, " "), source, path)
	}
	return fmt.Errorf("disallowed LC_LINKER_OPTION in %s: %s", source, path)
}

// cgoSecCheckDrectveDirectives validates the content of a COFF .drectve section,
// allowlisting known-safe MSVC and MinGW linker directives while rejecting
// anything that could inject dangerous linker behavior (e.g. --plugin, -T,
// --script, -L, --library-path, --dynamic-linker).
func cgoSecCheckDrectveDirectives(source, path string, data []byte) error {
	content := strings.TrimPrefix(string(data), "\xef\xbb\xbf")
	content = strings.TrimRight(content, "\x00 \t\r\n")
	if content == "" {
		return nil
	}
	tokens := cgoSecTokenizeDrectve(content)
	for _, tok := range tokens {
		if !cgoSecIsAllowedDrectve(tok) {
			return fmt.Errorf("disallowed .drectve directive %q in %s: %s", tok, source, path)
		}
	}
	return nil
}

const cgoSecDrectveSpace = " \t\r\n\v\f\x00"

func cgoSecTokenizeDrectve(s string) []string {
	var tokens []string
	s = strings.TrimLeft(s, cgoSecDrectveSpace)
	for len(s) > 0 {
		var tok strings.Builder
		inQuote := false
		i := 0
		for i < len(s) {
			if s[i] == '"' {
				inQuote = !inQuote
				i++
				continue
			}
			if !inQuote && strings.IndexByte(cgoSecDrectveSpace, s[i]) >= 0 {
				break
			}
			tok.WriteByte(s[i])
			i++
		}
		if tok.Len() > 0 {
			tokens = append(tokens, tok.String())
		}
		s = strings.TrimLeft(s[i:], cgoSecDrectveSpace)
	}
	return tokens
}

var cgoSecReDrectveSafeName = regexp.MustCompile(`^[a-zA-Z0-9_.][a-zA-Z0-9_.+\-@$=]*$`)

func cgoSecIsAllowedDrectve(tok string) bool {
	if strings.ContainsAny(tok, "\r\n\v\f\x00") {
		return false
	}
	norm := strings.ToUpper(tok)
	if strings.HasPrefix(norm, "-") && !strings.HasPrefix(norm, "--") {
		norm = "/" + norm[1:]
	}
	// MSVC, Clang, and MinGW directives: /FLAG:value or -flag:value
	if strings.HasPrefix(norm, "/DEFAULTLIB:") ||
		strings.HasPrefix(norm, "/DISALLOWLIB:") ||
		strings.HasPrefix(norm, "/EXPORT:") ||
		strings.HasPrefix(norm, "/INCLUDE:") ||
		strings.HasPrefix(norm, "/ALTERNATENAME:") ||
		strings.HasPrefix(norm, "/MERGE:") ||
		strings.HasPrefix(norm, "/SECTION:") ||
		strings.HasPrefix(norm, "/ALIGNCOMM:") ||
		strings.HasPrefix(norm, "/EXCLUDE-SYMBOLS:") ||
		strings.HasPrefix(norm, "/FAILIFMISMATCH:") ||
		strings.HasPrefix(norm, "/MANIFESTDEPENDENCY:") {
		val := tok[strings.IndexByte(tok, ':')+1:]
		val = strings.Trim(val, `"`)
		if val == "" {
			return false
		}
		if strings.ContainsAny(val, `/\`) && !strings.HasPrefix(norm, "/MANIFESTDEPENDENCY:") {
			return false
		}
		if (strings.HasPrefix(norm, "/DEFAULTLIB:") || strings.HasPrefix(norm, "/DISALLOWLIB:")) && (strings.Contains(val, ":") || strings.HasPrefix(val, "@")) {
			return false
		}
		if strings.Contains(val, "..") {
			return false
		}
		return true
	}
	// Flags with no value (or optional library name on NODEFAULTLIB) that are safe to pass through.
	if norm == "/NODEFAULTLIB" {
		return true
	}
	if strings.HasPrefix(norm, "/NODEFAULTLIB:") {
		val := strings.Trim(tok[strings.IndexByte(tok, ':')+1:], `"`)
		return val != "" && !strings.ContainsAny(val, `/\:`) && !strings.HasPrefix(val, "@") && !strings.Contains(val, "..")
	}
	if norm == "/SAFESEH" || norm == "/SAFESEH:NO" {
		return true
	}
	if norm == "/GUARD:CF" || norm == "/GUARD:NO" || norm == "/GUARD:EHCONT" || norm == "/GUARD:LONGJMP" {
		return true
	}
	if norm == "/DYNAMICBASE" || norm == "/DYNAMICBASE:NO" ||
		norm == "/NXCOMPAT" || norm == "/NXCOMPAT:NO" ||
		norm == "/CETCOMPAT" || norm == "/CETCOMPAT:NO" ||
		norm == "/HIGHENTROPYVA" || norm == "/HIGHENTROPYVA:NO" ||
		norm == "/LARGEADDRESSAWARE" || norm == "/LARGEADDRESSAWARE:NO" ||
		norm == "/TSAWARE" || norm == "/TSAWARE:NO" ||
		norm == "/EXPORT-ALL-SYMBOLS" || norm == "/EXCLUDE-ALL-SYMBOLS" {
		return true
	}
	// MinGW-style directives
	if strings.HasPrefix(tok, "-l") && len(tok) > 2 {
		libName := tok[2:]
		return cgoSecReDrectveSafeName.MatchString(libName) && !strings.Contains(libName, "..")
	}
	switch tok {
	case "--enable-auto-import", "--disable-auto-import",
		"--enable-runtime-pseudo-reloc", "--disable-runtime-pseudo-reloc",
		"--enable-stdcall-fixup", "--disable-stdcall-fixup",
		"--export-all-symbols", "--exclude-all-symbols":
		return true
	}
	return false
}

func cgoSecCheckPkgConfigFlags(name, source string, list []string) error {
	checkOverrides := false
	return cgoSecCheckFlags(name, source, list, nil, cgoSecValidPkgConfigFlags, nil, nil, checkOverrides)
}

func cgoSecValidPkgConfigName(pkg string) bool {
	if !cgoSecSafeArg(pkg) || !utf8.ValidString(pkg) {
		return false
	}
	if strings.ContainsAny(pkg, `/\:`) || strings.Contains(pkg, "..") {
		return false
	}
	fields := strings.FieldsFunc(pkg, func(r rune) bool {
		return r == ',' || unicode.IsSpace(r)
	})
	if len(fields) == 0 {
		return false
	}
	for _, field := range fields {
		lower := strings.ToLower(field)
		if strings.HasSuffix(lower, ".pc") {
			return false
		}
		for i := range len(lower) {
			if strings.IndexByte("<>=!", lower[i]) >= 0 && strings.HasSuffix(lower[:i], ".pc") {
				return false
			}
		}
	}
	return true
}

// cgoSecCheckCompilerFlagsForInternalLink returns an error if 'list'
// contains a flag or flags that may not be fully supported by
// internal linking (meaning that we should punt the link to the
// external linker).
func cgoSecCheckCompilerFlagsForInternalLink(name, source string, list []string) error {
	checkOverrides := false
	if err := cgoSecCheckFlags(name, source, list, cgoSecInvalidCompilerFlags, cgoSecValidCompilerFlags, nil, cgoSecValidCompilerFlagsWithNextArg, checkOverrides); err != nil {
		return err
	}
	// Currently the only flag on the allow list that causes problems
	// for the linker is "-flto"; check for it manually here.
	for _, fl := range list {
		if strings.HasPrefix(fl, "-flto") {
			return fmt.Errorf("flag %q triggers external linking", fl)
		}
	}
	return nil
}

func cgoSecCheckFlags(name, source string, list []string, invalid, valid []*regexp.Regexp, recombineNext, validNext []string, checkOverrides bool) error {
	// Let users override rules with $CGO_CFLAGS_ALLOW, $CGO_CFLAGS_DISALLOW, etc.
	var (
		allow    *regexp.Regexp
		disallow *regexp.Regexp
	)
	if checkOverrides {
		if env := cgoSecGetenv("CGO_" + name + "_ALLOW"); env != "" {
			r, err := regexp.Compile(env)
			if err != nil {
				return fmt.Errorf("parsing $CGO_%s_ALLOW: %v", name, err)
			}
			allow = r
		}
		if env := cgoSecGetenv("CGO_" + name + "_DISALLOW"); env != "" {
			r, err := regexp.Compile(env)
			if err != nil {
				return fmt.Errorf("parsing $CGO_%s_DISALLOW: %v", name, err)
			}
			disallow = r
		}
	}

Args:
	for i := 0; i < len(list); i++ {
		arg := list[i]
		if disallow != nil && disallow.FindString(arg) == arg {
			goto Bad
		}
		if allow != nil && allow.FindString(arg) == arg {
			continue Args
		}
		for _, cgoSecRe := range invalid {
			if cgoSecRe.FindString(arg) == arg { // must be complete match
				goto Bad
			}
		}
		for _, cgoSecRe := range valid {
			if match := cgoSecRe.FindString(arg); match == arg { // must be complete match
				continue Args
			} else if strings.HasPrefix(arg, "-Wl,--push-state,") {
				// Examples for --push-state are written
				//     -Wl,--push-state,--as-needed
				// Support other commands in the same -Wl arg.
				args := strings.Split(arg, ",")
				for _, a := range args[1:] {
					a = "-Wl," + a
					var found bool
					for _, cgoSecRe := range valid {
						if cgoSecRe.FindString(a) == a {
							found = true
							break
						}
					}
					if !found {
						goto Bad
					}
					for _, cgoSecRe := range invalid {
						if cgoSecRe.FindString(a) == a {
							goto Bad
						}
					}
				}
				continue Args
			}
		}
		if slices.Contains(recombineNext, arg) && i+1 < len(list) {
			combined := arg + list[i+1]
			if disallow != nil && disallow.FindString(combined) == combined {
				goto Bad
			}
			if allow == nil || allow.FindString(combined) != combined {
				for _, cgoSecRe := range invalid {
					if cgoSecRe.FindString(combined) == combined {
						goto Bad
					}
				}
			}
		}
		for _, x := range validNext {
			if arg == x {
				if arg == "-x" {
					if i+1 < len(list) && cgoSecValidCompilerXLang.MatchString(list[i+1]) {
						i++
						continue Args
					}
					if i+1 < len(list) {
						return fmt.Errorf("invalid flag in %s: %s %s (see https://go.dev/s/invalidflag)", source, arg, list[i+1])
					}
					return fmt.Errorf("invalid flag in %s: %s without argument (see https://go.dev/s/invalidflag)", source, arg)
				}
				if strings.HasPrefix(arg, "-Wl,") {
					// Permit -Wl,-framework -Wl,name.
					if i+1 < len(list) &&
						strings.HasPrefix(list[i+1], "-Wl,") &&
						cgoSecSafeArg(list[i+1][4:]) &&
						!strings.Contains(list[i+1][4:], ",") {
						i++
						continue Args
					}
					// Permit -Wl,-R /foo and -Wl,-R foo.
					if arg == "-Wl,-R" && i+1 < len(list) && cgoSecSafeArg(list[i+1]) {
						i++
						continue Args
					}
					if i+1 < len(list) {
						return fmt.Errorf("invalid flag in %s: %s %s (see https://go.dev/s/invalidflag)", source, arg, list[i+1])
					}
					return fmt.Errorf("invalid flag in %s: %s without argument (see https://go.dev/s/invalidflag)", source, arg)
				}
				if arg == "-l" {
					if i+1 < len(list) && cgoSecSafeArg(list[i+1]) && !strings.ContainsAny(list[i+1], `:=/\`) {
						i++
						continue Args
					}
					if i+1 < len(list) {
						return fmt.Errorf("invalid flag in %s: %s %s (see https://go.dev/s/invalidflag)", source, arg, list[i+1])
					}
					return fmt.Errorf("invalid flag in %s: %s without argument (see https://go.dev/s/invalidflag)", source, arg)
				}
				if i+1 < len(list) && cgoSecSafeArg(list[i+1]) {
					i++
					continue Args
				}

				// Permit -I= /path, -I $SYSROOT.
				if i+1 < len(list) && arg == "-I" {
					if (strings.HasPrefix(list[i+1], "=") || strings.HasPrefix(list[i+1], "$SYSROOT")) &&
						cgoSecSafeArg(list[i+1][1:]) {
						i++
						continue Args
					}
				}

				if i+1 < len(list) {
					return fmt.Errorf("invalid flag in %s: %s %s (see https://go.dev/s/invalidflag)", source, arg, list[i+1])
				}
				return fmt.Errorf("invalid flag in %s: %s without argument (see https://go.dev/s/invalidflag)", source, arg)
			}
		}
	Bad:
		return fmt.Errorf("invalid flag in %s: %s (see https://go.dev/s/invalidflag)", source, arg)
	}
	return nil
}
