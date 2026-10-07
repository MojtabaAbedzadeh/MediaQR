namespace MediaQR.Web.Models.Users;

public class UserListViewModel
{
    public List<UserItemViewModel> Users { get; set; } = new();
}

public class UserItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool IsLockedOut { get; set; }

    public string[] Roles { get; set; } = Array.Empty<string>();
}