using System;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using C = go.cgotestꓸC_package;

namespace GolibTests;

// cgo Phase 2 increment 4d (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.10): the exit gate's arms that no system library
// gives on every OS, run against the probe library (cgo-probe/probe.c) through the same generated golden as 3b1-4c. The
// probe is bound as any C library is: captured by name, registered with golib's resolver by the golden's module
// initializer (4c), and loaded the resolver's way, never from the current directory.
//
// The hosted golib-tests legs build the probe per RID with the OS's default pinned C toolchain and REQUIRE it
// (GO2CS_REQUIRE_CGO_PROBE=1), so a leg that cannot load it fails. Anywhere nothing built it, these read Inconclusive.
[TestClass]
[DoNotParallelize]
public unsafe class CgoProbeTests
{
    private static T OnAGoroutine<T>(Func<T> call) => CgoScalarWrapperTests.OnAGoroutine(call);

    // A pair by value both ways: §11.7's stand-in for inet_ntoa on windows, whose in_addr is a union there. (3, 4) sums
    // to 3004 in C, so a swapped or dropped half reads differently.
    [TestMethod]
    public void APairCrossesByValueBothWays()
    {
        CgoProbe.Require();

        (int32 a, int32 b, int32 sum) = OnAGoroutine(() =>
        {
            C.struct_go2cs_probe_pair pair = C.go2cs_probe_pair(3, 4);
            return ((int32)pair.a, (int32)pair.b, (int32)C.go2cs_probe_pair_sum(pair));
        });

        CgoProbe.ReadLoad();
        Assert.AreEqual(3, a, "the returned pair's first int");
        Assert.AreEqual(4, b, "and its second");
        Assert.AreEqual(3004, sum, "and the pair C was handed by value, in order");
    }

    // §5.2's aliasing arm on every OS: add_into(&p, &p, &y). C is handed ONE pointer for both, and its second store reads
    // x->a after the first wrote dst->a, so one shared object gives b = (1 + 10) + 20 where two copies give 1 + 20.
    [TestMethod]
    public void TwoPointersToOneStructAreOneObjectToC()
    {
        CgoProbe.Require();

        (int32 same, int32 a, int32 b) = OnAGoroutine(() =>
        {
            ж<C.struct_go2cs_probe_pair> p = new StandardBox<C.struct_go2cs_probe_pair>(new C.struct_go2cs_probe_pair { a = 1, b = 2 });
            ж<C.struct_go2cs_probe_pair> y = new StandardBox<C.struct_go2cs_probe_pair>(new C.struct_go2cs_probe_pair { a = 10, b = 20 });
            int32 aliased = C.go2cs_probe_add_into(p, p, y);
            return (aliased, (int32)p.Value.a, (int32)p.Value.b);
        });

        Assert.AreEqual(1, same, "C was handed the same address for dst and x");
        Assert.AreEqual(11, a, "dst->a = x->a + y->a");
        Assert.AreEqual(31, b, "dst->b read x->a after the write through dst: one object");
    }

    // A union inside a struct crosses as the bytes cmd/cgo gives it, after the padding that aligns it: C doubles the
    // member the tag names, as a long long (tag 0) or as a double (tag 1).
    [TestMethod]
    public void AUnionInAStructCrossesAsItsBytes()
    {
        CgoProbe.Require();

        (int32 tag0, long l, int32 tag1, double d) = OnAGoroutine(() =>
        {
            ж<C.struct_go2cs_probe_tagged> integer = Tagged(0, BitConverter.GetBytes(21L));
            ж<C.struct_go2cs_probe_tagged> real = Tagged(1, BitConverter.GetBytes(1.5));
            int32 t0 = C.go2cs_probe_tagged_double(integer);
            int32 t1 = C.go2cs_probe_tagged_double(real);
            return (t0, BitConverter.ToInt64(Bytes(integer.Value)), t1, BitConverter.ToDouble(Bytes(real.Value)));
        });

        Assert.AreEqual(0, tag0, "the tag crossed in its own member");
        Assert.AreEqual(42L, l, "and the union's bytes crossed at the union's offset, as a long long");
        Assert.AreEqual(1, tag1, "the second tag");
        Assert.AreEqual(3.0, d, "and as a double");
    }

    // Two doubles by pointer: a homogeneous floating-point aggregate on arm64. By value §11.7 refuses it (unit-tested in
    // the generator); by pointer it is a mirror like any other.
    [TestMethod]
    public void TwoDoublesCrossByPointer()
    {
        CgoProbe.Require();

        (double sum, double x, double y) = OnAGoroutine(() =>
        {
            ж<C.struct_go2cs_probe_hfa> h = new StandardBox<C.struct_go2cs_probe_hfa>(new C.struct_go2cs_probe_hfa { x = 1.5, y = 2.5 });
            float64 total = C.go2cs_probe_hfa_scale(h, 2.0);
            return (total, (float64)h.Value.x, (float64)h.Value.y);
        });

        Assert.AreEqual(8.0, sum, "the scaled sum");
        Assert.AreEqual(3.0, x, "x was written through the pointer");
        Assert.AreEqual(5.0, y, "and y");
    }

    // §11.10's errno arm, COORD's ruling (c): the probe calls strtol, so ERANGE is set by the real C library's strtol, and
    // the probe takes only a native char*. Out of range, strtol returns LONG_MAX with ERANGE; in range, a nil error.
    [TestMethod]
    public void StrtolsErrnoReachesTheErrnoForm()
    {
        CgoProbe.Require();

        if (OperatingSystem.IsWindows())
        {
            // On windows errno is the C RUNTIME's: the wrapper reads ucrtbase's slot (the golden's errno binding), so the
            // probe's strtol must be the UCRT's, imported from it, and not a static copy with an errno of its own.
            string crt = CgoProbe.ImportOf("strtol") ?? "(none: strtol is linked into the probe, with its own errno)";
            Console.WriteLine($"READING the probe imports strtol from {crt}");
            Assert.IsTrue(crt.StartsWith("api-ms-win-crt-", StringComparison.OrdinalIgnoreCase) || crt.Equals("ucrtbase.dll", StringComparison.OrdinalIgnoreCase),
                $"the probe's strtol is not the UCRT's, so the wrapper's ucrtbase errno cannot see it: {crt}");
        }

        int64 max = OperatingSystem.IsWindows() ? int.MaxValue : long.MaxValue;

        (int64 high, error? overflow) = Strtol("99999999999999999999999");
        (int64 small, error? none) = Strtol("42");

        Assert.AreEqual(max, high, "strtol returns LONG_MAX out of range");
        Assert.IsNotNull(overflow, "and its errno arrives as the error");
        Assert.AreEqual(new syscall_package.Errno(34), (syscall_package.Errno)overflow, "ERANGE");
        Assert.AreEqual(42L, small, "in range");
        Assert.IsNull(none, "a zero errno is a nil error, as cgo makes it");
    }

    private static (int64, error?) Strtol(string text)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(text + "\0");
        byte* memory = (byte*)NativeMemory.Alloc((nuint)bytes.Length);
        nuint address = (nuint)memory;

        try
        {
            bytes.CopyTo(new Span<byte>(memory, bytes.Length));

            return OnAGoroutine(() =>
            {
                (C.@long r, error e) = C.go2cs_probe_strtolꓸerrno(new NativeBox<C.@char>(address), 10);
                return ((int64)r, (error?)e);
            });
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    private static ж<C.struct_go2cs_probe_tagged> Tagged(int32 tag, byte[] bytes)
    {
        C.struct_go2cs_probe_tagged tagged = new() { tag = tag };

        for (int i = 0; i < 8; i++)
            tagged.u[i] = bytes[i];

        return new StandardBox<C.struct_go2cs_probe_tagged>(tagged);
    }

    private static byte[] Bytes(C.struct_go2cs_probe_tagged tagged) => [.. Enumerable.Range(0, 8).Select(i => tagged.u[i])];
}

// The probe library on this host: where the hosted leg built it, or an Inconclusive (a Fail where the leg requires it).
internal static class CgoProbe
{
    // The load-time name each OS's golden binds (src/go2cs's cgoProbeLibrary).
    public static string Name =>
        OperatingSystem.IsWindows() ? "go2csprobe.dll" :
        OperatingSystem.IsMacOS() ? "libgo2csprobe.dylib" :
        "libgo2csprobe.so";

    // The built probe's path. On windows it is also copied beside the test assembly, the directory the resolver searches
    // there; linux and darwin find it through the loader path the leg set.
    public static string Require()
    {
        string? directory = Environment.GetEnvironmentVariable("GO2CS_CGO_PROBE_DIR");
        string? path = string.IsNullOrEmpty(directory) ? null : Path.Combine(directory, Name);

        if (path is null || !File.Exists(path))
        {
            string why = path is null ? "GO2CS_CGO_PROBE_DIR is not set" : $"{path} does not exist";

            if (Environment.GetEnvironmentVariable("GO2CS_REQUIRE_CGO_PROBE") == "1")
                Assert.Fail($"this leg requires the cgo probe library, and {why}");

            Assert.Inconclusive($"the cgo probe library is built only on the hosted golib-tests legs ({why}): NOT MEASURED");
        }

        if (OperatingSystem.IsWindows())
        {
            string beside = Path.Combine(AppContext.BaseDirectory, Name);

            if (!File.Exists(beside))
                File.Copy(path, beside);
        }

        return path;
    }

    // Which route loaded the probe by its bare name, for the leg's summary: on darwin, whether DYLD_LIBRARY_PATH reached
    // this process (a hardened host strips it, and §11.12's absolute name is the fallback).
    public static void ReadLoad()
    {
        string route =
            OperatingSystem.IsWindows() ? "the test assembly's directory" :
            OperatingSystem.IsMacOS() ? $"DYLD_LIBRARY_PATH as this process sees it: {Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH") ?? "(stripped or unset)"}" :
            $"LD_LIBRARY_PATH: {Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? "(unset)"}";

        Console.WriteLine($"READING the probe loaded by its bare name {Name} through {route}");
    }

    // The DLL the probe imports symbol from, read from its PE import table, or null when it does not import it.
    public static string? ImportOf(string symbol)
    {
        byte[] image = File.ReadAllBytes(Require());
        using PEReader reader = new(new MemoryStream(image));

        PEHeaders headers = reader.PEHeaders;
        bool pe32Plus = headers.PEHeader!.Magic == PEMagic.PE32Plus;

        int Offset(int rva)
        {
            foreach (SectionHeader section in headers.SectionHeaders)
            {
                if (rva >= section.VirtualAddress && rva < section.VirtualAddress + Math.Max(section.VirtualSize, section.SizeOfRawData))
                    return rva - section.VirtualAddress + section.PointerToRawData;
            }

            throw new InvalidDataException($"RVA 0x{rva:x} is in no section of the probe");
        }

        string Ascii(int offset) => Encoding.ASCII.GetString(image, offset, Array.IndexOf(image, (byte)0, offset) - offset);

        for (int descriptor = Offset(headers.PEHeader.ImportTableDirectory.RelativeVirtualAddress); ; descriptor += 20)
        {
            int lookup = BitConverter.ToInt32(image, descriptor);
            int name = BitConverter.ToInt32(image, descriptor + 12);
            int thunks = BitConverter.ToInt32(image, descriptor + 16);

            if (name == 0)
                return null;

            for (int entry = Offset(lookup != 0 ? lookup : thunks); ; entry += pe32Plus ? 8 : 4)
            {
                long value = pe32Plus ? BitConverter.ToInt64(image, entry) : BitConverter.ToUInt32(image, entry);

                if (value == 0)
                    break;

                bool byOrdinal = pe32Plus ? value < 0 : (value & 0x80000000) != 0;

                if (!byOrdinal && Ascii(Offset((int)(value & 0x7fffffff)) + 2) == symbol)
                    return Ascii(Offset(name));
            }
        }
    }
}
