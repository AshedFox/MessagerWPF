﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerLib
{
    public enum DataPrefix
    {
        Text,
        Audio,
        Video,
        Image,
        File
    }

    public enum SystemMessageType
    {
        Register,
        Autorize,
        GetAllUserContacts,
        AddChat,
        GetChatUsers,
        GetNewChatMessages,
        RequestChatMessagesUpdate,
        SearchContacts,
        DeleteContact,
        GetAttachment,
        UpdateUserInfo
    }

    public enum UpdateType
    {
        Email,
        Name,
        Login,
        Password
    }

    public enum MessagePrefix
    {
        SystemMessage,
        DefaultMessage
    }

    public enum IdentificationResult
    {
        TIMEOUT = -1,
        ALL_OK = 0,
        DB_CONNECTION_ERROR = 1,
        USER_NOT_FOUND = 2,
        USER_ALREADY_EXISTS = 3,
        USER_SELECTION_ERROR = 4,
        INCORRECT_PASSWORD = 5,
        ANSWER_RECIEVING_ERROR = 6,
        UNDEFINED_ERROR
    }
}
