// GoModInfoAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace go;

/// <summary>
/// The program's module build info: the <c>path</c>, <c>mod</c> and <c>dep</c> lines <c>cmd/go</c> embeds in
/// a Go binary for <c>runtime/debug.ReadBuildInfo</c>, in Go's own line form (tab-separated fields, each
/// line ending in <c>\n</c>).
/// </summary>
/// <remarks>
/// <para>
/// Applied to the ENTRY assembly of a converted program, from its project file. A converted TEST host carries
/// no stamp: its <c>TestRegistry</c> registers the same lines at start-up (<see cref="Register"/>), from the
/// import path and module path the host already passes it, so no emitted host changes.
/// </para>
/// <para>
/// Only the module half is stamped. The <c>build</c> lines are composed at run time from the binary itself
/// (<see cref="Compose(string, string, bool)"/>): <c>GOOS</c>, <c>GOARCH</c> and <c>CGO_ENABLED</c> are the
/// converted runtime's own values, and <c>DefaultGODEBUG</c> is <see cref="GoDefaultGodebugAttribute"/>'s.
/// Settings with no honest managed value (<c>-compiler</c>, <c>-buildmode</c>, <c>GOAMD64</c>, <c>vcs.*</c>)
/// are never reported.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GoModInfoAttribute(string value) : Attribute
{
    /// <summary>The <c>path</c>, <c>mod</c> and <c>dep</c> lines, each ending in <c>\n</c>.</summary>
    public string Value { get; } = value;

    private static string? s_entryValue;
    private static string? s_registered;

    /// <summary>
    /// The entry assembly's module build info, or "" when it declares none (or there is no entry assembly).
    /// </summary>
    public static string EntryValue => s_entryValue ??= Assembly.GetEntryAssembly()?.GetCustomAttribute<GoModInfoAttribute>()?.Value ?? "";

    /// <summary>
    /// The lines a converted test host registered, or "" when none did.
    /// </summary>
    public static string Registered => s_registered ?? "";

    /// <summary>
    /// Registers a test binary's module build info as <c>go test</c> links it in: <c>path &lt;importPath&gt;.test</c>,
    /// or the bare <c>path &lt;importPath&gt;</c> when the package under test is <c>main</c>, plus
    /// <c>mod &lt;modulePath&gt; (devel)</c> for a package outside the standard library, then the <c>dep</c> lines of a main
    /// package's own build info.
    /// </summary>
    /// <remarks>
    /// cmd/go's test main copies a MAIN package's own build info, so its path carries no <c>.test</c>
    /// (cmd/go/internal/load/test.go). It re-derives it -- with <c>.test</c> -- when the test files' <c>//go:debug</c>
    /// lines change DefaultGODEBUG; a converted host cannot see that difference without an emission, and reports the
    /// bare path there too (a BOARD-recorded divergence, 2026-10-22).
    /// </remarks>
    /// <param name="importPath">Import path of the package under test.</param>
    /// <param name="modulePath">Its module path, or "" for the standard library.</param>
    /// <param name="mainPackage">Whether the package under test is <c>main</c>.</param>
    /// <param name="dependencies">The test binary's <c>dep</c> lines, each line's fields joined by tabs: a main package's
    /// modules, and none for any other package, as Go's test main lists none there.</param>
    public static void Register(string importPath, string modulePath, bool mainPackage, IReadOnlyList<string>? dependencies = null)
    {
        string lines = mainPackage ? $"path\t{importPath}\n" : $"path\t{importPath}.test\n";

        if (modulePath != "")
            lines += $"mod\t{modulePath}\t(devel)\t\n";

        // A main package's modules, after the mod line as cmd/go writes them (setBuildInfo).
        foreach (string dependency in dependencies ?? [])
            lines += $"dep\t{dependency}\n";

        s_registered = lines;

        // The composed value describes the registration; the next read composes it afresh.
        s_composed = null;
    }

    // cmd/go's modinfo sentinels (cmd/go/internal/modload/build.go infoStart / infoEnd), which
    // runtime/debug.ReadBuildInfo strips as data[16:len-16].
    private static readonly byte[] s_infoStart = Convert.FromHexString("3077af0c9274080241e1c107e6d618e6");
    private static readonly byte[] s_infoEnd = Convert.FromHexString("f932433186182072008242104116d8f2");

    // The composed runtime.modinfo, held in a class so a reader sees the whole value or none of it.
    private sealed class ComposedModInfo(@string value)
    {
        public readonly @string Value = value;
    }

    private static ComposedModInfo? s_composed;

    /// <summary>
    /// The running binary's <c>runtime.modinfo</c>: the registered test-host lines when there are any, else the
    /// entry stamp, followed by its build settings. "" when neither exists.
    /// </summary>
    /// <remarks>
    /// In Go, <c>modinfo</c> is a static string the linker writes, so reading it allocates nothing. It is composed here
    /// ONCE, on the first read, and every later read returns that same string; <see cref="Register"/> resets it. Two
    /// racing first reads may both compose, and both compose the same value.
    /// </remarks>
    /// <param name="goos">The converted runtime's <c>GOOS</c>.</param>
    /// <param name="goarch">The converted runtime's <c>GOARCH</c>.</param>
    /// <param name="cgoEnabled">The converted runtime's <c>iscgo</c>.</param>
    public static @string Compose(string goos, string goarch, bool cgoEnabled)
    {
        ComposedModInfo? composed = s_composed;

        if (composed is null)
        {
            composed = new ComposedModInfo(Compose(Registered != "" ? Registered : EntryValue, goos, goarch, cgoEnabled, GoDefaultGodebugAttribute.EntryValue));
            s_composed = composed;
        }

        return composed.Value;
    }

    /// <summary>
    /// Composes a <c>runtime.modinfo</c> value from its parts, sentinel-wrapped; "" when <paramref name="modules"/>
    /// is empty, since a binary with no module info reports none.
    /// </summary>
    /// <remarks>
    /// The build lines follow cmd/go's order (cmd/go/internal/load/pkg.go setBuildInfo): <c>DefaultGODEBUG</c>
    /// when there is one, then <c>CGO_ENABLED</c>, <c>GOARCH</c> and <c>GOOS</c>. None of these values can carry a
    /// space, tab, quote or newline, so none needs the quoting <c>BuildInfo.String</c> applies.
    /// </remarks>
    public static @string Compose(string modules, string goos, string goarch, bool cgoEnabled, string defaultGodebug)
    {
        if (modules == "")
            return ""u8;

        StringBuilder info = new(modules);

        if (defaultGodebug != "")
            info.Append($"build\tDefaultGODEBUG={defaultGodebug}\n");

        info.Append($"build\tCGO_ENABLED={(cgoEnabled ? "1" : "0")}\n");
        info.Append($"build\tGOARCH={goarch}\n");
        info.Append($"build\tGOOS={goos}\n");

        byte[] text = Encoding.UTF8.GetBytes(info.ToString());
        byte[] data = new byte[s_infoStart.Length + text.Length + s_infoEnd.Length];

        s_infoStart.CopyTo(data, 0);
        text.CopyTo(data, s_infoStart.Length);
        s_infoEnd.CopyTo(data, s_infoStart.Length + text.Length);

        return new @string(data);
    }
}
