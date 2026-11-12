namespace go;

using fmt = fmt_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: LinknameVarPullLib_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class main_package {

internal static @string secret { get => go.LinknameVarPullLib_package.secret; set => go.LinknameVarPullLib_package.secret = value; }

internal static void main() {
    fmt.Println(secret);
}

} // end main_package
