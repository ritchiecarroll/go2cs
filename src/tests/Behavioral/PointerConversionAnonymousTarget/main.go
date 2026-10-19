package main

import "fmt"

type Employee struct {
	Name string
	Age  int
}

func toAnonymous() {
	e := &Employee{Name: "ada", Age: 13}
	q := (*struct {
		Name string
		Age  int
	})(e)

	q.Age = 14
	fmt.Println("to anonymous, write through converted:", e.Age)
}

func tagsOnly() {
	t := &struct {
		Name string `json:"name"`
		Age  int    `json:"age"`
	}{"bea", 15}
	u := (*struct {
		Name string
		Age  int
	})(t)

	u.Age = 16
	fmt.Println("tags only, write through converted:", t.Age)
}

func main() {
	toAnonymous()
	tagsOnly()
}
