// GoArchExclusiveAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Mark a converted package as NATIVE to specific GOARCH values only, so harnesses skip it by name
/// on any other architecture rather than reporting it as a failure.
/// </summary>
/// <remarks>
/// <para>
/// The GOARCH sibling of <see cref="GoPlatformExclusiveAttribute"/>, and deliberately a SEPARATE
/// attribute rather than another token form of that one. The two axes are orthogonal: the package
/// this was written for is native to every GOOS and exclusive to one GOARCH, so folding it into the
/// platform marker would mean spelling all three GOOS names to say one thing about the arch -- and
/// that attribute's own contract says its arguments are GOOS names. A package may carry both; a
/// harness skips it when EITHER axis excludes the host.
/// </para>
/// <para>
/// THE CLASS, measured rather than supposed (2026-09-04). Go's own filename rule makes
/// <c>name_GOARCH.go</c> an implicit build constraint, so a package whose declarations live in
/// <c>*_amd64.go</c> files does not compile on any other architecture -- in GO, before go2cs is
/// involved at all. <c>StdLibInternalAbi</c> is the measured case: it copies <c>internal/abi</c> and
/// <c>internal/goarch</c> into a <c>package main</c> carrying <c>abi_amd64.go</c>,
/// <c>goarch_amd64.go</c> and <c>zgoarch_amd64.go</c>, and
/// <c>GOARCH=arm64 go build</c> fails with <c>undefined: IntArgRegs</c>, <c>undefined: _ArchFamily</c>
/// and the <c>Is*</c> endianness constants, where <c>GOARCH=amd64</c> builds clean.
/// </para>
/// <para>
/// WHAT THAT COSTS WITHOUT THE MARKER, and why it is not merely untidy. No harness passes
/// <c>-platforms</c>, so the converter's default (<c>runtime.GOOS/runtime.GOARCH</c> of the go2cs
/// process) makes the transpiled architecture whatever machine the run landed on. On an arm64 host
/// <c>go/packages</c> drops the three <c>_amd64.go</c> files, the package stops type-checking, the
/// converter emits a best-effort conversion, and the valueless const reaches the C# compiler as
/// <c>goarch.cs(23,22): error CS0145</c>. Both darwin censuses read exactly that on <c>osx-arm64</c>
/// while <c>osx-x64</c> passed every phase.
/// </para>
/// <para>
/// PINNING THE ARCH IS THE WRONG REMEDY, and the measurement says so directly. Passing
/// <c>-platforms darwin/amd64</c> from the harness would make the C# side compile and would then
/// fail the OUTPUT phase, because the oracle -- <c>go run</c> on that host -- cannot build the
/// package either. Nor can a layout dimension help: an arm64 sibling set would need
/// <c>abi_arm64.go</c>/<c>goarch_arm64.go</c>/<c>zgoarch_arm64.go</c> AND arm64-captured goldens.
/// Skip-by-name is the remedy that is both correct and verifiable.
/// </para>
/// <para>
/// UNLIKE the platform marker, this one implies NO <c>.slnx</c> consequence. That criterion is
/// "platform-exclusive AND not-windows-native", and it exists because the solution has exactly one
/// Windows flavour. The solution has no ARCH flavour -- C# compiles architecture-neutrally and the
/// project builds on every amd64 host the fleet has -- so an arch-exclusive project stays
/// REGISTERED and <c>check-solution-integrity.ps1</c> is not involved.
/// </para>
/// <para>
/// Hand-added to a package's <c>package_info.cs</c> beside <c>[GoTestMatchingConsoleOutput]</c>, and
/// preserved across a re-transpile by construction: <c>writePackageInfoFile</c> rebuilds only the
/// marker SECTIONS and copies every other line through verbatim, which is the same mechanism that
/// carries <c>[GoTestMatchingConsoleOutput]</c> and the platform markers.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoArchExclusiveAttribute : Attribute
{
    /// <summary>
    /// Creates a new <see cref="GoArchExclusiveAttribute"/>.
    /// </summary>
    /// <param name="arches">
    /// GOARCH names the package is native to, e.g. <c>"amd64"</c>. More than one is allowed for a
    /// package native to several but not all.
    /// </param>
    public GoArchExclusiveAttribute(params string[] arches) => Arches = arches;

    /// <summary>
    /// Gets the GOARCH names this package is native to.
    /// </summary>
    public string[] Arches { get; }
}
