// GoMemProfile.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

using System;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The memory profiler's allocation hook: Go's <c>mallocgc</c> profiling branch, at the golib doors a
/// converted program allocates through.
/// </summary>
/// <remarks>
/// <para>
/// <b>What Go does.</b> Every <c>mallocgc</c> reads <c>MemProfileRate</c>. Above 0 it counts the
/// allocation's size down from the M's <c>nextSample</c>; when the countdown is spent (or the rate is 1,
/// which records every allocation) it calls <c>profilealloc</c>, which draws the next countdown from an
/// exponential distribution with mean <c>MemProfileRate</c> and records the allocation's stack and size
/// in the memory profile (<c>mProf_Malloc</c>).
/// </para>
/// <para>
/// <b>Where it happens here.</b> Allocation never passes through <c>mallocgc</c> on this host: a Go
/// allocation is a golib constructor. <see cref="Charge{T}"/> is called at the doors that ARE Go
/// allocations: <c>&amp;T{}</c>, <c>new(T)</c> and an escaping local (<c>builtin.Ꮡ</c>,
/// <c>builtin.@new</c>, <c>builtin.heap</c>), <c>make([]T, n)</c>, and <c>append</c>'s growth. It is not
/// called for the view boxes over a field or element (Go allocates nothing for <c>&amp;x.f</c>), and the
/// allocations Go makes implicitly (an interface conversion of a value, a closure context, string
/// concatenation) or that the host adds (a heap box where Go's escape analysis keeps a local on the
/// stack) are outside it: the profile is the Go-visible explicit allocations, not Go's heap byte for byte.
/// </para>
/// <para>
/// <b>The storage of runtime.MemProfileRate is <see cref="Rate"/>.</b> Go code writes the runtime variable
/// directly (<c>runtime.MemProfileRate = 1</c>) and Go reads it on every allocation, so the value must be
/// readable here without a call into runtime. runtime declares <c>MemProfileRate</c> as a ref property
/// over this field (runtime/mprof_impl.cs).
/// </para>
/// <para>
/// <b>Cost.</b> Each charged allocation reads <see cref="Rate"/>, reads the Go size of its type from a
/// per-type static, and decrements a thread-static countdown; only a sampled allocation leaves the inlined
/// path, and only then is a stack walked.
/// </para>
/// </remarks>
public static class GoMemProfile
{
    /// <summary>The storage of <c>runtime.MemProfileRate</c>, Go's default: one sample per 512 KiB.</summary>
    public static nint Rate = 512 * 1024;

    /// <summary>
    /// Records one sampled allocation: the allocated object, its Go size in bytes, and whether its type
    /// holds no pointers (Go's <c>noscan</c>, which <c>roundupsize</c> reads). Installed by runtime.
    /// </summary>
    public static Action<object, nuint, bool>? Recorder;

    /// <summary>
    /// Whether the program can reach runtime/pprof: the inverse of Go's <c>disableMemoryProfiling</c>,
    /// decided once, before any Go code runs. runtime's module initializer starts <see cref="Rate"/> at 0
    /// when this is false, and a map's growth model runs only when it is true (map.cs, the growth model).
    /// </summary>
    /// <remarks>
    /// A static readonly field, so the JIT folds it into the code that reads it: with runtime/pprof out of
    /// the program, a map store compiles to what it was before the growth model existed. This class has no
    /// static constructor, so the runtime initializes it when the JIT compiles its first reader, before
    /// that reader's code is generated, under tiered compilation and without it.
    /// </remarks>
    public static readonly bool PprofReachable = PprofReachableFrom(AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string, findPprofPackage);

    private const string PprofAssemblyName = "runtime.pprof";

    // The type question is asked by name, not by walking references: under native AOT the compiler
    // resolves a constant Type.GetType name against the assemblies it was given, so the answer is the
    // static closure there, and GetReferencedAssemblies throws PlatformNotSupportedException. The name is a
    // constant AT THE CALL: the compiler does not follow it through a parameter. The entry assembly is asked
    // second, for a program that compiles runtime/pprof's sources into its own assembly rather than
    // referencing runtime.pprof. runtime/pprof's own test binary is not one: it references the assembly.
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "The entry-assembly lookup serves a program that compiles runtime/pprof's sources into itself, " +
                        "which calls pprof_package and so keeps it; missing, the answer is 'not linked', Go's own answer " +
                        "for a program without pprof (trim stage 1, docs/PLAN-golib-full-trim.md).")]
    private static Type? findPprofPackage() =>
        Type.GetType("go.runtime.pprof_package, runtime.pprof", throwOnError: false) ??
        System.Reflection.Assembly.GetEntryAssembly()?.GetType("go.runtime.pprof_package", throwOnError: false);

    /// <summary>
    /// Go's linker question, asked of the program's STATIC assembly closure: whether runtime.pprof is in
    /// <paramref name="trustedPlatformAssemblies"/> (the host's TRUSTED_PLATFORM_ASSEMBLIES, which is the
    /// app's deps.json list and is fixed before the first assembly loads), or else whether
    /// <paramref name="findPprof"/> resolves runtime.pprof's package type: the only question where the host
    /// has no such list (a native AOT or single-file publish), and the one that finds runtime/pprof compiled
    /// into the program's own assembly, which no list names. Never the assemblies loaded so far, which load
    /// lazily and would read "no pprof" at startup in every program. Where nothing answers, Go's default
    /// (reachable) stands.
    /// </summary>
    public static bool PprofReachableFrom(string? trustedPlatformAssemblies, Func<Type?> findPprof)
    {
        if (trustedPlatformAssemblies is not null)
        {
            foreach (string path in trustedPlatformAssemblies.Split(System.IO.Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(path), PprofAssemblyName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        try
        {
            return findPprof() is not null;
        }
        catch (Exception)
        {
            // The type could not be looked up. With a list, the list has answered; without one, keep Go's
            // default rather than silently switch the profile off.
            return trustedPlatformAssemblies is null;
        }
    }

    // Bytes left until this thread's next sample: Go's mcache.nextSample. Unseeded until the first
    // allocation this thread charges, as Go seeds it when the mcache is created.
    [ThreadStatic]
    private static long t_nextSample;

    [ThreadStatic]
    private static bool t_seeded;

    // Set while this thread records a sample. The recorder allocates (its stack buffer, a new bucket),
    // and Go's profiler allocates outside mallocgc, so none of that is profiled here either.
    [ThreadStatic]
    private static bool t_recording;

    // A pooled thread starts its next goroutine outside any recording. t_nextSample and t_seeded are kept on
    // purpose: Go's countdown lives on the M (mcache.nextSample), a thread, not on the goroutine.
    private static readonly bool s_recordingReset = go.golib.GoroutineThreadState.Register(static () => t_recording = false);

    /// <summary>Charges one Go allocation of <paramref name="count"/> values of <typeparamref name="T"/>.</summary>
    /// <param name="allocation">The object that holds the allocation (the box, or the backing array).</param>
    /// <param name="count">How many values of <typeparamref name="T"/> it holds (1 for a box).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Charge<T>(object allocation, nint count)
    {
        nint rate = Rate;

        if (rate <= 0)
            return;

        long size = GoSize<T>.Bytes * count;

        // Go: `if rate != 1 && size < c.nextSample { c.nextSample -= size } else { profilealloc(...) }`.
        if (rate != 1 && t_seeded && size < t_nextSample)
        {
            t_nextSample -= size;
            return;
        }

        Sample(allocation, size, GoSize<T>.NoScan, rate);
    }

    /// <summary>
    /// Charges one Go allocation that has no C# type of its own: <paramref name="size"/> Go bytes, pointer-free
    /// when <paramref name="noscan"/>. It is the door for the allocations a golib store models rather than
    /// makes (a map's groups, tables and directory: <c>map</c>'s growth model), and it counts down the same
    /// countdown as <see cref="Charge{T}"/>.
    /// </summary>
    /// <param name="allocation">
    /// An object whose lifetime is the modelled allocation's: its collection is the free. Made here, only when
    /// the charge reaches the sampler, when the caller passes none, so an unsampled charge allocates nothing.
    /// </param>
    public static void ChargeBytes(ref object? allocation, long size, bool noscan)
    {
        nint rate = Rate;

        if (rate <= 0)
            return;

        if (rate != 1 && t_seeded && size < t_nextSample)
        {
            t_nextSample -= size;
            return;
        }

        allocation ??= new object();
        Sample(allocation, size, noscan, rate);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Sample(object allocation, long size, bool noscan, nint rate)
    {
        if (t_recording)
            return;

        if (!t_seeded)
        {
            // The first charge on this thread seeds the countdown the way Go's mcache creation does, and
            // is sampled only if it spends it.
            t_seeded = true;
            t_nextSample = NextSample(rate);

            if (rate != 1 && size < t_nextSample)
            {
                t_nextSample -= size;
                return;
            }
        }

        // profilealloc: the next countdown, then the record.
        t_nextSample = NextSample(rate);

        if (Recorder is not { } recorder)
            return;

        t_recording = true;

        try
        {
            recorder(allocation, (nuint)size, noscan);
        }
        finally
        {
            t_recording = false;
        }
    }

    // Go's nextSample: 0 at rate 1 (sample every allocation), otherwise an exponential draw with mean
    // `rate` (fastexprand, capped as Go caps it).
    private static long NextSample(nint rate)
    {
        if (rate == 1)
            return 0;

        double mean = Math.Min((double)rate, 0x7000000);
        double u = Random.Shared.NextDouble();

        return (long)(-Math.Log(1.0 - u) * mean);
    }

    // The Go size of T and whether it holds pointers, read once per type.
    private static class GoSize<T>
    {
        internal static readonly long Bytes = Math.Max(0, (long)GoReflect.GoSizeOf(typeof(T)));

        internal static readonly bool NoScan = GoReflect.GoPtrBytesOf(typeof(T)) == 0;
    }
}
