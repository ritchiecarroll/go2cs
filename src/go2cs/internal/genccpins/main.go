// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// genccpins writes src/go2cs/ccToolchainPins.go: the pinned C toolchains a -cgo conversion provisions when no host
// compiler resolves (ccPinnedToolchain.go), and the SHA-256 it accepts for each archive.
//   - zig, the default on linux and darwin, for each of the six release platforms: read from ziglang.org's download
//     index (https://ziglang.org/download/index.json). Run from src/go2cs through `go generate .` on a host that
//     reaches ziglang.org, or with -index naming a saved copy.
//   - llvm-mingw, the default on windows (owner ruling, PLAN-cgo-interop §5.1), for windows-amd64: its publisher signs
//     nothing and publishes no index, so the pin is the GitHub release asset's SHA-256 digest, held below as
//     constants. It is the hash the converter-cgo instrument verified on hosted runners (.github/workflows/os-matrix.yml),
//     and main_test.go fails when the two disagree.
//
// main_test.go also fails while ccToolchainPins.go is not what this writes from testdata/cctoolchain's saved index.
package main

import (
	"bytes"
	"encoding/json"
	"flag"
	"fmt"
	"go/format"
	"io"
	"net/http"
	"os"
	"sort"
	"strings"
	"time"
)

// zigVersion is the pinned release. Moving it is a converter change: edit it, refresh the saved index, regenerate.
const zigVersion = "0.17.0"

// The llvm-mingw release and its windows archive: the UCRT x86_64 build, 190,612,575 bytes. Moving it is a converter
// change: edit these three, refresh the converter-cgo instrument's pin table to match, regenerate.
const (
	llvmMingwVersion = "20261020"
	llvmMingwFile    = "llvm-mingw-20261020-ucrt-x86_64.zip"
	llvmMingwSHA256  = "fd5462b7f5d93b2ff981fcd3f9c7e427e47f3203c64ea91c27bcc444eebd74cd"
)

const indexURL = "https://ziglang.org/download/index.json"

// platforms maps each Go platform go2cs ships for to the key ziglang.org's index files its archive under.
var platforms = map[string]string{
	"darwin-amd64":  "x86_64-macos",
	"darwin-arm64":  "aarch64-macos",
	"linux-amd64":   "x86_64-linux",
	"linux-arm64":   "aarch64-linux",
	"windows-amd64": "x86_64-windows",
	"windows-arm64": "aarch64-windows",
}

type indexArchive struct {
	Tarball string `json:"tarball"`
	Shasum  string `json:"shasum"`
}

func main() {
	index := flag.String("index", "", "a saved copy of ziglang.org's download index; fetched when empty")
	output := flag.String("o", "ccToolchainPins.go", "the file to write")
	flag.Parse()

	if err := run(*index, *output); err != nil {
		fmt.Fprintln(os.Stderr, "genccpins:", err)
		os.Exit(1)
	}
}

func run(indexFile, output string) error {
	data, err := readIndex(indexFile)
	if err != nil {
		return err
	}

	pins, err := pinsFromIndex(data)
	if err != nil {
		return err
	}

	source, err := render(pins)
	if err != nil {
		return err
	}

	return os.WriteFile(output, source, 0o644)
}

func readIndex(indexFile string) ([]byte, error) {
	if indexFile != "" {
		return os.ReadFile(indexFile)
	}

	client := &http.Client{Timeout: 2 * time.Minute}

	response, err := client.Get(indexURL)
	if err != nil {
		return nil, err
	}

	defer response.Body.Close()

	if response.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("GET %s: %s", indexURL, response.Status)
	}

	return io.ReadAll(response.Body)
}

// pinsFromIndex returns platform -> {archive file name, sha256} for the pinned release, refusing an index that lacks
// the release, any platform, or a well-formed hash.
func pinsFromIndex(data []byte) (map[string][2]string, error) {
	var index map[string]map[string]json.RawMessage

	if err := json.Unmarshal(data, &index); err != nil {
		return nil, fmt.Errorf("the index does not parse: %w", err)
	}

	release, ok := index[zigVersion]
	if !ok {
		return nil, fmt.Errorf("the index has no %s release", zigVersion)
	}

	pins := map[string][2]string{}

	for platform, key := range platforms {
		raw, ok := release[key]
		if !ok {
			return nil, fmt.Errorf("the %s release has no %s archive (for %s)", zigVersion, key, platform)
		}

		var archive indexArchive

		if err := json.Unmarshal(raw, &archive); err != nil {
			return nil, fmt.Errorf("the %s entry does not parse: %w", key, err)
		}

		file := archive.Tarball[strings.LastIndex(archive.Tarball, "/")+1:]

		if len(archive.Shasum) != 64 || strings.Trim(archive.Shasum, "0123456789abcdef") != "" {
			return nil, fmt.Errorf("the %s entry's shasum %q is not a SHA-256", key, archive.Shasum)
		}

		if !strings.HasSuffix(file, ".zip") && !strings.HasSuffix(file, ".tar.xz") {
			return nil, fmt.Errorf("the %s archive %q is not a .zip or .tar.xz", key, file)
		}

		pins[platform] = [2]string{file, archive.Shasum}
	}

	return pins, nil
}

func render(pins map[string][2]string) ([]byte, error) {
	var out bytes.Buffer

	fmt.Fprintf(&out, `// ccToolchainPins.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Code generated by genccpins from ziglang.org's download index and the llvm-mingw release digest; DO NOT EDIT.

// The zig release a -cgo conversion provisions as its C compiler when no host compiler resolves (ccPinnedToolchain.go),
// and the SHA-256 it accepts for each archive. Regenerate with "go generate ." from src/go2cs after genccpins'
// zigVersion moves, and refresh testdata/cctoolchain from the same index (internal/genccpins/main_test.go says how).

//go:generate go run ./internal/genccpins -o ccToolchainPins.go

package main

const pinnedZigVersion = %q

type ccArchivePin struct {
	file   string
	sha256 string
}

var pinnedZigArchives = map[string]ccArchivePin{
`, zigVersion)

	keys := make([]string, 0, len(pins))

	for key := range pins {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	for _, platform := range keys {
		fmt.Fprintf(&out, "\t%q: {%q, %q},\n", platform, pins[platform][0], pins[platform][1])
	}

	fmt.Fprintf(&out, `}

// llvm-mingw has no publisher signature: the pin is the GitHub release asset's SHA-256 digest alone.
const pinnedLLVMMingwVersion = %q

var pinnedLLVMMingwArchives = map[string]ccArchivePin{
	"windows-amd64": {%q, %q},
}
`, llvmMingwVersion, llvmMingwFile, llvmMingwSHA256)

	return format.Source(out.Bytes())
}
