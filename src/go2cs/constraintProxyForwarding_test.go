// constraintProxyForwarding_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"sort"
	"strings"
	"testing"
)

// A constraint proxy wraps the element's box in a separate C# object, so a self-referential constraint
// method whose `[]N`, `map[K]N`, `chan N`, `*N` or `List[N]` shares the element's storage cannot be
// forwarded without breaking Go's aliasing. These tests lock in that exactly the instantiations naming
// such a proxy are refused, by name and site, and that the forwardable shapes (a whole N, a tuple, a
// func re-wrapped at any depth, an array copied element-wise, a self-constrained type argument) and every
// value-element instantiation are not.
const constraintProxyForwardingFixture = `package fwd

type List[T any] struct{ items []T }

// Seq is a NAMED func type: it forwards as a func does.
type Seq[V any] func(yield func(V) bool)

// Tree's methods share the element's storage: a pointer element is refused.
type Tree[N any] interface {
	Children() []N
	Index() map[string]N
	Feed(chan N)
	Parent() *N
	Pairs() List[N]
	Visit(func([]N))
	Label() string
}

type GraphNode[N any] interface{ Label() string }

type Graph[N GraphNode[N]] struct{ nodes []N }

// Flow's methods all forward exactly.
type Flow[N GraphNode[N]] interface {
	Self() N
	Pair() (N, error)
	Next() func() N
	Each(func(func(N) bool))
	Triple() [3]N
	Owner() *Graph[N]
	All() Seq[N]
	Label() string
}

type Node struct{ kids []*Node }

func (n *Node) Children() []*Node           { return n.kids }
func (n *Node) Index() map[string]*Node     { return nil }
func (n *Node) Feed(chan *Node)             {}
func (n *Node) Parent() **Node              { return nil }
func (n *Node) Pairs() List[*Node]          { return List[*Node]{} }
func (n *Node) Visit(func([]*Node))         {}
func (n *Node) Label() string               { return "node" }
func (n *Node) Self() *Node                 { return n }
func (n *Node) Pair() (*Node, error)        { return n, nil }
func (n *Node) Next() func() *Node          { return nil }
func (n *Node) Each(func(func(*Node) bool)) {}
func (n *Node) Triple() [3]*Node            { return [3]*Node{} }
func (n *Node) Owner() *Graph[*Node]        { return nil }
func (n *Node) Kids() []*Node               { return n.kids }
func (n *Node) All() Seq[*Node]             { return nil }

type VNode struct{ kids []VNode }

func (n VNode) Children() []VNode       { return n.kids }
func (n VNode) Index() map[string]VNode { return nil }
func (n VNode) Feed(chan VNode)         {}
func (n VNode) Parent() *VNode          { return nil }
func (n VNode) Pairs() List[VNode]      { return List[VNode]{} }
func (n VNode) Visit(func([]VNode))     {}
func (n VNode) Label() string           { return "v" }

func Walk[N Tree[N]](n N) int { return len(n.Children()) }

func Run[N Flow[N]](n N) string { return n.Label() }

func Kids[N interface{ Kids() []N }](n N) int { return len(n.Kids()) }

type Forest[N Tree[N]] struct{ roots []N }

func walkPointer() int { return Walk(&Node{}) }

func walkValue() int { return Walk(VNode{}) }

func runPointer() string { return Run(&Node{}) }

func kidsPointer() int { return Kids(&Node{}) }

var forest Forest[*Node]

var valueForest Forest[VNode]
`

func loadForwardingRefusal(t *testing.T) *constraintProxyRefusal {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/fwd\n\ngo 1.23\n",
		"fwd.go": constraintProxyForwardingFixture,
	})

	return refuseUnforwardableConstraintProxies(loadProductionForDir(t, dir))
}

// fixtureLine is the 1-based line of the fixture holding `marker`.
func fixtureLine(t *testing.T, marker string) int {
	t.Helper()

	for i, line := range strings.Split(constraintProxyForwardingFixture, "\n") {
		if strings.Contains(line, marker) {
			return i + 1
		}
	}

	t.Fatalf("fixture has no line with %q", marker)
	return 0
}

func TestAPointerElementWhoseConstraintSharesStorageIsRefusedBySite(t *testing.T) {
	refusal := loadForwardingRefusal(t)

	if refusal == nil {
		t.Fatal("no instantiation was refused; Walk, Kids and Forest over *Node name proxies that cannot forward []N")
	}

	var got []int

	for _, site := range refusal.sites {
		got = append(got, site.position.Line)
	}

	want := []int{
		fixtureLine(t, "return Walk(&Node{})"),
		fixtureLine(t, "return Kids(&Node{})"),
		fixtureLine(t, "var forest Forest[*Node]"),
	}

	sort.Ints(want)

	if fmt.Sprint(got) != fmt.Sprint(want) {
		t.Fatalf("refused lines %v, want %v (Walk, Kids and Forest over *Node; never a value element or Run)\n%s", got, want, refusal.Error())
	}
}

func TestTheRefusalNamesEveryMethodThatCannotForward(t *testing.T) {
	refusal := loadForwardingRefusal(t)

	if refusal == nil {
		t.Fatal("no instantiation was refused")
	}

	walk := refusal.sites[0].text

	for _, phrase := range []string{
		"Walk[*Node] needs a C# constraint proxy for Tree",
		"Tree.Children (returns []N)",
		"Tree.Index (returns map[string]N)",
		"Tree.Feed (takes chan N)",
		"Tree.Parent (returns *N)",
		"Tree.Pairs (returns List[N])",
		"Tree.Visit (takes func([]N))",
	} {
		if !strings.Contains(walk, phrase) {
			t.Errorf("the Walk refusal lacks %q:\n%s", phrase, walk)
		}
	}

	if strings.Contains(walk, "Label") {
		t.Errorf("the Walk refusal names Label, which forwards:\n%s", walk)
	}

	if kids := refusal.sites[1].text; !strings.Contains(kids, "Kids[*Node] needs a C# constraint proxy for interface{…}") || !strings.Contains(kids, "Kids (returns []N)") {
		t.Errorf("the inline-constraint refusal does not name its interface and method:\n%s", kids)
	}
}

func TestEveryExactShapeForwards(t *testing.T) {
	refusal := loadForwardingRefusal(t)

	if refusal == nil {
		return
	}

	for _, site := range refusal.sites {
		if strings.Contains(site.text, "Run[") {
			t.Errorf("Run over *Node was refused, but every Flow method forwards exactly:\n%s", site.text)
		}
	}
}

