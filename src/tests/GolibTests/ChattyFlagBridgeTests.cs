using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using go.testing_runtime;
using any = System.Object;

namespace GolibTests;

/// <summary>
/// The host's <c>-test.v</c> registration, in both directions.
/// </summary>
/// <remarks>
/// <para>
/// THE INVARIANT: <c>-test.v</c> is not a boolean. Go's <c>testing.Init</c> registers it as a
/// <c>chattyFlag</c> — <c>IsBoolFlag()</c> true, <c>Set</c> accepting <c>true</c>/<c>false</c>/
/// <c>test2json</c>, <c>Get()</c> answering a <c>bool</c> for the first two and the STRING
/// <c>"test2json"</c> for the third — and <c>testing</c>'s own <c>TestFlag</c> re-execs the binary
/// and asserts exactly that through <c>flag.Lookup("test.v").Value</c> in all three arms.
/// </para>
/// <para>
/// The host cannot NAME <c>flag.Value</c> (it must not reference <c>flag</c>; see
/// TestFlagBridge's remarks for the measured reasons), so it builds one through golib's
/// <see cref="AdapterBinder"/>.
/// </para>
/// <para>
/// <b>WHICH ARMS DISCRIMINATE, measured rather than asserted.</b> The control forces the
/// <c>flag.Bool</c> belt (<c>Registrar.Chatty</c> returning false unconditionally) and
/// <b>3 of 7 go red</b>: <c>TestVEqualsTest2Json…</c>, <c>AnUnknownTestVValue…</c> and
/// <c>TheRegisteredValueIsTheHostsOwnChattyFlag</c>. The other two behavioural arms stay GREEN
/// under the belt on purpose and are worth knowing about: <c>flag</c>'s own <c>boolValue</c>
/// also answers <c>IsBoolFlag()</c> and also accepts <c>false</c>, so bare <c>-test.v</c> and
/// <c>-test.v=false</c> are cases where Go's boolean behaviour and Go's tri-state behaviour
/// COINCIDE. They are the must-not-regress direction — the tri-state has to keep doing what the
/// boolean did — not evidence about the mechanism. A first draft of this remark claimed four
/// discriminating arms; the control said two, and the third was added afterwards to pin the
/// mechanism itself rather than only its observable effects.
/// </para>
/// <para>
/// This binds the REAL converted <c>flag</c> package, for the reason
/// <c>HostTestMainParseOrderTests</c> gives: what a flag package does with a custom
/// <c>Value</c> is only observable there. <c>flag.CommandLine</c> is process-global, so each arm
/// installs a fresh <c>ContinueOnError</c> set first — <c>ExitOnError</c> would take the MSTest
/// host down (route #8), and a leftover definition from another class would make
/// <c>Register</c> skip the very flag under test.
/// </para>
/// </remarks>
[TestClass]
public class ChattyFlagBridgeTests
{
    // This class swaps the process-global flag.CommandLine; every other in-process class shares it,
    // so the original is put back after each test (i9, 2026-09-27: the shared-CommandLine class).
    private ж<flag_package.FlagSet>? m_originalCommandLine;

    [TestInitialize]
    public void CaptureTheCommandLine() => m_originalCommandLine = flag_package.CommandLine;

    [TestCleanup]
    public void RestoreTheCommandLine()
    {
        if (m_originalCommandLine is not null)
            flag_package.CommandLine = m_originalCommandLine;
    }

    private const string TestV = "test.v";

    private static void FreshCommandLine()
    {
        flag_package.CommandLine = flag_package.NewFlagSet((@string)"guard", flag_package.ContinueOnError);

        // flag's failf calls f.usage() after printing the message, which would dump every one of
        // the ~40 definitions Register installs into the test log on the negative arm.
        flag_package.CommandLine.Value.Usage = () => { };
    }

    // The host's registration, with the run's own verbosity, onto the fresh set above.
    private static void Register(params string[] hostArgs) =>
        TestFlagBridge.Register(TestOptions.Parse(hostArgs));

    private static error Parse(params string[] arguments)
    {
        @string[] converted = new @string[arguments.Length];

        for (int i = 0; i < arguments.Length; i++)
            converted[i] = (@string)arguments[i];

        return flag_package.CommandLine.Parse(new slice<@string>(converted));
    }

    private static any TestVValue()
    {
        ж<flag_package.Flag> flag = flag_package.Lookup((@string)TestV);

        Assert.IsNotNull(flag, "the host did not define -test.v at all");

        Assert.IsTrue((~flag).Value._<flag_package.Getter>(out flag_package.Getter? getter),
            "-test.v's Value is not a flag.Getter, so testFlagHelper's `f.Value.(flag.Getter)` cannot resolve");

        return getter!.Get();
    }

    [TestMethod]
    public void TestVIsRegisteredAsAFlagGetterCarryingTheRunsOwnVerbosity()
    {
        FreshCommandLine();
        Register();

        // Nothing parsed yet: the value reports the verbosity the host was started with, which is
        // also what flag.Var recorded as DefValue.
        Assert.AreEqual(false, TestVValue(), "an unparsed -test.v should report the host's own (false) verbosity");
        Assert.AreEqual("false", (~flag_package.Lookup((@string)TestV)).DefValue.ToString());

        FreshCommandLine();
        Register("-test.v");

        Assert.AreEqual(true, TestVValue(), "a host started verbose should seed -test.v true, as testing.Init does");
        Assert.AreEqual("true", (~flag_package.Lookup((@string)TestV)).DefValue.ToString());
    }

    [TestMethod]
    public void BareTestVParsesAsABooleanAndDoesNotConsumeTheNextToken()
    {
        FreshCommandLine();
        Register();

        // IsBoolFlag() is what stops parseOne taking the following token as the value. A plain
        // string Value would swallow "trailing" here, which is why this arm exists separately.
        Assert.IsNull(Parse("-test.v", "trailing"), "parsing a bare -test.v failed");

        Assert.AreEqual(true, TestVValue());

        slice<@string> rest = flag_package.Args();

        Assert.AreEqual(1, (int)len(rest), "bare -test.v consumed the following argument, so IsBoolFlag() did not resolve");
        Assert.AreEqual("trailing", rest[0].ToString());
    }

    [TestMethod]
    public void TestVEqualsTest2JsonYieldsTheStringAndNotABoolean()
    {
        FreshCommandLine();
        Register();

        Assert.IsNull(Parse("-test.v=test2json"),
            "-test.v=test2json was rejected — the registration is not Go's tri-state chattyFlag");

        any value = TestVValue();

        Assert.IsInstanceOfType(value, typeof(@string),
            "Get() answered a bool for the test2json arm; Go's chattyFlag answers the string");
        Assert.AreEqual("test2json", value.ToString());
        Assert.AreEqual("test2json", (~flag_package.Lookup((@string)TestV)).Value.String().ToString());
    }

    [TestMethod]
    public void TestVEqualsFalseYieldsFalse()
    {
        FreshCommandLine();
        Register("-test.v");

        Assert.IsNull(Parse("-test.v=false"), "-test.v=false was rejected");

        Assert.AreEqual(false, TestVValue());
        Assert.AreEqual("false", (~flag_package.Lookup((@string)TestV)).Value.String().ToString());
    }

    [TestMethod]
    public void AnUnknownTestVValueIsRejectedWithGosOwnText()
    {
        FreshCommandLine();
        Register();

        error err = Parse("-test.v=bogus");

        Assert.IsNotNull(err, "-test.v=bogus was accepted");
        StringAssert.Contains(err.Error().ToString(), "invalid flag -test.v=bogus",
            "the rejection did not come from Go's chattyFlag.Set");
    }

    [TestMethod]
    public void TheRegisteredValueIsTheHostsOwnChattyFlagAndNotFlagsBoolValue()
    {
        FreshCommandLine();
        Register();

        // The MECHANISM, not its effects: Register falls back to flag.Bool when no custom Value
        // could be built, and that belt is deliberately silent (a hard failure there would take
        // down every one of the test projects whose package imports flag). This is what makes the
        // degradation loud at the gate instead. The assert runs through golib's own type-assertion
        // path, so it also exercises the unwrap a converted test's `f.Value.(flag.Getter)` takes.
        Assert.IsTrue((~flag_package.Lookup((@string)TestV)).Value._<ж<TestFlagBridge.ChattyFlag>>(out _),
            "-test.v is not backed by the host's ChattyFlag — Register fell back to the flag.Bool belt, so the tri-state is silently unavailable");
    }

    [TestMethod]
    public void TheTestHostStillCarriesNoCompileTimeFlagReference()
    {
        // The constraint the whole late-binding design exists for, pinned mechanically: a
        // testing -> flag ProjectReference does not deploy flag.dll beside the test hosts whose
        // own package does not import flag, and costs every test project's build a measured +33%.
        // Building the tri-state Value through golib rather than declaring it must not have
        // quietly reintroduced that edge.
        string[] referenced = typeof(testing_package).Assembly
            .GetReferencedAssemblies()
            .Select(name => name.Name ?? "")
            .ToArray();

        Assert.IsFalse(referenced.Contains("flag"),
            "the test host now references the converted flag package at compile time — the late-binding contract in TestFlagBridge's remarks is broken");
    }
}
