using System;
using System.Net.Sockets;
using System.Text;

namespace ServerSideApplication
{
    internal class MonitoringServerConnection
    {
        private Socket Handler { get; set; }
   //     public string Name { get; set; }
        public MonitoringServerConnection(Socket _handler)
        {
            Handler = _handler;     
        }

        public string RequestMonitoringData(int FunctionIndex)
        {
          
            string reply = FunctionIndex.ToString();
            string data = "";
            byte[] msg = Encoding.UTF8.GetBytes(reply);
            Handler.BeginSend(msg, 0, msg.Length, SocketFlags.None, null, null);

            byte[] bytes = new byte[1024];
            int bytesRec = Handler.Receive(bytes);

            data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

            Console.Write("Полученный текст: " + data + "\n\n");

          //  Handler.(SocketShutdown.Both);
          // Handler.Close();
            return data;
        }


        public void CloseConnection()
        {
            Handler.Shutdown(SocketShutdown.Both);
            Handler.Close();
        }

    }
}
