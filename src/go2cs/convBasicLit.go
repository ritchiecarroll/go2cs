// convBasicLit.go - Gbtc
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
	"go/constant"
	"go/token"
	"go/types"
	"math"
	"strconv"
	"strings"
)

// stringLiteralNeedsByteArray reports whether a Go interpreted (double-quoted) string literal token
// must be emitted as a byte-array-backed @string rather than a C# string/u8 literal. It scans the
// token's RAW-BYTE escapes — BOTH of Go's forms denote one raw byte: `\xHH` is EXACTLY two hex
// digits, `\NNN` EXACTLY three octal digits. Neither survives the C# code-UNIT escape it would
// otherwise render as (`\x` is a GREEDY 1-to-4-hex-digit escape; an octal becomes the `\uXXXX` of
// replaceOctalChars), and a C# `"…"u8` literal UTF-8-re-encodes its content. An escape forces the
// byte-array form when EITHER:
//
//	(1) its byte value is >= 0x80 — a C# string/u8 literal UTF-8-re-encodes such a byte to a
//	    multi-byte sequence (or, when a `\x` escape greedily forms a lone surrogate, fails to encode
//	    at all — CS9026), so @string byte indexing / len would not match Go. Go's `"\377"` is the
//	    single byte 0xFF, but the `ÿ` an octal escape renders as is the CHARACTER U+00FF, which
//	    encodes to the TWO bytes 0xC3 0xBF; or
//	(2) a `\xHH` escape is immediately followed by a third hex digit — C# greedily folds e.g.
//	    `\xdb50` into the single code unit U+DB50, changing the decoded content even if every
//	    resulting byte is ASCII. An octal escape has no such case: Go's is exactly three digits and
//	    C#'s `\uXXXX` exactly four, so neither side can extend into the following text.
//
// Only raw-byte ESCAPES are inspected: a literal written with actual UTF-8 characters (`"Michał"`,
// `"白鵬翔"`) round-trips exactly through C#'s `"…"u8` encoding and keeps the readable string form, as
// does a sub-0x80 escape with no greedy extension — image/jpeg's `"\x00\x10\x01\x11"u8[i]`, or an
// octal `"\101"` whose `A` is the same ASCII 'A'. Only raw-byte data expressed with high or
// greedy escapes (zip blobs, embedded tzdata) is routed to the byte array. A raw (backtick) literal
// has no escapes and never trips this (checked by caller).
func stringLiteralNeedsByteArray(token string) bool {
	if _, err := strconv.Unquote(token); err != nil {
		return false
	}

	backslashes := 0

	for i := 0; i < len(token); i++ {
		c := token[i]

		if c == '\\' {
			backslashes++
			continue
		}

		if (c == 'x' || c == 'X') && backslashes%2 == 1 && i+2 < len(token) && isHexDigit(token[i+1]) && isHexDigit(token[i+2]) {
			b := hexValue(token[i+1])<<4 | hexValue(token[i+2])

			if b >= 0x80 || (i+3 < len(token) && isHexDigit(token[i+3])) {
				return true
			}
		}

		if isOctalDigit(c) && backslashes%2 == 1 && i+2 < len(token) && isOctalDigit(token[i+1]) && isOctalDigit(token[i+2]) {
			b := int(c-'0')<<6 | int(token[i+1]-'0')<<3 | int(token[i+2]-'0')

			if b >= 0x80 {
				return true
			}
		}

		backslashes = 0
	}

	return false
}

// isOctalDigit reports whether b is an ASCII octal digit.
func isOctalDigit(b byte) bool {
	return b >= '0' && b <= '7'
}

// isHexDigit reports whether b is an ASCII hexadecimal digit.
func isHexDigit(b byte) bool {
	return (b >= '0' && b <= '9') || (b >= 'a' && b <= 'f') || (b >= 'A' && b <= 'F')
}

// hexValue returns the numeric value (0-15) of an ASCII hex digit; callers guard with isHexDigit.
func hexValue(b byte) int {
	switch {
	case b >= '0' && b <= '9':
		return int(b - '0')
	case b >= 'a' && b <= 'f':
		return int(b-'a') + 10
	default:
		return int(b-'A') + 10
	}
}

// emitByteArrayString decodes a Go interpreted string literal token to its exact byte sequence and
// emits a PARENTHESIZED byte-array-backed C# @string — `((@string)(new byte[]{ 0xNN, ... }))`. This
// is the faithful representation for a Go string holding raw bytes (see stringLiteralNeedsByteArray):
// it preserves the exact bytes that C#'s UTF-16 string literal / greedy `\x` escape would otherwise
// mangle, and the @string's byte indexing (`s[i]`) then matches Go. The outer parentheses are load-
// bearing: an INLINE-indexed literal (`"…"[i]`) would otherwise bind `[i]` to the inner `byte[]`
// (postfix `[]` outranks the cast), indexing the raw array instead of the @string. Returns
// ("", false) if the token cannot be decoded as a Go string literal (caller falls back to the
// ordinary string-literal path).
func emitByteArrayString(token string) (string, bool) {
	decoded, err := strconv.Unquote(token)

	if err != nil {
		return "", false
	}

	return byteArrayStringLiteral(decoded), true
}

// byteArrayStringLiteral emits a byte-array-backed C# @string holding the EXACT bytes of `decoded` —
// `((@string)(new byte[]{ 0xNN, ... }))`. Used for a Go string holding raw bytes a C# string/u8
// literal would mangle (byte tables, blobs); the @string's byte indexing (`s[i]`) then matches Go.
// See emitByteArrayString (from a literal token) and the const-string path (from a folded value).
func byteArrayStringLiteral(decoded string) string {
	builder := &strings.Builder{}
	builder.WriteString("((@string)(new byte[]{")

	for i := 0; i < len(decoded); i++ {
		if i > 0 {
			builder.WriteString(", ")
		}

		fmt.Fprintf(builder, "0x%02x", decoded[i])
	}

	builder.WriteString("}))")

	return builder.String()
}

// replaceOctalChars rewrites every Go octal escape (`\NNN` — EXACTLY three digits) in a literal
// TOKEN to the C# code-unit escape for the same value. It is the shared step for both the
// token.STRING and the token.CHAR paths.
//
// Backslash PARITY decides what is an escape, exactly as in stringLiteralNeedsByteArray. In
// `"\\377"` the second backslash is escaped BY the first, so `377` is ordinary text and the Go
// value is the FOUR characters `\377`. A plain regex scan matched from that second backslash and
// rewrote the literal to `"\\u00ff"` — whose C# value is the SIX characters `ÿ`: the wrong
// content, the wrong length, and silently so. The scan is also positional: the old
// regex + strings.Replace pair replaced the first TEXTUAL occurrence of each match, so a literal
// carrying both forms could rewrite the wrong one.
//
// Three octal digits cap at `\777` = 0x1FF, so the C# `\uXXXX` code-unit escape always suffices.
func replaceOctalChars(value string) string {
	if !strings.Contains(value, "\\") {
		return value
	}

	builder := &strings.Builder{}
	backslashes := 0

	for i := 0; i < len(value); i++ {
		c := value[i]

		if c == '\\' {
			backslashes++
			builder.WriteByte(c)
			continue
		}

		if backslashes%2 == 1 && isOctalDigit(c) && i+2 < len(value) && isOctalDigit(value[i+1]) && isOctalDigit(value[i+2]) {
			// The opening backslash is already written and C#'s escape opens with one too, so only
			// the `uXXXX` tail is appended.
			fmt.Fprintf(builder, "u%04x", int(c-'0')<<6|int(value[i+1]-'0')<<3|int(value[i+2]-'0'))
			i += 2
			backslashes = 0
			continue
		}

		builder.WriteByte(c)
		backslashes = 0
	}

	return builder.String()
}

// floatLiteralSourceText returns the SOURCE text of a float-constant initializer that is a
// plain literal — an ast.BasicLit, optionally under a single unary sign (`-7.05306122448979611050e-01`
// parses as a UnaryExpr over the literal) — or "" when the initializer is a folded expression
// with no single literal form.
func floatLiteralSourceText(expr ast.Expr) string {
	sign := ""

	if unary, ok := expr.(*ast.UnaryExpr); ok {
		switch unary.Op {
		case token.SUB, token.ADD:
			sign = unary.Op.String()
			expr = unary.X
		default:
			return ""
		}
	}

	if lit, ok := expr.(*ast.BasicLit); ok && lit.Kind == token.FLOAT {
		return sign + lit.Value
	}

	return ""
}

// isValidCSharpRealLiteral reports whether a Go decimal float literal's source text is ALSO a
// valid C# real-literal, so it can be emitted verbatim. Go and C# share the decimal forms —
// digits with optional fraction, `e`/`E` exponent, and `_` digit separators (both languages
// restrict separators to between digits) — but Go additionally allows hex floats (`0x1p-2`)
// and a bare trailing dot (`5.`, `5.e2`), which C# does not.
func isValidCSharpRealLiteral(lit string) bool {
	body := lit

	if len(body) > 0 && (body[0] == '+' || body[0] == '-') {
		body = body[1:]
	}

	if len(body) == 0 {
		return false
	}

	if len(body) > 1 && body[0] == '0' {
		// Go-only radix prefixes: hex (also hex FLOATS, `0x1p-2`), octal `0o…`, binary `0b…` —
		// none lex as a C# real literal once the D/F suffix is appended (`0b101D` is CS1003;
		// `0xabcD` is worse — a VALID hex integer with D as a digit). An imaginary-literal
		// MANTISSA may be any Go int literal, which is how these reach a float context.
		if c := body[1]; c == 'x' || c == 'X' || c == 'o' || c == 'O' || c == 'b' || c == 'B' {
			return false
		}

		// LEGACY leading-zero forms (`0123`, `0_123`): octal-flavored Go int-literal source that
		// C# would re-read as decimal — re-render as the exact decimal value instead, mirroring
		// preserveGoIntLiteral's rule for the integer arm. (`0.25`/`0e5` pass through — '.'/'e'
		// are not digits.)
		if body[1] == '_' || (body[1] >= '0' && body[1] <= '9') {
			return false
		}
	}

	if i := strings.IndexByte(body, '.'); i != -1 && (i+1 >= len(body) || body[i+1] < '0' || body[i+1] > '9') {
		return false
	}

	return true
}

// exactFloatConstString returns the EXACT C# value text for a float-kind constant declaration.
// go/constant's Value.String() is a SHORTENED human-readable form (~6 significant digits), so
// emitting it silently truncated the COMPILED value (math cbrt's `C = 0.542857` — the exact
// 5.42857142857142815906e-01 survived only in the `/* … */` comment). Preference order:
//  1. The Go source literal VERBATIM when it is also valid C# syntax and parses to the same
//     value — the declaration then reads exactly like the Go source, and the comment-elision
//     check sees constVal == orgExpr and drops the now-redundant `/* original */` comment.
//  2. The shortest round-trip form (strconv.FormatFloat 'g'/-1) at the declaration's width —
//     bitSize 32 for a float32-typed const (the appended `f` suffix then parses with the same
//     single rounding Go applies converting the exact constant), 64 otherwise. This covers
//     folded const expressions and Go-only literal forms (hex floats, trailing-dot).
//
// A beyond-float64 value keeps the shortened String() form so the GoBigConst overflow path
// (strconv.ParseFloat fails → writeUntypedConst) still triggers exactly as before.
func exactFloatConstString(val constant.Value, source ast.Expr, isFloat32 bool) string {
	lit := ""

	if source != nil {
		lit = floatLiteralSourceText(source)
	}

	return exactFloatText(val, lit, isFloat32)
}

// exactFloatText is the text-based core of exactFloatConstString, shared with the float and
// imaginary literal arms of convBasicLit (which hold the literal's source text directly rather
// than an initializer expression). It returns `lit` verbatim when that text is valid C# syntax
// denoting the same value, and otherwise the shortest round-trip decimal at the value's width.
// Passing lit == "" forces the re-rendered form.
//
// The float32 width rounds the EXACT constant straight to float32 (constant.Float32Val), never
// float64-then-narrow: an exact hex-float mantissa that is not representable in either width
// would otherwise round twice and can land a ULP off Go's single rounding.
func exactFloatText(val constant.Value, lit string, isFloat32 bool) string {
	f64, _ := constant.Float64Val(val)

	if math.IsInf(f64, 0) {
		return val.String()
	}

	bitSize := 64
	target := f64

	if isFloat32 {
		bitSize = 32
		f32, _ := constant.Float32Val(val)
		target = float64(f32)
	}

	if lit != "" && isValidCSharpRealLiteral(lit) {
		if parsed, err := strconv.ParseFloat(lit, bitSize); err == nil && parsed == target {
			return lit
		}
	}

	return strconv.FormatFloat(target, 'g', -1, bitSize)
}

// exactComplexConstString returns the EXACT C# value text for a COMPLEX-kind constant declaration
// and reports whether the value is representable at the declaration's width.
//
// go/constant's ExactString for a complex value is `(re + imi)` with RATIONAL parts — `5.5+1.5i`
// renders as `(11/2 + 3/2i)` — which is neither C# syntax nor a form strconv.ParseComplex accepts
// (its grammar is Go *literal* syntax: no parentheses, no spaces around the sign, no `p/q`). So
// testing that whole text with ParseComplex was a representability check that could NEVER pass, and
// every complex constant — however ordinary — was misclassified as beyond-complex128 and emitted
// through the GoBigConst arm, whose BigInteger.Parse cannot hold a complex at all (strconv
// atoc_test's `const want = 1.5e308 + 1.0e307i` → CS0019 comparing GoBigConst to a complex128).
//
// Each HALF is rendered and range-tested exactly like a float const (exactFloatText, which handles
// the rational and hex-float forms), then recombined in the postfix `.i()` form convBasicLit's IMAG
// arm uses for written imaginary literals — `1.5e+308D + 1e+307D.i()`. The receiver's F/D suffix
// selects the golib i() overload (complex64 vs complex128), same as a literal's, and the halves'
// implicit float→complex conversion closes the `+`.
func exactComplexConstString(val constant.Value, isComplex64 bool) (string, bool) {
	suffix := "D"
	bitSize := 64

	if isComplex64 {
		suffix = "F"
		bitSize = 32
	}

	reText := exactFloatText(constant.Real(val), "", isComplex64)
	imText := exactFloatText(constant.Imag(val), "", isComplex64)

	if _, err := strconv.ParseFloat(reText, bitSize); err != nil {
		return "", false
	}

	if _, err := strconv.ParseFloat(imText, bitSize); err != nil {
		return "", false
	}

	// A zero real part renders as the bare imaginary literal — `2D.i()`, exactly the Go source
	// form of `const c = 2i` (the `0D + ` prefix would be noise).
	if reText == "0" {
		return imText + suffix + ".i()", true
	}

	// A NEGATIVE imaginary part composes correctly as written: member invocation binds tighter
	// than unary minus, so `re + -imD.i()` is re + -(im·i) — the value Go wrote, sign included
	// (see convBasicLit's IMAG arm).
	return reText + suffix + " + " + imText + suffix + ".i()", true
}

// goFloatLiteralText returns the C# real-literal text for a Go float literal's SOURCE text (for
// an imaginary literal, its mantissa — the trailing `i` already stripped). A form C# also accepts
// is kept VERBATIM, preserving the developer's own formatting per preserveGoIntLiteral's goal
// (`1.5e-3` must not flatten to `0.0015`) and leaving every literal that compiles today byte-
// identical. The Go-only forms C# cannot parse re-render as the shortest round-trip decimal at
// the literal's RESOLVED width:
//
//   - a hex float (`0x1p-2`) — C# has no hex-float syntax at all. With an INTEGER mantissa
//     (`0x10i`) the pasted-on suffix is worse than a syntax error: `0x10D` is a valid C# HEX
//     INTEGER (269), so the emission silently changed the value.
//   - a `.` not followed by a digit (`2.`, `1.e2`) — C# requires fractional digits after the
//     decimal point.
//
// `val` is the go/types-folded constant — the authoritative value, already rounded as Go rounds
// an untyped constant to its target type. When it is unavailable the source text is parsed
// directly; strconv accepts every Go float literal form, including hex floats.
func goFloatLiteralText(lit string, val constant.Value, isFloat32 bool) string {
	if isValidCSharpRealLiteral(lit) {
		return lit
	}

	if val != nil {
		return exactFloatText(val, "", isFloat32)
	}

	bitSize := 64

	if isFloat32 {
		bitSize = 32
	}

	if parsed, err := strconv.ParseFloat(lit, bitSize); err == nil {
		return strconv.FormatFloat(parsed, 'g', -1, bitSize)
	}

	return lit
}

// preserveGoIntLiteral returns the literal's SOURCE text when it is also a valid C# integer
// literal denoting the same value — hex `0x…`, binary `0b…`, and decimal, each with optional
// `_` digit separators — keeping the developer's own formatting (the visually-similar goal:
// `0x4000` must not flatten to `16384`). Go-only forms re-render as the decimal fallback:
// `0o…` octal has no C# syntax, and a LEGACY leading-zero octal (`0755`) would silently
// re-bind as decimal 755 in C#.
func preserveGoIntLiteral(source string, decimal string) string {
	if len(source) > 1 && source[0] == '0' {
		if c := source[1]; c == 'x' || c == 'X' || c == 'b' || c == 'B' {
			return source
		}

		return decimal
	}

	return source
}

// intLiteralFloatKind reports the floating (or complex) basic Kind an integer literal has been
// resolved to, and whether it was — so convBasicLit can render it as a C# floating literal (F/D
// suffix) rather than a bare integer that would either overflow a C# integral type (a value beyond
// int64/uint64) or bind an int-typed overload. A literal reaches a float type two ways, checked in
// order:
//
//	(1) go/types typed it DIRECTLY — a float-typed composite-literal element, typed const, argument,
//	    return, or assignment — recorded in info.Types as a NON-untyped float/complex (a NAMED type
//	    over float64 resolves through Underlying to its float64 Kind, matching the FLOAT case).
//	(2) an enclosing constant expression PROPAGATED a float context into it (untypedConstContexts —
//	    complex()/min/max/arithmetic operands, see markUntypedConstContexts); the literal itself
//	    stays untyped, so route (1) skips it and the recorded context supplies the Kind.
//
// Returns (0, false) for an ordinary integer-typed or integer-context literal (the common path).
func (v *Visitor) intLiteralFloatKind(basicLit *ast.BasicLit) (types.BasicKind, bool) {
	if tv, ok := v.info.Types[basicLit]; ok && tv.Type != nil {
		if basic, ok := tv.Type.Underlying().(*types.Basic); ok &&
			basic.Info()&types.IsUntyped == 0 && basic.Info()&(types.IsFloat|types.IsComplex) != 0 {
			return basic.Kind(), true
		}
	}

	if cc := v.untypedConstContext(basicLit); cc != nil && cc.Info()&(types.IsFloat|types.IsComplex) != 0 {
		return cc.Kind(), true
	}

	return 0, false
}

// intLiteralResolvedInteger returns the concrete INTEGER basic type an integer literal resolved
// to, or nil when it stayed untyped with no recoverable context. It is what decides whether a
// beyond-int32 literal is a C# `long` (Go int64 — no cast needed) or a C# `nint` (Go int — cast
// needed), and it must consult BOTH routes for the same reason intLiteralFloatKind does:
//
//	(1) go/types typed it DIRECTLY — a composite-literal element, typed const, argument, return
//	    or assignment records the concrete type in info.Types (a NAMED type over int64 resolves
//	    through Underlying to its int64 Kind).
//	(2) go/types deliberately left it UNTYPED because it is an operand of a CONSTANT expression.
//	    updateExprType0 short-circuits on `if x is a constant, the operands were constants` —
//	    those operands never materialize at runtime in Go, so it does not descend into them. The
//	    consequence is invisible until you look: in `[...]int64{-4181792142133755926, 1395769623340756751}`
//	    the NEGATED element records `untyped int` while its positive sibling records `int64`,
//	    purely because the first is wrapped in a unary minus. The propagated context
//	    (markUntypedConstContexts, which already pushes an integer context through unary +/-/^ and
//	    arithmetic operands) carries the type go/types dropped.
//
// Route (2) is what made all 607 of math/rand's `rngCooked` elements — every one of them
// negative — emit an `int`-typed `(nint)` cast inside a `new int64[]{…}`, a value that TRUNCATES
// on a 32-bit target (CS8778).
func (v *Visitor) intLiteralResolvedInteger(basicLit *ast.BasicLit) *types.Basic {
	if tv, ok := v.info.Types[basicLit]; ok && tv.Type != nil {
		if basic, ok := tv.Type.Underlying().(*types.Basic); ok &&
			basic.Info()&types.IsUntyped == 0 && basic.Info()&types.IsInteger != 0 {
			return basic
		}
	}

	if cc := v.untypedConstContext(basicLit); cc != nil && cc.Info()&types.IsInteger != 0 {
		return cc
	}

	return nil
}

func (v *Visitor) convBasicLit(basicLit *ast.BasicLit, context BasicLitContext) string {
	result := &strings.Builder{}
	value := basicLit.Value

	switch basicLit.Kind {
	case token.INT:
		// An int literal RESOLVED to a floating (or complex) type must render as a C# FLOATING
		// literal, not a bare integer. Two routes reach a float type:
		//   (1) go/types typed it DIRECTLY — a float-typed composite-literal element, typed const,
		//       function argument, return, or assignment (`{float64: 123456789123456789123456789}`);
		//       the literal is recorded as a non-untyped float/complex in info.Types.
		//   (2) an enclosing constant expression PROPAGATED a float context into it — a complex()
		//       element, a min/max arg, or a float-constant arithmetic operand (see
		//       markUntypedConstContexts); the literal stays untyped and the context is recorded in
		//       untypedConstContexts.
		// A bare integer literal is wrong on both routes. A LARGE value overflows every C# integral
		// type (strconv ftoa_test's `123456789123456789123456789` against a `float float64` field →
		// CS1021 "Integral constant is too large"), and even a small value binds int-typed overloads
		// rather than the float ones: `complex(0, math.Pi/2)` in a complex128 context must pick
		// golib's complex(float64, float64), not complex(float32, float32) — C# rates int->float a
		// better argument conversion than int->double, so a bare `0` drags the call onto the float32
		// overload and recomputes math.Pi/2 at float32, silently losing precision. The FLOAT/IMAG
		// cases below make the analogous F/D choice; an int literal has no fractional part, so its
		// digits plus the suffix suffice. The Go SOURCE digits are preserved verbatim when they also
		// form a valid C# real literal (the visually-similar goal: `123456789123456789123456789D` is
		// a valid C# double literal that rounds to the SAME float64 the bare form overflowed on — so
		// the emitted digits still read like the Go source). A hex/octal/binary/legacy-leading-zero
		// form cannot survive a suffix (`0x10D` is the C# HEX integer 269), so those re-render as the
		// exact DECIMAL digits of the folded constant (constant.ToInt.ExactString) — mirroring
		// isValidCSharpRealLiteral's rejection of radix prefixes for the FLOAT case.
		if floatKind, ok := v.intLiteralFloatKind(basicLit); ok {
			if tv, ok := v.info.Types[basicLit]; ok && tv.Value != nil {
				if ival := constant.ToInt(tv.Value); ival.Kind() == constant.Int {
					digits := ival.ExactString()

					if isValidCSharpRealLiteral(value) {
						digits = value
					}

					switch floatKind {
					case types.Float32, types.Complex64:
						return digits + "F"
					case types.Float64, types.Complex128:
						return digits + "D"
					}
				}
			}
		}

		// Parse literal octal, binary, etc as a decimal integer (the VALUE classifies the
		// emitted form below; the TEXT keeps the Go source formatting where C# supports it)
		if intval, err := strconv.ParseInt(value, 0, 64); err == nil {
			value = preserveGoIntLiteral(value, strconv.FormatInt(intval, 10))

			if intval > math.MaxInt32 || intval < math.MinInt32 {
				// A value outside int32 used in an unsigned context (e.g. 0x80000000
				// passed to a uint32 parameter) must emit an unsigned C# literal — a
				// signed (nint)…L does not convert to uint/uint32.
				if intval >= 0 && v.isUnsignedType(basicLit) {
					if intval > math.MaxUint32 {
						// Same native-width rule as the unsigned-parse branch below, and it is
						// called rather than restated so the two cannot disagree again — they
						// already did, which is defect D: this branch (a value that parses SIGNED,
						// i.e. at or below MaxInt64) emitted a bare `UL` for every unsigned context,
						// so `var word uintptr = 0x0102030405060708` became `word = …UL`. A ulong
						// has only an EXPLICIT operator to golib's uintptr, so it is CS0266 — while
						// the branch below, reached only ABOVE MaxInt64, had the rule right and
						// documented it.
						result.WriteString(v.nativeWidthUnsignedPrefix(basicLit))
						result.WriteString(value)
						result.WriteString("UL")
					} else {
						result.WriteString(value)
						result.WriteRune('U')
					}
				} else if resolved := v.intLiteralResolvedInteger(basicLit); resolved != nil && resolved.Kind() == types.Int64 {
					// Go int64 IS C# long, so the literal alone denotes it exactly — `4181792142133755926L`.
					// The `(nint)` cast this branch used to emit unconditionally was the untyped-int
					// DEFAULT type leaking through (see intLiteralResolvedInteger): wrong for the
					// declared element type, and a 32-bit truncation the compiler flags as CS8778.
					// Dropping it also reads closer to the Go source, which is the point.
					result.WriteString(value)
					result.WriteRune('L')
				} else {
					// Go `int` (and the untyped-int default, which is `int`) is C# `nint`, which has
					// NO implicit conversion from `long` — the cast stays, and it must stay for
					// boxing too (an `any` slot holding a Go int has to box as nint so a later
					// `x.(int)` succeeds). `unchecked` is what makes the beyond-int32 CONSTANT
					// conversion legal without a warning; nint is 64-bit on every platform go2cs
					// targets, so the value is exact at runtime. Same form visitValueSpec already
					// emits for a beyond-int32 native-int const.
					result.WriteString("unchecked((nint)")
					result.WriteString(value)
					result.WriteString("L)")
				}
			} else {
				result.WriteString(value)
			}
		} else if uintval, err := strconv.ParseUint(value, 0, 64); err == nil {
			value = preserveGoIntLiteral(value, strconv.FormatUint(uintval, 10))

			if uintval > math.MaxUint32 {
				// A literal above MaxInt64 only parses here, and its resolved type picks
				// the emitted form: a uint64 context (incl. named types over uint64 — their
				// [GoType] wrappers convert implicitly from ulong) takes the plain UL
				// literal — math.Float64frombits(0xFFF0000000000000). A (nuint) prefix
				// there is spurious: semantically wrong for a 64-bit target type, and
				// truncating on a 32-bit platform. Only a native-width unsigned context
				// (uint/uintptr -> C# nuint) keeps the (nuint) cast — a bare ulong literal
				// has no implicit conversion to nuint (CS0266); the non-constant unchecked
				// (nuint) conversion does compile.
				result.WriteString(v.nativeWidthUnsignedPrefix(basicLit))
				result.WriteString(value)
				result.WriteString("UL")
			} else {
				result.WriteString(value)
				result.WriteRune('U')
			}
		} else {
			v.showWarning("Failed to parse integer literal as a 64-bit signed or unsigned int: %s", value)
			result.WriteString(value)
		}
	case token.FLOAT:
		// The C# suffix must reflect the literal's resolved type, not merely whether the value
		// fits in float32. A Go untyped float constant defaults to float64, so emitting `F`
		// (float32) whenever it fits corrupts inferred types — `z := 1.0` would become a float32,
		// and subsequent float64 arithmetic on it fails (CS0266). Emit `F` only when go/types
		// resolves the literal as float32; otherwise `D` (double, matching Go's float64 default).
		// And when an integer-valued float constant is used in an integer context — `math.Inf(1.0)`,
		// where Inf takes an int — emit the integer form (`1`), not `1.0D` (which is CS1503).
		// A literal INSIDE a constant expression (`var b float32 = -3.5`, `complex(2.5, -3.5)`
		// in a complex64 context) stays recorded UNTYPED — go/types resolves the context on the
		// outermost expression only — so its float32-ness comes from the propagated context (see
		// markUntypedConstContexts); a complex64 context makes the literal a float32 operand too.
		isFloat32 := false
		intForm := ""

		var constVal constant.Value

		if tv, ok := v.info.Types[basicLit]; ok && tv.Type != nil {
			constVal = tv.Value

			if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
				if basic.Info()&types.IsInteger != 0 && tv.Value != nil {
					intForm = tv.Value.ExactString()
				} else if basic.Kind() == types.Float32 || basic.Kind() == types.Complex64 {
					// Complex64 for the same reason the propagated arm below takes it: a complex64
					// is TWO float32 components, so its float operand is float32 and wants `F`.
					// Reading the type's TOTAL width instead emits `D`, and golib's
					// double→complex64 conversion is EXPLICIT (float→complex64 is implicit), so a
					// bare literal does not bind — CS0266 on reflect's `[]complex64{1.414}`, while
					// `[]complex128{1.414}` on the line after it compiles precisely because ITS
					// components are float64.
					//
					// The untyped arm below already applied this rule; a composite-literal ELEMENT
					// never reaches it, because go/types assigns element types DIRECTLY rather than
					// leaving them untyped for markUntypedConstContexts to propagate.
					isFloat32 = true
				} else if basic.Info()&types.IsUntyped != 0 {
					if constContext := v.untypedConstContext(basicLit); constContext != nil {
						switch constContext.Kind() {
						case types.Float32, types.Complex64:
							isFloat32 = true
						}

						// A float literal INSIDE a constant expression resolved to an INTEGER
						// context takes the same integer form as the directly-typed case above
						// (`1e9 - 7` against an `int` field renders `1000000000 - 7`, keeping
						// the C# arithmetic `int` — see propagateUntypedConstContext). ToInt
						// guards exactness: a non-integral literal keeps its loud float form.
						if constContext.Info()&types.IsInteger != 0 && tv.Value != nil {
							if ival := constant.ToInt(tv.Value); ival.Kind() == constant.Int {
								intForm = ival.ExactString()
							}
						}
					}
				}
			}
		}

		if intForm != "" {
			result.WriteString(intForm)
		} else if isFloat32 {
			result.WriteString(goFloatLiteralText(value, constVal, true))
			result.WriteRune('F')
		} else {
			result.WriteString(goFloatLiteralText(value, constVal, false))
			result.WriteRune('D')
		}
	case token.IMAG:
		endsWith_i := strings.HasSuffix(value, "i")

		if endsWith_i {
			value = strings.TrimSuffix(value, "i")
		}

		// For complex literals, we use the golib `i()` extension method in POSTFIX form —
		// `3.5D.i()` — the closest C# rendering of Go's `3.5i`. Member access cannot be
		// shadowed by a local: `i` is the single most common Go loop/receiver variable, and a
		// bare `i(…)` call binds a local named `i` instead of the using-static import (encoding/
		// gob encComplex's `i *encInstr` parameter, `c != 0+0i` → CS0149 "Method name expected";
		// C# scope rules make even a LATER-declared local poison an earlier bare call —
		// CS0135/CS0844). The prior solution was the class-qualified `builtin.i(…)`; postfix
		// member access is equally shadow-immune with zero scope analysis (context-independent
		// output) and reads closer to the Go literal. The receiver's F/D suffix — emitted
		// UNCONDITIONALLY, so the receiver is always a real literal and `.i()` always lexes as
		// member access — selects the golib extension OVERLOAD: i(this float) returns complex64,
		// i(this double) returns complex128. It must reflect the literal's RESOLVED complex type,
		// not whether the value happens to fit in float32 (the old heuristic routed `0.1i` in a
		// complex128 context through complex64, silently losing precision). Like the FLOAT case
		// above, a literal inside a constant expression stays recorded untyped and takes its
		// complex64-ness from the propagated context (see markUntypedConstContexts); the untyped
		// default (complex128) emits D. Negation composes correctly: member invocation binds
		// tighter than unary minus, so `-3.5D.i()` is -(3.5i) — matching Go, down to the
		// negative-zero real part.
		isComplex64 := false

		// An imaginary literal whose RESOLVED type is a REAL float (not complex): Go permits an
		// untyped complex constant with a ZERO imaginary part (`0i`, value 0) to convert to a float
		// parameter — `complex(math.NaN(), 0i)`, where complex()'s second parameter is float64
		// (internal/fmtsort's sort_test.go). go/types records the literal's type as that float, and
		// its REAL part (0) is what must be emitted: a `.i()` imaginary would be a compile error
		// (CS1503, Complex not assignable to the float parameter). Only `0i` can reach this arm — a
		// nonzero imaginary constant is not representable as a real float, so go/types never records
		// one with a float type — but the emission is driven off the resolved type, not that fact.
		resolvedFloat := false
		resolvedFloat32 := false

		// The mantissa's own value — the folded constant is COMPLEX (`0x1p-2i` → 0+0.25i), so the
		// text handed to goFloatLiteralText must be matched against its IMAGINARY part, not the
		// whole complex value. In the real-context arm it is matched against the REAL part instead.
		var mantissaVal constant.Value
		var realVal constant.Value

		if tv, ok := v.info.Types[basicLit]; ok && tv.Type != nil {
			if tv.Value != nil {
				mantissaVal = constant.Imag(tv.Value)
				realVal = constant.Real(tv.Value)
			}

			if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
				switch {
				case basic.Kind() == types.Complex64:
					isComplex64 = true
				case basic.Info()&types.IsFloat != 0:
					resolvedFloat = true
					resolvedFloat32 = basic.Kind() == types.Float32
				case basic.Info()&types.IsUntyped != 0:
					if constContext := v.untypedConstContext(basicLit); constContext != nil {
						switch constContext.Kind() {
						case types.Complex64, types.Float32:
							isComplex64 = true
						}
					}
				}
			}
		}

		if !endsWith_i {
			result.WriteString(value)
		} else if resolvedFloat && resolvedFloat32 {
			result.WriteString(goFloatLiteralText(value, realVal, true))
			result.WriteRune('F')
		} else if resolvedFloat {
			result.WriteString(goFloatLiteralText(value, realVal, false))
			result.WriteRune('D')
		} else if isComplex64 {
			result.WriteString(fmt.Sprintf("%sF.i()", goFloatLiteralText(value, mantissaVal, true)))
		} else {
			result.WriteString(fmt.Sprintf("%sD.i()", goFloatLiteralText(value, mantissaVal, false)))
		}
	case token.CHAR:
		value = replaceOctalChars(value)
		intVal, err := strconv.Atoi(value)

		if err == nil {
			if intVal <= 0xFFFF {
				// Character can be represented as a char in C# Rune. QuoteRune escapes control
				// and special characters (`'\t'`, `'\n'`, `'\\'` — Go's escapes are all valid C#
				// char escapes for BMP runes); the raw `%c` form emitted literal control bytes,
				// and a raw newline inside a char literal does not even parse (CS1010).
				result.WriteString(fmt.Sprintf("(rune)%s", strconv.QuoteRune(rune(intVal))))
			} else {
				// For characters beyond BMP, we can use the direct code point
				result.WriteString(fmt.Sprintf("0x%X", intVal))
			}
		} else {
			// A QUOTED rune literal beyond the BMP ('\U0001D504') cannot be a C# char
			// literal (html's entity table, CS1012 ×133) — emit the code point instead.
			// BMP literals keep their source text verbatim (zero churn).
			emitted := false

			if len(value) >= 2 && value[0] == '\'' && value[len(value)-1] == '\'' {
				if r, _, _, uerr := strconv.UnquoteChar(value[1:len(value)-1], '\''); uerr == nil && r > 0xFFFF {
					result.WriteString(fmt.Sprintf("(rune)0x%X", r))
					emitted = true
				}
			}

			if !emitted {
				result.WriteString(fmt.Sprintf("(rune)%s", value))
			}
		}
	case token.STRING:
		// A Go interpreted string literal that carries a `\xHH` raw-byte escape is binary data
		// (zip blobs, embedded tzdata). C#'s `\x` escape is greedy (1-4 hex digits) and a C# UTF-16
		// string re-encodes bytes >= 0x80 to two UTF-8 bytes, so re-emitting the token as a C#
		// string literal both mis-parses (`\xdb50` -> lone surrogate U+DB50, CS9026) and corrupts
		// the bytes. Emit these as a byte-array-backed @string so the exact bytes are preserved and
		// @string byte indexing matches Go. Text-only literals keep the readable string form.
		if !strings.HasPrefix(value, "`") && stringLiteralNeedsByteArray(value) {
			if byteArray, ok := emitByteArrayString(value); ok {
				result.WriteString(byteArray)
				break
			}
		}

		strVal, isRawStr := v.getStringLiteral(value)

		if !isRawStr {
			strVal = replaceOctalChars(strVal)
		}

		if context.sourceIsRuneArray || context.castToGoString {
			result.WriteString("(@string)")
		}

		result.WriteString(strVal)

		if context.u8StringOK {
			result.WriteString("u8")
		}
	}

	return result.String()
}

// nativeWidthUnsignedPrefix returns the cast an above-MaxUint32 unsigned integer literal needs so it
// reaches its RESOLVED type, or "" when the bare `UL` literal already does.
//
// A literal above MaxUint32 emits with a `UL` suffix, which makes it a C# `ulong`. Where the resolved
// Go type is uint64, that is exactly right and a cast would be worse than redundant: golib's [GoType]
// wrappers over uint64 convert implicitly from ulong, and a `(nuint)` there is semantically wrong for
// a 64-bit target and TRUNCATES on a 32-bit platform — `math.Float64frombits(0xFFF0000000000000)` is
// the row that says so.
//
// Where the resolved type is NATIVE-WIDTH unsigned (Go `uint`/`uintptr` -> C# `nuint`), the bare
// ulong does NOT reach it: golib's uintptr takes nuint, uint8/16/32, char, UntypedInt and NilType
// IMPLICITLY and uint64 only EXPLICITLY, so `word = 0x0102030405060708UL` is CS0266. The `(nuint)`
// cast is a non-constant unchecked conversion, which compiles, and uintptr's implicit nuint operator
// then binds.
//
// It exists as ONE function because the two literal branches that need it — the signed parse (at or
// below MaxInt64) and the unsigned parse (above it) — had DRIFTED: only the second carried the rule,
// so the defect showed up exactly in the range the first one owns. Two derivations of one predicate
// is how that happens, so there is now one.
func (v *Visitor) nativeWidthUnsignedPrefix(basicLit *ast.BasicLit) string {
	basic, isBasic := v.getType(basicLit, true).(*types.Basic)

	if isBasic && basic.Kind() == types.Uint64 {
		return ""
	}

	return "(nuint)"
}
