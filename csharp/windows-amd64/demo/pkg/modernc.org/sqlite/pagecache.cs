// Copyright 2026 The Sqlite Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using pcacheMethods2 = go.modernc.org.sqlite.lib.sqlite3_package.Tsqlite3_pcache_methods2ᴛ1;

namespace go.modernc.org;

using errors = errors_package;
using fmt = fmt_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using Δlibc = go.modernc.org.libc_package;
using types = go.modernc.org.libc.sys.types_package;
using sqlite3 = go.modernc.org.sqlite.lib.sqlite3_package;
using go.modernc.org;
using go.modernc.org.libc.sys;
using go.sync;

partial class sqlite_package {

public static error ErrPageCacheTooLate = errors.New(
    "sqlite: RegisterPageCache called after first Open; "u8 + "SQLITE_CONFIG_PCACHE2 must be installed before any connection is opened"u8);

public static error ErrPageCacheConflict = errors.New(
    "sqlite: a different page cache module is already registered");

partial struct FetchMode /*num:int32*/;

public static FetchMode FetchLookup => 0;
public static FetchMode FetchCreateEasy => 1;
public static FetchMode FetchCreateForce => 2;

partial interface PageCache {
    (Cache, error) Create(nint pageSize, nint extraSize, bool purgeable);
}

partial interface Cache {
    void SetSize(nint n);
    nint PageCount();
    Page Fetch(uint32 key, FetchMode mode);
    void Unpin(Page p, bool discard);
    void Rekey(Page p, uint32 oldKey, uint32 newKey);
    void Truncate(uint32 limit);
    void Destroy();
    void Shrink();
}

partial interface Page {
    @unsafe.Pointer Buf();
    @unsafe.Pointer Extra();
}


partial struct pcacheStateᴛ1 /*dyn*/ {
    internal sync.RWMutex openGate;
    internal atomic.Bool opened;
    internal sync.Once configOnce;
    internal error configErr;
    internal PageCache registered;
    internal uintptr cMethods;
}
internal static readonly ж<pcacheStateᴛ1> ᏑpcacheState = new StaticBox<pcacheStateᴛ1>(default(pcacheStateᴛ1));
internal static ref pcacheStateᴛ1 pcacheState => ref ((StaticBox<pcacheStateᴛ1>)ᏑpcacheState).ValueSlot;

internal static void markConnectionOpened() {
    ᏑpcacheState.of(pcacheStateᴛ1.Ꮡopened).Store(true);
}

internal static error withOpenGate(Func<error> fn) {
    GoFrame ᒐ = default;
    try {
        ᏑpcacheState.of(pcacheStateᴛ1.ᏑopenGate).RLock();
        defer(ᏑpcacheState.of(pcacheStateᴛ1.ᏑopenGate).RUnlock, ref ᒐ);
        markConnectionOpened();
        return fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static error RegisterPageCache(PageCache m) {
    GoFrame ᒐ = default;
    try {
        if (m == nil) {
            return errors.New("sqlite: RegisterPageCache(nil)");
        }

        ᏑpcacheState.of(pcacheStateᴛ1.ᏑopenGate).Lock();
        defer(ᏑpcacheState.of(pcacheStateᴛ1.ᏑopenGate).Unlock, ref ᒐ);

        if (pcacheState.registered != nil) {
            if (AreEqual(pcacheState.registered, m)) {
                return default;
            }
            if (ᏑpcacheState.of(pcacheStateᴛ1.Ꮡopened).Load()) {
                return ErrPageCacheTooLate;
            }
            return ErrPageCacheConflict;
        }
        if (ᏑpcacheState.of(pcacheStateᴛ1.Ꮡopened).Load()) {
            return ErrPageCacheTooLate;
        }

        ᏑpcacheState.of(pcacheStateᴛ1.ᏑconfigOnce).Do(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    var r = recover(); if (r != nil) {
                        pcacheState.configErr = fmt.Errorf(
                            "sqlite: panic during PCACHE2 install: %v"u8, r);
                        pcacheState.registered = default;
                        pcacheState.cMethods = 0;
                    }
                }, ref ᒐ);

                var tls = Δlibc.NewTLS();
                var tlsʗ1 = tls;
                defer(tlsʗ1.Close, ref ᒐ);

                var methodsPtr = Δlibc.Xcalloc(tls, 1, (typesꓸSize_t)/* unsafe.Sizeof(pcacheMethods2{}) */ (uintptr)104);
                if (methodsPtr == 0) {
                    pcacheState.configErr = errors.New("sqlite: out of memory allocating pcache_methods2");
                    return;
                }
                populateCMethods(methodsPtr);

                var varArgs = Δlibc.Xmalloc(tls, (typesꓸSize_t)/* unsafe.Sizeof(uintptr(0)) */ (uintptr)8);
                if (varArgs == 0) {
                    Δlibc.Xfree(tls, methodsPtr);
                    pcacheState.configErr = errors.New("sqlite: out of memory allocating va_list");
                    return;
                }
                defer(Δlibc.Xfree, tls, varArgs, ref ᒐ);

                var rc = sqlite3.Xsqlite3_config(tls,
                    (int32)sqlite3.SQLITE_CONFIG_PCACHE2,
                    Δlibc.VaList(varArgs, methodsPtr));
                if (rc != sqlite3.SQLITE_OK) {
                    Δlibc.Xfree(tls, methodsPtr);
                    pcacheState.configErr = fmt.Errorf(
                        "sqlite: Xsqlite3_config(SQLITE_CONFIG_PCACHE2) returned %d"u8, rc);
                    return;
                }

                pcacheState.cMethods = methodsPtr;
                pcacheState.registered = m;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        return pcacheState.configErr;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static void MustRegisterPageCache(PageCache m) {
    var err = RegisterPageCache(m); if (err != nil) {
        throw panic(err);
    }
}

internal static void populateCMethods(uintptr ptr) {
    var dst = @unsafe.Pointer.To<pcacheMethods2>((@unsafe.Pointer)ptr);
    dst.Value.FiVersion = 1;
    dst.Value.FxInit = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(pcacheTrampolineInit);
    dst.Value.FxShutdown = cFuncPointer<Action<ж<Δlibc.TLS>, uintptr>>(pcacheTrampolineShutdown);
    dst.Value.FxCreate = cFuncPointer<Func<ж<Δlibc.TLS>, int32, int32, int32, uintptr>>(pcacheTrampolineCreate);
    dst.Value.FxCachesize = cFuncPointer<Action<ж<Δlibc.TLS>, uintptr, int32>>(pcacheTrampolineCachesize);
    dst.Value.FxPagecount = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, int32>>(pcacheTrampolinePagecount);
    dst.Value.FxFetch = cFuncPointer<Func<ж<Δlibc.TLS>, uintptr, uint32, int32, uintptr>>(pcacheTrampolineFetch);
    dst.Value.FxUnpin = cFuncPointer<Action<ж<Δlibc.TLS>, uintptr, uintptr, int32>>(pcacheTrampolineUnpin);
    dst.Value.FxRekey = cFuncPointer<Action<ж<Δlibc.TLS>, uintptr, uintptr, uint32, uint32>>(pcacheTrampolineRekey);
    dst.Value.FxTruncate = cFuncPointer<Action<ж<Δlibc.TLS>, uintptr, uint32>>(pcacheTrampolineTruncate);
    dst.Value.FxDestroy = cFuncPointer<Action<ж<Δlibc.TLS>, uintptr>>(pcacheTrampolineDestroy);
    dst.Value.FxShrink = cFuncPointer<Action<ж<Δlibc.TLS>, uintptr>>(pcacheTrampolineShrink);
}

} // end sqlite_package
