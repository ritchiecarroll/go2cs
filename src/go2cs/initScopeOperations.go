// initScopeOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// Go scopes an if, switch or type-switch init statement to its statement. C# has no init clause, so
// the converter opens a block for one: `{ init; if (cond) {…} }`. docs/PLAN-init-statement-scope.md
// (ruled, section 5) takes the block away wherever that cannot change what a name means:
//
//   - (a) the init moves into the enclosing block, `init; if (cond) {…}`, Go's own token order, when
//     the predicate in analyzeInitScopes admits it;
//   - a head if refused only because a sibling also declares its `ok` folds the init into the
//     condition, which declares no `ok`: golib's out-parameter assertion `x._<T>(out var v)` (c), or a
//     positional pattern on the comma-ok tuple, `m[k, ꟷ] is (var v, true)` (b1) and
//     `is not (var v, true)` (b2);
//   - an else-if's own init folds the same way, or into a `var` pattern, `f() is var (n, err) &&
//     err != nil` (b3);
//   - everything else keeps the block.
//
// Two of the ways a moved name changes meaning compile without a diagnostic (the PLAN's R6 and R8),
// so the predicate rules both out by construction rather than leaving them to the compiler.

// initScopeShape is how analyzeInitScopes decided a HEAD if, switch or type switch emits its init.
// An else-if's own init is decided where it is emitted (foldIfInit): its condition variables stay in
// the else's nested scope, so no enclosing-block predicate applies to it.
type initScopeShape int

const (
	// initScopeBraced is today's block, and the shape of every statement analyzeInitScopes did not see.
	initScopeBraced initScopeShape = iota

	// initScopeDropped is (a): the init is a statement of the enclosing block.
	initScopeDropped

	// initScopeFolded is a head if whose init folds into its condition, (c), b1 or b2.
	initScopeFolded
)

// initScopeCandidate is one head if, switch or type switch with an init, as analyzeInitScopes weighs it.
type initScopeCandidate struct {
	stmt  ast.Stmt
	space *ast.BlockStmt

	// declared holds the C# names the init declares, as initScopeName spells them.
	declared map[string]bool

	// closure is set when the statement holds anything the emitter turns into a lambda.
	closure bool

	// okName and okIdents are the `ok` of a head if that can fold (initFoldEligible): its name, and
	// the two idents that spell it (the declaration and the condition's use), which a fold removes.
	okName   string
	okIdents []*ast.Ident
}

// analyzeInitScopes decides, for each head if, switch and type switch with an init in body, whether
// the init's block can go. It runs at the end of performVariableAnalysis: every C# name the function
// declares is final there, so a Δ rename is already in the spelling it compares.
//
// Let D be the names the init declares and E the enclosing block, the nearest Go block around the
// statement (for a case clause that is the whole switch body, C#'s one switch block). The braces go
// when both hold:
//
//  1. No identifier spelling a name in D occurs anywhere in E outside the statement: no declaration,
//     use, parameter or type name, before the statement or after it, at any depth. A moved name could
//     collide with one (CS0128, CS0136, CS0844) or, worse, take over a use that bound to a field,
//     package member or outer name before (R6), which compiles. Member names after a `.`, struct
//     field keys and labels bind no local and are not counted.
//  2. The statement holds no closure. Moving a captured variable out of its block moves its closure
//     to E's entry, and a backward goto over the statement then shares one variable across what were
//     separate scopes (R8), which compiles too.
//
// A head if whose only collisions are its own `ok` folds instead, when its init is a comma-ok form
// the condition can absorb. A fold removes that `ok` from E, which can free a sibling refused only by
// it, so the decisions iterate until nothing changes.
func (v *Visitor) analyzeInitScopes(body *ast.BlockStmt) {
	v.initScopeShapes = map[ast.Stmt]initScopeShape{}

	if body == nil {
		return
	}

	excluded := v.initScopeExcludedIdents(body)
	occurrences := map[*ast.BlockStmt]map[string][]*ast.Ident{}

	var candidates []*initScopeCandidate
	var stack []ast.Node

	ast.Inspect(body, func(node ast.Node) bool {
		if node == nil {
			stack = stack[:len(stack)-1]
			return false
		}

		if candidate := v.initScopeCandidateFor(node, stack); candidate != nil {
			candidates = append(candidates, candidate)
		}

		stack = append(stack, node)
		return true
	})

	// Every identifier of each enclosing block, by C# spelling, gathered once per block.
	for _, candidate := range candidates {
		if _, ok := occurrences[candidate.space]; ok {
			continue
		}

		names := map[string][]*ast.Ident{}

		ast.Inspect(candidate.space, func(node ast.Node) bool {
			if ident, ok := node.(*ast.Ident); ok && !excluded[ident] && ident.Name != "_" {
				name := v.initScopeName(ident)
				names[name] = append(names[name], ident)
			}

			return true
		})

		occurrences[candidate.space] = names
	}

	// Each round weighs every undecided statement against the same suppressed set, so siblings that
	// refuse each other only by `ok` fold together (io.Copy's pair reads alike); only the next round
	// sees the `ok`s they gave up.
	suppressed := map[*ast.Ident]bool{}

	for {
		decided := map[*initScopeCandidate]initScopeShape{}

		for _, candidate := range candidates {
			if candidate.closure || v.initScopeShapes[candidate.stmt] != initScopeBraced {
				continue
			}

			collisions := candidate.collisions(occurrences[candidate.space], suppressed)

			if len(collisions) == 0 {
				decided[candidate] = initScopeDropped
			} else if candidate.okName != "" && len(collisions) == 1 && collisions[candidate.okName] {
				decided[candidate] = initScopeFolded
			}
		}

		if len(decided) == 0 {
			return
		}

		for candidate, shape := range decided {
			v.initScopeShapes[candidate.stmt] = shape

			if shape == initScopeFolded {
				for _, ident := range candidate.okIdents {
					suppressed[ident] = true
				}
			}
		}
	}
}

// initScopeCandidateFor returns node as a candidate when it is a head if, switch or type switch with
// an init; stack holds its ancestors, innermost last.
func (v *Visitor) initScopeCandidateFor(node ast.Node, stack []ast.Node) *initScopeCandidate {
	var init ast.Stmt
	var declaredIdents []*ast.Ident

	switch stmt := node.(type) {
	case *ast.IfStmt:
		// An else-if's init is decided where it is emitted (foldIfInit).
		if parent, ok := stackTop(stack).(*ast.IfStmt); ok && parent.Else == node {
			return nil
		}

		init = stmt.Init
	case *ast.SwitchStmt:
		init = stmt.Init
	case *ast.TypeSwitchStmt:
		init = stmt.Init

		// The guard's symbolic variable (`switch u := x.(type)`) is bound per clause, and its name
		// is emitted at the switch's own level.
		if assign, ok := stmt.Assign.(*ast.AssignStmt); ok && len(assign.Lhs) == 1 {
			if ident, ok := assign.Lhs[0].(*ast.Ident); ok && ident.Name != "_" {
				declaredIdents = append(declaredIdents, ident)
			}
		}
	default:
		return nil
	}

	if init == nil {
		return nil
	}

	var space *ast.BlockStmt

	for i := len(stack) - 1; i >= 0; i-- {
		if block, ok := stack[i].(*ast.BlockStmt); ok {
			space = block
			break
		}
	}

	if space == nil {
		return nil
	}

	ast.Inspect(init, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok && ident.Name != "_" && v.info.Defs[ident] != nil {
			declaredIdents = append(declaredIdents, ident)
		}

		return true
	})

	candidate := &initScopeCandidate{
		stmt:     node.(ast.Stmt),
		space:    space,
		declared: map[string]bool{},
		closure:  v.initScopeHasClosure(node),
	}

	for _, ident := range declaredIdents {
		candidate.declared[v.initScopeName(ident)] = true
	}

	if ifStmt, ok := node.(*ast.IfStmt); ok {
		if facts, ok := v.initFoldFactsFor(ifStmt); ok && facts.headFoldable() {
			candidate.okName = v.initScopeName(facts.ok)
			candidate.okIdents = []*ast.Ident{facts.ok, facts.okUse}
		}
	}

	return candidate
}

// collisions returns the declared names that also occur in the enclosing block outside the statement.
func (candidate *initScopeCandidate) collisions(names map[string][]*ast.Ident, suppressed map[*ast.Ident]bool) map[string]bool {
	collisions := map[string]bool{}
	start, end := candidate.stmt.Pos(), candidate.stmt.End()

	for name := range candidate.declared {
		for _, ident := range names[name] {
			if suppressed[ident] || (ident.Pos() >= start && ident.Pos() < end) {
				continue
			}

			collisions[name] = true
			break
		}
	}

	return collisions
}

// initScopeName is an identifier's C# spelling as the predicate compares it: its final name (a Δ
// rename, when performVariableAnalysis made one), escaped, without the `@` that escapes a keyword,
// since `@base` and `base` are one name to C#.
func (v *Visitor) initScopeName(ident *ast.Ident) string {
	name := ident.Name

	if renamed, ok := v.identNames[ident]; ok {
		name = renamed
	} else if renamed, ok := v.globalIdentNames[ident]; ok {
		name = renamed
	}

	return strings.TrimPrefix(getSanitizedIdentifier(name), "@")
}

// initScopeExcludedIdents returns the identifiers in body that bind no local, so they cannot collide
// with one: a member name after a `.`, a struct field (a composite literal key, a field declaration),
// a method name and a label.
func (v *Visitor) initScopeExcludedIdents(body *ast.BlockStmt) map[*ast.Ident]bool {
	excluded := map[*ast.Ident]bool{}

	ast.Inspect(body, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.SelectorExpr:
			excluded[n.Sel] = true
		case *ast.Ident:
			switch obj := v.info.ObjectOf(n).(type) {
			case *types.Var:
				if obj.IsField() {
					excluded[n] = true
				}
			case *types.Label:
				excluded[n] = true
			case *types.Func:
				if signature, ok := obj.Type().(*types.Signature); ok && signature.Recv() != nil {
					excluded[n] = true
				}
			}
		}

		return true
	})

	return excluded
}

// initScopeHasClosure reports whether node holds anything the emitter writes as a lambda over the
// enclosing method's variables: a func literal, a go or defer call, a select, a range over a func
// iterator (its body becomes the yield callback) or a method value.
func (v *Visitor) initScopeHasClosure(node ast.Node) bool {
	called := map[ast.Expr]bool{}
	closure := false

	ast.Inspect(node, func(n ast.Node) bool {
		if closure {
			return false
		}

		switch x := n.(type) {
		case *ast.FuncLit, *ast.GoStmt, *ast.DeferStmt, *ast.SelectStmt:
			closure = true
		case *ast.RangeStmt:
			if _, isFunc := v.info.TypeOf(x.X).Underlying().(*types.Signature); isFunc {
				closure = true
			}
		case *ast.CallExpr:
			called[ast.Unparen(x.Fun)] = true
		case *ast.SelectorExpr:
			if selection, ok := v.info.Selections[x]; ok && !called[x] {
				if selection.Kind() == types.MethodVal || selection.Kind() == types.MethodExpr {
					closure = true
				}
			}
		}

		return !closure
	})

	return closure
}

func stackTop(stack []ast.Node) ast.Node {
	if len(stack) == 0 {
		return nil
	}

	return stack[len(stack)-1]
}

// initFoldFacts is what an if's comma-ok init says about how it can fold into its condition:
// `v, ok := x.(T)` or `v, ok := m[k]`, with a condition of exactly `ok` or `!ok`.
type initFoldFacts struct {
	value  *ast.Ident // the value's ident; `_` when it is discarded
	ok     *ast.Ident // the declaration of `ok`
	okUse  *ast.Ident // the condition's one use of it
	assert bool       // x.(T), which golib's out-parameter overload takes; otherwise a map lookup

	negated         bool // the condition is `!ok`
	valueEscapes    bool // the value is heap-boxed, so its declaration is not a plain `var`
	valueUsedInBody bool
	valueUsedInElse bool
}

// headFoldable reports whether a head if's init can fold into its condition, keeping every use the
// shape has to keep: the out-parameter assertion assigns the value in both branches; b1's pattern
// variable is assigned only when the condition holds (R9), and b2's only when it fails.
func (facts initFoldFacts) headFoldable() bool {
	if facts.valueEscapes {
		return false
	}

	if facts.assert {
		return true
	}

	if facts.negated {
		return !facts.valueUsedInBody
	}

	return !facts.valueUsedInElse
}

// initFoldFactsFor reads ifStmt's init and condition as a comma-ok fold, or reports false.
func (v *Visitor) initFoldFactsFor(ifStmt *ast.IfStmt) (initFoldFacts, bool) {
	var facts initFoldFacts

	assign, ok := ifStmt.Init.(*ast.AssignStmt)

	if !ok || assign.Tok != token.DEFINE || len(assign.Lhs) != 2 || len(assign.Rhs) != 1 {
		return facts, false
	}

	switch rhs := ast.Unparen(assign.Rhs[0]).(type) {
	case *ast.TypeAssertExpr:
		if rhs.Type == nil {
			return facts, false
		}

		facts.assert = true
	case *ast.IndexExpr:
		if _, isMap := v.info.TypeOf(rhs.X).Underlying().(*types.Map); !isMap {
			return facts, false
		}
	default:
		return facts, false
	}

	facts.value, _ = assign.Lhs[0].(*ast.Ident)
	facts.ok, _ = assign.Lhs[1].(*ast.Ident)

	if facts.value == nil || facts.ok == nil || facts.ok.Name == "_" {
		return facts, false
	}

	okObj := v.info.Defs[facts.ok]

	if okObj == nil {
		return facts, false
	}

	cond := ast.Unparen(ifStmt.Cond)

	if not, ok := cond.(*ast.UnaryExpr); ok && not.Op == token.NOT {
		facts.negated = true
		cond = ast.Unparen(not.X)
	}

	if facts.okUse, _ = cond.(*ast.Ident); facts.okUse == nil || v.info.Uses[facts.okUse] != okObj {
		return facts, false
	}

	// `ok` is read by the condition and nowhere else, so the fold that stops declaring it loses no use.
	// Go scopes it to this statement, so the statement holds every use it has.
	okUses := 0

	ast.Inspect(ifStmt, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok && v.info.Uses[ident] == okObj {
			okUses++
		}

		return true
	})

	if okUses != 1 {
		return facts, false
	}

	if facts.value.Name != "_" {
		valueObj := v.info.Defs[facts.value]

		if valueObj == nil {
			return facts, false
		}

		facts.valueEscapes = v.identHasHeapBox(valueObj, valueObj.Type())
		facts.valueUsedInBody = v.usesObject(ifStmt.Body, valueObj)
		facts.valueUsedInElse = ifStmt.Else != nil && v.usesObject(ifStmt.Else, valueObj)
	}

	return facts, true
}

// usesObject reports whether node holds a use of obj.
func (v *Visitor) usesObject(node ast.Node, obj types.Object) bool {
	used := false

	ast.Inspect(node, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok && v.info.Uses[ident] == obj {
			used = true
		}

		return !used
	})

	return used
}

// foldIfInit folds an if's captured init into its condition, or reports false to keep the block.
// initText is the init exactly as emitted for the block, and cond the converted condition. A head if
// takes only the comma-ok folds, since its condition variables leak to the enclosing block, which
// analyzeInitScopes has cleared for them; an else-if's stay in the else's own nested scope, so it may
// also take b3, a `var` pattern over any `var` declaration.
//
// The fold works from the emitted text rather than re-deriving it, so whatever the init's emission
// decided (a Δ name, a typed or heap-boxed declaration) is what is folded or refused: only a single
// plain `var` declaration folds, and anything else keeps today's block, byte for byte.
func (v *Visitor) foldIfInit(ifStmt *ast.IfStmt, initText string, cond string, elseIf bool) (string, bool) {
	names, rhs, ok := initFoldParts(initText)

	if !ok {
		return "", false
	}

	if facts, ok := v.initFoldFactsFor(ifStmt); ok && len(names) == 2 && (elseIf || facts.headFoldable()) && !facts.valueEscapes {
		value := "var " + names[0]

		if names[0] == "_" {
			value = "_"
		}

		not := ""

		if facts.negated {
			not = "not "
		}

		if facts.assert && strings.HasSuffix(rhs, "("+TrueMarker+")") && strings.Contains(rhs, "._<") {
			folded := strings.TrimSuffix(rhs, "("+TrueMarker+")") + "(out " + value + ")"

			if facts.negated {
				return "!" + folded, true
			}

			return folded, true
		}

		if facts.headFoldable() {
			return initFoldOperand(rhs) + " is " + not + "(" + value + ", true)", true
		}
	}

	if !elseIf {
		return "", false
	}

	pattern := "var " + names[0]

	if len(names) > 1 {
		pattern = "var (" + strings.Join(names, ", ") + ")"
	}

	if hasTopLevelToken(cond, "||", "??", " ? ") {
		cond = "(" + cond + ")"
	}

	return initFoldOperand(rhs) + " is " + pattern + " && " + cond, true
}

// initFoldParts splits a captured init that is one plain `var` declaration, `var (n, err) = f();` or
// `var n = f();`, into its names and its value. Anything else reports false: a typed or `ref`
// declaration, a second statement (a heap box, a hoisted temp), a statement over several lines.
func initFoldParts(initText string) (names []string, rhs string, ok bool) {
	text := strings.TrimSpace(stripPositionSentinels(initText))

	if !strings.HasPrefix(text, "var ") || !strings.HasSuffix(text, ";") || strings.ContainsAny(text, "\r\n") {
		return nil, "", false
	}

	text = strings.TrimSuffix(strings.TrimPrefix(text, "var "), ";")

	var lhs string

	if strings.HasPrefix(text, "(") {
		end := strings.Index(text, ")")

		if end < 0 {
			return nil, "", false
		}

		lhs, rhs = text[1:end], text[end+1:]

		for _, name := range strings.Split(lhs, ",") {
			names = append(names, strings.TrimSpace(name))
		}
	} else {
		space := strings.Index(text, " ")

		if space < 0 {
			return nil, "", false
		}

		lhs, rhs = text[:space], text[space:]
		names = []string{lhs}
	}

	if !strings.HasPrefix(rhs, " = ") {
		return nil, "", false
	}

	rhs = rhs[len(" = "):]

	for _, name := range names {
		if name == "" || strings.ContainsAny(name, " ()=;") {
			return nil, "", false
		}
	}

	if rhs == "" || hasTopLevelToken(rhs, ";") {
		return nil, "", false
	}

	return names, rhs, true
}

// positionSentinelsOf returns the position sentinels in text, in order and with nothing else, so a
// fold that drops the init's statement text keeps the records it carried.
func positionSentinelsOf(text string) string {
	var sentinels strings.Builder

	for {
		open := strings.Index(text, PositionSentinel)

		if open < 0 {
			break
		}

		end := strings.Index(text[open+1:], PositionSentinel)

		if end < 0 {
			break
		}

		sentinels.WriteString(text[open : open+end+2])
		text = text[open+end+2:]
	}

	return sentinels.String()
}

// initFoldOperand is rhs as the left operand of `is`: as written when it is one primary expression
// (the emitter spaces every binary operator, so a primary has no space outside brackets and
// literals), parenthesized otherwise, since `is` binds tighter than equality and the logical
// operators.
func initFoldOperand(rhs string) string {
	if hasTopLevelToken(rhs, " ") {
		return "(" + rhs + ")"
	}

	return rhs
}

// hasTopLevelToken reports whether text holds any of tokens outside brackets and string or character
// literals.
func hasTopLevelToken(text string, tokens ...string) bool {
	depth := 0

	for i := 0; i < len(text); i++ {
		switch c := text[i]; c {
		case '(', '[', '{':
			depth++
			continue
		case ')', ']', '}':
			depth--
			continue
		case '"', '\'':
			i = skipLiteral(text, i)
			continue
		}

		if depth != 0 {
			continue
		}

		for _, token := range tokens {
			if strings.HasPrefix(text[i:], token) {
				return true
			}
		}
	}

	return false
}

// skipLiteral returns the index of the quote that closes the string or character literal opening at
// text[open], honouring backslash escapes; a verbatim or raw literal reads as one that never closes,
// which only makes hasTopLevelToken more conservative.
func skipLiteral(text string, open int) int {
	quote := text[open]

	for i := open + 1; i < len(text); i++ {
		switch text[i] {
		case '\\':
			i++
		case quote:
			return i
		}
	}

	return len(text)
}
