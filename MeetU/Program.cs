using MeetU.manager;
using MeetU.model;
using MeetU.util;
using System.Globalization;

var meetingManager = MeetingManager.GetInstance();
meetingManager.OnLogging += OnLogging;
var reminder = ReminderManager.GetInstance();
reminder.OnReminderLogging += OnReminderLogging;
Thread reminderThread = new Thread(new ThreadStart(reminder.Start));
reminderThread.IsBackground = true;
reminderThread.Start();

while(true)
{
    Console.Clear();
    Console.WriteLine($"{(int)Command.ReadWithDate}. Посмотреть расписание на день");
    Console.WriteLine($"{(int)Command.Add}. Назначить встречу");
    Console.WriteLine($"{(int)Command.Update}. Изменить встречу");
    Console.WriteLine($"{(int)Command.Delete}. Отменить встречу");
    Console.WriteLine($"{(int)Command.Export}. Экспортировать список встреч");
    Console.WriteLine($"{(int)Command.Exit}. Выход");

    Console.WriteLine("Введите номер команды: ");

    int command;
    if (!int.TryParse(Console.ReadLine(), out command))
    {
        Console.WriteLine("Ввод не корректен");
        continue;
    }

    switch ((Command)command)
    {
        case Command.ReadWithDate:
            {
                Console.WriteLine("Введите дату в формате 'dd.MM.yyyy' :");
                if(!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    Console.WriteLine("Ввод не корректен!");
                    WaitForUserRead();
                    break;
                }

                try
                {
                    List<Meeting>? meetingList = meetingManager.ReadMeetingsWithDate(date);
                    if (meetingList == null)
                    {
                        throw new Exception("Список встреч на выбранную дату пуст!");
                    }
                    else
                    {
                        Console.WriteLine($"Расписание встреч на {date.ToShortDateString()}");
                        foreach (Meeting meeting in meetingList)
                            Console.WriteLine(meeting.ToString());
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }

                WaitForUserRead();
                break;
            }

        case Command.Add:
            {                
                try
                {
                    Meeting? meeting = TryCreateMeetingWithConsole();
                    if (meeting == null)
                        throw new Exception("Не удалось назначить встречу!");
                    meetingManager.CreateMeeting(meeting);
                } catch(Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }

                WaitForUserRead();
                break;
            }

        case Command.Update:
            {
                Console.WriteLine("Введите дату и время начала старой встречи (в формате dd.MM.yyyy HH:mm):");
                if(!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime oldStart)) {
                    Console.WriteLine("Ввод не корректен!");
                    WaitForUserRead();
                    break;
                }
                
                try
                {
                    Meeting? oldMeeting = meetingManager.ReadMeetingWithStartDate(oldStart);
                    if(oldMeeting == null)
                        throw new Exception("Встреча не найдена!");

                    Console.WriteLine("Введите данные для изменения встречи: ");
                    Meeting? newMeeting = TryCreateMeetingWithConsole();
                    if (newMeeting == null)
                        throw new Exception("Не удалось назначить встречу!");

                    meetingManager.UpdateMeeting(oldMeeting, newMeeting);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
                WaitForUserRead();
                break;
            }

        case Command.Delete:
            {
                Console.WriteLine("Введите дату и время начала встречи в формате 'dd.mm.yyyy HH:mm' :");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime start))
                {
                    Console.WriteLine("Ввод не корректен!");
                    break;
                }
                
                try
                {
                    Meeting? meeting = meetingManager.ReadMeetingWithStartDate(start);
                    if (meeting == null)
                        throw new Exception("Встреча не найдена!");
                    meetingManager.DeleteMeeting(meeting);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
                WaitForUserRead();
                break;
            }

        case Command.Export:
            {
                Console.WriteLine("Введите дату в формате 'dd.MM.yyyy' :");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    Console.WriteLine("Ввод не корректен!");
                    WaitForUserRead();
                    break;
                }

                Console.WriteLine("Введите имя файла");
                string fileName = Console.ReadLine();

                try
                {
                    await meetingManager.ExportMettingWithDate(date, fileName);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
                WaitForUserRead();
                break;
            }

        case Command.Exit:
            {
                meetingManager.OnLogging -= OnLogging;
                reminder.OnReminderLogging -= OnReminderLogging;
                return;
            }
        default:
            {
                Console.WriteLine("Такая команда отсутствует :( \nПопробуйте ввести одну из доступных (1-6)");
                WaitForUserRead();
                break;
            }
    }
}

Meeting? TryCreateMeetingWithConsole()
{
    try
    {
        Console.WriteLine("Введите дату встречи в формате 'dd.mm.yyyy' :");
        DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime date);
        if(date < DateTime.Now.Date)
            throw new Exception("Ввод не корректен! Встречи можно планировать только на будущее.");

        Console.WriteLine("Введите время начала встречи в формате 'HH:mm' :");
        TimeSpan.TryParseExact(Console.ReadLine(), @"hh\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan startTime);

        Console.WriteLine("Введите дату и время конца встречи в формате 'HH:mm' :");
        TimeSpan.TryParseExact(Console.ReadLine(), @"hh\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan endTime);

        var start = date.Add(startTime);
        var end = date.Add(endTime);

        if (start < DateTime.Now || start > end)
            throw new Exception("Ввод не корректен! Возможно не соблюдено одно из условий: \nВстречи можно планировать только на будущее. \nКонец встречи должен быть не раньше её старта!");

        Console.WriteLine("Введите название встречи:");
        string title = Console.ReadLine();

        Console.WriteLine("Введите время до начала встречи для уведомления в формате 'HH:mm' :");
        TimeSpan.TryParseExact(Console.ReadLine(), @"hh\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan reminderTime);

        if (DateTime.Now > start.Subtract(reminderTime))
            throw new Exception("Уведомить можно только в будущем!");

        return new Meeting(title, start, end, reminderTime);
    } catch(Exception ex)
    {
        Console.WriteLine($"Ошбика: {ex.Message}");
        return null;
    }
}

void OnLogging(string message, ConsoleColor consoleColor)
{
    Console.ForegroundColor = consoleColor;
    Console.WriteLine(message);
    Console.ResetColor();
}

void OnReminderLogging(string message, ConsoleColor consoleColor)
{
    Console.Beep();
    Console.ForegroundColor = consoleColor;
    Console.WriteLine(message);
    Console.ResetColor();
}

void WaitForUserRead()
{
    Console.WriteLine("Нажмите любую кнопку, чтобы продолжить");
    Console.ReadKey();
}
