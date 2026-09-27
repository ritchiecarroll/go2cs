using System;
using System.Runtime.CompilerServices;
using static go.runtime_package;

// Go-source frames for the method-expression wrapper guards (runtime's callers() counts a method as a Go
// frame when its top-level type is a `*_package` class in namespace go). The lambdas are spelled the way
// the converter emits Go's two wrapper-shaped method expressions.
namespace go;

internal static class wrapperprobe_package
{
    internal interface I
    {
        slice<uintptr> M();
    }

    internal sealed class impl : I
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public slice<uintptr> M() => callersHere();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static slice<uintptr> callersHere()
    {
        slice<uintptr> pcs = new(32);
        return pcs[..(int)Callers(0, pcs)];
    }

    internal interface IStack
    {
        @string S();
    }

    internal sealed class stackImpl : IStack
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public @string S()
        {
            slice<byte> buf = new(8192);
            return (@string)buf[..(int)Stack(buf, false)];
        }
    }

    // IStack.S, the wrapper that calls an ordinary function: elided from runtime.Stack's text too.
    internal static readonly Func<IStack, @string> stackWrapper = ((Func<IStack, @string>)([GoWrapper("IStack.S")] (p0) => p0.S()));

    // An ordinary function value, for the reflect-token arm.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void plain() { }

    // I.M, as convSelectorExpr emits an interface method expression.
    internal static readonly Func<I, slice<uintptr>> interfaceWrapper = ((Func<I, slice<uintptr>>)([GoWrapper("I.M")] (p0) => p0.M()));
}
