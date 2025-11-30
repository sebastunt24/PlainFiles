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

        /// <summary>
        /// Devuelve todas las personas en memoria.
        /// </summary>
        public IReadOnlyList<Person> GetAll() => _people.AsReadOnly();

        /// <summary>
        /// Carga el archivo de personas al iniciar el programa.
        /// Formato esperado por línea:
        /// Id,FirstName,LastName,Phone,City,Balance
        /// </summary>
        public void Load()
        {
            _people.Clear();

            if (!File.Exists(_filePath))
            {
                // Si no existe el archivo, no lanzamos error; arrancamos con lista vacía.
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

        /// <summary>
        /// Guarda el archivo de personas.
        /// </summary>
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

        /// <summary>
        /// Busca una persona por ID.
        /// </summary>
        public Person? GetById(int id)
        {
            return _people.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Elimina una persona por ID (si existe).
        /// </summary>
        public void Delete(int id)
        {
            var person = GetById(id);
            if (person != null)
            {
                _people.Remove(person);
            }
        }

        /// <summary>
        /// Intenta agregar una persona aplicando las validaciones del taller:
        /// - ID numérico y positivo
        /// - ID único
        /// - Nombres y apellidos no vacíos
        /// - Teléfono válido (solo dígitos, 7 a 15 caracteres)
        /// - Balance positivo
        /// </summary>
        public bool TryAdd(Person person, out string errorMessage)
        {
            // ID positivo
            if (person.Id <= 0)
            {
                errorMessage = "El ID debe ser un número entero positivo.";
                return false;
            }

            // ID único
            if (_people.Any(p => p.Id == person.Id))
            {
                errorMessage = $"Ya existe una persona con el ID {person.Id}.";
                return false;
            }

            // Nombre obligatorio
            if (string.IsNullOrWhiteSpace(person.FirstName))
            {
                errorMessage = "El nombre no puede estar vacío.";
                return false;
            }

            // Apellido obligatorio
            if (string.IsNullOrWhiteSpace(person.LastName))
            {
                errorMessage = "El apellido no puede estar vacío.";
                return false;
            }

            // Teléfono "válido": solo dígitos, longitud razonable
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

            // Balance positivo
            if (person.Balance <= 0)
            {
                errorMessage = "El saldo debe ser mayor que cero.";
                return false;
            }

            // Si todo está bien, agregamos a la lista
            _people.Add(person);
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Intenta actualizar una persona ya existente aplicando validaciones:
        /// - Nombres y apellidos no vacíos
        /// - Teléfono válido (solo dígitos, 7 a 15 caracteres)
        /// - Balance positivo
        /// No verifica unicidad de ID porque se asume que no cambia.
        /// </summary>
        public bool TryUpdate(Person person, out string errorMessage)
        {
            // Nombre obligatorio
            if (string.IsNullOrWhiteSpace(person.FirstName))
            {
                errorMessage = "El nombre no puede estar vacío.";
                return false;
            }

            // Apellido obligatorio
            if (string.IsNullOrWhiteSpace(person.LastName))
            {
                errorMessage = "El apellido no puede estar vacío.";
                return false;
            }

            // Teléfono "válido": solo dígitos, longitud razonable
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

            // Balance positivo
            if (person.Balance <= 0)
            {
                errorMessage = "El saldo debe ser mayor que cero.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        // ==============================
        //   PUNTO F – INFORME POR CIUDAD
        // ==============================

        /// <summary>
        /// Devuelve las personas agrupadas por ciudad, ordenadas por ciudad.
        /// Cada grupo contiene la lista de personas de esa ciudad.
        /// </summary>
        public IEnumerable<IGrouping<string, Person>> GetPeopleGroupedByCity()
        {
            return _people
                .GroupBy(p => string.IsNullOrWhiteSpace(p.City) ? "SIN CIUDAD" : p.City)
                .OrderBy(g => g.Key);
        }

        /// <summary>
        /// Devuelve el saldo total general de todas las personas.
        /// </summary>
        public decimal GetTotalBalance()
        {
            return _people.Sum(p => p.Balance);
        }
    }
}