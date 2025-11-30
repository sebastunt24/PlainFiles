using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PlainFiles.Core
{
    public class PersonService
    {
        private readonly string _filePath;
        private readonly List<Person> _people = new();

        public PersonService(string filePath)
        {
            _filePath = filePath;
        }

        public IReadOnlyList<Person> GetAll() => _people.AsReadOnly();

        public void Load()
        {
            _people.Clear();

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
                if (parts.Length < 6)
                    continue;

                if (!int.TryParse(parts[0], out int id))
                    continue;

                var firstName = parts[1].Trim();
                var lastName = parts[2].Trim();
                var phone = parts[3].Trim();
                var city = parts[4].Trim();

                if (!decimal.TryParse(
                        parts[5],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out decimal balance))
                {
                    continue;
                }

                var person = new Person
                {
                    Id = id,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    City = city,
                    Balance = balance
                };

                _people.Add(person);
            }
        }

        public void Save()
        {
            var lines = _people.Select(p =>
                string.Join(",",
                    p.Id,
                    p.FirstName,
                    p.LastName,
                    p.Phone,
                    p.City,
                    p.Balance.ToString(CultureInfo.InvariantCulture)));

            File.WriteAllLines(_filePath, lines);
        }

        public Person? GetById(int id)
        {
            return _people.FirstOrDefault(p => p.Id == id);
        }

        public void Delete(int id)
        {
            var person = GetById(id);
            if (person != null)
            {
                _people.Remove(person);
            }
        }

        public bool TryAdd(Person person, out string errorMessage)
        {
            if (person.Id <= 0)
            {
                errorMessage = "El ID debe ser un número entero positivo.";
                return false;
            }

            if (_people.Any(p => p.Id == person.Id))
            {
                errorMessage = $"Ya existe una persona con el ID {person.Id}.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(person.FirstName))
            {
                errorMessage = "El nombre no puede estar vacío.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(person.LastName))
            {
                errorMessage = "El apellido no puede estar vacío.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(person.Phone))
            {
                errorMessage = "El teléfono no puede estar vacío.";
                return false;
            }

            var digitsOnly = new string(person.Phone.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
            {
                errorMessage = "El teléfono debe contener entre 7 y 15 dígitos.";
                return false;
            }

            if (person.Balance <= 0)
            {
                errorMessage = "El saldo debe ser mayor que cero.";
                return false;
            }

            _people.Add(person);
            errorMessage = string.Empty;
            return true;
        }

        public bool TryUpdate(Person person, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(person.FirstName))
            {
                errorMessage = "El nombre no puede estar vacío.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(person.LastName))
            {
                errorMessage = "El apellido no puede estar vacío.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(person.Phone))
            {
                errorMessage = "El teléfono no puede estar vacío.";
                return false;
            }

            var digitsOnly = new string(person.Phone.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
            {
                errorMessage = "El teléfono debe contener entre 7 y 15 dígitos.";
                return false;
            }

            if (person.Balance <= 0)
            {
                errorMessage = "El saldo debe ser mayor que cero.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public IEnumerable<IGrouping<string, Person>> GetPeopleGroupedByCity()
        {
            return _people
                .GroupBy(p => string.IsNullOrWhiteSpace(p.City) ? "SIN CIUDAD" : p.City)
                .OrderBy(g => g.Key);
        }

        public decimal GetTotalBalance()
        {
            return _people.Sum(p => p.Balance);
        }
    }
}