namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Message {
    public @string Text;
}

} partial class mainᴛMessage {

public static void Print(this Message m) {
    fmt.Println(m.Text);
}

} partial class main_package {

internal partial interface main_Printer /*dyn*/ {
    void Print();
}

internal static void main() {
    main_Printer p = new Message("Hello, from a function-scoped interface!");
    p.Print();
}

} // end main_package
