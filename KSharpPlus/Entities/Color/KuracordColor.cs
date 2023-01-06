using System.Globalization;

namespace KSharpPlus.Entities.Color;

public readonly partial struct KuracordColor {
    static readonly char[] HexAlphabet = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

    /// <summary>
    /// Gets the integer representation of this color.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Gets the red component of this color as an 8-bit integer.
    /// </summary>
    public byte R => (byte)(Value >> 16 & 0xFF);

    /// <summary>
    /// Gets the green component of this color as an 8-bit integer.
    /// </summary>
    public byte G => (byte)(Value >> 8 & 0xFF);

    /// <summary>
    /// Gets the blue component of this color as an 8-bit integer.
    /// </summary>
    public byte B => (byte)(Value & 0xFF);

    /// <summary>
    /// Creates a new color with specified value.
    /// </summary>
    /// <param name="color">Value of the color.</param>
    public KuracordColor(int color) => Value = color;

    /// <summary>
    /// Creates a new color with specified values for red, green, and blue components.
    /// </summary>
    /// <param name="r">Value of the red component.</param>
    /// <param name="g">Value of the green component.</param>
    /// <param name="b">Value of the blue component.</param>
    public KuracordColor(byte r, byte g, byte b) => Value = r << 16 | g << 8 | b;

    /// <summary>
    /// Creates a new color with specified values for red, green, and blue components.
    /// </summary>
    /// <param name="r">Value of the red component.</param>
    /// <param name="g">Value of the green component.</param>
    /// <param name="b">Value of the blue component.</param>
    public KuracordColor(float r, float g, float b) {
        if (r is < 0 or > 1 || g is < 0 or > 1 || b is < 0 or > 1)
            throw new ArgumentOutOfRangeException($"{r}; {g}; {b}", "Each component must be between 0.0 and 1.0 inclusive.");

        byte rb = (byte)(r * 255);
        byte gb = (byte)(g * 255);
        byte bb = (byte)(b * 255);

        Value = rb << 16 | gb << 8 | bb;
    }

    /// <summary>
    /// Creates a new color from specified string representation.
    /// </summary>
    /// <param name="color">String representation of the color. Must be 6 hexadecimal characters, optionally with # prefix.</param>
    public KuracordColor(string color) {
        if (string.IsNullOrWhiteSpace(color))
            throw new ArgumentNullException(nameof(color), "Null or empty values are not allowed!");

        if (color.Length != 6 && color.Length != 7)
            throw new ArgumentException("Color must be 6 or 7 characters in length.", nameof(color));

        color = color.Length switch {
            7 when color[0] != '#' => throw new ArgumentException("7-character colors must begin with #.", nameof(color)),
            7 => color[1..],
            _ => color.ToUpper()
        };

        if (color.Any(xc => !HexAlphabet.Contains(xc)))
            throw new ArgumentException("Colors must consist of hexadecimal characters only.", nameof(color));

        Value = int.Parse(color, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets a string representation of this color.
    /// </summary>
    /// <returns>String representation of this color.</returns>
    public override string ToString() => $"#{Value:X6}";

    public static implicit operator KuracordColor(int value) => new(value);
}