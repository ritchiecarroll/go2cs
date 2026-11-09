namespace go.github.com.itchyny;

partial class gojq_package {

partial interface Iter {
    (any, bool) Next();
}

public static Iter NewIter<T>(params Span<T> valuesʗp) {
    var values = valuesʗp.slice();

    switch (len(values)) {
    case 0: {
        return new emptyIter();
    }
    case 1: {
        return new unitIterжIter(Ꮡ(new unitIter(value: (values[0]).OrTypedNilOf())));
    }
    default: {
        ref var iter = ref heap<sliceIter<T>>((sliceIter<T>)values, out var Ꮡiter);
        return new sliceIterжIter<T>(Ꮡiter);
    }}

}

partial struct emptyIter {
}

internal static (any, bool) Next(this emptyIter _) {
    return (default, false);
}

partial struct unitIter {
    internal any value;
    internal bool done;
}

internal static (any, bool) Next(this ref unitIter iter) {
    if (iter.done) {
        return (default, false);
    }
    iter.done = true;
    return (iter.value, true);
}

partial struct sliceIter<T> /*[]T*/;

internal static (any, bool) Next<T>(this ref sliceIter<T> iter) {
    if (len(iter) == 0) {
        return (default, false);
    }
    var value = (iter)[0];
    iter = (iter)[1..];
    return ((value).OrTypedNilOf(), true);
}

} // end gojq_package
