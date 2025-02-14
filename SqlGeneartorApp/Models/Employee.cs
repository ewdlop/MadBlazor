namespace SqlGeneratorApp.Models;

// Example domain class
public class Employee
{
    public int EmployeeId { get; set; }
    public int? ManagerId { get; set; }
    public string Name { get; set; }
}