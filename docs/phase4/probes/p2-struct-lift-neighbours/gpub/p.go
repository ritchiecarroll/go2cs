package gpub

var v0 struct{ Hz int }

var X struct{ Hz int } = v0

func Run() int { return X.Hz + 3 }
