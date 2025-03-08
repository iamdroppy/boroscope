# ❌ Boroscope Datagram Control ❌

This code is available for **archiving purposes** only.

# Communications API of Boroscope

## How it works

It connects to the boroscope endpoint (`192.168.101.123`) at port `8030` via **UDP**.

Following is the datagrams used for communication.

⚠️  **This is merely for studying purposes of a boroscope - names will NOT be mentioned** ⚠️

## Datagrams

This was a Frame buffer reader/writer to a handheld Boroscope (which is like an endoscope).

- **CONTROL FRAME DATAGRAM** is a packet of `0x99 0x99 [CTRL]` followed by 21 `NUL` bytes.

▶️  Start (`CTRL` is `0x01`):

 > 0x99, 0x99, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 >
 > 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
 >
 > 0x00, 0x00

⏸️ Finish (`CTRL` is `0x02`):

 > 0x99, 0x99, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
 >
 > 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
 >
 > 0x00, 0x00

## Received Packet

 - Header (51 bytes)
 - body (51 bytes)

The order of the frame is header offset 0x21, and tag is in offset 12 (dec)

Frame starts at 0x1D then the length is a 16 bit (short) of offset 12 and 13 (dec)

Line 96 checks if the len of the dgram is different than the received length, if so, discards frame

On line 104 checks if it contains a pattern of END OF JPEG (as it sends in **JFIF**) : `FF D9`

Then it tries to reassemble the image and store it in a view.

It will loop receiving **JFIF** **FF D8** to **FF D9** frames until `CTRL` `0x02` is sent.
