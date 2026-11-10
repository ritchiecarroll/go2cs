// ccZigProvision_test.go - Gbtc
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

// zigTestRig serves each source's archive (and a signature beside it) and records what the provisioner verified and
// unpacked, so a test can show a bad archive was never unpacked.
type zigTestRig struct {
	z        *zigProvisioner
	server   *httptest.Server
	served   map[string][]byte
	verified []string
	unpacked int
}

func newZigTestRig(t *testing.T, pinSHA string) *zigTestRig {
	t.Helper()

	rig := &zigTestRig{served: map[string][]byte{}}

	rig.server = httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		body, ok := rig.served[r.URL.Path]
		if !ok {
			http.NotFound(w, r)
			return
		}

		w.Write(body)
	}))

	t.Cleanup(rig.server.Close)

	rig.z = &zigProvisioner{
		version: "0.17.0", goos: "linux", goarch: "amd64",
		pin:       zigArchivePin{file: fakeZigFile, sha256: pinSHA},
		cacheRoot: filepath.Join(t.TempDir(), "toolchains"),
		client:    rig.server.Client(),
	}

	rig.z.verifySignature = func(archive, signature string) error {
		data, err := os.ReadFile(signature)
		rig.verified = append(rig.verified, string(data))

		return err
	}

	rig.z.unpack = func(archive, dest string) error {
		rig.unpacked++
		return rig.z.unpackArchive(archive, dest)
	}

	return rig
}

// serve publishes body at /<name>/<file>, with a signature naming the source, and returns the source.
func (rig *zigTestRig) serve(name string, body []byte) zigSource {
	path := "/" + name + "/" + fakeZigFile
	rig.served[path] = body
	rig.served[path+".minisig"] = []byte("signature from " + name)

	return zigSource{name: name, url: rig.server.URL + path}
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
	sources := []zigSource{rig.serve("hostile-mirror", tampered), rig.serve("ziglang.org", good)}
	rig.z.sources = func() []zigSource { return sources }

	cc, env, err := rig.z.install()
	if err != nil {
		t.Fatalf("install: %v", err)
	}

	if rig.unpacked != 1 || len(rig.verified) != 1 || rig.verified[0] != "signature from ziglang.org" {
		t.Fatalf("unpacked %d, signatures checked %q; want only ziglang.org's archive checked and unpacked", rig.unpacked, rig.verified)
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

	sources := []zigSource{rig.serve("mirror-a", good), rig.serve("ziglang.org", good)}
	rig.z.sources = func() []zigSource { return sources }

	_, _, err := rig.z.install()
	if err == nil {
		t.Fatal("installed an archive whose SHA-256 is not the pin")
	}

	for _, want := range []string{"mirror-a: SHA-256", "ziglang.org: SHA-256", "does not match the pinned", "every source failed", "go2cs toolchain install -cc -from FILE"} {
		if !strings.Contains(err.Error(), want) {
			t.Fatalf("refusal %q does not name %q", err, want)
		}
	}

	if rig.unpacked != 0 || len(rig.verified) != 0 {
		t.Fatalf("unpacked %d, signatures checked %d; a hash mismatch must stop both", rig.unpacked, len(rig.verified))
	}

	if _, err := os.Stat(rig.z.root()); !errors.Is(err, os.ErrNotExist) {
		t.Fatalf("an install root exists after every source failed: %v", err)
	}

	if left := leftovers(t, rig.z.cacheRoot); len(left) != 0 {
		t.Fatalf("the cache root keeps %q", left)
	}
}

func TestZigRefusedSignatureInstallsNothing(t *testing.T) {
	good := fakeZigArchive(t)
	rig := newZigTestRig(t, sha256Hex(good))

	sources := []zigSource{rig.serve("ziglang.org", good)}
	rig.z.sources = func() []zigSource { return sources }
	rig.z.verifySignature = func(string, string) error { return errors.New("signature does not verify") }

	if _, _, err := rig.z.install(); err == nil || !strings.Contains(err.Error(), "ziglang.org: signature does not verify") {
		t.Fatalf("install = %v; want the signature refusal named with its source", err)
	}

	if rig.unpacked != 0 {
		t.Fatalf("unpacked %d archives whose signature was refused", rig.unpacked)
	}
}

func TestZigMissingSignatureIsASourceFailure(t *testing.T) {
	good := fakeZigArchive(t)
	rig := newZigTestRig(t, sha256Hex(good))

	source := rig.serve("mirror-a", good)
	delete(rig.served, "/mirror-a/"+fakeZigFile+".minisig")
	rig.z.sources = func() []zigSource { return []zigSource{source} }

	if _, _, err := rig.z.install(); err == nil || !strings.Contains(err.Error(), "mirror-a: its signature: GET") {
		t.Fatalf("install = %v; want the missing signature named", err)
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

	stamp := filepath.Join(rig.z.root(), zigInstallStampSHA)

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
