using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// S3 site 6 (docs/phase4/DATA-la64-s3-census.md §2.1, §2.2): syscall.Select takes each linux arch's own trap, and
/// with it Go's own timeout semantics.
/// - linux/amd64: Go's Select is select(2) on the caller's Timeval, which the kernel overwrites with the time not
///   slept, so an expired timeout reads back {0, 0}.
/// - linux/arm64: Go's Select is pselect6 on a TEMPORARY Timespec converted from the Timeval, so the caller's
///   Timeval comes back unchanged.
/// - The flavour is read from the converted table itself: SYS_PSELECT6 is 270 at amd64 and 72 at arm64 (Go's
///   go1.26.10 zsysnum tables).
/// - Linux flavour only: SYS_PSELECT6 exists in no other table (GolibTests.csproj excludes this file under every
///   other $(GoTargetOS)).
/// </summary>
[TestClass]
public class LinuxSelectTimeoutTests
{
    [TestMethod]
    public void SelectTakesEachArchsOwnTimeoutSemantics()
    {
        bool arm64 = (long)syscall_package.SYS_PSELECT6 switch
        {
            270 => false,
            72 => true,
            var other => throw new AssertFailedException($"SYS_PSELECT6 is {other}: neither linux/amd64's 270 nor linux/arm64's 72"),
        };

        // No descriptors, so the call can only time out: n is 0, and only the timeout's fate differs by arch.
        ж<syscall_package.Timeval> Ꮡtv = Ꮡ(new syscall_package.Timeval(Sec: 0, Usec: 20_000));

        var (n, err) = syscall_package.Select(0, null!, null!, null!, Ꮡtv);

        Assert.IsNull(err, "Select with no descriptors and a 20 ms timeout");
        Assert.AreEqual((nint)0, n, "no descriptor is ready");

        if (arm64)
        {
            Assert.AreEqual((0L, 20_000L), ((long)Ꮡtv.Value.Sec, (long)Ꮡtv.Value.Usec),
                "linux/arm64: pselect6 wrote Go's temporary Timespec, never the caller's Timeval");
        }
        else
        {
            Assert.AreEqual((0L, 0L), ((long)Ꮡtv.Value.Sec, (long)Ꮡtv.Value.Usec),
                "linux/amd64: select(2) wrote the time not slept (none) back into the caller's Timeval");
        }
    }
}
