using ClientServerLib;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace Server
{
    class DbManager
    {
        const string UsersTableName = "Users";
        const string MessagesTableName = "Messages";
        const string ChatsTableName = "Chats";
        const string ChatsUsersTableName = "ChatsUsers";
        const string AttachmentsTableName = "Attachments";
        const string AttachmentsGroupsTableName = "AttachmentsGroups";

        readonly SQLiteConnection connection;       

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

                                        CREATE TABLE IF NOT EXISTS {ChatsTableName} (
                                            id    INTEGER NOT NULL UNIQUE,
                                            name  TEXT,
	                                        PRIMARY KEY(id AUTOINCREMENT)
                                        );

                                        CREATE TABLE IF NOT EXISTS {ChatsUsersTableName} (
                                            chat_id   INTEGER NOT NULL,
	                                        user_id   INTEGER NOT NULL,
	                                        FOREIGN KEY(chat_id) REFERENCES {ChatsTableName}(id),
                                            FOREIGN KEY(user_id) REFERENCES {UsersTableName}(id)
                                        );

                                        CREATE TABLE IF NOT EXISTS {AttachmentsTableName} (
                                            id    INTEGER NOT NULL UNIQUE,
                                            name  TEXT,
                                            filename  TEXT UNIQUE,
	                                        PRIMARY KEY(id AUTOINCREMENT)
                                        );

                                        CREATE TABLE IF NOT EXISTS {AttachmentsGroupsTableName} (
                                            id    INTEGER NOT NULL UNIQUE,
                                            attachments_ids  TEXT NOT NULL,
	                                        PRIMARY KEY(id AUTOINCREMENT)
                                        );

                                        CREATE TABLE IF NOT EXISTS {MessagesTableName} (
                                            id    INTEGER NOT NULL UNIQUE,
                                            chat_id   INTEGER NOT NULL,
	                                        user_id   INTEGER NOT NULL,
                                            type INTEGER NOT NYLL
	                                        data  TEXT,
                                            attachment_group_id INTEGER
	                                        send_datetime  TEXT,
                                            PRIMARY KEY(id AUTOINCREMENT),
                                            FOREIGN KEY(user_id) REFERENCES {UsersTableName}(id),
	                                        FOREIGN KEY(chat_id) REFERENCES {ChatsTableName}(id),
                                            FOREIGN KEY(attachment_group_id) REFERENCES {AttachmentsGroupsTableName}(id)
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

                using SQLiteDataReader reader = selectCommand.ExecuteReader();
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

                if (insertCommand.ExecuteNonQuery() > 0)
                {
                    var chatId = connection.LastInsertRowId;

                    insertCommandText = $"INSERT INTO {ChatsUsersTableName} (chat_id, user_id)" +
                                            "VALUES (@chatId, @user1Id), (@chatId, @user2Id);";
                    insertCommand = new SQLiteCommand(insertCommandText, connection);
                    insertCommand.Parameters.AddWithValue("@chatId", chatId);
                    insertCommand.Parameters.AddWithValue("@user1Id", user1Id);
                    insertCommand.Parameters.AddWithValue("@user2Id", user2Id);

                    insertCommand.ExecuteNonQuery();

                    return chatId;

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

                using SQLiteDataReader usersReader = selectCommand.ExecuteReader();
                while (usersReader.Read())
                {
                    contactsInfo.Add(new ContactInfo((long)usersReader["id"], (string)usersReader["name"]));
                }
                resultCode = 1;
            }
            return (resultCode, contactsInfo);
        }

        public (int resultCode, ContactInfo contactInfo) GetContactById(long chatId, long userId)
        {
            ContactInfo contactInfo = new ContactInfo();
            contactInfo.id = chatId;
            int resultCode = -1;
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectChatsCommandText = $"SELECT name FROM {ChatsTableName} WHERE (id) = (@chatId);";
                SQLiteCommand selectChatsCommand = new SQLiteCommand(selectChatsCommandText, connection);
                selectChatsCommand.Parameters.AddWithValue("@chatId", chatId);
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

                            using SQLiteDataReader chatNameReader = selectChatNameCommand.ExecuteReader();
                            if (chatNameReader.Read())
                            {
                                chatName = (string)chatNameReader["name"];
                            }
                            else
                            {
                                chatName = "***";
                            }
                        }
                        contactInfo.id = chatId;
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

                //read message
                using SQLiteDataReader messageReader = selectMessageCommand.ExecuteReader();
                while (messageReader.Read())
                {
                    long userId = (long)messageReader["user_id"];
                    long messageId = (long)messageReader["id"];

                    string selectUserCommandText = $"SELECT name FROM {UsersTableName} WHERE (id) = (@userId)";
                    SQLiteCommand selectUserCommand = new SQLiteCommand(selectUserCommandText, connection);
                    selectUserCommand.Parameters.AddWithValue("@userId", userId);

                    //read user name
                    using SQLiteDataReader userReader = selectUserCommand.ExecuteReader();
                    if (userReader.Read())
                    {
                        string username = (string)userReader["name"];
                        string sendDateTime = (string)messageReader["send_datetime"];
                        string messageText = (string)messageReader["data"];
                        List<AttachmentInfo> attachmentsInfo = new List<AttachmentInfo>();

                        // read attachments if exists
                        if (messageReader["attachments_group_id"] != DBNull.Value) 
                        {
                            string selectAttachmentsIdsCommandText = $"SELECT attachments_ids FROM {AttachmentsGroupsTableName} " +
                            $"WHERE (id) = (@id)";
                            SQLiteCommand selectAttachmentsIdsCommand = new SQLiteCommand(selectAttachmentsIdsCommandText, connection);
                            selectAttachmentsIdsCommand.Parameters.AddWithValue("@id", messageReader["attachments_group_id"]);

                            using SQLiteDataReader attachmentsIdsReader = selectAttachmentsIdsCommand.ExecuteReader();
                            if (attachmentsIdsReader.Read())
                            {
                                string _ = (string)attachmentsIdsReader["attachments_ids"];
                                string[] idsText = _.Split(_, ';');
                                List<int> attachmentsIds = new List<int>();
                                foreach (var item in idsText)
                                {
                                    int attachmentId = int.Parse(item);

                                    string selectAttachmentCommandText = $"SELECT * FROM {AttachmentsTableName} WHERE (id) = (@attachmentId)";
                                    SQLiteCommand selectAttachmentCommand = new SQLiteCommand(selectAttachmentCommandText, connection);
                                    selectAttachmentCommand.Parameters.AddWithValue("@attachmentId", attachmentId);

                                    //read attachment data
                                    using SQLiteDataReader attachmentsReader = selectAttachmentCommand.ExecuteReader();
                                    if (attachmentsReader.Read())
                                    {
                                        string name = (string)attachmentsReader["name"];
                                        string filename = (string)attachmentsReader["filename"];
                                        int type = (int)attachmentsReader["type"];
                                        string extension = Path.GetExtension(name);

                                        attachmentsInfo.Add(new AttachmentInfo(filename,
                                                                               name,
                                                                               extension,
                                                                               (DataPrefix)type));
                                    }
                                }
                            }

                        }

                        result.Add(new MessageInfo(messageId,
                                                   chatId,
                                                   username,
                                                   sendDateTime,
                                                   messageText,
                                                   attachmentsInfo));
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

                            using SQLiteDataReader chatNameReader = selectChatNameCommand.ExecuteReader();
                            if (chatNameReader.Read())
                            {
                                chatName = (string)chatNameReader["name"];
                            }
                            else
                            {
                                chatName = "***";
                            }
                        }

                        result.Add(new ContactInfo(chatId, chatName));
                    }
                }
            }

            return result;
        }

        public List<long> AddAttachments(List<AttachmentInfo> attachmentsInfo)
        {
            List<long> ids = new List<long>();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectCommandText = $"SELECT id FROM {AttachmentsTableName} WHERE " +
                    $"(filename) = (@filename);";
                string insertCommandText = $"INSERT INTO {AttachmentsTableName} (type, name, filename) " +
                    $"VALUES (@type, @name, @filename);";
                foreach (var item in attachmentsInfo)
                {
                    SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                    selectCommand.Parameters.AddWithValue("@filename", item.Filename);

                    using SQLiteDataReader reader = selectCommand.ExecuteReader();
                    if (reader.Read()) ids.Add((long)reader["id"]);
                    else {

                        SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                        insertCommand.Parameters.AddWithValue("@type", (long)item.Type);
                        insertCommand.Parameters.AddWithValue("@name", item.Name);
                        insertCommand.Parameters.AddWithValue("@filename", item.Filename);
                        if (insertCommand.ExecuteNonQuery() > 0)
                        {
                            ids.Add(connection.LastInsertRowId);
                        }
                    }
                    
                }
            }
            return ids;
        }

        public long AddAttachmentsGroup(List<long> attachmentsIds)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string attachmentsIdsText = string.Empty;
                attachmentsIds.ForEach(el => attachmentsIdsText += el + ';');
                string insertCommandText = $"INSERT INTO {AttachmentsGroupsTableName} (attachments_ids) " +
                    $"VALUES (@attachmentsIds)";
                SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                insertCommand.Parameters.AddWithValue("attachmentsIds", attachmentsIdsText);

                if (insertCommand.ExecuteNonQuery() > 0) return connection.LastInsertRowId;
                else return -1;
            }
            else return -2;
        }

        public MessageInfo AddMessage(long chatId,
                                      long userId,
                                      string messageText,
                                      string sendDate,
                                      List<AttachmentInfo> attachmentsInfo)
        {
            MessageInfo messageInfo = null;
            if (connection.State == System.Data.ConnectionState.Open)
            {
                var attachmentsIds = AddAttachments(attachmentsInfo);
                if (attachmentsIds != null)
                {
                    long attachmentsGroupId = AddAttachmentsGroup(attachmentsIds);
                    if (attachmentsGroupId > 0)
                    {
                        string insertCommandText = $"INSERT INTO {MessagesTableName} " +
                            $"(chat_id, user_id, data, attachments_group_id, send_datetime) " +
                            $"VALUES (@chatId, @userId, @messageData, @attachmentsGroupId, @sendDate);";

                        SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                        insertCommand.Parameters.AddWithValue("@chatId", chatId);
                        insertCommand.Parameters.AddWithValue("@userId", userId);
                        insertCommand.Parameters.AddWithValue("@messageData", messageText);
                        insertCommand.Parameters.AddWithValue("@attachmentsGroupId", attachmentsGroupId);
                        insertCommand.Parameters.AddWithValue("@sendDate", sendDate);

                        if (insertCommand.ExecuteNonQuery() > 0)
                        {
                            long messageId = connection.LastInsertRowId;

                            string selectCommandText = $"SELECT name FROM {UsersTableName} WHERE (id) = " +
                                $"(@userId);";

                            SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                            selectCommand.Parameters.AddWithValue("@userId", userId);

                            using SQLiteDataReader userNameReader = selectCommand.ExecuteReader();
                            if (userNameReader.Read())
                            {
                                messageInfo = new MessageInfo(messageId,
                                                              chatId,
                                                              (string)userNameReader["name"],
                                                              sendDate,
                                                              messageText,
                                                              attachmentsInfo);
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

                using SQLiteDataReader userReader = selectCommand.ExecuteReader();
                while (userReader.Read())
                {
                    result.Add((long)userReader["user_id"]);
                }
            }
            return result;
        }
    }
}
