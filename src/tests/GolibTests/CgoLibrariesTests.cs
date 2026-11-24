using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// cgo Phase 2 increment 4a (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §6.2, read through §11.11): the resolver a converted
// cgo package registers for its own assembly. For a library its table owns, it loads the captured name from the places
// a Go binary's loader would search and NEVER the current directory, and a miss throws, naming the -l flag, the captured
// name and the places tried. A name it does not own is left to the runtime.
//
// The current-directory arm plants a COPY of a system library under a probe name, and first proves the copy loads by
// its absolute path, so a refusal cannot be a broken copy. darwin keeps its system dylibs in the shared cache, with no
// file on disk to copy, so its arm plants a copy of the probe library (increment 4d, CgoProbe), and the resolver is asked
// for it by its bare name, the name dyld's leaf-name search would meet. That search DID reach the current directory on
// both hosted RIDs (42936395011), so on darwin the resolver now refuses any name that is not absolute (§11.12), and this
// arm is that rule's guard.
[TestClass]
[DoNotParallelize]
public class CgoLibrariesTests
{
    private static readonly Assembly s_assembly = typeof(CgoLibrariesTests).Assembly;

    private static (string flag, string name) SystemLibrary =>
        OperatingSystem.IsWindows() ? ("-lucrt", "ucrtbase.dll") :
        OperatingSystem.IsMacOS() ? ("-lSystem", "/usr/lib/libSystem.B.dylib") :
        ("-lm", "libm.so.6");

    private static string ProbeFileName(string stem) =>
        OperatingSystem.IsWindows() ? $"{stem}.dll" :
        OperatingSystem.IsMacOS() ? $"lib{stem}.dylib" :
        $"lib{stem}.so";

    private static List<GoCgoLibrary> Table(params (string flag, string name)[] libraries) =>
        [.. libraries.Select(library => new GoCgoLibrary(library.flag, library.name))];

    // The file a planted copy is made from: the system library itself on windows, on linux the file the loader mapped
    // for libm.so.6, and on darwin the probe library, since its system dylibs have no file on disk.
    private static string SystemLibraryFile()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.SystemDirectory, "ucrtbase.dll");

        if (OperatingSystem.IsMacOS())
            return CgoProbe.Require();

        NativeLibrary.Free(NativeLibrary.Load("libm.so.6"));
        NativeLibrary.Load("libm.so.6");

        return File.ReadLines("/proc/self/maps")
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(fields => fields.Length >= 6 && Path.GetFileName(fields[5]) == "libm.so.6")
            .Select(fields => fields[5])
            .First();
    }

    // Increment 4c: the generated bindings (GoCgoWrappers.<os>.golden.cs) register their table with this resolver when
    // the module loads, so this assembly's resolver is already set and a second registration is refused.
    [TestMethod]
    public void TheGeneratedBindingsRegisterTheirLibrariesWhenTheModuleLoads()
    {
        InvalidOperationException? refused = null;

        try
        {
            NativeLibrary.SetDllImportResolver(s_assembly, static (_, _, _) => 0);
        }
        catch (InvalidOperationException ex)
        {
            refused = ex;
        }

        Assert.IsNotNull(refused, "no resolver was registered for this assembly when its module loaded");
    }

    [TestMethod]
    public void AnOwnedLibraryLoads()
    {
        (string flag, string name) = SystemLibrary;
        nint handle = GoCgoLibraries.Resolve(name, s_assembly, Table((flag, name)));

        Assert.AreNotEqual((nint)0, handle, $"{flag} binds {name}, which every host has");
    }

    [TestMethod]
    public void ANameTheTableDoesNotOwnIsLeftToTheRuntime()
    {
        (string flag, string name) = SystemLibrary;

        Assert.AreEqual((nint)0, GoCgoLibraries.Resolve(name, s_assembly, Table((flag, "someotherlibrary"))),
            "a hand-owned import in the same assembly keeps the runtime's own resolution");
    }

    // §5.3, as amended by §11.20 (increment 5b): bindings are converted for one target and register it, and a library they
    // own is refused by name on any other, since a linux binding run on windows would cross with linux's layouts. The
    // architecture is the PROCESS's (S1): an x64 process under Rosetta or on Windows on Arm runs amd64 code.
    [TestMethod]
    public void AnOwnedLibraryOfBindingsForAnotherTargetIsRefusedNamingBoth()
    {
        (string flag, string name) = SystemLibrary;
        PlatformNotSupportedException? thrown = null;

        try
        {
            GoCgoLibraries.Resolve(name, s_assembly, Table((flag, name)), "plan9/mips");
        }
        catch (PlatformNotSupportedException ex)
        {
            thrown = ex;
        }

        Assert.IsNotNull(thrown, "bindings converted for another target must not load here");
        StringAssert.Contains(thrown.Message, "plan9/mips");
        StringAssert.Contains(thrown.Message, GoCgoLibraries.RunningTarget);
    }

    // The review of increment 5b, S5: the target is held for every import the assembly makes, not only for a name the
    // table owns. A hand-owned import beside bindings converted for another target crosses with the wrong layouts too.
    [TestMethod]
    public void ANameTheTableDoesNotOwnIsRefusedUnderAnotherTargetToo()
    {
        (string flag, string name) = SystemLibrary;

        // ThrowsException matches the EXACT type: A6's refusal is GoCgoTargetMismatchException, a PlatformNotSupportedException.
        Assert.ThrowsException<GoCgoTargetMismatchException>(() => GoCgoLibraries.Resolve(name, s_assembly, Table((flag, "someotherlibrary")), "plan9/mips"),
            "an import of an assembly whose bindings were converted for another target must not load here");
    }

    [TestMethod]
    public void AnOwnedLibraryOfBindingsForTheRunningTargetLoads()
    {
        (string flag, string name) = SystemLibrary;
        nint handle = GoCgoLibraries.Resolve(name, s_assembly, Table((flag, name)), GoCgoLibraries.RunningTarget);

        Assert.AreNotEqual((nint)0, handle, $"{flag} binds {name} for {GoCgoLibraries.RunningTarget}, the running target");
    }

    [TestMethod]
    [DataRow("linux/amd64", "linux", Architecture.X64, false)]
    [DataRow("darwin/amd64", "darwin", Architecture.X64, false)]
    [DataRow("windows/amd64", "windows", Architecture.X64, false)]
    [DataRow("linux/amd64", "linux", Architecture.Arm64, true)]
    [DataRow("darwin/arm64", "darwin", Architecture.X64, true)]
    [DataRow("windows/amd64", "linux", Architecture.X64, true)]
    public void BindingsRunOnlyOnTheTargetTheyWereConvertedFor(string target, string hostGOOS, Architecture architecture, bool refused)
    {
        string? message = GoCgoLibraries.TargetMismatch(target, hostGOOS, architecture);

        Assert.AreEqual(refused, message is not null, $"bindings for {target} on {hostGOOS} with a {architecture} process: {message}");

        if (refused)
        {
            StringAssert.Contains(message, target);
            StringAssert.Contains(message, hostGOOS + "/" + (architecture == Architecture.X64 ? "amd64" : "arm64"));
        }
    }

    // cgo against go.* INC 3, A6: Register itself refuses bindings converted for another target, in the package's module
    // initializer, before ANY of its code runs (its managed code carries C layouts folded for the target). This row is A6's
    // guard (P3's read): with the eager check removed, Register reaches SetDllImportResolver, which refuses a second
    // resolver for this assembly with an InvalidOperationException, and the row fails naming it.
    [TestMethod]
    public void RegisterRefusesBindingsForAnotherTargetBeforeAnythingElse()
    {
        Exception? thrown = null;

        try
        {
            GoCgoLibraries.Register(s_assembly, "plan9/mips", new GoCgoLibrary("-lm", "libm.so.6"));
        }
        catch (Exception ex)
        {
            thrown = ex;
        }

        Assert.IsInstanceOfType(thrown, typeof(GoCgoTargetMismatchException), $"Register for plan9/mips must refuse at registration; it threw {thrown?.GetType().Name ?? "nothing"}: {thrown?.Message}");
        Assert.IsInstanceOfType(thrown, typeof(PlatformNotSupportedException), "the refusal is a PlatformNotSupportedException, so existing catches read it");
        StringAssert.Contains(thrown!.Message, "plan9/mips");
        StringAssert.Contains(thrown.Message, GoCgoLibraries.RunningTarget);
    }

    // The controls (P3's read, SHOULD): the two-argument Register (no target: older emissions) and the running target are
    // NOT checked or refused. Both reach SetDllImportResolver, which refuses only because this assembly's own generated
    // bindings registered first (TheGeneratedBindingsRegisterTheirLibrariesWhenTheModuleLoads).
    [TestMethod]
    public void RegisterWithNoTargetOrTheRunningTargetIsNotRefused()
    {
        Assert.ThrowsException<InvalidOperationException>(() => GoCgoLibraries.Register(s_assembly, new GoCgoLibrary("-lm", "libm.so.6")),
            "the two-argument Register is not checked: it reaches the resolver registration");

        if (GoCgoLibraries.Refusal(GoCgoLibraries.RunningTarget) is { } refusal)
            Assert.Inconclusive($"this process refuses its own target's bindings: {refusal}");

        Assert.ThrowsException<InvalidOperationException>(() => GoCgoLibraries.Register(s_assembly, GoCgoLibraries.RunningTarget, new GoCgoLibrary("-lm", "libm.so.6")),
            "the running target is accepted: it reaches the resolver registration");
    }

    // P3's read, item 4: the reverse of Rosetta. An arm64 process on osx-arm64 with darwin/amd64 bindings is refused; an x64
    // process (Rosetta) with them is accepted, since it runs amd64 code with amd64's layouts.
    [TestMethod]
    [DataRow("darwin/amd64", Architecture.Arm64, true)]
    [DataRow("darwin/amd64", Architecture.X64, false)]
    public void DarwinAmd64BindingsRunOnlyInAnAmd64Process(string target, Architecture architecture, bool refused) =>
        Assert.AreEqual(refused, GoCgoLibraries.TargetMismatch(target, "darwin", architecture) is not null, $"{target} with a {architecture} process");

    // A6b (P3's read, MUST 2, as COORD ruled): linux bindings are glibc's (the converter's R3 binds gnu only), so a linux
    // target runs only where EVERY libc reading says glibc: gnu_get_libc_version resolves (positive), the RID is not a musl
    // form, and musl's loader is not mapped (gcompat exports glibc's symbol on musl, and cannot hide ld-musl).
    [TestMethod]
    [DataRow(true, "linux-x64", false, false, "glibc, the portable RID")]
    [DataRow(true, "ubuntu.24.04-x64", false, false, "glibc, a distribution-built runtime's RID")]
    [DataRow(false, "linux-x64", false, true, "no glibc signal, whatever the RID says")]
    [DataRow(true, "linux-musl-x64", false, true, "a musl RID")]
    [DataRow(true, "alpine.3.22-x64", true, true, "musl's loader mapped, with gcompat exporting glibc's symbol")]
    [DataRow(true, "alpine.3.22-x64", false, true, "an Alpine RID with gcompat exporting glibc's symbol and the maps unreadable")]
    [DataRow(false, "alpine.3.22-x64", true, true, "Alpine's packaged runtime")]
    public void LinuxBindingsRunOnlyWhereEveryLibcReadingSaysGlibc(bool glibcExport, string rid, bool ldMusl, bool refused, string why)
    {
        string? message = GoCgoLibraries.LinuxLibcMismatch("linux/amd64", glibcExport, rid, ldMusl);

        Assert.AreEqual(refused, message is not null, $"{why}: {message}");

        if (refused)
            StringAssert.Contains(message, "glibc");
    }

    // P3's read of INC 3, MUST 1: the type-assertion binder counts PlatformNotSupportedException and TypeInitializationException
    // as a binding MISS, so a wrong-target program whose first touch of a cgo package is a type assertion would read "does
    // not implement" and carry on. A6's refusal, anywhere in the chain, is NOT a miss: the filter rethrows it. Every other
    // shape keeps today's reading.
    [TestMethod]
    public void TheBinderRethrowsA6sRefusalAndStillMissesEverythingElse()
    {
        GoCgoTargetMismatchException refusal = new("cgo: refused");

        Assert.IsFalse(go.AdapterBinder.IsBindingFailure(refusal), "A6's refusal itself");
        Assert.IsFalse(go.AdapterBinder.IsBindingFailure(new TypeInitializationException("pkg", refusal)), "A6's refusal from a module initializer");
        Assert.IsFalse(go.AdapterBinder.IsBindingFailure(new TargetInvocationException(new TypeInitializationException("pkg", refusal))), "nested twice");
        Assert.IsFalse(go.AdapterBinder.IsBindingFailure(new AggregateException(new InvalidOperationException(), new TypeInitializationException("pkg", refusal))), "inside an AggregateException");
        Assert.IsTrue(go.AdapterBinder.IsBindingFailure(new PlatformNotSupportedException("aot")), "an ordinary PlatformNotSupportedException is still a miss");
        Assert.IsTrue(go.AdapterBinder.IsBindingFailure(new TypeInitializationException("pkg", new InvalidOperationException())), "an ordinary static-initializer failure is still a miss");
    }

    // On a glibc linux host (every hosted linux runner), the readings of THIS process say glibc: the positive signal is real.
    [TestMethod]
    public void ThisLinuxProcessReadsAsGlibc()
    {
        if (!OperatingSystem.IsLinux())
            Assert.Inconclusive("not linux");

        Assert.IsNull(GoCgoLibraries.Refusal(GoCgoLibraries.RunningTarget), "a glibc linux test host must read its own target as runnable");
    }

    // §11.11's red: a deleted library throws, naming the -l, the captured name and the places tried. Returning 0 would
    // let the runtime probe on, current directory included on windows.
    [TestMethod]
    public void AMissingLibraryThrowsNamingTheFlagTheNameAndThePlaces()
    {
        string name = ProbeFileName("go2cscgomissing");
        DllNotFoundException? thrown = null;

        try
        {
            GoCgoLibraries.Resolve(name, s_assembly, Table(("-lgo2cscgomissing", name)));
        }
        catch (DllNotFoundException ex)
        {
            thrown = ex;
        }

        Assert.IsNotNull(thrown, "a library the table owns and the loader cannot find must throw, not fall through");
        StringAssert.Contains(thrown.Message, "-lgo2cscgomissing");
        StringAssert.Contains(thrown.Message, name);
        StringAssert.Contains(thrown.Message, "the current directory is never searched");
    }

    // §11.10's real current-directory arm: a library that exists ONLY in the current directory is not the one loaded.
    // On darwin a RED here is a security finding, not a test detail (COORD, 2026-11-10): dyld's search for a bare name
    // reached the current directory, and §11.12's absolute-name rule becomes mandatory for every darwin load.
    [TestMethod]
    public void ALibraryOnlyInTheCurrentDirectoryIsNotLoaded()
    {
        string name = ProbeFileName($"go2cscwdprobe{Environment.ProcessId}");
        string directory = Directory.CreateTempSubdirectory("go2cs-cgo-cwd-").FullName;
        string planted = Path.Combine(directory, name);
        string previous = Directory.GetCurrentDirectory();

        File.Copy(SystemLibraryFile(), planted);

        try
        {
            Assert.IsTrue(NativeLibrary.TryLoad(planted, out nint control), "the planted copy loads by its absolute path");
            NativeLibrary.Free(control);

            Directory.SetCurrentDirectory(directory);
            DllNotFoundException? thrown = null;
            nint handle = 0;

            try
            {
                handle = GoCgoLibraries.Resolve(name, s_assembly, Table(("-lgo2cscwdprobe", name)));
            }
            catch (DllNotFoundException ex)
            {
                thrown = ex;
            }

            if (handle != 0)
                NativeLibrary.Free(handle);

            Assert.AreEqual((nint)0, handle, "the library in the current directory was loaded");
            Assert.IsNotNull(thrown, "a library only in the current directory is a miss, and a miss throws");
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);

            try
            {
                Directory.Delete(directory, true);
            }
            catch (IOException)
            {
                // A copy the loader still maps cannot be deleted on windows; the temp directory is the OS's to clear.
            }
        }
    }

    // The windows half of §11.10's arm: the same library in the application directory IS loaded, as a Go binary's DLL
    // beside its executable would be. Linux and darwin search no application directory, as a Go binary's loader does not.
    [TestMethod]
    public void OnWindowsTheSameLibraryInTheApplicationDirectoryLoads()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Inconclusive("only windows searches the application directory, as a Go binary's loader does: NOT MEASURED here");

        string name = ProbeFileName($"go2csappdirprobe{Environment.ProcessId}");
        string planted = Path.Combine(AppContext.BaseDirectory, name);

        File.Copy(SystemLibraryFile(), planted, overwrite: true);

        try
        {
            nint handle = GoCgoLibraries.Resolve(name, s_assembly, Table(("-lgo2csappdirprobe", name)));

            Assert.AreNotEqual((nint)0, handle, "the library beside the application loads");
            NativeLibrary.Free(handle);
        }
        finally
        {
            try
            {
                File.Delete(planted);
            }
            catch (IOException)
            {
                // Still mapped: removed with the build output.
            }
        }
    }
}
