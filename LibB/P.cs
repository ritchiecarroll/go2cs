namespace B;

public static class Local
{
    public class Nested { }
}

public static class P
{
    public static int M(A.Outer.Nested n) => n is null ? 0 : 1;
    public static int F(A.Flat f) => f is null ? 0 : 1;
    public static int L(Local.Nested l) => l is null ? 0 : 1;
}
