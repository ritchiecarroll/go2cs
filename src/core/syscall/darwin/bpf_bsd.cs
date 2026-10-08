// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || dragonfly || freebsd || netbsd || openbsd
// Berkeley packet filter for BSD variants
namespace go;

using @unsafe = unsafe_package;

partial class syscall_package {

// Deprecated: Use golang.org/x/net/bpf instead.
public static ж<BpfInsn> BpfStmt(nint code, nint k) {
    return Ꮡ(new BpfInsn(Code: (uint16)code, K: (uint32)k));
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static ж<BpfInsn> BpfJump(nint code, nint k, nint jt, nint jf) {
    return Ꮡ(new BpfInsn(Code: (uint16)code, Jt: (uint8)jt, Jf: (uint8)jf, K: (uint32)k));
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) BpfBuflen(nint fd) {
    ref var l = ref heap(new nint(), out var Ꮡl);
    var err = ioctlPtr(fd, BIOCGBLEN, @unsafe.Pointer.FromPinnedBox(Ꮡl));
    if (err != default!) {
        return (0, err);
    }
    return (l, default!);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) SetBpfBuflen(nint fd, nint lʗp) {
    ref var l = ref heap(lʗp, out var Ꮡl);

    var err = ioctlPtr(fd, BIOCSBLEN, @unsafe.Pointer.FromPinnedBox(Ꮡl));
    if (err != default!) {
        return (0, err);
    }
    return (l, default!);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) BpfDatalink(nint fd) {
    ref var t = ref heap(new nint(), out var Ꮡt);
    var err = ioctlPtr(fd, BIOCGDLT, @unsafe.Pointer.FromPinnedBox(Ꮡt));
    if (err != default!) {
        return (0, err);
    }
    return (t, default!);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) SetBpfDatalink(nint fd, nint tʗp) {
    ref var t = ref heap(tʗp, out var Ꮡt);

    var err = ioctlPtr(fd, BIOCSDLT, @unsafe.Pointer.FromPinnedBox(Ꮡt));
    if (err != default!) {
        return (0, err);
    }
    return (t, default!);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfPromisc(nint fd, nint mʗp) {
    ref var m = ref heap(mʗp, out var Ꮡm);

    var err = ioctlPtr(fd, BIOCPROMISC, @unsafe.Pointer.FromPinnedBox(Ꮡm));
    if (err != default!) {
        return err;
    }
    return default!;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error FlushBpf(nint fd) {
    var err = ioctlPtr(fd, BIOCFLUSH, nil);
    if (err != default!) {
        return err;
    }
    return default!;
}

partial struct ivalue {
    internal array<byte> name = new(IFNAMSIZ);
    internal int16 value;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (@string, error) BpfInterface(nint fd, @string name) {
    ref var iv = ref heap(new ivalue(), out var Ꮡiv);
    var err = ioctlPtr(fd, BIOCGETIF, @unsafe.Pointer.FromPinnedBox(Ꮡiv));
    if (err != default!) {
        return ("", err);
    }
    return (name, default!);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfInterface(nint fd, @string name) {
    ref var iv = ref heap(new ivalue(), out var Ꮡiv);
    copy(iv.name[..], slice<byte>(name));
    var err = ioctlPtr(fd, BIOCSETIF, @unsafe.Pointer.FromPinnedBox(Ꮡiv));
    if (err != default!) {
        return err;
    }
    return default!;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (ж<Timeval>, error) BpfTimeout(nint fd) {
    ref var tv = ref heap(new Timeval(), out var Ꮡtv);
    var err = ioctlPtr(fd, BIOCGRTIMEOUT, @unsafe.Pointer.FromPinnedBox(Ꮡtv));
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡtv, default!);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfTimeout(nint fd, ж<Timeval> Ꮡtv) {
    var err = ioctlPtr(fd, BIOCSRTIMEOUT, @unsafe.Pointer.FromPinnedBox(Ꮡtv));
    if (err != default!) {
        return err;
    }
    return default!;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (ж<BpfStat>, error) BpfStats(nint fd) {
    ref var s = ref heap(new BpfStat(), out var Ꮡs);
    var err = ioctlPtr(fd, BIOCGSTATS, @unsafe.Pointer.FromPinnedBox(Ꮡs));
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡs, default!);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfImmediate(nint fd, nint mʗp) {
    ref var m = ref heap(mʗp, out var Ꮡm);

    var err = ioctlPtr(fd, BIOCIMMEDIATE, @unsafe.Pointer.FromPinnedBox(Ꮡm));
    if (err != default!) {
        return err;
    }
    return default!;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpf(nint fd, slice<BpfInsn> i) {
    ref var p = ref heap(new BpfProgram(), out var Ꮡp);
    p.Len = (uint32)len(i);
    p.Insns = Ꮡ(i, 0);
    var err = ioctlPtr(fd, BIOCSETF, @unsafe.Pointer.FromPinnedBox(Ꮡp));
    if (err != default!) {
        return err;
    }
    return default!;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error CheckBpfVersion(nint fd) {
    ref var v = ref heap(new BpfVersion(), out var Ꮡv);
    var err = ioctlPtr(fd, BIOCVERSION, @unsafe.Pointer.FromPinnedBox(Ꮡv));
    if (err != default!) {
        return err;
    }
    if (v.Major != BPF_MAJOR_VERSION || v.Minor != BPF_MINOR_VERSION) {
        return EINVAL;
    }
    return default!;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) BpfHeadercmpl(nint fd) {
    ref var f = ref heap(new nint(), out var Ꮡf);
    var err = ioctlPtr(fd, BIOCGHDRCMPLT, @unsafe.Pointer.FromPinnedBox(Ꮡf));
    if (err != default!) {
        return (0, err);
    }
    return (f, default!);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetBpfHeadercmpl(nint fd, nint fʗp) {
    ref var f = ref heap(fʗp, out var Ꮡf);

    var err = ioctlPtr(fd, BIOCSHDRCMPLT, @unsafe.Pointer.FromPinnedBox(Ꮡf));
    if (err != default!) {
        return err;
    }
    return default!;
}

} // end syscall_package
