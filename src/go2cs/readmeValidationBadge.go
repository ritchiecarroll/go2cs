// readmeValidationBadge.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Every converted standard-library package's NuGet README carries one badge paragraph — the first
// thing a visitor sees on nuget.org and in the repository. It holds four badges over TWO lines:
//
//	Tests      this package's validation state (below)
//	Docs       the official Go documentation for the very sources it was converted from, pinned to
//	           the version that produced them (readmeDocsBadgeLine)
//	· · · · · · · · · · · · · ·  hard line break  · · · · · · · · · · · · · ·
//	Source·Go  the Go sources themselves, in the Go repository at the release tag that produced them
//	           (readmeGoSourceBadgeLine)
//	Source·C#  the converted C# beside this README, in the go2cs repository at the release tag this
//	           package version was published under (readmeCSharpSourceBadgeLine)
//
// The two Source badges come LAST because they answer the reader's second question, not the first:
// Tests and Docs state what this package IS (validated, and what it mirrors), while the Source pair
// is the invitation to go read both sides. The pair is deliberately adjacent and deliberately in
// convert-FROM → convert-TO order, so the line reads as the conversion it describes.
//
// That reading order is also WHERE the break goes (user ruling 2026-08-15, from the live NuGet page).
// All four on one line wrapped raggedly in nuget.org's narrow README pane — it broke wherever the
// pane's width fell rather than where the meaning does. Splitting at the seam the order already names
// gives the one variable-width badge (Tests, whose message is `<m>/<t> validated`) the room, and
// leaves the fixed-width Source pair together on a line that cannot outgrow the pane. It stays ONE
// paragraph of two lines rather than two paragraphs: the four badges are a single statement.
//
// The Tests badge is the package's honesty contract:
//
//	green   <m>/<t> validated   Go's own test suite for this package was converted, run under the
//	                            Go-semantics test host and compared verdict for verdict against
//	                            `go test -json`. The badge LINKS its proof: the versioned page under
//	                            docs/validation/<version>/, which is the per-test differential.
//	orange  not yet validated   The package's Go sources DO define Test functions; they have not
//	                            been put through the pipeline yet. Never shown on a test-less
//	                            package — that would invent a debt that does not exist.
//	grey    none to validate    The package's Go sources define no Test functions at all, so there
//	                            is nothing for a green badge to ever claim.
//
// The three states partition the corpus exactly, which is what makes the badge auditable: a census
// over the emitted READMEs must reproduce the roster's own denominator (the packages whose Go
// sources define Test functions) with no package left unclassified.
//
// FALLBACK (load-bearing for reproducibility): the Tests badge is composed from two things that live
// in the REPOSITORY, not in the conversion — src/version.props (the published version, which pins the
// proof URL) and docs/validation/current/ (the counts). A conversion rooted anywhere else — a bare
// temp -go2cspath root, a deployed GOPATH runtime root — can locate neither and emits NO Tests badge
// at all rather than a half-composed URL. This is why a reconvert that must reproduce the committed
// READMEs byte-identically has to seed version.props and docs/validation alongside src/core; see
// CLAUDE.md's corpus-mechanics seed step. The Docs badge reads the TOOLCHAIN instead (go env
// GOVERSION, and GOROOT's own src/vendor/modules.txt for the vendored packages), so it survives an
// unseeded root — which is why an unseeded reconvert's READMEs now carry a Docs-only badge line
// rather than none at all. Seed anyway; only the symptom moved.

package main

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"unicode"
)

const (
	// validationSiteURL is the published documentation site (Jekyll over docs/), where the proof
	// pages the badges link to are served as .html.
	validationSiteURL = "https://go2cs.net"

	// validationRosterURL is the living roster every non-green badge points at — the honest landing
	// place for "why isn't this one green yet?".
	validationRosterURL = validationSiteURL + "/ValidatedTestPackages.html"

	// versionPropsFileName is the single source of truth for the published package version, read
	// from the same go2cs root the project references resolve against.
	versionPropsFileName = "version.props"

	// testProjectFileSuffix identifies a package's COMMITTED converted test project — the artifact
	// the validated-package commit policy banks beside the production code, and therefore the
	// on-disk signal that this package's suite has been through the pipeline.
	testProjectFileSuffix = ".tests.csproj"

	// shieldsBadgeHost renders both badges; the path form is `/badge/<label>-<message>-<color>`.
	shieldsBadgeHost = "https://img.shields.io"

	// goPackageDocsURL is the official Go package documentation site the Docs badge links.
	goPackageDocsURL = "https://pkg.go.dev"

	// goBrandColor is the Go project's own blue, so the Docs badge reads as the Go documentation it
	// points at rather than as another go2cs status light.
	goBrandColor = "00ADD8"

	// dotnetBrandColor is .NET's own purple. It is the one badge on the line that is NOT Go-blue,
	// which is exactly the distinction it carries: every other badge points at Go, this one points
	// at the converted C#.
	dotnetBrandColor = "512BD4"

	// goRepositoryURL is the Go project's own repository. Its release tags (`go1.23.1`) hold the
	// exact sources the conversion read, which is what makes a tree link under one reproducible.
	goRepositoryURL = "https://github.com/golang/go"

	// (go2csRepositoryURL — this project's own repository, which the C# Source badge links — is
	// declared beside the proof-page renderer that already needed it, in validationProofPages.go.)

	// releaseTagPrefix names a published release's git tag (`nuget-1.23.1.4`). The C# Source badge
	// links the TAG rather than a branch so the C# a reader lands on is the C# that shipped in the
	// package they are reading the README of — a `master` link would drift away from it with the
	// next commit. This is why the release flow mints the tag at SNAPSHOT time, before anything is
	// packed: see the tag block in push-nuget.ps1.
	releaseTagPrefix = "nuget-"

	// vendorImportPrefix is what the standard library's own vendored third-party packages carry in
	// their import path (`vendor/golang.org/x/crypto/chacha20`). It is a GOROOT-internal spelling
	// pkg.go.dev never serves, so the Docs badge resolves those through modules.txt instead.
	vendorImportPrefix = "vendor/"

	// vendorModulesFileName is GOROOT's own record — src/vendor/modules.txt — of which module and
	// which exact version each vendored package was drawn from.
	vendorModulesFileName = "modules.txt"

	// badgeSeparator spaces the badges that share a line. A single space is what lets a renderer
	// narrower than the pair wrap between them rather than clip, and it is also what terminates a
	// badge's URL for push-nuget.ps1's link-retarget patterns — see the segment note there.
	badgeSeparator = " "

	// badgeLineBreak ends the first badge line: a trailing BACKSLASH, which is CommonMark's explicit
	// hard line break, so the two lines render as two lines of ONE paragraph.
	//
	// Chosen over the two other candidates for the same reason, from opposite directions. `<br/>` is
	// raw HTML, and nuget.org — which renders READMEs with Markdig and publishes the exact list of
	// features it supports — does not list HTML at all, so the tag would be at the mercy of its
	// sanitizer on the one surface this break exists for. Two TRAILING SPACES are the classic form,
	// but they are invisible and every whitespace-trimming tool in the path (editors, linters,
	// `git diff --check`, a well-meant future format pass over the emitter) silently deletes them and
	// silently re-joins the badges. The backslash is visible, survives trimming, and is covered by
	// name in nuget.org's supported list — the CommonMark "backslash escapes" section is where the
	// end-of-line hard break is specified — as well as by GitHub's GFM.
	//
	// Verified by rendering the emitted README through both engines rather than by reading the specs:
	// GitHub's own /markdown API and Markdig (the engine nuget.org names) each produce ONE <p> holding
	// `Tests Docs <br /> Source Source` — two lines, one paragraph, in Markdig's core and advanced
	// pipelines alike.
	//
	// The newline is a bare \n on purpose: writeReadmeFile converts the whole README to CRLF in one
	// pass, so emitting \r\n here would double the CR.
	badgeLineBreak = "\\\n"
)

// The published version lives in version.props as two elements; these mirror push-nuget.ps1's own
// regexes so the converter and the release script read the file the same way.
var (
	goStdLibVersionPattern = regexp.MustCompile(`<GoStdLibVersion>([^<]+)</GoStdLibVersion>`)
	goBuildNumberPattern   = regexp.MustCompile(`<GoBuildNumber>([^<]+)</GoBuildNumber>`)
)

// readmeBadgeLine composes a converted stdlib package's whole badge paragraph — Tests and Docs on
// the first line, Source·Go and Source·C# on the second, one badgeLineBreak apart and single spaces
// within each line.
//
// Each badge is emitted only when it can be composed honestly, so the paragraph may hold any subset,
// or — when none has the inputs it needs — nothing at all, in which case the README carries no badge
// paragraph, exactly as it did before badges existed. The break is a JOIN between the two lines
// rather than a terminator on the first, so a missing badge can never leave a dangling break or an
// empty line: an unseeded reconvert (Docs and Source·Go only) emits one badge per line with the
// break between them, a package that can compose only one line emits that line alone with no break
// at all, and no badges at all is still the empty string.
//
// The badges answer independent questions from independent inputs, so none suppresses another: Tests
// and Source·C# read the repository (version.props, the proof pages), while Docs and Source·Go read
// the Go toolchain and GOROOT. That split is what an unseeded reconvert makes visible — it emits the
// two toolchain badges and neither repository one, one on each line, which is exactly why the break
// has to be composed from what is present rather than written into a fixed template.
func readmeBadgeLine(projectPath string, projectName string, sourceDir string, options Options) string {
	importPath := stdLibImportPath(sourceDir, options.goRoot)
	state := make([]string, 0, 2)
	source := make([]string, 0, 2)

	if badge := readmeValidationBadgeLine(projectPath, projectName, sourceDir); badge != "" {
		state = append(state, badge)
	}

	if badge := readmeDocsBadgeLine(importPath, goVersion(), options.goRoot); badge != "" {
		state = append(state, badge)
	}

	if badge := readmeGoSourceBadgeLine(importPath, goVersion(), options.goRoot); badge != "" {
		source = append(source, badge)
	}

	if badge := readmeCSharpSourceBadgeLine(projectPath); badge != "" {
		source = append(source, badge)
	}

	lines := make([]string, 0, 2)

	if len(state) > 0 {
		lines = append(lines, strings.Join(state, badgeSeparator))
	}

	if len(source) > 0 {
		lines = append(lines, strings.Join(source, badgeSeparator))
	}

	return strings.Join(lines, badgeLineBreak)
}

// readmeGoSourceBadgeLine returns the Source·Go badge — the ORIGINAL Go source this package was
// converted from, in the Go repository at the release tag that produced it, so a reader of the C#
// can open the Go beside it rather than take the conversion's word for what it says.
//
// It is the Docs badge's counterpart, not its duplicate: Docs reaches the rendered documentation,
// this reaches the sources. An ordinary package links `github.com/golang/go/tree/go1.23.1/src/bufio`.
// A GOROOT-vendored package links the vendored tree — `.../src/vendor/golang.org/x/crypto/chacha20`,
// which is literally the directory the converter read — and states the module version GOROOT's own
// src/vendor/modules.txt records, because "@1.23.1" would name a Go release for sources that are a
// third-party module's snapshot.
//
// An unresolvable vendor pin degrades the badge's TEXT to the Go release rather than dropping the
// badge, which is where this parts company with the Docs badge. The distinction is what each badge
// promises: the Docs link's ADDRESS is composed from the pin, so an unpinnable one has nowhere
// honest to point, whereas this link is the GOROOT tree at `go1.23.1` and is fully pinned by the Go
// release alone — the module version only makes the text more precise.
func readmeGoSourceBadgeLine(importPath string, version string, goRoot string) string {
	if importPath == "" || version == "" {
		return ""
	}

	pin := version

	if vendored := strings.TrimPrefix(importPath, vendorImportPrefix); vendored != importPath {
		if _, modulePin, ok := vendoredModulePin(goRoot, vendored); ok {
			pin = modulePin
		}
	}

	return fmt.Sprintf("[![Source](%s/badge/Source-@%s-%s?logo=go)](%s/tree/go%s/src/%s)",
		shieldsBadgeHost, shieldsBadgeMessage(pin), goBrandColor, goRepositoryURL, version, importPath)
}

// readmeCSharpSourceBadgeLine returns the Source·C# badge — the CONVERTED C# this README ships
// beside, in the go2cs repository at the release tag this version was published under. Paired with
// the Source·Go badge it is the whole point of the line: the two sides of the conversion, one click
// apart, for the exact version the reader holds.
//
// The link is pinned twice over. `nuget-<version>` names the tag the release flow mints at snapshot
// time, and the path is the package's own directory beneath the go2cs root's core/ — taken from
// where the conversion is WRITING, not re-derived from the import path, so the badge cannot name a
// directory this conversion did not produce.
//
// Returns "" on the same terms as the Tests badge: no go2cs root or no version.props means no
// published version to pin and therefore no honest link, which is what a bare temp -go2cspath
// reconvert hits. A project path outside the root's core/ (a -recurse conversion that happens to sit
// under a go2cs root) has no place in the go2cs repository at all, so it says nothing.
func readmeCSharpSourceBadgeLine(projectPath string) string {
	root := findGo2CSRootAbove(projectPath)

	if root == "" {
		return ""
	}

	version := publishedPackageVersion(root)

	if version == "" {
		return ""
	}

	relative, err := filepath.Rel(filepath.Join(root, "core"), projectPath)

	if err != nil || relative == "." || relative == ".." || strings.HasPrefix(relative, ".."+string(filepath.Separator)) {
		return ""
	}

	// The badge carries no language text (the logo already carries it, matching the Docs badge style),
	// so there is no '#' in the message field and nothing to pre-escape here.
	return fmt.Sprintf("[![Source](%s/badge/Source-@%s-%s?logo=dotnet)](%s/tree/%s%s/src/core/%s)",
		shieldsBadgeHost, shieldsBadgeMessage(version), dotnetBrandColor,
		go2csRepositoryURL, releaseTagPrefix, version, filepath.ToSlash(relative))
}

// readmeDocsBadgeLine returns the Docs badge — a link to the official Go documentation for the very
// sources this package was converted from, PINNED to the version that produced them, so a reader of
// the C# can always reach the Go it mirrors.
//
// An ordinary standard-library package pins the Go release: `https://pkg.go.dev/bufio@go1.23.1`.
// `internal/...` packages are included and need no special case — pkg.go.dev serves them like any
// other std package.
//
// A GOROOT-VENDORED package (`vendor/golang.org/x/crypto/chacha20`) is not a Go release artifact at
// all; it is a snapshot of a third-party module, and its `vendor/`-prefixed path exists only inside
// GOROOT. It therefore pins the module version GOROOT's own src/vendor/modules.txt records, and the
// badge's MESSAGE states that pin rather than the Go version: the badge names the documentation it
// actually links, and "@1.23.1" over a link to x/crypto@v0.23.1-0.20240603234054-0b431c7de36a would
// name documentation that does not exist.
//
// Returns "" when the version or the import path is unknown, or when a vendored package has no
// modules.txt entry — an UNPINNED docs link would resolve to whatever is current rather than to the
// sources this package actually holds, which is the one thing the badge exists to promise.
func readmeDocsBadgeLine(importPath string, version string, goRoot string) string {
	if importPath == "" || version == "" {
		return ""
	}

	message := version
	target := fmt.Sprintf("%s/%s@go%s", goPackageDocsURL, importPath, version)

	if vendored := strings.TrimPrefix(importPath, vendorImportPrefix); vendored != importPath {
		modulePath, pin, ok := vendoredModulePin(goRoot, vendored)

		if !ok {
			return ""
		}

		message = pin
		target = fmt.Sprintf("%s/%s@%s", goPackageDocsURL, modulePath, pin)

		if subPath := strings.TrimPrefix(strings.TrimPrefix(vendored, modulePath), "/"); subPath != "" {
			target += "/" + subPath
		}
	}

	return fmt.Sprintf("[![Docs](%s/badge/Docs-@%s-%s?logo=go)](%s)", shieldsBadgeHost, shieldsBadgeMessage(message), goBrandColor, target)
}

// vendoredModulePin resolves the module path and the exact version a GOROOT-vendored package was
// drawn from, by reading the src/vendor/modules.txt the Go distribution ships beside those sources —
// the same file `go mod vendor` writes, and the only place the real version survives (the vendored
// tree itself carries no go.mod).
//
// The shape it reads is that file's stable core: `# <module> <version>` opens a module block, `##`
// annotates it, and every other non-blank line is one package path belonging to the open block.
// Anything it cannot resolve returns not-ok, which suppresses the badge rather than guessing a pin.
func vendoredModulePin(goRoot string, packagePath string) (string, string, bool) {
	contents, err := os.ReadFile(filepath.Join(goRoot, "src", "vendor", vendorModulesFileName))

	if err != nil {
		return "", "", false
	}

	var modulePath, pin string

	for _, line := range strings.Split(string(contents), "\n") {
		line = strings.TrimSpace(line)

		if line == "" || strings.HasPrefix(line, "##") {
			continue
		}

		if strings.HasPrefix(line, "#") {
			modulePath, pin = "", ""

			if fields := strings.Fields(strings.TrimPrefix(line, "#")); len(fields) >= 2 {
				modulePath, pin = fields[0], fields[1]
			}

			continue
		}

		if line == packagePath && modulePath != "" && pin != "" {
			return modulePath, pin, true
		}
	}

	return "", "", false
}

// stdLibImportPath is a standard-library package's Go import path, taken from the one thing that is
// unambiguous about it: where its sources sit under GOROOT/src.
//
// The loader's own PkgPath is NOT usable here. The same vendored package reports
// `golang.org/x/crypto/chacha20` under one load configuration and
// `vendor/golang.org/x/crypto/chacha20` under another, and `internal/abi` can come back as
// `std/internal/abi`. The directory says exactly one thing — and this strips the GOROOT prefix with
// the same case-insensitive pathReplace getProjectName uses, so the Docs badge's import path and the
// project's dotted name are guaranteed by construction to name the same package.
func stdLibImportPath(sourceDir string, goRoot string) string {
	if sourceDir == "" || goRoot == "" {
		return ""
	}

	sourceDir = filepath.Clean(sourceDir)
	trimmed, rewritten := pathReplace(sourceDir, filepath.Join(goRoot, "src"), "")

	// A no-match means this is not a GOROOT/src package after all, so there is no stdlib import path
	// to report — the same answer the old `trimmed == sourceDir` comparison gave, now stated by the
	// replace itself rather than re-derived. Silent by design: readmeValidationBadge calls this for
	// every conversion, including the non-stdlib ones that legitimately have no import path.
	if !rewritten {
		return ""
	}

	return strings.TrimPrefix(filepath.ToSlash(trimmed), "/")
}

// shieldsBadgeMessage escapes a message for shields.io's path form, where the three fields are
// dash-separated and an underscore renders as a space: a literal dash or underscore is doubled, a
// space becomes an underscore, and a slash is percent-encoded so it cannot end the path segment.
// (The `@` the messages open with needs no encoding — shields serves it raw.)
func shieldsBadgeMessage(value string) string {
	value = strings.ReplaceAll(value, "_", "__")
	value = strings.ReplaceAll(value, "-", "--")
	value = strings.ReplaceAll(value, " ", "_")

	return strings.ReplaceAll(value, "/", "%2F")
}

// readmeValidationBadgeLine returns the one-line validation badge for a converted stdlib package, or
// "" when this conversion has no repository context to compose an honest badge from.
//
// projectPath is the package's OUTPUT directory, projectName its dotted id (`path.filepath`, which
// is both the NuGet id suffix and the proof page's flat file name), and sourceDir the directory its
// Go sources came from.
func readmeValidationBadgeLine(projectPath string, projectName string, sourceDir string) string {
	root := findGo2CSRootAbove(projectPath)

	if root == "" {
		return ""
	}

	version := publishedPackageVersion(root)

	if version == "" {
		return ""
	}

	currentPath := filepath.Join(filepath.Dir(root), "docs", validationDocsDirName, validationCurrentDirName)

	if info, err := os.Stat(currentPath); err != nil || !info.IsDir() {
		return ""
	}

	// GREEN requires BOTH signals to agree: the package's converted test project is committed beside
	// it (the pipeline ran and its results were banked) AND its proof page states the totals the
	// badge is about to claim. They are 1:1 across the corpus; requiring both means a badge can
	// never claim a number no committed evidence backs, and any disagreement shows up as a census
	// miscount rather than as a wrong badge.
	if hasCommittedTestProject(projectPath, projectName) {
		if matched, disclosed, ok := validatedPackageTotals(currentPath, projectName); ok {
			return validationBadge(
				fmt.Sprintf("%d%%2F%d_validated", matched, matched+disclosed),
				"brightgreen",
				fmt.Sprintf("%s/%s/%s/%s.html", validationSiteURL, validationDocsDirName, version, projectName))
		}
	}

	// Without the Go sources the orange/grey distinction cannot be made honestly, and guessing
	// either way would misstate the package. Say nothing instead.
	if sourceDir == "" {
		return ""
	}

	if packageDeclaresGoTests(sourceDir) {
		return validationBadge("not_yet_validated", "orange", validationRosterURL)
	}

	return validationBadge("none_to_validate", "lightgrey", validationRosterURL)
}

// validationBadge renders one shields.io badge as a Markdown image link. The message is already
// shields-encoded by the caller (spaces as underscores, "/" as %2F).
func validationBadge(message string, color string, target string) string {
	return fmt.Sprintf("[![Tests](%s/badge/Tests-%s-%s?logo=go)](%s)", shieldsBadgeHost, message, color, target)
}

// publishedPackageVersion reads the four-part published version (`1.23.1.2`) from the go2cs root's
// version.props — the same single source push-nuget.ps1 bumps. Returns "" when the file is absent
// (a conversion outside a repository checkout) or does not carry both elements.
func publishedPackageVersion(root string) string {
	contents, err := os.ReadFile(filepath.Join(root, versionPropsFileName))

	if err != nil {
		return ""
	}

	base := firstSubmatch(goStdLibVersionPattern, string(contents))
	build := firstSubmatch(goBuildNumberPattern, string(contents))

	if base == "" || build == "" {
		return ""
	}

	return base + "." + build
}

func firstSubmatch(pattern *regexp.Regexp, contents string) string {
	if match := pattern.FindStringSubmatch(contents); match != nil {
		return strings.TrimSpace(match[1])
	}

	return ""
}

// hasCommittedTestProject reports whether the package's converted test project is banked beside its
// production code — `<dot-id>.tests.csproj`, written by the -tests pipeline and committed only for
// a package whose suite validated.
func hasCommittedTestProject(projectPath string, projectName string) bool {
	_, err := os.Stat(filepath.Join(projectPath, projectName+testProjectFileSuffix))

	return err == nil
}

// validatedPackageTotals reads the matched/disclosed counts off the package's living proof page.
// The page is the converter's own output and parseProofTotals is derived from the very format
// string that renders it, so the badge cannot drift from the evidence it links.
func validatedPackageTotals(currentPath string, dotID string) (int, int, bool) {
	contents, err := os.ReadFile(filepath.Join(currentPath, dotID+".md"))

	if err != nil {
		return 0, 0, false
	}

	return parseProofTotals(string(contents))
}

// packageDeclaresGoTests reports whether the package's Go sources declare any test function — the
// roster's own denominator rule, applied to the GOROOT package directory.
//
// The files are PARSED rather than scanned lexically: `func Test...` appears inside string literals
// and doc comments throughout the standard library's own test sources, and a badge that claims a
// package has tests because a comment mentioned one is worse than no badge. Parse errors are
// tolerated (the partial AST is still inspected) so one unparseable file cannot silently demote a
// package that plainly has tests.
func packageDeclaresGoTests(sourceDir string) bool {
	entries, err := os.ReadDir(sourceDir)

	if err != nil {
		return false
	}

	fileSet := token.NewFileSet()

	for _, entry := range entries {
		if entry.IsDir() || !strings.HasSuffix(entry.Name(), "_test.go") {
			continue
		}

		file, _ := parser.ParseFile(fileSet, filepath.Join(sourceDir, entry.Name()), nil, parser.SkipObjectResolution)

		if file == nil {
			continue
		}

		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Recv != nil || funcDecl.Name == nil {
				continue
			}

			if isGoTestFunctionName(funcDecl.Name.Name) {
				return true
			}
		}
	}

	return false
}

// isGoTestFunctionName applies `go test`'s own naming rule: a test is `Test` optionally followed by
// a suffix that does NOT start with a lower-case letter (`TestFoo`, `Test_foo` and bare `Test` are
// tests; `Testify` is not).
func isGoTestFunctionName(name string) bool {
	const prefix = "Test"

	if !strings.HasPrefix(name, prefix) {
		return false
	}

	suffix := name[len(prefix):]

	if suffix == "" {
		return true
	}

	return !unicode.IsLower([]rune(suffix)[0])
}
