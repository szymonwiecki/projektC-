using System.Text;
using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;


var builder = WebApplication.CreateBuilder(args);

// Dodaj klucz JWT
var key = Encoding.UTF8.GetBytes("YourSecretKey12345"); 

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Dodaj konfiguracj� bazy danych
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Library API", Version = "v1" });

    // Konfiguracja uwierzytelniania JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Podaj token JWT w formacie: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    options.EnableAnnotations();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

app.UseCors("AllowAll"); // Dodaj t� lini�


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Dodanie danych testowych
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    try
    {
        // Sprawd�, czy baza danych istnieje
        if (db.Database.CanConnect())
        {
            // Je�li baza istnieje, zastosuj tylko brakuj�ce migracje
            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
                Console.WriteLine("Zastosowano brakuj�ce migracje.");
            }
            else
            {
                Console.WriteLine("Baza danych jest aktualna - nie trzeba stosowa� migracji.");
            }
        }
        else
        {
            // Je�li baza nie istnieje, utw�rz j� z migracjami
            db.Database.Migrate();
            Console.WriteLine("Utworzono now� baz� danych z migracjami.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"B��d podczas migracji bazy danych: {ex.Message}");
        // Mo�esz tutaj doda� dodatkowe logowanie b��d�w
    }

    // SEEDING � tylko je�li tabela Books jest pusta
    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new Book { Title = "1984", Author = "George Orwell", PublishedYear = 1949, Genre = "Dystopia" },
            new Book { Title = "Hobbit", Author = "J.R.R. Tolkien", PublishedYear = 1937, Genre = "Fantasy" },
            new Book { Title = "Nowy wspania�y �wiat", Author = "Aldous Huxley", PublishedYear = 1932, Genre = "Science Fiction" }
        );
        db.SaveChanges();
    }
}


app.UseAuthentication();

app.UseAuthorization();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();