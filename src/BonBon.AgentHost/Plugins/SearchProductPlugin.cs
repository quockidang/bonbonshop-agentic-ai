
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace BonBon.AgentHost.Plugins;

public class SearchProductPlugin
{
    private readonly HttpClient _httpClient;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public SearchProductPlugin(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = factory.CreateClient("McpServer");
        _httpContextAccessor = httpContextAccessor;
    }

    [KernelFunction("search_product")]
    [Description("Tìm kiếm sản phẩm theo tên hoặc mã sản phẩm.")]
    public async Task<string> SearchProductAsync(
        [Description("Từ khóa tìm kiếm")] string keyword)
    {

        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["TenantId"].ToString();
        var bearToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        var response = await _httpClient.PostAsJsonAsync("/tools/search_product", new { tenantId, keyword, bearToken });
        return await response.Content.ReadAsStringAsync();
    }
}