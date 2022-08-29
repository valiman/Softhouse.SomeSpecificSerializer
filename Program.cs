using System.Xml;
using System.Xml.Serialization;
using Softhouse.SomeSpecificSerializer.Models;

using Softhouse.SomeSpecificSerializer;
public class Program
{
    const string OUTPUT_FILE_NAME = "output.xml";
    const string EXPECTED_INPUT_FILE_EXTENSION = ".txt";
    static async Task Main(string[] args)
    {
        var pathInput = Path.GetFullPath(args[0]);
        if (!File.Exists(pathInput))
        {
            Console.WriteLine("File does not exist, exiting.");
            return;
        }
        else if (Path.GetExtension(pathInput) != EXPECTED_INPUT_FILE_EXTENSION)
        {
            Console.WriteLine($"File extension does not match, expected '{EXPECTED_INPUT_FILE_EXTENSION}', exiting.");
            return;
        }

        var errorLogs = new List<string>();
        var flatfileDeserializer = new FlatfileDeserializer('|', errorLogs);
        var persons = flatfileDeserializer.DeserializeFlatfile(pathInput);

        var memStream = SerializeToXML(GetOutputPath(), persons);

        try
        {
            System.Console.WriteLine($"Saving XML Serialization to {GetOutputPath()}");
            await SaveToFileAsync(memStream, GetOutputPath());
            System.Console.WriteLine("File saved.");

            if (errorLogs.Count == 0)
            {
                System.Console.WriteLine($"Successfully deserialized inputfile, see file '{OUTPUT_FILE_NAME}'.");
            }
            else
            {  
                System.Console.WriteLine($"Completed deserialization with {errorLogs.Count} errors.");
                System.Console.WriteLine("To see these errors, enter 1 else press any key to continue (c) The Simpsons.");
                var input = Console.ReadLine();
                if (input == "1")
                {
                    foreach (var error in errorLogs)
                        System.Console.WriteLine(error);

                    System.Console.WriteLine("Done, press any key to continue (c) The Simpsons.");
                    Console.ReadLine();
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Could not save output file, see error message: {ex.Message}");
        }
    }

    static string GetOutputPath()
    {
        return Directory.GetCurrentDirectory() + "\\" + OUTPUT_FILE_NAME;
    }

    static async Task SaveToFileAsync(MemoryStream memStream, string outputPath)
    {
        using var fileStream = new FileStream(outputPath, FileMode.Create);
        memStream.WriteTo(fileStream);
        await memStream.DisposeAsync();
    }

    static MemoryStream SerializeToXML(string filepathOutput, List<Person> persons)
    {
        var serializer = new XmlSerializer(typeof(List<Person>), 
            new XmlRootAttribute("Persons"));

        var xmlNamespaces = new XmlSerializerNamespaces(new [] { XmlQualifiedName.Empty});

        var stream = new MemoryStream();
        serializer.Serialize(stream, persons, xmlNamespaces);

        return stream;
    }
}