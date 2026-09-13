// toolchainResolution.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/version"
	"os"
	"path/filepath"
	"runtime"
	"strings"

	"golang.org/x/mod/modfile"
)

// Go 1.21 gave a module the power to demand a toolchain NEWER than the one the user invoked, and the
// go command satisfies that demand by re-execing a downloaded toolchain whose GOROOT lives in the
// module cache. go/packages inherits that choice — it loads from the module directory
// (moduleConverter.go's cfg.Dir) — while the converter resolved GOROOT from the AMBIENT environment,
// so the two could describe different standard libraries entirely.
//
// That divergence is not cosmetic: every "is this package standard library?" decision keys off
// GOROOT (ModuleConverter.classify, getImportPackageInfo, getLocalModulePackageInfo,
// resolveGorootVendoredPath), so a mismatch reclassifies the WHOLE standard library as third-party
// and emits references to projects that were never generated — while the run exits 0. Root cause,
// measurements and the reproduction: docs/phase4/FINDING-toolchain-goroot-divergence.md.
//
// Asking `go env GOROOT` from the loader's directory is what makes the two agree — the same move
// loaderReleaseTags already makes for GOVERSION, and for the same reason. It costs a subprocess
// (~300ms on Windows), which is why toolchainSwitchPossible gates it: nothing in the converted
// corpus and no behavioral test asks for a newer toolchain, so that cost stays at exactly zero for
// every run that could not have been affected.

// normalizeGoVersion puts a Go version into the `goX.Y[.Z]` form go/version accepts, tolerating the
// two spellings that reach us: `go env GOVERSION` and go.mod's `toolchain` carry the `go` prefix,
// go.mod's `go` directive does not. Returns "" for anything not a recognizable release version —
// a devel toolchain, a malformed directive, an empty string — leaving callers on their fallback.
func normalizeGoVersion(goVersion string) string {
	goVersion = strings.TrimSpace(goVersion)

	if goVersion == "" {
		return ""
	}

	if !strings.HasPrefix(goVersion, "go") {
		goVersion = "go" + goVersion
	}

	if !version.IsValid(goVersion) {
		return ""
	}

	return goVersion
}

// moduleToolchainRequest returns the newest Go version the go.mod at moduleRoot asks for — the
// higher of its `go` and `toolchain` directives — in normalized `goX.Y.Z` form, or "" when there is
// no readable go.mod or neither directive names a usable version.
//
// Parsing is LAX on purpose. Strict Parse rejects any directive newer than the x/mod this converter
// was built against, which is exactly the forward-compatible case this check exists to serve: a
// go.mod written by the newer toolchain we are trying to detect. A go.mod whose SYNTAX is broken
// still yields "" — there is nothing to read — and the caller falls back to the ambient GOROOT,
// which is the pre-existing behavior.
func moduleToolchainRequest(moduleRoot string) string {
	if moduleRoot == "" {
		return ""
	}

	goModPath := filepath.Join(moduleRoot, "go.mod")
	contents, err := os.ReadFile(goModPath)

	if err != nil {
		return ""
	}

	parsed, err := modfile.ParseLax(goModPath, contents, nil)

	if err != nil || parsed == nil {
		return ""
	}

	request := ""

	if parsed.Go != nil {
		request = normalizeGoVersion(parsed.Go.Version)
	}

	if toolchain := normalizeGoVersion(toolchainDirective(parsed)); toolchain != "" {
		if request == "" || version.Compare(toolchain, request) > 0 {
			request = toolchain
		}
	}

	return request
}

// toolchainDirective reads the `toolchain` line out of a laxly-parsed go.mod.
//
// It cannot come from parsed.Toolchain: ParseLax discards every directive outside
// go/module/retract/require before it reaches the semantic layer (x/mod modfile/rule.go's
// non-strict gate), so that field is ALWAYS nil here no matter what the file says. Skipping this
// and trusting parsed.Toolchain reads `go 1.21` + `toolchain go1.25.0` as a 1.21 request — the
// shape `go mod tidy` writes whenever it bumps a module — and silently disables the GOROOT
// re-resolution in the most common case that needs it. The retained syntax tree still holds the
// line, so read it from there.
func toolchainDirective(parsed *modfile.File) string {
	if parsed == nil || parsed.Syntax == nil {
		return ""
	}

	for _, statement := range parsed.Syntax.Stmt {
		line, isLine := statement.(*modfile.Line)

		if !isLine || len(line.Token) < 2 || line.Token[0] != "toolchain" {
			continue
		}

		return line.Token[1]
	}

	return ""
}

// toolchainSwitchPossible reports whether the go command might run a toolchain other than the one
// running this converter when it loads from moduleRoot. It is the cheap precondition that keeps the
// GOROOT re-resolution free for the overwhelming majority of runs: reading one go.mod versus
// spawning `go env`.
//
// Two documented triggers, both checked without a subprocess: an explicit GOTOOLCHAIN naming a
// specific toolchain, and a module asking for a release newer than the running one. A GOTOOLCHAIN
// persisted by `go env -w` rather than set in the environment is NOT visible here — that case falls
// back to the ambient GOROOT, which is the pre-existing behavior, so the gate can only ever improve
// on it.
func toolchainSwitchPossible(moduleRoot string) bool {
	// `auto` and `local` are the two selectors that cannot by themselves force a different
	// toolchain; anything else names one (`go1.24.0`, `path`, `go1.24.0+auto`).
	if selector := strings.TrimSpace(os.Getenv("GOTOOLCHAIN")); selector != "" && selector != "auto" && selector != "local" {
		return true
	}

	request := moduleToolchainRequest(moduleRoot)

	if request == "" {
		return false
	}

	running := normalizeGoVersion(runtime.Version())

	if running == "" {
		// A devel or otherwise unrecognizable running toolchain: we cannot prove a switch is
		// impossible, so pay the subprocess rather than guess.
		return true
	}

	return version.Compare(request, running) > 0
}

// resolveLoaderGoRoot returns the GOROOT and GOVERSION of the toolchain the go command actually
// selects when loading packages from inputPath, together with whether that differs from the ambient
// GOROOT the converter resolved at start-up. The version is returned alongside because both describe
// the same toolchain and both are wrong together: GOROOT decides what counts as standard library,
// GOVERSION becomes the emitted $(GoStdLibVersion). The version is "" when it could not be read,
// which leaves goVersion() on its own ambient lookup.
//
// inputPath may name a directory or a single file; the module root is found by walking up from it.
// Every failure path returns the ambient GOROOT unchanged — an unreachable go command, an
// unparseable answer, or no module context at all are all legitimate (converting standalone code is
// a supported mode), and none of them is a reason to abandon a GOROOT that is usually correct.
func resolveLoaderGoRoot(inputPath string, ambientGoRoot string) (string, string, bool) {
	if strings.TrimSpace(inputPath) == "" {
		return ambientGoRoot, "", false
	}

	inputDir := inputPath

	if info, err := os.Stat(inputPath); err == nil && !info.IsDir() {
		inputDir = filepath.Dir(inputPath)
	}

	moduleRoot := moduleRootDir(inputDir)

	if moduleRoot == "" || !toolchainSwitchPossible(moduleRoot) {
		return ambientGoRoot, "", false
	}

	resolved, err := getGoEnvFrom(moduleRoot, "GOROOT")

	if err != nil {
		return ambientGoRoot, "", false
	}

	resolved = strings.TrimSpace(resolved)

	if resolved == "" || samePath(resolved, ambientGoRoot) {
		return ambientGoRoot, "", false
	}

	// Only now — with a switch confirmed — is the second subprocess worth spending.
	loaderVersion := ""

	if value, versionErr := getGoEnvFrom(moduleRoot, "GOVERSION"); versionErr == nil {
		loaderVersion = strings.TrimSpace(value)
	}

	return resolved, loaderVersion, true
}

// publishedStdLibRelease is the Go release this converter emits go.<pkg> NuGet references for. It is
// the release the binary itself was BUILT with, which is the same thing: the repository's
// version.props tracks its go.mod, so a go2cs built on Go 1.23.1 is the converter whose corpus ships
// as go.<pkg> 1.23.1.x. Reading it from runtime keeps the two in step with no build plumbing and no
// file to go stale.
func publishedStdLibRelease() string {
	return version.Lang(normalizeGoVersion(runtime.Version()))
}

// checkNuGetStdLibCompatibility rejects a -recurse=nuget conversion whose standard library comes from
// a DIFFERENT Go release than the one go2cs publishes packages for.
//
// Without it the run succeeds and hands back a project that cannot restore: the emitted
// $(GoStdLibVersion) names a release, and every package that release does not contain — the six
// crypto/* and weak packages Go 1.24 added, in the reported case — resolves to nothing. The user
// meets the problem as a wall of NU1101s naming packages they never heard of, at restore time, with
// nothing pointing back at the toolchain. Both numbers are known here, before a single file is
// written, so this is the honest place to stop.
//
// Only the language release is compared: 1.23.1 versus 1.23.5 is the same standard library and the
// floating revision in $(GoStdLibVersion) already covers it. A converting release that cannot be
// parsed is not grounds to refuse — it means the toolchain could not be interrogated, which is a
// legitimate standalone-conversion state.
func checkNuGetStdLibCompatibility(convertingRelease string, publishedRelease string) error {
	converting := version.Lang(normalizeGoVersion(convertingRelease))

	if converting == "" || publishedRelease == "" || converting == publishedRelease {
		return nil
	}

	return fmt.Errorf(
		"-recurse=nuget cannot be satisfied: go2cs publishes its converted standard library as go.<pkg> packages for %s, "+
			"but the module being converted resolves to %s. Packages that %s added or moved have no published counterpart, "+
			"so the generated project would fail to restore (NU1101) on exactly those. "+
			"Either pin the module and its dependencies to a %s-compatible set, or drop -recurse=nuget and reference a "+
			"locally converted standard library instead",
		publishedRelease, converting, converting, publishedRelease)
}

// corpusPinnedRelease reads the Go release the CORPUS is pinned to — <GoStdLibVersion> from the go2cs
// root's version.props, the same element publishedPackageVersion composes the published four-part
// version from and the same one push-nuget.ps1 bumps.
//
// The root is read DIRECTLY rather than walked up from. Every caller already holds the resolved
// $(go2csPath) root — self-located for -tests, the output root under -stdlib — and that is precisely
// the tree whose corpus this conversion writes into or references. The badge machinery walks because
// it starts from a package directory deep inside core/; here the walk's answer is already in hand.
//
// Returns "" when the file is absent or carries no <GoStdLibVersion>. That is a fresh or unseeded
// root, which is the normal state of a first -stdlib conversion — not a pin to check.
func corpusPinnedRelease(root string) string {
	if root == "" {
		return ""
	}

	contents, err := os.ReadFile(filepath.Join(root, versionPropsFileName))

	if err != nil {
		return ""
	}

	return firstSubmatch(goStdLibVersionPattern, string(contents))
}

// gorootVersionFileName is Go's own record, at the root of a GOROOT, of which release that tree is.
const gorootVersionFileName = "VERSION"

// gorootRelease reads the Go release of the tree at goRoot out of its own VERSION file — the release
// whose SOURCES a conversion reading that GOROOT will actually convert.
//
// This is preferred over `go env GOVERSION`, because the two can disagree and when they do it is
// GOVERSION that misdescribes the input. An ambient GOROOT environment variable overrides the
// selected toolchain's own root, and main() treats a GOROOT it did not derive as PINNED — which
// switches off the resolveLoaderGoRoot correction above. A 1.23.1 go binary then reports go1.23.1
// while the converter reads a 1.23.2 tree, and a guard trusting the label would approve exactly the
// mixed-release conversion it exists to stop. Measured on a lane machine, 2026-08-11: GOROOT set to
// the 1.23.2 installation with a 1.23.1 toolchain selected.
//
// Returns "" when the file is absent or unreadable, leaving the caller on the reported version.
func gorootRelease(goRoot string) string {
	if goRoot == "" {
		return ""
	}

	contents, err := os.ReadFile(filepath.Join(goRoot, gorootVersionFileName))

	if err != nil {
		return ""
	}

	// Go 1.21 added a `time <stamp>` line beneath the version; the release is the first line.
	release, _, _ := strings.Cut(string(contents), "\n")

	return strings.TrimSpace(release)
}

// convertingRelease reports the Go release a corpus-defining conversion will read its INPUT from,
// which is the number the pin has to be checked against. GOROOT's own VERSION wins over the
// toolchain's reported version for the reason gorootRelease documents; the reported version remains
// the fallback for a GOROOT carrying no VERSION file.
func convertingRelease(goRoot string) string {
	if release := gorootRelease(goRoot); release != "" {
		return release
	}

	return goVersion()
}

// checkCorpusToolchainPin refuses a CORPUS-DEFINING conversion — -stdlib or -tests — running on a
// toolchain other than the release the corpus is pinned to.
//
// Both modes read GOROOT's sources as their INPUT, so the toolchain silently decides what gets
// converted. A -stdlib run emits the running toolchain's standard library into a tree every gate
// afterwards measures against the pinned release's goldens; a -tests run converts the OTHER release's
// test sources against this release's corpus, so no roster count can honestly come from it. Neither
// shows up as a failure, because each side stays internally consistent — which is why nothing caught
// it. Before this guard nothing anywhere asserted the pin: a lane that ran its gates on 1.23.2 against
// a 1.23.1 corpus passed at banked counts, and that was luck about patch-release test-set stability
// rather than protection.
//
// The comparison is the FULL release, PATCH INCLUDED — which is exactly where this parts company with
// checkNuGetStdLibCompatibility above. That guard compares version.Lang on purpose, because 1.23.1 and
// 1.23.5 publish the same set of go.<pkg> package IDs and the floating revision in $(GoStdLibVersion)
// already spans them. Here the patch is the entire point: 1.23.1 and 1.23.2 are different SOURCES, and
// converting one against goldens captured from the other is the drift this exists to stop. Equality is
// exact rather than ordered for the same reason — a pin is not a floor, and a corpus converted by a
// NEWER toolchain is no more measurable than one converted by an older one.
//
// A release that cannot be read on either side is never grounds to refuse. An absent version.props
// means a fresh or unseeded root (the normal first -stdlib conversion), and an unreadable GOVERSION
// means the toolchain could not be interrogated — neither is a mismatch, and manufacturing one would
// refuse legitimate runs. The failure this guard prevents is silent; its own failure mode is loud and
// names both numbers, so erring toward refusing is the safe direction whenever both ARE readable.
func checkCorpusToolchainPin(mode string, convertingVersion string, pinnedRelease string) error {
	converting := normalizeGoVersion(convertingVersion)
	pinned := normalizeGoVersion(pinnedRelease)

	if converting == "" || pinned == "" || converting == pinned {
		return nil
	}

	// version.props spells the release bare (it is also a NuGet version component); `go env GOVERSION`
	// spells it with the prefix. Each is quoted the way its own source writes it so the reader can go
	// look at both without translating.
	pinnedBare := strings.TrimPrefix(pinned, "go")

	consequence := fmt.Sprintf(
		"converting %s's test sources against a corpus built from %s, which is a run no roster count can honestly come from",
		converting, pinnedBare)

	if mode == "-stdlib" {
		consequence = fmt.Sprintf(
			"emitting %s's standard library into a corpus every gate afterwards measures against %s goldens",
			converting, pinnedBare)
	}

	return fmt.Errorf(
		"%s cannot be satisfied on this toolchain: %s pins the corpus to Go %s (<GoStdLibVersion>%s</GoStdLibVersion>), "+
			"but the Go tree this run would read is %s. %s reads GOROOT's sources as its INPUT, so it would end up %s — "+
			"a divergence no gate reports, because each side is internally consistent. "+
			"Either run on %s with GOROOT pointing at that same tree, or, if the corpus is deliberately moving to %s, "+
			"bump <GoStdLibVersion> to %s first. (That release is read from GOROOT's own VERSION file, so if "+
			"`go env GOVERSION` disagrees with it, a GOROOT environment variable is overriding the selected "+
			"toolchain — and GOROOT is the tree that actually gets converted.)",
		mode, versionPropsFileName, pinnedBare, pinnedBare, converting, mode, consequence,
		pinned, converting, strings.TrimPrefix(converting, "go"))
}
