using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using SmartToggle.BusinessLogic;

var builder = WebApplication.CreateBuilder(args);

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Cosmos DB
var cosmosAccountEndpoint = builder.Configuration["CosmosDb:AccountEndpoint"]!;
var cosmosDatabaseName = builder.Configuration["CosmosDb:DatabaseName"]!;
var companiesContainerName = builder.Configuration["CosmosDb:CompaniesContainerName"]!;
var servicesContainerName = builder.Configuration["CosmosDb:ServicesContainerName"]!;
var featureFlagsContainerName = builder.Configuration["CosmosDb:FeatureFlagsContainerName"]!;

var cosmosClient = new CosmosClient(cosmosAccountEndpoint, new DefaultAzureCredential());
builder.Services.AddSingleton(cosmosClient);

// Register repositories
builder.Services.AddScoped<ICompanyRepository>(provider =>
    new CompanyRepository(cosmosClient, cosmosDatabaseName, companiesContainerName));

builder.Services.AddScoped<IServiceRepository>(provider =>
    new ServiceRepository(cosmosClient, cosmosDatabaseName, servicesContainerName));

builder.Services.AddScoped<IFeatureFlagRepository>(provider =>
    new FeatureFlagRepository(cosmosClient, cosmosDatabaseName, featureFlagsContainerName));

// Register business logic
builder.Services.AddScoped<ICompanyBusinessLogic, CompanyBusinessLogic>();
builder.Services.AddScoped<IServiceBusinessLogic, ServiceBusinessLogic>();
builder.Services.AddScoped<IFeatureFlagBusinessLogic, FeatureFlagBusinessLogic>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowUI");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
