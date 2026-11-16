package aptr

func Run() int { var p *struct{ Hz int } = psrc(); return p.Hz }

func psrc() *struct{ Hz int } { var r struct{ Hz int }; r.Hz = 3; return &r }
