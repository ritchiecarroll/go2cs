package aslice

func Run() int { var s []struct{ Hz int } = all(); return s[0].Hz }

func all() []struct{ Hz int } { var r struct{ Hz int }; r.Hz = 3; return []struct{ Hz int }{r} }
