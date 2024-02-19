using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class PipeWorkerService : BackgroundService
{
    private readonly ILogger<PipeWorkerService> _logger;
    private readonly string logFilePath = @"C:\Path\To\Your\Log\Directory\logs.txt"; // Adjust the path as needed

    public PipeWorkerService(ILogger<PipeWorkerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogServiceState("Service started.");
        await Task.Run(() => StartServerAsync(stoppingToken), stoppingToken);
    }

    private async Task StartServerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var namedPipeServer = new NamedPipeServerStream("MyPipe", PipeDirection.InOut, -1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                try
                {
                    LogServiceState("Waiting for pipe client connection...");
                    await namedPipeServer.WaitForConnectionAsync(cancellationToken);
                    LogServiceState("Pipe client connected.");

                    // Example of handling communication with the client
                    // Read from and/or write to namedPipeServer here

                    // After handling communication, disconnect to allow for new connections
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation
                    break;
                }
                catch (Exception ex)
                {
                    LogError($"Error in pipe server: {ex.Message}");
                }
                finally
                {
                    if (namedPipeServer.IsConnected)
                    {
                        namedPipeServer.Disconnect();
                        LogClientDisconnected();
                    }
                }
            }

            // Optional: Include a delay before attempting to recreate the pipe server
            await Task.Delay(1000, cancellationToken); // 1 second delay before retrying
        }
    }

    private void LogClientDisconnected()
    {
        string logText = $"{DateTime.Now}: Client disconnected.";
        _logger.LogInformation(logText);
        WriteToLogFile(logText);
    }

    private void LogServiceState(string message)
    {
        string logText = $"{DateTime.Now}: {message}";
        _logger.LogInformation(logText);
        WriteToLogFile(logText);
    }

    private void LogError(string message)
    {
        _logger.LogError(message);
        WriteToLogFile(message);
    }

    private void WriteToLogFile(string logText)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)); // Ensure the directory exists
        File.AppendAllText(logFilePath, logText + Environment.NewLine);
    }
}
