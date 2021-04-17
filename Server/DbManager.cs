using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.IO;
using ClientServerLib;

namespace Server
{


    class DbManager
    {

        const string UsersTableName = "Users";
        const string MessagesTableName = "Messages";
        const string ChatsTableName = "Chats";
        const string ChatsUsersTableName = "ChatsUsers";

        SQLiteConnection connection;

        public DbManager()
        {
            if (!File.Exists(@"messenger.db"))
            {
                SQLiteConnection.CreateFile("messenger.db");
            }
            connection = new SQLiteConnection(@"DataSource=messenger.db; Version=3;");
        }

        public void ConnectToDB()
        {
            connection.Open();

            CreateTables();
        }

        void CreateTables()
        {
            string createCommandText = $@"PRAGMA foreign_keys = true;
                                        CREATE TABLE IF NOT EXISTS {UsersTableName}(
                                            id    INTEGER NOT NULL UNIQUE,
                                            login TEXT NOT NULL,
                                            email TEXT NOT NULL,
                                            password  TEXT NOT NULL,
                                            name  TEXT,
                                            PRIMARY KEY(id AUTOINCREMENT)
                                        );

                                        CREATE TABLE IF NOT EXISTS {MessagesTableName} (
                                            id    INTEGER NOT NULL UNIQUE,
                                            chat_id   INTEGER NOT NULL,
	                                        user_id   INTEGER NOT NULL,
	                                        data  TEXT,
	                                        send_datetime  TEXT,
                                            PRIMARY KEY(id AUTOINCREMENT),
                                            FOREIGN KEY(user_id) REFERENCES {UsersTableName}(id),
	                                        FOREIGN KEY(chat_id) REFERENCES {ChatsTableName}(id)
                                        );

                                        CREATE TABLE IF NOT EXISTS {ChatsTableName} (
                                            id    INTEGER NOT NULL UNIQUE,
                                            Name  TEXT,
	                                        PRIMARY KEY(id AUTOINCREMENT)
                                        );

                                        CREATE TABLE IF NOT EXISTS {ChatsUsersTableName} (
                                            chat_id   INTEGER NOT NULL,
	                                        user_id   INTEGER NOT NULL,
	                                        FOREIGN KEY(chat_id) REFERENCES {ChatsTableName}(id),
                                            FOREIGN KEY(user_id) REFERENCES {UsersTableName}(id)
                                        );
            ";

            SQLiteCommand createCommand = new SQLiteCommand(createCommandText, connection);
            createCommand.ExecuteNonQuery();
        }

        public void CloseConnectionToDB()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }


        //ACTIONS WITH CLIENTS TABLE
        public (long id, IdentificationResult resultCode) AddClient(ClientInfo clientInfo)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string insertCommandText = $"INSERT INTO {UsersTableName} (login, email, password, name)"
                                           + "VALUES (@login,@email,@password,@name);";

                SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                //insertCommand.Parameters.AddWithValue("@id", DBNull.Value);
                insertCommand.Parameters.AddWithValue("@login", clientInfo.login);
                insertCommand.Parameters.AddWithValue("@email", clientInfo.email);
                insertCommand.Parameters.AddWithValue("@password", clientInfo.password);
                insertCommand.Parameters.AddWithValue("@name", clientInfo.name);

                try
                {
                    insertCommand.ExecuteNonQuery();

                    string selectCommandText = $"SELECT last_insert_rowid();";

                    SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);

                    var result = selectCommand.ExecuteScalar();

                    return result == null ? (-1, IdentificationResult.USER_SELECTION_ERROR)
                                            : ((long)result, IdentificationResult.ALL_OK);
                }
                catch
                {
                    return (-1, IdentificationResult.USER_ALREADY_EXISTS);
                }

            }
            else return (-1, IdentificationResult.DB_CONNECTION_ERROR);
        }

        public (ClientInfo info, IdentificationResult resultCode) GetClientInfoByLogin(string loginOrEmail)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectCommandText = $"SELECT * FROM {UsersTableName} WHERE (login) = (@loginOrEmail) " 
                                           + "or (email) = (@loginOrEmail);";

                SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@loginOrEmail", loginOrEmail);

                using (SQLiteDataReader reader = selectCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ClientInfo info = new ClientInfo
                        (
                            (long)reader["id"],
                            (string)reader["login"],
                            (string)reader["email"],
                            (string)reader["password"],
                            (string)reader["name"]
                        );

                        return (info, IdentificationResult.ALL_OK);
                    }
                    else return (null, IdentificationResult.USER_NOT_FOUND);
                }
            }
            else return (null, IdentificationResult.DB_CONNECTION_ERROR);            
        }

        //

        public long AddChat(long user1Id, long user2Id)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectCommandText = $"SELECT * FROM {UsersTableName} WHERE (id) = (@user2Id)";

                SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@user2Id", user2Id);

                //string conversationName = "";

                using (SQLiteDataReader reader = selectCommand.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return -1;
                    }
                }

                selectCommandText = $"SELECT * FROM {ChatsTableName} " +
                    $"INNER JOIN {ChatsUsersTableName} p1 ON p1.chat_id = id AND p1.user_id = (@user1Id) " +
                    $"INNER JOIN {ChatsUsersTableName} p2 ON p2.chat_id = id AND p2.user_id = (@user2Id); ";

                selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@user1Id", user1Id);
                selectCommand.Parameters.AddWithValue("@user2Id", user2Id);

                using (SQLiteDataReader reader = selectCommand.ExecuteReader())
                {
                    if (reader.Read())
                        return -1;
                }

                string insertCommandText = $"INSERT INTO {ChatsTableName} (name) " +
                                           $"VALUES (@name);";

                SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                insertCommand.Parameters.AddWithValue("@name", "");

                insertCommand.ExecuteNonQuery();

                selectCommandText = $"SELECT last_insert_rowid();";

                selectCommand = new SQLiteCommand(selectCommandText, connection);

                var chatId = selectCommand.ExecuteScalar();

                if (chatId != null)
                {
                    insertCommandText = $"INSERT INTO {ChatsUsersTableName} (chat_id, user_id)" +
                                          "VALUES (@chatId, @user1Id), (@chatId, @user2Id);";
                    insertCommand = new SQLiteCommand(insertCommandText, connection);
                    insertCommand.Parameters.AddWithValue("@chatId", (long)chatId);
                    insertCommand.Parameters.AddWithValue("@user1Id", user1Id);
                    insertCommand.Parameters.AddWithValue("@user2Id", user2Id);

                    insertCommand.ExecuteNonQuery();

                    return (long)chatId;
                }
            }

            return -1;
        }

        public (int resultCode, List<ContactInfo> contactsInfo) GetContactsByNamePart(string namePattern)
        {
            List<ContactInfo> contactsInfo = new List<ContactInfo>();
            int resultCode = -1;

            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectCommandText = $"SELECT * FROM {UsersTableName} WHERE (name) LIKE (@namePattern) LIMIT 50;";
                SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@namePattern", '%' + namePattern + '%');

                using (SQLiteDataReader usersReader = selectCommand.ExecuteReader())
                {
                    while (usersReader.Read())
                    {
                        contactsInfo.Add(new ContactInfo((long)usersReader["id"], (string)usersReader["name"]));
                    }
                    resultCode = 1;
                }
            }
            return (resultCode, contactsInfo);
        }

        public (int resultCode, ContactInfo contactInfo) GetContactById(long chatId, long userId)
        {
            ContactInfo contactInfo = new ContactInfo();
            int resultCode = -1;
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectChatsCommandText = $"SELECT * FROM {ChatsTableName} WHERE (id) IN " +
                                                $"(SELECT chat_id FROM {ChatsUsersTableName} WHERE (user_id) = (@userId));";
                SQLiteCommand selectChatsCommand = new SQLiteCommand(selectChatsCommandText, connection);
                selectChatsCommand.Parameters.AddWithValue("@userId", userId);

                using (SQLiteDataReader contactReader = selectChatsCommand.ExecuteReader())
                {
                    if (contactReader.Read())
                    {
                        resultCode = 0;
                        string chatName = (string)contactReader["name"];

                        if (string.IsNullOrEmpty(chatName))
                        {
                            string selectChatNameCommandText = $"SELECT name FROM {UsersTableName} WHERE (id) = " +
                                $" (SELECT user_id FROM {ChatsUsersTableName} WHERE (chat_id) = (@chatId)" +
                                $" AND (user_id) != (@userId));";

                            SQLiteCommand selectChatNameCommand = new SQLiteCommand(selectChatNameCommandText, connection);
                            selectChatNameCommand.Parameters.AddWithValue("@chatId", chatId);
                            selectChatNameCommand.Parameters.AddWithValue("@userId", userId);

                            using (SQLiteDataReader chatNameReader = selectChatNameCommand.ExecuteReader())
                            {
                                if (chatNameReader.Read())
                                {
                                    chatName = (string)chatNameReader["name"];
                                }
                                else
                                {
                                    chatName = "***";
                                }
                            }
                        }
                        contactInfo.name = chatName;
                    }
                }
            }
            return (resultCode, contactInfo);
        }

        public List<MessageInfo> GetMessagesByChat(long chatId)
        {       
            List<MessageInfo> result = new List<MessageInfo>();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectMessageCommandText = $"SELECT * FROM {MessagesTableName} WHERE (chat_id) = (@chatId)";
                SQLiteCommand selectMessageCommand = new SQLiteCommand(selectMessageCommandText, connection);
                selectMessageCommand.Parameters.AddWithValue("@chatId", chatId);

                using (SQLiteDataReader messageReader = selectMessageCommand.ExecuteReader())
                {
                    while (messageReader.Read())
                    {                       
                        long userId = (long)messageReader["user_id"];
                        long messageId = (long)messageReader["id"];

                        string selectUserCommandText = $"SELECT name FROM {UsersTableName} WHERE (id) = (@userId)";
                        SQLiteCommand selectUserCommand = new SQLiteCommand(selectUserCommandText, connection);
                        selectUserCommand.Parameters.AddWithValue("@userId", userId);

                        using (SQLiteDataReader userReader = selectUserCommand.ExecuteReader())
                        {
                            if (userReader.Read())
                            {
                                string username = (string)userReader["name"];

                                result.Add(new MessageInfo(
                                        messageId,
                                        username,
                                        (string)messageReader["send_datetime"],
                                        (string)messageReader["data"])
                                    );
                            }
                        }
                    }
                }
            }

            return result;
        }

        public List<ContactInfo> GetAllUserContacts(long userId)
        {
            List<ContactInfo> result = new List<ContactInfo>();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectChatsCommandText = $"SELECT * FROM {ChatsTableName} WHERE (id) IN " +
                                                $"(SELECT chat_id FROM {ChatsUsersTableName} WHERE (user_id) = (@userId));";
                SQLiteCommand selectChatsCommand = new SQLiteCommand(selectChatsCommandText, connection);
                selectChatsCommand.Parameters.AddWithValue("@userId", userId);

                using (SQLiteDataReader contactReader = selectChatsCommand.ExecuteReader())
                {
                    while (contactReader.Read())
                    {
                        long chatId = (long)contactReader["id"];
                        string chatName = (string)contactReader["Name"];

                        if (string.IsNullOrEmpty(chatName))
                        {
                            string selectChatNameCommandText = $"SELECT name FROM {UsersTableName} WHERE (id) = " +
                                $" (SELECT user_id FROM {ChatsUsersTableName} WHERE (chat_id) = (@chatId)" +
                                $" AND (user_id) != (@userId));";

                            SQLiteCommand selectChatNameCommand = new SQLiteCommand(selectChatNameCommandText, connection);
                            selectChatNameCommand.Parameters.AddWithValue("@chatId", chatId);
                            selectChatNameCommand.Parameters.AddWithValue("@userId", userId);

                            using (SQLiteDataReader chatNameReader = selectChatNameCommand.ExecuteReader())
                            {
                                if (chatNameReader.Read())
                                {
                                    chatName = (string)chatNameReader["name"];
                                }
                                else
                                {
                                    chatName = "***";
                                }
                            }
                        }

                        result.Add(new ContactInfo( chatId, chatName));
                    }
                }
            }

            return result;
        }

        public MessageInfo AddMessage(long chatId, long userId, string messageData, string sendDate)
        {
            MessageInfo messageInfo = null;
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string insertCommandText = $"INSERT INTO {MessagesTableName} (chat_id, user_id, data, send_datetime) " +
                                           $"VALUES (@chatId, @userId, @messageData, @sendDate);";

                SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                insertCommand.Parameters.AddWithValue("@chatId", chatId);
                insertCommand.Parameters.AddWithValue("@userId", userId);
                insertCommand.Parameters.AddWithValue("@messageData", messageData);
                insertCommand.Parameters.AddWithValue("@sendDate", sendDate);

                if (insertCommand.ExecuteNonQuery() > 0)
                {
                    string selectCommandText = $"SELECT last_insert_rowid();";

                    SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                    long? messageId;

                    if ((messageId = (long?)selectCommand.ExecuteScalar()) != null)
                    {
                        selectCommandText = $"SELECT name FROM {UsersTableName} WHERE (id) = " +
                                            $"(@userId);";

                        selectCommand = new SQLiteCommand(selectCommandText, connection);
                        selectCommand.Parameters.AddWithValue("@userId", userId);

                        using (SQLiteDataReader userNameReader = selectCommand.ExecuteReader())
                        {
                            if (userNameReader.Read())
                            {
                                messageInfo = new MessageInfo(messageId.Value, (string)userNameReader["name"],
                                                              sendDate, messageData);
                            }
                        }
                    }
                }
            }
            return messageInfo;
        }

        public List<long> GetChatUsersIds(long chatId)
        {
            List<long> result = new List<long>();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectCommandText = $"SELECT user_id FROM {ChatsUsersTableName} WHERE (chat_id) = (@chatId)";

                SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@chatId", chatId);

                using (SQLiteDataReader userReader = selectCommand.ExecuteReader())
                {
                    while (userReader.Read())
                    {
                        result.Add((long)userReader["user_id"]);
                    }
                }
            }
            return result;
        }
    }
}
