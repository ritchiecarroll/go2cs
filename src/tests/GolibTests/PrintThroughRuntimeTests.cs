using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using Δruntime = go.runtime_package;

namespace GolibTests;

// PRINT FIDELITY (ruling 2026-09-30: R1 with S1, S2 and S3). Go's print and println are the RUNTIME's
// printer: every byte goes through gwrite, which keeps the print backlog (recordForPanic) and honours a
// goroutine's writebuf capture, and writeErr writes Go's own bytes to standard error. A converted print
// bound golib's builtin.print, which wrote text to Console.Error and reached neither.
[TestClass]
public class PrintThroughRuntimeTests
{
    // The sink is registered by the runtime assembly's module initializer, which runs when that assembly
    // is first touched. Run it here, so an arm that touches no runtime type before it prints (the stderr
    // and rune arms) tests the runtime's printer whatever order the arms run in, never golib's fallback.
    [ClassInitialize]
    public static void LoadTheRuntimesPrinter(TestContext _) =>
        System.Runtime.CompilerServices.RuntimeHelpers.RunModuleConstructor(typeof(Δruntime).Module.ModuleHandle);

    [TestMethod]
    public void PrintLandsInTheRuntimesPrintBacklog()
    {
        builtin.print((@string)"go2cs-print-backlog-marker\n");

        string backlog = Encoding.Latin1.GetString(Δruntime.GoPrintBacklog());

        StringAssert.Contains(backlog, "go2cs-print-backlog-marker", "print did not reach the runtime's print backlog (recordForPanic)");
    }

    [TestMethod]
    public void PrintHonoursAGoroutinesWritebufCapture()
    {
        byte[] captured = Δruntime.GoCaptureWritebuf(() => builtin.println((@string)"captured", 7));

        Assert.AreEqual("captured 7\n", Encoding.Latin1.GetString(captured), "println did not reach the goroutine's writebuf capture");
    }

    // Print writes in bounded chunks (a 512-byte stack buffer for formatted text, a 512-byte reused buffer
    // into gwrite), and a host that replaced Console.Error decodes each write as text. One ASCII byte
    // before 2-byte runes puts a 512-byte cut INSIDE a rune, so a chunk that split it would print U+FFFD.
    [TestMethod]
    public void ALongPrintKeepsEveryRuneWhole()
    {
        string runes = "a" + new string('\u00E9', 400);
        TextWriter savedWriter = Console.Error;
        StringWriter capture = new();

        try
        {
            Console.SetError(capture);
            builtin.println((@string)runes, new RuneText(runes));
        }
        finally
        {
            Console.SetError(savedWriter);
        }

        Assert.AreEqual(runes + " " + runes + "\n", capture.ToString(), "a chunk boundary split a rune");
    }

    // A non-string argument, so its text takes the formatted path (UTF-16 encoded through the stack buffer).
    private sealed record RuneText(string Text)
    {
        public override string ToString() => Text;
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int dup(int fd);

    [DllImport("libc", SetLastError = true)]
    private static extern int dup2(int oldfd, int newfd);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    // S2's condition: with Console.Error the process's own stderr writer, a raw write must not overtake
    // text the writer still holds, the bytes are Go's (an invalid UTF-8 byte stays itself), and the
    // terminator is a bare \n. Descriptor 2 is pointed at a file for the arm and restored after.
    [TestMethod]
    public void StandardErrorKeepsGoBytesAndOrder()
    {
        if (!OperatingSystem.IsLinux())
            Assert.Inconclusive("descriptor 2 is redirected with libc dup2; linux only");

        string path = Path.GetTempFileName();
        TextWriter savedWriter = Console.Error;
        int savedFd = dup(2);
        Assert.IsTrue(savedFd >= 0, "dup(2)");

        try
        {
            using (FileStream target = new(path, FileMode.Truncate, FileAccess.Write))
            {
                Assert.AreEqual(2, dup2((int)target.SafeFileHandle.DangerousGetHandle(), 2), "dup2(file, 2)");
            }

            // The console writer .NET would make, over the redirected descriptor, and NOT autoflushing,
            // so text it holds is what a raw write could overtake.
            Console.SetError(new StreamWriter(Console.OpenStandardError(), new UTF8Encoding(false)) { AutoFlush = false });

            Console.Error.Write("A");
            builtin.print(new @string(new byte[] { (byte)'b', 0xFF }));
            Console.Error.Write("C");
            builtin.println();
            Console.Error.Flush();
        }
        finally
        {
            Console.SetError(savedWriter);
            dup2(savedFd, 2);
            close(savedFd);
        }

        byte[] written = File.ReadAllBytes(path);
        File.Delete(path);

        CollectionAssert.AreEqual(new byte[] { (byte)'A', (byte)'b', 0xFF, (byte)'C', (byte)'\n' }, written,
            $"standard error read {BitConverter.ToString(written)}; Go writes 41-62-FF-43-0A");
    }
}
