//-----------------------------------------------------------------------
// <copyright file="AutomationJson.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// The one JSON format of the automation tools for geometry and colours: rectangles <c>{"x","y","width","height"}</c>,
/// points <c>{"x","y"}</c>, sizes <c>{"width","height"}</c> (integers, or numbers for the floating-point types), colours
/// <c>"#RRGGBB"</c>, or <c>"#AARRGGBB"</c> when the colour is not opaque, and <c>null</c> for <see cref="Color.Empty"/>.
/// A tool passes the <see cref="System.Drawing"/> value itself and <see cref="Options"/> writes it.
/// </summary>
internal static class AutomationJson
{
	/// <summary>The serializer options of every tool result and dump.</summary>
	public static JsonSerializerOptions Options { get; } = new()
	{
		WriteIndented = true,
		Converters =
		{
			new PointConverter(),
			new PointFConverter(),
			new SizeConverter(),
			new SizeFConverter(),
			new RectangleConverter(),
			new RectangleFConverter(),
			new ColorConverter(),
		},
	};

	/// <summary>A colour as the tools write it: <c>#RRGGBB</c>, or <c>#AARRGGBB</c> when it is not opaque.</summary>
	public static string Hex(Color color)
		=> color.A == 255
			? string.Create(CultureInfo.InvariantCulture, $"#{color.R:X2}{color.G:X2}{color.B:X2}")
			: string.Create(CultureInfo.InvariantCulture, $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");

	/// <summary>A colour as the tools write it (<see cref="Hex(Color)"/>), null for <see cref="Color.Empty"/>.</summary>
	public static string? HexOrNull(Color color) => color.IsEmpty ? null : Hex(color);

	/// <summary>Writes only: the tools never read these shapes back.</summary>
	private abstract class WriteOnlyConverter<T> : JsonConverter<T>
	{
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> throw new NotSupportedException($"{typeof(T).Name} is written only.");
	}

	private sealed class PointConverter : WriteOnlyConverter<Point>
	{
		public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("x", value.X);
			writer.WriteNumber("y", value.Y);
			writer.WriteEndObject();
		}
	}

	private sealed class PointFConverter : WriteOnlyConverter<PointF>
	{
		public override void Write(Utf8JsonWriter writer, PointF value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("x", value.X);
			writer.WriteNumber("y", value.Y);
			writer.WriteEndObject();
		}
	}

	private sealed class SizeConverter : WriteOnlyConverter<Size>
	{
		public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("width", value.Width);
			writer.WriteNumber("height", value.Height);
			writer.WriteEndObject();
		}
	}

	private sealed class SizeFConverter : WriteOnlyConverter<SizeF>
	{
		public override void Write(Utf8JsonWriter writer, SizeF value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("width", value.Width);
			writer.WriteNumber("height", value.Height);
			writer.WriteEndObject();
		}
	}

	private sealed class RectangleConverter : WriteOnlyConverter<Rectangle>
	{
		public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("x", value.X);
			writer.WriteNumber("y", value.Y);
			writer.WriteNumber("width", value.Width);
			writer.WriteNumber("height", value.Height);
			writer.WriteEndObject();
		}
	}

	private sealed class RectangleFConverter : WriteOnlyConverter<RectangleF>
	{
		public override void Write(Utf8JsonWriter writer, RectangleF value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("x", value.X);
			writer.WriteNumber("y", value.Y);
			writer.WriteNumber("width", value.Width);
			writer.WriteNumber("height", value.Height);
			writer.WriteEndObject();
		}
	}

	private sealed class ColorConverter : WriteOnlyConverter<Color>
	{
		public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
		{
			if (value.IsEmpty)
				writer.WriteNullValue();
			else
				writer.WriteStringValue(Hex(value));
		}
	}
}
