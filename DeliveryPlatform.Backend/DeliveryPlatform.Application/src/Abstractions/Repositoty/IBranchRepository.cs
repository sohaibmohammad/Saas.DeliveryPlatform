using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Abstractions.Repositoty;

public interface IBranchRepository : IGenericRepository<Branch>
{
    Task<List<Branch>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken);
    Task<Branch?> GetByBusinessIdAndBranchIdAsync(Guid businessId, Guid branchId, CancellationToken cancellationToken);
}
