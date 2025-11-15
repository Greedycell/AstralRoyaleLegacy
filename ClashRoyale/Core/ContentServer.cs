using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClashRoyale.Core
{
    public static class ContentServer
    {
        private static TcpListener _server;

        public static async Task StartAsync(int port = 9340)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
            Console.WriteLine($"ContentServer listening on port {port}...");

            while (true)
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, leaveOpen: true);

                string requestLine = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(requestLine))
                {
                    string[] tokens = requestLine.Split(' ');
                    if (tokens.Length >= 2 && tokens[0] == "GET")
                    {
                        string relativePath = tokens[1].TrimStart('/').Replace("/", "\\");
                        string filePath = Path.Combine("GameAssets/update", relativePath);

                        Console.WriteLine($"Requested file: {filePath}");

                        if (File.Exists(filePath))
                        {
                            byte[] content = await File.ReadAllBytesAsync(filePath);
                            string header = "HTTP/1.1 200 OK\r\n" +
                                            $"Content-Length: {content.Length}\r\n" +
                                            "Content-Type: application/octet-stream\r\n\r\n";
                            byte[] headerBytes = Encoding.ASCII.GetBytes(header);

                            await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
                            await stream.WriteAsync(content, 0, content.Length);
                        }
                        else
                        {
                            string notFound = "HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\n\r\n";
                            byte[] notFoundBytes = Encoding.ASCII.GetBytes(notFound);
                            await stream.WriteAsync(notFoundBytes, 0, notFoundBytes.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        public static void Stop()
        {
            _server?.Stop();
            Console.WriteLine("ContentServer stopped.");
        }
    }
}