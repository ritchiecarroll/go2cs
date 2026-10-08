namespace go.AnonStructNamedConversion;

partial class structs_package {

partial struct AssignB {
    public nint A;
}

public static nint Sum(this AssignB x) {
    return x.A * 10;
}

} // end structs_package
