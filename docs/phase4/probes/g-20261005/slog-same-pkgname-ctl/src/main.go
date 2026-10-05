package main

import (
	"fmt"
	"log/slog"

	lslog "sloghandler/lib"
)

func main() {
	h := lslog.NewHandler()
	s := slog.New(lslog.NewHandler())
	t := slog.New(h)
	s.Info("x")
	t.Info("y")
	fmt.Println(h.Count())
}
