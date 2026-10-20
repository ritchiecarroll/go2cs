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

internal static @string String(this speaker _) {
    return "speaker";
}

partial struct mute {
}

internal partial struct main_i /*dyn*/ {
    public slice<byte> B;
    public map<@string, nint> M;
}

internal static void Main() {
    var bytes = reflect.TypeOf(slice<byte>(default!));
    var named = reflect.TypeOf((myBytes)default!);
    var ints = reflect.TypeOf(slice<nint>(default!));
    var namedInts = reflect.TypeOf((myInts)default!);

    fmt.Println("slice", bytes.ConvertibleTo(named), named.ConvertibleTo(bytes));
    fmt.Println("slice", named.ConvertibleTo(named), bytes.ConvertibleTo(bytes));
    fmt.Println("slice", bytes.ConvertibleTo(namedInts), ints.ConvertibleTo(namedInts));
    fmt.Println("slice", named.ConvertibleTo(ints), ints.ConvertibleTo(bytes));

    map<@string, nint> unnamedMap = default!;
    var plain = reflect.TypeOf(unnamedMap);
    var namedMap = reflect.TypeOf((myMap)default!);
    var wideMap = reflect.TypeOf((myWideMap)default!);
    var keyMap = reflect.TypeOf((myKeyMap)default!);
    fmt.Println("map", plain.ConvertibleTo(namedMap), namedMap.ConvertibleTo(plain));
    fmt.Println("map", plain.ConvertibleTo(wideMap), plain.ConvertibleTo(keyMap));
    fmt.Println("map", wideMap.ConvertibleTo(keyMap), namedMap.ConvertibleTo(namedMap));

    var ptr = reflect.TypeOf((ж<nint>)nil);
    var namedPtr = reflect.TypeOf((myPtr)nil);
    var widePtr = reflect.TypeOf((myWidePtr)nil);
    fmt.Println("ptr", ptr.ConvertibleTo(namedPtr), namedPtr.ConvertibleTo(ptr));
    fmt.Println("ptr", ptr.ConvertibleTo(widePtr), widePtr.ConvertibleTo(namedPtr));

    var arr = reflect.TypeOf(new byte[]{}.array(3));
    var namedArr = reflect.TypeOf(new myArray(new byte[3].array()));
    var wideArr = reflect.TypeOf(new myWideArray(new byte[4].array()));
    fmt.Println("array", arr.ConvertibleTo(namedArr), namedArr.ConvertibleTo(arr));
    fmt.Println("array", arr.ConvertibleTo(wideArr), wideArr.ConvertibleTo(namedArr));
    fmt.Println("array", arr.Len(), namedArr.Len(), wideArr.Len());

    var myChanType = reflect.TypeOf((myChan)default!);

    fmt.Println("elem", named.Elem().Kind(), namedMap.Key().Kind(), namedMap.Elem().Kind());
    fmt.Println("elem", namedPtr.Elem().Kind(), myChanType.Elem().Kind(), namedArr.Elem().Kind());

    var other = reflect.TypeOf((myOtherBytes)default!);
    fmt.Println("assign", bytes.AssignableTo(named), named.AssignableTo(bytes));
    fmt.Println("assign", named.AssignableTo(named), bytes.AssignableTo(bytes));
    fmt.Println("assign", named.AssignableTo(other), other.AssignableTo(named));
    fmt.Println("assign", named.AssignableTo(namedInts), bytes.AssignableTo(ints));
    fmt.Println("assign", named.AssignableTo(namedMap), plain.AssignableTo(named));

    var stringerType = reflect.TypeOf((ж<fmt.Stringer>)nil).Elem();
    var emptyType = reflect.TypeOf((ж<any>)nil).Elem();
    fmt.Println("iface", reflect.TypeOf(new speaker(nil)).AssignableTo(stringerType));
    fmt.Println("iface", reflect.TypeOf(new mute(nil)).AssignableTo(stringerType));
    fmt.Println("iface", named.AssignableTo(emptyType), stringerType.AssignableTo(emptyType));

    var structA = reflect.TypeOf(new fieldsA(nil));
    var structWideElem = reflect.TypeOf(new fieldsWideElem(nil));
    var structRenamed = reflect.TypeOf(new fieldsRenamed(nil));
    var structShort = reflect.TypeOf(new fieldsShort(nil));
    var namedStructA = reflect.TypeOf(new namedFieldsA(nil));
    fmt.Println("struct", structA.ConvertibleTo(structA), structA.ConvertibleTo(structWideElem));
    fmt.Println("struct", structA.ConvertibleTo(structRenamed), structA.ConvertibleTo(structShort));
    fmt.Println("struct", structA.AssignableTo(namedStructA), namedStructA.AssignableTo(structA));
    fmt.Println("struct", structWideElem.AssignableTo(namedStructA), structShort.AssignableTo(namedStructA));
    var structTagged = reflect.TypeOf(new fieldsTagged(nil));
    var unnamedStruct = reflect.TypeOf(new main_i());
    fmt.Println("struct", structA.ConvertibleTo(structTagged), structTagged.ConvertibleTo(structA));
    fmt.Println("struct", unnamedStruct.AssignableTo(structA), unnamedStruct.AssignableTo(structTagged));
    fmt.Println("struct", unnamedStruct.ConvertibleTo(structTagged), unnamedStruct.AssignableTo(structShort));

    var funcIB = reflect.TypeOf(bool (nint _) => false);
    var funcSB = reflect.TypeOf(bool (@string _) => false);
    var funcII = reflect.TypeOf(nint (nint _) => 0);
    var funcI2B = reflect.TypeOf(bool (nint _Δp0, nint _Δp1) => false);
    fmt.Println("func", funcIB.ConvertibleTo(funcIB), funcIB.ConvertibleTo(funcSB));
    fmt.Println("func", funcIB.ConvertibleTo(funcII), funcIB.ConvertibleTo(funcI2B));

    var plainChan = reflect.TypeOf(new channel<nint>(0));
    var wideChan = reflect.TypeOf(new channel<int64>(0));
    fmt.Println("chan", plainChan.AssignableTo(myChanType), myChanType.AssignableTo(plainChan));
    fmt.Println("chan", wideChan.AssignableTo(myChanType), plainChan.AssignableTo(plainChan));
    fmt.Println("chan", plainChan.ConvertibleTo(myChanType), wideChan.ConvertibleTo(myChanType));
}

} // end main_package
