using FileProcessor.Application.UseCases;
using FileProcessor.Core.Constants.Api;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MinimalApis.Discovery;

namespace FileProcessor.Application.Endpoints
{
    public class AdminHandler : BaseApiHandler, IApi
    {
        public void Register(IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup(EndpointsRoutes.ADMIN_GROUP_PREFIX)
                               .WithTags(EndpointsRoutes.ADMIN_TAG);

            group.MapPost(EndpointsRoutes.PROCESS_DOCUMENT_BATCH, ProcessDocumentBatch);
        }

        private static Task<IResult> ProcessDocumentBatch(
            ISender sender,
            ProcessDocumentBatchRequest request,
            CancellationToken cancellationToken)
        => SendRequest<ProcessDocumentBatchRequest, ProcessDocumentBatchResponse>(sender, request, cancellationToken);
    }
}