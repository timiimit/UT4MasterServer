using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UT4MasterServer;

public class SwaggerAuthorizationHeaderOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (operation.Parameters == null)
			operation.Parameters = new List<OpenApiParameter>();

		//var authorizeAttributes = context.ApiDescription
		//	.CustomAttributes()
		//	.Union(context.ApiDescription.CustomAttributes().OfType<BasicAuthenticationAttribute>())
		//	.Union(context.ApiDescription.CustomAttributes().OfType<BearerAuthenticationAttribute>());
		var authorizeAttributes = context.ApiDescription
			.CustomAttributes()
			.OfType<AuthorizeAttribute>();
		var allowAnonymousAttributes = context.ApiDescription.CustomAttributes().OfType<AllowAnonymousAttribute>();

		if (!authorizeAttributes.Any() && !allowAnonymousAttributes.Any())
		{
			return;
		}

		var parameter = new OpenApiParameter()
		{
			Name = "Authorization",
			In = ParameterLocation.Header,
			Description = "The bearer token",
			Required = true,
		};

		operation.Parameters.Add(parameter);
	}
}