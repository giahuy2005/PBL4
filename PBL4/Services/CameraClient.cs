using CommunityToolkit.Mvvm.ComponentModel;
using PBL4.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
namespace PBL4.Services
{
    public partial class CameraClient : ObservableObject
    {
        [ObservableProperty]
        private string clientState = "off";
        private ClientWebSocket _ws = new ClientWebSocket();
        public event Action<string, BitmapImage?>? FrameReceived;
        public async Task ConnectAsync(string uri)
        {
            try
            {
                await _ws.ConnectAsync(new Uri(uri), CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                MessageBox.Show($"Lỗi khi kết nối tới server: {ex.Message}", "WebSocket");
                return;
            }
          //  MessageBox.Show("Kết nối WebSocket thành công!", "Thông báo");
            ClientState="on";
            _ = Task.Run(async () => await ReceiveLoop());
        }

        public bool check_ws()
        {
             if (_ws == null)
            {
                MessageBox.Show("WebSocket chưa được khởi tạo!", "Lỗi");
                return false;
            }
            if (_ws.State != WebSocketState.Open)
            {
                MessageBox.Show($"WebSocket không ở trạng thái Open (hiện tại: {_ws.State})", "Lỗi WebSocket");
                return false;
            }
            return true;
        }
        public async Task StartCamera(string camId, string url)
        {
            if (_ws == null)
            {
                MessageBox.Show("WebSocket chưa được khởi tạo!", "Lỗi");
                return;
            }
            if (_ws.State != WebSocketState.Open)
            {

                MessageBox.Show($"WebSocket không ở trạng thái Open (hiện tại: {_ws.State})", "Lỗi WebSocket");
                return;
            }
            var message = new
            {
                cmd = "add",
                camera = camId,
                url = url
            };
            string json = JsonSerializer.Serialize(message);

            await _ws.SendAsync(
                Encoding.UTF8.GetBytes(json),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
        public async Task CloseAsync(string reason = "Client closing")
        {
            if (_ws != null)
            {
                try
                {
                    if (_ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseReceived)
                    {
                        await _ws.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            reason,
                            CancellationToken.None
                        );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("có lỗi khi ngắt socket :" +ex);
                }
                finally
                {
                    _ws.Dispose();
                    _ws = new ClientWebSocket();
                    ClientState = "off";
                }
            }
        }

        public async Task StopCamera(string camId)
        {
            var message = new
            {
                cmd = "stop",
                camera = camId
            };
            string json = JsonSerializer.Serialize(message);
            await _ws.SendAsync(Encoding.UTF8.GetBytes(json),
            WebSocketMessageType.Text, true, CancellationToken.None);
            ClientState="off";
            // báo lên dừng cam_id nào
            Console.WriteLine($"dừng camera có {camId}");
        }
        public async Task StopServer()
        {
            try
            {
                    var message = new { cmd = "shutdown" };
                    string json = JsonSerializer.Serialize(message);
                    await _ws.SendAsync(
                        Encoding.UTF8.GetBytes(json),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi lệnh shutdown: " + ex.Message);
            }

            await CloseAsync();
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 4];
            var sb = new StringBuilder();

            while (_ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }

                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    string json = sb.ToString();
                    sb.Clear();

                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        // nếu nhận lỗi là error
                        if (doc.RootElement.TryGetProperty("error", out var errorProp))
                        {
                            string err = errorProp.GetString()!;
                            MessageBox.Show($"Lỗi từ server: {err}", "Lỗi Server", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        // nếu không nhận lỗi là gì thì là frame
                        else
                        {
                            string camId = doc.RootElement.GetProperty("camera").GetString()!;
                            string frameBase64 = doc.RootElement.GetProperty("frame").GetString()!;
                            //MessageBox.Show($"Đã nhận frame từ camera {camId}", "Thông báo");
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = new MemoryStream(Convert.FromBase64String(frameBase64));
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.Freeze();

                            FrameReceived?.Invoke(camId, image);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi parse JSON: " + ex.Message);
                    }
                }
            }
        }

    }
}
