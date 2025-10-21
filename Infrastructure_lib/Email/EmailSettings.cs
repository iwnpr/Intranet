namespace Infrastructure_lib.Email;

public class EmailSettings
{
    public string? Host { get; set; }

    public int Port { get; set; } = 25;

    public bool EnableSsl { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? From { get; set; }

    public string? OrganizationRequestRecipient { get; set; }
}
