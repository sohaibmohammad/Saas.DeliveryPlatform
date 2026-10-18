using DeliveryPlatform.Application.src.Abstractions;
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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
    return _context.SaveChangesAsync(cancellationToken);
    }
}
