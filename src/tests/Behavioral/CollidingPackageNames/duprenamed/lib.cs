namespace go.collidea;

using static dup_package;

partial class dup_package {

partial struct Widget {
}

} partial class dupᴛWidget {

public static @string Marker(this Widget _) { return "widget-marker"; }

} partial class dup_package {

partial struct Marker {
    public @string Value;
}

public static @string Greeting() { return "hello-from-duprenamed"; }

partial struct Box<T> {
    public T V;
}

} partial class dupᴛBox {

public static T Get<T>(this Box<T> b) { return b.V; }

} // end dupᴛBox
