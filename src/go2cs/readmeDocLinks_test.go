// readmeDocLinks_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/doc/comment"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// The doc-link grammar is closed, so this test enumerates it rather than sampling it. go/doc/comment
// documents exactly five field combinations on DocLink; every one appears below, and each asserts a
// FULLY-QUALIFIED URL, because a site-root-relative one — which is what the printer emits with no
// resolver installed — is dead on all three surfaces a converted README renders on.
func TestDocLinkResolvesToFullyQualifiedURL(t *testing.T) {
	const version = "1.23.1"

	tests := []struct {
		name    string
		link    comment.DocLink
		current string
		want    string
	}{
		{
			name: "ImportPath alone resolves to the pinned package page",
			link: comment.DocLink{ImportPath: "io"},
			want: "https://pkg.go.dev/io@go1.23.1",
		},
		{
			name: "ImportPath and Name resolve to the pinned symbol anchor",
			link: comment.DocLink{ImportPath: "io", Name: "Reader"},
			want: "https://pkg.go.dev/io@go1.23.1#Reader",
		},
		{
			name: "ImportPath, Recv and Name resolve to the pinned method anchor",
			link: comment.DocLink{ImportPath: "io", Recv: "Writer", Name: "Write"},
			want: "https://pkg.go.dev/io@go1.23.1#Writer.Write",
		},
		{
			name:    "Name alone resolves against the package being converted",
			link:    comment.DocLink{Name: "Marshal"},
			current: "encoding/json",
			want:    "https://pkg.go.dev/encoding/json@go1.23.1#Marshal",
		},
		{
			name:    "Recv and Name alone resolve against the package being converted",
			link:    comment.DocLink{Recv: "Decoder", Name: "Decode"},
			current: "encoding/json",
			want:    "https://pkg.go.dev/encoding/json@go1.23.1#Decoder.Decode",
		},
		{
			name: "a multi-element import path keeps every element",
			link: comment.DocLink{ImportPath: "path/filepath", Name: "Walk"},
			want: "https://pkg.go.dev/path/filepath@go1.23.1#Walk",
		},
		{
			name: "a command package resolves like any other std path",
			link: comment.DocLink{ImportPath: "cmd/go"},
			want: "https://pkg.go.dev/cmd/go@go1.23.1",
		},
		{
			name: "an internal package needs no special case",
			link: comment.DocLink{ImportPath: "internal/abi", Name: "Type"},
			want: "https://pkg.go.dev/internal/abi@go1.23.1#Type",
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			got := resolveDocLinkURL(&test.link, test.current, version, "")

			if got != test.want {
				t.Errorf("resolveDocLinkURL = %q, want %q", got, test.want)
			}

			if strings.HasPrefix(got, "/") || strings.HasPrefix(got, "#") {
				t.Errorf("resolveDocLinkURL returned a relative URL %q — dead on GitHub, Pages and nuget.org", got)
			}
		})
	}
}

// An external module path cannot be pinned to a Go release — it is not a Go release artifact. It is
// pinned to the module snapshot when GOROOT's own modules.txt records the exact package, and emitted
// fully qualified but UNVERSIONED when it does not, rather than borrowing a pin the distribution
// never made for that package.
func TestDocLinkResolvesExternalModulePaths(t *testing.T) {
	goRoot := t.TempDir()
	vendorDir := filepath.Join(goRoot, "src", "vendor")

	if err := os.MkdirAll(vendorDir, 0755); err != nil {
		t.Fatal(err)
	}

	modules := "# golang.org/x/sys v0.22.0\n## explicit; go 1.18\ngolang.org/x/sys/cpu\n"

	if err := os.WriteFile(filepath.Join(vendorDir, "modules.txt"), []byte(modules), 0644); err != nil {
		t.Fatal(err)
	}

	tests := []struct {
		name string
		link comment.DocLink
		want string
	}{
		{
			name: "a vendored package pins the module version modules.txt records",
			link: comment.DocLink{ImportPath: "golang.org/x/sys/cpu", Name: "X86"},
			want: "https://pkg.go.dev/golang.org/x/sys@v0.22.0/cpu#X86",
		},
		{
			name: "GOROOT's internal vendor/ spelling resolves the same way",
			link: comment.DocLink{ImportPath: "vendor/golang.org/x/sys/cpu"},
			want: "https://pkg.go.dev/golang.org/x/sys@v0.22.0/cpu",
		},
		{
			name: "an unvendored package of a vendored module is left unpinned, not borrowed",
			link: comment.DocLink{ImportPath: "golang.org/x/sys/windows"},
			want: "https://pkg.go.dev/golang.org/x/sys/windows",
		},
		{
			name: "a module GOROOT does not vendor at all is fully qualified but unpinned",
			link: comment.DocLink{ImportPath: "google.golang.org/protobuf", Name: "Marshal"},
			want: "https://pkg.go.dev/google.golang.org/protobuf#Marshal",
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if got := resolveDocLinkURL(&test.link, "", "1.23.1", goRoot); got != test.want {
				t.Errorf("resolveDocLinkURL = %q, want %q", got, test.want)
			}
		})
	}
}

// The one case with nowhere honest to point stays exactly as it was before the resolver existed,
// rather than inventing an absolute URL for a package this conversion cannot name.
func TestDocLinkWithoutAnImportPathFallsBackToDefault(t *testing.T) {
	link := comment.DocLink{Name: "Reader"}

	if got, want := resolveDocLinkURL(&link, "", "1.23.1", ""), "#Reader"; got != want {
		t.Errorf("resolveDocLinkURL = %q, want %q", got, want)
	}
}

// An unknown toolchain version costs the pin, never the qualification: the link still resolves.
func TestDocLinkWithoutAVersionStaysFullyQualified(t *testing.T) {
	link := comment.DocLink{ImportPath: "io", Name: "Reader"}

	if got, want := resolveDocLinkURL(&link, "", "", ""), "https://pkg.go.dev/io#Reader"; got != want {
		t.Errorf("resolveDocLinkURL = %q, want %q", got, want)
	}
}

// The end-to-end shape: real godoc markup in, Markdown out, with every doc link fully qualified and
// every ALREADY-ABSOLUTE link passed through untouched.
//
// *Link URLs are absolute by construction (parseLink and autoURL both require a scheme://), so the
// pass-through is asserted here rather than in resolveDocLinkURL, which never sees them.
func TestRenderPackageDocQualifiesLinksAndPassesAbsolutesThrough(t *testing.T) {
	goRoot := t.TempDir()
	sourceDir := filepath.Join(goRoot, "src", "bufio")

	if err := os.MkdirAll(sourceDir, 0755); err != nil {
		t.Fatal(err)
	}

	markup := strings.Join([]string{
		"Package bufio wraps an [io.Reader].",
		"",
		"See [io], [io.Writer.Write] and [path/filepath.Walk], plus https://go.dev/ref/spec and the",
		"[Go Blog].",
		"",
		"[Go Blog]: https://go.dev/blog",
	}, "\n")

	rendered := renderPackageDoc(markup, sourceDir, Options{goRoot: goRoot})

	// Derived, not spelled: renderPackageDoc pins its links to goVersion() — the ACTIVE toolchain —
	// so the expectations must derive from the same source or this test goes stale at every Go
	// release hop. It did exactly that at the 1.23.1→1.23.12 hop (hop A, 2026-08-25): hardcoded
	// "@go1.23.1" wants failed the H1↔H2 window and would have kept failing after the pair landed.
	// The resolveDocLinkURL table above stays hermetic the other way — it INJECTS its version, so
	// its pinned wants can never drift from its pinned input.
	v := goVersion()

	for _, want := range []string{
		"(https://pkg.go.dev/io@go" + v + "#Reader)",
		"(https://pkg.go.dev/io@go" + v + ")",
		"(https://pkg.go.dev/io@go" + v + "#Writer.Write)",
		"(https://pkg.go.dev/path/filepath@go" + v + "#Walk)",
		"(https://go.dev/ref/spec)",
		"(https://go.dev/blog)",
	} {
		if !strings.Contains(rendered, want) {
			t.Errorf("rendered README is missing %q\n---\n%s\n---", want, rendered)
		}
	}

	// The defect this whole file exists for: no link target may start at the site root.
	if strings.Contains(rendered, "](/") {
		t.Errorf("rendered README still carries a site-root-relative link:\n---\n%s\n---", rendered)
	}
}
