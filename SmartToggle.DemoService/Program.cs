using Microsoft.Identity.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var app = builder.Build();

var smartToggleApiBase = app.Configuration["SmartToggleApi:BaseUrl"] ?? "https://smarttoggle-api.azurewebsites.net";
var serviceId = app.Configuration["SmartToggleApi:ServiceId"] ?? "c848db6e-7782-477b-bc84-a67dca9f8ef3";
var tenantId = app.Configuration["AzureAd:TenantId"]!;
var clientId = app.Configuration["AzureAd:ClientId"]!;
var clientSecret = app.Configuration["AzureAd:ClientSecret"]!;
var smartToggleApiClientId = app.Configuration["SmartToggleApi:ClientId"] ?? "19e962db-511a-4e6b-b8af-a5752b3d54e5";

// Returns current feature flags as JSON (called by the demo page via polling)
app.MapGet("/api/flags", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        // Get token from Tenant B using client credentials
        var confidentialClient = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
            .Build();

        var tokenResult = await confidentialClient
            .AcquireTokenForClient([$"api://{smartToggleApiClientId}/.default"])
            .ExecuteAsync();

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);

        var response = await client.GetAsync($"{smartToggleApiBase}/api/featureflag/service/{serviceId}");
        if (!response.IsSuccessStatusCode)
            return Results.Ok(new List<object>());

        var flags = await response.Content.ReadFromJsonAsync<List<FeatureFlag>>();
        return Results.Ok(flags);
    }
    catch
    {
        return Results.Ok(new List<object>());
    }
});

// Serves the demo HTML page
app.MapGet("/", () => Results.Content(DemoPage.Html, "text/html"));

app.Run();

record FeatureFlag(string FlagId, bool DefaultValue);

static class DemoPage
{
    public static string Html => """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>SmartToggle Live Demo</title>
            <style>
                * { box-sizing: border-box; margin: 0; padding: 0; }

                body {
                    font-family: 'Segoe UI', sans-serif;
                    background: #0f172a;
                    color: #f1f5f9;
                    min-height: 100vh;
                    padding: 40px 20px;
                    transition: background 0.4s, color 0.4s, font-size 0.3s;
                }

                body.light-theme {
                    background: #f8fafc;
                    color: #0f172a;
                }

                body.large-font {
                    font-size: 1.25rem;
                }

                .container {
                    max-width: 720px;
                    margin: 0 auto;
                }

                header {
                    display: flex;
                    align-items: center;
                    gap: 12px;
                    margin-bottom: 32px;
                }

                .logo { font-size: 2rem; }

                h1 { font-size: 1.8rem; font-weight: 700; }

                .subtitle {
                    color: #94a3b8;
                    margin-top: 4px;
                    font-size: 0.95rem;
                }

                body.light-theme .subtitle { color: #64748b; }

                .card {
                    background: #1e293b;
                    border-radius: 12px;
                    padding: 28px;
                    margin-bottom: 24px;
                    border: 1px solid #334155;
                    transition: background 0.4s, border-color 0.4s;
                }

                body.light-theme .card {
                    background: #ffffff;
                    border-color: #e2e8f0;
                    box-shadow: 0 1px 4px rgba(0,0,0,0.06);
                }

                .card h2 {
                    font-size: 1.1rem;
                    margin-bottom: 16px;
                    color: #7c3aed;
                }

                .flag-row {
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    padding: 12px 0;
                    border-bottom: 1px solid #334155;
                }

                body.light-theme .flag-row { border-bottom-color: #e2e8f0; }

                .flag-row:last-child { border-bottom: none; }

                .flag-name { font-weight: 500; }

                .badge {
                    padding: 4px 12px;
                    border-radius: 20px;
                    font-size: 0.8rem;
                    font-weight: 600;
                    letter-spacing: 0.05em;
                }

                .badge.on { background: #dcfce7; color: #15803d; }
                .badge.off { background: #fee2e2; color: #b91c1c; }

                .demo-area {
                    text-align: center;
                    padding: 32px;
                    border-radius: 12px;
                    margin-bottom: 24px;
                    background: linear-gradient(135deg, #7c3aed22, #2563eb22);
                    border: 1px dashed #7c3aed66;
                }

                .demo-area p { line-height: 1.7; margin-bottom: 8px; }

                .status-bar {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    font-size: 0.8rem;
                    color: #64748b;
                    margin-top: 8px;
                }

                .dot {
                    width: 8px; height: 8px;
                    border-radius: 50%;
                    background: #22c55e;
                    animation: pulse 2s infinite;
                }

                @keyframes pulse {
                    0%, 100% { opacity: 1; }
                    50% { opacity: 0.4; }
                }

                .how-it-works ol {
                    padding-left: 20px;
                    line-height: 2;
                    color: #94a3b8;
                }

                body.light-theme .how-it-works ol { color: #475569; }

                a.manage-link {
                    display: inline-block;
                    margin-top: 20px;
                    padding: 10px 20px;
                    background: #7c3aed;
                    color: white;
                    border-radius: 8px;
                    text-decoration: none;
                    font-weight: 500;
                    font-size: 0.9rem;
                }

                a.manage-link:hover { background: #6d28d9; }
            </style>
        </head>
        <body id="demoBody">
            <div class="container">
                <header>
                    <span class="logo">⚡</span>
                    <div>
                        <h1>SmartToggle Live Demo</h1>
                        <p class="subtitle">Feature flags changing this page in real time</p>
                    </div>
                </header>

                <div class="card">
                    <h2>Active Feature Flags</h2>
                    <div id="flagList"><p style="color:#64748b">Loading flags...</p></div>
                </div>

                <div class="demo-area" id="demoArea">
                    <p><strong>This area changes based on your feature flags.</strong></p>
                    <p id="themeMsg">Enable <code>dark-theme</code> to switch to light mode, or <code>large-font</code> to increase text size.</p>
                </div>

                <div class="card how-it-works">
                    <h2>How It Works</h2>
                    <ol>
                        <li>Feature flags are stored in <strong>Azure Cosmos DB</strong></li>
                        <li>This app (Tenant B) authenticates to SmartToggle API (Tenant A) using <strong>OAuth2 client credentials</strong></li>
                        <li>This page polls the <strong>SmartToggle API</strong> every 10 seconds</li>
                        <li>Toggle a flag in SmartToggle → this page updates automatically</li>
                        <li>No redeployment needed — changes take effect instantly</li>
                    </ol>
                    <a class="manage-link" href="https://github.com/wpmapat/SmartToggle" target="_blank">View on GitHub →</a>
                </div>

                <div class="status-bar">
                    <span class="dot"></span>
                    <span id="lastUpdated">Connecting...</span>
                </div>
            </div>

            <script>
                async function loadFlags() {
                    try {
                        const res = await fetch('/api/flags');
                        const flags = await res.json();

                        const body = document.getElementById('demoBody');
                        const flagList = document.getElementById('flagList');

                        if (!flags.length) {
                            flagList.innerHTML = '<p style="color:#64748b">No flags found for this service.</p>';
                        } else {
                            flagList.innerHTML = flags.map(f => `
                                <div class="flag-row">
                                    <span class="flag-name">${f.flagId}</span>
                                    <span class="badge ${f.defaultValue ? 'on' : 'off'}">${f.defaultValue ? 'ON' : 'OFF'}</span>
                                </div>
                            `).join('');
                        }

                        // Apply dark-theme flag
                        const darkTheme = flags.find(f => f.flagId === 'dark-theme');
                        if (darkTheme?.defaultValue) {
                            body.classList.remove('light-theme');
                        } else {
                            body.classList.add('light-theme');
                        }

                        // Apply large-font flag
                        const largeFont = flags.find(f => f.flagId === 'large-font');
                        if (largeFont?.defaultValue) {
                            body.classList.add('large-font');
                        } else {
                            body.classList.remove('large-font');
                        }

                        document.getElementById('lastUpdated').textContent =
                            'Last updated: ' + new Date().toLocaleTimeString();
                    } catch {
                        document.getElementById('lastUpdated').textContent = 'Failed to fetch flags — retrying...';
                    }
                }

                loadFlags();
                setInterval(loadFlags, 10000);
            </script>
        </body>
        </html>
        """;
}
