using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;  

namespace AnimalShelter.Validation
{
    class Validator
    {
        static string numberPattern = "^([0-9]{1,5})$";
        static string textPattern = "([A-z]|[А-я]|\'s'){1,45}$";
        static string pinPattern = "^([0-9]){10}$";
        static string usernameAndPasswordPattern = "^([A-Za-z0-9]){3,45}$";
        static string phoneNumberPattern = "^(\'+'[0-9]{3}[0-9]{9})|([0-9]{10})$";
        static string addressPattern = "([A-z]|[А-я]|[0-9]|\'s'){1,45}$";
        static string emailPattern = "^([A-z0-9._-]+@[A-z0-9.-]+.[A-z]){1,45}$";

        public static bool NumberValidator(TextBox input)
        {
            return Regex.IsMatch(input.Text, numberPattern);
        }

        public static bool TextValidator(TextBox input)
        {
            return Regex.IsMatch(input.Text, textPattern);
        }

        public static bool PinValidator(TextBox input)
        {
            return Regex.IsMatch(input.Text, pinPattern);
        }

        public static bool UsernameAndPasswordValidator(TextBox input)
        {
            return Regex.IsMatch(input.Text, usernameAndPasswordPattern);
        }

        public static bool UsernameAndPasswordValidator(PasswordBox input)
        {
            return Regex.IsMatch(input.Password.ToString(), usernameAndPasswordPattern);
        }

        public static bool PhoneNumberValidator(TextBox input)
        {
            return Regex.IsMatch(input.Text, phoneNumberPattern);
        }

        public static bool AddressValidator(TextBox input)
        {
            return Regex.IsMatch(input.Text, addressPattern);
        }
        public static bool EmailValidator(TextBox input)
        {
            return Regex.IsMatch(input.Text, emailPattern);
        }
    }
}
