using System.Net;
using System.Net.Sockets;
using OscCore;

// https://steamdb.info/patchnotes/8740383/
public class VrcOsc
{
    public static void SendIsOverlayOpen(bool isOpen) {
        var host = "127.0.0.1";
        var port = 9000;
        var endpoint = IPEndPoint.Parse($"{host}:{port}");

        UdpClient udpClient = new UdpClient();
        var msgBytes = new OscMessage("/avatar/parameters/isOverlayOpen", isOpen).ToByteArray();
        udpClient.Send(msgBytes, msgBytes.Length, endpoint);
    }

}
