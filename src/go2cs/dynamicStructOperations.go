// dynamicStructOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns ANONYMOUS and STRUCTURALLY-TYPED structs — Go's `struct{ a int; b string }`
// written inline, with no name to convert.
//
// C# has no anonymous struct with a stable identity across files, so the converter LIFTS each one
// to a generated named type. That raises two questions this file answers: whether a given type is
// one of these "dynamic" structs at all, and how a value of one converts to another shape —
// including reaching a field PROMOTED from an embedded struct, which Go resolves silently and C#
// must be told the full path to.
//
// The naming of lifted types lives in liftedTypeNames.go; the cross-file resolution of a name
// decided in a sibling file lives in dynamicTypeOperations.go.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

func isDynamicStruct(t types.Type) bool {
	if t == nil {
		return false
	}

	// If it's a pointer, get its element.
	if ptr, ok := t.(*types.Pointer); ok {
		t = ptr.Elem()
	}

	// If it's a named type, then it’s not dynamic.
	if _, ok := t.(*types.Named); ok {
		return false
	}

	// Finally, check if it is a direct struct.
	_, ok := t.(*types.Struct)

	return ok
}

func (v *Visitor) checkForDynamicStructs(argType types.Type, targetType types.Type) string {
	if argType == nil || targetType == nil {
		return ""
	}

	// Only proceed if the target type is a dynamic (anonymous) struct
	if !isDynamicStruct(targetType) {
		return ""
	}

	// If targetType is a pointer, get its element and underlying type
	if ptrType, ok := targetType.(*types.Pointer); ok {
		targetType = ptrType.Elem().Underlying()
	}

	if _, ok := targetType.(*types.Struct); ok {
		// Likewise for argType.
		if ptrType, ok := argType.(*types.Pointer); ok {
			argType = ptrType.Elem().Underlying()
		}

		var argTypeName, targetTypeName string

		if _, ok := argType.(*types.Struct); ok {
			// Argument is a dynamic struct and target is a dynamic struct, track implicit conversions
			argTypeName = v.getCSharpTypeName(argType)
			targetTypeName = v.getCSharpTypeName(targetType)
		} else if _, ok := argType.(*types.Named); ok {
			// Argument is a named type and target is a dynamic struct, track implicit conversions
			argTypeName = v.getCSharpTypeName(argType)
			targetTypeName = v.getCSharpTypeName(targetType)
		}

		if len(argTypeName) > 0 && len(targetTypeName) > 0 {
			// In C#, operators are only allowed to be public, so if target type is
			// private and argument type is public, we need to manually apply conversions
			// instead of relying on implicit conversions
			argScope := getAccess(argTypeName)
			targetScope := getAccess(targetTypeName)

			if argScope == "public" && targetScope == "internal" {
				return v.dynamicCast(argType, targetType, targetTypeName)
			} else {
				// Track implicit conversions
				packageLock.Lock()

				var conversions HashSet[string]
				var exists bool

				if conversions, exists = implicitConversions[argTypeName]; exists {
					conversions.Add(targetTypeName)
				} else {
					conversions = NewHashSet([]string{targetTypeName})
					implicitConversions[argTypeName] = conversions
				}

				packageLock.Unlock()

				v.addImplicitSubStructConversions(argType, targetTypeName, false)
			}
		}
	}

	return ""
}

// dynamicCast generates a C# expression to cast a value of sourceType to targetType
// where both are structs that match "structurally" but are different types. This is
// used only as a fallback operation when no implicit conversion is allowed, e.g.,
// when the source type is public and the anonymous target type is internal. This is
// required since C# does not allow implicit operator conversions between structs
// with differnet access scopes, i.e., all operators in C# must be public, hence the
// types used with an operator must also be public :-p
func (v *Visitor) dynamicCast(sourceType types.Type, targetType types.Type, targetTypeName string) string {
	// Unwrap pointer types if needed
	if sourcePtr, ok := sourceType.(*types.Pointer); ok {
		sourceType = sourcePtr.Elem()
	}

	if targetPtr, ok := targetType.(*types.Pointer); ok {
		targetType = targetPtr.Elem()
	}

	// Get the underlying struct types
	sourceStruct, ok := sourceType.Underlying().(*types.Struct)

	if !ok {
		v.showWarning("Source type '%s' used with 'dynamicCast' is not a struct", sourceType.String())
		return ""
	}

	targetStruct, ok := targetType.Underlying().(*types.Struct)

	if !ok {
		v.showWarning("Target type '%s' used with 'dynamicCast' is not a struct", targetType.String())
		return ""
	}

	// Track all fields we need to include in the constructor
	params := make([]string, 0, targetStruct.NumFields())

	// Process target struct fields -- note that we are ignoring unexported
	// fields here since the target use case is to create a new instance of
	// an internal struct that is not accessible outside the package
	for i := range targetStruct.NumFields() {
		targetField := targetStruct.Field(i)
		targetFieldName := targetField.Name()

		// Sanitize the field name to avoid C# keyword conflicts
		sanitizedFieldName := getSanitizedIdentifier(targetFieldName)
		found := false

		// First try to find field directly in source struct
		for j := range sourceStruct.NumFields() {
			sourceField := sourceStruct.Field(j)
			if sourceField.Name() == targetFieldName {
				params = append(params, fmt.Sprintf("%s.%s", DynamicCastArgMarker, sanitizedFieldName))
				found = true
				break
			}
		}

		// If not found directly, check for promoted fields in embedded structs
		if !found {
			accessPath := v.findPromotedFieldPath(sourceStruct, targetFieldName, "")

			if len(accessPath) > 0 {
				params = append(params, fmt.Sprintf("%s.%s", DynamicCastArgMarker, accessPath))
				found = true
			}
		}

		// If field not found in source at all, leave a comment
		if !found {
			// This is an unexpected error so long as this function is called in context of checking
			// for needed dynamic struct casts, as the source and target types should be structurally
			// equivalent in order to get to this point
			v.showWarning("Field '%s' not found in source struct '%s' for dynamic cast", targetFieldName, sourceType.String())
			return ""
		}
	}

	// Construct the expression using object initializer syntax
	return fmt.Sprintf("new %s(%s)", targetTypeName, strings.Join(params, ", "))
}

// findPromotedFieldPath recursively searches for a promoted field in a struct
// and returns the access path to that field or an empty string if not found
func (v *Visitor) findPromotedFieldPath(sourceStruct *types.Struct, targetFieldName string, pathPrefix string) string {
	for i := range sourceStruct.NumFields() {
		field := sourceStruct.Field(i)

		// Check if this is an embedded field (anonymous struct field)
		if field.Anonymous() {
			var currentPath string

			if field.Name() == "" {
				currentPath = pathPrefix // Unnamed embedded field
			} else {
				// Named embedded field
				if pathPrefix == "" {
					currentPath = getSanitizedIdentifier(field.Name())
				} else {
					currentPath = pathPrefix + "." + getSanitizedIdentifier(field.Name())
				}
			}

			// Check if the field itself is what we're looking for
			if field.Name() == targetFieldName {
				return currentPath
			}

			// Check the embedded field's type for further embedding
			if fieldStruct, ok := field.Type().Underlying().(*types.Struct); ok {
				// Search within the embedded struct
				if result := v.findPromotedFieldPath(fieldStruct, targetFieldName, currentPath); result != "" {
					return result
				}
			}
		}
	}

	return "" // Field not found in any embedded struct
}

func isEmptyStruct(structType *ast.StructType) bool {
	if structType == nil {
		return false
	}

	// Empty struct has no fields
	return len(structType.Fields.List) == 0
}

func isEmptyStructType(structType *types.Struct) bool {
	if structType == nil {
		return false
	}

	// Empty struct has no fields
	return structType.NumFields() == 0
}

// extractStructType returns the anonymous struct literal an expression's TYPE syntax reaches,
// together with its resolved types.Type, so the caller can lift it to a named C# type. Empty
// structs are excluded — they map to golib's EmptyStruct and are never lifted.
func (v *Visitor) extractStructType(expr ast.Expr) (*ast.StructType, types.Type) {
	found := firstAnonymousTypeLiteral(typeSyntaxOf(expr), func(candidate ast.Expr) bool {
		structType, isStruct := candidate.(*ast.StructType)
		return isStruct && !isEmptyStruct(structType)
	})

	if found == nil {
		return nil, nil
	}

	return found.(*ast.StructType), v.getType(found, false)
}
