// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// pcensus is the parity census instrument of the 1.26.10.2 visual-parity train (docs/PLAN-parity-train.md §1 "#22").
//
// It has two halves:
//   - count: per item of the plan's footprint table, the occurrences and files of its shape in each of the three
//     sets (production, converted tests, goldens), read from a checkout or from a git ref with no checkout;
//   - classify: every -/+ line pair of a unified diff is attributed to exactly one item's MECHANISM (the rewrite the
//     seat makes), to a named composition of two, or reported UNCLASSIFIED. It is the check each seat's golden
//     re-baseline (§3.3), each rung (§3.2) and the end regen's test diff (§3.4) are read through.
//
// Usage, from src/go2cs:
//
//	go run ./internal/pcensus count [-ref REF | -root DIR] [-items ID,...] [-re REGEX]
//	go run ./internal/pcensus classify [-items ID,...] [-test-only] DIFF
//	go run ./internal/pcensus items
//
// count prints one TSV row per item and set; classify prints the per-item pair counts and every pair it refuses,
// and exits 1 when any pair is UNCLASSIFIED, AMBIGUOUS, outside the -items set, or in a hunk whose -/+ counts differ.
package main

import (
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"
)

func main() {
	if len(os.Args) < 2 {
		usage()
	}
	switch os.Args[1] {
	case "count":
		os.Exit(runCount(os.Args[2:]))
	case "classify":
		os.Exit(runClassify(os.Args[2:]))
	case "items":
		for _, it := range items {
			fmt.Printf("%s\t%s\t%s\n", it.ID, it.Seat, it.Title)
		}
	default:
		usage()
	}
}

func usage() {
	fmt.Fprintln(os.Stderr, "usage: pcensus count [-ref REF | -root DIR] [-items ID,...] [-re REGEX]")
	fmt.Fprintln(os.Stderr, "       pcensus classify [-items ID,...] [-test-only] DIFF")
	fmt.Fprintln(os.Stderr, "       pcensus items")
	os.Exit(2)
}

// repoRoot answers the repository root from the working directory: the nearest ancestor holding src/go2cs.
func repoRoot() string {
	dir, _ := os.Getwd()
	for d := dir; ; d = filepath.Dir(d) {
		if st, err := os.Stat(filepath.Join(d, "src", "go2cs")); err == nil && st.IsDir() {
			return d
		}
		if filepath.Dir(d) == d {
			return dir
		}
	}
}

func runCount(args []string) int {
	fl := flag.NewFlagSet("count", flag.ExitOnError)
	ref := fl.String("ref", "", "read the sets from this git ref (no checkout)")
	root := fl.String("root", "", "read the sets from this checkout root (default: the repository holding the working directory)")
	only := fl.String("items", "", "comma-separated item IDs (default: all)")
	re := fl.String("re", "", "count this regular expression instead of the items (a probe)")
	fl.Parse(args)

	var files []file
	var err error
	if *ref != "" {
		files, err = readRef(repoRoot(), *ref)
	} else {
		if *root == "" {
			*root = repoRoot()
		}
		files, err = readTree(*root)
	}
	if err != nil {
		fmt.Fprintln(os.Stderr, "pcensus:", err)
		return 2
	}

	measures, err := selectMeasures(*only, *re)
	if err != nil {
		fmt.Fprintln(os.Stderr, "pcensus:", err)
		return 2
	}

	nfiles := map[string]int{}
	for _, f := range files {
		nfiles[f.set]++
	}
	fmt.Printf("# files\tprod=%d\ttest=%d\tgolden=%d\n", nfiles[setProd], nfiles[setTest], nfiles[setGolden])
	fmt.Println("item\tmeasure\tset\toccurrences\tfiles")
	for _, row := range census(files, measures) {
		fmt.Printf("%s\t%s\t%s\t%d\t%d\n", row.Item, row.Measure, row.Set, row.Occurrences, row.Files)
	}
	return 0
}

func selectMeasures(only, probe string) ([]measure, error) {
	if probe != "" {
		rx, err := regexp.Compile(probe)
		if err != nil {
			return nil, err
		}
		return []measure{{Item: "probe", Name: "probe", Pattern: rx}}, nil
	}
	want := map[string]bool{}
	for _, id := range strings.Split(only, ",") {
		if id = strings.TrimSpace(id); id != "" {
			want[id] = true
		}
	}
	var out []measure
	for _, it := range items {
		if len(want) > 0 && !want[it.ID] {
			continue
		}
		delete(want, it.ID)
		out = append(out, it.Measures...)
	}
	for id := range want {
		return nil, fmt.Errorf("no item %q (see `pcensus items`)", id)
	}
	return out, nil
}
