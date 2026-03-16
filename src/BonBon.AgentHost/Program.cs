using BonBon.AgentHost.Plugins;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
                options.Audience = "BonbonShop";
            });


builder.Services.AddHttpClient("McpServer", client => 
{
    var mcpUrl = builder.Configuration["MCP_INVENTORY_URL"] ?? "http://localhost:5243";
    client.BaseAddress = new Uri(mcpUrl);
});

var aiSettings = builder.Configuration.GetSection("AI");

var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.Plugins.AddFromType<OrderManagementPlugin>();
kernelBuilder.Plugins.AddFromType<SearchProductPlugin>();
kernelBuilder.Plugins.AddFromType<CreateOrderPlugin>();

if (builder.Environment.IsDevelopment())
{
    var ollamaAspireUrl = builder.Configuration["services:ollama:http:0"] ?? "http://localhost:11434/v1"; 
    // RUN ollama run qwen3.5:9b
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: aiSettings["LocalModelId"] ?? "qwen3.5:9b",
        apiKey: "lm-studio",
        endpoint: new Uri(ollamaAspireUrl),
        httpClient: new HttpClient { Timeout = TimeSpan.FromMinutes(15) }
    );

}
else
{
    // MÔI TRƯỜNG PRODUCTION: Sử dụng OpenAI
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: aiSettings["OpenAIModelId"] ?? "gpt-4o",
        apiKey: aiSettings["OpenAIApiKey"] ?? "sk-placeholder"
    );
}

// TODO: Đăng ký McpDotNet Client trỏ về Inventory MCP (Dựa trên tài liệu McpDotNet)
// var inventoryMcpUrl = builder.Configuration["MCP_INVENTORY_URL"] ?? "http://localhost:5000/mcp";
// builder.Services.AddMcpClient("inventory", new Uri(inventoryMcpUrl)); // Mã giả định

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapOpenApi();

var chatHistory = new ChatHistory();

chatHistory.AddSystemMessage("""
    # ROLE
Bạn là **BonBon Buddy**, Chuyên gia Hỗ trợ Vận hành và Mua sắm thông minh của hệ thống BonBonShop. Bạn là cầu nối giữa người dùng và hệ thống quản trị (ERP/CRM) thông qua giao thức MCP.

# CONTEXT
- Hệ thống backend: .NET 9 tích hợp ABP Framework.
- Cơ sở dữ liệu: MySQL 5.7.
- Đối tượng người dùng: Chủ cửa hàng tạp hóa, đại lý và người dùng cuối.
- Mục tiêu: Giúp khách hàng tìm sản phẩm, kiểm tra điểm thưởng và thực hiện đặt hàng nhanh chóng.

# CAPABILITIES & TOOLS (MCP)
Bạn có quyền truy cập vào các công cụ sau:
- `search_product`: Tìm kiếm sản phẩm theo tên/thuộc tính.
- `create_order`: Tạo đơn hàng mới.


# OPERATING GUIDELINES
1. **Phân tích Ý định (Intent Analysis):** - Luôn xác định xem người dùng muốn: Hỏi thông tin, Kiểm tra điểm, hay Đặt hàng.
   - Nếu thông tin thiếu (Vd: thiếu số lượng), hãy hỏi lại trước khi gọi Tool.

2. **Quy trình Tạo đơn hàng (BẮT BUỘC):**
   - Bước 1: Liệt kê danh sách sản phẩm và tổng tiền dự kiến.
   - Bước 2: Nhắc người dùng về số điểm tích lũy họ sẽ nhận được sau đơn này.
   - Bước 3: Chỉ gọi tool `create_bonbon_order` sau khi nhận được sự xác nhận rõ ràng (Vd: "Đồng ý", "Mua đi").

3. **Xử lý Dữ liệu:**
   - Ngày tháng: Nếu khách nói "hôm nay", hãy dùng mốc thời gian hệ thống là {current_date}.
   - Tiền tệ: Luôn hiển thị định dạng VNĐ (Vd: 500.000 VNĐ).

# CONSTRAINTS & SAFETY
- **Tuyệt đối không** thực hiện các lệnh xóa (Delete) hoặc thay đổi cấu trúc database.
- Không được bịa đặt (hallucinate) về giá sản phẩm hoặc tồn kho nếu Tool không trả về dữ liệu.
- Nếu gặp lỗi hệ thống từ MCP, hãy báo: "Hệ thống đang bận xử lý dữ liệu, anh/chị vui lòng thử lại sau giây lát."
- Giữ thái độ chuyên nghiệp, thân thiện, sử dụng "Dạ", "Anh/Chị".

# THOUGHT PROCESS (Chain of Thought)
Trước mỗi hành động, hãy tự trả lời ngầm:
- Người dùng là ai? (CustomerId)
- Tôi cần thông tin gì từ database? (ProductId, Stock)
- Tôi có cần người dùng xác nhận lại không?
"""
);

app.MapPost("/api/agent/chat", async (string prompt, Kernel kernel) =>
{
    // 1. Lấy dịch vụ Chat Completion từ Kernel
    var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

    // 2. Cấu hình để AI tự động gọi Tool (Function Calling)
    OpenAIPromptExecutionSettings settings = new() 
    { 
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
    };

    // 3. Thêm tin nhắn mới của người dùng vào lịch sử (ChatHistory)
    chatHistory.AddUserMessage(prompt);

    // 4. Gọi LLM với đầy đủ lịch sử và các Plugin đã đăng ký trong Kernel
    // Lưu ý: Phải truyền 'kernel' vào để AI có thể tìm thấy và thực thi các Plugin
    var response = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: settings,
        kernel: kernel
    );

    // 5. Quan trọng: Lưu phản hồi của AI vào lịch sử để lần chat sau nó nhớ nội dung này
    chatHistory.Add(response);

    // 6. Trả kết quả về cho Client
    return Results.Ok(new { Response = response.Content });
})
.WithName("ChatWithAgent")
.WithOpenApi();

app.Run();

public class OrderManagementPlugin
{
    private readonly HttpClient _httpClient;

    public OrderManagementPlugin(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("McpServer");
    }

    [KernelFunction("get_order")]
    [Description("Lấy thông tin tổng tiền đơn hàng khi biết Tenant và Order Code. Bắt buộc gọi để lấy doanh số.")]
    public async Task<string> GetOrderAsync(
        [Description("Mã Tenant (ví dụ: tenant-vn)")] string tenantId,
        [Description("Mã đơn hàng (ví dụ: SO-123)")] string orderCode)
    {
        var response = await _httpClient.PostAsJsonAsync("/tools/get_order", new { tenantId, orderCode });
        return await response.Content.ReadAsStringAsync();
    }
}
