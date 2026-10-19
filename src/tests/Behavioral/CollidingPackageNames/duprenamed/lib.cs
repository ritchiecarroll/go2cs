namespace go.collidea;

partial class dup_package {

partial struct Widget {
}

public static @string Marker(this Widget _) {
    return "widget-marker";
}

partial struct ΔMarker {
    public @string Value;
}

public static @string Greeting() {
    return "hello-from-duprenamed";
}

partial struct Box<T> {
    public T V;
}

public static T Get<T>(this Box<T> b) {
    return b.V;
}

} // end dup_package
