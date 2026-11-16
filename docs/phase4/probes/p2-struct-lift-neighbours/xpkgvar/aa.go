package xpkgvar

var X struct{ Hz int } = V0

func Run() int { return X.Hz + 3 }
