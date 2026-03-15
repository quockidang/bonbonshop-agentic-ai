using BonBon.McpServers.Clients;
using BonBon.McpServers.Tools;
using System.Text.Json;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();


// Đăng ký Tool vào DI Container và cấu hình McpDotNet Server
builder.Services.AddTransient<GetOrderTool>();
builder.Services.AddTransient<SearchProductTool>();

builder.Services.AddRefitClient<IBonbonShopApi>()
    .ConfigureHttpClient(client =>
    {
        var bonbonShopUrl = "https://bbs-gateway-new-system.qc.bonbonshop.vn/brandapi";
        client.BaseAddress = new Uri(bonbonShopUrl);
    });


var app = builder.Build();

app.MapDefaultEndpoints();

app.MapPost("/tools/get_order", async (GetOrderTool tool, JsonDocument body) =>
{
    var arguments = body.RootElement.ToString();
    var result = await tool.ExecuteAsync(arguments, default);
    return Results.Text(result, "application/json");
});

app.MapPost("/tools/search_product", async (SearchProductTool tool, JsonDocument body) =>
{
    var arguments = body.RootElement.ToString();
    var result = await tool.ExecuteAsync(arguments, default);
    return Results.Text(result, "application/json");
});

app.Run();
