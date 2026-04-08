namespace Tesis.Components.Shared;

public partial class CarrouselContainer
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string ImagePath { get; set; } = string.Empty;
    [Parameter] public string TextExample { get; set; } = string.Empty;
}