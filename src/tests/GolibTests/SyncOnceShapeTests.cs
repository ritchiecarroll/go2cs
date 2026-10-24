#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using sync = go.sync_package;

namespace GolibTests;

/// <summary>
/// sync.Once's fields as Go 1.26 declares them: <c>_ noCopy</c>, then <c>done atomic.Bool</c> (an <c>atomic.Uint32</c>
/// through Go 1.25), then <c>m Mutex</c>. Do never shows the difference; reflection and <c>%#v</c> do. Expected text
/// is go1.26.10's own for the same program (linux/amd64, measured 2026-10-24). The Mutex that follows is the hand-owned
/// shape (a <c>gate</c> where Go has <c>mu</c>), so the formatted text is pinned up to it.
/// </summary>
[TestClass]
public class SyncOnceShapeTests
{
    [TestMethod]
    public void TheFieldsAreGos()
    {
        reflect.ΔType t = reflect.TypeOf(new sync.Once());

        Assert.AreEqual(3, (int)t.NumField(), "_, done, m");
        Assert.AreEqual("_ sync.noCopy", Field(t, 0));
        Assert.AreEqual("done atomic.Bool", Field(t, 1));
        Assert.AreEqual("m sync.Mutex", Field(t, 2));

        static string Field(reflect.ΔType t, int i)
        {
            reflect.StructField field = t.Field(i);
            return $"{field.Name} {field.Type.String()}";
        }
    }

    [TestMethod]
    public void FormattingReadsTheBool()
    {
        ж<sync.Once> once = builtin.@new<sync.Once>();

        StringAssert.StartsWith(fmt.Sprintf("%#v", once.Value).ToString(), "sync.Once{_:sync.noCopy{}, done:atomic.Bool{_:atomic.noCopy{}, v:0x0}, m:sync.Mutex{");

        once.Do(() => { });

        StringAssert.StartsWith(fmt.Sprintf("%v", once.Value).ToString(), "{{} {{} 1} {");
    }
}
