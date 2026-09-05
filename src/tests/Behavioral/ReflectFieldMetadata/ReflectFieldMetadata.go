// Struct-field METADATA through reflect, three roots measured by reflect's own suite and pinned here
// against `go run` (increment E2 of the reflect tail):
//
//   1. Go's flag.ro(): an EMBEDDED read-only field's elements stay read-only through Index and
//      Slice, so CanSet answers false two hops down (TestIssue22031).
//   2. FieldByName's MULTIPLICITY rule: an embedded type reached twice at one depth annihilates
//      itself -- its field is absent, not found once (TestFieldByName's S3.B / S10.X / S14.X).
//   3. StructField.PkgPath is the package that DECLARED the field, even when the struct is a defined
//      type over another package's struct (TestFieldPkgPath's localOtherPkgFields row).
//
//   4. StructField.Anonymous for an embedded BUILTIN (`struct{ int }`, `struct{ *int }`): a plain field
//      the converter stamps [GoEmbedded] (increment E2b -- the other half of TestFieldPkgPath), beside
//      the controls that must NOT read embedded (a plain field of a named-int type) and the embedded
//      NAMED int the struct-wrapper route already reported.
//
//   5. The clause that an annihilation at one depth inhibits every DEEPER match: a struct EMBEDDING
//      `twice` (`type deeper struct{ twice }`). This is the same visited-vs-multiplicity error E2 fixed
//      in reflect's promotedFieldByName, on the GENERATOR's side -- StructTypeTemplate's promotion walk
//      threaded ONE `seenTypes` set across sibling branches, so `base`, reached through `viaX` AND
//      `*viaY` at the same depth, was walked once and its `B` counted once: `deeper` promoted
//      `B => ref twice.B` (a hop that does not exist, CS1061 x2) while `twice`'s own shell correctly
//      promoted no `B`. Fixed in increment E2c by scoping the walk's guard to the current PATH; this
//      row is what showed it RED before the cut and green after.
package main

import (
	"fmt"
	"reflect"

	"ReflectFieldMetadata/fieldlib"
)

// --- root 1: flag.ro() ---
type sElem []struct{ C int }
type embeds struct{ sElem }   // the slice type is EMBEDDED and unexported: flagEmbedRO
type holds struct{ f sElem }  // a plain unexported field: flagStickyRO

// --- root 2: multiplicity ---
type base struct{ B int }
// base carries a METHOD as well as a field: the generator has THREE walks with the same shared-set
// shape (the field walk and two method walks), so the same lattice measures whether the method half
// promotes what Go annihilates -- a fix is owed only where a control can be made to fail.
func (b base) M() int { return b.B }

type viaX struct{ base }
type viaY struct{ base }
type twice struct { // base is reached through viaX AND *viaY at depth 2: B annihilates
	viaX
	*viaY
	D int
}
type once struct { // base reached ONCE: B is found at depth 2
	viaX
	D int
}

// deeper embeds the AMBIGUOUS type: Go's rule is that the annihilation at twice's depth inhibits
// every deeper match, so `deeper.B` is not found either -- and `deeper.D`, unambiguous inside twice,
// still promotes through it (the control that keeps the fix from simply refusing everything).
type deeper struct{ twice }

// --- root 3: PkgPath through a defined type over a foreign struct ---
type local fieldlib.Outer

// --- root 4 (E2b): Anonymous for an embedded builtin ---
type myInt int
type embedsInt struct{ int }        // embedded PREDECLARED type: Anonymous, named "int"
type embedsIntPtr struct{ *int }    // embedded POINTER to a predeclared type: Anonymous, named "int"
type holdsNamed struct{ n myInt }   // a plain field of a named-int type: NOT embedded (control)
type embedsNamed struct{ myInt }    // an embedded NAMED int: Anonymous through the struct-wrapper route (control)

func main() {
	// 1. CanSet through an embedded read-only slice's element's field.
	fmt.Println("CanSet via embedded  :", reflect.ValueOf(embeds{sElem{{}}}).Field(0).Index(0).Field(0).CanSet())
	fmt.Println("CanSet via unexported:", reflect.ValueOf(holds{sElem{{}}}).Field(0).Index(0).Field(0).CanSet())
	// and the same read-only survives a Slice window
	fmt.Println("CanSet via Slice     :", reflect.ValueOf(embeds{sElem{{}}}).Field(0).Slice(0, 1).Index(0).Field(0).CanSet())

	// 2. FieldByName: found / not found, and the index path when found.
	_, foundTwice := reflect.TypeOf(twice{}).FieldByName("B")
	fOnce, foundOnce := reflect.TypeOf(once{}).FieldByName("B")
	fD, foundD := reflect.TypeOf(twice{}).FieldByName("D")
	fmt.Println("twice.B  found:", foundTwice)
	fmt.Println("once.B   found:", foundOnce, "index:", fOnce.Index)
	fmt.Println("twice.D  found:", foundD, "index:", fD.Index)
	// the Value side agrees with the Type side
	fmt.Println("twice.B  value valid:", reflect.ValueOf(twice{}).FieldByName("B").IsValid())
	// 2b (E2c). The annihilation is INHERITED: deeper embeds twice, so B is not found through it either,
	// while D -- unambiguous inside twice -- still promotes one level further out.
	_, foundDeeperB := reflect.TypeOf(deeper{}).FieldByName("B")
	fDeeperD, foundDeeperD := reflect.TypeOf(deeper{}).FieldByName("D")
	fmt.Println("deeper.B found:", foundDeeperB, " value valid:", reflect.ValueOf(deeper{}).FieldByName("B").IsValid())
	fmt.Println("deeper.D found:", foundDeeperD, "index:", fDeeperD.Index, " value:", reflect.ValueOf(deeper{}).FieldByName("D").IsValid())

	// 2c (E2c). The METHOD half of the same rule: base.M is reached twice through twice, so it is
	// promoted onto neither twice nor deeper; once (base reached ONCE) does promote it.
	_, mTwice := reflect.TypeOf(twice{}).MethodByName("M")
	_, mDeeper := reflect.TypeOf(deeper{}).MethodByName("M")
	_, mOnce := reflect.TypeOf(once{}).MethodByName("M")
	fmt.Println("M promoted -- twice:", mTwice, " deeper:", mDeeper, " once:", mOnce)

	// 3. PkgPath of a field declared in another package, reached through a defined local type. The
	// rows compare PATHS rather than print them, so the guard measures E2's root -- the declaring
	// package survives the defined-type hop -- and not the spelling of a sub-library's import path.
	lt := reflect.TypeOf(local{})
	ot := reflect.TypeOf(fieldlib.Outer{})
	mine := reflect.TypeOf(struct{ u int }{}).Field(0).PkgPath
	fmt.Println("local.Field(0) exported, PkgPath empty:", lt.Field(0).IsExported(), lt.Field(0).PkgPath == "")
	fmt.Println("local.Field(1) exported:", lt.Field(1).IsExported())
	fmt.Println("local.Field(1) PkgPath == Outer.Field(1) PkgPath:", lt.Field(1).PkgPath == ot.Field(1).PkgPath)
	fmt.Println("local.Field(1) PkgPath is foreign (not this package, not empty):", lt.Field(1).PkgPath != mine, lt.Field(1).PkgPath != "")
	fmt.Println("anon unexported PkgPath is this package:", mine == "main")

	// 4. Anonymous for an embedded builtin, with its controls.
	anon := func(label string, t reflect.Type) {
		f := t.Field(0)
		fmt.Printf("%-14s name=%-5s anonymous=%-5v type=%-6v pkgPath=%q\n", label, f.Name, f.Anonymous, f.Type, f.PkgPath)
	}
	anon("struct{ int }", reflect.TypeOf(embedsInt{}))
	anon("struct{ *int }", reflect.TypeOf(embedsIntPtr{}))
	anon("struct{ n T }", reflect.TypeOf(holdsNamed{}))
	anon("struct{ myInt }", reflect.TypeOf(embedsNamed{}))
}
