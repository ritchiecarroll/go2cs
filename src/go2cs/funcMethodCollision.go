// funcMethodCollision.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/token"
	"go/types"
	"path/filepath"
	"strings"
	"sync"
)

// A package-level FREE function and a METHOD may share a name in Go, because method names live in their type's
// namespace. In C# both land in the package class, and a method's receiver becomes the extension method's leading
// `this` parameter, which signature identity ignores. So `func (t *Type) IsTupleType() bool` and
// `func IsTupleType(t *Type) bool` both emit `static bool IsTupleType(ж<Type>)` (tsc's internal/checker, C40, CS0111).
//
// resolveReceiverParameterCollisions settles this shape inside a `-tests` variant by renaming the TEST-side
// declarator, because production names are pinned there. A collision between two PRODUCTION declarators breaks
// the production conversion itself, and the free function takes the Δ-rename here, the same side the test rule
// chooses when both declarators are test-declared.
//
// The answer is computed from go/types alone, so the declaring package and every package that references the
// function agree without any metadata between them: a consumer asks about the same *types.Func and reaches the
// same answer from the declaring package's scope. Only declarations in non-test files count, so a `_test.go`
// method can never rename a production function whose committed .cs spells it bare.

var (
	funcMethodCollisionMu    sync.Mutex
	funcMethodCollisionCache = map[types.Object]bool{}
)

// funcRenamedForMethodCollision reports whether obj is a production package-level FREE function whose emitted C#
// signature collides with a production method of a type its own package declares, so it emits, and is referenced,
// Δ-renamed.
func funcRenamedForMethodCollision(obj types.Object, fset *token.FileSet) bool {
	fn, ok := obj.(*types.Func)

	if !ok || fn.Pkg() == nil {
		return false
	}

	signature, ok := fn.Type().(*types.Signature)

	if !ok || signature.Recv() != nil {
		return false
	}

	funcMethodCollisionMu.Lock()
	defer funcMethodCollisionMu.Unlock()

	if answer, cached := funcMethodCollisionCache[obj]; cached {
		return answer
	}

	answer := false

	if declaredInProductionFile(fn, fset) {
		scope := fn.Pkg().Scope()

		for _, name := range scope.Names() {
			typeName, ok := scope.Lookup(name).(*types.TypeName)

			if !ok || typeName.IsAlias() {
				continue
			}

			named, ok := typeName.Type().(*types.Named)

			if !ok {
				continue
			}

			for i := range named.NumMethods() {
				method := named.Method(i)

				if method.Name() != fn.Name() || !declaredInProductionFile(method, fset) {
					continue
				}

				if methodSignature, ok := method.Type().(*types.Signature); ok && emittedSignaturesCollide(methodSignature, signature) {
					answer = true
					break
				}
			}

			if answer {
				break
			}
		}
	}

	funcMethodCollisionCache[obj] = answer

	return answer
}

// declaredInProductionFile reports whether obj is declared in a file that is not a `_test.go` file. An object with
// no position (no file set, or one from export data) counts as production: export data never carries test files.
func declaredInProductionFile(obj types.Object, fset *token.FileSet) bool {
	if fset == nil || !obj.Pos().IsValid() {
		return true
	}

	position := fset.Position(obj.Pos())

	return position.Filename == "" || !strings.HasSuffix(strings.ToLower(filepath.Base(position.Filename)), "_test.go")
}
