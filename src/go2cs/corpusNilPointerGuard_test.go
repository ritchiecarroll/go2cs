// corpusNilPointerGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A corpus guard for the ONE nil test a hand-own must not write.
//
// `nil` does not reach a converted `ж<T>` parameter as a C# null. golib's conversion is
// `implicit operator ж<T>(NilType) => NilBox`, and NilBox is a real `StandardBox<T>` instance whose
// `.Value` throws — so `Ꮡx is null` is FALSE for every Go nil pointer, a guard written that way takes
// the wrong branch, and the dereference behind it faults. A C# null is reachable at the same sites
// too (an uninitialised `ж<T>?`), and `.IsNilPointer` on a genuine null would itself throw, so
// NEITHER half is sufficient alone: the corpus form is `x is not null && !x.IsNilPointer` and its
// inverse `x is null || x.IsNilPointer`.
//
// Measured 2026-09-02, and the split was total: 24 sites across two Linux hand-owns
// (syscall/linux/structclass_linux_impl.cs, syscall/linux/zsyscall_linux_amd64_impl.cs) carried the
// one-sided form and ZERO carried the predicate, while all four Windows sites carried it. Every one
// of the 24 was a PARAMETER — reachable with a Go nil from any caller — across Select, FcntlFlock,
// Statfs, Fstatfs, Sysinfo, Adjtimex, Fstat, fstatat, wait4 and Uname. The crash that found it was
// `syscall.Wait4(pid, &status, 0, nil)`, which is Go's own os/exec wait shape.
//
// The guard is corpus-wide rather than hand-own-only on purpose: the converter never emits this form
// (generated code compares with `== nil`), so a walk of every `.cs` under src/core has no exceptions
// to carve out and catches the next hand-own wherever somebody puts it.

package main

import (
	"bufio"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// A `Ꮡ`-prefixed identifier is the converter's spelling for a Go pointer, so this matches the
// pointers and not an ordinary C# reference — which may legitimately be tested with `is null`.
var corpusGoPointerNilTest = regexp.MustCompile(`Ꮡ[A-Za-z_][A-Za-z0-9_]* is (not )?null`)

func TestCorpusHandOwnsUseTheGoNilPredicate(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	if info, err := os.Stat(filepath.Join(coreDir, "golib", "golib.csproj")); err != nil || !info.Mode().IsRegular() {
		t.Skip("src/core is not beside the converter; nothing to walk")
	}

	var offenders []string
	scanned := 0

	err := filepath.WalkDir(coreDir, func(filePath string, entry os.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if entry.IsDir() {
			// Build output carries copies of the sources and would double every finding.
			switch entry.Name() {
			case "bin", "obj", "Generated":
				return filepath.SkipDir
			}
			return nil
		}

		if !strings.HasSuffix(entry.Name(), ".cs") {
			return nil
		}

		file, err := os.Open(filePath)
		if err != nil {
			return err
		}
		defer file.Close()

		scanned++
		scanner := bufio.NewScanner(file)
		scanner.Buffer(make([]byte, 0, 64*1024), 4*1024*1024)

		for line := 1; scanner.Scan(); line++ {
			text := scanner.Text()

			// A comment describing the form is not an instance of it — this file's own header would
			// otherwise be a finding if it ever moved into the corpus.
			if strings.HasPrefix(strings.TrimSpace(text), "//") {
				continue
			}

			if !corpusGoPointerNilTest.MatchString(text) || strings.Contains(text, "IsNilPointer") {
				continue
			}

			offenders = append(offenders, filepath.ToSlash(filePath)+":"+itoa(line)+": "+strings.TrimSpace(text))
		}

		return scanner.Err()
	})

	if err != nil {
		t.Fatalf("walking %s: %v", coreDir, err)
	}

	if scanned == 0 {
		t.Fatal("no .cs file was scanned; the walk is not reaching src/core and this guard would pass over anything")
	}

	if len(offenders) > 0 {
		t.Errorf("%d site(s) test a Go pointer with a C# null test alone. `nil` arrives as golib's NilBox — a real "+
			"StandardBox<T> — so `is null` is FALSE for it and the branch behind the test faults. Use the corpus form "+
			"(`x is not null && !x.IsNilPointer`, or `x is null || x.IsNilPointer`), which the syscall/windows hand-owns "+
			"already carry:\n\t%s", len(offenders), strings.Join(offenders, "\n\t"))
	}
}

func itoa(n int) string {
	if n == 0 {
		return "0"
	}

	var digits [20]byte
	i := len(digits)

	for n > 0 {
		i--
		digits[i] = byte('0' + n%10)
		n /= 10
	}

	return string(digits[i:])
}
