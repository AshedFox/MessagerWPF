using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Messager
{
    public static class Validation
    {
        public static string CheckLogin(string login)
        {
            if (!Regex.IsMatch(login, @"[a-z0-9_\-]{3,50}", RegexOptions.IgnoreCase))
            {
                return "Логин может содержать цифры, буквы латинского алфавита, _ или -, а также " +
                       "должен быть длиной от 3 до 50 символов";
            }
            return "";
        }

        public static string CheckEmail(string email)
        {
            string pattern =
                @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";
            if (!Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase))
            {
                return "Email имеет неправильный формат";
            }
            return "";
        }

        public static string CheckPassword(string password)
        {
            Regex regex = new Regex(@"^\w{8,30}$");
            if (!regex.IsMatch(password))
            {
                return "Пароль должен быть длиной от 8 до 30 символов";
            }
            return "";
        }

        public static string CheckName(string name)
        {
            Regex regex = new Regex(@"^[\w\s]{3,50}$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(name))
            {
                return "Никнейм может содержать цифры, буквы, _ или -, а также " +
                       "должен быть длиной от 3 до 50 символов";
            }
            return "";
        }
    }
}
