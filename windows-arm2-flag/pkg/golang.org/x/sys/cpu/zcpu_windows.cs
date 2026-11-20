namespace go.golang.org.x.sys;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class cpu_package {

internal static @unsafe.Pointer _ᴛ1ʗ;

internal static UntypedInt errnoERROR_IO_PENDING => 997;

internal static error errERROR_IO_PENDING = ((syscall.Errno)errnoERROR_IO_PENDING);
internal static error errERROR_EINVAL = ((syscall.Errno)syscall.EINVAL);

internal static error errnoErr(syscall.Errno e) {
    switch (e) {
    case var ᴛ when ᴛ == (syscall.Errno)(0): {
        return errERROR_EINVAL;
    }
    case var ᴛ when ᴛ == errnoERROR_IO_PENDING: {
        return errERROR_IO_PENDING;
    }}

    return e;
}

internal static ж<syscall.LazyDLL> modkernel32 = syscall.NewLazyDLL("kernel32.dll");
internal static ж<syscall.LazyProc> procIsProcessorFeaturePresent = modkernel32.NewProc("IsProcessorFeaturePresent");

internal static bool /*ret*/ isProcessorFeaturePresent(uint32 ProcessorFeature) {
    bool ret = default;

    var (r0, _, _) = syscall.SyscallN(procIsProcessorFeaturePresent.Addr(), (uintptr)ProcessorFeature);
    ret = r0 != 0;
    return ret;
}

} // end cpu_package
