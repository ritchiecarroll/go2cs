using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards runtime/managed_impl.cs goRecvForwardee, which maps RecvGenerator's <c>this ж&lt;T&gt;</c> forwarder
/// back to the Go method it forwards to, so a method expression <c>(*T).m</c> names that method as Go does:
/// <c>recvprobe.(*T).pointerMethod</c>. Face lift A made a pointer receiver an UNMARKED <c>this ref T</c>, and
/// the lookup still required [GoRecv]: the behavioral MethodValueFuncNames printed <c>main.pointerMethod</c>.
/// The lookup now follows golib's receiver rule (TypeExtensions.IsPointerSetByRefReceiver), so a
/// [GoCopyBound] by-ref receiver, a value receiver, is never the forwardee.
/// </summary>
[TestClass]
public class RecvForwardeeTests
{
    private static string NameOf(Delegate fn) =>
        (string)FuncForPC(reflect_package.ValueOf(fn).Pointer()).Name();

    [TestMethod]
    public void APointerMethodExpressionNamesItsUnmarkedByRefMethod() =>
        Assert.AreEqual("recvprobe.(*T).pointerMethod", NameOf((Func<ж<recvprobe_package.T>, nint>)recvprobe_package.pointerMethod));

    [TestMethod]
    public void TheForwardeeIsTheUnmarkedByRefMethod()
    {
        System.Reflection.MethodInfo forwarder = ProbeMethod("pointerMethod", typeof(ж<recvprobe_package.T>));
        System.Reflection.MethodInfo method = ProbeMethod("pointerMethod", typeof(recvprobe_package.T).MakeByRefType());

        Assert.IsFalse(method.IsDefined(typeof(GoRecvAttribute), inherit: false), "the probe is the face-lift form: no [GoRecv]");
        Assert.AreSame(method, RecvForwardee(forwarder), "the ж forwarder maps to the by-ref Go method");
    }

    [TestMethod]
    public void ACopyBoundByRefMethodIsNeverTheForwardee()
    {
        System.Reflection.MethodInfo forwarder = ProbeMethod("copyBoundMethod", typeof(ж<recvprobe_package.V>));
        System.Reflection.MethodInfo method = ProbeMethod("copyBoundMethod", typeof(recvprobe_package.V).MakeByRefType());

        Assert.IsTrue(method.IsDefined(typeof(GoCopyBoundAttribute), inherit: false), "the control's by-ref method is copy-bound");
        Assert.IsNull(RecvForwardee(forwarder), "a [GoCopyBound] `this ref V` is a value receiver, not the forwardee");
    }

    private static System.Reflection.MethodInfo ProbeMethod(string name, Type receiver)
    {
        System.Reflection.MethodInfo? method = typeof(recvprobe_package).GetMethod(name,
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, [receiver]);

        Assert.IsNotNull(method, $"probe method {name}({receiver.Name})");
        return method!;
    }

    // goRecvForwardee is private to the runtime's package class; read through reflection, as the other
    // runtime-internal guards here do.
    private static System.Reflection.MethodBase? RecvForwardee(System.Reflection.MethodBase forwarder) =>
        (System.Reflection.MethodBase?)typeof(runtime_package)
            .GetMethod("goRecvForwardee", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(null, [forwarder]);
}
