// A public Go package: no GO2CS0014.
public static class Program
{
    public static int Main()
    {
        nint width = go.unicode.utf8_package.RuneLen('a');
#if PUBLIC_ONLY
        int version = 0;
#else
        int version = Internal.Version();
#endif
        System.Console.WriteLine($"INTERNAL-PACKAGE-WARNING: {(width == 1 ? "ok" : "wrong")} {version}");
        return width == 1 ? 0 : 1;
    }
}
