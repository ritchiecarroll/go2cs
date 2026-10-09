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
