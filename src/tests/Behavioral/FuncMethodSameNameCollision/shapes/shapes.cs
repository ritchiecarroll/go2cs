namespace go.FuncMethodSameNameCollision;

partial class shapes_package {

partial struct Type {
    public nint Flags;
}

internal static bool isTupleType(ref Type t) { return (t.Flags & 1) != 0; }

internal static bool IsTupleType⁔(/*this*/ ж<Type> Ꮡt) { return isTupleType(ref (Ꮡt).DerefOrNull()); }

public static bool IsTupleType(ж<Type> t) { return isTupleType(ref (t).DerefOrNull()); }

internal static nint Combine⁔(/*this*/ ref Type t, nint other) { return t.Flags + other; }

public static nint Combine(ж<Type> Ꮡt, nint other) {
    ref var t = ref Ꮡt.DerefOrNull();

    return t.Flags * 100 + other;
}

internal static nint Size⁔(/*this*/ ref Type t) { return t.Flags; }

public static nint Size(ж<Type> Ꮡt, nint scale) {
    ref var t = ref Ꮡt.DerefOrNull();

    return t.Flags * scale;
}

} // end shapes_package
