package main

import (
	"fmt"
	"os"
	"time"
)

func message() string {
	return "tick v1"
}

func main() {
	for i := 0; i < 240; i++ {
		fmt.Println(os.Getpid(), i, message())
		time.Sleep(500 * time.Millisecond)
	}
}
