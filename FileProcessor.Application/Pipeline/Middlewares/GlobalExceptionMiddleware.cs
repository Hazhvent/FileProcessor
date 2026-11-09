using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Exceptions;
using FileProcessor.Core.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using System.Data.Common;
using System.Net.Sockets;

namespace FileProcessor.Application.Pipeline.Middlewares
{
    public class GlobalExceptionMiddleware(RequestDelegate next)
    {
        public async Task Invoke(
            HttpContext context,
            IHttpErrorFormatter errorResponseWriter,
            ILoggingService<GlobalExceptionMiddleware> logger)
        {
            try
            {
                await next(context);
            }
            catch (ApiException apiEx)
            {
                logger.LogError(apiEx, LogsConstants.ERROR_FILE_PROCESSING);
                await errorResponseWriter.WriteErrorResponseAsync(context, apiEx);
            }
            catch (BadHttpRequestException syntaxError)
            {
                logger.LogError(syntaxError, LogsConstants.ERROR_FILE_PROCESSING);
                var apiEx = new Exceptions.BadRequestException(syntaxError.Message);
                await errorResponseWriter.WriteErrorResponseAsync(context, apiEx);
            }
            catch (DbException dbError)
            {
                logger.LogError(dbError, LogsConstants.ERROR_FILE_PROCESSING);
                var apiEx = new Exceptions.ServiceUnavailableException(dbError.Message);
                await errorResponseWriter.WriteErrorResponseAsync(context, apiEx);
            }
            catch (SocketException socketEx)
            {
                logger.LogError(socketEx, LogsConstants.ERROR_FILE_PROCESSING);
                var apiEx = new Exceptions.ServiceUnavailableException(socketEx.Message);
                await errorResponseWriter.WriteErrorResponseAsync(context, apiEx);
            }
            catch (AuthenticationException authEx)
            {
                logger.LogError(authEx, LogsConstants.ERROR_FILE_PROCESSING);
                var apiEx = new Exceptions.BadRequestException(authEx.Message);
                await errorResponseWriter.WriteErrorResponseAsync(context, apiEx);
            }
            catch (SmtpCommandException smtpEx)
            {
                logger.LogError(smtpEx, LogsConstants.ERROR_FILE_PROCESSING);
                var apiEx = new Exceptions.ServiceUnavailableException(smtpEx.Message);
                await errorResponseWriter.WriteErrorResponseAsync(context, apiEx);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, LogsConstants.ERROR_FILE_PROCESSING);
                var apiEx = new Exceptions.InternalServerErrorException(ex.Message);
                await errorResponseWriter.WriteErrorResponseAsync(context, apiEx);
            }
        }
    }
}