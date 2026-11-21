package lib

// Holder2 and Box2 repeat the two anonymous types in a second file, which is converted after a.go.

// Holder2's exported field has With's anonymous interface type.
type Holder2 struct {
	W interface {
		Option
		Other
	}
}

// Mk2 stores With's result in the field.
func Mk2(n int) Holder2 { return Holder2{W: With(n)} }

// Box2's exported field has Pair's anonymous struct type.
type Box2 struct{ Q struct{ X, Y int } }

// MkBox2 stores Pair's result in the field.
func MkBox2(n int) Box2 { return Box2{Q: Pair(n)} }
