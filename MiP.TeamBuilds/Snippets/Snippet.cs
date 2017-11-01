using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MiP.TeamBuilds.Snippets
{
    public class UdpReceiver : IDisposable
    {
        private readonly UdpClient udp = new UdpClient(15000);

        public void Dispose()
        {
            udp.Dispose();
        }

        public void StartListening()
        {
            udp.BeginReceive(Receive, new object());
        }

        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = null;
            byte[] bytes = udp.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            Console.WriteLine(message + " from: " + ip);
            StartListening();
        }
    }

    public class UdpSender
    {
        public void Send(string m)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 15000);
            byte[] bytes = Encoding.ASCII.GetBytes(m);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
            client.Dispose();
        }
    }

    public class LocalHostHelper
    {
        public static bool IsLocalHost(string host)
        {
            IPHostEntry localHost = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress ipAddress = null;

            if (IPAddress.TryParse(host, out ipAddress))
                return localHost.AddressList.Any(x => x.Equals(ipAddress));

            IPHostEntry hostEntry = Dns.GetHostEntry(host);

            return localHost.AddressList.Any(x => hostEntry.AddressList.Any(y => x.Equals(y)));
        }
    }
}
