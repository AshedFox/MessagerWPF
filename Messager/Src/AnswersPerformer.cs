using ClientServerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public static class AnswersPerformer
    {
        public static string PerformIdentificationResult(IdentificationResult identificationResult)
        {
            switch (identificationResult) {
                case IdentificationResult.ALL_OK:
                    return string.Empty;
                case IdentificationResult.TIMEOUT:
                    return "Первышено время обработки запроса сервером";
                case IdentificationResult.USER_NOT_FOUND:
                    return "Пользователь с заданным логином не существует";
                case IdentificationResult.DB_CONNECTION_ERROR:
                    return "Не удалось получить данные о пользователях";
                case IdentificationResult.USER_ALREADY_EXISTS:
                    return "Не удалось зарегистрироваться: логин или email уже занят";
                case IdentificationResult.INCORRECT_PASSWORD:
                    return "Неверный пароль";
                case IdentificationResult.ANSWER_RECIEVING_ERROR:
                    return "Не удалось получить ответ от сервера";
                case IdentificationResult.UNDEFINED_ERROR:
                    return "Неизвестная ошибка";
                default:
                    return "Неизвестная ошибка";
            }
        }
    }
}
