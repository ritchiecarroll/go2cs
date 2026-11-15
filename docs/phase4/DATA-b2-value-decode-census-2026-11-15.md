# DATA: B2 value-decode census, 2026-11-15 (both runs)
<!-- {% raw %} — Jekyll/Liquid guard: the census lines quote nested Go struct types (}}) that Liquid would otherwise parse. -->

A record, not a procedure. The census gates the B2 cut (COORD ruling 2026-11-15 16:53Z, Q2); COORD cleared the cut
on the second run (17:06Z) and asked for both runs' logs to be kept in the seat. Posted to COORD at 17:05Z.

Instrument: `TestInlineArrayValueDecodeCensus` (src/go2cs/inlineArrayValueDecodeCensus_test.go), go1.26.11.
- Kept-out set: `computeInlineArrayAdmission(...).blocked`, unioned across every loaded package.
- Site: an rvalue `*p` over `*T`, T kept out. Not an assignment target, not the operand of `&` or `++`/`--`, not a
  field-selection base.
- Scopes: std (non-test packages) on four targets; `modernc.org/libc` v1.80.1 `./...` on linux amd64 and arm64.

Run 1 counted every `unsafe.Sizeof(*p)` as a read. Run 2 excludes any `*p` under an expression go/types records a
constant value for, since the operand of a constant expression is never evaluated. Control module (2 positives, 3
negatives): 2/2 at run 2; with the exclusion neutered it reads 3, the extra being exactly the `unsafe.Sizeof(*p)` arm.

| scope | target | packages | kept-out | run 1 sites | run 2 sites |
|---|---|---|---|---|---|
| std | linux/amd64 | 362 | 44 | 4 | 0 |
| std | windows/amd64 | 364 | 48 | 6 | 0 |
| std | darwin/amd64 | 362 | 47 | 4 | 0 |
| std | linux/arm64 | 360 | 42 | 4 | 0 |
| libc | linux/amd64 | 30 | 4 | 1 | 1 |
| libc | linux/arm64 | 30 | 3 | 1 | 1 |

The one libc site on each target is the W14 seed, `_BF_init_state = *(*TBF_ctx)(unsafe.Pointer(&[1042]uint32{...}))`.

## Run 1 (before the constant-expression exclusion)

### std-linux-amd64

```text
B2CENSUS KEPT-OUT debug/pe.COFFSymbol (in debug/pe)
B2CENSUS KEPT-OUT debug/pe.COFFSymbolAuxFormat5 (in debug/pe)
B2CENSUS KEPT-OUT reflect.makeFuncImpl (in reflect)
B2CENSUS KEPT-OUT reflect.methodValue (in reflect)
B2CENSUS KEPT-OUT runtime._func (in runtime)
B2CENSUS KEPT-OUT runtime.checkmarksMap (in runtime)
B2CENSUS KEPT-OUT runtime.cleanupBlock (in runtime)
B2CENSUS KEPT-OUT runtime.dloggerImpl (in runtime)
B2CENSUS KEPT-OUT runtime.finBlock (in runtime)
B2CENSUS KEPT-OUT runtime.g (in runtime)
B2CENSUS KEPT-OUT runtime.gcBitsArena (in runtime)
B2CENSUS KEPT-OUT runtime.heapArena (in runtime)
B2CENSUS KEPT-OUT runtime.immortalWeakHandle (in runtime)
B2CENSUS KEPT-OUT runtime.itabTableType (in runtime)
B2CENSUS KEPT-OUT runtime.m (in runtime)
B2CENSUS KEPT-OUT runtime.mcache (in runtime)
B2CENSUS KEPT-OUT runtime.mcontext (in runtime)
B2CENSUS KEPT-OUT runtime.memRecord (in runtime)
B2CENSUS KEPT-OUT runtime.mheap (in runtime)
B2CENSUS KEPT-OUT runtime.p (in runtime)
B2CENSUS KEPT-OUT runtime.sigcontext (in runtime)
B2CENSUS KEPT-OUT runtime.spanInlineMarkBits (in runtime)
B2CENSUS KEPT-OUT runtime.spanSetBlock (in runtime)
B2CENSUS KEPT-OUT runtime.stackObjectBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackWorkBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackmap (in runtime)
B2CENSUS KEPT-OUT runtime.traceBuf (in runtime)
B2CENSUS KEPT-OUT runtime.traceMapNode (in runtime)
B2CENSUS KEPT-OUT runtime.traceRegionAllocBlock (in runtime)
B2CENSUS KEPT-OUT runtime.ucontext (in runtime)
B2CENSUS KEPT-OUT runtime.workbuf (in runtime)
B2CENSUS KEPT-OUT runtime.xRegState (in runtime)
B2CENSUS KEPT-OUT sync.poolLocal (in sync)
B2CENSUS KEPT-OUT syscall.IPv6MTUInfo (in syscall)
B2CENSUS KEPT-OUT syscall.Msghdr (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrAny (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet4 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet6 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrLinklayer (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrUnix (in syscall)
B2CENSUS KEPT-OUT syscall.SockFprog (in syscall)
B2CENSUS KEPT-OUT syscall.iflags (in syscall)
B2CENSUS KEPT-OUT value:[256]runtime.typeCacheBucket (in runtime)
B2CENSUS KEPT-OUT value:[43]crypto/internal/fips140/nistec.p256AffineTable (in crypto/internal/fips140/nistec)
B2CENSUS SITE runtime runtime/malloc.go:880 *runtime.heapArena
B2CENSUS SITE runtime runtime/malloc.go:882 *runtime.heapArena
B2CENSUS SITE runtime runtime/mcheckmark.go:48 *runtime.checkmarksMap
B2CENSUS SITE runtime runtime/trace.go:809 *runtime.traceBuf
B2CENSUS TOTAL target=linux/amd64 packages=362 kept-out=44 rvalue-sites=4
```

### std-windows-amd64

```text
B2CENSUS KEPT-OUT debug/pe.COFFSymbol (in debug/pe)
B2CENSUS KEPT-OUT debug/pe.COFFSymbolAuxFormat5 (in debug/pe)
B2CENSUS KEPT-OUT internal/syscall/windows.FILE_LINK_INFORMATION (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.FILE_RENAME_INFORMATION (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.FILE_RENAME_INFORMATION_EX (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.IpAdapterAddresses (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.TOKEN_GROUPS (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.TOKEN_PRIVILEGES (in internal/syscall/windows)
B2CENSUS KEPT-OUT reflect.makeFuncImpl (in reflect)
B2CENSUS KEPT-OUT reflect.methodValue (in reflect)
B2CENSUS KEPT-OUT runtime._func (in runtime)
B2CENSUS KEPT-OUT runtime.checkmarksMap (in runtime)
B2CENSUS KEPT-OUT runtime.cleanupBlock (in runtime)
B2CENSUS KEPT-OUT runtime.dloggerImpl (in runtime)
B2CENSUS KEPT-OUT runtime.finBlock (in runtime)
B2CENSUS KEPT-OUT runtime.g (in runtime)
B2CENSUS KEPT-OUT runtime.gcBitsArena (in runtime)
B2CENSUS KEPT-OUT runtime.heapArena (in runtime)
B2CENSUS KEPT-OUT runtime.immortalWeakHandle (in runtime)
B2CENSUS KEPT-OUT runtime.itabTableType (in runtime)
B2CENSUS KEPT-OUT runtime.m (in runtime)
B2CENSUS KEPT-OUT runtime.mcache (in runtime)
B2CENSUS KEPT-OUT runtime.memRecord (in runtime)
B2CENSUS KEPT-OUT runtime.mheap (in runtime)
B2CENSUS KEPT-OUT runtime.p (in runtime)
B2CENSUS KEPT-OUT runtime.spanInlineMarkBits (in runtime)
B2CENSUS KEPT-OUT runtime.spanSetBlock (in runtime)
B2CENSUS KEPT-OUT runtime.stackObjectBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackWorkBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackmap (in runtime)
B2CENSUS KEPT-OUT runtime.traceBuf (in runtime)
B2CENSUS KEPT-OUT runtime.traceMapNode (in runtime)
B2CENSUS KEPT-OUT runtime.traceRegionAllocBlock (in runtime)
B2CENSUS KEPT-OUT runtime.workbuf (in runtime)
B2CENSUS KEPT-OUT runtime.xRegState (in runtime)
B2CENSUS KEPT-OUT sync.poolLocal (in sync)
B2CENSUS KEPT-OUT syscall.DNSRecord (in syscall)
B2CENSUS KEPT-OUT syscall.IpAdapterInfo (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrAny (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet4 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet6 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrUnix (in syscall)
B2CENSUS KEPT-OUT syscall.WSAData (in syscall)
B2CENSUS KEPT-OUT syscall.mountPointReparseBuffer (in syscall)
B2CENSUS KEPT-OUT syscall.symbolicLinkReparseBuffer (in syscall)
B2CENSUS KEPT-OUT syscall.win32finddata1 (in syscall)
B2CENSUS KEPT-OUT value:[256]runtime.typeCacheBucket (in runtime)
B2CENSUS KEPT-OUT value:[43]crypto/internal/fips140/nistec.p256AffineTable (in crypto/internal/fips140/nistec)
B2CENSUS SITE internal/poll internal/poll/fd_windows.go:1351 *syscall.RawSockaddrInet4
B2CENSUS SITE internal/poll internal/poll/fd_windows.go:1363 *syscall.RawSockaddrInet6
B2CENSUS SITE runtime runtime/malloc.go:880 *runtime.heapArena
B2CENSUS SITE runtime runtime/malloc.go:882 *runtime.heapArena
B2CENSUS SITE runtime runtime/mcheckmark.go:48 *runtime.checkmarksMap
B2CENSUS SITE runtime runtime/trace.go:809 *runtime.traceBuf
B2CENSUS TOTAL target=windows/amd64 packages=364 kept-out=48 rvalue-sites=6
```

### std-darwin-amd64

```text
B2CENSUS KEPT-OUT debug/pe.COFFSymbol (in debug/pe)
B2CENSUS KEPT-OUT debug/pe.COFFSymbolAuxFormat5 (in debug/pe)
B2CENSUS KEPT-OUT reflect.makeFuncImpl (in reflect)
B2CENSUS KEPT-OUT reflect.methodValue (in reflect)
B2CENSUS KEPT-OUT runtime._func (in runtime)
B2CENSUS KEPT-OUT runtime.checkmarksMap (in runtime)
B2CENSUS KEPT-OUT runtime.cleanupBlock (in runtime)
B2CENSUS KEPT-OUT runtime.dloggerImpl (in runtime)
B2CENSUS KEPT-OUT runtime.finBlock (in runtime)
B2CENSUS KEPT-OUT runtime.g (in runtime)
B2CENSUS KEPT-OUT runtime.gcBitsArena (in runtime)
B2CENSUS KEPT-OUT runtime.heapArena (in runtime)
B2CENSUS KEPT-OUT runtime.immortalWeakHandle (in runtime)
B2CENSUS KEPT-OUT runtime.itabTableType (in runtime)
B2CENSUS KEPT-OUT runtime.m (in runtime)
B2CENSUS KEPT-OUT runtime.mcache (in runtime)
B2CENSUS KEPT-OUT runtime.memRecord (in runtime)
B2CENSUS KEPT-OUT runtime.mheap (in runtime)
B2CENSUS KEPT-OUT runtime.p (in runtime)
B2CENSUS KEPT-OUT runtime.spanInlineMarkBits (in runtime)
B2CENSUS KEPT-OUT runtime.spanSetBlock (in runtime)
B2CENSUS KEPT-OUT runtime.stackObjectBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackWorkBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackmap (in runtime)
B2CENSUS KEPT-OUT runtime.traceBuf (in runtime)
B2CENSUS KEPT-OUT runtime.traceMapNode (in runtime)
B2CENSUS KEPT-OUT runtime.traceRegionAllocBlock (in runtime)
B2CENSUS KEPT-OUT runtime.ucontext (in runtime)
B2CENSUS KEPT-OUT runtime.workbuf (in runtime)
B2CENSUS KEPT-OUT runtime.xRegState (in runtime)
B2CENSUS KEPT-OUT runtime/pprof.machVMRegionBasicInfoData (in runtime/pprof)
B2CENSUS KEPT-OUT sync.poolLocal (in sync)
B2CENSUS KEPT-OUT syscall.BpfProgram (in syscall)
B2CENSUS KEPT-OUT syscall.IPv6MTUInfo (in syscall)
B2CENSUS KEPT-OUT syscall.InterfaceAddrMessage (in syscall)
B2CENSUS KEPT-OUT syscall.InterfaceMessage (in syscall)
B2CENSUS KEPT-OUT syscall.InterfaceMulticastAddrMessage (in syscall)
B2CENSUS KEPT-OUT syscall.Msghdr (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrAny (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrDatalink (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet4 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet6 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrUnix (in syscall)
B2CENSUS KEPT-OUT syscall.RouteMessage (in syscall)
B2CENSUS KEPT-OUT syscall.ivalue (in syscall)
B2CENSUS KEPT-OUT value:[256]runtime.typeCacheBucket (in runtime)
B2CENSUS KEPT-OUT value:[43]crypto/internal/fips140/nistec.p256AffineTable (in crypto/internal/fips140/nistec)
B2CENSUS SITE runtime runtime/malloc.go:880 *runtime.heapArena
B2CENSUS SITE runtime runtime/malloc.go:882 *runtime.heapArena
B2CENSUS SITE runtime runtime/mcheckmark.go:48 *runtime.checkmarksMap
B2CENSUS SITE runtime runtime/trace.go:809 *runtime.traceBuf
B2CENSUS TOTAL target=darwin/amd64 packages=362 kept-out=47 rvalue-sites=4
```

### std-linux-arm64

```text
B2CENSUS KEPT-OUT debug/pe.COFFSymbol (in debug/pe)
B2CENSUS KEPT-OUT debug/pe.COFFSymbolAuxFormat5 (in debug/pe)
B2CENSUS KEPT-OUT reflect.makeFuncImpl (in reflect)
B2CENSUS KEPT-OUT reflect.methodValue (in reflect)
B2CENSUS KEPT-OUT runtime._func (in runtime)
B2CENSUS KEPT-OUT runtime.checkmarksMap (in runtime)
B2CENSUS KEPT-OUT runtime.cleanupBlock (in runtime)
B2CENSUS KEPT-OUT runtime.dloggerImpl (in runtime)
B2CENSUS KEPT-OUT runtime.finBlock (in runtime)
B2CENSUS KEPT-OUT runtime.g (in runtime)
B2CENSUS KEPT-OUT runtime.gcBitsArena (in runtime)
B2CENSUS KEPT-OUT runtime.heapArena (in runtime)
B2CENSUS KEPT-OUT runtime.immortalWeakHandle (in runtime)
B2CENSUS KEPT-OUT runtime.itabTableType (in runtime)
B2CENSUS KEPT-OUT runtime.m (in runtime)
B2CENSUS KEPT-OUT runtime.mcache (in runtime)
B2CENSUS KEPT-OUT runtime.memRecord (in runtime)
B2CENSUS KEPT-OUT runtime.mheap (in runtime)
B2CENSUS KEPT-OUT runtime.p (in runtime)
B2CENSUS KEPT-OUT runtime.spanInlineMarkBits (in runtime)
B2CENSUS KEPT-OUT runtime.spanSetBlock (in runtime)
B2CENSUS KEPT-OUT runtime.stackObjectBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackWorkBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackmap (in runtime)
B2CENSUS KEPT-OUT runtime.traceBuf (in runtime)
B2CENSUS KEPT-OUT runtime.traceMapNode (in runtime)
B2CENSUS KEPT-OUT runtime.traceRegionAllocBlock (in runtime)
B2CENSUS KEPT-OUT runtime.ucontext (in runtime)
B2CENSUS KEPT-OUT runtime.workbuf (in runtime)
B2CENSUS KEPT-OUT runtime.xRegState (in runtime)
B2CENSUS KEPT-OUT sync.poolLocal (in sync)
B2CENSUS KEPT-OUT syscall.IPv6MTUInfo (in syscall)
B2CENSUS KEPT-OUT syscall.Msghdr (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrAny (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet4 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet6 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrLinklayer (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrUnix (in syscall)
B2CENSUS KEPT-OUT syscall.SockFprog (in syscall)
B2CENSUS KEPT-OUT syscall.iflags (in syscall)
B2CENSUS KEPT-OUT value:[256]runtime.typeCacheBucket (in runtime)
B2CENSUS KEPT-OUT value:[43]crypto/internal/fips140/nistec.p256AffineTable (in crypto/internal/fips140/nistec)
B2CENSUS SITE runtime runtime/malloc.go:880 *runtime.heapArena
B2CENSUS SITE runtime runtime/malloc.go:882 *runtime.heapArena
B2CENSUS SITE runtime runtime/mcheckmark.go:48 *runtime.checkmarksMap
B2CENSUS SITE runtime runtime/trace.go:809 *runtime.traceBuf
B2CENSUS TOTAL target=linux/arm64 packages=360 kept-out=42 rvalue-sites=4
```

### libc-linux-amd64

```text
B2CENSUS KEPT-OUT anon:struct{FPS [0][1042]modernc.org/libc.TBF_word; Fs struct{FP modernc.org/libc.TBF_key; FS [4][256]modernc.org/libc.TBF_word}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Fln2hi float64; Fln2lo float64; Fpoly [5]float64; Fpoly1 [11]float64; Ftab [128]struct{Finvc float64; Flogc float64}; Ftab2 [128]struct{Fchi float64; Fclo float64}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Fln2hi float64; Fln2lo float64; Fpoly [7]float64; Ftab [128]struct{Finvc float64; Fpad float64; Flogc float64; Flogctail float64}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Freq [0]struct{Fnlh modernc.org/libc.Tnlmsghdr; Fg modernc.org/libc.Trtgenmsg}; Freply [0]modernc.org/libc.Tnlmsghdr; Fbuf [8192]modernc.org/libc.Tuint8_t} (in modernc.org/libc)
B2CENSUS SITE modernc.org/libc libc@v1.80.1/ccgo_linux_amd64.go:7741 *anon:struct{FPS [0][1042]modernc.org/libc.TBF_word; Fs struct{FP modernc.org/libc.TBF_key; FS [4][256]modernc.org/libc.TBF_word}}
B2CENSUS TOTAL target=linux/amd64 packages=30 kept-out=4 rvalue-sites=1
```

### libc-linux-arm64

```text
B2CENSUS KEPT-OUT anon:struct{FPS [0][1042]modernc.org/libc.TBF_word; Fs struct{FP modernc.org/libc.TBF_key; FS [4][256]modernc.org/libc.TBF_word}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Fln2hi float64; Fln2lo float64; Fpoly [7]float64; Ftab [128]struct{Finvc float64; Fpad float64; Flogc float64; Flogctail float64}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Freq [0]struct{Fnlh modernc.org/libc.Tnlmsghdr; Fg modernc.org/libc.Trtgenmsg}; Freply [0]modernc.org/libc.Tnlmsghdr; Fbuf [8192]modernc.org/libc.Tuint8_t} (in modernc.org/libc)
B2CENSUS SITE modernc.org/libc libc@v1.80.1/ccgo_linux_arm64.go:7664 *anon:struct{FPS [0][1042]modernc.org/libc.TBF_word; Fs struct{FP modernc.org/libc.TBF_key; FS [4][256]modernc.org/libc.TBF_word}}
B2CENSUS TOTAL target=linux/arm64 packages=30 kept-out=3 rvalue-sites=1
```

## Run 2 (the reading of record)

### std-linux-amd64

```text
B2CENSUS KEPT-OUT debug/pe.COFFSymbol (in debug/pe)
B2CENSUS KEPT-OUT debug/pe.COFFSymbolAuxFormat5 (in debug/pe)
B2CENSUS KEPT-OUT reflect.makeFuncImpl (in reflect)
B2CENSUS KEPT-OUT reflect.methodValue (in reflect)
B2CENSUS KEPT-OUT runtime._func (in runtime)
B2CENSUS KEPT-OUT runtime.checkmarksMap (in runtime)
B2CENSUS KEPT-OUT runtime.cleanupBlock (in runtime)
B2CENSUS KEPT-OUT runtime.dloggerImpl (in runtime)
B2CENSUS KEPT-OUT runtime.finBlock (in runtime)
B2CENSUS KEPT-OUT runtime.g (in runtime)
B2CENSUS KEPT-OUT runtime.gcBitsArena (in runtime)
B2CENSUS KEPT-OUT runtime.heapArena (in runtime)
B2CENSUS KEPT-OUT runtime.immortalWeakHandle (in runtime)
B2CENSUS KEPT-OUT runtime.itabTableType (in runtime)
B2CENSUS KEPT-OUT runtime.m (in runtime)
B2CENSUS KEPT-OUT runtime.mcache (in runtime)
B2CENSUS KEPT-OUT runtime.mcontext (in runtime)
B2CENSUS KEPT-OUT runtime.memRecord (in runtime)
B2CENSUS KEPT-OUT runtime.mheap (in runtime)
B2CENSUS KEPT-OUT runtime.p (in runtime)
B2CENSUS KEPT-OUT runtime.sigcontext (in runtime)
B2CENSUS KEPT-OUT runtime.spanInlineMarkBits (in runtime)
B2CENSUS KEPT-OUT runtime.spanSetBlock (in runtime)
B2CENSUS KEPT-OUT runtime.stackObjectBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackWorkBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackmap (in runtime)
B2CENSUS KEPT-OUT runtime.traceBuf (in runtime)
B2CENSUS KEPT-OUT runtime.traceMapNode (in runtime)
B2CENSUS KEPT-OUT runtime.traceRegionAllocBlock (in runtime)
B2CENSUS KEPT-OUT runtime.ucontext (in runtime)
B2CENSUS KEPT-OUT runtime.workbuf (in runtime)
B2CENSUS KEPT-OUT runtime.xRegState (in runtime)
B2CENSUS KEPT-OUT sync.poolLocal (in sync)
B2CENSUS KEPT-OUT syscall.IPv6MTUInfo (in syscall)
B2CENSUS KEPT-OUT syscall.Msghdr (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrAny (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet4 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet6 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrLinklayer (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrUnix (in syscall)
B2CENSUS KEPT-OUT syscall.SockFprog (in syscall)
B2CENSUS KEPT-OUT syscall.iflags (in syscall)
B2CENSUS KEPT-OUT value:[256]runtime.typeCacheBucket (in runtime)
B2CENSUS KEPT-OUT value:[43]crypto/internal/fips140/nistec.p256AffineTable (in crypto/internal/fips140/nistec)
B2CENSUS TOTAL target=linux/amd64 packages=362 kept-out=44 rvalue-sites=0
```

### std-windows-amd64

```text
B2CENSUS KEPT-OUT debug/pe.COFFSymbol (in debug/pe)
B2CENSUS KEPT-OUT debug/pe.COFFSymbolAuxFormat5 (in debug/pe)
B2CENSUS KEPT-OUT internal/syscall/windows.FILE_LINK_INFORMATION (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.FILE_RENAME_INFORMATION (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.FILE_RENAME_INFORMATION_EX (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.IpAdapterAddresses (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.TOKEN_GROUPS (in internal/syscall/windows)
B2CENSUS KEPT-OUT internal/syscall/windows.TOKEN_PRIVILEGES (in internal/syscall/windows)
B2CENSUS KEPT-OUT reflect.makeFuncImpl (in reflect)
B2CENSUS KEPT-OUT reflect.methodValue (in reflect)
B2CENSUS KEPT-OUT runtime._func (in runtime)
B2CENSUS KEPT-OUT runtime.checkmarksMap (in runtime)
B2CENSUS KEPT-OUT runtime.cleanupBlock (in runtime)
B2CENSUS KEPT-OUT runtime.dloggerImpl (in runtime)
B2CENSUS KEPT-OUT runtime.finBlock (in runtime)
B2CENSUS KEPT-OUT runtime.g (in runtime)
B2CENSUS KEPT-OUT runtime.gcBitsArena (in runtime)
B2CENSUS KEPT-OUT runtime.heapArena (in runtime)
B2CENSUS KEPT-OUT runtime.immortalWeakHandle (in runtime)
B2CENSUS KEPT-OUT runtime.itabTableType (in runtime)
B2CENSUS KEPT-OUT runtime.m (in runtime)
B2CENSUS KEPT-OUT runtime.mcache (in runtime)
B2CENSUS KEPT-OUT runtime.memRecord (in runtime)
B2CENSUS KEPT-OUT runtime.mheap (in runtime)
B2CENSUS KEPT-OUT runtime.p (in runtime)
B2CENSUS KEPT-OUT runtime.spanInlineMarkBits (in runtime)
B2CENSUS KEPT-OUT runtime.spanSetBlock (in runtime)
B2CENSUS KEPT-OUT runtime.stackObjectBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackWorkBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackmap (in runtime)
B2CENSUS KEPT-OUT runtime.traceBuf (in runtime)
B2CENSUS KEPT-OUT runtime.traceMapNode (in runtime)
B2CENSUS KEPT-OUT runtime.traceRegionAllocBlock (in runtime)
B2CENSUS KEPT-OUT runtime.workbuf (in runtime)
B2CENSUS KEPT-OUT runtime.xRegState (in runtime)
B2CENSUS KEPT-OUT sync.poolLocal (in sync)
B2CENSUS KEPT-OUT syscall.DNSRecord (in syscall)
B2CENSUS KEPT-OUT syscall.IpAdapterInfo (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrAny (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet4 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet6 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrUnix (in syscall)
B2CENSUS KEPT-OUT syscall.WSAData (in syscall)
B2CENSUS KEPT-OUT syscall.mountPointReparseBuffer (in syscall)
B2CENSUS KEPT-OUT syscall.symbolicLinkReparseBuffer (in syscall)
B2CENSUS KEPT-OUT syscall.win32finddata1 (in syscall)
B2CENSUS KEPT-OUT value:[256]runtime.typeCacheBucket (in runtime)
B2CENSUS KEPT-OUT value:[43]crypto/internal/fips140/nistec.p256AffineTable (in crypto/internal/fips140/nistec)
B2CENSUS TOTAL target=windows/amd64 packages=364 kept-out=48 rvalue-sites=0
```

### std-darwin-amd64

```text
B2CENSUS KEPT-OUT debug/pe.COFFSymbol (in debug/pe)
B2CENSUS KEPT-OUT debug/pe.COFFSymbolAuxFormat5 (in debug/pe)
B2CENSUS KEPT-OUT reflect.makeFuncImpl (in reflect)
B2CENSUS KEPT-OUT reflect.methodValue (in reflect)
B2CENSUS KEPT-OUT runtime._func (in runtime)
B2CENSUS KEPT-OUT runtime.checkmarksMap (in runtime)
B2CENSUS KEPT-OUT runtime.cleanupBlock (in runtime)
B2CENSUS KEPT-OUT runtime.dloggerImpl (in runtime)
B2CENSUS KEPT-OUT runtime.finBlock (in runtime)
B2CENSUS KEPT-OUT runtime.g (in runtime)
B2CENSUS KEPT-OUT runtime.gcBitsArena (in runtime)
B2CENSUS KEPT-OUT runtime.heapArena (in runtime)
B2CENSUS KEPT-OUT runtime.immortalWeakHandle (in runtime)
B2CENSUS KEPT-OUT runtime.itabTableType (in runtime)
B2CENSUS KEPT-OUT runtime.m (in runtime)
B2CENSUS KEPT-OUT runtime.mcache (in runtime)
B2CENSUS KEPT-OUT runtime.memRecord (in runtime)
B2CENSUS KEPT-OUT runtime.mheap (in runtime)
B2CENSUS KEPT-OUT runtime.p (in runtime)
B2CENSUS KEPT-OUT runtime.spanInlineMarkBits (in runtime)
B2CENSUS KEPT-OUT runtime.spanSetBlock (in runtime)
B2CENSUS KEPT-OUT runtime.stackObjectBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackWorkBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackmap (in runtime)
B2CENSUS KEPT-OUT runtime.traceBuf (in runtime)
B2CENSUS KEPT-OUT runtime.traceMapNode (in runtime)
B2CENSUS KEPT-OUT runtime.traceRegionAllocBlock (in runtime)
B2CENSUS KEPT-OUT runtime.ucontext (in runtime)
B2CENSUS KEPT-OUT runtime.workbuf (in runtime)
B2CENSUS KEPT-OUT runtime.xRegState (in runtime)
B2CENSUS KEPT-OUT runtime/pprof.machVMRegionBasicInfoData (in runtime/pprof)
B2CENSUS KEPT-OUT sync.poolLocal (in sync)
B2CENSUS KEPT-OUT syscall.BpfProgram (in syscall)
B2CENSUS KEPT-OUT syscall.IPv6MTUInfo (in syscall)
B2CENSUS KEPT-OUT syscall.InterfaceAddrMessage (in syscall)
B2CENSUS KEPT-OUT syscall.InterfaceMessage (in syscall)
B2CENSUS KEPT-OUT syscall.InterfaceMulticastAddrMessage (in syscall)
B2CENSUS KEPT-OUT syscall.Msghdr (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrAny (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrDatalink (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet4 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet6 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrUnix (in syscall)
B2CENSUS KEPT-OUT syscall.RouteMessage (in syscall)
B2CENSUS KEPT-OUT syscall.ivalue (in syscall)
B2CENSUS KEPT-OUT value:[256]runtime.typeCacheBucket (in runtime)
B2CENSUS KEPT-OUT value:[43]crypto/internal/fips140/nistec.p256AffineTable (in crypto/internal/fips140/nistec)
B2CENSUS TOTAL target=darwin/amd64 packages=362 kept-out=47 rvalue-sites=0
```

### std-linux-arm64

```text
B2CENSUS KEPT-OUT debug/pe.COFFSymbol (in debug/pe)
B2CENSUS KEPT-OUT debug/pe.COFFSymbolAuxFormat5 (in debug/pe)
B2CENSUS KEPT-OUT reflect.makeFuncImpl (in reflect)
B2CENSUS KEPT-OUT reflect.methodValue (in reflect)
B2CENSUS KEPT-OUT runtime._func (in runtime)
B2CENSUS KEPT-OUT runtime.checkmarksMap (in runtime)
B2CENSUS KEPT-OUT runtime.cleanupBlock (in runtime)
B2CENSUS KEPT-OUT runtime.dloggerImpl (in runtime)
B2CENSUS KEPT-OUT runtime.finBlock (in runtime)
B2CENSUS KEPT-OUT runtime.g (in runtime)
B2CENSUS KEPT-OUT runtime.gcBitsArena (in runtime)
B2CENSUS KEPT-OUT runtime.heapArena (in runtime)
B2CENSUS KEPT-OUT runtime.immortalWeakHandle (in runtime)
B2CENSUS KEPT-OUT runtime.itabTableType (in runtime)
B2CENSUS KEPT-OUT runtime.m (in runtime)
B2CENSUS KEPT-OUT runtime.mcache (in runtime)
B2CENSUS KEPT-OUT runtime.memRecord (in runtime)
B2CENSUS KEPT-OUT runtime.mheap (in runtime)
B2CENSUS KEPT-OUT runtime.p (in runtime)
B2CENSUS KEPT-OUT runtime.spanInlineMarkBits (in runtime)
B2CENSUS KEPT-OUT runtime.spanSetBlock (in runtime)
B2CENSUS KEPT-OUT runtime.stackObjectBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackWorkBuf (in runtime)
B2CENSUS KEPT-OUT runtime.stackmap (in runtime)
B2CENSUS KEPT-OUT runtime.traceBuf (in runtime)
B2CENSUS KEPT-OUT runtime.traceMapNode (in runtime)
B2CENSUS KEPT-OUT runtime.traceRegionAllocBlock (in runtime)
B2CENSUS KEPT-OUT runtime.ucontext (in runtime)
B2CENSUS KEPT-OUT runtime.workbuf (in runtime)
B2CENSUS KEPT-OUT runtime.xRegState (in runtime)
B2CENSUS KEPT-OUT sync.poolLocal (in sync)
B2CENSUS KEPT-OUT syscall.IPv6MTUInfo (in syscall)
B2CENSUS KEPT-OUT syscall.Msghdr (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrAny (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet4 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrInet6 (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrLinklayer (in syscall)
B2CENSUS KEPT-OUT syscall.RawSockaddrUnix (in syscall)
B2CENSUS KEPT-OUT syscall.SockFprog (in syscall)
B2CENSUS KEPT-OUT syscall.iflags (in syscall)
B2CENSUS KEPT-OUT value:[256]runtime.typeCacheBucket (in runtime)
B2CENSUS KEPT-OUT value:[43]crypto/internal/fips140/nistec.p256AffineTable (in crypto/internal/fips140/nistec)
B2CENSUS TOTAL target=linux/arm64 packages=360 kept-out=42 rvalue-sites=0
```

### libc-linux-amd64

```text
B2CENSUS KEPT-OUT anon:struct{FPS [0][1042]modernc.org/libc.TBF_word; Fs struct{FP modernc.org/libc.TBF_key; FS [4][256]modernc.org/libc.TBF_word}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Fln2hi float64; Fln2lo float64; Fpoly [5]float64; Fpoly1 [11]float64; Ftab [128]struct{Finvc float64; Flogc float64}; Ftab2 [128]struct{Fchi float64; Fclo float64}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Fln2hi float64; Fln2lo float64; Fpoly [7]float64; Ftab [128]struct{Finvc float64; Fpad float64; Flogc float64; Flogctail float64}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Freq [0]struct{Fnlh modernc.org/libc.Tnlmsghdr; Fg modernc.org/libc.Trtgenmsg}; Freply [0]modernc.org/libc.Tnlmsghdr; Fbuf [8192]modernc.org/libc.Tuint8_t} (in modernc.org/libc)
B2CENSUS SITE modernc.org/libc libc@v1.80.1/ccgo_linux_amd64.go:7741 *anon:struct{FPS [0][1042]modernc.org/libc.TBF_word; Fs struct{FP modernc.org/libc.TBF_key; FS [4][256]modernc.org/libc.TBF_word}}
B2CENSUS TOTAL target=linux/amd64 packages=30 kept-out=4 rvalue-sites=1
```

### libc-linux-arm64

```text
B2CENSUS KEPT-OUT anon:struct{FPS [0][1042]modernc.org/libc.TBF_word; Fs struct{FP modernc.org/libc.TBF_key; FS [4][256]modernc.org/libc.TBF_word}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Fln2hi float64; Fln2lo float64; Fpoly [7]float64; Ftab [128]struct{Finvc float64; Fpad float64; Flogc float64; Flogctail float64}} (in modernc.org/libc)
B2CENSUS KEPT-OUT anon:struct{Freq [0]struct{Fnlh modernc.org/libc.Tnlmsghdr; Fg modernc.org/libc.Trtgenmsg}; Freply [0]modernc.org/libc.Tnlmsghdr; Fbuf [8192]modernc.org/libc.Tuint8_t} (in modernc.org/libc)
B2CENSUS SITE modernc.org/libc libc@v1.80.1/ccgo_linux_arm64.go:7664 *anon:struct{FPS [0][1042]modernc.org/libc.TBF_word; Fs struct{FP modernc.org/libc.TBF_key; FS [4][256]modernc.org/libc.TBF_word}}
B2CENSUS TOTAL target=linux/arm64 packages=30 kept-out=3 rvalue-sites=1
```

<!-- {% endraw %} -->
