namespace FoodDrinkApp.Services;

public static class AccessibilityService
{
    public static bool LargeTextEnabled { get; set; } = false;
    private static readonly Dictionary<int, double> _originalSizes = new();

    public static void ApplyFontScale(Element parent)
    {
        if (parent == null) return;
        TraverseAndScale(parent);
    }

    private static void TraverseAndScale(Element element)
    {
        ScaleElement(element);

        if (element is ContentPage page && page.Content != null)
            TraverseAndScale((Element)page.Content);
        else if (element is ScrollView scrollView && scrollView.Content != null)
            TraverseAndScale((Element)scrollView.Content);
        else if (element is ContentView contentView && contentView.Content != null)
            TraverseAndScale((Element)contentView.Content);
        else if (element is Layout layout)
        {
            foreach (var child in layout.Children)
                if (child is Element childElement)
                    TraverseAndScale(childElement);
        }
    }

    private static void ScaleElement(Element element)
    {
        if (element is Label label) ProcessFontSize(element, label.FontSize, size => label.FontSize = size);
        else if (element is Button button) ProcessFontSize(element, button.FontSize, size => button.FontSize = size);
        else if (element is Entry entry) ProcessFontSize(element, entry.FontSize, size => entry.FontSize = size);
        else if (element is Editor editor) ProcessFontSize(element, editor.FontSize, size => editor.FontSize = size);
        else if (element is SearchBar searchBar) ProcessFontSize(element, searchBar.FontSize, size => searchBar.FontSize = size);
    }

    private static void ProcessFontSize(Element element, double currentSize, Action<double> setSize)
    {
        int hash = element.GetHashCode();
        if (!_originalSizes.ContainsKey(hash))
        {
            _originalSizes[hash] = currentSize;
        }

        double baseSize = _originalSizes[hash];
        double newSize = LargeTextEnabled ? baseSize * 1.4 : baseSize;
        setSize(newSize);
    }
}