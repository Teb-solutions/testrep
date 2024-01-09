using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EasyGas.Services.Core.Commands
{
    public class CommandResult : ObjectResult
    {

        public static CommandResult Ok = new CommandResult(HttpStatusCode.NoContent);

        public bool IsOk { get; }
        public Exception Error { get; }

        public string Message { get; }
        public object Content { get; }

        public CommandResult(HttpStatusCode code) : this(HttpStatusCode.OK, (object)null)
        {

        }


        public CommandResult(HttpStatusCode code, object content) : base(content)
        {
            IsOk = (int)code < (int)HttpStatusCode.BadRequest;
            StatusCode = (int)code;
            Content = content;
            Message = null;

        }

        public CommandResult(HttpStatusCode code, string message) : this(code, (object)message)
        {
            Content = null;
            Message = message;
        }

        public CommandResult(Exception exception, HttpStatusCode statusCode) : base(exception.Message)
        {
            IsOk = false;
            Error = exception;
            Message = exception.Message;
            StatusCode = (int)statusCode;
            Content = null;
        }

        public static CommandResult NonExistentCommand(string command)
        {
            return new CommandResult(HttpStatusCode.InternalServerError,
                $"Invalid command type {command}");
        }

        public static CommandResult FromValidationErrors(IEnumerable<string> errors)
        {
            var problemDetails = new ValidationProblemDetails()
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Some error has occured. Please try again."
            };

            problemDetails.Errors.Add("Validations", errors.ToArray());

            if (errors.Count() > 0)
            {
                problemDetails.Detail = errors.First();
            }
            return new CommandResult(HttpStatusCode.BadRequest, problemDetails);
        }

        public static CommandResult FromValidationErrors(string errors)
        {
            var problemDetails = new ValidationProblemDetails()
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = errors
            };

            return new CommandResult(HttpStatusCode.BadRequest, problemDetails);
        }


        public static CommandResult FromValidationErrors()
        {
            var problemDetails = new ValidationProblemDetails()
            {
                Status = StatusCodes.Status400BadRequest,
                Detail = "Some error has occured. Please try again."
            };

            return new CommandResult(HttpStatusCode.BadRequest, problemDetails);
        }

    }
}