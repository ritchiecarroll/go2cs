using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// Package initialization's frame names (runtime/managed_impl.cs, goFrameName / goInitFrameName), as Go
// 1.24.13 spells them, measured with a three-file probe (the InitFrameNames behavioral guard):
//
//   a package-level var initializer          -> pkg.init
//   a func literal inside one                -> pkg.init.func1, pkg.init.func2, ...
//   the package's init functions             -> pkg.init.0, pkg.init.1, ... (files by name, then declaration order)
//   a func literal inside an init function   -> pkg.init.2.func1
//
// The converter emits a var initializer into the package class's static constructor (`.cctor`) and the
// init functions as [GoInit] -- module-initializer -- methods `init`, `initΔ1`, `initΔ2`, ..., numbered
// in that same order. Before this rule the frames read `pkg..cctor`, `pkg..cctor.func0`, `pkg.init`,
// `pkg.initΔ2`; pkg/errors' TestFrameFormat / TestFrameMarshalText / TestFrameMarshalJSON print `%n` of
// a var initializer's frame and differed from Go on exactly that.
//
// The classes at the foot of this file are hand-written stand-ins for the emitted shapes, named as the
// converter names them; the CallerFrameTestVariantNamingTests helper reports its caller's Go name.
namespace GolibTests
{
    [TestClass]
    public class InitFrameNameTests
    {
        [TestMethod]
        public void AVarInitializerFrameIsThePackagesInit()
        {
            Assert.AreEqual("initguard/probe.init", go.initguard.probe_package.direct);
        }

        [TestMethod]
        public void ALiteralInAVarInitializerIsInitFuncN()
        {
            Assert.AreEqual("initguard/probe.init.func1", go.initguard.probe_package.viaLiteral);
        }

        [TestMethod]
        public void TheInitFunctionsAreInitDotN()
        {
            go.initguard.probe_package.init();
            go.initguard.probe_package.initΔ2();

            Assert.AreEqual("initguard/probe.init.0", go.initguard.probe_package.firstInit);
            Assert.AreEqual("initguard/probe.init.2", go.initguard.probe_package.thirdInit);
        }

        [TestMethod]
        public void ALiteralInAnInitFunctionCarriesItsInitDotN()
        {
            go.initguard.probe_package.initΔ2();

            // The literal's own counter has no GoPositionMap record in this file, so only the
            // enclosing name is asserted; the behavioral guard pins the full `init.2.func1`.
            StringAssert.StartsWith(go.initguard.probe_package.literalInThirdInit, "initguard/probe.init.2.func");
        }

        [TestMethod]
        public void ControlAPlainMethodNamedInitKeepsItsName()
        {
            // Not a module initializer: a Go METHOD or helper spelled `init` is not package initialization.
            Assert.AreEqual("initguard/probe2.init", go.initguard.probe2_package.init());
        }
    }
}

namespace go.initguard
{
    public static class probe_package
    {
        // Run by the static constructor: Go's `init` frame.
        internal static readonly string direct = GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();

        internal static readonly string viaLiteral =
            ((Func<string>)([MethodImpl(MethodImplOptions.NoInlining)] () => GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName()))();

        internal static string firstInit = string.Empty;

        internal static string thirdInit = string.Empty;

        internal static string literalInThirdInit = string.Empty;

        // The converter's first init function. Module initializers also run at load; the tests call
        // them again directly, which is all the frame name needs.
        [ModuleInitializer]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void init() => firstInit = GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();

        [ModuleInitializer]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void initΔ2()
        {
            thirdInit = GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();

            Func<string> literal = [MethodImpl(MethodImplOptions.NoInlining)] () => GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();

            literalInThirdInit = literal();
        }
    }

    public static class probe2_package
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string init() => GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();
    }
}
