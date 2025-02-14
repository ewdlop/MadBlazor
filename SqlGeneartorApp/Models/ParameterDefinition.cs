namespace SqlGeneratorApp.Models;

public class ParameterDefinition
{
    public string Name { get; set; }
    public string DataType { get; set; }
    public bool IsOutput { get; set; }
}
