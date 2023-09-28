using System;
using System.Threading.Tasks;
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
        private readonly IPasswordHasher<LocalAnonymousAccount> passwordHasher = new PasswordHasher<LocalAnonymousAccount>();

        [Test]
        public async Task Create()
        {
            // ### START INPUT ###
            const string Name = "Jan";
            const string Email = "jan@mensch.id";
            const Language PreferedLanguage = Language.en;
            // ### END INPUT ####

            var passwordGenerator = new TemporaryPasswordGenerator { AllowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" };
            var password = passwordGenerator.Generate();
            Console.WriteLine($"Password: {password}");

            var accountCollection = GetCollection<Account>();
            if (await IsEmailInUse(accountCollection, Email))
                throw new Exception("Account with the same email already exists");
            var account = new AssignerAccount
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Email = Email,
                IsEmailVerified = true,
                PreferedLanguage = PreferedLanguage
            };
            account.PasswordHash = passwordHasher.HashPassword(account, password);

            await accountCollection.InsertOneAsync(account);
        }

        private static async Task<bool> IsEmailInUse(
            IMongoCollection<Account> accountCollection,
            string email)
        {
            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.Eq(nameof(LocalAccount.Email), email);
            return await accountCollection.Find(filter).AnyAsync();
        }
    }
}
