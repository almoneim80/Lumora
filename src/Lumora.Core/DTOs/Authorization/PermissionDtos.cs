namespace Lumora.DTOs.Authorization;

public class AddPermissionDto
{
    public string RoleName { get; set; } = null!;
    public string Permission { get; set; } = null!;
}

public class AddMultiplePermissionsDto
{
    public string RoleName { get; set; } = null!;
    public List<string> Permissions { get; set; } = new();
}

public class RemovePermissionDto
{
    public string RoleName { get; set; } = null!;
    public string Permission { get; set; } = null!;
}
