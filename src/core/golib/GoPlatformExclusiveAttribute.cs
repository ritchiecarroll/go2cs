// GoPlatformExclusiveAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Mark a converted package as NATIVE to specific platforms only, so harnesses skip it by name on
/// any other host rather than reporting it as a failure or, worse, as a vacuous pass.
/// </summary>
/// <remarks>
/// <para>
/// The attribute names the platform(s) the package's Go source can be type-checked and run on. It
/// says NOTHING about which host is reading it -- that comparison belongs to the harness.
/// </para>
/// <para>
/// The class it gates is real and bites in BOTH directions. A package whose Go source calls
/// unix-only <c>syscall</c> API (<c>Socketpair</c>, <c>UnixRights</c>, <c>SendmsgN</c>) cannot
/// type-check on Windows; one calling <c>FindFirstFile</c> or the WSA surface cannot type-check on
/// Linux. Before this marker each side reported the other's packages as NOT MEASURED -- correct but
/// indistinguishable, by name, from a genuine conversion failure, so a real regression could hide
/// among expected lines.
/// </para>
/// <para>
/// A marked package on a non-native host is SKIPPED BY NAME: printed with its platform, counted in
/// the summary, and excluded from the byte-identical and pass counts -- never silently dropped from
/// the enumeration, which would trade one invisible problem for another. On its native host it is
/// enumerated exactly as an unmarked package.
/// </para>
/// <para>
/// <para>
/// THE GATING SET IS DERIVED, AND THE DERIVATION HAS TWO CRITERIA, NOT ONE. The first is a real
/// CNR on the foreign host: a package it reports NOT MEASURED cannot be type-checked there and is
/// marked. That was the whole rule when this attribute landed (2026-09-02), and it named six
/// windows-exclusive packages plus <c>ScmRightsSeam</c>. Two amendments the same day, each from a
/// package the CNR derivation cannot see:
/// </para>
/// <para>
/// <b>SendtoSeam moved from early-out to marker, 2026-09-02.</b> The F8 derivation left it
/// unmarked because it type-checks everywhere and carried a <c>runtime.GOOS</c> early-out printing
/// one fixed line off Linux -- green and honest, as that note said. What the early-out could not fix
/// is the GOLDEN: the package still transpiled on both hosts, and its emitted C# DIFFERS by platform
/// (the Windows <c>syscall</c> flavor mints Δ-prefixed <c>Sockaddr</c>/<c>Handle</c> aliases the
/// Linux one does not), so the committed <c>.cs</c> had to be one platform's and read as standing
/// drift on the other -- which its own commit predicted by name. A skipped package is neither
/// transpiled nor compared, so the marker retires that drift where the early-out could not. So the
/// SECOND criterion is: a package whose EMISSION is platform-dependent, even though its source
/// type-checks everywhere.
/// </para>
/// <para>
/// <b>LocalTimeZone</b> is the third criterion and was flagged rather than marked for exactly the
/// right reason: it type-checks on Linux and fails at the OUTPUT phase on a kernel32 fault, which no
/// CNR can see. One marker covers all three cases by construction -- a non-native host skips every
/// phase -- which is why the skip lines say "cannot measure" rather than "cannot type-check".
/// </para>
/// Hand-added to a package's <c>package_info.cs</c> beside <c>[GoTestMatchingConsoleOutput]</c>, and
/// preserved across a re-transpile by the same mechanism (measured on ScmRightsSeam: re-transpiling
/// with the marker present leaves the file differing from its pre-marker self by exactly the one
/// added line).
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoPlatformExclusiveAttribute : Attribute
{
    /// <summary>
    /// Creates a new <see cref="GoPlatformExclusiveAttribute"/>.
    /// </summary>
    /// <param name="platforms">
    /// GOOS names the package is native to, e.g. <c>"linux"</c> or <c>"windows"</c>. More than one
    /// is allowed for a package native to several but not all.
    /// </param>
    public GoPlatformExclusiveAttribute(params string[] platforms) => Platforms = platforms;

    /// <summary>
    /// Gets the GOOS names this package is native to.
    /// </summary>
    public string[] Platforms { get; }
}
