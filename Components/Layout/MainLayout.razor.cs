namespace Tesis.Components.Layout;

public partial class MainLayout
{
    [Inject] private UserSessionService SessionService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private void Logout()
    {
        SessionService.Logout();
        Navigation.NavigateTo("/login");
    }
}