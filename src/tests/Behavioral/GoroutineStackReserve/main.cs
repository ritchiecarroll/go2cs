namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static UntypedInt depth => 200_000;

internal static nint depthOf(nint n, /*[4]*/ array<nint> acc) {
    acc = acc.Clone();

    if (n == 0) {
        return acc[0] + acc[1] + acc[2] + acc[3];
    }

    acc[n % 4]++;
    return depthOf(n - 1, acc);
}

internal static void produce(nint id, channel/*<-*/<nint> @out) {
    @out.ᐸꟷ(square(id));
}

internal static nint square(nint n) {
    return n * n;
}

internal static partial void Main() {
    var deep = new channel<nint>(0);

    var deepʗ1 = deep;
    goǃ(() => {
        deepʗ1.ᐸꟷ(depthOf(depth, new nint[]{}.array(4)));
    });

    fmt.Println("deep recursion levels:", ᐸꟷ(deep));

    const nint workers = 100;
    var @out = new channel<nint>(workers);

    for (nint i = 1; i <= workers; i++) {
        goǃ(produce, i, @out.WithDirection(GoChanDir.Send), GoStack.Bounded);
    }

    nint sum = 0;
    foreach (var _ᴛ1 in range(workers)) {
        sum += ᐸꟷ(@out);
    }

    fmt.Println("bounded goroutines:", (nint)(workers), "sum of squares:", sum);
}

} // end main_package
