namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Message {
    public @string Text;
}

public static void Print(this Message m) {
    fmt.Println(m.Text);
}

internal partial interface main_Printer /*dyn*/ {
    void Print();
}

internal static void Main() {
    main_Printer p = new Message("Hello, from a function-scoped interface!"u8);
    p.Print();
}

} // end main_package
