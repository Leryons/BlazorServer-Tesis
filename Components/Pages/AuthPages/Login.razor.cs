namespace Tesis.Components.Pages.AuthPages;

public partial class Login : IDisposable
{
    [Inject] private UserServices UserServices { get; set; } = null!;
    [Inject] private ESPCom SerialService { get; set; } = null!;
    [Inject] private UserSessionService SessionService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private string Email { get; set; } = "";
    private string Password { get; set; } = "";
    private string statusMessage = "Esperando acción...";
    public User? currentUser;

    protected override void OnInitialized()
    {
        if (SerialService.isOnline())
        {
            SerialService.OnMessageReceived += HandleMessage;
        }
    }

    private void LoginUsuario()
    {
        currentUser = UserServices.GetUserByEmail(Email);

        if (currentUser == null || currentUser.Password != Password)
        {
            statusMessage = "Credenciales incorrectas.";
            return;
        }

        statusMessage = "Contraseña correcta. Coloque el dedo en el sensor...";
        Task.Delay(2000).ContinueWith(_ => SerialService.SendCommand("VERIFY_FINGER"));
    }

    private async void HandleMessage(string msg)
    {
        if (msg.StartsWith("MATCH"))
        {
            string fingerId = msg.Split(':')[1];
            if (currentUser != null && currentUser.FingerId.ToString() == fingerId)
            {
                statusMessage = $"¡Bienvenido {currentUser.Name}!";
                SessionService.Login(currentUser);
                
                await InvokeAsync(StateHasChanged);
                await Task.Delay(1500);
                Navigation.NavigateTo("/home");
                Navigation.Refresh();
                return;
            }
            else
            {
                statusMessage = "Huella no coincide.";
            }
        }
        else if (msg == "NO_MATCH")
        {
            statusMessage = "Huella no reconocida.";
        }
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        SerialService.OnMessageReceived -= HandleMessage;
    }
}