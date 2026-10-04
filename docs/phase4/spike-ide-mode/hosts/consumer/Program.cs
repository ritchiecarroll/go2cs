using go.example.com;

for (var i = 0; i < 180; i++)
{
    Console.WriteLine($"{Environment.ProcessId} {i} {shared_package.Greet("dotnet")} cs-a");
    Thread.Sleep(500);
}
