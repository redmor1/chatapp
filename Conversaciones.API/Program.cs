using Conversaciones.API.Data;
using Conversaciones.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar BD
var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");
builder.Services.AddDbContext<ConversacionesDbContext>(options =>
    options.UseMySQL(connectionString!));

// Configurar Auth0
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
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
                          policy.SetIsOriginAllowed(origin => true) // Permitir cualquier origen para debugging
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});


// Registrar servicios
builder.Services.AddScoped<IConversacionService, ConversacionService>();

// Configurar HttpClient para Usuarios.API
builder.Services.AddHttpClient<IUsuariosApiClient, UsuariosApiClient>(client =>
{
    var usuariosApiUrl = builder.Configuration["Services:UsuariosApi"];
    client.BaseAddress = new Uri(usuariosApiUrl!);
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowVercelPolicy");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
