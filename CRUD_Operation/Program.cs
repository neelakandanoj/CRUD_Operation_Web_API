using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CRUD_Operation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ======================
            // CORS for Angular + Azure
            // ======================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                    policy.WithOrigins(
                            "http://localhost:4200",
                            "https://neelakandan-app-hcgmcxdch2ftcah2.centralindia-01.azurewebsites.net",
                            "https://neelakandan-amazon-fpcwcndfcyc8a7fz.southindia-01.azurewebsites.net"

                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                );
            });

            builder.Services.AddControllers();

            // ======================
            // JWT SETTINGS
            // ======================
            var jwtsettings = new Jwtsettingscs();
            builder.Configuration.GetSection("JwtSettings").Bind(jwtsettings);
            builder.Services.AddSingleton(jwtsettings);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtsettings.Issuer,
                    ValidAudience = jwtsettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtsettings.SecretKey)
                    )
                };
            });

            // ❌ REMOVE THIS — it breaks your auth
            // builder.Services.AddAuthentication();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

           
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
               c.RoutePrefix = string.Empty;  // 👈 Load swagger at root
            });
            

            app.UseCors("AllowAngularApp");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
