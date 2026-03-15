


using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BonBon.McpServers.Clients.SeedWork;
using Microsoft.AspNetCore.Authorization;
using Refit;

namespace BonBon.McpServers.Clients;

public interface IBonbonShopApi
{

    [Post("/{tenantId}/product/list/v2")]
    [Headers("Accept: application/json")]
    Task<ApiResult<ResultPaging<ProductListCategoryDto>>> SearchProductAsync(
        [Header("Authorization")] string authorization, 
        [AliasAs("tenantId")] string tenantId, [Body] SearchProductRequest request);
}

public class SearchProductRequest
{
    [JsonPropertyName("keyword")]
    public string Keyword { get; init; }
}

public record ProductListCategoryDto
{
    [JsonPropertyName("category_id")]
    public long? CategoryId { get; init; }
    
    [JsonPropertyName("category_name")]
    public string? CategoryName { get; init; }
    
    [JsonPropertyName("children")]
    public IEnumerable<ProductListDto> Children { get; init; } = Enumerable.Empty<ProductListDto>();
}

public record ProductListDto
{
    [JsonPropertyName("category_id")]
    public long CategoryId { get; init; }

    [JsonPropertyName("category_name")]
    public string CategoryName { get; init; }

    [JsonPropertyName("product_id")]
    public int ProductId { get; init; }

    [JsonPropertyName("product_code")]
    public string ProductCode { get; init; }

    [JsonPropertyName("product_display_name")]
    public string ProductDisplayName { get; init; }

    [JsonPropertyName("sorting")]
    public string Sorting { get; init; }

    [JsonPropertyName("is_promotion")]
    public bool? IsPromotion { get; init; }

    [JsonPropertyName("promotion")]
    public string? Promotion { get; init; }

    [JsonPropertyName("retail_reference_price")]
    public double? RetailReferencePrice { get; init; }

    [JsonPropertyName("sales_unit")]
    public string SalesUnit { get; init; }

    [JsonPropertyName("product_image")]
    public string ProductImage { get; init; }

    [JsonPropertyName("product_uom_id")]
    public long ProductUomId { get; init; }
    
    [JsonIgnore]
    public decimal? ConvFactor { get; init; }
    
    [JsonPropertyName("conv_factor_text")]
    public string? ConvFactorText
    {
        get
        {
            if (string.IsNullOrEmpty(BaseUomName))
            {
                return null;
            }
            
            var containsNumber = Regex.IsMatch(BaseUomName, @"\d");
            if (containsNumber)
            {
                return BaseUomName;
            }
            return ConvFactor > 1 ? $"{decimal.ToInt64(ConvFactor.Value).ToString()} {BaseUomName}" : null;
        }
    }
    [JsonPropertyName("base_uom_name")]
    [JsonIgnore]
    public string? BaseUomName { get; init; }

    [JsonPropertyName("recent_order_quantity")]
    public int? RecentOrderQuantity { get; set; }
}