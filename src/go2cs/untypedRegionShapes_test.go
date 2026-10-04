// untypedRegionShapes_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A package that does not fully type-check converts best-effort (issue #33,
// untypedPackageConversion_test.go), and go/types records NO type for an expression whose operand went
// invalid, so types.Info.TypeOf returns nil for it. The visitor reads TypeOf results at hundreds of
// sites and calls methods on them; guarding them one panic at a time is the approach issue #33 began and
// the shapes below outran (`return undefinedName + len(s)` faulted in convBinaryExprCore, reported as
// "visit file error" and losing the whole file).
//
// So the guard is per CONTEXT, not per site: one invalid operand in every expression context the
// visitor has a path for, each converted on its own with the per-file recover OFF (debugMode), so a
// fault surfaces as a panic here instead of a skipped file. Each must convert and keep the healthy
// declaration beside it.

package main

import (
	"bytes"
	"go/build"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"runtime/debug"
	"strings"
	"testing"
)

// untypedRegionShapes: `u` and `U` are undefined, so every use of them is an invalid operand; a few
// shapes use a valid operand in an invalid operation instead (mismatched operands, a wrong argument
// count), which leaves the same untyped region behind.
var untypedRegionShapes = []struct{ name, decls, body string }{
	{"binary", "", "x := u + 1; _ = x"},
	{"binaryRet", "", "var s string = 1; _ = s; println(u + len(s))"},
	{"compareNil", "", "if u == nil { }"},
	{"unaryNeg", "", "x := -u; _ = x"},
	{"unaryNot", "", "if !u { }"},
	{"addr", "", "p := &u; _ = p"},
	{"deref", "", "x := *u; _ = x"},
	{"callFun", "", "u()"},
	{"callArg", "", "println(u)"},
	{"callArgFmt", "", "fmt.Println(u)"},
	{"methodCall", "", "u.M()"},
	{"field", "", "x := u.F; _ = x"},
	{"index", "", "x := u[0]; _ = x"},
	{"indexAssign", "", "u[0] = 1"},
	{"slice", "", "x := u[1:]; _ = x"},
	{"compositeElem", "", "x := []int{u}; _ = x"},
	{"mapLit", "", "m := map[string]int{\"a\": u}; _ = m"},
	{"structLitBadField", "type H struct{ N int }", "h := H{Z: 1}; _ = h"},
	{"conversion", "", "x := int(u); _ = x"},
	{"typeAssert", "", "x := u.(int); _ = x"},
	{"typeSwitch", "", "switch u.(type) { case int: }"},
	{"typeSwitchBind", "", "switch y := u.(type) { case int: _ = y }"},
	{"switchTag", "", "switch u { case 1: }"},
	{"rangeKey", "", "for i := range u { _ = i }"},
	{"rangeKV", "", "for k, v := range u { _, _ = k, v }"},
	{"returnU", "func r() int { return u }", ""},
	{"multiAssign", "", "a, b := u(); _, _ = a, b"},
	{"assignOp", "", "x := 0; x += u; _ = x"},
	{"incU", "", "u++"},
	{"goStmt", "", "go u()"},
	{"deferStmt", "", "defer u()"},
	{"send", "", "ch := make(chan int); ch <- u"},
	{"recv", "", "x := <-u; _ = x"},
	{"selectRecv", "", "select { case x := <-u: _ = x }"},
	{"closure", "", "f := func() int { return u }; _ = f"},
	{"pkgVar", "var V = u", ""},
	{"pkgVarTyped", "var V int = u", ""},
	{"pkgConst", "const C = u", ""},
	{"concat", "", "s := \"a\" + u; _ = s"},
	{"lenU", "", "n := len(u); _ = n"},
	{"appendU", "", "s := append(u, 1); _ = s"},
	{"makeU", "", "m := make(U); _ = m"},
	{"newU", "", "p := new(U); _ = p"},
	{"structFieldU", "type S struct{ F U }", "var s S; _ = s"},
	{"paramU", "func g(x U) { _ = x }", ""},
	{"recvU", "func (r U) M() { }", ""},
	{"ifInit", "", "if x := u; x > 0 { }"},
	{"forCond", "", "for u { }"},
	{"mismatchedOp", "", "x := \"a\" - 1; _ = x"},
	{"wrongArgCount", "", "s := strings.Repeat(\"a\"); _ = s"},
	{"shiftU", "", "x := 1 << u; _ = x"},
	{"compareMismatch", "", "if 1 == \"a\" { }"},
	{"genericU", "func G[T any](t T) T { return t }", "x := G[U](1); _ = x"},
	{"embedU", "type E struct{ U }", "var e E; _ = e"},
	{"varTypedU", "", "var x U; _ = x"},
	{"ptrU", "", "var x *U; _ = x"},
	{"chanOfU", "", "c := make(chan U); _ = c"},
	{"funcLitParamU", "", "f := func(x U) {}; _ = f"},
	{"methodValue", "", "f := u.M; _ = f"},
	{"interfaceU", "type I interface{ M() U }", "var i I; _ = i"},
	{"arrayLenU", "", "var a [u]int; _ = a"},
	{"keyedArrayU", "", "a := [...]int{u: 1}; _ = a"},
	{"returnMulti", "func r2() (int, error) { return u, nil }", ""},
	{"namedResultAssign", "func r3() (n int) { n = u; return }", ""},
	{"structLitU", "type H struct{ N int }", "h := H{N: u}; _ = h"},
	{"ptrStructLitU", "type H struct{ N int }", "h := &H{N: u}; _ = h"},
	{"callResultField", "", "x := u().F; _ = x"},
	{"stringIndexU", "", "s := \"abc\"; b := s[u]; _ = b"},
	{"sliceOfValidU", "", "s := []int{1}; x := s[u:]; _ = x"},
	{"mapIndexU", "", "m := map[string]int{}; x := m[u]; _ = x"},
	{"mapIndexOk", "", "m := map[string]int{}; x, ok := m[u]; _, _ = x, ok"},
	{"deleteU", "", "delete(u, 1)"},
	{"copyU", "", "copy(u, u)"},
	{"panicU", "", "panic(u)"},
	{"printfU", "", "fmt.Printf(\"%d\", u)"},
	{"errorsNewU", "", "e := fmt.Errorf(u); _ = e"},
	{"labelGoto", "", "goto L; L: _ = u"},
}

func TestUntypedRegionShapesConvertWithoutPanic(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads each fixture package via go/packages")
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	for _, shape := range untypedRegionShapes {
		t.Run(shape.name, func(t *testing.T) {
			root := t.TempDir()
			pkgDir := filepath.Join(root, "p")

			writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/p\n\ngo 1.24\n")
			writeModuleFile(t, filepath.Join(pkgDir, "p.go"), "package p\n\nimport (\n\t\"fmt\"\n\t\"strings\"\n)\n\nvar _ = fmt.Sprint\nvar _ = strings.Repeat\n\n"+
				shape.decls+"\n\nfunc F() {\n\t"+strings.ReplaceAll(shape.body, "; ", "\n\t")+"\n}\n\nfunc Healthy() int { return 42 }\n")

			options := Options{
				goRoot:              goRoot,
				goPath:              build.Default.GOPATH,
				go2csPath:           filepath.Join(root, "runtime"),
				targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
				indentSpaces:        4,
				preferVarDecl:       true,
				useChannelOperators: true,
				debugMode:           true,
			}

			build.Default.GOROOT = options.goRoot
			build.Default.GOPATH = options.goPath

			outDir := filepath.Join(root, "out")

			func() {
				defer func() {
					if r := recover(); r != nil {
						t.Fatalf("converting a function with an untyped region panicked: %v\n%s", r, debug.Stack())
					}
				}()

				if err := processConversion(pkgDir, true, outDir, options); err != nil {
					t.Fatalf("conversion failed: %v", err)
				}
			}()

			if converted := readGenerated(t, filepath.Join(outDir, "p.cs")); !strings.Contains(converted, "Healthy()") {
				t.Errorf("the healthy declaration beside the untyped region was lost:\n%s", converted)
			}
		})
	}
}

// TestUntypedRegionIsReportedNotFaulted is the reported case through the real command line: the type
// errors print as the summary and canonical GO2CS1003 lines, the package converts, and no recovered
// visitor fault ("visit file error") stands in for them.
func TestUntypedRegionIsReportedNotFaulted(t *testing.T) {
	dir := t.TempDir()

	writeCgoFixture(t, dir, "go.mod", "module tefixture\n\ngo 1.24\n")
	writeCgoFixture(t, dir, "te.go", "package te\n\nfunc F() int {\n\tvar s string = 1\n\treturn undefinedName + len(s)\n}\n")

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	outDir := t.TempDir()

	var stderr bytes.Buffer
	cmd := exec.Command(driverBinary(t), "-go2cspath", treeSrc, dir, outDir)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=0")
	cmd.Stdout, cmd.Stderr = &bytes.Buffer{}, &stderr

	if err := cmd.Run(); err != nil {
		t.Fatalf("conversion exited %v\nstderr:\n%s", err, stderr.String())
	}

	if strings.Contains(stderr.String(), "visit file error") {
		t.Errorf("the untyped region faulted the visitor\nstderr:\n%s", stderr.String())
	}

	requireStderrLine(t, stderr.String(), dir, "te.go", "(5,9): error GO2CS1003: undefined: undefinedName")

	if converted := readGenerated(t, filepath.Join(outDir, "te.cs")); !strings.Contains(converted, "F()") {
		t.Errorf("te.cs lost F:\n%s", converted)
	}
}
