// InterceptedGoCreatorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests
{
    /// <summary>
    /// Guards the <c>created by</c> line of a goroutine whose <c>go</c> statement go2cs-gen intercepts. The converter records
    /// a goroutine it proved cannot recurse in package_info.cs, and go2cs-gen intercepts that <c>goǃ</c> call to start it on
    /// the bounded reservation, so a frame of go2cs-gen's own sits between the <c>go</c> statement and golib's launcher. The
    /// creator is the first frame above the launcher, so without a skip that frame would be named as the creator and every
    /// such goroutine's traceback would end with a go2cs-gen method instead of the function holding the <c>go</c> statement.
    /// Red before Goroutine.CreatorFrame skipped go2cs-gen's interceptor frames.
    /// </summary>
    [TestClass]
    public class InterceptedGoCreatorTests
    {
        [TestMethod]
        public void AnInterceptedGoStatementNamesTheFunctionHoldingIt()
        {
            string child = "";
            int goLine = 0, reserve = 0;

            Task.Run(() =>
            {
                using Goroutine.Scope scope = Goroutine.Enter();
                (child, goLine, reserve) = interceptedgo_package.spawn();
            }).GetAwaiter().GetResult();

            Match createdBy = Regex.Match(child, @"\ncreated by (\S+) in goroutine (\d+)\n\t(.+):(\d+)\n$");
            Assert.IsTrue(createdBy.Success, "the child's block must end with created by and a position line: " + child);
            Assert.AreEqual("interceptedgo.spawn", createdBy.Groups[1].Value, child);
            StringAssert.EndsWith(createdBy.Groups[3].Value, "InterceptedGoCreatorTests.cs", child);
            Assert.AreEqual(goLine.ToString(), createdBy.Groups[4].Value, "the position is the go statement's line: " + child);

            // The interception still starts the goroutine on the bounded reservation.
            Assert.AreEqual(Goroutine.BoundedStackReserve, reserve, "the intercepted goroutine ran on a worker of another reserve");
        }
    }
}

namespace go
{
    // A Go-source frame (a `*_package` class in namespace go, see TracebackDecorationProbe.cs), so it prints as
    // `interceptedgo.spawn`.
    internal static class interceptedgo_package
    {
        // func spawn() (child string) {
        //     done := make(chan struct{})
        //     go func() { child = tracebackprobe.StackText(); close(done) }()   // proved bounded, so intercepted
        //     <-done
        // }
        // goLine is the C# line of the go statement (this file has no Go position map, so the traceback names the C#
        // position, the same fallback every frame of an unmapped file takes).
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static (string child, int goLine, int reserve) spawn()
        {
            string child = "";
            int reserve = 0;
            using ManualResetEventSlim done = new();

            int goLine = here(); go2cs.Interceptors.BoundedGoInterceptorProbe.goǃ(() =>
            {
                child = tracebackprobe_package.StackText();
                reserve = GoroutineThreadPool.CurrentStackReserve;
                done.Set();
            });

            done.Wait();
            return (child, goLine, reserve);
        }

        private static int here([CallerLineNumber] int line = 0) => line;
    }
}

namespace go2cs.Interceptors
{
    // The shape of a go2cs-gen interceptor of an unmarked goǃ rung (BoundedGoroutineInterceptorGenerator): a real frame
    // (NoInlining here, so the guard does not depend on the JIT's choice), hidden from .NET's stack trace, forwarding to
    // the GoStack-marked rung with the intercepted call's own caller information.
    internal static class BoundedGoInterceptorProbe
    {
        [GeneratedCode("go2cs-gen", "probe")]
        [StackTraceHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void goǃ(Action action, [CallerFilePath] string ᴛcallerFile = "", [CallerLineNumber] int ᴛcallerLine = 0) =>
            builtin.goǃ(action, GoStack.Bounded, ᴛcallerFile, ᴛcallerLine);
    }
}
