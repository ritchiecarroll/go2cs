// TEMPORARY census instrumentation for the non-ident-receiver method-value defect family.
// Not part of any permanent converter behavior — strip this file and its three call sites
// before banking anything. Gated on GO2CS_CENSUS_NONIDENT_RECV so a normal run is unaffected.
package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"os"
)

var censusNonIdentRecvEnabled = os.Getenv("GO2CS_CENSUS_NONIDENT_RECV") != ""

// censusMethodValueReceiver logs one method-value receiver site: which emission decision point
// reached it, the receiver's structural shape, and its type kind. site is a short constant
// naming the call site (e.g. "convSelectorExpr:1130", "visitAssignStmt:reassign",
// "visitAssignStmt:define").
func (v *Visitor) censusMethodValueReceiver(site string, selectorExpr *ast.SelectorExpr) {
	if !censusNonIdentRecvEnabled {
		return
	}

	recvExpr := selectorExpr.X
	shape := censusClassifyReceiverShape(recvExpr)

	kind := "value"

	if recvType := v.getType(recvExpr, false); recvType != nil {
		switch recvType.Underlying().(type) {
		case *types.Pointer:
			kind = "pointer"
		case *types.Interface:
			kind = "interface"
		}
	}

	pkgPath := "<nil>"

	if v.pkg != nil {
		pkgPath = v.pkg.Path()
	}

	fmt.Fprintf(os.Stderr, "CENSUS_NONIDENT_RECV site=%s shape=%s kind=%s pkg=%s method=%s\n",
		site, shape, kind, pkgPath, selectorExpr.Sel.Name)
}

// censusClassifyReceiverShape buckets a method-value receiver expression by whether its
// evaluation timing is observable: "ident" (a bare identifier — safe to snapshot by name),
// "field-chain" (a pure field path rooted in an identifier, no call/index in the chain —
// conditionally safe, depends on no intervening write to the root), "index" (an index
// expression anywhere in the chain — the header/root gets snapshotted today but the backing
// store does not, so a slice-typed root is unsafe), "call" (a call expression anywhere in the
// chain — nothing is snapshotted today, the call itself re-runs live), or "other" (deref or
// anything else not covered above).
func censusClassifyReceiverShape(expr ast.Expr) string {
	if _, ok := expr.(*ast.Ident); ok {
		return "ident"
	}

	hasCall := false
	hasIndex := false
	hasOther := false

	var walk func(ast.Expr)

	walk = func(e ast.Expr) {
		switch n := e.(type) {
		case *ast.Ident:
			// root reached, nothing further to classify
		case *ast.SelectorExpr:
			walk(n.X)
		case *ast.IndexExpr:
			hasIndex = true
			walk(n.X)
		case *ast.CallExpr:
			hasCall = true

			if n.Fun != nil {
				walk(n.Fun)
			}
		case *ast.StarExpr:
			hasOther = true
			walk(n.X)
		case *ast.ParenExpr:
			walk(n.X)
		default:
			hasOther = true
		}
	}

	walk(expr)

	switch {
	case hasCall:
		return "call"
	case hasIndex:
		return "index"
	case hasOther:
		return "other"
	default:
		return "field-chain"
	}
}
