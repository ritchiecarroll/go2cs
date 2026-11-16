package gbox

var v0 struct{ Hz int }

type Box struct{ V struct{ Hz int } }

func Run() int { v0.Hz = 3; b := Box{V: v0}; return b.V.Hz }
