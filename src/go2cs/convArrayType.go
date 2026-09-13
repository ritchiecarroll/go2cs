// convArrayType.go - Gbtc
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
	"strings"
)

// goArrayDims returns the Go fixed-size array dimensions of t, outermost first ([4][8]byte -> 4, 8),
// or nil when t is not an unnamed array type.
//
// The dimension is the ONE part of a Go array type the managed emission cannot carry: `[32]byte`
// renders as golib `array<byte>`, and C# has no const generic parameter to hold the 32 in the TYPE.
// Every other position recovers it from a live source instead -- a value reveals its own length
// (GoReflect.ArrayDimsOfValue), and a struct FIELD recovers it from the declaring type's zero
// instance, because the converter emits the dimension as a field initializer (`= new(32)`) that the
// generated parameterless constructor runs. A func PARAMETER has neither: there is no value at a
// type-only position and no initializer to read, so `reflect.TypeOf(f).In(0)` answered a dims-less
// array descriptor, `Len()` 0 and `String()` "[]uint8" -- which is why testing/quick synthesized a
// ZERO-length array for a `[32]byte` parameter and every property test over a fixed-size array ran
// against the empty value. See emitGoArrayDimsAttribute for the carrier.
func goArrayDims(t types.Type) []int64 {
	var dims []int64

	for {
		array, ok := types.Unalias(t).(*types.Array)

		if !ok {
			return dims
		}

		dims = append(dims, array.Len())
		t = array.Elem()
	}
}

// emitGoArrayDimsAttribute renders the `[GoArrayDims(...)]` parameter attribute carrying the Go
// array dimensions t DENOTES -- its own when t is an unnamed array type, its pointee's when t is a
// pointer to one -- or "" when neither.
//
// It is a PARAMETER attribute rather than anything on the type because the parameter position is
// the only place the datum can live: the emitted delegate type is a bare `Func<array<byte>, bool>`,
// shared by `func([32]byte) bool` and `func([64]byte) bool` alike. The reflection bridge reads it
// back off the delegate INSTANCE (`Delegate.Method.GetParameters()`), which resolves for every
// shape go2cs emits -- a declared func used as a method group, a non-capturing lambda, a capturing
// lambda's display-class method, and a natural-typed lambda -- and stamps it as descriptor cargo in
// abi.TypeOf, so reflect.Type.In(i) hands out an array type that knows its length.
//
// The POINTER hop is there because `*[N]T` is where a caller ALLOCATES from the parameter type: a
// callee that must write its result through the parameter takes a pointer, so the type-only
// position a length has to survive is exactly the pointee's. net/rpc is the case -- every service
// method's reply is a `*T`, and the server does `reflect.New(mtype.ReplyType.Elem())` -- and with
// no dims on the parameter it allocated a ZERO-length array for a `*[1]int` reply, which the callee
// then indexed ("index out of range [0] with length 0", net/rpc/jsonrpc's TestBuiltinTypes). The
// dims ride the POINTER descriptor and the bridge's Elem() hands them to the pointee; one hop only,
// because one hop is what a Go signature ever spells at this position.
func emitGoArrayDimsAttribute(t types.Type) string {
	dims := goArrayDims(t)

	if len(dims) == 0 {
		if pointer, ok := types.Unalias(t).(*types.Pointer); ok {
			dims = goArrayDims(pointer.Elem())
		}
	}

	if len(dims) == 0 {
		return ""
	}

	values := make([]string, len(dims))

	for i, dim := range dims {
		values[i] = fmt.Sprintf("%d", dim)
	}

	return fmt.Sprintf("[GoArrayDims(%s)] ", strings.Join(values, ", "))
}

func (v *Visitor) convArrayType(arrayType *ast.ArrayType, context ArrayTypeContext) string {
	typeName := v.getExpressionTypeName(arrayType, false)

	if context.compositeInitializer && strings.Contains(typeName, "[") {
		// Remove array brackets for composite literal initialization
		start := strings.Index(typeName, "[")
		end := strings.Index(typeName[start:], "]") + start

		if end != -1 {
			typeName = typeName[:start] + typeName[end+1:]
		}
	}

	typeName = convertToCSTypeName(typeName)

	if context.indexedInitializer {
		// For indexed initialization use a sparse array for initialization of slice or array.
		// golib's support types live in the go.golib child namespace (renamed from go.runtime,
		// which collided with the REAL runtime package's import alias -- CS0576 in any package
		// importing runtime once golib is referenced, e.g. iter/weak).
		return fmt.Sprintf("golib.SparseArray<%s>", typeName)
	} else if context.compositeInitializer {
		// Use basic array type for composite literal initialization of slice or array
		return fmt.Sprintf("%s[]", typeName)
	}

	var suffix string

	if context.maxLength > 0 {
		suffix = fmt.Sprintf("(%s)", context.lengthArgs())
	}

	if v.options.preferVarDecl {
		return fmt.Sprintf("%s%s", typeName, suffix)
	}

	if len(suffix) > 0 {
		return suffix
	}

	return "()"
}
