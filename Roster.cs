public class Roster
{
    public Dictionary<Duty, Person> DutyAssignments { get; set; }

    public Roster(Dictionary<Duty, Person> dutyAssignments)
    {
        DutyAssignments = dutyAssignments;
    }
}