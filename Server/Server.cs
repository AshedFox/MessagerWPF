using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using ClientServerLib;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Linq;

namespace Server
{
    public class Server
    {
        readonly TcpListener server;
        readonly List<ClientObject> clients = new List<ClientObject>();

        readonly string attachmentsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments/");

        public Server(int port)
        {
            if (!Directory.Exists(attachmentsDir))
            {
                Directory.CreateDirectory(attachmentsDir);
            }
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(client);

                    new Thread(() => ProcessClient(clientObject)).Start();
                }
            }
            catch
            {
                server.Stop();

                foreach (var client in clients)
                {
                    if (client.Client.Connected)
                    {
                        client.Client.Close();
                    }
                }
            }
        }

        void AutorizeUser(ClientObject clientObject, out IdentificationResult resultCode, out ClientInfo clientInfo)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            string loginOrEmail = binaryReader.ReadString();
            string password = binaryReader.ReadString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            var result = dbManager.GetClientInfoByLogin(loginOrEmail);
            resultCode = result.resultCode;
            clientInfo = null;

            dbManager.CloseConnectionToDB();

            if (resultCode == IdentificationResult.ALL_OK)
            {
                if (result.info.password == password)
                {
                    clientInfo = new ClientInfo(result.info.id, result.info.login, result.info.email, "", result.info.name);
                    clientObject.Id = result.info.id;
                    clientObject.Name = result.info.name;
                    clientObject.IsAutorized = true;
                }
                else resultCode = IdentificationResult.INCORRECT_PASSWORD;
            }
        }

        void RegistrateUser(ClientObject clientObject, out IdentificationResult resultCode, out ClientInfo clientInfo)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            string login = binaryReader.ReadString();
            string email = binaryReader.ReadString();
            string password = binaryReader.ReadString();
            string name = binaryReader.ReadString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            var result = dbManager.AddClient(new ClientInfo(login, email, password, name));
            resultCode = result.resultCode;
            clientInfo = new ClientInfo(result.id, login, email, "", name);

            dbManager.CloseConnectionToDB();

            if (resultCode == IdentificationResult.ALL_OK)
            {
                clientObject.Id = result.id;
                clientObject.Name = login;
                clientObject.IsAutorized = true;
            }
        }

        void GetAllUserContacts(ClientObject clientObject, out int resultCode, out List<ContactInfo> contacts)
        {
            resultCode = 0;

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            contacts = dbManager.GetAllUserContacts(clientObject.Id);

            dbManager.CloseConnectionToDB();
        }

        void SearchContacts(ClientObject clientObject, out int resultCode, out List<ContactInfo> contacts)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            string namePattern = binaryReader.ReadString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            (resultCode, contacts) = dbManager.GetContactsByNamePart(clientObject.Id, namePattern);   

            dbManager.CloseConnectionToDB();
        }

        void AddChat(ClientObject clientObject, out int resultCode, out ContactInfo contactInfo)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            long user2Id = binaryReader.ReadInt64();
            long user1Id = clientObject.Id;

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            long chatId = dbManager.AddChat(user1Id, user2Id);
            var result = dbManager.GetContactById(chatId, user1Id);
            contactInfo = result.contactInfo;
            resultCode = result.resultCode;

            dbManager.CloseConnectionToDB();
        }
        private void GetChatsUsers(ClientObject clientObject, out long chatId, out int resultCode, out List<(long, string)> chatsUsers)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            chatId = binaryReader.ReadInt64();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            chatsUsers = dbManager.GetChatUsers(chatId);

            dbManager.CloseConnectionToDB();
            resultCode = 1;
        }

        void GetMessagesByChat(ClientObject clientObject, out int resultCode, out List<MessageInfo> messages)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            long chatId = binaryReader.ReadInt64();
            long startMessageId = binaryReader.ReadInt64();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            messages = dbManager.GetMessagesByChat(chatId, startMessageId);

            dbManager.CloseConnectionToDB();

            resultCode = 1;
        }

        void DeleteContact(ClientObject clientObject)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            long chatId = binaryReader.ReadInt64();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            dbManager.DeleteUserFromChat(chatId, clientObject.Id);

            dbManager.CloseConnectionToDB();
        }

        private (bool result, string newValue) UpdateLogin(ClientObject clientObject)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            string newLogin = binaryReader.ReadString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            bool result = dbManager.UpdateUserLogin(clientObject.Id, newLogin);

            dbManager.CloseConnectionToDB();
            return (result, newLogin);
        }
        private (bool result, string newValue) UpdateEmail(ClientObject clientObject)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            string newEmail = binaryReader.ReadString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            bool result = dbManager.UpdateUserEmail(clientObject.Id, newEmail);

            dbManager.CloseConnectionToDB();
            return (result, newEmail);
        }

        private (bool, string) UpdatePassword(ClientObject clientObject)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            string oldPassword = binaryReader.ReadString();
            string newPassword = binaryReader.ReadString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            bool result = dbManager.UpdateUserPassword(clientObject.Id, oldPassword, newPassword);

            dbManager.CloseConnectionToDB();
            return (result,"");
        }

        private (bool result, string newValue) UpdateName(ClientObject clientObject)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            string newName = binaryReader.ReadString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            bool result = dbManager.UpdateUserName(clientObject.Id, newName);
            if (result) clientObject.Name = newName;

            dbManager.CloseConnectionToDB();
            return (result, newName);
        }

        void AddMessage(ClientObject clientObject,
                        out MessageInfo messageInfo,
                        out List<long> usersIdsToSend)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            long chatId = binaryReader.ReadInt64();
            string messageText = binaryReader.ReadString();
            string datePattern = @"yyyy-MM-dd HH:mm:ss";
            string sendDate = DateTime.UtcNow.ToString(datePattern);

            long attachmentsCount = binaryReader.ReadInt64();
            List<AttachmentInfo> attachmentsInfo = new List<AttachmentInfo>();

            for (int i = 0; i < attachmentsCount; i++)
            {
                DataPrefix type = (DataPrefix)binaryReader.ReadInt32();
                string name = binaryReader.ReadString();
                string extension = binaryReader.ReadString();

                long length = binaryReader.ReadInt64();
                using MemoryStream memoryStream = new MemoryStream();

                byte[] buff = new byte[2048];
                int count;
                do
                {
                    if (length < buff.Length)
                    {
                        count = binaryReader.Read(buff, 0, (int)length);
                    }
                    else
                    {
                        count = binaryReader.Read(buff, 0, buff.Length);
                    }
                    memoryStream.Write(buff, 0, count);
                    length -= count;
                } while (length > 0);

                var md5 = MD5.Create();
                byte[] myHash = md5.ComputeHash(memoryStream.ToArray());
                StringBuilder builder = new StringBuilder();
                for (int j = 0; j < myHash.Length; j++)
                {
                    builder.Append(myHash[j].ToString("x2"));
                }
                string filename = builder.ToString();

                string path = Path.ChangeExtension(Path.Combine(attachmentsDir, filename), extension);
                if (!File.Exists(path))
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                    {
                        memoryStream.WriteTo(fileStream);
                    }
                }
                attachmentsInfo.Add(new AttachmentInfo(filename, name, extension, type));
            }

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            messageInfo = dbManager.AddMessage(chatId,
                                               clientObject.Id,
                                               messageText,
                                               sendDate,
                                               attachmentsInfo);

            usersIdsToSend = dbManager.GetChatUsersIds(chatId);

            dbManager.CloseConnectionToDB();
        }

        void ProcessClient(ClientObject clientObject)
        {
            using NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            BinaryWriter binaryWriter = new BinaryWriter(stream);

            try
            {
                while (clientObject.Client.Connected)
                {
                    MessagePrefix dataPrefix = (MessagePrefix)binaryReader.ReadInt32();
                    switch (dataPrefix)
                    {
                        case MessagePrefix.SystemMessage:
                            {
                                
                                SystemMessageType type = (SystemMessageType)binaryReader.ReadInt32();

                                switch (type)
                                {
                                    case SystemMessageType.Register:
                                        {
                                            IdentificationResult resultCode = IdentificationResult.TIMEOUT;
                                            ClientInfo clientInfo = new ClientInfo();

                                            Thread thread = new Thread(() =>
                                                   RegistrateUser(clientObject, out resultCode, out clientInfo));

                                            thread.Start();
                                            thread.Join(30000);

                                            if (resultCode == IdentificationResult.ALL_OK)
                                            {
                                                clients.Add(clientObject);
                                                Console.WriteLine(clientObject.Name + " connected and registrate");
                                            }
                                            else
                                            {
                                                Console.WriteLine(clientObject.Name + " failed to registrate");
                                            }
                                            SendSystemMessage(clientObject, type,
                                                $"{(int)resultCode}\n{clientInfo}\n");

                                            break;
                                        }
                                    case SystemMessageType.Autorize:
                                        {
                                            IdentificationResult resultCode = IdentificationResult.TIMEOUT;
                                            ClientInfo clientInfo = new ClientInfo();

                                            Thread thread = new Thread(() =>
                                                   AutorizeUser(clientObject, out resultCode, out clientInfo));

                                            thread.Start();
                                            thread.Join(5000);

                                            if (resultCode == IdentificationResult.ALL_OK)
                                            {
                                                if (!clients.Contains(clientObject))
                                                    clients.Add(clientObject);
                                                Console.WriteLine(clientObject.Name + " autorized");
                                            }
                                            else
                                            {
                                                Console.WriteLine(clientObject.Name + " failed to autorize");
                                            }

                                            SendSystemMessage(clientObject, type,
                                                $"{(long)resultCode}\n{clientInfo}\n");

                                            break;
                                        }
                                    case SystemMessageType.GetAllUserContacts:
                                        {
                                            List<ContactInfo> contacts = new List<ContactInfo>();
                                            int errorCode = -1;

                                            Thread thread = new Thread(() => 
                                                   GetAllUserContacts(clientObject, out errorCode, out contacts));

                                            thread.Start();
                                            thread.Join(30000);

                                            string message;

                                            if (errorCode == 0)
                                            {
                                                message = $"{contacts.Count}\n";
                                                foreach (var contact in contacts)
                                                {
                                                    message += contact.ToString() + '\n';
                                                }
                                            }
                                            else
                                            {
                                                message = $"{0}\n";
                                            }

                                            SendSystemMessage(clientObject, type, message);
                                            break;
                                        }
                                    case SystemMessageType.AddChat:
                                        {
                                            int resultCode = -1;
                                            ContactInfo contactInfo = new ContactInfo();
                                            Thread thread = new Thread(() => 
                                                   AddChat(clientObject, out resultCode, out contactInfo));

                                            thread.Start();
                                            thread.Join(30000);

                                            SendSystemMessage(clientObject, type, $"{resultCode}\n{contactInfo}\n");

                                            break;
                                        }
                                    case SystemMessageType.GetChatUsers:
                                        {
                                            int resultCode = -1;
                                            long chatId = 0;
                                            List<(long, string)> chatsUsers = new List<(long, string)>();
                                            Thread thread = new Thread(() => GetChatsUsers(clientObject, out chatId, out resultCode, out chatsUsers));
                                            thread.Start();
                                            thread.Join(900);

                                            if (resultCode > 0)
                                            {
                                                SendChatUsers(clientObject, chatId, chatsUsers);
                                            }
                                            break;
                                        }
                                    case SystemMessageType.GetNewChatMessages:
                                        {
                                            int resultCode = -1;
                                            List<MessageInfo> messages = new List<MessageInfo>();
                                            Thread thread = new Thread(() =>
                                                   GetMessagesByChat(clientObject, out resultCode, out messages));

                                            thread.Start();
                                            thread.Join(120000);

                                            if (resultCode > 0)
                                            {
                                                //new Thread(() => SendAllChatMessages(clientObject, messages)).Start();
                                                SendMessagesUpdate(clientObject, messages);
                                            }

                                            break;
                                        }
                                    case SystemMessageType.RequestChatMessagesUpdate:
                                        {
                                            int resultCode = -1;
                                            List<MessageInfo> messages = new List<MessageInfo>();
                                            Thread thread = new Thread(() => 
                                                   GetMessagesByChat(clientObject, out resultCode, out messages));

                                            thread.Start();
                                            thread.Join(120000);

                                            if (resultCode > 0)
                                            {
                                                new Thread(() => SendAllChatMessages(clientObject, messages)).Start();
                                            }

                                            break;
                                        }
                                    case SystemMessageType.GetAttachment:
                                        {
                                            string filename = binaryReader.ReadString();
                                            string extension = binaryReader.ReadString();
                                            string path = Path.ChangeExtension(Path.Combine(attachmentsDir, filename),
                                                                        extension);

                                            if (File.Exists(path))
                                            {
                                                lock (this)
                                                {
                                                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                                                    {
                                                        using (MemoryStream memoryStream = new MemoryStream())
                                                        {
                                                            fileStream.CopyTo(memoryStream);
                                                            byte[] buff = memoryStream.ToArray();
                                                            SendAttachment(clientObject, filename, extension, buff);
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        }
                                    case SystemMessageType.SearchContacts:
                                        {
                                            int resultCode = -1;
                                            List<ContactInfo> contacts = new List<ContactInfo>();
                                            Thread thread = new Thread(() =>
                                                   SearchContacts(clientObject, out resultCode, out contacts));

                                            thread.Start();
                                            thread.Join(30000);

                                            string message;
                                            if (resultCode > 0)
                                            {
                                                message = $"{contacts.Count}\n";
                                                foreach (ContactInfo contactInfo in contacts)
                                                {
                                                    message += contactInfo.ToString() + '\n';
                                                }
                                            }
                                            else
                                            {
                                                message = $"{0}\n";
                                            }

                                            SendSystemMessage(clientObject, type, message);

                                            break;
                                        }
                                    case SystemMessageType.DeleteContact:
                                        {
                                            DeleteContact(clientObject);
                                            break;
                                        }
                                    case SystemMessageType.UpdateUserInfo:
                                        {
                                            UpdateType updateType = (UpdateType)binaryReader.ReadInt32();
                                            (bool isSuccessful, string newValue) result = (false, "");
                                            switch (updateType)
                                            {
                                                case UpdateType.Login:
                                                    {
                                                        result = UpdateLogin(clientObject);
                                                        break;
                                                    }
                                                case UpdateType.Email:
                                                    {
                                                        result = UpdateEmail(clientObject);
                                                        break;
                                                    }
                                                case UpdateType.Password:
                                                    {
                                                        result = UpdatePassword(clientObject);
                                                        break;
                                                    }
                                                case UpdateType.Name:
                                                    {
                                                        result = UpdateName(clientObject);
                                                        break;
                                                    }
                                            }
                                            SendUpdateConfirmation(clientObject, updateType, result.isSuccessful, result.newValue);
                                            break;
                                        }
                                }
                                break;
                            }
                        case MessagePrefix.DefaultMessage:
                            {
                                if (clientObject.IsAutorized)
                                {
                                    MessageInfo messageInfo = null;
                                    List<long> usersToSendIds = new List<long>();
                                    AddMessage(clientObject, out messageInfo, out usersToSendIds);

                                    if (messageInfo != null && usersToSendIds.Count > 0)
                                    {
                                        BroadcastMessage(messageInfo, usersToSendIds);
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine($"{clientObject.Name} disconnected");
                clients.Remove(clientObject);
                clientObject.Client.Close();
                clientObject.Client.Dispose();
            }

        }

        private void SendChatUsers(ClientObject clientObject, long chatId, List<(long id, string name)> chatsUsers)
        {
            BinaryWriter binaryWriter = new BinaryWriter(clientObject.Client.GetStream());
            binaryWriter.Write((int)MessagePrefix.SystemMessage);
            binaryWriter.Write((int)SystemMessageType.GetChatUsers);
            binaryWriter.Write(chatId);
            binaryWriter.Write(chatsUsers.Count);
            foreach (var item in chatsUsers)
            {
                binaryWriter.Write(item.id);
                binaryWriter.Write(item.name);
            }
            binaryWriter.Flush();
        }

        void BroadcastMessage(MessageInfo messageInfo, List<long> usersIds)
        {
            List<ClientObject> usersToSend = new List<ClientObject>();

            foreach (long id in usersIds)
            {
                ClientObject clientObject = clients.Find(cli => cli.Id == id);
                if (clientObject != null)
                    usersToSend.Add(clientObject);
            }
            foreach (var user in usersToSend)
            {
                try
                {
                    BinaryWriter binaryWriter = new BinaryWriter(user.Client.GetStream());
                    binaryWriter.Write((int)MessagePrefix.DefaultMessage);
                    binaryWriter.Write(messageInfo.ChatId);
                    binaryWriter.Write(messageInfo.MessageId);
                    binaryWriter.Write(messageInfo.SenderId);
                    binaryWriter.Write(messageInfo.MessageText);
                    binaryWriter.Write(messageInfo.SendDateTime);
                    binaryWriter.Write(messageInfo.AttachmentsInfo.Count);
                    foreach (var item in messageInfo.AttachmentsInfo)
                    {
                        binaryWriter.Write(item.Filename);
                        binaryWriter.Write(item.Name);
                        binaryWriter.Write(item.Extension);
                        binaryWriter.Write((int)item.Type);
                        lock (this)
                        {
                            using (FileStream fileStream = new FileStream(
                                Path.ChangeExtension(
                                Path.Combine(attachmentsDir, item.Filename), item.Extension),
                                FileMode.Open))
                            {
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    fileStream.CopyTo(memoryStream);
                                    byte[] buff = memoryStream.ToArray();
                                    binaryWriter.Write(buff.LongLength);
                                    binaryWriter.Write(buff);
                                }
                            }
                        }
                    }
                    binaryWriter.Flush();
                }
                catch
                {
                    Console.WriteLine("broadcasting error");
                }
            }
        }

        void SendAllChatMessages(ClientObject clientObject, List<MessageInfo> messagesInfo)
        {
            BinaryWriter binaryWriter = new BinaryWriter(clientObject.Client.GetStream());
            foreach (var messageInfo in messagesInfo)
            {
                try
                {
                    binaryWriter.Write((int)MessagePrefix.DefaultMessage);
                    binaryWriter.Write(messageInfo.ChatId);
                    binaryWriter.Write(messageInfo.MessageId);
                    binaryWriter.Write(messageInfo.SenderId);
                    binaryWriter.Write(messageInfo.MessageText);
                    binaryWriter.Write(messageInfo.SendDateTime);
                    binaryWriter.Write(messageInfo.AttachmentsInfo.Count);
                    foreach (var item in messageInfo.AttachmentsInfo)
                    {
                        binaryWriter.Write(item.Filename);
                        binaryWriter.Write(item.Name);
                        binaryWriter.Write(item.Extension);
                        binaryWriter.Write((int)item.Type);
                        lock (this)
                        {
                            using (FileStream fileStream = new FileStream(
                                Path.ChangeExtension(
                                Path.Combine(attachmentsDir, item.Filename), item.Extension),
                                FileMode.Open))
                            {
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    fileStream.CopyTo(memoryStream);
                                    byte[] buff = memoryStream.ToArray();
                                    binaryWriter.Write(buff.LongLength);
                                    binaryWriter.Write(buff);
                                }
                            }
                        }
                    }
                    binaryWriter.Flush();
                }
                catch
                {
                    Console.WriteLine("broadcasting error");
                }
            }
        }
       
        void SendSystemMessage(ClientObject clientObject, SystemMessageType type, string message)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(clientObject.Client.GetStream());
                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)type);
                binaryWriter.Write(message);
                binaryWriter.Flush();
            }
            catch
            {
                Console.WriteLine("broadcasting error");
            }
        }

        void SendMessagesUpdate(ClientObject clientObject, List<MessageInfo> messagesInfo)
        {
            BinaryWriter binaryWriter = new BinaryWriter(clientObject.Client.GetStream());
            foreach (var messageInfo in messagesInfo)
            {
                try
                {
                    binaryWriter.Write((int)MessagePrefix.SystemMessage);
                    binaryWriter.Write((int)SystemMessageType.GetNewChatMessages);
                    binaryWriter.Write(messageInfo.ChatId);
                    binaryWriter.Write(messageInfo.MessageId);
                    binaryWriter.Write(messageInfo.SenderId);
                    binaryWriter.Write(messageInfo.MessageText);
                    binaryWriter.Write(messageInfo.SendDateTime);
                    binaryWriter.Write(messageInfo.AttachmentsInfo.Count);
                    foreach (var item in messageInfo.AttachmentsInfo)
                    {
                        binaryWriter.Write(item.Filename);
                        binaryWriter.Write(item.Name);
                        binaryWriter.Write(item.Extension);
                        binaryWriter.Write((int)item.Type);
                    }
                    binaryWriter.Flush();
                }
                catch
                {
                    Console.WriteLine("broadcasting error");
                }
            }
        }

        void SendAttachment(ClientObject clientObject, string filename, string extension, byte[] data)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(clientObject.Client.GetStream());

                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.GetAttachment);
                binaryWriter.Write(filename);
                binaryWriter.Write(extension);
                binaryWriter.Write(data.LongLength);
                binaryWriter.Write(data);

                binaryWriter.Flush();
            }
            catch
            {
                Console.WriteLine("broadcasting error");
            }
        }

        void SendUpdateConfirmation(ClientObject clientObject, UpdateType type, bool isSuccesful, string newValue)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(clientObject.Client.GetStream());

                binaryWriter.Write((int)MessagePrefix.SystemMessage);
                binaryWriter.Write((int)SystemMessageType.UpdateUserInfo);
                binaryWriter.Write((int)type);
                binaryWriter.Write(isSuccesful);
                binaryWriter.Write(newValue);

                binaryWriter.Flush();
            }
            catch
            {
                Console.WriteLine("broadcasting error");
            }
        }
    }
}
