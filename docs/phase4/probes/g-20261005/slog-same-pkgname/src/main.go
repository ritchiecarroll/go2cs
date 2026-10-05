package main

import (
	"fmt"
	"io"
	"log/slog"

	lslog "sloghandler/lib"
)

func use(w io.Writer) { fmt.Fprint(w, "x") }

func main() {
	h := lslog.NewHandler()
	s := slog.New(lslog.NewHandler())
	t := slog.New(h)
	s.Info("x")
	t.Info("y")
	use(lslog.NewWriter())
	fmt.Println(h.Count())
}
