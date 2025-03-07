using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExileCore;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using ImGuiNET;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace AreaStatVisual;

public class AreaStatVisual : BaseSettingsPlugin<AreaStatVisualSettings>
{
    private readonly TimeCache<Dictionary<string, Color>> _drawStringsCache;

    public AreaStatVisual()
    {
        Name = "Area Stat Visual";
        _drawStringsCache = new TimeCache<Dictionary<string, Color>>(BuildDrawStrings, 200);
    }

    public override void Render()
    {
        var drawStrings = _drawStringsCache.Value;
        DrawTextBoxes(drawStrings);
    }

    private void DrawTextBoxes(Dictionary<string, Color> drawStrings)
    {
        var windowRect = GameController.Window.GetWindowRectangle();
        var screenWidth = (int)windowRect.Width;
        var screenHeight = (int)windowRect.Height;

        var startX = screenWidth * (Settings.Display.Position.XPos.Value / 100f);
        var startY = screenHeight * (Settings.Display.Position.YPos.Value / 100f);

        var lines = new List<(string text, Color color, Vector2 position, Vector2 textSize)>();
        var currentY = startY;
        var minX = float.MaxValue;
        var maxX = float.MinValue;

        foreach (var (text, textColor) in drawStrings)
        {
            var textSize = Settings.Display.Visuals.UseCustomFont
                ? Graphics.MeasureText(text, Settings.Display.Visuals.CustomLoadedFont.Value)
                : Graphics.MeasureText(text);

            var x = startX;
            if (Settings.Display.Visuals.RightAlignedText.Value)
                x = startX - textSize.X;

            var position = new Vector2(x, currentY);
            lines.Add((text, textColor, position, textSize));

            if (x < minX)
                minX = x;
            if (x + textSize.X > maxX)
                maxX = x + textSize.X;

            currentY += textSize.Y + Settings.Display.Visuals.TextSpacing.Value;
        }

        if (lines.Count > 0)
            currentY -= Settings.Display.Visuals.TextSpacing.Value;

        var paddingValue = Settings.Display.Visuals.BorderPadding.Value;
        var boxRect = new RectangleF(minX - paddingValue, startY - paddingValue, maxX - minX + 2 * paddingValue, currentY - startY + 2 * paddingValue);

        Graphics.DrawBox(boxRect, Settings.Display.Visuals.BackgroundColor.Value, Settings.Display.Visuals.BorderRounding.Value);
        Graphics.DrawFrame(
            boxRect, Settings.Display.Visuals.BorderColor.Value, Settings.Display.Visuals.BorderRounding.Value, Settings.Display.Visuals.BorderThickness.Value,
            (int)ImDrawFlags.RoundCornersAll);

        foreach (var line in lines)
        {
            if (Settings.Display.Visuals.UseCustomFont)
                Graphics.DrawText(line.text, line.position, line.color, Settings.Display.Visuals.CustomLoadedFont.Value, FontAlign.Left);
            else
                Graphics.DrawText(line.text, line.position, line.color, FontAlign.Left);
        }
    }

    private Dictionary<string, Color> BuildDrawStrings()
    {
        var newDrawStrings = new Dictionary<string, Color>();

        foreach (var stat in Settings.AreaStats.Content)
        {
            if (!stat.Show.Value)
                continue;

            var regex = new Regex(stat.GameStatRegex.Value, RegexOptions.IgnoreCase);

            foreach (var instanceStat in GameController.IngameState.Data.MapStats)
            {
                var statKey = instanceStat.Key.ToString();

                if (regex.IsMatch(statKey))
                {
                    var displayString = !string.IsNullOrEmpty(stat.ReplacementString.Value) ? stat.ReplacementString.Value : statKey;
                    displayString += stat.ShowKeyValue.Value ? $": {instanceStat.Value}" : "";
                    newDrawStrings[displayString] = stat.TextColor.Value;
                }
            }
        }

        return newDrawStrings;
    }
}