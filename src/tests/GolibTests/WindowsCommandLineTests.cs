#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// C44: a converted program's os.Args on Windows, under its apphost AND under the `dotnet prog.dll` muxer
/// (syscall/windows/zsyscall_windows_cmdline_impl.cs cuts the muxer's prefix from GetCommandLine; os's own
/// commandLineToArgv still splits what remains).
/// - The LIVE arm builds a tiny probe once, a console program referencing this directory's converted os, syscall and
///   golib assemblies that prints os.Args[1:] one per line. It launches the probe as probe.exe, `dotnet probe.dll` and
///   `dotnet exec probe.dll`, with arguments carrying spaces, quotes and trailing backslashes (ArgumentList does the
///   Windows quoting; Go's split must undo it). Every launch must read back exactly the arguments it was given.
/// - The MIRROR arm holds syscall's scanner equal to os.commandLineToArgv over a corpus of tricky lines, because the
///   cut is located by the mirror and the split is done by Go.
/// - The DETECTION arm drives the four rules over explicit inputs, so each refusal is pinned.
/// - Windows flavour only: GolibTests.csproj excludes this file under any other $(GoTargetOS).
/// </summary>
[TestClass]
public class WindowsCommandLineTests
{
    private static string? s_probeDir;
    private static string? s_buildFailure;

    [ClassInitialize]
    public static void BuildProbe(TestContext _)
    {
        string bin = AppContext.BaseDirectory;
        string dir = Path.Combine(Path.GetTempPath(), $"go2cs-c44-probe-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);

        string Ref(string name) =>
            $"<Reference Include=\"{name}\"><HintPath>{Path.Combine(bin, name + ".dll")}</HintPath></Reference>";

        File.WriteAllText(Path.Combine(dir, "probe.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType>" +
            "<TargetFramework>net10.0</TargetFramework><Nullable>disable</Nullable><ImplicitUsings>disable</ImplicitUsings>" +
            "<AssemblyName>probe</AssemblyName><UseAppHost>true</UseAppHost></PropertyGroup><ItemGroup>" +
            Ref("go.os") + Ref("go.syscall") + Ref("golib") + "</ItemGroup></Project>");

        File.WriteAllText(Path.Combine(dir, "Program.cs"),
            "public static class Program { public static void Main() {" +
            " var args = go.os_package.Args;" +
            " for (int i = 1; i < (int)go.builtin.len(args); i++) System.Console.WriteLine(args[i].ToString()); } }");

        (int code, string output) = Run("dotnet", ["build", Path.Combine(dir, "probe.csproj"), "-c", "Release", "-o", Path.Combine(dir, "out"), "-nologo", "-v", "q"], dir);
        if (code != 0)
            s_buildFailure = $"the probe did not build (exit {code}):\n{output}";
        else
            s_probeDir = Path.Combine(dir, "out");
    }

    private static (int code, string output) Run(string file, IEnumerable<string> args, string workingDir)
    {
        ProcessStartInfo start = new(file) { WorkingDirectory = workingDir, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (string a in args)
            start.ArgumentList.Add(a);
        start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        start.Environment["DOTNET_NOLOGO"] = "1";

        using Process p = Process.Start(start)!;
        string stdout = p.StandardOutput.ReadToEnd();
        string stderr = p.StandardError.ReadToEnd();
        if (!p.WaitForExit(120_000))
        {
            p.Kill(entireProcessTree: true);
            return (-1, "TIMEOUT\n" + stdout + stderr);
        }
        return (p.ExitCode, stdout + (stderr.Length > 0 ? "\nSTDERR:\n" + stderr : ""));
    }

    private static readonly string[] Arguments = ["x", "y", "a b", "say \"hi\"", @"c:\dir\", @"back\\slash", "", "tab\there"];

    private static void AssertLaunchReadsBack(string file, string[] prefix)
    {
        if (s_buildFailure is not null)
            Assert.Fail(s_buildFailure);

        (int code, string output) = Run(file, prefix.Concat(Arguments), s_probeDir!);
        Assert.AreEqual(0, code, $"the probe exited {code}:\n{output}");

        string[] lines = output.Replace("\r\n", "\n").Split('\n');
        // The probe prints one line per argument, then the final newline leaves one empty element.
        string[] got = lines.Take(lines.Length - 1).ToArray();
        CollectionAssert.AreEqual(Arguments, got, $"os.Args[1:] under `{file} {string.Join(' ', prefix)}`:\n{output}");
    }

    [TestMethod]
    public void TheApphostLaunchReadsItsOwnArguments() =>
        AssertLaunchReadsBack(Path.Combine(s_probeDir ?? "", "probe.exe"), []);

    [TestMethod]
    public void TheMuxerLaunchReadsItsOwnArguments() =>
        AssertLaunchReadsBack("dotnet", [Path.Combine(s_probeDir ?? "", "probe.dll")]);

    [TestMethod]
    public void AMuxerLaunchWithHostOptionsReadsItsOwnArguments() =>
        AssertLaunchReadsBack("dotnet", ["exec", "--runtimeconfig", Path.Combine(s_probeDir ?? "", "probe.runtimeconfig.json"), Path.Combine(s_probeDir ?? "", "probe.dll")]);

    // Lines whose split exercises every rule readNextArg has: whitespace and tabs, quotes, the 2n/2n+1 backslash rule,
    // the "Prior to 2008" doubled quote, an unterminated quote, an empty argument, trailing backslashes, non-ASCII.
    private static readonly string[] Corpus =
    [
        "prog.exe a b",
        "\"C:\\Program Files\\dotnet\\dotnet.exe\" \"C:\\my app\\prog.dll\" \"a b\" c",
        "a\\\\\\\"b \"c d\\\\\" e",
        "\"a\"\"b\" c",
        "x \t  \t y\t",
        "\"unterminated quote and more",
        "trailing\\\\",
        "a\\\\\\\\\"b c\" d",
        "\"\" empty \"\"",
        "h\u00e9llo \"w\u00f6rld x\" \u4e2d",
        "\\\\server\\share\\x.dll \"a\\\" b\"",
        "   leading and trailing   ",
    ];

    [TestMethod]
    public void TheMirrorScannerEqualsGosSplit()
    {
        foreach (string line in Corpus)
        {
            CollectionAssert.AreEqual(os_package.GoCommandLineToArgvProbe(line), syscall_package.GoCommandLineScanProbe(line),
                $"the mirror and os.commandLineToArgv disagree on: {line}");
        }
    }

    [TestMethod]
    public void TheDetectionCutsOnlyAMuxerLaunchOfThisProgram()
    {
        const string dotnet = @"C:\dotnet\dotnet.exe";
        const string entry = @"C:\app\prog.dll";

        string Cut(string raw, string? host, string? dll) => raw[syscall_package.GoMuxerPrefixLengthProbe(raw, host, dll)..];

        Assert.AreEqual(@"C:\app\prog.dll a b", Cut(@"""C:\dotnet\dotnet.exe"" C:\app\prog.dll a b", dotnet, entry), "the plain muxer line");
        Assert.AreEqual(@"C:\app\prog.dll x", Cut(@"dotnet exec --runtimeconfig C:\app\prog.runtimeconfig.json C:\app\prog.dll x", dotnet, entry), "host options before the dll");
        Assert.AreEqual(@"""C:\my app\prog.dll"" ""a b""", Cut(@"dotnet ""C:\my app\prog.dll"" ""a b""", dotnet, @"C:\my app\prog.dll"), "a quoted path with a space, cut at its raw start");
        Assert.AreEqual(@"C:\APP\PROG.DLL a", Cut(@"dotnet C:\APP\PROG.DLL a", dotnet, entry), "the path compare ignores case");

        Assert.AreEqual(0, syscall_package.GoMuxerPrefixLengthProbe(@"C:\app\prog.exe a b", @"C:\app\prog.exe", entry), "an apphost launch never rewrites");
        Assert.AreEqual(0, syscall_package.GoMuxerPrefixLengthProbe(@"dotnet C:\app\prog.dll a", dotnet, ""), "no entry path (single-file) never rewrites");
        Assert.AreEqual(0, syscall_package.GoMuxerPrefixLengthProbe(@"dotnet C:\app\other.dll a", dotnet, entry), "a muxer line that does not name this program is left alone");
        Assert.AreEqual(0, syscall_package.GoMuxerPrefixLengthProbe(@"C:\app\prog.dll a", dotnet, entry), "token 0 is the host and is never the cut");
    }
}
