using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// <c>Must()</c>: the consumer bridge from a Go <c>(value, error)</c> result to a value or an exception.
/// A failed <c>Must</c> is Go's <c>panic(err)</c>, as <c>template.Must</c> is, so the exception is a
/// <see cref="PanicException"/> whose value is the original error, and <see cref="GoErrorException.Err"/>
/// hands that same error back.
/// </summary>
[TestClass]
public class MustTests
{
    private sealed class TestError(string text) : error
    {
        public @string Error() => text;
    }

    private static readonly error s_failure = new TestError("disk full");

    private static (nint, error) Parse(bool fail) => fail ? (0, s_failure) : (42, null!);

    private static (@string, nint, error) Pair(bool fail) => fail ? ("", 0, s_failure) : ("go", 2, null!);

    private static (@string, nint, bool, error) Triple(bool fail) => fail ? ("", 0, false, s_failure) : ("go", 2, true, null!);

    [TestMethod]
    public void MustReturnsTheValueWhenTheErrorIsNil()
    {
        Assert.AreEqual((nint)42, Parse(false).Must());
        Assert.AreEqual(((@string)"go", (nint)2), Pair(false).Must());
        Assert.AreEqual(((@string)"go", (nint)2, true), Triple(false).Must());

        error none = null!;
        none.Must();
    }

    [TestMethod]
    public void MustThrowsTheOriginalErrorWhenItIsNotNil()
    {
        GoErrorException value = Assert.ThrowsException<GoErrorException>(() => Parse(true).Must());
        Assert.AreSame(s_failure, value.Err);
        Assert.AreEqual("disk full", value.Message);

        Assert.AreSame(s_failure, Assert.ThrowsException<GoErrorException>(() => Pair(true).Must()).Err);
        Assert.AreSame(s_failure, Assert.ThrowsException<GoErrorException>(() => Triple(true).Must()).Err);
        Assert.AreSame(s_failure, Assert.ThrowsException<GoErrorException>(() => s_failure.Must()).Err);
    }

    // Go's Must panics with the error, so the exception is that panic: its value is the error itself,
    // and a Go recover() between the throw and the catch returns it.
    [TestMethod]
    public void AFailedMustIsAGoPanicWhoseValueIsTheError()
    {
        GoErrorException failed = Assert.ThrowsException<GoErrorException>(() => s_failure.Must());

        Assert.IsInstanceOfType(failed, typeof(PanicException));
        Assert.AreSame(s_failure, failed.State);
    }

    [TestMethod]
    public void GoErrorExceptionRefusesANilError()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new GoErrorException(null!));
    }
}
