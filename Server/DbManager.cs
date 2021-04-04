using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.IO;
using ClientServerLib;

namespace Server
{
    public class ClientInfo
    {
        public long id;
        public string login;
        public string email;
        public string password;
        public string name;

        public ClientInfo()
        {
        }

        public ClientInfo(string login, string email, string password, string name)
        {
            this.login = login;
            this.email = email;
            this.password = password;
            this.name = name;
        }

        public ClientInfo(long id, string login, string email, string password, string name)
        {
            this.id = id;
            this.login = login;
            this.email = email;
            this.password = password;
            this.name = name;
        }
    }

    class DbManager
    {

        const string UsersDbName = "Users";

        SQLiteConnection connection;

        public DbManager()
        {
            if (!File.Exists(@"messenger.db"))
            {
                SQLiteConnection.CreateFile("messenger.db");
            }
        }

        public void ConnectToDB()
        {
            connection = new SQLiteConnection(@"DataSource=messenger.db; Version=3;");

            string createCommandText = $@"CREATE TABLE IF NOT EXISTS {UsersDbName}(
                                        id    INTEGER NOT NULL UNIQUE,
                                        login TEXT,
                                        email TEXT,
                                        password  TEXT,
                                        name  TEXT,
                                        PRIMARY KEY(id AUTOINCREMENT));";

            SQLiteCommand createCommand = new SQLiteCommand(createCommandText, connection);
            connection.Open();
            createCommand.ExecuteNonQuery();
        }

        public void CloseConnectionToDB()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }

        public (long id, IdentificationResult resultCode) AddClient(ClientInfo clientInfo)
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                string insertCommandText = $@"INSERT INTO {UsersDbName} (login, email, password, name)
                                          VALUES (@login,@email,@password,@name);";

                SQLiteCommand insertCommand = new SQLiteCommand(insertCommandText, connection);
                //insertCommand.Parameters.AddWithValue("@id", DBNull.Value);
                insertCommand.Parameters.AddWithValue("@login", clientInfo.login);
                insertCommand.Parameters.AddWithValue("@email", clientInfo.email);
                insertCommand.Parameters.AddWithValue("@password", clientInfo.password);
                insertCommand.Parameters.AddWithValue("@name", clientInfo.name);

                try
                {
                    insertCommand.ExecuteNonQuery();

                    string selectCommandText = $@"SELECT last_insert_rowid();";

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
                string selectCommandText = $@"SELECT * FROM {UsersDbName} WHERE (login) = (@loginOrEmail) or (email) = (@loginOrEmail);";

                SQLiteCommand insertCommand = new SQLiteCommand(selectCommandText, connection);
                insertCommand.Parameters.AddWithValue("@loginOrEmail", loginOrEmail);

                using (SQLiteDataReader reader = insertCommand.ExecuteReader())
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
    }
}
