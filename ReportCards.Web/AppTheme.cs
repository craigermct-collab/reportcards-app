using MudBlazor;

namespace ReportCards.Web;

public static class AppTheme
{
    public static MudTheme Theme => new()
    {
        PaletteLight = new PaletteLight
        {
            // ── Core ──────────────────────────────────────────
            Primary             = "#1B4F72",
            PrimaryDarken       = "#154060",
            PrimaryLighten      = "#2E86C1",
            PrimaryContrastText = "#FFFFFF",

            Secondary             = "#E67E22",
            SecondaryDarken       = "#CA6F1E",
            SecondaryLighten      = "#FAD7A0",
            SecondaryContrastText = "#FFFFFF",

            // ── Semantic ──────────────────────────────────────
            Success = "#1E8449",
            Warning = "#D4AC0D",
            Error   = "#C0392B",
            Info    = "#2E86C1",

            // ── Surface / Background ──────────────────────────
            Background       = "#F4F6F9",
            Surface          = "#FFFFFF",
            DrawerBackground = "#1B4F72",
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
            ActionDefault           = "#5D7285",
            ActionDisabled          = "#9BADB9",
            ActionDisabledBackground = "#F4F6F9",

            // ── Hover / overlay ───────────────────────────────
            HoverOpacity = 0.06,
            RippleOpacity = 0.08,
            OverlayDark  = "rgba(27,79,114,0.5)",
            OverlayLight = "rgba(244,246,249,0.8)",

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
