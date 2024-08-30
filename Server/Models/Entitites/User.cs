namespace API.Models.Entitites;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
}