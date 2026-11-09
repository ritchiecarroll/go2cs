// AnonIfaceParamCrossPackage guards a concrete value passed to ANOTHER package's anonymous-interface parameter
// (TypeScript's debug.FailBadSyntaxKind, catalogue row C1 of EXPERIMENT-ts-readiness-2026-11-07.md). The callee's
// package lifts the interface; the call site here must name that lift, not the raw Go type text.
package main

import (
	"fmt"

	dbg "go/anonifacedbg"
)

type node struct{ k string }

func (n *node) KindString() string { return n.k }

type leaf string

func (l leaf) KindString() string { return "leaf:" + string(l) }
func (l leaf) Name() string       { return string(l) }
func (l leaf) Kind() dbg.Kind     { return 7 }

type kinder interface{ KindString() string }

// local spells the same anonymous interface here, so this package lifts one of its own.
func local(n interface{ KindString() string }) string { return "local:" + n.KindString() }

func main() {
	fmt.Println(local(&node{k: "l"}))

	fmt.Println(dbg.Fail(&node{k: "x"}))
	fmt.Println(dbg.Fail(&node{k: "y"}, " ", 42))
	fmt.Println(dbg.Fail(leaf("v")))

	var k kinder = &node{k: "i"}
	fmt.Println(dbg.Fail(k))

	fmt.Println(dbg.Describe(leaf("d")))
}
