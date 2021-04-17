using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using ClientServerLib;
using System.Threading.Tasks;

namespace Server
{
    public class Server
    {
        TcpListener server;
        List<ClientObject> clients = new List<ClientObject>();

        public Server(int port)
        {
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
                    clientObject.Username = result.info.name;
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

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            var result = dbManager.AddClient(new ClientInfo(login, email, password, login));
            resultCode = result.resultCode;
            clientInfo = new ClientInfo(result.id, login, email, "", login);

            dbManager.CloseConnectionToDB();

            if (resultCode == IdentificationResult.ALL_OK)
            {
                clientObject.Id = result.id;
                clientObject.Username = login;
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

            (resultCode, contacts) = dbManager.GetContactsByNamePart(namePattern);           

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

        void GetMessagesByChat(ClientObject clientObject, out int resultCode, out List<MessageInfo> messages)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            long chatId = binaryReader.ReadInt64();

            resultCode = 5;

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            messages = dbManager.GetMessagesByChat(chatId);

            dbManager.CloseConnectionToDB();
        }

        void AddMessage(ClientObject clientObject, out MessageInfo messageInfo, out List<long>usersIdsToSend)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            long chatId = binaryReader.ReadInt64();
            string messageData = binaryReader.ReadString();

            string sendDate = DateTime.UtcNow.ToString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            messageInfo = dbManager.AddMessage(chatId, clientObject.Id, messageData, sendDate);
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
                    DataPrefix dataPrefix = (DataPrefix)binaryReader.ReadInt32();
                    switch (dataPrefix)
                    {
                        case DataPrefix.SystemMessage:
                            {
                                
                                string result = binaryReader.ReadString();

                                switch (result)
                                {
                                    case "REG":
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
                                                Console.WriteLine(clientObject.Username + " connected and registrate");
                                            }
                                            else
                                            {
                                                Console.WriteLine(clientObject.Username + " failed to registrate");
                                            }
                                            SendSystemMessage(clientObject, $"REG\n{(int)resultCode}\n{clientInfo}\n");

                                            break;
                                        }
                                    case "AUT":
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
                                                Console.WriteLine(clientObject.Username + " autorized");
                                            }
                                            else
                                            {
                                                Console.WriteLine(clientObject.Username + " failed to autorize");
                                            }

                                            SendSystemMessage(clientObject, $"AUT\n{(long)resultCode}\n{clientInfo}\n");

                                            break;
                                        }
                                    case "CONTACTSALL":
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
                                                message = $"CONTACTSALL\n{contacts.Count}\n";
                                                foreach (var contact in contacts)
                                                {
                                                    message += contact.ToString() + '\n';
                                                }
                                            }
                                            else
                                            {
                                                message = $"CONTACTSALL\n{0}\n";
                                            }

                                            SendSystemMessage(clientObject, message);
                                            break;
                                        }
                                    case "ADDCHAT":
                                        {
                                            int resultCode = -1;
                                            ContactInfo contactInfo = new ContactInfo();
                                            Thread thread = new Thread(() => AddChat(clientObject, out resultCode, out contactInfo));

                                            thread.Start();
                                            thread.Join(5000);

                                            SendSystemMessage(clientObject, $"ADDCHAT\n{resultCode}\n{contactInfo}\n");

                                            break;
                                        }
                                    case "MESSAGESCHATALL":
                                        {
                                            int resultCode = -1;
                                            List<MessageInfo> messages = new List<MessageInfo>();
                                            Thread thread = new Thread(() => 
                                                   GetMessagesByChat(clientObject, out resultCode, out messages));

                                            thread.Start();
                                            thread.Join(5000);

                                            string message;
                                            if (resultCode > 0)
                                            {
                                                message = $"MESSAGESCHATALL\n{messages.Count}\n";
                                                foreach(MessageInfo messageInfo in messages)
                                                {
                                                    message += messageInfo.ToString() + '\n';
                                                }
                                            }
                                            else
                                            {
                                                message = $"MESSAGESCHATALL\n{0}\n";
                                            }

                                            SendSystemMessage(clientObject, message);

                                            break;
                                        }
                                    case "SEARCH":
                                        {
                                            int resultCode = -1;
                                            List<ContactInfo> contacts = new List<ContactInfo>();
                                            Thread thread = new Thread(() =>
                                                   SearchContacts(clientObject, out resultCode, out contacts));

                                            thread.Start();
                                            thread.Join(5000);

                                            string message;
                                            if (resultCode > 0)
                                            {
                                                message = $"SEARCH\n{contacts.Count}\n";
                                                foreach (ContactInfo contactInfo in contacts)
                                                {
                                                    message += contactInfo.ToString() + '\n';
                                                }
                                            }
                                            else
                                            {
                                                message = $"SEARCH\n{0}\n";
                                            }

                                            SendSystemMessage(clientObject, message);

                                            break;
                                        }
                                }
                                break;
                            }
                        case DataPrefix.Text:
                            {
                                if (clientObject.IsAutorized)
                                {
                                    MessageInfo messageInfo = null;
                                    List<long> usersToSendIds = new List<long>();
                                    Thread thread = new Thread(() => AddMessage(clientObject, 
                                                                out messageInfo, out usersToSendIds));
                                    thread.Start();
                                    thread.Join(100);
                                    if (messageInfo != null && usersToSendIds.Count > 0)
                                        BroadcastMessage(messageInfo, usersToSendIds);
                                }
                                break;
                            }
                        case DataPrefix.Audio:
                            {
                                if (clientObject.IsAutorized)
                                {
                                    long length = binaryReader.ReadInt64();

                                    string extension = binaryReader.ReadString();

                                    MemoryStream memoryStream = new MemoryStream();

                                    byte[] buff = new byte[2048];
                                    int count;
                                    do
                                    {
                                        count = binaryReader.Read(buff, 0, buff.Length);
                                        memoryStream.Write(buff, 0, count);
                                        length -= count;
                                    } while (length > 0);

                                    new Thread(() => BroadcastFile(DataPrefix.Audio, extension, memoryStream.ToArray())).Start();
                                    memoryStream.Dispose();
                                }
                                break;
                            }
                        case DataPrefix.Video:
                            {
                                if (clientObject.IsAutorized)
                                {
                                    long length = binaryReader.ReadInt64();

                                    string extension = binaryReader.ReadString();

                                    MemoryStream memoryStream = new MemoryStream();

                                    byte[] buff = new byte[4096];
                                    int count;
                                    do
                                    {
                                        count = binaryReader.Read(buff, 0, buff.Length);
                                        memoryStream.Write(buff, 0, count);
                                        length -= count;
                                    } while (length > 0);

                                    new Thread(() => BroadcastFile(DataPrefix.Video, extension, memoryStream.ToArray())).Start();
                                    memoryStream.Dispose();
                                }
                                break;
                            }
                        case DataPrefix.Image:
                            {
                                if (clientObject.IsAutorized)
                                {
                                    long length = binaryReader.ReadInt64();

                                    string extension = binaryReader.ReadString();

                                    MemoryStream memoryStream = new MemoryStream();

                                    byte[] buff = new byte[512];
                                    int count;
                                    do
                                    {
                                        count = binaryReader.Read(buff, 0, buff.Length);
                                        memoryStream.Write(buff, 0, count);
                                        length -= count;
                                    } while (length > 0);

                                    new Thread(() => BroadcastFile(DataPrefix.Image, extension, memoryStream.ToArray())).Start();
                                    memoryStream.Dispose();
                                }
                                break;
                            }
                        case DataPrefix.File:
                            {
                                if (clientObject.IsAutorized)
                                {
                                    long length = binaryReader.ReadInt64();

                                    string extension = binaryReader.ReadString();

                                    MemoryStream memoryStream = new MemoryStream();

                                    byte[] buff = new byte[2048];
                                    int count;
                                    do
                                    {
                                        count = binaryReader.Read(buff, 0, buff.Length);
                                        memoryStream.Write(buff, 0, count);
                                        length -= count;
                                    } while (length > 0);

                                    new Thread(() => BroadcastFile(DataPrefix.File, extension, memoryStream.ToArray())).Start();
                                    memoryStream.Dispose();
                                }
                                break;
                            }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Processing error");
            }
            finally
            {
                Console.WriteLine($"{clientObject.Username} disconnected");
                clients.Remove(clientObject);
            }
            clientObject.Client.Close();

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
                    binaryWriter.Write((int)DataPrefix.Text);
                    binaryWriter.Write(messageInfo.messageId);
                    binaryWriter.Write(messageInfo.senderName);
                    binaryWriter.Write(messageInfo.messageText);
                    binaryWriter.Write(messageInfo.sendDateTime);
                    binaryWriter.Flush();
                }
                catch
                {
                    Console.WriteLine("broadcasting error");
                }
            }
        }

        void BroadcastFile(DataPrefix dataPrefix, string extension,  byte[] buff)
        {
            foreach (var client in clients)
            {
                //if (client != clientObject)
                //{
                    try
                    {
                        BinaryWriter binaryWriter = new BinaryWriter(client.Client.GetStream());
                        binaryWriter.Write((int)dataPrefix);
                        binaryWriter.Write(buff.LongLength);
                        binaryWriter.Write(extension);
                        binaryWriter.Write(buff);
                        binaryWriter.Flush();
                    }
                    catch
                    {
                        Console.WriteLine("broadcasting error");
                    }
                //}
            }
        }

        void SendSystemMessage(ClientObject clientObject, string message)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(clientObject.Client.GetStream());
                binaryWriter.Write((int)DataPrefix.SystemMessage);
                binaryWriter.Write(message);
                binaryWriter.Flush();
            }
            catch
            {
                Console.WriteLine("broadcasting error");
            }
        }
    }
}
