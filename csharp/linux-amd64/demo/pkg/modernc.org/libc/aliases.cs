// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using DIR = go.modernc.org.libc_package.TDIRᴛ1;
global using FILE = go.modernc.org.libc_package.TFILEᴛ1;
global using HEADER = go.modernc.org.libc_package.THEADERᴛ1;

namespace go.modernc.org;

partial class libc_package {

public static void X__vm_wait(ж<TLS> tls) { }

public static readonly ж<int32> ᏑX__atexit_lockptr = new StaticBox<int32>(default(int32));
public static ref int32 X__atexit_lockptr => ref ((StaticBox<int32>)ᏑX__atexit_lockptr).ValueSlot;
public static readonly ж<int32> ᏑX__bump_lockptr = new StaticBox<int32>(default(int32));
public static ref int32 X__bump_lockptr => ref ((StaticBox<int32>)ᏑX__bump_lockptr).ValueSlot;

public static int32 X__aio_close(ж<TLS> tls, int32 fd) {
    return fd;
}

public static partial void Xtzset(ж<TLS> tls) {
    ___tzset(tls);
}

public static UntypedInt DT_DETACHED => /* _DT_DETACHED */ 3;

public static UntypedInt DT_EXITING => /* _DT_EXITING */ 1;

public static UntypedInt DT_JOINABLE => /* _DT_JOINABLE */ 2;

public static partial int32 /*r*/ Xfcntl64(ж<TLS> tls, int32 fd, int32 cmd, uintptr va) {
    return Xfcntl(tls, fd, cmd, va);
}

public static partial uintptr /*r*/ Xfopen64(ж<TLS> tls, uintptr filename, uintptr mode) {
    return Xfopen(tls, filename, mode);
}

public static partial int32 /*r*/ Xfstat64(ж<TLS> tls, int32 fd, uintptr st) {
    return Xfstat(tls, fd, st);
}

public static partial int32 /*r*/ Xftruncate64(ж<TLS> tls, int32 fd, Toff_t length) {
    return Xftruncate(tls, fd, length);
}

public static partial int32 /*r*/ Xgetrlimit64(ж<TLS> tls, int32 resource, uintptr rlim) {
    return Xgetrlimit(tls, resource, rlim);
}

public static partial Toff_t /*r*/ Xlseek64(ж<TLS> tls, int32 fd, Toff_t offset, int32 whence) {
    return Xlseek(tls, fd, offset, whence);
}

public static partial int32 /*r*/ Xlstat64(ж<TLS> tls, uintptr path, uintptr buf) {
    return Xlstat(tls, path, buf);
}

public static partial int32 /*r*/ Xmkstemp64(ж<TLS> tls, uintptr template) {
    return Xmkstemp(tls, template);
}

public static partial int32 /*r*/ Xmkstemps64(ж<TLS> tls, uintptr template, int32 len1) {
    return Xmkstemps(tls, template, len1);
}

public static partial uintptr /*r*/ Xmmap64(ж<TLS> tls, uintptr start, Tsize_t len1, int32 prot, int32 flags, int32 fd, Toff_t off) {
    return Xmmap(tls, start, len1, prot, flags, fd, off);
}

public static partial int32 /*r*/ Xopen64(ж<TLS> tls, uintptr filename, int32 flags, uintptr va) {
    return Xopen(tls, filename, flags, va);
}

public static partial uintptr /*r*/ Xreaddir64(ж<TLS> tls, uintptr dir) {
    return Xreaddir(tls, dir);
}

public static partial int32 /*r*/ Xsetrlimit64(ж<TLS> tls, int32 resource, uintptr rlim) {
    return Xsetrlimit(tls, resource, rlim);
}

public static partial int32 /*r*/ Xstat64(ж<TLS> tls, uintptr path, uintptr buf) {
    return Xstat(tls, path, buf);
}

public static int32 Xpthread_setcancelstate(ж<TLS> tls, int32 @new, uintptr old) {
    return _pthread_setcancelstate(ref (tls).DerefOrNull(), @new, old);
}

public static int32 Xpthread_sigmask(ж<TLS> tls, int32 now, uintptr set, uintptr old) {
    return _pthread_sigmask(ref (tls).DerefOrNull(), now, set, old);
}

} // end libc_package
