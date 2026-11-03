using DeliveryPlatform.Domain.src.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Infrastructure.src.Configuration;

internal class OrderConfiguraion : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(o => o.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(o => o.DeliveryFee)
            .HasPrecision(18, 2);

        builder.Property(o => o.Total)
            .HasPrecision(18, 2);

        builder.Property(o => o.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Business)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Branch)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => new { o.BusinessId, o.CreatedAt });
        builder.HasIndex(o => new { o.BranchId, o.CreatedAt });
    }
}
