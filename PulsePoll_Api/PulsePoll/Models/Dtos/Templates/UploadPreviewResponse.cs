namespace PulsePoll.Models.Dtos.Templates;

// Shape returned by POST /templates/upload (2.2) — preview only, nothing persisted yet
public class UploadPreviewResponse
{
    public string PreviewToken { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<QuestionDto> Questions { get; set; } = new();
}
