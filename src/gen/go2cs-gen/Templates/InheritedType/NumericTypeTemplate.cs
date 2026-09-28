// NumericTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go2cs.Templates.InheritedType;

internal static class NumericTypeTemplate
{
    private static bool IsUnsignedType(string typeName) =>
        typeName is "uint8" or "uint16" or "uint32" or "uint64" or "byte" or "rune" or "uintptr" or "nuint" or "uint";

    public static string Generate(string typeName, string targetTypeName) =>
        $$"""
        
                public bool Equals({{targetTypeName}} other) => m_value == other.m_value;

                public override bool Equals(object? obj)
                {
                    return obj switch
                    {
                        {{targetTypeName}} other => Equals(other),
                        {{typeName}} value => Equals(value),
                        _ => false
                    };
                }
                
                public override int GetHashCode() => m_value.GetHashCode();{{GetComparisonOperators(typeName, targetTypeName)}}

                public static {{targetTypeName}} operator +({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value + right.m_value);
                
                public static {{targetTypeName}} operator -({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value - right.m_value);{{GetUnaryNegationOperator(typeName, targetTypeName)}}
                
                public static {{targetTypeName}} operator *({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value * right.m_value);
                
                public static {{targetTypeName}} operator /({{targetTypeName}} left, {{targetTypeName}} right) => {{GetDivisionBody(typeName, targetTypeName)}};{{GetModulusOperator(typeName, targetTypeName)}}

                public static {{targetTypeName}} operator ++({{targetTypeName}} value) => ({{targetTypeName}})(value.m_value + ({{typeName}})1);

                public static {{targetTypeName}} operator --({{targetTypeName}} value) => ({{targetTypeName}})(value.m_value - ({{typeName}})1);{{GetComplementOperator(typeName, targetTypeName)}}
        """;

    // Go defines the ordered comparisons on every numeric kind EXCEPT complex (the spec limits
    // complex types to == / !=), and C#'s complex representations have no <//<=/>/>= either —
    // emitting them for a named complex type is CS0019 ×4 (testing/quick's TestComplex64Alias).
    // The IComparisonOperators interface declaration is gated the same way (InheritedTypeTemplate).
    //
    // CompareTo rides this same gate — it is IComparable<T>'s only member (declared alongside
    // IComparisonOperators in InheritedTypeTemplate, see the note there). It forwards to the
    // UNDERLYING value's CompareTo rather than being written out of the wrapper's own </>
    // operators, so a named FLOAT keeps the BCL's total order — NaN below everything — which is
    // what makes golib's `min`/`max` yield NaN when any argument is NaN, as Go's do. Every
    // underlying a `[GoType num:]` wrapper can name is IComparable<itself>: the aliases are BCL
    // primitives, `uintptr` is a golib struct that declares it, and a wrapper over another
    // wrapper picks up the member this very template gives it.
    private static string GetComparisonOperators(string typeName, string targetTypeName) => typeName.StartsWith("complex") ? "" :
       $"""


                public int CompareTo({targetTypeName} other) => m_value.CompareTo(other.m_value);

                public static bool operator <({targetTypeName} left, {targetTypeName} right) => left.m_value < right.m_value;

                public static bool operator <=({targetTypeName} left, {targetTypeName} right) => left.m_value <= right.m_value;

                public static bool operator >({targetTypeName} left, {targetTypeName} right) => left.m_value > right.m_value;

                public static bool operator >=({targetTypeName} left, {targetTypeName} right) => left.m_value >= right.m_value;
        """;

    // Go defines % on integers only, but C# floats carry a native % — keep the float emission
    // (inert for converted Go, harmless) and gate only complex, where C# has no % and the
    // emission is CS0019 (same kind-gate shape as GetComplementOperator below).
    private static string GetModulusOperator(string typeName, string targetTypeName) => typeName.StartsWith("complex") ? "" :
       $"""


                public static {targetTypeName} operator %({targetTypeName} left, {targetTypeName} right) => {GetRemainderBody(typeName, targetTypeName)};
        """;

    // Go wraps the one overflowing signed quotient, MinInt / -1, to MinInt and makes MinInt % -1 zero,
    // with no panic; .NET throws OverflowException for both at 32 and 64 bits, which a deferred
    // recover() cannot see. The -1 arm lives here, inside the named type's own operators, so a named
    // integer's `a / b` keeps its Go spelling; the converter emits golib's quo/rem only for an UNNAMED
    // signed division by a non-constant divisor. A narrower underlying needs no arm: C# promotes it to
    // int, and the wrapper's own cast back wraps the quotient exactly as Go does.
    private static bool IsSignedWideType(string typeName) =>
        typeName is "int32" or "int64" or "nint" or "rune" or "int" or "long";

    private static string GetDivisionBody(string typeName, string targetTypeName) => IsSignedWideType(typeName) ?
        $"right.m_value == -1 ? ({targetTypeName})unchecked(-left.m_value) : ({targetTypeName})(left.m_value / right.m_value)" :
        $"({targetTypeName})(left.m_value / right.m_value)";

    private static string GetRemainderBody(string typeName, string targetTypeName) => IsSignedWideType(typeName) ?
        $"right.m_value == -1 ? ({targetTypeName})({typeName})0 : ({targetTypeName})(left.m_value % right.m_value)" :
        $"({targetTypeName})(left.m_value % right.m_value)";

    // Bitwise complement keeps the WRAPPER type (Go `^T(0)` all-ones idiom - os exec_windows'
    // ^syscall.Handle(0) passed to a Handle parameter; the implicit-to-underlying conversion
    // made `~x` the raw numeric, CS1503). Integer underlyings only - `~` is invalid on floats.
    // Shifts and the binary bitwise operators keep the wrapper type for the same reason: Go
    // `x[i] >> ŝ` on a named integer (math/big's Word) IS a Word, but with no wrapper operator
    // C# resolved through the implicit-to-underlying conversion and the whole expression
    // degraded to the raw numeric (CS0266 ×45 across math/big's arith.cs).
    //
    // The two SHIFTS route through golib's GoShift guards rather than C#'s native operators,
    // because Go's shift count is UNBOUNDED (`x >> n` with n >= width is 0, or the sign for a
    // signed right shift) while C# MASKS the count to the operand's width - `x >> 64` on a
    // 64-bit value is `x >> 0`, i.e. x. The converter already applies this guard for a shift on
    // an unnamed basic type whose count it cannot prove in range (convBinaryExpr's
    // shiftCountGuarded); a NAMED numeric type resolves through this operator instead, so the
    // guard has to live here or that whole family keeps the masked answer. It did:
    // math/big's lehmerSimulate reads `A.abs[n-2] >> (_W - h)` on `Word`, which for a normalized
    // operand (h == 0) is a shift by exactly 64 - Go yields 0, C# yielded the word itself, and
    // the corrupted Lehmer cosequences made GCD's `for len(B.abs) > 1` loop stop converging.
    // That is an INFINITE LOOP in math/big reachable from crypto/elliptic's generic CurveParams
    // path: `elliptic.P256().Params().Double(Gx, Gy)` never returned, which is crypto/ecdsa's
    // P256/Generic subtest hanging every suite that includes it. `>>>` is left native: Go has no
    // unsigned-right-shift operator, so nothing converted ever calls it.
    private static string GetComplementOperator(string typeName, string targetTypeName) => typeName.StartsWith("float") || typeName.StartsWith("complex") ? "" :
       $"""


                public static {targetTypeName} operator ~({targetTypeName} value) => ({targetTypeName})(~value.m_value);

                public static {targetTypeName} operator <<({targetTypeName} value, int shift) => ({targetTypeName})value.m_value.Lsh((uint64)shift);

                public static {targetTypeName} operator >>({targetTypeName} value, int shift) => ({targetTypeName})value.m_value.Rsh((uint64)shift);

                public static {targetTypeName} operator >>>({targetTypeName} value, int shift) => ({targetTypeName})(value.m_value >>> shift);

                public static {targetTypeName} operator &({targetTypeName} left, {targetTypeName} right) => ({targetTypeName})(left.m_value & right.m_value);

                public static {targetTypeName} operator |({targetTypeName} left, {targetTypeName} right) => ({targetTypeName})(left.m_value | right.m_value);

                public static {targetTypeName} operator ^({targetTypeName} left, {targetTypeName} right) => ({targetTypeName})(left.m_value ^ right.m_value);
        """;

    // Go defines `-x` on EVERY numeric type, unsigned included, as the wrap-around `0 - x`. C# has
    // no unary minus for ulong (and widens uint/byte/ushort to a signed type), so the unsigned form
    // is written as that subtraction under `unchecked`, which is exactly Go's semantics. Emitting it
    // for unsigned too is what lets a generated named type satisfy the IUnaryNegationOperators
    // constraint the converter lifts for the Arithmetic operator set (constraintOperations.go
    // getLiftedConstraints) — without it, a generic over `~uint64` instantiated with a named
    // unsigned type is CS0315 (internal/trace's `dataTable[EI ~uint64, E]` over `type stringID uint64`).
    private static string GetUnaryNegationOperator(string typeName, string targetTypeName) => IsUnsignedType(typeName) ?
       $"""


                public static {targetTypeName} operator -({targetTypeName} value) => ({targetTypeName})unchecked(({typeName})(({typeName})0 - value.m_value));
        """ :
       $"""


                public static {targetTypeName} operator -({targetTypeName} value) => ({targetTypeName})(-value.m_value);
        """;
}
