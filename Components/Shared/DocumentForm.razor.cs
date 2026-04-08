namespace Tesis.Components.Shared;

public partial class DocumentForm
{
    [Inject] private UserServices userServices { get; set; } = null!;
    [Inject] private UserSessionService userSessionService { get; set; } = null!;
    [Inject] private IWebHostEnvironment webHostEnvironment { get; set; } = null!;

    [Parameter] public DocumentType SelectedDocument { get; set; }

    private async Task HandleFileSelection(InputFileChangeEventArgs eventArgs)
    {
        // Store selected file; actual upload happens when user clicks the button.
        try
        {
            var file = eventArgs.File;
            if (file == null) return;
            selectedFile = file;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al seleccionar el archivo: {ex.Message}");
        }
    }

    private IBrowserFile? selectedFile;
    private bool isUploading = false;
    private string errorMessage = string.Empty;

    private async Task UploadDocument()
    {
        if (selectedFile == null) return;
        if (userSessionService.User == null)
        {
            Console.WriteLine("Error: El usuario no ha iniciado sesión.");
            return;
        }

        isUploading = true;
        StateHasChanged();

        try
        {
            long maxAllowedSize = 1024 * 1024 * 10;
            string fileName = $"{SelectedDocument}_{userSessionService.User.Id}{Path.GetExtension(selectedFile.Name)}";
            string relativeFolder = "Uploads";
            var folderPath = Path.Combine(webHostEnvironment.WebRootPath, relativeFolder);

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);
            var relativePath = $"{relativeFolder}/{fileName}";

            using var stream = selectedFile.OpenReadStream(maxAllowedSize);
            using var fileStream = File.Create(fullPath);
            await stream.CopyToAsync(fileStream);

            var document = new Document
            {
                Title = fileName,
                Type = SelectedDocument,
                FilePath = relativePath,
                UserId = userSessionService.User.Id
            };

            var success = await userServices.UploadDocument(userSessionService.User.Id, document);
            if (success)
            {
                selectedFile = null;
                errorMessage = string.Empty;
            }
            else
            {
                errorMessage = $"Ya tienes un documento de tipo {SelectedDocument} subido. No puedes subir otro.";
                // No limpiar selectedFile para que el usuario pueda intentarlo de nuevo o cambiar
            }
            isUploading = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al subir el archivo: {ex.Message}");
            isUploading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}