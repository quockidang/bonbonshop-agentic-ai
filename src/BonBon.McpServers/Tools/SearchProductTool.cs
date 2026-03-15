using System.Text.Json;
using BonBon.McpServers.Clients;

namespace BonBon.McpServers.Tools;

public class SearchProductTool(IBonbonShopApi bonbonShopApi)
{
    public string Name => "search_product_tool";
    public string Description => "Tìm kiếm sản phẩm theo tên hoặc mã sản phẩm. Công cụ này yêu cầu TenatId để xác định database đích.";

    public string InputSchema => """
    {
        "type": "object",
        "properties": {
            "tenantId": { 
                "type": "string", 
                "description": "Mã Tenant hiện tại của User (ví dụ: tenant-vn, tenant-sg)" 
            },
            "keyword": { 
                "type": "string", 
                "description": "Từ khóa tìm kiếm sản phẩm" 
            },
            "bearToken": {
                "type": "string",
                "description": "Token để xác thực khi gọi API"
            }
        },
        "required": ["tenantId", "keyword"]
    }
    """;

    public async Task<string> ExecuteAsync(string arguments, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        using var jsonDoc = JsonDocument.Parse(arguments);
        var tenantId = jsonDoc.RootElement.GetProperty("tenantId").GetString();
        var keyword = jsonDoc.RootElement.GetProperty("keyword").GetString();
        var token = jsonDoc.RootElement.GetProperty("bearToken").GetString();

        Console.WriteLine($"TenantId: {tenantId}");
        Console.WriteLine($"Keyword: {keyword}");

       // Call API de lay danh sach san pham tu MCP Server
         var apiResult = await bonbonShopApi.SearchProductAsync(token!, tenantId!, new SearchProductRequest { Keyword = keyword! });

        // Flatten dữ liệu trước khi trả về cho Agent
        var flattenedItems = apiResult.Data.Items.SelectMany(category => category.Children.Select(product => new
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            ProductDisplayName = product.ProductDisplayName,
            Sorting = product.Sorting,
            IsPromotion = product.IsPromotion
        })).ToList();

        return JsonSerializer.Serialize(flattenedItems);
    }
}
