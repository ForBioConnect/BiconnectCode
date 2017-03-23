
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelnetServer
{
    class Program
    {
        // State object for reading client data asynchronously 
        public class StateObject
        {
            // Client  socket. 
            public Socket workSocket = null;
            // Size of receive buffer. 
            public const int BufferSize = 1024;
            // Receive buffer. 
            public byte[] buffer = new byte[BufferSize];
            // Received data string. 
            public StringBuilder sb = new StringBuilder();
        }
        
        public class AsynchronousSocketListener : IDisposable
        {
            // Create a TCP/IP socket. 
            private static Socket listener = new Socket(AddressFamily.InterNetwork,
                 SocketType.Stream, ProtocolType.Tcp);
            #region"Property"
            volatile bool disposed = false;
            volatile bool isDisposing = false;
            // Thread signal. 
            public static ManualResetEvent allDone = new ManualResetEvent(false);
            private static long _Count;
            public static long Count

            {

                get { return _Count; }

                set { _Count = value; }

            }
            private static long _Counter;
            public static long Counter

            {

                get { return _Counter; }

                set { _Counter = value; }

            }
                        #endregion
            
            #region "Constructor and Destructor"

            public AsynchronousSocketListener()

            {

            }





            /// <summary>

            /// Desctructor called by GC to dispose this object automatically.

            /// </summary>

            ~AsynchronousSocketListener()
            {

                Dispose(false);

            }

            #endregion

            #region Disposal

            /// <summary>

            /// Implementation of the IDisposable interface to allow for manual release of the object's resources.

            /// </summary>

            public void Dispose()

            {

                Dispose(true);

                GC.SuppressFinalize(this);

            }







            private void Dispose(bool disposing)

            {

                if (!disposing)

                {

                    isDisposing = true;



                    //listener.Shutdown(SocketShutdown.Both);

                    listener.Disconnect(true);

                    listener.Close();

                    //listener.Close();

                    ReleaseRef();



                    isDisposing = false;



                    disposed = true;

                }

            }



            #endregion

            private static void AddRef()
            {
                Count++;
                Counter++;
            }
            private static void ReleaseRef()
            {
                Count--;
            }
            private static void StartServer()
            {
                // Data buffer for incoming data. 
                byte[] bytes = new Byte[1024];
                // Establish the local endpoint for the socket. 
                // The DNS name of the computer 
                // running the listener
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 55555);
                // Bind the socket to the local endpoint and listen for incoming connections.  
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(100);
                    while (true)
                    {
                        // Set the event to nonsignaled state. 
                        allDone.Reset();
                        // Start an asynchronous socket to listen for connections. 
                        Console.WriteLine("Waiting for a connection...");
                        // Signal the main thread to continue. 
                        //allDone.Set();
                        listener.BeginAccept(
                           new AsyncCallback(AcceptCallback),
                           listener);
                        allDone.WaitOne();
                        AddRef();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error" + ex.Message);
                }
            }
            private static void StopServer()
            {
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
                ReleaseRef();
             }
            private static void StartListening()
            {
                try {
                
                        listener.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listener);
                        // Wait until a connection is made before continuing. 
                        allDone.WaitOne();
                    }                
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                Console.WriteLine("\nPress ENTER to continue...");
                Console.Read();
            }
            private static void AcceptCallback(IAsyncResult ar)
            {
                allDone.Set();

                // Get the socket that handles the client request. 
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                // Create the state object. 
                StateObject state = new StateObject();
                state.workSocket = handler;
                
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            private static void ReadCallback(IAsyncResult ar)
            {
                String content = String.Empty;
                // Retrieve the state object and the handler socket 
                // from the asynchronous state object. 
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;
                // Read data from the client socket.  
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far. 
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    // Check for end-of-file tag. If it is not there, read  
                    // more data. 
                    content = state.sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        // All the data has been read from the  
                        // client. Display it on the console. 
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content);
                        // Echo the data back to the client. 
                        Send(handler, content);
                    }
                    else
                    {
                        // Not all data received. Get more. 
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }

                }
            }
            private static int GeneratePrime()
            {
                Random rnd = new Random();
                int RndNum= rnd.Next(2, 100);
                int reminder = 0;
                int cnt = 0;
                int i = 0;
               // for (i = 2; i <= RndNum; i++)
                {
                    for (int j = 1; j <= 100; j++)
                    {
                        reminder = RndNum % j;
                        if (reminder == 0)
                            cnt++;
                        if (cnt == 1)
                            return RndNum;
                    }
                   
                }
                 return GeneratePrime();

            }
            private static void Send(Socket handler, String data)
            {
                string SendServer = string.Empty;
                data = data.ToUpper().Replace(@"<EOF>", "");
                switch (data)
                {
                    case "HELLO":
                        SendServer = "Hi # Timeout 5 seconds";
                        break;
                    case "COUNT":
                        SendServer = Convert.ToString(Count) + " # Integer Representing count of successful hanshakes";
                        break;
                    case "COUNTER":
                        SendServer = Convert.ToString(Counter) + " # cOUNTER";
                        break;
                    case "PRIME":
                        SendServer = GeneratePrime().ToString() + "# returns a randomly generated prime number";
                        break;
                    case "TERMINATE":
                        SendServer = "BYE # Server terminates the connection";
                      
                        break;
                    default:
                        break;

                }
                // Convert the string data to byte data using ASCII encoding. 
                byte[] byteData = Encoding.ASCII.GetBytes(SendServer);
                StateObject state = new StateObject();
                state.workSocket = handler;
                state.buffer = byteData;
                // Begin sending the data to the remote device. 
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), state);
                if (data == "Terminate")
                {
                    StopServer();
                }
            }            
            private static void SendCallback(IAsyncResult ar)
            {
                try
                {    string strReceived = string.Empty;

                    // Retrieve the socket from the state object. 

                    StateObject state = (StateObject)ar.AsyncState;

                    Socket handler = state.workSocket;
                    int bytesRead = handler.EndReceive(ar);
                     if (bytesRead > 0)
                     {
                         // There  might be more data, so sore the data received so far. 
                         state.sb.Append(Encoding.ASCII.GetString(
                             state.buffer, 0, bytesRead));
                          //Check for end-of-file tag. If it is not there, read  
                         // more data. 
                         strReceived = state.sb.ToString();
                     }


                    Console.WriteLine("Sent {0} bytes to client.", strReceived);
                           handler.Shutdown(SocketShutdown.Send);
                          handler.Close();
                   

                    //ReleaseRef();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
            public static int Main(String[] args)

            {
                StartServer();
               // StartListening();

                return 0;

            }

        }

    }

}


