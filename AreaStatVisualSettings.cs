using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace AreaStatVisual;

public class AreaStatVisualSettings : ISettings
{
    public DisplaySettings Display { get; set; } = new();
    public ContentNode<CustomAreaStatSettings> AreaStats { get; set; } = new() { ItemFactory = () => new CustomAreaStatSettings() };
    public ToggleNode Enable { get; set; } = new(false);
}

[Submenu]
public class CustomAreaStatSettings
{
    public TextNode GameStatRegex { get; set; } = new("^$");

    public TextNode ReplacementString { get; set; } = new("");

    public ToggleNode ShowKeyValue { get; set; } = new(false);

    public ColorNode TextColor { get; set; } = new(Color.White);

    public ToggleNode Show { get; set; } = new(true);
}

[Submenu]
public class DisplaySettings
{
    public LocationPositionSettings Position { get; set; } = new();
    public LocationVisualsSettings Visuals { get; set; } = new();
}

[Submenu]
public class LocationPositionSettings
{
    public RangeNode<float> XPos { get; set; } = new(50, 0, 100);
    public RangeNode<float> YPos { get; set; } = new(50, 0, 100);
}

[Submenu]
public class LocationVisualsSettings
{
    public ToggleNode UseCustomFont { get; set; } = new(false);
    public TextNode CustomLoadedFont { get; set; } = new("default:13");
    public ToggleNode RightAlignedText { get; set; } = new(false);
    public RangeNode<int> TextSpacing { get; set; } = new(8, 0, 200);
    public RangeNode<float> BorderPadding { get; set; } = new(4, 1, 200);
    public RangeNode<int> BorderThickness { get; set; } = new(2, 1, 200);
    public RangeNode<float> BorderRounding { get; set; } = new(0, 0, 45);
    public ColorNode BorderColor { get; set; } = new(Color.White);
    public ColorNode BackgroundColor { get; set; } = new(Color.Black);
}