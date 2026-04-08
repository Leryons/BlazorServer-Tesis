namespace Tesis.Components.Pages.AuthPages;

public partial class SignUp : IDisposable
{
    [Inject] private UserServices UserServices { get; set; } = null!;
    [Inject] private ESPCom SerialService { get; set; } = null!;
    [Inject] private UserSessionService SessionService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private User newUser = new User();
    private string ConfirmPassword { get; set; } = "";
    private string statusMessage = "Esperando acción...";
    private int nextFingerId;
    private bool registroEnProceso = false;
    private bool waitingForSensor = false;

    protected override void OnInitialized()
    {
        if (SerialService.isOnline())
        {
            SerialService.OnMessageReceived += HandleMessage;
        }
    }

    private void RegisterUser()
    {
        if (newUser.Password != ConfirmPassword)
        {
            statusMessage = "Las contraseñas no coinciden.";
            return;
        }

        if (string.IsNullOrWhiteSpace(newUser.Email) || string.IsNullOrWhiteSpace(newUser.Name))
        {
            statusMessage = "Por favor, complete todos los campos.";
            return;
        }

        nextFingerId = UserServices.GetNextFingerId();
        waitingForSensor = true;
        registroEnProceso = false;
        statusMessage = "Coloque el dedo en el sensor para registrar su huella...";
        StateHasChanged();

        SerialService.SendCommand($"REGISTER_FINGER:{nextFingerId}");

        Task.Delay(2000).ContinueWith(async _ => {
            if (waitingForSensor)
            {
                statusMessage = "Retire el dedo, y coloque nuevamente para confirmar.";
                await InvokeAsync(StateHasChanged);
            }
        });
    }

    private async void HandleMessage(string msg)
    {
        Console.WriteLine("Mensaje recibido en Blazor: " + msg);
        if (msg.StartsWith("SUCCESS") && waitingForSensor && !registroEnProceso)
        {
            // Proceed to save user after successful capture
            registroEnProceso = true;
            waitingForSensor = false;

            try
            {
                var insertedId = await UserServices.RegisterUser(newUser, nextFingerId);
                newUser.Id = insertedId;

                statusMessage = "¡Usuario y huella registrados con éxito!";

                SessionService.Login(newUser);

                await InvokeAsync(StateHasChanged);
                await Task.Delay(2500);

                Navigation.NavigateTo("/home");
            }
            catch (Exception ex)
            {
                statusMessage = "Error al guardar en base de datos: " + ex.Message;
            }
            finally
            {
                registroEnProceso = false;
            }
        }
        else if (msg == "FAILED")
        {
            statusMessage = "El sensor no pudo capturar la huella. Intente de nuevo.";
        }

        await InvokeAsync(StateHasChanged);
    }

    private bool IsFormValid()
    {
        return !string.IsNullOrWhiteSpace(newUser.Name)
            && !string.IsNullOrWhiteSpace(newUser.Email)
            && !string.IsNullOrWhiteSpace(newUser.Password)
            && newUser.Password == ConfirmPassword
            && newUser.Email.Contains("@");
    }

    public void Dispose()
    {
        SerialService.OnMessageReceived -= HandleMessage;
    }
}