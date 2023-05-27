namespace MeetU.util
{
    internal class Printer
    {
        public void PrintMessage(string? message)
        {
            if (message != null)
                Console.WriteLine(message);
            Console.WriteLine("Нажмите любую кнопку, чтобы продолжить");
            Console.ReadKey();
        }
    }
}
