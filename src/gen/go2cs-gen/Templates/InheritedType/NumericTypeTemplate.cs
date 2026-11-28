// NumericTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go2cs.Templates.InheritedType;

internal static class NumericTypeTemplate
{
    // The members this wrapper declares for its own use under an ASCII name a Go method can also declare (R7; see
    // ISliceTypeTemplate.ClaimableMembers). GenTests' NamedChanMapNumericClaimedMemberTests.
    internal static readonly string[] ClaimableMembers = ["CompareTo"];

    private static bool IsUnsignedType(string typeName) =>
        typeName is "uint8" or "uint16" or "uint32" or "uint64" or "byte" or "rune" or "uintptr" or "nuint" or "uint";

    // goMethods are the Go methods declared on the type, and its own name (InheritedTypeTemplate.GoMethodNames): a claimed
    // CompareTo moves to its explicit IComparable<T> implementation, which golib's min/max and cmp's ordered helpers bind.
    public static string Generate(string typeName, string targetTypeName, System.Collections.Generic.ICollection<string> goMethods) =>
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
                
                public override int GetHashCode() => m_value.GetHashCode();{{GetComparisonOperators(typeName, targetTypeName, goMethods)}}

                public static {{targetTypeName}} operator +({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value + right.m_value);
                
                public static {{targetTypeName}} operator -({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value - right.m_value);{{GetUnaryNegationOperator(typeName, targetTypeName)}}
                
                public static {{targetTypeName}} operator *({{targetTypeName}} left, {{targetTypeName}} right) => ({{targetTypeName}})(left.m_value * right.m_value);
                
                public static {{targetTypeName}} operator /({{targetTypeName}} left, {{targetTypeName}} right) => {{GetDivisionBody(typeName, targetTypeName)}};{{GetModulusOperator(typeName, targetTypeName)}}

                public static {{targetTypeName}} operator ++({{targetTypeName}} value) => ({{targetTypeName}})(value.m_value + ({{typeName}})1);

                public static {{targetTypeName}} operator --({{targetTypeName}} value) => ({{targetTypeName}})(value.m_value - ({{typeName}})1);{{GetComplementOperator(typeName, targetTypeName)}}{{GetNumericWrapperMembers(typeName, targetTypeName)}}
        """;

    // Whether the wrapper implements golib's IGoNumericWrapper<T>, the conversions golib's ConvertToType, ConvertToUInt64,
    // ConvertToFloat64 and ConvertToFloat32 take for a named numeric type argument (`T(x)` with T instantiated as
    // `type stringID uint64`). Complex is left out: Go converts no integer or float to a complex type, and golib's
    // ladder has no complex rung for the unnamed kinds either. InheritedTypeTemplate declares the interface on the
    // same gate.
    internal static bool ImplementsNumericWrapper(string typeName) => !typeName.StartsWith("complex");

    // Each member converts through golib's ladder for the wrapper's own kind and wraps or unwraps the result, so a named
    // type converts exactly as its underlying kind does, with no reflection (golib's IGoNumericWrapper). The members are
    // explicit, so no Go method name is taken. Every member takes its operand as an argument and ignores its receiver:
    // golib calls them on one cached default value of the wrapper.
    //
    // A FLOAT kind converts an integer with C#'s numeric conversion, which rounds to nearest as Go's float64(x) does;
    // the source's signedness picks the overload (golib's ConvertToType(long) and ConvertToType(ulong) say which).
    private static string GetNumericWrapperMembers(string typeName, string targetTypeName)
    {
        if (!ImplementsNumericWrapper(typeName))
            return "";

        string wrapper = $"global::go.golib.IGoNumericWrapper<{targetTypeName}>";
        string fromInteger = typeName.StartsWith("float") ? $"new(({typeName})value)" : $"new(global::go.builtin.ConvertToType<{typeName}>(value))";

        return $"""


                {targetTypeName} {wrapper}.FromUInt64(ulong value) => {fromInteger};

                {targetTypeName} {wrapper}.FromInt64(long value) => {fromInteger};

                {targetTypeName} {wrapper}.FromFloat64(double value) => new(global::go.builtin.ConvertToType<{typeName}>(value));

                ulong {wrapper}.ToUInt64({targetTypeName} value) => global::go.builtin.ConvertToUInt64(value.m_value);

                double {wrapper}.ToFloat64({targetTypeName} value) => global::go.builtin.ConvertToFloat64(value.m_value);

                float {wrapper}.ToFloat32({targetTypeName} value) => global::go.builtin.ConvertToFloat32(value.m_value);
        """;
    }

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
    private static string GetComparisonOperators(string typeName, string targetTypeName, System.Collections.Generic.ICollection<string> goMethods) => typeName.StartsWith("complex") ? "" :
       $"""


                {(goMethods.Contains("CompareTo") ? $"int global::System.IComparable<{targetTypeName}>.CompareTo({targetTypeName} other)" : $"public int CompareTo({targetTypeName} other)")} => m_value.CompareTo(other.m_value);

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
    // signed division (or a type parameter's) where the divisor may be -1 and the dividend may be the
    // most negative value. A narrower underlying needs no arm: C# promotes it to int, and the
    // wrapper's own cast back wraps the quotient exactly as Go does.
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
    //
    // The `int` count serves a literal or provably in-range count. An UNBOUNDED count takes the
    // WIDE overloads instead, which the converter binds by passing the count widened exactly as it
    // does for an unnamed operand (shiftCountUint64Operand): a `uint64` count keeps its full
    // magnitude, so `w << (1 << 32)` is 0 rather than the `(int)` narrowing's `w << 0`, and an
    // `int64` count binds golib's signed-count guard, which raises Go's "negative shift amount"
    // panic where the narrowing read -1 as a huge count and answered 0. C# 11 lets a user-defined
    // shift operator take a non-`int` count.
    private static string GetComplementOperator(string typeName, string targetTypeName) => typeName.StartsWith("float") || typeName.StartsWith("complex") ? "" :
       $"""


                public static {targetTypeName} operator ~({targetTypeName} value) => ({targetTypeName})(~value.m_value);

                public static {targetTypeName} operator <<({targetTypeName} value, int shift) => ({targetTypeName})value.m_value.Lsh((uint64)shift);

                public static {targetTypeName} operator >>({targetTypeName} value, int shift) => ({targetTypeName})value.m_value.Rsh((uint64)shift);

                public static {targetTypeName} operator <<({targetTypeName} value, uint64 shift) => ({targetTypeName})value.m_value.Lsh(shift);

                public static {targetTypeName} operator >>({targetTypeName} value, uint64 shift) => ({targetTypeName})value.m_value.Rsh(shift);

                public static {targetTypeName} operator <<({targetTypeName} value, int64 shift) => ({targetTypeName})value.m_value.Lsh(shift);

                public static {targetTypeName} operator >>({targetTypeName} value, int64 shift) => ({targetTypeName})value.m_value.Rsh(shift);

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
