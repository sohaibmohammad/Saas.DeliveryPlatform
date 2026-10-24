using DeliveryPlatform.Domain.src.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Business.Create;

public record CreateBusinessRequest(
    string Name,
    string? Description,
    BusinessType BusinessType
);
