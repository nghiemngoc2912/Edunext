using Edunext.Models;

namespace Edunext.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string Email)
        {
            return new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(Email);
        }
        public static bool IsValidPassword(string Password)
        {
            return Password.Length >7;
        }

        public static bool CheckInputName(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false; // Trả về false nếu text là null hoặc rỗng

            return text.All(char.IsLetter);
        }
        public static bool CheckInputNotSpace(string text)
        {
            return !(string.IsNullOrEmpty(text)) ;
        }
    }
}
