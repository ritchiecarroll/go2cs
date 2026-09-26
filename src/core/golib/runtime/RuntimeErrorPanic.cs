// RuntimeErrorPanic.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;

namespace go.golib;

/// <summary>
/// Represents common runtime error messages thrown in Go environment.
/// </summary>
public static class RuntimeErrorPanic
{
    private const string RuntimeErrorMessage = "runtime error: ";

    private const string NilPointerDereferenceMessage = $"{RuntimeErrorMessage}invalid memory address or nil pointer dereference";
    public static PanicException NilPointerDereference()
    {
        return new PanicException(NilPointerDereferenceMessage);
    }

    private const string TokenArithmeticMessage =
        $"{RuntimeErrorMessage}unsafe pointer arithmetic on a managed pointer with no address "
        + "(a Go-layout byte offset into CLR-laid-out storage cannot be honoured)";
    /// <summary>
    /// The REFUSAL: a number derived by arithmetic from a pointer that has no machine address.
    /// </summary>
    /// <remarks>
    /// Refusing BY NAME, catchably, is the whole point. The alternative is not "it works" — it is a
    /// native box over a number that is not an address, and the first write through it takes the
    /// process down uncatchably, so a package reports nothing instead of reporting a failure.
    /// This says what happened, at the point it happened, and lets recover() see it.
    ///
    /// The underlying model question — a Go-layout offset landing on a managed reference slot in a
    /// CLR-auto-laid-out struct — is NOT answered here and is not meant to be: it stays open, and
    /// this makes it loud instead of fatal.
    /// </remarks>
    public static PanicException UnsafePointerArithmeticWithoutAddress()
    {
        return new PanicException(TokenArithmeticMessage);
    }

    private const string OrderTokenDereferenceMessage =
        $"{RuntimeErrorMessage}dereference of a managed pointer with no address "
        + "(*{0} over 0x{1} — the order token of a reference-bearing pointee at offset 0, Q44 §10.3 "
        + "arm 2a; a Go-layout byte offset into CLR-laid-out storage cannot be honoured)";
    /// <summary>
    /// The REFUSAL, arm 2a: a dereference of a native box whose address is a live box's ORDER
    /// TOKEN — a number no memory answers to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The SIBLING of <see cref="UnsafePointerArithmeticWithoutAddress"/>, one arm over, and it
    /// exists because that one cannot fire here BY CONSTRUCTION: <c>IsTokenArithmetic</c> requires
    /// the number to differ from its allocation base, and at OFFSET 0 the number IS the base. So a
    /// reference-bearing pointee reinterpreted at offset 0 — Go's
    /// <c>*(*V)(unsafe.Add(unsafe.Pointer(&amp;in), 0))</c>, which reflect's own <c>setField</c>
    /// helper performs — reached a native box over a number that is not an address, and the write
    /// through it took the process down.
    /// </para>
    /// <para>
    /// ⚠ AT THE DEREFERENCE AND NOT AT THE CONVERSION, which is measured rather than preferred:
    /// refusing the conversion took SEVEN GolibTests red against an empty base, because the native
    /// box over a token is a deliberate carrier whose address IS the token (the full reading is at
    /// <c>ж.NativeBox.cs</c>'s <c>OrderTokenRefusal</c>). The failure MODE is the entire point,
    /// exactly as for the arithmetic twin: the write was an UNCATCHABLE AccessViolation, so a
    /// package reported nothing at all rather than reporting a failure — measured at reflect's
    /// TestIsZero, which ended the host after 195 tests had started. A caught panic the harness can
    /// attribute leaves the model question open and loud instead of fatal.
    /// </para>
    /// </remarks>
    public static PanicException UnsafePointerOrderTokenDereferenced(Type pointee, nuint token)
    {
        return new PanicException(string.Format(OrderTokenDereferenceMessage, pointee.Name, token.ToString("x")));
    }

    private const string NativeArrayViewMessage =
        $"{RuntimeErrorMessage}cannot view native memory as {{0}}: the address has no managed element "
        + "storage behind it, and a Go array is a window on a real managed array "
        + "(see docs/phase4/DESIGN-native-array-view.md)";
    /// <summary>
    /// The SAFETY FLOOR: a genuinely-native address converted to a pointer whose pointee is an
    /// <c>array&lt;E&gt;</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>array&lt;E&gt;</c> is a MANAGED struct whose first field is an <c>E[]</c> reference, and a
    /// native box materializes its value out of the pointed-at bytes — so the composition
    /// REINTERPRETS whatever lives at that address AS A MANAGED REFERENCE and dereferences it.
    /// Measured: zeroed memory reads a length of 0, a silent wrong answer; memory filled with 0xAB
    /// reads a length of -1414812757, the data bytes themselves, returned instead of faulting by
    /// luck. Refusing BY NAME converts a type-safety hole into a diagnostic that names its own
    /// cause, at the cost of turning latent-SILENT into latent-LOUD.
    /// </para>
    /// <para>
    /// It is reached only when the address carries NO provenance record — a pinned-managed address
    /// resolves to its box and never arrives here, which is what keeps Go-legal reinterprets over
    /// pinned storage working. The message names the shape rather than the site because the site is
    /// a conversion the reader has to find in the emitted C#; the design document is where the fork
    /// and its remedy are written down.
    /// </para>
    /// </remarks>
    public static PanicException NativeArrayViewWithoutElementStorage(Type arrayType)
    {
        Type[] arguments = arrayType.IsGenericType ? arrayType.GetGenericArguments() : [];
        string shape = arguments.Length == 1 ? $"array<{arguments[0].Name}>" : arrayType.Name;

        return new PanicException(string.Format(NativeArrayViewMessage, shape));
    }

    private const string IndexOutOfRangeMessage = $"{RuntimeErrorMessage}index out of range [{{0}}] with length {{1}}";
    public static PanicException IndexOutOfRange(int64 index, int64 length)
    {
        return new PanicException(string.Format(IndexOutOfRangeMessage, index, length));
    }

    private const string SliceBoundsOutOfRangeMessage = $"{RuntimeErrorMessage}slice bounds out of range ";
    public static PanicException SliceBoundsOutOfRange(int64 low, int64 high, int64 max, int64 capacity)
    {
        // Mirrors the Go runtime's message shapes for a slice expression s[low:high:max]
        string bounds;

        if (max > capacity)
            bounds = $"[::{max}] with capacity {capacity}";
        else if (high > max)
            bounds = max == capacity ? $"[:{high}] with capacity {capacity}" : $"[:{high}:{max}]";
        else if (low < 0)
            bounds = $"[{low}:]";
        else
            bounds = $"[{low}:{high}]";

        return new PanicException(SliceBoundsOutOfRangeMessage + bounds);
    }

    private const string ArrayConversionLengthMessage = $"{RuntimeErrorMessage}cannot convert slice with length {{0}} to array or pointer to array with length {{1}}";

    /// <summary>
    /// Go's panic for a <c>[N]T(s)</c> / <c>(*[N]T)(s)</c> conversion whose source slice is shorter
    /// than <c>N</c> — the message the runtime itself prints, raised as a recoverable panic.
    /// </summary>
    public static PanicException ArrayConversionLength(int64 sourceLength, int64 length)
    {
        return new PanicException(string.Format(ArrayConversionLengthMessage, sourceLength, length));
    }

    private const string IntegerDivideByZeroMessage = $"{RuntimeErrorMessage}integer divide by zero";

    /// <summary>
    /// Supplies the Go runtime's OWN divide-by-zero panic value (<c>runtime.divideError</c>, whose
    /// dynamic type is the unexported <c>runtime.errorString</c>).
    /// </summary>
    /// <remarks>
    /// Go's compiler lowers an integer division to a zero check plus <c>runtime.panicdivide()</c>, so
    /// <c>recover()</c> yields a value satisfying <c>runtime.Error</c> — math/bits' TestDiv32PanicZero
    /// asserts exactly that (<c>err.(runtime.Error)</c>) on the panic raised by the IMPLICIT hardware
    /// division in <c>Div32</c>, unlike Div/Div64 which panic explicitly. golib sits UNDER the converted
    /// <c>runtime</c> package and so cannot name that value; the runtime package registers it here
    /// (see its <c>panicvalues_impl.cs</c> bridge), leaving this layer dependency-free. When nothing has
    /// registered — a converted program that never links <c>runtime</c> — the panic falls back to the
    /// plain message below, which still reads and prints identically and only loses the type assertion.
    /// </remarks>
    public static Func<object>? IntegerDivideByZeroValue { get; set; }

    public static PanicException IntegerDivideByZero()
    {
        return new PanicException(IntegerDivideByZeroValue?.Invoke() ?? IntegerDivideByZeroMessage);
    }

    private const string ComparingUncomparableTypeMessage = $"{RuntimeErrorMessage}comparing uncomparable type {{0}}";

    /// <summary>
    /// Go's panic for <c>==</c> between two interface values whose shared dynamic type has no equal
    /// algorithm — a slice, map or func, or a struct/array that transitively contains one.
    /// </summary>
    /// <param name="goTypeName">Dynamic type, spelled as Go spells it (<c>map[string]int</c>, <c>[1][]int</c>, <c>main.withSlice</c>).</param>
    /// <remarks>
    /// Recoverable, like every other runtime error minted here: Go raises this from
    /// <c>runtime.panicunsafe</c>-family code with a <c>runtime.Error</c> value, so a
    /// <c>defer func(){ recover() }()</c> observes it and prints the message verbatim. Measured
    /// against go1.23.12 for each shape the caller gates (see <c>builtin.AreEqual</c>).
    /// </remarks>
    public static PanicException ComparingUncomparableType(string goTypeName)
    {
        return new PanicException(string.Format(ComparingUncomparableTypeMessage, goTypeName));
    }

    private const string MakeSliceLenOutOfRangeMessage = $"{RuntimeErrorMessage}makeslice: len out of range";
    public static PanicException MakeSliceLenOutOfRange()
    {
        return new PanicException(MakeSliceLenOutOfRangeMessage);
    }

    private const string RangeFunctionContinuedMessage = $"{RuntimeErrorMessage}range function continued iteration after function for loop body returned false";

    /// <summary>
    /// Go's panic for a range-over-func sequence that calls <c>yield</c> again after the loop body
    /// answered false (runtime/panic.go's <c>rangeDoneError</c>).
    /// </summary>
    public static PanicException RangeFunctionContinued()
    {
        return new PanicException(RangeFunctionContinuedMessage);
    }

    private const string MakeSliceCapOutOfRangeMessage = $"{RuntimeErrorMessage}makeslice: cap out of range";
    public static PanicException MakeSliceCapOutOfRange()
    {
        return new PanicException(MakeSliceCapOutOfRangeMessage);
    }

    /// <summary>
    /// Converts a .NET exception that corresponds to a Go runtime panic into a <see cref="PanicException"/>,
    /// so it can be recovered with <c>recover()</c> and reported like a Go panic. Returns <c>false</c>
    /// (leaving the exception to propagate unchanged) for exceptions that are not Go runtime panics.
    /// </summary>
    /// <param name="ex">Exception to inspect.</param>
    /// <param name="panic">Resulting panic when the exception maps to a Go runtime panic.</param>
    /// <returns><c>true</c> if <paramref name="ex"/> is (or maps to) a Go panic; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// This is the ONE place a .NET exception becomes a Go panic, so it is also where the panic's
    /// origin is snapshotted (<see cref="PanicException.PanicTrace"/>) — a mapped runtime error is
    /// synthesized HERE and was never thrown at the fault site, so only <paramref name="ex"/> carries
    /// those frames, and once this returns they are the caller's last chance to be recorded. Doing it
    /// at the adoption point rather than in each adopter is what lets a panic that passes through NO
    /// <c>GoFunc</c> at all — a function that never defers, so nothing wraps it — still report where
    /// it faulted. The snapshot is once-only, so a panic re-adopted by each enclosing frame keeps the
    /// innermost (deepest) origin, and nothing is computed on a non-panicking path.
    /// </remarks>
    public static bool TryAsPanic(Exception ex, [NotNullWhen(true)] out PanicException? panic)
    {
        switch (ex)
        {
            case PanicException panicException:
                panic = panicException;
                panic.CaptureThrowSite(ex);
                return true;
            case DivideByZeroException:
                // Go: integer division or modulo by zero panics with a runtime error. .NET raises
                // DivideByZeroException for the same operation; map it so recover() behaves like Go.
                panic = IntegerDivideByZero();
                panic.CaptureThrowSite(ex);
                return true;
            case NullReferenceException:
                // Go: dereferencing a nil pointer panics with "runtime error: invalid memory address
                // or nil pointer dereference" — a RECOVERABLE panic that real Go code relies on
                // (sync's TestNilPool calls Get/Put on a nil *Pool and requires recover() to catch
                // it). The converted equivalent — reading `Ꮡp.Value` through a nil ж<T>, or any nil
                // reference deref in emitted code — raises NullReferenceException, which recover()
                // could not see, so the panic escaped as an unrecoverable infrastructure error.
                // Mapping it here gives recover() Go's behavior and prints Go's message verbatim.
                panic = NilPointerDereference();
                panic.CaptureThrowSite(ex);
                return true;
            default:
                panic = null;
                return false;
        }
    }
}
