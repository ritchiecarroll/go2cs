// TestLogBridge.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using System.Reflection;
using System.Text;

// go2cs HAND-OWNED (whole file) — part of the Phase-4 test host, a structural replacement for Go's
// testing package rather than a conversion of it (the rationale is in testing.cs). No converted source
// emits at this path, so this marker declares ownership rather than resolving a collision.
[module: go.GoManualConversion]

namespace go.testing_runtime;

/// <summary>
/// Go's <c>-test.testlogfile</c>: the test action log <c>cmd/go</c> reads to decide whether a cached test
/// result is still valid. <c>M.Run</c> opens the file before the tests (testing.go's <c>m.before</c>) and
/// hands it to <c>testdeps.StartTestLog</c>, which installs an <c>internal/testlog</c> logger the
/// <c>os</c> package reports every <c>Getenv</c>, <c>Stat</c>, <c>Open</c> and <c>Chdir</c> to;
/// <c>m.writeProfiles</c> flushes and closes it after them (go1.26.9 testing.go:2339-2357, 2697-2706,
/// testing/internal/testdeps/deps.go:82-150).
/// </summary>
/// <remarks>
/// <para>
/// Until this existed the flag was REGISTERED (TestFlagBridge, so a parse accepts it) and then ignored:
/// no file was ever written. os's TestFileChdirTestlog re-runs itself with the flag and reads the file
/// back, and failed reading a file that did not exist.
/// </para>
/// <para>
/// <b>Why <c>internal/testlog</c> is bound LATE.</b> For the reason TestFlagBridge states for
/// <c>flag</c>: the converted test projects set <c>DisableTransitiveProjectReferences=true</c>, so a
/// <c>testing</c> → <c>internal/testlog</c> project reference would not deploy the assembly beside a
/// host whose package does not reach it. <c>internal/testlog</c> is present exactly when the package
/// under test reaches <c>os</c>, which is the only package that logs to it. When it is absent there is
/// nothing to observe the log, and the file still gets Go's header, as Go's would for such a package.
/// The logger is <c>testlog.Interface</c>, a public interface this assembly cannot name, so it is
/// implemented at run time by a <see cref="DispatchProxy"/> (the host runs JIT; golib already emits
/// types the same way).
/// </para>
/// </remarks>
internal static class TestLogBridge
{
    private static readonly object s_lock = new();

    // testdeps' testLog: the writer while a log is open, and whether the logger and header are installed.
    private static StreamWriter? s_writer;
    private static bool s_set;

    // Go's m.numRun as this log sees it: the host makes one M per process, so the count of opens is it.
    private static int s_runs;

    /// <summary>
    /// m.before's testlog block. The first run creates the file (Go's <c>m.numRun == 1</c>), a later one
    /// appends to it. Returns null, or Go's exit status 2 after Go's message.
    /// </summary>
    internal static int? Start(string path)
    {
        bool firstRun = System.Threading.Interlocked.Increment(ref s_runs) == 1;
        FileStream file;

        try
        {
            file = firstRun
                ? new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)
                : new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);

            if (!firstRun)
                file.Seek(0, SeekOrigin.End);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            Console.Error.Write($"testing: open {path}: {ex.Message}\n");
            return 2;
        }

        lock (s_lock)
        {
            s_writer = new StreamWriter(file, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) { NewLine = "\n" };

            // StartTestLog: the logger and the header once per process, however many times m.Run runs.
            if (!s_set)
            {
                s_set = true;
                InstallLogger();
                s_writer.Write("# test log\n"); // known to cmd/go/internal/test/test.go
            }
        }

        return null;
    }

    /// <summary>
    /// writeProfiles' testlog block: StopTestLog's flush, then the close. Returns null, or Go's exit
    /// status 2 after Go's message.
    /// </summary>
    internal static int? Stop(string path)
    {
        lock (s_lock)
        {
            StreamWriter? writer = s_writer;
            s_writer = null;

            if (writer is null)
                return null;

            try
            {
                writer.Dispose();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.Error.Write($"testing: can't write {path}: {ex.Message}\n");
                return 2;
            }
        }

        return null;
    }

    // testLog.add: one `op name` line; a name holding a newline, or empty, is not logged.
    internal static void Add(string op, string name)
    {
        if (name.Length == 0 || name.Contains('\n'))
            return;

        lock (s_lock)
        {
            if (s_writer is null)
                return;

            s_writer.Write(op);
            s_writer.Write(' ');
            s_writer.Write(name);
            s_writer.Write('\n');
        }
    }

    // testlog.SetLogger(proxy), when the package under test carries internal/testlog at all.
    private static void InstallLogger()
    {
        Type? package = LoadTestlogPackage();
        Type? contract = package?.GetNestedType("Interface", BindingFlags.Public);
        MethodInfo? setLogger = package?.GetMethod("SetLogger", BindingFlags.Public | BindingFlags.Static, contract is null ? Type.EmptyTypes : [contract]);

        if (contract is null || setLogger is null)
            return;

        object logger = DispatchProxy.Create(contract, typeof(TestLogProxy));
        setLogger.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [logger], null);
    }

    private static Type? LoadTestlogPackage()
    {
        const string AssemblyName = "internal.testlog";
        const string PackageType = "go.internal.testlog_package";

        foreach (Assembly loaded in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (loaded.GetName().Name == AssemblyName)
                return loaded.GetType(PackageType);
        }

        try
        {
            return Assembly.Load(new AssemblyName(AssemblyName)).GetType(PackageType);
        }
        catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            return null;
        }
    }
}

/// <summary>
/// testdeps' testLog as <c>internal/testlog.Interface</c>: each of the interface's four methods is
/// logged under its own lower-cased name (Getenv → <c>getenv</c>, Stat → <c>stat</c>, Open → <c>open</c>,
/// Chdir → <c>chdir</c>), exactly testLog's ops. Public and unsealed because DispatchProxy derives from it.
/// </summary>
public class TestLogProxy : DispatchProxy
{
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod is not null && args is [{ } name])
            TestLogBridge.Add(targetMethod.Name.ToLowerInvariant(), name.ToString() ?? string.Empty);

        return null;
    }
}
