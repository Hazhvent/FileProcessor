using FileProcessor.Core.Exceptions;
using FluentValidation;
using MediatR;

namespace FileProcessor.Application.Pipeline.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Si hay validadores registrados para el tipo de solicitud actual (TRequest),
        // se ejecutan todos de forma asíncrona con el contexto actual.
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            // Si existen errores, se agrupan como ApiSubErrors y se lanza una excepción con código 422.
            if (failures.Count != 0)
            {
                var subErrors = failures.Select(error => new ApiSubError(error.PropertyName, error.ErrorMessage)).ToList();
                throw new Exceptions.UnprocessableEntityException(subErrors);
            }
        }

        return await next(cancellationToken);
    }
}
