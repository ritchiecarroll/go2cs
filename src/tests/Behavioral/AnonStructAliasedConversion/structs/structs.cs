namespace go.AnonStructAliasedConversion;

partial class structs_package {

[GoType] partial struct AssignB {
    public nint A;
}

public static nint Sum(this AssignB x) {
    return x.A * 10;
}

} // end structs_package
