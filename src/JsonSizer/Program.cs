using System.Text;

using JsonSizer;

byte[] jsonBytes;

switch (args.Length)
{
    case 0:
        jsonBytes = Encoding.UTF8.GetBytes(Console.In.ReadToEnd());
        break;
    case 1:
        jsonBytes = File.ReadAllBytes(args[0]);
        break;
    default:
        Console.WriteLine("Usage: jsonsizer [FILE]");
        return;
}

var bytesPerPath = Sizer.Size(jsonBytes);

foreach (var (path, bytes) in bytesPerPath)
{
    Console.WriteLine($"{path} {bytes}");
}