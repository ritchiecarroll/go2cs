namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using strings = strings_package;
using syscall = syscall_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string readErrorˢ = "read error"u8;
private static readonly @string gidˢ = "Gid:"u8;
private static readonly @string noGidLineˢ = "no Gid line"u8;

internal static @string gidLine(nint tid) {
    var (b, err) = os.ReadFile(fmt.Sprintf("/proc/self/task/%d/status"u8, tid));
    if (err != default!) {
        return readErrorˢ;
    }
    foreach (var (_, line) in strings.Split(((@string)b), "\n"u8)) {
        if (strings.HasPrefix(line, gidˢ)) {
            return strings.Join(strings.Fields(line[4..]), " "u8);
        }
    }
    return noGidLineˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object notLinuxNothingToObserveˢ = (@string)"not linux; nothing to observe"u8;
private static readonly object skipNeedsRootThisSeamˢ = (@string)"SKIP: needs root -- this seam changes the process's effective gid and reads /proc"u8;
private static readonly object beforeMainˢ = (@string)"before main  :"u8;
private static readonly object beforeParkedˢ = (@string)"before parked:"u8;
private static readonly object setegid1Failedˢ = (@string)"setegid(1) failed:"u8;
private static readonly object afterMainˢ = (@string)"after  main  :"u8;
private static readonly object afterParkedˢ = (@string)"after  parked:"u8;
private static readonly object theParkedThreadFollowedˢ = (@string)"the parked thread followed the change:"u8;
private static readonly object restoreFailedˢ = (@string)"restore failed:"u8;
private static readonly object restoredMainˢ = (@string)"restored main:"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void Main() {
    if (runtime.GOOS != "linux"u8) {
        fmt.Println(notLinuxNothingToObserveˢ);
        return;
    }
    if (os.Geteuid() != 0) {
        fmt.Println(skipNeedsRootThisSeamˢ);
        return;
    }
    var tidCh = new channel<nint>(0);
    var release = new channel<EmptyStruct>(0);
    var done = new channel<@string>(0);
    var doneʗ1 = done;
    var releaseʗ1 = release;
    var tidChʗ1 = tidCh;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            runtime.LockOSThread();
            defer(runtime.UnlockOSThread, ref ᒐ);
            nint tid = syscall.Gettid();
            tidChʗ1.ᐸꟷ(tid);
            ᐸꟷ(releaseʗ1);
            doneʗ1.ᐸꟷ(gidLine(tid));
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    nint parked = ᐸꟷ(tidCh);
    nint self = syscall.Gettid();
    fmt.Println(beforeMainˢ, gidLine(self));
    fmt.Println(beforeParkedˢ, gidLine(parked));
    {
        var err = syscall.Setegid(1); if (err != default!) {
            fmt.Println(setegid1Failedˢ, err);
            return;
        }
    }
    @string mainAfter = gidLine(self);
    fmt.Println(afterMainˢ, mainAfter);
    close(release);
    @string parkedAfter = ᐸꟷ(done);
    fmt.Println(afterParkedˢ, parkedAfter);
    fmt.Println(theParkedThreadFollowedˢ, parkedAfter == mainAfter);
    {
        var err = syscall.Setegid(0); if (err != default!) {
            fmt.Println(restoreFailedˢ, err);
            return;
        }
    }
    fmt.Println(restoredMainˢ, gidLine(self));
}

} // end main_package
