public class Service
{
    public string Name { get; set; }
    public List<ServiceDuty> ServiceDuties { get; set; }
    public DateTime ServiceTime { get; set; } // Renamed field
    public Roster Roster { get; private set; }

    private static Dictionary<string, Queue<Person>> rotationQueues = new Dictionary<string, Queue<Person>>();

    public Service(string name, List<ServiceDuty> serviceDuties, DateTime serviceTime)
    {
        Name = name;
        ServiceDuties = serviceDuties;
        ServiceTime = serviceTime;
        Roster = new Roster(new Dictionary<Duty, Person>());
    }

    public void CreateRoster(List<Person> people, Dictionary<Duty, Person> weeklyAssignments)
    {
        var dutyAssignments = new Dictionary<Duty, Person>();
        var assignedPersons = new HashSet<Person>();
        var random = new Random();

        foreach (var serviceDuty in ServiceDuties.OrderBy(sd => sd.Duty.Importance))
        {
            var duty = serviceDuty.Duty;

            // Check if the duty is weekly and already assigned
            if (duty.Weekly && weeklyAssignments.ContainsKey(duty))
            {
                dutyAssignments[duty] = weeklyAssignments[duty];
                assignedPersons.Add(weeklyAssignments[duty]);
            }
            else if (duty.Rotation)
            {
                if (!rotationQueues.ContainsKey(duty.Name))
                {
                    rotationQueues[duty.Name] = new Queue<Person>(people.Where(p => p.Duties.Contains(duty)));
                }

                var rotationQueue = rotationQueues[duty.Name];
                Person assignedPerson = null;

                while (rotationQueue.Count > 0)
                {
                    var person = rotationQueue.Dequeue();
                    if (!assignedPersons.Contains(person) && CanPerformDuty(person, ServiceTime))
                    {
                        assignedPerson = person;
                        break;
                    }
                    rotationQueue.Enqueue(person);
                }

                if (assignedPerson != null)
                {
                    dutyAssignments[duty] = assignedPerson;
                    assignedPersons.Add(assignedPerson);
                    rotationQueue.Enqueue(assignedPerson);

                    // If the duty is weekly, save the assignment
                    if (duty.Weekly)
                    {
                        weeklyAssignments[duty] = assignedPerson;
                    }
                }
                else
                {
                    dutyAssignments[duty] = null;
                }
            }
            else
            {
                var availablePersons = people.Where(person => !assignedPersons.Contains(person) && person.Duties.Any(d => d.Name == duty.Name) && CanPerformDuty(person, ServiceTime)).ToList();
                if (availablePersons.Any())
                {
                    var assignedPerson = availablePersons[random.Next(availablePersons.Count)];
                    dutyAssignments[duty] = assignedPerson;
                    assignedPersons.Add(assignedPerson);

                    // If the duty is weekly, save the assignment
                    if (duty.Weekly)
                    {
                        weeklyAssignments[duty] = assignedPerson;
                    }
                }
                else
                {
                    dutyAssignments[duty] = null;
                }
            }
        }
        Roster = new Roster(dutyAssignments);
    }

    private bool CanPerformDuty(Person person, DateTime serviceTime)
    {
        if (serviceTime.DayOfWeek == DayOfWeek.Sunday && serviceTime.Hour < 12)
        {
            return person.SundayAM;
        }
        else if (serviceTime.DayOfWeek == DayOfWeek.Sunday && serviceTime.Hour >= 12)
        {
            return person.SundayPM;
        }
        else if (serviceTime.DayOfWeek == DayOfWeek.Wednesday)
        {
            return person.WednesdayPM;
        }
        return true;
    }
}