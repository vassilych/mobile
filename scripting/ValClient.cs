using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SplitAndMerge;

namespace Proxy
{
    class ProxyClient
    {
        static Socket s_client;

        static CancellationTokenSource s_cancelTokenSource = new CancellationTokenSource();

        static bool s_loggedin = false;

        static string pass = "mi_900.";
        static string keySalt = "poo12.";
        static string ivSalt = "puu14T";
        static byte[] key = SHA256.Create().ComputeHash(Encoding.Unicode.GetBytes(keySalt + pass));
        static byte[] iv = SHA256.Create().ComputeHash(Encoding.Unicode.GetBytes(ivSalt + pass)).Take(16).ToArray();

        static string s_username = "";
        static string s_password = "";

        static Socket GetConnection(string host, int port)
        {
            var hostEntry = Dns.GetHostEntry(host).AddressList[0];
            IPAddress ipAddress = IPAddress.Parse(hostEntry.ToString());

            Socket client = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result = client.BeginConnect(ipAddress, port, null, null);

            result.AsyncWaitHandle.WaitOne(10 * 1000, true);
            if (!client.Connected)
            {
                return null;
            }
            client.EndConnect(result);

            return client;
        }

        static void StartClient(CustomFunction callbackFunction, bool forced = false,
                                string host = "167.86.100.196", int port = 33333)
        {
            if (!forced && s_client != null && s_client.Connected)
            {
                return;
            }
            if (s_client != null)
            {
                if (s_client.Connected)
                {
                    s_client.Disconnect(true);
                }
                s_client.Dispose();
            }

            s_client = GetConnection(host, port);

            if (s_client == null)
            {
                throw new ArgumentException("Could not connect to " + host);
            }

        }

        static string GetData(bool encoded = false)
        {
            byte[] bytes = new byte[256];

            s_client.ReceiveTimeout = 10000;
            int bytesReceived = s_client.Receive(bytes);
            var newArray = TruncateArray(bytes);
            string received = Encoding.UTF8.GetString(newArray, 0, bytesReceived).Trim();
            int msgSize;
            if (string.IsNullOrWhiteSpace(received) ||
                !int.TryParse(received, out msgSize))
            {
                return received;
            }

            byte[] msg = Encoding.UTF8.GetBytes("OK");
            s_client.Send(msg);

            bytesReceived = 0;
            bytes = new byte[msgSize];
            int round = 0;
            while (bytesReceived < msgSize && s_client.Connected)
            {
                int rec = s_client.Receive(bytes, bytesReceived, msgSize - bytesReceived, SocketFlags.None);
                bytesReceived += rec;
                round++;
                //Buffer.BlockCopy(src, 5, dest, 7, 6);
            }

            received = encoded ? DecryptStringFromBytes(bytes, key, iv) :
                       Encoding.UTF8.GetString(bytes, 0, bytesReceived).Trim();
            return received;
        }

        internal static void Login(CustomFunction callbackFunction, string user, string passw, bool forced = false)
        {
            if (s_loggedin && s_client != null && s_client.Connected && !forced)
            {
                return;
            }

            s_username = user;
            s_password = passw;

            try
            {
                StartClient(callbackFunction, forced);

                string request = "login|" + user + " " + passw;
                byte[] bytes = EncryptStringToBytes(request, key, iv);

                int bytesSent = s_client.Send(bytes);

                var result = GetData();

                s_loggedin = result.StartsWith("OK", StringComparison.OrdinalIgnoreCase);

                scripting.CommonFunctions.RunOnMainThread(callbackFunction, "", result);
            }
            catch (Exception e)
            {
                var msg = e is SocketException ? "Connection timeout" : e.Message;
                scripting.CommonFunctions.RunOnMainThread(callbackFunction, "", msg);
                s_loggedin = false;
            }
        }

        static void TaskRequest(string request, string load, CustomFunction callbackFunction,
             bool encoded = false)
        {
            try
            {
                StartClient(callbackFunction, true);

                request += "|" + s_username + " " + s_password + "|" + load;
                //byte[] bytes = Encoding.UTF8.GetBytes(request);
                byte[] bytes = EncryptStringToBytes(request, key, iv);

                int sent = s_client.Send(bytes);

                string result = GetData(encoded);

                scripting.CommonFunctions.RunOnMainThread(callbackFunction, load, result);
            }
            catch (Exception e)
            {
                var msg = e is SocketException ? "Connection timeout" : e.Message;
                scripting.CommonFunctions.RunOnMainThread(callbackFunction, "", msg);
                s_loggedin = false;
            }
        }

        internal static void QueueRequest(CustomFunction callbackFunction, string request, string load = "",
             bool encoded = false)
        {
            Task.Run(() => TaskRequest(request, load, callbackFunction));
        }

        internal static void QueueLogin(CustomFunction callbackFunction, string request, string load = "",
             bool encoded = false)
        {
            Task.Run(() => Login(callbackFunction, request, load, true));
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        static byte[] TruncateArray(byte[] array)
        {
            int i = array.Length - 1;
            while (i >= 0 && array[i] == 0)
            {
                --i;
            }
            byte[] newArray = new byte[i + 1];
            Array.Copy(array, newArray, i + 1);
            return newArray;
        }

        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }

    internal class GetServerDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            string request = Utils.GetSafeString(args, 0);
            string load = Utils.GetSafeString(args, 1);
            string callBack = Utils.GetSafeString(args, 2);

            CustomFunction callbackFunction = ParserFunction.GetFunction(callBack, null) as CustomFunction;
            if (callbackFunction == null)
            {
                throw new ArgumentException("Error: Couldn't find function [" + callBack + "]");
            }

            ProxyClient.QueueRequest(callbackFunction, request, load);
            return Variable.EmptyInstance;
        }
    }

    internal class AddOrderedDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 4, m_name);

            Variable root = Utils.GetSafeVariable(args, 0);
            string key = Utils.GetSafeString(args, 1);
            double y = Utils.GetSafeDouble(args, 3);
            bool xNumeric = Utils.GetSafeString(args, 4, "double") == "double";

            double x = xNumeric ? Utils.GetSafeDouble(args, 2) : 0.0;
            string xStr = Utils.GetSafeString(args, 2);

            root.SetAsArray();
            Variable data = root.GetVariable(key);
            if (data == Variable.EmptyInstance)
            {
                data = new Variable(Variable.VarType.ARRAY);
                data.ParsingToken = key;
            }

            int index = 0;
            for (; index < data.Tuple.Count; index += 2)
            {
                if (xStr == data.Tuple[index].AsString())
                {
                    return Variable.EmptyInstance;
                }
                if ((xNumeric && x < data.Tuple[index].AsDouble()))
                {
                    break;
                }
            }

            index = Math.Min(index, data.Tuple.Count);
            data.Tuple.Insert(index, new Variable(xStr));
            data.Tuple.Insert(index + 1, new Variable(y));

            root.SetHashVariable(key, data);
            AddGlobal(root.ParsingToken, new GetVarFunction(root));

            return Variable.EmptyInstance;
        }
    }

    internal class LoginFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string username = args[0].AsString();
            string password = args[1].AsString();
            string callBack = args[2].AsString();

            CustomFunction callbackFunction = ParserFunction.GetFunction(callBack, null) as CustomFunction;
            if (callbackFunction == null)
            {
                throw new ArgumentException("Error: Couldn't find function [" + callBack + "]");
            }

            ProxyClient.QueueLogin(callbackFunction, username, password, true);
            return Variable.EmptyInstance;
        }
    }
}
