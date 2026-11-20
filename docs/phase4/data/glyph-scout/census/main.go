// Counts Go identifiers that contain, or end in, each candidate glyph, over every .go file under the roots given.
// Tokenized with go/scanner, so comments and strings are not identifiers.
package main

import (
	"fmt"
	"go/scanner"
	"go/token"
	"io/fs"
	"os"
	"path/filepath"
	"strings"
)

func main() {
	glyphs := []string{"ᴍ", "⁀", "ᐟ", "ᴛ", "ꓸ", "ж"}
	files, idents := 0, 0
	contains := map[string]int{}
	ends := map[string]int{}
	examples := map[string][]string{}

	for _, root := range os.Args[1:] {
		filepath.WalkDir(root, func(path string, d fs.DirEntry, err error) error {
			if err != nil || d.IsDir() || !strings.HasSuffix(path, ".go") {
				return nil
			}
			src, err := os.ReadFile(path)
			if err != nil {
				return nil
			}
			files++
			fset := token.NewFileSet()
			var s scanner.Scanner
			s.Init(fset.AddFile(path, -1, len(src)), src, nil, 0)
			for {
				_, tok, lit := s.Scan()
				if tok == token.EOF {
					break
				}
				if tok != token.IDENT {
					continue
				}
				idents++
				for _, g := range glyphs {
					if strings.Contains(lit, g) {
						contains[g]++
						if len(examples[g]) < 3 {
							examples[g] = append(examples[g], path+": "+lit)
						}
					}
					if strings.HasSuffix(lit, g) {
						ends[g]++
					}
				}
			}
			return nil
		})
	}

	fmt.Printf("files=%d identifier-tokens=%d\n", files, idents)
	for _, g := range glyphs {
		fmt.Printf("%s U+%04X  contains=%d  ends-in=%d  %v\n", g, []rune(g)[0], contains[g], ends[g], examples[g])
	}
}
