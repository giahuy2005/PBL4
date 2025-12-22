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
        private ClientWebSocket _ws = new ClientWebSocket();
        private TaskCompletionSource<bool>? _checkCameraTcs;
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
            // check lỗi
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
        public async Task StartCamera(string camId, string url,string user,string admin )
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
            string fullurl = BuildCameraUrl(url, user, admin);
            var message = new
            {
                cmd = "add",
                camera = camId,
                url = fullurl
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
        private string BuildCameraUrl(string url, string user, string pass)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            bool hasCredential = !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass);

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                return url; // nếu URL không hợp lệ thì trả nguyên

            string scheme = uri.Scheme.ToLower();

            // Không chèn cho onvif (theo yêu cầu trước)
            if (scheme == "onvif")
                return url;

            if (!hasCredential)
                return url;

            // Nếu URL đã có userinfo thì thay thế
            var hostWithPort = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";
            // Cách an toàn hơn: giữ path và query
            var pathAndQuery = uri.PathAndQuery; // bao gồm / và ?...

            // Build lại với user:pass và port nếu có
            return $"{uri.Scheme}://{user}:{pass}@{hostWithPort}{pathAndQuery}";
        }
        public async Task<bool> CheckCamera(string camID,string url,string user,string pass)
        {
            if (!check_ws())
                return false;

            _checkCameraTcs = new TaskCompletionSource<bool>();
            string fullurl =  BuildCameraUrl(url,user,pass);
            var message = new
            {
                cmd = "check",
                camera = camID,
                url = fullurl
            };

            string json = JsonSerializer.Serialize(message);
            await _ws.SendAsync(
                Encoding.UTF8.GetBytes(json),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            var task = await Task.WhenAny(_checkCameraTcs.Task, Task.Delay(21000));

            if (task != _checkCameraTcs.Task)
                return false;

            return await _checkCameraTcs.Task;
        }
        public async Task<bool> CancelCheckCamera(string camId)
        {
            if (!check_ws())
                return false;

            var message = new
            {
                cmd = "cancel_check",
                camera = camId
            };

            string json = JsonSerializer.Serialize(message);

            await _ws.SendAsync(
                Encoding.UTF8.GetBytes(json),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            // Hủy luôn TaskCompletionSource để UI không bị treo
            _checkCameraTcs?.TrySetCanceled();
            return true;
        }



        private async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 8];
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
                            _checkCameraTcs?.TrySetResult(false);
                            continue;
                        }
                        // nếu không nhận lỗi là gì thì là frame
                        else
                        {
                            string cmd = doc.RootElement.TryGetProperty("cmd", out var cmdProp)
                            ? cmdProp.GetString()!
                            : "";

                            if (cmd == "check_response")
                            {
                                string status = doc.RootElement.GetProperty("status").GetString();

                                if (doc.RootElement.TryGetProperty("error", out var errProp))
                                {
                                    string errorMsg = errProp.GetString()!;
                                    MessageBox.Show($"Camera lỗi: {errorMsg}", "Lỗi Camera", MessageBoxButton.OK, MessageBoxImage.Warning);

                                    _checkCameraTcs?.SetResult(false);
                                    continue;
                                }

                                if (status != "ok")
                                {
                                    _checkCameraTcs?.SetResult(false);
                                    continue;
                                }
                                _checkCameraTcs?.SetResult(true);
                                continue;
                            }
                            else if (cmd == "check_cancelled")
                            {
                                string camId = doc.RootElement.GetProperty("camera").GetString()!;
                                _checkCameraTcs?.TrySetCanceled();
                                MessageBox.Show($"Đã hủy kiểm tra camera {camId}",
                                    "Huỷ kiểm tra", MessageBoxButton.OK, MessageBoxImage.Information);
                                continue;
                            }
                            else if (cmd == "add_success")
                            {
                                MessageBox.Show("add thành công");
                                continue;
                            }
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
