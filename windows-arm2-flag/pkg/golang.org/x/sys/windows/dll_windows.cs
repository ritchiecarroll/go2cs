// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using sync = sync_package;
using atomic = go.sync.atomic_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.sync;
using ꓸꓸꓸuintptr = Span<uintptr>;
using static windows_package;

partial class windows_package {

/*linkname*/ internal static partial (Handle handle, Errno err) syscall_loadlibrary(ж<uint16> filename) {
    var (ᴛ1, ᴛ2) = syscall.loadlibrary(filename);
    return ((Handle)(uintptr)ᴛ1, (Errno)(uintptr)ᴛ2);
}

/*linkname*/ internal static partial (uintptr proc, Errno err) syscall_getprocaddress(Handle handle, ж<uint8> procname) {
    var (ᴛ1, ᴛ2) = syscall.getprocaddress((uintptr)handle, procname);
    return ((uintptr)(uintptr)ᴛ1, (Errno)(uintptr)ᴛ2);
}

partial struct DLLError {
    public error Err;
    public @string ObjName;
    public @string Msg;
}

} partial class windowsᴛDLLError {

public static @string Error(this ref DLLError e) { return e.Msg; }

public static error Unwrap(this ref DLLError e) { return e.Err; }

} partial class windows_package {

partial struct DLL {
    public @string Name;
    public Handle Handle;
}

public static (ж<DLL> dll, error err) LoadDLL(@string name) {
    error err = default;

    (var namep, err) = UTF16PtrFromString(name);
    if (err != nil) {
        return (default, err);
    }
    ref var h = ref heap<Handle>(out var Ꮡh);
    ref var e = ref heap<Errno>(out var Ꮡe);
    (h, e) = syscall_loadlibrary(namep);
    if (e != 0) {
        return (default, new DLLErrorжerror(Ꮡ(new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to load "u8 + name + ": "u8 + e.Error()
        ))));
    }
    var d = Ꮡ(new DLL(
        Name: name,
        Handle: h
    ));
    return (d, default);
}

public static ж<DLL> MustLoadDLL(@string name) {
    var (d, e) = LoadDLL(name);
    if (e != nil) {
        throw panic(e);
    }
    return d;
}

} partial class windowsᴛDLL {

public static (ж<Proc> proc, error err) FindProc(this ж<DLL> Ꮡd, @string name) {
    error err = default;
    ref var d = ref Ꮡd.DerefOrNull();

    (var namep, err) = BytePtrFromString(name);
    if (err != nil) {
        return (default, err);
    }
    ref var a = ref heap<uintptr>(out var Ꮡa);
    ref var e = ref heap<Errno>(out var Ꮡe);
    (a, e) = syscall_getprocaddress(d.Handle, namep);
    if (e != 0) {
        return (default, new DLLErrorжerror(Ꮡ(new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to find "u8 + name + " procedure in "u8 + d.Name + ": "u8 + e.Error()
        ))));
    }
    var p = Ꮡ(new Proc(
        Dll: Ꮡd,
        Name: name,
        addr: a
    ));
    return (p, default);
}

public static ж<Proc> MustFindProc(this ж<DLL> Ꮡd, @string name) {
    var (p, e) = Ꮡd.FindProc(name);
    if (e != nil) {
        throw panic(e);
    }
    return p;
}

public static (ж<Proc> proc, error err) FindProcByOrdinal(this ж<DLL> Ꮡd, uintptr ordinal) {
    ref var d = ref Ꮡd.DerefOrNull();

    ref var a = ref heap<uintptr>(out var Ꮡa);
    (a, var e) = GetProcAddressByOrdinal(d.Handle, ordinal);
    ref var name = ref heap<@string>("#"u8 + itoa((nint)ordinal), out var Ꮡname);
    if (e != nil) {
        return (default, new DLLErrorжerror(Ꮡ(new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to find "u8 + name + " procedure in "u8 + d.Name + ": "u8 + e.Error()
        ))));
    }
    var p = Ꮡ(new Proc(
        Dll: Ꮡd,
        Name: name,
        addr: a
    ));
    return (p, default);
}

public static ж<Proc> MustFindProcByOrdinal(this ж<DLL> Ꮡd, uintptr ordinal) {
    var (p, e) = Ꮡd.FindProcByOrdinal(ordinal);
    if (e != nil) {
        throw panic(e);
    }
    return p;
}

public static error /*err*/ Release(this ref DLL d) {
    return FreeLibrary(d.Handle);
}

} partial class windows_package {

partial struct Proc {
    public ж<DLL> Dll;
    public @string Name;
    internal uintptr addr;
}

} partial class windowsᴛProc {

public static uintptr Addr(this ref Proc p) {
    return p.addr;
}

public static (uintptr r1, uintptr r2, error lastErr) Call(this ref Proc p, params ꓸꓸꓸuintptr aʗp) {
    var a = aʗp.sslice();

    var (ᴛ1, ᴛ2, ᴛ3) = syscall.SyscallN(p.Addr(), a);
    return (ᴛ1, ᴛ2, ᴛ3);
}

} partial class windows_package {

partial struct LazyDLL {
    public @string Name;
    public bool System;
    internal sync.Mutex mu;
    internal ж<DLL> dll;
}

} partial class windowsᴛLazyDLL {

public static error Load(this ж<LazyDLL> Ꮡd) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        if (atomic.LoadPointer(Ꮡd.Ꮡdll) != nil) {
            return default;
        }
        d.mu.Lock();
        ᒐd1 = true;
        if (d.dll != nil) {
            return default;
        }

        ж<DLL> dll = default;
        error err = default;
        if (d.Name == "kernel32.dll"u8) {
            (dll, err) = LoadDLL(d.Name);
        } else {
            (dll, err) = loadLibraryEx(d.Name, d.System);
        }
        if (err != nil) {
            return err;
        }

        atomic.StorePointer(Ꮡd.Ꮡdll, dll);
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡd.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void mustLoad(this ж<LazyDLL> Ꮡd) {
    var e = Ꮡd.Load();
    if (e != nil) {
        throw panic(e);
    }
}

public static uintptr Handle(this ж<LazyDLL> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    Ꮡd.mustLoad();
    return (uintptr)d.dll.Handle;
}

public static ж<LazyProc> NewProc(this ж<LazyDLL> Ꮡd, @string name) {
    return Ꮡ(new LazyProc(l: Ꮡd, Name: name));
}

} partial class windows_package {

public static ж<LazyDLL> NewLazyDLL(@string name) {
    return Ꮡ(new LazyDLL(Name: name));
}

public static ж<LazyDLL> NewLazySystemDLL(@string name) {
    return Ꮡ(new LazyDLL(Name: name, System: true));
}

partial struct LazyProc {
    public @string Name;
    internal sync.Mutex mu;
    internal ж<LazyDLL> l;
    internal ж<Proc> proc;
}

} partial class windowsᴛLazyProc {

public static error Find(this ж<LazyProc> Ꮡp) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        if (atomic.LoadPointer(Ꮡp.Ꮡproc) == nil) {
            p.mu.Lock();
            ᒐd1 = true;
            if (p.proc == nil) {
                var e = p.l.Load();
                if (e != nil) {
                    return e;
                }
                (var proc, e) = p.l.dll.FindProc(p.Name);
                if (e != nil) {
                    return e;
                }
                atomic.StorePointer(Ꮡp.Ꮡproc, proc);
            }
        }
        return default;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡp.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static void mustFind(this ж<LazyProc> Ꮡp) {
    var e = Ꮡp.Find();
    if (e != nil) {
        throw panic(e);
    }
}

public static uintptr Addr(this ж<LazyProc> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    Ꮡp.mustFind();
    return p.proc.Addr();
}

public static (uintptr r1, uintptr r2, error lastErr) Call(this ж<LazyProc> Ꮡp, params ꓸꓸꓸuintptr aʗp) {
    var a = aʗp.sslice();
    ref var p = ref Ꮡp.DerefOrNull();

    Ꮡp.mustFind();
    return p.proc.Call(a.ꓸꓸꓸ);
}

} partial class windows_package {


partial struct canDoSearchSystem32Onceᴛ1 /*dyn*/ {
    /*embed*/ public sync_package.Once Once;
    internal bool v;
}
internal static readonly ж<canDoSearchSystem32Onceᴛ1> ᏑcanDoSearchSystem32Once = new StaticBox<canDoSearchSystem32Onceᴛ1>(new canDoSearchSystem32Onceᴛ1(nil));
internal static ref canDoSearchSystem32Onceᴛ1 canDoSearchSystem32Once => ref ((StaticBox<canDoSearchSystem32Onceᴛ1>)ᏑcanDoSearchSystem32Once).ValueSlot;

internal static void initCanDoSearchSystem32() {
    canDoSearchSystem32Once.v = (modkernel32.NewProc("AddDllDirectory").Find() == nil);
}

internal static bool canDoSearchSystem32() {
    ᏑcanDoSearchSystem32Once.of(canDoSearchSystem32Onceᴛ1.ᏑOnce).Do(initCanDoSearchSystem32);
    return canDoSearchSystem32Once.v;
}

internal static bool isBaseName(@string name) {
    foreach (var (_, c) in name) {
        if (c == ':' || c == '/' || c == '\\') {
            return false;
        }
    }
    return true;
}

internal static (ж<DLL>, error) loadLibraryEx(@string name, bool system) {
    @string loadDLL = name;
    uintptr flags = default;
    if (system) {
        if (canDoSearchSystem32()) {
            flags = LOAD_LIBRARY_SEARCH_SYSTEM32;
        } else if (isBaseName(name)) {
            var (systemdir, errΔ1) = GetSystemDirectory();
            if (errΔ1 != nil) {
                return (default, errΔ1);
            }
            loadDLL = systemdir + "\\"u8 + name;
        }
    }
    ref var h = ref heap<Handle>(out var Ꮡh);
    (h, var err) = LoadLibraryEx(loadDLL, 0, flags);
    if (err != nil) {
        return (default, err);
    }
    return (Ꮡ(new DLL(Name: name, Handle: h)), default);
}

} // end windows_package
