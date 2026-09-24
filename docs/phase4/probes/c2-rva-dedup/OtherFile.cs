using System.Runtime.CompilerServices;

// A second SOURCE FILE of the same compilation, the way a generated module initializer would sit beside
// the converted files whose literals it registers.
static class OtherFile
{
    [MethodImpl(MethodImplOptions.NoInlining)] public static nint Abc() => Program.Addr("abc"u8);
    [MethodImpl(MethodImplOptions.NoInlining)] public static nint N() => Program.Addr("n"u8);
}
