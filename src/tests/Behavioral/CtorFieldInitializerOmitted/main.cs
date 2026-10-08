namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Holder {
    public @string Name;
    public /*<-*/channel<EmptyStruct> Done = /*<-*/channel<EmptyStruct>.RecvOnly;
}

internal static void Main() {
    var a = new Holder(Name: "a"u8);
    var b = @new<Holder>();
    b.Value.Name = "a"u8;
    fmt.Println(reflect.DeepEqual(a, b.Value));
    fmt.Printf("%T\n"u8, a.Done);
    fmt.Printf("%T\n"u8, (~b).Done);
}

} // end main_package
