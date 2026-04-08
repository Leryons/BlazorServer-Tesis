using System.IO;

namespace Tesis.Services;

public class UserServices
{
    private readonly string _connectionString;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public UserServices(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        string server = "192.168.20.13"; 
        string bd = "tesisdb";
        string user = "tesis_user";
        string password = "112233445566";
        string port = "3306";

        _connectionString = $"Server={server};Port={port};Database={bd};Uid={user};Pwd={password};";

        InitializeDataBase();
    }
    private void InitializeDataBase()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Email VARCHAR(255) NOT NULL UNIQUE,
                Password VARCHAR(255) NOT NULL,
                FingerId INT NOT NULL,
                Role VARCHAR(50) NOT NULL,
                RfidUid VARCHAR(255)
            );
            
            CREATE TABLE IF NOT EXISTS Documents (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                UserId INT NOT NULL,
                Title VARCHAR(255) NOT NULL,
                Type VARCHAR(50) NOT NULL,
                FilePath VARCHAR(500) NOT NULL,
                FOREIGN KEY(UserId) REFERENCES Users(Id)
            );";

        command.ExecuteNonQuery();
    }
    public async Task<int> RegisterUser(User newUser, int fingerId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Users (Name, Email, Password, FingerId, Role)
            VALUES (@name, @email, @password, @fingerId, @role);";

        command.Parameters.AddWithValue("@name", newUser.Name);
        command.Parameters.AddWithValue("@email", newUser.Email.Trim().ToLower());
        command.Parameters.AddWithValue("@password", newUser.Password);
        command.Parameters.AddWithValue("@fingerId", fingerId);
        command.Parameters.AddWithValue("@role", newUser.Role.ToString());
        await command.ExecuteNonQueryAsync();

        var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT LAST_INSERT_ID();";
        var result = await idCmd.ExecuteScalarAsync();
        var insertedId = Convert.ToInt32(result);
        return insertedId;
    }
    public User? GetUserByEmail(string email)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Email, Password, FingerId, Role, RfidUid FROM Users WHERE Email = @email;";
        command.Parameters.AddWithValue("@email", email.Trim().ToLower());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Password = reader.GetString(3),
                FingerId = reader.GetInt32(4),
                Role = Enum.Parse<UserRole>(reader.GetString(5)),
                RfidUid = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            };
        }
        return null;
    }
    public int GetNextFingerId()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT MAX(FingerId) FROM Users;";

        var result = command.ExecuteScalar();

        if (result == null || result == DBNull.Value)
        {
            return 1;
        }
        
        return Convert.ToInt32(result) + 1;
    }
    public async Task<List<Document>> GetAllDocumentsForRfidUi(string rfidUid)
    {
        var documents = new List<Document>();
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT d.Id, d.UserId, u.Name, d.Title, d.Type, d.FilePath
            FROM Documents d
            JOIN Users u ON d.UserId = u.Id
            WHERE u.RfidUid = @rfidUid
            ORDER BY d.Title ASC;";

        command.Parameters.AddWithValue("@rfidUid", rfidUid);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            documents.Add(new Document {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                UserName = reader.GetString(2),
                Title = reader.GetString(3),
                Type = Enum.Parse<DocumentType>(reader.GetString(4)),
                FilePath = reader.GetString(5)
            });
        }
        return documents;
    }
    public async Task<List<Document>> GetAllDocumentsForUser(int userId)
    {
        var documents = new List<Document>();
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT d.Id, d.UserId, u.Name, d.Title, d.Type, d.FilePath
            FROM Documents d
            JOIN Users u ON d.UserId = u.Id
            WHERE d.UserId = @userId
            ORDER BY d.Title ASC;";
        
        command.Parameters.AddWithValue("@userId", userId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            documents.Add(new Document {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                UserName = reader.GetString(2),
                Title = reader.GetString(3),
                Type = Enum.Parse<DocumentType>(reader.GetString(4)),
                FilePath = reader.GetString(5)
            });
        }
        return documents;
    }
    public async Task<bool> UploadDocument(int userId, Document document)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Verificar si ya existe un documento del mismo tipo para este usuario
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT COUNT(*) FROM Documents
            WHERE UserId = @userId AND Type = @type;";
        checkCommand.Parameters.AddWithValue("@userId", userId);
        checkCommand.Parameters.AddWithValue("@type", document.Type.ToString());
        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

        if (count > 0)
        {
            return false; // Ya existe un documento de este tipo
        }

        // Insertar el nuevo documento
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Documents (UserId, Title, Type, FilePath)
            VALUES (@userId, @title, @type, @filePath);";

        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@title", document.Title);
        command.Parameters.AddWithValue("@type", document.Type.ToString());
        command.Parameters.AddWithValue("@filePath", document.FilePath);

        await command.ExecuteNonQueryAsync();
        return true;
    }
    public async Task<bool> UpdateUserRfidUid(int userId, string rfidUid)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Verificar si el RFID ya está asignado a otro usuario
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
            SELECT COUNT(*) FROM Users
            WHERE RfidUid = @rfidUid AND Id != @userId;";
        checkCommand.Parameters.AddWithValue("@rfidUid", rfidUid);
        checkCommand.Parameters.AddWithValue("@userId", userId);
        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

        if (count > 0)
        {
            return false; // Ya asignado a otro usuario
        }

        // Actualizar el RFID para este usuario
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Users
            SET RfidUid = @rfidUid
            WHERE Id = @userId;";

        command.Parameters.AddWithValue("@rfidUid", rfidUid);
        command.Parameters.AddWithValue("@userId", userId);

        await command.ExecuteNonQueryAsync();
        return true;
    }
    public async Task DeleteDocument(int documentId)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Primero obtener el FilePath para borrarlo después
        var selectCommand = connection.CreateCommand();
        selectCommand.CommandText = "SELECT FilePath FROM Documents WHERE Id = @documentId;";
        selectCommand.Parameters.AddWithValue("@documentId", documentId);
        var filePath = await selectCommand.ExecuteScalarAsync() as string;

        // Borrar de la base de datos
        var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = @"DELETE FROM Documents WHERE Id = @documentId;";
        deleteCommand.Parameters.AddWithValue("@documentId", documentId);
        await deleteCommand.ExecuteNonQueryAsync();

        // Borrar el archivo físico si existe
        if (!string.IsNullOrEmpty(filePath))
        {
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}