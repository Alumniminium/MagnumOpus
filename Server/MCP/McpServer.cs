namespace MagnumOpus.MCP;

public static class McpServer
{
    public static void Start()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly();

        var app = builder.Build();

        app.MapMcp();

        app.Run("http://0.0.0.0:5000");

    }
}