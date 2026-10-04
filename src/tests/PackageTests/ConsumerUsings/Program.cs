// No `using go;` and no `using static go.builtin;`: both come from go.lib's buildTransitive targets.
public static class Program
{
    public static int Main()
    {
        slice<int> numbers = new[] { 1, 2, 3 };
        @string text = "consumer usings";
        slice<int> none = nil;
        var ok = len(numbers) == 3 && len(text) == 15 && len(none) == 0;
        System.Console.WriteLine($"CONSUMER-USINGS: {(ok ? "ok" : "wrong")}");
        return ok ? 0 : 1;
    }
}
