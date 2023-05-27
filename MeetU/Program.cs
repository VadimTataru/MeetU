using MeetU.manager;
using MeetU.model;
using MeetU.util;
using System.Globalization;

var meetingManager = MeetingManager.GetInstance();
var reminder = ReminderManager.GetInstance();
Thread reminderThread = new Thread(new ThreadStart(reminder.Start));
reminderThread.IsBackground = true;
var printer = new Printer();

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
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                List<Meeting> meetingList = meetingManager.ReadMeetingsWithDate(date);
                if(meetingList.Count == 0)
                {
                    printer.PrintMessage("На выбранную дату встреч пока не запланировано");

                } else
                {
                    Console.WriteLine($"Расписание встреч на {date.ToShortDateString()}");
                    foreach (Meeting meeting in meetingList)
                        Console.WriteLine(meeting.ToString());
                    printer.PrintMessage(null);
                }

                break;
            }

        case Command.Add:
            {
                Console.WriteLine("Введите дату встречи в формате 'dd.mm.yyyy' :");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Console.WriteLine("Введите время начала встречи в формате 'HH:mm' :");
                if (!TimeSpan.TryParseExact(Console.ReadLine(), @"hh\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan startTime))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Console.WriteLine("Введите дату и время конца встречи в формате 'HH:mm' :");
                if (!TimeSpan.TryParseExact(Console.ReadLine(), @"hh\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan endTime))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }
                var start = date.Add(startTime);
                var end = date.Add(endTime);

                if (start < DateTime.Now || start > end)
                {
                    printer.PrintMessage("Ввод не корректен! Возможно не соблюдено одно из условий: \nВстречи можно планировать только на будущее. \nКонец встречи должен быть не раньше её старта!");
                    break;
                }

                Console.WriteLine("Введите название встречи:");
                string title = Console.ReadLine();

                Console.WriteLine("Введите время напоминания в формате 'HH:mm' :");
                if (!TimeSpan.TryParseExact(Console.ReadLine(), @"hh\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan reminderTime))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Meeting meeting = new Meeting(title, start, end, reminderTime);

                try
                {
                    meetingManager.CreateMeeting(meeting);
                    printer.PrintMessage("Встреча успешно добавлена!");

                } catch(Exception ex)
                {
                    printer.PrintMessage($"Ошибка: {ex.Message}");
                }

                break;
            }

        case Command.Update:
            {
                Console.WriteLine("Введите дату и время начала старой встречи (в формате dd.MM.yyyy HH:mm):");
                if(!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime oldStart)) {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Console.WriteLine("Введите дату и время окончания старой встречи (в формате dd.MM.yyyy HH:mm):");
                if(!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime oldEnd))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Console.WriteLine("Введите дату и время начала новой встречи (в формате dd.MM.yyyy HH:mm):");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime newStart))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Console.WriteLine("Введите дату и время окончания новой встречи (в формате dd.MM.yyyy HH:mm):");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime newEnd))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Console.WriteLine("Введите название новой встречи:");
                string newTitle = Console.ReadLine();

                Console.WriteLine("Введите время напоминания новой встречи (в формате HH:mm):");
                if (!TimeSpan.TryParseExact(Console.ReadLine(), @"hh\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan newReminderTime))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Meeting? oldMeeting = meetingManager.ReadMeetingsWithDate(oldStart.Date).FirstOrDefault(m => m.StartTime == oldStart && m.EndTime == oldEnd);
                if (oldMeeting == null)
                {
                    printer.PrintMessage("Встреча не найдена");
                    break;
                }

                Meeting newMeeting = new Meeting(newTitle, newStart, newEnd, newReminderTime);

                try
                {
                    meetingManager.UpdateMeeting(oldMeeting, newMeeting);
                    printer.PrintMessage("Встреча успешно изменена!");
                }
                catch (Exception ex)
                {
                    printer.PrintMessage($"Ошибка: {ex.Message}");
                }
                break;
            }

        case Command.Delete:
            {
                Console.WriteLine("Введите дату и время начала встречи в формате 'dd.mm.yyyy HH:mm' :");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime start))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Console.WriteLine("Введите дату и время конца встречи в формате 'dd.mm.yyyy HH:mm' :");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime end))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Meeting? meeting = meetingManager.ReadMeetingsWithDate(start.Date).FirstOrDefault(m => m.StartTime == start && m.EndTime == end);
                if (meeting == null)
                {
                    printer.PrintMessage("Встреча не найдена");
                    break;
                }
                meetingManager.DeleteMeeting(meeting);
                printer.PrintMessage("Встреча успешно удалена");
                break;
            }

        case Command.Export:
            {
                Console.WriteLine("Введите дату в формате 'dd.MM.yyyy' :");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    printer.PrintMessage("Ввод не корректен!");
                    break;
                }

                Console.WriteLine("Введите имя файла");
                string fileName = Console.ReadLine();
                if (string.IsNullOrEmpty(fileName))
                    fileName = "exported_meetings.txt";

                if(!await meetingManager.ExportMettingWithDate(date, fileName))
                {
                    printer.PrintMessage($"Ошибка при записи! Проверьте есть ли у Вас встречи на выбранный день или не существует ли файл с таким же названием.");
                    break;
                }
                printer.PrintMessage($"Список встреч успешно записан в файл {fileName}");
                break;
            }

        case Command.Exit:
            {
                return;
            }
        default:
            {
                printer.PrintMessage("Команда отсутствует :(");
                break;
            }
    }
}
