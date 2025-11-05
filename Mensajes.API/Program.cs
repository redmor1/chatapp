using Mensajes.API.Data;
using Mensajes.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar BD
var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");

builder.Services.AddDbContext<MensajesDbContext>(options =>
options.UseMySQL(connectionString));


// Configurar Auth0
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth0:Authority"];
        options.Audience = builder.Configuration["Auth0:Audience"];
    });

// Añadir la política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowVercelPolicy",
                      policy =>
                      {
                          policy.SetIsOriginAllowed(origin =>
                          {
                              if (string.IsNullOrEmpty(origin)) return false;
                              
                              // Comprueba si el origen es tu URL de Vercel O una preview
                              // (ej. https://chatapp-front-one.vercel.app O https://chatapp-front-one-....vercel.app)
                              if (origin.StartsWith("https://chatapp-front-one") && origin.EndsWith(".vercel.app"))
                                  return true;
                                  
                              // Permite localhost para desarrollo
                              if (builder.Environment.IsDevelopment() && origin.StartsWith("http://localhost"))
                                  return true;

                              return false;
                          })
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});


builder.Services.AddScoped<IMensajeService, MensajeService>();

// Agregar servicios
builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowVercelPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
