namespace go.DotImportNameCollision.b;

partial class keys_package {

partial struct Key {
    internal nint id;
}

public static ж<Key> NewKey(nint id) {
    return Ꮡ(new Key(id: id));
}

public static nint ID(this ref Key k) {
    return k.id;
}

} // end keys_package
