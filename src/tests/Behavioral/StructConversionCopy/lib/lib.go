package lib

// Inner carries an array, so a struct holding an Inner carries one a level down.
type Inner struct {
	V [2]int
}

type Chord struct {
	Name  string
	Notes [2]int
	In    Inner
}
