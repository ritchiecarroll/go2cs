// main.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Command e1census sizes the lift of the M:N twin generator's rule E1: it counts the generic functions and methods
// of generic types that park, directly or through their static call closure (census.go says what parks).
//
//	go run ./internal/e1census -label std std
//	go run ./internal/e1census -label x/sync -dir <module root> ./...
//	go run ./internal/e1census -list ...   (also prints one line per parking generic, with its first reason)
//
// The load is go/packages over the patterns, with the syntax of every dependency, so a call into the standard
// library is followed into its body. GOOS and GOARCH are the environment's; the record names them.
package main

import (
	"flag"
	"fmt"
	"os"
	"runtime"
	"strings"

	"golang.org/x/tools/go/packages"
)

func main() {
	dir := flag.String("dir", "", "the directory the patterns are resolved in (a module root)")
	label := flag.String("label", "", "the row's name in the output")
	list := flag.Bool("list", false, "also print each parking generic and its first reason")
	flag.BoolVar(&followRuntime, "runtime", false, "follow calls into package runtime and internal/runtime/... (off: the boundary census.go states)")
	why := flag.String("why", "", "print how the named function (pkgpath.Name or pkgpath.(*Recv).Name) parks, in both readings")
	flag.Parse()

	roots, err := load(*dir, flag.Args())
	if err != nil {
		fmt.Fprintln(os.Stderr, "e1census:", err)
		os.Exit(1)
	}

	c := build(roots)
	r, parkers := c.summarize(*label)
	goos, goarch := os.Getenv("GOOS"), os.Getenv("GOARCH")
	if goos == "" {
		goos = runtime.GOOS
	}
	if goarch == "" {
		goarch = runtime.GOARCH
	}
	fmt.Printf("e1census %s/%s, %d target packages, %s, runtime followed: %v\n", goos, goarch, len(roots), runtime.Version(), followRuntime)
	fmt.Println("module\tfuncs\tparkers\tgeneric\tgeneric-exported\tgeneric-parks-direct\tgeneric-parks-transitive\tgeneric-parkers-exported\tgeneric-noparks-dynamic\tgeneric-noparks-static\tgeneric-parks-nolocks\tgeneric-parks-nolocks-exported")
	fmt.Printf("%s\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\t%d\n", r.Module, r.Funcs, r.Parkers, r.Generic, r.GenericExported,
		r.GenericDirect, r.GenericTransitive, r.GenericParkersExported, r.GenericNoParkDynamic, r.GenericNoParkStatic,
		r.GenericParksNoLocks, r.GenericParksNoLocksExported)
	if *why != "" {
		fmt.Println("WHY all-parks:", strings.Join(c.chain(*why, false), " -> "))
		fmt.Println("WHY no-locks: ", strings.Join(c.chain(*why, true), " -> "))
	}
	if *list {
		for _, p := range parkers {
			exp := "unexported"
			if p.Exported {
				exp = "exported"
			}
			kind := "parks"
			if p.Locks {
				kind = "locks-only"
			}
			fmt.Printf("PARKS\t%s\t%s\t%s\t%s\n", p.Name, exp, kind, p.Reason)
		}
	}
}

// load type-checks the patterns with every dependency's syntax, refusing a load whose packages carry errors (a
// census over a partly type-checked closure would under-count silently).
func load(dir string, patterns []string) ([]*packages.Package, error) {
	if len(patterns) == 0 {
		return nil, fmt.Errorf("no package patterns")
	}
	cfg := &packages.Config{Mode: packages.LoadAllSyntax, Dir: dir, Tests: false}
	roots, err := packages.Load(cfg, patterns...)
	if err != nil {
		return nil, err
	}
	if len(roots) == 0 {
		return nil, fmt.Errorf("the patterns matched no package")
	}
	var bad int
	packages.Visit(roots, nil, func(p *packages.Package) {
		for _, e := range p.Errors {
			bad++
			if bad <= 5 {
				fmt.Fprintln(os.Stderr, "e1census: load error:", e)
			}
		}
	})
	if bad > 0 {
		return nil, fmt.Errorf("%d package error(s) in the load", bad)
	}
	return roots, nil
}
