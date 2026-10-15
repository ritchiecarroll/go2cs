// l3BlockPosition_test.go - Gbtc
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

// THE L3 BLOCK-POSITION GUARD (H7 consolidated re-emit, finding 2).
//
// A Layout-L3 project file's conditioned reference block has ONE position: directly after the shared
// reference group and its blank line (renderPlatformReferences, platformProject.go). Every other group
// after it -- licensing's shared-LICENSE <None> group, the merge's kept remainder -- is appended before
// </Project> by its own pass. When the renderer also placed the block at </Project>, the committed
// order followed whichever pass ran first, and a re-emit with identical inputs moved three projects'
// LICENSE group (internal/runtime/cgroup, internal/syscall/windows, crypto/internal/fips140deps/time).
// The converter-side proof is TestPlatformBlockPositionIsIndependentOfPassOrder; this reads the
// COMMITTED files, so a project written by an older converter, or by hand, cannot carry the other order.
//
// POSITIVE CONTROL: TestL3BlockPositionScannerFires plants the licensing-first shape.

const (
	l3BlockHeader      = "  <!-- Layout L3: this package's direct imports differ by GOOS"
	l3SharedAnchor     = "    <ProjectReference Include=\"$(go2csPath)core/golib/golib.csproj\" />\n"
	l3ReferenceClose   = "  </ItemGroup>\n"
	l3MinimumL3Corpora = 20
)

// l3BlockMisplaced reports why a project file's conditioned block is not where the renderer puts it,
// or "" when it is (or when the file carries no block). Line endings are normalized first.
func l3BlockMisplaced(contents string) string {
	text := strings.ReplaceAll(contents, "\r\n", "\n")
	blockAt := strings.Index(text, l3BlockHeader)

	if blockAt < 0 {
		return ""
	}

	anchorAt := strings.Index(text, l3SharedAnchor)

	if anchorAt < 0 {
		return "the block has no shared reference group (golib line) to follow"
	}

	closeAt := strings.Index(text[anchorAt:], l3ReferenceClose)

	if closeAt < 0 {
		return "the shared reference group is not closed"
	}

	want := anchorAt + closeAt + len(l3ReferenceClose) + len("\n")

	if blockAt != want {
		between := strings.TrimSpace(text[min(want, blockAt):max(want, blockAt)])

		if len(between) > 80 {
			between = between[:80] + "…"
		}

		return "the block does not directly follow the shared reference group; between them: " + between
	}

	return ""
}

func TestL3BlockFollowsTheSharedReferenceGroup(t *testing.T) {
	root := repoRootFromPackageDir(t)
	scanned, l3 := 0, 0

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

			if !strings.HasSuffix(path, ".csproj") {
				return nil
			}

			data, err := os.ReadFile(path)

			if err != nil {
				return err
			}

			scanned++

			if !strings.Contains(string(data), "Layout L3: this package's direct imports differ by GOOS") {
				return nil
			}

			l3++

			if why := l3BlockMisplaced(string(data)); why != "" {
				rel, _ := filepath.Rel(root, path)
				t.Errorf("%s: %s -- re-emit it with the current converter", filepath.ToSlash(rel), why)
			}

			return nil
		})

		if err != nil {
			t.Fatalf("walking %s: %v", tree, err)
		}
	}

	if l3 < l3MinimumL3Corpora {
		t.Fatalf("read %d project files and only %d carry the L3 block: too few to be the corpus", scanned, l3)
	}

	t.Logf("scanned %d project files, %d Layout-L3", scanned, l3)
}

func TestL3BlockPositionScannerFires(t *testing.T) {
	refs := "  <ItemGroup>\n" + l3SharedAnchor + "    <ProjectReference Include=\"$(go2csPath)core/io/io.csproj\" />\n" + l3ReferenceClose + "\n"
	block := l3BlockHeader + " ... -->\n  <ItemGroup Condition=\"'$(GoTargetOS)'=='windows'\" />\n\n"
	license := "  <ItemGroup>\n    <None Include=\"../LICENSE\" Pack=\"true\" PackagePath=\"\" Condition=\"!Exists('$(MSBuildProjectDirectory)/LICENSE')\" />\n  </ItemGroup>\n"

	if why := l3BlockMisplaced("<Project>\n" + refs + block + license + "</Project>\n"); why != "" {
		t.Errorf("the renderer's own order was flagged: %s", why)
	}

	if why := l3BlockMisplaced(strings.ReplaceAll("<Project>\n"+refs+license+block+"</Project>\n", "\n", "\r\n")); why == "" {
		t.Errorf("the licensing-first order (block after the LICENSE group) was not flagged")
	}

	if why := l3BlockMisplaced("<Project>\n" + refs + license + "</Project>\n"); why != "" {
		t.Errorf("a file without the block was flagged: %s", why)
	}
}
