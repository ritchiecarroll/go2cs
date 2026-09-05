// visitChanType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

// Handles channel types in context of a TypeSpec
func (v *Visitor) visitChanType(chanType *ast.ChanType, identType types.Type, name string) {
	// A defined channel type — `type closeWaiter chan struct{}` (net/http's h2 bundle) — emits
	// the `[GoType("chan T")] partial struct` forward declaration whose Channel template
	// go2cs-gen implements (the wrapper holds a `channel<T>` and forwards its send/receive/
	// select surface). The old stub emitted only a comment, leaving the type undeclared
	// (CS0246). The descriptor is direction-agnostic: golib's `channel<T>` does not model
	// `<-chan`/`chan<-` restrictions, so a directional named type wraps the same surface.
	elemType := convertToCSTypeName(v.getAliasQualifiedTypeName(v.info.TypeOf(chanType.Value), false))
	access := v.consumePendingTypeAccess()

	// A channel type declared inside a function body cannot be a method-body statement in C#; hoist
	// it to member level (see liftLocalTypeDecl). A package-level declaration is unaffected — target
	// is v.outputBuilder and finish() is a no-op.
	preLiftName := name
	name, target, finish := v.liftLocalTypeDecl(name, identType)

	// The element was resolved ABOVE, i.e. before this declaration's own hoist registered its
	// lifted name — so a SELF-referential local type (`type C chan C`) rendered the source name,
	// which no longer exists at member level. Re-resolve through liftedTypeMap whenever a hoist
	// actually renamed the declaration; a package-level declaration never renames, so its
	// emission is unchanged.
	if name != preLiftName {
		if elemGoType := v.info.TypeOf(chanType.Value); elemGoType != nil {
			if lifted, ok := v.liftedTypeMap[elemGoType]; ok {
				elemType = lifted
			}
		}
	}

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	v.recordTypeAccessibility("struct", getSanitizedIdentifier(name), "", access, "")
	// A DIRECTIONAL defined channel type carries its direction chain on the wrapper as TYPE-level descriptor
	// cargo (increment E3 follow-up 7e-b): the marker stays `chan T` -- go2cs-gen dispatches the Channel
	// template on that prefix -- and `[GoChanDir(...)]` beside it is what reflect's descriptor reads back,
	// so `type R <-chan T` answers ChanDir() RecvDir and ConvertibleTo refuses what Go refuses. The chain
	// walks the UNDERLYING channel (chanDirChain refuses a Named type by design); a bidirectional type
	// stamps nothing, so no existing emission moves (census: production 0 named directional channel types).
	dirAttr := ""

	if chain := chanDirChain(identType.Underlying()); len(chain) > 0 {
		members := make([]string, len(chain))

		for i, dir := range chain {
			members[i] = goChanDirMember(dir)
		}

		dirAttr = "[GoChanDir(" + strings.Join(members, ", ") + ")] "
	}

	v.writeStringLn(target, "%s[GoType(\"chan %s\")] %s%spartial struct %s;", v.localNameAttrFor(identType), elemType, dirAttr, access, getSanitizedIdentifier(name))
	finish()
}

// namedChanElemTypeArg returns an explicit type-argument list ("<T>") for a golib channel-op
// free-function call (`ᐸꟷ(ch)`, `close(ch)`) whose operand's static type is a NAMED channel
// type: the `[GoType("chan T")]` wrapper struct reaches `channel<T>` only through a
// user-defined conversion, which C# generic inference never considers (CS0411) — naming the
// element type explicitly lets the wrapper's implicit conversion apply at the argument
// instead. A plain `chan T` operand returns "" (emission unchanged, byte-identical).
func (v *Visitor) namedChanElemTypeArg(expr ast.Expr) string {
	named, ok := types.Unalias(v.getExprType(expr)).(*types.Named)

	if !ok {
		return ""
	}

	chanType, ok := named.Underlying().(*types.Chan)

	if !ok {
		return ""
	}

	return fmt.Sprintf("<%s>", convertToCSTypeName(v.getAliasQualifiedTypeName(chanType.Elem(), false)))
}
