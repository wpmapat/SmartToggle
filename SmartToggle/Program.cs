using Microsoft.Azure.Cosmos;
using SmartToggle.BusinessLogic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Cosmos DB
var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"]!;
var cosmosDatabaseName = builder.Configuration["CosmosDb:DatabaseName"]!;
var companiesContainerName = builder.Configuration["CosmosDb:CompaniesContainerName"]!;
var servicesContainerName = builder.Configuration["CosmosDb:ServicesContainerName"]!;
var featureFlagsContainerName = builder.Configuration["CosmosDb:FeatureFlagsContainerName"]!;

var cosmosClient = new CosmosClient(cosmosConnectionString);
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
