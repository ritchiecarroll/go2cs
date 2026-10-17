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
	"strings"

	"github.com/ritchiecarroll/hashset"
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

	// An assertion to the anonymous EMPTY interface holds for every non-nil dynamic value and fails
	// only on a nil interface -- Go's rule, and `any`'s exactly. Written as a type literal it reached
	// convInterfaceType, which LIFTED it to a dynamic interface (objx's `v.data.(interface{})` became
	// `_<Inter_type>`), and no value implements that: every non-nil assert failed ("interface {} is
	// string, not interface {}"). It is `any`, as the identifier already spells it.
	if iface, ok := v.getExprType(typeAssertExpr.Type).(*types.Interface); ok && iface.Empty() {
		return fmt.Sprintf("%s._<any>(%s)", v.convExpr(typeAssertExpr.X, nil), safeAssertDescriminator)
	}

	context := DefaultIdentContext()
	context.isType = true

	// convExpr on the TYPE expression already yields the C# form — do NOT run it through
	// convertToCSTypeName again: the conversion is not idempotent, re-sanitizing machinery
	// names inside generic args (`d.(chan struct{})` → `channel<EmptyStruct>` whose inner
	// arg re-sanitized to the reserved-Δ `ΔEmptyStruct` — context CS0246 ×4/CS0019).
	//
	// Go allows the asserted type to be PARENTHESIZED — objx writes `v.data.((Map))` — and a
	// ParenExpr renders with its parentheses, which a C# type argument cannot carry
	// (`_<(Map)>()`, CS1525). The parentheses are syntax only, so the type is rendered unwrapped.
	typeExpr := v.convExpr(ast.Unparen(typeAssertExpr.Type), []ExprContext{context})

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

	receiver := v.convExpr(typeAssertExpr.X, nil)

	if safeAssertDescriminator != "" {
		return fmt.Sprintf("%s._<%s>(%s)", receiver, typeExpr, safeAssertDescriminator)
	}

	// Go prints the static interface the assertion started from ("io.Reader is *os.File, not
	// *bytes.Buffer"), which golib's `_<T>(this object)` cannot see. Three shapes, by what the source
	// interface can name; see assertSourceFormFor.
	switch form := v.assertSourceFormFor(exprType); form {
	case assertSourceExplicit:
		return fmt.Sprintf("%s._<%s, %s>()", receiver, v.getCSharpTypeName(exprType), typeExpr)
	case assertSourceWiden:
		receiver = fmt.Sprintf("((object)%s)", receiver)
	}

	return fmt.Sprintf("%s._<%s>()", receiver, typeExpr)
}

// assertSourceForm is how a single-value assertion spells its source interface.
type assertSourceForm int

const (
	// assertSourceBare is `x._<T>()`. For an exported interface go2cs-gen emits an extension overload
	// `_<T>(this I source)` in the package's top-level `<pkg>_package_asserts` class, which C# picks over
	// golib's object form by identity conversion; for any other source the object form binds and the
	// text names "interface {}".
	assertSourceBare assertSourceForm = iota

	// assertSourceExplicit is `x._<I, T>()`, binding golib's `_<TSource, T>(this TSource)`. It is used
	// wherever the interface can be named but no generated overload is reachable: an unexported
	// interface is a private nested type that a top-level class cannot name in a signature; a generic
	// one takes no overload; and an exported one's overload may sit where C# lookup cannot reach it.
	assertSourceExplicit

	// assertSourceWiden is `((object)x)._<T>()`, for a source the file cannot name that embeds named
	// interfaces: left bare, the call would bind an embedded interface's overload and name THAT
	// interface (or be ambiguous, CS0121, between two), so it is cast to object and named "interface {}".
	assertSourceWiden
)

// assertSourceFormFor chooses the spelling for a single-value assertion on a value of static type
// sourceType.
//
// The converter can name the source in `x._<I, T>()` whenever the interface is declared at package level
// and is either of this package or exported (the alias or using that spells it is added as for any other
// reference), and then the text is exactly Go's. The bare form is kept where an overload is reachable,
// because it reads as Go does. Only an unexported interface of ANOTHER package, and one declared inside
// a function, cannot be named; those keep golib's object form and the text "interface {}", and are
// the residuals the BOARD row records.
func (v *Visitor) assertSourceFormFor(sourceType types.Type) assertSourceForm {
	if sourceType == nil {
		return assertSourceBare
	}

	named, isNamed := types.Unalias(sourceType).(*types.Named)

	if !isNamed {
		return assertSourceBare
	}

	iface, isInterface := named.Underlying().(*types.Interface)

	if !isInterface {
		return assertSourceBare
	}

	obj := named.Obj()
	pkg := obj.Pkg()

	// The universe `error` has golib's own `errorExtensions._<T>`.
	if pkg == nil {
		return assertSourceBare
	}

	generic := named.TypeParams().Len() > 0 || named.TypeArgs().Len() > 0
	declaredInPackageScope := obj.Parent() == pkg.Scope()
	nameable := declaredInPackageScope && (pkg == v.pkg || obj.Exported())

	if nameable {
		if generic || !obj.Exported() || !v.assertOverloadIsReachable(pkg) {
			return assertSourceExplicit
		}

		return assertSourceBare
	}

	for i := 0; i < iface.NumEmbeddeds(); i++ {
		if embedded, ok := types.Unalias(iface.EmbeddedType(i)).(*types.Named); ok {
			if _, ok := embedded.Underlying().(*types.Interface); ok {
				return assertSourceWiden
			}
		}
	}

	return assertSourceBare
}

// assertOverloadIsReachable reports whether the `<pkg>_package_asserts` class of pkg is found by C#
// extension-method lookup from the file being converted, AHEAD of golib's `_<T>(this object)`.
//
// Lookup walks the enclosing namespaces from the innermost out. At each it tries the namespace's own
// types, then the namespace's using directives, and stops at the first level that has an applicable
// candidate. golib's object form is in the root namespace and applies to everything, so it ends the
// walk there: a class is found only if it is in the root namespace itself, or in the file's own
// namespace or one that encloses it, or is imported by a using directive of a file that is not itself
// in the root namespace (a using directive is tried after the types of ITS OWN namespace, and the root
// namespace's types include golib's). A file in the root namespace, as every `main` package and every
// root-level stdlib package is, can therefore not see an overload in any nested namespace.
func (v *Visitor) assertOverloadIsReachable(pkg *types.Package) bool {
	if pkg == v.pkg {
		return true
	}

	// The class path reads "io_package" for a root package and "io.fs_package" for a nested one; the
	// namespace is everything before the class segment, under the root namespace.
	classPath := convertImportPathToNamespace(pkg.Path(), PackageSuffix)
	lastDot := strings.LastIndex(classPath, ".")

	if lastDot == -1 {
		return true
	}

	if packageNamespace == RootNamespace {
		return false
	}

	namespace := RootNamespace + "." + classPath[:lastDot]

	if packageNamespace == namespace || strings.HasPrefix(packageNamespace, namespace+".") {
		return true
	}

	for _, usings := range []hashset.HashSet[string]{v.requiredUsings, v.methodNamespaceUsings} {
		for using := range usings {
			if resolved, ok := resolveUsingNamespace(using); ok && resolved == namespace {
				return true
			}
		}
	}

	return false
}
