module XpkgPromotedMethodSet

go 1.24

require (
	XpkgPromotedInnerLib v0.0.0
	XpkgPromotedMidLib v0.0.0
)

replace XpkgPromotedInnerLib => ../XpkgPromotedInnerLib

replace XpkgPromotedMidLib => ../XpkgPromotedMidLib
