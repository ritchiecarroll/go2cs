namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct myBytes /*[]byte*/;

partial struct myInts /*[]nint*/;

partial struct myMap /*map[@string, nint]*/;

partial struct myWideMap /*map[@string, int64]*/;

partial struct myKeyMap /*map[nint, nint]*/;

partial class myPtr /*ж<nint>*/;

partial class myWidePtr /*ж<int64>*/;

partial struct myChan /*chan nint*/;

partial struct myArray /*[3]byte*/;

partial struct myWideArray /*[4]byte*/;

partial struct myOtherBytes /*[]byte*/;

partial struct fieldsA {
    public slice<byte> B;
    public map<@string, nint> M;
}

partial struct namedFieldsA {
    public slice<byte> B;
    public map<@string, nint> M;
}

partial struct fieldsWideElem {
    public slice<byte> B;
    public map<@string, int64> M;
}

partial struct fieldsRenamed {
    public slice<byte> B;
    public map<@string, nint> N;
}

partial struct fieldsShort {
    public slice<byte> B;
}

partial struct fieldsTagged {
    public slice<byte> B; /*`json:"b"`*/
    public map<@string, nint> M; /*`json:"m"`*/
}

partial struct speaker {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string speakerˢ = "speaker"u8;

internal static @string String(this speaker _) {
    return speakerˢ;
}

partial struct mute {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sliceˢ = (@string)"slice"u8;
private static readonly object mapˢ = (@string)"map"u8;
private static readonly object ptrˢ = (@string)"ptr"u8;
private static readonly object arrayˢ = (@string)"array"u8;
private static readonly object elemˢ = (@string)"elem"u8;
private static readonly object assignˢ = (@string)"assign"u8;
private static readonly object ifaceˢ = (@string)"iface"u8;
private static readonly object structˢ = (@string)"struct"u8;
private static readonly object funcˢ = (@string)"func"u8;
private static readonly object chanˢ = (@string)"chan"u8;

internal partial struct main_i /*dyn*/ {
    public slice<byte> B;
    public map<@string, nint> M;
}

internal static void Main() {
    var bytes = reflect.TypeOf(slice<byte>(default!));
    var named = reflect.TypeOf(((myBytes)default!));
    var ints = reflect.TypeOf(slice<nint>(default!));
    var namedInts = reflect.TypeOf(((myInts)default!));
    fmt.Println(sliceˢ, bytes.ConvertibleTo(named), named.ConvertibleTo(bytes));
    fmt.Println(sliceˢ, named.ConvertibleTo(named), bytes.ConvertibleTo(bytes));
    fmt.Println(sliceˢ, bytes.ConvertibleTo(namedInts), ints.ConvertibleTo(namedInts));
    fmt.Println(sliceˢ, named.ConvertibleTo(ints), ints.ConvertibleTo(bytes));
    map<@string, nint> unnamedMap = default!;
    var plain = reflect.TypeOf(unnamedMap);
    var namedMap = reflect.TypeOf(((myMap)default!));
    var wideMap = reflect.TypeOf(((myWideMap)default!));
    var keyMap = reflect.TypeOf(((myKeyMap)default!));
    fmt.Println(mapˢ, plain.ConvertibleTo(namedMap), namedMap.ConvertibleTo(plain));
    fmt.Println(mapˢ, plain.ConvertibleTo(wideMap), plain.ConvertibleTo(keyMap));
    fmt.Println(mapˢ, wideMap.ConvertibleTo(keyMap), namedMap.ConvertibleTo(namedMap));
    var ptr = reflect.TypeOf(((ж<nint>)nil));
    var namedPtr = reflect.TypeOf(((myPtr)nil));
    var widePtr = reflect.TypeOf(((myWidePtr)nil));
    fmt.Println(ptrˢ, ptr.ConvertibleTo(namedPtr), namedPtr.ConvertibleTo(ptr));
    fmt.Println(ptrˢ, ptr.ConvertibleTo(widePtr), widePtr.ConvertibleTo(namedPtr));
    var arr = reflect.TypeOf(new byte[]{}.array(3));
    var namedArr = reflect.TypeOf(new myArray(new byte[3].array()));
    var wideArr = reflect.TypeOf(new myWideArray(new byte[4].array()));
    fmt.Println(arrayˢ, arr.ConvertibleTo(namedArr), namedArr.ConvertibleTo(arr));
    fmt.Println(arrayˢ, arr.ConvertibleTo(wideArr), wideArr.ConvertibleTo(namedArr));
    fmt.Println(arrayˢ, arr.Len(), namedArr.Len(), wideArr.Len());
    var myChanType = reflect.TypeOf(((myChan)default!));
    fmt.Println(elemˢ, named.Elem().Kind(), namedMap.Key().Kind(), namedMap.Elem().Kind());
    fmt.Println(elemˢ, namedPtr.Elem().Kind(), myChanType.Elem().Kind(), namedArr.Elem().Kind());
    var other = reflect.TypeOf(((myOtherBytes)default!));
    fmt.Println(assignˢ, bytes.AssignableTo(named), named.AssignableTo(bytes));
    fmt.Println(assignˢ, named.AssignableTo(named), bytes.AssignableTo(bytes));
    fmt.Println(assignˢ, named.AssignableTo(other), other.AssignableTo(named));
    fmt.Println(assignˢ, named.AssignableTo(namedInts), bytes.AssignableTo(ints));
    fmt.Println(assignˢ, named.AssignableTo(namedMap), plain.AssignableTo(named));
    var stringerType = reflect.TypeOf(((ж<fmt.Stringer>)nil)).Elem();
    var emptyType = reflect.TypeOf(((ж<any>)nil)).Elem();
    fmt.Println(ifaceˢ, reflect.TypeOf(new speaker(nil)).AssignableTo(stringerType));
    fmt.Println(ifaceˢ, reflect.TypeOf(new mute(nil)).AssignableTo(stringerType));
    fmt.Println(ifaceˢ, named.AssignableTo(emptyType), stringerType.AssignableTo(emptyType));
    var structA = reflect.TypeOf(new fieldsA(nil));
    var structWideElem = reflect.TypeOf(new fieldsWideElem(nil));
    var structRenamed = reflect.TypeOf(new fieldsRenamed(nil));
    var structShort = reflect.TypeOf(new fieldsShort(nil));
    var namedStructA = reflect.TypeOf(new namedFieldsA(nil));
    fmt.Println(structˢ, structA.ConvertibleTo(structA), structA.ConvertibleTo(structWideElem));
    fmt.Println(structˢ, structA.ConvertibleTo(structRenamed), structA.ConvertibleTo(structShort));
    fmt.Println(structˢ, structA.AssignableTo(namedStructA), namedStructA.AssignableTo(structA));
    fmt.Println(structˢ, structWideElem.AssignableTo(namedStructA), structShort.AssignableTo(namedStructA));
    var structTagged = reflect.TypeOf(new fieldsTagged(nil));
    var unnamedStruct = reflect.TypeOf(new main_i());
    fmt.Println(structˢ, structA.ConvertibleTo(structTagged), structTagged.ConvertibleTo(structA));
    fmt.Println(structˢ, unnamedStruct.AssignableTo(structA), unnamedStruct.AssignableTo(structTagged));
    fmt.Println(structˢ, unnamedStruct.ConvertibleTo(structTagged), unnamedStruct.AssignableTo(structShort));
    var funcIB = reflect.TypeOf(bool (nint _) => false);
    var funcSB = reflect.TypeOf(bool (@string _) => false);
    var funcII = reflect.TypeOf(nint (nint _) => 0);
    var funcI2B = reflect.TypeOf(bool (nint _Δp0, nint _Δp1) => false);
    fmt.Println(funcˢ, funcIB.ConvertibleTo(funcIB), funcIB.ConvertibleTo(funcSB));
    fmt.Println(funcˢ, funcIB.ConvertibleTo(funcII), funcIB.ConvertibleTo(funcI2B));
    var plainChan = reflect.TypeOf(new channel<nint>(0));
    var wideChan = reflect.TypeOf(new channel<int64>(0));
    fmt.Println(chanˢ, plainChan.AssignableTo(myChanType), myChanType.AssignableTo(plainChan));
    fmt.Println(chanˢ, wideChan.AssignableTo(myChanType), plainChan.AssignableTo(plainChan));
    fmt.Println(chanˢ, plainChan.ConvertibleTo(myChanType), wideChan.ConvertibleTo(myChanType));
}

} // end main_package
