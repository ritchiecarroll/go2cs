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
// for it by its bare name, the name dyld's leaf-name search would meet.
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
