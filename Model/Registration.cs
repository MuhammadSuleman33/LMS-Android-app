namespace lms.android.Model;

public class Registration
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public required string Fullname { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Password { get; set; }
}