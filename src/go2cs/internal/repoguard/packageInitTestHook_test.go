// packageInitTestHook_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"io/fs"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// THE PRODUCTION-INIT HOOK GUARD (H7 consolidated re-emit, finding 1; ruling F).
//
// A production package_init.cs is written by -stdlib and -tests alike, so it must be the same file
// from either. It once ended its static ctor, under -tests only, with a call to an erasable
// `initᴛᴛtests()` partial declared beside it for an internal test variant to implement, so the file
// flipped with whichever mode wrote it last -- and nothing in the corpus implemented it. The converter
// no longer emits it in either mode (TestPackageInitIsIdenticalFromEitherMode), and an internal
// variant that needs it runs its relocations from a module initializer in its own *_test.cs. This
// reads the COMMITTED production files: none may mention the hook.
//
// POSITIVE CONTROL: TestPackageInitHookScannerFires.

const packageInitHookName = "initᴛᴛtests"

func packageInitCarriesTestHook(contents string) bool {
	return strings.Contains(contents, packageInitHookName)
}

func TestNoProductionPackageInitCarriesTheTestHook(t *testing.T) {
	root := repoRootFromPackageDir(t)
	read := 0

	for _, tree := range []string{filepath.Join(root, "src", "core"), filepath.Join(root, "src", "tests")} {
		err := filepath.WalkDir(tree, func(path string, entry fs.DirEntry, err error) error {
			if err != nil {
				return err
			}

			if entry.IsDir() {
				if name := entry.Name(); name == "bin" || name == "obj" {
					return filepath.SkipDir
				}

				return nil
			}

			if entry.Name() != "package_init.cs" {
				return nil
			}

			data, err := os.ReadFile(path)

			if err != nil {
				return err
			}

			read++

			if packageInitCarriesTestHook(string(data)) {
				rel, _ := filepath.Rel(root, path)
				t.Errorf("%s carries the %s test hook -- re-emit it with the current converter", filepath.ToSlash(rel), packageInitHookName)
			}

			return nil
		})

		if err != nil {
			t.Fatalf("walking %s: %v", tree, err)
		}
	}

	if read < 40 {
		t.Fatalf("read %d package_init.cs: too few to be the corpus", read)
	}

	t.Logf("read %d production package_init.cs", read)
}

func TestPackageInitHookScannerFires(t *testing.T) {
	hooked := "partial class p_package {\r\n    static p_package() {\r\n        initᴛX();\r\n        initᴛᴛtests();\r\n    }\r\n\r\n    static partial void initᴛᴛtests();\r\n}\r\n"
	clean := "partial class p_package {\r\n    static p_package() {\r\n        initᴛX();\r\n    }\r\n}\r\n"

	if !packageInitCarriesTestHook(hooked) {
		t.Errorf("the -tests hook shape was not flagged")
	}

	if packageInitCarriesTestHook(clean) {
		t.Errorf("a hook-free package_init.cs was flagged")
	}
}
