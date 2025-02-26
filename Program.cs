// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Text.Json;
using System.IO;
using Fclp;

// Main method to run the program
public static class Program
{
    public static void Main(string[] args)
    {
        var p = new FluentCommandLineParser<ApplicationArguments>();

        // specify which property the value will be assigned too.
        p.Setup(arg => arg.InputFilePath)
         .As('i', "input") // define the short and long option name
         .SetDefault("data.json")
         .WithDescription("Path to the input data file. (Default: data.json)");

        p.Setup(arg => arg.OutputFilePath)
         .As('o', "output")
         .SetDefault("output.txt")
         .WithDescription("Path to the output file. (Default: output.txt)");

        p.Setup(arg => arg.Silent)
         .As('s', "silent")
         .SetDefault(false)
         .WithDescription("Do not output roster to console.");

        p.SetupHelp("?", "help")
         .Callback(text => Console.WriteLine(text));

        var result = p.Parse(args);

        if (result.HasErrors == false && !result.HelpCalled)
        {
            try
            {
                var rosterCreator = new RosterCreator(p.Object.InputFilePath);
                rosterCreator.CreateRoster(p.Object.OutputFilePath, p.Object.Silent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

public class RosterCreator
{
    private Data data;
    private List<Duty> duties;
    private List<Service> services;
    private List<Person> people;

    public RosterCreator(string jsonFilePath)
    {
        var jsonData = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            data = JsonSerializer.Deserialize<Data>(jsonData, options) ?? throw new InvalidOperationException("Deserialized data is null");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON data: {ex.Message}");
            throw;
        }

        if (data.Duties == null || data.Services == null || data.People == null)
        {
            Console.WriteLine("Error: One or more properties in the deserialized data are null.");
            throw new InvalidOperationException("Invalid data");
        }

        duties = data.Duties.Select(d => new Duty(d.Name, d.Importance, d.Weekly, d.Rotation)).ToList();
        services = data.Services.Select(s => new Service(
            s.Name,
            s.Duties.Select(sd => new ServiceDuty(duties.First(d => d.Name == sd.Name), sd.PrintOrder)).ToList(),
            DateTime.Parse(s.ServiceTime)
        )).ToList();
        people = data.People.Select(p => new Person(
            p.Name,
            p.Duties.Select(d => duties.First(duty => duty.Name == d)).ToList(),
            p.SundayAM,
            p.SundayPM,
            p.WednesdayPM
        )).ToList();
    }

    public void CreateRoster(string outputFilePath, bool silent = false)
    {
        var weeklyAssignments = new Dictionary<string, Dictionary<Duty, Person>>();
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            foreach (var service in services)
            {
                var weekKey = $"{service.ServiceTime.Year}-W{CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(service.ServiceTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)}";
                if (!weeklyAssignments.ContainsKey(weekKey))
                {
                    weeklyAssignments[weekKey] = new Dictionary<Duty, Person>();
                }
                service.CreateRoster(people, weeklyAssignments[weekKey]);
                string serviceInfo = $"Roster for service: {service.Name} on {service.ServiceTime:MMMM dd, yyyy 'at' hh:mm tt}";
                writer.WriteLine(serviceInfo);
                if (!silent)
                {
                    Console.WriteLine(serviceInfo);
                }
                foreach (var assignment in service.Roster.DutyAssignments.OrderBy(a => service.ServiceDuties.First(sd => sd.Duty == a.Key).PrintOrder))
                {
                    var personName = assignment.Value != null ? assignment.Value.Name : "No person assigned";
                    string dutyInfo = $"- {assignment.Key.Name}: {personName}";
                    writer.WriteLine(dutyInfo);
                    if (!silent)
                    {
                        Console.WriteLine(dutyInfo);
                    }
                }
            }
        }
    }
}

public class Data
{
    public List<DutyData> Duties { get; set; }
    public List<ServiceData> Services { get; set; }
    public List<PersonData> People { get; set; }
}

public class DutyData
{
    public string Name { get; set; }
    public int Importance { get; set; }
    public bool Weekly { get; set; }
    public bool Rotation { get; set; }
    // Removed MayReuse property
}

public class ServiceData
{
    public string Name { get; set; }
    public List<ServiceDutyData> Duties { get; set; }
    public string ServiceTime { get; set; }
}

public class ServiceDutyData
{
    public string Name { get; set; }
    public int PrintOrder { get; set; }
}

public class PersonData
{
    public string Name { get; set; }
    public List<string> Duties { get; set; }
    public bool SundayAM { get; set; } = true;
    public bool SundayPM { get; set; } = true;
    public bool WednesdayPM { get; set; } = true;
}

public class ApplicationArguments
{
    public string InputFilePath { get; set; }
    public string OutputFilePath { get; set; }
    public bool Silent { get; set; }
}

