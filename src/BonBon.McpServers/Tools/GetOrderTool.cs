using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;

namespace BonBon.McpServers.Tools;

public class GetOrderTool
{
    public string Name => "get_order_tool";
    public string Description => "Lấy thông tin đơn hàng. Công cụ này yêu cầu TenatId để xác định database đích.";

    public string InputSchema => """
    {
        "type": "object",
        "properties": {
            "tenantId": { 
                "type": "string", 
                "description": "Mã Tenant hiện tại của User (ví dụ: tenant-vn, tenant-sg)" 
            },
            "orderCode": { 
                "type": "string", 
                "description": "Mã đơn hàng" 
            }
        },
        "required": ["tenantId", "orderCode"]
    }
    """;

    public async Task<string> ExecuteAsync(string arguments, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        using var jsonDoc = JsonDocument.Parse(arguments);
        var tenantId = jsonDoc.RootElement.GetProperty("tenantId").GetString();
        var orderCode = jsonDoc.RootElement.GetProperty("orderCode").GetString();

        Console.WriteLine($"TenantId: {tenantId}");
        Console.WriteLine($"OrderCode: {orderCode}");

        // 1. Resolve Connection String dựa theo TenantId (Multitenancy)
        var connectionString = ResolveConnectionString(tenantId);

        // 2. Query MySQL 5.7 Legacy
        using var connection = new MySqlConnection(connectionString);
        var orderObject = await connection.QueryFirstOrDefaultAsync<object>(
            "SELECT * FROM dmspro_mys_order_list WHERE ret_order_code = @orderCode", new { orderCode });

        return JsonSerializer.Serialize(new
        {
            TenantId = tenantId,
            OrderCode = orderCode,
            orderObject = orderObject,
            Status = "Success"
        });
    }

    private string ResolveConnectionString(string? tenantId)
    {
        // Định tuyến connection string theo tenant, giả định:
        return $"server=bbsqc.mysql.database.azure.com;port=3306;Database=dmspro_usermanagement_237;user=adminbbs;password=bonbonshop_qc123!;Charset=utf8mb4;Character Set=utf8mb4;Convert Zero Datetime=True;allow zero datetime=no;ConnectionTimeout=15;MaximumPoolSize=600;";
    }
}
