// The sample program for github.com/golang-jwt/jwt/v5 v5.3.1 (first-wave plan, section 3). It signs a fixed claim set
// with HS256 and a fixed key, so the token is the same on every run (encoding/json sorts map keys), then parses it back.
package main

import (
	"fmt"

	"github.com/golang-jwt/jwt/v5"
)

func main() {
	key := []byte("go2cs-demo-key")
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, jwt.MapClaims{"sub": "go2cs", "aud": "nugetgo"})
	signed, err := token.SignedString(key)
	fmt.Println(signed, err)
	parsed, err := jwt.Parse(signed, func(*jwt.Token) (any, error) { return key, nil }, jwt.WithValidMethods([]string{"HS256"}))
	fmt.Println(parsed.Valid, err)
	subject, err := parsed.Claims.GetSubject()
	fmt.Println(subject, err)
}
