using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using HealthHub.API.Services;
using HealthHub.API.Data;
using SoapCore;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL
builder.Services.AddDbContext<HealthHubContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure mTLS
builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.ValidateValidityPeriod = false;
        options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;
    });

// Configure SOAP Core with proper DateTime handling
builder.Services.AddSoapCore();
builder.Services.AddSoapModelBindingFilter(new SoapModelBindingFilter
{
    DateTimeOffset = DateTimeHandling.ConvertToUtc
});

// Configure SOAP Services as scoped
builder.Services.AddScoped<IEhrService, EhrService>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<IGovernmentHealthService, GovernmentHealthService>();

// Configure Kestrel for HTTPS and mTLS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(https =>
    {
        https.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        https.CheckCertificateRevocation = false;
        https.ServerCertificateSelector = (context, name) =>
        {
            return X509Certificate2.CreateFromPemFile("/app/certs/server.crt", "/app/certs/server.key");
        };
        https.ClientCertificateValidation = (cert, chain, errors) =>
        {
            if (cert == null) return false;
            
            using var chainBuilder = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority,
                    TrustMode = X509ChainTrustMode.System
                }
            };
            
            return chainBuilder.Build(cert);
        };
    });
});

var app = builder.Build();

// Initialize database with retries
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var maxRetries = 5;
    var retryDelay = TimeSpan.FromSeconds(5);

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            logger.LogInformation("Attempting database initialization (Attempt {Attempt} of {MaxAttempts})", i + 1, maxRetries);
            var context = services.GetRequiredService<HealthHubContext>();
            await context.Database.EnsureCreatedAsync();
            await DbInitializer.Initialize(context);
            logger.LogInformation("Database initialization successful");
            break;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization attempt {Attempt} of {MaxAttempts} failed", i + 1, maxRetries);
            if (i == maxRetries - 1)
            {
                logger.LogError("All database initialization attempts failed");
                throw;
            }
            await Task.Delay(retryDelay);
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add request logging
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request {Method} {Path} started", context.Request.Method, context.Request.Path);
    
    // Log request body
    if (context.Request.ContentLength > 0)
    {
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;
        logger.LogInformation("Request Body: {Body}", body);
    }
    
    try
    {
        await next();
        logger.LogInformation("Request completed: {StatusCode}", context.Response.StatusCode);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Request failed");
        throw;
    }
});

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Add error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
        
        if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
        {
            context.Response.ContentType = "text/xml";
            await context.Response.WriteAsync(@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                    <soap:Body>
                        <soap:Fault>
                            <faultcode>soap:Client</faultcode>
                            <faultstring>Endpoint not found</faultstring>
                        </soap:Fault>
                    </soap:Body>
                </soap:Envelope>");
        }
    }
    catch (Exception ex)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/xml";
            await context.Response.WriteAsync(@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                    <soap:Body>
                        <soap:Fault>
                            <faultcode>soap:Server</faultcode>
                            <faultstring>Internal Server Error</faultstring>
                        </soap:Fault>
                    </soap:Body>
                </soap:Envelope>");
        }
        
        throw;
    }
});

// Configure SOAP endpoints with proper DateTime handling
app.UseEndpoints(endpoints =>
{
    var encoder = new SoapEncoderOptions
    {
        WriteEncoding = System.Text.Encoding.UTF8,
        ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max,
        DateTimeHandling = DateTimeHandling.ConvertToUtc
    };

    endpoints.UseSoapEndpoint<IEhrService>("/EhrService.asmx", encoder, SoapSerializer.XmlSerializer);
    endpoints.UseSoapEndpoint<IInsuranceService>("/InsuranceService.asmx", encoder, SoapSerializer.XmlSerializer);
    endpoints.UseSoapEndpoint<IGovernmentHealthService>("/GovernmentHealthService.asmx", encoder, SoapSerializer.XmlSerializer);
});

app.Run(); 