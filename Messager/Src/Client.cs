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
        

        public readonly IPAddress serverIP;
        public readonly int port;

        public Action<MessageInfo> recieveMessage { get; set; }
/*        public Action<AttachmentInfo, MemoryStream> recieveAudioMessage { get; set; }
        public Action<AttachmentInfo, MemoryStream> recieveVideoMessage { get; set; }
        public Action<AttachmentInfo, MemoryStream> recieveImageMessage { get; set; }
        public Action<AttachmentInfo, MemoryStream> recieveFileMessage { get; set; }*/

        bool isAutorized = false;
        bool isReadingAvailable = false;

        public bool IsAutorized { get => isAutorized; private set => isAutorized = value; }
        public bool IsReadingAvailable { get => isReadingAvailable; set => isReadingAvailable = value; }

        public Client(string ip, int port)
        {
            this.port = port;
            if (IPAddress.TryParse(ip, out IPAddress serverIP))
            {
                this.serverIP = serverIP;
                SetupClient();
            }
        }

        void SetupClient()
        {
            client = new TcpClient();
            try
            {
                client.Connect(serverIP, port);
                stream = client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine("connection error: " + e.Message);
                return;
            }
        }

        public void SendRegistrationData(string login, string email, string password)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
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
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
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
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
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
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
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
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
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
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write("SEARCH");
                binaryWriter.Write(namePattern);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendMessage(long chatId,
                                string messageText,
                                long attachmentsCount = 0,
                                DataPrefix type = 0,
                                string name = null,
                                string extension = null,
                                byte[] data = null)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.DefaultMessage);
                binaryWriter.Write(chatId);
                binaryWriter.Write(messageText);
                binaryWriter.Write(attachmentsCount);
                for (int i = 0; i < attachmentsCount; i++)
                {
                    binaryWriter.Write((int)type);
                    binaryWriter.Write(name);
                    binaryWriter.Write(extension);
                    binaryWriter.Write(data.LongLength);
                    binaryWriter.Write(data);
                }
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

            if ((MessagePrefix)binaryReader.ReadInt32() == MessagePrefix.SystemMessage)
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

                while (true)
                {
                    if (IsAutorized && IsReadingAvailable && stream.DataAvailable)
                    {
                        MessagePrefix messagePrefix = (MessagePrefix)binaryReader.ReadInt32();
                        switch (messagePrefix)
                        {
                            case MessagePrefix.DefaultMessage:
                                {
                                    long chatId = binaryReader.ReadInt64();
                                    long messageId = binaryReader.ReadInt64();
                                    string senderName = binaryReader.ReadString();
                                    string messageText = binaryReader.ReadString();
                                    string sendDate = binaryReader.ReadString();

                                    MessageInfo messageInfo = new MessageInfo(messageId,
                                                                              chatId,
                                                                              senderName,
                                                                              sendDate,
                                                                              messageText,
                                                                              new List<AttachmentInfo>());


                                    int attachmentsCount = binaryReader.ReadInt32();
                                    for (int i = 0; i < attachmentsCount; i++)
                                    {
                                        string filename = binaryReader.ReadString();
                                        string name = binaryReader.ReadString();
                                        string extension = binaryReader.ReadString();
                                        DataPrefix type = (DataPrefix)binaryReader.ReadInt32();

                                        long length = binaryReader.ReadInt64();

                                        using (MemoryStream memoryStream = new MemoryStream())
                                        {
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

                                            string path = Path.Combine(MainWindow.attachmentsPath,
                                                                       Path.ChangeExtension(filename,
                                                                       extension));

                                            if (!File.Exists(path))
                                            {
                                                using (FileStream fileStream = new FileStream(path,
                                                                                              FileMode.Create,
                                                                                              FileAccess.Write))
                                                {
                                                    memoryStream.WriteTo(fileStream);
                                                }
                                            }
                                            messageInfo.AttachmentsInfo.Add(new AttachmentInfo(filename,
                                                                                               name,
                                                                                               extension,
                                                                                               type));
                                        }
                                    }

                                    recieveMessage(messageInfo);

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
