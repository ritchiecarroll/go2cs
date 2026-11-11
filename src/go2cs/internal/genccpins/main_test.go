// main_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"crypto/sha256"
	"encoding/hex"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// The saved index is ziglang.org's download/index.json as read on 2026-11-09, stored verbatim: the generator's input
// is only as good as the copy, so the copy is pinned by its own hash. Refresh it from the live index in the same
// change that moves zigVersion, and update this hash with it.
var savedIndex = filepath.Join("..", "..", "testdata", "cctoolchain", "zig-index-"+zigVersion+".json")

const savedIndexSHA256 = "81080b1daf59c569e04ebd90c3a9d2f87bbfd25716be61042c9602d3dbd44a3b"

// The four archives the scout's instrument downloaded and verified on hosted runners (os-matrix.yml's converter-cgo
// pin table). The generator must reproduce exactly these from the index: its positive control.
var instrumentPins = map[string]string{
	"windows-amd64": "b5663f69581dcf391293fbf16c06cb80d81d806545ce618b4d0bab7f0eb8c428",
	"linux-amd64":   "1cbe9df9f27e6b78d14ccbca43b6703a404ef79ef1c463de901d7f088d4e2026",
	"darwin-amd64":  "4f9a1c5269aa17ebda5e6d3c2b89d6cbf36f7d2b22a0306e9ab98f25f95529c6",
	"darwin-arm64":  "b607e9b9234790a008116ae5bdb71c6243b84b9fb42a53a9e70fde41c06c536a",
}

func TestTheSavedIndexIsTheCopyOfRecord(t *testing.T) {
	data, err := os.ReadFile(savedIndex)
	if err != nil {
		t.Fatal(err)
	}

	sum := sha256.Sum256(data)

	if got := hex.EncodeToString(sum[:]); got != savedIndexSHA256 || len(data) != 90260 {
		t.Fatalf("%s is %d bytes with SHA-256 %s; the copy of record is 90260 bytes, %s", savedIndex, len(data), got, savedIndexSHA256)
	}
}

func TestTheGeneratorReproducesTheInstrumentPins(t *testing.T) {
	data, err := os.ReadFile(savedIndex)
	if err != nil {
		t.Fatal(err)
	}

	pins, err := pinsFromIndex(data)
	if err != nil {
		t.Fatal(err)
	}

	for platform, want := range instrumentPins {
		if pins[platform][1] != want {
			t.Fatalf("%s: the generator reads %q, the instrument verified %s", platform, pins[platform], want)
		}
	}

	if len(pins) != len(platforms) {
		t.Fatalf("pinned %d platforms, want %d", len(pins), len(platforms))
	}
}

// ccToolchainPins.go is generated, so it must be exactly what the generator writes from the saved index.
func TestCCToolchainPinsAreWhatTheGeneratorWrites(t *testing.T) {
	output := filepath.Join(t.TempDir(), "ccToolchainPins.go")

	if err := run(savedIndex, output); err != nil {
		t.Fatal(err)
	}

	want, err := os.ReadFile(output)
	if err != nil {
		t.Fatal(err)
	}

	got, err := os.ReadFile(filepath.Join("..", "..", "ccToolchainPins.go"))
	if err != nil {
		t.Fatal(err)
	}

	if !bytes.Equal(bytes.ReplaceAll(got, []byte("\r\n"), []byte("\n")), want) {
		t.Fatalf("src/go2cs/ccToolchainPins.go is not what genccpins writes from the saved index; regenerate it:\n%s", want)
	}
}

// The negative control: one changed byte in a pinned hash changes the generated pins, and a malformed hash refuses.
func TestAOneByteEditOfTheIndexChangesThePins(t *testing.T) {
	data, err := os.ReadFile(savedIndex)
	if err != nil {
		t.Fatal(err)
	}

	edited := bytes.Replace(data, []byte(instrumentPins["linux-amd64"]), []byte("0"+instrumentPins["linux-amd64"][1:]), 1)

	if bytes.Equal(edited, data) {
		t.Fatal("the saved index does not hold linux-amd64's hash; the edit changed nothing")
	}

	pins, err := pinsFromIndex(edited)
	if err != nil {
		t.Fatal(err)
	}

	if pins["linux-amd64"][1] == instrumentPins["linux-amd64"] {
		t.Fatal("an edited index produced the same linux-amd64 pin")
	}

	malformed := bytes.Replace(data, []byte(instrumentPins["darwin-arm64"]), []byte("not-a-hash"), 1)

	if _, err := pinsFromIndex(malformed); err == nil || !strings.Contains(err.Error(), "aarch64-macos") {
		t.Fatalf("a malformed hash: %v; want a refusal naming aarch64-macos", err)
	}
}

// llvm-mingw's pin is held as constants (no index to read), so it is checked against the instrument that verified it
// on hosted runners: the converter-cgo step's pin table in os-matrix.yml.
func TestTheLLVMMingwPinIsTheInstruments(t *testing.T) {
	data, err := os.ReadFile(filepath.Join("..", "..", "..", "..", ".github", "workflows", "os-matrix.yml"))
	if err != nil {
		t.Fatal(err)
	}

	for _, line := range strings.Split(string(data), "\n") {
		if strings.Contains(line, "'llvm-mingw|win-x64'") {
			if !strings.Contains(line, "/"+llvmMingwFile+"\"") || !strings.Contains(line, "sha256 = '"+llvmMingwSHA256+"'") {
				t.Fatalf("the instrument pins %q; genccpins pins %s %s", strings.TrimSpace(line), llvmMingwFile, llvmMingwSHA256)
			}

			return
		}
	}

	t.Fatal("os-matrix.yml has no llvm-mingw|win-x64 pin; the guard proves nothing")
}
