var builder = DistributedApplication.CreateBuilder(args);

var inventoryMcp = builder.AddProject<Projects.BonBon_McpServers>("inventory-mcp");

var agentHost = builder.AddProject<Projects.BonBon_AgentHost>("agent-host")
    .WithEnvironment("MCP_INVENTORY_URL", inventoryMcp.GetEndpoint("http"))
    .WithReference(inventoryMcp);

builder.Build().Run();
