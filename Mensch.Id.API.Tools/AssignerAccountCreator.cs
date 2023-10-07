using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Commons.IO;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    public class AssignerAccountCreator : DatabaseAccess
    {
        private readonly IPasswordHasher<IAccountWithPassword> passwordHasher = new PasswordHasher<IAccountWithPassword>();

        [Test]
        public async Task Create()
        {
            // ### START INPUT ###
            const string Name = "Jan";
            const string Username = "jan@mensch.id";
            const Language PreferedLanguage = Language.en;
            // ### END INPUT ####

            var passwordGenerator = new TemporaryPasswordGenerator { AllowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" };
            var password = passwordGenerator.Generate();
            Console.WriteLine($"Password: {password}");

            var accountCollection = GetCollection<Account>();
            if (await IsUsernameInUse(accountCollection, Username))
                throw new Exception("Account with the same email already exists");
            var account = new AssignerAccount
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Username = Username,
                PreferedLanguage = PreferedLanguage
            };
            account.PasswordHash = passwordHasher.HashPassword(account, password);

            await accountCollection.InsertOneAsync(account);
        }

        private static async Task<bool> IsUsernameInUse(
            IMongoCollection<Account> accountCollection,
            string username)
        {
            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.Eq(nameof(ProfessionalAccount.Username), username);
            return await accountCollection.Find(filter).AnyAsync();
        }

        [Test]
        [TestCase(@"F:\datasets\menschID-assigners.csv")]
        public async Task ImportFromCsv(string filePath)
        {
            var table = CsvReader.ReadTable(filePath, x => x);
            var accounts = new List<Account>();
            foreach (var row in table.Rows)
            {
                var hospital = row["Hospital"];
                var street = row["Street"];
                var postalCode = row["PostalCode"];
                var city = row["City"];
                var country = row["Country"];
                var username = row["Username"];
                var password = row["Password"];

                var account = new AssignerAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = hospital,
                    Username = username,
                    PreferedLanguage = Language.de,
                    Contact = new Contact
                    {
                        Address = new Address
                        {
                            Street = street,
                            PostalCode = postalCode,
                            City = city,
                            Country = country
                        }
                    }
                };
                account.PasswordHash = passwordHasher.HashPassword(account, password);
                accounts.Add(account);
            }
            var accountCollection = GetCollection<Account>();
            await accountCollection.InsertManyAsync(accounts);
        }

    }
}
