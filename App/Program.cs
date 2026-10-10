using System.Diagnostics.CodeAnalysis;

static class Program
{
    // A documentation-ID DynamicDependency naming a method whose parameter is a nested type of ANOTHER assembly.
    [DynamicDependency("M(A.Outer.Nested)", typeof(B.P))]
    static int Main() => 0;
}
