using System;

namespace SV22T1020670.Shop.AppCodes
{
    public static class StringHelper
    {
        /// <summary>
        /// Che giấu số điện thoại (VD: 0912345678 -> 091****678)
        /// </summary>
        public static string MaskPhone(this string phone)
        {
            if (string.IsNullOrEmpty(phone)) return "";

            phone = phone.Trim();

            if (phone.Length < 7) return phone; 

            return phone.Substring(0, 3) + "****" + phone.Substring(phone.Length - 3);
        }
    }
}