namespace go.EmbeddedInterfaceWitness;

using static iolike_package;

partial class iolike_package {

partial interface Reader {
    @string Read();
}

partial interface ReadWriter {
    @string Read();
    @string Write(@string s);
}

partial struct Base {
    public @string Tag;
}

} partial class iolikeᴛBase {

public static @string Read(this Base b) { return "read:"u8 + b.Tag; }

} // end iolikeᴛBase
