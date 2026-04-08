namespace Tesis.Services;

public class ESPCom : IDisposable
{
    private readonly SerialPort _port;
    public event Action<string>? OnMessageReceived;

    public ESPCom(string portName = "COM4", int baudRate = 115200)
    {
        _port = new SerialPort(portName, baudRate);
        _port.DataReceived += DataReceivedHandler;
    }

    public void Open()
    {
        if (!_port.IsOpen)
            _port.Open();
    }

    public void Close()
    {
        if (_port.IsOpen)
            _port.Close();
    }

    public void SendCommand(string command)
    {
        if (_port.IsOpen)
            _port.WriteLine(command);
    }

    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string data = _port.ReadLine();
            OnMessageReceived?.Invoke(data.Trim());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error leyendo puerto: " + ex.Message);
        }
    }

    public void Dispose()
    {
        Close();
        _port.Dispose();
    }

    public bool isOnline()
    {
        if (_port.IsOpen)
        {
            return true;
        }

        else
            return false;

    }
}