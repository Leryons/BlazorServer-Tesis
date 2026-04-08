namespace Tesis.Components.Shared;

public partial class NfcConfig
{
    [Inject] private ESPCom _espCom { get; set; } = null!;
    [Inject] private UserSessionService userSessionService { get; set; } = null!;
    [Inject] private UserServices userServices { get; set; } = null!;

    public string statusMessage = "Esperando para iniciar la configuración RFID...";
    public string statusClass = "info";
    public bool isProcessing = false;
    public bool isProcessingRfid = false;

    public async Task<string?> ReadRfidAsync()
    {
        var tcs = new TaskCompletionSource<string?>();
        
        Action<string> handler = null!;
        handler = (message) =>
        {
            if (message.StartsWith("MATCH_RFID:"))
            {
                _espCom.OnMessageReceived -= handler;
                tcs.SetResult(message.Replace("MATCH_RFID:", ""));
            }
            else if (message == "TIMEOUT_RFID")
            {
                _espCom.OnMessageReceived -= handler;
                tcs.SetResult(null);
            }
        };

        _espCom.OnMessageReceived += handler;
        _espCom.SendCommand("READ_RFID");

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(6000));
        
        if (completedTask == tcs.Task)
        {
            return await tcs.Task;
        }
        else
        {
            _espCom.OnMessageReceived -= handler;
            return null;
        }
    } 

    private async Task VinculateRfid()
    {
        if (isProcessing) return;

        isProcessing = true;
        isProcessingRfid = true;
        statusMessage = "Acerque su anillo al lector...";
        statusClass = "info";

        try
        {
            string? rfidUid = await ReadRfidAsync();

            if(!string.IsNullOrEmpty(rfidUid))
            {
                if(userSessionService.User == null)
                {
                    statusMessage = "Error: Usuario no autenticado.";
                    statusClass = "error";
                    return;
                }
                
                userSessionService.User.RfidUid = rfidUid;
                var success = await userServices.UpdateUserRfidUid(userSessionService.User.Id, rfidUid);
                
                if (success)
                {
                    statusMessage = $"¡Anillo vinculado correctamente! (ID: {rfidUid})";
                    statusClass = "success";
                }
                else
                {
                    statusMessage = "Este anillo ya está asignado a otro usuario. Use un anillo diferente.";
                    statusClass = "error";
                    userSessionService.User.RfidUid = string.Empty; // Revertir en memoria
                }
            }
            else
            {
                statusMessage = "No se detectó ningún anillo. Intente nuevamente.";
                statusClass = "error";
            }
        }
        catch (Exception ex)
        {
            statusMessage = $"Error técnico: {ex.Message}";
            statusClass = "error";
        }
        finally
        {
            isProcessing = false;
            isProcessingRfid = false;
            StateHasChanged();
        }
    }
}