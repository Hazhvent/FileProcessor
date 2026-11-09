using FileProcessor.Core.Constants.Api;
using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Dto;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Repositories;
using FileProcessor.Core.Utils;
using MediatR;

namespace FileProcessor.Application.Actions
{
    public sealed record RegisterPolicyActionRequest(
        long PolizaId,
        int CertificadoId,
        int RamoId,
        int ProductoId,
        double Capital,
        double Prima,
        int TipoDocId,
        int IdDocumento,
        int ReglaAprobacionId,
        int MonedaId,
        int Firma,
        string LoteGenerado,
        string GeneraSaludo,
        string NombreContratante,
        string NombreAsegurado,
        string DocumentoAsegurado,
        string DireccionAsegurado,
        string SexoAsegurado,
        string CorreoUsuario,
        string DocumentoContratante,
        string MonedaSimbolo,
        DateTime FechaInicioVigencia,
        DateTime FechaFinVigencia,
        DateTime FechaEmision,
        DateTime? FechaNacimientoAsegurado
    ) : IRequest<RegisterPolicyActionResponse>;

    public sealed record RegisterPolicyActionResponse(Polizas PolizaRegistrada);

    public class RegisterPolicyActionHandler(IRepository repository)
        : IRequestHandler<RegisterPolicyActionRequest, RegisterPolicyActionResponse>
    {
        public async Task<RegisterPolicyActionResponse> Handle(RegisterPolicyActionRequest request, CancellationToken cancellationToken)
        {
            Polizas polizaUniversal = await repository.SetPolizaAsync(
                request.PolizaId,
                request.Firma,
                request.RamoId,
                request.Capital,
                request.ProductoId,
                request.FechaInicioVigencia,
                request.NombreContratante,
                request.NombreAsegurado,
                request.LoteGenerado,
                request.TipoDocId,
                request.ReglaAprobacionId,
                request.IdDocumento,
                request.CertificadoId,
                request.GeneraSaludo
            ) ?? throw new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_POLICY_REGISTRATION_FAILED, request.PolizaId));

            string fechaNacimientoFormat = request.FechaNacimientoAsegurado.HasValue && request.FechaNacimientoAsegurado.Value != default
                ? request.FechaNacimientoAsegurado.Value.ToString(SettingsConstants.DEFAULT_DATE_FORMAT)
                : ValidationConstants.LEGACY_EMPTY_PLACEHOLDER;

            bool updateExitoso = await repository.UpdateAsync(
                Helper.SanitizeText(request.DocumentoContratante),
                (int)request.Prima,
                Helper.SanitizeDate(request.FechaInicioVigencia.ToString(SettingsConstants.DEFAULT_DATE_FORMAT))!,
                Helper.SanitizeDate(request.FechaFinVigencia.ToString(SettingsConstants.DEFAULT_DATE_FORMAT))!,
                Helper.SanitizeText(request.MonedaSimbolo),
                Helper.SanitizeText(request.DocumentoAsegurado),
                Helper.SanitizeText(request.DireccionAsegurado),
                Helper.SanitizeText(request.NombreAsegurado),
                Helper.SanitizeText(request.SexoAsegurado),
                Helper.SanitizeText(request.CorreoUsuario),
                ValidationConstants.LEGACY_EMPTY_PLACEHOLDER,
                Helper.SanitizeDate(fechaNacimientoFormat)!,
                Helper.SanitizeText(request.DireccionAsegurado),
                (int)request.Prima,
                Helper.SanitizeDate(request.FechaEmision.ToString(SettingsConstants.DEFAULT_DATE_FORMAT))!,
                Helper.SanitizeDate(request.FechaFinVigencia.ToString(SettingsConstants.DEFAULT_DATE_FORMAT))!,
                request.TipoDocId,
                polizaUniversal.IDTBLGENERICA,
                request.MonedaId
            );

            if (!updateExitoso)
            {
                throw new Exceptions.InternalServerErrorException(string.Format(ErrorConstants.INTERNAL_CORE_UPDATE_FAILED, request.PolizaId));
            }

            return new RegisterPolicyActionResponse(polizaUniversal);
        }
    }
}