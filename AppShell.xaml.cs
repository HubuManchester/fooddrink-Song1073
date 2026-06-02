using FoodDrinkApp.Views;

namespace FoodDrinkApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register navigation routes for application subpages to enable Shell.Current.GoToAsync navigation
        Routing.RegisterRoute("AddItemPage", typeof(AddItemPage));
        Routing.RegisterRoute("FoodDetailPage", typeof(FoodDetailPage));
        Routing.RegisterRoute("EditProfilePage", typeof(EditProfilePage));
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        // Reset the navigation stack to the root page when switching tabs.
        // This ensures that when returning to the Home or Profile tabs, the root page is shown
        // instead of remaining stuck on sub-pages (like FoodDetail or AddItem).
        // It also ensures that any font size changes from Settings take effect correctly 
        // as the pages will be re-rendered or trigger OnAppearing normally.
        if (args.Source == ShellNavigationSource.ShellSectionChanged)
        {
            if (Current.Navigation.NavigationStack.Count > 1)
            {
                // Pop to root without animation so it instantly feels like a fresh tab switch
                Current.Navigation.PopToRootAsync(animated: false);
            }
        }
    }
}