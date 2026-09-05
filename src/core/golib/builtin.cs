// builtin.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable UseSymbolAlias
// ReSharper disable StaticMemberInGenericType

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using go.golib;
using static System.Math;
using static go2cs.Symbols;
using TypeExtensions = go.golib.TypeExtensions;

namespace go;

/// <summary>
/// Go's built-in functions, as C# statics — <c>append</c>, <c>len</c>, <c>cap</c>, <c>make</c>,
/// <c>copy</c>, <c>new</c>, <c>delete</c>, <c>panic</c>, <c>recover</c>, <c>print</c> — plus the
/// execution helpers the converter emits around goroutines, deferred calls and recover scopes.
/// Every converted file gets this class through a project-wide <c>using static go.builtin</c>, so a
/// bare <c>len(x)</c> in emitted C# reads exactly as it does in Go.
/// </summary>
/// <remarks>
/// <para>
/// Declared <c>partial</c> so the three combinatorial ARITY LADDERS can live in sibling files —
/// <c>builtin.GoroutineLaunchers.cs</c>, <c>builtin.DeferRegistrations.cs</c> and
/// <c>builtin.FuncExecutionContexts.cs</c> — along with the type-parameter integer conversions in
/// <c>builtin.TypeParamConversions.cs</c>. Those ladders are mechanical repetition, sixteen
/// near-identical rungs each, and burying the real builtin surface underneath ~1,800 lines of them
/// is the only reason this file was ever hard to navigate. <c>partial</c> is source- and
/// binary-compatible: the compiler emits one <c>builtin</c> type either way.
/// </para>
/// <para>
/// Adding a member? Put it HERE if it is a Go builtin or a close relative. Put it in a ladder file
/// only if it is one more rung of that ladder — each of those files states its own extension rule.
/// </para>
/// </remarks>
public static partial class builtin
{
    public const int StackAllocThreshold = 1024; // 1KB

    private static readonly ThreadLocal<bool> s_fallthrough = new();

    [ModuleInitializer]
    internal static void InitializeGoLib()
    {
        // FIRST, before anything can touch System.Console: close the .NET runtime's startup
        // duplicates of the standard descriptors so that Go code closing fd 0/1/2 releases the
        // underlying pipe the way it does in a Go binary. The ordering is part of that fix's
        // safety contract - see builtin.LinuxStdDescriptors.cs.
        InitializeLinuxStdDescriptorHygiene();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8;

        // Go's runtime.osinit opts every Windows process into long-path handling before any user
        // code runs; a converted program is an ordinary .NET process and gets nothing of the kind
        // unless golib asks for it here. See builtin.WindowsLongPaths.cs.
        InitializeWindowsLongPaths();

        // The thread that first touches golib is the one running `main`. Registering it makes the
        // main goroutine visible to the live-goroutine registry, as Go's own accounting sees it —
        // without marking the thread as being "on a goroutine", which is a different question that
        // only runtime.Goexit asks (golib/runtime/Goroutine.cs).
        //
        // There is deliberately NO ThreadPool.SetMinThreads floor here any more. golib used to raise
        // the pool's minimum to max(4×cores, 256) because goroutines were pool work items and a
        // parked one held a pool thread, so a burst of parked goroutines could starve the
        // not-yet-started goroutine that would unblock them. That floor called itself "a mitigation,
        // not a scheduler", and its premise is now gone: goroutines get dedicated threads
        // (Goroutine.Start), so nothing Go-semantic parks on the pool and there is no shortfall to
        // pre-warm. The floor was also process-GLOBAL — it distorted the host, timer callbacks and
        // every other .NET consumer of the pool for a problem none of them had. The pool goes back
        // to being .NET's utility, at .NET's defaults.
        Goroutine.RegisterMainGoroutine();
        return;

        [MethodImpl(MethodImplOptions.Synchronized)]
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception? ex = e.ExceptionObject as Exception;

            // Match Go: an unrecovered panic in any goroutine reports on stderr and terminates the
            // process with exit code 2 — never stdout / exit 0, which polluted compared output and
            // signaled false success to callers (shells, CI, the Phase-4 differential oracle).
            //
            // The unwrap covers a .NET exception that MAPS to a Go runtime panic (an unrecovered
            // integer divide by zero, a nil dereference) and one that arrives WRAPPED by machinery
            // it travelled through — Task.Wait's AggregateException, reflection's
            // TargetInvocationException. A Go panic is a Go panic whatever wrapped it in transit,
            // and reporting the wrapper instead is what made an escaped panic print a CLR dump.
            if (CrashReport.TryUnwrapPanic(ex, out PanicException? panic, out Exception? thrown))
            {
                // Go's crash report, in Go's own format: the panic value, a blank line, the
                // goroutine header, and the Go-spelled traceback — plus a copy to the descriptor
                // debug.SetCrashOutput configured, if any. See CrashReport and
                // docs/phase4/DESIGN-crash-report.md. This used to be the first line alone.
                CrashReport.Report(panic, thrown);
            }
            else if (ex is not null)
            {
                // NOT a Go panic — a managed failure escaping converted code, i.e. a defect to
                // diagnose rather than a program behavior to report. Message alone throws away the
                // evidence: a TypeInitializationException's own Message merely names the type and
                // says "see inner exception", so the actual fault and its stack were LOST (a whole
                // gob run's real cause was invisible this way). ToString() carries the full chain.
                Console.Error.WriteLine(ex);
            }
            else
            {
                Console.Error.WriteLine($"unhandled exception: {e.ExceptionObject}");
            }

            Environment.Exit(2);
        }
    }

    private static class Zero<T>
    {
        public static T Default = default!;
    }

    /// <summary>
    /// Predeclared identifier representing the untyped integer ordinal number of the current
    /// const specification in a (usually parenthesized) const declaration.
    /// It is zero-indexed.
    /// </summary>
    public const nint iota = 0;

    /// <summary>
    /// nil is a predeclared identifier representing the zero value for a pointer, channel,
    /// func, interface, map, or slice type.
    /// </summary>
    public static readonly NilType nil = NilType.Default;

    /// <summary>
    /// Defines a true constant used as a discriminator for accessing overloads with
    /// different return types.
    /// </summary>
    /// <remarks>
    /// Common use is for expression-based switch values where focus is
    /// on <c>when</c> conditions and not the value itself.
    /// </remarks>
    public const bool ᐧ = true;

    /// <summary>
    /// Opaque <c>true</c> for a <c>when</c> clause the compiler must NOT fold.
    /// </summary>
    /// <remarks>
    /// A Go expressionless switch may lead with a constant-true case (<c>switch { case true:
    /// ... }</c> - time parseStrictRFC3339 deliberately disabling its strict checks) whose LATER
    /// cases Go compiles as dead code; a foldable <c>when true</c> makes C# reject those cases
    /// outright (CS8120), so the converter emits this marker there instead. A separate marker is
    /// required because the FOLDABILITY of the const <see cref="ᐧ"/> is itself load-bearing:
    /// <c>case ᐧ when ...</c> label patterns need a constant, and an infinite 
    /// <c>for (...; ᐧ ;...)</c> relies on the fold for reachability (CS0161).
    /// </remarks>
    public static readonly bool ᐧᐧ = true;

    /// <summary>
    /// Defines a false constant used as a discriminator for accessing overloads with
    /// different return types.
    /// </summary>
    /// <remarks>
    /// Often used with functions that return a value and a boolean success or error state
    /// as a tuple.
    /// </remarks>
    public const bool ꟷ = false;

    /// <summary>
    /// Defines a nil constant used as a discriminator for accessing overloads with
    /// different return types.
    /// </summary>
    /// <remarks>
    /// Often used with functions that have an operation that will be continued, e.g.:
    /// <c>switch (select(f.ᐸꟷ(x, ꓸꓸꓸ), ᐸꟷ(quit, ꓸꓸꓸ)))</c>
    /// The above channel operations return a <see cref="SelectOp"/> case descriptor that
    /// registers the pending operation with the select.
    /// </remarks>
    public static readonly NilType ꓸꓸꓸ = nil;

    /// <summary>
    /// Instructs a switch case extension call to transfer control to the first
    /// statement of the next case clause in an expression.
    /// </summary>
    /// <remarks>
    /// Note that <c>fallthrough</c> is a reserved Go keyword, so it will not be
    /// used as a variable name.
    /// </remarks>
    public static bool fallthrough
    {
        get
        {
            if (!s_fallthrough.Value)
                return false;

            s_fallthrough.Value = false;
            return true;
        }
        set
        {
            s_fallthrough.Value = value;
        }
    }

    /// <summary>
    /// Runs the converted Go package that declares <paramref name="packageType"/> through its
    /// package initialization, if it has not already run.
    /// </summary>
    /// <param name="packageType">The package's <c>&lt;name&gt;_package</c> class.</param>
    /// <remarks>
    /// Go initializes every imported package before the importing package, whether or not anything
    /// in it is ever referenced — which is the entire point of a BLANK import (<c>import _ "image/png"</c>),
    /// whose only purpose is the imported package's <c>init</c> side effects. A converted Go <c>init</c>
    /// becomes a <c>[GoInit]</c> (i.e. <see cref="ModuleInitializerAttribute"/>) method, and .NET makes
    /// the WEAKER guarantee: a referenced assembly's module constructor runs at first access to
    /// something in that module, so an assembly the program never names is never loaded and its
    /// <c>init</c> never runs. This forces it, and the runtime guarantees a module constructor runs at
    /// most once, so repeated calls (several blank importers of one package) are no-ops.
    /// <para>
    /// Only the module constructor is forced. That is exactly the package's <c>init</c> functions; the
    /// package's own package-level variable initializers are C# static field initializers, which the
    /// CLR still runs lazily at first access to the package class — unchanged from every other import.
    /// </para>
    /// </remarks>
    public static void initPackage(Type packageType)
    {
        RuntimeHelpers.RunModuleConstructor(packageType.Module.ModuleHandle);
    }

    /// <summary>
    /// Returns a new <see cref="PanicException"/> with the specified <paramref name="state"/>.
    /// </summary>
    /// <param name="state">State of panic exception.</param>
    public static PanicException panic(object state)
    {
        // Go's `panic` takes an `any`, so the value's DYNAMIC TYPE is observable: a recovering
        // comparison (`if p != "x"`), a type assertion (`err.(string)`), and a `case string:` arm
        // all test it. A C# `System.String` reaching here is a Go `string` that never acquired its
        // @string boxing — from a bare literal (`panic("x")`, which the emission deliberately does
        // not u8-suffix) or from a stub returning C# string (baseline fmt.Sprintf). Boxed as
        // System.String it matches nothing on the recover side, which compares against @string:
        // sync's testOncePanicX reported the self-contradictory `want panic x, got x`. Normalizing
        // at this single boxing boundary covers every caller — literal, computed, and hand-owned —
        // where a cast at the emission site could only cover the literal.
        return new PanicException(state is string s ? (@string)s : state);
    }

    /// <summary>
    /// Returns the value of the panic being handled by this frame's deferred sequence, if any,
    /// and stops the panic — Go's <c>recover</c>.
    /// </summary>
    /// <returns>Recovered panic state, or <c>null</c> when no panic is in flight.</returns>
    /// <remarks>
    /// Reads the one thread-local slot the emitted <c>catch</c> parked the panic in
    /// (<see cref="GoFrame.Capture"/>), so it resolves STATICALLY from wherever it is called —
    /// which is what lets a deferred closure recover without holding a handle on the frame that
    /// registered it. It replaces the <c>Recover</c> delegate the retired
    /// <c>func((defer, recover) =&gt; ...)</c> execution context passed into the body; that
    /// delegate's <c>HandleRecover</c> did exactly this, against exactly this slot.
    /// </remarks>
    public static object? recover()
    {
        object? state = GoFuncRoot.CapturedPanicValue?.State;
        GoFuncRoot.CapturedPanicValue = null;
        return state;
    }

    /// <summary>
    /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<T> append<T>(in slice<T> slice, params T[] elems)
    {
        return go.slice<T>.Append(slice, elems);
    }

    /// <summary>
    /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <typeparam name="T">Type of slice.</typeparam>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<T> append<T>(ISlice slice, params T[] elems)
    {
        return (slice<T>)slice.Append(elems.Cast<object>().ToArray())!;
    }

    /// <summary>
    /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <typeparam name="T">Type of slice.</typeparam>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<T> append<T>(slice<T> slice, params T[] elems)
    {
        return go.slice<T>.Append(slice, elems);
    }

    /// <summary>
    /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <typeparam name="T">Type of slice.</typeparam>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<T> append<T>(slice<T> slice, params Span<T> elems)
    {
        return go.slice<T>.Append(slice, elems);
    }

    /// <summary>
    /// Appends a whole slice's elements — Go's <c>append(s, t...)</c> with a slice-typed spread
    /// operand, the spread riding on the NAME (<c>appendꓸꓸꓸ(s, t)</c>, the same glyph the operand
    /// spread and the variadic delegate families already use). The operand arrives AS THE SLICE IT
    /// IS rather than projected to a <see cref="Span{T}"/> at the call boundary (the
    /// slice-shaped-spread arc): lengths stay <c>nint</c> end to end, and a window no span can
    /// express meets Go's own <c>len out of range</c> panic instead of a span-construction
    /// failure. A DISTINCT name by design: sharing <c>append</c>'s overload set re-entered the
    /// params/betterness thicket the C#14 flip re-tiled (measured CS0121 against the
    /// constrained span twin), and the converter alone mints these calls.
    /// </summary>
    /// <typeparam name="T">Type of slice.</typeparam>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Slice whose elements are appended.</param>
    /// <returns>New slice with specified values appended.</returns>
    public static slice<T> appendꓸꓸꓸ<T>(slice<T> slice, ISlice<T> elems)
    {
        return go.slice<T>.Append(slice, elems);
    }

    /// <summary>
    /// Appends runes to the end of a byte slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<byte> append(slice<byte> slice, params ReadOnlySpan<rune> elems)
    {
        return go.slice<byte>.Append(slice, elems.ToUTF8Bytes());
    }

    /// <summary>
    /// Converts a span of runes to a UTF-8 byte array.
    /// </summary>
    /// <param name="runes">Runes to convert to bytes.</param>
    /// <returns>byte array representing UTF-8 encoding of runes.</returns>
    /// <exception cref="InvalidOperationException">Buffer too small for UTF-8 encoding.</exception>
    public static byte[] ToUTF8Bytes(this in ReadOnlySpan<rune> runes)
    {
        // Estimate buffer size (4 bytes per rune as worst case)
        int estimatedBytes = runes.Length * 4;
        
        Span<byte> buffer = estimatedBytes <= StackAllocThreshold ?
            stackalloc byte[estimatedBytes] :
            AllocationCounter.NewArray<byte>(estimatedBytes);

        int bytesWritten = 0;

        foreach (rune value in runes)
        {
            // Go encodes an invalid rune (surrogate or out of range) as U+FFFD (RuneError, 3 bytes)
            // rather than failing — e.g. string([]rune{0xD800}) is "�"; the explicit int-to-Rune
            // conversion threw for exactly those values.
            if (!Rune.TryCreate(value, out Rune codePoint))
                codePoint = Rune.ReplacementChar;

            // Encoding not expected to fail given 4x buffer
            if (!codePoint.TryEncodeToUtf8(buffer[bytesWritten..], out int runeBytes))
                throw new InvalidOperationException("Buffer too small for UTF-8 encoding");

            bytesWritten += runeBytes;
        }

        return AllocationCounter.CopyOf<byte>(buffer[..bytesWritten]);
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/> (same as len(array)).
    /// </summary>
    /// <param name="array">Target array pointer.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint cap<T>(in array<T> array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/> (same as len(array)).
    /// </summary>
    /// <param name="array">Target array pointer.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint cap(IArray array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the maximum length the <paramref name="slice"/> can reach when resliced.
    /// </summary>
    /// <param name="slice">Target slice pointer.</param>
    /// <returns>The capacity of the <paramref name="slice"/>.</returns>
    public static nint cap<T>(in slice<T> slice)
    {
        return slice.Capacity;
    }

    /// <summary>
    /// Gets the maximum length the stack-only <paramref name="slice"/> can reach when resliced.
    /// </summary>
    /// <param name="slice">Target stack slice.</param>
    /// <returns>The capacity of the <paramref name="slice"/>.</returns>
    public static nint cap<T>(in sslice<T> slice)
    {
        return slice.Capacity;
    }

    /// <summary>
    /// Gets the maximum length the <paramref name="slice"/> can reach when resliced.
    /// </summary>
    /// <param name="slice">Target slice pointer.</param>
    /// <returns>The capacity of the <paramref name="slice"/>.</returns>
    public static nint cap(ISlice slice)
    {
        return slice.Capacity;
    }

    /// <summary>
    /// Gets the maximum capacity of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The capacity of the <paramref name="channel"/>.</returns>
    public static nint cap<T>(in channel<T> channel)
    {
        return channel.Capacity;
    }

    /// <summary>
    /// Gets the maximum capacity of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The capacity of the <paramref name="channel"/>.</returns>
    public static nint cap(IChannel channel)
    {
        return channel.Capacity;
    }

    /// <summary>
    /// Closes the channel.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    public static void close<T>(in channel<T> channel)
    {
        // An "in" parameter works here because the close method operates on channel structure's
        // private class-based member references, not on value types
        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
        channel.Close();
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>Received value.</returns>
    /// <remarks>
    /// <para>
    /// If the channel is empty, method will block the current thread until a value is sent to the channel.
    /// </para>
    /// <para>
    /// Defines a Go style channel <see cref="channel{T}.Receive()"/>> operation.
    /// </para>
    /// </remarks>
    public static T ᐸꟷ<T>(channel<T> channel)
    {
        return channel.Receive();
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꟷ"/>.</param>
    /// <returns>
    /// Received value and boolean result reporting whether the communication succeeded which is
    /// <c>true</c> if the value received was delivered by a successful send operation ; otherwise,
    /// <c>false</c> if a zero value generated because the channel is closed and empty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If the channel is empty, method will block the current thread until a value is sent to the channel.
    /// </para>
    /// <para>
    /// Defines a Go style channel <see cref="channel{T}.Receive()"/>> operation.
    /// </para>
    /// </remarks>
    public static (T val, bool ok) ᐸꟷ<T>(channel<T> channel, bool _)
    {
        return channel.Receive(_);
    }

    /// <summary>
    /// Gets the select-case descriptor registering a receive on the channel.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꓸꓸꓸ"/>.</param>
    /// <returns>Receive-case descriptor for <see cref="select"/>.</returns>
    /// <remarks>
    /// Defines a Go style channel <see cref="channel{T}.Receiving"/> select registration.
    /// </remarks>
    public static SelectOp ᐸꟷ<T>(channel<T> channel, NilType _)
    {
        return channel.Receiving;
    }

    /// <summary>
    /// Executes a blocking Go <c>select</c> over the registered channel <paramref name="ops"/>:
    /// commits exactly ONE ready case — chosen uniformly at random when several are ready — or
    /// parks until one becomes ready.
    /// </summary>
    /// <param name="ops">Registered send/receive case descriptors, in case order.</param>
    /// <returns>Ordinal of the single case that fired.</returns>
    /// <remarks>
    /// A committed receive's value is handed to the winning case's guard
    /// (<c>case N when ch.ꟷᐳ(out v):</c>) through a per-thread pending-frame stack the guard pops
    /// (a stack so a select nested in the guard's target expression cannot destroy the outer
    /// commit). Nil-channel cases are never ready; a send case on a closed channel panics.
    /// </remarks>
    public static int select(params SelectOp[] ops)
    {
        return SelectRuntime.Run(ops);
    }

    /// <summary>
    /// Executes a non-blocking Go <c>select</c> (the <c>default:</c> form) over the registered
    /// channel <paramref name="ops"/>: commits exactly ONE ready case — chosen uniformly at random
    /// when several are ready — or returns -1 (the default sentinel) so the emitted switch's
    /// <c>default:</c> label runs.
    /// </summary>
    /// <param name="ops">Registered send/receive case descriptors, in case order.</param>
    /// <returns>Ordinal of the single case that fired, or -1 when no case was ready.</returns>
    /// <remarks>
    /// Shares <see cref="select"/>'s poll pass (same locking and pollorder) but never parks. A
    /// committed receive's value crosses to the winning case's guard through the same per-thread
    /// pending-frame stack; a send case on a closed channel panics even though a default exists
    /// (Go semantics).
    /// </remarks>
    public static int trySelect(params SelectOp[] ops)
    {
        return SelectRuntime.TryRun(ops);
    }

    /// <summary>
    /// Enumerates over a range of integers.
    /// </summary>
    /// <param name="n">Number of integers to enumerate.</param>
    /// <returns>Enumerable range of integers.</returns>
    /// <remarks>
    /// Yields <see cref="nint"/> values (Go's <c>int</c>, the type of a range-over-int index in
    /// <c>for i := range n</c>) so a <c>var i</c> loop variable infers <c>nint</c> rather than a C#
    /// <c>int</c> — the latter subtly diverges from Go's index type and makes downstream generic calls
    /// like <c>append(s, i)</c> ambiguous. Producing values 0..n-1 (and no iterations when n &lt;= 0)
    /// matches Go's integer-range semantics exactly.
    /// </remarks>
    public static IEnumerable<nint> range(nint n)
    {
        for (nint i = 0; i < n; i++)
        {
            yield return i;
        }
    }

    /// <summary>
    /// Enumerates over a range of integers whose operand is a C# <see cref="int"/>.
    /// </summary>
    /// <param name="n">Number of integers to enumerate.</param>
    /// <returns>Enumerable range of <see cref="nint"/> values — Go's <c>int</c>.</returns>
    /// <remarks>
    /// An <see cref="int"/>-typed operand in emitted code is a Go untyped integer CONSTANT
    /// (<c>for i := range 3</c>); a Go <c>int</c> expression already renders as <c>nint</c>, and every
    /// other Go integer width is emitted with an explicit type argument on <see cref="range{T}(T)"/>.
    /// So this overload means "a C# int operand is Go's int", and it exists to keep the constant case
    /// on <see cref="nint"/>: without it the generic below is an IDENTITY match for an <c>int</c>
    /// argument where <see cref="range(nint)"/> needs an implicit numeric conversion, so the generic
    /// wins outright and <c>for i := range 3</c> starts yielding <see cref="int"/> — the exact
    /// divergence <c>Tests/Behavioral/RangeIntIndexAppend</c> guards (a <c>var i</c> of the wrong
    /// width made <c>append(s, i)</c> ambiguous, CS0121). Measured, not reasoned: the generic really
    /// does take the literal, so the tie-break that prefers a non-generic candidate never applies.
    /// </remarks>
    public static IEnumerable<nint> range(int n)
    {
        return range((nint)n);
    }

    /// <summary>
    /// Enumerates over a range of integers of any Go integer type.
    /// </summary>
    /// <typeparam name="T">Integer type of the range operand.</typeparam>
    /// <param name="n">Number of integers to enumerate.</param>
    /// <returns>Enumerable range of <typeparamref name="T"/> values.</returns>
    /// <remarks>
    /// Go's range-over-integer accepts ANY integer type, not just <c>int</c>, and gives the iteration
    /// variable THAT type — <c>for i := range n</c> over a <c>uintptr</c> yields <c>uintptr</c> values.
    /// This overload is that general case. The converter names <typeparamref name="T"/> EXPLICITLY at
    /// every such site (<c>range&lt;uintptr&gt;(n)</c>), which is what keeps the widths that share a
    /// CLR type distinct: Go's <c>rune</c> and C#'s <c>int</c> are one type here, so an inferred call
    /// could not tell <c>for i := range r</c> (yields <c>rune</c>) from <c>for i := range 3</c>
    /// (yields Go's <c>int</c>). Producing values 0..n-1 (and no iterations when n &lt;= 0) matches
    /// Go's integer-range semantics exactly.
    /// <para>
    /// The constraint is the OPERATOR pair the loop actually uses, not <c>IBinaryInteger&lt;T&gt;</c>:
    /// golib's own <see cref="uintptr"/> is a hand-written struct that implements the generic-math
    /// OPERATOR interfaces but not the <c>INumberBase</c> hierarchy, and <c>uintptr</c> is exactly
    /// the type <c>for range atyp.Len</c> ranges over. <c>default(T)</c> is the zero of every Go
    /// integer type, so no <c>T.Zero</c> — and therefore no <c>INumberBase</c> — is needed.
    /// </para>
    /// </remarks>
    public static IEnumerable<T> range<T>(T n) where T : struct, System.Numerics.IComparisonOperators<T, T, bool>, System.Numerics.IIncrementOperators<T>
    {
        for (T i = default; i < n; i++)
        {
            yield return i;
        }
    }

    /// <summary>
    /// Enumerates over a yield function.
    /// </summary>
    /// <param name="enumerator">Yield function.</param>
    /// <returns>Enumerable range of values.</returns>
    public static IEnumerable<object> range(Action<Func<bool>> enumerator)
    {
        return range<object>(e =>
        {
            // ReSharper disable once UnusedParameter.Local
            bool yielder(object _) => e(null!);
            enumerator(() => yielder(null!));
        });
    }

    /// <summary>
    /// Enumerates over a yield function.
    /// </summary>
    /// <typeparam name="T">Type of enumeration.</typeparam>
    /// <param name="enumerator">Yield function.</param>
    /// <returns>Enumerable range of values.</returns>
    public static IEnumerable<T> range<T>(Action<Func<T, bool>> enumerator)
    {
        return new YieldFunctionEnumerable<T>(enumerator);
    }

    /// <summary>
    /// Enumerates over a two-value yield function (Go's `for k, v := range seq2` on an
    /// iter.Seq2-shaped func) by adapting the pair into the tuple-typed yield machinery.
    /// </summary>
    /// <typeparam name="T1">First enumeration type.</typeparam>
    /// <typeparam name="T2">Second enumeration type.</typeparam>
    /// <param name="enumerator">Two-value yield function.</param>
    /// <returns>Enumerable range of value pairs.</returns>
    public static IEnumerable<(T1, T2)> range<T1, T2>(Action<Func<T1, T2, bool>> enumerator)
    {
        return new YieldFunctionEnumerable<(T1, T2)>(yield => enumerator((k, v) => yield((k, v))));
    }

    /// <summary>
    /// Constructs a complex value from two floating-point values.
    /// </summary>
    /// <param name="realPart">Real-part of complex value.</param>
    /// <param name="imaginaryPart">Imaginary-part of complex value.</param>
    /// <returns>New complex value from specified <paramref name="realPart"/> and <paramref name="imaginaryPart"/>.</returns>
    public static complex64 complex(float32 realPart, float32 imaginaryPart)
    {
        return new complex64(realPart, imaginaryPart);
    }

    /// <summary>
    /// Constructs a complex value from two floating-point values.
    /// </summary>
    /// <param name="realPart">Real-part of complex value.</param>
    /// <param name="imaginaryPart">Imaginary-part of complex value.</param>
    /// <returns>New complex value from specified <paramref name="realPart"/> and <paramref name="imaginaryPart"/>.</returns>
    public static complex128 complex(float64 realPart, float64 imaginaryPart)
    {
        return new complex128(realPart, imaginaryPart);
    }

    /// <summary>
    /// Copies elements from a source array into a destination array.
    /// </summary>
    /// <param name="dst">Destination array.</param>
    /// <param name="src">Source array.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in array<T1> dst, in array<T2> src)
    {
        return copy(dst.Slice(0, dst.Length), src.Slice(0, src.Length));
    }

    /// <summary>
    /// Copies elements from a source array pointer into a destination array pointer.
    /// </summary>
    /// <param name="dst">Destination array pointer.</param>
    /// <param name="src">Source array pointer.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in ж<array<T1>> dst, in ж<array<T2>> src)
    {
        ArgumentNullException.ThrowIfNull((object?)dst);
        ArgumentNullException.ThrowIfNull((object?)src);

        return copy(dst.Value, src.Value);
    }

    /// <summary>
    /// Copies elements from a source array pointer into a destination array.
    /// </summary>
    /// <param name="dst">Destination array pointer.</param>
    /// <param name="src">Source array.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in ж<array<T1>> dst, in array<T2> src)
    {
        ArgumentNullException.ThrowIfNull((object?)dst);

        return copy(dst.Value, src);
    }

    /// <summary>
    /// Copies elements from a source array into a destination array pointer.
    /// </summary>
    /// <param name="dst">Destination array.</param>
    /// <param name="src">Source array pointer.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in array<T1> dst, in ж<array<T2>> src)
    {
        ArgumentNullException.ThrowIfNull((object?)src);

        return copy(dst, src.Value);
    }

    /// <summary>
    /// Copies elements from a source <see cref="ISlice{T}"/> into a destination slice.
    /// </summary>
    /// <param name="dst">Destination slice.</param>
    /// <param name="src">Source slice implementation (e.g., a defined type over a slice, like Go's <c>type pMask []uint32</c>).</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    /// <remarks>
    /// A named slice type (`[GoType("[]uint32")]` wrapper) implements <see cref="ISlice{T}"/> but is not a
    /// <see cref="slice{T}"/>, so generic inference cannot bind the slice/slice overload from it (CS1503 —
    /// runtime <c>copy(nidlepMask, idlepMask)</c>, idlepMask a <c>pMask</c>). The source window comes from the
    /// interface's <see cref="IArray{T}.ToSpan"/>, which views the wrapper's underlying storage — no copy.
    /// A genuine <see cref="slice{T}"/> source still binds the more specific slice/slice overload.
    /// </remarks>
    public static nint copy<T1, T2>(in slice<T1> dst, ISlice<T2> src)
    {
        nint min = Min(dst.Length, src.Length);

        if (ZeroSizeCopy<T1, T2>())
            return min;

        if (min > 0)
        {
            // Same-type sources copy via the window spans: ToSpan() views each side's true slice
            // window (indexing with an added Low here would double-offset — both indexers are
            // window-relative), and Span.CopyTo has memmove semantics, so an overlapping src/dst
            // (Go permits overlap in copy) transfers correctly.
            if (src is ISlice<T1> same)
            {
                same.ToSpan()[..(int)min].CopyTo(dst.ToSpan());
            }
            else
            {
                for (nint i = 0; i < min; i++)
                    dst[i] = ConvertElement<T1>(src[i]!);
            }
        }

        return min;
    }

    /// <summary>
    /// Copies elements from a source slice into a destination slice.
    /// The source and destination may overlap.
    /// </summary>
    /// <param name="dst">Destination slice.</param>
    /// <param name="src">Source slice.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in slice<T1> dst, in slice<T2> src)
    {
        nint min = Min(dst.Length, src.Length);

        if (ZeroSizeCopy<T1, T2>())
            return min;

        if (min > 0)
        {
            if (typeof(T1) == typeof(T2))
            {
                // The identical-element case, which is the ONLY one converted Go emits — Go's
                // `copy` requires the element types to match. The test folds at JIT time, so this
                // arm compiles to exactly one span copy and the fork below disappears from the
                // dominant path: native-to-native and native-to-managed transfers vectorize
                // instead of walking element by element, and the managed case reaches the same
                // Buffer.Memmove Array.Copy would have. Overlap stays correct either way (Go
                // permits it in copy, and CopyTo has memmove semantics).
                //
                // The Span<T2>→Span<T1> re-spelling is a reinterpret of the span header, not of
                // the data: the two are the same runtime type here, which is exactly what the
                // guard establishes and what MemoryMarshal.Cast cannot express on unconstrained
                // type parameters.
                Span<T2> source = src.ToSpan();

                Unsafe.As<Span<T2>, Span<T1>>(ref source)[..(int)min].CopyTo(dst.ToSpan());
            }
            else if (typeof(T1).IsAssignableFrom(typeof(T2)))
            {
                if (dst.IsNativeBacked || src.IsNativeBacked)
                {
                    // A native window exists only for unmanaged elements (the creation door
                    // enforces it), and assignability between distinct value types means identical
                    // representation — Unsafe.As re-spells each element without a copy-through-
                    // object, and the spans read/write the mapping itself (slice.ToSpan is the
                    // design's discriminant-once point). The generic parameters are unconstrained,
                    // which is what rules out the single AsBytes block copy here.
                    Span<T2> source = src.ToSpan();
                    Span<T1> destination = dst.ToSpan();

                    for (int i = 0; i < (int)min; i++)
                        destination[i] = Unsafe.As<T2, T1>(ref source[i]);
                }
                else
                {
                    Array.Copy(src.m_array, src.Low, dst.m_array, dst.Low, min);
                }
            }
            else
            {
                // Both indexers are window-relative (each adds its own Low and bounds-checks
                // against Length), so adding Low here double-offsets — the rule the ISlice
                // overload above states. Guarded by GolibTests.SliceCopyWindowTests.
                for (nint i = 0; i < min; i++)
                    dst[i] = ConvertElement<T1>(src[i]!);
            }
        }

        return min;
    }

    /// <summary>
    /// Copies elements from a source slice pointer into a destination slice pointer.
    /// The source and destination may overlap.
    /// </summary>
    /// <param name="dst">Destination slice pointer.</param>
    /// <param name="src">Source slice pointer.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in ж<slice<T1>> dst, in ж<slice<T2>> src)
    {
        ArgumentNullException.ThrowIfNull((object?)dst);
        ArgumentNullException.ThrowIfNull((object?)src);

        return copy(dst.Value, src.Value);
    }

    /// <summary>
    /// Copies elements from a source slice pointer into a destination slice.
    /// The source and destination may overlap.
    /// </summary>
    /// <param name="dst">Destination slice pointer.</param>
    /// <param name="src">Source slice.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in ж<slice<T1>> dst, in slice<T2> src)
    {
        ArgumentNullException.ThrowIfNull((object?)dst);

        return copy(dst.Value, src);
    }

    /// <summary>
    /// Copies elements from a source slice into a destination slice pointer.
    /// The source and destination may overlap.
    /// </summary>
    /// <param name="dst">Destination slice.</param>
    /// <param name="src">Source slice pointer.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in slice<T1> dst, in ж<slice<T2>> src)
    {
        ArgumentNullException.ThrowIfNull((object?)src);

        return copy(dst, src.Value);
    }

    /// <summary>
    /// Copies elements from a source slice into a destination slice.
    /// The source and destination may overlap.
    /// </summary>
    /// <param name="dst">Destination slice.</param>
    /// <param name="src">Source slice.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    /// <remarks>
    /// As a special case, it also will copy bytes from a string to a slice of bytes.
    /// </remarks>
    public static nint copy(in slice<byte> dst, in @string src)
    {
        // Go's `copy(b, s)` is ONE copy and no allocation. Binding the implicit
        // `slice<byte>(@string)` operator here cost two of each: that operator materializes a
        // charged full-LENGTH copy of the string's bytes — it must, since a slice is writable and
        // a string backing must never become so — and the slice/slice copy then copied a second
        // time into dst, even when dst was far shorter than the string. `strings.Reader.Read`
        // binds this overload once per Read with the whole remaining tail, so every
        // `strings.NewReader` consumer paid a full-tail allocation per read.
        //
        // The string is immutable and dst's window span already serves both backings, so the
        // bytes go straight across. CopyTo's memmove semantics cover the one case where the two
        // could share storage (a transient alias), which is the same guarantee Go's copy gives.
        nint min = Min(dst.Length, (nint)src.Length);

        if (min > 0)
            src.Bytes[..(int)min].CopyTo(dst.ToSpan());

        return min;
    }

    /// <summary>
    /// Copies elements into a destination held as a constrained slice type parameter
    /// (<c>S ~[]E</c> boxed to its <see cref="ISlice{T}"/> constraint). The box wraps the same
    /// backing array, so writes through the interface land in the caller's storage; source and
    /// destination may overlap (span CopyTo has memmove semantics). Concrete
    /// <see cref="slice{T}"/> arguments still bind the exact overloads above.
    /// </summary>
    /// <param name="dst">Destination slice view.</param>
    /// <param name="src">Source slice view.</param>
    /// <returns>The number of elements copied — min of len(src) and len(dst).</returns>
    public static nint copy<T1, T2>(ISlice<T1> dst, ISlice<T2> src)
    {
        nint min = Min(dst.Length, src.Length);

        if (ZeroSizeCopy<T1, T2>())
            return min;

        if (min > 0)
        {
            if (src is ISlice<T1> same)
            {
                same.ToSpan()[..(int)min].CopyTo(dst.ToSpan());
            }
            else
            {
                for (nint i = 0; i < min; i++)
                    dst[i] = ConvertElement<T1>(src[i]!);
            }
        }

        return min;
    }

    // The element conversion behind every `copy` fallback arm. Those arms run only when the source
    // element type is NOT assignable to the destination's, which converted Go never produces (Go's
    // `copy` requires identical element types) — they serve hand-written and interop callers, and
    // that is why the defect below survived: no corpus gate reaches them.
    //
    // TypeExtensions.ConvertToType answers the Go representation of what the value ALREADY is, so
    // its boxed result unboxes to T only when the two element types agreed to begin with (a named
    // numeric wrapper and its underlying, say — the case the arms were written against). A
    // genuinely different element type, which is the only way to arrive here, needs a conversion
    // TOWARD T, or the unbox throws InvalidCastException. The `is T` test keeps the original
    // no-conversion path exact and pays for ChangeType only where the direct unbox could not have
    // worked at all; the arm already boxes per element, so this is not the per-element allocation
    // the NumericConversions header warns against introducing.
    /// <summary>
    /// Whether a <c>copy</c> between these element types moves nothing because the elements occupy no
    /// storage — Go's <c>copy</c> is <c>memmove(dst, src, n * elemSize)</c>, which for a zero-size
    /// element is a memmove of zero bytes that still RETURNS <c>n</c>.
    /// </summary>
    /// <remarks>
    /// The return value is the whole observable result there, so every <c>copy</c> overload answers it
    /// from the length arithmetic it has already done and never reaches for storage: a zero-size slice
    /// may be longer than any array or span, so the span path would raise the ceiling panic
    /// (<c>slice&lt;T&gt;.ToSpan</c>) on a copy Go performs for free. Both sides are tested because a
    /// mixed pair — which converted Go cannot produce, since Go's <c>copy</c> requires identical
    /// element types — would still owe a real conversion. Folded per closed pair, so no ordinary copy
    /// pays for the test.
    /// </remarks>
    private static bool ZeroSizeCopy<T1, T2>()
    {
        return GoZeroSizeFacts<T1>.IsZeroSize && GoZeroSizeFacts<T2>.IsZeroSize;
    }

    private static T ConvertElement<T>(object element)
    {
        object converted = TypeExtensions.ConvertToType((IConvertible)element);

        return converted is T value ? value : (T)Convert.ChangeType(converted, typeof(T));
    }

    /// <summary>
    /// Copies string bytes into a destination held as a constrained slice type parameter
    /// (<c>S ~[]byte</c> boxed to <see cref="ISlice{T}"/> of byte).
    /// </summary>
    /// <param name="dst">Destination byte-slice view.</param>
    /// <param name="src">Source string.</param>
    /// <returns>The number of bytes copied.</returns>
    public static nint copy(ISlice<byte> dst, in @string src)
    {
        // Same de-allocation as the slice<byte> overload above; the interface box wraps the
        // caller's own backing, so its window span is the destination.
        nint min = Min(dst.Length, (nint)src.Length);

        if (min > 0)
            src.Bytes[..(int)min].CopyTo(dst.ToSpan());

        return min;
    }

    /// <summary>
    /// Copies elements from a source slice pointer into a destination slice.
    /// The source and destination may overlap.
    /// </summary>
    /// <param name="dst">Destination slice pointer.</param>
    /// <param name="src">Source slice.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    /// <remarks>
    /// As a special case, it also will copy bytes from a string to a slice of bytes.
    /// </remarks>
    public static nint copy(in ж<slice<byte>> dst, in @string src)
    {
        ArgumentNullException.ThrowIfNull((object?)dst);

        return copy(dst.Value, src);
    }

    /// <summary>
    /// Deletes the element with the specified key from the map. If m is nil or there is no such element, delete is a no-op.
    /// </summary>
    /// <param name="map">Target map.</param>
    /// <param name="key">Key to remove.</param>
    public static void delete<TKey, TValue>(map<TKey, TValue> map, TKey key) where TKey : notnull
    {
        map.Remove(key);
    }

    /// <summary>
    /// Deletes a key from a map held as a constrained map type parameter (<c>M ~map[K]V</c>,
    /// boxed to its <see cref="IMap{TKey, TValue}"/> constraint — the maps package's
    /// DeleteFunc). Key and value types infer from the interface conversion.
    /// </summary>
    /// <param name="map">Map view.</param>
    /// <param name="key">Key to delete.</param>
    public static void delete<TKey, TValue>(IMap<TKey, TValue> map, TKey key) where TKey : notnull
    {
        map.Remove(key);
    }

    /// <summary>
    /// Zeros all elements of a slice (the Go 1.21 <c>clear</c> built-in). The length and capacity are
    /// unchanged; only the element values are reset to the zero value of T.
    /// </summary>
    /// <param name="slice">Target slice.</param>
    /// <summary>
    /// Zeroes the elements of a slice held as a constrained slice type parameter
    /// (<c>S ~[]E</c> boxed to its <see cref="ISlice{T}"/> constraint) — the boxed view
    /// aliases the caller's backing array.
    /// </summary>
    public static void clear<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(ISlice<T> slice)
    {
        // A zero-size element has one value and it IS the zero value, so clearing is complete before
        // it starts — and asking for the span would be asking for storage the slice does not own.
        if (GoZeroSizeFacts<T>.IsZeroSize)
            return;

        clear(slice.ToSpan());
    }

    /// <summary>
    /// Sub-slices a value held as a constrained slice type parameter (<c>S ~[]E</c>) to the END of
    /// its window — Go's <c>s[low:]</c> — PRESERVING its type: a sub-slice of a named slice type
    /// yields the same named type sharing the same backing storage. The converter emits the type
    /// arguments explicitly (T is constraint-only and C# cannot infer it).
    /// </summary>
    /// <param name="s">Constrained slice value.</param>
    /// <param name="low">Low bound. Every value is a real bound; a negative one panics like Go.</param>
    /// <returns>The sub-slice, as the same constrained type.</returns>
    /// <remarks>
    /// <para>
    /// <b>This overload exists so that no bound is ever a SENTINEL.</b> The omitted-high form used to
    /// travel as <c>high = -1</c> through the three-argument method, which made <c>s[i:]</c> and
    /// <c>s[i:-1]</c> the identical call — and the same convention on <c>low</c> was worse, because
    /// the method clamped EVERY negative low to 0 rather than only the sentinel. Go panics for a
    /// negative index, so `slices.Insert(s, -1, …)` and `slices.Replace(s, -1, 2, …)` — whose whole
    /// bodies begin with the bounds-check expressions `_ = s[i:]` and `_ = s[i:j]` — silently
    /// succeeded where Go's own tests require a panic (slices' TestInsertPanics, TestReplacePanics).
    /// </para>
    /// <para>
    /// The omitted LOW needed no overload: Go's <c>s[:high]</c> IS <c>s[0:high]</c>, so the converter
    /// emits the 0 and the ambiguity disappears at the source. A three-index expression cannot omit
    /// its high bound at all (Go's grammar requires it), so <see cref="subslice3{S, T}"/> takes three
    /// real bounds and needs no companion.
    /// </para>
    /// </remarks>
    public static S subslice<S, T>(S s, nint low) where S : ISlice<T>, ISliceWrap<S, T>
    {
        slice<T> window = new(s);

        return S.Wrap(window.Reslice(low, window.Length, window.Capacity));
    }

    /// <summary>
    /// Sub-slices a value held as a constrained slice type parameter (<c>S ~[]E</c>) — Go's
    /// <c>s[low:high]</c> — PRESERVING its type. Both bounds are real; see the two-argument
    /// <see cref="subslice{S, T}(S, nint)"/> remarks for why neither is a sentinel.
    /// </summary>
    /// <param name="s">Constrained slice value.</param>
    /// <param name="low">Low bound.</param>
    /// <param name="high">High bound.</param>
    /// <returns>The sub-slice, as the same constrained type.</returns>
    public static S subslice<S, T>(S s, nint low, nint high) where S : ISlice<T>, ISliceWrap<S, T>
    {
        slice<T> window = new(s);

        // Reslice, not the `slice()` extension: the extension carries golib's own -1 defaulting
        // convention, which would re-open the collision this overload pair closes.
        return S.Wrap(window.Reslice(low, high, window.Capacity));
    }

    /// <summary>
    /// Appends elements to a value held as a constrained slice type parameter (<c>S ~[]E</c>),
    /// PRESERVING its type — Go's <c>append(s, items...)</c> on a named slice type yields the
    /// same named type (S infers from the first argument, T from the element span).
    /// </summary>
    /// <param name="s">Constrained slice value.</param>
    /// <param name="items">Elements to append.</param>
    /// <returns>The appended slice, as the same constrained type.</returns>
    /// <summary>
    /// Span twin of the constrained append below: a spread of another constrained value
    /// (`append(s, v.ꓸꓸꓸ)`) yields Span&lt;T&gt;, for which the legacy `params T[]` candidate wins
    /// betterness with T = Span&lt;E&gt; (a ref struct as a type argument — CS9244); the exact Span
    /// normal form wins instead.
    /// </summary>
    public static S append<S, T>(S s, params Span<T> items) where S : ISlice<T>, ISliceWrap<S, T>
    {
        return S.Wrap(go.slice<T>.Append(new slice<T>(s), items));
    }

    /// <summary>
    /// Slice-shaped twin of the constrained append: a spread whose operand is itself
    /// slice-shaped (<c>append(s, v...)</c> in a generic body over <c>S ~[]E</c>) passes the
    /// operand AS A SLICE instead of projecting it through the interface's spread property to a
    /// <see cref="Span{T}"/> — the slice-shaped-spread arc's constrained form. Both type
    /// arguments are EXPLICIT at the emission (<c>T</c> cannot be inferred from <c>S</c>'s
    /// constraint surface), and the name carries the spread for the same reason as the
    /// <c>slice&lt;T&gt;</c> form above.
    /// </summary>
    public static S appendꓸꓸꓸ<S, T>(S s, ISlice<T> items) where S : ISlice<T>, ISliceWrap<S, T>
    {
        return S.Wrap(go.slice<T>.Append(new slice<T>(s), items));
    }

    /// <summary>
    /// Full (3-index) sub-slice of a constrained slice type parameter, preserving its type —
    /// Go's <c>s[low:high:max]</c> on a named slice type. All three bounds are real: Go's grammar
    /// requires the high bound in a full slice expression, and the converter emits <c>0</c> for an
    /// omitted low, so this method has no defaults to apply and no sentinel to mistake for one
    /// (see <see cref="subslice{S, T}(S, nint)"/>).
    /// </summary>
    public static S subslice3<S, T>(S s, nint low, nint high, nint max) where S : ISlice<T>, ISliceWrap<S, T>
    {
        return S.Wrap(new slice<T>(s).Reslice(low, high, max));
    }

    public static S append<S, T>(S s, params ReadOnlySpan<T> items) where S : ISlice<T>, ISliceWrap<S, T>
    {
        // Route to the core slice Append directly — a recursive `append(...)` call would resolve
        // back to THIS overload (slice<T> itself satisfies the constraints), so it would be
        // infinite recursion.
        //
        // The items go across as the span they already are. This used to call items.ToArray(),
        // allocating and copying a whole array per constrained append purely because the core
        // Append took Span; it takes ReadOnlySpan now, and its body never wanted the wider access.
        slice<T> result = go.slice<T>.Append(new slice<T>(s), items);
        return S.Wrap(result);
    }

    public static void clear<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(slice<T> slice)
    {
        // See the ISlice overload: zero-size elements are already their own zero value.
        if (GoZeroSizeFacts<T>.IsZeroSize)
            return;

        clear(slice.ToSpan());
    }

    /// <summary>
    /// Zeros all elements of a span — the Go 1.21 <c>clear</c> built-in applied to a slice backed by a
    /// span (e.g. a <c>(*[N]T)(ptr)[:n]</c> unsafe array view).
    /// </summary>
    /// <param name="span">Target span.</param>
    public static void clear<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Span<T> span)
    {
        // An element whose Go zero value must be BUILT cannot be zeroed by Span.Clear (which fills
        // `default`) — see GoZero. The check is a per-T constant, so every ordinary element type
        // keeps the vectorized clear.
        if (ZeroFacts<T>.IsDefault)
        {
            span.Clear();
            return;
        }

        for (int i = 0; i < span.Length; i++)
            span[i] = GoZero(span[i]);
    }

    /// <summary>
    /// Gets the Go ZERO VALUE for <typeparamref name="T"/>, shaped like <paramref name="template"/>.
    /// </summary>
    /// <typeparam name="T">Type whose Go zero value is wanted.</typeparam>
    /// <param name="template">An existing value of that type, consulted ONLY for run-time shape.</param>
    /// <returns>The Go zero value of <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// <para>
    /// <c>default(T)</c> is the Go zero value for almost everything, but NOT for the two emitted
    /// shapes whose zero value has to be built:
    /// </para>
    /// <list type="bullet">
    /// <item>a fixed-size <see cref="array{T}"/> (<see cref="IGoZeroShaped"/>) — its Go length lives
    /// only in the instance, so <c>default</c> yields a LENGTH-ZERO array; and</item>
    /// <item>a converted Go struct, whose generated parameterless constructor is what runs the field
    /// initializers allocating its fixed-array fields and promoted-embed boxes — <c>default</c> skips
    /// it. Calling that constructor is always correct: it is exactly what the converter emits for
    /// <c>var x T</c>, and for a plain struct it is <c>default</c>.</item>
    /// </list>
    /// <para>
    /// This is the RUN-TIME half of zero-value construction. The converter's <c>arrayZeroValueArgs</c>
    /// and go2cs-gen's <c>AppendZeroValueInitializers</c> build a zero value where the shape is known
    /// statically from the Go type; this one recovers the shape from a value that already carries it,
    /// which is what a built-in like <see cref="clear{T}(slice{T})"/> is handed. Centralizing it here
    /// is what keeps each NEW consumer from re-opening the zero-value-construction defect class.
    /// </para>
    /// </remarks>
    public static T GoZero<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(T template)
    {
        // Hoisted per closed T, so the overwhelmingly common case folds to a constant `default`.
        if (ZeroFacts<T>.IsDefault)
            return default!;

        return template is IGoZeroShaped shaped ? (T)shaped.GoZeroLike() : (T)Activator.CreateInstance(typeof(T))!;
    }

    /// <summary>
    /// Determines whether <c>default(T)</c> is already the Go zero value for <typeparamref name="T"/>
    /// — the per-T fact behind <see cref="GoZero{T}"/>, exposed for containers that fill their own
    /// backing (see <see cref="array{T}"/>).
    /// </summary>
    internal static bool ZeroIsDefault<T>()
    {
        return ZeroFacts<T>.IsDefault;
    }

    // Per-closed-generic classification of how T's Go zero value is produced. Same shape (and same
    // reasoning) as AssertFacts<T> below: the answer is an immutable fact about the type, and reading
    // it as a static field lets the JIT fold the test to a constant for the closed instantiation.
    private static class ZeroFacts<T>
    {
        internal static readonly bool IsDefault = Classify();

        private static bool Classify()
        {
            Type type = typeof(T);

            // Every reference type's Go zero value is nil.
            if (!type.IsValueType)
                return true;

            // A fixed-size array's Go length is INSTANCE state, never type state.
            if (typeof(IGoZeroShaped).IsAssignableFrom(type))
                return false;

            // A converted Go struct is zeroed by calling its generated parameterless constructor;
            // anything else golib carries (@string, slice<T>, map<K,V>, the numeric primitives) is
            // already correctly zeroed by `default`.
            return !type.IsDefined(typeof(GoTypeAttribute), false) || type.GetConstructor(Type.EmptyTypes) is null;
        }
    }

    /// <summary>
    /// Removes all entries from a map (the Go 1.21 <c>clear</c> built-in applied to a map). Clearing a
    /// nil map is a no-op.
    /// </summary>
    /// <param name="map">Target map.</param>
    public static void clear<TKey, TValue>(map<TKey, TValue> map) where TKey : notnull
    {
        map.Clear();
    }

    /// <summary>
    /// Removes all entries from a NAMED map type's value (the Go 1.21 <c>clear</c> built-in applied
    /// to a named map, e.g. <c>clear(h)</c> on an <c>http.Header</c>) — the generated wrapper
    /// implements <see cref="IMap{TKey, TValue}"/> and forwards to the shared underlying map, so
    /// clearing through the boxed view empties the caller's storage. Clearing a nil map is a no-op.
    /// </summary>
    /// <param name="map">Named-map value to clear.</param>
    public static void clear<TKey, TValue>(IMap<TKey, TValue> map) where TKey : notnull
    {
        // Spelled through ICollection rather than as a bare `map.Clear()`: IMap{TKey, TValue}
        // inherits a Clear from BOTH IMap and ICollection{KeyValuePair{,}}, so the unqualified call
        // is CS0121-ambiguous. Both name the same operation on the same storage — this picks one
        // rather than widening the interface, which would force every generated named-map wrapper
        // to declare a Clear of its own.
        ((ICollection<KeyValuePair<TKey, TValue>>)map).Clear();
    }

    /// <summary>
    /// Returns a shallow clone of a map held as a boxed <c>any</c> — the runtime intrinsic Go
    /// implements as <c>runtime.mapclone</c> and links into <c>maps.clone</c> (the un-generic
    /// worker behind <c>maps.Clone</c>). The argument arrives boxed (an <c>any</c> wrapping a
    /// <see cref="map{TKey, TValue}"/> or a named-map wrapper); the returned <c>any</c> wraps a
    /// fresh map with the same key/value pairs and an INDEPENDENT backing store, so mutating the
    /// clone does not affect the original. A nil map clones to a nil map. Dispatch flows through
    /// <see cref="IMap.CloneMap"/> so the concrete key/value types are recovered without reflection.
    /// </summary>
    /// <param name="m">Boxed map value to clone.</param>
    /// <returns>A boxed independent clone of <paramref name="m"/>.</returns>
    public static any mapclone(any m)
    {
        return m is IMap source ? source.CloneMap() : m;
    }

    /// <summary>
    /// Returns the smaller of two values via comparison operators — the form a constrained type
    /// parameter satisfies (Go's <c>cmp.Ordered</c> lifts to IComparisonOperators; such a
    /// parameter has no IComparable&lt;T&gt; conversion). Exact two-argument calls on
    /// operator-capable types prefer this normal form over the params overload below.
    /// </summary>
    /// <param name="x">First value (Go requires at least one argument).</param>
    /// <param name="y">Second value.</param>
    /// <returns>The minimum of <paramref name="x"/> and <paramref name="y"/>, NaN if either is NaN.</returns>
    /// <remarks>
    /// The NaN arm is Go's own rule, not a courtesy: the spec states that <c>min</c>/<c>max</c> yield
    /// NaN when ANY argument is a NaN. C#'s comparison operators answer <c>false</c> for every
    /// comparison involving a NaN, so the bare <c>x &lt; y ? x : y</c> this replaced silently returned
    /// the NON-NaN side whenever the NaN sat on the left (`min(NaN, 1)` answered 1). Only <c>x</c>
    /// needs the test: when <c>y</c> is the NaN, <c>x &lt; y</c> is already false and <c>y</c> — the NaN
    /// — is what comes back. Measured by slices' <c>TestMinMaxNaNs</c>, which replaces each element of a
    /// float64 slice with NaN in turn and requires both <c>Min</c> and <c>Max</c> to propagate it.
    /// <see cref="OrderedFacts{T}"/>.IsFloating is a static readonly per-T constant, so the whole arm
    /// folds away for every integer instantiation and the integer path keeps the codegen it had.
    /// </remarks>
    public static T min<T>(T x, T y) where T : System.Numerics.IComparisonOperators<T, T, bool>
    {
        if (OrderedFacts<T>.IsFloating && OrderedFacts<T>.IsNaN(in x))
            return x;

        return x < y ? x : y;
    }

    /// <summary>
    /// Returns the smallest value among its ordered arguments (the Go 1.21 <c>min</c> built-in) — the
    /// form a three-or-more-argument call binds, and the form a type with <see cref="IComparable{T}"/>
    /// but no comparison operators binds at two.
    /// </summary>
    /// <param name="x">First value (Go requires at least one argument).</param>
    /// <param name="y">Remaining values to compare.</param>
    /// <returns>The minimum of <paramref name="x"/> and <paramref name="y"/>, NaN if any is NaN.</returns>
    /// <remarks>
    /// <see cref="IComparable{T}"/> ordering sorts NaN BELOW every other value, which happens to be
    /// Go's answer for <c>min</c> and is the opposite of Go's answer for <c>max</c> — so the NaN rule
    /// is applied explicitly here rather than inherited from the total order, and both overloads carry
    /// the same arm. (The remark this replaced claimed the total order matched Go for both; it does
    /// not, and <c>max</c> was wrong.)
    /// </remarks>
    public static T min<T>(T x, params ReadOnlySpan<T> y) where T : IComparable<T>
    {
        T result = x;

        if (OrderedFacts<T>.IsFloating)
        {
            if (OrderedFacts<T>.IsNaN(in x))
                return x;

            foreach (T value in y)
            {
                if (OrderedFacts<T>.IsNaN(in value))
                    return value;

                if (value.CompareTo(result) < 0)
                    result = value;
            }

            return result;
        }

        foreach (T value in y)
        {
            if (value.CompareTo(result) < 0)
                result = value;
        }

        return result;
    }

    /// <summary>
    /// Returns the larger of two values via comparison operators — see the two-argument
    /// <see cref="builtin.min{T}(T, T)"/> remarks, NaN rule included.
    /// </summary>
    /// <param name="x">First value (Go requires at least one argument).</param>
    /// <param name="y">Second value.</param>
    /// <returns>The maximum of <paramref name="x"/> and <paramref name="y"/>, NaN if either is NaN.</returns>
    public static T max<T>(T x, T y) where T : System.Numerics.IComparisonOperators<T, T, bool>
    {
        if (OrderedFacts<T>.IsFloating && OrderedFacts<T>.IsNaN(in x))
            return x;

        return x > y ? x : y;
    }

    /// <summary>
    /// Returns the largest value among its ordered arguments (the Go 1.21 <c>max</c> built-in) — see
    /// the params <see cref="builtin.min{T}(T, ReadOnlySpan{T})"/> remarks.
    /// </summary>
    /// <param name="x">First value (Go requires at least one argument).</param>
    /// <param name="y">Remaining values to compare.</param>
    /// <returns>The maximum of <paramref name="x"/> and <paramref name="y"/>, NaN if any is NaN.</returns>
    public static T max<T>(T x, params ReadOnlySpan<T> y) where T : IComparable<T>
    {
        T result = x;

        if (OrderedFacts<T>.IsFloating)
        {
            if (OrderedFacts<T>.IsNaN(in x))
                return x;

            foreach (T value in y)
            {
                if (OrderedFacts<T>.IsNaN(in value))
                    return value;

                if (value.CompareTo(result) > 0)
                    result = value;
            }

            return result;
        }

        foreach (T value in y)
        {
            if (value.CompareTo(result) > 0)
                result = value;
        }

        return result;
    }

    /// <summary>
    /// The per-<typeparamref name="T"/> facts Go's <c>min</c>/<c>max</c> need about an ORDERED type:
    /// whether its value set contains a NaN, and how to recognize one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Go's <c>cmp.Ordered</c> admits <c>~float32 | ~float64</c>, so the floating kinds arrive both as
    /// the BCL primitives and as the generated <c>[GoType("num:floatNN")]</c> wrapper a NAMED Go float
    /// becomes. The wrapper is a single-instance-field struct over its underlying — the shape
    /// <c>NumericTypeTemplate</c> emits — so the classification walks that one field rather than
    /// parsing the attribute's definition string, which is what makes a wrapper-over-a-wrapper resolve
    /// for free.
    /// </para>
    /// <para>
    /// <b>Why not <c>x != x</c>:</b> the operator overloads could ask that (their constraint extends
    /// <see cref="System.Numerics.IEqualityOperators{T, T, TResult}"/>), but the params overloads
    /// cannot — <see cref="IComparable{T}"/> has no operators, and neither <c>CompareTo</c> nor
    /// <c>Equals</c> can see a NaN: <c>double.NaN.CompareTo(double.NaN)</c> is 0 and
    /// <c>double.NaN.Equals(double.NaN)</c> is true, both by BCL design. One mechanism serving all four
    /// overloads is what keeps the family from answering the same question two ways.
    /// </para>
    /// <para>
    /// The re-spelling is the blessed same-representation reinterpret, not a general cast: it runs only
    /// after the classification has established that <typeparamref name="T"/> is a value type whose
    /// single field chain ends at the primitive AND that the two are the same width, which is the same
    /// pair of facts <c>slice&lt;T&gt;.AliasOfElement</c>'s caller establishes before re-spelling a
    /// backing array.
    /// </para>
    /// </remarks>
    internal static class OrderedFacts<T>
    {
        // 8 = double, 4 = float, 0 = not a floating kind. A single field so the JIT folds both the
        // IsFloating gate and the width switch inside IsNaN per closed T.
        //
        // Same width or no reinterpret: a wrapper that padded or laid itself out explicitly is not one
        // representation under two names, and answering NaN off its first bytes would be a guess.
        // Reporting "not floating" there costs Go's NaN rule for a type nothing emits; reporting a
        // wrong NaN would corrupt every comparison it touched.
        private static readonly int s_floatWidth =
            Classify(typeof(T)) is int width && width != 0 && Unsafe.SizeOf<T>() == width ? width : 0;

        internal static readonly bool IsFloating = s_floatWidth != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsNaN(in T value)
        {
            return s_floatWidth switch
            {
                8 => double.IsNaN(Unsafe.As<T, double>(ref Unsafe.AsRef(in value))),
                4 => float.IsNaN(Unsafe.As<T, float>(ref Unsafe.AsRef(in value))),
                _ => false
            };
        }

        private static int Classify(Type type)
        {
            if (type == typeof(double))
                return 8;

            if (type == typeof(float))
                return 4;

            if (!type.IsValueType || type.IsPrimitive || type.IsEnum)
                return 0;

            // A generated numeric wrapper carries exactly one instance field, its underlying value.
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return fields.Length == 1 ? Classify(fields[0].FieldType) : 0;
        }
    }

    /// <summary>
    /// Gets the imaginary part of the complex number <paramref name="c"/>.
    /// </summary>
    /// <param name="c">Complex number.</param>
    /// <returns>Imaginary part of the complex number <paramref name="c"/>.</returns>
    public static float imag(complex64 c)
    {
        return c.Imaginary;
    }

    /// <summary>
    /// Gets the imaginary part of the complex number <paramref name="c"/>.
    /// </summary>
    /// <param name="c">Complex number.</param>
    /// <returns>Imaginary part of the complex number <paramref name="c"/>.</returns>
    public static double imag(complex128 c)
    {
        return c.Imaginary;
    }

    /// <summary>
    /// Gets the real part of the complex number <paramref name="c"/>.
    /// </summary>
    /// <param name="c">Complex number.</param>
    /// <returns>Real part of the complex number <paramref name="c"/>.</returns>
    public static float real(complex64 c)
    {
        return c.Real;
    }

    /// <summary>
    /// Gets the real part of the complex number <paramref name="c"/>.
    /// </summary>
    /// <param name="c">Complex number.</param>
    /// <returns>Real part of the complex number <paramref name="c"/>.</returns>
    public static double real(complex128 c)
    {
        return c.Real;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/>.
    /// </summary>
    /// <param name="array">Target array.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint len<T>(in array<T> array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/>.
    /// </summary>
    /// <param name="array">Target array.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint len<T>(T[] array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/>.
    /// </summary>
    /// <param name="array">Target array.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint len(IArray array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/>.
    /// </summary>
    /// <param name="array">Target array pointer.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint len<T>(in ж<array<T>> array)
    {
        return array.Value.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="slice"/>.
    /// </summary>
    /// <param name="slice">Target slice.</param>
    /// <returns>The length of the <paramref name="slice"/>.</returns>
    public static nint len<T>(in slice<T> slice)
    {
        return slice.Length;
    }

    /// <summary>
    /// Gets the length of the stack-only <paramref name="slice"/>.
    /// </summary>
    /// <param name="slice">Target stack slice.</param>
    /// <returns>The length of the <paramref name="slice"/>.</returns>
    public static nint len<T>(in sslice<T> slice)
    {
        return slice.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="slice"/>.
    /// </summary>
    /// <param name="slice">Target slice.</param>
    /// <returns>The length of the <paramref name="slice"/>.</returns>
    public static nint len(ISlice slice)
    {
        return slice.Length;
    }

    /// <summary>
    /// Gets the length of the byte sequence <paramref name="seq"/> (a `string | []byte`-constrained value).
    /// </summary>
    /// <param name="seq">Target byte sequence.</param>
    /// <returns>The length of the <paramref name="seq"/>.</returns>
    /// <remarks>
    /// Generic so it is only selected for an IByteSeq-constrained type parameter — overload resolution
    /// prefers the non-generic len overloads for a concrete slice/string, avoiding ambiguity with
    /// len(ISlice)/len(@string) when the argument also implements those interfaces.
    /// <para>
    /// The sequence is taken as the CONSTRAINED type parameter, not as the IByteSeq interface: an
    /// interface parameter boxes the caller's struct on every call, and `len(s)` sits in the loop
    /// condition of every union-constrained body (time's parseRFC3339 reaches it eight times per
    /// parse). Constrained like this, the call is a `constrained.` direct dispatch on the value type.
    /// </para>
    /// </remarks>
    public static nint len<TSeq>(TSeq seq) where TSeq : IByteSeq
    {
        return seq.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="slice"/>.
    /// </summary>
    /// <param name="slice">Target slice pointer.</param>
    /// <returns>The length of the <paramref name="slice"/>.</returns>
    public static nint len<T>(in ж<slice<T>> slice)
    {
        return slice.Value.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target string.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    public static nint len(in @string str)
    {
        return str.Length;
    }

    /// <summary>
    /// Gets the length of the stack string <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target stack string.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    public static nint len(in sstring str)
    {
        return str.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target string.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    public static nint len(in ReadOnlySpan<byte> str)
    {
        return str.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target string pointer.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    public static nint len(in ж<@string> str)
    {
        return str.Value.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target string.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    /// <remarks>
    /// Go's <c>len(string)</c> counts UTF-8 bytes, not UTF-16 chars — the two only agree
    /// for pure-ASCII values, so the C# <see cref="string.Length"/> shortcut is wrong for
    /// any multi-byte rune (<c>len("a☺b☻")</c> is 8, not 4).
    /// </remarks>
    public static nint len(string str)
    {
        return Encoding.UTF8.GetByteCount(str);
    }

    /// <summary>
    /// Gets the length of the <paramref name="map"/>.
    /// </summary>
    /// <param name="map">Target map.</param>
    /// <returns>The length of the <paramref name="map"/>.</returns>
    public static nint len<TKey, TValue>(in map<TKey, TValue> map) where TKey : notnull
    {
        return map.Count;
    }

    /// <summary>
    /// Gets the length of the <paramref name="map"/>.
    /// </summary>
    /// <param name="map">Target map.</param>
    /// <returns>The length of the <paramref name="map"/>.</returns>
    public static nint len(IMap map)
    {
        return map.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="map"/>.
    /// </summary>
    /// <param name="map">Target map pointer.</param>
    /// <returns>The length of the <paramref name="map"/>.</returns>
    public static nint len<TKey, TValue>(in ж<map<TKey, TValue>> map) where TKey : notnull
    {
        return map.Value.Count;
    }

    /// <summary>
    /// Gets the length of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The length of the <paramref name="channel"/>.</returns>
    public static nint len<T>(in channel<T> channel)
    {
        return channel.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The length of the <paramref name="channel"/>.</returns>
    public static nint len(IChannel channel)
    {
        return channel.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The length of the <paramref name="channel"/>.</returns>
    public static nint len<T>(in ж<channel<T>> channel)
    {
        return channel.Value.Length;
    }

    /// <summary>
    /// Allocates and initializes a new object for known types, e.g., <see cref="ISlice{T}"/>,
    /// <see cref="IMap{TKey, TValue}"/>, and <see cref="IChannel"/>.
    /// </summary>
    /// <param name="p1">First integer parameter, commonly for size.</param>
    /// <param name="p2">Second integer parameter, commonly for capacity.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <returns>New object.</returns>
    public static T make<T>(nint p1 = 0, nint p2 = -1) where T : ISupportMake<T>, new()
    {
        if (p1 == 0 && p2 == -1)
            return new T();

        return T.Make(p1, p2);
    }

    /// <summary>
    /// Allocates and initializes a new object.
    /// </summary>
    /// <param name="p1">First integer parameter, commonly for size.</param>
    /// <param name="p2">Second integer parameter, commonly for capacity.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <returns>New object.</returns>
    public static T makeǃ<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(nint p1 = 0, nint p2 = -1) where T : new()
    {
        if (p1 == 0 && p2 == -1)
            return new T();

        if (p2 != -1)
            return (T)Activator.CreateInstance(typeof(T), p1, p2)!;

        return (T)Activator.CreateInstance(typeof(T), p1)!;
    }

    /// <summary>
    /// Gets a reference to a zero value instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <returns>Reference to a zero value instance of type <typeparamref name="T"/>.</returns>
    public static ref T zero<T>()
    {
        return ref Zero<T>.Default;
    }

    /// <summary>
    /// Gets dereferenced value that <paramref name="source"/> points to.
    /// </summary>
    /// <typeparam name="T">Source type of reference.</typeparam>
    /// <param name="source">Source pointer.</param>
    /// <returns>Dereferenced value that <paramref name="source"/> points to.</returns>
    /// <remarks>
    /// This is equivalent to <c>~source</c> and <c>source.Value</c>.
    /// </remarks>
    public static T ж<T>(in ж<T> source)
    {
        return source.Value;
    }

    /// <summary>
    /// Gets a pointer to a new heap allocated copy of <paramref name="target"/> value.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target value.</param>
    /// <returns>Pointer to heap allocated copy of <paramref name="target"/> value.</returns>
    public static ж<T> Ꮡ<T>(in T target)
    {
        // a consumer type's box carries the field-view slot (ж.Views.cs)
        if (BoxShape<T>.Slotted)
            return new SlottedStandardBox<T>(target);

        return new StandardBox<T>(target);
    }

    /// <summary>
    /// Gets a pointer to slice or array element at <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target value.</param>
    /// <param name="index">Index of element.</param>
    /// <returns>Pointer to slice or array element at <paramref name="index"/>.</returns>
    /// <remarks>
    /// <paramref name="target"/> is deliberately passed BY VALUE, not <c>in</c>. It is an interface
    /// reference — <c>in</c> buys no copy elision — but it forces the caller's boxing temp (a
    /// <c>slice&lt;T&gt;</c>/<c>array&lt;T&gt;</c> header is a struct, so every call boxes one) to be
    /// ADDRESS-EXPOSED, and an address-exposed slot is not lifetime-tracked: the JIT then reports it
    /// live for the whole enclosing method, so a single <c>Ꮡ(s, i)</c> pinned <c>s</c>'s backing
    /// array to the caller's frame until that method RETURNED, even in fully optimized code. Go's
    /// <c>&amp;s[i]</c> keeps the array alive only while the pointer itself is live, which is what
    /// by-value passing restores (measured: the <c>in</c> form leaked the array, the by-value form
    /// releases it).
    /// </remarks>
    public static ж<T> Ꮡ<T>(IArray<T> target, int index)
    {
        // ONE object beyond the box: the caller's boxing temp. A slice<T>/array<T> header is a
        // struct, so `Ꮡ(s, i)` — the only shape the converter emits for Go's `&s[i]` — boxes one on
        // every call. That box is emitted at the CALL SITE rather than inside golib, but this
        // overload is the only thing it can be handed to, so charging it here attributes it exactly.
        // (A caller already holding an IArray<T> reference would be overcharged by one; the
        // converter never emits that shape, and overstating is the safe direction for a budget.)
        //
        // Native-backed slices route to the address box — see the nint overload below.
        if (target is slice<T> view && view.IsNativeBacked)
        {
            unsafe
            {
                return new NativeBox<T>(view.NativeElementAddress(index));
            }
        }

        AllocationCounter.Count();
        return new ElemRefBox<T>(target, index);
    }

    /// <summary>
    /// Gets a pointer to slice or array element at <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target value.</param>
    /// <param name="index">Index of element.</param>
    /// <returns>Pointer to slice or array element at <paramref name="index"/>.</returns>
    /// <remarks>By value, for the lifetime reason the <see cref="int"/> overload documents.</remarks>
    public static ж<T> Ꮡ<T>(IArray<T> target, nint index)
    {
        // A NATIVE-backed slice element has a real address, and the pointer to it is the
        // address-model box over exactly that address (the design table: (uintptr)Ꮡ(s,i) yields
        // the mapping, which is what Mprotect(b[:n]) hands the kernel). The managed identity
        // machinery (array-index refs, order tokens) exists for storage the GC can move; native
        // memory needs none of it.
        if (target is slice<T> view && view.IsNativeBacked)
        {
            unsafe
            {
                return new NativeBox<T>(view.NativeElementAddress(index));
            }
        }

        // The caller's boxing temp, exactly as in the int overload above.
        AllocationCounter.Count();
        return new ElemRefBox<T>(target, (int)index);
    }

    // ---- the CONCRETE-header element takes (the os want-zero residue's candidate A, 2026-09-05) ----
    //
    // A slice<T>/array<T> header is a readonly struct, so the IArray<T> overloads above cost the
    // caller a boxing temp (56 B, one counted object) beside the 64 B ElemRefBox on EVERY `&s[i]`:
    // the os row's two element sites read 120 B / 2 objects each (SUB-Q5's per-frame probe). These
    // overloads take the header itself, so nothing boxes: overload resolution prefers the identity
    // conversion over the boxing one, every existing `Ꮡ(s, i)` site binds here with no emission
    // change, and the take charges ONE object — the box — through ElemRefBox's own ctor.
    //
    // BY VALUE, not `in`, for the reason the interface overload's remark records: an
    // address-exposed header is reported live for the whole caller and pinned its backing array
    // to the frame; a 32-byte struct copy is the price and it is not an allocation. The
    // native-backed slice arm is unchanged. A NAMED slice/array type (a go2cs-gen struct wrapper
    // implementing ISlice<T>/IArray<T>) still binds the interface overloads and keeps its temp —
    // a constrained generic form cannot infer T from the wrapper — stated as the residual.

    /// <summary>
    /// Gets a pointer to slice element at <paramref name="index"/> without boxing the header.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target slice.</param>
    /// <param name="index">Index of element.</param>
    /// <returns>Pointer to slice element at <paramref name="index"/>.</returns>
    public static ж<T> Ꮡ<T>(slice<T> target, int index)
    {
        if (target.IsNativeBacked)
        {
            unsafe
            {
                return new NativeBox<T>(target.NativeElementAddress(index));
            }
        }

        // ONE object: the box (charged in its ctor). No header temp exists to charge.
        return new ElemRefBox<T>(target, index);
    }

    /// <summary>
    /// Gets a pointer to slice element at <paramref name="index"/> without boxing the header.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target slice.</param>
    /// <param name="index">Index of element.</param>
    /// <returns>Pointer to slice element at <paramref name="index"/>.</returns>
    public static ж<T> Ꮡ<T>(slice<T> target, nint index)
    {
        if (target.IsNativeBacked)
        {
            unsafe
            {
                return new NativeBox<T>(target.NativeElementAddress(index));
            }
        }

        return new ElemRefBox<T>(target, (int)index);
    }

    /// <summary>
    /// Gets a pointer to array element at <paramref name="index"/> without boxing the header.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target array.</param>
    /// <param name="index">Index of element.</param>
    /// <returns>Pointer to array element at <paramref name="index"/>.</returns>
    public static ж<T> Ꮡ<T>(array<T> target, int index)
    {
        return new ElemRefBox<T>(target, index);
    }

    /// <summary>
    /// Gets a pointer to array element at <paramref name="index"/> without boxing the header.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target array.</param>
    /// <param name="index">Index of element.</param>
    /// <returns>Pointer to array element at <paramref name="index"/>.</returns>
    public static ж<T> Ꮡ<T>(array<T> target, nint index)
    {
        return new ElemRefBox<T>(target, (int)index);
    }

    /// <summary>
    /// Creates a pointer to an ARRAY that names NATIVE memory -- Go's <c>*[N]T</c> over storage the
    /// GC does not own.
    /// </summary>
    /// <typeparam name="T">Element type of the pointed-at array.</typeparam>
    /// <param name="address">Base address of the native block. Zero is the nil pointer.</param>
    /// <param name="length">Element count, the <c>N</c> of the Go type.</param>
    /// <returns>A pointer to array over <paramref name="address"/>.</returns>
    /// <remarks>
    /// <para>
    /// The one caller is CONVERTER EMISSION, for Go's write-barrier-dodging store into a slot whose
    /// static type is a pointer to array:
    /// </para>
    /// <code language="go">
    ///     *(*uintptr)(unsafe.Pointer(&amp;p.chunks[c.l1()])) = uintptr(r)
    /// </code>
    /// <para>
    /// Go stores a raw address into a pointer slot to keep the write off the write barrier. Punching
    /// that address through a <c>uintptr</c> view of the slot writes a raw word where a managed
    /// reference belongs, and the next read of the slot reinterprets element bytes as an
    /// <see cref="array{T}"/> header -- a garbage <c>T[]</c>, and a fault with no managed frame to
    /// name it. MINTING the pointer instead is what makes the slot readable, which is the whole of
    /// Q58's write half.
    /// </para>
    /// <para>
    /// It is a door on <c>builtin</c> rather than a call to the box's own internal creator so the
    /// emission compiles in ANY converted assembly. The alternative -- naming the golib type
    /// directly -- happens to compile today because golib grants <c>InternalsVisibleTo("runtime")</c>
    /// and the shape's only site in the pinned GOROOT is in runtime; an emission rule that is correct
    /// only in one assembly is a trap for whichever package meets the shape next.
    /// </para>
    /// </remarks>
    public static ж<array<T>> NativeArrayPointer<T>(nuint address, nint length)
    {
        return NativeArrayBox<T>.Over(address, length);
    }

    /// <summary>
    /// Creates a new heap allocated instance of the zero value for type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="pointer">Out reference to pointer to heap allocated zero value.</param>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <returns>Reference to heap allocated instance of the zero value for type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This is a convenience function to allow default local struct ref and <see cref="ж{T}"/>
    /// to be created in a single call, e.g.:
    /// <code language="cs">
    ///     ref var v = ref heap(out ж&lt;Vertex&gt; Ꮡv);
    /// </code>
    /// </remarks>
    public static ref T heap<T>(out ж<T> pointer)
    {
        pointer = Ꮡ<T>(default!);
        // ValueSlot, not Value: the box was just allocated, so it is structurally a non-nil pointer —
        // the Value getter's nil-pointer-dereference check is always spurious here. For a value-type T
        // this is identical to Value; for a reference-type T (a heap-boxed pointer/slice/map local whose
        // zero value is null) it avoids a spurious panic when establishing the ref-local alias.
        return ref pointer.ValueSlot;
    }

    /// <summary>
    /// Creates a new heap allocated copy of existing <paramref name="target"/> value.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target value.</param>
    /// <param name="pointer">Out reference to pointer to heap allocated copy of <paramref name="target"/> value.</param>
    /// <returns>Reference to heap allocated copy of <paramref name="target"/> value.</returns>
    /// <remarks>
    /// This is a convenience function to allow local struct ref and <see cref="ж{T}"/>
    /// to be created in a single call, e.g.:
    /// <code language="cs">
    ///     ref var v = ref heap(new Vertex(40.68433, -74.39967), out var Ꮡv);
    /// </code>
    /// </remarks>
    public static ref T heap<T>(in T target, out ж<T> pointer)
    {
        pointer = Ꮡ(target);
        // ValueSlot, not Value: a freshly allocated box is structurally non-nil, so the Value getter's
        // nil-pointer-dereference check is spurious here (identical to Value for a value-type T; avoids a
        // spurious panic for a reference-type T whose target value is null).
        return ref pointer.ValueSlot;
    }

    /// <summary>
    /// The ж-box arc's eager field/element-address nil check (docs/phase4/DESIGN-zh-box-reduction.md
    /// §3.3, ruling §10.4): validates that <paramref name="target"/> is not a null reference before a
    /// lowered call site derives a field address from it, and hands the same reference back.
    /// </summary>
    /// <typeparam name="T">Referent type.</typeparam>
    /// <param name="target">The base reference a lowered field address is about to be derived from.</param>
    /// <returns>The same reference, guaranteed non-null.</returns>
    /// <remarks>
    /// <para>
    /// Go panics EAGERLY at <c>&amp;e.x</c> when <c>e</c> is nil — before later arguments evaluate,
    /// before the callee is entered. A lowered call site (<c>f(ref nonnil(ref e).x, …)</c>) would
    /// otherwise form an interior reference over a null base, which faults nowhere at formation
    /// (byref arithmetic does not dereference) and surfaces only at the callee's first use — after
    /// side effects Go never runs, catchable by a callee <c>recover</c> that can never fire in Go
    /// (the panel's S-F1 third-behavior refutation). One branch, zero allocation; the throw is the
    /// exact panic <see cref="ж{T}.Value"/> raises for a nil box, so the message and recoverability
    /// are unchanged.
    /// </para>
    /// <para>
    /// The converter elides the wrap where the base provably cannot be a null reference (a value
    /// local, a value parameter, an addressed global's ref property); it emits it where the base is
    /// a pointer's deref alias, whose <see cref="PointerExtensions.DerefOrNull{T}"/> binding is a
    /// null reference exactly when the pointer is nil.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T nonnil<T>(ref T target)
    {
        if (Unsafe.IsNullRef(ref target))
            throw RuntimeErrorPanic.NilPointerDereference();

        return ref target;
    }

    /// <summary>
    /// Creates a heap allocated pointer reference to a new zero value instance of type.
    /// </summary>
    /// <returns>Pointer to heap allocated zero value of provided type.</returns>
    public static ж<T> @new<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
    {
        // No `where T : new()` bound: an unconstrained Go type parameter does not carry one, so that a
        // delegate/func type argument like `atomic.Pointer[func()]` stays legal. Construct the zero value the
        // way Go's `new(T)` does: a value type is zero-initialized via its parameterless ctor (running
        // any field initializers, e.g. fixed-size array fields); a reference/delegate type is null — with
        // the one kind-family where running that ctor is NOT Go's zero carved out by GoNewZero.
        T value = GoNewZero<T>.UseDefault ? default! : Activator.CreateInstance<T>();

        // the same type gate as Ꮡ<T>(): a consumer type's box carries the field-view slot
        if (BoxShape<T>.Slotted)
            return new SlottedStandardBox<T>(value);

        return new StandardBox<T>(value);
    }

    // Whether `new(T)` must take `default(T)` rather than run T's parameterless constructor.
    //
    // Go's `new(T)` yields a pointer to the ZERO value of T, and for the three REFERENCE kinds that zero
    // is the NIL one — `p := new([]int); *p == nil` and `p := new(map[K]V); *p == nil` are both true in
    // Go. golib's container structs each declare a parameterless constructor that ALLOCATES (map<K,V>
    // makes its backing Dictionary, slice<T> takes the empty array), and Activator.CreateInstance<T>
    // honors a declared parameterless constructor — so `new(map[string]any)` and `new([]any)` handed back
    // a pointer to a non-nil EMPTY container where Go hands back nil.
    //
    // Silently, which is why it lasted: the two differ only under `== nil`, len() agrees at 0, a range
    // over either yields nothing, and encoding/json marshals them by the same branch. It surfaced through
    // reflection, where the two zero-FABRICATION paths finally met — reflect.Zero/New build a zero through
    // GoReflect.ZeroValueOf, which has always answered the NIL container for these kinds — so
    // `reflect.DeepEqual(new([]any), reflect.New(typ).Interface())` compared a nil slice against an empty
    // one and was false. That comparison is the precondition encoding/json's whole TestUnmarshal table
    // checks before every subtest, and it blocked forty-odd of them. The two rules now agree by
    // construction: this IS ZeroValueOf's classification, asked of the same KindOf.
    //
    // Every other kind must still RUN the constructor — that is what materializes a struct's fixed-size
    // ARRAY fields from the initializers the converter emits into it (`= new(4)`).
    private static class GoNewZero<T>
    {
        internal static readonly bool UseDefault =
            !typeof(T).IsValueType || GoReflect.KindOf(typeof(T)) is GoReflect.Slice or GoReflect.Map or GoReflect.Chan;
    }

    /// <summary>
    /// Creates a new reference for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="inputs">Constructor parameters.</param>
    /// <returns>New reference for <typeparamref name="T"/>.</returns>
    public static ж<T> @new<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params object[] inputs)
    {
        // Activator returns its result as object, so a value-type T arrives BOXED and is then
        // unboxed into the box below — one object beyond what the ж constructor charges. The
        // caller's params array and the boxes of its arguments are emitted at the call site and,
        // like every other compiler-emitted allocation in converted code, are not charged.
        AllocationCounter.Count();
        return new StandardBox<T>((T)Activator.CreateInstance(typeof(T), inputs)!);
    }

    /// <summary>
    /// Formats arguments in an implementation-specific way and writes the result to standard-error.
    /// </summary>
    /// <param name="args">Arguments to display.</param>
    public static void print(params object[] args)
    {
        Console.Error.Write(string.Join(" ", args.Select(printArg)));
    }

    /// <summary>
    /// Formats arguments in an implementation-specific way and writes the result to standard-error along with a new line.
    /// </summary>
    /// <param name="args">Arguments to display.</param>
    public static void println(params object[] args)
    {
        Console.Error.WriteLine(string.Join(" ", args.Select(printArg)));
    }

    // Formats a single print/println argument the way gc's runtime printer does where the BCL
    // rendering diverges: a bool prints lowercase true/false (bool.ToString() yields True/False).
    private static string? printArg(object arg)
    {
        return arg is bool value ? value ? "true" : "false" : arg.ToString();
    }

    /// <summary>
    /// Exits application with a fatal error.
    /// </summary>
    /// <param name="message">Fatal error message.</param>
    /// <param name="code">Application exit code.</param>
    public static void fatal(string message, nint code = 1)
    {
        if (!string.IsNullOrEmpty(message))
            message = $"fatal error: {message}";

    #if DEBUG
        throw new InvalidOperationException($"{message} [{code}]");
    #else
        Console.Error.WriteLine(message);
        Environment.Exit((int)code);
    #endif
    }

    // ** Type Assertion Functions **

    /// <summary>
    /// Type asserts <paramref name="target"/> object as type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Desired type for <paramref name="target"/>.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <returns><paramref name="target"/> value cast as <typeparamref name="T"/>, if successful.</returns>
    public static T _<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicFields
    )] T>(this object target)
    {
        if (TryTypeAssert(target, out T value))
            return value;

        while (target is IInterfaceAdapter interfaceAdapter)
            target = interfaceAdapter.Value!;

        throw new PanicException($"interface conversion: interface {{}} is {GetGoTypeName(target)}, not {GetGoTypeName(typeof(T))}");
    }

    /// <summary>
    /// Core non-throwing type assertion of <paramref name="target"/> object as type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Desired type for <paramref name="target"/>.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <param name="value">Value cast as <typeparamref name="T"/>, if successful; otherwise <c>default</c>.</param>
    /// <returns><c>true</c> if type assertion was successful; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This is the dispatch point for emitted type-switch case guards (<c>case {} ᴛ0 when
    /// ᴛ0._&lt;Iface&gt;(out var x):</c>), where a MISS is the normal control flow — it must not
    /// throw on the failure path.
    /// </remarks>
    private static bool TryTypeAssert<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicFields
    )] T>(object? target, out T value)
    {
        // One IGoAdapter probe gates BOTH adapter tiers — this unwrap loop and the IжAdapter match
        // below — so an ordinary Go value settles "not a wrapper" with a single failing interface
        // test instead of one per kind (see IGoAdapter). Re-read after each unwrap, since what an
        // adapter yields need not be one itself.
        bool isAdapter = target is IGoAdapter;

        while (isAdapter && target is IInterfaceAdapter interfaceAdapter)
        {
            target = interfaceAdapter.Value;
            isAdapter = target is IGoAdapter;
        }

        // A nil interface asserts as a failure, never a match (Go: 'v, ok := nil.(T)' is ok=false)
        if (target is null)
        {
            value = default!;
            return false;
        }

        Type typeOfT = typeof(T);

        switch (target)
        {
            case string str when typeOfT == typeof(@string):
                value = (T)(object)new @string(str);
                return true;
            case T typedTarget:
                value = typedTarget;
                return true;
        }

        // The CANONICAL NIL FUNC (GoReflect.CanonicalNilFunc — a nil func packed into interface
        // space) asserts to exactly its own delegate type, yielding Go's nil func: success with
        // the null delegate. Every other target type is Go's failed assertion. Placed BELOW the
        // `case T` arm so an assert to `any`/object keeps the carrier in interface space.
        if (target is NilFuncValue nilFunc)
        {
            value = default!;
            return nilFunc.Type == typeOfT;
        }

        // An interface value created from a Go POINTER (`var s Iface = &t`) is a generated
        // IжAdapter wrapping the receiver box; a type assert back to the pointer type
        // (`s.(*T)`, targeting ж<T>) unwraps the adapter to the original box, matching
        // Go's interface-holds-the-pointer semantics. Kept BELOW the two matches above: an
        // adapter that is itself a T resolves as that T, exactly as the switch ordering did.
        if (isAdapter && target is IжAdapter { Box: T box })
        {
            value = box;
            return true;
        }

        // An interface value created from a VALUE of a struct this assembly cannot partial (one
        // declared in ANOTHER assembly, or a named-func delegate) is a generated IValueAdapter
        // wrapping a copy; a type assert back to the struct type (`s.(T)`) unwraps to that copy,
        // which is the interface value's Go dynamic value. Same placement and reasoning as the
        // pointer tier above.
        if (isAdapter && target is IValueAdapter { Value: T wrapped })
        {
            value = wrapped;
            return true;
        }

        // The wrapped value is a NULL delegate (a named func type crossing an interface boundary
        // as a nil value) — Go's assert still succeeds, with a nil result (`f, ok :=
        // handler.(HandlerFunc)` is ok=true, f=nil for a typed-nil-in-interface). The pattern
        // above can never see this: a C# type pattern excludes null by design, so `Value: T
        // wrapped` fails for exactly the case it most needs to match. Recover the wrapped field's
        // DECLARED type from the adapter's own class metadata, the only channel a null value still
        // answers through (GoReflect.GoDynamicTypeOf's identical fallback).
        if (isAdapter && target is IValueAdapter { Value: null } && GoReflect.ValueAdapterWrappedType(target.GetType()) == typeOfT)
        {
            value = default!;
            return true;
        }

        if (AssertFacts<T>.IsInterface)
        {
            // The Go DYNAMIC VALUE, never the wrapper. A pointer-sourced interface value is a
            // generated IжAdapter standing in for the *T it wraps — the adapter class itself
            // carries NONE of T's methods, so probing the adapter never matches: a pointer-sourced
            // error could not assert to any dyn interface, and errors.Is's
            // `err.(interface{ Is(error) bool })` silently failed for every wrapped error. Unwrap
            // to the receiver box so every tier below sees the box's (= *T's) method set.
            // Null-guarded: an adapter holding a nil *T keeps the adapter view, since no
            // dispatchable receiver exists to build an implementation over.
            // Gated on the same probe as the tiers above — only an IGoAdapter can be an IжAdapter
            // or an IValueAdapter, so this is equivalent and costs an ordinary interface assert one
            // less failing test. A VALUE adapter unwraps for the same reason: the adapter class
            // carries only the ONE interface it was generated for, so probing it can never resolve
            // the wrapped struct's other interfaces (image's color.Color value could not assert to
            // any dyn interface, and `ColorModel().(color.Palette)` failed for image/gif).
            object subject = isAdapter ? UnwrapAdapter(target) : target;
            Type subjectType = subject.GetType();
            int epoch = AdapterRegistry.Epoch;

            // Go's itab cache: ONE entry per (dynamic type, interface), whichever tier produced it.
            // Everything below this block is the FILL, and runs at most once per pair per epoch.
            if (Itab<T>.TryGetResolver(subjectType, epoch, out Func<object, object>? resolve))
            {
                if (resolve is not null && resolve(subject) is T resolved)
                {
                    value = resolved;
                    return true;
                }

                value = default!;
                return false;
            }

            // NAMED-interface resolution by Go METHOD-SET semantics: a raw receiver box (ж<X>)
            // matches an interface that *X implements — re-wrap it in the generated pointer
            // adapter its module initializer registered. An IжAdapter asserting to a DIFFERENT
            // interface re-wraps its box the same way (Go re-derives the target interface from
            // the dynamic type *X). This nominal tier keeps its precedence over the shell tier.
            if (AdapterRegistry.TryGetAdapterFactory(subjectType, typeOfT, out Func<object, object>? nominal) &&
                nominal(subject) is T nominalValue)
            {
                Itab<T>.Record(subjectType, epoch, nominal);
                value = nominalValue;
                return true;
            }

            // The structural probe matched but no compile-time adapter exists for the pair. For a
            // NAMED interface that is the case the GoImplement recorders CANNOT cover — a dynamic
            // type in an assembly converted after the interface's own (io/fs before os) — and for
            // an ANONYMOUS one it is the normal case, since a Go type never nominally implements an
            // interface literal. Both are answered the same way: build the implementation at run
            // time from the shells go2cs-gen emitted beside the interface. This tier fires only
            // where the tier above answered MISS, so it cannot regress an assertion that already
            // resolved.
            if (Implements<T>(subject) && AdapterBinder.TryCreate(subject, typeOfT, out object? shell) && shell is T shellValue)
            {
                // Read the decision back from AdapterRegistry — the durable record — rather than
                // forming a second one, so the projection can never disagree with it. If the
                // read-back finds nothing the pair simply stays undecided and is re-formed on next
                // use, which is what the previous per-interface projection did too.
                if (AdapterRegistry.TryGetShellFactory(subjectType, typeOfT, out Func<object, object>? shellFactory) && shellFactory is not null)
                    Itab<T>.Record(subjectType, epoch, shellFactory);

                value = shellValue;
                return true;
            }

            // A decided MISS is memoized exactly like a hit — building a shell costs ~1 µs and fmt
            // probes three interfaces per formatted value, so the negative is what keeps the runtime
            // tier off the hot path. Recording null (rather than reading a factory back) is also
            // what makes the miss STABLE: a factory that exists but throws its binding failure out
            // of construction answers false here, and reading that factory back would have made the
            // NEXT assert re-throw instead of answering false again.
            Itab<T>.Record(subjectType, epoch, null);
            value = default!;
            return false;
        }

        // Only dynamic, unnamed types can be converted to each other in Go, so an assert to a NAMED
        // struct — the ordinary `v, ok := x.(T)` — is already decided a MISS here. Testing that from
        // the hoisted per-T fact keeps the miss off IsDynamicType, whose custom-attribute lookup is
        // ~780 ns and 368 bytes a call: PerfIface's assert misses 4 of every 6 iterations and that
        // one call was ~99% of the benchmark's runtime.
        if (AssertFacts<T>.IsValueType && AssertFacts<T>.IsDynamic)
        {
            Type targetType = target.GetType();

            if (targetType.IsValueType && targetType.IsDynamicType())
            {
                ImmutableHashSet<string> typeOfTFieldNames = typeOfT.GetStructFieldNames();

                // Check if target type has the same fields as the asserted type
                if (targetType.GetStructFieldNames().Except(typeOfTFieldNames).Count == 0)
                {
                    // Create a new instance of the asserted type — held BOXED for the copy:
                    // FieldInfo.SetValue on an unboxed struct mutates a fresh transient box per
                    // call and the writes vanish (the copy came back all zeros).
                    object newInstance = Activator.CreateInstance(typeOfT)!;

                    // Copy the values of the fields from the target to the new instance. The
                    // lookup must span NONPUBLIC fields: an unexported Go field emits `internal`,
                    // so the public-only GetField(string) answered null and the copy of any
                    // struct with a lowercase field NRE'd — reflectlite's TestBigUnnamedStruct
                    // (`struct{ a, b, c, d int64 }` lifted twice by two spellings) was the
                    // measured caller of both halves.
                #pragma warning disable IL2075
                    const BindingFlags CopyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                    foreach (string field in typeOfTFieldNames)
                        typeOfT.GetField(field, CopyFlags)!.SetValue(newInstance, targetType.GetField(field, CopyFlags)!.GetValue(target));
                #pragma warning restore IL2075

                    value = (T)newInstance;
                    return true;
                }
            }
        }

        value = default!;
        return false;
    }

    // Per-closed-generic facts about the asserted type. typeof(T).IsInterface and .IsValueType are
    // RuntimeType property CALLS on the assert hot path; hoisting them here turns each into a static
    // field read the JIT folds to a constant for the closed instantiation. Same shape (and same
    // reasoning) as Cache<TInterface> below.
    private static class AssertFacts<T>
    {
        internal static readonly bool IsInterface = typeof(T).IsInterface;
        internal static readonly bool IsValueType = typeof(T).IsValueType;
        internal static readonly bool IsDynamic = typeof(T).IsDynamicType();
    }

    // Go's itab cache, projected per closed interface: ONE entry per (dynamic type, interface),
    // whichever tier produced it — a registered nominal adapter factory, a runtime duck-typing shell
    // factory, or null for a decided MISS. It replaces consulting the nominal (Type, Type) registry
    // and a separate per-interface shell memo in sequence on every assert, which cost two dictionary
    // lookups (the first of which could never hit for a shell-resolved pair) and three GetType()
    // calls. AdapterRegistry remains the authoritative durable record — every decision is FORMED
    // there and only projected here.
    private static class Itab<TInterface>
    {
        // Dynamic type + resolver + the registration epoch they were decided under, published as ONE
        // immutable object. Two separate static fields could be observed TORN across threads — type
        // A paired with resolver B would silently construct the WRONG implementation — and carrying
        // the epoch inside the entry lets a late registration invalidate every entry at once with no
        // clearing step that could race a concurrent fill (a stale entry is simply never matched,
        // and is overwritten the next time its pair is formed).
        private sealed class Entry(int epoch, Type type, Func<object, object>? resolve)
        {
            internal readonly int Epoch = epoch;
            internal readonly Type Type = type;
            internal readonly Func<object, object>? Resolve = resolve;
        }

        private static readonly ConcurrentDictionary<Type, Entry> s_entries = [];

        // Monomorphic slot in front of the dictionary: an assert site is overwhelmingly single-typed,
        // the same locality assumption Go's compiler-emitted per-site type checks rely on. A hit is a
        // static field read, an int compare and a reference compare; a miss costs one extra reference
        // compare before the dictionary.
        private static Entry? s_last;

        public static bool TryGetResolver(Type dynamicType, int epoch, out Func<object, object>? resolve)
        {
            Entry? last = Volatile.Read(ref s_last);

            if (last is not null && last.Epoch == epoch && ReferenceEquals(last.Type, dynamicType))
            {
                resolve = last.Resolve;
                return true;
            }

            if (s_entries.TryGetValue(dynamicType, out Entry? entry) && entry.Epoch == epoch)
            {
                Volatile.Write(ref s_last, entry);
                resolve = entry.Resolve;
                return true;
            }

            resolve = null;
            return false;
        }

        public static void Record(Type dynamicType, int epoch, Func<object, object>? resolve)
        {
            Entry entry = new(epoch, dynamicType, resolve);
            s_entries[dynamicType] = entry;
            Volatile.Write(ref s_last, entry);
        }
    }

    // Open generic definition of TryTypeAssert<T>, resolved once for the runtime-Type-driven
    // overload below (the two TryTypeAssert members are disambiguated by generic arity).
    private static readonly MethodInfo s_tryTypeAssertDefinition = typeof(builtin)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
        .Single(static method => method.Name == nameof(TryTypeAssert) && method.IsGenericMethodDefinition);

    private static readonly ConcurrentDictionary<Type, MethodInfo> s_tryTypeAssertByType = new();

    /// <summary>
    /// Non-generic type assertion of <paramref name="target"/> to a runtime-only interface
    /// <paramref name="type"/>, with the full duck-typed machinery of the generic form.
    /// </summary>
    /// <param name="target">Source value to type assert.</param>
    /// <param name="type">Target interface type, known only at run time.</param>
    /// <param name="value">Asserted value implementing <paramref name="type"/>, if successful.</param>
    /// <returns><c>true</c> if the assertion succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The reflection bridge's <c>Value.Set</c> into an interface-typed destination only knows the
    /// destination interface as a <see cref="Type"/> — this routes it through the same assert
    /// machinery emitted <c>_&lt;T&gt;</c> sites use, so reflection and direct asserts can never
    /// disagree about a method set. See docs/phase4/DESIGN-reflection-bridge.md.
    /// </remarks>
    public static bool TryTypeAssert(object? target, Type type, [NotNullWhen(true)] out object? value)
    {
        MethodInfo closedAssert = s_tryTypeAssertByType.GetOrAdd(type, static assertedType =>
            s_tryTypeAssertDefinition.MakeGenericMethod(assertedType));

        object?[] args = [target, null];
        bool matched = (bool)closedAssert.Invoke(null, args)!;

        value = matched ? args[1] : null;
        return matched && value is not null;
    }

    /// <summary>
    /// Attempts type assert of <paramref name="target"/> object as type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Desired type for <paramref name="target"/>.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꟷ"/>.</param>
    /// <returns>Tuple of <paramref name="target"/> value cast as <typeparamref name="T"/> and success boolean.</returns>
    public static (T?, bool) _<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicFields
    )] T>(this object target, bool _)
    {
        return TryTypeAssert(target, out T? value) ? (value, true) : (default, false);
    }

    /// <summary>
    /// Attempts type assert of <paramref name="target"/> object as type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Desired type for <paramref name="target"/>.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <param name="value">Value cast as <typeparamref name="T"/>, if successful; otherwise <c>default</c>.</param>
    /// <returns><c>true</c> if type assertion was successful; otherwise, <c>false</c>.</returns>
    public static bool _<[DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.PublicFields
    )] T>(this object target, out T? value)
    {
        return TryTypeAssert(target, out value);
    }

    /// <summary>
    /// Gets common Go type for given <paramref name="target"/>.
    /// </summary>
    /// <param name="target">Target value.</param>
    /// <returns>Common Go type for given <paramref name="target"/>.</returns>
    /// <remarks>
    /// This is the type-switch operand (<c>switch v.(type)</c> emits <c>switch (v.type())</c>):
    /// the value it returns is what the emitted C# <c>case</c> patterns match against, so it must
    /// surface the Go DYNAMIC value, not the C#-only wrapper classes the runtime uses to carry it.
    /// </remarks>
    public static object type(this object target)
    {
        // Both adapter tiers below are gated on ONE IGoAdapter probe: an ordinary Go value is
        // neither kind, and settling that with a single failing interface test rather than one per
        // kind is what keeps a type switch off the ~3 ns-per-probe cost (see IGoAdapter).
        if (target is IGoAdapter)
        {
            // Go interface-to-interface assignment preserves the original dynamic value — unwrap the
            // generated interface adapters so case patterns see that value (mirrors the type-assert
            // machinery in _<T>).
            while (target is IInterfaceAdapter { Value: not null } interfaceAdapter)
                target = interfaceAdapter.Value;

            // An interface value created from a Go POINTER (`var s Iface = &t`) is a generated
            // IжAdapter wrapping the receiver box — its Go dynamic type is the pointer itself,
            // so a `case *T:` label (emitted `case ж<T> t:`) must match the box. One created from
            // a VALUE this assembly cannot partial is a generated IValueAdapter wrapping a copy,
            // whose Go dynamic type is the struct — so `case color.NRGBA:` must match that copy.
            // A null Box (interface holding a nil *T) stays wrapped: Go would still match
            // `case *T:` there, but no C# type pattern can bind null — the adapter at least keeps
            // `case null:` (Go `case nil:`) a non-match, since such an interface value is NOT nil.
            target = UnwrapAdapter(target);
        }

        // Infer common go type as needed
        return target is string str ? new @string(str) : target;
    }

    /// <summary>
    /// Gets the Go DYNAMIC VALUE a generated interface-implementation adapter stands in for: the
    /// receiver box of a POINTER-sourced adapter, or the wrapped copy of a VALUE-sourced one.
    /// </summary>
    /// <remarks>
    /// Returns <paramref name="target"/> unchanged when it carries neither — an adapter holding a
    /// nil <c>*T</c> keeps the adapter view, since no dispatchable receiver exists to stand in for
    /// it. Callers gate this behind one <see cref="IGoAdapter"/> probe, so an ordinary Go value
    /// never reaches it (see <see cref="IGoAdapter"/> for why that gate matters).
    /// </remarks>
    private static object UnwrapAdapter(object target)
    {
        return target switch
        {
            IжAdapter { Box: not null } pointerAdapter => pointerAdapter.Box,
            IValueAdapter { Value: not null } valueAdapter => valueAdapter.Value,
            _ => target
        };
    }

    // ** Conversion Functions **

    /// <summary>
    /// Converts C# <paramref name="source"/> array to Go <see cref="go.slice{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of array.</typeparam>
    /// <param name="source">C# source array.</param>
    /// <returns>Go <see cref="go.slice{T}"/> wrapper for C# <paramref name="source"/> array.</returns>
    public static slice<T> slice<T>(T[] source)
    {
        return source;
    }

    /// <summary>
    /// Converts a C# read-only <paramref name="source"/> span to a Go <see cref="go.slice{T}"/> over a
    /// detached copy of its elements.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">C# source span — most importantly a <c>"…"u8</c> literal (a zero-allocation
    /// static ROM <see cref="ReadOnlySpan{T}"/>).</param>
    /// <returns>Go <see cref="go.slice{T}"/> holding a copy of <paramref name="source"/>.</returns>
    /// <remarks>
    /// This is the emission for a <c>[]byte("…")</c> conversion of a plain-text string literal: the
    /// <c>u8</c> ROM span feeds the slice directly (<c>slice&lt;byte&gt;("…"u8)</c>), skipping the
    /// intermediate heap <see cref="@string"/> the older <c>slice&lt;byte&gt;((@string)"…")</c> form
    /// allocated. Not ambiguous with the <c>T[]</c> overload above: a <c>u8</c> literal is a span (exact
    /// match here), an <see cref="@string"/> converts to <c>byte[]</c> (exact match there) but not to a
    /// span, and a <c>byte[]</c> binds the array overload.
    /// </remarks>
    public static slice<T> slice<T>(ReadOnlySpan<T> source)
    {
        return new slice<T>(source);
    }

    /// <summary>
    /// Projects a slice's elements into a widened target type via the supplied conversion.
    /// </summary>
    /// <typeparam name="T">Source element type.</typeparam>
    /// <typeparam name="TWide">Widened target element type, e.g., an interface type.</typeparam>
    /// <param name="source">Source slice.</param>
    /// <param name="conv">Element widening conversion.</param>
    /// <returns>New <see cref="go.slice{T}"/> of <typeparamref name="TWide"/> over the projected elements.</returns>
    /// <remarks>
    /// This is the emission for a Go generic call whose interface-constrained type argument is a
    /// pointer type — <c>walkList(v, n.Names)</c> instantiating <c>walkList[N Node]</c> with
    /// <c>N=*Ident</c>: the C# <c>ж&lt;Ident&gt;</c> box does not implement the interface (its
    /// generated pointer adapter does), so the slice is projected element-wise through the
    /// adapter. Only the slice header is copied; elements alias the original objects through the
    /// shared boxes.
    /// <para>
    /// It is also the emission for a string ↔ byte/rune-slice CONVERSION whose element type is a
    /// DEFINED type — <c>[]Uint8("hello")</c>, <c>string(b)</c> over a <c>[]myByte</c>. There
    /// <c>slice&lt;byte&gt;</c> and <c>slice&lt;myByte&gt;</c> are unrelated generic instantiations
    /// with no conversion between them, so the elements are projected one at a time through the
    /// element wrapper's own operator. "Widen" is the wrong word for the narrowing direction, but
    /// the operation is the same one and a second primitive would only duplicate it; Go's
    /// string↔slice conversion always materializes fresh storage, so the copy is faithful there.
    /// </para>
    /// </remarks>
    public static slice<TWide> widen<T, TWide>(slice<T> source, Func<T, TWide> conv)
    {
        // Preserve the source's nil-vs-empty identity: only a NIL source projects to nil — a
        // non-nil empty slice projects to a non-nil empty (the old zero-length early return
        // manufactured nil from an empty, observable through `== nil` in the instantiated callee).
        if (source == nil)
            return default;

        TWide[] result = new TWide[source.Length];

        for (int i = 0; i < source.Length; i++)
            result[i] = conv(source[i]);

        return new slice<TWide>(result);
    }


    /// <summary>
    /// Converts value to a complex64 imaginary number.
    /// </summary>
    /// <param name="imaginary">Value to convert to imaginary.</param>
    /// <returns>New complex number with specified <paramref name="imaginary"/> part and a zero value real part.</returns>
    /// <remarks>
    /// An extension method so converted code renders a Go imaginary literal in postfix form,
    /// e.g. Go <c>3.5i</c> in a complex64 context emits <c>3.5F.i()</c> — member access cannot
    /// be shadowed by a local named <c>i</c>, unlike a bare <c>i(…)</c> call. The static call
    /// form <c>builtin.i(3.5F)</c> remains valid.
    /// </remarks>
    public static complex64 i(this float imaginary)
    {
        return new complex64(0.0F, imaginary);
    }

    /// <summary>
    /// Converts value to a complex128 imaginary number.
    /// </summary>
    /// <param name="imaginary">Value to convert to imaginary.</param>
    /// <returns>New complex number with specified <paramref name="imaginary"/> part and a zero value real part.</returns>
    /// <remarks>
    /// An extension method so converted code renders a Go imaginary literal in postfix form,
    /// e.g. Go <c>3.5i</c> emits <c>3.5D.i()</c> — member access cannot be shadowed by a local
    /// named <c>i</c>, unlike a bare <c>i(…)</c> call. The static call form
    /// <c>builtin.i(3.5D)</c> remains valid.
    /// </remarks>
    public static complex128 i(this double imaginary)
    {
        return new complex128(0.0D, imaginary);
    }

    /// <summary>
    /// Creates a TRANSIENT Go string that ALIASES <paramref name="source"/>'s backing bytes without
    /// copying — the go2cs form of the Go compiler's <c>m[string(b)]</c> map-lookup special case
    /// (<c>runtime.slicebytetostringtmp</c>), where the conversion's result provably does not outlive
    /// the expression, so no copy is needed. Zero allocation, matching Go's cost model for the idiom.
    /// </summary>
    /// <param name="source">Byte slice whose backing the transient string aliases.</param>
    /// <returns>A <see cref="go.@string"/> WINDOW over <paramref name="source"/>'s live storage.</returns>
    /// <remarks>
    /// ⚠ The result shares MUTABLE storage with <paramref name="source"/> — @string's immutability
    /// holds only because the value is consumed before the slice can next be written. The converter
    /// emits this solely where Go's compiler applies the same optimization (a map-index READ key,
    /// which is hashed and compared but never retained); every path where the string ESCAPES — a
    /// return, a store, a map WRITE key — keeps the copying conversion. Do not use from hand-written
    /// code unless the value dies within the consuming expression.
    /// </remarks>
    public static @string tmpstring(in slice<byte> source)
    {
        return go.@string.TransientAliasOf(source);
    }

    /// <summary>
    /// Compares two objects for equality.
    /// </summary>
    /// <param name="left">Left object.</param>
    /// <param name="right">Right object.</param>
    /// <returns><c>true</c> if both objects are equal; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// When both objects being compared are interface references, this method will match Go's behavior for comparing
    /// interfaces , i.e., two interface values are considered equal if they have identical dynamic types and equal
    /// dynamic values or if both have value nil. Since the type being referenced by the interface is not known at
    /// compile time, this method uses reflection to get the equality operator for the type and calls it if available.
    /// If the equality operator is not found, the default <see cref="object.Equals(object, object)"/> method is used.
    /// </remarks>
    /// <summary>
    /// Fast-path generic equality for same-typed operands — the form emitted inside generic
    /// bodies (a `comparable`-constrained Go type parameter erases to no C# constraint, and
    /// `==` on it routes here). For a VALUE-type <typeparamref name="T"/> the JIT specializes
    /// this method per type and devirtualizes <see cref="EqualityComparer{T}.Default"/> to the
    /// type's own <see cref="IEquatable{T}"/> implementation — operator-comparable speed with
    /// no reflection or boxing (golib wrappers emit `operator ==` and `Equals` as consistent
    /// pairs, so semantics match the reflective path). Reference and interface type arguments
    /// delegate to the reflective overload below, preserving its typed-null and runtime-type
    /// semantics; C# prefers the non-generic overload for plain `object` operands, so the
    /// delegation cannot recurse.
    /// </summary>
    /// <returns><c>true</c> if operands are equal; otherwise, <c>false</c>.</returns>
    public static bool AreEqual<T>(T? left, T? right)
    {
        // Go's `==` on floating-point follows IEEE-754: NaN compares unequal to everything,
        // itself included. EqualityComparer instead routes through the type's Equals, which
        // reports NaN equal to NaN (double/float bitwise-style Equals; complex componentwise
        // Equals) — that inverted every generic NaN probe of the `x != x` form (cmp.isNaN,
        // the basis of slices.Sort's NaN-first ordering). The typeof guards are JIT-time
        // constants per value-type instantiation, so each branch compiles to the bare
        // operator compare with the box-cast elided.
        if (typeof(T) == typeof(double))
            return (double)(object)left! == (double)(object)right!;

        if (typeof(T) == typeof(float))
            return (float)(object)left! == (float)(object)right!;

        if (typeof(T) == typeof(complex128))
            return (complex128)(object)left! == (complex128)(object)right!;

        if (typeof(T) == typeof(complex64))
            return (complex64)(object)left! == (complex64)(object)right!;

        return default(T) is null ?
            AreEqual(left, (object?)right) :
            EqualityComparer<T>.Default.Equals(left!, right!);
    }

    public static bool AreEqual(object? left, object? right)
    {
        // An interface created from an interface is a generated IInterfaceAdapter wrapping
        // the original interface value; Go compares such interfaces by instance value.
        // Unwrap both sides so the comparison below sees the root values.
        while (left is IInterfaceAdapter leftInterfaceAdapter)
            left = leftInterfaceAdapter.Value;

        while (right is IInterfaceAdapter rightInterfaceAdapter)
            right = rightInterfaceAdapter.Value;

        // An interface value created from a Go POINTER is a generated IжAdapter wrapping the
        // receiver box; Go compares such interfaces (against each other, or against a raw
        // pointer) by POINTER IDENTITY. Unwrap both sides so the comparison below sees the
        // boxes themselves (ж<T>.Equals is identity-based).
        if (left is IжAdapter leftAdapter)
            left = leftAdapter.Box;

        if (right is IжAdapter rightAdapter)
            right = rightAdapter.Box;

        // An interface value created from a VALUE this assembly cannot partial is a generated
        // IValueAdapter wrapping a copy; Go compares it by the DYNAMIC TYPE and value it carries.
        // Unwrap both sides BEFORE the type check below — an adapter compared against the raw
        // struct (image/draw's `got != want` over color.NRGBA) is a type MISMATCH as long as one
        // side is still the wrapper class, so it answered false without ever reaching an Equals.
        // Null-guarded, unlike the pointer tier (whose Box is never null — a generated pointer
        // adapter substitutes ж<T>.NilBox): an adapter over a NIL named-func delegate keeps its
        // wrapper view, so `v == nil` stays FALSE for it exactly as in Go, where an interface
        // holding a nil func value is not a nil interface.
        if (left is IValueAdapter { Value: not null } leftValueAdapter)
            left = leftValueAdapter.Value;

        if (right is IValueAdapter { Value: not null } rightValueAdapter)
            right = rightValueAdapter.Value;

        // Two adapters BOTH still wrapping a nil delegate (neither matched the guard above) are
        // exactly Go's "comparing uncomparable type" case: func values are comparable only to
        // nil, never to each other — and that holds even when both sides are nil (verified
        // against go1.23.12: two independently-nil-wrapped values of one named func type panic
        // on `==`, where either compared to a literal nil correctly answers false via the null
        // checks below). Left unguarded, this reaches the shells' generated Equals override,
        // which calls m_value.Equals(...) on a null m_value and crashes — the general
        // exception-to-panic translation downstream then reports Go's WRONG panic ("invalid
        // memory address or nil pointer dereference" instead of "comparing uncomparable type"),
        // which is what this check preempts. Rhymes with the registererr chip's Defect B
        // (reflectPointerToken): the same null-Value shell reaching a path built only for the
        // non-null shape.
        if (left is IValueAdapter { Value: null } && right is IValueAdapter { Value: null } &&
            GoReflect.ValueAdapterWrappedType(left.GetType()) is { } leftWrapped &&
            leftWrapped == GoReflect.ValueAdapterWrappedType(right.GetType()))
        {
            throw RuntimeErrorPanic.ComparingUncomparableType(GetGoTypeName(leftWrapped));
        }

        // Check if both are null
        if (left is null && right is null)
            return true;

        // Check if one is null
        if (left is null || right is null)
            return false;

        Type leftType = left.GetType();

        // Check if both are the same type
        if (leftType != right.GetType())
            return false;

        // Go's `==` on two interface values PANICS when their shared dynamic type has no equal
        // algorithm — "runtime error: comparing uncomparable type map[string]int" — and the C#
        // answered quietly instead, for every shape but the nil-func-adapter one handled above.
        // Measured against go1.23.12: a map, slice or func held in an interface; a struct or an
        // array that TRANSITIVELY contains one; and a named type whose underlying type is any of
        // those, which panics under its OWN name (`main.myMap`, not `map[string]int`).
        //
        // ORDERING is load-bearing and mirrors the runtime's, which is also what makes this safe
        // against the ~1,300 emitted call sites: both nil legs and the dynamic-type mismatch above
        // answer WITHOUT panicking (`m == nil` is false, `m == someInt` is false — both verified in
        // Go), so this is reached only once Go itself would have run the type's equal algorithm and
        // found none. The converter emits AreEqual only for a comparison Go's own type checker
        // admitted, so a panic raised here is a panic Go raises too.
        if (!isComparableType(leftType))
            throw RuntimeErrorPanic.ComparingUncomparableType(uncomparableTypeName(leftType, left));

        // Get equality "==" operator for type using reflection,
        // lookup is cached for performance.
        // (This already yields Go's IEEE-754 `==` for boxed floating-point values: on .NET 7+
        // the primitives DECLARE op_Equality — the IEqualityOperators implementation — so the
        // lookup finds the real operator and never falls to double/float.Equals, whose NaN
        // self-equality would diverge from Go. Complex128/complex64 likewise declare
        // componentwise-IEEE operators. Guarded by ReverseSortNaNOrder's boxed-`any` NaN leg.)
        MethodInfo? equalityOperator = leftType.GetEqualityOperator();

        // If equality operator is not found, use default object.Equals
        if (equalityOperator is null)
            return left.Equals(right);

        // Call equality operator.
        //
        // DoNotWrapExceptions is load-bearing, not a micro-optimization: this operator is Go's own
        // equality for the type, so a panic raised INSIDE it is a Go panic and must reach the
        // caller as one. A struct with an interface-typed FIELD is the case that proves it — the
        // generated Equals compares such a field back through AreEqual (TypeGenerator routes it
        // there deliberately, since C# `==` on an interface operand is reference identity), so
        // `struct{ V any }` holding a map panics one frame down, INSIDE this invoke. Left wrapped,
        // reflection delivers a TargetInvocationException instead, which recover() does not match
        // (RuntimeErrorPanic.TryAsPanic adopts PanicException, DivideByZeroException and
        // NullReferenceException — not that wrapper), so the panic escaped every `defer func(){
        // recover() }()` and killed the process where Go recovers cleanly. Measured against
        // go1.23.12: `struct{any=map}` recovers there and, before this flag, exited 2 here with an
        // unrecovered traceback through InterpretedInvoke_Method.
        //
        // The flag is preferred over unwrapping the wrapper afterwards because it keeps ONE
        // exception identity end to end — nothing to re-throw, no stack to lose, and the
        // uncomparable panic minted above propagates as itself whether it is raised in this frame
        // or in a nested one.
        return (bool)equalityOperator.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [left, right], null)!;
    }

    // Comparability is a pure function of the managed Type and immutable for the life of the
    // process, so the field walk GoReflect.StructFieldsComparable performs is done ONCE per type
    // and read back from here afterwards. `==` is a hot path — the corpus emits AreEqual on
    // roughly every `err == io.EOF` in the standard library — and an uncached verdict would
    // re-reflect over a converted struct's fields on every comparison. A ConcurrentDictionary
    // probe is a lock-free read for a present key, which is noise beside the reflective
    // operator Invoke this gate sits immediately in front of.
    //
    // Deliberately delegating to GoReflect.IsComparable rather than restating Go's rule: that
    // method is already the comparability signal the reflection bridge populates abi.Type.Equal
    // from (reflect.Type.Comparable and reflectlite's Comparable both read it), so `==` and
    // reflect answering from ONE definition is what keeps them from drifting apart.
    private static readonly ConcurrentDictionary<Type, bool> s_comparableTypes = [];

    private static bool isComparableType(Type type)
    {
        return s_comparableTypes.GetOrAdd(type, static t => GoReflect.IsComparable(t));
    }

    // Go names the offending type in the message exactly as `%T` spells it, and for an ARRAY that
    // includes the length — `[1][]int`, never `[][]int`. A managed array<T> does not carry its
    // length in its Type (only the value knows), so the dims come off the live operand through the
    // same recovery reflect's descriptor synthesis uses. Every other kind is spelled by the type
    // alone. GoReflect.GoTypeName is the renderer rather than builtin.GetGoTypeName because the
    // latter's fallback hands back the RAW managed name for a bare container — `go.slice`1[[...]]`
    // where Go says `[]int` — since a slice is neither a pointer box, an adapter, nor nested in a
    // `<pkg>_package` class.
    private static string uncomparableTypeName(Type type, object value)
    {
        return GoReflect.KindOf(type) == GoReflect.Array ?
            GoReflect.GoTypeName(type, GoReflect.ArrayDimsOfValue(value)) :
            GoReflect.GoTypeName(type);
    }

    /// <summary>
    /// Checks if the specified <paramref name="value"/> implements the specified interface <typeparamref name="TInterface"/>.
    /// </summary>
    /// <typeparam name="TInterface">Interface type to check.</typeparam>
    /// <param name="value">Object to check for interface implementation.</param>
    /// <returns><c>true</c> if the specified <paramref name="value"/> implements the specified interface <typeparamref name="TInterface"/>; otherwise, <c>false</c>.</returns>
    public static bool Implements<TInterface>(object? value)
    {
        while (value is IInterfaceAdapter interfaceAdapter)
            value = interfaceAdapter.Value;

        return value switch
        {
            TInterface => true,
            null => false,
            _ => Cache<TInterface>.Implements(value.GetType())
        };
    }

    // Per-interface structural-implementation cache, reached from the `_ =>` arm of Implements above.
    //
    // KEEP — a dead-code scan will flag this and be wrong. The only call site names the type through a
    // type parameter (`Cache<TInterface>.Implements(...)`), so a search for the literal text
    // "Cache<" finds the definition and one use that reads like part of the same declaration.
    // It is live on the structural duck-typing path every dynamic type assert takes, and the
    // AnonInterfaceSignatureAssert behavioral test exercises it by name.
    //
    // Keying on the CLOSED generic type is the optimization: the JIT specializes a distinct static
    // dictionary per interface, so the run-time check is a single value-Type-keyed lookup with no
    // composite-key hashing and no cross-interface contention — where a shared
    // ConcurrentDictionary<(Type, Type), bool> made every interface share one lock-striped table and
    // hash a tuple on each probe. The value-type dimension stays a dictionary because one
    // Implements<TInterface> instantiation is exercised against many runtime types.
    private static class Cache<TInterface>
    {
        private static readonly ConcurrentDictionary<Type, bool> s_results = [];

        // Run-time structural check (Go duck-typing). Encountered for dynamically defined interface types
        // or when a function checks a private library interface internally. Results are cached, but there
        // is an initial cost for lookup creation.
        // Matches each interface method by NAME *and* SIGNATURE (parameter + return types), scoped to the
        // value's actual Go method set — so `interface{ Unwrap() error }` and `interface{ Unwrap() []error }`
        // stay distinct, and a pointer value ж<X> is not matched against the pointer methods of other types.
        // FUTURE: Consider if there's a way to do this at transpile time for increased performance
        public static bool Implements(Type valueType)
        {
            return s_results.GetOrAdd(valueType, static vt => vt.StructurallyImplements(typeof(TInterface)));
        }
    }

    /// <summary>
    /// Gets the common Go type name for the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to evaluate.</param>
    /// <returns>Common Go type name for the specified <paramref name="value"/>.</returns>
    public static string GetGoTypeName(object? value)
    {
        return value switch
        {
            // A generated pointer-sourced interface adapter stands in for the *T it wraps —
            // Go's dynamic type is the pointer, never the adapter class.
            IжAdapter adapter => GetGoTypeName(adapter.Box),
            // A value-sourced adapter stands in for the struct copy it wraps.
            IValueAdapter adapter => GetGoTypeName(adapter.Value),
            // An interface-to-interface adapter preserves the original dynamic value.
            IInterfaceAdapter adapter => GetGoTypeName(adapter.Value),
            _ => GetGoTypeName(value?.GetType())
        };
    }

    /// <summary>
    /// Gets the common Go type name for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Target type</param>
    /// <returns>Common Go type name for the specified <paramref name="type"/>.</returns>
    public static string GetGoTypeName(Type? type)
    {
        if (type is null)
            return "nil";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.String => "string",
            TypeCode.Char => "rune",
            TypeCode.Boolean => "bool",
            TypeCode.SByte => "int8",
            TypeCode.Int16 => "int16",
            TypeCode.Int32 => "int",
            TypeCode.Int64 => "int64",
            TypeCode.Byte => "byte",
            TypeCode.UInt16 => "uint16",
            TypeCode.UInt32 => "uint32",
            TypeCode.UInt64 => "uint64",
            TypeCode.Single => "float32",
            TypeCode.Double => "float64",
            _ => handleDefault()
        };

        string handleDefault()
        {
            string typeName = type.FullName ?? type.Name;

            switch (typeName)
            {
                case "go.string": return "string";
                case "System.IntPtr": return "int";
                // uintptr is a distinct struct (go.uintptr); a bare System.UIntPtr is Go's uint
                case "System.UIntPtr": return "uint";
                case "go.uintptr": return "uintptr";
                case "System.Numerics.Complex": return "complex128";
                case "go.complex64": return "complex64";
            }

            if (type == typeof(object))
                return "interface {}";

            // A pointer box renders as Go's *T, a generated interface adapter as the Go dynamic
            // type it stands in for, and a converted named type package-qualifies (main.Point) —
            // GoReflect.GoTypeName covers all three; everything else keeps the raw managed name.
            // Fix W with the M exemption: the box test walks the base chain; @unsafe.Pointer keeps
            // its raw-managed-name route here, exactly as today.
            if (!typeof(IUnsafePointer).IsAssignableFrom(type) && GoReflect.TryBoxPointee(type, out Type? _0) ||
                GoReflect.TryAdapterWrappedType(type, out Type? _1, out bool _2) ||
                type.DeclaringType?.Name.EndsWith(PackageSuffix, StringComparison.Ordinal) == true)
            {
                return GoReflect.GoTypeName(type);
            }

            return typeName;
        }
    }

    /// <summary>
    /// Gets the common Go type name for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <returns>Common Go type name for the specified type <typeparamref name="T"/>.</returns>
    public static string GetGoTypeName<T>()
    {
        return GetGoTypeName(typeof(T));
    }

    // Materializes a stack string (sstring) as a heap string (@string) — used at an escape boundary
    // where a converted value must live on the heap (a field, an interface, a return). The implicit
    // sstring-to-@string conversion does the same; str(...) is the explicit form the converter emits
    // where an inferred target slot cannot trigger the implicit conversion on its own.
    public static @string str(sstring value) => value;

    /// <summary>
    /// Converts a span of UTF-8 bytes to a heap allocated <see cref="@string"/>.
    /// </summary>
    /// <param name="value">UTF-8 bytes.</param>
    /// <returns>Heap allocated string.</returns>
    public static @string str(ReadOnlySpan<byte> value) => (@string)value;
}
