//Token
// CalculateFactorial -> Calculate, Factor, ial
// private int Add(int a, int b) => a + b; -> private, int, Add, (, int, a, ,, int, b, ), =>, a, +, b, ;


object GetUserById(int id)
{
    return new { Id = id, Name = "User" + id };
}