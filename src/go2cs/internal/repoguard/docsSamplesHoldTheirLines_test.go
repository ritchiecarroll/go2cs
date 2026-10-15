// docsSamplesHoldTheirLines_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"bytes"
	"errors"
	"go/build"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"sort"
	"strconv"
	"strings"
	"testing"
)

// A code sample's source comment must cite the lines that hold the sample. The pages show converted code
// and Go code under `<!-- source: <file>:<lines> -->`, and src/tools/docsamples regenerates each sample
// from its file. Nothing failed when a citation went stale: run at one ref, docsamples files every sample
// it cannot place as a NOTE and exits 0, and the provenance-note guard skips source comments by design.
// So when the Go 1.26 hop's regeneration moved the corpus under the pages, about fifty citations kept
// their old numbers while their samples sat a few lines away, and every instrument read green.
//
// WHAT IS CHECKED: docsamples -recite, dry, at HEAD over the living pages, with GOROOT/ comments read
// from the toolchain running this test (the repository's pinned one). -recite re-cites a comment exactly
// when every run of its sample is in the cited file as written, in order, but not inside the cited lines:
// the sample is its own quoted anchor, so a moved citation is caught by content, not just existence. Each
// row it would re-cite fails here, unless the known-list (testdata/docsamples-known-recite.txt) names it.
// The known-list only shrinks: a row docsamples no longer re-cites fails too.
//
// WHAT IS NOT: a sample that is abridged or hand-edited (a run not in the file as written) is not
// re-citable, so it stays a note and passes. docsamples' own TestHandEditsAreRefusedAndWrongLinesAreRecited
// holds that line. A docsamples ERROR (a sample whose file changed between refs) cannot occur at one ref.
//
// POSITIVE CONTROL: verified red-first when this guard landed. Moving the citation at
// docs/ConversionStrategies-Reference/comments.md:96 two lines down failed this test at that line, where
// plain docsamples had reported a note and exited 0.
func TestDocsSamplesHoldTheirLines(t *testing.T) {
	root := repoRootFromPackageDir(t)
	goroot := build.Default.GOROOT

	if goroot == "" {
		goroot = runtime.GOROOT()
	}

	exe := filepath.Join(t.TempDir(), "docsamples")

	if runtime.GOOS == "windows" {
		exe += ".exe"
	}

	compile := exec.Command(filepath.Join(goroot, "bin", "go"), "build", "-o", exe, ".")
	compile.Dir = filepath.Join(root, "src", "tools", "docsamples")

	if out, err := compile.CombinedOutput(); err != nil {
		t.Fatalf("building src/tools/docsamples: %v\n%s", err, out)
	}

	run := exec.Command(exe, "-recite", "-goroot", goroot)
	run.Dir = root
	var stdout, stderr bytes.Buffer
	run.Stdout, run.Stderr = &stdout, &stderr

	if err := run.Run(); err != nil {
		var exit *exec.ExitError

		if !errors.As(err, &exit) {
			t.Fatalf("running docsamples: %v", err)
		}

		t.Fatalf("docsamples exited %d (1: a stale sample it cannot regenerate; 2: it could not run):\n%s%s",
			exit.ExitCode(), stdout.String(), stderr.String())
	}

	// The summary line is the liveness check: no line, or no comment read, and nothing above was tested.
	summary := regexp.MustCompile(`(?m)^docsamples: .*: \d+ page\(s\) read, (\d+) source comment\(s\)`).FindStringSubmatch(stdout.String())

	if summary == nil || summary[1] == "0" {
		t.Fatalf("docsamples read no source comment; its output:\n%s%s", stdout.String(), stderr.String())
	}

	recited := map[string][]string{} // page<TAB>citation -> the page lines carrying it
	rows := regexp.MustCompile(`(?m)^RECITED\s+(\S+):(\d+)\s*$`).FindAllStringSubmatch(stdout.String(), -1)
	pages := map[string][]string{}

	for _, r := range rows {
		page, line := r[1], r[2]

		if pages[page] == nil {
			data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(page)))

			if err != nil {
				t.Fatalf("reading %s: %v", page, err)
			}

			pages[page] = strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n")
		}

		n, _ := strconv.Atoi(line) // the pattern admits digits only

		key := page + "\t" + sourceCitation(pages[page][n-1])
		recited[key] = append(recited[key], page+":"+line)
	}

	known := readKnownRecites(t, filepath.Join(root, "src", "go2cs", "internal", "repoguard", "testdata", "docsamples-known-recite.txt"))
	var keys []string

	for k := range recited {
		keys = append(keys, k)
	}

	for k := range known {
		if _, ok := recited[k]; !ok {
			keys = append(keys, k)
		}
	}

	sort.Strings(keys)

	for _, k := range keys {
		at, excused := recited[k], known[k]
		citation := k[strings.IndexByte(k, '\t')+1:]

		switch {
		case len(at) > excused:
			for _, where := range at[excused:] {
				t.Errorf("%s: `%s` does not cite the lines holding its sample, which is verbatim elsewhere in that file. "+
					"Re-cite it: docsamples -recite -goroot <pinned GOROOT> -write <page>, run from the repository root", where, citation)
			}
		case len(at) < excused:
			t.Errorf("testdata/docsamples-known-recite.txt: %s `%s` is listed %d time(s) but re-cited %d: delete the row(s) it no longer needs",
				k[:strings.IndexByte(k, '\t')], citation, excused, len(at))
		}
	}

	t.Logf("docsamples (GOROOT of %s): %s source comment(s); %d re-citable, %d of them known", runtime.Version(), summary[1], len(rows), len(rows)-countUnknown(recited, known))
}

// sourceCitation is a source comment's text after "source:", the form the known-list stores.
func sourceCitation(line string) string {
	if m := regexp.MustCompile(`^\s*<!--\s*source:\s*(.*?)\s*-->\s*$`).FindStringSubmatch(line); m != nil {
		return m[1]
	}

	return strings.TrimSpace(line)
}

// readKnownRecites returns the known-list as page<TAB>citation -> times listed.
func readKnownRecites(t *testing.T, path string) map[string]int {
	t.Helper()
	data, err := os.ReadFile(path)

	if err != nil {
		t.Fatalf("reading the known-list: %v", err)
	}

	known := map[string]int{}

	for _, line := range strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n") {
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}

		if !strings.Contains(line, "\t") {
			t.Fatalf("known-list row %q is not page<TAB>citation", line)
		}

		known[line]++
	}

	return known
}

func countUnknown(recited map[string][]string, known map[string]int) int {
	n := 0

	for k, at := range recited {
		if len(at) > known[k] {
			n += len(at) - known[k]
		}
	}

	return n
}
