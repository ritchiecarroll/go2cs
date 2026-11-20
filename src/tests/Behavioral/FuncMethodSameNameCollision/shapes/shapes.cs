namespace go.FuncMethodSameNameCollision;

using static shapes_package;

partial class shapes_package {

partial struct Type {
    public nint Flags;
}

internal static bool isTupleType(ref Type t) { return (t.Flags & 1) != 0; }

} partial class shapesᴛType {

public static bool IsTupleType(this ж<Type> Ꮡt) { return isTupleType(ref (Ꮡt).DerefOrNull()); }

} partial class shapes_package {

public static bool IsTupleType(ж<Type> t) { return isTupleType(ref (t).DerefOrNull()); }

} partial class shapesᴛType {

public static nint Combine(this ref Type t, nint other) { return t.Flags + other; }

} partial class shapes_package {

public static nint Combine(ж<Type> Ꮡt, nint other) {
    ref var t = ref Ꮡt.DerefOrNull();

    return t.Flags * 100 + other;
}

} partial class shapesᴛType {

public static nint Size(this ref Type t) { return t.Flags; }

} partial class shapes_package {

public static nint Size(ж<Type> Ꮡt, nint scale) {
    ref var t = ref Ꮡt.DerefOrNull();

    return t.Flags * scale;
}

} // end shapes_package
