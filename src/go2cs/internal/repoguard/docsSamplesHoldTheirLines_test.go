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
// WHAT IS CHECKED: docsamples -recite -check, dry, at HEAD over the living pages, with GOROOT/ comments
// read from the toolchain running this test (the repository's pinned one).
//
//   - A MOVED citation. -recite re-cites a comment exactly when every run of its sample is in the cited
//     file as written, in order, but not inside the cited lines: the sample is its own quoted anchor, so a
//     moved citation is caught by content, not just existence. Each row it would re-cite fails here, unless
//     the known-list (testdata/docsamples-known-recite.txt) names it. The known-list only shrinks: a row
//     docsamples no longer re-cites fails too.
//   - A DRIFTED sample, and every CHECK line -check prints: a sample that differs from its file by a line
//     or two, one whose runs are not in the file as written (hand-edited, or a skip with no ellipsis: a
//     run is the file's lines exactly, blank lines included), a citation in a form the tool cannot read,
//     a missing file, and a csharp block on a published page with neither a source comment nor a label
//     (`<!-- illustration: <why> -->` or `<!-- attribute-shown: <why> -->`).
//
// WHAT IS NOT: a block marked `<!-- source (not checked): <what> -->`, whose source the tool cannot read
// (a generator's build output); a labelled block; and a source comment with no fence after it, a note
// over a table or prose. A docsamples ERROR (a sample whose file changed between refs) cannot occur at
// one ref.
//
// POSITIVE CONTROLS. TestDocsSamplesCheckNamesEachPlantedSample runs the same tool on a planted page and
// wants each planted defect named at its line, and nothing else. Verified red-first when this guard
// landed: moving the citation at docs/ConversionStrategies-Reference/comments.md:96 two lines down failed
// this test at that line, where plain docsamples had reported a note and exited 0; and when -check was
// added, one changed line in a cited sample on a real page failed it at that sample's comment line, and
// the page was restored byte for byte.
func TestDocsSamplesHoldTheirLines(t *testing.T) {
	root := repoRootFromPackageDir(t)
	goroot := docsamplesGoroot()
	stdout, stderr, checks := runDocsamples(t, root, buildDocsamples(t, root, goroot), "-recite", "-check", "-goroot", goroot)

	for _, c := range checks {
		t.Errorf("%s", c)
	}

	// The summary line is the liveness check: no line, or no comment read, and nothing above was tested.
	summary := regexp.MustCompile(`(?m)^docsamples: .*: \d+ page\(s\) read, (\d+) source comment\(s\)`).FindStringSubmatch(stdout)

	if summary == nil || summary[1] == "0" {
		t.Fatalf("docsamples read no source comment; its output:\n%s%s", stdout, stderr)
	}

	recited := map[string][]string{} // page<TAB>citation -> the page lines carrying it
	rows := regexp.MustCompile(`(?m)^RECITED\s+(\S+):(\d+)\s*$`).FindAllStringSubmatch(stdout, -1)
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

// docsamplesGoroot is the toolchain running this test, the repository's pinned one.
func docsamplesGoroot() string {
	if goroot := build.Default.GOROOT; goroot != "" {
		return goroot
	}

	return runtime.GOROOT()
}

// buildDocsamples builds src/tools/docsamples with the toolchain at goroot and returns the binary.
func buildDocsamples(t *testing.T, root, goroot string) string {
	t.Helper()
	exe := filepath.Join(t.TempDir(), "docsamples")

	if runtime.GOOS == "windows" {
		exe += ".exe"
	}

	compile := exec.Command(filepath.Join(goroot, "bin", "go"), "build", "-o", exe, ".")
	compile.Dir = filepath.Join(root, "src", "tools", "docsamples")

	if out, err := compile.CombinedOutput(); err != nil {
		t.Fatalf("building src/tools/docsamples: %v\n%s", err, out)
	}

	return exe
}

var docsamplesCheckRow = regexp.MustCompile(`(?m)^CHECK\s+(\S+:\d+: .*)$`)

// runDocsamples runs the tool from the repository root and returns its output and its CHECK rows. Exit 1
// with CHECK rows is -check reporting them; any other failure is fatal.
func runDocsamples(t *testing.T, root, exe string, args ...string) (stdout, stderr string, checks []string) {
	t.Helper()
	run := exec.Command(exe, args...)
	run.Dir = root
	var out, errOut bytes.Buffer
	run.Stdout, run.Stderr = &out, &errOut
	err := run.Run()
	stdout, stderr = out.String(), errOut.String()

	for _, m := range docsamplesCheckRow.FindAllStringSubmatch(stdout, -1) {
		checks = append(checks, m[1])
	}

	if err != nil {
		var exit *exec.ExitError

		if !errors.As(err, &exit) {
			t.Fatalf("running docsamples: %v", err)
		}

		if exit.ExitCode() != 1 || len(checks) == 0 {
			t.Fatalf("docsamples exited %d (1: a stale sample it cannot regenerate, or a CHECK row; 2: it could not run):\n%s%s",
				exit.ExitCode(), stdout, stderr)
		}
	}

	return stdout, stderr, checks
}

// TestDocsSamplesCheckNamesEachPlantedSample is the guard's positive control: docsamples -check over
// testdata/docsamples-planted/page.md, whose samples cite source.cs beside it (read from disk, so the
// control needs no commit). Each planted defect must be named at its line, and nothing else: a sample
// that holds, a not-checked comment and a labelled block pass.
func TestDocsSamplesCheckNamesEachPlantedSample(t *testing.T) {
	root := repoRootFromPackageDir(t)
	page := "src/go2cs/internal/repoguard/testdata/docsamples-planted/page.md"
	_, _, checks := runDocsamples(t, root, buildDocsamples(t, root, docsamplesGoroot()), "-check", "-base", "WORKTREE", page)
	var got []string

	for _, c := range checks {
		got = append(got, c[:strings.Index(c, ": ")])
	}

	want := []string{
		page + ":12", // one line changed: drifted
		page + ":19", // lines skipped across a blank line, with no ellipsis
		page + ":26", // a free-form citation over a fence
		page + ":31", // an unlabelled csharp block, named at its fence
	}

	sort.Strings(got)

	if strings.Join(got, "\n") != strings.Join(want, "\n") {
		t.Errorf("CHECK rows name:\n  %s\nwant:\n  %s\nrows:\n  %s", strings.Join(got, "\n  "), strings.Join(want, "\n  "), strings.Join(checks, "\n  "))
	}
}
