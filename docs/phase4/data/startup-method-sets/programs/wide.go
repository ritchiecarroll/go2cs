package main

import (
	"bytes"
	"fmt"
	"go/ast"
	"go/token"
	"math/big"
	"net/url"
	"os"
	"reflect"
	"runtime"
	"strings"
	"time"
)

func main() {
	fmt.Println("hello")
	fmt.Println(time.Duration(1500) * time.Millisecond)
	fmt.Println(big.NewInt(42))
	fmt.Println(&url.URL{Scheme: "https", Host: "example.com"})
	fmt.Println(token.Pos(3), token.ADD)
	fmt.Println(reflect.TypeOf(1), reflect.Int)
	_, err := os.Open("/nonexistent/file")
	fmt.Println(err)
	fmt.Println(&ast.Ident{Name: "x"})
	fmt.Println(&runtime.TypeAssertionError{})
	fmt.Println(bytes.NewBufferString("b"), strings.NewReplacer("a", "b") != nil)
}
