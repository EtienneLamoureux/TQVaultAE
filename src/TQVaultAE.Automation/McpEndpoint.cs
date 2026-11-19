using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TQVaultAE.Automation;

/// <summary>
/// Starts the in-app MCP server of TQVaultAE and ARZExplorer: streamable HTTP, stateless, at <c>/mcp</c>.
/// </summary>
public static class McpEndpoint
{
	/// <summary>Path of the endpoint on the server.</summary>
	public const string Path = "/mcp";

	/// <summary>Address the clients use for a server listening on the loopback <paramref name="port"/>.</summary>
	public static string AddressOf(int port) => $"http://127.0.0.1:{port}{Path}";

	/// <summary>
	/// Builds and starts the server.
	/// </summary>
	/// <param name="serverName">Name the clients see (<c>tqvault</c>, <c>arzexplorer</c>).</param>
	/// <param name="endPoint">Where to listen (the loopback interface).</param>
	/// <param name="toolTypes">Classes whose static <c>[McpServerTool]</c> methods are the tools.</param>
	/// <param name="instructions">Server instructions sent to the clients, if any.</param>
	/// <param name="configureLogging">Logging of the server, after its default providers are removed.</param>
	/// <param name="attempts">Tries while the port is still held (a restarted process whose previous one has not let it go yet), 500 ms apart.</param>
	/// <returns>The running server; stop it with <c>StopAsync</c>.</returns>
	public static async Task<WebApplication> StartAsync(string serverName, IPEndPoint endPoint, IEnumerable<Type> toolTypes
		, string? instructions = null, Action<ILoggingBuilder>? configureLogging = null, int attempts = 1)
	{
		// A list, not an array: WithTools(Type[]) binds to WithTools<T>(T target), the tools of an instance of T.
		var tools = toolTypes.ToList();
		for (int attempt = 1; ; attempt++)
		{
			var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
			{
				ContentRootPath = AppContext.BaseDirectory,
				Args = [],
			});

			builder.WebHost.ConfigureKestrel(options => options.Listen(endPoint));
			builder.Logging.ClearProviders();
			configureLogging?.Invoke(builder.Logging);

			builder.Services
				.AddMcpServer(options =>
				{
					options.ServerInfo = new() { Name = serverName, Version = "1.0.0" };
					options.ServerInstructions = instructions;
				})
				.WithHttpTransport(options => options.Stateless = true)
				.WithTools((IEnumerable<Type>)tools);

			var app = builder.Build();
			app.MapMcp(Path);
			try
			{
				await app.StartAsync().ConfigureAwait(false);
				return app;
			}
			catch (IOException ex) when (ex.InnerException is AddressInUseException && attempt < attempts)
			{
				await app.DisposeAsync().ConfigureAwait(false);
				await Task.Delay(500).ConfigureAwait(false);
			}
			catch
			{
				await app.DisposeAsync().ConfigureAwait(false);
				throw;
			}
		}
	}
}
