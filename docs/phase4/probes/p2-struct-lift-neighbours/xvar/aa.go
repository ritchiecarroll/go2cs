package xvar

func Run() int { var x struct{ Hz int } = Src(); return x.Hz }
