using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlainFiles.Core
{
    public class UserService
    {
        private readonly string _filePath;
        private readonly List<User> _users = new();

        public UserService(string filePath)
        {
            _filePath = filePath;
        }

        public IReadOnlyList<User> GetAll() => _users.AsReadOnly();

        public void Load()
        {
            _users.Clear();

            if (!File.Exists(_filePath))
            {
                return;
            }

            var lines = File.ReadAllLines(_filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                if (parts.Length < 3)
                    continue;

                var username = parts[0].Trim();
                var password = parts[1].Trim();
                var activeText = parts[2].Trim();

                bool isActive = true;

                if (!bool.TryParse(activeText, out isActive))
                {
                    isActive = true;
                }

                var user = new User
                {
                    Username = username,
                    Password = password,
                    IsActive = isActive
                };

                _users.Add(user);
            }
        }

        public void Save()
        {
            var lines = _users.Select(u =>
                $"{u.Username},{u.Password},{u.IsActive.ToString().ToLower()}");

            File.WriteAllLines(_filePath, lines);
        }

        public User? Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return null;

            if (!user.IsActive)
            {
                return null;
            }

            if (user.Password == password)
            {
                return user;
            }

            return null;
        }

        public void BlockUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return;

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return;

            user.IsActive = false;
            Save();
        }
    }
}