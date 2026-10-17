// TestLogBridge.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.IO;
using System.Threading;
using go.golib;
using static go.builtin;
using os = go.os_package;
using testlog = go.@internal.testlog_package;

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
/// <b>Go's own <c>os</c> and <c>internal/testlog</c>, referenced directly.</b> Both are in every host's
/// closure already (testing → fmt → os → internal/testlog; measured in testing/synctest's host deps.json,
/// a package that imports neither), so the edges cost no assembly. Going through Go's <c>os</c> is what
/// makes the observable parts Go's: <c>os.Create</c>'s error is Go's <c>*PathError</c> text; the names
/// <c>os</c> reports arrive as <c>@string</c> and are written as their raw bytes, invalid UTF-8 included;
/// and a failure ends the process through <c>os.Exit(2)</c>, as Go's does, rather than returning a status
/// a TestMain could carry on past (G's review of 6f50ffb840).
/// </para>
/// <para>
/// The open and close DECISIONS (<see cref="Open"/>, <see cref="Close"/>) return Go's message, or null, and
/// M.Run does the print and the exit: the decisions are what a test can read, since the exit ends the
/// process.
/// </para>
/// </remarks>
internal static class TestLogBridge
{
    // bufio.NewWriter's default size: testdeps writes through one, flushed when full and at StopTestLog.
    private const int BufferSize = 4096;

    private static readonly object s_lock = new();

    // testdeps' testLog: the file and its buffer while a log is open, and whether the logger and header are
    // installed (log.set).
    private static ж<os.File>? s_file;
    private static MemoryStream? s_buffer;
    private static error? s_writeError;
    private static bool s_set;

    // Go's m.numRun as this log sees it: the host makes one M per process, so the count of opens is it.
    private static int s_runs;

    /// <summary>
    /// m.before's testlog block. The first run creates the file (Go's <c>m.numRun == 1</c>), a later one
    /// opens it write-only and seeks to its end. Returns null, or the message Go prints before os.Exit(2).
    /// </summary>
    internal static string? Open(string path)
    {
        bool firstRun = Interlocked.Increment(ref s_runs) == 1;

        (ж<os.File> file, error err) = firstRun
            ? os.Create(path)
            : os.OpenFile(path, os.O_WRONLY, default);

        if (err == nil && !firstRun)
            file.Seek(0, 2); // io.SeekEnd

        if (err != nil)
            return $"testing: {err.Error()}\n";

        lock (s_lock)
        {
            s_file = file;
            s_buffer = new MemoryStream();
            s_writeError = null;

            // StartTestLog: the logger and the header once per process, however many times m.Run runs.
            if (!s_set)
            {
                s_set = true;
                testlog.SetLogger(new TestLogger());
                WriteLocked("# test log\n"u8); // known to cmd/go/internal/test/test.go
            }
        }

        return null;
    }

    /// <summary>
    /// writeProfiles' testlog block: StopTestLog's flush, then the close. Returns null, or the message Go
    /// prints before os.Exit(2).
    /// </summary>
    internal static string? Close(string path)
    {
        ж<os.File>? file;
        error? flushError;

        lock (s_lock)
        {
            file = s_file;

            if (file is null)
                return null;

            FlushLocked();
            flushError = s_writeError;
            s_file = null;
            s_buffer = null;
        }

        if (flushError is not null && flushError != nil)
            return $"testing: can't write {path}: {flushError.Error()}\n";

        error closeError = file.Close();

        return closeError != nil ? $"testing: can't write {path}: {closeError.Error()}\n" : null;
    }

    /// <summary>
    /// Go's failure arm for both decisions: <c>fmt.Fprintf(os.Stderr, ...)</c> then <c>os.Exit(2)</c>,
    /// through Go's own os, so the process ends as Go's does. A null message continues.
    /// </summary>
    internal static void ExitIfFailed(string? message)
    {
        if (message is null)
            return;

        os.Stderr.WriteString(message);
        os.Exit(2);
    }

    // testLog.add: one `op name` line, the name's own bytes; an empty name, or one holding a newline, is not
    // logged.
    private static void Add(ReadOnlySpan<byte> op, @string name)
    {
        ReadOnlySpan<byte> bytes = name.ToSpan();

        if (bytes.IsEmpty || bytes.IndexOf((byte)'\n') >= 0)
            return;

        lock (s_lock)
        {
            if (s_buffer is null)
                return;

            WriteLocked(op);
            WriteLocked(" "u8);
            WriteLocked(bytes);
            WriteLocked("\n"u8);
        }
    }

    // bufio.Writer.Write: buffer, and write the buffer out when it fills. A write error sticks, as bufio's
    // does, and Close reports it.
    private static void WriteLocked(ReadOnlySpan<byte> bytes)
    {
        s_buffer!.Write(bytes);

        if (s_buffer.Length >= BufferSize)
            FlushLocked();
    }

    private static void FlushLocked()
    {
        if (s_buffer is null || s_file is null || s_buffer.Length == 0 || s_writeError is not null)
            return;

        (nint _, error err) = s_file.Write(new slice<byte>(s_buffer.ToArray()));
        s_buffer.SetLength(0);

        if (err != nil)
            s_writeError = err;
    }

    // testdeps' testLog as internal/testlog.Interface: each method logs under testLog's op name.
    private sealed class TestLogger : testlog.Interface
    {
        public void Getenv(@string key) => Add("getenv"u8, key);

        public void Stat(@string file) => Add("stat"u8, file);

        public void Open(@string file) => Add("open"u8, file);

        public void Chdir(@string dir) => Add("chdir"u8, dir);
    }
}
