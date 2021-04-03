using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Middlewares.Implementations.Middlewares
{
	public class ErrorHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IWebHostEnvironment _environment;

		#region Constructor

		public ErrorHandlerMiddleware(RequestDelegate next, IWebHostEnvironment environment)
		{
			_next = next;
			_environment = environment;
		}

		#endregion Constructor

		#region Methods

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception exception)
			{
				await HandlingExceptionAsync(context, exception);
			}
		}

		private async Task HandlingExceptionAsync(HttpContext context, Exception exception)
		{
			var error = exception switch
			{
				KeyNotFoundException _ => GetApiError((short)HttpStatusCode.NotFound, "The requested resource was not found"),
				ArgumentException _ => GetApiError((short)HttpStatusCode.BadRequest, "The submitted arguments are not valid"),
				NullReferenceException _ => GetApiError((short)HttpStatusCode.InternalServerError, "An internal application failure has occurred"),
				_ => GetApiError((short)HttpStatusCode.InternalServerError, "An unexpected application failure has occurred")
			};

			if (_environment.IsDevelopment())
				error.Detail = exception.StackTrace;

			Log.Error(exception, "ID: {Id}", error.Id);

			var response = context.Response;
			response.ContentType = "application/json";
			response.StatusCode = error.Status;

			var result = JsonSerializer.Serialize(error);

			await response.WriteAsync(result);
		}

		private static Error GetApiError(short statusCode, string message)
		{
			return new Error
			{
				Id = Guid.NewGuid(),
				Status = statusCode,
				Title = $"{message}, Please report the error Id to the administrators!"
			};
		}

		#endregion Methods
	}

	public record Error
	{
		#region Properties

		public Guid Id { get; init; }
		public short Status { get; init; }
		public string Title { get; init; }
		public string Detail { get; set; }

		#endregion Properties
	}
}
