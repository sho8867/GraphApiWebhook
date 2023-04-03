using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Linq;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation(req.Path.Value);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            Microsoft.Extensions.Primitives.StringValues queryVal;
            if (req.Query.TryGetValue("validationToken", out queryVal))
            {
                var token = queryVal.ToString();
                log.LogInformation("validationToken include ");
                return new OkObjectResult(token);
            }

            var notifications = JsonConvert.DeserializeObject<Notifications>(requestBody);
            if(notifications.Items.Count() == 0)
            {
                log.LogInformation("value not found");
                return new OkObjectResult("OK");
            }

            // 対称キーを復号する
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider();
            RSAParameters param = RsaUtil.CreateParameter("<RSAKey"); // RSA key
            rsaProvider.ImportParameters(param);

            foreach (var notification in notifications.Items)
            {
                byte[] encryptedSymmetricKey = Convert.FromBase64String(notification.encryptedContent.DataKey);
                byte[] decryptedSymmetricKey = rsaProvider.Decrypt(encryptedSymmetricKey, fOAEP: true);

                // HMAC - SHA256 を使用してデータの署名を比較する
                byte[] encryptedPayload = Convert.FromBase64String(notification.encryptedContent.Data);  //< the value from the data property, still encrypted>;
                byte[] expectedSignature = Convert.FromBase64String(notification.encryptedContent.DataSignature);
                byte[] actualSignature;

                using (HMACSHA256 hmac = new HMACSHA256(decryptedSymmetricKey))
                {
                    actualSignature = hmac.ComputeHash(encryptedPayload);
                }

                if (actualSignature.SequenceEqual(expectedSignature))
                {
                    // リソース データ コンテンツを復号する
                    AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider();
                    aesProvider.Key = decryptedSymmetricKey;
                    aesProvider.Padding = PaddingMode.PKCS7;
                    aesProvider.Mode = CipherMode.CBC;
                    //var aes = Aes.Create();
                    //aes.Key = decryptedSymmetricKey;
                    //aes.Padding = PaddingMode.PKCS7;
                    //aes.Mode = CipherMode.CBC;

                    // Obtain the intialization vector from the symmetric key itself.
                    int vectorSize = 16;
                    byte[] iv = new byte[vectorSize];
                    Array.Copy(decryptedSymmetricKey, iv, vectorSize);
                    aesProvider.IV = iv;

                    string decryptedResourceData;
                    // Decrypt the resource data content.
                    using (var decryptor = aesProvider.CreateDecryptor())
                    {
                        using (MemoryStream msDecrypt = new MemoryStream(encryptedPayload))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    decryptedResourceData = srDecrypt.ReadToEnd();
                                    log.LogInformation(decryptedResourceData);
                                }
                            }
                        }
                    }

                }
                else
                {
                    log.LogInformation("actualSignature unmatch");
                }
            }
            return new OkObjectResult("OK");
        }
    }
}
