package lib

// Inner carries an array, so a struct that embeds an Inner carries one through the embed.
type Inner struct {
	V [2]int
}

// Emb carries its only array through the embedded Inner.
type Emb struct {
	Inner
	N int
}
