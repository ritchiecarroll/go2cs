using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// Go's UNEXPORTED-method rule: an interface method whose name is unexported can only be satisfied
// by a method declared in the INTERFACE's own package — two same-named unexported methods from
// different packages are DIFFERENT methods. go/ast's `Expr { Pos(); End(); exprNode() }` is the
// canonical consumer (the marker-method pattern): a foreign type carrying its own private
// `exprNode` must NOT implement it, which is exactly what internal/reflectlite's TestImplements
// asserts with `*reflectlite_test.notAnExpr` — and what the name+signature structural probe
// answered TRUE for. A false positive in an implements relation is read by every caller as
// permission, the most dangerous shape the bridge produces.
//
// The fixtures are shaped like converted emission: a top-level `<pkg>_package` static class holds
// the interface / the receiver type, and the receiver's methods are extension methods declared IN
// its package class — which is what gives the probe the two package identities to compare
// (GoReflect.GoPackageClassPath over the method's declaring class).
[TestClass]
public class GoUnexportedMethodPackageTests
{
    // The foreign satisfier: same names, same signatures, DIFFERENT package for the unexported
    // marker method. Go: does not implement.
    [TestMethod]
    public void AForeignTypesOwnUnexportedMethodDoesNotSatisfyTheInterface()
    {
        Assert.IsFalse(typeof(ж<markerimpl_package.ForeignNode>).StructurallyImplements(typeof(markerdecl_package.markedExpr)),
            "an unexported interface method is satisfiable only from the interface's own package");
    }

    // The same-package satisfier: identical shape, declared beside the interface. Go: implements.
    [TestMethod]
    public void ASamePackageUnexportedMethodSatisfiesTheInterface()
    {
        Assert.IsTrue(typeof(ж<markerdecl_package.HomeNode>).StructurallyImplements(typeof(markerdecl_package.markedExpr)),
            "the interface's own package satisfies its unexported method");
    }

    // A PROMOTED unexported method. Go: a struct that embeds a type of its own package has that type's unexported
    // methods in its method set, so it satisfies its package's marker interface (net's `*TCPConn` is a
    // `buffersWriter` through the embedded `conn`). From 1.26.10.4 go2cs-gen writes the forwarder in a class of
    // the struct's own, marked [GoPromotedHost], and no longer in the package class: the forwarder's package is
    // its STRUCT's, read through the attribute.
    [TestMethod]
    public void APromotedUnexportedMethodSatisfiesTheInterfaceOfItsStructsPackage()
    {
        Assert.IsTrue(typeof(ж<markerdecl_package.OuterNode>).StructurallyImplements(typeof(markerdecl_package.markedExpr)),
            "a forwarder in a struct's promoted-method class is a method of the struct's package");
    }

    // The control for the test above: the attribute names the package, it does not waive the rule. A foreign
    // struct's promoted-method class is read (it satisfies the all-exported interface), and its unexported
    // forwarder still does not satisfy another package's marker.
    [TestMethod]
    public void AForeignStructsPromotedUnexportedMethodDoesNotSatisfyTheInterface()
    {
        Assert.IsTrue(typeof(ж<markerimpl_package.ForeignOuter>).StructurallyImplements(typeof(markerdecl_package.PlainNode)),
            "the foreign struct's promoted-method class is read: its exported forwarder satisfies");
        Assert.IsFalse(typeof(ж<markerimpl_package.ForeignOuter>).StructurallyImplements(typeof(markerdecl_package.markedExpr)),
            "a promoted unexported method is satisfiable only from the interface's own package");
    }

    // A DECLARED unexported method. The methods a type declares are in a class of the type's own, marked
    // [GoMethodHost], and no longer in the package class: the method's package is its receiver TYPE's, read
    // through the attribute. Read as the holder's own class it matched no package, and a type stopped satisfying
    // its own package's marker interface.
    [TestMethod]
    public void AnUnexportedMethodOfAMethodHolderSatisfiesTheInterfaceOfItsTypesPackage()
    {
        Assert.IsTrue(typeof(ж<markerdecl_package.HeldNode>).StructurallyImplements(typeof(markerdecl_package.markedExpr)),
            "a method in a type's method holder is a method of the type's package");
    }

    // The control for the test above: a foreign type's holder is read (its exported method satisfies), and its
    // unexported method still does not satisfy another package's marker.
    [TestMethod]
    public void AForeignMethodHoldersUnexportedMethodDoesNotSatisfyTheInterface()
    {
        Assert.IsTrue(typeof(ж<markerimpl_package.ForeignHeld>).StructurallyImplements(typeof(markerdecl_package.PlainNode)),
            "the foreign type's method holder is read: its exported method satisfies");
        Assert.IsFalse(typeof(ж<markerimpl_package.ForeignHeld>).StructurallyImplements(typeof(markerdecl_package.markedExpr)),
            "an unexported method is satisfiable only from the interface's own package");
    }

    // EXPORTED methods carry no package constraint — the foreign type still satisfies an
    // all-exported interface cross-package (ordinary Go duck typing).
    [TestMethod]
    public void ExportedMethodsKeepCrossPackageSatisfaction()
    {
        Assert.IsTrue(typeof(ж<markerimpl_package.ForeignNode>).StructurallyImplements(typeof(markerdecl_package.PlainNode)),
            "exported interface methods stay satisfiable from any package");
    }
}

// "package markerdecl" — declares the marker interface, an all-exported control interface, and a
// same-package satisfier.
public static class markerdecl_package
{
    public interface markedExpr
    {
        nint Pos();
        void exprNode();
    }

    public interface PlainNode
    {
        nint Pos();
    }

    public struct HomeNode;

    // Embeds HomeNode, so Go promotes Pos and exprNode to it (markerdeclᴛOuterNodeᴛpkg, below).
    public struct OuterNode;

    // Declares Pos and exprNode in its method holder (markerdeclᴛHeldNode, below).
    public struct HeldNode;

    [GoRecv] public static nint Pos(this ref HomeNode n) => 1;

    public static nint Pos(this ж<HomeNode> n) => 1;

    [GoRecv] public static void exprNode(this ref HomeNode n) { }

    public static void exprNode(this ж<HomeNode> n) { }
}

// "package markerimpl" — a foreign type with the SAME method names and signatures, its own
// private exprNode.
public static class markerimpl_package
{
    public struct ForeignNode;

    // Embeds ForeignNode, so Go promotes Pos and exprNode to it (markerimplᴛForeignOuterᴛpkg, below).
    public struct ForeignOuter;

    // Declares Pos and exprNode in its method holder (markerimplᴛForeignHeld, below).
    public struct ForeignHeld;

    [GoRecv] public static nint Pos(this ref ForeignNode n) => 2;

    public static nint Pos(this ж<ForeignNode> n) => 2;

    [GoRecv] public static void exprNode(this ref ForeignNode n) { }

    public static void exprNode(this ж<ForeignNode> n) { }
}

// The promoted methods of markerdecl's OuterNode, in the shape go2cs-gen emits from 1.26.10.4: a class of the
// struct's own beside the package class, found by its attribute.
[GoPromotedHost(typeof(markerdecl_package.OuterNode))]
public static class markerdeclᴛOuterNodeᴛpkg
{
    public static nint Pos(this ж<markerdecl_package.OuterNode> n) => 1;

    internal static void exprNode(this ж<markerdecl_package.OuterNode> n) { }
}

// The promoted methods of markerimpl's ForeignOuter.
[GoPromotedHost(typeof(markerimpl_package.ForeignOuter))]
public static class markerimplᴛForeignOuterᴛpkg
{
    public static nint Pos(this ж<markerimpl_package.ForeignOuter> n) => 2;

    internal static void exprNode(this ж<markerimpl_package.ForeignOuter> n) { }
}

// The methods markerdecl's HeldNode declares, in the shape the converter emits: a class of the type's own
// beside the package class, found by its attribute, with go2cs-gen's ж twins beside the originals.
[GoMethodHost(typeof(markerdecl_package.HeldNode))]
public static class markerdeclᴛHeldNode
{
    public static nint Pos(this ref markerdecl_package.HeldNode n) => 1;

    public static nint Pos(this ж<markerdecl_package.HeldNode> n) => 1;

    internal static void exprNode(this ref markerdecl_package.HeldNode n) { }

    internal static void exprNode(this ж<markerdecl_package.HeldNode> n) { }
}

// The methods markerimpl's ForeignHeld declares.
[GoMethodHost(typeof(markerimpl_package.ForeignHeld))]
public static class markerimplᴛForeignHeld
{
    public static nint Pos(this ref markerimpl_package.ForeignHeld n) => 2;

    public static nint Pos(this ж<markerimpl_package.ForeignHeld> n) => 2;

    internal static void exprNode(this ref markerimpl_package.ForeignHeld n) { }

    internal static void exprNode(this ж<markerimpl_package.ForeignHeld> n) { }
}
