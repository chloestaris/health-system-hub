using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using HealthHub.API.Services;
using HealthHub.API.Data;
using SoapCore;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
builder.Services.AddDbContext<HealthHubContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure mTLS
builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.ValidateValidityPeriod = false;  // Disable validity period check for testing
        options.RevocationMode = X509RevocationMode.NoCheck;  // Disable revocation check for self-signed certs
    });

// Configure SOAP Core
builder.Services.AddSoapCore();

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
        https.CheckCertificateRevocation = false;  // Disable revocation check for self-signed certs
        https.ServerCertificateSelector = (context, name) =>
        {
            // Load the server certificate
            var serverCert = X509Certificate2.CreateFromPemFile("/app/certs/server.crt", "/app/certs/server.key");
            return serverCert;
        };
        https.ClientCertificateValidation = (certificate, chain, errors) => true;  // Accept all client certificates
    });
});

var app = builder.Build();

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<HealthHubContext>();
        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
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
        logger.LogInformation("Request {Method} {Path} completed with status {StatusCode}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Request {Method} {Path} failed",
            context.Request.Method, context.Request.Path);
        throw;
    }
});

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
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
                            <faultstring>" + ex.Message + @"</faultstring>
                        </soap:Fault>
                    </soap:Body>
                </soap:Envelope>");
        }
        
        var logger = context.RequestServices.GetService<ILogger<Program>>();
        logger?.LogError(ex, "An unhandled exception occurred");
    }
});

// Configure SOAP endpoints
app.UseEndpoints(endpoints =>
{
    var encoder = new SoapEncoderOptions
    {
        WriteEncoding = System.Text.Encoding.UTF8,
        ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max
    };

    endpoints.UseSoapEndpoint<IEhrService>("/EhrService.asmx", encoder, SoapSerializer.XmlSerializer);
    endpoints.UseSoapEndpoint<IInsuranceService>("/InsuranceService.asmx", encoder, SoapSerializer.XmlSerializer);
    endpoints.UseSoapEndpoint<IGovernmentHealthService>("/GovernmentHealthService.asmx", encoder, SoapSerializer.XmlSerializer);
});

app.Run(); 