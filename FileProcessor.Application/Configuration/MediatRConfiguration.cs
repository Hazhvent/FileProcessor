using FileProcessor.Application.Configuration;
using FileProcessor.Application.Pipeline.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace FileProcessor.Application.Configuration
{
    public static class MediatRConfiguration
    {
        public static TBuilder AddMediatR<TBuilder>(this TBuilder builder)
               where TBuilder : IHostApplicationBuilder
        {
            builder.Services.AddMediatR(cfg =>
            {
                // Registro de todas las clases que hereden de IRequestHandler<TRequest, TResponse> (Manejadores)
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                AddBehaviors(cfg); // 👈 Behaviors
            });

            // Registro de todas las clases que hereden de AbstractValidator<T> (Validadores)
            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return builder;
        }

        private static void AddBehaviors(MediatRServiceConfiguration cfg)
        {
            cfg.AddOpenBehaviors([
                typeof(ValidationBehavior<,>)
            ]);
        }

    }
}
