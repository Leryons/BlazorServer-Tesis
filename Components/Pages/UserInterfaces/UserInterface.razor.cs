namespace Tesis.Components.Pages.UserInterfaces;

public partial class UserInterface
{
    public UserInterfaceSection SelectedSection { get; set; } = UserInterfaceSection.None;
    public DocumentType SelectedDocument { get; set; }
    [Inject] public UserSessionService userSessionService { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    
    public enum UserInterfaceSection
    {
        None,
        UploadSection,
        ShowcaseSection,
        SettingsSection,
        SearchSection,
    }

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("UserInterface component initialized.");
        await base.OnInitializedAsync();
    }

    private void SelectSection(UserInterfaceSection section)
    {
        SelectedSection = section;

        Console.WriteLine($"Selected section changed to: {section}");
        Console.WriteLine($"Current document mode: {SelectedDocument}");
        InvokeAsync(StateHasChanged);
    }

    private void Logout()
    {
        userSessionService.Logout();
        Navigation.NavigateTo("/", true);
    }
}
