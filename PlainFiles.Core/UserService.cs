using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlainFiles.Core;

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

        /// <summary>
        /// Devuelve la lista de usuarios en memoria (solo lectura).
        /// </summary>
        public IReadOnlyList<User> GetAll() => _users.AsReadOnly();

        /// <summary>
        /// Carga el archivo Users.txt en memoria.
        /// Si el archivo no existe, crea una lista vacía.
        /// </summary>
        public void Load()
        {
            _users.Clear();

            if (!File.Exists(_filePath))
            {
                // Si no existe el archivo, no lanzamos excepción, solo dejamos la lista vacía.
                return;
            }

            var lines = File.ReadAllLines(_filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Formato esperado: usuario,contraseña,activo
                var parts = line.Split(',');

                if (parts.Length < 3)
                    continue; // línea mal formada, la ignoramos

                var username = parts[0].Trim();
                var password = parts[1].Trim();
                var activeText = parts[2].Trim();

                bool isActive = true;
                // Intentamos interpretar el tercer campo como booleano
                if (!bool.TryParse(activeText, out isActive))
                {
                    // Si no se puede interpretar, asumimos que está activo
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

        /// <summary>
        /// Guarda la lista de usuarios en el archivo Users.txt
        /// usando el formato: usuario,contraseña,activo
        /// </summary>
        public void Save()
        {
            var lines = _users.Select(u =>
                $"{u.Username},{u.Password},{u.IsActive.ToString().ToLower()}");

            File.WriteAllLines(_filePath, lines);
        }

        /// <summary>
        /// Autentica un usuario:
        /// - Debe existir en la lista
        /// - Debe estar activo (IsActive = true)
        /// - La contraseña debe coincidir exactamente
        /// Devuelve el User si es válido; null en caso contrario.
        /// </summary>
        public User? Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            // Buscamos por nombre de usuario, ignorando mayúsculas/minúsculas
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return null;

            if (!user.IsActive)
            {
                // Usuario bloqueado
                return null;
            }

            // Comparación exacta de contraseña (podrías mejorarla con hash en un futuro)
            if (user.Password == password)
            {
                return user;
            }

            return null;
        }

        /// <summary>
        /// Bloquea al usuario (IsActive = false) y guarda los cambios en el archivo.
        /// Si el usuario no existe, no hace nada.
        /// </summary>
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