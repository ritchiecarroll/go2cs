// valueConstraintRecordNeed_test.go - Gbtc
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
	"runtime"
	"strings"
	"testing"
)

// recordValueConstraintImplementation records `GoImplement<arg, constraint>` at a VALUE instantiation of
// a method-set constraint, because C# checks `where T : constraint` nominally against the argument. A
// generic FUNCTION's clause is emitted only where its body needs it (methodSetConstraintNeeded): Go 1.26's
// errors.AsType[E error] drops it, so `AsType<alertError>` constrains nothing and a record for it is an
// implementation no C# check asks for -- four such lines per target across the -stdlib emission
// (crypto/tls x3, runtime/pprof or internal/testenv), measured before this guard.
//
// Arms: a value argument at a dropped clause records NOTHING; two controls keep their record -- a
// generic TYPE (its clause is always emitted) and a function whose body uses E as an error. No argument
// is converted to `error` anywhere, so only the instantiation path can mint a record.

func TestValueConstraintRecordFollowsTheEmittedClause(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/vcneed\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type dropped struct{ s string }

func (e dropped) Error() string { return e.s }

type slotted struct{ s string }

func (e slotted) Error() string { return e.s }

type described struct{ s string }

func (e described) Error() string { return e.s }

// The errors.AsType shape: E is asserted to and returned, never used as an error -- no clause.
func AsType[E error](err error) (E, bool) {
	e, ok := err.(E)
	return e, ok
}

// CONTROL: a generic TYPE always emits its clause.
type slot[E error] struct{ e E }

// CONTROL: a method called on the parameter keeps the clause.
func Describe[E error](e E) string { return e.Error() }

func main() {
	var err error = fmt.Errorf("plain")
	_, ok := AsType[dropped](err)
	var s slot[slotted]
	describe := Describe[described]
	fmt.Println(ok, s.e.s == "", describe != nil)
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

	info := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "vcneed", "package_info.cs"))

	for _, arm := range []struct {
		record string
		want   bool
	}{
		{"GoImplement<dropped, error>", false},
		{"GoImplement<slotted, error>", true},
		{"GoImplement<described, error>", true},
	} {
		if got := strings.Contains(info, arm.record); got != arm.want {
			t.Errorf("%s present=%v, want %v", arm.record, got, arm.want)
		}
	}

	if t.Failed() {
		t.Logf("package_info.cs:\n%s", info)
	}
}
