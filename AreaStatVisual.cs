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
        var screenWidth = windowRect.Width;
        var screenHeight = windowRect.Height;

        var anchorX = screenWidth * (Settings.Display.Position.XPos.Value / 100f);
        var anchorY = screenHeight * (Settings.Display.Position.YPos.Value / 100f);

        var measuredLines = new List<(string text, Color color, Vector2 size)>();
        foreach (var (text, textColor) in drawStrings)
        {
            var textSize = Settings.Display.Visuals.UseCustomFont
                ? Graphics.MeasureText(text, Settings.Display.Visuals.CustomLoadedFont.Value)
                : Graphics.MeasureText(text);
            measuredLines.Add((text, textColor, textSize));
        }

        var lines = new List<(string text, Color color, Vector2 position, Vector2 size)>();

        var currentY = anchorY;
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var (text, color, size) in measuredLines)
        {
            var textSize = size;
            var x = Settings.Display.Visuals.RightAlignedText.Value ? anchorX - textSize.X : anchorX;
            Vector2 pos;

            if (!Settings.Display.Visuals.DrawAscending.Value)
            {
                pos = new Vector2(x, currentY);
                currentY += textSize.Y + Settings.Display.Visuals.TextSpacing.Value;
            }
            else
            {
                currentY -= textSize.Y;
                pos = new Vector2(x, currentY);
                currentY -= Settings.Display.Visuals.TextSpacing.Value;
            }

            lines.Add((text, color, pos, textSize));

            if (x < minX)
                minX = x;
            if (x + textSize.X > maxX)
                maxX = x + textSize.X;
            if (pos.Y < minY)
                minY = pos.Y;
            if (pos.Y + textSize.Y > maxY)
                maxY = pos.Y + textSize.Y;
        }

        var pad = Settings.Display.Visuals.BorderPadding.Value;
        var boxRect = new RectangleF(minX - pad, minY - pad, maxX - minX + 2 * pad, maxY - minY + 2 * pad);

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
                    var displayString = !string.IsNullOrEmpty(stat.ReplacementString.Value)
                        ? stat.ReplacementString.Value
                        : Regex.Replace(statKey, "(?<!^)([A-Z])", " $1");

                    displayString += stat.ShowKeyValue.Value ? $": {instanceStat.Value}" : "";
                    newDrawStrings[displayString] = stat.TextColor.Value;
                }
            }
        }

        return newDrawStrings;
    }
}