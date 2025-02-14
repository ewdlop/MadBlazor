namespace SqlGeneratorApp.Models;

public enum SqlStatementType
{
    Select = 1,
    Insert = 2,
    Update = 3,
    Delete = 4,
    Create = 5,
    Alter = 6,
    Drop = 7,
    Merge = 8,
    StoredProcedure = 9,
    Function = 10,
    View = 11
}
