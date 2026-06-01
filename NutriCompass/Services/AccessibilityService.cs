using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace NutriCompass.Services;

public sealed class AccessibilityService
{
    private const double LargeFontScale = 1.35;
    private readonly Dictionary<Element, double> _originalFontSizes = new();
    public bool IsLargeFontEnabled { get; private set; }

    public void SetLargeFont(bool enabled, Page? currentPage)
    {
        if (IsLargeFontEnabled == enabled)
        {
            return;
        }

        IsLargeFontEnabled = enabled;
        ApplyFontScale(currentPage);
    }

    public void ApplyFontScale(Page? page)
    {
        if (page is null)
        {
            return;
        }

        foreach (var element in GetFontElements(page))
        {
            var property = GetFontSizeProperty(element);
            if (property is null)
            {
                continue;
            }

            var baseSize = _originalFontSizes.TryGetValue(element, out var stored)
                ? stored
                : (double)(property.GetValue(element) ?? 0);

            _originalFontSizes[element] = baseSize;

            var scaled = IsLargeFontEnabled
                ? Math.Ceiling(baseSize * LargeFontScale)
                : baseSize;

            property.SetValue(element, scaled);
        }
    }

    private static IEnumerable<Element> GetFontElements(Element root)
    {
        if (HasFontSize(root))
        {
            yield return root;
        }

        if (root is not IElementController controller)
        {
            yield break;
        }

        foreach (var child in controller.LogicalChildren.OfType<Element>())
        {
            foreach (var descendant in GetFontElements(child))
            {
                yield return descendant;
            }
        }
    }

    private static bool HasFontSize(Element element)
    {
        return GetFontSizeProperty(element) is not null;
    }

    private static PropertyInfo? GetFontSizeProperty(Element element)
    {
        return element.GetType().GetProperty("FontSize", BindingFlags.Public | BindingFlags.Instance);
    }
}
