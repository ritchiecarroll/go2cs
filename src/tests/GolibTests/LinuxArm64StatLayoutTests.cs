using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// S3's arm64 Stat_t mirror (syscall/linux-arm64/zsyscall_linux_arm64_impl.cs): its measured size and field offsets,
/// through GoNativeStatLayoutProbe, against Go's own ztypes_linux_arm64.go layout.
/// - The expected values are Go's, read by the census's layoutprobe
///   (docs/phase4/data/la64-s3/layouts-linux-amd64-arm64.tsv); a timespec is Sec@0 Nsec@8 within its field.
/// - linux/arm64 flavour only: the mirror exists in no other build (GolibTests.csproj excludes this file unless
///   $(GoTargetOS) is linux AND $(GoTargetArch) is arm64).
/// </summary>
[TestClass]
public class LinuxArm64StatLayoutTests
{
    [TestMethod]
    public void TheArm64StatMirrorHasGosSizeAndOffsets()
    {
        var (size, offsets) = syscall_package.GoNativeStatLayoutProbe();

        Assert.AreEqual(128, size, "size (amd64's struct stat is 144)");

        (string field, int offset)[] expected =
        [
            ("Dev", 0), ("Ino", 8), ("Mode", 16), ("Nlink", 20), ("Uid", 24), ("Gid", 28), ("Rdev", 32),
            ("X__pad1", 40), ("Size", 48), ("Blksize", 56), ("X__pad2", 60), ("Blocks", 64),
            ("Atim.Sec", 72), ("Atim.Nsec", 80), ("Mtim.Sec", 88), ("Mtim.Nsec", 96),
            ("Ctim.Sec", 104), ("Ctim.Nsec", 112), ("X__glibc_reserved", 120),
        ];

        CollectionAssert.AreEqual(expected, offsets, "field offsets, in Go's field order");
    }
}
