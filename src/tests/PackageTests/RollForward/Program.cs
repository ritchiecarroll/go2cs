// A go.lib consumer that prints the runtime major it runs on. `slice<int>` and `len` arrive through go.lib, so the
// program cannot build or start without the package.
public static class Program
{
    public static int Main()
    {
        slice<int> numbers = new[] { 1, 2, 3 };
        bool ok = len(numbers) == 3;
        System.Console.WriteLine($"ROLL-FORWARD: {(ok ? "ok" : "wrong")} runtime={System.Environment.Version.Major}");
        return ok ? 0 : 1;
    }
}
