using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

// The host's event lines reach the comparer whole, whatever else writes to the same pipe.
//
// WHAT IT GUARDS. The comparer reads the C# side's events from stdout and stderr merged onto one pipe.
// Console's own writer hands a line to that pipe in pieces of its 256-character buffer, so a line longer
// than that is several writes, and a raw-handle writer (converted Go code's os.Stdout / os.Stderr) lands
// between them: the line is torn and its event lost. Measured in a child process on linux (a raw writer
// beside 200 parallel tests whose JSON lines run past 300 characters): 53 to 146 of 200 events arrived
// whole before TestReporter.WriteEventLine, 200 of 200 after; the i9 measured 15% to 65% lost on windows.
//
// HOW. MSTest replaces Console.Out itself, so this guard cannot reach the real console. It rebuilds the
// shape instead: a pipe-like stream that takes each write atomically, Console.Out set to Console's own
// writer shape over it (256-char buffer, autoflush, synchronized), and a raw writer on the same stream.
// The guard seams point the host's process-console branch at that stream.
[TestClass]
public class ConsoleEventLineAtomicityTests
{
    private const int Events = 200;

    // One write call lands whole, as a pipe write up to PIPE_BUF does; calls from different threads
    // interleave only between calls.
    private sealed class PipeLikeStream : Stream
    {
        private readonly MemoryStream m_bytes = new();
        private readonly object m_lock = new();

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (m_lock)
                m_bytes.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            lock (m_lock)
                m_bytes.Write(buffer);
        }

        public string Text()
        {
            lock (m_lock)
                return Encoding.UTF8.GetString(m_bytes.ToArray());
        }
    }

    private static int IntactPassEvents(string text)
    {
        int intact = 0;

        foreach (string line in text.Split('\n'))
        {
            // A JSON event line opens with Go's framing marker, which the comparer strips (H2, EventLineFramingTests).
            string trimmed = line.TrimEnd('\r').TrimStart(TestReporter.FramingMarker);

            if (!trimmed.Contains("TestLong", StringComparison.Ordinal))
                continue;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(trimmed);

                if (doc.RootElement.GetProperty("action").GetString() == "pass")
                    intact++;
            }
            catch (JsonException)
            {
                // a torn line: its event is lost
            }
        }

        return intact;
    }

    [TestMethod]
    public void AnEventLinePastTheConsoleBufferArrivesWhole()
    {
        PipeLikeStream pipe = new();
        TextWriter previous = Console.Out;
        string pad = new('x', 300);

        Console.SetOut(TextWriter.Synchronized(new StreamWriter(pipe, new UTF8Encoding(false), 256, leaveOpen: true) { AutoFlush = true }));
        TestReporter.ProcessConsoleForGuard = true;
        TestReporter.StdoutForGuard = pipe;

        try
        {
            TestReporter reporter = new("guard", json: true, verbose: false);
            byte[] noise = Encoding.ASCII.GetBytes(new string('E', 40) + "\n");
            using CancellationTokenSource stop = new();

            Thread raw = new(() =>
            {
                while (!stop.IsCancellationRequested)
                    pipe.Write(noise, 0, noise.Length);
            });

            raw.Start();
            Parallel.For(0, Events, i => reporter.Report(new TestEvent("guard", $"TestLong{i:D4}_{pad}", "pass")));
            stop.Cancel();
            raw.Join();
        }
        finally
        {
            TestReporter.ProcessConsoleForGuard = null;
            TestReporter.StdoutForGuard = null;
            Console.SetOut(previous);
        }

        Assert.AreEqual(Events, IntactPassEvents(pipe.Text()), "an event line longer than the console writer's buffer was torn by a concurrent raw writer");
    }

    // The redirect test reads a PRIVATE runtime field. Its fallback (read as redirected) keeps captures
    // safe but silently puts the real console back on the tearing path, so a runtime that renames or
    // retypes the field must turn this red, naming it, rather than revert quietly.
    [TestMethod]
    public void TheConsoleRedirectFlagResolvesOnThisRuntime()
    {
        string name = TestReporter.ConsoleRedirectFlagName;
        System.Reflection.FieldInfo field = TestReporter.ConsoleRedirectFlagForGuard;

        Assert.IsNotNull(field, $"System.Console.{name} does not resolve on .NET {Environment.Version}: the host's event lines are back on the tearing path (TestReporter.WriteEventLine)");
        Assert.AreEqual(typeof(bool), field.FieldType, $"System.Console.{name} is no longer a bool on .NET {Environment.Version}: TestReporter.WriteEventLine's redirect test reads it as one");
        Assert.IsTrue(field.IsStatic, $"System.Console.{name} is no longer static on .NET {Environment.Version}");
    }

    // The other branch: a host whose Console.Out was replaced (an in-process capture) gets the line there.
    [TestMethod]
    public void ARedirectedConsoleOutStillReceivesTheLine()
    {
        StringWriter captured = new();
        TextWriter previous = Console.Out;

        Console.SetOut(captured);
        TestReporter.ProcessConsoleForGuard = false;

        try
        {
            new TestReporter("guard", json: true, verbose: false).Report(new TestEvent("guard", "TestCaptured", "pass"));
        }
        finally
        {
            TestReporter.ProcessConsoleForGuard = null;
            Console.SetOut(previous);
        }

        StringAssert.Contains(captured.ToString(), "\"test\":\"TestCaptured\"");
    }
}
