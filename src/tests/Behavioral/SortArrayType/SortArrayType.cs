namespace go;

using fmt = fmt_package;
using sort = sort_package;

partial class main_package {

internal const float32 w = 1.0f;
internal static UntypedInt _addr_X => 1;
public static UntypedInt Y => 2;
public static UntypedInt Z => 3;

public static UntypedInt A1 => iota;
public static UntypedInt B => /* iota * 100 */ 0;
internal static UntypedInt _ᴛ1ʗ => 1;
public static UntypedInt D => 100;
public static UntypedInt E211 => 2;
public static UntypedInt F => 200;
public static readonly GoBigConst Giant = /* 1 << 100 */
    GoBigConst.Parse("1267650600228229401496703205376");
public static readonly GoBigConst Giant2 = /* 1 << 200 */
    GoBigConst.Parse("1606938044258990275541962092341162602522202993782792835301376");
public static readonly @string String = "Hello"u8;
public static readonly @string String2 = "World"u8;
public static readonly @string String3 = "世界 \u0053\u004a3"u8;
public static UntypedFloat Float => 3.14;
public static UntypedFloat Float2 => 3.14e100;
public static readonly GoBigConst GiantFloat = /* 1e309 */
    GoBigConst.Parse("1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
public static readonly @string MultiLine = """

        Line1 /123
        Line2 ""Yo""
        Line3
        
"""u8;
public static readonly @string MultiLine2 = """"

        Line1 /123
        Line2 """Yo"""
        Line3
""""u8;
public static readonly @string MultiLine3 = """
Line1
        Line2
        "Yo"
        Line3
"""u8;

public static nint A2 = 1;
public static @string B2 = "42"u8;
public static nint C21 = dynamicFn1();
public static bool D21;
public static bool E2;
public static bool ΔTa_package = false;
internal static bool otherID = true;

internal static nint dynamicFn1() {
    return 4;
}

partial interface NodeR {
    nint Pos();
    nint End12();
    @string Name(nint offset);
}

partial struct Person {
    public @string Name;
    public nint Age; /*`json:"Tag"`*/
    public float32 ShoeSize;
}

internal static void consumeOne(ref slice<Person> v) {
    v = (v)[1..];
}

partial struct PeopleByShoeSize /*[]Person*/;

partial struct PeopleByAge /*[]Person*/;

partial struct Roster /*[]Person*/;

internal static nint headcount(this Roster r) {
    return len(r);
}

partial struct levelToken /*num:uint32*/;

internal static UntypedInt markerConst => 256;

internal static void sort(this PeopleByAge p) {
    sort_package.Sort(p);
}

public static nint Len(this PeopleByShoeSize p) {
    return len(p);
}

public static void Swap(this PeopleByShoeSize p, nint i, nint j) {
    (p[i], p[j]) = (p[j], p[i]);
}

public static bool /*a*/ Less(this PeopleByShoeSize p, nint i, nint j) {
    bool a = default!;

    a = (p[i].ShoeSize < p[j].ShoeSize);
    return a;
}

public static nint Len(this PeopleByAge p) {
    return len(p);
}

public static void Swap(this PeopleByAge p, nint i, nint j) {
    (p[i], p[j]) = (p[j], p[i]);
}

public static bool Less(this PeopleByAge p, nint i, nint j) {
    return (p[i].Age < p[j].Age);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloˢ = "Hello"u8;

public static (nint E2, @string p) Testing() {
    nint E2 = default!;
    @string p = default!;

    fmt.Println(E2);
    E2 = 1;
    nint B2 = 2;
    fmt.Println(E2, B2);
    p = helloˢ;
    {
        nint E2Δ1 = 99;
        nint B2Δ1 = 199;
        fmt.Println(E2Δ1, B2Δ1);
    }
    {
        E2 = 100;
        nint B2Δ2 = 200;
        fmt.Println(E2, B2Δ2);
    }
    fmt.Println(E2, B2);
    return (E2, p);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloSj3ˢ = "Hello, 世界 \u0053\u004a3"u8;
private static readonly object consumedˢ = (@string)"consumed:"u8;

internal static void Main() {
    var agesDemo = new PeopleByAge(new Person[]{new("Zed"u8, 30, 11.5F), new("Amy"u8, 25, 7.0F)}.slice());
    agesDemo.sort();
    fmt.Println(agesDemo[0].Name);
    var lt = ((levelToken)markerConst);
    fmt.Println((uint32)lt + 1);
    fmt.Println(MultiLine);
    fmt.Println(MultiLine2);
    fmt.Println(MultiLine3);
    Testing();
    @string x = helloSj3ˢ;
    fmt.Println(x);
    var people = new Person[]{
        new(
            Name: "Person1"u8,
            Age: 26,
            ShoeSize: 8F
        ),
        new(
            Name: "Person2"u8,
            Age: 21,
            ShoeSize: 4F
        ),
        new(
            Name: "Person3"u8,
            Age: 15,
            ShoeSize: 9F
        ),
        new(
            Name: "Person4"u8,
            Age: 45,
            ShoeSize: 15F
        ),
        new(
            Name: "Person5"u8,
            Age: 25,
            ShoeSize: 8.5F
        )
    }.slice();
    fmt.Println(people);
    sort_package.Sort(((PeopleByShoeSize)people));
    fmt.Println(people);
    sort_package.Sort(((PeopleByAge)people));
    fmt.Println(people);
    var byAge = ((PeopleByAge)people);
    fmt.Println(byAge[0..3].Len(), byAge[2..].Len());
    var pb = @new<PeopleByAge>();
    pb.ValueSlot = byAge[1..4];
    fmt.Println((~pb).Len());
    var ros = ((Roster)(slice<Person>)(byAge[0..2]));
    fmt.Println(ros.headcount(), len(ros));
    ref var crew = ref heap<Roster>(out var Ꮡcrew);
    crew = new Roster(new Person[]{new(Name: "Ann"u8, Age: 30), new(Name: "Bob"u8, Age: 40)}.slice());
    consumeOne(ref (Ꮡcrew.of(Roster.Ꮡm_value)).DerefOrNull());
    fmt.Println(consumedˢ, len(crew), crew[0].Name);
    x = """

        SELECT *
        FROM 
"""u8 + "`Role`"u8;
}

} // end main_package
