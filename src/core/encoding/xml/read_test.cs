// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using static go.encoding.xml_package;

partial class xml_internal_test_package {

// Stripped down Atom feed data structures.
public static void TestUnmarshalFeed(ж<testing.T> Ꮡt) {
    ref var f = ref heap(new Feed(), out var Ꮡf);
    {
        var err = Unmarshal(slice<byte>(atomFeedString), Ꮡf); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal: %s"u8, err);
        }
    }
    if (!reflect.DeepEqual(f, atomFeed)) {
        Ꮡt.Fatalf("have %#v\nwant %#v"u8, f, atomFeed);
    }
}

// hget http://codereview.appspot.com/rss/mine/rsc
internal static readonly @string atomFeedString = """

<?xml version="1.0" encoding="utf-8"?>
<feed xmlns="http://www.w3.org/2005/Atom" xml:lang="en-us" updated="2009-10-04T01:35:58+00:00"><title>Code Review - My issues</title><link href="http://codereview.appspot.com/" rel="alternate"></link><link href="http://codereview.appspot.com/rss/mine/rsc" rel="self"></link><id>http://codereview.appspot.com/</id><author><name>rietveld&lt;&gt;</name></author><entry><title>rietveld: an attempt at pubsubhubbub
</title><link href="http://codereview.appspot.com/126085" rel="alternate"></link><updated>2009-10-04T01:35:58+00:00</updated><author><name>email-address-removed</name></author><id>urn:md5:134d9179c41f806be79b3a5f7877d19a</id><summary type="html">
  An attempt at adding pubsubhubbub support to Rietveld.
http://code.google.com/p/pubsubhubbub
http://code.google.com/p/rietveld/issues/detail?id=155

The server side of the protocol is trivial:
  1. add a &amp;lt;link rel=&amp;quot;hub&amp;quot; href=&amp;quot;hub-server&amp;quot;&amp;gt; tag to all
     feeds that will be pubsubhubbubbed.
  2. every time one of those feeds changes, tell the hub
     with a simple POST request.

I have tested this by adding debug prints to a local hub
server and checking that the server got the right publish
requests.

I can&amp;#39;t quite get the server to work, but I think the bug
is not in my code.  I think that the server expects to be
able to grab the feed and see the feed&amp;#39;s actual URL in
the link rel=&amp;quot;self&amp;quot;, but the default value for that drops
the :port from the URL, and I cannot for the life of me
figure out how to get the Atom generator deep inside
django not to do that, or even where it is doing that,
or even what code is running to generate the Atom feed.
(I thought I knew but I added some assert False statements
and it kept running!)

Ignoring that particular problem, I would appreciate
feedback on the right way to get the two values at
the top of feeds.py marked NOTE(rsc).


</summary></entry><entry><title>rietveld: correct tab handling
</title><link href="http://codereview.appspot.com/124106" rel="alternate"></link><updated>2009-10-03T23:02:17+00:00</updated><author><name>email-address-removed</name></author><id>urn:md5:0a2a4f19bb815101f0ba2904aed7c35a</id><summary type="html">
  This fixes the buggy tab rendering that can be seen at
http://codereview.appspot.com/116075/diff/1/2

The fundamental problem was that the tab code was
not being told what column the text began in, so it
didn&amp;#39;t know where to put the tab stops.  Another problem
was that some of the code assumed that string byte
offsets were the same as column offsets, which is only
true if there are no tabs.

In the process of fixing this, I cleaned up the arguments
to Fold and ExpandTabs and renamed them Break and
_ExpandTabs so that I could be sure that I found all the
call sites.  I also wanted to verify that ExpandTabs was
not being used from outside intra_region_diff.py.


</summary></entry></feed> 	   
"""u8;

public partial struct Feed {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"http://www.w3.org/2005/Atom feed"`*/
    public @string Title; /*`xml:"title"`*/
    public @string ID; /*`xml:"id"`*/
    public slice<Link> Link; /*`xml:"link"`*/
    public time.Time Updated; /*`xml:"updated,attr"`*/
    public Person Author; /*`xml:"author"`*/
    public slice<Entry> Entry; /*`xml:"entry"`*/
}

public partial struct Entry {
    public @string Title; /*`xml:"title"`*/
    public @string ID; /*`xml:"id"`*/
    public slice<Link> Link; /*`xml:"link"`*/
    public time.Time Updated; /*`xml:"updated"`*/
    public Person Author; /*`xml:"author"`*/
    public Text Summary; /*`xml:"summary"`*/
}

public partial struct Link {
    public @string Rel; /*`xml:"rel,attr,omitempty"`*/
    public @string Href; /*`xml:"href,attr"`*/
}

public partial struct Person {
    public @string Name; /*`xml:"name"`*/
    public @string URI; /*`xml:"uri"`*/
    public @string Email; /*`xml:"email"`*/
    public @string InnerXML; /*`xml:",innerxml"`*/
}

public partial struct Text {
    public @string Type; /*`xml:"type,attr,omitempty"`*/
    public @string Body; /*`xml:",chardata"`*/
}

internal static Feed atomFeed = new Feed(
    XMLName: new Name("http://www.w3.org/2005/Atom"u8, "feed"u8),
    Title: "Code Review - My issues"u8,
    Link: new Link[]{
        new(Rel: "alternate"u8, Href: "http://codereview.appspot.com/"u8),
        new(Rel: "self"u8, Href: "http://codereview.appspot.com/rss/mine/rsc"u8)
    }.slice(),
    ID: "http://codereview.appspot.com/"u8,
    Updated: ParseTime("2009-10-04T01:35:58+00:00"u8),
    Author: new Person(
        Name: "rietveld<>"u8,
        InnerXML: "<name>rietveld&lt;&gt;</name>"u8
    ),
    Entry: new Entry[]{
        new(
            Title: "rietveld: an attempt at pubsubhubbub\n"u8,
            Link: new Link[]{
                new(Rel: "alternate"u8, Href: "http://codereview.appspot.com/126085"u8)
            }.slice(),
            Updated: ParseTime("2009-10-04T01:35:58+00:00"u8),
            Author: new Person(
                Name: "email-address-removed"u8,
                InnerXML: "<name>email-address-removed</name>"u8
            ),
            ID: "urn:md5:134d9179c41f806be79b3a5f7877d19a"u8,
            Summary: new Text(
                Type: "html"u8,
                Body: """

  An attempt at adding pubsubhubbub support to Rietveld.
http://code.google.com/p/pubsubhubbub
http://code.google.com/p/rietveld/issues/detail?id=155

The server side of the protocol is trivial:
  1. add a &lt;link rel=&quot;hub&quot; href=&quot;hub-server&quot;&gt; tag to all
     feeds that will be pubsubhubbubbed.
  2. every time one of those feeds changes, tell the hub
     with a simple POST request.

I have tested this by adding debug prints to a local hub
server and checking that the server got the right publish
requests.

I can&#39;t quite get the server to work, but I think the bug
is not in my code.  I think that the server expects to be
able to grab the feed and see the feed&#39;s actual URL in
the link rel=&quot;self&quot;, but the default value for that drops
the :port from the URL, and I cannot for the life of me
figure out how to get the Atom generator deep inside
django not to do that, or even where it is doing that,
or even what code is running to generate the Atom feed.
(I thought I knew but I added some assert False statements
and it kept running!)

Ignoring that particular problem, I would appreciate
feedback on the right way to get the two values at
the top of feeds.py marked NOTE(rsc).



"""u8
            )
        ),
        new(
            Title: "rietveld: correct tab handling\n"u8,
            Link: new Link[]{
                new(Rel: "alternate"u8, Href: "http://codereview.appspot.com/124106"u8)
            }.slice(),
            Updated: ParseTime("2009-10-03T23:02:17+00:00"u8),
            Author: new Person(
                Name: "email-address-removed"u8,
                InnerXML: "<name>email-address-removed</name>"u8
            ),
            ID: "urn:md5:0a2a4f19bb815101f0ba2904aed7c35a"u8,
            Summary: new Text(
                Type: "html"u8,
                Body: """

  This fixes the buggy tab rendering that can be seen at
http://codereview.appspot.com/116075/diff/1/2

The fundamental problem was that the tab code was
not being told what column the text began in, so it
didn&#39;t know where to put the tab stops.  Another problem
was that some of the code assumed that string byte
offsets were the same as column offsets, which is only
true if there are no tabs.

In the process of fixing this, I cleaned up the arguments
to Fold and ExpandTabs and renamed them Break and
_ExpandTabs so that I could be sure that I found all the
call sites.  I also wanted to verify that ExpandTabs was
not being used from outside intra_region_diff.py.



"""u8
            )
        )
    }.slice()
);

internal static readonly @string pathTestString = """

<Result>
    <Before>1</Before>
    <Items>
        <Item1>
            <Value>A</Value>
        </Item1>
        <Item2>
            <Value>B</Value>
        </Item2>
        <Item1>
            <Value>C</Value>
            <Value>D</Value>
        </Item1>
        <_>
            <Value>E</Value>
        </_>
    </Items>
    <After>2</After>
</Result>

"""u8;

public partial struct PathTestItem {
    public @string Value;
}

public partial struct PathTestA {
    public slice<PathTestItem> Items; /*`xml:">Item1"`*/
    public @string Before, After;
}

public partial struct PathTestB {
    public slice<PathTestItem> Other; /*`xml:"Items>Item1"`*/
    public @string Before, After;
}

public partial struct PathTestC {
    public slice<@string> Values1; /*`xml:"Items>Item1>Value"`*/
    public slice<@string> Values2; /*`xml:"Items>Item2>Value"`*/
    public @string Before, After;
}

public partial struct PathTestSet {
    public slice<PathTestItem> Item1;
}

public partial struct PathTestD {
    public PathTestSet Other; /*`xml:"Items"`*/
    public @string Before, After;
}

public partial struct PathTestE {
    public @string Underline; /*`xml:"Items>_>Value"`*/
    public @string Before, After;
}

internal static slice<any> pathTests = new any[]{
    Ꮡ(new PathTestA(Items: new PathTestItem[]{new("A"u8), new("D"u8)}.slice(), Before: "1"u8, After: "2"u8)),
    Ꮡ(new PathTestB(Other: new PathTestItem[]{new("A"u8), new("D"u8)}.slice(), Before: "1"u8, After: "2"u8)),
    Ꮡ(new PathTestC(Values1: new @string[]{"A"u8, "C"u8, "D"u8}.slice(), Values2: new @string[]{"B"u8}.slice(), Before: "1"u8, After: "2"u8)),
    Ꮡ(new PathTestD(Other: new PathTestSet(Item1: new PathTestItem[]{new("A"u8), new("D"u8)}.slice()), Before: "1"u8, After: "2"u8)),
    Ꮡ(new PathTestE(Underline: "E"u8, Before: "1"u8, After: "2"u8))
}.slice();

public static void TestUnmarshalPaths(ж<testing.T> Ꮡt) {
    foreach (var (_, pt) in pathTests) {
        var v = reflect.New(reflect.TypeOf(pt).Elem()).Interface();
        {
            var err = Unmarshal(slice<byte>(pathTestString), v); if (err != default!) {
                Ꮡt.Fatalf("Unmarshal: %s"u8, err);
            }
        }
        if (!reflect.DeepEqual(v, pt)) {
            Ꮡt.Fatalf("have %#v\nwant %#v"u8, v, pt);
        }
    }
}

public partial struct BadPathTestA {
    public @string First; /*`xml:"items>item1"`*/
    public @string Other; /*`xml:"items>item2"`*/
    public @string Second; /*`xml:"items"`*/
}

public partial struct BadPathTestB {
    public @string Other; /*`xml:"items>item2>value"`*/
    public @string First; /*`xml:"items>item1"`*/
    public @string Second; /*`xml:"items>item1>value"`*/
}

public partial struct BadPathTestC {
    public @string First;
    public @string Second; /*`xml:"First"`*/
}

public partial struct BadPathTestD {
    public partial ref BadPathEmbeddedA BadPathEmbeddedA { get; }
    public partial ref BadPathEmbeddedB BadPathEmbeddedB { get; }
}

public partial struct BadPathEmbeddedA {
    public @string First;
}

public partial struct BadPathEmbeddedB {
    public @string Second; /*`xml:"First"`*/
}


partial struct badPathTestsᴛ1 /*dyn*/ {
    internal any v, e;
}
internal static slice<badPathTestsᴛ1> badPathTests = new badPathTestsᴛ1[]{
    new(Ꮡ(new BadPathTestA(nil)), Ꮡ(new TagPathError(reflect.TypeFor<BadPathTestA>(), "First"u8, "items>item1"u8, "Second"u8, "items"u8))),
    new(Ꮡ(new BadPathTestB(nil)), Ꮡ(new TagPathError(reflect.TypeFor<BadPathTestB>(), "First"u8, "items>item1"u8, "Second"u8, "items>item1>value"u8))),
    new(Ꮡ(new BadPathTestC(nil)), Ꮡ(new TagPathError(reflect.TypeFor<BadPathTestC>(), "First"u8, ""u8, "Second"u8, "First"u8))),
    new(Ꮡ(new BadPathTestD(nil)), Ꮡ(new TagPathError(reflect.TypeFor<BadPathTestD>(), "First"u8, ""u8, "Second"u8, "First"u8)))
}.slice();

public static void TestUnmarshalBadPaths(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in badPathTests) {
        var err = Unmarshal(slice<byte>(pathTestString), tt.v);
        if (!reflect.DeepEqual(err, tt.e)) {
            Ꮡt.Fatalf("Unmarshal with %#v didn't fail properly:\nhave %#v,\nwant %#v"u8, tt.v, err, tt.e);
        }
    }
}

public static readonly @string OK = "OK"u8;

internal static readonly @string withoutNameTypeData = """

<?xml version="1.0" charset="utf-8"?>
<Test3 Attr="OK" />
"""u8;

public partial struct TestThree {
    public global::go.encoding.xml_package.Name XMLName; /*`xml:"Test3"`*/
    public @string Attr; /*`xml:",attr"`*/
}

public static void TestUnmarshalWithoutNameType(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new TestThree(), out var Ꮡx);
    {
        var err = Unmarshal(slice<byte>(withoutNameTypeData), Ꮡx); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal: %s"u8, err);
        }
    }
    if (x.Attr != OK) {
        Ꮡt.Fatalf("have %v\nwant %v"u8, x.Attr, OK);
    }
}

internal partial struct TestUnmarshalAttr_ParamVal /*dyn*/ {
    public nint Int; /*`xml:"int,attr"`*/
}

internal partial struct TestUnmarshalAttr_ParamPtr /*dyn*/ {
    public ж<nint> Int; /*`xml:"int,attr"`*/
}

internal partial struct TestUnmarshalAttr_ParamStringPtr /*dyn*/ {
    public ж<@string> Int; /*`xml:"int,attr"`*/
}

public static void TestUnmarshalAttr(ж<testing.T> Ꮡt) {
    var x = slice<byte>(@"<Param int=""1"" />"u8);
    var p1 = Ꮡ(new TestUnmarshalAttr_ParamPtr(nil));
    {
        var err = Unmarshal(x, p1.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal: %s"u8, err);
        }
    }
    if ((~p1).Int == nil){
        Ꮡt.Fatalf("Unmarshal failed in to *int field"u8);
    } else 
    if ((~p1).Int.Value != 1) {
        Ꮡt.Fatalf("Unmarshal with %s failed:\nhave %#v,\n want %#v"u8, x, (~p1).Int.OrTypedNil(), (nint)(1));
    }
    var p2 = Ꮡ(new TestUnmarshalAttr_ParamVal(nil));
    {
        var err = Unmarshal(x, p2.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal: %s"u8, err);
        }
    }
    if ((~p2).Int != 1) {
        Ꮡt.Fatalf("Unmarshal with %s failed:\nhave %#v,\n want %#v"u8, x, (~p2).Int, (nint)(1));
    }
    var p3 = Ꮡ(new TestUnmarshalAttr_ParamStringPtr(nil));
    {
        var err = Unmarshal(x, p3.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal: %s"u8, err);
        }
    }
    if ((~p3).Int == nil){
        Ꮡt.Fatalf("Unmarshal failed in to *string field"u8);
    } else 
    if ((~p3).Int.Value != "1"u8) {
        Ꮡt.Fatalf("Unmarshal with %s failed:\nhave %#v,\n want %#v"u8, x, (~p3).Int.OrTypedNil(), (nint)(1));
    }
}

public partial struct Tables {
    public @string HTable; /*`xml:"http://www.w3.org/TR/html4/ table"`*/
    public @string FTable; /*`xml:"http://www.w3schools.com/furniture table"`*/
}


partial struct tablesᴛ1 /*dyn*/ {
    internal @string xml;
    internal Tables tab;
    internal @string ns;
}
internal static slice<tablesᴛ1> tables = new tablesᴛ1[]{
    new(
        xml: @"<Tables>"u8 + @"<table xmlns=""http://www.w3.org/TR/html4/"">hello</table>"u8 + @"<table xmlns=""http://www.w3schools.com/furniture"">world</table>"u8 + @"</Tables>"u8,
        tab: new Tables("hello"u8, "world"u8)
    ),
    new(
        xml: @"<Tables>"u8 + @"<table xmlns=""http://www.w3schools.com/furniture"">world</table>"u8 + @"<table xmlns=""http://www.w3.org/TR/html4/"">hello</table>"u8 + @"</Tables>"u8,
        tab: new Tables("hello"u8, "world"u8)
    ),
    new(
        xml: @"<Tables xmlns:f=""http://www.w3schools.com/furniture"" xmlns:h=""http://www.w3.org/TR/html4/"">"u8 + @"<f:table>world</f:table>"u8 + @"<h:table>hello</h:table>"u8 + @"</Tables>"u8,
        tab: new Tables("hello"u8, "world"u8)
    ),
    new(
        xml: @"<Tables>"u8 + @"<table>bogus</table>"u8 + @"</Tables>"u8,
        tab: new Tables(nil)
    ),
    new(
        xml: @"<Tables>"u8 + @"<table>only</table>"u8 + @"</Tables>"u8,
        tab: new Tables(HTable: "only"u8),
        ns: "http://www.w3.org/TR/html4/"u8
    ),
    new(
        xml: @"<Tables>"u8 + @"<table>only</table>"u8 + @"</Tables>"u8,
        tab: new Tables(FTable: "only"u8),
        ns: "http://www.w3schools.com/furniture"u8
    ),
    new(
        xml: @"<Tables>"u8 + @"<table>only</table>"u8 + @"</Tables>"u8,
        tab: new Tables(nil),
        ns: "something else entirely"u8
    )
}.slice();

public static void TestUnmarshalNS(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in tables) {
        ref var dst = ref heap(new Tables(), out var Ꮡdst);
        error err = default!;
        if (tt.ns != ""u8){
            var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tt.xml)));
            d.Value.DefaultSpace = tt.ns;
            err = d.Decode(Ꮡdst);
        } else {
            err = Unmarshal(slice<byte>(tt.xml), Ꮡdst);
        }
        if (err != default!) {
            Ꮡt.Errorf("#%d: Unmarshal: %v"u8, i, err);
            continue;
        }
        var want = tt.tab;
        if (dst != want) {
            Ꮡt.Errorf("#%d: dst=%+v, want %+v"u8, i, dst, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tablesTableXmlnsHttpWwwˢ = @"<Tables><table xmlns=""http://www.w3.org/TR/html4/"">hello</table><table xmlns=""http://www.w3schools.com/furniture"">world</table></Tables>"u8;

public static void TestMarshalNS(ж<testing.T> Ꮡt) {
    ref var dst = ref heap<Tables>(out var Ꮡdst);
    dst = new Tables("hello"u8, "world"u8);
    var (data, err) = Marshal(Ꮡdst);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal: %v"u8, err);
    }
    @string want = tablesTableXmlnsHttpWwwˢ;
    @string str = ((@string)data);
    if (str != want) {
        Ꮡt.Errorf("have: %q\nwant: %q\n"u8, str, want);
    }
}

public partial struct TableAttrs {
    public TAttr TAttr;
}

public partial struct TAttr {
    public @string HTable; /*`xml:"http://www.w3.org/TR/html4/ table,attr"`*/
    public @string FTable; /*`xml:"http://www.w3schools.com/furniture table,attr"`*/
    public @string Lang; /*`xml:"http://www.w3.org/XML/1998/namespace lang,attr,omitempty"`*/
    public @string Other1; /*`xml:"http://golang.org/xml/ other,attr,omitempty"`*/
    public @string Other2; /*`xml:"http://golang.org/xmlfoo/ other,attr,omitempty"`*/
    public @string Other3; /*`xml:"http://golang.org/json/ other,attr,omitempty"`*/
    public @string Other4; /*`xml:"http://golang.org/2/json/ other,attr,omitempty"`*/
}

// Default space does not apply to attribute names.
// Default space does not apply to attribute names.
// Default space does not apply to attribute names.
// Default space does not apply to attribute names.

partial struct tableAttrsᴛ1 /*dyn*/ {
    internal @string xml;
    internal TableAttrs tab;
    internal @string ns;
}
internal static slice<tableAttrsᴛ1> tableAttrs = new tableAttrsᴛ1[]{
    new(
        xml: @"<TableAttrs xmlns:f=""http://www.w3schools.com/furniture"" xmlns:h=""http://www.w3.org/TR/html4/""><TAttr "u8 + @"h:table=""hello"" f:table=""world"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(new TAttr(HTable: "hello"u8, FTable: "world"u8))
    ),
    new(
        xml: @"<TableAttrs><TAttr xmlns:f=""http://www.w3schools.com/furniture"" xmlns:h=""http://www.w3.org/TR/html4/"" "u8 + @"h:table=""hello"" f:table=""world"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(new TAttr(HTable: "hello"u8, FTable: "world"u8))
    ),
    new(
        xml: @"<TableAttrs><TAttr "u8 + @"h:table=""hello"" f:table=""world"" xmlns:f=""http://www.w3schools.com/furniture"" xmlns:h=""http://www.w3.org/TR/html4/"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(new TAttr(HTable: "hello"u8, FTable: "world"u8))
    ),
    new(
        xml: @"<TableAttrs xmlns=""http://www.w3schools.com/furniture"" xmlns:h=""http://www.w3.org/TR/html4/""><TAttr "u8 + @"h:table=""hello"" table=""world"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(new TAttr(HTable: "hello"u8, FTable: ""u8))
    ),
    new(
        xml: @"<TableAttrs xmlns:f=""http://www.w3schools.com/furniture""><TAttr xmlns=""http://www.w3.org/TR/html4/"" "u8 + @"table=""hello"" f:table=""world"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(new TAttr(HTable: ""u8, FTable: "world"u8))
    ),
    new(
        xml: @"<TableAttrs><TAttr "u8 + @"table=""bogus"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(nil)
    ),
    new(
        xml: @"<TableAttrs xmlns:h=""http://www.w3.org/TR/html4/""><TAttr "u8 + @"h:table=""hello"" table=""world"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(new TAttr(HTable: "hello"u8, FTable: ""u8)),
        ns: "http://www.w3schools.com/furniture"u8
    ),
    new(
        xml: @"<TableAttrs xmlns:f=""http://www.w3schools.com/furniture""><TAttr "u8 + @"table=""hello"" f:table=""world"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(new TAttr(HTable: ""u8, FTable: "world"u8)),
        ns: "http://www.w3.org/TR/html4/"u8
    ),
    new(
        xml: @"<TableAttrs><TAttr "u8 + @"table=""bogus"" "u8 + @"/></TableAttrs>"u8,
        tab: new TableAttrs(nil),
        ns: "something else entirely"u8
    )
}.slice();

public static void TestUnmarshalNSAttr(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in tableAttrs) {
        ref var dst = ref heap(new TableAttrs(), out var Ꮡdst);
        error err = default!;
        if (tt.ns != ""u8){
            var d = NewDecoder(new xml_test_package.strings_ReaderжReader(strings.NewReader(tt.xml)));
            d.Value.DefaultSpace = tt.ns;
            err = d.Decode(Ꮡdst);
        } else {
            err = Unmarshal(slice<byte>(tt.xml), Ꮡdst);
        }
        if (err != default!) {
            Ꮡt.Errorf("#%d: Unmarshal: %v"u8, i, err);
            continue;
        }
        var want = tt.tab;
        if (dst != want) {
            Ꮡt.Errorf("#%d: dst=%+v, want %+v"u8, i, dst, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tableAttrsTAttrXmlnsˢ = @"<TableAttrs><TAttr xmlns:html4=""http://www.w3.org/TR/html4/"" html4:table=""hello"" xmlns:furniture=""http://www.w3schools.com/furniture"" furniture:table=""world"" xml:lang=""en_US"" xmlns:_xml=""http://golang.org/xml/"" _xml:other=""other1"" xmlns:_xmlfoo=""http://golang.org/xmlfoo/"" _xmlfoo:other=""other2"" xmlns:json=""http://golang.org/json/"" json:other=""other3"" xmlns:json_1=""http://golang.org/2/json/"" json_1:other=""other4""></TAttr></TableAttrs>"u8;

public static void TestMarshalNSAttr(ж<testing.T> Ꮡt) {
    ref var src = ref heap<TableAttrs>(out var Ꮡsrc);
    src = new TableAttrs(new TAttr("hello"u8, "world"u8, "en_US"u8, "other1"u8, "other2"u8, "other3"u8, "other4"u8));
    var (data, err) = Marshal(Ꮡsrc);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal: %v"u8, err);
    }
    @string want = tableAttrsTAttrXmlnsˢ;
    @string str = ((@string)data);
    if (str != want) {
        Ꮡt.Errorf("Marshal:\nhave: %#q\nwant: %#q\n"u8, str, want);
    }
    ref var dst = ref heap(new TableAttrs(), out var Ꮡdst);
    {
        var errΔ1 = Unmarshal(data, Ꮡdst); if (errΔ1 != default!) {
            Ꮡt.Errorf("Unmarshal: %v"u8, errΔ1);
        }
    }
    if (dst != src) {
        Ꮡt.Errorf("Unmarshal = %q, want %q"u8, dst, src);
    }
}

public partial struct MyCharData {
    internal @string body;
}

public static error UnmarshalXML(this ref MyCharData m, ж<global::go.encoding.xml_package.Decoder> Ꮡd, global::go.encoding.xml_package.StartElement start) {
    ref var d = ref Ꮡd.DerefOrNull();

    while (ᐧ) {
        var (t, err) = d.Token();
        if (AreEqual(err, io.EOF)) {
            // found end of element
            break;
        }
        if (err != default!) {
            return err;
        }
        {
            var (@char, ok) = t._<CharData>(ᐧ); if (ok) {
                m.body += ((@string)(slice<byte>)@char);
            }
        }
    }
    return default!;
}

internal static global::go.encoding.xml_package.Unmarshaler _ᴛ3ʗ = new xml_internal_test_package.MyCharDataжUnmarshaler(((ж<MyCharData>)nil));

public static error UnmarshalXMLAttr(this ref MyCharData m, global::go.encoding.xml_package.Attr attr) {
    throw panic("must not call");
}

public partial struct MyAttr {
    internal @string attr;
}

public static error UnmarshalXMLAttr(this ref MyAttr m, global::go.encoding.xml_package.Attr attr) {
    m.attr = attr.Value;
    return default!;
}

internal static global::go.encoding.xml_package.UnmarshalerAttr _ᴛ4ʗ = new xml_internal_test_package.MyAttrжUnmarshalerAttr(((ж<MyAttr>)nil));

public partial struct MyStruct {
    public ж<MyCharData> Data;
    public ж<MyAttr> Attr; /*`xml:",attr"`*/
    public MyCharData Data2;
    public MyAttr Attr2; /*`xml:",attr"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xmlVersion10EncodingUtf8ˢ2 = """
<?xml version="1.0" encoding="utf-8"?>
		<MyStruct Attr="attr1" Attr2="attr2">
		<Data>hello <!-- comment -->world</Data>
		<Data2>howdy <!-- comment -->world</Data2>
		</MyStruct>
	
"""u8;

public static void TestUnmarshaler(ж<testing.T> Ꮡt) {
    @string xml = xmlVersion10EncodingUtf8ˢ2;
    ref var m = ref heap(new MyStruct(), out var Ꮡm);
    {
        var err = Unmarshal(slice<byte>(xml), Ꮡm); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    if (m.Data == nil || m.Attr == nil || (~m.Data).body != "hello world"u8 || (~m.Attr).attr != "attr1"u8 || m.Data2.body != "howdy world"u8 || m.Attr2.attr != "attr2"u8) {
        Ꮡt.Errorf("m=%#+v\n"u8, m);
    }
}

public partial struct Pea {
    public @string Cotelydon;
}

public partial struct Pod {
    public any Pea; /*`xml:"Pea"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string podPeaCotelydonGreenˢ = @"<Pod><Pea><Cotelydon>Green stuff</Cotelydon></Pea></Pod>"u8;
internal static readonly @string greenStuffˢ = "Green stuff"u8;

// https://golang.org/issue/6836
public static void TestUnmarshalIntoInterface(ж<testing.T> Ꮡt) {
    var pod = @new<Pod>();
    pod.Value.Pea = @new<Pea>();
    @string xml = podPeaCotelydonGreenˢ;
    var err = Unmarshal(slice<byte>(xml), pod.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("failed to unmarshal %q: %v"u8, xml, err);
    }
    var (pea, ok) = (~pod).Pea._<ж<Pea>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("unmarshaled into wrong type: have %T want *Pea"u8, (~pod).Pea);
    }
    @string have = pea.Value.Cotelydon;
    @string want = greenStuffˢ;
    if (have != want) {
        Ꮡt.Errorf("failed to unmarshal into interface, have %q want %q"u8, have, want);
    }
}

public partial struct X {
    public @string D; /*`xml:",comment"`*/
}

// Issue 11112. Unmarshal must reject invalid comments.
public static void TestMalformedComment(ж<testing.T> Ꮡt) {
    var testData = new @string[]{
        "<X><!-- a---></X>"u8,
        "<X><!-- -- --></X>"u8,
        "<X><!-- a--b --></X>"u8,
        "<X><!------></X>"u8
    }.slice();
    foreach (var (i, test) in testData) {
        var data = slice<byte>(test);
        var v = @new<X>();
        {
            var err = Unmarshal(data, v.OrTypedNil()); if (err == default!) {
                Ꮡt.Errorf("%d: unmarshal should reject invalid comments"u8, i);
            }
        }
    }
}

public partial struct IXField {
    public nint Five; /*`xml:"five"`*/
    public slice<@string> NotInnerXML; /*`xml:",innerxml"`*/
}

// Issue 15600. ",innerxml" on a field that can't hold it.
public static void TestInvalidInnerXMLType(ж<testing.T> Ꮡt) {
    var v = @new<IXField>();
    {
        var err = Unmarshal(slice<byte>(@"<tag><five>5</five><innertag/></tag>"u8), v.OrTypedNil()); if (err != default!) {
            Ꮡt.Errorf("Unmarshal failed: got %v"u8, err);
        }
    }
    if ((~v).Five != 5) {
        Ꮡt.Errorf("Five = %v, want 5"u8, (~v).Five);
    }
    if ((~v).NotInnerXML != default!) {
        Ꮡt.Errorf("NotInnerXML = %v, want nil"u8, (~v).NotInnerXML);
    }
}

partial struct Child_G /*dyn*/ {
    public nint I;
}

public partial struct Child {
    public Child_G G;
}

public partial struct ChildToEmbed {
    public bool X;
}

public partial struct Parent {
    public nint I;
    public ж<nint> IPtr;
    public slice<nint> Is;
    public slice<ж<nint>> IPtrs;
    public float32 F;
    public ж<float32> FPtr;
    public slice<float32> Fs;
    public slice<ж<float32>> FPtrs;
    public bool B;
    public ж<bool> BPtr;
    public slice<bool> Bs;
    public slice<ж<bool>> BPtrs;
    public slice<byte> Bytes;
    public ж<slice<byte>> BytesPtr;
    public @string S;
    public ж<@string> SPtr;
    public slice<@string> Ss;
    public slice<ж<@string>> SPtrs;
    public MyInt MyI;
    public Child Child;
    public slice<Child> Children;
    public ж<Child> ChildPtr;
    public partial ref ChildToEmbed ChildToEmbed { get; }
}

internal static readonly @string emptyXML = """

<Parent>
    <I></I>
    <IPtr></IPtr>
    <Is></Is>
    <IPtrs></IPtrs>
    <F></F>
    <FPtr></FPtr>
    <Fs></Fs>
    <FPtrs></FPtrs>
    <B></B>
    <BPtr></BPtr>
    <Bs></Bs>
    <BPtrs></BPtrs>
    <Bytes></Bytes>
    <BytesPtr></BytesPtr>
    <S></S>
    <SPtr></SPtr>
    <Ss></Ss>
    <SPtrs></SPtrs>
    <MyI></MyI>
    <Child></Child>
    <Children></Children>
    <ChildPtr></ChildPtr>
    <X></X>
</Parent>

"""u8;

// golang.org/issues/13417
public static void TestUnmarshalEmptyValues(ж<testing.T> Ꮡt) {
    // Test first with a zero-valued dst.
    var v = @new<Parent>();
    {
        var err = Unmarshal(slice<byte>(emptyXML), v.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatalf("zero: Unmarshal failed: got %v"u8, err);
        }
    }
    ref var zBytes = ref heap<slice<byte>>(out var ᏑzBytes);
    zBytes = new byte[]{}.slice();
    ref var zInt = ref heap<nint>(out var ᏑzInt);
    zInt = 0;
    ref var zStr = ref heap<@string>(out var ᏑzStr);
    zStr = ""u8;
    ref var zFloat = ref heap<float32>(out var ᏑzFloat);
    zFloat = (float32)0F;
    ref var zBool = ref heap<bool>(out var ᏑzBool);
    zBool = false;
    var want = Ꮡ(new Parent(
        IPtr: ᏑzInt,
        Is: new nint[]{zInt}.slice(),
        IPtrs: new ж<nint>[]{ᏑzInt}.slice(),
        FPtr: ᏑzFloat,
        Fs: new float32[]{zFloat}.slice(),
        FPtrs: new ж<float32>[]{ᏑzFloat}.slice(),
        BPtr: ᏑzBool,
        Bs: new bool[]{zBool}.slice(),
        BPtrs: new ж<bool>[]{ᏑzBool}.slice(),
        Bytes: new byte[]{}.slice(),
        BytesPtr: ᏑzBytes,
        SPtr: ᏑzStr,
        Ss: new @string[]{zStr}.slice(),
        SPtrs: new ж<@string>[]{ᏑzStr}.slice(),
        Children: new Child[]{new()}.slice(),
        ChildPtr: @new<Child>(),
        ChildToEmbed: new ChildToEmbed(nil)
    ));
    if (!reflect.DeepEqual(v.OrTypedNil(), want.OrTypedNil())) {
        Ꮡt.Fatalf("zero: Unmarshal:\nhave:  %#+v\nwant: %#+v"u8, v.OrTypedNil(), want.OrTypedNil());
    }
    // Test with a pre-populated dst.
    // Multiple addressable copies, as pointer-to fields will replace value during unmarshal.
    var vBytes0 = slice<byte>("x"u8);
    ref var vInt0 = ref heap<nint>(out var ᏑvInt0);
    vInt0 = 1;
    ref var vStr0 = ref heap<@string>(out var ᏑvStr0);
    vStr0 = "x"u8;
    ref var vFloat0 = ref heap<float32>(out var ᏑvFloat0);
    vFloat0 = (float32)1F;
    ref var vBool0 = ref heap<bool>(out var ᏑvBool0);
    vBool0 = true;
    ref var vBytes1 = ref heap<slice<byte>>(out var ᏑvBytes1);
    vBytes1 = slice<byte>("x"u8);
    ref var vInt1 = ref heap<nint>(out var ᏑvInt1);
    vInt1 = 1;
    ref var vStr1 = ref heap<@string>(out var ᏑvStr1);
    vStr1 = "x"u8;
    ref var vFloat1 = ref heap<float32>(out var ᏑvFloat1);
    vFloat1 = (float32)1F;
    ref var vBool1 = ref heap<bool>(out var ᏑvBool1);
    vBool1 = true;
    ref var vInt2 = ref heap<nint>(out var ᏑvInt2);
    vInt2 = 1;
    ref var vStr2 = ref heap<@string>(out var ᏑvStr2);
    vStr2 = "x"u8;
    ref var vFloat2 = ref heap<float32>(out var ᏑvFloat2);
    vFloat2 = (float32)1F;
    ref var vBool2 = ref heap<bool>(out var ᏑvBool2);
    vBool2 = true;
    v = Ꮡ(new Parent(
        I: vInt0,
        IPtr: ᏑvInt1,
        Is: new nint[]{vInt0}.slice(),
        IPtrs: new ж<nint>[]{ᏑvInt2}.slice(),
        F: vFloat0,
        FPtr: ᏑvFloat1,
        Fs: new float32[]{vFloat0}.slice(),
        FPtrs: new ж<float32>[]{ᏑvFloat2}.slice(),
        B: vBool0,
        BPtr: ᏑvBool1,
        Bs: new bool[]{vBool0}.slice(),
        BPtrs: new ж<bool>[]{ᏑvBool2}.slice(),
        Bytes: vBytes0,
        BytesPtr: ᏑvBytes1,
        S: vStr0,
        SPtr: ᏑvStr1,
        Ss: new @string[]{vStr0}.slice(),
        SPtrs: new ж<@string>[]{ᏑvStr2}.slice(),
        MyI: ((MyInt)vInt0),
        Child: new Child(G: new Child_G(I: vInt0)),
        Children: new Child[]{new(G: new Child_G(I: vInt0))}.slice(),
        ChildPtr: Ꮡ(new Child(G: new Child_G(I: vInt0))),
        ChildToEmbed: new ChildToEmbed(X: vBool0)
    ));
    {
        var err = Unmarshal(slice<byte>(emptyXML), v.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatalf("populated: Unmarshal failed: got %v"u8, err);
        }
    }
    want = Ꮡ(new Parent(
        IPtr: ᏑzInt,
        Is: new nint[]{vInt0, zInt}.slice(),
        IPtrs: new ж<nint>[]{ᏑvInt0, ᏑzInt}.slice(),
        FPtr: ᏑzFloat,
        Fs: new float32[]{vFloat0, zFloat}.slice(),
        FPtrs: new ж<float32>[]{ᏑvFloat0, ᏑzFloat}.slice(),
        BPtr: ᏑzBool,
        Bs: new bool[]{vBool0, zBool}.slice(),
        BPtrs: new ж<bool>[]{ᏑvBool0, ᏑzBool}.slice(),
        Bytes: new byte[]{}.slice(),
        BytesPtr: ᏑzBytes,
        SPtr: ᏑzStr,
        Ss: new @string[]{vStr0, zStr}.slice(),
        SPtrs: new ж<@string>[]{ᏑvStr0, ᏑzStr}.slice(),
        Child: new Child(G: new Child_G(I: vInt0)), // I should == zInt0? (zero value)

        Children: new Child[]{new(G: new Child_G(I: vInt0)), new()}.slice(),
        ChildPtr: Ꮡ(new Child(G: new Child_G(I: vInt0)))
    ));
    // I should == zInt0? (zero value)
    if (!reflect.DeepEqual(v.OrTypedNil(), want.OrTypedNil())) {
        Ꮡt.Fatalf("populated: Unmarshal:\nhave:  %#+v\nwant: %#+v"u8, v.OrTypedNil(), want.OrTypedNil());
    }
}

public partial struct WhitespaceValuesParent {
    public bool BFalse;
    public bool BTrue;
    public nint I;
    public nint INeg;
    public int8 I8;
    public int8 I8Neg;
    public int16 I16;
    public int16 I16Neg;
    public int32 I32;
    public int32 I32Neg;
    public int64 I64;
    public int64 I64Neg;
    public nuint UI;
    public uint8 UI8;
    public uint16 UI16;
    public uint32 UI32;
    public uint64 UI64;
    public float32 F32;
    public float32 F32Neg;
    public float64 F64;
    public float64 F64Neg;
}

internal static readonly @string whitespaceValuesXML = """

<WhitespaceValuesParent>
    <BFalse>   false   </BFalse>
    <BTrue>   true   </BTrue>
    <I>   266703   </I>
    <INeg>   -266703   </INeg>
    <I8>  112  </I8>
    <I8Neg>  -112  </I8Neg>
    <I16>  6703  </I16>
    <I16Neg>  -6703  </I16Neg>
    <I32>  266703  </I32>
    <I32Neg>  -266703  </I32Neg>
    <I64>  266703  </I64>
    <I64Neg>  -266703  </I64Neg>
    <UI>   266703   </UI>
    <UI8>  112  </UI8>
    <UI16>  6703  </UI16>
    <UI32>  266703  </UI32>
    <UI64>  266703  </UI64>
    <F32>  266.703  </F32>
    <F32Neg>  -266.703  </F32Neg>
    <F64>  266.703  </F64>
    <F64Neg>  -266.703  </F64Neg>
</WhitespaceValuesParent>

"""u8;

// golang.org/issues/22146
public static void TestUnmarshalWhitespaceValues(ж<testing.T> Ꮡt) {
    ref var v = ref heap<WhitespaceValuesParent>(out var Ꮡv);
    v = new WhitespaceValuesParent(nil);
    {
        var err = Unmarshal(slice<byte>(whitespaceValuesXML), Ꮡv); if (err != default!) {
            Ꮡt.Fatalf("whitespace values: Unmarshal failed: got %v"u8, err);
        }
    }
    var want = new WhitespaceValuesParent(
        BFalse: false,
        BTrue: true,
        I: 266703,
        INeg: -266703,
        I8: 112,
        I8Neg: (int8)(-112),
        I16: 6703,
        I16Neg: (int16)(-6703),
        I32: 266703,
        I32Neg: -266703,
        I64: 266703,
        I64Neg: -266703,
        UI: 266703,
        UI8: 112,
        UI16: 6703,
        UI32: 266703,
        UI64: 266703,
        F32: 266.703F,
        F32Neg: -266.703F,
        F64: 266.703D,
        F64Neg: -266.703D
    );
    if (v != want) {
        Ꮡt.Fatalf("whitespace values: Unmarshal:\nhave: %#+v\nwant: %#+v"u8, v, want);
    }
}

public partial struct WhitespaceAttrsParent {
    public bool BFalse; /*`xml:",attr"`*/
    public bool BTrue; /*`xml:",attr"`*/
    public nint I; /*`xml:",attr"`*/
    public nint INeg; /*`xml:",attr"`*/
    public int8 I8; /*`xml:",attr"`*/
    public int8 I8Neg; /*`xml:",attr"`*/
    public int16 I16; /*`xml:",attr"`*/
    public int16 I16Neg; /*`xml:",attr"`*/
    public int32 I32; /*`xml:",attr"`*/
    public int32 I32Neg; /*`xml:",attr"`*/
    public int64 I64; /*`xml:",attr"`*/
    public int64 I64Neg; /*`xml:",attr"`*/
    public nuint UI; /*`xml:",attr"`*/
    public uint8 UI8; /*`xml:",attr"`*/
    public uint16 UI16; /*`xml:",attr"`*/
    public uint32 UI32; /*`xml:",attr"`*/
    public uint64 UI64; /*`xml:",attr"`*/
    public float32 F32; /*`xml:",attr"`*/
    public float32 F32Neg; /*`xml:",attr"`*/
    public float64 F64; /*`xml:",attr"`*/
    public float64 F64Neg; /*`xml:",attr"`*/
}

internal static readonly @string whitespaceAttrsXML = """

<WhitespaceAttrsParent
    BFalse="  false  "
    BTrue="  true  "
    I="  266703  "
    INeg="  -266703  "
    I8="  112  "
    I8Neg="  -112  "
    I16="  6703  "
    I16Neg="  -6703  "
    I32="  266703  "
    I32Neg="  -266703  "
    I64="  266703  "
    I64Neg="  -266703  "
    UI="  266703  "
    UI8="  112  "
    UI16="  6703  "
    UI32="  266703  "
    UI64="  266703  "
    F32="  266.703  "
    F32Neg="  -266.703  "
    F64="  266.703  "
    F64Neg="  -266.703  "
>
</WhitespaceAttrsParent>

"""u8;

// golang.org/issues/22146
public static void TestUnmarshalWhitespaceAttrs(ж<testing.T> Ꮡt) {
    ref var v = ref heap<WhitespaceAttrsParent>(out var Ꮡv);
    v = new WhitespaceAttrsParent(nil);
    {
        var err = Unmarshal(slice<byte>(whitespaceAttrsXML), Ꮡv); if (err != default!) {
            Ꮡt.Fatalf("whitespace attrs: Unmarshal failed: got %v"u8, err);
        }
    }
    var want = new WhitespaceAttrsParent(
        BFalse: false,
        BTrue: true,
        I: 266703,
        INeg: -266703,
        I8: 112,
        I8Neg: (int8)(-112),
        I16: 6703,
        I16Neg: (int16)(-6703),
        I32: 266703,
        I32Neg: -266703,
        I64: 266703,
        I64Neg: -266703,
        UI: 266703,
        UI8: 112,
        UI16: 6703,
        UI32: 266703,
        UI64: 266703,
        F32: 266.703F,
        F32Neg: -266.703F,
        F64: 266.703D,
        F64Neg: -266.703D
    );
    if (v != want) {
        Ꮡt.Fatalf("whitespace attrs: Unmarshal:\nhave: %#+v\nwant: %#+v"u8, v, want);
    }
}

internal partial struct TestUnmarshalIntoNil_T /*dyn*/ {
    public nint A; /*`xml:"A"`*/
}

// golang.org/issues/53350
public static void TestUnmarshalIntoNil(ж<testing.T> Ꮡt) {
    ж<TestUnmarshalIntoNil_T> nilPointer = default!;
    var err = Unmarshal(slice<byte>("<T><A>1</A></T>"u8), nilPointer.OrTypedNil());
    if (err == default!) {
        Ꮡt.Fatalf("no error in unmarshaling"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshalDidNotFailˢ = (@string)"Unmarshal did not fail"u8;

internal partial struct TestCVE202228131_nested /*dyn*/ {
    public ж<TestCVE202228131_nested> Parent; /*`xml:",any"`*/
}

public static void TestCVE202228131(ж<testing.T> Ꮡt) {
    ref var n = ref heap(new TestCVE202228131_nested(), out var Ꮡn);
    var err = Unmarshal(bytes.Repeat(slice<byte>("<a>"u8), maxUnmarshalDepth + 1), Ꮡn);
    if (err == default!){
        Ꮡt.Fatal(unmarshalDidNotFailˢ);
    } else 
    if (!errors.Is(err, errUnmarshalDepth)) {
        Ꮡt.Fatalf("Unmarshal unexpected error: got %q, want %q"u8, err, errUnmarshalDepth);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testRequiresSignificantˢ = (@string)"test requires significant memory"u8;
internal static readonly object unmarshalPanickedˢ = (@string)"Unmarshal panicked"u8;

internal partial struct TestCVE202230633_example /*dyn*/ {
    public slice<@string> Things;
}

public static void TestCVE202230633(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short() || runtime.GOARCH == "wasm"u8) {
            Ꮡt.Skip(testRequiresSignificantˢ);
        }
        defer(() => {
            var p = recover();
            if (p != default!) {
                Ꮡt.Fatal(unmarshalPanickedˢ);
            }
        }, ref ᒐ);
        ref var example = ref heap(new TestCVE202230633_example(), out var Ꮡexample);
        Unmarshal(bytes.Repeat(slice<byte>("<a>"u8), 17_000_000), Ꮡexample);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end xml_internal_test_package
