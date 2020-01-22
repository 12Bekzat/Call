using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace CallProject
{
    public partial class MainWindow : Window
    {
        private bool connected;
        private string IpAddress = "127.0.0.1";
        Socket client;
        WaveIn input;
        WaveOut output;
        BufferedWaveProvider bufferStream;
        Thread in_thread;
        Socket listeningSocket;

        public MainWindow()
        {
            InitializeComponent();

            input = new WaveIn();
            input.WaveFormat = new WaveFormat(8000, 16, 1);
            input.DataAvailable += Voice_Input;
            output = new WaveOut();
            bufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
            output.Init(bufferStream);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            connected = true;
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            in_thread = new Thread(new ThreadStart(Listening));
            in_thread.Start();
        }

        private void Voice_Input(object sender, WaveInEventArgs e)
        {
            try
            {
                IPEndPoint remote_point = new IPEndPoint(IPAddress.Parse(ipaddress.Text), 5555);
                client.SendTo(e.Buffer, remote_point);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StartCall(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(ipaddress.Text))
            {
                MessageBox.Show("Укажите адресс");
                return;
            }
            IpAddress = ipaddress.Text;

            input.StartRecording();
        }

        private void Listening()
        {
            IPEndPoint localIP = new IPEndPoint(IPAddress.Parse(IpAddress), 3231);
            listeningSocket.Bind(localIP);
            output.Play();
            EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
            while (connected == true)
            {
                try
                {
                    byte[] data = new byte[65535];
                    int received = listeningSocket.ReceiveFrom(data, ref remoteIp);
                    bufferStream.AddSamples(data, 0, received);
                }
                catch (SocketException ex)
                { }
            }
        }
    }
}
