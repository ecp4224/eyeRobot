using System.Net.Sockets;

public interface IClient
{
    Socket socket { get; }
    
    void Send(byte[] data);
}