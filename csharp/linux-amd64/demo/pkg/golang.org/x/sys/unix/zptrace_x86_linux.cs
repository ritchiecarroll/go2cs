namespace go.golang.org.x.sys;

using @unsafe = unsafe_package;

partial class unix_package {

partial struct PtraceRegs386 {
    public int32 Ebx;
    public int32 Ecx;
    public int32 Edx;
    public int32 Esi;
    public int32 Edi;
    public int32 Ebp;
    public int32 Eax;
    public int32 Xds;
    public int32 Xes;
    public int32 Xfs;
    public int32 Xgs;
    public int32 Orig_eax;
    public int32 Eip;
    public int32 Xcs;
    public int32 Eflags;
    public int32 Esp;
    public int32 Xss;
}

public static error PtraceGetRegs386(nint pid, ж<PtraceRegs386> regsout) {
    return ptracePtr(PTRACE_GETREGS, pid, 0, @unsafe.Pointer.FromPinnedBox(regsout));
}

public static error PtraceSetRegs386(nint pid, ж<PtraceRegs386> regs) {
    return ptracePtr(PTRACE_SETREGS, pid, 0, @unsafe.Pointer.FromPinnedBox(regs));
}

partial struct PtraceRegsAmd64 {
    public uint64 R15;
    public uint64 R14;
    public uint64 R13;
    public uint64 R12;
    public uint64 Rbp;
    public uint64 Rbx;
    public uint64 R11;
    public uint64 R10;
    public uint64 R9;
    public uint64 R8;
    public uint64 Rax;
    public uint64 Rcx;
    public uint64 Rdx;
    public uint64 Rsi;
    public uint64 Rdi;
    public uint64 Orig_rax;
    public uint64 Rip;
    public uint64 Cs;
    public uint64 Eflags;
    public uint64 Rsp;
    public uint64 Ss;
    public uint64 Fs_base;
    public uint64 Gs_base;
    public uint64 Ds;
    public uint64 Es;
    public uint64 Fs;
    public uint64 Gs;
}

public static error PtraceGetRegsAmd64(nint pid, ж<PtraceRegsAmd64> regsout) {
    return ptracePtr(PTRACE_GETREGS, pid, 0, @unsafe.Pointer.FromPinnedBox(regsout));
}

public static error PtraceSetRegsAmd64(nint pid, ж<PtraceRegsAmd64> regs) {
    return ptracePtr(PTRACE_SETREGS, pid, 0, @unsafe.Pointer.FromPinnedBox(regs));
}

} // end unix_package
