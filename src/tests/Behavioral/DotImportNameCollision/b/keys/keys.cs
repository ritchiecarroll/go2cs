namespace go.DotImportNameCollision.b;

using static keys_package;

partial class keys_package {

partial struct Key {
    internal nint id;
}

public static ж<Key> NewKey(nint id) { return Ꮡ(new Key(id: id)); }

} partial class keysᴛKey {

public static nint ID(this ref Key k) { return k.id; }

} // end keysᴛKey
