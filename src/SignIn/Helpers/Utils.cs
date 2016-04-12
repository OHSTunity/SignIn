﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SignIn {
    public class Utils {

        /// <summary>
        /// Check if Email has the correct syntax
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        static public bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Build gravatar url
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        static public string GetGravatarUrl(string email) {

            using (MD5 md5Hash = MD5.Create()) {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant()));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++) {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return "http://www.gravatar.com/avatar/" + sBuilder.ToString() + "?s=32&d=mm";
            }
        }

        public static string RandomString(int Size) {
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;

            for (int i = 0; i < Size; i++) {
                ch = input[random.Next(0, input.Length)];
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
