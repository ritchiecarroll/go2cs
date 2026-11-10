#nullable enable

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// <c>GoHostOfRecord.Describe</c>, the run-time half of the unshipped-platform refusal (go.lib.targets'
/// GoRefuseUnshippedRid is the build-time half). A portable app built on a shipped platform and RUN elsewhere loads
/// <c>lib/</c>, the Windows flavour, or a flavour built for another architecture; the build cannot see the host, so
/// the converted runtime describes the mismatch once on stderr. Every case is the decision alone, with the host
/// supplied, so each one runs on any machine.
/// </summary>
[TestClass]
public class HostOfRecordTests
{
    [TestMethod]
    public void TheBuildOfTheHostsOwnPlatformSaysNothing()
    {
        Assert.IsNull(GoHostOfRecord.Describe("linux", "amd64", "linux", Architecture.X64));
        Assert.IsNull(GoHostOfRecord.Describe("windows", "amd64", "windows", Architecture.X64));
        Assert.IsNull(GoHostOfRecord.Describe("darwin", "amd64", "darwin", Architecture.X64));
    }

    [TestMethod]
    public void AnotherOperatingSystemsBuildIsNamed()
    {
        string? message = GoHostOfRecord.Describe("windows", "amd64", "linux", Architecture.X64);
        Assert.IsNotNull(message);
        StringAssert.Contains(message, "\"windows\" build");
        StringAssert.Contains(message, "\"linux\" host");
    }

    [TestMethod]
    public void AnUnrecognizedHostSaysNothing()
    {
        Assert.IsNull(GoHostOfRecord.Describe("windows", "amd64", "", Architecture.Arm64));
    }

    // The linux flavour makes system calls by number and lays out kernel structures by architecture, so its amd64
    // build on an arm64 host is a different ABI: the case a portable app built on linux-x64 and run on linux-arm64
    // reaches, which no build-time check can see.
    [TestMethod]
    public void TheLinuxBuildOfAnotherArchitectureIsNamed()
    {
        string? message = GoHostOfRecord.Describe("linux", "amd64", "linux", Architecture.Arm64);
        Assert.IsNotNull(message);
        StringAssert.Contains(message, "\"amd64\"");
        StringAssert.Contains(message, "\"arm64\"");
    }

    // The windows and darwin flavours reach the operating system through its own libraries, whose calls do not
    // depend on the architecture, and their amd64 build is what the packages ship for win-arm64 (with GO2CS3004 at
    // build time) and for Apple silicon (a supported platform). A warning there would be a false alarm.
    [TestMethod]
    public void TheWindowsAndDarwinBuildsOfAnotherArchitectureSayNothing()
    {
        Assert.IsNull(GoHostOfRecord.Describe("windows", "amd64", "windows", Architecture.Arm64));
        Assert.IsNull(GoHostOfRecord.Describe("darwin", "amd64", "darwin", Architecture.Arm64));
    }

    // The old single-argument Verify passes no GOARCH: an empty one checks the operating system only, so a runtime
    // assembly built before this check keeps its behaviour against a newer go.lib.
    [TestMethod]
    public void NoArchitectureChecksTheOperatingSystemOnly()
    {
        Assert.IsNull(GoHostOfRecord.Describe("linux", "", "linux", Architecture.Arm64));
    }
}
