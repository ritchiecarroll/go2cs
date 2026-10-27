// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// gentoolchainpins writes src/go2cs/toolchainPins.go: the Go release a release build of go2cs downloads
// on first use (go.mod's go line) and the hashes it accepts for each of the six release platforms, the
// release archives' SHA-256 values from go.dev's JSON index and the golang.org/toolchain modules' h1:
// values from sum.golang.org. Run from src/go2cs through `go generate .` on a host that reaches both;
// -dl-json reads a saved copy of the go.dev index instead (https://go.dev/dl/?mode=json&include=all).
// toolchainProvision_test.go fails while the pins describe another release than go.mod's, and main_test.go
// while toolchainPins.go is not what this writes from testdata/, a saved slice of both sources.
package main

import (
	"bufio"
	"bytes"
	"encoding/json"
	"flag"
	"fmt"
	"go/format"
	"io"
	"net/http"
	"os"
	"regexp"
	"sort"
	"strings"
	"time"

	"golang.org/x/mod/modfile"
)

// platforms are the six the release ships go2cs for, as go.dev and the toolchain module name them.
var platforms = []string{"darwin-amd64", "darwin-arm64", "linux-amd64", "linux-arm64", "windows-amd64", "windows-arm64"}

type dlFile struct {
	Filename string `json:"filename"`
	OS       string `json:"os"`
	Arch     string `json:"arch"`
	SHA256   string `json:"sha256"`
	Kind     string `json:"kind"`
}

type dlRelease struct {
	Version string   `json:"version"`
	Files   []dlFile `json:"files"`
}

var (
	sha256Pattern = regexp.MustCompile(`^[0-9a-f]{64}$`)
	h1Pattern     = regexp.MustCompile(`^h1:[A-Za-z0-9+/]{43}=$`)
)

func main() {
	dlJSON := flag.String("dl-json", "https://go.dev/dl/?mode=json&include=all", "go.dev's release index: a URL or a saved file")
	sumDB := flag.String("sumdb", "https://sum.golang.org", "the checksum database recording the golang.org/toolchain modules: a URL or a saved lookup/ tree")
	output := flag.String("o", "toolchainPins.go", "the file to write")
	flag.Parse()

	if err := run("go.mod", *dlJSON, *sumDB, *output); err != nil {
		fmt.Fprintf(os.Stderr, "gentoolchainpins: %v\n", err)
		os.Exit(1)
	}
}

// run reads go.mod's go line, the index at dlJSON and the checksum database at sumDB (an https:// URL, or a
// directory holding lookup/<module>@<version> files), and writes the pins to output.
func run(goMod, dlJSON, sumDB, output string) error {
	data, err := os.ReadFile(goMod)
	if err != nil {
		return err
	}

	parsed, err := modfile.Parse(goMod, data, nil)
	if err != nil {
		return err
	}

	version := "go" + parsed.Go.Version

	index, err := read(dlJSON)
	if err != nil {
		return fmt.Errorf("reading the go.dev index: %w (pass -dl-json with a saved copy)", err)
	}

	var releases []dlRelease

	if err := json.Unmarshal(index, &releases); err != nil {
		return fmt.Errorf("parsing the go.dev index: %w", err)
	}

	archives, err := archivePins(releases, version)
	if err != nil {
		return err
	}

	modules := map[string]string{}

	for _, platform := range platforms {
		sum, err := modulePin(sumDB, version, platform)
		if err != nil {
			return err
		}

		modules[platform] = sum
	}

	source, err := render(version, archives, modules)
	if err != nil {
		return err
	}

	if err := os.WriteFile(output, source, 0o644); err != nil {
		return err
	}

	fmt.Printf("gentoolchainpins: wrote %s (%s, %d archives, %d modules)\n", output, version, len(archives), len(modules))

	return nil
}

func read(location string) ([]byte, error) {
	if !strings.HasPrefix(location, "https://") {
		return os.ReadFile(location)
	}

	client := &http.Client{Timeout: 2 * time.Minute}

	response, err := client.Get(location)
	if err != nil {
		return nil, err
	}

	defer response.Body.Close()

	if response.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("GET %s: %s", location, response.Status)
	}

	return io.ReadAll(response.Body)
}

// archivePins picks each platform's release archive (kind "archive": a .tar.gz, or a .zip on Windows).
func archivePins(releases []dlRelease, version string) (map[string][2]string, error) {
	for _, release := range releases {
		if release.Version != version {
			continue
		}

		pins := map[string][2]string{}

		for _, file := range release.Files {
			platform := file.OS + "-" + file.Arch

			if file.Kind != "archive" || !contains(platforms, platform) {
				continue
			}

			if !sha256Pattern.MatchString(file.SHA256) {
				return nil, fmt.Errorf("%s: %q is not a SHA-256", file.Filename, file.SHA256)
			}

			pins[platform] = [2]string{file.Filename, file.SHA256}
		}

		for _, platform := range platforms {
			if _, ok := pins[platform]; !ok {
				return nil, fmt.Errorf("the go.dev index lists no %s archive for %s", platform, version)
			}
		}

		return pins, nil
	}

	return nil, fmt.Errorf("the go.dev index does not list %s", version)
}

// modulePin reads the toolchain module's h1: line from the checksum database's lookup endpoint.
func modulePin(sumDB, version, platform string) (string, error) {
	module := "golang.org/toolchain@v0.0.1-" + version + "." + platform

	body, err := read(strings.TrimRight(sumDB, "/") + "/lookup/" + module)
	if err != nil {
		return "", err
	}

	want := "golang.org/toolchain v0.0.1-" + version + "." + platform + " "
	scanner := bufio.NewScanner(bytes.NewReader(body))

	for scanner.Scan() {
		if sum, ok := strings.CutPrefix(scanner.Text(), want); ok && h1Pattern.MatchString(sum) {
			return sum, nil
		}
	}

	return "", fmt.Errorf("%s: no h1: line in the checksum database's answer", module)
}

func render(version string, archives map[string][2]string, modules map[string]string) ([]byte, error) {
	var out bytes.Buffer

	fmt.Fprintf(&out, `// toolchainPins.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Code generated by gentoolchainpins from go.mod, go.dev's release index and sum.golang.org; DO NOT EDIT.

// The Go release a release build of go2cs downloads on first use (toolchainProvision.go), and the
// hashes it accepts: the release archives' SHA-256 values from go.dev's JSON index and the
// golang.org/toolchain modules' h1: values recorded by sum.golang.org. Regenerate with
// "go generate ." from src/go2cs after the go line in go.mod moves, and refresh
// internal/gentoolchainpins/testdata from the same sources (its main_test.go says how).

//go:generate go run ./internal/gentoolchainpins

package main

const pinnedGoVersion = %q

type goArchivePin struct {
	file   string
	sha256 string
}

var pinnedGoArchives = map[string]goArchivePin{
`, version)

	for _, platform := range sortedKeys(archives) {
		fmt.Fprintf(&out, "\t%q: {%q, %q},\n", platform, archives[platform][0], archives[platform][1])
	}

	out.WriteString("}\n\nvar pinnedGoModules = map[string]string{\n")

	for _, platform := range sortedKeys(modules) {
		fmt.Fprintf(&out, "\t%q: %q,\n", platform, modules[platform])
	}

	out.WriteString("}\n")

	return format.Source(out.Bytes())
}

func sortedKeys[V any](m map[string]V) []string {
	keys := make([]string, 0, len(m))

	for key := range m {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	return keys
}

func contains(list []string, value string) bool {
	for _, item := range list {
		if item == value {
			return true
		}
	}

	return false
}
