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
                          policy.SetIsOriginAllowed(origin => true) // Permitir cualquier origen para debugging
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Necesario para SignalR
                      });
});


builder.Services.AddScoped<IMensajeService, MensajeService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IConversacionesApiClient, ConversacionesApiClient>(client =>
{
    var conversacionesApiUrl = builder.Configuration["Services:ConversacionesApi"];
    // Fallback if config is missing, though it should be there
    if (string.IsNullOrEmpty(conversacionesApiUrl)) conversacionesApiUrl = "http://localhost:5002"; // Adjust port as needed
    client.BaseAddress = new Uri(conversacionesApiUrl);
});

builder.Services.AddSignalR();

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowVercelPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<Mensajes.API.Hubs.MensajesHub>("/hubs/mensajes");
app.Run();
