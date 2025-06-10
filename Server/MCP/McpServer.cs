using System.ComponentModel;
using ModelContextProtocol.Server;

namespace MagnumOpus.MCP;

public static class McpServer
{
    private static WebApplication? _app;
    private static Task? _serverTask;

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

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";
}