using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PBL4.Services
{
    class RunPython
    {
        private static RunPython? _instance;
        private Process? _process;
        private readonly string _logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "python_server.log"
        );

        private RunPython() { }

        public static RunPython Instance => _instance ??= new RunPython();

        private void WriteLog(string message)
        {
            string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(log);
            try
            {
                File.AppendAllText(_logFilePath, log + Environment.NewLine);
            }
            catch { }
        }

        public void RunPythonScript(string python_path, string script_path)
        {
            if (_process != null && !_process.HasExited)
            {
                WriteLog("Python server đã chạy, không khởi động lại.");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = python_path,
                    Arguments = $"\"{script_path}\"",
                    WorkingDirectory = Path.GetDirectoryName(script_path),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _process = new Process();
                _process.StartInfo = psi;
                _process.EnableRaisingEvents = true;

                _process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        WriteLog("[PYTHON] " + e.Data);
                };
                _process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        WriteLog("[PYTHON-ERR] " + e.Data);
                };
                _process.Exited += (s, e) =>
                {
                    WriteLog($"Python process exited with code {_process.ExitCode}");
                    _process = null;
                };

                _process.Start();
                WriteLog($"Python process started with PID {_process.Id}");
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi khi chạy Python: " + ex);
            }
        }

        public void StopPython()
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    _process.Kill(entireProcessTree: true);
                    _process.WaitForExit(3000);
                    WriteLog($"Đã kill Python server (PID {_process.Id})");
                }
                catch (Exception ex)
                {
                    WriteLog("Lỗi khi dừng Python: " + ex.Message);
                }
                finally
                {
                    _process.Dispose();
                    _process = null;
                }
            }
        }




    }
}
