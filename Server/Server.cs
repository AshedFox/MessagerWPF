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

        void AutorizeUser(ClientObject clientObject, out IdentificationResult resultCode)
        {
            NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            string loginOrEmail = binaryReader.ReadString();
            string password = binaryReader.ReadString();

            DbManager dbManager = new DbManager();
            dbManager.ConnectToDB();

            var result = dbManager.GetClientInfoByLogin(loginOrEmail);
            resultCode = result.resultCode;

            dbManager.CloseConnectionToDB();

            if (resultCode == IdentificationResult.ALL_OK)
            {
                if (result.info.password == password)
                {
                    clientObject.Id = result.info.id;
                    clientObject.Username = result.info.name;
                    clientObject.IsAutorized = true;
                }
                else resultCode = IdentificationResult.INCORRECT_PASSWORD;
            }
        }

        void RegistrateUser(ClientObject clientObject, out IdentificationResult resultCode)
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

            dbManager.CloseConnectionToDB();

            if (resultCode == IdentificationResult.ALL_OK)
            {
                clientObject.Id = result.id;
                clientObject.Username = login;
                clientObject.IsAutorized = true;
            }
        }

        void ProcessClient(ClientObject clientObject)
        {
            using NetworkStream stream = clientObject.Client.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);

            try
            {
                while (clientObject.Client.Connected)
                {
                    switch ((DataPrefixes)binaryReader.ReadByte())
                    {
                        case DataPrefixes.SystemMessage:
                            string result = binaryReader.ReadString();

                            if (result == "REG")
                            {
                                IdentificationResult resultCode = IdentificationResult.TIMEOUT;
                                Thread thread = new Thread(() => RegistrateUser(clientObject, out resultCode));

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
                                SendSystemMessage(clientObject, $"{ (int)resultCode}");
                            }
                            else if (result == "AUT")
                            {
                                IdentificationResult resultCode = IdentificationResult.TIMEOUT;
                                Thread thread = new Thread(() => AutorizeUser(clientObject, out resultCode));

                                thread.Start();
                                thread.Join(5000);

                                if (resultCode == IdentificationResult.ALL_OK)
                                {
                                    clients.Add(clientObject);
                                    Console.WriteLine(clientObject.Username + " autorized");
                                }
                                else
                                {
                                    Console.WriteLine(clientObject.Username + " failed to autorize");
                                }
                                SendSystemMessage(clientObject, $"{(int)resultCode}");
                            }
                            break;
                        case DataPrefixes.Text:
                            if (clientObject.IsAutorized)
                            {
                                string message = binaryReader.ReadString();

                                message = $"{DateTime.Now.ToUniversalTime()} {clientObject.Username}:\r\n{message}";
                                Console.WriteLine(message);

                                new Thread(() => BroadcastMessage(message)).Start();
                            }
                            break;
                        case DataPrefixes.Audio:
                            if (clientObject.IsAutorized)
                            {
                                MemoryStream memoryStream = new MemoryStream();

                                byte[] buff = new byte[1024];
                                int count;
                                do
                                {
                                    count = binaryReader.Read(buff, 0, 1024);
                                    memoryStream.Write(buff, 0, count);
                                } while (stream.DataAvailable);

                                new Thread(() => BroadcastAudio(clientObject, memoryStream.ToArray())).Start();
                                memoryStream.Dispose();
                            }
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
                Console.WriteLine("Processing error");
            }
            finally
            {
                Console.WriteLine($"{clientObject.Username} disconnected");
                clients.Remove(clientObject);
            }
            clientObject.Client.Close();

        }

        void BroadcastMessage(string message)
        {

            foreach (var client in clients)
            {
                try
                {
                    BinaryWriter binaryWriter = new BinaryWriter(client.Client.GetStream());
                    binaryWriter.Write((byte)DataPrefixes.Text);
                    binaryWriter.Write(message);
                    binaryWriter.Flush();
                }
                catch
                {
                    Console.WriteLine("broadcasting error");
                }
            }
        }

        void BroadcastAudio(ClientObject clientObject, byte[] buff)
        {
            foreach (var client in clients)
            {
                if (client != clientObject)
                {
                    try
                    {
                        BinaryWriter binaryWriter = new BinaryWriter(client.Client.GetStream());
                        binaryWriter.Write((byte)DataPrefixes.Audio);
                        binaryWriter.Write(buff);
                        binaryWriter.Flush();
                    }
                    catch
                    {
                        Console.WriteLine("broadcasting error");
                    }
                }
            }
        }

        void SendSystemMessage(ClientObject clientObject, string message)
        {
            try
            {
                BinaryWriter binaryWriter = new BinaryWriter(clientObject.Client.GetStream());
                binaryWriter.Write((byte)DataPrefixes.SystemMessage);
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
