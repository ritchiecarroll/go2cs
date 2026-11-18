namespace go.ReflectShellAccessors;

using static typelib_package;

partial class typelib_package {

partial struct Count /*num:nint*/;

} partial class typelibᴛCount {

public static @string Name(this Count c) { return "count"; }

} partial class typelib_package {

partial struct Label /*@string*/;

} partial class typelibᴛLabel {

public static @string Name(this Label l) { return "label"; }

} partial class typelib_package {

partial struct List /*[]nint*/;

} partial class typelibᴛList {

public static @string Name(this List l) { return "list"; }

} partial class typelib_package {

partial struct Dict /*map[@string, nint]*/;

} partial class typelibᴛDict {

public static @string Name(this Dict d) { return "dict"; }

} partial class typelib_package {

partial struct Mark {
    public @string Tag;
}

} partial class typelibᴛMark {

public static @string Name(this Mark m) { return "mark"; }

} partial class typelib_package {

partial struct Box {
    public @string Tag;
}

} partial class typelibᴛBox {

public static @string Name(this ref Box b) { return "box"; }

} partial class typelib_package {

public static List NewList() { return new List(new nint[]{3, 1, 2}.slice()); }

public static Dict NewDict() { return new Dict(new map<@string, nint>{["a"] = 1}); }

public static Mark NewMark() { return new Mark(Tag: "x"); }

public static ж<Box> NewBox() { return Ꮡ(new Box(Tag: "y")); }

} // end typelib_package
