using APIFC3;
using APIFC3.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173","http://192.168.0.4") // Reemplaza el puerto 3000 con el puerto en el que se ejecuta tu aplicación React
                   .AllowAnyHeader()
                   .AllowAnyMethod();

            //builder.AllowAnyOrigin() // Reemplaza el puerto 3000 con el puerto en el que se ejecuta tu aplicación React
            //       .AllowAnyHeader()
            //       .AllowAnyMethod();

        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSqlServer<FlujoCajaContext>(builder.Configuration.GetConnectionString("flujoCajaString"));

/*para jwt*/

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer( opt =>
{
    
    var signingCredentials = new SigningCredentials(Constantes.signingKey, SecurityAlgorithms.HmacSha256Signature);

    opt.RequireHttpsMetadata = false;

    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = Constantes.signingKey,
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();

app.MapControllers();

app.Run();
