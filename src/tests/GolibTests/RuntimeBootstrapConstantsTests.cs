using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards the linux bootstrap constants (increment 7 of the runtime row): physPageSize is the OS page
/// size Go's osinit path would have read from AT_PAGESZ, and physHugePageSize is the transparent
/// huge page size parsed exactly as getHugePageSize parses the sysfs file — read here a SECOND way so
/// the arm is a derivation, not a restatement. Linux only: the fields are the linux flavour's.
/// </summary>
[TestClass]
public class RuntimeBootstrapConstantsTests
{
    private static bool OnLinux => OperatingSystem.IsLinux();

    [TestMethod]
    public void PhysPageSizeIsTheSystemPageSize()
    {
        if (!OnLinux) Assert.Inconclusive("the constants are the linux flavour's");
        (nuint pageSize, nuint _) = GoBootstrapConstants();
        Assert.AreEqual((nuint)Environment.SystemPageSize, pageSize, "sysconf(_SC_PAGESIZE) is AT_PAGESZ");
        Assert.IsTrue(pageSize >= 4096, "mallocinit's minPhysPageSize");
        Assert.AreEqual((nuint)0, pageSize & (pageSize - 1), "a power of two");
    }

    [TestMethod]
    public void PhysHugePageSizeIsTheSysfsValueUnderGosParse()
    {
        if (!OnLinux) Assert.Inconclusive("the constants are the linux flavour's");
        (nuint _, nuint hugePageSize) = GoBootstrapConstants();

        // The second derivation: the file, read and parsed independently of the runtime's own read.
        const string path = "/sys/kernel/mm/transparent_hugepage/hpage_pmd_size";
        nuint expected = 0;

        if (File.Exists(path))
        {
            string text = File.ReadAllText(path).Trim();
            if (ulong.TryParse(text, out ulong value) && value != 0 && (value & (value - 1)) == 0)
                expected = (nuint)value;
        }

        Assert.AreEqual(expected, hugePageSize, "the runtime holds what the file says, or 0 when it cannot be read");
        Assert.AreEqual((nuint)0, hugePageSize & (hugePageSize - 1), "zero or a power of two — mallocinit's own check");
    }

    [TestMethod]
    public void TheParseIsGetHugePageSizes()
    {
        Assert.AreEqual((nuint)2097152, GoParseHugePageSize("2097152\n"), "leading digits, newline ignored");
        Assert.AreEqual((nuint)0, GoParseHugePageSize(""), "an empty read answers 0");
        Assert.AreEqual((nuint)0, GoParseHugePageSize("abc"), "no digits answers 0");
        Assert.AreEqual((nuint)0, GoParseHugePageSize("3000000"), "a non-power-of-two answers 0, as Go's `v&(v-1) != 0` does");
        Assert.AreEqual((nuint)4096, GoParseHugePageSize("4096 bytes"), "Go stops at the first non-digit");
    }
}
