namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void answer(channel<nint> ch, nint v) {
    ch.ᐸꟷ(v);
}

internal static partial void rowReceive() {
    var ch = new channel<nint>(0);
    goǃ(ᴛ1 => answer(ᴛ1, 1), ch);
    fmt.Println("receive:", ᐸꟷ(ch));
}

internal static partial void rowWait() {
    var done = new channel<bool>(0);
    var relay = new channel<nint>(0);
    var relayʗ1 = relay;
    goǃ(() => {
        relayʗ1.ᐸꟷ(2);
    });
    var doneʗ1 = done;
    var relayʗ2 = relay;
    goǃ(() => {
        fmt.Println("wait:", ᐸꟷ(relayʗ2));
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
}

internal static void rowElement() {
    var steps = slice<Action>([rowReceive, rowWait]);
    foreach (var i in range(len(steps))) {
        steps[i]();
    }
}

internal static slice<@string> trace;

internal static nint nextIndex() {
    trace = append(trace, (@string)"index");
    return 0;
}

internal static nint arg() {
    trace = append(trace, (@string)"arg");
    return 20;
}

internal static partial nint @double(nint v) {
    var ch = new channel<nint>(0);
    var chʗ1 = ch;
    goǃ(() => {
        chʗ1.ᐸꟷ(v * 2);
    });
    return ᐸꟷ(ch);
}

internal static void rowValue() {
    var fs = slice<Func<nint, nint>>([@double]);
    nint x = fs[nextIndex()](arg());
    fmt.Println("value:", x, trace);
}

partial struct job {
    internal Action run;
}

internal static void rowJob() {
    var j = new job(run: rowReceive);
    j.run();
}

internal partial struct main_rows /*dyn*/ {
    internal @string name;
    internal Action run;
}

internal static void main() {
    var rows = slice<main_rows>([
        new("receive", rowReceive),
        new("wait", rowWait),
        new("element", rowElement),
        new("value", rowValue),
        new("job", rowJob)
    ]);

    foreach (var (_, r) in range(rows)) {
        fmt.Println("row", r.name);
        r.run();
    }
}

} // end main_package
