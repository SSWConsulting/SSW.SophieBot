using SSWSophieBot.HttpClientAction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Clients
{
    public class GravatarManager : IAvatarManager
    {
        public Task<List<GetEmployeeModel>> GetAvatarUrlsAsync(IEnumerable<GetEmployeeModel> employees)
        {
            if (employees == null || !employees.Any())
            {
                return Task.FromResult(new List<GetEmployeeModel>());
            }

            var resultEmployees = employees.ToList();
            resultEmployees.ForEach(e =>
            {
                if (string.IsNullOrWhiteSpace(e.AvatarUrl) && !string.IsNullOrWhiteSpace(e.EmailAddress))
                {
                    var md5 = new MD5CryptoServiceProvider();
                    var emailData = Encoding.UTF8.GetBytes(e.EmailAddress.ToLower());
                    var hashedData = md5.ComputeHash(emailData);
                    var encryptedString = BitConverter.ToString(hashedData).Replace("-", string.Empty).ToLower();

                    e.AvatarUrl = $"https://secure.gravatar.com/avatar/{encryptedString}?s=100&d=identicon";
                }
            });

            return Task.FromResult(resultEmployees);
        }
    }
}
