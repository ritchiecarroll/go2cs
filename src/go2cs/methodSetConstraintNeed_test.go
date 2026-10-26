// methodSetConstraintNeed_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"testing"
)

// Go 1.26's errors.AsType[E error](err error) (E, bool) is instantiated at POINTER-receiver error types --
// net's AsType[*DNSError], debug/buildinfo's AsType[*fs.PathError] -- and a pointer type argument is
// golib's box ж<T>, which no C# interface can be implemented by: `where E : error` was CS0311 at both
// sites, and net alone blocked every remaining project of H7 pass 6. Yet nothing in AsType's body uses
// E AS an error: it type-asserts to E, returns E, and allocates a *E. So the C# clause enforces nothing
// Go needs and only refuses the instantiation. A method-set constraint is now emitted only where the
// body uses the type parameter as that interface (methodSetConstraintNeeded); elsewhere it is a
// breadcrumb comment. Go's checker already validated every instantiation.
//
// Arms: the AsType shape (and its recursive helper) drops the clause; three controls keep it -- a
// method called on the parameter, a parameter value passed to an `error` slot, and the parameter
// instantiating a generic that needs it.

func TestMethodSetConstraintIsEmittedOnlyWhereTheBodyNeedsIt(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/msneed\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type MyErr struct{ code int }

func (e *MyErr) Error() string { return fmt.Sprint("code ", e.code) }

// The errors.AsType shape: E is asserted to, returned and allocated -- never used as an error.
func AsType[E error](err error) (E, bool) {
	if err == nil {
		var zero E
		return zero, false
	}
	var pe *E
	return asType(err, &pe)
}

func asType[E error](err error, ppe **E) (_ E, _ bool) {
	if e, ok := err.(E); ok {
		return e, true
	}
	if *ppe == nil {
		*ppe = new(E)
	}
	if u, ok := err.(interface{ Unwrap() error }); ok {
		return asType(u.Unwrap(), ppe)
	}
	return
}

// CONTROL: a method called on the parameter.
func Describe[E error](e E) string { return e.Error() }

// CONTROL: a parameter value converted to the interface.
func Wrap[E error](e E) error { return e }

// CONTROL: the parameter instantiates a generic that needs it.
func DescribeTwice[E error](e E) string { return Describe(e) + Describe(e) }

// CONTROL: the SIGNATURE names a generic type constrained on the parameter (ecdsa's *Curve[P]); the body
// never touches E, but C# checks Box<E>'s clause wherever Box<E> is named.
type Box[E error] struct{ e E }

func Peek[E error](b *Box[E]) bool { return b != nil }

func main() {
	var err error = &MyErr{7}
	m, ok := AsType[*MyErr](err)
	fmt.Println(m.code, ok, Describe(m), Wrap(m) != nil, DescribeTwice(m))
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	cs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "msneed", "main.cs"))

	clause := func(function string) string {
		match := regexp.MustCompile(`(?s)` + function + `<E>\([^)]*\)\s*(where E : [^\r\n{]*)?\s*\{`).FindStringSubmatch(cs)

		if match == nil {
			t.Fatalf("the fixture no longer emits %s<E>:\n%s", function, cs)
		}

		return match[1]
	}

	for _, function := range []string{"AsType", "asType"} {
		if got := clause(function); got != "" {
			t.Errorf("%s<E> keeps a C# constraint its body never uses (a pointer type argument cannot satisfy it): %q", function, got)
		}
	}

	for _, function := range []string{"Describe", "Wrap", "DescribeTwice", "Peek"} {
		if got := clause(function); got != "where E : error" {
			t.Errorf("CONTROL: %s<E> uses E as an error and must keep `where E : error`, got %q", function, got)
		}
	}

	// The instantiation itself is the box, as at net's call site.
	if !regexp.MustCompile(`AsType<ж<MyErr>>\(err\)`).MatchString(cs) {
		t.Errorf("the call site no longer instantiates AsType at the box ж<MyErr>:\n%s", cs)
	}
}

// A callee in ANOTHER FILE or PACKAGE is asked over its own syntax and types.Info (genericFuncSource):
// errors_test's testAsType[E compError] calling errors.AsType[E] kept `where E : compError`, and every
// ж<fs.PathError> type argument was CS0311, while that callee counted as unseen. CONTROL: a cross-package
// callee that uses E as an error (lib.Describe) still makes its caller keep the clause.
func TestMethodSetConstraintFollowsACalleeInAnotherFileOrPackage(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/msxpkg\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

func AsType[E error](err error) (E, bool) {
	var zero E
	if e, ok := err.(E); ok {
		return e, true
	}
	return zero, false
}

func Describe[E error](e E) string { return e.Error() }
`)
	writeModuleFile(t, filepath.Join(appDir, "other.go"), `package main

func otherFileAs[E error](err error) (E, bool) {
	var zero E
	if e, ok := err.(E); ok {
		return e, true
	}
	return zero, false
}
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/msxpkg/lib"
)

type MyErr struct{ code int }

func (e *MyErr) Error() string { return fmt.Sprint("code ", e.code) }

func viaPackage[E error](err error) (E, bool) { return lib.AsType[E](err) }

func viaOtherFile[E error](err error) (E, bool) { return otherFileAs[E](err) }

func describeVia[E error](e E) string { return lib.Describe(e) }

func main() {
	var err error = &MyErr{7}
	m, ok := viaPackage[*MyErr](err)
	n, _ := viaOtherFile[*MyErr](err)
	fmt.Println(m.code, ok, n.code, describeVia(m))
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	cs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "msxpkg", "main.cs"))

	clause := func(function string) string {
		match := regexp.MustCompile(`(?s)` + function + `<E>\([^)]*\)\s*(where E : [^\r\n{]*)?\s*\{`).FindStringSubmatch(cs)

		if match == nil {
			t.Fatalf("the fixture no longer emits %s<E>:\n%s", function, cs)
		}

		return match[1]
	}

	for _, function := range []string{"viaPackage", "viaOtherFile"} {
		if got := clause(function); got != "" {
			t.Errorf("%s<E> keeps a C# constraint its callee never needs (a pointer type argument cannot satisfy it): %q", function, got)
		}
	}

	if got := clause("describeVia"); got != "where E : error" {
		t.Errorf("CONTROL: describeVia<E> instantiates a cross-package callee that uses E as an error and must keep `where E : error`, got %q", got)
	}
}
