using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.testing_runtime;

namespace GolibTests;

// H2: an event line is FRAMED the way Go's test binary frames one, so it survives a test's output that
// did not end its line.
//
// WHAT IT GUARDS. A test that writes to os.Stdout without a trailing newline leaves the shared stdout
// mid-line, and the host's next event lands on the end of that output. x/mod/sumdb/tlog's
// TestCertificateTransparency writes an HTTP body with os.Stdout.Write under -v; its pass event arrived
// glued to the body and the comparer never saw it: C#="" against Go's pass (measured 2026-10-02, linux).
//
// GO'S MECHANISM, read before choosing. Under -test.v=test2json (what `go test -json` runs the binary
// with) every framing line starts with ^V (testing.go: chattyFlag.prefix, `const marker = byte(0x16)`),
// and cmd/test2json ends a line at "\n" OR just before a ^V that does not begin one (test2json.go,
// indexEOL), stripping the leading ^V of a framing line. The host's --json mode is that pair in one
// process: it prints the events test2json would. So the host marks each JSON event line with ^V and the
// comparer reads with indexEOL's rule. No newline is invented: the partial output stays partial, as it
// does in Go's stream.
//
// HOW. The process-console branch, with a raw writer on the same stream, read back with test2json's rule.
[TestClass]
public class EventLineFramingTests
{
    private const char Marker = '\u0016';

    // cmd/test2json's line rule (indexEOL), as the comparer applies it: a line ends at '\n', or just
    // before a ^V that does not begin a line; a leading ^V is stripped.
    private static List<string> Test2JsonLines(string text)
    {
        List<string> lines = [];

        foreach (string raw in text.Split('\n'))
        {
            string line = raw;
            int cut;

            while (line.Length > 1 && (cut = line.IndexOf(Marker, 1)) > 0)
            {
                lines.Add(line[..cut].TrimStart(Marker));
                line = line[cut..];
            }

            lines.Add(line.TrimStart(Marker));
        }

        return lines;
    }

    private static string RunWithRawOutputFirst(string rawOutput)
    {
        MemoryStream stdout = new();
        TextWriter previous = Console.Out;

        Console.SetOut(new StringWriter());
        TestReporter.ProcessConsoleForGuard = true;
        TestReporter.StdoutForGuard = stdout;

        try
        {
            // Converted Go code's os.Stdout is a raw handle on the same stdout: its bytes go straight in.
            byte[] raw = Encoding.UTF8.GetBytes(rawOutput);
            stdout.Write(raw, 0, raw.Length);

            new TestReporter("guard", json: true, verbose: true).Report(new TestEvent("guard", "TestBody", "pass", 0.1D));
        }
        finally
        {
            TestReporter.ProcessConsoleForGuard = null;
            TestReporter.StdoutForGuard = null;
            Console.SetOut(previous);
        }

        return Encoding.UTF8.GetString(stdout.ToArray());
    }

    private static string? PassedTest(IEnumerable<string> lines)
    {
        foreach (string line in lines)
        {
            if (!line.StartsWith('{'))
                continue;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(line);

                if (doc.RootElement.TryGetProperty("action", out JsonElement action) && action.GetString() == "pass")
                    return doc.RootElement.GetProperty("test").GetString();
            }
            catch (JsonException)
            {
                // output glued to an event: the comparer cannot read it, so its event is lost
            }
        }

        return null;
    }

    [TestMethod]
    public void AnEventAfterUnterminatedOutputStillReads()
    {
        // tlog's shape: a JSON body written without a trailing newline, then the test passes.
        string written = RunWithRawOutputFirst("{\"consistency\":[\"80Sfbj6U\"]}");
        List<string> lines = Test2JsonLines(written);

        Assert.AreEqual("TestBody", PassedTest(lines), $"the pass event did not read as its own line: {Escape(written)}");
        Assert.AreEqual("{\"consistency\":[\"80Sfbj6U\"]}", lines[0], "the partial output must stay as the test wrote it, with no newline invented");
    }

    [TestMethod]
    public void AnEventAtALineStartReadsUnchanged()
    {
        string written = RunWithRawOutputFirst("whole line\n");

        Assert.AreEqual("TestBody", PassedTest(Test2JsonLines(written)), $"wrote {Escape(written)}");
        Assert.IsTrue(written.EndsWith("}\n", StringComparison.Ordinal), $"an event line still ends in a bare \"\\n\" (D5): {Escape(written)}");
    }

    private static string Escape(string text) => "\"" + text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\u0016", "^V") + "\"";
}
