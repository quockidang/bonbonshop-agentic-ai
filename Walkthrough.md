# BonBonShop Agentic AI - Boilerplate Walkthrough

Tôi đã hoàn tất việc khởi tạo toàn bộ Solution và Boilerplate cho dự án **BonBonShop Agentic AI**. Thay vì chỉ liệt kê code tĩnh, tôi đã chủ động tạo sẵn một **Solution thực tế** bằng .NET 9 SDK tại thư mục gốc của bạn (`/Volumes/kidang data/Documents/bonbonshop-agentic-ai`). Solution này đã được cấu hình chặt chẽ theo chuẩn Microservices với .NET Aspire và hoàn toàn biên dịch (`dotnet build`) thành công!

Dưới đây là các điểm nổi bật của Boilerplate đã sinh ra:

## 1. Cấu trúc Dự Án và Packages

Mọi file `.csproj` đã được định cấu hình thống nhất sử dụng `<TargetFramework>net9.0</TargetFramework>` và các thư viện tiên tiến nhất:

- **BonBon.AppHost**: Chứa .NET Aspire 9.0 Host và tích hợp **CommunityToolkit.Aspire.Hosting.Ollama** để tự động kéo Container Ollama trên máy Dev chứa model `llama3.2`.
- **BonBon.AgentHost**: Đã phân luồng sẵn **Semantic Kernel** giữa `IsDevelopment` (dùng cấu hình Local Ollama tiêm bởi AppHost) và môi trường `Production` (dùng OpenAI Key).
- **BonBon.McpServers**: Đã cài đặt sẵn **Dapper**, **MySqlConnector** để thao tác cực nhanh với legacy MySQL 5.7, và thư viện **mcpdotnet** để publish Tools.
- **BonBon.Shared** & **BonBon.ServiceDefaults**: Các file chuẩn đoán và interface chung.

## 2. Các File Logic Quan Trọng Nhất

Bạn có thể mở Repository ra và xem trực tiếp các file sau (tôi đã cấu hình chuẩn xác dựa trên Best Practices):

### [MODIFY] [BonBon.AgentHost/Program.cs](file:///Volumes/kidang%20data/Documents/bonbonshop-agentic-ai/src/BonBon.AgentHost/Program.cs)
Đây là não bộ của hệ thống. Tôi đã rẽ nhánh Logic như bạn yêu cầu thông qua `builder.Environment.EnvironmentName == "Development"`.
Đáng chú ý nhất, trong API chat `/api/agent/chat`, tính năng Function Calling đã được bật tự động:
```csharp
var settings = new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
var response = await kernel.InvokePromptAsync(prompt, new(settings));
```

### [NEW] [BonBon.McpServers/Tools/GetInventoryTool.cs](file:///Volumes/kidang%20data/Documents/bonbonshop-agentic-ai/src/BonBon.McpServers/Tools/GetInventoryTool.cs)
Đây là điểm nhấn **Multitenancy**. Công cụ `get_inventory` này tuân thủ định nghĩa chuẩn của MCP Protocol, ép buộc Semantic Kernel phải truyền `tenantId`:
```json
// Input Schema
"tenantId": { 
    "type": "string", 
    "description": "Mã Tenant hiện tại của User (ví dụ: tenant-vn, tenant-sg)" 
}
```
Thông qua code Dapper, bạn có thể phân tuyến chuỗi kết nối MySQL (`ResolverConnectionString`) dựa trên Tenant này để query bảng Inventory.

### [MODIFY] [BonBon.AppHost/Program.cs](file:///Volumes/kidang%20data/Documents/bonbonshop-agentic-ai/src/BonBon.AppHost/Program.cs)
Tại đây, Ollama và các MCP Server được nhúng trực tiếp thành các Reference cho `agent-host`. Aspire sẽ tự động chia sẻ port và biến môi trường:
```csharp
var chatModel = builder.AddOllama("ollama")
                       .WithDataVolume()
                       .AddModel("llama3.2"); 
agentHost.WithReference(chatModel);
```

## 3. Cách chạy thử toàn bộ hệ thống ngay bây giờ

Hệ thống đã sẵn sàng để khởi chạy trên máy Mac của bạn:

1. Mở Cửa sổ Terminal mới.
2. Di chuyển vào thư mục Aspire Host: `cd "/Volumes/kidang data/Documents/bonbonshop-agentic-ai/src/BonBon.AppHost"`
3. Chạy lệnh: `dotnet run`
4. Một trang web **.NET Aspire Dashboard** sẽ hiện ra. Tại đây bạn sẽ thấy các container/service bật lên (bao gồm cả tiến trình tải model `llama3.2` của Docker/Ollama).
5. Bạn có thể chọc vào Swagger UI của `agent-host` để thử gọi API Chat.

> [!TIP]
> Do bạn đang dùng mac, hãy chắc chắn máy bạn cài Docker Desktop (hoặc OrbStack) đang bật để Aspire có thể pull các container Ollama về tự động phục vụ Local LLM nhé!
