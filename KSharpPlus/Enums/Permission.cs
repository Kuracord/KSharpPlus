namespace KSharpPlus.Enums;

public static class PermissionMethods {
    internal static Permissions FullPerms => (Permissions)15;

    /// <summary>
    /// Calculates whether this permission set contains the given permission.
    /// </summary>
    /// <param name="p">The permissions to calculate from</param>
    /// <param name="permission">permission you want to check</param>
    /// <returns>True if the permission contains the given permission, otherwise false.</returns>
    public static bool HasPermission(this Permissions p, Permissions permission) => 
        p.HasFlag(Permissions.Administrator) || (p & permission) == permission;

    /// <summary>
    /// Grants permissions.
    /// </summary>
    /// <param name="p">The permissions to add to.</param>
    /// <param name="grant">Permission to add.</param>
    /// <returns>Permissions with the granted permissions.</returns>
    public static Permissions Grant(this Permissions p, Permissions grant) => p | grant;

    /// <summary>
    /// Revokes permissions.
    /// </summary>
    /// <param name="p">The permissions to take from.</param>
    /// <param name="revoke">Permission to take.</param>
    /// <returns>Permissions without the revoked permissions.</returns>
    public static Permissions Revoke(this Permissions p, Permissions revoke) => p & ~revoke;
}

/// <summary>
/// Whether a permission is allowed, denied or unset
/// </summary>
public enum PermissionLevel {
    /// <summary>
    /// Said permission is Allowed
    /// </summary>
    Allowed,

    /// <summary>
    /// Said permission is Denied
    /// </summary>
    Denied,

    /// <summary>
    /// Said permission is Unset
    /// </summary>
    Unset
}

/// <summary>
/// Defines a readable name for this permission.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class PermissionStringAttribute : Attribute {
    /// <summary>
    /// Gets the readable name for this permission.
    /// </summary>
    public string String { get; }

    /// <summary>
    /// Defines a readable name for this permission.
    /// </summary>
    /// <param name="str">Readable name for this permission.</param>
    public PermissionStringAttribute(string str) => String = str;
}

/// <summary>
/// Bitwise permission flags.
/// </summary>
[Flags]
public enum Permissions : long {
    /// <summary>
    /// Indicates no permissions given.
    /// </summary>
    [PermissionString("No permissions")]
    None = 0,
    
    /// <summary>
    /// Allows accessing text and voice channels. Disabling this permission hides channels.
    /// </summary>
    [PermissionString("Read messages")]
    ViewChannels = 1 << 0,
    
    /// <summary>
    /// Allows sending messages.
    /// </summary>
    [PermissionString("Send messages")]
    SendMessages = 1 << 1,
    
    /// <summary>
    /// Allows kicking members.
    /// </summary>
    [PermissionString("Kick members")]
    KickMembers = 1 << 2,
    
    /// <summary>
    /// Allows banning and unbanning members.
    /// </summary>
    [PermissionString("Ban members")]
    BanMembers = 1 << 3,
    
    /// <summary>
    /// Enables full access on a given guild. This also overrides other permissions.
    /// </summary>
    [PermissionString("Administrator")]
    Administrator = 1 << 4,
    
    /// <summary>
    /// ???
    /// </summary>
    [PermissionString("???")]
    LocalAdministrator = 1 << 5
}