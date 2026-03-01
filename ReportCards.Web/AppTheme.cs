using MudBlazor;

namespace ReportCards.Web;

public static class AppTheme
{
    /// <summary>
    /// Returns a MudTheme using the provided colour overrides.
    /// Falls back to the KinderKollege defaults if any value is null/empty.
    /// </summary>
    public static MudTheme Build(
        string? primary   = null,
        string? secondary = null,
        string? navDark   = null)
    {
        var p  = NullOrEmpty(primary)   ? "#1B4F72" : primary!;
        var s  = NullOrEmpty(secondary) ? "#E67E22" : secondary!;
        var nav = NullOrEmpty(navDark)  ? "#1B4F72" : navDark!;

        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                // ── Core ──────────────────────────────────────────
                Primary             = p,
                PrimaryDarken       = Darken(p),
                PrimaryLighten      = Lighten(p),
                PrimaryContrastText = "#FFFFFF",

                Secondary             = s,
                SecondaryDarken       = Darken(s),
                SecondaryLighten      = Lighten(s),
                SecondaryContrastText = "#FFFFFF",

                // ── Semantic ──────────────────────────────────────
                Success = "#1E8449",
                Warning = "#D4AC0D",
                Error   = "#C0392B",
                Info    = "#2E86C1",

                // ── Surface / Background ──────────────────────────
                Background       = "#F4F6F9",
                Surface          = "#FFFFFF",
                DrawerBackground = nav,
                DrawerText       = "rgba(255,255,255,0.85)",
                DrawerIcon       = "rgba(255,255,255,0.65)",

                AppbarBackground = "#FFFFFF",
                AppbarText       = "#1A2B3C",

                // ── Text ──────────────────────────────────────────
                TextPrimary   = "#1A2B3C",
                TextSecondary = "#5D7285",
                TextDisabled  = "#9BADB9",

                // ── Lines ─────────────────────────────────────────
                Divider    = "#DDE3EC",
                TableLines = "#DDE3EC",

                // ── Action states ─────────────────────────────────
                ActionDefault            = "#5D7285",
                ActionDisabled           = "#9BADB9",
                ActionDisabledBackground = "#F4F6F9",

                // ── Hover / overlay ───────────────────────────────
                HoverOpacity  = 0.06,
                RippleOpacity = 0.08,
                OverlayDark   = "rgba(27,79,114,0.5)",
                OverlayLight  = "rgba(244,246,249,0.8)",

                LinesDefault = "#DDE3EC",
                LinesInputs  = "#DDE3EC",
            },

            Typography = new Typography
            {
                Default = new Default
                {
                    FontFamily = new[] { "DM Sans", "Segoe UI", "sans-serif" },
                    FontSize   = ".875rem",
                    FontWeight = 400,
                    LineHeight = 1.5,
                },
                H1 = new H1
                {
                    FontFamily = new[] { "DM Serif Display", "Georgia", "serif" },
                    FontSize   = "2rem",
                    FontWeight = 400,
                    LineHeight = 1.2,
                },
                H2 = new H2
                {
                    FontFamily = new[] { "DM Serif Display", "Georgia", "serif" },
                    FontSize   = "1.6rem",
                    FontWeight = 400,
                    LineHeight = 1.25,
                },
                H3 = new H3
                {
                    FontFamily = new[] { "DM Sans", "Segoe UI", "sans-serif" },
                    FontSize   = "1.25rem",
                    FontWeight = 700,
                    LineHeight = 1.3,
                },
                H4 = new H4
                {
                    FontFamily = new[] { "DM Sans", "Segoe UI", "sans-serif" },
                    FontSize   = "1.1rem",
                    FontWeight = 700,
                    LineHeight = 1.35,
                },
                H5 = new H5
                {
                    FontFamily = new[] { "DM Sans", "Segoe UI", "sans-serif" },
                    FontSize   = ".95rem",
                    FontWeight = 600,
                },
                H6 = new H6
                {
                    FontFamily = new[] { "DM Sans", "Segoe UI", "sans-serif" },
                    FontSize   = ".85rem",
                    FontWeight = 600,
                },
                Button = new Button
                {
                    FontFamily    = new[] { "DM Sans", "Segoe UI", "sans-serif" },
                    FontSize      = ".8rem",
                    FontWeight    = 600,
                    TextTransform = "none",
                },
                Caption = new Caption
                {
                    FontFamily = new[] { "DM Sans", "Segoe UI", "sans-serif" },
                    FontSize   = ".72rem",
                    FontWeight = 500,
                },
                Overline = new Overline
                {
                    FontFamily    = new[] { "DM Sans", "Segoe UI", "sans-serif" },
                    FontSize      = ".65rem",
                    FontWeight    = 700,
                    LetterSpacing = ".08em",
                    TextTransform = "uppercase",
                },
            },

            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "8px",
                DrawerWidthLeft     = "240px",
                DrawerMiniWidthLeft = "56px",
                AppbarHeight        = "60px",
            },

            ZIndex = new ZIndex
            {
                Drawer   = 1100,
                AppBar   = 1200,
                Dialog   = 1300,
                Snackbar = 1400,
                Tooltip  = 1500,
            }
        };
    }

    // ── Colour helpers ────────────────────────────────────────────

    private static bool NullOrEmpty(string? s) => string.IsNullOrWhiteSpace(s);

    /// <summary>Darken a hex colour by ~15%.</summary>
    private static string Darken(string hex)
    {
        if (!TryParseHex(hex, out var r, out var g, out var b)) return hex;
        return ToHex((int)(r * 0.85), (int)(g * 0.85), (int)(b * 0.85));
    }

    /// <summary>Lighten a hex colour by blending 40% toward white.</summary>
    private static string Lighten(string hex)
    {
        if (!TryParseHex(hex, out var r, out var g, out var b)) return hex;
        return ToHex(r + (int)((255 - r) * 0.4), g + (int)((255 - g) * 0.4), b + (int)((255 - b) * 0.4));
    }

    private static bool TryParseHex(string hex, out int r, out int g, out int b)
    {
        r = g = b = 0;
        hex = hex.TrimStart('#');
        if (hex.Length != 6) return false;
        try
        {
            r = Convert.ToInt32(hex[..2], 16);
            g = Convert.ToInt32(hex[2..4], 16);
            b = Convert.ToInt32(hex[4..6], 16);
            return true;
        }
        catch { return false; }
    }

    private static string ToHex(int r, int g, int b) =>
        $"#{Math.Clamp(r, 0, 255):X2}{Math.Clamp(g, 0, 255):X2}{Math.Clamp(b, 0, 255):X2}";
}
