using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Middlewares.Implementations.Middlewares;

namespace Middlewares.Implementations
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllers();
			services.AddSwaggerGen(configuration =>
			{
				configuration.SwaggerDoc("v1", new OpenApiInfo { Title = "Middlewares Implementations", Version = "v1" });
			});
		}

		public void Configure(IApplicationBuilder application, IWebHostEnvironment environment)
		{
			if (environment.IsDevelopment())
			{
				application.UseDeveloperExceptionPage();
				application.UseSwagger();
				application.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Middlewares.Implementations v1"));
			}

			application.UseMiddleware<ErrorHandlerMiddleware>(environment);

			application.UseHttpsRedirection();
			application.UseRouting();
			application.UseAuthorization();			

			application.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
