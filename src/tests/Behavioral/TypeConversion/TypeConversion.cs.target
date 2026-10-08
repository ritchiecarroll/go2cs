namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Person_Address /*dyn*/ {
    public @string Street;
    public @string City;
}

partial struct Person {
    public @string Name;
    public ж<Person_Address> Address;
}

internal partial struct main_data_Address /*dyn*/ {
    public @string Street; /*`json:"street"`*/
    public @string City; /*`json:"city"`*/
}

internal partial struct main_data /*dyn*/ {
    public @string Name; /*`json:"name"`*/
    public ж<main_data_Address> Address; /*`json:"address"`*/
}

internal static void Main() {
    ж<main_data> data = default!;
    ref var mine = ref heap(new Person(), out var Ꮡmine);
    ж<Person> person = ((ж<Person>)(data?.Value ?? default!));
    person = Ꮡmine;
    fmt.Println(mine == person.Value);
    fmt.Println(slice<rune>(((@string)"白鵬翔"u8)));
}

} // end main_package
