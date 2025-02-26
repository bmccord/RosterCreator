// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Text.Json;
using System.IO;

// Load data from JSON file
var jsonData = File.ReadAllText("data.json");
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

Data data;
try
{
    data = JsonSerializer.Deserialize<Data>(jsonData, options) ?? throw new InvalidOperationException("Deserialized data is null");
}
catch (JsonException ex)
{
    Console.WriteLine($"Error deserializing JSON data: {ex.Message}");
    return;
}

// Check if any properties in data are null
if (data.Duties == null || data.Services == null || data.People == null)
{
    Console.WriteLine("Error: One or more properties in the deserialized data are null.");
    return;
}

var duties = data.Duties.Select(d => new Duty(d.Name, d.Importance, d.Weekly, d.Rotation)).ToList();

List<Service> services = data.Services.Select(s => new Service(
    s.Name,
    s.Duties.Select(sd => new ServiceDuty(duties.First(d => d.Name == sd.Name), sd.PrintOrder)).ToList(),
    DateTime.Parse(s.ServiceTime)
)).ToList();

List<Person> people = data.People.Select(p => new Person(
    p.Name,
    p.Duties.Select(d => duties.First(duty => duty.Name == d)).ToList(),
    p.SundayAM,
    p.SundayPM,
    p.WednesdayPM
)).ToList();


// Create roster for all services
var weeklyAssignments = new Dictionary<string, Dictionary<Duty, Person>>();
using (StreamWriter writer = new StreamWriter("output.txt"))
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
        Console.WriteLine(serviceInfo);
        foreach (var assignment in service.Roster.DutyAssignments.OrderBy(a => service.ServiceDuties.First(sd => sd.Duty == a.Key).PrintOrder))
        {
            var personName = assignment.Value != null ? assignment.Value.Name : "No person assigned";
            string dutyInfo = $"- {assignment.Key.Name}: {personName}";
            writer.WriteLine(dutyInfo);
            Console.WriteLine(dutyInfo);
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

