namespace KSharpPlus.Entities.Color;

public readonly partial struct KuracordColor {
    #region Black and White

    /// <summary>
    /// Represents no color, or integer 0;
    /// </summary>
    public static KuracordColor None { get; } = new(0);

    /// <summary>
    /// A near-black color. Due to API limitations, the color is #010101, rather than #000000, as the latter is treated as
    /// no color.
    /// </summary>
    public static KuracordColor Black { get; } = new(0x010101);

    /// <summary>
    /// White, or #FFFFFF.
    /// </summary>
    public static KuracordColor White { get; } = new(0xFFFFFF);

    /// <summary>
    /// Gray, or #808080.
    /// </summary>
    public static KuracordColor Gray { get; } = new(0x808080);

    /// <summary>
    /// Dark gray, or #A9A9A9.
    /// </summary>
    public static KuracordColor DarkGray { get; } = new(0xA9A9A9);

    /// <summary>
    /// Light gray, or #808080.
    /// </summary>
    public static KuracordColor LightGray { get; } = new(0xD3D3D3);

    /// <summary>
    /// Very dark gray, or #666666.
    /// </summary>
    public static KuracordColor VeryDarkGray { get; } = new(0x666666);

    #endregion

    #region Dicksord branding colors

    /// <summary>
    /// Dicksord Blurple, or #7289DA.
    /// </summary>
    public static KuracordColor Blurple { get; } = new(0x7289DA);

    /// <summary>
    /// Dicksord Grayple, or #99AAB5.
    /// </summary>
    public static KuracordColor Grayple { get; } = new(0x99AAB5);

    /// <summary>
    /// Dicksord Dark, But Not Black, or #2C2F33.
    /// </summary>
    public static KuracordColor DarkButNotBlack { get; } = new(0x2C2F33);

    /// <summary>
    /// Dicksord Not QuiteBlack, or #23272A.
    /// </summary>
    public static KuracordColor NotQuiteBlack { get; } = new(0x23272A);

    #endregion

    #region Other colors

    /// <summary>
    /// Red, or #FF0000.
    /// </summary>
    public static KuracordColor Red { get; } = new(0xFF0000);

    /// <summary>
    /// Dark red, or #7F0000.
    /// </summary>
    public static KuracordColor DarkRed { get; } = new(0x7F0000);

    /// <summary>
    /// Green, or #00FF00.
    /// </summary>
    public static KuracordColor Green { get; } = new(0x00FF00);

    /// <summary>
    /// Dark green, or #007F00.
    /// </summary>
    public static KuracordColor DarkGreen { get; } = new(0x007F00);

    /// <summary>
    /// Blue, or #0000FF.
    /// </summary>
    public static KuracordColor Blue { get; } = new(0x0000FF);

    /// <summary>
    /// Dark blue, or #00007F.
    /// </summary>
    public static KuracordColor DarkBlue { get; } = new(0x00007F);

    /// <summary>
    /// Yellow, or #FFFF00.
    /// </summary>
    public static KuracordColor Yellow { get; } = new(0xFFFF00);

    /// <summary>
    /// Cyan, or #00FFFF.
    /// </summary>
    public static KuracordColor Cyan { get; } = new(0x00FFFF);

    /// <summary>
    /// Magenta, or #FF00FF.
    /// </summary>
    public static KuracordColor Magenta { get; } = new(0xFF00FF);

    /// <summary>
    /// Teal, or #008080.
    /// </summary>
    public static KuracordColor Teal { get; } = new(0x008080);

    /// <summary>
    /// Aquamarine, or #00FFBF.
    /// </summary>
    public static KuracordColor Aquamarine { get; } = new(0x00FFBF);

    /// <summary>
    /// Gold, or #FFD700.
    /// </summary>
    public static KuracordColor Gold { get; } = new(0xFFD700);

    /// <summary>
    /// Goldenrod, or #DAA520.
    /// </summary>
    public static KuracordColor Goldenrod { get; } = new(0xDAA520);

    /// <summary>
    /// Azure, or #007FFF.
    /// </summary>
    public static KuracordColor Azure { get; } = new(0x007FFF);

    /// <summary>
    /// Rose, or #FF007F.
    /// </summary>
    public static KuracordColor Rose { get; } = new(0xFF007F);

    /// <summary>
    /// Spring green, or #00FF7F.
    /// </summary>
    public static KuracordColor SpringGreen { get; } = new(0x00FF7F);

    /// <summary>
    /// Chartreuse, or #7FFF00.
    /// </summary>
    public static KuracordColor Chartreuse { get; } = new(0x7FFF00);

    /// <summary>
    /// Orange, or #FFA500.
    /// </summary>
    public static KuracordColor Orange { get; } = new(0xFFA500);

    /// <summary>
    /// Purple, or #800080.
    /// </summary>
    public static KuracordColor Purple { get; } = new(0x800080);

    /// <summary>
    /// Violet, or #EE82EE.
    /// </summary>
    public static KuracordColor Violet { get; } = new(0xEE82EE);

    /// <summary>
    /// Brown, or #A52A2A.
    /// </summary>
    public static KuracordColor Brown { get; } = new(0xA52A2A);

    /// <summary>
    /// Hot pink, or #FF69B4
    /// </summary>
    public static KuracordColor HotPink { get; } = new(0xFF69B4);

    /// <summary>
    /// Lilac, or #C8A2C8.
    /// </summary>
    public static KuracordColor Lilac { get; } = new(0xC8A2C8);

    /// <summary>
    /// Cornflower blue, or #6495ED.
    /// </summary>
    public static KuracordColor CornflowerBlue { get; } = new(0x6495ED);

    /// <summary>
    /// Midnight blue, or #191970.
    /// </summary>
    public static KuracordColor MidnightBlue { get; } = new(0x191970);

    /// <summary>
    /// Wheat, or #F5DEB3.
    /// </summary>
    public static KuracordColor Wheat { get; } = new(0xF5DEB3);

    /// <summary>
    /// Indian red, or #CD5C5C.
    /// </summary>
    public static KuracordColor IndianRed { get; } = new(0xCD5C5C);

    /// <summary>
    /// Turquoise, or #30D5C8.
    /// </summary>
    public static KuracordColor Turquoise { get; } = new(0x30D5C8);

    /// <summary>
    /// Sap green, or #507D2A.
    /// </summary>
    public static KuracordColor SapGreen { get; } = new(0x507D2A);

    /// <summary>
    /// Phthalo blue, or #000F89.
    /// </summary>
    public static KuracordColor PhthaloBlue { get; } = new(0x000F89);
    
    /// <summary>
    /// Phthalo green, or #123524.
    /// </summary>
    public static KuracordColor PhthaloGreen { get; } = new(0x123524);

    /// <summary>
    /// Sienna, or #882D17.
    /// </summary>
    public static KuracordColor Sienna { get; } = new(0x882D17);

    #endregion
}