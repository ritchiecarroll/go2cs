// convTypeAssertExpr.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) convTypeAssertExpr(typeAssertExpr *ast.TypeAssertExpr) string {
	if typeAssertExpr.Type == nil {
		return fmt.Sprintf("%s.type()", v.convExpr(typeAssertExpr.X, nil))
	}

	// An EMPTY-source assertion (`any`) whose expression still carries a RESOLVABLE concrete type
	// records that declared pairing, so the nominal adapter — the 1.1 ns fast path — is generated
	// for the type this site actually converts. Nothing is inferred here: the record is the concrete
	// getUnderlyingType yields, exactly as for an explicit conversion or witness.
	//
	// A NON-EMPTY source asserting to a named target records NOTHING (the structural recorders were
	// retired with the run-time interface shells — see ConversionStrategies-Reference "Interface type
	// assertions"): getUnderlyingType yields the interface itself there, not the dynamic type, so any
	// record would have to be GUESSED by enumerating the current package's concrete types — a scan
	// that was blind by construction to a dynamic type living in a later-converted assembly. golib's
	// AdapterBinder resolves such an assert at run time from the concrete's OWN assembly instead.
	exprType := v.getExprType(typeAssertExpr.X)

	if sourceIsInterface, sourceIsEmpty := isInterface(exprType); sourceIsInterface && sourceIsEmpty {
		targetType := v.getExprType(typeAssertExpr.Type)

		if needsInterfaceCast, isEmpty := isInterface(targetType); needsInterfaceCast && !isEmpty {
			concreteType := v.getUnderlyingType(typeAssertExpr.X)

			if concreteType != nil {
				v.convertToInterfaceType(targetType, concreteType, "")
			}
		}
	}

	// Get the type information for this expression
	typesInfo := v.info.TypeOf(typeAssertExpr)
	var safeAssertDescriminator string

	// Check if this is used in a multi-value context, thus making it a safe assertion
	if tuple, isTuple := typesInfo.(*types.Tuple); isTuple && tuple.Len() == 2 {
		safeAssertDescriminator = TrueMarker
	}

	context := DefaultIdentContext()
	context.isType = true

	// convExpr on the TYPE expression already yields the C# form — do NOT run it through
	// convertToCSTypeName again: the conversion is not idempotent, re-sanitizing machinery
	// names inside generic args (`d.(chan struct{})` → `channel<EmptyStruct>` whose inner
	// arg re-sanitized to the reserved-Δ `ΔEmptyStruct` — context CS0246 ×4/CS0019).
	typeExpr := v.convExpr(typeAssertExpr.Type, []ExprContext{context})

	// A methodless named func type collapses to its base C# delegate everywhere it is REFERENCED
	// (getAliasQualifiedTypeName), so its NAME is never emitted. A type-assertion target `ci.(Compressor)` where
	// `type Compressor func(io.Writer) (io.WriteCloser, error)` must therefore assert against the
	// collapsed delegate `Func<…>`, not the bare (undefined) name `Compressor` — convExpr on the type
	// ident renders the name (CS0246, archive/zip's compressor/decompressor registries). Render the
	// collapsed form for such a target.
	// An ANONYMOUS func type target takes the same route: the AST render above goes through the
	// string-based type-name path, which skips the VARIADIC params-Span delegate lowering —
	// `cw.(func(string, ...any))` emitted `._<Action<@string, .any>>` with a literal `.any`
	// (CS1001, net/http transport.go logf). getCSharpTypeName builds the delegate structurally via
	// iifeDelegateType (`Actionꓸꓸꓸ<@string, any>`); non-variadic signatures render identically
	// on both paths.
	if targetType := v.getExprType(typeAssertExpr.Type); targetType != nil {
		_, isMethodlessNamed := methodlessNamedFuncSignature(targetType)
		_, isAnonSig := targetType.(*types.Signature)

		if isMethodlessNamed || isAnonSig {
			typeExpr = v.getCSharpTypeName(targetType)
		}
	}

	return fmt.Sprintf("%s._<%s>(%s)", v.convExpr(typeAssertExpr.X, nil), typeExpr, safeAssertDescriminator)
}
