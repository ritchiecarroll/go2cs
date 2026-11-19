namespace go;

using fmt = fmt_package;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using static main_package;

partial class main_package {

partial struct aNode {
    internal nint k;
}

} partial class mainᴛaNode {

internal static ж<aNode> Root(this ж<aNode> Ꮡn) { return Ꮡn; }
internal static ж<aNode> Self(this ж<aNode> Ꮡn) { return Ꮡn; }

} partial class main_package {

partial struct aDefault {
    /*embed*/ internal aNode aNode;
    internal nint hits;
}

} partial class mainᴛaDefault {

internal static ж<aNode> Self(this ж<aDefault> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    d.hits++;
    return Ꮡd.ᏑaNode;
}

} partial class main_package {

partial struct aBase {
    /*embed*/ internal aDefault aDefault;
}

partial struct aSide {
    internal nint f;
}

} partial class mainᴛaSide {

internal static nint Flag(this ref aSide s) { return s.f; }

} partial class main_package {

partial struct aStmt {
    /*embed*/ internal aBase aBase;
    /*embed*/ internal aSide aSide;
}

partial struct aIf {
    /*embed*/ internal aStmt aStmt;
}

partial interface selfer {
    ж<aNode> Root();
    ж<aNode> Self();
}

partial struct bDeep {
}

} partial class mainᴛbDeep {

internal static @string Name(this ref bDeep d) { return "deep"; }

} partial class main_package {

partial struct bMid {
    /*embed*/ internal bDeep bDeep;
}

partial struct bFlat {
}

} partial class mainᴛbFlat {

internal static @string Name(this ref bFlat f) { return "flat"; }

} partial class main_package {

partial struct bBoth {
    /*embed*/ internal bMid bMid;
    /*embed*/ internal bFlat bFlat;
}

partial struct bOuter {
    /*embed*/ internal bBoth bBoth;
}

partial struct bFirst {
    /*embed*/ internal bFlat bFlat;
    /*embed*/ internal bMid bMid;
}

partial struct bFirstOuter {
    /*embed*/ internal bFirst bFirst;
}

partial interface namer {
    @string Name();
}

partial struct bFlow {
    internal nint n;
}

} partial class mainᴛbFlow {

internal static ж<bFlow> Data(this ж<bFlow> Ꮡf) { return Ꮡf; }

} partial class main_package {

partial struct bNode {
}

} partial class mainᴛbNode {

internal static ж<bFlow> Data(this ref bNode n) { return default; }

} partial class main_package {

partial struct bNodeBase {
    /*embed*/ internal bNode bNode;
}

[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct bStmt {
    [FieldOffset(0)] /*embed*/ internal readonly bNodeBase bNodeBase;
    [FieldOffset(0)] /*embed*/ internal bFlow bFlow;
}

partial struct bIf {
    /*embed*/ internal bStmt bStmt;
}

partial interface flowData {
    ж<bFlow> Data();
}

partial struct cNode {
    internal nint k;
}

} partial class mainᴛcNode {

internal static nint Visit(this ref cNode n) { return n.k; }

} partial class main_package {

partial struct cDefault {
    /*embed*/ internal cNode cNode;
}

} partial class mainᴛcDefault {

internal static nint Visit(this ж<cDefault> Ꮡd) {
    var p = Ꮡd.ᏑcNode;
    return p.k + 100;
}

} partial class main_package {

partial struct cBase {
    /*embed*/ internal cDefault cDefault;
}

partial struct cStmt {
    /*embed*/ internal cBase cBase;
}

partial interface visitor {
    nint Visit();
}

partial struct dLeft {
}

} partial class mainᴛdLeft {

internal static @string Tag(this ref dLeft l) { return "left"; }

} partial class main_package {

partial struct dRight {
}

} partial class mainᴛdRight {

internal static @string Tag(this ref dRight r) { return "right"; }

} partial class main_package {

partial struct dPair {
    /*embed*/ internal dLeft dLeft;
    /*embed*/ internal dRight dRight;
}

partial struct dTop {
}

} partial class mainᴛdTop {

internal static @string Tag(this ref dTop t) { return "top"; }

} partial class main_package {

partial struct dAll {
    /*embed*/ internal dPair dPair;
    /*embed*/ internal dTop dTop;
}

partial struct dOuter {
    /*embed*/ internal dAll dAll;
}

partial interface tagger {
    @string Tag();
}

partial struct eFar {
}

} partial class mainᴛeFar {

internal static nint Pick(this ref eFar f, nint n) { return n; }

} partial class main_package {

partial struct eVia {
    /*embed*/ internal eFar eFar;
}

partial struct eNear {
}

} partial class mainᴛeNear {

internal static @string Pick(this ref eNear n) { return "near"; }

} partial class main_package {

partial struct eFork {
    /*embed*/ internal eVia eVia;
    /*embed*/ internal eNear eNear;
}

partial struct eOuter {
    /*embed*/ internal eFork eFork;
}

partial interface picker {
    @string Pick();
}

partial struct fOne {
}

} partial class mainᴛfOne {

internal static @string Tie(this ref fOne o) { return "one"; }

} partial class main_package {

partial struct fTwo {
}

} partial class mainᴛfTwo {

internal static @string Tie(this ref fTwo t) { return "two"; }

} partial class main_package {

partial struct fBoth {
    /*embed*/ internal fOne fOne;
    /*embed*/ internal fTwo fTwo;
}

partial struct fDeep {
}

} partial class mainᴛfDeep {

internal static @string Tie(this ref fDeep d) { return "deep"; }

} partial class main_package {

partial struct fWrap {
    /*embed*/ internal fDeep fDeep;
}

partial struct fAll {
    /*embed*/ internal fBoth fBoth;
    /*embed*/ internal fWrap fWrap;
}

partial interface tier {
    @string Tie();
}

partial struct gFacts /*num:nint*/;

public partial struct gVisitor {
    internal nint visits;
}

partial interface gData {
    ж<gNode> AsNode();
    ж<gNode> VisitEachChild(ж<gVisitor> v);
    ж<gFlowBase> FlowNodeData();
    nint Modifiers();
    void setModifiers(nint m);
    gFacts computeSubtreeFacts();
    gFacts subtreeFactsWorker(gData self);
    gFacts propagateSubtreeFacts();
}

public partial struct gNode {
    public nint Kind;
    public ж<gNode> Parent;
    internal gData data;
}

} partial class mainᴛgNode {

public static ж<gNode> AsNode(this ж<gNode> Ꮡn) { return Ꮡn; }
public static ж<gNode> VisitEachChild(this ref gNode n, ж<gVisitor> v) { return n.data.VisitEachChild(v); }
public static ж<gFlowBase> FlowNodeData(this ref gNode n) { return n.data.FlowNodeData(); }
public static nint Modifiers(this ref gNode n) { return n.data.Modifiers(); }
internal static gFacts propagateSubtreeFacts(this ref gNode n) { return n.data.propagateSubtreeFacts(); }

} partial class main_package {

partial struct gDefault {
    /*embed*/ internal gNode gNode;
}

} partial class mainᴛgDefault {

internal static ж<gNode> AsNode(this ж<gDefault> Ꮡnode) { return Ꮡnode.ᏑgNode; }
internal static ж<gNode> VisitEachChild(this ж<gDefault> Ꮡnode, ж<gVisitor> Ꮡv) {
    ref var v = ref Ꮡv.DerefOrNull();

    v.visits++;
    return Ꮡnode.AsNode();
}
internal static ж<gFlowBase> FlowNodeData(this ref gDefault node) { return default; }
internal static nint Modifiers(this ref gDefault node) { return 0; }
internal static void setModifiers(this ref gDefault node, nint m) { }
internal static gFacts computeSubtreeFacts(this ref gDefault node) { return 1; }
internal static gFacts subtreeFactsWorker(this ref gDefault node, gData self) {
    return self.computeSubtreeFacts() + 10;
}
internal static gFacts propagateSubtreeFacts(this ref gDefault node) {
    return node.data.subtreeFactsWorker(node.data) + 100;
}

} partial class main_package {

partial struct gBase {
    /*embed*/ internal gDefault gDefault;
}

public partial struct gFlowBase {
    internal nint flow;
}

} partial class mainᴛgFlowBase {

public static ж<gFlowBase> FlowNodeData(this ж<gFlowBase> Ꮡnode) { return Ꮡnode; }

} partial class main_package {

partial struct gStmtBase {
    /*embed*/ internal gBase gBase;
    /*embed*/ internal gFlowBase gFlowBase;
}

partial struct gTypeSyntax {
}

} partial class mainᴛgTypeSyntax {

internal static gFacts computeSubtreeFacts(this ref gTypeSyntax node) { return 2; }
internal static gFacts propagateSubtreeFacts(this ref gTypeSyntax node) { return 3; }

} partial class main_package {

partial struct gTypeBase {
    /*embed*/ internal gTypeSyntax gTypeSyntax;
    /*embed*/ internal gBase gBase;
}

partial struct gModsBase {
    internal nint mods;
}

} partial class mainᴛgModsBase {

internal static nint Modifiers(this ref gModsBase node) { return node.mods; }
internal static void setModifiers(this ref gModsBase node, nint m) { node.mods = m; }

} partial class main_package {

partial struct gIf {
    /*embed*/ internal gStmtBase gStmtBase;
    internal nint cond;
}

partial struct gVarStmt {
    /*embed*/ internal gStmtBase gStmtBase;
    /*embed*/ internal gModsBase gModsBase;
}

partial struct gUnion {
    /*embed*/ internal gTypeBase gTypeBase;
    internal nint n;
}

} partial class mainᴛgUnion {

internal static gFacts computeSubtreeFacts(this ref gUnion node) { return 7; }

} partial class main_package {

partial struct gTypeArgsBase {
    /*embed*/ internal gTypeBase gTypeBase;
}

partial struct gRef {
    /*embed*/ internal gTypeArgsBase gTypeArgsBase;
    internal nint name;
}

partial struct gToken {
    /*embed*/ internal gBase gBase;
}

internal static ж<gNode> newNode(nint kind, gData data) {
    var n = data.AsNode();
    n.Kind = kind;
    n.data = data;
    return n;
}

internal static void main() {
    var @as = Ꮡ(new aStmt());
    (@as.k, @as.f) = (2, 7);
    var ai = Ꮡ(new aIf());
    (ai.k, ai.f) = (3, 8);
    selfer sx = new aStmtжselfer(@as);
    selfer sy = new aIfжselfer(ai);
    var (_, aok) = ((any)ai.OrTypedNil())._<selfer>(ᐧ);
    fmt.Println("A", @as.of(aStmt.ᏑaBase).of(aBase.ᏑaDefault).Self().k, ai.of(aIf.ᏑaStmt).of(aStmt.ᏑaBase).of(aBase.ᏑaDefault).Self().k, sx.Self().k, sy.Self() == ai.of(aIf.ᏑaNode), ai.of(aIf.ᏑaStmt).of(aStmt.ᏑaSide).Flag(), aok);
    fmt.Println("A", @as.of(aStmt.ᏑaBase).of(aBase.ᏑaDefault).of(aDefault.ᏑaNode).Root().k, ai.of(aIf.ᏑaStmt).of(aStmt.ᏑaBase).of(aBase.ᏑaDefault).of(aDefault.ᏑaNode).Root().k, sx.Root().k, sy.Root() == ai.of(aIf.ᏑaNode), @as.hits, ai.hits);

    var bb = Ꮡ(new bBoth());
    var bo = Ꮡ(new bOuter());
    namer nx = new bBothжnamer(bb);
    namer ny = new bOuterжnamer(bo);
    fmt.Println("B", bb.of(bBoth.ᏑbFlat).Name(), bo.of(bOuter.ᏑbBoth).of(bBoth.ᏑbFlat).Name(), nx.Name(), ny.Name());

    var bf = Ꮡ(new bFirst());
    var bfo = Ꮡ(new bFirstOuter());
    (nx, ny) = (new bFirstжnamer(bf), new bFirstOuterжnamer(bfo));
    fmt.Println("B", bf.of(bFirst.ᏑbFlat).Name(), bfo.of(bFirstOuter.ᏑbFirst).of(bFirst.ᏑbFlat).Name(), nx.Name(), ny.Name());

    var bs = Ꮡ(new bStmt());
    bs.n = 4;
    var bi = Ꮡ(new bIf());
    bi.n = 5;
    flowData fx = new bStmtжflowData(bs);
    flowData fy = new bIfжflowData(bi);
    fmt.Println("B", bs.of(bStmt.ᏑbFlow).Data().n, bi.of(bIf.ᏑbStmt).of(bStmt.ᏑbFlow).Data().n, fx.Data().n, fy.Data() == bi.of(bIf.ᏑbFlow));

    var cb = Ꮡ(new cBase());
    cb.k = 1;
    var cs = Ꮡ(new cStmt());
    cs.k = 2;
    visitor vx = new cBaseжvisitor(cb);
    visitor vy = new cStmtжvisitor(cs);
    fmt.Println("C", cb.of(cBase.ᏑcDefault).Visit(), cs.of(cStmt.ᏑcBase).of(cBase.ᏑcDefault).Visit(), vx.Visit(), vy.Visit());

    var da = Ꮡ(new dAll());
    var @do = Ꮡ(new dOuter());
    tagger tx = new dAllжtagger(da);
    tagger ty = new dOuterжtagger(@do);
    var (_, pairOk) = ((any)(Ꮡ(new dPair())))._<tagger>(ᐧ);
    fmt.Println("D", da.of(dAll.ᏑdTop).Tag(), @do.of(dOuter.ᏑdAll).of(dAll.ᏑdTop).Tag(), tx.Tag(), ty.Tag(), pairOk);

    var ef = Ꮡ(new eFork());
    var eo = Ꮡ(new eOuter());
    picker px = new eForkжpicker(ef);
    picker py = new eOuterжpicker(eo);
    fmt.Println("E", ef.of(eFork.ᏑeNear).Pick(), eo.of(eOuter.ᏑeFork).of(eFork.ᏑeNear).Pick(), px.Pick(), py.Pick(), eo.of(eOuter.ᏑeVia).of(eVia.ᏑeFar).Pick(6));

    var (_, tieOk) = ((any)(Ꮡ(new fAll())))._<tier>(ᐧ);
    var (_, bothOk) = ((any)(Ꮡ(new fBoth())))._<tier>(ᐧ);
    var (_, wrapOk) = ((any)(Ꮡ(new fWrap())))._<tier>(ᐧ);
    fmt.Println("F", tieOk, bothOk, wrapOk);

    var gi = Ꮡ(new gIf());
    gi.flow = 9;
    var ifNode = newNode(1, new gIfжgData(gi));
    var gv = Ꮡ(new gVarStmt());
    gv.flow = 4;
    var varNode = newNode(2, new gVarStmtжgData(gv));
    varNode.data.setModifiers(5);
    var unionNode = newNode(3, new gUnionжgData(Ꮡ(new gUnion())));
    var tokenNode = newNode(4, new gTokenжgData(Ꮡ(new gToken())));
    var refNode = newNode(5, new gRefжgData(Ꮡ(new gRef())));
    var v = Ꮡ(new gVisitor());
    fmt.Println("G", ifNode.AsNode() == ifNode, ifNode.VisitEachChild(v) == ifNode, ifNode.FlowNodeData().flow,
        ifNode.Modifiers(), ifNode.propagateSubtreeFacts(), ifNode.data.subtreeFactsWorker(ifNode.data));
    fmt.Println("G", varNode.FlowNodeData().flow, varNode.Modifiers(), varNode.propagateSubtreeFacts());
    fmt.Println("G", unionNode.FlowNodeData() == nil, unionNode.propagateSubtreeFacts(),
        unionNode.data.computeSubtreeFacts(), unionNode.data.subtreeFactsWorker(unionNode.data));
    fmt.Println("G", tokenNode.VisitEachChild(v) == tokenNode, tokenNode.FlowNodeData() == nil,
        tokenNode.propagateSubtreeFacts(), v.visits, ifNode.Kind + varNode.Kind + unionNode.Kind + tokenNode.Kind);
    fmt.Println("G", refNode.AsNode() == refNode, refNode.propagateSubtreeFacts(), refNode.data.computeSubtreeFacts(),
        refNode.data.subtreeFactsWorker(refNode.data), refNode.Modifiers());
}

} // end main_package
