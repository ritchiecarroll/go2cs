namespace go.DotImportNameCollision.a;

partial class keys_package {

partial struct Key {
    internal @string name;
}

public static ж<Key> NewKey(@string name) {
    return Ꮡ(new Key(name: name));
}

public static @string Name(this ref Key k) {
    return k.name;
}

partial struct Count /*num:nint*/;

partial struct Box<T> {
    public T V;
}

public static Box<T> Wrap<T>(T v) {
    return new Box<T>(V: v);
}

} // end keys_package
