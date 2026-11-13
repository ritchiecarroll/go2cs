// GoCgoImportDynamicAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Records one Go <c>//go:cgo_import_dynamic</c> pragma: the bodyless trampoline the emission
/// declares, the dynamic symbol it stands for, and the library that exports it —
/// <c>//go:cgo_import_dynamic libc_fork fork "/usr/lib/libSystem.B.dylib"</c> ⇒
/// <c>[assembly: GoCgoImportDynamic("libc_fork_trampoline", "fork", "/usr/lib/libSystem.B.dylib")]</c>.
/// </summary>
/// <remarks>
/// <para>
/// This is the CLASS-B half of the synthetic-PC design's discriminator, and it exists because the two
/// halves of <c>abi.FuncPCABI0</c> want opposite things. A PC read BACK — pprof, <c>runtime.Callers</c>,
/// <c>textAddr</c> — wants a synthetic token that symbolizes and is never dereferenced. A darwin
/// trampoline wants the exact opposite: a REAL, callable address, because the value is what
/// <c>rawSyscall</c> jumps to. Handing that site a token would be the same failure as the
/// <c>return default</c> it replaces — plausible, unique, stable, and fatal on first call.
/// </para>
/// <para>
/// The discriminator is therefore "is this argument a cgo-imported trampoline", and it is decided by
/// the presence of a record here. A stub the <c>PartialStubGenerator</c> marked as having no managed
/// body, WITH a record, is class B and resolves through <see cref="System.Runtime.InteropServices.NativeLibrary"/>;
/// the same stub WITHOUT a record is class C — Go's own assembly (<c>goexit</c>, <c>asyncPreempt</c>,
/// <c>sigtramp</c>, the syscall family), which has nothing to resolve from and no managed equivalent,
/// and stays a loud throw. No silent zero survives on either path.
/// </para>
/// <para>
/// It is an ASSEMBLY attribute rather than a central registry for a reason of ownership: the resolver
/// lives in <c>internal/abi</c> while the trampolines live in <c>syscall</c> and
/// <c>crypto/x509/internal/macos</c>, so the resolver must read data it does not own and cannot
/// reference. Reaching it from the ARGUMENT — the delegate's method, its declaring type, that type's
/// assembly, and this attribute on that assembly — needs no cross-assembly registry, no initialization
/// order, and no contribution at all from a package that declares no trampolines.
/// </para>
/// <para>
/// The population is selected by the pragma's own shape rather than by a hand-listed set of
/// libraries: the library argument is an ABSOLUTE PATH. Over the 1650 <c>//go:cgo_import_dynamic</c>
/// records in Go 1.23.12 outside <c>cmd/</c> and <c>vendor/</c>, every darwin record names one
/// (<c>/usr/lib/libSystem.B.dylib</c>, <c>/usr/lib/libresolv.9.dylib</c>, and the two
/// <c>/System/Library/Frameworks/...</c> frameworks <c>crypto/x509/internal/macos</c> imports) while
/// every other platform names a BARE library - windows' 51 <c>kernel32.dll</c>, openbsd and solaris'
/// <c>libc.so</c>, aix's <c>libc.a/shr_64.o</c> - or names none at all, as <c>runtime/race</c>'s 196
/// darwin records do. Selecting on the leading slash and selecting on "<c>.dylib</c> or a framework
/// path" are two independent derivations of the same 345 records and they agree on every one, which
/// is what makes the shape safe to read instead of a list a later Go release could add to.
/// </para>
/// <para>
/// An earlier draft of this comment offered the pragma's NAMING as a free cross-check, generalizing
/// a count taken over one file to the whole darwin tree. Measured, <c>local == "libc_" + symbol</c>
/// holds in <b>312</b> of the 345 - a majority, not a rule. The 33 exceptions are 28
/// <c>x509_&lt;sym&gt;</c>, 3 <c>libresolv_&lt;sym&gt;</c>, one <c>libc&lt;sym&gt;</c> with no
/// underscore, and one outlier, <c>libc_error</c> / <c>__error</c>, which is the single record in all
/// 345 whose local does not even END with its symbol. That last row is the argument against the
/// cross-check rather than the count is: it would be correct 344 times and silently wrong once. The
/// binding rule the
/// converter uses is the one that does - see <c>cgoDynamicImports.go</c>, where a record is minted
/// only for a bodyless <c>func &lt;local&gt;_trampoline()</c> whose package carries the matching
/// pragma. That holds for 297 of 297 declarations outside <c>runtime</c> and 0 of 43 inside it, so
/// runtime's trampolines - whose correspondence lives in the <c>.s</c> file the converter does not
/// read - mint nothing and stay class C rather than being reached by a normalizer.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoCgoImportDynamicAttribute : Attribute
{
    /// <summary>
    /// Creates a new <see cref="GoCgoImportDynamicAttribute"/>.
    /// </summary>
    /// <param name="trampolineName">
    /// The emitted trampoline method's name — the key, matched against
    /// <see cref="System.Reflection.MemberInfo.Name"/> of the delegate's target method.
    /// </param>
    /// <param name="symbol">The dynamic symbol the trampoline stands for.</param>
    /// <param name="library">The library path the pragma names.</param>
    public GoCgoImportDynamicAttribute(string trampolineName, string symbol, string library)
    {
        TrampolineName = trampolineName;
        Symbol = symbol;
        Library = library;
    }

    /// <summary>
    /// Gets the emitted trampoline method's name, e.g. <c>libc_fork_trampoline</c>.
    /// </summary>
    /// <remarks>
    /// The key is the METHOD NAME rather than a handle because an attribute argument must be a
    /// compile-time constant, and a <c>RuntimeMethodHandle</c> is not one. The name is sufficient:
    /// trampolines are file-scoped statics in one package class per platform flavor, so a name is
    /// unique within the assembly the lookup is scoped to.
    /// </remarks>
    public string TrampolineName { get; }

    /// <summary>
    /// Gets the dynamic symbol, e.g. <c>fork</c>.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Gets the exporting library, e.g. <c>/usr/lib/libSystem.B.dylib</c>.
    /// </summary>
    public string Library { get; }
}
