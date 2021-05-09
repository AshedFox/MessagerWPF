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

        public Action<string, string, MemoryStream> receiveAttachment;
        public Action<MessageInfo> receiveMessage;
        public Action<MessageInfo> receiveMessagesUpdate;
        public Action<long, Dictionary<long, string>> recieveChatUsers;

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
                Task connect = client.ConnectAsync(serverIP, port);
                connect.Start();
                connect.Wait();
                //client.Connect(serverIP, port);
                stream = client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine("connection error: " + e.Message);
                return;
            }
        }

        public void SendRegistrationData(string login, string email, string password, string name)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.Register);
                binaryWriter.Write(login);
                binaryWriter.Write(email);
                binaryWriter.Write(password);
                binaryWriter.Write(name);
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
                binaryWriter.Write((int)SystemMessageType.Autorize);
                binaryWriter.Write(login);
                binaryWriter.Write(password);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendAddChatRequest(long user2Id)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.AddChat);
                binaryWriter.Write(user2Id);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendGetChatUsersRequest(long chatId)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.GetChatUsers);
                binaryWriter.Write(chatId);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendGetAllUserContactsRequest()
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.GetAllUserContacts);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendGetNewChatMessagesRequest(long chatId, long startMesageId = 0)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.GetNewChatMessages);
                binaryWriter.Write(chatId);
                binaryWriter.Write(startMesageId);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendUpdateChatMessagesRequest(long chatId, long startMesageId = 0)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.RequestChatMessagesUpdate);
                binaryWriter.Write(chatId);
                binaryWriter.Write(startMesageId);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendAttachmentDataRequest(Attachment attachmentInfo)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.GetAttachment);
                binaryWriter.Write(attachmentInfo.Filename);
                binaryWriter.Write(attachmentInfo.Extension);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }   

        public void SendSearchContactsRequest(string namePattern)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.SearchContacts);
                binaryWriter.Write(namePattern);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendDeleteContactRequest(long chatId)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.DeleteContact);
                binaryWriter.Write(chatId);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendUpdateLoginRequest(string newValue)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.UpdateUserInfo);
                binaryWriter.Write((int)UpdateType.Login);
                binaryWriter.Write(newValue);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }        
        
        public void SendUpdateEmailRequest(string newValue)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.UpdateUserInfo);
                binaryWriter.Write((int)UpdateType.Email);
                binaryWriter.Write(newValue);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }          
        public void SendUpdatePasswordRequest(string oldValue, string newValue)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.UpdateUserInfo);
                binaryWriter.Write((int)UpdateType.Password);
                binaryWriter.Write(oldValue);
                binaryWriter.Write(newValue);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }        
        
        public void SendUpdateNameRequest(string newValue)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.UpdateUserInfo);
                binaryWriter.Write((int)UpdateType.Name);
                binaryWriter.Write(newValue);
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void SendMessage(long chatId,
                                string messageText,
                                List<(AttachmentInfo info, MemoryStream data)> attachmentsInfo = null)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                binaryWriter.Write((int)MessagePrefix.DefaultMessage);
                binaryWriter.Write(chatId);
                binaryWriter.Write(messageText);
                if (attachmentsInfo == null || attachmentsInfo.Count == 0)
                    binaryWriter.Write((long)0);
                else
                {
                    binaryWriter.Write((long)attachmentsInfo.Count);
                    foreach (var item in attachmentsInfo)
                    {
                        binaryWriter.Write((int)item.info.Type);
                        binaryWriter.Write(item.info.Name);
                        binaryWriter.Write(item.info.Extension);
                        byte[] buff = item.data.ToArray();
                        binaryWriter.Write(buff.LongLength);
                        binaryWriter.Write(buff);
                    }
                }
                binaryWriter.Flush();
            }
            catch
            {
                Debug.WriteLine("Ошибка отправки");
            }
        }

        public void ReceiveConfirmationMessage(out List<string> result)
        {
            result = new List<string>();
            result.Add(AnswersPerformer.PerformIdentificationResult(IdentificationResult.TIMEOUT));
            if (stream != null)
            {
                BinaryReader binaryReader = new BinaryReader(stream);

                if ((MessagePrefix)binaryReader.ReadInt32() == MessagePrefix.SystemMessage)
                {
                    SystemMessageType type = (SystemMessageType)binaryReader.ReadInt32();
                    string message = binaryReader.ReadString();

                    string[] messageParts = message.Split('\n');

                    switch (type)
                    {
                        case SystemMessageType.Register:
                            {
                                string errorMessage = AnswersPerformer.PerformIdentificationResult(
                                    (IdentificationResult)int.Parse(messageParts[0])
                                    );

                                result[0] = errorMessage;

                                if (errorMessage == string.Empty)
                                {
                                    for (int i = 1; i <= 5; i++)
                                    {
                                        result.Add(messageParts[i]);
                                    }
                                }

                                break;
                            }
                        case SystemMessageType.Autorize:
                            {
                                string errorMessage = AnswersPerformer.PerformIdentificationResult(
                                    (IdentificationResult)int.Parse(messageParts[0])
                                    );

                                result[0] = errorMessage;
                                if (errorMessage == string.Empty)
                                {
                                    for (int i = 1; i <= 5; i++)
                                    {
                                        result.Add(messageParts[i]);
                                    }
                                }

                                break;
                            }
                        case SystemMessageType.GetAllUserContacts:
                            {
                                int contactsCount = int.Parse(messageParts[0]);

                                for (int i = 0; i < contactsCount; i++)
                                {
                                    result.Add(messageParts[1 + i * 2] + '\n' + messageParts[1 + i * 2 + 1]);
                                }

                                break;
                            }
                        case SystemMessageType.AddChat:
                            {
                                if (long.Parse(messageParts[0]) >= 0)
                                {
                                    result.Add(messageParts[1] + '\n' + messageParts[2]);
                                }

                                break;
                            }
                        case SystemMessageType.SearchContacts:
                            {
                                int contactsCount = int.Parse(messageParts[0]);

                                for (int i = 0; i < contactsCount; i++)
                                {
                                    result.Add(messageParts[1 + i * 2] + '\n' + messageParts[1 + i * 2 + 1]);
                                }

                                break;
                            }
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
                new Thread(ReceiveMessage).Start();
            }
        }

        void ReceiveMessage()
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
                            case MessagePrefix.SystemMessage:
                                {
                                    SystemMessageType msgType = (SystemMessageType)binaryReader.ReadInt32();
                                    switch (msgType)
                                    {
                                        case SystemMessageType.GetChatUsers:
                                            {
                                                long chatId = binaryReader.ReadInt64();
                                                int usersCount = binaryReader.ReadInt32();
                                                Dictionary<long, string> chatUsers = new Dictionary<long, string>();
                                                for (int i = 0; i < usersCount; i++)
                                                {
                                                    long userId = binaryReader.ReadInt64();
                                                    string name = binaryReader.ReadString();
                                                    chatUsers.Add(userId, name);
                                                }
                                                recieveChatUsers?.Invoke(chatId, chatUsers);
                                                break;
                                            }
                                        case SystemMessageType.GetNewChatMessages:
                                            {
                                                long chatId = binaryReader.ReadInt64();
                                                long messageId = binaryReader.ReadInt64();
                                                long senderId = binaryReader.ReadInt64();
                                                string messageText = binaryReader.ReadString();
                                                string sendDate = binaryReader.ReadString();

                                                MessageInfo messageInfo = new MessageInfo(messageId,
                                                                                          chatId,
                                                                                          senderId,
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
                                                    messageInfo.AttachmentsInfo.Add(new AttachmentInfo(filename, name, extension, type));
                                                }

                                                receiveMessagesUpdate(messageInfo);
                                                break;
                                            }
                                        case SystemMessageType.GetAttachment:
                                            {
                                                string filename = binaryReader.ReadString();
                                                string extension = binaryReader.ReadString();

                                                long length = binaryReader.ReadInt64();

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
                                                receiveAttachment?.Invoke(filename, extension, memoryStream);
                                                memoryStream.Dispose();

                                                break;
                                            }
                                        case SystemMessageType.UpdateUserInfo:
                                            {
                                                UpdateType updateType = (UpdateType)binaryReader.ReadInt32();
                                                bool result = binaryReader.ReadBoolean();
                                                string newValue = binaryReader.ReadString();
                                                PagesManager.Instance.SettingsPage.ShowConfirmationMessage(updateType, result, newValue);
                                                break;
                                            }
                                    }

                                    break;
                                }
                            case MessagePrefix.DefaultMessage:
                                {
                                    long chatId = binaryReader.ReadInt64();
                                    long messageId = binaryReader.ReadInt64();
                                    long senderId = binaryReader.ReadInt64();
                                    string messageText = binaryReader.ReadString();
                                    string sendDate = binaryReader.ReadString();

                                    MessageInfo messageInfo = new MessageInfo(messageId,
                                                                              chatId,
                                                                              senderId,
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

                                    receiveMessage?.Invoke(messageInfo);

                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
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
