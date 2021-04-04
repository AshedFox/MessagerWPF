using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.FileFormats;
using NAudio.CoreAudioApi;
using NAudio;
using ClientServerLib;
using Messager;

namespace Client
{
    public class Client
    {
        TcpClient client;
        NetworkStream stream;

        public Action<string> recieveTextMessage { get; set; }
        public Action<byte[]> recieveAudioMessage { get; set; }

        bool isAutorized = false;

        public bool IsAutorized { get => isAutorized; private set => isAutorized = value; }

        public Client(string ip, int port)
        {
            if (IPAddress.TryParse(ip, out IPAddress serverIP))
            {
                client = new TcpClient();
                try
                {
                    client.Connect(serverIP, port);
                }
                catch
                {
                    return;
                }
                stream = client.GetStream();
            }
        }

        public void SendRegistrationData(string login, string email, string password)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(client.GetStream());
                binaryWriter.Write((byte)DataPrefixes.SystemMessage);
                binaryWriter.Write("REG");
                binaryWriter.Write(login);
                binaryWriter.Write(email);
                binaryWriter.Write(password);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendAutorizationData(string login, string password)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(client.GetStream());
                binaryWriter.Write((byte)DataPrefixes.SystemMessage);
                binaryWriter.Write("AUT");
                binaryWriter.Write(login);
                binaryWriter.Write(password);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendTextMessage(string message)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((byte)DataPrefixes.Text);
                binaryWriter.Write(message);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendAudio(byte[] data)
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            binaryWriter.Write((byte)DataPrefixes.Audio);
            binaryWriter.Write(data);
            binaryWriter.Flush();
        }

        public void RecieveConfirmationMessage(out string result)
        {
            IdentificationResult serverAnswer = IdentificationResult.ALL_OK;
            try
            {
                BinaryReader binaryReader = new BinaryReader(stream);

                if ((DataPrefixes)binaryReader.ReadByte() == DataPrefixes.SystemMessage)
                {
                    string message = binaryReader.ReadString();

                    serverAnswer = (IdentificationResult)int.Parse(message);
                }
                else serverAnswer = IdentificationResult.ANSWER_RECIEVING_ERROR;
            }
            catch
            {
                serverAnswer = IdentificationResult.ANSWER_RECIEVING_ERROR;
                Disconnect();
            }

            result = AnswersPerformer.PerformIdentificationResult(serverAnswer);
        }

        public void StartRecieving()
        {
            if (!isAutorized)
            {
                IsAutorized = true;
                new Thread(RecieveMessage).Start();
            }
        }

        void RecieveMessage()
        {
            try
            {
                BinaryReader binaryReader = new BinaryReader(stream);

                while (true)
                {                   
                    switch ((DataPrefixes)binaryReader.ReadByte())
                    {
                        case DataPrefixes.Text:
                            string message = binaryReader.ReadString();

                            if (message != string.Empty)
                            {
                                recieveTextMessage(message);
                                message = string.Empty;
                            }
                            break;
                        case DataPrefixes.Audio:
                            MemoryStream memoryStream = new MemoryStream();
                            byte[] buff = new byte[1024];
                            int count = 0;
                            do
                            {
                                count = binaryReader.Read(buff, 0, 1024);
                                memoryStream.Write(buff, 0, count);
                            } while (stream.DataAvailable);

                            Thread thread = new Thread(() => recieveAudioMessage(memoryStream.ToArray()));
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            thread.Join();

                            memoryStream.Dispose();
                            break;
                        case DataPrefixes.Video:
                            break;
                        case DataPrefixes.Image:
                            break;
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Ошибка приёма");
            }
        }

        public void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
