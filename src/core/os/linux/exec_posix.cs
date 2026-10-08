// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1 || windows
namespace go;

using itoa = @internal.itoa_package;
using execenv = @internal.syscall.execenv_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @internal;
using @internal.syscall;
using fs = go.io.fs_package;

partial class os_package {

// The only signal values guaranteed to be present in the os package on all
// systems are os.Interrupt (send the process an interrupt) and os.Kill (force
// the process to exit). On Windows, sending os.Interrupt to a process with
// os.Process.Signal is not implemented; it will return an error instead of
// sending a signal.
public static ΔSignal Interrupt = new syscall_ΔSignalᴠΔSignal(((syscallꓸSignal)syscall.SIGINT));

public static ΔSignal ΔKill = new syscall_ΔSignalᴠΔSignal(((syscallꓸSignal)syscall.SIGKILL));

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chdirˢ = "chdir"u8;

internal static (ж<Process> p, error err) startProcess(@string name, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    error err = default!;

    ref var attr = ref Ꮡattr.DerefOrNull();
    // If there is no SysProcAttr (ie. no Chroot or changed
    // UID/GID), double-check existence of the directory we want
    // to chdir into. We can make the error clearer this way.
    if (Ꮡattr != nil && attr.Sys == nil && attr.Dir != ""u8) {
        {
            var (_, errΔ1) = Stat(attr.Dir); if (errΔ1 != default!) {
                var pe = errΔ1._<ж<PathError>>();
                pe.Value.Op = chdirˢ;
                return (default!, new fs.PathErrorжerror(pe));
            }
        }
    }
    var (attrSys, shouldDupPidfd) = ensurePidfd(attr.Sys);
    var sysattr = Ꮡ(new syscall.ProcAttr(
        Dir: attr.Dir,
        Env: attr.Env,
        Sys: attrSys
    ));
    if ((~sysattr).Env == default!) {
        (sysattr.Value.Env, err) = execenv.Default((~sysattr).Sys);
        if (err != default!) {
            return (default!, err);
        }
    }
    sysattr.Value.Files = new slice<uintptr>(0, len(attr.Files));
    foreach (var (_, f) in attr.Files) {
        sysattr.Value.Files = append((~sysattr).Files, f.Fd());
    }
    var (pid, h, e) = syscall.StartProcess(name, argv, sysattr);
    // Make sure we don't run the finalizers of attr.Files.
    runtime.KeepAlive(Ꮡattr.OrTypedNil());
    if (e != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "fork/exec"u8, Path: name, Err: e))));
    }
    // For Windows, syscall.StartProcess above already returned a process handle.
    if (runtime.GOOS != "windows"u8) {
        bool ok = default!;
        (h, ok) = getPidfd(ref ((~sysattr).Sys).DerefOrNull(), shouldDupPidfd);
        if (!ok) {
            return (newPIDProcess(pid), default!);
        }
    }
    return (newHandleProcess(pid, h), default!);
}

internal static error kill(this ж<Process> Ꮡp) {
    return Ꮡp.Signal(ΔKill);
}

// ProcessState stores information about a process, as reported by Wait.
partial struct ProcessState {
    internal nint pid;               // The process's id.
    internal syscall.WaitStatus status; // System-dependent status info.
    internal ж<syscall.Rusage> rusage;
}

// Pid returns the process id of the exited process.
public static nint Pid(this ref ProcessState p) {
    return p.pid;
}

internal static bool exited(this ref ProcessState p) {
    return p.status.Exited();
}

internal static bool success(this ref ProcessState p) {
    return p.status.ExitStatus() == 0;
}

internal static any sys(this ref ProcessState p) {
    return p.status;
}

internal static any sysUsage(this ref ProcessState p) {
    return p.rusage.OrTypedNil();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "<nil>"u8;
internal static readonly @string continuedˢ = "continued"u8;

public static @string String(this ж<ProcessState> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return nilˢ;
    }
    var status = p.Sys()._<syscall.WaitStatus>();
    @string res = ""u8;
    switch (ᐧ) {
    case {} when status.Exited(): {
        nint code = status.ExitStatus();
        if (runtime.GOOS == "windows"u8 && (nuint)code >= ((nuint)1 << (int)(16))){
            // windows uses large hex numbers
            res = "exit status "u8 + itoa.Uitox((nuint)code);
        } else {
            // unix systems use small decimal integers
            res = "exit status "u8 + itoa.Itoa(code); // unix
        }
        break;
    }
    case {} when status.Signaled(): {
        res = "signal: "u8 + status.Signal().String();
        break;
    }
    case {} when status.Stopped(): {
        res = "stop signal: "u8 + status.StopSignal().String();
        if (status.StopSignal() == syscall.SIGTRAP && status.TrapCause() != 0) {
            res += " (trap "u8 + itoa.Itoa(status.TrapCause()) + ")"u8;
        }
        break;
    }
    case {} when status.Continued(): {
        res = continuedˢ;
        break;
    }}

    if (status.CoreDump()) {
        res += " (core dumped)"u8;
    }
    return res;
}

// ExitCode returns the exit code of the exited process, or -1
// if the process hasn't exited or was terminated by a signal.
public static nint ExitCode(this ж<ProcessState> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    // return -1 if the process hasn't started.
    if (Ꮡp == nil) {
        return -1;
    }
    return p.status.ExitStatus();
}

} // end os_package
