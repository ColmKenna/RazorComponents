namespace RazorComponents.TagHelpers.Tabs;

/// <summary>
/// Manages the state and communication between a TabTagHelper parent and its TabItemTagHelper children.
/// This context is stored in TagHelperContext.Items to enable parent-child communication without brittle HTML parsing.
/// </summary>
public class TabContext
{
    private readonly List<TabItemInfo> _tabs = new();

    /// <summary>
    /// Gets the list of registered tab items.
    /// </summary>
    public IReadOnlyList<TabItemInfo> Tabs => _tabs.AsReadOnly();

    /// <summary>
    /// Registers a tab item with the parent tab container.
    /// </summary>
    /// <param name="tabInfo">Information about the tab item being registered.</param>
    public void RegisterTab(TabItemInfo tabInfo)
    {
        _tabs.Add(tabInfo);
    }

    /// <summary>
    /// Determines which tab should be selected.
    /// If a tab is explicitly marked as selected, uses that.
    /// Otherwise, selects the first tab.
    /// </summary>
    /// <returns>The ID of the tab that should be selected, or null if no tabs are registered.</returns>
    public string? GetSelectedTabId()
    {
        if (_tabs.Count == 0)
            return null;

        // Check if any tab is explicitly selected
        var selectedTab = _tabs.FirstOrDefault(t => t.IsSelected);

        // If no tab is selected, select the first one
        return selectedTab?.Id ?? _tabs[0].Id;
    }
}

/// <summary>
/// Information about a registered tab item.
/// </summary>
public class TabItemInfo
{
    /// <summary>
    /// Gets or sets the ID of the tab item.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets whether this tab is explicitly marked as selected.
    /// </summary>
    public bool IsSelected { get; init; }
}
