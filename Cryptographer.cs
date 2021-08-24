using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace zakupki.gov.ru
{
    internal static class Cryptographer
    {
        public static string Encryption(string ishText, string pass = "T6jfGux",
       string sol = "zakupki.gov.ru", string cryptographicAlgorithm = "SHA1",
       int passIter = 2, string initVec = "a8doSuDitOz1hZe#",
       int keySize = 256)
        {
            if (string.IsNullOrEmpty(ishText))
                return "";

            byte[] initVecB = Encoding.ASCII.GetBytes(initVec);
            byte[] solB = Encoding.ASCII.GetBytes(sol);
            byte[] ishTextB = Encoding.UTF8.GetBytes(ishText);

            PasswordDeriveBytes derivPass = new PasswordDeriveBytes(pass, solB, cryptographicAlgorithm, passIter);
            byte[] keyBytes = derivPass.GetBytes(keySize / 8);
            RijndaelManaged symmK = new RijndaelManaged
            {
                Mode = CipherMode.CBC
            };

            byte[] cipherTextBytes = null;

            using (ICryptoTransform encryptor = symmK.CreateEncryptor(keyBytes, initVecB))
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(ishTextB, 0, ishTextB.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherTextBytes = memStream.ToArray();
                        memStream.Close();
                        cryptoStream.Close();
                    }
                }
            }

            symmK.Clear();
            return Convert.ToBase64String(cipherTextBytes);
        }
        private static string Decryption(string ciphText, string pass = "T6jfGux",
           string sol = "zakupki.gov.ru", string cryptographicAlgorithm = "SHA1",
           int passIter = 2, string initVec = "a8doSuDitOz1hZe#", int keySize = 256)
        {
            try
            {
                if (string.IsNullOrEmpty(ciphText))
                    return "";

                byte[] initVecB = Encoding.ASCII.GetBytes(initVec);
                byte[] solB = Encoding.ASCII.GetBytes(sol);
                byte[] cipherTextBytes = Convert.FromBase64String(ciphText);

                PasswordDeriveBytes derivPass = new PasswordDeriveBytes(pass, solB, cryptographicAlgorithm, passIter);
                byte[] keyBytes = derivPass.GetBytes(keySize / 8);

                RijndaelManaged symmK = new RijndaelManaged();
                symmK.Mode = CipherMode.CBC;

                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int byteCount = 0;

                using (ICryptoTransform decryptor = symmK.CreateDecryptor(keyBytes, initVecB))
                {
                    using (MemoryStream mSt = new MemoryStream(cipherTextBytes))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(mSt, decryptor, CryptoStreamMode.Read))
                        {
                            byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            mSt.Close();
                            cryptoStream.Close();
                        }
                    }
                }

                symmK.Clear();
                return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
            }
            catch
            {
                return Membership.GeneratePassword(10, 2);
            }
        }

        private static string Encryption64(string text)
        {
            byte[] simpleTextBytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(simpleTextBytes);
        }
        private static string Decryption64(string text)
        {
            byte[] enTextBytes = Convert.FromBase64String(text);
            return Encoding.UTF8.GetString(enTextBytes);
        }
        private static string GetHDDSerial()
        {
            string HDDSerial = string.Empty;
            ManagementObjectCollection searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive").Get();
            foreach (ManagementBaseObject wmi_HDD in searcher)
            {
                HDDSerial = wmi_HDD["SerialNumber"].ToString();
                break;
            }

            return HDDSerial;
        }
        public static bool CheckActivationApp(string activationNumber)
        {
            string dec = Decryption(activationNumber);
            string lic = GetLicenseKey();
            return dec == lic;
        }
        public static string GetLicenseKey()
        {
            return Encryption64(GetHDDSerial());
        }

    }
}
