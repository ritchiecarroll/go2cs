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

// A TEST FILE NOT BUILT FOR EVERY ARCHITECTURE ITS FLAVOUR SHIPS IS WITHDRAWN FROM BOTH SIDES ON THAT FLAVOUR
// (COORD rulings 2026-10-25 and 2026-10-26, option 2; BOARD "an amd64-constrained test file runs on the
// osx-arm64 RID").
//
// Each corpus flavour (corpusFlavourOf, testsCorpusArch.go) is converted at its own arch, and darwin's ONE
// flavour, converted at amd64, ships under both mac RIDs, osx-x64 and osx-arm64, where the roster banks ONE
// darwin count. A test file whose build line excludes arm64 was therefore run natively on arm64 too, where
// Go would never build it: crypto/sha1's issue15617_test.go (`//go:build amd64 && (linux || darwin)`)
// hardcodes 4 KiB pages, and on Apple silicon's 16 KiB pages its mprotect answers EINVAL. Withdrawing such a
// test only where it cannot build would split the two RIDs' counts, and a count that varies by RID cannot bank.
//
// So on a flavour that ships more than one architecture, a test declared in a file that some shipped
// architecture does not build leaves the run on EVERY RID of that flavour: its name joins the -skip string
// handed to both sides, exactly as a host-fatal test's does. It is deterministic, read from the source and
// the shipped set, so both mac RIDs answer the same. The windows and linux amd64 flavours ship amd64 alone,
// so nothing leaves there, and every withdrawn test still runs on them against the same amd64 code paths.
// The arm leg is the opposite of darwin: a linux/arm64 flavour is converted at arm64, run at arm64 and
// published under one RID, so nothing leaves there either. Not a disclosure: nothing diverged. The
// comparison record (testComparison.TargetArchExcluded) and the row's proof page name each one with its
// reason, so no count shrinks unstated.

// shippedGOARCHs answers the architectures the published go.* packages ship the corpus FLAVOUR serving a
// target (corpusFlavourOf) under, in Go's spelling: the RIDs push-nuget.ps1 packs (win-x64, linux-x64,
// osx-x64, osx-arm64), as compileRuntimeIdentifierForTarget maps them. A flavour ships its own arch, and
// darwin/amd64, the one flavour published under two RIDs, ships arm64 too; a target no flavour serves ships
// nothing. Per flavour and not per GOOS: per GOOS, a linux/arm64 flavour would read linux's amd64 and
// withdraw every test of an arm64-only file from its own run (S1b, 2026-11-09).
func shippedGOARCHs(lattice *platformLattice, target string) []string {
	flavour := corpusFlavourOf(lattice, target)

	if !lattice.includes(flavour) {
		return nil
	}

	if flavour == "darwin/amd64" {
		return []string{"amd64", "arm64"}
	}

	return []string{archOfTarget(flavour)}
}

// targetArchExclusion is one test withdrawn because its file is not built for every shipped architecture.
type targetArchExclusion struct {
	Name   string
	Detail string
}

// targetArchExcludedTests answers the Test/Benchmark/Fuzz/Example functions declared in the package's
// _test.go files that the conversion's target selects but some other architecture its flavour ships
// excludes, under the same GOOS and the conversion's own tags. Empty on a flavour that ships one
// architecture, unless that architecture is not the target's own.
func targetArchExcludedTests(inputPath string, options Options) []targetArchExclusion {
	return targetArchExcludedTestsOver(corpusLattice(), inputPath, options)
}

// targetArchExcludedTestsOver is targetArchExcludedTests under a given lattice.
func targetArchExcludedTestsOver(lattice *platformLattice, inputPath string, options Options) []targetArchExclusion {
	goos, targetArch, found := strings.Cut(options.targetPlatform, "/")

	if !found {
		return nil
	}

	shipped := shippedGOARCHs(lattice, options.targetPlatform)
	var others []string

	for _, arch := range shipped {
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

	if len(shipped) == 2 {
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

// buildLineSuffix renders a file's //go:build line for the record as a code span (" (`//go:build …`)"), or ""
// when the exclusion comes from the file name alone (x_amd64_test.go). The span is for the proof page, where the
// detail is markdown: a bare `||` in an OR'd constraint makes the withdrawn section a pipe-only block the site
// publishes as a table.
func buildLineSuffix(file *ast.File) string {
	for _, group := range file.Comments {
		if group.Pos() >= file.Package {
			break
		}

		for _, comment := range group.List {
			if strings.HasPrefix(comment.Text, "//go:build ") {
				return " (`" + comment.Text + "`)"
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

// proofPageArchWithdrawals answers, for a standard-library row's proof page, every test a flavour that ships
// more than one architecture withdraws from the package, one line each ("<goos>: <name>: <detail>"), in GOOS
// order, whatever platform writes the page: the page is the row's one record, and the withdrawal is read from
// the source, not from a run. nil when nothing is withdrawn, which leaves every other page unchanged.
func proofPageArchWithdrawals(goRoot, importPath string, buildTags []string) []string {
	return proofPageArchWithdrawalsOver(corpusLattice(), goRoot, importPath, buildTags)
}

// proofPageArchWithdrawalsOver is proofPageArchWithdrawals over a given lattice's flavours.
func proofPageArchWithdrawalsOver(lattice *platformLattice, goRoot, importPath string, buildTags []string) []string {
	if goRoot == "" || importPath == "" {
		return nil
	}

	dir := filepath.Join(goRoot, "src", filepath.FromSlash(importPath))
	var out []string

	flavours := append([]string(nil), lattice.universe...)
	sort.SliceStable(flavours, func(i, j int) bool { return goosOfTarget(flavours[i]) < goosOfTarget(flavours[j]) })

	for _, flavour := range flavours {
		if len(shippedGOARCHs(lattice, flavour)) < 2 {
			continue
		}

		goos := goosOfTarget(flavour)
		options := Options{targetPlatform: flavour, buildTags: buildTags}

		for _, line := range targetArchExcludedRecord(targetArchExcludedTestsOver(lattice, dir, options)) {
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
