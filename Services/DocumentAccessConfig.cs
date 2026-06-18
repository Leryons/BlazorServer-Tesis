namespace Tesis.Services;

public static class DocumentAccessConfig
{
    private static readonly Dictionary<UserRole, DocumentType[]> _allowed = new()
    {
        //Roles y sus permisos a documentos
        { UserRole.User, new DocumentType[] { DocumentType.Cedula, DocumentType.Passport, DocumentType.License, DocumentType.MedicalRecord } },
        { UserRole.Police, new DocumentType[] { DocumentType.Cedula, DocumentType.License } },
        { UserRole.Doctor, new DocumentType[] { DocumentType.Cedula, DocumentType.MedicalRecord } },
    };

    public static DocumentType[] GetAllowedTypes(UserRole role)
    {
        if (_allowed.TryGetValue(role, out var types)) return types;
        return Array.Empty<DocumentType>();
    }

    public static bool IsAllowed(UserRole role, DocumentType type)
    {
        return GetAllowedTypes(role).Contains(type);
    }
}
