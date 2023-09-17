// See https://aka.ms/new-console-template for more information



using System.Security.Cryptography;
using System.Text;

using var rsa = RSA.Create();
//var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
//var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

var publicKey = rsa.ExportSubjectPublicKeyInfoPem();
var privateKey = rsa.ExportPkcs8PrivateKeyPem();

// -- 클라이언트측
//using var rsa2 = RSA.Create();
//rsa2.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
//var encryptData = rsa2.Encrypt("hello"u8, RSAEncryptionPadding.OaepSHA256);

// 1. https://jsfiddle.net/ 에서 수행
// 2. 리소스 추가(url): https://cdnjs.cloudflare.com/ajax/libs/jsencrypt/3.2.1/jsencrypt.min.js
var encryptData = Convert.FromBase64String("FPF90LLc+RfzqodRotvnwhbW5A7MzE9OBZ8IA+CnI9KwP6/BskmF9xDnDI/+m/MVyOUgsPC/VOA6GpQdzpsZlSzwbMBCxXg8SA2qaZzZiMNrUFpwFbdhHte0CHWIoORaGTPnBHhxY18sXkO2Eyn6DG7RWhpfxPD+tcCvEjoihAdgGp/ma9Vw3jrVYUuJI7587N8vXUOaMMwEx03/X7A56JJ1GQwKQh0ZDjtNzdL9seH5uz+MahJzO0JRVmULkAK1ZC1RQmjI6yL3YQ2wPeCJujnadbI6WbkrRoKoHXhn86Z9nUFFps+iBj2/OE2Mu7+Peuq328hlkATZIVQgwYyfPw==");

// -- 서버측
using var rsa3 = RSA.Create();
//rsa3.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
rsa3.ImportFromPem(privateKey);
//var decryptData = rsa3.Decrypt(encryptData, RSAEncryptionPadding.OaepSHA256);
var decryptData = rsa3.Decrypt(encryptData, RSAEncryptionPadding.Pkcs1);

Console.WriteLine(Encoding.UTF8.GetString(decryptData));
