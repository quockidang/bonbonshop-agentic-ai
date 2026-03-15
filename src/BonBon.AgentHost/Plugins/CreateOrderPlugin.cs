
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BonBon.AgentHost.Plugins;

public class CreateOrderPlugin(IHttpClientFactory factory)
{
    private readonly HttpClient _httpClient = factory.CreateClient("McpServer");


    [KernelFunction("create_order")]
    [Description("Tạo đơn hàng mới.")]
    public async Task<string> CreateOrderAsync(
        [Description("Mã Tenant (ví dụ: tenant-vn)")] string tenantId)
    {

        var fakeOrderData = new
        {
            TenantId = tenantId,
            OrderCode = "SO-123",
            TotalAmount = "500.000 VNĐ",
            PointsEarned = 50
        };
        return System.Text.Json.JsonSerializer.Serialize(fakeOrderData);
    }
}