// liftedTypeParams.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/types"
	"sort"
)

// An anonymous type LIFTED out of a generic declaration closes over that declaration's type parameters
// -- Go 1.26's crypto/x509 `type constraints[T, V any] struct { permitted interface{ query(V) (T, bool) } }`.
// C# has no anonymous types, so the lift is a declaration of its own, and a declaration that names `T`
// and `V` must declare them: `partial interface constraints_permitted<T, V>`, referenced as
// `constraints_permitted<T, V>` wherever the generic struct names it. Lifted without them, both names
// were unbound (CS0246), and crypto/x509 blocked every remaining project of H7 pass 7.

// liftCapturedTypeParams returns the type parameters t mentions, in their declared order.
func liftCapturedTypeParams(t types.Type) []*types.TypeParam {
	if t == nil {
		return nil
	}

	found := map[*types.TypeParam]bool{}
	seen := map[types.Type]bool{}

	var walk func(types.Type)

	walk = func(t types.Type) {
		if t == nil || seen[t] {
			return
		}

		seen[t] = true

		switch typ := t.(type) {
		case *types.TypeParam:
			found[typ] = true
		case *types.Pointer:
			walk(typ.Elem())
		case *types.Slice:
			walk(typ.Elem())
		case *types.Array:
			walk(typ.Elem())
		case *types.Map:
			walk(typ.Key())
			walk(typ.Elem())
		case *types.Chan:
			walk(typ.Elem())
		case *types.Tuple:
			for i := 0; i < typ.Len(); i++ {
				walk(typ.At(i).Type())
			}
		case *types.Signature:
			walk(typ.Params())
			walk(typ.Results())
		case *types.Struct:
			for i := 0; i < typ.NumFields(); i++ {
				walk(typ.Field(i).Type())
			}
		case *types.Interface:
			for i := 0; i < typ.NumExplicitMethods(); i++ {
				walk(typ.ExplicitMethod(i).Type())
			}

			for i := 0; i < typ.NumEmbeddeds(); i++ {
				walk(typ.EmbeddedType(i))
			}
		case *types.Named:
			if args := typ.TypeArgs(); args != nil {
				for i := 0; i < args.Len(); i++ {
					walk(args.At(i))
				}
			}
		case *types.Alias:
			walk(types.Unalias(typ))
		}
	}

	walk(t)

	captured := make([]*types.TypeParam, 0, len(found))

	for typeParam := range found {
		captured = append(captured, typeParam)
	}

	sort.Slice(captured, func(i, j int) bool {
		if captured[i].Index() != captured[j].Index() {
			return captured[i].Index() < captured[j].Index()
		}

		return captured[i].Obj().Name() < captured[j].Obj().Name()
	})

	return captured
}

// capturedLiftKey identifies a capturing lift by the parameter OBJECTS it closes over and its signature:
// the same text is the same type only under the same declaration's parameters.
func capturedLiftKey(captured []*types.TypeParam, signature types.Type) string {
	key := ""

	for _, typeParam := range captured {
		key += fmt.Sprintf("%p,", typeParam.Obj())
	}

	if signature != nil {
		key += "|" + signature.String()
	}

	return key
}

// capturedTypeParamNames is the captured parameters' names, for liftedTypeParamList (visitIdent.go),
// the spelling every lift that binds type parameters shares. As for a lift out of a generic function
// (localTypeUsedTypeParams), the lift carries NO constraint clauses: the enclosing declaration's own
// clauses govern every use, and a lifted interface binding `<T, V>` is legal C# without them.
func capturedTypeParamNames(captured []*types.TypeParam) []string {
	names := make([]string, len(captured))

	for i, typeParam := range captured {
		names[i] = getSanitizedIdentifier(typeParam.Obj().Name())
	}

	return names
}
