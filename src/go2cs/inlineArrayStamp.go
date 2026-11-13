// inlineArrayStamp.go - Gbtc
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
	"go/token"
	"go/types"
	"regexp"
	"slices"
	"sort"
	"strings"
)

// INLINE-ARRAY STAMP (I0 condition 3; inlineArrayAdmission.go)
//
// The admission is decided once, by the conversion of the package's production files, and recorded in its
// package_info.cs. Two readers use the record:
//
//   - a -tests conversion of the package applies the recorded RULE to the production files and checks that the named
//     types it admits are the recorded ones. It never decides from _test.go, and never from its own -inline-arrays
//     value, which a -tests run need not repeat;
//   - a package that reinterprets another package's struct through unsafe.Pointer reads that package's record to judge
//     the pair rule (GO2CS2010) across the two packages.
//
// The record names the rule only when it is not the default ("forced" or "off"), and each admitted NAMED type by its Go
// name. An anonymous struct type is not recorded: the -tests check re-derives it under the recorded rule, and a pair
// that reaches another package's anonymous type is counted, not judged. A package with nothing to record carries no
// section, so a package that admits nothing is byte-identical to one converted before the stamp existed.

const (
	inlineArraysSectionStart = "// <InlineArrays>"
	inlineArraysSectionEnd   = "// </InlineArrays>"

	inlineArraysModeForced = "forced"
	inlineArraysModeOff    = "off"
	inlineArraysModeAuto   = "auto"
)

// inlineArraysProseLines is the section's explanatory comment.
func inlineArraysProseLines() []string {
	return []string{
		"// The struct types whose arrays this package holds inline (the go.arrays family), as the converter",
		"// admitted them: a `GoInlineArrayType` record per named type, and a `GoInlineArrays` record naming",
		"// the rule when it is not the default. A -tests conversion applies the same rule rather than deciding",
		"// again. The section exists only while there is a record to hold.",
	}
}

var inlineArraysSection = recordSection{
	start:    inlineArraysSectionStart,
	end:      inlineArraysSectionEnd,
	prose:    inlineArraysProseLines,
	isRecord: func(line string) bool { return inlineArrayRecordPattern.MatchString(strings.TrimSpace(line)) },
}

// inlineArrayRecordPattern parses either record, in any spelling rootAttributeName gives it.
var inlineArrayRecordPattern = regexp.MustCompile(`^\[assembly: (?:global::)?(?:go\.golib\.)?(GoInlineArrays|GoInlineArrayType)\("([^"]+)"\)\]$`)

// packageInlineArrayRecords holds the records the package being converted writes into its package_info.cs.
// resetPackageState clears it.
var packageInlineArrayRecords []string

// inlineArrayStamp is a package's recorded admission.
type inlineArrayStamp struct {
	mode  string   // inlineArraysModeAuto, inlineArraysModeForced or inlineArraysModeOff
	types []string // the admitted named types' Go names, sorted
}

// inlineArrayMode returns the rule the options select for a package, or "" for a package I0 never admits (std).
func inlineArrayMode(pkgPath string, pkgDir string, options Options) string {
	switch {
	case inlineArraysExcluded(pkgDir, options):
		return ""
	case options.inlineArrays.matches(options.inlineArrays.off, pkgPath):
		return inlineArraysModeOff
	case options.inlineArrays.matches(options.inlineArrays.force, pkgPath):
		return inlineArraysModeForced
	default:
		return inlineArraysModeAuto
	}
}

// inlineArrayStampRecords renders a package's records: its rule when not the default, and its admitted named types.
func inlineArrayStampRecords(a *inlineArrayAdmission, mode string) []string {
	var records []string

	if mode == inlineArraysModeForced || mode == inlineArraysModeOff {
		records = append(records, fmt.Sprintf("[assembly: %s(%q)]", rootAttributeName("golib.GoInlineArrays", false), mode))
	}

	for _, name := range a.namedTypes() {
		records = append(records, fmt.Sprintf("[assembly: %s(%q)]", rootAttributeName("golib.GoInlineArrayType", false), name))
	}

	return records
}

// namedTypes returns the Go names of the admitted named struct types the package declares, sorted.
func (a *inlineArrayAdmission) namedTypes() []string {
	if a == nil {
		return nil
	}

	var names []string
	prefix := a.pkgPath + "."

	for _, key := range a.keyOrder {
		if name, ok := strings.CutPrefix(key, prefix); ok && !strings.ContainsAny(name, "[ ") {
			names = append(names, name)
		}
	}

	sort.Strings(names)
	return names
}

// parseInlineArrayStamp reads a package_info.cs line set's record. A file with no section records the default rule
// and no type.
func parseInlineArrayStamp(lines []string) inlineArrayStamp {
	stamp := inlineArrayStamp{mode: inlineArraysModeAuto}

	for _, line := range lines {
		matches := inlineArrayRecordPattern.FindStringSubmatch(strings.TrimSpace(line))

		if matches == nil {
			continue
		}

		if matches[1] == "GoInlineArrays" {
			stamp.mode = matches[2]
		} else {
			stamp.types = append(stamp.types, matches[2])
		}
	}

	sort.Strings(stamp.types)
	return stamp
}

// readInlineArrayStamp reads the record a converted package's package_info.cs carries, reporting false when the file
// cannot be read.
func readInlineArrayStamp(packageInfoFile string) (inlineArrayStamp, bool) {
	lines, err := readPackageInfoLines(packageInfoFile)

	if err != nil {
		return inlineArrayStamp{}, false
	}

	return parseInlineArrayStamp(lines), true
}

// lookupInlineArrayStamp reads an IMPORTED package's record from its converted package_info.cs, reporting false when the
// package has not been converted into this output root (or is reached through a published assembly, whose metadata
// carries no record at I0). A variable so a test can stand in for converted dependencies.
var lookupInlineArrayStamp = func(importPath string, options Options) (inlineArrayStamp, bool) {
	info, ok := getImportPackageInfo([]string{importPath}, options)[importPath]

	if !ok || info.Err != nil || info.TargetDir == "" || info.NuGetSubstitution != nil {
		return inlineArrayStamp{}, false
	}

	return readInlineArrayStamp(platformPackageInfoPath(info.TargetDir, options.targetPlatform))
}

// foreignAdmits reports whether another package admitted the named struct type t, from that package's record, and
// whether the record could be read at all. A package I0 never admits (std) needs no record.
func (a *inlineArrayAdmission) foreignAdmits(t types.Type) (admitted bool, known bool) {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok || named.Obj().Pkg() == nil || named.TypeArgs().Len() > 0 {
		return false, false
	}

	pkgPath := named.Obj().Pkg().Path()

	if a.foreignStamps == nil {
		a.foreignStamps = map[string]*inlineArrayStamp{}
	}

	stamp, cached := a.foreignStamps[pkgPath]

	if !cached {
		if read, ok := lookupInlineArrayStamp(pkgPath, a.options); ok {
			stamp = &read
		}

		a.foreignStamps[pkgPath] = stamp
	}

	if stamp == nil {
		return false, false
	}

	_, found := slices.BinarySearch(stamp.types, named.Obj().Name())
	return found, true
}

// stampedOptions returns options whose -inline-arrays value applies a recorded rule to pkgPath and nothing else.
func stampedOptions(options Options, pkgPath string, mode string) Options {
	options.inlineArrays = inlineArrayOverride{}

	switch mode {
	case inlineArraysModeForced:
		options.inlineArrays.force = []string{pkgPath}
	case inlineArraysModeOff:
		options.inlineArrays.off = []string{pkgPath}
	}

	return options
}

// admitFromStamp is the -tests conversion's admission: the production files under the rule production recorded, checked
// against the named types production recorded. A disagreement means the production output on disk was not converted
// from these sources (or by this converter), and the test assembly would compile its files against another layout.
func admitFromStamp(files []*ast.File, fset *token.FileSet, pkg *types.Package, info *types.Info, pkgDir string, stamp inlineArrayStamp, options Options) (*inlineArrayAdmission, error) {
	if stamp.mode != inlineArraysModeAuto && stamp.mode != inlineArraysModeForced && stamp.mode != inlineArraysModeOff {
		return nil, fmt.Errorf("package_info.cs records inline-array rule %q, which this converter does not know", stamp.mode)
	}

	a := computeInlineArrayAdmission(files, fset, pkg, info, pkgDir, stampedOptions(options, pkg.Path(), stamp.mode))

	if got := a.namedTypes(); !slices.Equal(got, stamp.types) {
		return nil, fmt.Errorf("the production package_info.cs records inline arrays for %s, but its production files admit %s under the same rule; reconvert the package first",
			describeInlineNames(stamp.types), describeInlineNames(got))
	}

	return a, nil
}

func describeInlineNames(names []string) string {
	if len(names) == 0 {
		return "no named type"
	}

	return strings.Join(names, ", ")
}
