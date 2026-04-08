namespace Tesis.Services;

public class UserSessionService
{
    public User? User { get; private set; }
    public bool IsAuthenticated => User != null;
    public event Action? OnUserChanged;

    public void Login(User usuario)
    {
        User = usuario;
        NotifyUserChanged();
        Console.WriteLine($"User {usuario.Name} logged in.");
    }

    public void Logout()
    {
        User = null;
        NotifyUserChanged();
        Console.WriteLine("User logged out.");
    }

    private void NotifyUserChanged()
    {
        OnUserChanged?.Invoke();
    }
}