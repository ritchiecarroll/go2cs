// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using maps = maps_package;
using os = os_package;
using reflect = reflect_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.sync;
using go.unicode;
using io = io_package;

partial class gob_package {

// userTypeInfo stores the information associated with a type the user has handed
// to the package. It's computed once and stored in a map keyed by reflection
// type.
partial struct userTypeInfo {
    internal reflectꓸType user; // the type the user handed us
    internal reflectꓸType @base; // the base type after all indirections
    internal nint indir;         // number of indirections to reach the base type
    internal nint externalEnc;         // xGob, xBinary, or xText
    internal nint externalDec;         // xGob, xBinary, or xText
    internal int8 encIndir;         // number of indirections to reach the receiver type; may be negative
    internal int8 decIndir;         // number of indirections to reach the receiver type; may be negative
}

// externalEncoding bits
internal static UntypedInt xGob => /* 1 + iota */ 1; // GobEncoder or GobDecoder

internal static UntypedInt xBinary => 2; // encoding.BinaryMarshaler or encoding.BinaryUnmarshaler

internal static UntypedInt xText => 3; // encoding.TextMarshaler or encoding.TextUnmarshaler

internal static ж<sync.Map> ᏑuserTypeCache = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map userTypeCache => ref ᏑuserTypeCache.Value; // map[reflect.Type]*userTypeInfo

// validUserType returns, and saves, the information associated with user-provided type rt.
// If the user type is not valid, err will be non-nil. To be used when the error handler
// is not set up.
internal static (ж<userTypeInfo>, error) validUserType(reflectꓸType rt) {
    {
        var (uiΔ1, ok) = ᏑuserTypeCache.Load(rt); if (ok) {
            return (uiΔ1._<ж<userTypeInfo>>(), default!);
        }
    }
    // Construct a new userTypeInfo and atomically add it to the userTypeCache.
    // If we lose the race, we'll waste a little CPU and create a little garbage
    // but return the existing value anyway.
    var ut = @new<userTypeInfo>();
    ut.Value.@base = rt;
    ut.Value.user = rt;
    // A type that is just a cycle of pointers (such as type T *T) cannot
    // be represented in gobs, which need some concrete data. We use a
    // cycle detection algorithm from Knuth, Vol 2, Section 3.1, Ex 6,
    // pp 539-540.  As we step through indirections, run another type at
    // half speed. If they meet up, there's a cycle.
    var slowpoke = ut.Value.@base; // walks half as fast as ut.base
    while (ᐧ) {
        var pt = ut.Value.@base;
        if (pt.Kind() != reflect.ΔPointer) {
            break;
        }
        ut.Value.@base = pt.Elem();
        if (AreEqual((~ut).@base, slowpoke)) {
            // ut.base lapped slowpoke
            // recursive pointer type.
            return (default!, errors.New("can't represent recursive pointer type "u8 + (~ut).@base.String()));
        }
        if ((~ut).indir % 2 == 0) {
            slowpoke = slowpoke.Elem();
        }
        ut.Value.indir++;
    }
    {
        var (ok, indir) = implementsInterface((~ut).user, gobEncoderInterfaceType); if (ok){
            (ut.Value.externalEnc, ut.Value.encIndir) = (xGob, indir);
        } else 
        {
            var (okΔ1, indirΔ1) = implementsInterface((~ut).user, binaryMarshalerInterfaceType); if (okΔ1) {
                (ut.Value.externalEnc, ut.Value.encIndir) = (xBinary, indirΔ1);
            }
        }
    }
    // NOTE(rsc): Would like to allow MarshalText here, but results in incompatibility
    // with older encodings for net.IP. See golang.org/issue/6760.
    // } else if ok, indir := implementsInterface(ut.user, textMarshalerInterfaceType); ok {
    // 	ut.externalEnc, ut.encIndir = xText, indir
    // }
    {
        var (ok, indir) = implementsInterface((~ut).user, gobDecoderInterfaceType); if (ok){
            (ut.Value.externalDec, ut.Value.decIndir) = (xGob, indir);
        } else 
        {
            var (okΔ1, indirΔ1) = implementsInterface((~ut).user, binaryUnmarshalerInterfaceType); if (okΔ1) {
                (ut.Value.externalDec, ut.Value.decIndir) = (xBinary, indirΔ1);
            }
        }
    }
    // See note above.
    // } else if ok, indir := implementsInterface(ut.user, textUnmarshalerInterfaceType); ok {
    // 	ut.externalDec, ut.decIndir = xText, indir
    // }
    var (ui, _) = ᏑuserTypeCache.LoadOrStore(rt, ut.OrTypedNil());
    return (ui._<ж<userTypeInfo>>(), default!);
}

internal static reflectꓸType gobEncoderInterfaceType = reflect.TypeFor<GobEncoder>();
internal static reflectꓸType gobDecoderInterfaceType = reflect.TypeFor<GobDecoder>();
internal static reflectꓸType binaryMarshalerInterfaceType = reflect.TypeFor<encoding.BinaryMarshaler>();
internal static reflectꓸType binaryUnmarshalerInterfaceType = reflect.TypeFor<encoding.BinaryUnmarshaler>();
internal static reflectꓸType textMarshalerInterfaceType = reflect.TypeFor<encoding.TextMarshaler>();
internal static reflectꓸType textUnmarshalerInterfaceType = reflect.TypeFor<encoding.TextUnmarshaler>();
internal static reflectꓸType wireTypeType = reflect.TypeFor<wireType>();

// implementsInterface reports whether the type implements the
// gobEncoder/gobDecoder interface.
// It also returns the number of indirections required to get to the
// implementation.
internal static (bool success, int8 indir) implementsInterface(reflectꓸType typ, reflectꓸType gobEncDecType) {
    bool success = default!;
    int8 indir = default!;

    if (typ == default!) {
        return (success, indir);
    }
    var rt = typ;
    // The type might be a pointer and we need to keep
    // dereferencing to the base type until we find an implementation.
    while (ᐧ) {
        if (rt.Implements(gobEncDecType)) {
            return (true, indir);
        }
        {
            var p = rt; if (p.Kind() == reflect.ΔPointer) {
                indir++;
                if (indir > 100) {
                    // insane number of indirections
                    return (false, 0);
                }
                rt = p.Elem();
                continue;
            }
        }
        break;
    }
    // No luck yet, but if this is a base type (non-pointer), the pointer might satisfy.
    if (typ.Kind() != reflect.ΔPointer) {
        // Not a pointer, but does the pointer work?
        if (reflect.PointerTo(typ).Implements(gobEncDecType)) {
            return (true, (int8)(-1));
        }
    }
    return (false, 0);
}

// userType returns, and saves, the information associated with user-provided type rt.
// If the user type is not valid, it calls error.
internal static ж<userTypeInfo> userType(reflectꓸType rt) {
    var (ut, err) = validUserType(rt);
    if (err != default!) {
        error_(err);
    }
    return ut;
}

public partial struct typeId /*num:int32*/;

internal static ж<sync.Mutex> ᏑtypeLock = new StandardBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex typeLock => ref ᏑtypeLock.Value; // set while building a type

internal static UntypedInt firstUserId => 64; // lowest id number granted to user

partial interface ΔgobType {
    typeId id();
    void setId(typeId id);
    @string name();
    @string @string(); // not public; only for debugging
    @string safeString(map<typeId, bool> seen);
}

internal static map<reflectꓸType, ΔgobType> types = new map<reflectꓸType, ΔgobType>(32);
internal static slice<ΔgobType> idToTypeSlice = new slice<ΔgobType>(1, firstUserId);
internal static array<ΔgobType> builtinIdToTypeSlice = new(64); // set in init() after builtins are established

internal static ΔgobType idToType(typeId id) {
    if (id < 0 || (nint)(int32)id >= len(idToTypeSlice)) {
        return default!;
    }
    return idToTypeSlice[id];
}

internal static ΔgobType builtinIdToType(typeId id) {
    if (id < 0 || (nint)(int32)id >= len(builtinIdToTypeSlice)) {
        return default!;
    }
    return builtinIdToTypeSlice[id];
}

internal static void setTypeId(ΔgobType typ) {
    // When building recursive types, someone may get there before us.
    if (typ.id() != 0) {
        return;
    }
    var nextId = ((typeId)(int32)len(idToTypeSlice));
    typ.setId(nextId);
    idToTypeSlice = append(idToTypeSlice, typ);
}

internal static ΔgobType gobType(this typeId t) {
    if (t == 0) {
        return default!;
    }
    return idToType(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "<nil>"u8;

// string returns the string representation of the type associated with the typeId.
internal static @string @string(this typeId t) {
    if (t.gobType() == default!) {
        return nilˢ;
    }
    return t.gobType().@string();
}

// Name returns the name of the type associated with the typeId.
internal static @string name(this typeId t) {
    if (t.gobType() == default!) {
        return nilˢ;
    }
    return t.gobType().name();
}

// CommonType holds elements of all types.
// It is a historical artifact, kept for binary compatibility and exported
// only for the benefit of the package's encoding of type descriptors. It is
// not intended for direct use by clients.
partial struct CommonType {
    public @string Name;
    public typeId Id;
}

internal static typeId id(this ref CommonType t) {
    return t.Id;
}

internal static void setId(this ref CommonType t, typeId id) {
    t.Id = id;
}

internal static @string @string(this ref CommonType t) {
    return t.Name;
}

internal static @string safeString(this ref CommonType t, map<typeId, bool> seen) {
    return t.Name;
}

internal static @string name(this ref CommonType t) {
    return t.Name;
}

// Create and check predefined types
// The string for tBytes is "bytes" not "[]byte" to signify its specialness.
internal static typeId tBool = bootstrapType("bool"u8, ((ж<bool>)nil));
internal static typeId tInt = bootstrapType("int"u8, ((ж<nint>)nil));
internal static typeId tUint = bootstrapType("uint"u8, ((ж<nuint>)nil));
internal static typeId tFloat = bootstrapType("float"u8, ((ж<float64>)nil));
internal static typeId tBytes = bootstrapType("bytes"u8, ((ж<slice<byte>>)nil));
internal static typeId tString = bootstrapType("string"u8, ((ж<@string>)nil));
internal static typeId tComplex = bootstrapType("complex"u8, ((ж<complex128>)nil));
internal static typeId tInterface = bootstrapType("interface"u8, ((ж<any>)nil));

    partial struct eᴛ1 /*dyn*/ {
        internal nint r7;
    }
internal static typeId tReserved7 = bootstrapType("_reserved1"u8, ((ж<eᴛ1>)nil));

    partial struct eᴛ2 /*dyn*/ {
        internal nint r6;
    }
internal static typeId tReserved6 = bootstrapType("_reserved1"u8, ((ж<eᴛ2>)nil));

    partial struct eᴛ3 /*dyn*/ {
        internal nint r5;
    }
internal static typeId tReserved5 = bootstrapType("_reserved1"u8, ((ж<eᴛ3>)nil));

    partial struct eᴛ4 /*dyn*/ {
        internal nint r4;
    }
internal static typeId tReserved4 = bootstrapType("_reserved1"u8, ((ж<eᴛ4>)nil));

    partial struct eᴛ5 /*dyn*/ {
        internal nint r3;
    }
internal static typeId tReserved3 = bootstrapType("_reserved1"u8, ((ж<eᴛ5>)nil));

    partial struct eᴛ6 /*dyn*/ {
        internal nint r2;
    }
internal static typeId tReserved2 = bootstrapType("_reserved1"u8, ((ж<eᴛ6>)nil));

    partial struct eᴛ7 /*dyn*/ {
        internal nint r1;
    }
internal static typeId tReserved1 = bootstrapType("_reserved1"u8, ((ж<eᴛ7>)nil));

// Predefined because it's needed by the Decoder
internal static typeId tWireType;
internal static void initᴛtWireType() { tWireType = (~mustGetTypeInfo(wireTypeType)).id; }

internal static ж<userTypeInfo> wireTypeUserInfo; // userTypeInfo of wireType

[GoInit] internal static void initΔ1() {
    // Some magic numbers to make sure there are no surprises.
    checkId(16, tWireType);
    checkId(17, (~mustGetTypeInfo(reflect.TypeFor<arrayType>())).id);
    checkId(18, (~mustGetTypeInfo(reflect.TypeFor<CommonType>())).id);
    checkId(19, (~mustGetTypeInfo(reflect.TypeFor<sliceType>())).id);
    checkId(20, (~mustGetTypeInfo(reflect.TypeFor<structType>())).id);
    checkId(21, (~mustGetTypeInfo(reflect.TypeFor<fieldType>())).id);
    checkId(23, (~mustGetTypeInfo(reflect.TypeFor<mapType>())).id);
    copy(builtinIdToTypeSlice[..], idToTypeSlice);
    // Move the id space upwards to allow for growth in the predefined world
    // without breaking existing files.
    {
        nint nextId = len(idToTypeSlice); if (nextId > firstUserId) {
            throw panic(fmt.Sprintln((@string)"nextId too large:"u8, nextId));
        }
    }
    idToTypeSlice = idToTypeSlice[..(int)(firstUserId)];
    registerBasics();
    wireTypeUserInfo = userType(wireTypeType);
}

// Array type
public partial struct arrayType {
    public partial ref CommonType CommonType { get; }
    public typeId Elem;
    public nint Len;
}

internal static ж<arrayType> newArrayType(@string name) {
    var a = Ꮡ(new arrayType(new CommonType(Name: name), 0, 0));
    return a;
}

internal static void init(this ж<arrayType> Ꮡa, ΔgobType elem, nint len) {
    ref var a = ref Ꮡa.DerefOrNull();

    // Set our type id before evaluating the element's, in case it's our own.
    setTypeId(new arrayTypeжΔgobType(Ꮡa));
    a.Elem = elem.id();
    a.Len = len;
}

internal static @string safeString(this ref arrayType a, map<typeId, bool> seen) {
    if (seen[a.Id]) {
        return a.Name;
    }
    seen[a.Id] = true;
    return fmt.Sprintf("[%d]%s"u8, a.Len, a.Elem.gobType().safeString(seen));
}

internal static @string @string(this ref arrayType a) {
    return a.safeString(new map<typeId, bool>());
}

// GobEncoder type (something that implements the GobEncoder interface)
public partial struct gobEncoderType {
    public partial ref CommonType CommonType { get; }
}

internal static ж<gobEncoderType> newGobEncoderType(@string name) {
    var g = Ꮡ(new gobEncoderType(new CommonType(Name: name)));
    setTypeId(new gobEncoderTypeжΔgobType(g));
    return g;
}

internal static @string safeString(this ref gobEncoderType g, map<typeId, bool> seen) {
    return g.Name;
}

internal static @string @string(this ref gobEncoderType g) {
    return g.Name;
}

// Map type
public partial struct mapType {
    public partial ref CommonType CommonType { get; }
    public typeId Key;
    public typeId Elem;
}

internal static ж<mapType> newMapType(@string name) {
    var m = Ꮡ(new mapType(new CommonType(Name: name), 0, 0));
    return m;
}

internal static void init(this ж<mapType> Ꮡm, ΔgobType key, ΔgobType elem) {
    ref var m = ref Ꮡm.DerefOrNull();

    // Set our type id before evaluating the element's, in case it's our own.
    setTypeId(new mapTypeжΔgobType(Ꮡm));
    m.Key = key.id();
    m.Elem = elem.id();
}

internal static @string safeString(this ref mapType m, map<typeId, bool> seen) {
    if (seen[m.Id]) {
        return m.Name;
    }
    seen[m.Id] = true;
    @string key = m.Key.gobType().safeString(seen);
    @string elem = m.Elem.gobType().safeString(seen);
    return fmt.Sprintf("map[%s]%s"u8, key, elem);
}

internal static @string @string(this ref mapType m) {
    return m.safeString(new map<typeId, bool>());
}

// Slice type
public partial struct sliceType {
    public partial ref CommonType CommonType { get; }
    public typeId Elem;
}

internal static ж<sliceType> newSliceType(@string name) {
    var s = Ꮡ(new sliceType(new CommonType(Name: name), 0));
    return s;
}

internal static void init(this ж<sliceType> Ꮡs, ΔgobType elem) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Set our type id before evaluating the element's, in case it's our own.
    setTypeId(new sliceTypeжΔgobType(Ꮡs));
    // See the comments about ids in newTypeObject. Only slices and
    // structs have mutual recursion.
    if (elem.id() == 0) {
        setTypeId(elem);
    }
    s.Elem = elem.id();
}

internal static @string safeString(this ref sliceType s, map<typeId, bool> seen) {
    if (seen[s.Id]) {
        return s.Name;
    }
    seen[s.Id] = true;
    return fmt.Sprintf("[]%s"u8, s.Elem.gobType().safeString(seen));
}

internal static @string @string(this ref sliceType s) {
    return s.safeString(new map<typeId, bool>());
}

// Struct type
public partial struct fieldType {
    public @string Name;
    public typeId Id;
}

public partial struct structType {
    public partial ref CommonType CommonType { get; }
    public slice<fieldType> Field;
}

internal static @string safeString(this ж<structType> Ꮡs, map<typeId, bool> seen) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (Ꮡs == nil) {
        return nilˢ;
    }
    {
        var (_, ok) = seen[s.Id, ꟷ]; if (ok) {
            return s.Name;
        }
    }
    seen[s.Id] = true;
    @string str = s.Name + " = struct { "u8;
    foreach (var (_, f) in s.Field) {
        str += fmt.Sprintf("%s %s; "u8, f.Name, f.Id.gobType().safeString(seen));
    }
    str += "}"u8;
    return str;
}

internal static @string @string(this ж<structType> Ꮡs) {
    return Ꮡs.safeString(new map<typeId, bool>());
}

internal static ж<structType> newStructType(@string name) {
    var s = Ꮡ(new structType(new CommonType(Name: name), default!));
    // For historical reasons we set the id here rather than init.
    // See the comment in newTypeObject for details.
    setTypeId(new structTypeжΔgobType(s));
    return s;
}

// newTypeObject allocates a gobType for the reflection type rt.
// Unless ut represents a GobEncoder, rt should be the base type
// of ut.
// This is only called from the encoding side. The decoding side
// works through typeIds and userTypeInfos alone.
internal static (ΔgobType, error) newTypeObject(@string name, ref userTypeInfo ut, reflectꓸType rt) {
    GoFrame ᒐ = default;
    try {
        // Does this type implement GobEncoder?
        if (ut.externalEnc != 0) {
            return (new gobEncoderTypeжΔgobType(newGobEncoderType(name)), default!);
        }
        ref var err = ref heap<error>(out var Ꮡerr);
        ΔgobType type0 = default!;
        ΔgobType type1 = default!;
        defer(() => {
            if (Ꮡerr.ValueSlot != default!) {
                delete(types, rt);
            }
        }, ref ᒐ);
        // Install the top-level type before the subtypes (e.g. struct before
        // fields) so recursive types can be constructed safely.
        {
            var t = rt;
            var exprᴛ1 = t.Kind();
            if (exprᴛ1 == reflect.ΔBool) {
                return (tBool.gobType(), default!);
            }
            if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
                return (tInt.gobType(), default!);
            }
            if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
                return (tUint.gobType(), default!);
            }
            if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
                return (tFloat.gobType(), default!);
            }
            if (exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
                return (tComplex.gobType(), default!);
            }
            if (exprᴛ1 == reflect.ΔString) {
                return (tString.gobType(), default!);
            }
            if (exprᴛ1 == reflect.ΔInterface) {
                return (tInterface.gobType(), default!);
            }
            if (exprᴛ1 == reflect.Array) {
                var at = newArrayType(name);
                types[rt] = new arrayTypeжΔgobType(at);
                (type0, err) = getBaseType(""u8, // All basic types are easy: they are predefined.
 t.Elem());
                if (err != default!) {
                    return (default!, err);
                }
                at.init(type0, // Historical aside:
 // For arrays, maps, and slices, we set the type id after the elements
 // are constructed. This is to retain the order of type id allocation after
 // a fix made to handle recursive types, which changed the order in
 // which types are built. Delaying the setting in this way preserves
 // type ids while allowing recursive types to be described. Structs,
 // done below, were already handling recursion correctly so they
 // assign the top-level id before those of the field.
 t.Len());
                return (new arrayTypeжΔgobType(at), default!);
            }
            if (exprᴛ1 == reflect.Map) {
                var mt = newMapType(name);
                types[rt] = new mapTypeжΔgobType(mt);
                (type0, err) = getBaseType(""u8, t.Key());
                if (err != default!) {
                    return (default!, err);
                }
                (type1, err) = getBaseType(""u8, t.Elem());
                if (err != default!) {
                    return (default!, err);
                }
                mt.init(type0, type1);
                return (new mapTypeжΔgobType(mt), default!);
            }
            if (exprᴛ1 == reflect.ΔSlice) {
                if (t.Elem().Kind() == reflect.Uint8) {
                    // []byte == []uint8 is a special case
                    return (tBytes.gobType(), default!);
                }
                var st = newSliceType(name);
                types[rt] = new sliceTypeжΔgobType(st);
                (type0, err) = getBaseType(t.Elem().Name(), t.Elem());
                if (err != default!) {
                    return (default!, err);
                }
                st.init(type0);
                return (new sliceTypeжΔgobType(st), default!);
            }
            if (exprᴛ1 == reflect.Struct) {
                var st = newStructType(name);
                types[rt] = new structTypeжΔgobType(st);
                idToTypeSlice[st.of(structType.ᏑCommonType).id()] = new structTypeжΔgobType(st);
                for (nint i = 0; i < t.NumField(); i++) {
                    var f = t.Field(i);
                    if (!isSent(ref f)) {
                        continue;
                    }
                    var typ = userType(f.Type).Value.@base;
                    @string tname = typ.Name();
                    if (tname == ""u8) {
                        var tΔ2 = userType(f.Type).Value.@base;
                        tname = tΔ2.String();
                    }
                    var (gt, errΔ2) = getBaseType(tname, f.Type);
                    if (errΔ2 != default!) {
                        return (default!, errΔ2);
                    }
                    // Some mutually recursive types can cause us to be here while
                    // still defining the element. Fix the element type id here.
                    // We could do this more neatly by setting the id at the start of
                    // building every type, but that would break binary compatibility.
                    if (gt.id() == 0) {
                        setTypeId(gt);
                    }
                    st.Value.Field = append((~st).Field, new fieldType(f.Name, gt.id()));
                }
                return (new structTypeжΔgobType(st), default!);
            }
            { /* default: */
                return (default!, errors.New("gob NewTypeObject can't handle type: "u8 + rt.String()));
            }
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// isExported reports whether this is an exported - upper case - name.
internal static bool isExported(@string name) {
    var (rune, _) = utf8.DecodeRuneInString(name);
    return unicode.IsUpper(rune);
}

// isSent reports whether this struct field is to be transmitted.
// It will be transmitted only if it is exported and not a chan or func field
// or pointer to chan or func.
internal static bool isSent(ref reflect.StructField field) {
    if (!isExported(field.Name)) {
        return false;
    }
    // If the field is a chan or func or pointer thereto, don't send it.
    // That is, treat it like an unexported field.
    var typ = field.Type;
    while (typ.Kind() == reflect.ΔPointer) {
        typ = typ.Elem();
    }
    if (typ.Kind() == reflect.Chan || typ.Kind() == reflect.Func) {
        return false;
    }
    return true;
}

// getBaseType returns the Gob type describing the given reflect.Type's base type.
// typeLock must be held.
internal static (ΔgobType, error) getBaseType(@string name, reflectꓸType rt) {
    var ut = userType(rt);
    return getType(name, ref (ut).DerefOrNull(), (~ut).@base);
}

// getType returns the Gob type describing the given reflect.Type.
// Should be called only when handling GobEncoders/Decoders,
// which may be pointers. All other types are handled through the
// base type, never a pointer.
// typeLock must be held.
internal static (ΔgobType, error) getType(@string name, ref userTypeInfo ut, reflectꓸType rt) {
    var (typ, present) = types[rt, ꟷ];
    if (present) {
        return (typ, default!);
    }
    (typ, var err) = newTypeObject(name, ref ut, rt);
    if (err == default!) {
        types[rt] = typ;
    }
    return (typ, err);
}

internal static void checkId(typeId want, typeId got) {
    if (want != got) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "checkId: %d should be %d\n"u8, (nint)(int32)got, (nint)(int32)want);
        throw panic("bootstrap type wrong id: " + got.name() + " " + got.@string() + " not " + want.@string());
    }
}

// used for building the basic types; called only from init().  the incoming
// interface always refers to a pointer.
internal static typeId bootstrapType(@string name, any e) {
    var rt = reflect.TypeOf(e).Elem();
    var (_, present) = types[rt, ꟷ];
    if (present) {
        throw panic("bootstrap type already present: " + name + ", " + rt.String());
    }
    var typ = Ꮡ(new CommonType(Name: name));
    types[rt] = new CommonTypeжΔgobType(typ);
    setTypeId(new CommonTypeжΔgobType(typ));
    return typ.id();
}

// Representation of the information we send and receive about this type.
// Each value we send is preceded by its type definition: an encoded int.
// However, the very first time we send the value, we first send the pair
// (-id, wireType).
// For bootstrapping purposes, we assume that the recipient knows how
// to decode a wireType; it is exactly the wireType struct here, interpreted
// using the gob rules for sending a structure, except that we assume the
// ids for wireType and structType etc. are known. The relevant pieces
// are built in encode.go's init() function.
// To maintain binary compatibility, if you extend this type, always put
// the new fields last.
partial struct wireType {
    public ж<arrayType> ArrayT;
    public ж<sliceType> SliceT;
    public ж<structType> StructT;
    public ж<mapType> MapT;
    public ж<gobEncoderType> GobEncoderT;
    public ж<gobEncoderType> BinaryMarshalerT;
    public ж<gobEncoderType> TextMarshalerT;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string unknownᶜ = "unknown type"u8;

internal static @string @string(this ж<wireType> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    @string unknown = unknownᶜ;
    if (Ꮡw == nil) {
        return unknown;
    }
    switch (ᐧ) {
    case {} when w.ArrayT != nil: {
        return (~w.ArrayT).Name;
    }
    case {} when w.SliceT != nil: {
        return (~w.SliceT).Name;
    }
    case {} when w.StructT != nil: {
        return (~w.StructT).Name;
    }
    case {} when w.MapT != nil: {
        return (~w.MapT).Name;
    }
    case {} when w.GobEncoderT != nil: {
        return (~w.GobEncoderT).Name;
    }
    case {} when w.BinaryMarshalerT != nil: {
        return (~w.BinaryMarshalerT).Name;
    }
    case {} when w.TextMarshalerT != nil: {
        return (~w.TextMarshalerT).Name;
    }}

    return unknown;
}

partial struct typeInfo {
    internal typeId id;
    internal sync.Mutex encInit; // protects creation of encoder
    internal atomic.Pointer<encEngine> encoder;
    internal wireType wire;
}

// typeInfoMap is an atomic pointer to map[reflect.Type]*typeInfo.
// It's updated copy-on-write. Readers just do an atomic load
// to get the current version of the map. Writers make a full copy of
// the map and atomically update the pointer to point to the new map.
// Under heavy read contention, this is significantly faster than a map
// protected by a mutex.
internal static ж<atomic.Value> ᏑtypeInfoMap = new StandardBox<atomic.Value>(default(atomic.Value));
internal static ref atomic.Value typeInfoMap => ref ᏑtypeInfoMap.Value;

// typeInfoMapInit is used instead of typeInfoMap during init time,
// as types are registered sequentially during init and we can save
// the overhead of making map copies.
// It is saved to typeInfoMap and set to nil before init finishes.
internal static map<reflectꓸType, ж<typeInfo>> typeInfoMapInit = new map<reflectꓸType, ж<typeInfo>>(16);

internal static ж<typeInfo> lookupTypeInfo(reflectꓸType rt) {
    {
        var mΔ1 = typeInfoMapInit; if (mΔ1 != default!) {
            return mΔ1[rt];
        }
    }
    var (m, _) = ᏑtypeInfoMap.Load()._<map<reflectꓸType, ж<typeInfo>>>(ᐧ);
    return m[rt];
}

internal static (ж<typeInfo>, error) getTypeInfo(ref userTypeInfo ut) {
    var rt = ut.@base;
    if (ut.externalEnc != 0) {
        // We want the user type, not the base type.
        rt = ut.user;
    }
    {
        var info = lookupTypeInfo(rt); if (info != nil) {
            return (info, default!);
        }
    }
    return buildTypeInfo(ref ut, rt);
}

// buildTypeInfo constructs the type information for the type
// and stores it in the type info map.
internal static (ж<typeInfo>, error) buildTypeInfo(ref userTypeInfo ut, reflectꓸType rt) {
    GoFrame ᒐ = default;
    try {
        ᏑtypeLock.Lock();
        defer(ᏑtypeLock.Unlock, ref ᒐ);
        {
            var infoΔ1 = lookupTypeInfo(rt); if (infoΔ1 != nil) {
                return (infoΔ1, default!);
            }
        }
        var (gt, err) = getBaseType(rt.Name(), rt);
        if (err != default!) {
            return (default!, err);
        }
        var info = Ꮡ(new typeInfo(id: gt.id()));
        if (ut.externalEnc != 0){
            var (userType, errΔ1) = getType(rt.Name(), ref ut, rt);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            var gtΔ1 = userType.id().gobType()._<ж<gobEncoderType>>();
            var exprᴛ1 = ut.externalEnc;
            if (exprᴛ1 == xGob) {
                info.Value.wire.GobEncoderT = gtΔ1;
            }
            else if (exprᴛ1 == xBinary) {
                info.Value.wire.BinaryMarshalerT = gtΔ1;
            }
            else if (exprᴛ1 == xText) {
                info.Value.wire.TextMarshalerT = gtΔ1;
            }

            rt = ut.user;
        } else {
            var t = (~info).id.gobType();
            {
                var typ = rt;
                var exprᴛ2 = typ.Kind();
                if (exprᴛ2 == reflect.Array) {
                    info.Value.wire.ArrayT = t._<ж<arrayType>>();
                }
                else if (exprᴛ2 == reflect.Map) {
                    info.Value.wire.MapT = t._<ж<mapType>>();
                }
                else if (exprᴛ2 == reflect.ΔSlice) {
                    if (typ.Elem().Kind() != reflect.Uint8) {
                        // []byte == []uint8 is a special case handled separately
                        info.Value.wire.SliceT = t._<ж<sliceType>>();
                    }
                }
                else if (exprᴛ2 == reflect.Struct) {
                    info.Value.wire.StructT = t._<ж<structType>>();
                }
            }

        }
        {
            var mΔ1 = typeInfoMapInit; if (mΔ1 != default!) {
                mΔ1[rt] = info;
                return (info, default!);
            }
        }
        // Create new map with old contents plus new entry.
        var (m, _) = ᏑtypeInfoMap.Load()._<map<reflectꓸType, ж<typeInfo>>>(ᐧ);
        var newm = maps.Clone<map<reflectꓸType, ж<typeInfo>>, reflectꓸType, ж<typeInfo>>(m);
        newm[rt] = info;
        ᏑtypeInfoMap.Store(newm);
        return (info, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Called only when a panic is acceptable and unexpected.
internal static ж<typeInfo> mustGetTypeInfo(reflectꓸType rt) {
    var (t, err) = getTypeInfo(ref (userType(rt)).DerefOrNull());
    if (err != default!) {
        throw panic("getTypeInfo: " + err.Error());
    }
    return t;
}

// GobEncoder is the interface describing data that provides its own
// representation for encoding values for transmission to a GobDecoder.
// A type that implements GobEncoder and GobDecoder has complete
// control over the representation of its data and may therefore
// contain things such as private fields, channels, and functions,
// which are not usually transmissible in gob streams.
//
// Note: Since gobs can be stored permanently, it is good design
// to guarantee the encoding used by a GobEncoder is stable as the
// software evolves. For instance, it might make sense for GobEncode
// to include a version number in the encoding.
partial interface GobEncoder {
    // GobEncode returns a byte slice representing the encoding of the
    // receiver for transmission to a GobDecoder, usually of the same
    // concrete type.
    (slice<byte>, error) GobEncode();
}

// GobDecoder is the interface describing data that provides its own
// routine for decoding transmitted values sent by a GobEncoder.
partial interface GobDecoder {
    // GobDecode overwrites the receiver, which must be a pointer,
    // with the value represented by the byte slice, which was written
    // by GobEncode, usually for the same concrete type.
    error GobDecode(slice<byte> _);
}

internal static ж<sync.Map> ᏑnameToConcreteType = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map nameToConcreteType => ref ᏑnameToConcreteType.Value; // map[string]reflect.Type
internal static ж<sync.Map> ᏑconcreteTypeToName = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map concreteTypeToName => ref ᏑconcreteTypeToName.Value; // map[reflect.Type]string

// RegisterName is like [Register] but uses the provided name rather than the
// type's default.
public static void RegisterName(@string name, any value) {
    if (name == ""u8) {
        // reserved for nil
        throw panic("attempt to register empty name");
    }
    var ut = userType(reflect.TypeOf(value));
    // Check for incompatible duplicates. The name must refer to the
    // same user type, and vice versa.
    // Store the name and type provided by the user....
    {
        var (t, dup) = ᏑnameToConcreteType.LoadOrStore(name, reflect.TypeOf(value)); if (dup && !AreEqual(t, (~ut).user)) {
            throw panic(fmt.Sprintf("gob: registering duplicate types for %q: %s != %s"u8, name, t, (~ut).user));
        }
    }
    // but the flattened type in the type table, since that's what decode needs.
    {
        var (n, dup) = ᏑconcreteTypeToName.LoadOrStore((~ut).@base, name); if (dup && !AreEqual(n, name)) {
            ᏑnameToConcreteType.Delete(name);
            throw panic(fmt.Sprintf("gob: registering duplicate names for %s: %q != %q"u8, (~ut).user, n, name));
        }
    }
}

// Register records a type, identified by a value for that type, under its
// internal type name. That name will identify the concrete type of a value
// sent or received as an interface variable. Only types that will be
// transferred as implementations of interface values need to be registered.
// Expecting to be used only during initialization, it panics if the mapping
// between types and names is not a bijection.
public static void Register(any value) {
    // Default to printed representation for unnamed types
    var rt = reflect.TypeOf(value);
    @string name = rt.String();
    // But for named types (or pointers to them), qualify with import path (but see inner comment).
    // Dereference one pointer looking for a named type.
    @string star = ""u8;
    if (rt.Name() == ""u8) {
        {
            var pt = rt; if (pt.Kind() == reflect.ΔPointer) {
                star = "*"u8;
                // NOTE: The following line should be rt = pt.Elem() to implement
                // what the comment above claims, but fixing it would break compatibility
                // with existing gobs.
                //
                // Given package p imported as "full/p" with these definitions:
                //     package p
                //     type T1 struct { ... }
                // this table shows the intended and actual strings used by gob to
                // name the types:
                //
                // Type      Correct string     Actual string
                //
                // T1        full/p.T1          full/p.T1
                // *T1       *full/p.T1         *p.T1
                //
                // The missing full path cannot be fixed without breaking existing gob decoders.
                rt = pt;
            }
        }
    }
    if (rt.Name() != ""u8) {
        if (rt.PkgPath() == ""u8){
            name = star + rt.Name();
        } else {
            name = star + rt.PkgPath() + "."u8 + rt.Name();
        }
    }
    RegisterName(name, value);
}

internal static void registerBasics() {
    Register((nint)0);
    Register((int8)0);
    Register((int16)0);
    Register((int32)0);
    Register((int64)0);
    Register((nuint)0);
    Register((uint8)0);
    Register((uint16)0);
    Register((uint32)0);
    Register((uint64)0);
    Register((float32)0F);
    Register((float64)0D);
    Register((complex64)0F.i());
    Register((complex128)0D.i());
    Register((uintptr)0);
    Register(false);
    Register((@string)""u8);
    Register(slice<byte>(default!));
    Register(slice<nint>(default!));
    Register(slice<int8>(default!));
    Register(slice<int16>(default!));
    Register(slice<int32>(default!));
    Register(slice<int64>(default!));
    Register(slice<nuint>(default!));
    Register(slice<uint8>(default!));
    Register(slice<uint16>(default!));
    Register(slice<uint32>(default!));
    Register(slice<uint64>(default!));
    Register(slice<float32>(default!));
    Register(slice<float64>(default!));
    Register(slice<complex64>(default!));
    Register(slice<complex128>(default!));
    Register(slice<uintptr>(default!));
    Register(slice<bool>(default!));
    Register(slice<@string>(default!));
}

[GoInit] internal static void initΔ2() {
    ᏑtypeInfoMap.Store(typeInfoMapInit);
    typeInfoMapInit = default!;
}

} // end gob_package
