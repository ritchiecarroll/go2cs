// dynamicInterfaceBase.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/hex"
	"fmt"
	"go/types"
	"sort"
	"strconv"
	"strings"
)

// Go type IDENTITY across lifts (H7 class E, part b). Go 1.26's crypto/x509 fills
// `constraints[string, string]{…, newDNSConstraints(…), …}`: the function returns
// `interface{ query(string) (string, bool) }` and the field is `interface{ query(V) (T, bool) }` at
// T=V=string -- in Go ONE type. In C# the function's result is a lifted dyn interface of its own
// (`newDNSConstraints_R0`) and the field is the generic lift `constraints_permitted<T, V>`
// (liftedTypeParams.go), two unrelated interfaces, so the literal could not convert.
//
// For every instantiation of a generic struct whose field interface was lifted generic, the concrete
// shape's own lift is made to DERIVE the closed generic: `partial interface newDNSConstraints_R0 :
// constraints_permitted<@string, @string> {}`, an implicit reference conversion, so the literal compiles
// unchanged and a value of either interface dispatches through the one method its adapter implements.
// The part is written as a deferred marker on a line of its own and resolved after the file-visit
// barrier through the dyn-name registry, so the lift it names may live in any file of the package; a
// shape no lift ever claimed resolves to an empty line (a direct assignment from a concrete type is
// served by the ordinary GoImplement adapter for the closed generic).
//
// The concrete lift's member then REDECLARES the inherited one, CS0108 in every such build. So every
// dyn lift's member is written behind a second marker that resolves to `new ` exactly for the lifts that
// gained a base, and to nothing everywhere else -- every other dyn interface stays byte-identical.

const (
	dynamicBaseMarkerPrefix = "«DYNBASE:"
	dynamicBaseMarkerSuffix = ":DYNBASE»"
	dynamicNewMarkerPrefix  = "«DYNNEW:"
	dynamicNewMarkerSuffix  = ":DYNNEW»"
)

// dynamicNewMarker prefixes a dyn lift's member; it resolves to `new ` when the lift gained a base.
func dynamicNewMarker(interfaceName string) string {
	return dynamicNewMarkerPrefix + hex.EncodeToString([]byte(interfaceName)) + dynamicNewMarkerSuffix
}

// dynamicBaseMarker is one deferred base part: the concrete shape's signature, the closed generic it
// derives, and the indentation the resolved declaration takes.
func dynamicBaseMarker(signature, closedGeneric string, indent int) string {
	return dynamicBaseMarkerPrefix + hex.EncodeToString([]byte(signature)) + "." + hex.EncodeToString([]byte(closedGeneric)) + "." + strconv.Itoa(indent) + dynamicBaseMarkerSuffix
}

// writeLiftedInterfaceBaseMarkers writes one base marker per concrete instantiation of the generic struct
// structType whose field fieldName was lifted generic as baseName (`constraints_permitted`).
func (v *Visitor) writeLiftedInterfaceBaseMarkers(target *strings.Builder, structType types.Type, fieldName, baseName string) {
	named, ok := structType.(*types.Named)

	if !ok || named.TypeParams().Len() == 0 || v.liftBaseMarked[baseName] {
		return
	}

	if v.liftBaseMarked == nil {
		v.liftBaseMarked = map[string]bool{}
	}

	v.liftBaseMarked[baseName] = true

	if target == nil {
		target = v.outputBuilder
	}

	markers := map[string]string{}

	for ident, instance := range v.info.Instances {
		if v.info.Uses[ident] != named.Obj() {
			continue
		}

		instantiated, ok := instance.Type.(*types.Named)

		if !ok {
			continue
		}

		fields, ok := instantiated.Underlying().(*types.Struct)

		if !ok {
			continue
		}

		for i := 0; i < fields.NumFields(); i++ {
			field := fields.Field(i)

			// An instantiation inside generic code (`constraints[T, V]` in a signature) is still open.
			if field.Name() != fieldName || len(liftCapturedTypeParams(field.Type())) > 0 {
				continue
			}

			arguments := make([]string, instance.TypeArgs.Len())

			for j := range arguments {
				arguments[j] = v.getCSharpTypeName(instance.TypeArgs.At(j))
			}

			closed := baseName + "<" + strings.Join(arguments, ", ") + ">"
			signature := field.Type().String()
			markers[signature+"\x00"+closed] = dynamicBaseMarker(signature, closed, len(v.indent(v.indentLevel)))
		}
	}

	keys := make([]string, 0, len(markers))

	for key := range markers {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	for _, key := range keys {
		target.WriteString(markers[key])
		target.WriteString(v.newline)
	}
}

// resolveDynamicInterfaceBases resolves a package's base parts, then the `new` its derived lifts'
// members take. Runs after resolveDynamicTypeMarkers, over the same file set.
func resolveDynamicInterfaceBases(outputFileNames []string) {
	derived := map[string]bool{}

	rewriteDeferredMarkers(outputFileNames, "dynamic interface base", dynamicBaseMarkerPrefix, dynamicBaseMarkerSuffix,
		func(fileName string, line int, payload string) (string, bool) {
			parts := strings.Split(payload, ".")

			if len(parts) != 3 {
				return "", false
			}

			signature, sigErr := hex.DecodeString(parts[0])
			closed, closedErr := hex.DecodeString(parts[1])
			indent, indentErr := strconv.Atoi(parts[2])

			if sigErr != nil || closedErr != nil || indentErr != nil {
				return "", false
			}

			// Only a lift declared in THIS package can take a partial part; a production-registry
			// lift lives in another assembly.
			name := lookupDynamicTypeName(string(signature))

			if name == "" {
				return "", false
			}

			derived[name] = true

			return fmt.Sprintf("%spartial interface %s : %s {}", strings.Repeat(" ", indent), name, closed), false
		})

	rewriteDeferredMarkers(outputFileNames, "dynamic interface new", dynamicNewMarkerPrefix, dynamicNewMarkerSuffix,
		func(fileName string, line int, payload string) (string, bool) {
			name, err := hex.DecodeString(payload)

			if err == nil && derived[string(name)] {
				return "new ", true
			}

			return "", true
		})
}
