using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;

namespace Files.App.Extensions;

public static class TitleBarPassthroughExtensions
{
	/// <summary>
	/// Titlebar passthrough FrameworkElements and tab items in TabView/
	/// </summary>
	/// <param name="window">its titlebar needs passthrough</param>
	/// <param name="frameworkElements">elements to passthrough</param>
	/// <param name="tabView">passthrough tab items in it</param>
	public static void TitleBarPassthrough(this Window window, ICollection<FrameworkElement> frameworkElements, TabView? tabView = null)
	{
		if (window.AppWindow is null)
			return;
		double scale = window.Content.XamlRoot.RasterizationScale;
		RectInt32[] rects = null!;

		if (tabView is not null && tabView.TabItems.Count > 0)
		{
			rects = new RectInt32[frameworkElements.Count + 1];
			TabViewItem firstTab = (TabViewItem) tabView.ContainerFromIndex(0);
			Point position = firstTab.TransformToVisual(window.Content).TransformPoint(new());
			Rect tabItemsRect = tabView.TransformToVisual(null).TransformBounds(new Rect(
				x: 0,
				y: 0,
				width: firstTab.ActualWidth * tabView.TabItems.Count
					   + (double) Application.Current.Resources["TabViewItemAddButtonWidth"]
					   + ((Thickness) Application.Current.Resources["TabViewItemAddButtonContainerPadding"]).Left
					   + ((Thickness) Application.Current.Resources["TabViewItemAddButtonContainerPadding"]).Right,
				height: firstTab.ActualHeight
			));
			rects[^1] = ScaledRectInt32(tabItemsRect, scale);
		}
		else
		{
			rects = new RectInt32[frameworkElements.Count];
		}
		int count = 0;
		foreach (var frameworkElement in frameworkElements)
		{
			rects[count] = window.GetScaledUIElementRectInt32(frameworkElement, scale);
			count++;
		}
		InputNonClientPointerSource
			.GetForWindowId(window.AppWindow.Id)
			.SetRegionRects(NonClientRegionKind.Passthrough, rects);
	}

	public static void ResetPassthrough(this Window window)
	{
		InputNonClientPointerSource
			.GetForWindowId(window.AppWindow.Id)
			.ClearRegionRects(NonClientRegionKind.Passthrough);
	}

	private static RectInt32 GetScaledUIElementRectInt32(this Window window, FrameworkElement uiElement, double scale)
	{
		Point position = uiElement.TransformToVisual(window.Content).TransformPoint(new());
		Rect rect = uiElement.TransformToVisual(null).TransformBounds(new Rect(
			x: 0,
			y: 0,
			width: uiElement.ActualWidth,
			height: uiElement.ActualHeight
		));
		return ScaledRectInt32(rect, scale);
	}

	private static RectInt32 ScaledRectInt32(Rect bounds, double scale)
	{
		return new(
			_X: (int) Math.Round(bounds.X * scale),
			_Y: (int) Math.Round(bounds.Y * scale),
			_Width: (int) Math.Round(bounds.Width * scale),
			_Height: (int) Math.Round(bounds.Height * scale)
		);
	}
}
