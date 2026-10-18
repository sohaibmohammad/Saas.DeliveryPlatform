using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Infrastructure.src.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Infrastructure.src.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
    return await _context.SaveChangesAsync(cancellationToken);
    }
}
