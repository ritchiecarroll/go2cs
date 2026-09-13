// zsyscall_windows_certchain_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of crypt32's CERTIFICATE-CHAIN seam -- the one call
// crypto/x509's Windows system verifier is built on, and the only member of the syscall
// struct-passing class that fails in BOTH directions at once.
//
// CertGetCertificateChain is where the two classes this package answers separately meet:
//
//     err = syscall.CertGetCertificateChain(0, storeCtx, verifyTime, storeCtx.Store,
//                                           para, flags, 0, &topCtx)   // root_windows.go
//
//   * `&topCtx` is the `**T` OUT-PARAMETER class (zsyscall_windows_ptrout_impl.cs): a slot the
//     kernel writes a raw address into, which a golib pointer box has no machine word to lend.
//     That half is unchanged and still uses this package's native out-cell plus publishPointerOut.
//
//   * `para` is the STRUCT-PASSING class (zsyscall_windows_impl.cs): a record handed to the kernel
//     BY ADDRESS whose converted layout is not the native one. This file is what adds the second
//     half, and the record is the worst-shaped member of that class yet -- see below.
//
// WHY CERT_CHAIN_PARA CANNOT CROSS BY ADDRESS. Native CERT_CHAIN_PARA is 80 bytes, and Go's
// syscall.CertChainPara mirrors those 80 bytes exactly -- which is why root_windows.go writes
// `para.Size = unsafe.Sizeof(*para)` and means the native size by it. The CONVERTED struct does
// not: `RequestedUsage.Usage.UsageIdentifiers` is a `ж<ж<byte>>` and `CacheResync` a `ж<Filetime>`,
// both MANAGED REFERENCES, so the CLR lays the record out reference-first at roughly a third of the
// size. Handing crypt32 that address has the kernel read 80 bytes off a much smaller managed
// object: every field past the first comes from the wrong offset.
//
// The measured consequence is not a fault but a HANG. `dwUrlRetrievalTimeout` sits at native offset
// 56 -- a BLOCKING NETWORK BUDGET, in milliseconds, for AIA and CRL retrieval -- and reading it out
// of whatever managed bytes land there hands crypt32 an arbitrary deadline while the chain engine
// goes to the network. crypto/x509's own offline probe (a self-signed leaf, verified with
// Roots == nil, which cannot need the network at all) blocked inside the call for minutes at ~1.7 s
// of CPU; crypto/tls's TestVerifyHostname and TestQUICHandshakeError reach the same call and both
// stop there, one as an SEHException out of Syscall9 and one as a timeout with no verdict at all.
//
// `RequestedUsage.Usage.UsageIdentifiers` is the field that makes this more than a layout copy. Go
// builds it as an array of C string pointers into the package-level windowsExtKeyUsageOIDs map:
//
//     para.RequestedUsage.Usage.UsageIdentifiers = &oids[0]     // []*byte
//
// Under go2cs each `*byte` addresses a NUL-terminated MANAGED byte slice, and the array of them is
// a managed array of boxes. Neither has any native form, so there is nothing to hand over and
// nothing to fix in the pointer model: the identifiers are transcribed into one native block for
// the duration of the call -- the mirror-is-a-LOCAL doctrine the synchronous members of this class
// follow -- and freed before it returns. crypt32 copies what it needs out of CERT_CHAIN_PARA during
// the call (the structure is documented as input-only), so nothing native escapes.
//
// THE READ-BACK HALF, AND WHY IT IS IN THE SAME FILE. Fixing the parameter alone was MEASURED not
// to be enough, and the reason is the transferable part: one of this call's arguments is a field the
// CALLER reads out of a native record before the wrapper is ever entered.
//
//     err = syscall.CertGetCertificateChain(0, storeCtx, verifyTime, storeCtx.Store, ...)
//
// `storeCtx` came back from CertAddCertificateContextToStore as a raw PCCERT_CONTEXT, and
// `storeCtx.Store` reads hCertStore at the CONVERTED CertContext's offset -- which, under the CLR's
// reference-first auto-layout, is where the NATIVE record keeps cbCertEncoded. The store handle
// crypt32 receives is therefore a certificate LENGTH. With the parameter mirror in place and nothing
// else, this file's own probe still died with the identical SEHException out of Syscall9: the input
// wall and the read-back wall are not separable at this call site.
//
// The read-back cannot take the shape zsyscall_windows_addrinfo_impl.cs uses for ADDRINFOW. That
// file transcribes a native chain into managed records and makes the free a NO-OP, which works
// because nothing native has to survive the call. Here the ORIGINAL pointer must go back to crypt32
// three more times -- as this call's leaf, to CertVerifyCertificateChainPolicy, and to the two
// CertFree* routines, which release memory crypt32 owns and reference-counts. A managed view alone
// would leak every context and every chain; a native box alone reads every field from the wrong
// offset. The record has to be BOTH.
//
// SO A VIEW REMEMBERS ITS NATIVE IDENTITY. Each pointer the kernel hands back becomes a managed
// transcription -- a real ж<CertContext> / ж<CertChainContext> whose fields the converted x509 code
// reads exactly as it reads any other Go struct -- and the address it was built from is recorded
// beside it in a weak side table (s_nativeIdentity). Every wrapper that must hand a pointer back to
// crypt32 asks that table first and falls back to the box's own address, so a context this file did
// not build -- CertCreateCertificateContext's, a plain native box nothing reads through -- keeps
// working unchanged and needs no hand-own. The table is a syscall-local seam, deliberately not a
// golib one: `ж<T>` has no business knowing that one particular pointee is reference-counted by
// crypt32, and the sync point ("the moment a raw word becomes a pointer box") is again something
// only the wrapper knows.
//
// CertVerifyCertificateChainPolicy -- the file's third concern, and the LAST crypt32 member on the
// system-verifier path -- closed the mint-site problem its earlier header named as still open. Its
// CERT_CHAIN_POLICY_PARA carries `pvExtraPolicyPara` as Go's opaque `syscall.Pointer`, minted in
// crypto/x509 as `unsafe.Pointer(sslPara)` over an SSLExtraCertChainPolicyPara whose ServerName is
// itself a managed reference. What used to arrive here was a transient managed address with no
// recoverable box behind it -- a MINT-SITE problem, not a boundary one, and it is fixed at the
// mint: the converter's opaquePointerMintEmission (convCallExpr.go) emits golib's
// ManagedPointerTokens.MintOpaque for `T(unsafe.Pointer(p))` over a `*struct{}` target, so the
// scalar this wrapper receives for a reference-bearing pointee is the referent's own pointer-order
// token, resolvable back to the box -- the same round trip the ADDRINFOW hand-own mints for its
// sockaddr fields, minted at the emission so every author of the Go shape (crypto/x509, converted
// test suites, user code) is covered rather than one hand-owned function. The wrapper below is the
// RESOLVER: it recovers the SSLExtraCertChainPolicyPara, transcribes it and the server name into
// native mirrors for exactly the duration of the call, hands crypt32 the chain's remembered native
// identity, and copies the policy status back. Reached only when a chain is TRUSTED and the caller
// supplied a DNS name; the SystemCertVerify behavioral test's policy rows are the offline guard
// (ALLOW_UNKNOWN_CA waives this test fixture's untrusted root so the name check's own verdict
// shows through -- match answers 0, mismatch answers CERT_E_CN_NO_MATCH, an answer crypt32 can
// only give if the name crossed the boundary intact).

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_certchain_impl.go exists, so a reconvert never regenerates this
// file). The declarations it replaces are registered in the converter's manualConversionFuncs, which
// is what turns the generated bodies into placeholders.
[module: go.GoManualConversion]

// The native mirrors, the identifier block and the out-cells are pointer work throughout -- see
// zsyscall_windows_impl.cs for why this is declared rather than inherited.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // CERT_ENHKEY_USAGE (== CTL_USAGE) exactly as Windows lays it out: a count and an array of
    // LPSTR. `UsageIdentifiers` is one machine pointer here and a `ж<ж<byte>>` in the conversion,
    // which is the whole difference.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCertEnhKeyUsage
    {
        public uint32 UsageIdentifierCount;
        // The padding the pointer's 8-byte alignment introduces. Spelled out rather than left to
        // the layout engine so this mirror can be read against wincrypt.h line for line; public so
        // it is a stated field of the layout and not an unused-private warning.
        public uint32 Alignment;
        public nuint UsageIdentifiers;
    }

    // CERT_USAGE_MATCH: 24 bytes, a match type and the usage array above.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCertUsageMatch
    {
        public uint32 Type;
        public uint32 Alignment;
        public NativeCertEnhKeyUsage Usage;
    }

    // CERT_CHAIN_PARA with CERT_CHAIN_PARA_HAS_EXTRA_FIELDS: 80 bytes, which is the number
    // root_windows.go writes into Size. The two Vista-era trailing members (pStrongSignPara,
    // dwStrongSignFlags) are absent from Go's declaration and are therefore absent here -- Size is
    // what tells crypt32 how much of the structure exists, and it must describe THIS layout.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCertChainPara
    {
        public uint32 Size;
        public uint32 Alignment;
        public NativeCertUsageMatch RequestedUsage;
        public NativeCertUsageMatch RequestedIssuancePolicy;
        public uint32 URLRetrievalTimeout;
        public uint32 CheckRevocationFreshnessTime;
        public uint32 RevocationFreshnessTime;
        public uint32 TrailingAlignment;
        public nuint CacheResync;
    }

    // ---- the POLICY-CALL mirrors -----------------------------------------------------------------
    //
    // CERT_CHAIN_POLICY_PARA: 16 bytes, which is what root_windows.go's `unsafe.Sizeof(*para)`
    // computes -- but the mirror states it from its own layout, exactly as the chain para above.
    // The converted record is reference-bearing (ExtraPolicyPara is a managed Pointer), so it can
    // no more cross by address than CERT_CHAIN_PARA could.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCertChainPolicyPara
    {
        public uint32 Size;
        public uint32 Flags;
        public nuint ExtraPolicyPara;
    }

    // SSL_EXTRA_CERT_CHAIN_POLICY_PARA: 24 bytes. The pointer's 8-byte alignment introduces the
    // stated padding word, as in NativeCertEnhKeyUsage above.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSslExtraCertChainPolicyPara
    {
        public uint32 Size;
        public uint32 AuthType;
        public uint32 Checks;
        public uint32 Alignment;
        public nuint ServerName;
    }

    // CERT_CHAIN_POLICY_STATUS: 24 bytes, the one record of this trio crypt32 WRITES.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCertChainPolicyStatus
    {
        public uint32 Size;
        public uint32 Error;
        public uint32 ChainIndex;
        public uint32 ElementIndex;
        public nuint ExtraPolicyStatus;
    }

    // ---- the READ-BACK mirrors -------------------------------------------------------------------
    //
    // Explicit layout, not Sequential, for every record crypt32 WRITES: the whole point of these is
    // the offsets, so they are stated rather than inferred, and each Size matches the wincrypt.h
    // structure through the last member Go's declaration carries. CERT_CHAIN_CONTEXT and
    // CERT_CHAIN_ELEMENT both continue past that point natively (dwCreateFlags/ChainId,
    // pwszExtendedErrorInfo's successors); reading is bounded by these declarations, never by Size.

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    private unsafe struct NativeCertContext
    {
        [FieldOffset(0)] public uint32 EncodingType;
        [FieldOffset(8)] public byte* EncodedCert;
        [FieldOffset(16)] public uint32 Length;
        [FieldOffset(24)] public nuint CertInfo;
        [FieldOffset(32)] public nuint CertStore;
    }

    [StructLayout(LayoutKind.Explicit, Size = 48)]
    private unsafe struct NativeCertChainContext
    {
        [FieldOffset(0)] public uint32 Size;
        [FieldOffset(4)] public uint32 TrustErrorStatus;
        [FieldOffset(8)] public uint32 TrustInfoStatus;
        [FieldOffset(12)] public uint32 ChainCount;
        [FieldOffset(16)] public nuint* Chains;
        [FieldOffset(24)] public uint32 LowerQualityChainCount;
        [FieldOffset(32)] public nuint* LowerQualityChains;
        [FieldOffset(40)] public uint32 HasRevocationFreshnessTime;
        [FieldOffset(44)] public uint32 RevocationFreshnessTime;
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    private unsafe struct NativeCertSimpleChain
    {
        [FieldOffset(0)] public uint32 Size;
        [FieldOffset(4)] public uint32 TrustErrorStatus;
        [FieldOffset(8)] public uint32 TrustInfoStatus;
        [FieldOffset(12)] public uint32 ElementCount;
        [FieldOffset(16)] public nuint* Elements;
        [FieldOffset(24)] public nuint TrustListInfo;
        [FieldOffset(32)] public uint32 HasRevocationFreshnessTime;
        [FieldOffset(36)] public uint32 RevocationFreshnessTime;
    }

    [StructLayout(LayoutKind.Explicit, Size = 56)]
    private struct NativeCertChainElement
    {
        [FieldOffset(0)] public uint32 Size;
        [FieldOffset(8)] public nuint CertContext;
        [FieldOffset(16)] public uint32 TrustErrorStatus;
        [FieldOffset(20)] public uint32 TrustInfoStatus;
        [FieldOffset(24)] public nuint RevocationInfo;
        [FieldOffset(32)] public nuint IssuanceUsage;
        [FieldOffset(40)] public nuint ApplicationUsage;
        [FieldOffset(48)] public nuint ExtendedErrorInfo;
    }

    // ---- the native identity a managed view remembers --------------------------------------------

    // view box -> the crypt32 address it was transcribed from. ConditionalWeakTable so a view that
    // becomes unreachable takes its entry with it: this table must never be the reason a
    // transcription stays alive, and it holds no claim on the NATIVE memory either -- crypt32's own
    // reference count, released by the Go code's `defer CertFree*`, owns that.
    private static readonly ConditionalWeakTable<object, object> s_nativeIdentity = new();

    private static void rememberNativeIdentity(object view, nuint address) {
        if (address != 0) {
            s_nativeIdentity.Add(view, address);
        }
    }

    // The address to hand back to crypt32 for a pointer the Go code is holding. A view this file
    // built answers from the table; anything else -- CertCreateCertificateContext's native box, or a
    // nil pointer -- answers with its own address exactly as the generated wrapper would, which is
    // what keeps the un-hand-owned producers working.
    private static uintptr nativeIdentityOf<T>(ж<T> box) {
        if (box is not null && s_nativeIdentity.TryGetValue(box, out object? remembered)) {
            return (uintptr)(nuint)remembered;
        }

        return (uintptr)box;
    }

    // CertGetCertificateChain(engine, leaf, time, additionalStore, para, flags, reserved, &chainCtx).
    //
    // Three of the eight arguments need the boundary's help and the other five are scalars or
    // handles that convert faithfully:
    //
    //   para      -- transcribed into the blittable mirror above, identifiers and all, for exactly
    //                the duration of the call.
    //   time      -- pTimeToVerify, an OPTIONAL FILETIME the kernel reads. Copied into a stack cell
    //                rather than passed by box address: Filetime is blittable, so the box COULD
    //                report an address, but it is an address the collector may move and the mirror
    //                states the lifetime instead of relying on it.
    //   &chainCtx -- the out-parameter, unchanged from the ptrout file's treatment.
    //
    // `Size` is an INPUT this mirror owns, exactly as Process32First's dwSize is: the caller's value
    // is Go's `unsafe.Sizeof(*para)`, which is the native 80 by construction, but the mirror is the
    // only thing that can be sure of it and so states it from its own layout.
    public static unsafe error /*err*/ CertGetCertificateChain(ΔHandle engine, ж<CertContext> Ꮡleaf, ж<Filetime> Ꮡtime, ΔHandle additionalStore, ж<CertChainPara> Ꮡpara, uint32 flags, uintptr reserved, ж<ж<CertChainContext>> ᏑchainCtx) {
        nuint cell = 0;
        uintptr cellAddr = ᏑchainCtx == nil ? 0 : (uintptr)(void*)(&cell);

        NativeFiletime time = default;
        NativeFiletime* timePtr = null;

        if (Ꮡtime != nil) {
            ref Filetime managedTime = ref Ꮡtime.Value;

            time.LowDateTime = managedTime.LowDateTime;
            time.HighDateTime = managedTime.HighDateTime;
            timePtr = &time;
        }

        NativeCertChainPara para = default;
        NativeCertChainPara* paraPtr = null;
        NativeFiletime cacheResync = default;
        void* identifiers = null;
        void* issuanceIdentifiers = null;

        try {
            if (Ꮡpara != nil) {
                ref CertChainPara managedPara = ref Ꮡpara.Value;

                para.Size = (uint32)sizeof(NativeCertChainPara);
                para.URLRetrievalTimeout = managedPara.URLRetrievalTimeout;
                para.CheckRevocationFreshnessTime = managedPara.CheckRevocationFreshnessTime;
                para.RevocationFreshnessTime = managedPara.RevocationFreshnessTime;

                copyUsageMatch(ref managedPara.RequestedUsage, ref para.RequestedUsage, ref identifiers);
                copyUsageMatch(ref managedPara.RequstedIssuancePolicy, ref para.RequestedIssuancePolicy, ref issuanceIdentifiers);

                // pftCacheResync is optional and root_windows.go never sets it; carried anyway,
                // because a mirror that silently drops a field the caller CAN set is the quiet
                // wrong answer this class's history keeps warning about. The cell is a frame local
                // (declared above the try, so its address outlives this block and the call).
                if (managedPara.CacheResync != nil) {
                    ref Filetime managedResync = ref managedPara.CacheResync.Value;

                    cacheResync.LowDateTime = managedResync.LowDateTime;
                    cacheResync.HighDateTime = managedResync.HighDateTime;
                    para.CacheResync = (nuint)(void*)(&cacheResync);
                }

                paraPtr = &para;
            }

            var (r1, _, e1) = Syscall9(procCertGetCertificateChain.Addr(), 8, (uintptr)engine, nativeIdentityOf(Ꮡleaf), (uintptr)(void*)timePtr, (uintptr)additionalStore, (uintptr)(void*)paraPtr, (uintptr)flags, (uintptr)reserved, cellAddr, 0);

            if (r1 == 0) {
                return errnoErr(e1);
            }
        }
        finally {
            // Freed here, always: CERT_CHAIN_PARA is input-only, so crypt32 has taken whatever it
            // needs by the time the call returns and nothing native escapes this function.
            if (identifiers != null) {
                NativeMemory.Free(identifiers);
            }

            if (issuanceIdentifiers != null) {
                NativeMemory.Free(issuanceIdentifiers);
            }
        }

        // NOT publishPointerOut: that publishes a NATIVE-address box, which is right for the members
        // in zsyscall_windows_ptrout_impl.cs (an opaque handle, a blittable string) and wrong here --
        // crypto/x509 walks this record field by field. ValueSlot for the same reason that file gives:
        // the caller's box legitimately holds null on entry and `Value`'s nil guard value-peeks.
        if (ᏑchainCtx != nil) {
            ᏑchainCtx.ValueSlot = viewCertChainContext(cell);
        }

        return default!;
    }

    // CertAddCertificateContextToStore(store, certContext, addDisposition, &storeContext). The
    // out-parameter half is the ptrout file's native cell; what is added here is that the published
    // pointer is a managed VIEW rather than a native box, because the very next statement in
    // crypto/x509 reads a FIELD through it (`storeCtx.Store`, this package's own additionalStore
    // argument one call later). ppStoreContext is documented OPTIONAL and root_windows.go passes nil
    // for every intermediate it adds, so the nil arm is a real caller.
    public static unsafe error /*err*/ CertAddCertificateContextToStore(ΔHandle store, ж<CertContext> ᏑcertContext, uint32 addDisposition, ж<ж<CertContext>> ᏑstoreContext) {
        nuint cell = 0;
        uintptr cellAddr = ᏑstoreContext == nil ? 0 : (uintptr)(void*)(&cell);

        var (r1, _, e1) = Syscall6(procCertAddCertificateContextToStore.Addr(), 4, (uintptr)store, nativeIdentityOf(ᏑcertContext), (uintptr)addDisposition, cellAddr, 0, 0);

        if (r1 == 0) {
            return errnoErr(e1);
        }

        if (ᏑstoreContext != nil) {
            ᏑstoreContext.ValueSlot = viewCertContext(cell);
        }

        return default!;
    }

    // CertFreeCertificateContext / CertFreeCertificateChain release memory crypt32 owns and
    // reference-counts, so they must be handed the ORIGINAL address -- the whole reason a view
    // remembers one. Freeing a managed transcription's own address would hand crypt32 a GC-heap
    // pointer; doing NOTHING (the FreeAddrInfoW answer) would leak a chain per verification.
    //
    // The managed view outlives the free and is deliberately left intact: everything crypto/x509
    // reads out of a chain it has already copied (extractSimpleChain runs bytes.Clone over the DER
    // before the defer fires), and the transcription's storage is managed, so a read after the free
    // is a stale VALUE rather than a use-after-free. The identity in the table is dangling from this
    // point on, which is exactly the lifetime Go gives the pointer it just released.
    public static error /*err*/ CertFreeCertificateContext(ж<CertContext> Ꮡctx) {
        var (r1, _, e1) = Syscall(procCertFreeCertificateContext.Addr(), 1, nativeIdentityOf(Ꮡctx), 0, 0);

        if (r1 == 0) {
            return errnoErr(e1);
        }

        return default!;
    }

    public static void CertFreeCertificateChain(ж<CertChainContext> Ꮡctx) {
        Syscall(procCertFreeCertificateChain.Addr(), 1, nativeIdentityOf(Ꮡctx), 0, 0);
    }

    // CertVerifyCertificateChainPolicy(policyOID, chainContext, para, &status). All three concerns
    // this file exists for meet in one call: the chain is a VIEW whose native identity the side
    // table remembers; the para is the struct-passing class with an OPAQUE pointer inside it; the
    // status is the same class written backward. The extra-policy pointer is where the opaque MINT
    // resolves (see the file header): a scalar that ManagedPointerTokens recognizes recovers the
    // SSLExtraCertChainPolicyPara box the converted x509 code built, and its fields -- the server
    // name especially -- are transcribed into one native block for exactly the duration of the
    // call, the mirror-is-a-LOCAL doctrine throughout. A scalar the table does NOT recognize
    // passes through unchanged: that is the nil pointer, and it is also a genuinely native (or
    // pinned reference-free) address, for which pass-through is the correct native call.
    //
    // `Size` on both mirrors is stated from the mirror's own layout, per this file's doctrine --
    // with one asymmetry worth recording: Go leaves the STATUS record's cbSize zero
    // (root_windows.go constructs `syscall.CertChainPolicyStatus{}` and crypt32 tolerates it),
    // and nothing is written back into the managed Size field, so a program that prints
    // status.Size observes exactly what it observes under Go.
    public static unsafe error /*err*/ CertVerifyCertificateChainPolicy(uintptr policyOID, ж<CertChainContext> Ꮡchain, ж<CertChainPolicyPara> Ꮡpara, ж<CertChainPolicyStatus> Ꮡstatus) {
        NativeCertChainPolicyPara para = default;
        NativeCertChainPolicyPara* paraPtr = null;
        NativeSslExtraCertChainPolicyPara sslPara = default;
        NativeCertChainPolicyStatus status = default;
        NativeCertChainPolicyStatus* statusPtr = null;
        void* serverName = null;

        try {
            if (Ꮡpara != nil) {
                ref CertChainPolicyPara managedPara = ref Ꮡpara.Value;

                para.Size = (uint32)sizeof(NativeCertChainPolicyPara);
                para.Flags = managedPara.Flags;

                if (managedPara.ExtraPolicyPara != nil) {
                    nuint scalar = (uintptr)managedPara.ExtraPolicyPara;

                    if (ManagedPointerTokens.Resolve(scalar) is ж<SSLExtraCertChainPolicyPara> Ꮡssl) {
                        ref SSLExtraCertChainPolicyPara managedSsl = ref Ꮡssl.Value;

                        sslPara.Size = (uint32)sizeof(NativeSslExtraCertChainPolicyPara);
                        sslPara.AuthType = managedSsl.AuthType;
                        sslPara.Checks = managedSsl.Checks;

                        if (managedSsl.ServerName != nil) {
                            serverName = allocUtf16z(managedSsl.ServerName);
                            sslPara.ServerName = (nuint)serverName;
                        }

                        para.ExtraPolicyPara = (nuint)(&sslPara);
                    } else {
                        para.ExtraPolicyPara = scalar;
                    }
                }

                paraPtr = &para;
            }

            if (Ꮡstatus != nil) {
                status.Size = (uint32)sizeof(NativeCertChainPolicyStatus);
                statusPtr = &status;
            }

            var (r1, _, e1) = Syscall6(procCertVerifyCertificateChainPolicy.Addr(), 4, policyOID, nativeIdentityOf(Ꮡchain), (uintptr)(void*)paraPtr, (uintptr)(void*)statusPtr, 0, 0);

            if (r1 == 0) {
                return errnoErr(e1);
            }
        }
        finally {
            // Freed here, always: the policy parameters are input-only, so nothing native escapes.
            if (serverName != null) {
                NativeMemory.Free(serverName);
            }
        }

        if (Ꮡstatus != nil) {
            ref CertChainPolicyStatus view = ref Ꮡstatus.Value;

            view.Error = status.Error;
            view.ChainIndex = status.ChainIndex;
            view.ElementIndex = status.ElementIndex;
            // ExtraPolicyStatus is left as the caller set it: the SSL policy writes nothing there,
            // and a native address fabricated into a managed Pointer would be the exact defect
            // class this file exists to keep out.
        }

        return default!;
    }

    // ---- the transcriptions ----------------------------------------------------------------------

    // A CERT_CONTEXT as a managed record. `EncodedCert` is COPIED rather than aliased: the field is a
    // `ж<byte>` and crypto/x509 reads it as `unsafe.Slice(cert.EncodedCert, cert.Length)`, which over
    // a native-address box would snapshot native memory into a managed slice anyway -- copying here
    // states the lifetime instead of inheriting it, and keeps the DER readable after the context is
    // freed.
    //
    // `CertInfo` is left nil: Go declares CertInfo as an EMPTY struct ("Not implemented" in
    // types_windows.go), so there is nothing to transcribe and no Go code that can read through it.
    private static unsafe ж<CertContext> viewCertContext(nuint address) {
        if (address == 0) {
            return default!;
        }

        NativeCertContext* native = (NativeCertContext*)address;
        var box = @new<CertContext>();
        ref CertContext view = ref box.Value;

        view.EncodingType = native->EncodingType;
        view.Length = native->Length;
        view.EncodedCert = copyNativeBytes(native->EncodedCert, native->Length);
        view.Store = ((ΔHandle)(uintptr)native->CertStore);

        rememberNativeIdentity(box, address);

        return box;
    }

    // A CERT_CHAIN_CONTEXT as a managed record, recursively: rgpChain and
    // rgpLowerQualityChainContext become managed arrays of boxes, so crypto/x509's
    // `unsafe.Slice(chainCtx.Chains, chainCtx.ChainCount)` windows managed storage and the walk that
    // follows reads real boxes at every level.
    private static unsafe ж<CertChainContext> viewCertChainContext(nuint address) {
        if (address == 0) {
            return default!;
        }

        NativeCertChainContext* native = (NativeCertChainContext*)address;
        var box = @new<CertChainContext>();
        ref CertChainContext view = ref box.Value;

        view.Size = native->Size;
        view.TrustStatus = new CertTrustStatus{
            ErrorStatus = native->TrustErrorStatus,
            InfoStatus = native->TrustInfoStatus
        };
        view.ChainCount = native->ChainCount;
        view.Chains = viewPointerArray<CertSimpleChain>(native->Chains, native->ChainCount, viewCertSimpleChain);
        view.LowerQualityChainCount = native->LowerQualityChainCount;
        view.LowerQualityChains = viewPointerArray<CertChainContext>(native->LowerQualityChains, native->LowerQualityChainCount, viewCertChainContext);
        view.HasRevocationFreshnessTime = native->HasRevocationFreshnessTime;
        view.RevocationFreshnessTime = native->RevocationFreshnessTime;

        rememberNativeIdentity(box, address);

        return box;
    }

    private static unsafe ж<CertSimpleChain> viewCertSimpleChain(nuint address) {
        if (address == 0) {
            return default!;
        }

        NativeCertSimpleChain* native = (NativeCertSimpleChain*)address;
        var box = @new<CertSimpleChain>();
        ref CertSimpleChain view = ref box.Value;

        view.Size = native->Size;
        view.TrustStatus = new CertTrustStatus{
            ErrorStatus = native->TrustErrorStatus,
            InfoStatus = native->TrustInfoStatus
        };
        view.NumElements = native->ElementCount;
        view.Elements = viewPointerArray<CertChainElement>(native->Elements, native->ElementCount, viewCertChainElement);
        view.HasRevocationFreshnessTime = native->HasRevocationFreshnessTime;
        view.RevocationFreshnessTime = native->RevocationFreshnessTime;

        rememberNativeIdentity(box, address);

        return box;
    }

    // `RevocationInfo`, `IssuanceUsage`, `ApplicationUsage` and `ExtendedErrorInfo` are left nil:
    // each points at a further native record whose converted form is itself reference-bearing, and
    // nothing in the corpus reads any of them (crypto/x509 takes CertContext alone). Transcribing
    // them on speculation would be the machinery this project's rules exclude -- a reader that ever
    // needs one gets nil, which is a determinate value, not a fabricated reference.
    private static unsafe ж<CertChainElement> viewCertChainElement(nuint address) {
        if (address == 0) {
            return default!;
        }

        NativeCertChainElement* native = (NativeCertChainElement*)address;
        var box = @new<CertChainElement>();
        ref CertChainElement view = ref box.Value;

        view.Size = native->Size;
        view.CertContext = viewCertContext(native->CertContext);
        view.TrustStatus = new CertTrustStatus{
            ErrorStatus = native->TrustErrorStatus,
            InfoStatus = native->TrustInfoStatus
        };

        rememberNativeIdentity(box, address);

        return box;
    }

    // Transcribes a native array of pointers into a managed slice of boxes and hands back a pointer
    // to its first element -- the `&array[0]` form the converted field holds, which unsafe.Slice
    // rebuilds as a window over the same storage.
    private static unsafe ж<ж<T>> viewPointerArray<T>(nuint* addresses, uint32 count, Func<nuint, ж<T>> view) {
        if (addresses == null || count == 0) {
            return default!;
        }

        var boxes = new slice<ж<T>>((nint)count);

        for (nint i = 0; i < (nint)count; i++) {
            boxes[i] = view(addresses[i]);
        }

        return Ꮡ(boxes, 0);
    }

    // Copies a native byte run into managed storage and points at its first element.
    private static unsafe ж<byte> copyNativeBytes(byte* source, uint32 length) {
        if (source == null || length == 0) {
            return default!;
        }

        var copy = new slice<byte>((nint)length);

        for (nint i = 0; i < (nint)length; i++) {
            copy[i] = source[i];
        }

        return Ꮡ(copy, 0);
    }

    // Transcribes one converted CertUsageMatch into its native mirror, allocating the LPSTR array
    // the native side needs when there is one. `block` receives the single allocation so the caller
    // can free it after the call; a zero count leaves it null and the native array pointer NULL,
    // which is what USAGE_MATCH_TYPE_AND with no identifiers means.
    private static unsafe void copyUsageMatch(ref CertUsageMatch managed, ref NativeCertUsageMatch native, ref void* block) {
        native.Type = managed.Type;
        native.Usage.UsageIdentifierCount = managed.Usage.Length;

        if (managed.Usage.Length == 0 || managed.Usage.UsageIdentifiers == nil) {
            native.Usage.UsageIdentifierCount = 0;
            native.Usage.UsageIdentifiers = 0;
            return;
        }

        block = allocUsageIdentifiers(managed.Usage.UsageIdentifiers, (nint)managed.Usage.Length);
        native.Usage.UsageIdentifiers = (nuint)block;
    }

    // Copies a NUL-terminated managed UTF-16 run (syscall.UTF16PtrFromString's `*uint16`, an
    // element pointer into a managed []uint16) into one native WCHAR block, terminator included.
    // The terminator is part of the data, so the length is found by walking to it -- through a
    // `fixed`, which holds the backing array still for exactly the copy, the same pattern
    // allocUsageIdentifiers uses one element size down.
    private static unsafe void* allocUtf16z(ж<uint16> text) {
        fixed (uint16* source = &text.Value) {
            nint length = 0;

            while (source[length] != 0) {
                length++;
            }

            uint16* allocation = (uint16*)NativeMemory.Alloc((nuint)(length + 1) * sizeof(uint16));

            for (nint i = 0; i <= length; i++) {
                allocation[i] = source[i];
            }

            return allocation;
        }
    }

    // Builds the native `LPSTR[count]` crypt32 reads the requested EKUs from: one allocation holding
    // the pointer array followed by the NUL-terminated strings it points at, so one Free releases
    // the whole thing.
    //
    // Each identifier arrives as a `ж<byte>` into a MANAGED byte slice
    // (windowsExtKeyUsageOIDs stores `[]byte(oid.String() + "\x00")`), so the terminator is part of
    // the data and the length is found by walking to it -- through a `fixed`, which holds the
    // backing still for exactly the copy. Copying rather than pinning the managed arrays in place is
    // deliberate: the array of POINTERS has no managed form at all, so a pin would only solve half
    // the problem and would leave the block's lifetime tied to a box instead of to this call.
    private static unsafe void* allocUsageIdentifiers(ж<ж<byte>> identifiers, nint count) {
        slice<ж<byte>> boxes = unsafe_package.Slice(identifiers, count);
        var staged = new byte[count][];
        nuint payload = 0;

        for (nint i = 0; i < count; i++) {
            ж<byte> identifier = boxes[i];

            if (identifier == nil) {
                staged[i] = [];
                payload++;
                continue;
            }

            fixed (byte* text = &identifier.Value) {
                nint length = 0;

                while (text[length] != 0) {
                    length++;
                }

                var copy = new byte[length + 1];

                for (nint j = 0; j < length; j++) {
                    copy[j] = text[j];
                }

                staged[i] = copy;
                payload += (nuint)(length + 1);
            }
        }

        nuint headerBytes = (nuint)count * (nuint)sizeof(nuint);
        byte* allocation = (byte*)NativeMemory.AllocZeroed(headerBytes + payload);
        nuint* slots = (nuint*)allocation;
        byte* cursor = allocation + headerBytes;

        for (nint i = 0; i < count; i++) {
            byte[] copy = staged[i];

            slots[i] = (nuint)cursor;

            for (nint j = 0; j < copy.Length; j++) {
                cursor[j] = copy[j];
            }

            cursor += copy.Length == 0 ? 1 : copy.Length;
        }

        return allocation;
    }
}
