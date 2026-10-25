// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using time = time_package;
using static go.encoding.xml_package;

partial class xml_internal_test_package {

internal static ж<Feed> atomValue = Ꮡ(new Feed(
    XMLName: new Name("http://www.w3.org/2005/Atom"u8, "feed"u8),
    Title: "Example Feed"u8,
    Link: new Link[]{new(Href: "http://example.org/"u8)}.slice(),
    Updated: ParseTime("2003-12-13T18:30:02Z"u8),
    Author: new Person(Name: "John Doe"u8),
    ID: "urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6"u8,

    Entry: new Entry[]{
        new(
            Title: "Atom-Powered Robots Run Amok"u8,
            Link: new Link[]{new(Href: "http://example.org/2003/12/13/atom03"u8)}.slice(),
            ID: "urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a"u8,
            Updated: ParseTime("2003-12-13T18:30:02Z"u8),
            Summary: NewText("Some text."u8)
        )
    }.slice()
));

internal static @string atomXML = @""u8 + @"<feed xmlns=""http://www.w3.org/2005/Atom"" updated=""2003-12-13T18:30:02Z"">"u8 + @"<title>Example Feed</title>"u8 + @"<id>urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6</id>"u8 + @"<link href=""http://example.org/""></link>"u8 + @"<author><name>John Doe</name><uri></uri><email></email></author>"u8 + @"<entry>"u8 + @"<title>Atom-Powered Robots Run Amok</title>"u8 + @"<id>urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a</id>"u8 + @"<link href=""http://example.org/2003/12/13/atom03""></link>"u8 + @"<updated>2003-12-13T18:30:02Z</updated>"u8 + @"<author><name></name><uri></uri><email></email></author>"u8 + @"<summary>Some text.</summary>"u8 + @"</entry>"u8 + @"</feed>"u8;

public static time.Time ParseTime(@string str) {
    var (t, err) = time.Parse(time.RFC3339, str);
    if (err != default!) {
        throw panic(err);
    }
    return t;
}

public static Text NewText(@string text) {
    return new Text(
        Body: text
    );
}

} // end xml_internal_test_package
