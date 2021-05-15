using ClientServerLib;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;

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
                SQLiteConnection.CreateFile(@"messenger.db");
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
                                            user_state INTEGER NOT NULL,
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
                                            type INTEGER NOT NULL,
	                                        data  TEXT,
                                            attachment_group_id INTEGER,
	                                        send_datetime  TEXT,
                                            PRIMARY KEY(id AUTOINCREMENT),
                                            FOREIGN KEY(user_id) REFERENCES {UsersTableName}(id),
	                                        FOREIGN KEY(chat_id) REFERENCES {ChatsTableName}(id),
                                            FOREIGN KEY(attachment_group_id) REFERENCES {AttachmentsGroupsTableName}(id)
                                        );
            ";

            SQLiteCommand createCommand = new SQLiteCommand(createCommandText, connection);
            _ = createCommand.ExecuteNonQueryAsync().Result;
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
                    _ = insertCommand.ExecuteNonQueryAsync().Result;

                    string selectCommandText = $"SELECT last_insert_rowid();";

                    SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                    var result = selectCommand.ExecuteScalarAsync().Result;;

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

                using SQLiteDataReader reader = (SQLiteDataReader)selectCommand.ExecuteReaderAsync().Result;
                if (reader.Read())
                {
                    ClientInfo info = new ClientInfo
                    (
                        (long)reader["id"],
                        (string)reader["login"],
                        (string)reader["email"],
                        (byte[])reader["password"],
                        (string)reader["name"]
                    );

                    return (info, IdentificationResult.ALL_OK);
                }
                else return (null, IdentificationResult.USER_NOT_FOUND);
            }
            else return (null, IdentificationResult.DB_CONNECTION_ERROR);            
        }

        public long AddChat(long user1Id, long user2Id)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                if (user1Id == user2Id) return -1;
                string selectCommandText = $"SELECT * FROM {UsersTableName} WHERE (id) = (@user2Id)";

                SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@user2Id", user2Id);

                //string conversationName = "";

                using (SQLiteDataReader reader =  (SQLiteDataReader)selectCommand.ExecuteReaderAsync().Result)
                {
                    if (!reader.Read())
                    {
                        return -1;
                    }
                }

                selectCommandText = $"SELECT id, p1.user_state AS user1_state, p2.user_state AS user2_state" +
                    $" FROM {ChatsTableName} " +
                    $"INNER JOIN {ChatsUsersTableName} p1 ON p1.chat_id = id AND p1.user_id = (@user1Id) " +
                    $"INNER JOIN {ChatsUsersTableName} p2 ON p2.chat_id = id AND p2.user_id = (@user2Id); ";

                selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@user1Id", user1Id);
                selectCommand.Parameters.AddWithValue("@user2Id", user2Id);

                using (SQLiteDataReader reader =  (SQLiteDataReader)selectCommand.ExecuteReaderAsync().Result)
                {
                    if (reader.Read())
                    {
                        if ((long)reader["user1_state"] != 1)
                        {
                            string updateCommandText = $"UPDATE {ChatsUsersTableName} SET (user_state) = (1) WHERE " +
                                                       $"(user_id, chat_id) = (@userId, @chatId);";

                            SQLiteCommand updateCommand = new SQLiteCommand(updateCommandText, connection);
                            updateCommand.Parameters.AddWithValue("@userId", user1Id);
                            updateCommand.Parameters.AddWithValue("@chatId", (long)reader["id"]);

                            _ = updateCommand.ExecuteNonQueryAsync().Result;
                            return (long)reader["id"];
                        }
                        return -1;
                    }
                }

                string insertCommandText = $"INSERT INTO {ChatsTableName} (name) " +
                                           $"VALUES (@name);";

                SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                insertCommand.Parameters.AddWithValue("@name", "");

                if (insertCommand.ExecuteNonQueryAsync().Result > 0)
                {
                    var chatId = connection.LastInsertRowId;

                    insertCommandText = $"INSERT INTO {ChatsUsersTableName} (chat_id, user_id, user_state)" +
                                        $"VALUES (@chatId, @user1Id, 1), (@chatId, @user2Id, 1);";
                    insertCommand = new SQLiteCommand(insertCommandText, connection);
                    insertCommand.Parameters.AddWithValue("@chatId", chatId);
                    insertCommand.Parameters.AddWithValue("@user1Id", user1Id);
                    insertCommand.Parameters.AddWithValue("@user2Id", user2Id);

                    _ = insertCommand.ExecuteNonQueryAsync().Result;

                    return chatId;

                }
            }

            return -1;
        }

        public (int resultCode, List<ContactInfo> contactsInfo) GetContactsByNamePart(long userId, string namePattern)
        {
            List<ContactInfo> contactsInfo = new List<ContactInfo>();
            List<(long chat_id, string chat_name, long user_id)> buff = 
                new List<(long chat_id, string chat_name, long user_id)>();
            int resultCode = -1;

            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectCommandText = $"SELECT p0.id AS chat_id, p2.user_id AS user_id, p3.name AS user_name, p0.name AS chat_name " +
                    $"FROM {ChatsTableName} AS p0 " +
                    $"INNER JOIN {ChatsUsersTableName} AS p1 ON p1.chat_id = p0.id AND p1.user_id = @userId AND p1.user_state = 1 " +
                    $"INNER JOIN {ChatsUsersTableName} AS p2 ON p2.chat_id = p0.id AND p2.user_id IN " +
                    $"(SELECT id FROM {UsersTableName} WHERE (id) != (@userId) AND (name) LIKE (@namePattern) LIMIT 50)" +
                    $"INNER JOIN {UsersTableName} AS p3 ON p3.id = p2.user_id;";
                SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@namePattern", '%' + namePattern + '%');
                selectCommand.Parameters.AddWithValue("@userId", userId);

                using SQLiteDataReader contactsReader =  (SQLiteDataReader)selectCommand.ExecuteReaderAsync().Result;
                while (contactsReader.Read())
                {
                    if ((string)contactsReader["chat_name"] == string.Empty) 
                    {
                        buff.Add(((long)contactsReader["chat_id"],
                                 (string)contactsReader["user_name"],
                                 (long)contactsReader["user_id"]));
                    }
                    else
                    {
                        buff.Add(((long)contactsReader["chat_id"],
                                 (string)contactsReader["chat_name"],
                                 (long)contactsReader["user_id"]));
                    }
                }

                selectCommandText = $"SELECT * FROM {UsersTableName} WHERE (id) != (@userId) " +
                    $"AND (name) LIKE (@namePattern) LIMIT 50";
                selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@namePattern", '%' + namePattern + '%');
                selectCommand.Parameters.AddWithValue("@userId", userId); 
                using SQLiteDataReader usersReader =  (SQLiteDataReader)selectCommand.ExecuteReaderAsync().Result;

                while (usersReader.Read())
                {
                    if (!buff.Exists(el => el.user_id == (long)usersReader["id"]))
                    {
                        contactsInfo.Add(new ContactInfo((long)usersReader["id"], (string)usersReader["name"]));
                    }
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
                using (SQLiteDataReader contactReader = (SQLiteDataReader)selectChatsCommand.ExecuteReaderAsync().Result)
                {
                    if (contactReader.Read())
                    {
                        resultCode = 0;
                        string chatName = (string)contactReader["name"];

                        if (string.IsNullOrEmpty(chatName))
                        {
                            string selectChatNameCommandText = $"SELECT name FROM {UsersTableName} WHERE (id) IN " +
                                $" (SELECT user_id FROM {ChatsUsersTableName} WHERE (chat_id) = (@chatId)" +
                                $" AND (user_id) != (@userId));";

                            SQLiteCommand selectChatNameCommand = new SQLiteCommand(selectChatNameCommandText, connection);
                            selectChatNameCommand.Parameters.AddWithValue("@chatId", chatId);
                            selectChatNameCommand.Parameters.AddWithValue("@userId", userId);

                            using SQLiteDataReader chatNameReader = (SQLiteDataReader)selectChatNameCommand.ExecuteReaderAsync().Result;
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

        public List<MessageInfo> GetMessagesByChat(long chatId, long startMessageId = 0)
        {       
            List<MessageInfo> result = new List<MessageInfo>();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                SQLiteCommand selectMessageCommand;
                if (startMessageId == 0)
                {
                    string selectMessageCommandText = $"SELECT * FROM {MessagesTableName} WHERE (chat_id) = (@chatId)";
                    selectMessageCommand = new SQLiteCommand(selectMessageCommandText, connection);
                    selectMessageCommand.Parameters.AddWithValue("@chatId", chatId);
                }
                else
                {
                    string selectMessageCommandText = $"SELECT * FROM {MessagesTableName} WHERE (chat_id) = (@chatId) " +
                        $"AND (id) > (@startMessageId);";
                        //$"AND (strftime('%s', send_datetime)) > (strftime('%s', @startDate))";
                    selectMessageCommand = new SQLiteCommand(selectMessageCommandText, connection);
                    selectMessageCommand.Parameters.AddWithValue("@chatId", chatId);
                    selectMessageCommand.Parameters.AddWithValue("@startMessageId", startMessageId);
                }

                //read message
                using SQLiteDataReader messageReader = (SQLiteDataReader)selectMessageCommand.ExecuteReaderAsync().Result;
                while (messageReader.Read())
                {
                    long userId = (long)messageReader["user_id"];
                    long messageId = (long)messageReader["id"];

                    string sendDateTime = (string)messageReader["send_datetime"];
                    string messageText = (string)messageReader["data"];
                    var attachmentsGroupId = messageReader["attachments_group_id"];
                    List<AttachmentInfo> attachmentsInfo = new List<AttachmentInfo>();

                    // read attachments if exists
                    if (attachmentsGroupId != DBNull.Value) 
                    {
                        string selectAttachmentsIdsCommandText = $"SELECT attachments_ids FROM {AttachmentsGroupsTableName} " +
                        $"WHERE (id) = (@id)";
                        SQLiteCommand selectAttachmentsIdsCommand = new SQLiteCommand(selectAttachmentsIdsCommandText, connection);
                        selectAttachmentsIdsCommand.Parameters.AddWithValue("@id", (long)attachmentsGroupId);

                        using SQLiteDataReader attachmentsIdsReader = (SQLiteDataReader)selectAttachmentsIdsCommand.ExecuteReaderAsync().Result;
                        if (attachmentsIdsReader.Read())
                        {
                            string ids = (string)attachmentsIdsReader["attachments_ids"];
                            string[] idsText = ids.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var item in idsText)
                            {
                                long attachmentId = long.Parse(item.Trim());

                                string selectAttachmentCommandText = $"SELECT * FROM {AttachmentsTableName} WHERE (id) = (@attachmentId)";
                                SQLiteCommand selectAttachmentCommand = new SQLiteCommand(selectAttachmentCommandText, connection);
                                selectAttachmentCommand.Parameters.AddWithValue("@attachmentId", attachmentId);

                                //read attachment data
                                using SQLiteDataReader attachmentsReader = (SQLiteDataReader)selectAttachmentCommand.ExecuteReaderAsync().Result;
                                if (attachmentsReader.Read())
                                {
                                    string name = (string)attachmentsReader["name"];
                                    string filename = (string)attachmentsReader["filename"];
                                    long type = (long)attachmentsReader["type"];
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
                                               userId,
                                               sendDateTime,
                                               messageText,
                                               attachmentsInfo));
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
                                                $"(SELECT chat_id FROM {ChatsUsersTableName} WHERE " +
                                                $"(user_id) = (@userId) AND (user_state) = (1));";
                SQLiteCommand selectChatsCommand = new SQLiteCommand(selectChatsCommandText, connection);
                selectChatsCommand.Parameters.AddWithValue("@userId", userId);

                using (SQLiteDataReader contactReader = (SQLiteDataReader)selectChatsCommand.ExecuteReaderAsync().Result)
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

                            using SQLiteDataReader chatNameReader = (SQLiteDataReader)selectChatNameCommand.ExecuteReaderAsync().Result;
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

                    using SQLiteDataReader reader =  (SQLiteDataReader)selectCommand.ExecuteReaderAsync().Result;
                    if (reader.Read()) ids.Add((long)reader["id"]);
                    else {

                        SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                        insertCommand.Parameters.AddWithValue("@type", (long)item.Type);
                        insertCommand.Parameters.AddWithValue("@name", item.Name);
                        insertCommand.Parameters.AddWithValue("@filename", item.Filename);
                        if (insertCommand.ExecuteNonQueryAsync().Result > 0)
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
                attachmentsIds.ForEach(el => attachmentsIdsText += $"{el} ");
                string insertCommandText = $"INSERT INTO {AttachmentsGroupsTableName} (attachments_ids) " +
                    $"VALUES (@attachmentsIds)";
                SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                insertCommand.Parameters.AddWithValue("attachmentsIds", attachmentsIdsText);

                if (insertCommand.ExecuteNonQueryAsync().Result > 0) return connection.LastInsertRowId;
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
                long? attachmentsGroupId = null;
                if (attachmentsInfo.Count > 0)
                {
                    var attachmentsIds = AddAttachments(attachmentsInfo);
                    attachmentsGroupId = AddAttachmentsGroup(attachmentsIds);
                }
                if (attachmentsGroupId > 0 || attachmentsGroupId == null)
                {
                    string insertCommandText = $"INSERT INTO {MessagesTableName} " +
                        $"(chat_id, user_id, data, attachments_group_id, send_datetime) " +
                        $"VALUES (@chatId, @userId, @messageData, @attachmentsGroupId, @sendDate);";

                    SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                    insertCommand.Parameters.AddWithValue("@chatId", chatId);
                    insertCommand.Parameters.AddWithValue("@userId", userId);
                    insertCommand.Parameters.AddWithValue("@messageData", messageText);
                    if (attachmentsGroupId == null)
                    {
                        insertCommand.Parameters.AddWithValue("@attachmentsGroupId", DBNull.Value);
                    }
                    else
                    {
                        insertCommand.Parameters.AddWithValue("@attachmentsGroupId", attachmentsGroupId.Value);
                    }
                    insertCommand.Parameters.AddWithValue("@sendDate", sendDate);

                    if (insertCommand.ExecuteNonQueryAsync().Result > 0)
                    {
                        long messageId = connection.LastInsertRowId;

                        string updateCommandText = $"UPDATE {ChatsUsersTableName} " +
                            $"SET (user_state) = (1) WHERE (chat_id) = (@chatId)";
                        SQLiteCommand updateCommand = new SQLiteCommand(updateCommandText, connection);
                        updateCommand.Parameters.AddWithValue("@chatId", chatId);
                        _ = updateCommand.ExecuteNonQueryAsync().Result;

                        messageInfo = new MessageInfo(messageId,
                                                      chatId,
                                                      userId,
                                                      sendDate,
                                                      messageText,
                                                      attachmentsInfo);
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

                using SQLiteDataReader userReader =  (SQLiteDataReader)selectCommand.ExecuteReaderAsync().Result;
                while (userReader.Read())
                {
                    result.Add((long)userReader["user_id"]);
                }
            }
            return result;
        }

        internal List<(long id, string name)> GetChatUsers(long chatId)
        {
            List<(long id, string name)> result = new List<(long id, string name)>();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string selectCommandText = $"SELECT user_id, name FROM {ChatsUsersTableName} " +
                    $"INNER JOIN {UsersTableName} p1 WHERE user_id = p1.id AND chat_id = (@chatId);";

                SQLiteCommand selectCommand = new SQLiteCommand(selectCommandText, connection);
                selectCommand.Parameters.AddWithValue("@chatId", chatId);

                using SQLiteDataReader userReader =  (SQLiteDataReader)selectCommand.ExecuteReaderAsync().Result;
                while (userReader.Read())
                {
                    result.Add(((long)userReader["user_id"], (string)userReader["name"]));
                }
            }
            return result;
        }

        public void DeleteUserFromChat(long chatId, long userId)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string deleteCommandText = $"UPDATE {ChatsUsersTableName} SET (user_state) = (2) " +
                                           $"WHERE (chat_id, user_id) = (@chatId, @userId);";
                SQLiteCommand deleteCommand = new SQLiteCommand(deleteCommandText, connection);
                deleteCommand.Parameters.AddWithValue("@chatId", chatId);
                deleteCommand.Parameters.AddWithValue("@userId", userId);
                _ = deleteCommand.ExecuteNonQueryAsync().Result;
            }
        }

        public bool UpdateUserLogin(long id, string newLogin)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string updateCommandText = $"UPDATE {UsersTableName} SET (login) = (@newLogin) WHERE (id) = (@id);";
                SQLiteCommand updateCommand = new SQLiteCommand(updateCommandText, connection);
                updateCommand.Parameters.AddWithValue("@newLogin", newLogin);
                updateCommand.Parameters.AddWithValue("@id", id);
                try
                {
                    long count = updateCommand.ExecuteNonQueryAsync().Result;
                    return count == 1 ? true : false;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        public bool UpdateUserEmail(long id, string newEmail)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string updateCommandText = $"UPDATE {UsersTableName} SET (email) = (@newEmail) WHERE (id) = (@id);";
                SQLiteCommand updateCommand = new SQLiteCommand(updateCommandText, connection);
                updateCommand.Parameters.AddWithValue("@newEmail", newEmail);
                updateCommand.Parameters.AddWithValue("@id", id);
                try
                {
                    long count = updateCommand.ExecuteNonQueryAsync().Result;
                    return count == 1 ? true : false;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public bool UpdateUserName(long id, string newName)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string updateCommandText = $"UPDATE {UsersTableName} SET (name) = (@newName) WHERE (id) = (@id);";
                SQLiteCommand updateCommand = new SQLiteCommand(updateCommandText, connection);
                updateCommand.Parameters.AddWithValue("@newName", newName);
                updateCommand.Parameters.AddWithValue("@id", id);
                try
                {
                    long count = updateCommand.ExecuteNonQueryAsync().Result;
                    return count == 1 ? true : false;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        public bool UpdateUserPassword(long id, byte[] oldPassword, byte[] newPassword)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string updateCommandText = $"UPDATE {UsersTableName} SET (password) = (@newPassword) " +
                    $"WHERE (id, password) = (@id, @oldPassword);";

                SQLiteCommand updateCommand = new SQLiteCommand(updateCommandText, connection);
                updateCommand.Parameters.AddWithValue("@newPassword", newPassword);
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.Parameters.AddWithValue("@oldPassword", oldPassword);
                try
                {
                    long count = updateCommand.ExecuteNonQueryAsync().Result;
                    return count == 1 ? true : false;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
