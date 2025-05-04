public class Template
{
    public string Name { get; set; }
    public List<TemplateDuty> TemplateDuties { get; set; }

    public Template(string name, List<TemplateDuty> templateDuties)
    {
        Name = name;
        TemplateDuties = templateDuties;
    }
}