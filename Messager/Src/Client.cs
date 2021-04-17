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
using ClientServerLib;
using Messager;

namespace Client
{
    public class Client
    {
        TcpClient client;
        NetworkStream stream;

        public Action<MessageInfo> recieveTextMessage { get; set; }
        public Action<string, byte[]> recieveAudioMessage { get; set; }
        public Action<string, byte[]> recieveVideoMessage { get; set; }
        public Action<string, byte[]> recieveImageMessage { get; set; }
        public Action<string, byte[]> recieveFileMessage { get; set; }

        bool isAutorized = false;
        bool isReadingAvailable = false;

        public bool IsAutorized { get => isAutorized; private set => isAutorized = value; }
        public bool IsReadingAvailable { get => isReadingAvailable; set => isReadingAvailable = value; }

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
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)DataPrefix.SystemMessage);
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
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)DataPrefix.SystemMessage);
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

        public void SendAddchatRequest(long user2Id)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)DataPrefix.SystemMessage);
                binaryWriter.Write("ADDCHAT");
                binaryWriter.Write(user2Id);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }        
        
        public void SendGetAllContactsRequest()
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)DataPrefix.SystemMessage);
                binaryWriter.Write("CONTACTSALL");
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendGetAllMessagesByChatRequest(long chatId)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)DataPrefix.SystemMessage);
                binaryWriter.Write("MESSAGESCHATALL");
                binaryWriter.Write(chatId);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendSearchRequest(string namePattern)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)DataPrefix.SystemMessage);
                binaryWriter.Write("SEARCH");
                binaryWriter.Write(namePattern);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendTextMessage(long chatId, string message)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)DataPrefix.Text);
                binaryWriter.Write(chatId);
                binaryWriter.Write(message);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendFile(DataPrefix dataPrefix, string extension, byte[] data)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)dataPrefix);
                binaryWriter.Write(data.LongLength);
                binaryWriter.Write(extension);
                binaryWriter.Write(data);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void RecieveConfirmationMessage(out List<string> result)
        {
            result = new List<string>();
            result.Add(AnswersPerformer.PerformIdentificationResult(IdentificationResult.TIMEOUT));

            BinaryReader binaryReader = new BinaryReader(stream);

            if ((DataPrefix)binaryReader.ReadInt32() == DataPrefix.SystemMessage)
            {
                string message = binaryReader.ReadString();

                string[] messageParts = message.Split('\n');

                switch (messageParts[0])
                {
                    case "REG":
                        {
                            string errorMessage = AnswersPerformer.PerformIdentificationResult(
                                (IdentificationResult)int.Parse(messageParts[1])
                                );

                            result[0] = errorMessage;

                            if (errorMessage == string.Empty)
                            {
                                for (int i = 2; i <= 6; i++)
                                {
                                    result.Add(messageParts[i]);
                                }
                            }

                            break;
                        }
                    case "AUT":
                        {
                            string errorMessage = AnswersPerformer.PerformIdentificationResult(
                                (IdentificationResult)int.Parse(messageParts[1])
                                );

                            result[0] = errorMessage;
                            if (errorMessage == string.Empty)
                            {
                                for (int i = 2; i <= 6; i++)
                                {
                                    result.Add(messageParts[i]);
                                }
                            }

                            break;
                        }
                    case "CONTACTSALL":
                        {
                            int contactsCount = int.Parse(messageParts[1]);

                            for (int i = 0; i < contactsCount; i++)
                            {
                                result.Add(messageParts[2 + i * 2] + '\n' + messageParts[2 + i * 2 + 1]);
                            }

                            break;
                        }
                    case "ADDCHAT":
                        {
                            if (long.Parse(messageParts[1]) >= 0)
                            {
                                result.Add(messageParts[2] + '\n' + messageParts[3]);
                            }

                            break;
                        }
                    case "MESSAGESCHATALL":
                        {
                            int messageCount = int.Parse(messageParts[1]);

                            for (int i = 0; i < messageCount; i++)
                            {
                                result.Add(messageParts[2 + i * 4] + '\n' + messageParts[2 + i * 4 + 1] + '\n' +
                                           messageParts[2 + i * 4 + 2] + '\n' + messageParts[2 + i * 4 + 3] + '\n');
                            }

                            break;
                        }
                    case "SEARCH":
                        {
                            int contactsCount = int.Parse(messageParts[1]);

                            for (int i = 0; i < contactsCount; i++)
                            {
                                result.Add(messageParts[2 + i * 2] + '\n' + messageParts[2 + i * 2 + 1]);
                            }

                            break;
                        }
                }

            }
        }

        public void StartRecieving()
        {
            if (!isAutorized)
            {
                IsAutorized = true;
                IsReadingAvailable = true;
                new Thread(RecieveMessage).Start();
            }
        }

        void RecieveMessage()
        {
            try
            {
                BinaryReader binaryReader = new BinaryReader(stream);
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                MemoryStream streamM = new MemoryStream();

                while (true)
                {
                    if (IsReadingAvailable && stream.DataAvailable)
                    {
                        DataPrefix dataPrefix = (DataPrefix)binaryReader.ReadInt32();
                        switch (dataPrefix)
                        {
                            case DataPrefix.Text:
                                {
                                    long chatId = binaryReader.ReadInt64();
                                    string senderName = binaryReader.ReadString();
                                    string messageInfo = binaryReader.ReadString();
                                    string sendDate = binaryReader.ReadString();

                                    recieveTextMessage(new MessageInfo(chatId, senderName, sendDate, messageInfo));

                                    break;
                                }
                            case DataPrefix.Audio:
                                {
                                    long length = binaryReader.ReadInt64();
                                    string extension = binaryReader.ReadString();

                                    MemoryStream memoryStream = new MemoryStream();
                                    byte[] buff = new byte[2048];
                                    int count = 0;
                                    do
                                    {
                                        if (length >= buff.Length)
                                        {
                                            count = binaryReader.Read(buff, 0, buff.Length);
                                        }
                                        else
                                        {
                                            count = binaryReader.Read(buff, 0, (int)length);
                                        }
                                        memoryStream.Write(buff, 0, count);
                                        length -= count;
                                    } while (length > 0);

                                    Thread thread = new Thread(() => recieveAudioMessage(extension, memoryStream.ToArray()));
                                    thread.SetApartmentState(ApartmentState.STA);
                                    thread.Start();
                                    thread.Join();

                                    memoryStream.Dispose();
                                    break;
                                }
                            case DataPrefix.Video:
                                {
                                    long length = binaryReader.ReadInt64();
                                    string extension = binaryReader.ReadString();

                                    MemoryStream memoryStream = new MemoryStream();
                                    byte[] buff = new byte[4096];
                                    int count = 0;
                                    do
                                    {
                                        if (length >= buff.Length)
                                        {
                                            count = binaryReader.Read(buff, 0, buff.Length);
                                        }
                                        else
                                        {
                                            count = binaryReader.Read(buff, 0, (int)length);
                                        }
                                        memoryStream.Write(buff, 0, count);
                                        length -= count;
                                    } while (length > 0);

                                    Thread thread = new Thread(() => recieveVideoMessage(extension, memoryStream.ToArray()));
                                    thread.SetApartmentState(ApartmentState.STA);
                                    thread.Start();
                                    thread.Join();

                                    memoryStream.Dispose();
                                    break;
                                }
                            case DataPrefix.Image:
                                {
                                    long length = binaryReader.ReadInt64();
                                    string extension = binaryReader.ReadString();

                                    MemoryStream memoryStream = new MemoryStream();
                                    byte[] buff = new byte[512];
                                    int count = 0;
                                    do
                                    {
                                        if (length >= buff.Length)
                                        {
                                            count = binaryReader.Read(buff, 0, buff.Length);
                                        }
                                        else
                                        {
                                            count = binaryReader.Read(buff, 0, (int)length);
                                        }
                                        memoryStream.Write(buff, 0, count);
                                        length -= count;
                                    } while (length > 0);

                                    Thread thread = new Thread(() => recieveImageMessage(extension, memoryStream.ToArray()));
                                    thread.SetApartmentState(ApartmentState.STA);
                                    thread.Start();
                                    thread.Join();

                                    memoryStream.Dispose();
                                    break;
                                }
                            case DataPrefix.File:
                                {
                                    long length = binaryReader.ReadInt64();
                                    string extension = binaryReader.ReadString();

                                    MemoryStream memoryStream = new MemoryStream();
                                    byte[] buff = new byte[2048];
                                    int count = 0;
                                    do
                                    {
                                        if (length >= buff.Length)
                                        {
                                            count = binaryReader.Read(buff, 0, buff.Length);
                                        }
                                        else
                                        {
                                            count = binaryReader.Read(buff, 0, (int)length);
                                        }
                                        memoryStream.Write(buff, 0, count);
                                        length -= count;
                                    } while (length > 0);

                                    Thread thread = new Thread(() => recieveFileMessage(extension, memoryStream.ToArray()));
                                    thread.SetApartmentState(ApartmentState.STA);
                                    thread.Start();
                                    thread.Join();

                                    memoryStream.Dispose();
                                    break;
                                }
                        }
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
