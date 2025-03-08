# Boroscope Datagram Control
=====================================

This code is available for **archiving purposes** only.

## Overview
------------

The Boroscope Datagram Control is a C# library that connects to a handheld Boroscope (like an endoscope) via UDP and reads/writes data using a Frame buffer reader/writer. This library provides a simple API for controlling the boroscope device.

### Connection Details

* **Endpoint**: `192.168.10.123`
* **Port**: `8030`

## Datagrams
------------

The library uses two types of datagrams to communicate with the boroscope:

### Control Frame Datagram

A packet of `0x99 0x99 [CTRL]` followed by 21 `NUL` bytes is sent to start and finish communication. The control frame consists of:

* **Start (`CTRL` is `0x01`)**:
	+ `0x99, 0x99, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,`
	+ `0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,`
	+ `0x00, 0x00`
* **Finish (`CTRL` is `0x02`)**:
	+ `0x99, 0x99, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,`
	+ `0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,`
	+ `0x00, 0x00`

### Header (51 bytes)

The header contains metadata about the packet.

### Body (51 bytes)

The body is the actual data being transmitted.

## Analysis
------------

When a frame arrives, the library checks if it contains a pattern of `FF D9` to indicate the end of a JPEG image. If so, it reassembles the image and stores it in a view.

Once the transmission has completed, the library waits for the `CTRL` `0x02` signal to start sending again.

## Usage
--------

To use this library, simply create an instance of the `BoroscopeDatagramControl` class and call the `Start()` method to begin communication with the boroscope. You can then read or write data using the `Read()`, `Write()`, and `Close()` methods.

```csharp
using BoroscopeDatagramControl;

class Program
{
    static void Main()
    {
        var control = new BoroscopeDatagramControl();
        control.Start();

        // Read some data from the boroscope
        var data = control.Read(1024);
        Console.WriteLine(data);

        // Write some data to the boroscope
        control.Write("Hello, World!", 13);

        control.Close();
    }
}
```

Note: This is just a basic example of how to use the library. You should consult the documentation for more information on available methods and parameters.
