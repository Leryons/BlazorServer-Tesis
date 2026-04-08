namespace Tesis.Models
{
    public class Document
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    public enum DocumentType
    {
        None,
        Cedula,
        Passport,
        License,
        MedicalRecord,
    }
}