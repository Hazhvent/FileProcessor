using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Repositories;
using MediatR;

namespace FileProcessor.Application.Actions
{
    public sealed record GetApprovalRuleAndReviewersActionRequest(
        double Capital,
        int MonedaId,
        string? PolizaNumero = null,
        int? CertificadoId = null,
        bool EsConsultaNormal = false
    ) : IRequest<GetApprovalRuleAndReviewersActionResponse>;

    public sealed record GetApprovalRuleAndReviewersActionResponse(
        ReglaAprobacion ReglaAprobacion,
        List<Correos> CorreosRevisores
    );

    public class GetApprovalRuleAndReviewersActionHandler(IRepository repository)
        : IRequestHandler<GetApprovalRuleAndReviewersActionRequest, GetApprovalRuleAndReviewersActionResponse>
    {
        public async Task<GetApprovalRuleAndReviewersActionResponse> Handle(
            GetApprovalRuleAndReviewersActionRequest request,
            CancellationToken cancellationToken)
        {
            var reglaAprobacion = await repository.GetReglaAsync(request.Capital, request.MonedaId)
                ?? throw new Exceptions.NotFoundException(string.Format(ErrorConstants.REQUEST_APPROVAL_RULE_NOT_FOUND, request.Capital, request.MonedaId));

            List<Correos> correosRevisores;

            if (request.EsConsultaNormal && !string.IsNullOrWhiteSpace(request.PolizaNumero) && request.CertificadoId.HasValue)
            {
                var correosResult = await repository.GetCorreosRevisorGenNormalAsync(request.Capital, request.PolizaNumero, request.CertificadoId.Value);
                correosRevisores = correosResult?.ToList()
                    ?? throw new Exceptions.BadRequestException(string.Format(ErrorConstants.REQUEST_REVIEWERS_NOT_FOUND, request.Capital, request.MonedaId));
            }
            else
            {
                var correosResult = await repository.GetCorreosRevisorAsync(request.Capital, request.MonedaId);
                correosRevisores = correosResult?.ToList()
                    ?? throw new Exceptions.BadRequestException(string.Format(ErrorConstants.REQUEST_REVIEWERS_NOT_FOUND, request.Capital, request.MonedaId));
            }

            return new GetApprovalRuleAndReviewersActionResponse(reglaAprobacion, correosRevisores);
        }
    }
}