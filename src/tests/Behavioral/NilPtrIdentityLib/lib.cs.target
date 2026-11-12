namespace go;

partial class NilPtrIdentityLib_package {

partial struct Message {
    public nint Code;
}

public static ж<Message> Default = Ꮡ(new Message(Code: 2344));

public static T OrElse<T>(T value, T orElse) {
    if (!AreEqual(value, @new<T>().ValueSlot)) {
        return value;
    }

    return orElse;
}

public static bool IsZero<T>(T value) {
    T zero = GoZero<T>();
    return AreEqual(value, zero);
}

public static bool Same<T>(T a, T b) {
    return AreEqual(a, b);
}

} // end NilPtrIdentityLib_package
