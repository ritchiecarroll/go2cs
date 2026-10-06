// pointerReceiverMark_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the receiver rule of docs/PLAN-marker-comment-parity.md, 5.1 (face lift A), read from the
// converter's own rendering: a Go POINTER receiver is emitted `this ref T` with NO mark, and a VALUE
// receiver `this T`, never by reference. golib's method-set readers, RecvGenerator and TypeGenerator
// all read an unmarked by-ref receiver as a pointer-set method, so the receiver's ref kind alone
// carries what [GoRecv] used to say — which holds only while the converter never emits a by-ref
// receiver for a value method. Struct, named-slice and generic receivers are covered, beside the
// dual-embed type whose interface-promoted forwarder the converter writes itself.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestPointerReceiversAreEmittedByRefWithNoMark(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/prm\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type counter struct{ n int }

func (c counter) Get() int   { return c.n }
func (c *counter) Set(v int) { c.n = v }

type list []int

func (l list) Len() int       { return len(l) }
func (l *list) Push(v int)    { *l = append(*l, v) }

type box[T any] struct{ v T }

func (b box[T]) Peek() T     { return b.v }
func (b *box[T]) Put(v T)    { b.v = v }

type ReadWriter interface {
	Read() string
	Write(s string) string
}

type base struct{ tag string }

func (b *base) Write(s string) string { return "b:" + s + b.tag }

type dual struct {
	ReadWriter
	*base
}

func (d *dual) Write(s string) string { return "d:" + s }

type plain struct{}

func (plain) Read() string          { return "read" }
func (plain) Write(s string) string { return "p:" + s }

func main() {
	c := counter{}
	c.Set(3)
	l := list{}
	l.Push(4)
	b := box[string]{}
	b.Put("x")
	d := &dual{ReadWriter: plain{}, base: &base{tag: "t"}}
	var rw ReadWriter = d
	fmt.Println(c.Get(), l.Len(), b.Peek(), rw.Read(), rw.Write("w"))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "prm", "main.cs"))

	// No mark anywhere in converted output: the attribute stays a hand-written spelling only.
	for index, line := range strings.Split(mainCs, "\n") {
		if code, _, _ := strings.Cut(line, "//"); strings.Contains(code, "GoRecv") {
			t.Errorf("main.cs:%d still carries [GoRecv]: %s", index+1, strings.TrimSpace(line))
		}
	}

	// Pointer receivers: `this ref T`, with nothing between the access modifier and the declaration.
	for _, want := range []string{
		"internal static void Set(this ref counter c, nint v)",
		"internal static void Push(this ref list l, nint v)",
		"internal static void Put<T>(this ref box<T> b, T v)",
		"internal static @string Write(this ref @base b, @string s)",
		"internal static @string Write(this ref dual d, @string s)",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("pointer receiver: want %q in:\n%s", want, mainCs)
		}
	}

	// Value receivers: `this T`, by value.
	for _, want := range []string{
		"internal static nint Get(this counter c)",
		"internal static nint Len(this list l)",
		"internal static T Peek<T>(this box<T> b)",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("value receiver: want %q in:\n%s", want, mainCs)
		}
	}

	// Every by-ref receiver in the file is one of the five pointer receivers: a by-ref VALUE receiver
	// would be read as pointer-set by every reader, and nothing would say otherwise.
	byRef := regexp.MustCompile(`\b(\w+)(?:<\w+>)?\(this ref `).FindAllStringSubmatch(mainCs, -1)
	var names []string

	for _, match := range byRef {
		names = append(names, match[1])
	}

	if strings.Join(names, ",") != "Set,Push,Put,Write,Write" {
		t.Errorf("by-ref receivers: got %v, want the five pointer receivers [Set Push Put Write Write]:\n%s", names, mainCs)
	}
}
