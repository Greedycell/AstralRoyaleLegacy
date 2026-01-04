using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ClashRoyale;

namespace ClashRoyale.WebAPI
{
    public static class API
    {
        private static int Port = GetPortFromConfig();
        private static TcpListener Listener;
        private static Thread WebAPIThread;
        private static bool Running = false;

        public static void Start()
        {
            if (Running) return;

            Running = true;
            WebAPIThread = new Thread(StartSafe)
            {
                IsBackground = true
            };
            WebAPIThread.Start();
        }

        private static void StartSafe()
        {
            try
            {
                Logger.Log($"Starting WebAPI on port {Port}...", null);

                Listener = new TcpListener(IPAddress.Any, Port);
                Listener.Start();

                Logger.Log($"WebAPI running on port {Port}.", null);

                while (Running)
                {
                    if (!Listener.Pending())
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    TcpClient client = Listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
            }
            catch (SocketException ex)
            {
                Logger.Log($"WebAPI socket error: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                Logger.Log($"WebAPI failed: {ex.Message}", null);
            }
            finally
            {
                Listener?.Stop();
                Logger.Log("WebAPI stopped.", null);
            }
        }

        public static void Stop()
        {
            Running = false;

            try
            {
                Listener?.Stop();
                WebAPIThread?.Join(500);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error stopping WebAPI: {ex.Message}", null);
            }
        }

        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;

            try
            {
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream);
                using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                string requestLine = reader.ReadLine();
                if (string.IsNullOrEmpty(requestLine)) return;

                string[] tokens = requestLine.Split(' ');
                if (tokens.Length < 2) return;

                string path = tokens[1];
                string responseText;
                string contentType = "text/html; charset=UTF-8";

                if (path.StartsWith("/api"))
                {
                    responseText = GetJsonAPI();
                    contentType = "application/json; charset=UTF-8";
                }
                else
                {
                    responseText = GetStatisticHTML();
                }

                byte[] body = Encoding.UTF8.GetBytes(responseText);

                writer.WriteLine("HTTP/1.1 200 OK");
                writer.WriteLine($"Content-Type: {contentType}");
                writer.WriteLine($"Content-Length: {body.Length}");
                writer.WriteLine("Connection: close");
                writer.WriteLine();
                stream.Write(body, 0, body.Length);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling client: {ex.Message}", null);
            }
            finally
            {
                client.Close();
            }
        }

        private static int GetPortFromConfig()
        {
            return 8888;
        }

        public static string GetStatisticHTML()
        {
            try
            {
                return HTML()
                    .Replace("%ONLINEPLAYERS%", Resources.Players.Count.ToString())
                    .Replace("%INMEMORYPLAYERS%", API_Stats.PlayerStat.ToString())
                    .Replace("%INMEMORYALLIANCES%", API_Stats.AllianceStat.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log($"Error generating statistics HTML: {ex.Message}", null);
                return "The server encountered an internal error or misconfiguration. (500)";
            }
        }

        public static string GetJsonAPI()
        {
            try
            {
                JObject data = new JObject
                {
                    { "online_players", Resources.Players.Count },
                    { "in_mem_players", API_Stats.PlayerStat },
                    { "in_mem_alliances", API_Stats.AllianceStat }
                };
                return JsonConvert.SerializeObject(data, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error generating JSON API: {ex.Message}", null);
                return JsonConvert.SerializeObject(new { error = "Internal Server Error" }, Formatting.Indented);
            }
        }

        public static string HTML()
        {
            try
            {
                using StreamReader sr = new StreamReader("WebAPI/HTML/Statistics.html");
                return sr.ReadToEnd();
            }
            catch
            {
                return "HTML file not found or could not be loaded.";
            }
        }
    }
}