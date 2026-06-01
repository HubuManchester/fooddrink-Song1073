using FoodDrinkApp.Views;

namespace FoodDrinkApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // 注册我们应用的子页面路由，这样才能使用 Shell.Current.GoToAsync 正常跳转
        Routing.RegisterRoute("AddItemPage", typeof(AddItemPage));
        Routing.RegisterRoute("FoodDetailPage", typeof(FoodDetailPage));
    }
}