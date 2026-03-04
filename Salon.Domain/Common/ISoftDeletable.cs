namespace Salon.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    int? DeletedBy { get; }
    void SoftDelete(int userId);
}
