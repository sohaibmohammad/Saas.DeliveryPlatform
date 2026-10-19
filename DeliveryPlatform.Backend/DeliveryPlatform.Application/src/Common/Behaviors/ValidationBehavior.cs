using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var errors = _validators.SelectMany(validator=>
        validator.Validate(context).Errors
        ).Where(f => f != null).ToList();

        if (errors.Count!=0)
        {
            throw new ValidationException(errors);
        }
        
        return next();
    }
}
