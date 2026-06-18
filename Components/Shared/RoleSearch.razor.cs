namespace Tesis.Components.Shared;

public partial class RoleSearch
{
    [Inject] private ESPCom EspCom { get; set; } = null!;
    [Inject] private UserServices userServices { get; set; } = null!;
    [Inject] private UserSessionService userSessionService { get; set; } = null!;

    private bool isReading = false;
    private string statusMessage = "Presione 'Leer RFID' y acerque el anillo al lector.";
    private List<Document>? documents;

    private async Task<string?> ReadRfidAsync()
    {
        var tcs = new TaskCompletionSource<string?>();
        Action<string> handler = null!;
        handler = (message) =>
        {
            if (message.StartsWith("MATCH_RFID:"))
            {
                EspCom.OnMessageReceived -= handler;
                tcs.SetResult(message.Replace("MATCH_RFID:", ""));
            }
            else if (message == "TIMEOUT_RFID")
            {
                EspCom.OnMessageReceived -= handler;
                tcs.SetResult(null);
            }
        };

        EspCom.OnMessageReceived += handler;
        EspCom.SendCommand("READ_RFID");

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(6000));
        if (completedTask == tcs.Task) return await tcs.Task;
        EspCom.OnMessageReceived -= handler;
        return null;
    }

    private async Task BeginRead()
    {
        if (isReading) return;
        isReading = true;
        statusMessage = "Acerque el anillo al lector...";
        documents = null;
        StateHasChanged();

        try
        {
            var rfid = await ReadRfidAsync();
            if (string.IsNullOrEmpty(rfid))
            {
                statusMessage = "No se detectó anillo. Intente de nuevo.";
                return;
            }

            statusMessage = $"RFID detectado: {rfid}. Recuperando documentos...";

            var allDocs = await userServices.GetAllDocumentsForRfidUi(rfid);

            var allowed = DocumentAccessConfig.GetAllowedTypes(userSessionService.User?.Role ?? UserRole.User);

            documents = allDocs.Where(d => allowed.Contains(d.Type)).ToList();

            if (documents.Count == 0)
            {
                statusMessage = "No hay documentos accesibles para este RFID con su rol.";
            }
            else
            {
                statusMessage = $"Se encontraron {documents.Count} documentos.";
            }
        }
        catch (Exception ex)
        {
            statusMessage = "Error técnico: " + ex.Message;
        }
        finally
        {
            isReading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
