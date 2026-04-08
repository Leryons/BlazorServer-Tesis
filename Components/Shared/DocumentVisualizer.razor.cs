using Microsoft.JSInterop;

namespace Tesis.Components.Shared;

public partial class DocumentVisualizer
{
    [Inject] private UserServices userServices { get; set; } = null!;
    [Inject] private UserSessionService userSessionService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public List<Document>? Documents { get; set; }

    List<Document> documents = new List<Document>();

    Document document = new Document();

    private bool showModal = false;
    private string modalImagePath = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (Documents != null)
        {
            documents = Documents;
            return;
        }

        if (userSessionService.User != null)
        {
            documents = await userServices.GetAllDocumentsForUser(userSessionService.User.Id);
        }
    }

    private string GetIcon(DocumentType type) => type switch
    {
        DocumentType.Cedula => "bx bxs-id-card",
        DocumentType.Passport => "bx bxs-paper-plane",
        DocumentType.License => "bx bxs-id-card",
        DocumentType.MedicalRecord => "bx bxs-plus-medical",
        _ => "bx bxs-file"
    };

    private string GetColorClass(DocumentType type) => type switch
    {
        DocumentType.Cedula => "bg-blue",
        DocumentType.Passport => "bg-green",
        DocumentType.License => "bg-orange",
        DocumentType.MedicalRecord => "bg-red",
        _ => "bg-secondary"
    };

    private void OpenFile(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            modalImagePath = path.StartsWith("/") ? path : "/" + path;
            showModal = true;
            StateHasChanged();
        }
    }

    private void CloseModal()
    {
        showModal = false;
        modalImagePath = string.Empty;
    }

    private async Task DeleteDocument(Document doc)
    {
        bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"¿Estás seguro de que quieres eliminar el documento '{doc.Title}'?");
        if (confirmed)
        {
            await userServices.DeleteDocument(doc.Id);
            documents.Remove(doc);
            StateHasChanged();
        }
    }
}