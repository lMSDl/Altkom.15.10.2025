//Token
// CalculateFactorial -> Calculate, Factor, ial
// private int Add(int a, int b) => a + b; -> private, int, Add, (, int, a, ,, int, b, ), =>, a, +, b, ;


object GetUserById(int id)
{
    return new { Id = id, Name = "User" + id };
}

//Prompt opisowy:
// Napisz metodę, która zwraca listę użytkowników posortowaną malejąco po dacie rejestracji
List<object> GetUsersSortedByRegistrationDate(List<object> users)
{
    return users.OrderByDescending(u => ((dynamic)u).RegistrationDate).ToList();
}

//Prompt deklaratywny:
// Walidacja numeru pesel
bool IsValidPesel(string pesel)
{
    if (pesel.Length != 11 || !pesel.All(char.IsDigit))
        return false;
    int[] weights = { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 };
    int sum = 0;
    for (int i = 0; i < weights.Length; i++)
    {
        sum += weights[i] * (pesel[i] - '0');
    }
    int controlDigit = (10 - (sum % 10)) % 10;
    return controlDigit == (pesel[10] - '0');
}

//Prompt step-by-step:
// 1. Sprawdź czy numer ma 11 cyfr
// 2. Oblicz sumę kontrolną
// 3. Zwróć true/false
bool IsValidPeselStepByStep(string pesel)
{
    // 1. Sprawdź czy numer ma 11 cyfr
    if (pesel.Length != 11 || !pesel.All(char.IsDigit))
        return false;
    // 2. Oblicz sumę kontrolną
    int[] weights = { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 };
    int sum = 0;
    for (int i = 0; i < weights.Length; i++)
    {
        sum += weights[i] * (pesel[i] - '0');
    }
    int controlDigit = (10 - (sum % 10)) % 10;
    // 3. Zwróć true/false
    return controlDigit == (pesel[10] - '0');
}


bool IsBirthDateValid(string pesel, DateTime birtDate)
{
    if (pesel.Length != 11 || !pesel.All(char.IsDigit))
        return false;
    int year = int.Parse(pesel.Substring(0, 2));
    int month = int.Parse(pesel.Substring(2, 2));
    int day = int.Parse(pesel.Substring(4, 2));
    int fullYear;
    if (month >= 1 && month <= 12)
        fullYear = 1900 + year;
    else if (month >= 21 && month <= 32)
    {
        fullYear = 2000 + year;
        month -= 20;
    }
    else if (month >= 41 && month <= 52)
    {
        fullYear = 2100 + year;
        month -= 40;
    }
    else if (month >= 61 && month <= 72)
    {
        fullYear = 2200 + year;
        month -= 60;
    }
    else if (month >= 81 && month <= 92)
    {
        fullYear = 1800 + year;
        month -= 80;
    }
    else
        return false;
    try
    {
        DateTime peselDate = new DateTime(fullYear, month, day);
        return peselDate.Date == birtDate.Date;
    }
    catch
    {
        return false;
    }
}

List<object> GetUsersWithInvalidPesel(List<object> users)
{
    return users
        .Where(u => {
            var pesel = ((dynamic)u).Pesel as string;
            return pesel == null || !IsValidPesel(pesel);
        })
        .ToList();
}


