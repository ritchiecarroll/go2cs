// corpus.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bufio"
	"bytes"
	"fmt"
	"io"
	"io/fs"
	"os"
	"os/exec"
	"path/filepath"
	"sort"
	"strconv"
	"strings"
)

// The three file sets the plan's footprint table counts over (docs/PLAN-parity-train.md §1, measured at 5931bad29b:
// 3,221 production files, 1,148 converted-test files and 925 goldens). Every file is counted once, wherever layout L3
// puts it: a per-GOOS copy is its own file.
const (
	setProd   = "prod"
	setTest   = "test"
	setGolden = "golden"
)

var setOrder = []string{setProd, setTest, setGolden}

// setOf answers which set a repository-relative, slash-separated path belongs to, or "".
//   - prod: every .cs under src/core outside golib/ that is not a converted test;
//   - test: every *_test.cs under src/core outside golib/;
//   - golden: every .cs.target under src/tests/Behavioral.
func setOf(path string) string {
	switch {
	case strings.HasPrefix(path, "src/core/golib/"):
		return ""
	case strings.HasPrefix(path, "src/core/") && strings.HasSuffix(path, "_test.cs"):
		return setTest
	case strings.HasPrefix(path, "src/core/") && strings.HasSuffix(path, ".cs"):
		return setProd
	case strings.HasPrefix(path, "src/tests/Behavioral/") && strings.HasSuffix(path, ".cs.target"):
		return setGolden
	}
	return ""
}

// file is one counted file: its set, its repository-relative path and its text.
type file struct {
	set, path string
	text      []byte
}

// readTree reads the sets from a repository checkout rooted at root (the directory that holds src/).
func readTree(root string) ([]file, error) {
	var files []file
	for _, top := range []string{"src/core", "src/tests/Behavioral"} {
		err := filepath.WalkDir(filepath.Join(root, filepath.FromSlash(top)), func(p string, d fs.DirEntry, err error) error {
			if err != nil {
				return err
			}
			if d.IsDir() {
				// Build output is never corpus.
				if n := d.Name(); n == "bin" || n == "obj" {
					return filepath.SkipDir
				}
				return nil
			}
			rel, err := filepath.Rel(root, p)
			if err != nil {
				return err
			}
			rel = filepath.ToSlash(rel)
			set := setOf(rel)
			if set == "" {
				return nil
			}
			text, err := os.ReadFile(p)
			if err != nil {
				return err
			}
			files = append(files, file{set, rel, text})
			return nil
		})
		if err != nil {
			return nil, err
		}
	}
	sort.Slice(files, func(i, j int) bool { return files[i].path < files[j].path })
	return files, nil
}

// readRef reads the sets from a git ref of the repository at repo, through `git ls-tree` and one
// `git cat-file --batch`, with no checkout.
func readRef(repo, ref string) ([]file, error) {
	out, err := exec.Command("git", "-C", repo, "ls-tree", "-r", "-z", ref, "--", "src/core", "src/tests/Behavioral").Output()
	if err != nil {
		return nil, fmt.Errorf("git ls-tree %s: %w", ref, err)
	}
	var files []file
	var objects []string
	for _, rec := range bytes.Split(out, []byte{0}) {
		// "<mode> <type> <object>\t<path>"
		meta, path, ok := bytes.Cut(rec, []byte{'\t'})
		if !ok {
			continue
		}
		fields := strings.Fields(string(meta))
		if len(fields) != 3 || fields[1] != "blob" {
			continue
		}
		if set := setOf(string(path)); set != "" {
			files = append(files, file{set: set, path: string(path)})
			objects = append(objects, fields[2])
		}
	}

	cmd := exec.Command("git", "-C", repo, "cat-file", "--batch")
	cmd.Stdin = strings.NewReader(strings.Join(objects, "\n") + "\n")
	stdout, err := cmd.StdoutPipe()
	if err != nil {
		return nil, err
	}
	if err := cmd.Start(); err != nil {
		return nil, err
	}
	r := bufio.NewReaderSize(stdout, 1<<20)
	for i := range files {
		header, err := r.ReadString('\n')
		if err != nil {
			return nil, fmt.Errorf("git cat-file: %w", err)
		}
		// "<object> blob <size>"
		f := strings.Fields(header)
		if len(f) != 3 || f[1] != "blob" {
			return nil, fmt.Errorf("git cat-file: unexpected header %q for %s", strings.TrimSpace(header), files[i].path)
		}
		size, err := strconv.Atoi(f[2])
		if err != nil {
			return nil, err
		}
		text := make([]byte, size+1) // the blob and its trailing newline
		if _, err := io.ReadFull(r, text); err != nil {
			return nil, err
		}
		files[i].text = text[:size]
	}
	if err := cmd.Wait(); err != nil {
		return nil, fmt.Errorf("git cat-file: %w", err)
	}
	sort.Slice(files, func(i, j int) bool { return files[i].path < files[j].path })
	return files, nil
}
