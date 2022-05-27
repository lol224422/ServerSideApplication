using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace ServerSideApplication
{
    internal class Source
    {
        private List<MonitoringServerConnection> AppliedConnections = new List<MonitoringServerConnection>();

        public Source()
        {
              Task SocketConncection = new Task(() =>
              {
                  ApplyAllSocketConnectiong();
              });
              SocketConncection.Start();

             // Console.WriteLine("Read data from servers ?");

              /*while (true)
              {
                  Console.ReadLine();
                  Logic();
              }*/


            TelegrammInterface.Source(AppliedConnections);

        }
        private void  ApplyAllSocketConnectiong()
        {
            //IPHostEntry ipHost = Dns.GetHostEntry("127.0.0.1");

            IPAddress ipAddr = IPAddress.Parse("192.168.15.11");
            Console.WriteLine(ipAddr.ToString());
            Console.WriteLine("Server address -  {0}", ipAddr);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);
                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Waiting connection on port {0}", ipEndPoint.Port);

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    MonitoringServerConnection Connection = new MonitoringServerConnection(handler);
                    AppliedConnections.Add(Connection);
                    Console.WriteLine("Total applied connections - {0} ", AppliedConnections.Count);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
