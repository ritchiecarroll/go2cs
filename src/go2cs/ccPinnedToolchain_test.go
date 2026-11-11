// ccPinnedToolchain_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"archive/zip"
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"errors"
	"net/http"
	"net/http/httptest"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

const fakeZigFile = "zig-x86_64-linux-0.17.0.zip"

// fakeZigArchive is a zip shaped like zig's: one top-level directory holding the zig executable and its lib tree.
func fakeZigArchive(t *testing.T) []byte {
	t.Helper()

	var buf bytes.Buffer
	w := zip.NewWriter(&buf)

	for name, body := range map[string]string{
		"zig-x86_64-linux-0.17.0/zig":                  "#!/bin/sh\n",
		"zig-x86_64-linux-0.17.0/lib/libc/include/x.h": "/* header */\n",
	} {
		f, err := w.Create(name)
		if err != nil {
			t.Fatal(err)
		}

		if _, err := f.Write([]byte(body)); err != nil {
			t.Fatal(err)
		}
	}

	if err := w.Close(); err != nil {
		t.Fatal(err)
	}

	return buf.Bytes()
}

func sha256Hex(data []byte) string {
	sum := sha256.Sum256(data)
	return hex.EncodeToString(sum[:])
}

// zigTestRig serves each source's archive and records what the provisioner requested and unpacked, so a test can show a
// bad archive was never unpacked and a refused install requested nothing.
type zigTestRig struct {
	z        *pinnedProvisioner
	server   *httptest.Server
	served   map[string][]byte
	requests int
	unpacked int
}

func newZigTestRig(t *testing.T, pinSHA string) *zigTestRig {
	t.Helper()

	rig := &zigTestRig{served: map[string][]byte{}}

	rig.server = httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		rig.requests++
		body, ok := rig.served[r.URL.Path]
		if !ok {
			http.NotFound(w, r)
			return
		}

		w.Write(body)
	}))

	t.Cleanup(rig.server.Close)

	rig.z = &pinnedProvisioner{
		name: ccToolchainZig, version: "0.17.0", exeRel: "zig", ccArgs: " cc", goos: "linux", goarch: "amd64",
		pin:       ccArchivePin{file: fakeZigFile, sha256: pinSHA},
		cacheRoot: filepath.Join(t.TempDir(), "toolchains"),
		client:    rig.server.Client(),
		lookPath:  exec.LookPath,
	}

	rig.z.unpack = func(archive, dest string) error {
		rig.unpacked++
		return rig.z.unpackArchive(archive, dest)
	}

	return rig
}

// serve publishes body at /<name>/<file> and returns the source.
func (rig *zigTestRig) serve(name string, body []byte) ccSource {
	path := "/" + name + "/" + fakeZigFile
	rig.served[path] = body

	return ccSource{name: name, url: rig.server.URL + path}
}

// leftovers lists what the cache root holds besides the install and zig's caches: downloads and staging directories
// must all be gone.
func leftovers(t *testing.T, cacheRoot string) []string {
	t.Helper()

	entries, err := os.ReadDir(cacheRoot)
	if err != nil && !errors.Is(err, os.ErrNotExist) {
		t.Fatal(err)
	}

	var names []string

	for _, entry := range entries {
		if strings.HasPrefix(entry.Name(), ".") {
			names = append(names, entry.Name())
		}
	}

	return names
}

func TestZigWrongHashIsDeletedAndTheNextSourceTried(t *testing.T) {
	good := fakeZigArchive(t)
	rig := newZigTestRig(t, sha256Hex(good))

	tampered := append(bytes.Clone(good), 0)
	sources := []ccSource{rig.serve("hostile-mirror", tampered), rig.serve("ziglang.org", good)}
	rig.z.sources = func() []ccSource { return sources }

	cc, env, err := rig.z.install()
	if err != nil {
		t.Fatalf("install: %v", err)
	}

	if rig.unpacked != 1 {
		t.Fatalf("unpacked %d archives; want only ziglang.org's, the one whose bytes verify", rig.unpacked)
	}

	if want := rig.z.exe() + " cc"; cc != want {
		t.Fatalf("cc = %q, want %q", cc, want)
	}

	if len(env) != 2 || !strings.HasPrefix(env[0], "ZIG_GLOBAL_CACHE_DIR="+rig.z.cacheRoot) || !strings.HasPrefix(env[1], "ZIG_LOCAL_CACHE_DIR="+rig.z.cacheRoot) {
		t.Fatalf("env = %q, want both zig caches under %s", env, rig.z.cacheRoot)
	}

	if left := leftovers(t, rig.z.cacheRoot); len(left) != 0 {
		t.Fatalf("the cache root keeps %q; every download and staging directory must be removed", left)
	}

	if _, _, ok := rig.z.cached(); !ok {
		t.Fatal("the install is not seen as cached")
	}
}

func TestZigPlantedWrongPinRefusesBeforeUnpacking(t *testing.T) {
	good := fakeZigArchive(t)
	rig := newZigTestRig(t, sha256Hex(append(bytes.Clone(good), 'x')))

	sources := []ccSource{rig.serve("mirror-a", good), rig.serve("ziglang.org", good)}
	rig.z.sources = func() []ccSource { return sources }

	_, _, err := rig.z.install()
	if err == nil {
		t.Fatal("installed an archive whose SHA-256 is not the pin")
	}

	for _, want := range []string{"mirror-a: SHA-256", "ziglang.org: SHA-256", "does not match the pinned", "every source failed", "go2cs toolchain install -cc -from FILE"} {
		if !strings.Contains(err.Error(), want) {
			t.Fatalf("refusal %q does not name %q", err, want)
		}
	}

	if rig.unpacked != 0 {
		t.Fatalf("unpacked %d archives; a hash mismatch must stop the unpack", rig.unpacked)
	}

	if _, err := os.Stat(rig.z.root()); !errors.Is(err, os.ErrNotExist) {
		t.Fatalf("an install root exists after every source failed: %v", err)
	}

	if left := leftovers(t, rig.z.cacheRoot); len(left) != 0 {
		t.Fatalf("the cache root keeps %q", left)
	}
}

func TestZigCachedNeedsTheStampOfThisPin(t *testing.T) {
	good := fakeZigArchive(t)
	rig := newZigTestRig(t, sha256Hex(good))

	if _, _, ok := rig.z.cached(); ok {
		t.Fatal("an empty cache reads as cached")
	}

	if err := os.MkdirAll(rig.z.root(), 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(rig.z.exe(), []byte("zig"), 0o755); err != nil {
		t.Fatal(err)
	}

	if _, _, ok := rig.z.cached(); ok {
		t.Fatal("an install with no stamp reads as cached")
	}

	stamp := filepath.Join(rig.z.root(), ccInstallStampSHA)

	if err := os.WriteFile(stamp, []byte(strings.Repeat("0", 64)+"\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	if _, _, ok := rig.z.cached(); ok {
		t.Fatal("an install stamped with another pin's hash reads as cached")
	}

	if err := os.WriteFile(stamp, []byte(rig.z.pin.sha256+"\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	if _, _, ok := rig.z.cached(); !ok {
		t.Fatal("an install stamped with this pin's hash is not cached")
	}
}

func TestZigSourcesAreTheMirrorsThenZiglang(t *testing.T) {
	rig := newZigTestRig(t, strings.Repeat("0", 64))
	rig.served["/mirrors.txt"] = []byte("https://a.example/zig/\n\n# comment\nhttp://plain.example\nhttps://b.example\n")
	rig.z.mirrorURL = rig.server.URL + "/mirrors.txt"

	sources := rig.z.defaultSources()

	var names []string

	for _, source := range sources {
		names = append(names, source.name)
	}

	if len(sources) != 3 || sources[2].name != "ziglang.org" || sources[2].url != "https://ziglang.org/download/0.17.0/"+fakeZigFile {
		t.Fatalf("sources = %q; want the two https mirrors, then ziglang.org's 0.17.0 archive", names)
	}

	for _, source := range sources[:2] {
		if source.url != source.name+"/"+fakeZigFile || (source.name != "https://a.example/zig" && source.name != "https://b.example") {
			t.Fatalf("mirror source %+v", source)
		}
	}

	rig.z.mirrorURL = rig.server.URL + "/missing.txt"

	if sources := rig.z.defaultSources(); len(sources) != 1 || sources[0].name != "ziglang.org" {
		t.Fatalf("with no mirror list, sources = %+v; want ziglang.org alone", sources)
	}
}

func TestQuoteCCWord(t *testing.T) {
	for _, c := range []struct {
		in, want string
		ok       bool
	}{
		{"/cache/zig", "/cache/zig", true},
		{`C:\Users\A B\zig.exe`, `'C:\Users\A B\zig.exe'`, true},
		{"/o'brien/zig", `"/o'brien/zig"`, true},
		{`/a "b"/zig`, `'/a "b"/zig'`, true},
		{`/a'b"/zig`, "", false},
	} {
		if got, ok := quoteCCWord(c.in); got != c.want || ok != c.ok {
			t.Fatalf("quoteCCWord(%q) = %q, %v; want %q, %v", c.in, got, ok, c.want, c.ok)
		}
	}
}

// GNU tar execs an `xz` binary that slim images lack, so a linux host without one is refused BEFORE any download.
func TestZigNeedsXzBeforeDownloadingOnLinux(t *testing.T) {
	rig := newZigTestRig(t, strings.Repeat("0", 64))
	rig.z.pin.file = "zig-x86_64-linux-0.17.0.tar.xz"
	source := rig.serve("ziglang.org", []byte("never fetched"))
	rig.z.sources = func() []ccSource { return []ccSource{source} }
	rig.z.lookPath = func(name string) (string, error) {
		if name == "xz" {
			return "", errors.New("not found")
		}

		return "/usr/bin/" + name, nil
	}

	_, _, err := rig.z.install()

	want := "the pinned zig needs xz to unpack: install xz-utils, or pass -cc / CC, or `go2cs toolchain install -cc -from FILE` with an archive unpacked elsewhere"

	if err == nil || err.Error() != want {
		t.Fatalf("install = %v; want %q", err, want)
	}

	if rig.requests != 0 {
		t.Fatalf("%d requests; the refusal must come before any download", rig.requests)
	}

	// macOS's bsdtar reads xz itself, so darwin needs tar alone.
	rig.z.goos = "darwin"

	if err := rig.z.checkUnpackTools(); err != nil {
		t.Fatalf("darwin with tar and no xz: %v", err)
	}
}

// A user whose cache path holds a space (a Windows profile "C:\Users\A B") must get a CC that cmd/go splits back into
// the zig executable and `cc`; a wrong quote reads as "compiler not found" for exactly those users.
func TestZigCCSurvivesASpaceInTheCacheRoot(t *testing.T) {
	for _, root := range []string{filepath.Join(t.TempDir(), "A B", "toolchains"), filepath.Join(t.TempDir(), "o'brien x", "toolchains")} {
		z := newPinnedProvisionerFor(ccToolchainZig, "linux", "amd64", root)

		cc, _, err := z.choice()
		if err != nil {
			t.Fatal(err)
		}

		if fields := splitCC(cc); len(fields) != 2 || fields[0] != z.exe() || fields[1] != "cc" {
			t.Fatalf("cc %q splits into %q; want [%q cc]", cc, fields, z.exe())
		}
	}
}

// The system-tar path, end to end, where this host has tar and xz: the archive's top-level directory is dropped.
func TestZigUnpacksATarXzThroughTheSystemTar(t *testing.T) {
	for _, tool := range []string{"tar", "xz"} {
		if _, err := exec.LookPath(tool); err != nil {
			t.Skipf("no %s on this host", tool)
		}
	}

	work := t.TempDir()
	top := filepath.Join(work, "zig-x86_64-linux-0.17.0")

	if err := os.MkdirAll(filepath.Join(top, "lib"), 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(top, "zig"), []byte("#!/bin/sh\n"), 0o755); err != nil {
		t.Fatal(err)
	}

	archive := filepath.Join(work, "zig.tar.xz")

	if out, err := exec.Command("tar", "-cJf", archive, "-C", work, "zig-x86_64-linux-0.17.0").CombinedOutput(); err != nil {
		t.Fatalf("building the fixture: %v: %s", err, out)
	}

	dest := filepath.Join(work, "dest")

	if err := os.MkdirAll(dest, 0o755); err != nil {
		t.Fatal(err)
	}

	if err := unpackTarXz(archive, dest, "zig-x86_64-linux-0.17.0/"); err != nil {
		t.Fatal(err)
	}

	if _, err := os.Stat(filepath.Join(dest, "zig")); err != nil {
		t.Fatalf("the zig executable is not at the top of the unpacked tree: %v", err)
	}
}

// install -cc -from takes an archive the user already has, only when it matches the pin, and downloads nothing.
func TestZigInstallFromAFileTakesOnlyThePinnedArchive(t *testing.T) {
	good := fakeZigArchive(t)
	rig := newZigTestRig(t, sha256Hex(good))
	rig.z.sources = func() []ccSource { return []ccSource{rig.serve("ziglang.org", good)} }

	dir := t.TempDir()
	goodFile, badFile := filepath.Join(dir, "good.zip"), filepath.Join(dir, "bad.zip")

	if err := os.WriteFile(goodFile, good, 0o644); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(badFile, append(bytes.Clone(good), 0), 0o644); err != nil {
		t.Fatal(err)
	}

	if err := rig.z.installCommand(badFile); err == nil || !strings.Contains(err.Error(), "is not the pinned zig") {
		t.Fatalf("install -from a wrong archive: %v; want a refusal", err)
	}

	if err := rig.z.installCommand(goodFile); err != nil {
		t.Fatalf("install -from the pinned archive: %v", err)
	}

	if _, _, ok := rig.z.cached(); !ok {
		t.Fatal("install -from left no whole install")
	}

	if err := rig.z.installCommand(badFile); err == nil {
		t.Fatal("install -from a wrong archive over a whole install was reported fine")
	}

	if rig.requests != 0 {
		t.Fatalf("install -from made %d requests; it must download nothing", rig.requests)
	}
}

// The default pin is PER OS (owner ruling, PLAN-cgo-interop §5.1): llvm-mingw on windows, zig on linux and darwin. A
// named pin is reachable wherever this go2cs carries one for the platform, and nowhere else.
func TestTheDefaultPinIsPerOS(t *testing.T) {
	root := t.TempDir()

	for _, c := range []struct {
		name, goos, goarch string
		want, file, exeRel string
	}{
		{"", "windows", "amd64", ccToolchainLLVMMingw, "llvm-mingw-20261020-ucrt-x86_64.zip", "bin/x86_64-w64-mingw32-clang.exe"},
		{"", "linux", "amd64", ccToolchainZig, "zig-x86_64-linux-0.17.0.tar.xz", "zig"},
		{"", "darwin", "arm64", ccToolchainZig, "zig-aarch64-macos-0.17.0.tar.xz", "zig"},
		{ccToolchainZig, "windows", "amd64", ccToolchainZig, "zig-x86_64-windows-0.17.0.zip", "zig.exe"},
	} {
		z := newPinnedProvisionerFor(c.name, c.goos, c.goarch, root)
		if z == nil || z.name != c.want || z.pin.file != c.file || z.exeRel != c.exeRel {
			t.Fatalf("%q on %s/%s: %+v; want %s %s %s", c.name, c.goos, c.goarch, z, c.want, c.file, c.exeRel)
		}
	}

	// No pin, no provisioner: llvm-mingw is pinned for windows-amd64 only, so windows-arm64 has no default pin.
	for _, c := range [][3]string{{"", "windows", "arm64"}, {ccToolchainLLVMMingw, "linux", "amd64"}, {"gcc", "linux", "amd64"}} {
		if z := newPinnedProvisionerFor(c[0], c[1], c[2], root); z != nil {
			t.Fatalf("%q on %s/%s: %+v; want no pin", c[0], c[1], c[2], z)
		}
	}
}

// llvm-mingw's CC is its compiler alone, with no zig caches, and its only source is the GitHub release.
func TestLLVMMingwIsItsCompilerAloneFromItsRelease(t *testing.T) {
	z := newPinnedProvisionerFor(ccToolchainLLVMMingw, "windows", "amd64", filepath.Join(t.TempDir(), "A B"))

	cc, env, err := z.choice()
	if err != nil || env != nil {
		t.Fatalf("choice = %q %v %v; want the compiler and no environment", cc, env, err)
	}

	if fields := splitCC(cc); len(fields) != 1 || fields[0] != z.exe() || !strings.HasSuffix(fields[0], filepath.FromSlash("bin/x86_64-w64-mingw32-clang.exe")) {
		t.Fatalf("cc %q splits into %q; want the one compiler path", cc, fields)
	}

	sources := z.defaultSources()
	if len(sources) != 1 || sources[0].url != "https://github.com/mstorsjo/llvm-mingw/releases/download/20261020/llvm-mingw-20261020-ucrt-x86_64.zip" {
		t.Fatalf("sources = %+v; want the GitHub release alone", sources)
	}
}

// An llvm-mingw archive installs end to end: the pinned hash, the top-level directory dropped, the compiler under bin.
func TestLLVMMingwInstallsThroughTheSamePath(t *testing.T) {
	var buf bytes.Buffer
	w := zip.NewWriter(&buf)

	for _, name := range []string{"llvm-mingw-20261020-ucrt-x86_64/bin/x86_64-w64-mingw32-clang.exe", "llvm-mingw-20261020-ucrt-x86_64/include/stdio.h"} {
		f, err := w.Create(name)
		if err != nil {
			t.Fatal(err)
		}

		f.Write([]byte("x"))
	}

	if err := w.Close(); err != nil {
		t.Fatal(err)
	}

	archive := buf.Bytes()
	rig := newZigTestRig(t, sha256Hex(archive))
	llvm := newPinnedProvisionerFor(ccToolchainLLVMMingw, "windows", "amd64", rig.z.cacheRoot)
	llvm.pin.sha256 = sha256Hex(archive)
	llvm.client = rig.z.client
	path := "/release/llvm-mingw-20261020-ucrt-x86_64.zip"
	rig.served[path] = archive
	llvm.sources = func() []ccSource { return []ccSource{{name: "release", url: rig.server.URL + path}} }

	cc, env, err := llvm.install()
	if err != nil {
		t.Fatal(err)
	}

	if want, _ := quoteCCWord(llvm.exe()); cc != want || env != nil {
		t.Fatalf("install = %q %v; want %q and no environment", cc, env, want)
	}

	if _, _, ok := llvm.cached(); !ok {
		t.Fatal("the llvm-mingw install is not seen as cached")
	}
}
