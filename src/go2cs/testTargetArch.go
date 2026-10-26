// testTargetArch.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"path/filepath"
	"regexp"
	"sort"
	"strings"

	"github.com/ritchiecarroll/hashset"
)

// A TEST FILE NOT BUILT FOR EVERY ARCHITECTURE ITS GOOS SHIPS IS WITHDRAWN FROM BOTH SIDES ON THAT GOOS
// (COORD rulings 2026-10-25 and 2026-10-26, option 2; BOARD "an amd64-constrained test file runs on the
// osx-arm64 RID").
//
// The corpus is converted for corpusArch, and darwin ships that ONE build under both mac RIDs, osx-x64 and
// osx-arm64, where the roster banks ONE darwin count. A test file whose build line excludes arm64 was
// therefore run natively on arm64 too, where Go would never build it: crypto/sha1's issue15617_test.go
// (`//go:build amd64 && (linux || darwin)`) hardcodes 4 KiB pages, and on Apple silicon's 16 KiB pages its
// mprotect answers EINVAL. Withdrawing such a test only where it cannot build would split the two RIDs'
// counts, and a count that varies by RID cannot bank.
//
// So on a GOOS that ships more than one architecture, a test declared in a file that some shipped
// architecture does not build leaves the run on EVERY RID of that GOOS: its name joins the -skip string
// handed to both sides, exactly as a host-fatal test's does. It is deterministic, read from the source and
// the shipped set, so both mac RIDs answer the same. windows and linux ship amd64 alone, so nothing leaves
// there, and every withdrawn test still runs on them against the same amd64 code paths. Not a disclosure:
// nothing diverged. The comparison record (testComparison.TargetArchExcluded) and the row's proof page name
// each one with its reason, so no count shrinks unstated.

// shippedGOARCHs answers the architectures the published go.* packages ship a GOOS's ONE converted build
// under, in Go's spelling: the RIDs push-nuget.ps1 packs (win-x64, linux-x64, osx-x64, osx-arm64), as
// compileRuntimeIdentifierForTarget maps them.
func shippedGOARCHs(goos string) []string {
	switch goos {
	case "darwin":
		return []string{"amd64", "arm64"}
	case "windows", "linux":
		return []string{"amd64"}
	default:
		return nil
	}
}

// targetArchExclusion is one test withdrawn because its file is not built for every shipped architecture.
type targetArchExclusion struct {
	Name   string
	Detail string
}

// targetArchExcludedTests answers the Test/Benchmark/Fuzz/Example functions declared in the package's
// _test.go files that the conversion's target selects but some other architecture its GOOS ships excludes,
// under the same GOOS and the conversion's own tags. Empty on a GOOS that ships one architecture.
func targetArchExcludedTests(inputPath string, options Options) []targetArchExclusion {
	goos, targetArch, found := strings.Cut(options.targetPlatform, "/")

	if !found {
		return nil
	}

	var others []string

	for _, arch := range shippedGOARCHs(goos) {
		if arch != targetArch {
			others = append(others, arch)
		}
	}

	if len(others) == 0 {
		return nil
	}

	paths, err := filepath.Glob(filepath.Join(inputPath, "*_test.go"))

	if err != nil {
		return nil
	}

	sort.Strings(paths)

	serves := "every shipped RID"

	if len(shippedGOARCHs(goos)) == 2 {
		serves = "both RIDs"
	}

	seen := hashset.HashSet[string]{}
	var result []targetArchExclusion

	for _, filePath := range paths {
		converted, err := CheckBuildConstraints(filePath, options.targetPlatform, options.buildTags, inputPath)

		if err != nil || !converted {
			continue // not in the conversion, so no RID carries it
		}

		var missing []string

		for _, arch := range others {
			if built, err := CheckBuildConstraints(filePath, goos+"/"+arch, options.buildTags, inputPath); err == nil && !built {
				missing = append(missing, goos+"/"+arch)
			}
		}

		if len(missing) == 0 {
			continue // built for every architecture the GOOS ships
		}

		fset := token.NewFileSet()
		file, err := parser.ParseFile(fset, filePath, nil, parser.SkipObjectResolution|parser.ParseComments)

		if err != nil {
			continue
		}

		detail := fmt.Sprintf("%s is not built for %s%s; one %s build serves %s",
			filepath.Base(filePath), strings.Join(missing, ", "), buildLineSuffix(file), goos, serves)

		for _, decl := range file.Decls {
			fn, ok := decl.(*ast.FuncDecl)

			if !ok || fn.Recv != nil || fn.Name == nil || fn.Type.Results.NumFields() != 0 || seen.Contains(fn.Name.Name) {
				continue
			}

			name := fn.Name.Name

			switch {
			case isGoTestName(name, "Example") && fn.Type.Params.NumFields() == 0,
				isGoTestName(name, "Test") && name != "TestMain" && hasSingleTestingParam(fn, "T"),
				isGoTestName(name, "Benchmark") && hasSingleTestingParam(fn, "B"),
				isGoTestName(name, "Fuzz") && hasSingleTestingParam(fn, "F"):
				seen.Add(name)
				result = append(result, targetArchExclusion{Name: name, Detail: detail})
			}
		}
	}

	return result
}

// buildLineSuffix renders a file's //go:build line for the record (" (//go:build …)"), or "" when the
// exclusion comes from the file name alone (x_amd64_test.go).
func buildLineSuffix(file *ast.File) string {
	for _, group := range file.Comments {
		if group.Pos() >= file.Package {
			break
		}

		for _, comment := range group.List {
			if strings.HasPrefix(comment.Text, "//go:build ") {
				return " (" + comment.Text + ")"
			}
		}
	}

	return ""
}

// targetArchExcludedNames answers the withdrawn tests' names.
func targetArchExcludedNames(exclusions []targetArchExclusion) []string {
	names := make([]string, 0, len(exclusions))

	for _, exclusion := range exclusions {
		names = append(names, exclusion.Name)
	}

	return names
}

// targetArchExcludedRecord renders the exclusions for testComparison.TargetArchExcluded, sorted.
func targetArchExcludedRecord(exclusions []targetArchExclusion) []string {
	var out []string

	for _, exclusion := range exclusions {
		out = append(out, exclusion.Name+": "+exclusion.Detail)
	}

	sort.Strings(out)
	return out
}

// proofPageArchWithdrawals answers, for a standard-library row's proof page, every test a GOOS that ships
// more than one architecture withdraws from the package, one line each ("<goos>: <name>: <detail>"),
// whatever platform writes the page: the page is the row's one record, and the withdrawal is read from the
// source, not from a run. nil when nothing is withdrawn, which leaves every other page unchanged.
func proofPageArchWithdrawals(goRoot, importPath string, buildTags []string) []string {
	if goRoot == "" || importPath == "" {
		return nil
	}

	dir := filepath.Join(goRoot, "src", filepath.FromSlash(importPath))
	var out []string

	for _, goos := range []string{"darwin", "linux", "windows"} {
		if len(shippedGOARCHs(goos)) < 2 {
			continue
		}

		options := Options{targetPlatform: goos + "/" + corpusArch, buildTags: buildTags}

		for _, line := range targetArchExcludedRecord(targetArchExcludedTests(dir, options)) {
			out = append(out, goos+": "+line)
		}
	}

	return out
}

// withdrawnSkipExpression is hostFatalSkipExpression over the host-fatal names AND the tests a shipped
// architecture does not build: the ONE regexp handed verbatim to both sides, anchored per name and sorted.
func withdrawnSkipExpression(disclosures map[string]testDisclosure, archExcluded []string) string {
	names := hashset.HashSet[string]{}

	for name, d := range disclosures {
		if d.Class == hostFatalClass {
			names.Add(regexp.QuoteMeta(name))
		}
	}

	for _, name := range archExcluded {
		names.Add(regexp.QuoteMeta(name))
	}

	if names.IsEmpty() {
		return ""
	}

	quoted := names.Keys()
	sort.Strings(quoted)
	return "^(?:" + strings.Join(quoted, "|") + ")$"
}
