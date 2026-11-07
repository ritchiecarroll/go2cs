#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// AdapterBinder binds an interface's VARIADIC members for the object shell through a bound delegate built by compiled
// code, so a struct, or a pointer to one, asserts to an interface with a variadic method without the generic shell.
// Native AOT cannot close the generic shell over a struct (MakeGenericType has no native code there), and before the
// object shell carried variadic members every such assertion read false under AOT where Go and the JIT read true
// (docs/phase4/DATA-aot-variadic-shell.md). Each interface here carries a generic shell that THROWS when closed, as it
// does under AOT, so these run the AOT path under the JIT. The types are unique to this class because the binder's
// decisions are process-wide and never cleared.
[TestClass]
public class VariadicShellBindingTests
{
    private static vshell_package.ILog? Shell(object value) =>
        AdapterBinder.TryCreate(value, typeof(vshell_package.ILog), out object? shell) ? (vshell_package.ILog)shell : null;

    [TestMethod]
    public void AValueBindsThroughItsMethodSetArm()
    {
        vshell_package.ILog? shell = Shell(new vshell_package.Rec { n = 3 });

        Assert.IsNotNull(shell, "a value binds through the object shell, with no generic shell");
        Assert.IsInstanceOfType(shell, typeof(vshell_package.ΔLogᴛObj));
        Assert.AreEqual("Rec.Log 3 a,1", vshell_package.Call(() => shell.Log("a", (nint)1)));
        Assert.AreEqual((nint)3, shell.Count(), "a member the invoker carries is unchanged");
    }

    [TestMethod]
    public void APointerBindsThroughItsMethodSetArmAndReadsThePointeeAtTheCall()
    {
        ж<vshell_package.RecP> box = Ꮡ(new vshell_package.RecP { n = 4 });
        vshell_package.ILog? shell = Shell(box);

        Assert.IsNotNull(shell, "a pointer binds through the object shell when the generic shell cannot be closed");
        box.Value.n = 5;
        Assert.AreEqual("RecP.Log 5 x", vshell_package.Call(() => shell.Log("x")), "the call reads the pointee written after the bind");
    }

    [TestMethod]
    public void AHandOwnedPointerBindsThroughItsBoxOverload()
    {
        ж<vshell_package.Hand> box = Ꮡ(new vshell_package.Hand { n = 6 });
        vshell_package.ILog? shell = Shell(box);

        Assert.IsNotNull(shell, "a type with no generated method set binds its pointer methods through their ж overload");
        Assert.AreEqual("Hand.Log 6 ", vshell_package.Call(() => shell.Log()));
        Assert.AreEqual((nint)6, shell.Count());
    }

    [TestMethod]
    public void AValueOfAPointerMethodSetIsAMiss()
    {
        Assert.IsNull(Shell(new vshell_package.HandV { n = 7 }), "pointer methods are not in a value's method set (Go's rule)");
    }

    [TestMethod]
    public void AnArmOfAnotherDelegateTypeIsAMiss()
    {
        Assert.IsNull(Shell(new vshell_package.Skewed { n = 8 }), "a bound arm the shell would cast wrongly is refused before a shell is handed out");
    }
}

public static class vshell_package
{
    [GoInterfaceShell(typeof(ΔLog<>), typeof(ΔLogᴛObj), "Log", "Count",
        VariadicMethods = ["Log"], VariadicDelegates = [typeof(Actionꓸꓸꓸ<object>)])]
    public interface ILog
    {
        void Log(params Span<object> args);

        nint Count();
    }

    // Stands in for the generic shell under Native AOT: closing it over a struct fails, as MakeGenericType does there.
    public sealed class ΔLog<T> : ILog
    {
        static ΔLog() => throw new NotSupportedException("missing native code");

        public ΔLog(T value) { }

        public void Log(params Span<object> args) { }

        public nint Count() => 0;
    }

    // The object shell as go2cs-gen writes it for this interface.
    public sealed class ΔLogᴛObj : ILog, IInterfaceAdapter
    {
        private readonly object m_target;
        private readonly GoShellBinding m_binding;
        private readonly Delegate?[]? m_variadic;

        public ΔLogᴛObj(object target, GoShellBinding binding)
        {
            m_target = target;
            m_binding = binding;
            m_variadic = binding.BindVariadic(target);
        }

        object? IInterfaceAdapter.Value => m_target;

        public void Log(params Span<object> args) => ((Actionꓸꓸꓸ<object>)m_variadic![0]!)(args);

        public nint Count() => (nint)m_binding.Invoke(1, m_target)!;
    }

    private static readonly List<string> s_calls = [];

    public static string Call(Action call)
    {
        lock (s_calls)
        {
            s_calls.Clear();
            call();
            return string.Join("|", s_calls);
        }
    }

    private static void Record(string text) => s_calls.Add(text);

    private static string Text(Span<object> args)
    {
        List<string> parts = [];

        foreach (object arg in args)
            parts.Add(arg.ToString()!);

        return string.Join(",", parts);
    }

    // A converted value type: value methods, with the generated method set's bound arms.
    public struct Rec : IGoMethodSet
    {
        public nint n;

        readonly Delegate? IGoMethodSet.ᴛMethod(string method, bool pointer, bool bound, object? receiver)
        {
            if (!bound || method != "Log")
                return null;

            if (pointer)
            {
                ж<Rec> p = (ж<Rec>)receiver!;
                return (Actionꓸꓸꓸ<object>)(args => Log(p.Value, args));
            }

            Rec r = (Rec)receiver!;
            return (Actionꓸꓸꓸ<object>)(args => Log(r, args));
        }
    }

    public static void Log(this Rec r, params Span<object> args) => Record($"Rec.Log {r.n} {Text(args)}");

    public static nint Count(this Rec r) => r.n;

    // The same shape, asserted through a pointer.
    public struct RecP : IGoMethodSet
    {
        public nint n;

        readonly Delegate? IGoMethodSet.ᴛMethod(string method, bool pointer, bool bound, object? receiver)
        {
            if (!bound || method != "Log" || !pointer)
                return null;

            ж<RecP> p = (ж<RecP>)receiver!;
            return (Actionꓸꓸꓸ<object>)(args => Log(p.Value, args));
        }
    }

    public static void Log(this RecP r, params Span<object> args) => Record($"RecP.Log {r.n} {Text(args)}");

    public static nint Count(this RecP r) => r.n;

    // A hand-owned type, as testing's T is: no generated method set, pointer methods written with a ж overload.
    public struct Hand
    {
        public nint n;
    }

    [GoRecv] public static void Log(this ref Hand h, params Span<object> args) => Record($"Hand.Log {h.n} {Text(args)}");

    public static void Log(this ж<Hand> h, params Span<object> args) => Log(ref h.Value, args);

    [GoRecv] public static nint Count(this ref Hand h) => h.n;

    public static nint Count(this ж<Hand> h) => Count(ref h.Value);

    public struct HandV
    {
        public nint n;
    }

    [GoRecv] public static void Log(this ref HandV h, params Span<object> args) => Record($"HandV.Log {h.n}");

    public static void Log(this ж<HandV> h, params Span<object> args) => Log(ref h.Value, args);

    [GoRecv] public static nint Count(this ref HandV h) => h.n;

    public static nint Count(this ж<HandV> h) => Count(ref h.Value);

    // A method set whose Log arm is of a delegate type the shell would not cast to.
    public struct Skewed : IGoMethodSet
    {
        public nint n;

        readonly Delegate? IGoMethodSet.ᴛMethod(string method, bool pointer, bool bound, object? receiver) =>
            bound && method == "Log" ? new Action<object[]>(_ => { }) : null;
    }

    public static void Log(this Skewed s, params Span<object> args) { }

    public static nint Count(this Skewed s) => s.n;
}
