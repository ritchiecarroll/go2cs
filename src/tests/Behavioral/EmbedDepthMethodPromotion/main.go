package main

// Go's rule for a promoted method: the selector x.M names the M at the SHALLOWEST embedding depth, and
// it is legal only when exactly one M sits at that depth. Every section below reaches one method name at
// more than one depth, or reaches a method that needs its receiver's address through more than one value
// embed. It is the TypeScript compiler's ast shape: NodeDefault embeds Node and redeclares some of its
// methods, NodeBase embeds NodeDefault, and the concrete nodes embed NodeBase beside sibling embeds.

import "fmt"

// --- A: methods that need their receiver's address, two and three value embeds down, behind a struct
// with TWO embeds. Root is declared once; Self is declared twice, and aDefault's shadows aNode's.

type aNode struct{ k int }

func (n *aNode) Root() *aNode { return n }
func (n *aNode) Self() *aNode { return n }

type aDefault struct {
	aNode
	hits int
}

func (d *aDefault) Self() *aNode {
	d.hits++
	return &d.aNode
}

type aBase struct{ aDefault }

type aSide struct{ f int }

func (s *aSide) Flag() int { return s.f }

type aStmt struct {
	aBase
	aSide
}

type aIf struct{ aStmt }

type selfer interface {
	Root() *aNode
	Self() *aNode
}

// --- B: one name through two direct embeds at DIFFERENT depths. Depth 1 wins.

type bDeep struct{}

func (d *bDeep) Name() string { return "deep" }

type bMid struct{ bDeep }

type bFlat struct{}

func (f *bFlat) Name() string { return "flat" }

type bBoth struct {
	bMid
	bFlat
}

type bOuter struct{ bBoth }

// The same with the empty winning embed listed FIRST.

type bFirst struct {
	bFlat
	bMid
}

type bFirstOuter struct{ bFirst }

type namer interface{ Name() string }

// The same, where the shallow method returns its receiver.

type bFlow struct{ n int }

func (f *bFlow) Data() *bFlow { return f }

type bNode struct{}

func (n *bNode) Data() *bFlow { return nil }

type bNodeBase struct{ bNode }

type bStmt struct {
	bNodeBase
	bFlow
}

type bIf struct{ bStmt }

type flowData interface{ Data() *bFlow }

// --- C: cDefault.Visit shadows cNode.Visit. The shadowing method takes a field's address; the
// shadowed one does not.

type cNode struct{ k int }

func (n *cNode) Visit() int { return n.k }

type cDefault struct{ cNode }

func (d *cDefault) Visit() int {
	p := &d.cNode
	return p.k + 100
}

type cBase struct{ cDefault }

type cStmt struct{ cBase }

type visitor interface{ Visit() int }

// --- D: dPair reaches Tag twice at one depth, so dPair has no Tag. dAll reaches Tag once at depth 1
// (dTop) and twice at depth 2 (through dPair), so dAll.Tag is dTop.Tag.

type dLeft struct{}

func (l *dLeft) Tag() string { return "left" }

type dRight struct{}

func (r *dRight) Tag() string { return "right" }

type dPair struct {
	dLeft
	dRight
}

type dTop struct{}

func (t *dTop) Tag() string { return "top" }

type dAll struct {
	dPair
	dTop
}

type dOuter struct{ dAll }

type tagger interface{ Tag() string }

// --- E: inside the one embed eFork, Pick sits at depth 1 (eNear) and at depth 2 (eFar) with a
// DIFFERENT signature. The promoted Pick has eNear's signature.

type eFar struct{}

func (f *eFar) Pick(n int) int { return n }

type eVia struct{ eFar }

type eNear struct{}

func (n *eNear) Pick() string { return "near" }

type eFork struct {
	eVia
	eNear
}

type eOuter struct{ eFork }

type picker interface{ Pick() string }

// --- F: a tie at the shallowest depth promotes nothing: Tie sits three times at depth 2.

type fOne struct{}

func (o *fOne) Tie() string { return "one" }

type fTwo struct{}

func (t *fTwo) Tie() string { return "two" }

type fBoth struct {
	fOne
	fTwo
}

type fDeep struct{}

func (d *fDeep) Tie() string { return "deep" }

type fWrap struct{ fDeep }

type fAll struct {
	fBoth
	fWrap
}

type tier interface{ Tie() string }

// --- G: the ast chain in miniature. A node's data is an interface with unexported members; concrete
// nodes satisfy it through gBase and the sibling bases, and gNode's own methods call back through it.

type gFacts int

type gVisitor struct{ visits int }

type gData interface {
	AsNode() *gNode
	VisitEachChild(v *gVisitor) *gNode
	FlowNodeData() *gFlowBase
	Modifiers() int
	setModifiers(m int)
	computeSubtreeFacts() gFacts
	subtreeFactsWorker(self gData) gFacts
	propagateSubtreeFacts() gFacts
}

type gNode struct {
	Kind   int
	Parent *gNode
	data   gData
}

func (n *gNode) AsNode() *gNode                    { return n }
func (n *gNode) VisitEachChild(v *gVisitor) *gNode { return n.data.VisitEachChild(v) }
func (n *gNode) FlowNodeData() *gFlowBase          { return n.data.FlowNodeData() }
func (n *gNode) Modifiers() int                    { return n.data.Modifiers() }
func (n *gNode) propagateSubtreeFacts() gFacts     { return n.data.propagateSubtreeFacts() }

type gDefault struct{ gNode }

func (node *gDefault) AsNode() *gNode { return &node.gNode }
func (node *gDefault) VisitEachChild(v *gVisitor) *gNode {
	v.visits++
	return node.AsNode()
}
func (node *gDefault) FlowNodeData() *gFlowBase    { return nil }
func (node *gDefault) Modifiers() int              { return 0 }
func (node *gDefault) setModifiers(m int)          {}
func (node *gDefault) computeSubtreeFacts() gFacts { return 1 }
func (node *gDefault) subtreeFactsWorker(self gData) gFacts {
	return self.computeSubtreeFacts() + 10
}
func (node *gDefault) propagateSubtreeFacts() gFacts {
	return node.data.subtreeFactsWorker(node.data) + 100
}

type gBase struct{ gDefault }

type gFlowBase struct{ flow int }

func (node *gFlowBase) FlowNodeData() *gFlowBase { return node }

type gStmtBase struct {
	gBase
	gFlowBase
}

type gTypeSyntax struct{}

func (node *gTypeSyntax) computeSubtreeFacts() gFacts   { return 2 }
func (node *gTypeSyntax) propagateSubtreeFacts() gFacts { return 3 }

type gTypeBase struct {
	gTypeSyntax
	gBase
}

type gModsBase struct{ mods int }

func (node *gModsBase) Modifiers() int     { return node.mods }
func (node *gModsBase) setModifiers(m int) { node.mods = m }

type gIf struct {
	gStmtBase
	cond int
}

type gVarStmt struct {
	gStmtBase
	gModsBase
}

type gUnion struct {
	gTypeBase
	n int
}

func (node *gUnion) computeSubtreeFacts() gFacts { return 7 }

// An intermediate base over gTypeBase, and a concrete node over it that declares nothing itself.

type gTypeArgsBase struct{ gTypeBase }

type gRef struct {
	gTypeArgsBase
	name int
}

type gToken struct{ gBase }

func newNode(kind int, data gData) *gNode {
	n := data.AsNode()
	n.Kind = kind
	n.data = data
	return n
}

func main() {
	// A
	as := &aStmt{}
	as.k, as.f = 2, 7
	ai := &aIf{}
	ai.k, ai.f = 3, 8
	var sx selfer = as
	var sy selfer = ai
	_, aok := any(ai).(selfer)
	fmt.Println("A", as.Self().k, ai.Self().k, sx.Self().k, sy.Self() == &ai.aNode, ai.Flag(), aok)
	fmt.Println("A", as.Root().k, ai.Root().k, sx.Root().k, sy.Root() == &ai.aNode, as.hits, ai.hits)

	// B
	bb := &bBoth{}
	bo := &bOuter{}
	var nx namer = bb
	var ny namer = bo
	fmt.Println("B", bb.Name(), bo.Name(), nx.Name(), ny.Name())

	bf := &bFirst{}
	bfo := &bFirstOuter{}
	nx, ny = bf, bfo
	fmt.Println("B", bf.Name(), bfo.Name(), nx.Name(), ny.Name())

	bs := &bStmt{}
	bs.n = 4
	bi := &bIf{}
	bi.n = 5
	var fx flowData = bs
	var fy flowData = bi
	fmt.Println("B", bs.Data().n, bi.Data().n, fx.Data().n, fy.Data() == &bi.bFlow)

	// C
	cb := &cBase{}
	cb.k = 1
	cs := &cStmt{}
	cs.k = 2
	var vx visitor = cb
	var vy visitor = cs
	fmt.Println("C", cb.Visit(), cs.Visit(), vx.Visit(), vy.Visit())

	// D
	da := &dAll{}
	do := &dOuter{}
	var tx tagger = da
	var ty tagger = do
	_, pairOk := any(&dPair{}).(tagger)
	fmt.Println("D", da.Tag(), do.Tag(), tx.Tag(), ty.Tag(), pairOk)

	// E
	ef := &eFork{}
	eo := &eOuter{}
	var px picker = ef
	var py picker = eo
	fmt.Println("E", ef.Pick(), eo.Pick(), px.Pick(), py.Pick(), eo.eVia.Pick(6))

	// F
	_, tieOk := any(&fAll{}).(tier)
	_, bothOk := any(&fBoth{}).(tier)
	_, wrapOk := any(&fWrap{}).(tier)
	fmt.Println("F", tieOk, bothOk, wrapOk)

	// G
	gi := &gIf{}
	gi.flow = 9
	ifNode := newNode(1, gi)
	gv := &gVarStmt{}
	gv.flow = 4
	varNode := newNode(2, gv)
	varNode.data.setModifiers(5)
	unionNode := newNode(3, &gUnion{})
	tokenNode := newNode(4, &gToken{})
	refNode := newNode(5, &gRef{})
	v := &gVisitor{}
	fmt.Println("G", ifNode.AsNode() == ifNode, ifNode.VisitEachChild(v) == ifNode, ifNode.FlowNodeData().flow,
		ifNode.Modifiers(), ifNode.propagateSubtreeFacts(), ifNode.data.subtreeFactsWorker(ifNode.data))
	fmt.Println("G", varNode.FlowNodeData().flow, varNode.Modifiers(), varNode.propagateSubtreeFacts())
	fmt.Println("G", unionNode.FlowNodeData() == nil, unionNode.propagateSubtreeFacts(),
		unionNode.data.computeSubtreeFacts(), unionNode.data.subtreeFactsWorker(unionNode.data))
	fmt.Println("G", tokenNode.VisitEachChild(v) == tokenNode, tokenNode.FlowNodeData() == nil,
		tokenNode.propagateSubtreeFacts(), v.visits, ifNode.Kind+varNode.Kind+unionNode.Kind+tokenNode.Kind)
	fmt.Println("G", refNode.AsNode() == refNode, refNode.propagateSubtreeFacts(), refNode.data.computeSubtreeFacts(),
		refNode.data.subtreeFactsWorker(refNode.data), refNode.Modifiers())
}
