using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Infrastructure.src.Database;
using Microsoft.EntityFrameworkCore;
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



    public async Task ExecuteInTransactionAsync(
    Func<CancellationToken, Task> operation,
    CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await operation(cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                Console.WriteLine($"Entity: {entry.Metadata.Name}");
                Console.WriteLine($"State: {entry.State}");

                foreach (var property in entry.Properties)
                {
                    Console.WriteLine(
                        $"{property.Metadata.Name}: " +
                        $"Original={property.OriginalValue}, " +
                        $"Current={property.CurrentValue}");
                }
            }

            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    } 


    }
