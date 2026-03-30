namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Services for entities that belong to an user should implement this interface.
/// This is used together with the authorize attribute to check for owner permission.
/// </summary>
public interface IResourceOwnerService
{
    /// <summary>
    /// Get the entity's ownner User Id.
    /// </summary>
    /// <param name="resourceId">Entity's ID</param>
    Task<Guid> GetOwnerIdAsync(Guid resourceId);
}