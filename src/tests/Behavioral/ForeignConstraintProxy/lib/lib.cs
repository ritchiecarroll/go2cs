namespace go.ForeignConstraintProxy;

using static lib_package;

partial class lib_package {

partial interface Lesser<T> {
    bool Less(T _);
}

partial struct Ord2<T>
    where T : Lesser<T>
{
    public T V;
}

} partial class libᴛOrd2 {

public static bool Below<T>(this Ord2<T> o, T t)
    where T : Lesser<T>
{ return o.V.Less(t); }

} partial class lib_package {

public partial interface Ord_T<T> /*dyn*/ {
    bool Less(T _);
}

partial struct Ord<T>
    where T : Ord_T<T>
{
    public T V;
}

} partial class libᴛOrd {

public static bool Below<T>(this Ord<T> o, T t)
    where T : Ord_T<T>
{ return o.V.Less(t); }

} // end libᴛOrd
