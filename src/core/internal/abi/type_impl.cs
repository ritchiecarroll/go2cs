// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion (the reflection bridge — Phase 4).
//
// Go's abi.TypeOf reads an interface's type-word via unsafe.Pointer to reach a Go runtime type
// descriptor that has no managed equivalent: an `any` here is a single System.Object reference, not
// a two-word eface over a descriptor, so reinterpreting the reference reads garbage and NREs (the
// first operational hit is fmt.Print/Sprint → doPrint → reflect.TypeOf(arg).Kind()). Instead,
// synthesize an abi.Type descriptor whose Kind_ is classified from the value's managed System.Type
// (golib GoReflect.KindOf), and record the System.Type on the descriptor box so the hand-owned
// reflect Type/Value methods can recover Go type info from it. The converter skips the auto form of
// TypeOf via the manualConversionFuncs registry (go2cs/manualTypeOperations.go); this module marker
// also makes go2cs skip re-converting this file wholesale, and the overlay restores it over auto
// output on every reconversion. See docs/phase4/DESIGN-reflection-bridge.md.

[module: GoManualConversion]

namespace go.@internal;

partial class abi_package {

// The managed System.Type this synthetic abi.Type stands for — carried directly on the descriptor so
// the reflect Type methods (String/Name/Elem/Field) can recover Go type info from it (the reflect
// rtype wraps an abi.Type by value, so the field rides along the copy). Null for a non-synthesized Type.
// arrayDims is NON-IDENTITY cargo (increment 2): the Go array length(s) when the descriptor stands for
// an array type AND a source knew them (a live value, a declaring struct's zero-instance field
// initializers, a pointee behind an addressable Value) — reflect.Type interning stays keyed on the
// System.Type alone, so identity is deliberately length-blind (the recorded §5 limitation) while
// Len()/Size()/New/Zero consume real lengths wherever one is knowable. Null = unknown ([0]T is [0]).
// funcParamDims is the same NON-IDENTITY cargo one level out: a FUNC descriptor's per-parameter
// array dims, one entry per Go parameter and null where that parameter is not a fixed-size array.
// It exists because the parameter position is the one place no other dims source reaches — a
// `[32]byte` parameter has no value to measure and no field initializer to read, and the emitted
// delegate type is a bare Func<array<byte>, bool> that func([32]byte) bool and func([64]byte) bool
// share — so the converter stamps [GoArrayDims] on the parameter and GoReflect.FuncParamDims reads
// it back off the delegate INSTANCE here. RESULT dims are deliberately not carried: a multi-result
// Go func returns a ValueTuple, which has no per-element attribute position, and no measured
// consumer reads Out(i).Len().
// chanDir is the same NON-IDENTITY cargo for the one part of a CHANNEL type the managed emission
// cannot hold: its direction. `chan T`, `chan<- T` and `<-chan T` are three distinct Go types over
// one golib channel<T>, so the direction reaches the descriptor from whichever source knew it — a
// live value (the converter seeds it at the make site), the value behind a pointer (`new(chan<- T)`),
// or a struct field's initializer-borne zero. Unstamped is the honest "nothing narrowed this",
// answered as BothDir, which is what ChanDir() reported for every channel before the cargo existed.
// keyDims is the same cargo one accessor over. The carried arrayDims describe what Elem() hands
// down — an array's tail, a pointer's pointee, a MAP's element — which leaves Key(), a map type's
// second accessor, with no slot at all: `map[[2]string][2]*float64` could carry the element's 2 and
// never the key's. So the key's dims ride here, stamped by the converter at the one position that
// reads them (a struct FIELD, whose map is nil at a decode target and reveals nothing), and handed
// down by Key() exactly as arrayDims are by Elem().
partial struct Type {
    [GoReflectCompanion] public System.Type? sysType;
    [GoReflectCompanion] public nint[]? arrayDims;
    [GoReflectCompanion] public nint[]?[]? funcParamDims;
    [GoReflectCompanion] public GoChanDir[]? chanDirChain;
    [GoReflectCompanion] public nint[]? keyDims;
}

// synthType builds a managed-backed abi.Type from a System.Type: Kind_ classified from it (GoReflect),
// the System.Type carried on the descriptor, Go size/alignment stamped when knowable (binary's sizeof
// reads Size_ for the scalar kinds), and array dims carried as cargo when the caller knew them. The
// single builder behind both TypeOf (from a value) and reflect's Type.Elem/Field (from a type).
public static ж<Type> synthType(System.Type? st) {
    return synthType(st, null, null);
}

public static ж<Type> synthType(System.Type? st, nint[]? arrayDims) {
    return synthType(st, arrayDims, null);
}

// Descriptors are immutable once synthesized — intern them per (managed type, dims) so the
// per-TypeOf cost (kind classification + size stamping's field walk) is paid once, not per
// call (fmt classifies every argument; binary sizes in loops). The func param dims join the key
// for the same reason the array dims are in it: func([32]byte) bool and func([64]byte) bool are
// DISTINCT Go types over ONE managed delegate type, so interning them together would let the
// first to arrive answer In(0).Len() for both.
private static readonly System.Collections.Concurrent.ConcurrentDictionary<(System.Type, string), ж<Type>> s_descriptors = new();

public static ж<Type> synthType(System.Type? st, nint[]? arrayDims, nint[]?[]? funcParamDims) {
    return synthType(st, arrayDims, funcParamDims, GoChanDir.Unstamped);
}

public static ж<Type> synthType(System.Type? st, nint[]? arrayDims, nint[]?[]? funcParamDims, GoChanDir chanDir) {
    return synthType(st, arrayDims, funcParamDims, chanDir, null);
}

// The SCALAR form is every pre-2b caller's entry and stays exactly as wide as it was: one stamped
// direction describes one channel. It lifts to the one-element chain, which normalization then
// renders to the identical key the scalar era produced — that equality is the whole backward-
// compatibility claim, and it is measured by the guard rows rather than asserted here.
public static ж<Type> synthType(System.Type? st, nint[]? arrayDims, nint[]?[]? funcParamDims, GoChanDir chanDir, nint[]? keyDims) {
    return synthType(st, arrayDims, funcParamDims, chanDir == GoChanDir.Unstamped ? null : new[] { chanDir }, keyDims);
}

// normalizeChanDirChain is the ONE authority for a direction chain's canonical spelling, and it
// GENERALIZES the Both → Unstamped fold rather than replacing it. The rule, in three clauses:
//
//   - TRAILING "nothing narrowed" entries are trimmed. Both and Unstamped are the same claim (a
//     channel nothing narrowed IS bidirectional), so a chain ending in either says nothing its
//     absence would not say. This is the clause that keeps `chan<- chan T` — [Send, Both] — keyed
//     exactly as the scalar era's [Send], so no existing descriptor re-interns.
//   - INTERIOR entries are KEPT even when Both, because there they are load-bearing: `chan (<-chan
//     T)` is [Both, Recv], and dropping the head would spell `<-chan chan T`, a different Go type.
//     A chain is positional; only its tail is free.
//   - An ALL-Both chain normalizes to ABSENT, which is the scalar fold verbatim: it is what makes
//     reflect.ChanOf(BothDir, T) and a value-derived `chan T` land on ONE descriptor.
//
// That last clause is not hypothetical. ChanOf(BothDir, T) and MakeChan both stamped Both while
// every value-derived bidirectional channel read Unstamped, so checkSameType(ChanOf(BothDir, T1),
// (chan T1)(nil)) failed on descriptor identity before any direction semantics were in play
// (TestChanOf's first assertion). Normalizing at the stamp SITES instead would re-open the split
// with every new site — the token-class lesson: one authority, and this is it.
private static GoChanDir[]? normalizeChanDirChain(GoChanDir[]? chain) {
    if (chain is null) {
        return null;
    }
    int end = chain.Length;
    while (end > 0 && (chain[end - 1] == GoChanDir.Both || chain[end - 1] == GoChanDir.Unstamped)) {
        end--;
    }
    return end == 0 ? null : end == chain.Length ? chain : chain[..end];
}

public static ж<Type> synthType(System.Type? st, nint[]? arrayDims, nint[]?[]? funcParamDims, GoChanDir[]? chanDirChain, nint[]? keyDims) {
    if (st is null) {
        return default!;
    }
    // A DEFINED type's cargo is TYPE-level (increment E3 follow-ups 7e-b / 7g): the converter stamps the
    // direction of `type R <-chan T` and the pointee's dims of `type P *[N]T` on the wrapper itself, beside
    // the [GoType] marker that cannot spell either, and every route -- a value, a slot, Elem() -- takes it
    // AHEAD of the key, so a nil and a live value of one named type intern to ONE descriptor (TestConvert's
    // matrix held MyBytesArrayPtr0 twice). The STAMP DECIDES: a defined type's direction and its pointee's
    // length are fixed by its declaration, so cargo a caller measured must agree with it, and cargo that
    // disagrees is refused by name rather than averaged (COORD's condition at 27c307e3d).
    if (GoReflect.TypeStampedChanDirChain(st) is { } stampedChain) {
        GoChanDir[]? carried = normalizeChanDirChain(chanDirChain);
        if (carried is not null && !carried.AsSpan().SequenceEqual(normalizeChanDirChain(stampedChain)!)) {
            throw panic("reflect: descriptor cargo for " + GoReflect.GoTypeName(st) + " disagrees with its stamp: direction chain [" +
                        string.Join(",", carried) + "] vs stamped [" + string.Join(",", stampedChain) + "]");
        }
        chanDirChain = stampedChain;
    }
    chanDirChain = normalizeChanDirChain(chanDirChain);
    string dimsKey = descriptorDimsKey(arrayDims, funcParamDims, chanDirChain, keyDims);
    return s_descriptors.GetOrAdd((st, dimsKey), _ => synthesizeDescriptor(st, arrayDims, funcParamDims, chanDirChain, keyDims));
}

// descriptorDimsKey renders the descriptor's dims cargo as the interning key's second component —
// shared with reflect's canonType so a Type wrapper and the descriptor it wraps are interned under
// the same knowledge classes.
public static string descriptorDimsKey(nint[]? arrayDims, nint[]?[]? funcParamDims) {
    return descriptorDimsKey(arrayDims, funcParamDims, GoChanDir.Unstamped);
}

// The channel DIRECTION joins the key for exactly the reason the array and func-param dims are in
// it: `chan<- int` and `chan int` are DISTINCT Go types over ONE managed channel<int>, so interning
// them together would let whichever arrived first answer ChanDir() and String() for both.
public static string descriptorDimsKey(nint[]? arrayDims, nint[]?[]? funcParamDims, GoChanDir chanDir) {
    return descriptorDimsKey(arrayDims, funcParamDims, chanDir, null);
}

// The map KEY's dims join the key for the third time and the third reason, all the same reason:
// `map[[2]string]V` and `map[[3]string]V` are DISTINCT Go types over ONE managed map<array<@string>,
// V>, so interning them together would let whichever arrived first answer Key().Len() for both.
public static string descriptorDimsKey(nint[]? arrayDims, nint[]?[]? funcParamDims, GoChanDir chanDir, nint[]? keyDims) {
    return descriptorDimsKey(arrayDims, funcParamDims,
        chanDir == GoChanDir.Unstamped ? null : new[] { chanDir }, keyDims);
}

// The chain renders "@" followed by its entries comma-joined, which makes a ONE-element chain
// spell exactly what the scalar spelled — "@2" for Send — so every descriptor the corpus already
// interned keys to the same string and nothing re-interns. A nested chain extends rightward
// ("@3,2" is `chan (<-chan T)`), and normalization guarantees the last entry is never a bare Both,
// so no two spellings of one Go type can reach this function.
public static string descriptorDimsKey(nint[]? arrayDims, nint[]?[]? funcParamDims, GoChanDir[]? chanDirChain, nint[]? keyDims) {
    // Normalized HERE as well as in synthType, and the distinction matters: the RULE has one
    // definition (normalizeChanDirChain) and is applied at both entry points, which is not the
    // per-site normalization the token-class lesson forbids. synthType must normalize what it
    // STORES, because Elem() and ChanDir() read the stored chain; this function is PUBLIC and is
    // called directly by reflect's canonType, so a caller holding a raw chain would otherwise
    // render a key that splits one Go type across two descriptors. Idempotent, and free for the
    // null chain every non-channel descriptor carries.
    chanDirChain = normalizeChanDirChain(chanDirChain);
    string key = arrayDims is null ? "" : string.Join(',', arrayDims);
    if (chanDirChain is { Length: > 0 }) {
        key += "@" + string.Join(',', System.Array.ConvertAll(chanDirChain, static d => ((byte)d).ToString()));
    }
    if (keyDims is not null) {
        key += "#" + string.Join(',', keyDims);
    }
    if (funcParamDims is null) {
        return key;
    }
    var builder = new System.Text.StringBuilder(key);
    foreach (nint[]? paramDims in funcParamDims) {
        builder.Append('|');
        if (paramDims is not null) {
            builder.Append(string.Join(',', paramDims));
        }
    }
    return builder.ToString();
}

// The single builder — synthType is its only caller, and it always has every cargo slot in hand.
// (The shorter private forwarders this replaced went dead when keyDims joined the chain.)
private static ж<Type> synthesizeDescriptor(System.Type st, nint[]? arrayDims, nint[]?[]? funcParamDims, GoChanDir[]? chanDirChain, nint[]? keyDims) {
    ref var t = ref heap<Type>(out var Ꮡt);
    t.Kind_ = (ΔKind)((uint8)GoReflect.KindOf(st));
    // TFlagNamed — the descriptor bit that says "this is a DEFINED type", carried because three
    // separate readers consult it and a synthesized descriptor answered NO to all of them:
    // abi.Type.HasName(), internal/reflectlite's rtype.Name() (which gates on it and therefore
    // answered "" for EVERY type), and — the reason this had to land before Go's assignability
    // rule could — reflect/reflectlite's directlyAssignable, whose FIRST gate is
    // `T.HasName() && V.HasName()`. Go's rule admits two types with identical underlying types
    // only when at least one side is UNDEFINED; with the bit never set, that gate passed for
    // every pair and would have called two DISTINCT named types over one underlying type
    // assignable, which Go rejects.
    //
    // GoReflect.HasGoName is the managed reconstruction of the bit (it mirrors GoTypeName arm for
    // arm, so a type reports a name exactly when it has one), and it is the SAME gate reflect's
    // own hand-owned rtype.Name() already stood on — so the descriptor bit and the name a Type
    // reports cannot disagree. Populating it honors the r39d rule rather than bending it: this is
    // a field whose read CAN be honored, and leaving it zero was the untruth.
    //
    // The rest of TFlag stays zero, deliberately. TFlagUncommon would promise a uncommonType
    // sub-record behind the descriptor (there is none); TFlagExtraStar promises a name blob whose
    // first byte is a '*' to strip; TFlagRegularMemory/TFlagUnrolledBitmap describe a GC bitmap
    // layout the managed heap does not have. Each is a read this bridge cannot honor.
    if (GoReflect.HasGoName(st)) {
        t.TFlag |= (TFlag)TFlagNamed;
    }
    t.sysType = st;
    t.arrayDims = arrayDims;
    t.funcParamDims = funcParamDims;
    t.chanDirChain = chanDirChain;
    t.keyDims = keyDims;
    // Derivability is the question, not the sign: a Go size of 2^63 and up came back -1 from
    // the signed form and left Size_ UNSTAMPED on a type whose size was known exactly.
    if (GoReflect.TryGoSizeOf(st, arrayDims, out nuint size)) {
        t.Size_ = (uintptr)size;
        nint align = GoReflect.GoAlignOf(st);
        t.Align_ = (uint8)align;
        t.FieldAlign_ = (uint8)align;
    }
    // PtrBytes is the pointer-bearing PREFIX of the type, and it is what Pointers() reports. Leaving
    // it unstamped made every synthesized descriptor answer "I hold no pointers", which is a claim
    // and not an absence: reflect's addTypeBits builds a frame's pointer bitmap from each parameter's
    // Kind() but returns immediately unless Pointers() is true, so funcLayout's stack bitmap came out
    // empty, and the frame type's own PtrBytes — which funcLayout derives FROM that bitmap — was then
    // zero, emptying the GC bitmap as well. Stamped only when KNOWN: GoPtrBytesOf reports -1 for the
    // same unknowable-array case GoSizeOf does, and a guess here would be worse than the honest zero,
    // because three sites read the VALUE and not merely its nil-ness (reflect/type.cs's ptrs
    // divisions and typeptrdata's field walk).
    nint ptrBytes = GoReflect.GoPtrBytesOf(st, arrayDims);
    if (ptrBytes >= 0) {
        t.PtrBytes = (uintptr)(nuint)ptrBytes;
    }
    // KindDirectIface — the descriptor bit that says "an interface holding this type stores the
    // value ITSELF in the data word", which is Go's isDirectIface rule: pointer-shaped types
    // (pointer, chan, map, func, unsafe.Pointer) and the one-element array / one-field struct that
    // reduces to one. synthType never stamped it, so every synthesized descriptor answered
    // IfaceIndir() == true unconditionally.
    //
    // MEASURED LATENT, not shipped-wrong (reflect, 2026-09-01): stamping it moves the reflect row
    // count by ZERO — no row fixed, none broken — and the reason is structural rather than lucky.
    // The widest reader, abi.cs:174's addRcvr, tests `IfaceIndir() || Pointers()`, and every type
    // this bit turns on is pointer-shaped, so PtrBytes != 0 already made that disjunction true;
    // the remaining readers (packEface, copyVal, Select, storeRcvr) are not reached with a
    // correctly-classified direct-iface type on this suite. The zero is a real measurement and not
    // a change that failed to compile: the positive control — stamping UNCONDITIONALLY — breaks
    // exactly one row (TestFuncLayout/uintptr.func(uintptr)), which is the misclassification of a
    // non-pointer-shaped type reaching funcLayout. So the readers are live; the honest
    // classification simply agrees with what they already concluded.
    //
    // Kind() masks with KindMask (31), so bit 5 cannot disturb the kind it sits beside.
    if (GoReflect.GoIsDirectIface(st, arrayDims)) {
        t.Kind_ |= (ΔKind)KindDirectIface;
    }
    // Carry Go comparability on the descriptor: reflect.Type.Comparable and internal/reflectlite's
    // Comparable both report `Equal != nil`, and errors.Is gates its equality match on the latter — so a
    // comparable Go type (e.g. the *errorString behind a sentinel like csv.ErrFieldCount) must have a
    // non-nil Equal or errors.Is(err, sentinel) silently returns false. A synthetic descriptor carries no
    // addressable value memory, so this is a comparability signal, not a real bit-compare; the delegate
    // compares its pointer arguments as a safe, non-throwing fallback should any path invoke it directly.
    if (GoReflect.IsComparable(st)) {
        t.Equal = static (p, q) => AreEqual(p, q);
    }
    return Ꮡt;
}

// TypeOf returns the abi.Type of some value. The descriptor stands for the value's GO dynamic
// type: an interface-carrier wrapper (IжAdapter / IInterfaceAdapter chain) unwraps to the *T box
// / original value it stands for (GoReflect.GoDynamicTypeOf, R10), so adapter-held and raw-box
// values of one Go type share one canonical descriptor. A live array value reveals its real
// dims, carried on the descriptor as non-identity cargo (increment 2) — and a live FUNC value
// reveals its parameters' dims the same way, off the [GoArrayDims] the converter stamped, which is
// the only route by which a `[32]byte` PARAMETER's length reaches reflect at all.
public static ж<Type> TypeOf(any a) {
    if (a == default!) {
        return default!;
    }
    System.Type dyn = GoReflect.GoDynamicTypeOf(a);
    nint kind = GoReflect.KindOf(dyn);
    // A POINTER carries its POINTEE's dims unshifted, which is the rule Elem() already applies when
    // it hands the cargo down — so `*[3]int` must be measured here or `Elem()` describes a
    // dimension-less array and reflect.New allocates a zero-length one. See PointeeArrayDims.
    nint[]? dims = kind == GoReflect.Array ? GoReflect.ArrayDimsOfValue(a)
                 : kind == GoReflect.Pointer ? GoReflect.PointeeArrayDims(a)
                 : kind == GoReflect.Slice ? GoReflect.SliceElemArrayDims(a)
                 : kind == GoReflect.Map ? GoReflect.MapElemArrayDims(a)
                 : null;
    nint[]? keyDims = kind == GoReflect.Map ? GoReflect.MapKeyArrayDims(a) : null;
    nint[]?[]? paramDims = kind == GoReflect.Func ? GoReflect.FuncParamDims(a) : null;
    // A CHANNEL value carries the direction of the type it was made with, and a POINTER carries its
    // pointee's unshifted — the same two positions the array dims occupy, for the same reason.
    // Increment D: the VALUE route reads the unified cargo off the channel (or the channel behind a
    // pointer): the direction CHAIN rather than the scalar head, and the element's array dims, which
    // a channel has no present element to measure and so can only carry.
    ChanCargo? chanCargo = kind == GoReflect.Chan ? GoReflect.ChanCargoOfValue(a)
                         : kind == GoReflect.Pointer ? GoReflect.PointeeChanCargo(a)
                         : null;
    dims ??= chanCargo?.ElemDims;
    return synthType(dyn, dims, paramDims, chanCargo?.DirChain, keyDims);
}

// ==== the descriptor SPECIALIZATIONS: StructType() / ArrayType() ====
//
// Go's `(*structType)(unsafe.Pointer(t))` is the prefix-downcast idiom: the linker really did
// allocate a structType and hand out a pointer to its embedded Type header, so casting back
// reaches the sub-record. There is nothing behind a ж<abi.Type> to downcast to — the box holds an
// abi.Type and only an abi.Type — and golib's Reinterpret correctly REFUSES to alias managed
// storage for a reference-bearing pair (it would fabricate object references), so the auto form
// fell back to the raw-address route and read the specialization's fields out of whatever memory
// follows the value slot. Measured: `Fields` came back with Length 8830452760576 over a fabricated
// StructField[] reference — an IndexOutOfRangeException on the FIRST iteration of
// unique.buildStructCloneSeq, and internal/reflectlite's NumField/Len read the same garbage.
//
// The specializations are therefore SYNTHESIZED from the descriptor's carried System.Type, exactly
// as the descriptor itself is, over the same golib layout machinery (GoReflect.GoFields /
// GoSizeOf / GoAlignOf) that stamps Size_/Align_ — so a field's Offset and the descriptor's Size_
// can never disagree. What is NOT knowable is not invented: a descriptor with no System.Type, or a
// struct holding a field whose Go size cannot be computed, answers Go's nil rather than a
// plausible-looking record (the r39d rule — a descriptor field whose read cannot be honored must
// not be populated to look truthful), and every Go caller already tests that nil.
//
// StructField.Name and StructType.PkgPath stay the ZERO ΔName on purpose. A ΔName is a pointer
// into the linker's name blob, and every reader of one (Name/IsExported/IsEmbedded/ReadVarint)
// walks it with `addChecked` raw-address arithmetic — the same route that produced the garbage
// above. Go's own ΔName.Name() answers "" for a nil Bytes, so the zero value is a state the
// format DEFINES, not a fabrication; a synthesized encoding would instead hand every reader an
// address whose arithmetic has never been exercised. reflect's own hand-owned rtype.Field is
// where a named field descriptor already comes from (over GoReflect.GoFields), and no converted
// caller of abi.StructType reads a field name: unique reads Typ and Offset, internal/reflectlite
// reads len(Fields).

private static readonly System.Collections.Concurrent.ConcurrentDictionary<ж<Type>, ж<ΔStructType>> s_structTypes = new();
private static readonly System.Collections.Concurrent.ConcurrentDictionary<ж<Type>, ж<ΔArrayType>> s_arrayTypes = new();

// StructType returns t cast to a *StructType, or nil if its tag does not match.
public static ж<ΔStructType> StructType(this ж<Type> Ꮡt) {
    if (Ꮡt == nil || Ꮡt.Value.Kind() != Struct || Ꮡt.Value.sysType is null) {
        return default!;
    }
    return s_structTypes.GetOrAdd(Ꮡt, static box => synthesizeStructType(box));
}

private static ж<ΔStructType> synthesizeStructType(ж<Type> Ꮡt) {
    System.Type st = Ꮡt.Value.sysType!;
    GoReflect.GoFieldInfo[] infos = GoReflect.GoFields(st);
    nint[]? offsets = GoReflect.GoFieldOffsets(st);

    // A struct holding a field whose Go size is unknowable has no truthful layout at all — one
    // unknown size makes every later offset a guess. Answer Go's nil rather than a plausible record.
    if (offsets is null) {
        return default!;
    }
    StructField[] fields = new StructField[infos.Length];

    for (int i = 0; i < infos.Length; i++) {
        GoReflect.GoFieldInfo info = infos[i];
        nint fieldKind = GoReflect.KindOf(info.Type);
        // An ARRAY field's dims are its own; a POINTER's and a MAP's are what their Elem() hands
        // down, carried on the same slot and stamped by the converter because no zero instance can
        // measure a nil pointee or an absent map entry.
        nint[]? dims = fieldKind == GoReflect.Array || fieldKind == GoReflect.Pointer || fieldKind == GoReflect.Map ? info.ArrayDims : null;
        nint[]? fieldKeyDims = fieldKind == GoReflect.Map || fieldKind == GoReflect.Pointer ? info.KeyDims : null;
        // Increment D: a channel field's cargo carries its chain and its element dims; the scalar
        // direction is that chain's head and stays for every reader that asks for it.
        GoChanDir[]? fieldChain = fieldKind == GoReflect.Chan ? info.ChanCargo?.DirChain : null;
        if (fieldKind == GoReflect.Chan && dims is null) {
            dims = info.ChanCargo?.ElemDims;
        }
        // The DESCRIPTOR CARRIER, when the converter stamped one: this field's Go type is a
        // DEFINED type over a named interface, which the emission erased to a `using` alias, so
        // info.Type is the bare `object`/target interface and carries no Go name. Substituting the
        // carrier changes only what the DESCRIPTOR reports — the field's storage, offset and Kind
        // are untouched (a carrier is an interface, exactly as the erased type is), and the offsets
        // this loop pairs with come from GoFieldOffsets over the real managed type either way.
        System.Type fieldDescriptorType = info.DescriptorSelf ?? info.Type;
        fields[i] = new StructField(
            Name: default!,
            Typ: synthType(fieldDescriptorType, dims, null, fieldChain, fieldKeyDims),
            Offset: (uintptr)(nuint)offsets[i]
        );
    }

    return new StandardBox<ΔStructType>(new ΔStructType(
        Type: Ꮡt.Value,
        PkgPath: default!,
        Fields: new slice<StructField>(fields)
    ));
}

// ArrayType returns t cast to a *ArrayType, or nil if its tag does not match.
public static ж<ΔArrayType> ArrayType(this ж<Type> Ꮡt) {
    if (Ꮡt == nil || Ꮡt.Value.Kind() != Array || Ꮡt.Value.sysType is null) {
        return default!;
    }
    return s_arrayTypes.GetOrAdd(Ꮡt, static box => synthesizeArrayType(box));
}

// ==== the FUNC specialization — the same operation, and the same answer ==========================
//
// FuncType is the third member of the prefix-downcast family StructType and ArrayType above already
// answer, and it failed the same way: the auto body tag-checks correctly (`Kind() != Func → nil`,
// Go's own "or nil if its tag does not match") and then reaches
// `Reinterpret<Type, ΔFuncType>`, which golib rightly refuses — ΔFuncType is larger than Type and
// carries managed references, so aliasing the header's storage as the wider record would fabricate
// references. The refusal is correct; what it PRODUCES is a nil one frame up, where reflect's
// funcLayout renames it `funcLayout of non-func type` for a type that is perfectly good.
//
// Measured consumer: reflect's TestFuncLayout, whose export_test reaches funcLayout through exactly
// that downcast. 10 comparison rows on this host.
//
// InSlice/OutSlice had to come with it, and they are the reason fixing the accessor alone is not
// enough. Go stores a func's parameter and result descriptors in the memory IMMEDIATELY AFTER the
// FuncType record — the auto bodies walk there by pointer arithmetic (`unsafe.Sizeof(*t)`, plus 16
// when TFlagUncommon is set) and build a span of `*Type` over it. That layout is the linker's, and
// the managed model has no equivalent; worse, the span's element type is `ж<Type>`, a MANAGED
// reference, so reading it over a fabricated address is the same class of type-safety break the
// descriptor downcast itself was refused for.
//
// Both are synthesized instead, from the descriptor's carried System.Type over GoReflect.TryFuncShape
// — the SAME projection reflect's own hand-owned rtype.NumIn/In/NumOut/Out use one layer up
// (value_impl.cs), so the descriptor layer and the reflect layer cannot disagree about a func's
// shape. That is the property the Elem/Key hand-owns above were written for, applied to funcs.
private static readonly System.Collections.Concurrent.ConcurrentDictionary<ж<Type>, ж<ΔFuncType>> s_funcTypes = new();

// FuncType returns t cast to a *FuncType, or nil if its tag does not match.
public static ж<ΔFuncType> FuncType(this ж<Type> Ꮡt) {
    if (Ꮡt == nil || Ꮡt.Value.Kind() != Func || Ꮡt.Value.sysType is null) {
        return default!;
    }
    return s_funcTypes.GetOrAdd(Ꮡt, static box => synthesizeFuncType(box));
}

private static ж<ΔFuncType> synthesizeFuncType(ж<Type> Ꮡt) {
    if (!GoReflect.TryFuncShape(Ꮡt.Value.sysType!, out System.Type[]? ins, out System.Type[]? outs, out bool isVariadic)) {
        return default!;
    }

    // Go packs the variadic flag into OutCount's top bit (abi/type.go: "top bit is set if last input
    // parameter is ..."), and NumOut masks it back off. Carrying the bit rather than a separate
    // field is what lets IsVariadic/NumOut stay the auto conversions they already are.
    uint16 outCount = (uint16)outs.Length;

    if (isVariadic) {
        outCount |= 1 << 15;
    }
    return new StandardBox<ΔFuncType>(new ΔFuncType(
        Type: Ꮡt.Value,
        InCount: (uint16)ins.Length,
        OutCount: outCount
    ));
}

// The parameter descriptors, derived rather than walked. Per-parameter array dims come from the
// descriptor's own funcParamDims cargo, which exists for exactly this: an array parameter's LENGTH
// is not recoverable from `array<T>` alone.
private static slice<ж<Type>> synthesizeFuncSide(ж<ΔFuncType> Ꮡt, bool wantIns) {
    if (Ꮡt == nil) {
        return default!;
    }
    System.Type? st = Ꮡt.Value.Type.sysType;

    if (st is null || !GoReflect.TryFuncShape(st, out System.Type[]? ins, out System.Type[]? outs, out _)) {
        return default!;
    }
    System.Type[] side = wantIns ? ins : outs;

    if (side.Length == 0) {
        return default!;
    }
    nint[]?[]? dims = Ꮡt.Value.Type.funcParamDims;
    var descriptors = new ж<Type>[side.Length];

    for (int i = 0; i < side.Length; i++) {
        // funcParamDims indexes the INPUTS; results carry no dims cargo today, which is a known
        // narrowing rather than an oversight — an array RESULT's length is the same gap one step
        // further out, and nothing measured reaches it yet.
        nint[]? paramDims = wantIns && dims is not null && i < dims.Length ? dims[i] : null;
        descriptors[i] = synthType(side[i], paramDims);
    }
    return new slice<ж<Type>>(descriptors);
}

public static slice<ж<Type>> InSlice(this ж<ΔFuncType> Ꮡt) {
    return synthesizeFuncSide(Ꮡt, wantIns: true);
}

public static slice<ж<Type>> OutSlice(this ж<ΔFuncType> Ꮡt) {
    return synthesizeFuncSide(Ꮡt, wantIns: false);
}

// ==== the descriptor ACCESSORS that reach an element or a key: Elem() / Key() ====
//
// The SAME prefix-downcast idiom as the specializations above, one level in: Go's Elem() casts the
// Type header to the sliceType/arrayType/chanType/mapType/ptrType the linker really allocated
// behind it and reads that record's Elem field, and Key() does it for a mapType. So both inherited
// exactly the defect documented above — Reinterpret rightly refuses to alias managed storage for a
// reference-bearing pair — and answered nil for every slice, array, chan, map and pointer
// descriptor in the corpus.
//
// Nil is NOT a state Go's callers test for here, which is what made it fatal rather than merely
// wrong: reflect's haveIdenticalType recurses straight into nameFor(t), which reads the
// descriptor's carried System.Type and nil-dereferences. That is the whole of
// ConvertibleTo/AssignableTo for any operand that is not a scalar — measured as database/sql's
// TestConversions and TestUserDefinedBytes, and it gates every such recursion corpus-wide.
//
// Synthesized from the carried System.Type over the SAME golib element/key resolution that
// reflect's own hand-owned rtype.Elem / rtype.Key use one layer up, so the descriptor layer and
// the reflect layer cannot disagree about what an element type is. Nothing that is not knowable is
// invented (the r39d rule): a descriptor with no System.Type, or a managed type with no element,
// answers Go's nil — which is exactly what Go's own Elem() answers for a kind that has none.
public static ж<Type> Elem(this ж<Type> Ꮡt) {
    if (Ꮡt == nil) {
        return default!;
    }
    var kind = Ꮡt.Value.Kind();
    // Go's own switch: the five kinds that carry an element type, and nil for everything else.
    if (kind != Array && kind != Chan && kind != Map && kind != Pointer && kind != Slice) {
        return default!;
    }
    System.Type? elem = GoReflect.ElementType(Ꮡt.Value.sysType);
    if (elem is null) {
        return default!;
    }
    // An ARRAY descriptor's dims read [outer]…[inner], so its element takes the TAIL; a POINTER's
    // and a MAP's dims are the pointee's / the element's already and pass through UNSHIFTED, neither
    // having a length of its own. The same rule rtype.Elem applies to the same cargo one layer up.
    nint[]? dims = Ꮡt.Value.arrayDims;
    // Every ELEMENT-CARGO kind hands its dims down UNSHIFTED (pointer, map, slice, channel: none has a
    // length of its own); only an ARRAY consumes its head. This is the predicate reflect's own Elem()
    // already applies; this site named pointer and map alone, so a slice's or a channel's element
    // dims were shifted off here and lost, invisible until D put dims on a channel.
    nint[]? elemDims = GoReflect.KindCarriesElementCargo((int)kind) ? dims : dims is { Length: > 1 } ? dims[1..] : null;
    // A POINTER's channel-direction cargo is its POINTEE's, so it descends here and nowhere else —
    // this is the hop `new(chan<- string)` takes to reach `Elem().String()`. A CHANNEL's own
    // direction describes the channel, never its element, so it stops. The map KEY dims descend the
    // same one hop, for the same reason: `*map[[2]string]V` holds its pointee's whole type.
    GoChanDir[]? chain = Ꮡt.Value.chanDirChain;
    GoChanDir[]? elemChanDirChain = kind == Pointer ? chain
                                  : kind == Chan && chain is { Length: > 1 } ? chain[1..]
                                  : null;
    nint[]? elemKeyDims = kind == Pointer ? Ꮡt.Value.keyDims : null;
    return synthType(elem, elemDims, null, elemChanDirChain, elemKeyDims);
}

// ChanDir returns the direction of t if t is a channel type, otherwise InvalidDir.
//
// The FOURTH accessor of the same downcast family — `(*chanType)(unsafe.Pointer(t))` reaching the
// record's Dir field — and the one that had no synthesis for longest, because the direction is not
// merely unpopulated: it is not IN the managed type at all. A Go channel type emits as golib's
// `channel<T>` whatever its direction, so `<-chan int`, `chan<- int` and `chan int` are ONE managed
// type and no descriptor built from the TYPE alone can tell them apart.
//
// So the direction is carried as descriptor CARGO the way an array's length is (2026-08-20), from
// whichever source knew it: a live channel VALUE, whose direction the converter seeds at the make
// site; the value behind a POINTER, which is the `new(chan<- string)` position; and a struct
// FIELD's initializer-borne zero. Each is a position no other reaches, exactly as with the dims —
// and the reason a NIL channel must be able to carry a direction at all is that two of the three
// have nothing but a zero value to read.
//
// Unstamped cargo still answers BothDir, and that is not a fallback but the same honest answer this
// accessor gave before the cargo existed: a channel nothing narrowed IS bidirectional.
//
// AMENDED 2026-09-01 — the narrowing exclusion below stood on "no measured consumer asks", and
// that premise DIED: reflect's own suite measured four (TestAll #12, TestTypes #20-22, TestChanOf,
// TestChanOfDir), so the coordinator ruled the narrowing CARRIED and the construction-shaped
// positions (a directional var's zero, a nil cast) joined the converter's stamp set — see
// chanDirectionCargo.go for the amended site list. What the exclusion still covers is only the
// LIVE-COPY narrowing (`var s chan<- int = ch`), which remains a plain struct copy with no
// construction to hook and no measured consumer yet. The original sentence is kept beneath so the
// next reader sees why the rule stood and how it fell rather than meeting a hole:
//   (pre-amendment) What is deliberately NOT carried is a NARROWING conversion — `var s chan<- int
//   = ch` still describes the bidirectional type, because the narrowing has no emission position
//   to stamp and no measured consumer asks (the r39d rule, and the same boundary the func-param
//   dims draw at results).
//
// The read this replaced was the worst kind of wrong: NON-DETERMINISTIC, reinterpreting the
// descriptor onto the linker's chanType record and reading `.Dir` out of the memory after the value
// slot, so reflect.MakeChan's `ChanDir() != BothDir` guard and haveIdenticalUnderlyingType's chan
// arm each answered differently run to run.
public static ΔChanDir ChanDir(this ж<Type> Ꮡt) {
    if (Ꮡt == nil || Ꮡt.Value.Kind() != Chan) {
        return InvalidDir;
    }
    GoChanDir[]? chain = Ꮡt.Value.chanDirChain;
    GoChanDir carried = chain is { Length: > 0 } ? chain[0] : GoChanDir.Unstamped;
    return carried == GoChanDir.Unstamped ? BothDir : (ΔChanDir)(nint)(byte)carried;
}

// Key returns the key type for t if t is a map, otherwise nil.
//
// The key's own array dims ride the descriptor's keyDims cargo, which is what makes
// `map[[2]string]V`'s key a `[2]string` rather than a dimension-less one: `map<array<@string>, V>`
// is ONE managed type for every key length, so the datum reaches here from the converter's stamp on
// the declaring FIELD — the position a decode target reads, where the map itself is still nil and a
// live entry could reveal nothing. Unstamped answers Go's dimension-less array, unchanged.
public static ж<Type> Key(this ж<Type> Ꮡt) {
    if (Ꮡt == nil || Ꮡt.Value.Kind() != Map) {
        return default!;
    }
    System.Type? key = GoReflect.KeyType(Ꮡt.Value.sysType);
    return key is null ? default! : synthType(key, Ꮡt.Value.keyDims);
}

// Len returns the length of t if t is an array type, otherwise 0 — the descriptor's carried dims,
// which is exactly what reflect's own rtype.Len reads one layer up.
//
// The third accessor of the same recursion, and the one whose downcast failed WORST: Elem()/Key()
// answered a clean nil, but Len() read a uintptr out of the memory following the descriptor's value
// slot, so two array descriptors read two DIFFERENT pieces of garbage and haveIdenticalUnderlyingType
// reported [3]byte and [3]byte as different types. A length the descriptor does not know is still
// answered as Go's 0 — array dims are non-identity cargo (the recorded §5 limitation), so two
// dimension-less array descriptors compare equal rather than randomly unequal.
public static nint Len(this ж<Type> Ꮡt) {
    if (Ꮡt == nil || Ꮡt.Value.Kind() != Array) {
        return 0;
    }
    return Ꮡt.Value.arrayDims is { Length: > 0 } dims ? dims[0] : 0;
}

private static ж<ΔArrayType> synthesizeArrayType(ж<Type> Ꮡt) {
    System.Type at = Ꮡt.Value.sysType!;
    System.Type? elem = GoReflect.ElementType(at);
    nint[]? dims = Ꮡt.Value.arrayDims;

    // array<T> carries its element type but not its LENGTH — a descriptor built without dims
    // cannot answer Len, and guessing 0 would read as a real empty array.
    if (elem is null || dims is not { Length: > 0 }) {
        return default!;
    }
    nint[]? elemDims = dims.Length > 1 ? dims[1..] : null;

    return new StandardBox<ΔArrayType>(new ΔArrayType(
        Type: Ꮡt.Value,
        Elem: synthType(elem, elemDims),
        Slice: synthType(typeof(slice<>).MakeGenericType(elem)),
        Len: (uintptr)(nuint)dims[0]
    ));
}

// Whether this descriptor stands for a STACK FRAME LAYOUT rather than for a Go type — the
// System.Type-less kind the descriptor contract admits as first-class
// (DESIGN-descriptor-contract.md §3, amended 2026-08-31).
//
// reflect.funcLayout MINTS such a descriptor: Go's own source sets Align_, Size_ and PtrBytes and
// nothing else, deliberately, because "the returned type exists only for GC". A frame is not a Go
// type, so there is no System.Type for the box to carry and no synthType path that could stamp one
// — the descriptor is outside the "carries a System.Type plus side cargo" premise, not an edge of it.
//
// The test is on the KIND, never on the absence of a System.Type. That distinction is the whole
// point: a descriptor that BYPASSED synthType still names a real Go type and so still reports a
// real Kind, and must keep tripping canonType's assert; only a descriptor naming NO Go kind at all
// is admitted here. Go's Kind_ is left zero by the mint and Kind() masks with KindMask, so Invalid
// is exactly "this descriptor names no Go kind". It also fails CLOSED: were a future Go release to
// start stamping a kind on the frame type, this predicate would stop recognizing it and the assert
// would fire — a loud stop, not a silent admission.
//
// Deliberately NOT a new field on Type. ReinterpretAliasesStorage keys its alias decision on
// Unsafe.SizeOf<T>(), so widening abi.Type would move that gate for every pair involving it —
// a corpus-wide blast radius for a marker, when Go's own encoding already carries the fact.
// The kind is the SOLE discriminator here on purpose: adding "and it has no System.Type" would
// make this a conjunction containing the absence test, and a reader could then reasonably carry
// the absence half forward as the operative one. Callers establish the absence; this answers only
// "does this descriptor name a Go kind at all".
public static bool IsFrameLayoutDescriptor(this ж<Type> Ꮡt) {
    return Ꮡt != nil && Ꮡt.Value.Kind() == Invalid;
}

} // end abi_package
