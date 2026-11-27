// InternalCpuArm64Reading.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cpu = go.@internal.cpu_package;

namespace GolibTests;

/// <summary>
/// Go's own reading of <c>internal/cpu.ARM64</c> on this host, and the field-by-field comparison the darwin row
/// (DarwinCpuArm64AgainstGoTests) makes against the converted struct (D4 of DESIGN-darwin-arm64-flavour.md, section 5).
/// User Go code cannot import <c>internal/cpu</c>, so the reading is a <c>go test</c> of the standard library's own
/// <c>internal/cpu</c> package with one test file added by <c>-overlay</c>: an external test that prints every
/// exported bool field of <c>cpu.ARM64</c>. The toolchain is the pinned oracle and its GOROOT is not modified.
/// The comparison is by NAME, both ways: a field one side has and the other lacks is a difference, so a field Go adds
/// at a later release cannot be skipped in silence. The helpers are compiled on every flavour so that their own arms
/// (InternalCpuArm64ReadingTests, below) run everywhere; the row itself is darwin's.
/// </summary>
internal static class InternalCpuArm64Reading
{
    internal const string Prefix = "GO2CS-CPU ARM64.";

    internal const string OverlayTest = """
        package cpu_test

        import (
        	"fmt"
        	"reflect"
        	"testing"

        	"internal/cpu"
        )

        func TestGo2csArm64Reading(t *testing.T) {
        	v := reflect.ValueOf(&cpu.ARM64).Elem()
        	for i := 0; i < v.NumField(); i++ {
        		f := v.Type().Field(i)
        		if f.IsExported() && f.Type.Kind() == reflect.Bool {
        			fmt.Printf("GO2CS-CPU ARM64.%s=%v\n", f.Name, v.Field(i).Bool())
        		}
        	}
        }
        """;

    /// <summary>Parses the overlay test's output into field name and value. Lines without the prefix are ignored.</summary>
    internal static Dictionary<string, bool> Parse(string output)
    {
        Dictionary<string, bool> fields = new(StringComparer.Ordinal);

        foreach (string raw in output.Split('\n'))
        {
            string line = raw.TrimEnd('\r');

            if (!line.StartsWith(Prefix, StringComparison.Ordinal))
                continue;

            string[] pair = line[Prefix.Length..].Split('=');

            if (pair.Length != 2 || !bool.TryParse(pair[1], out bool value) || !fields.TryAdd(pair[0], value))
                throw new FormatException($"unreadable reading line: {line}");
        }

        return fields;
    }

    /// <summary>The converted struct's public bool fields, read from <c>cpu.ARM64</c> as start-up left it.</summary>
    internal static Dictionary<string, bool> Converted()
    {
        object boxed = cpu.ARM64;

        return boxed.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(bool))
            .ToDictionary(f => f.Name, f => f.Name == "HasCRC32" ? !(bool)f.GetValue(boxed)! : (bool)f.GetValue(boxed)!, StringComparer.Ordinal);
    }

    /// <summary>
    /// Every difference between Go's reading and the converted one, one line each, sorted by field name; empty when
    /// they agree. A name in <paramref name="departures"/> is left out of the comparison (it must still be a field).
    /// </summary>
    internal static List<string> Differences(IReadOnlyDictionary<string, bool> go, IReadOnlyDictionary<string, bool> converted,
        IReadOnlySet<string> departures)
    {
        List<string> differences = [];

        foreach (string name in go.Keys.Union(converted.Keys).Order(StringComparer.Ordinal))
        {
            bool inGo = go.TryGetValue(name, out bool goValue);
            bool inConverted = converted.TryGetValue(name, out bool convertedValue);

            if (!inGo)
                differences.Add($"{name}: the converted struct has it and Go's does not");
            else if (!inConverted)
                differences.Add($"{name}: Go's struct has it and the converted one does not");
            else if (goValue != convertedValue && !departures.Contains(name))
                differences.Add($"{name}: Go reads {goValue}, the converted side reads {convertedValue}");
        }

        foreach (string name in departures.Where(d => !go.ContainsKey(d) || !converted.ContainsKey(d)).Order(StringComparer.Ordinal))
            differences.Add($"{name}: named as a departure, but it is not a field on both sides");

        return differences;
    }

    /// <summary>
    /// Runs the overlay test with the pinned oracle: (Go's reading, null), or (null, why it could not be taken). A
    /// toolchain that resolves and then fails to answer is a failure, not a skip.
    /// </summary>
    internal static (Dictionary<string, bool>? Reading, string? Skip) TakeGoReading()
    {
        (string? go, string? skip) = ExecutionTracerParserTests.Oracle();

        if (go is null)
            return (null, skip);

        string work = Path.Combine(Path.GetTempPath(), $"go2cs-cpu-reading-{Guid.NewGuid():N}");
        Directory.CreateDirectory(work);

        try
        {
            (int envCode, string goRoot, string envErr) = Run(go, work, "env", "GOROOT");
            Assert.AreEqual(0, envCode, $"go env GOROOT failed: {envErr}");
            goRoot = goRoot.Trim();

            string test = Path.Combine(work, "reading_test.go");
            File.WriteAllText(test, OverlayTest);

            // The overlay names the file as it would sit in GOROOT's internal/cpu; JSON needs its backslashes doubled.
            string target = Path.Combine(goRoot, "src", "internal", "cpu", "zz_go2cs_reading_test.go");
            string overlay = Path.Combine(work, "overlay.json");
            File.WriteAllText(overlay, "{\"Replace\":{\"" + Json(target) + "\":\"" + Json(test) + "\"}}");

            (int code, string output, string err) = Run(go, work, "test", $"-overlay={overlay}", "-run=^TestGo2csArm64Reading$",
                "-count=1", "-v", "internal/cpu");
            Assert.AreEqual(0, code, $"go test of internal/cpu with the reading overlay failed:\n{output}\n{err}");

            Dictionary<string, bool> reading = Parse(output);
            Assert.AreNotEqual(0, reading.Count, $"the overlay test printed no reading:\n{output}");
            return (reading, null);
        }
        finally
        {
            try { Directory.Delete(work, recursive: true); } catch (IOException) { }
        }
    }

    private static string Json(string path) => path.Replace("\\", "\\\\");

    private static (int Code, string Out, string Err) Run(string go, string directory, params string[] args)
    {
        ProcessStartInfo start = new(go, args)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        // The working directory holds no go.mod, so nothing can switch the toolchain; local makes that explicit.
        start.Environment["GOTOOLCHAIN"] = "local";
        start.Environment.Remove("GOFLAGS");

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        Assert.IsTrue(process.WaitForExit(300_000), $"go {string.Join(' ', args)} did not finish in 5 minutes");
        return (process.ExitCode, stdout.Result, stderr.Result);
    }
}

/// <summary>
/// The comparison's own arms, on every flavour: a flipped field, a field on one side only and a misnamed departure
/// each name themselves, and agreement reads empty. Without these the darwin row could pass by comparing nothing.
/// </summary>
[TestClass]
public class InternalCpuArm64ReadingTests
{
    private static readonly HashSet<string> s_dit = new(StringComparer.Ordinal) { "HasDIT" };

    private static Dictionary<string, bool> Fields(params (string Name, bool Value)[] fields) =>
        fields.ToDictionary(f => f.Name, f => f.Value, StringComparer.Ordinal);

    [TestMethod]
    public void TheReadingParsesEveryPrefixedLineAndNothingElse()
    {
        Dictionary<string, bool> reading = InternalCpuArm64Reading.Parse(
            "=== RUN   TestGo2csArm64Reading\nGO2CS-CPU ARM64.HasAES=true\r\nGO2CS-CPU ARM64.HasDIT=false\n--- PASS: TestGo2csArm64Reading (0.00s)\nPASS\n");

        CollectionAssert.AreEquivalent(new[] { "HasAES", "HasDIT" }, reading.Keys.ToArray());
        Assert.IsTrue(reading["HasAES"]);
        Assert.IsFalse(reading["HasDIT"]);
    }

    [TestMethod]
    public void AnUnreadableOrRepeatedReadingLineIsRefused()
    {
        Assert.ThrowsException<FormatException>(() => InternalCpuArm64Reading.Parse("GO2CS-CPU ARM64.HasAES=yes\n"));
        Assert.ThrowsException<FormatException>(() => InternalCpuArm64Reading.Parse("GO2CS-CPU ARM64.HasAES=true\nGO2CS-CPU ARM64.HasAES=true\n"));
    }

    [TestMethod]
    public void AgreementReadsNoDifference()
    {
        var go = Fields(("HasAES", true), ("HasCRC32", true), ("HasDIT", true));
        var converted = Fields(("HasAES", true), ("HasCRC32", true), ("HasDIT", false));

        CollectionAssert.AreEqual(Array.Empty<string>(), InternalCpuArm64Reading.Differences(go, converted, s_dit),
            "HasDIT differs, and it is the named departure");
    }

    [TestMethod]
    public void AFlippedFieldNamesItself()
    {
        var go = Fields(("HasAES", true), ("HasCRC32", true), ("HasDIT", true));
        var converted = Fields(("HasAES", true), ("HasCRC32", false), ("HasDIT", false));

        CollectionAssert.AreEqual(new[] { "HasCRC32: Go reads True, the converted side reads False" },
            InternalCpuArm64Reading.Differences(go, converted, s_dit));
    }

    [TestMethod]
    public void AFieldOnOneSideOnlyNamesItself()
    {
        var go = Fields(("HasAES", true), ("HasDIT", true), ("HasSVE", false));
        var converted = Fields(("HasAES", true), ("HasDIT", false), ("HasOld", false));

        CollectionAssert.AreEqual(new[]
        {
            "HasOld: the converted struct has it and Go's does not",
            "HasSVE: Go's struct has it and the converted one does not"
        }, InternalCpuArm64Reading.Differences(go, converted, s_dit));
    }

    [TestMethod]
    public void ADepartureThatIsNotAFieldNamesItself()
    {
        var go = Fields(("HasAES", true));
        var converted = Fields(("HasAES", true));

        CollectionAssert.AreEqual(new[] { "HasDIT: named as a departure, but it is not a field on both sides" },
            InternalCpuArm64Reading.Differences(go, converted, s_dit));
    }

    // The instrument itself, on every leg (each exports GOROOT): Go's reading is taken and names exactly the converted
    // struct's fields. Values are the darwin row's question, not this one's: on linux/arm64 the converted side leaves
    // IsNeoverse false by design (linux-arm64/cpu_arm64_hwcap_impl.cs), where Go reads MIDR.
    [TestMethod]
    public void GosOwnReadingIsTakenAndNamesTheConvertedFields()
    {
        (Dictionary<string, bool>? go, string? skip) = InternalCpuArm64Reading.TakeGoReading();

        if (go is null)
        {
            Assert.Inconclusive(skip);
            return;
        }

        Dictionary<string, bool> converted = InternalCpuArm64Reading.Converted();
        string[] goOnly = go.Keys.Except(converted.Keys).Order(StringComparer.Ordinal).ToArray();
        string[] convertedOnly = converted.Keys.Except(go.Keys).Order(StringComparer.Ordinal).ToArray();

        Assert.IsTrue(goOnly.Length == 0 && convertedOnly.Length == 0,
            $"Go's internal/cpu.ARM64 and the converted struct must carry the same fields; Go's only: [{string.Join(", ", goOnly)}], " +
            $"the converted struct's only: [{string.Join(", ", convertedOnly)}]");
    }

    [TestMethod]
    public void TheConvertedReadingCarriesEveryFieldOfGosStruct()
    {
        // internal/cpu's ARM64 at the pinned release (cpu.go), in its order.
        CollectionAssert.AreEquivalent(
            new[] { "HasAES", "HasPMULL", "HasSHA1", "HasSHA2", "HasSHA512", "HasSHA3", "HasCRC32", "HasATOMICS", "HasCPUID", "HasDIT", "IsNeoverse" },
            InternalCpuArm64Reading.Converted().Keys.ToArray());
    }
}
