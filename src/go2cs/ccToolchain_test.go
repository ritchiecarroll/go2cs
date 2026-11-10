// ccToolchain_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"strings"
	"testing"
)

// fakePinnedCC is step 4 under test: an install that may already be cached, and an install that may fail.
type fakePinnedCC struct {
	isCached   bool
	installErr error
	installs   int
}

func (f *fakePinnedCC) platform() string { return "linux/amd64" }

func (f *fakePinnedCC) cached() (string, []string, bool) {
	return "/cache/zig cc", []string{"ZIG_GLOBAL_CACHE_DIR=/cache/zc"}, f.isCached
}

func (f *fakePinnedCC) install() (string, []string, error) {
	f.installs++

	if f.installErr != nil {
		return "", nil, f.installErr
	}

	return "/cache/zig cc", []string{"ZIG_GLOBAL_CACHE_DIR=/cache/zc"}, nil
}

func ccResolverFor(flag string, download bool, env map[string]string, onPath []string, pinned ccPinnedToolchain) *ccResolver {
	return &ccResolver{
		flag: flag, download: download, goos: "linux", pinned: pinned,
		getenv: func(key string) string { return env[key] },
		lookPath: func(name string) (string, error) {
			for _, p := range onPath {
				if p == name {
					return "/usr/bin/" + name, nil
				}
			}

			return "", errors.New("not found")
		},
	}
}

func TestCCResolutionOrder(t *testing.T) {
	cases := []struct {
		name     string
		flag     string
		download bool
		env      map[string]string
		onPath   []string
		pinned   *fakePinnedCC
		wantCC   string
		wantHow  string
		installs int
	}{
		{"-cc wins over everything", "clang-19 -O0", true, map[string]string{"CC": "cc"}, []string{"gcc"}, &fakePinnedCC{}, "clang-19 -O0", "-cc", 0},
		{"CC from the environment comes next", "", true, map[string]string{"CC": " zig cc "}, []string{"gcc"}, &fakePinnedCC{}, "zig cc", "CC from the environment", 0},
		{"then the platform default on PATH", "", true, nil, []string{"gcc"}, &fakePinnedCC{}, "gcc", "gcc on PATH", 0},
		{"then the pinned toolchain, installed once", "", true, nil, nil, &fakePinnedCC{}, "/cache/zig cc", "the pinned toolchain", 1},
		{"a cached pin is used without downloading", "", false, nil, nil, &fakePinnedCC{isCached: true}, "/cache/zig cc", "the pinned toolchain", 0},
		{"-cc pinned skips CC and PATH", ccFlagPinned, true, map[string]string{"CC": "cc"}, []string{"gcc"}, &fakePinnedCC{}, "/cache/zig cc", "the pinned toolchain", 1},
		{"-cc host still takes PATH", ccFlagHost, true, nil, []string{"gcc"}, &fakePinnedCC{}, "gcc", "gcc on PATH", 0},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			choice, err := ccResolverFor(c.flag, c.download, c.env, c.onPath, c.pinned).resolve()
			if err != nil {
				t.Fatalf("resolve: %v", err)
			}

			if choice.cc != c.wantCC || choice.how != c.wantHow {
				t.Fatalf("got %q (%s), want %q (%s)", choice.cc, choice.how, c.wantCC, c.wantHow)
			}

			if choice.env[0] != "CC="+c.wantCC {
				t.Fatalf("env[0] = %q, want CC=%s", choice.env[0], c.wantCC)
			}

			if c.pinned.installs != c.installs {
				t.Fatalf("installs = %d, want %d", c.pinned.installs, c.installs)
			}

			if c.wantHow == "the pinned toolchain" && !strings.Contains(strings.Join(choice.env, " "), "ZIG_GLOBAL_CACHE_DIR=") {
				t.Fatalf("the pinned toolchain's own environment is missing: %v", choice.env)
			}
		})
	}

	// The darwin default is clang, as cmd/go's is.
	r := ccResolverFor("", true, nil, []string{"clang"}, &fakePinnedCC{})
	r.goos = "darwin"

	if choice, err := r.resolve(); err != nil || choice.cc != "clang" {
		t.Fatalf("darwin: got %q, %v; want clang on PATH", choice.cc, err)
	}
}

func TestCCResolutionRefusesByName(t *testing.T) {
	cases := []struct {
		name   string
		flag   string
		down   bool
		pinned ccPinnedToolchain
		want   []string
	}{
		{"-cc host with nothing on the host", ccFlagHost, true, &fakePinnedCC{}, []string{"-cc host refuses the pinned one", "gcc on PATH"}},
		{"-cc-download=false with no cached pin", "", false, &fakePinnedCC{}, []string{"-cc-download=false", "go2cs toolchain install -cc -from FILE", "CC in the environment"}},
		{"no pin for this platform", "", true, nil, []string{"no pinned C toolchain"}},
		{"-cc pinned with no pin for this platform", ccFlagPinned, true, nil, []string{"no pinned C toolchain"}},
		{"an install that fails", "", true, &fakePinnedCC{installErr: errors.New("every source failed")}, []string{"every source failed"}},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			_, err := ccResolverFor(c.flag, c.down, nil, nil, c.pinned).resolve()
			if err == nil {
				t.Fatal("resolved; want a refusal")
			}

			for _, want := range c.want {
				if !strings.Contains(err.Error(), want) {
					t.Fatalf("refusal %q does not name %q", err, want)
				}
			}
		})
	}
}

func TestCCFlagsNeedCgo(t *testing.T) {
	if err := checkCCFlags(false, "", true); err != nil {
		t.Fatalf("no -cc flags without -cgo: %v", err)
	}

	for _, c := range []struct {
		cc       string
		download bool
	}{{"gcc", true}, {"", false}, {ccFlagPinned, true}} {
		if err := checkCCFlags(false, c.cc, c.download); err == nil {
			t.Fatalf("-cc %q -cc-download=%v without -cgo: accepted, want a refusal", c.cc, c.download)
		}

		if err := checkCCFlags(true, c.cc, c.download); err != nil {
			t.Fatalf("-cc %q -cc-download=%v with -cgo: %v", c.cc, c.download, err)
		}
	}
}

func TestResolveCToolchainResolvesOncePerProcess(t *testing.T) {
	saved := ccFlags
	t.Cleanup(func() { ccFlags = saved; resetCToolchainResolution() })

	resetCToolchainResolution()
	ccFlags.cmd, ccFlags.download = "mycc -O0", true

	cc, env, version, err := resolveCToolchain()
	if err != nil || cc != "mycc -O0" || env != nil || !strings.Contains(version, "(-cc") {
		t.Fatalf("got %q %v %q %v; want mycc -O0, no extra environment, a version line naming -cc", cc, env, version, err)
	}

	ccFlags.cmd = "other"

	if cc, _, _, _ := resolveCToolchain(); cc != "mycc -O0" {
		t.Fatalf("a second call resolved again: %q", cc)
	}

	resetCToolchainResolution()

	if cc, _, _, _ := resolveCToolchain(); cc != "other" {
		t.Fatalf("after the reset: %q, want other", cc)
	}
}

func TestSplitCCIsCmdGosSplit(t *testing.T) {
	for _, c := range []struct {
		in   string
		want []string
	}{
		{"gcc", []string{"gcc"}},
		{"  zig  cc ", []string{"zig", "cc"}},
		{"'/a b/zig' cc", []string{"/a b/zig", "cc"}},
		{`"/o'b/zig" cc -O0`, []string{"/o'b/zig", "cc", "-O0"}},
		{"a'b c", []string{"a'b", "c"}},
	} {
		if got := splitCC(c.in); strings.Join(got, "|") != strings.Join(c.want, "|") {
			t.Fatalf("splitCC(%q) = %q, want %q", c.in, got, c.want)
		}
	}
}
