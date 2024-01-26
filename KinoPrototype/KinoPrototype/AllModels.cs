using System.ComponentModel.DataAnnotations;

namespace KinoPrototype;

public class User
{
    [Key] public int Id { get; set; }
    
    public string Nickname { get; set; }
}


public class JoinEvent
{
    
}