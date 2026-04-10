using MDator.Samples.Console.Features;

while (true)
{
    System.Console.Clear();
    System.Console.ForegroundColor = ConsoleColor.Cyan;
    System.Console.WriteLine("=== MDator Samples ===");
    System.Console.ResetColor();
    System.Console.WriteLine();
    System.Console.WriteLine("  1. Basic Requests");
    System.Console.WriteLine("  2. Notifications");
    System.Console.WriteLine("  3. Pipeline Behaviors");
    System.Console.WriteLine("  4. Streaming");
    System.Console.WriteLine("  5. Validation");
    System.Console.WriteLine("  6. Exception Handling");
    System.Console.WriteLine("  7. Pre/Post Processors");
    System.Console.WriteLine("  0. Exit");
    System.Console.WriteLine();
    System.Console.Write("Choose an option: ");

    var key = System.Console.ReadKey(intercept: true).KeyChar;
    System.Console.WriteLine(key);
    System.Console.WriteLine();

    switch (key)
    {
        case '1': await BasicRequestsDemo.RunAsync(); break;
        case '2': await NotificationsDemo.RunAsync(); break;
        case '3': await PipelineBehaviorsDemo.RunAsync(); break;
        case '4': await StreamingDemo.RunAsync(); break;
        case '5': await ValidationDemo.RunAsync(); break;
        case '6': await ExceptionHandlingDemo.RunAsync(); break;
        case '7': await PrePostProcessorsDemo.RunAsync(); break;
        case '0': return;
        default:
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Invalid option.");
            System.Console.ResetColor();
            break;
    }

    System.Console.WriteLine();
    System.Console.ForegroundColor = ConsoleColor.DarkGray;
    System.Console.WriteLine("Press any key to return to the menu...");
    System.Console.ResetColor();
    System.Console.ReadKey(intercept: true);
}
