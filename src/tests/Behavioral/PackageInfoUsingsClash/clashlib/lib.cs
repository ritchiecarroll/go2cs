namespace go.PackageInfoUsingsClash;

using fmt = fmt_package;
using strings = strings_package;

partial class clashlib_package {

partial struct MethodImpl {
    internal nint n;
}

internal static nint Get⁔(/*this*/ MethodImpl m) { return m.n; }

internal static void Bump⁔(/*this*/ ref MethodImpl m, nint by) { m.n += by; }

partial struct StackTrace /*[]@string*/;

internal static @string String⁔(/*this*/ StackTrace s) { return strings.Join(s, " <- "); }

internal static void Push⁔(/*this*/ ref StackTrace s, @string frame) { s = append(s, frame); }

internal static fmt.Stringer Describe⁔(/*this*/ StackTrace s) { return s; }

public static MethodImpl New(nint n) { return new MethodImpl(n: n); }

} // end clashlib_package
