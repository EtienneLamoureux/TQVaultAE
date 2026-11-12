using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Moq.Protected;
using TQVaultAE.Config;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class PasteBinServiceTests
{
	private const string TestApiKey = "test-api-key";
	private const string TestExpiration = "1M";

	private static UserSettings CreateSettings()
		=> new()
		{
			PasteBinApiKey = TestApiKey,
			PasteBinExpiration = TestExpiration
		};

	private static PasteBinService CreateService(HttpMessageHandler handler)
	{
		var httpClient = new HttpClient(handler);
		var settings = CreateSettings();
		return new PasteBinService(httpClient, settings);
	}

	[Fact]
	public async Task UploadAsync_WithSuccessResponse_ShouldReturnPasteUrl()
	{
		var handler = new Mock<HttpMessageHandler>();
		handler.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("https://pastebin.com/ABC123")
			});

		var service = CreateService(handler.Object);
		var url = await service.UploadAsync("test content");

		url.Should().Be("https://pastebin.com/ABC123");
	}

	[Fact]
	public async Task UploadAsync_WithApiError_ShouldThrowInvalidOperationException()
	{
		var handler = new Mock<HttpMessageHandler>();
		handler.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("Bad API request, invalid api_option")
			});

		var service = CreateService(handler.Object);
		var act = () => service.UploadAsync("test content");

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Bad API request*");
	}

	[Fact]
	public async Task FetchPasteAsync_WithValidUrl_ShouldReturnContent()
	{
		var handler = new Mock<HttpMessageHandler>();
		handler.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("paste content here")
			});

		var service = CreateService(handler.Object);
		var content = await service.FetchPasteAsync("https://pastebin.com/ABC123");

		content.Should().Be("paste content here");
	}

	[Fact]
	public async Task FetchPasteAsync_WithHttpError_ShouldThrowInvalidOperationException()
	{
		var handler = new Mock<HttpMessageHandler>();
		handler.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.NotFound,
				Content = new StringContent("")
			});

		var service = CreateService(handler.Object);
		var act = () => service.FetchPasteAsync("https://pastebin.com/invalid");

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*404*");
	}

	[Theory]
	[InlineData(null, UserSettings.DefaultPasteBinExpiration)]
	[InlineData("1W", "1W")]
	public async Task UploadAsync_PostsTheExpiration_DefaultWhenUnset(string? expiration, string expected)
	{
		string postedForm = string.Empty;
		var handler = new Mock<HttpMessageHandler>();
		handler.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.Callback<HttpRequestMessage, CancellationToken>((request, token) => postedForm = request.Content!.ReadAsStringAsync(token).GetAwaiter().GetResult())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("https://pastebin.com/ABC123")
			});

		var settings = new UserSettings { PasteBinApiKey = TestApiKey, PasteBinExpiration = expiration! };
		var service = new PasteBinService(new HttpClient(handler.Object), settings);
		await service.UploadAsync("test content");

		postedForm.Should().Contain($"api_paste_expire_date={expected}");
	}
}
