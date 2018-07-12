//#define MAIN_THREAD_CHECK

using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;

//using SignalR.Hubs;

namespace SplitAndMerge
{
    public class DebuggerServer
    {
        public static Action<Debugger, string> OnRequest;
        public static bool DebuggerAttached { set; get; }

        static Debugger m_debugger;
        static TcpClient m_client;
        static NetworkStream m_stream;

        static BlockingCollection<string> m_queue = new BlockingCollection<string>();

        public static void StartServer(int port = 13337)
        {
            ThreadPool.QueueUserWorkItem(StartServerBlocked, port);
        }

        public static void StartServerBlocked(Object threadContext)
        {
            int port = (int)threadContext;

            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            TcpListener server = new TcpListener(localAddr, port);
            server.Start();
            DebuggerAttached = true;

            while (true)
            {
                Console.Write("Waiting for a connection on {0}... ", port);

                // Perform a blocking call to accept requests.
                m_client = server.AcceptTcpClient();
                m_stream = m_client.GetStream();

                ThreadPool.QueueUserWorkItem(RunClient);
                Thread.Sleep(1000);
            }
        }

        static void StartProcessing(Object threadContext)
        {
#if MAIN_THREAD_CHECK
            System.Timers.Timer runTimer = new System.Timers.Timer(0.25*1000);
            bool processing = false;
            runTimer.Elapsed += (sender, e) => 
            {
                if (processing) {
                    return;
                }
                processing = true;
                scripting.iOS.AppDelegate.GetCurrentController().InvokeOnMainThread(() =>
                {
                    ProcessQueue();
                });
                processing = false;
            };
            runTimer.Start();
#elif UNITY_EDITOR || UNITY_STANDALONE
      // Do nothing: ProcessQueue() will be called from the Unity Main Thread
#else
            try
            {
                ProcessQueue();
            }
            catch (Exception exc)
            {
                Console.Write("Connection is dead: {0}", exc.Message);
            }
#endif
        }

        public static void ProcessQueue()
        {
            string data;
#if UNITY_EDITOR || UNITY_STANDALONE || MAIN_THREAD_CHECK
            while(m_queue.TryTake(out data))
            { // Exit as soon as done processing.
                Console.WriteLine("STARTED " + data);
                m_debugger.ProcessClientCommands(data);
                Console.WriteLine("DONE " + data);
#else
            while (true)
            { // A blocking call.
                data = m_queue.Take();

                if (Debugger.IsPureReplRequest(data))
                {
                    m_debugger.ProcessClientCommands(data);
                    return;
                }
                ThreadPool.QueueUserWorkItem(RunRequestBlocked, data);
#endif
            }
        }

        static void RunRequestBlocked(Object threadContext)
        {
            string data = (string)threadContext;
            m_debugger.ProcessClientCommands(data);
        }

        static void RunClient(Object threadContext)
        {
            Byte[] bytes = new Byte[2048];
            string data = null;
            Console.WriteLine("Starting client {0}", m_client.Client.RemoteEndPoint);

            ThreadPool.QueueUserWorkItem(StartProcessing, null);

#if UNITY_EDITOR == false && UNITY_STANDALONE == false && __ANDROID__ == false && __IOS__ == false
            Interpreter.Instance.Init();
#endif

            m_debugger = new Debugger();
            Debugger.OnResult += SendBack;

            int i;
            try
            {
                while ((i = m_stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                    if (data.StartsWith("bye|"))
                    {
                        break;
                    }
                    else if (m_debugger.CanProcess(data))
                    {
                        ThreadPool.QueueUserWorkItem(RunRequestBlocked, data);
                    }
                    else
                    {
                        m_queue.Add(data);
                    }
                }
            }
            catch (Exception exc)
            {
                Console.Write("Client disconnected: {0}", exc.Message);
            }

            Debugger.OnResult -= SendBack;

            // Shutdown and end connection
            Console.Write("Closed connection.");
            m_client.Close();
        }

        static void SendBack(string str)
        {
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(str);
            try
            {
                m_stream.Write(msg, 0, msg.Length);
                m_stream.Flush();
            }
            catch (Exception exc)
            {
                Console.Write("Client disconnected: {0}", exc.Message);
                return;
            }
        }
    }
}
