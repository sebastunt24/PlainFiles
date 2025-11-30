using PlainFiles.Core;

Console.Title = "Sistema de Gestión – PlainFiles";

// ===============================================
//   RUTAS DE ARCHIVOS
// ===============================================

string basePath = AppDomain.CurrentDomain.BaseDirectory;
string peopleFile = Path.Combine(basePath, "people.txt");
string usersFile = Path.Combine(basePath, "Users.txt");
string logFile = Path.Combine(basePath, "log.txt");

// ===============================================
//   SERVICIOS PRINCIPALES
// ===============================================

var userService = new UserService(usersFile);
var personService = new PersonService(peopleFile);
var logger = new FileLogger(logFile);

// Cargar usuarios y personas desde archivo
userService.Load();
personService.Load();

// ===============================================
//   LOGIN (3 intentos con bloqueo)
// ===============================================

User? currentUser = null;
int attempts = 3;

while (attempts > 0 && currentUser == null)
{
    Console.Clear();
    Console.WriteLine("=== LOGIN ===");
    Console.Write("Usuario: ");
    string username = Console.ReadLine() ?? string.Empty;

    Console.Write("Contraseña: ");
    string password = Console.ReadLine() ?? string.Empty;

    var user = userService.Authenticate(username, password);
    if (user != null)
    {
        currentUser = user;
        logger.Log(username, "LOGIN", "OK");
        break;
    }

    // Falló el login
    attempts--;

    if (attempts > 0)
    {
        logger.Log(username, "LOGIN", "FAILED");
        Console.WriteLine($"Credenciales incorrectas. Intentos restantes: {attempts}");
    }
    else
    {
        // Se agotaron los intentos: bloqueamos al usuario
        userService.BlockUser(username);
        logger.Log(username, "LOGIN", "FAILED_3_TIMES_USER_BLOCKED");
        Console.WriteLine("Usuario bloqueado por intentos fallidos.");
    }

    Thread.Sleep(1500);
}

if (currentUser == null)
{
    Console.WriteLine("Acceso denegado. Contacte al administrador para desbloqueo.");
    return;
}

// ===============================================
//   MENÚ PRINCIPAL
// ===============================================

string option = "";
while (option != "0")
{
    Console.Clear();
    Console.WriteLine("====================================");
    Console.WriteLine("1. Mostrar personas");
    Console.WriteLine("2. Agregar persona");
    Console.WriteLine("3. Editar persona");
    Console.WriteLine("4. Borrar persona");
    Console.WriteLine("5. Guardar cambios");
    Console.WriteLine("6. Informe por ciudad");
    Console.WriteLine("0. Salir");
    Console.WriteLine("====================================");
    Console.Write("Seleccione una opción: ");
    option = Console.ReadLine() ?? string.Empty;

    Console.Clear();

    switch (option)
    {
        case "1":
            ShowPeople();
            break;

        case "2":
            AddPerson();
            break;

        case "3":
            EditPerson();
            break;

        case "4":
            DeletePerson();
            break;

        case "5":
            personService.Save();
            userService.Save();
            logger.Log(currentUser!.Username, "SAVE", "OK");
            Console.WriteLine("Cambios guardados correctamente.");
            break;

        case "6":
            ReportByCity();
            break;

        case "0":
            Console.WriteLine("Saliendo...");
            break;

        default:
            Console.WriteLine("Opción no válida.");
            break;
    }

    Console.WriteLine("\nPresione ENTER para continuar...");
    Console.ReadLine();
}

// ===============================================
//   MÉTODOS DEL MENÚ
// ===============================================

void ShowPeople()
{
    Console.WriteLine("=== LISTADO DE PERSONAS ===\n");

    var people = personService.GetAll();
    if (people.Count == 0)
    {
        Console.WriteLine("No hay personas registradas.");
        return;
    }

    foreach (var p in people)
    {
        Console.WriteLine($"ID: {p.Id}");
        Console.WriteLine($"Nombre: {p.FirstName} {p.LastName}");
        Console.WriteLine($"Teléfono: {p.Phone}");
        Console.WriteLine($"Ciudad: {p.City}");
        Console.WriteLine($"Saldo: {p.Balance:C2}");
        Console.WriteLine("-----------------------------------");
    }
}

void AddPerson()
{
    Console.WriteLine("=== AGREGAR PERSONA ===");

    // ID numérico
    int id;
    while (true)
    {
        Console.Write("ID (numérico y positivo): ");
        var idText = Console.ReadLine();

        if (!int.TryParse(idText, out id))
        {
            Console.WriteLine("El ID debe ser un número entero.");
            continue;
        }

        if (id <= 0)
        {
            Console.WriteLine("El ID debe ser mayor que cero.");
            continue;
        }

        // Si llega aquí, el ID es numérico y > 0
        break;
    }

    Console.Write("Nombre: ");
    string firstName = Console.ReadLine() ?? string.Empty;

    Console.Write("Apellido: ");
    string lastName = Console.ReadLine() ?? string.Empty;

    Console.Write("Teléfono: ");
    string phone = Console.ReadLine() ?? string.Empty;

    Console.Write("Ciudad: ");
    string city = Console.ReadLine() ?? string.Empty;

    Console.Write("Saldo: ");
    var balanceText = Console.ReadLine();
    if (!decimal.TryParse(balanceText, out decimal balance))
    {
        Console.WriteLine("El saldo debe ser un número válido.");
        return;
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

    if (personService.TryAdd(person, out string errorMessage))
    {
        logger.Log(currentUser!.Username, "ADD_PERSON", $"ID={id}");
        Console.WriteLine("Persona agregada correctamente.");
    }
    else
    {
        Console.WriteLine($"No se pudo agregar la persona: {errorMessage}");
    }
}

void EditPerson()
{
    Console.WriteLine("=== EDITAR PERSONA ===");
    Console.Write("Ingrese el ID de la persona que desea editar: ");
    var idText = Console.ReadLine();

    if (!int.TryParse(idText, out int id))
    {
        Console.WriteLine("El ID debe ser un número entero.");
        return;
    }

    var person = personService.GetById(id);
    if (person == null)
    {
        Console.WriteLine($"No se encontró ninguna persona con ID {id}.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Editando a: {person.FirstName} {person.LastName}");
    Console.WriteLine("Si desea conservar el valor actual, deje el campo vacío y presione ENTER.\n");

    // Nombre
    Console.Write($"Nuevo nombre ({person.FirstName}): ");
    var input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        person.FirstName = input;
    }

    // Apellido
    Console.Write($"Nuevo apellido ({person.LastName}): ");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        person.LastName = input;
    }

    // Teléfono
    Console.Write($"Nuevo teléfono ({person.Phone}): ");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        person.Phone = input;
    }

    // Ciudad
    Console.Write($"Nueva ciudad ({person.City}): ");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        person.City = input;
    }

    // Saldo
    Console.Write($"Nuevo saldo ({person.Balance}): ");
    input = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(input))
    {
        if (decimal.TryParse(input, out var newBalance))
        {
            person.Balance = newBalance;
        }
        else
        {
            Console.WriteLine("El saldo ingresado no es válido. Se conservará el saldo anterior.");
        }
    }

    // Validaciones finales
    if (personService.TryUpdate(person, out string errorMessage))
    {
        logger.Log(currentUser!.Username, "EDIT_PERSON", $"ID={id}");
        Console.WriteLine("Persona actualizada correctamente.");
    }
    else
    {
        Console.WriteLine($"No se pudo actualizar la persona: {errorMessage}");
    }
}

void DeletePerson()
{
    Console.WriteLine("=== BORRAR PERSONA ===");
    Console.Write("Ingrese el ID de la persona que desea borrar: ");
    var idText = Console.ReadLine();

    if (!int.TryParse(idText, out int id))
    {
        Console.WriteLine("El ID debe ser un número entero.");
        return;
    }

    var person = personService.GetById(id);
    if (person == null)
    {
        Console.WriteLine($"No se encontró ninguna persona con ID {id}.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Se encontraron los siguientes datos:");
    Console.WriteLine($"ID: {person.Id}");
    Console.WriteLine($"Nombre: {person.FirstName} {person.LastName}");
    Console.WriteLine($"Teléfono: {person.Phone}");
    Console.WriteLine($"Ciudad: {person.City}");
    Console.WriteLine($"Saldo: {person.Balance:C2}");
    Console.WriteLine();

    Console.Write("¿Está seguro de que desea eliminar esta persona? (S/N): ");
    var confirm = (Console.ReadLine() ?? string.Empty).Trim().ToUpperInvariant();

    if (confirm == "S")
    {
        personService.Delete(id);
        logger.Log(currentUser!.Username, "DELETE_PERSON", $"ID={id}");
        Console.WriteLine("Persona eliminada correctamente.");
    }
    else
    {
        Console.WriteLine("Operación cancelada. No se realizaron cambios.");
    }
}

void ReportByCity()
{
    Console.WriteLine("=== INFORME POR CIUDAD ===\n");

    var people = personService.GetAll();
    if (people.Count == 0)
    {
        Console.WriteLine("No hay personas registradas.");
        return;
    }

    var groups = personService.GetPeopleGroupedByCity();
    decimal grandTotal = 0m;

    foreach (var cityGroup in groups)
    {
        Console.WriteLine($"Ciudad: {cityGroup.Key}");
        Console.WriteLine("ID   Nombre          Apellido        Saldo");

        decimal cityTotal = 0m;

        foreach (var p in cityGroup)
        {
            Console.WriteLine("{0,-4} {1,-14} {2,-14} {3,10:C2}",
                p.Id,
                p.FirstName,
                p.LastName,
                p.Balance);

            cityTotal += p.Balance;
        }

        Console.WriteLine("=====");
        Console.WriteLine($"Total {cityGroup.Key}: {cityTotal:C2}\n");

        grandTotal += cityTotal;
    }

    Console.WriteLine("=====");
    Console.WriteLine($"Total general: {grandTotal:C2}");

    logger.Log(currentUser!.Username, "REPORT_BY_CITY", $"TOTAL={grandTotal}");
}