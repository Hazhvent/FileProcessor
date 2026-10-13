namespace FileProcessor.Core.Exceptions
{
    public class Exceptions
    {
        public class BadRequestException : ApiException
        {
            public BadRequestException() : base(400) { }
            public BadRequestException(string message) : base(400, detail: message) { }
        }

        public class UnauthorizedException : ApiException
        {
            public UnauthorizedException() : base(401) { }
            public UnauthorizedException(string message) : base(401, detail: message) { }
        }

        public class ForbiddenException : ApiException
        {
            public ForbiddenException() : base(403) { }
            public ForbiddenException(string message) : base(403, detail: message) { }
        }

        public class NotFoundException : ApiException
        {
            public NotFoundException() : base(404) { }
            public NotFoundException(string message) : base(404, detail: message) { }
        }

        public class MethodNotAllowedException : ApiException
        {
            public MethodNotAllowedException() : base(405) { }
        }

        public class ConflictException : ApiException
        {
            public ConflictException() : base(409) { }
            public ConflictException(string message) : base(409, detail: message) { }
        }

        public class PayloadTooLargeException : ApiException
        {
            public PayloadTooLargeException() : base(413) { }
        }

        public class UnsupportedMediaTypeException : ApiException
        {
            public UnsupportedMediaTypeException() : base(415) { }
        }

        public class UnprocessableEntityException : ApiException
        {
            public UnprocessableEntityException(List<ApiSubError> subErrors) : base(422, subErrors: subErrors) { }
        }

        public class TooManyRequestsException : ApiException
        {
            public TooManyRequestsException() : base(429) { }
        }

        public class InternalServerErrorException : ApiException
        {
            public InternalServerErrorException() : base(500) { }
            public InternalServerErrorException(string message) : base(500, detail: message) { }
        }

        public class ServiceUnavailableException : ApiException
        {
            public ServiceUnavailableException() : base(503) { }
            public ServiceUnavailableException(string message) : base(503, detail: message) { }
        }
    }
}
