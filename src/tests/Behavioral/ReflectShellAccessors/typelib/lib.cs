namespace go.ReflectShellAccessors;

partial class typelib_package {

partial struct Count /*num:nint*/;

internal static @string Name⁔(/*this*/ Count c) { return "count"; }

partial struct Label /*@string*/;

internal static @string Name⁔(/*this*/ Label l) { return "label"; }

partial struct List /*[]nint*/;

internal static @string Name⁔(/*this*/ List l) { return "list"; }

partial struct Dict /*map[@string, nint]*/;

internal static @string Name⁔(/*this*/ Dict d) { return "dict"; }

partial struct Mark {
    public @string Tag;
}

internal static @string Name⁔(/*this*/ Mark m) { return "mark"; }

partial struct Box {
    public @string Tag;
}

internal static @string Name⁔(/*this*/ ref Box b) { return "box"; }

public static List NewList() { return new List(new nint[]{3, 1, 2}.slice()); }

public static Dict NewDict() { return new Dict(new map<@string, nint>{["a"] = 1}); }

public static Mark NewMark() { return new Mark(Tag: "x"); }

public static ж<Box> NewBox() { return Ꮡ(new Box(Tag: "y")); }

} // end typelib_package
