using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrameTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FormClosed += Form1_FormClosed;
            _listener = new TcpListener(IPAddress.Any, 10020);
            _listener.Start();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_client != null)
            {
                SendDgram(_client, new byte[] { 0x99, 0x99, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            }
        }

        UdpClient _client;
        TcpListener _listener;

        public async Task StartRecording()
        {
            await Task.Yield();

            await Task.Run(async () =>
            {
                //Socket socket = _listener.AcceptSocket();
                MessageBox.Show("Accepted socket");
                IPEndPoint remote = new IPEndPoint(IPAddress.Parse("192.168.10.123"), 8030);
                _client = new UdpClient();
                _client.Client.ReceiveTimeout = 1000;
                _client.Client.SendTimeout = 3000;
                _client.Connect(remote);

                //starts new connection

                ulong i = 0;

                Dictionary<byte, SortedDictionary<byte, byte[]>> frameOrder = new Dictionary<byte, SortedDictionary<byte, byte[]>>();
                
                bool receiving = false;
                while (true)
                {
                    byte[] dgram;

                    try
                    {
                        if (!receiving)
                            SendDgram(_client, new byte[] { 0x99, 0x99, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                        dgram = _client.Receive(ref remote);
                    }
                    catch
                    {
                        continue;
                    }
                    receiving = true;



                    bool discardFrame = false;

                    if (dgram.Length > 0)
                    {
                        byte[] header = dgram.Take(51).ToArray();
                        byte[] body = dgram.Skip(51).ToArray();

                        byte order = header[0x21];
                        byte tag = header[12];

                        byte frame = header[0x1D];

                        short len = BitConverter.ToInt16(new byte[] { header[12], header[13] }, 0);

                        if (len != dgram.Length)
                            discardFrame = true;

                        if (!frameOrder.ContainsKey(frame))
                            frameOrder.Add(frame, new SortedDictionary<byte, byte[]>());

                        var frameData = frameOrder[frame];

                        if (ContainsPattern(body, new byte[] { 0xFF, 0xD9 }, out int position))
                        {
                            // ends stream


                            List<byte> bS = new List<byte>();
                            int packetId = 0;
                            foreach (byte[] b in frameData.Select(s => s.Value))
                            {
                                bS.AddRange(b);
                                packetId++;
                            }

                            bS.AddRange(body);

                            pictureBox1.Invoke((MethodInvoker)delegate
                            {
                                try { pictureBox1.Image = new Bitmap(new MemoryStream(bS.ToArray())); }
                                catch { }
                            });
                            //socket.Send(bS.ToArray());

                            //File.WriteAllBytes($"f{frame}.jpg", bS.ToArray());

                            frameOrder.Remove(frame);

                        }
                        else
                        {
                            if (frameData.ContainsKey(order))
                                frameData[order] = body;
                            else
                                frameData.Add(order, body);

                            Console.WriteLine(order);
                        }

                        i++;
                    }
                }

                // tries to stop current connection
                SendDgram(_client, new byte[] { 0x99, 0x99, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            });
        }

        public async Task SetImage()
        {
            //await Task.Yield();
            //await Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        try
            //        {
            //            if (_images.TryDequeue(out byte[] result))
            //            {
                            
            //            }
            //        }
            //        catch { }
            //    }
            //});
        }

        public static bool ContainsPattern(byte[] source, byte[] pattern, out int position, int startIndex = 0)
        {
            position = 0;
            for (int i = startIndex; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    position = i;
                    return true;
                }
            }

            return false;
        }

        static void SendDgram(UdpClient client, byte[] dgram)
        {
            client.Send(dgram, dgram.Length);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            StartRecording();
            SetImage();
        }
    }
}
