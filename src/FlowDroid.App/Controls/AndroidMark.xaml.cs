using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowDroid.Controls;

public partial class AndroidMark : UserControl
{
    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill), typeof(Brush), typeof(AndroidMark), new PropertyMetadata(null));

    public static readonly DependencyProperty EyeBrushProperty = DependencyProperty.Register(
        nameof(EyeBrush), typeof(Brush), typeof(AndroidMark), new PropertyMetadata(null));

    public AndroidMark()
    {
        InitializeComponent();
    }

    /// <summary>Colour of the dome and antennas.</summary>
    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>Colour of the eyes (normally the surface behind the mark).</summary>
    public Brush? EyeBrush
    {
        get => (Brush?)GetValue(EyeBrushProperty);
        set => SetValue(EyeBrushProperty, value);
    }
}
