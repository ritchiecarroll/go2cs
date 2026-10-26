// Seat #7 A/B probe. Each scenario reaches a site the seat changed, in the shape the converter emits:
// defer/recover and a re-panic (GoFrame.ReRaisedByTheDeferredDelegate), a recovered bounds panic
// (RuntimePanicCheck.RaisedInRuntimePackage), goroutine launches (Goroutine.CreatorFrame), and
// GoArrayZero.New<T, TDims> (GoArrayDims). Output goes to stdout only; the arms are compared byte for byte.
using System;
using go;
using static go.builtin;

public static class Program
{
    private sealed class ᴀ4 : IGoArrayDims { public static nint[] Dims { get; } = [4]; }
    private sealed class ᴀ0 : IGoArrayDims { public static nint[] Dims { get; } = []; }

    private static string Show(object e) => e is null ? "<nil>" : e is error err ? (string)err.Error() : e.ToString();

    private static void RecoverDirect()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(() => { var e = recover(); Console.WriteLine("1 recovered: " + Show(e)); }, ref ᒐ);
            throw panic("boom");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void RecoverBounds()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(() => { var e = recover(); Console.WriteLine("2 recovered: " + Show(e)); }, ref ᒐ);
            slice<nint> s = new nint[] { 1, 2 }.slice();
            nint i = 5;
            Console.WriteLine(s[i]);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void PanicTwice()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(() => throw panic("second"), ref ᒐ);
            throw panic("first");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void RecoverRePanic()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(() => { var e = recover(); Console.WriteLine("3 recovered: " + Show(e)); }, ref ᒐ);
            PanicTwice();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void Handle()
    {
        var calledByDefer = GoFrame.TakeDeferredCall();
        var e = recover(calledByDefer);
        Console.WriteLine("4 recovered: " + Show(e));
    }

    private static void RecoverThroughHandler()
    {
        GoFrame ᒐ = default;
        try
        {
            defer(Handle, ref ᒐ, recovers: true);
            throw panic("handled");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void Goroutines()
    {
        var ch = new channel<nint>(0);
        goǃ(() => ch.ᐸꟷ((nint)42));
        Console.WriteLine("5 received: " + ᐸꟷ(ch));

        var done = new channel<@string>(0);
        goǃ(() =>
        {
            GoFrame ᒐ = default;
            try
            {
                defer(() => { var e = recover(); done.ᐸꟷ((@string)("6 goroutine recovered: " + Show(e))); }, ref ᒐ);
                throw panic("in goroutine");
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Console.WriteLine((string)ᐸꟷ(done));
    }

    private static string Zero<T, Tᴀ>() where Tᴀ : IGoArrayDims
    {
        ж<T> p = GoArrayZero.New<T, Tᴀ>();
        return p.ValueSlot is array<nint> a ? $"array len {a.Length}: [{a[0]} {a[3]}]" : $"{p.ValueSlot}";
    }

    public static int Main()
    {
        RecoverDirect();
        RecoverBounds();
        RecoverRePanic();
        RecoverThroughHandler();
        Goroutines();
        Console.WriteLine("7 new(nint): " + Zero<nint, ᴀ0>());
        Console.WriteLine("8 new([4]nint): " + Zero<array<nint>, ᴀ4>());
        Console.WriteLine("done");
        return 0;
    }
}
