# Boroscope Datagram Control

📚 This code is available for **archiving purposes** only. 📚

## Overview

The Boroscope Datagram Control is a C# library that connects to a handheld Boroscope (like an endoscope) via UDP and reads/writes data using a Frame buffer reader/writer. This library provides a simple API for controlling the boroscope device.

### ⚙️ Dependencies

It uses, over simplicity, [Spectre.Console](https://spectreconsole.net/).

`dotnet add package Spectre.Console --version 0.49.1`

It was now adapted (for archiving purposes) to use [Spectre.Console](https://spectreconsole.net/) instead of a `.cs` that belongeth to a legacy `Windows Forms Application`

## 💻  Connection Details

* **Endpoint**: `192.168.10.123`
* **Port**: `8030`

### 🔍  Datagrams

The library uses two types of datagrams to communicate with the boroscope:

#### 💉 Control Frame Datagram

A packet of `0x99 0x99 [CTRL]` followed by 21 `NUL` bytes is sent to start and finish communication. The control frame consists of:

* **Start (`CTRL` is `0x01`)**:
	+ `0x99, 0x99, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,`
	+ `0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,`
	+ `0x00, 0x00`
* **Finish (`CTRL` is `0x02`)**:
	+ `0x99, 0x99, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,`
	+ `0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,`
	+ `0x00, 0x00`

##### 📈 Header (first 51 bytes)

The header contains metadata about the packet.

##### 📈 Body (after 51 bytes)

The body is the actual data being transmitted.

#### Basically...

When a frame arrives, the library checks if it contains a pattern of `FF D9` to indicate the end of a JPEG image. If so, it reassembles the image and stores it in a view.

Once the transmission has completed, the library waits for the `CTRL` `0x02` signal to start sending again.

## Usage

To use this library, simply create an instance of the `BoroscopeDatagramControl` class and call the `Start()` method to begin communication with the boroscope. You can then read or write data using the `Read()`, `Write()`, and `Close()` methods.

```csharp
using BoroscopeDatagramControl;
using Spectre.Console;

try
{
	FrameCaptureSystem capture = new();
	await capture.StartRecording();
}
catch (Exception ex)
{
	AnsiConsole.WriteException(ex);
}
```

Note: This is just a basic example of how to use the library. You should consult the documentation for more information on available methods and parameters.
