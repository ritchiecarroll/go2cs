using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards the calling goroutine's <c>created by</c> line and the position beneath it (census family A7,
/// the created-by half; COORD ruling 2026-09-29 08:46). Go ends every goroutine's block with
/// <c>created by &lt;fn&gt; in goroutine &lt;parent&gt;</c> and, on the next line, the tab-indented
/// position of the <c>go</c> statement (traceback.go's printcreatedby1); runtime's parseTraceback
/// requires that line, and TestTracebackParentChildGoroutines reads the parent id from the first.
/// Red against 0b7492b33a, which prints neither for the calling goroutine.
/// </summary>
[TestClass]
public class CreatedByPositionTests
{
    [TestMethod]
    public void TheCallingGoroutinesBlockEndsWithItsCreatorAndTheGoStatementsPosition()
    {
        string parent = "", child = "";
        int goLine = 0;

        Task.Run(() =>
        {
            using Goroutine.Scope scope = Goroutine.Enter();
            (parent, child, goLine) = tracebackdeco_package.spawn();
        }).GetAwaiter().GetResult();

        string parentId = Regex.Match(parent, @"^goroutine (\d+) \[").Groups[1].Value;
        Assert.AreNotEqual("", parentId, parent);

        Match createdBy = Regex.Match(child, @"\ncreated by (\S+) in goroutine (\d+)\n\t(.+):(\d+)\n$");
        Assert.IsTrue(createdBy.Success, "the child's block must end with created by and a position line: " + child);
        Assert.AreEqual("tracebackdeco.spawn", createdBy.Groups[1].Value, child);
        Assert.AreEqual(parentId, createdBy.Groups[2].Value, child);
        StringAssert.EndsWith(createdBy.Groups[3].Value, "TracebackDecorationProbe.cs", child);
        Assert.AreEqual(goLine.ToString(), createdBy.Groups[4].Value, "the position is the go statement's line: " + child);

        // A goroutine the host entered has no creator, as Go prints none for goroutine 1.
        Assert.IsFalse(parent.Contains("created by"), parent);
    }
}
