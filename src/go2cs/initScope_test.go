// initScope_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the init-statement scope shapes of docs/PLAN-init-statement-scope.md (initScopeOperations.go):
// each shape the predicate admits, and each refusal class, as emitted for one Go source.

package main

import (
	"strings"
	"testing"
)

const initScopeSource = `package main

import (
	"errors"
	"fmt"
)

type A interface{ M() int }
type B interface{ N() int }
type T struct{}

func (T) M() int { return 1 }
func (T) N() int { return 2 }

const limit = 5

func f() (int, error) { return 1, nil }
func h() int          { return 3 }

func dropIf() int {
	if n := h(); n > 2 {
		return n
	}
	return 0
}

func dropIfElse() int {
	if n := h(); n > 5 {
		return 1
	} else {
		return n
	}
}

func dropChainHead(x any) int {
	if n := h(); n > 9 {
		return 1
	} else if n > 8 {
		return 2
	}
	return 0
}

func dropSwitch() int {
	switch n := h(); n {
	case 1:
		return 1
	}
	return 0
}

func dropTypeSwitch(x any) int {
	switch y := x; v := y.(type) {
	case A:
		return v.M()
	}
	return 0
}

func dropAssign() int {
	n := 0
	if n = h(); n > 2 {
		return n
	}
	return 0
}

func foldAssert(x any) int {
	if a, ok := x.(A); ok {
		return a.M()
	}
	if b, ok := x.(B); ok {
		return b.N()
	}
	return 0
}

func foldAssertNot(x any) int {
	if _, ok := x.(A); !ok {
		return 1
	}
	if _, ok := x.(B); !ok {
		return 2
	}
	return 0
}

func foldMap(m map[string]int) int {
	if v, ok := m["a"]; ok {
		return v
	}
	if w, ok := m["b"]; ok {
		return w
	}
	return 0
}

func foldMapNot(m map[string]int) int {
	if _, ok := m["a"]; !ok {
		return 1
	}
	if _, ok := m["b"]; !ok {
		return 2
	}
	return 0
}

func elseIfAssert(x any) int {
	if x == nil {
		return 0
	} else if m, ok := x.(A); ok {
		return m.M()
	}
	return 1
}

func elseIfMap(m map[string]int) int {
	if m == nil {
		return 0
	} else if v, ok := m["a"]; ok {
		return v
	}
	return 1
}

func elseIfMapNot(m map[string]int) int {
	if m == nil {
		return 0
	} else if v, ok := m["a"]; !ok {
		return 1
	} else {
		return v
	}
}

func elseIfVar() int {
	if h() > 9 {
		return 0
	} else if v, err := f(); err != nil || v > 1 {
		return v
	}
	return 1
}

func elseIfVarSingle() error {
	if h() > 9 {
		return nil
	} else if err := errors.New("x"); err != nil {
		return err
	}
	return nil
}

func elseIfTyped() int {
	if h() > 9 {
		return 0
	} else if k := h(); k > 1 {
		return k
	}
	return 1
}

func elseIfAssignInit() int {
	k := 0
	if h() > 9 {
		return 0
	} else if k = h(); k > 1 {
		return k
	}
	return 1
}

func refuseSiblings() error {
	if err := errors.New("a"); err != nil {
		return err
	}
	if err := errors.New("b"); err != nil {
		return err
	}
	return nil
}

func refuseClosureSibling() []func() int {
	var fns []func() int
	if x := h(); x > 0 {
		fns = append(fns, func() int { return x })
	}
	if x := h(); x > 1 {
		return fns
	}
	return nil
}

func refuseRebinding() int {
	if limit := h(); limit > 1 {
		fmt.Println(limit)
	}
	return limit
}

func refuseUseBefore() int {
	fmt.Println(limit)
	if limit := h(); limit > 1 {
		return limit
	}
	return 0
}

func refuseClosure() []func() int {
	var fns []func() int
	if x := h(); x > 0 {
		fns = append(fns, func() int { return x })
	}
	return fns
}

func refuseDefer() (r int) {
	if x := h(); x > 0 {
		defer fmt.Println(x)
	}
	return 1
}

func refuseMethodValue() func() int {
	if t := (T{}); t.M() > 0 {
		return t.M
	}
	return nil
}

func refuseFoldValueCollides(x any) int {
	if a, ok := x.(A); ok {
		return a.M()
	}
	if a, ok := x.(B); ok {
		return a.N()
	}
	return 0
}

func dropAfterSiblingFolds(m map[string]int) int {
	if v, ok := m["a"]; ok {
		return 1
	} else if v > 0 {
		return v
	}
	if w, ok := m["b"]; ok {
		return w
	}
	return 0
}

func main() {
	fmt.Println(dropIf(), dropIfElse(), dropChainHead(nil), dropSwitch(), dropTypeSwitch(T{}), dropAssign())
	fmt.Println(foldAssert(T{}), foldAssertNot(T{}), foldMap(nil), foldMapNot(nil))
	fmt.Println(elseIfAssert(T{}), elseIfMap(nil), elseIfMapNot(nil), elseIfVar(), elseIfVarSingle(), elseIfTyped(), elseIfAssignInit())
	fmt.Println(refuseSiblings(), len(refuseClosureSibling()), refuseRebinding(), refuseUseBefore(), len(refuseClosure()), refuseDefer())
	fmt.Println(refuseMethodValue() != nil, refuseFoldValueCollides(T{}), dropAfterSiblingFolds(nil))
}
`

// initScopeBodies converts initScopeSource and returns each emitted function, keyed by name, from its
// signature line to its closing brace.
func initScopeBodies(t *testing.T) map[string]string {
	t.Helper()
	isolateConverterPathGlobals(t)

	lines := convertWithoutComments(t, initScopeSource)
	bodies := map[string]string{}

	for i := 0; i < len(lines); i++ {
		line := lines[i]

		if !strings.HasPrefix(line, "internal static ") {
			continue
		}

		open := strings.Index(line, "(")

		if open < 0 || !strings.HasSuffix(line, "{") {
			continue
		}

		name := line[strings.LastIndex(line[:open], " ")+1 : open]
		end := i

		// A frame-form function closes on `} catch … }`, the try block's brace (goFrameTail).
		for end < len(lines) && !strings.HasPrefix(lines[end], "}") {
			end++
		}

		bodies[name] = strings.Join(lines[i:end+1], "\n")
		i = end
	}

	return bodies
}

func assertInitScopeBodies(t *testing.T, want map[string]string) {
	t.Helper()

	got := initScopeBodies(t)

	for name, body := range want {
		if got[name] != body {
			t.Errorf("%s emitted\n%s\nwant\n%s", name, got[name], body)
		}
	}
}

// An init whose names occur nowhere else in the enclosing block becomes a statement of that block, as Go
// writes it: an if, an if/else, an if heading an else-if chain, a switch, a type switch, an `=` init (PLAN (a)).
func TestInitScopeDropsTheBlockWhenNoNameCollides(t *testing.T) {
	assertInitScopeBodies(t, map[string]string{
		"dropIf": `internal static nint dropIf() {
    nint n = h(); if (n > 2) {
        return n;
    }
    return 0;
}`,
		"dropIfElse": `internal static nint dropIfElse() {
    nint n = h(); if (n > 5) {
        return 1;
    } else {
        return n;
    }
}`,
		"dropChainHead": `internal static nint dropChainHead(any x) {
    nint n = h(); if (n > 9) {
        return 1;
    } else if (n > 8) {
        return 2;
    }
    return 0;
}`,
		"dropSwitch": `internal static nint dropSwitch() {
    nint n = h();
    switch (n) {
    case 1: {
        return 1;
    }}

    return 0;
}`,
		"dropTypeSwitch": `internal static nint dropTypeSwitch(any x) {
    var y = x;
    switch (y.type()) {
    case {} Δv when Δv._<A>(out var v): {
        return v.M();
    }}
    return 0;
}`,
		"dropAssign": `internal static nint dropAssign() {
    nint n = 0;
    n = h(); if (n > 2) {
        return n;
    }
    return 0;
}`,
	})
}

// A head if refused only because a sibling declares the same `ok` folds its init into the condition, which
// declares no `ok`: golib's out-parameter assertion (c) or a pattern on the comma-ok tuple (b1, b2). Siblings that
// refuse each other only by `ok` fold together, and a fold frees a sibling refused only by the `ok` it gave up.
func TestInitScopeFoldsAHeadIfRefusedOnlyForOk(t *testing.T) {
	assertInitScopeBodies(t, map[string]string{
		"foldAssert": `internal static nint foldAssert(any x) {
    if (x._<A>(out var a)) {
        return a.M();
    }
    if (x._<B>(out var b)) {
        return b.N();
    }
    return 0;
}`,
		"foldAssertNot": `internal static nint foldAssertNot(any x) {
    if (!x._<A>(out _)) {
        return 1;
    }
    if (!x._<B>(out _)) {
        return 2;
    }
    return 0;
}`,
		"foldMap": `internal static nint foldMap(map<@string, nint> m) {
    if (m["a", ꟷ] is (var v, true)) {
        return v;
    }
    if (m["b", ꟷ] is (var w, true)) {
        return w;
    }
    return 0;
}`,
		"foldMapNot": `internal static nint foldMapNot(map<@string, nint> m) {
    if (m["a", ꟷ] is not (_, true)) {
        return 1;
    }
    if (m["b", ꟷ] is not (_, true)) {
        return 2;
    }
    return 0;
}`,
		"dropAfterSiblingFolds": `internal static nint dropAfterSiblingFolds(map<@string, nint> m) {
    var (v, ok) = m["a", ꟷ]; if (ok) {
        return 1;
    } else if (v > 0) {
        return v;
    }
    if (m["b", ꟷ] is (var w, true)) {
        return w;
    }
    return 0;
}`,
	})
}

// An else-if's own init folds into its condition: (c) for an assertion, b1/b2 for a comma-ok whose value the
// pattern keeps assigned, else b3 for any `var` declaration. A typed declaration or an `=` init keeps today's
// else block.
func TestInitScopeFoldsAnElseIfsOwnInit(t *testing.T) {
	assertInitScopeBodies(t, map[string]string{
		"elseIfAssert": `internal static nint elseIfAssert(any x) {
    if (x == nil) {
        return 0;
    } else if (x._<A>(out var m)) {
        return m.M();
    }
    return 1;
}`,
		"elseIfMap": `internal static nint elseIfMap(map<@string, nint> m) {
    if (m == nil) {
        return 0;
    } else if (m["a", ꟷ] is (var v, true)) {
        return v;
    }
    return 1;
}`,
		"elseIfMapNot": `internal static nint elseIfMapNot(map<@string, nint> m) {
    if (m == nil) {
        return 0;
    } else if (m["a", ꟷ] is not (var v, true)) {
        return 1;
    } else {
        return v;
    }
}`,
		"elseIfVar": `internal static nint elseIfVar() {
    if (h() > 9) {
        return 0;
    } else if (f() is var (v, err) && (err != nil || v > 1)) {
        return v;
    }
    return 1;
}`,
		"elseIfVarSingle": `internal static error elseIfVarSingle() {
    if (h() > 9) {
        return default;
    } else if (errors.New("x") is var err && err != nil) {
        return err;
    }
    return default;
}`,
		"elseIfTyped": `internal static nint elseIfTyped() {
    if (h() > 9) {
        return 0;
    } else {
        nint k = h(); if (k > 1) {
            return k;
        }
    }
    return 1;
}`,
		"elseIfAssignInit": `internal static nint elseIfAssignInit() {
    nint k = 0;
    if (h() > 9) {
        return 0;
    } else {
        k = h(); if (k > 1) {
            return k;
        }
    }
    return 1;
}`,
	})
}

// Every refusal class keeps today's block: a sibling declaring the same name (R4), a sibling that keeps its own
// block (R5), a later use that binds to a package member (R6), an earlier one (R7), and any closure in the
// statement (R8): a func literal, a deferred call, a method value. A fold whose value also collides is refused too.
func TestInitScopeKeepsTheBlockForEachRefusal(t *testing.T) {
	assertInitScopeBodies(t, map[string]string{
		"refuseSiblings": `internal static error refuseSiblings() {
    {
        var err = errors.New("a"); if (err != nil) {
            return err;
        }
    }
    {
        var err = errors.New("b"); if (err != nil) {
            return err;
        }
    }
    return default;
}`,
		"refuseClosureSibling": `internal static slice<Func<nint>> refuseClosureSibling() {
    slice<Func<nint>> fns = default;
    {
        nint x = h(); if (x > 0) {
            fns = append(fns, () => x);
        }
    }
    {
        nint x = h(); if (x > 1) {
            return fns;
        }
    }
    return default;
}`,
		"refuseRebinding": `internal static nint refuseRebinding() {
    {
        nint limit = h(); if (limit > 1) {
            fmt.Println(limit);
        }
    }
    return main_package.limit;
}`,
		"refuseUseBefore": `internal static nint refuseUseBefore() {
    fmt.Println((nint)(main_package.limit));
    {
        nint limit = h(); if (limit > 1) {
            return limit;
        }
    }
    return 0;
}`,
		"refuseClosure": `internal static slice<Func<nint>> refuseClosure() {
    slice<Func<nint>> fns = default;
    {
        nint x = h(); if (x > 0) {
            fns = append(fns, () => x);
        }
    }
    return fns;
}`,
		"refuseDefer": `internal static nint /*r*/ refuseDefer() { nint r = default; GoFrame ᒐ = default; try {
    {
        nint x = h(); if (x > 0) {
            defer(ᴛ1 => fmt.Println(ᴛ1), x, ref ᒐ);
        }
    }
    r = 1;
} catch (Exception ᒐe) when (ᒐ.Is(ᒐe)) { ᒐ.Capture(); } finally { ᒐ.Run(); } return r; }`,
		"refuseMethodValue": `internal static Func<nint> refuseMethodValue() {
    {
        var t = (new T()); if (t.M() > 0) {
            var tʗ1 = t;
            return () => tʗ1.M();
        }
    }
    return default;
}`,
		"refuseFoldValueCollides": `internal static nint refuseFoldValueCollides(any x) {
    {
        var (a, ok) = x._<A>(ᐧ); if (ok) {
            return a.M();
        }
    }
    {
        var (a, ok) = x._<B>(ᐧ); if (ok) {
            return a.N();
        }
    }
    return 0;
}`,
	})
}

// initFoldParts takes one plain `var` declaration apart and refuses anything else, so a fold only ever
// rewrites text it can read whole.
func TestInitFoldPartsReadsOnlyAPlainVarDeclaration(t *testing.T) {
	for _, c := range []struct {
		text  string
		names string
		rhs   string
	}{
		{"\r\n    var (sw, ok) = w._<StringWriter>(\u1427);", "sw,ok", "w._<StringWriter>(\u1427)"},
		{"\r\n    \x0042\x00var err = errors.New(\"a; b\");", "err", "errors.New(\"a; b\")"},
		{"\r\n    nint n = h();", "", ""},
		{"\r\n    ref var x = ref heap(out var \u13D1x);", "", ""},
		{"\r\n    var x = a; var y = b;", "", ""},
		{"\r\n    var x = f(\r\n        a);", "", ""},
	} {
		names, rhs, ok := initFoldParts(c.text)

		if got := strings.Join(names, ","); got != c.names || rhs != c.rhs || ok != (c.names != "") {
			t.Errorf("initFoldParts(%q) = %q, %q, %v; want %q, %q", c.text, got, rhs, ok, c.names, c.rhs)
		}
	}
}

// The left operand of `is` is parenthesized unless it is one primary expression, since `is` binds tighter
// than equality and the logical operators: `a == b is var x` would test `b`.
func TestInitFoldOperandParenthesizesAllButAPrimary(t *testing.T) {
	for rhs, want := range map[string]string{
		"f(a, b)":             "f(a, b)",
		"m[\"a b\", k]":       "m[\"a b\", k]",
		"(nint)x":             "(nint)x",
		"a == b":              "(a == b)",
		"x ?? y":              "(x ?? y)",
		"g(\"(\") + h(\")\")": "(g(\"(\") + h(\")\"))",
	} {
		if got := initFoldOperand(rhs); got != want {
			t.Errorf("initFoldOperand(%q) = %q; want %q", rhs, got, want)
		}
	}
}
