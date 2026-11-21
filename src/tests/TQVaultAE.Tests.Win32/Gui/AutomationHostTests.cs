using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// The start and stop of the MCP automation endpoint by the settings (<c>AutomationHost.Apply</c>, what Release builds
/// run at startup and when the settings change): a real server on a free loopback port.
/// </summary>
public sealed class AutomationHostTests
{
	private static readonly Type Host = typeof(TQVaultAE.GUI.Program).Assembly.GetType("TQVaultAE.GUI.Automation.AutomationHost", throwOnError: true)!;

	private static void Apply(bool enabled, int port)
		=> Host.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [enabled, port, null]);

	private static string? Address => (string?)Host.GetProperty("Address", BindingFlags.Public | BindingFlags.Static)!.GetValue(null);

	private static int FreePort()
	{
		using var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		return ((IPEndPoint)listener.LocalEndpoint).Port;
	}

	private static async Task<string?> WaitForAddressAsync(Func<string?, bool> done)
	{
		for (var i = 0; i < 100 && !done(Address); i++)
			await Task.Delay(100, TestContext.Current.CancellationToken);
		return Address;
	}

	private static async Task<JsonElement> CallAsync(HttpClient http, string address, string method)
	{
		var body = JsonSerializer.Serialize(new
		{
			jsonrpc = "2.0",
			id = 1,
			method,
			@params = new { protocolVersion = "2025-06-18", capabilities = new { }, clientInfo = new { name = "test", version = "1" } },
		});
		using var request = new HttpRequestMessage(HttpMethod.Post, address) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
		request.Headers.Accept.ParseAdd("application/json");
		request.Headers.Accept.ParseAdd("text/event-stream");
		request.Headers.Add("MCP-Protocol-Version", "2025-06-18");
		using var response = await http.SendAsync(request, TestContext.Current.CancellationToken);
		var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		var json = text.Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith("data:", StringComparison.Ordinal))?[5..] ?? text;
		return JsonDocument.Parse(json).RootElement.Clone();
	}

	[Fact]
	public async Task Apply_OnThenOff_ServesTheToolsOnLoopbackThenStops()
	{
		var port = FreePort();
		using var http = new HttpClient();
		try
		{
			Apply(enabled: true, port);
			var address = await WaitForAddressAsync(a => a is not null);

			address.Should().Be($"http://127.0.0.1:{port}/mcp");

			var initialize = await CallAsync(http, address!, "initialize");
			initialize.GetProperty("result").GetProperty("instructions").GetString().Should().Contain("caravan", "the rules reach the agent at connection");

			var tools = await CallAsync(http, address!, "tools/list");
			var names = tools.GetProperty("result").GetProperty("tools").EnumerateArray().Select(t => t.GetProperty("name").GetString()).ToList();
			names.Should().Contain(["get_state", "pick_item", "capture"]);
#if !DEBUG
			names.Should().NotContain(["control_tree", "set_control", "picture_fault"], "Release builds serve the user tools only");
#endif
		}
		finally
		{
			Apply(enabled: false, port);
			await WaitForAddressAsync(a => a is null);
		}

		Address.Should().BeNull();
		var stopped = () => new TcpClient().Connect(IPAddress.Loopback, port);
		stopped.Should().Throw<SocketException>("the endpoint stops when the setting is turned off");
	}
}
