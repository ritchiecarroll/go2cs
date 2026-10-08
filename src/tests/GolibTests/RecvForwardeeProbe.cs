using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

// Go-source methods for the receiver-forwarder naming guards (RecvForwardeeTests). Each pair is spelled the
// way converted code and go2cs-gen emit it since face lift A: the Go method on an UNMARKED by-reference
// receiver (`this ref T`, no [GoRecv]), and RecvGenerator's `this ж<T>` forwarder beside it, which is what a
// method expression `(*T).m` binds. The copy-bound pair is the value-set control: a `this ref` receiver
// marked [GoCopyBound] is a VALUE receiver bound through a copy, never a pointer receiver.
namespace go;

internal static class recvprobe_package
{
    internal struct T
    {
        internal nint n;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static nint pointerMethod(this ref T t) => t.n;

    [GeneratedCode("go2cs-gen", "1.24.13")]
    internal static nint pointerMethod(this ж<T> Ꮡt)
    {
        ref var t = ref Ꮡt.DerefOrNull();
        return t.pointerMethod();
    }

    internal struct V
    {
        internal nint n;
    }

    [GoCopyBound, MethodImpl(MethodImplOptions.NoInlining)]
    internal static nint copyBoundMethod(this ref V v) => v.n;

    [GeneratedCode("go2cs-gen", "1.24.13")]
    internal static nint copyBoundMethod(this ж<V> Ꮡv)
    {
        ref var v = ref Ꮡv.DerefOrNull();
        return v.copyBoundMethod();
    }
}
