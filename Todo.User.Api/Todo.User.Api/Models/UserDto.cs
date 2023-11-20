namespace Todo.User.Api.Models
{
    public class UserDto
    {
        required public string Login { get; set; }
        required public string Password { get; set; }
    }
}
