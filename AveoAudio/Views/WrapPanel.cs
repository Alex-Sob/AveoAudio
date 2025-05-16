using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System;

using Windows.Foundation;

namespace AveoAudio.Views;

public partial class WrapPanel : Panel
{
    /// <summary>
    /// Gets or sets the distance between the border and its child object.
    /// </summary>
    /// <returns>
    /// The dimensions of the space between the border and its child as a Thickness value.
    /// Thickness is a structure that stores dimension values using pixel measures.
    /// </returns>
    public Thickness Padding
    {
        get { return (Thickness)GetValue(PaddingProperty); }
        set { SetValue(PaddingProperty, value); }
    }

    /// <summary>
    /// Identifies the Padding dependency property.
    /// </summary>
    /// <returns>The identifier for the <see cref="Padding"/> dependency property.</returns>
    public static readonly DependencyProperty PaddingProperty =
        DependencyProperty.Register(
            nameof(Padding),
            typeof(Thickness),
            typeof(WrapPanel),
            new PropertyMetadata(default(Thickness), LayoutPropertyChanged));

    /// <summary>
    /// Gets or sets a uniform Vertical distance (in pixels) between items when <see cref="Orientation"/> is set to Vertical,
    /// or between rows of items when <see cref="Orientation"/> is set to Horizontal.
    /// </summary>
    public double VerticalSpacing
    {
        get { return (double)GetValue(VerticalSpacingProperty); }
        set { SetValue(VerticalSpacingProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="VerticalSpacing"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(
            nameof(VerticalSpacing),
            typeof(double),
            typeof(WrapPanel),
            new PropertyMetadata(0d, LayoutPropertyChanged));

    private static void LayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WrapPanel wp)
        {
            wp.InvalidateMeasure();
            wp.InvalidateArrange();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double curRowWidth = 0, maxRowWidth = 0, curRowHeight = 0, totalHeight = 0;

        var childAvailableSize = new Size(
            availableSize.Width - Padding.Left - Padding.Right,
            availableSize.Height - Padding.Top - Padding.Bottom);

        foreach (var child in Children)
        {
            child.Measure(childAvailableSize);

            if (curRowWidth + child.DesiredSize.Width <= availableSize.Width)
            {
                curRowWidth += child.DesiredSize.Width;
                curRowHeight = Math.Max(curRowHeight, child.DesiredSize.Height);
            }
            else
            {
                curRowWidth = child.DesiredSize.Width;
                totalHeight += curRowHeight + this.VerticalSpacing;
                curRowHeight = child.DesiredSize.Height;
            }

            if (curRowWidth > maxRowWidth) maxRowWidth = curRowWidth;
        }

        totalHeight += curRowHeight;

        return new Size(maxRowWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double curX = 0, curY = 0, curRowWidth = 0, curRowHeight = 0;

        foreach (var child in Children)
        {
            if (curRowWidth + child.DesiredSize.Width <= finalSize.Width)
            {
                curRowWidth += child.DesiredSize.Width;
                curRowHeight = Math.Max(curRowHeight, child.DesiredSize.Height);
            }
            else
            {
                curX = 0;
                curY += curRowHeight + this.VerticalSpacing;

                curRowWidth = child.DesiredSize.Width;
                curRowHeight = child.DesiredSize.Height;
            }

            var topLeft = new Point(curX, curY);
            child.Arrange(new Rect(topLeft, child.DesiredSize));

            curX = curRowWidth;
        }

        return finalSize;
    }
}