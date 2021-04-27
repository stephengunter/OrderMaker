using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security
{
	public class AESEncrypt
	{

		public static byte[] EncryptionByAES(string plainText, byte[] key, byte[] IV)
		{
			byte[] encrypted;

			if (plainText == null || plainText.Length <= 0) throw new ArgumentNullException("empty plainText.");

			if (key == null || key.Length <= 0) throw new ArgumentNullException("empty key.");

			if (IV == null || IV.Length <= 0) throw new ArgumentNullException("empty IV");

			using (Aes aes = Aes.Create())
			{
				aes.Key = key;
				aes.IV = IV;

				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (MemoryStream mEncrypt = new MemoryStream())
				{
					using (CryptoStream cEncrypt = new CryptoStream(mEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swStream = new StreamWriter(cEncrypt))
						{
							swStream.Write(plainText);
						}

						encrypted = mEncrypt.ToArray();
					}
				}
			}


			return encrypted;
		}

		public static string DecryptionByAES(byte[] cipherText, byte[] key, byte[] IV)
		{
			string decrypted;

			if (cipherText == null || cipherText.Length <= 0) throw new ArgumentNullException("empty cipherText");

			if (key == null || key.Length <= 0) throw new ArgumentNullException("empty key");

			if (IV == null || IV.Length <= 0) throw new ArgumentNullException("empty IV");


			using (Aes aes = Aes.Create())
			{
				aes.Key = key;
				aes.IV = IV;

				ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV);

				using (MemoryStream mDecrypt = new MemoryStream(cipherText))
				{

					using (CryptoStream cDecrypt = new CryptoStream(mDecrypt, encryptor, CryptoStreamMode.Read))
					{

						using (StreamReader swStream = new StreamReader(cDecrypt))
						{

							decrypted = swStream.ReadToEnd();
						}
					}
				}
			}
			return decrypted;
		}
	}
}
