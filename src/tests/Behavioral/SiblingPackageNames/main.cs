namespace go;

using fmt = fmt_package;
using foo1 = SiblingPackageNames.teststructs.foo1.foo_package;
using foo2 = SiblingPackageNames.teststructs.foo2.foo_package;
using SiblingPackageNames.teststructs.foo1;
using SiblingPackageNames.teststructs.foo2;
using foo = SiblingPackageNames.teststructs.foo1.foo_package;

partial class main_package {

internal static void Main() {
    fmt.Println(foo1.Name(), foo2.Name());
    foo1.Pair a = new foo1.Pair(Left: 1, Right: 2);
    var b = new foo2.Triple(A: "x"u8, B: "y"u8, C: "z"u8);
    fmt.Println(a.Sum(), b.Join());
    foo1ꓸAlias c = new foo1.Pair(Left: 3, Right: 4);
    foo2ꓸAlias d = new foo2.Triple(A: "p"u8, B: "q"u8, C: "r"u8);
    fmt.Println(c.Sum(), d.Join());
}

} // end main_package
