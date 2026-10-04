package main

import (
	"fmt"

	"example.com/multi/p1"
)

var msg = "main v70002"

func main() {
	fmt.Println(msg, p1.Value())
}
