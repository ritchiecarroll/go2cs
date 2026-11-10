// toolchainRename_windows_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build windows

package main

import (
	"errors"
	"os"
	"path/filepath"
	"syscall"
	"testing"
	"time"
)

// The install unpacks into a staging directory and renames it into place (installFile). On Windows that rename
// fails with "Access is denied" while any process holds a handle on a file inside the directory without
// FILE_SHARE_DELETE, as a virus scanner or the search indexer does for a moment after a file is written. Go's
// os.Open opens without FILE_SHARE_DELETE, so a handle it holds stands in for theirs.

// writeFakeToolchain writes fakeToolchainFiles into dest.
func writeFakeToolchain(t *testing.T, dest string) {
	t.Helper()

	for name, body := range fakeToolchainFiles("windows") {
		target := filepath.Join(dest, filepath.FromSlash(name))

		if err := os.MkdirAll(filepath.Dir(target), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(target, []byte(body), 0o644); err != nil {
			t.Fatal(err)
		}
	}
}

// The mechanism, alone: a held handle inside a directory denies its rename.
func TestRenameOfADirectoryWithAHeldHandleIsDenied(t *testing.T) {
	base := t.TempDir()
	staging := filepath.Join(base, ".unpack-1")
	writeFakeToolchain(t, staging)

	held, err := os.Open(filepath.Join(staging, "src", "fmt", "print.go"))
	if err != nil {
		t.Fatal(err)
	}

	err = os.Rename(staging, filepath.Join(base, fakeRelease))
	held.Close()

	if !errors.Is(err, syscall.ERROR_ACCESS_DENIED) {
		t.Fatalf("renaming a directory while a file in it is held: got %v, want ERROR_ACCESS_DENIED", err)
	}
}

// The install: a handle held for a moment after unpacking must not fail it. The handle is released 150 ms after the
// unpack returns, inside the retry budget.
func TestToolchainInstallSurvivesAHandleHeldAfterUnpacking(t *testing.T) {
	p := testProvisioner(t, map[string]string{}, nil)

	if err := os.MkdirAll(p.cacheRoot, 0o755); err != nil {
		t.Fatal(err)
	}

	released := make(chan struct{})
	source := toolchainSource{
		name:   "test",
		verify: func(string) error { return nil },
		unpack: func(_, dest string) error {
			writeFakeToolchain(t, dest)

			held, err := os.Open(filepath.Join(dest, "src", "fmt", "print.go"))
			if err != nil {
				return err
			}

			go func() {
				time.Sleep(150 * time.Millisecond)
				held.Close()
				close(released)
			}()

			return nil
		},
	}

	root := p.cachedRoot()
	err := p.installFile(source, "unused", root)
	<-released

	if err != nil {
		t.Fatalf("install with a handle held for 150 ms after unpacking: %v", err)
	}

	if err := p.checkRelease(root); err != nil {
		t.Fatalf("no whole install at %s: %v", root, err)
	}
}
