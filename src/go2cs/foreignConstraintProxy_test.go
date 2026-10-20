// foreignConstraintProxy_test.go - Gbtc
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

// A self-referential constraint over a FOREIGN interface at a pointer to an element the interface's package does not
// declare: `lib.Ord2[*elem.Node]` from b1 and main, `lib.Ord2[*mnode]` from main. The proxy is the type argument, so it
// is part of the type's identity, and every importer must name one. constraintProxyFor named a concrete proxy in the
// interface's package (`lib.NodeжLesser`) that nothing could declare: CS0426 at every such site. It now names the
// interface package's GENERIC proxy (`lib.жLesser<elem.Node>`) and records a bind, unless the interface's package
// published a concrete proxy for the element itself (the `own` arm, std's fips140 ecdh/ecdsa shape), which still wins.
// A written instantiation renders the proxy too: a type assertion's AST type walk rendered the box.
//
// Controls: `own` instantiating its constraint over elem's Node publishes the concrete proxy and b1 names it, binding
// nothing; `twin` publishes two concrete proxies of one name (two elements called Node) and its consumer refuses to
// choose, leaving the box.

func TestForeignConstraintNamesTheInterfacePackagesProxy(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fcp\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

type Lesser[T any] interface{ Less(T) bool }

type Ord2[T Lesser[T]] struct{ V T }

func (o Ord2[T]) Below(t T) bool { return o.V.Less(t) }
`)
	writeModuleFile(t, filepath.Join(appDir, "elem", "elem.go"), `package elem

type Node struct{ K int }

func (n *Node) Less(o *Node) bool { return n.K < o.K }
`)
	writeModuleFile(t, filepath.Join(appDir, "elem2", "elem2.go"), `package elem2

type Node struct{ K int }

func (n *Node) Less(o *Node) bool { return n.K < o.K }
`)
	writeModuleFile(t, filepath.Join(appDir, "own", "own.go"), `package own

import "example.com/fcp/elem"

type Lesser2[T any] interface{ Less(T) bool }

type Ord3[T Lesser2[T]] struct{ V T }

func Make(k int) Ord3[*elem.Node] { return Ord3[*elem.Node]{V: &elem.Node{K: k}} }
`)
	writeModuleFile(t, filepath.Join(appDir, "twin", "twin.go"), `package twin

import (
	"example.com/fcp/elem"
	"example.com/fcp/elem2"
)

type Lesser3[T any] interface{ Less(T) bool }

type Ord4[T Lesser3[T]] struct{ V T }

var A = Ord4[*elem.Node]{V: &elem.Node{}}
var B = Ord4[*elem2.Node]{V: &elem2.Node{}}
`)
	writeModuleFile(t, filepath.Join(appDir, "b1", "b1.go"), `package b1

import (
	"example.com/fcp/elem"
	"example.com/fcp/lib"
	"example.com/fcp/own"
	"example.com/fcp/twin"
)

func Make(k int) lib.Ord2[*elem.Node] { return lib.Ord2[*elem.Node]{V: &elem.Node{K: k}} }

func Own(k int) own.Ord3[*elem.Node] { return own.Make(k) }

func Twin() twin.Ord4[*elem.Node] { return twin.A }
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/fcp/b1"
	"example.com/fcp/elem"
	"example.com/fcp/lib"
)

type mnode struct{ k int }

func (n *mnode) Less(o *mnode) bool { return n.k < o.k }

func main() {
	var a any = b1.Make(1)
	x, ok := a.(lib.Ord2[*elem.Node])
	fmt.Println(x.Below(&elem.Node{K: 2}), ok, lib.Ord2[*mnode]{V: &mnode{1}}.Below(&mnode{0}))
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

	dir := filepath.Join(options.go2csPath, "src", "example.com", "fcp")
	mainCS := readGenerated(t, filepath.Join(dir, "main.cs"))
	mainInfo := readGenerated(t, filepath.Join(dir, "package_info.cs"))
	b1CS := readGenerated(t, filepath.Join(dir, "b1", "b1.cs"))
	b1Info := readGenerated(t, filepath.Join(dir, "b1", "package_info.cs"))
	ownInfo := readGenerated(t, filepath.Join(dir, "own", "package_info.cs"))

	const nodeBind = "[assembly: GoImplement<go.example.com.fcp.elem_package.Node, go.example.com.fcp.lib_package.Lesser<go.example.com.fcp.elem_package.Node>>(ConstraintProxy = true)]"

	for _, arm := range []struct {
		name, text, want string
	}{
		{"b1 names the generic proxy", b1CS, "lib.Ord2<lib.жLesser<elem.Node>> Make("},
		{"b1 binds it", b1Info, nodeBind},
		{"main's type assertion names the generic proxy", mainCS, "._<lib.Ord2<lib.жLesser<elem.Node>>>("},
		{"main binds it too", mainInfo, nodeBind},
		{"main's own element takes the generic proxy", mainCS, "new lib.Ord2<lib.жLesser<mnode>>("},
		{"main binds its own element", mainInfo, "[assembly: GoImplement<mnode, go.example.com.fcp.lib_package.Lesser<mnode>>(ConstraintProxy = true)]"},
		{"control: own publishes its concrete proxy", ownInfo, "[assembly: GoImplement<go.example.com.fcp.elem_package.Node, Lesser2<go.example.com.fcp.elem_package.Node>>(ConstraintProxy = true)]"},
		{"control: b1 names own's concrete proxy", b1CS, "own.Ord3<own.NodeжLesser2> Own("},
		{"control: twin's two same-named proxies are refused, leaving the box", b1CS, "twin.Ord4<ж<elem.Node>> Twin("},
	} {
		if !strings.Contains(arm.text, arm.want) {
			t.Errorf("%s: missing %s", arm.name, arm.want)
		}
	}

	for _, absent := range []string{"Lesser2<", "Lesser3<"} {
		if strings.Contains(b1Info, absent) {
			t.Errorf("b1 must bind no proxy of a package that published a concrete one: found %s", absent)
		}
	}

	if strings.Contains(b1CS, "NodeжLesser>") || strings.Contains(mainCS, "NodeжLesser>") {
		t.Errorf("no consumer may name a concrete proxy the interface's package never published")
	}
}
