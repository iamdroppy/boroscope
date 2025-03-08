using System;
using System.Collections.Generic;
using Spectre.Console;

public class FrameCaptureSystem
{
    /// <summary>
    /// The main frame capture system class.
    /// </summary>
    private UdpClient _udpClient;
    private Dictionary<byte, SortedDictionary<byte, byte[]>> _frameOrder; // Stores captured frames in memory
    private int _frameIndex = 0; // Track the current frame index
    private static const string RemoteIpAddress = "192.168.10.123";

    /// <summary>
    /// Starts the frame capture process.
    /// </summary>
    public async Task StartRecording()
    {
        AnsiConsole.MarkupLine("[green]Frame Capture System[/][green underline] started.[/]");
        AnsiConsole.MarkupLine("-------------------------------");

        try
        {
            // Establish connection with remote source using UDP
            _udpClient = new UdpClient();
            await ConnectToRemoteSource();

            while (true)
            {
                AnsiConsole.MarkupLine("-------------------------------");
                AnsiConsole.MarkupLine("[green]Waiting for incoming frames[/][green blink]...");
                AnsiConsole.MarkupLine("-------------------------------");

                // Receive frames from remote source and process them
                await ProcessReceivedFrames();

                AnsiConsole.MarkupLine("[green]Processing complete.[/]");
                AnsiConsole.MarkupLine("-------------------------------");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            throw; // throws if it cannot start capturing frames...
        }
    }

    /// <summary>
    /// Connects to the remote source.
    /// </summary>
    private async Task ConnectToRemoteSource()
    {
        var client = new UdpClient();

        try
        {
            // Attempt to connect to the remote source
            await client.Connect(RemoteIpAddress, 8030);
            _udpClient = client; // Store the connected client

            AnsiConsole.MarkupLine($"[green]Connected[/][green dim] to [/][white bold]{RemoteIpAddress}[/]:8030.");
            AnsiConsole.MarkupLine("-------------------------------");

            AnsiConsole.MarkupLine("[yellow]Waiting for incoming frames[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    /// <summary>
    /// Processes received frames from the remote source.
    /// </summary>
    private async Task ProcessReceivedFrames()
    {
        try
        {
            // Receive a frame from the remote source
            byte[] dgram = _udpClient.Receive(ref RemoteSourceAddress);

            AnsiConsole.MarkupLine($"[green]Received[/][blue] [/][green underline]packet from[/][green] [/][green]{RemoteIpAddress}[/] [green bold]boroscope[/].");
            AnsiConsole.WriteLine("-------------------------------");

            // Extract header, body, and order information
            var header = dgram.Take(51).ToArray();
            var body = dgram.Skip(51).ToArray();
            
            // maybe cut from FF D8 towards FF D9? Broken frames from JFIF (JPEGs) are also born there...
            // maybe libavcodec?
            if (body.Contains(new byte[] { 0xFF, 0xD9 }))
            {
                AnsiConsole.MarkupLine("[purple bold underline]End of JPEG frame[/] ([yellow dim]0xFF 0xD9[/])");
                await SaveCapturedFrame(body);
            }
            else
            {
                AddFrameToMemory(header, body);

                AnsiConsole.MarkupLine("[green]Frame added[/][green dim] to [/][yellow]{0:X8}[/].");
            }

            AnsiConsole.MarkupLine($"[green]Processing [/][italic green]{header.Length + body.Length} bytes[/][green] complete.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            AnsiConsole.MarkupLine("[red]Error occurred while processing frame:[/]");
            AnsiConsole.MarkupLine("[blue]Frame details: [/][yellow]header[/][green]/{0:X8}[/]", header[12], body.Length);
        }
    }

    /// <summary>
    /// Saves the captured frame to a file.
    /// </summary>
    private async Task SaveCapturedFrame(byte[] data)
    {
        try
        {
            var fileStream = new FileStream($"./output/{_frameIndex}.jpg", FileMode.Create, FileAccess.Write);
            await fileStream.WriteAsync(data, 0, data.Length);
            AnsiConsole.MarkupLine("[green]Frame saved[/][green dim] as [/][yellow]{0:X8}.jpg[/].", _frameIndex);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }

        _frameIndex++;
    }

    /// <summary>
    /// Adds a frame to the memory.
    /// </summary>
    private void AddFrameToMemory(byte[] header, byte[] body)
    {
        var sortedDictionary = new SortedDictionary<byte, byte[]>(body.Length); // Create a new sorted dictionary

        if (!sortedDictionary.ContainsKey(header[12]))
        {
            sortedDictionary.Add(header[12], body);
        }
        else
        {
            sortedDictionary[header[12]] += body;
        }

        _frameOrder.TryAdd(header[0], sortedDictionary); // Add to the main frame order dictionary

        AnsiConsole.MarkupLine("[green]Frame added[/][blue] to [/][yellow]{0:X8}[/].");
    }
}
