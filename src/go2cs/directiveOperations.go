// directiveOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bufio"
	"fmt"
	"go/build"
	"go/build/constraint"
	"os"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"sync"
)

// goBuildPrefix is the token that opens a modern build-constraint line. constraint.Parse wants a
// whole line, so a caller holding only the expression text gets this prepended.
const goBuildPrefix = "//go:build "

// BuildConstraintEvaluator evaluates build constraints against a set of allowed platforms
type BuildConstraintEvaluator struct {
	allowedPlatforms map[string]bool

	// loaderDir is the directory go/packages loaded from, which selects the toolchain whose release
	// tags apply; empty means there is no loader context. releaseTags caches the resolved set and is
	// filled on FIRST USE — resolving it costs a `go env` subprocess, and most files never mention a
	// release tag at all (no behavioral package does, and exactly one standard-library package does),
	// so resolving eagerly would buy nothing and cost one subprocess per module root.
	loaderDir   string
	releaseTags []string
}

// NewBuildConstraintEvaluator creates a new build constraint evaluator
func NewBuildConstraintEvaluator(platforms []string) *BuildConstraintEvaluator {
	allowedMap := make(map[string]bool)

	// Track Unix-like systems to handle the "unix" tag
	hasUnixSystem := false

	unixSystems := map[string]bool{
		"linux":     true,
		"freebsd":   true,
		"netbsd":    true,
		"openbsd":   true,
		"dragonfly": true,
		"solaris":   true,
		"illumos":   true,
		"darwin":    true,
		"ios":       true,
	}

	// Handle all platforms
	for _, p := range platforms {
		allowedMap[p] = true

		// Split into OS and arch parts
		parts := strings.Split(p, "/")

		if len(parts) == 2 {
			os := parts[0]
			arch := parts[1]

			allowedMap[os] = true   // OS
			allowedMap[arch] = true // Architecture

			// Check if this is a Unix-like system
			if unixSystems[os] {
				hasUnixSystem = true
			}

			// Handle special case for js
			if os == "js" && arch == "wasm" {
				allowedMap["js"] = true
			}
		}
	}

	// Set special categories based on included platforms
	if hasUnixSystem {
		allowedMap["unix"] = true
	}

	// Automatically set "posix" if we have any Unix system (simplified - true POSIX compliance is more complex)
	if hasUnixSystem {
		allowedMap["posix"] = true
	}

	// Set standard compiler tags
	allowedMap["gc"] = true // Standard Go compiler
	allowedMap["gccgo"] = false
	allowedMap["tinygo"] = false

	// Go RELEASE tags (`go1.21`) are deliberately NOT seeded here — matchTag resolves them from the
	// toolchain's own list instead, so the answer cannot drift from the loader's.

	// For cgo, we defer to explicit settings rather than auto-detecting

	return &BuildConstraintEvaluator{
		allowedPlatforms: allowedMap,
	}
}

// SetTag marks a build tag as satisfied, so a file guarded by `//go:build <tag>` is included.
//
// This is how the -tags flag reaches the evaluator: every requested tag (purego by default under
// -stdlib) is turned on here before any file is scanned. Tags are stored alongside the GOOS/GOARCH
// values in the same map because a build constraint treats them identically — `linux`, `amd64` and
// `purego` are all just names that are either satisfied or not.
func (e *BuildConstraintEvaluator) SetTag(tag string, value bool) {
	e.allowedPlatforms[tag] = value
}

// matchTag reports whether a single build tag is satisfied for the target platform. It is the
// callback go/build/constraint's Expr.Eval drives, so EVERY name a constraint can mention — a GOOS,
// a GOARCH, the derived `unix`/`posix` categories, a -tags value, a Go release tag, a dotted tool
// tag — resolves here, and the boolean structure around them (`&&`, `||`, `!`, parentheses) is the
// stdlib's problem rather than ours.
//
// Tags are matched CASE-SENSITIVELY, as the Go toolchain matches them. The evaluator this replaced
// lowercased the whole expression first, which quietly made a mixed-case `-tags MyTag` unsatisfiable
// (the tag was stored verbatim by SetTag but looked up folded).
func (e *BuildConstraintEvaluator) matchTag(tag string) bool {
	// GOOS/GOARCH, the derived categories, the compiler tags and every -tags value share one map
	// because a build constraint treats them identically — `linux`, `amd64` and `purego` are all
	// just names that are either satisfied or not. A tag present-but-false (`gccgo`) is answered
	// here rather than falling through to the toolchain lists below.
	if allowed, ok := e.allowedPlatforms[tag]; ok {
		return allowed
	}

	// A Go RELEASE tag (`go1.21`) is not a platform property, so it must resolve exactly as it did
	// for go/packages when it selected these files: this evaluator only ever re-checks the loader's
	// output against a possibly-different TARGET platform, and any disagreement on a non-platform
	// tag can only drop code the build really does include. It is what decides between the paired
	// `go1.N`/`!go1.N` variants of a file (sort's sort_impl_go121.go / sort_impl_120.go), so getting
	// it wrong in either direction leaves the package with NEITHER half.
	//
	// Testing the SHAPE first is what keeps resolution lazy: no release tag in the constraint, no
	// `go env` subprocess. A release tag is also never a tool tag, so an unsatisfied one is final.
	if isGoReleaseTag(tag) {
		for _, releaseTag := range e.resolveReleaseTags() {
			if releaseTag == tag {
				return true
			}
		}

		return false
	}

	// A DOTTED build tag — `goexperiment.coverageredesign`, `amd64.v1` — is matched against the
	// host toolchain's active tool tags (go/build's ToolTags), again exactly as go/packages did.
	// Without this the tag falls through to `false`, so a `//go:build goexperiment.X` _on.go file
	// for an experiment ON by default (coverageredesign, regabiwrappers, regabiargs) was
	// re-EXCLUDED here after packages.Load had INCLUDED it, dropping that file's consts (testing's
	// `goexperiment.CoverageRedesign`, CS0117). An experiment that is off keeps its tag out of
	// ToolTags → false → the `!goexperiment.X` _off.go file is included instead, so exactly one of
	// the pair survives, as Go intends.
	if strings.Contains(tag, ".") {
		for _, toolTag := range build.Default.ToolTags {
			if strings.EqualFold(toolTag, tag) {
				return true
			}
		}
	}

	return false
}

// Release tags are resolved by asking the go command, not by trusting the list compiled into this
// binary, because GOTOOLCHAIN=auto makes the two diverge in an ordinary setup: the go command
// re-execs a NEWER toolchain when the main module asks for one, so go2cs.exe built with Go 1.23 while
// converting a module that declares `go 1.25` would evaluate `go1.24` against its own 1.23 list and
// silently drop every file gated between the two — along with the `!go1.24` sibling the loader had
// already excluded, leaving the package with neither. Over-exclusion is this evaluator's recurring
// failure mode (the purego and goexperiment seedings exist for the same reason), and it is the
// dangerous direction: the loader has already applied the full constraint for the target platform,
// so anything this pass subtracts is code the build really does contain.
//
// `go env` costs roughly 300ms on Windows, so the cost is contained twice over. Resolution is LAZY —
// a constraint that names no release tag never triggers it, which is every behavioral package and all
// but one standard-library package — and the answer is then cached per MODULE ROOT rather than per
// directory, because GOTOOLCHAIN resolution is a property of the module. Both matter: the behavioral
// corpus is 569 SEPARATE modules, so the cache alone would still pay 569 lookups to answer a question
// none of them asks, while a -stdlib run reaches the lookup once and GOROOT/src carries a single
// go.mod above all 302 packages.
var (
	releaseTagsLock  sync.Mutex
	releaseTagsCache = map[string][]string{}
)

// isGoReleaseTag reports whether a tag has the shape of a Go release tag — `go1.` followed by digits
// and nothing else. It is the cheap test that keeps release-tag resolution lazy.
func isGoReleaseTag(tag string) bool {
	minor, found := strings.CutPrefix(tag, "go1.")

	if !found || minor == "" {
		return false
	}

	for i := 0; i < len(minor); i++ {
		if minor[i] < '0' || minor[i] > '9' {
			return false
		}
	}

	return true
}

// resolveReleaseTags returns this evaluator's release tags, resolving them from the loader directory
// on first use and caching the answer for the rest of the evaluator's life.
func (e *BuildConstraintEvaluator) resolveReleaseTags() []string {
	if e.releaseTags == nil {
		e.releaseTags = loaderReleaseTags(e.loaderDir)
	}

	return e.releaseTags
}

// loaderReleaseTags returns the Go release tags satisfied by the toolchain the go command selects in
// loaderDir — the directory go/packages was configured to load from. An empty loaderDir means there
// is no loader context to ask about, so the tags compiled into this binary are the best answer
// available.
//
// It falls back to those same compiled-in tags whenever the go command cannot be reached or its
// answer cannot be parsed. Converting standalone code with no module context is legitimate, and an
// unreachable toolchain is never a reason to start excluding files.
func loaderReleaseTags(loaderDir string) []string {
	if loaderDir == "" {
		return build.Default.ReleaseTags
	}

	moduleRoot := moduleRootDir(loaderDir)

	releaseTagsLock.Lock()
	defer releaseTagsLock.Unlock()

	if tags, cached := releaseTagsCache[moduleRoot]; cached {
		return tags
	}

	tags := build.Default.ReleaseTags

	if version, err := getGoEnvFrom(moduleRoot, "GOVERSION"); err == nil {
		if resolved := releaseTagsForVersion(version); resolved != nil {
			tags = resolved
		}
	}

	releaseTagsCache[moduleRoot] = tags

	return tags
}

// releaseTagsForVersion expands a `go env GOVERSION` string (`go1.25.0`, `go1.25rc1`, `devel …`) into
// the release-tag list a toolchain of that version satisfies: go1.1 through its own minor, which is
// exactly how go/build builds the list. Returns nil when the version is not of a recognizable release
// shape, leaving the caller on its fallback.
func releaseTagsForVersion(version string) []string {
	minor := strings.TrimPrefix(version, "go1.")

	if minor == version {
		return nil
	}

	// Trim any patch or pre-release suffix: `25.0` and `25rc1` both describe Go 1.25.
	end := 0

	for end < len(minor) && minor[end] >= '0' && minor[end] <= '9' {
		end++
	}

	latest, err := strconv.Atoi(minor[:end])

	if err != nil || latest < 1 {
		return nil
	}

	tags := make([]string, 0, latest)

	for i := 1; i <= latest; i++ {
		tags = append(tags, fmt.Sprintf("go1.%d", i))
	}

	return tags
}

// moduleRootDir returns the nearest ancestor of dir that holds a go.mod — the unit GOTOOLCHAIN
// resolution actually keys on — or dir itself when there is none.
func moduleRootDir(dir string) string {
	if dir == "" {
		return ""
	}

	for current := filepath.Clean(dir); ; {
		if _, err := os.Stat(filepath.Join(current, "go.mod")); err == nil {
			return current
		}

		parent := filepath.Dir(current)

		if parent == current {
			return filepath.Clean(dir)
		}

		current = parent
	}
}

// EvaluateConstraint evaluates a build constraint against the allowed platforms. It accepts either a
// bare expression (`go1.21 && amd64`) or a whole constraint line in either syntax (`//go:build …`,
// `// +build …`), so a caller can hand over whichever form it is holding.
func (e *BuildConstraintEvaluator) EvaluateConstraint(expression string) (bool, error) {
	line := strings.TrimSpace(expression)

	if !constraint.IsGoBuild(line) && !constraint.IsPlusBuild(line) {
		line = goBuildPrefix + line
	}

	expr, err := constraint.Parse(line)

	if err != nil {
		return false, fmt.Errorf("failed to parse build constraint: %w", err)
	}

	return expr.Eval(e.matchTag), nil
}

// readFileBuildConstraint returns the build constraint a Go source file declares, or nil when it
// declares none.
//
// Only the file HEADER is scanned — everything above the package clause, line comments only —
// matching go/build's own reader: a `//go:build` line quoted in documentation below the package
// clause, or sitting inside a block comment, is prose and not a constraint. go/build's precedence
// between the two syntaxes is applied verbatim: a `//go:build` line wins outright, and the legacy
// `// +build` lines apply only in its absence (ANDed across lines, with `,` meaning AND and a space
// meaning OR within one). Legacy-only files are exactly what a -recurse run meets in third-party
// modules predating Go 1.17, and the regex-based scanner this replaced could not see them at all —
// it only matched `//go:`-prefixed lines, so those files silently converted as unconstrained.
func readFileBuildConstraint(filename string) (constraint.Expr, error) {
	file, err := os.Open(filename)

	if err != nil {
		return nil, fmt.Errorf("failed to open file %s: %w", filename, err)
	}

	defer file.Close()

	var goBuildLine string
	var plusBuildLines []string

	scanner := bufio.NewScanner(file)
	inBlockComment := false

	for scanner.Scan() {
		line := scanner.Text()
		trimmed := strings.TrimSpace(line)

		if inBlockComment {
			end := strings.Index(trimmed, "*/")

			if end < 0 {
				continue
			}

			inBlockComment = false
			trimmed = strings.TrimSpace(trimmed[end+len("*/"):])
		} else if strings.HasPrefix(trimmed, "/*") && !strings.Contains(trimmed, "*/") {
			inBlockComment = true
			continue
		}

		// Constraints live above the package clause; nothing below it can be one.
		if trimmed == "package" || strings.HasPrefix(trimmed, "package ") || strings.HasPrefix(trimmed, "package\t") {
			break
		}

		// The untrimmed line is what gets tested: both recognizers require the comment to start at
		// column zero, which is the rule the toolchain enforces.
		switch {
		case constraint.IsGoBuild(line):
			// Go permits at most one //go:build line per file. Keep the first and ignore any stray
			// extras rather than inventing a combination the toolchain would have rejected outright.
			if goBuildLine == "" {
				goBuildLine = line
			}
		case constraint.IsPlusBuild(line):
			plusBuildLines = append(plusBuildLines, line)
		}
	}

	if err := scanner.Err(); err != nil {
		return nil, fmt.Errorf("failed to read file %s: %w", filename, err)
	}

	if goBuildLine != "" {
		expr, err := constraint.Parse(goBuildLine)

		if err != nil {
			return nil, fmt.Errorf("failed to parse build constraint: %w", err)
		}

		return expr, nil
	}

	// Multiple `// +build` lines are ANDed together, so they combine into one expression the same way
	// go/build's own reader combines them.
	var plusBuildExpr constraint.Expr

	for _, plusBuildLine := range plusBuildLines {
		expr, err := constraint.Parse(plusBuildLine)

		if err != nil {
			return nil, fmt.Errorf("failed to parse build constraint: %w", err)
		}

		if plusBuildExpr == nil {
			plusBuildExpr = expr
		} else {
			plusBuildExpr = &constraint.AndExpr{X: plusBuildExpr, Y: expr}
		}
	}

	return plusBuildExpr, nil
}

// isFileNameCompatible checks if a Go source file should be included in a build
// based on its filename suffixes (e.g., _linux.go, _windows_amd64.go)
func isFileNameCompatible(filename string, evaluator *BuildConstraintEvaluator) bool {
	// Ignore non-Go files
	if !strings.HasSuffix(filename, ".go") {
		return false
	}

	// Extract the base name without extension
	base := strings.TrimSuffix(filepath.Base(filename), ".go")

	// If the file doesn't have an underscore, it's included in all builds
	if !strings.Contains(base, "_") {
		return true
	}

	// Check for test files which have different build rules
	isTest := strings.HasSuffix(base, "_test")
	if isTest {
		base = strings.TrimSuffix(base, "_test")
	}

	parts := strings.Split(base, "_")
	n := len(parts)

	// A trailing GOOS/GOARCH suffix only acts as a build constraint when the
	// final one or two underscore-delimited components are recognized GOOS
	// and/or GOARCH names (matching go/build's goodOSArchFile). Descriptive
	// suffixes like "_tables" or "_errors" are part of the filename and impose
	// no constraint, so files such as bits_tables.go must not be excluded.
	if n < 2 {
		return true
	}

	last := parts[n-1]

	// name_GOOS_GOARCH.go: second-to-last is a GOOS and last is a GOARCH; both must match.
	if n >= 3 {
		secondLast := parts[n-2]
		if isKnownGOOS(secondLast) && isKnownGOARCH(last) {
			return evaluator.allowedPlatforms[secondLast] && evaluator.allowedPlatforms[last]
		}
	}

	// name_GOARCH.go
	if isKnownGOARCH(last) {
		return evaluator.allowedPlatforms[last]
	}

	// name_GOOS.go
	if isKnownGOOS(last) {
		return evaluator.allowedPlatforms[last]
	}

	// Trailing component is neither a GOOS nor a GOARCH: not a platform constraint.
	return true
}

// knownGOOS lists the recognized Go target operating systems (go/build/syslist.go).
var knownGOOS = map[string]bool{
	"aix": true, "android": true, "darwin": true, "dragonfly": true,
	"freebsd": true, "hurd": true, "illumos": true, "ios": true, "js": true,
	"linux": true, "nacl": true, "netbsd": true, "openbsd": true, "plan9": true,
	"solaris": true, "wasip1": true, "windows": true, "zos": true,
}

// knownGOARCH lists the recognized Go target architectures (go/build/syslist.go).
var knownGOARCH = map[string]bool{
	"386": true, "amd64": true, "amd64p32": true, "arm": true, "armbe": true,
	"arm64": true, "arm64be": true, "loong64": true, "mips": true, "mipsle": true,
	"mips64": true, "mips64le": true, "mips64p32": true, "mips64p32le": true,
	"ppc": true, "ppc64": true, "ppc64le": true, "riscv": true, "riscv64": true,
	"s390": true, "s390x": true, "sparc": true, "sparc64": true, "wasm": true,
}

func isKnownGOOS(s string) bool   { return knownGOOS[s] }
func isKnownGOARCH(s string) bool { return knownGOARCH[s] }

// CheckBuildConstraints reports whether a Go source file belongs in the build for the given target
// platform, honoring both its filename suffixes and its declared build constraint.
//
// loaderDir is the directory go/packages loaded this file's package from; it selects the toolchain
// whose Go release tags the constraint is evaluated against (see loaderReleaseTags). Pass "" when
// there is no loader context and the tags compiled into this binary are the best available answer.
func CheckBuildConstraints(filename string, targetPlatform string, buildTags []string, loaderDir string) (bool, error) {
	// Create a new build constraint evaluator
	evaluator := NewBuildConstraintEvaluator([]string{targetPlatform})
	evaluator.loaderDir = loaderDir

	// A -tags tag must be honored HERE too, not just by the go/packages loader. The loader selects
	// files with the tag applied — `-tags purego` picks crypto/sha256's sha256block_generic.go, which
	// has a real body, over the bodyless sha256block_decl.go — but every file it selected is then
	// re-checked against this evaluator, where an unknown identifier evaluates to false. Without the
	// tag seeded, `//go:build (!amd64 && ...) || purego` fails here and the file the tag just selected
	// is RE-EXCLUDED, leaving the package with no implementation at all rather than the portable one.
	// Same class of bug as the dotted goexperiment tags handled in evaluateExpr.
	for _, tag := range buildTags {
		evaluator.SetTag(tag, true)
	}

	// First, check if the filename itself indicates build constraints
	if !isFileNameCompatible(filename, evaluator) {
		return false, nil
	}

	expr, err := readFileBuildConstraint(filename)

	if err != nil {
		return false, err
	}

	// If the file declares no constraint, the filename check above is the whole verdict
	if expr == nil {
		return true, nil
	}

	return expr.Eval(evaluator.matchTag), nil
}

// containsManualConversionMarker checks if a file contains the GoManualConversion module marker
// that is not commented out. It supports both the standard form "[module: go.GoManualConversion]"
// and the C# attribute form "[module: go.GoManualConversionAttribute]", with or without
// the "go." namespace prefix. Function will exit early if a class definition is detected, as the
// marker should appear before class definitions.
func containsManualConversionMarker(filename string) (bool, error) {
	return containsModuleMarker(filename, ManualConversionMarker)
}

// containsRequiresUnsafeMarker checks if a file declares the GoRequiresUnsafe module marker — a
// hand-owned file stating that its own code needs `/unsafe`, which the converter's `usesUnsafeCode`
// predicate cannot see because that predicate only observes the converter's OWN emission.
//
// Deliberately the same scan as the hand-own marker above rather than a second one: the lexing this
// shares (stripCSharpComments) is the whole reason a marker mentioned in prose, or one sitting after
// a line comment that merely LOOKS like it opens a block, is not mistaken for a declaration.
func containsRequiresUnsafeMarker(filename string) (bool, error) {
	return containsModuleMarker(filename, RequiresUnsafeMarker)
}

// containsModuleMarker reports whether a file declares the named module-scoped marker attribute in
// code — not in a comment. Both spellings C# accepts are matched (with or without the `go.`
// namespace qualifier, with or without the `Attribute` suffix), and the scan stops at the first
// class definition because every marker belongs to the file header above it.
func containsModuleMarker(filename string, markerName string) (bool, error) {
	// Check if file exists
	if _, err := os.Stat(filename); os.IsNotExist(err) {
		return false, nil
	}

	file, err := os.Open(filename)

	if err != nil {
		return false, fmt.Errorf("error opening file: %w", err)
	}

	defer file.Close()

	scanner := bufio.NewScanner(file)

	// Regex to match the module pattern with or without "go." namespace prefix
	// and with or without the "Attribute" suffix (for C# compatibility)
	modulePatternRE := regexp.MustCompile(`\[\s*module\s*:\s*(?:go\.)?\s*` + regexp.QuoteMeta(markerName) + `(?:Attribute)?\s*\]`)

	// Regex to detect class definition
	// This looks for the word "class" surrounded by spaces/boundaries
	// followed by an identifier (which is a sequence of letters, digits, or underscores, starting with a letter)
	classDefinitionRE := regexp.MustCompile(`\bclass\s+\w+`)

	// Whether the scan is inside an unterminated /* ... */ block, carried across lines
	inBlockComment := false

	for scanner.Scan() {
		// Comments are removed by a lexer rather than probed for with Contains, so a delimiter that
		// only LOOKS like an opener cannot hide the rest of the file (see stripCSharpComments)
		code := stripCSharpComments(scanner.Text(), &inBlockComment)

		if strings.TrimSpace(code) == "" {
			continue
		}

		// Check if the line contains the marker
		if modulePatternRE.MatchString(code) {
			return true, nil
		}

		// Check if the line contains a class definition - exit early if found
		// This is fine because valid module markers will come before class definitions
		if classDefinitionRE.MatchString(code) {
			return false, nil
		}
	}

	if err := scanner.Err(); err != nil {
		return false, fmt.Errorf("error reading file: %w", err)
	}

	return false, nil
}

// stripCSharpComments returns the code content of one line with its comments removed, carrying the
// open-block state across lines through inBlockComment.
//
// The ORDER the two openers are tested in is the whole point. This was a pair of strings.Contains
// probes that asked "does this line contain /*?" WITHOUT first asking whether that /* was itself
// inside a // line comment. core/runtime/runtime2_impl.cs opens with prose describing Go's pointer
// family — "hide a *g/*p/*m inside a uintptr" — whose `/*` is ordinary English inside a line
// comment. The probe read it as opening a block that never closes, so every line after it was
// treated as comment interior, including the file's own [module: GoManualConversion] on line 24:
// the converter's marker predicate reported a hand-owned file as NOT hand-owned. Inert where it was
// found (an _impl companion has no Go counterpart emitted at that path, so nothing overwrites it),
// but it is precisely the shape that silently clobbers a whole-file hand-own on the next reconvert.
//
// In C# a // comment runs to the end of the line and nothing inside it opens a block comment, which
// is what this lexes. String literals are deliberately NOT lexed: the region this predicate reads is
// a file header of usings, comments and attributes, and the scan stops at the first class
// definition, so a delimiter inside a literal cannot reach it.
func stripCSharpComments(line string, inBlockComment *bool) string {
	var code strings.Builder

	// Byte-wise is safe for the ASCII delimiters below: no UTF-8 continuation byte can be one.
	for index := 0; index < len(line); {
		remaining := line[index:]

		if *inBlockComment {
			if strings.HasPrefix(remaining, "*/") {
				*inBlockComment = false
				index += 2
				continue
			}

			index++
			continue
		}

		// Tested BEFORE the block opener: a /* that follows a // on the same line opens nothing.
		if strings.HasPrefix(remaining, "//") {
			break
		}

		if strings.HasPrefix(remaining, "/*") {
			*inBlockComment = true
			index += 2
			continue
		}

		code.WriteByte(line[index])
		index++
	}

	return code.String()
}
