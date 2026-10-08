namespace go.collidea;

partial class dup_package {

partial struct Widget {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string widgetMarkerˢ = "widget-marker"u8;

public static @string Marker(this Widget _) {
    return widgetMarkerˢ;
}

partial struct ΔMarker {
    public @string Value;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helloFromDuprenamedˢ = "hello-from-duprenamed"u8;

public static @string Greeting() {
    return helloFromDuprenamedˢ;
}

partial struct Box<T> {
    public T V;
}

public static T Get<T>(this Box<T> b) {
    return b.V;
}

} // end dup_package
