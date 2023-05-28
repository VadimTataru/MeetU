using MeetU.model;

namespace MeetU.manager
{
    public class MeetingManager
    {
        public Action<string, ConsoleColor> OnLogging;

        private List<Meeting> meetingList;
        private int untitledMeetingCount = 1;
 
        private static MeetingManager? instance;

        private MeetingManager()
        {
            meetingList = new List<Meeting>();
        }

        public static MeetingManager GetInstance()
        {
            if(instance == null)
                instance = new MeetingManager();
            return instance;
        }

        /// <summary>
        /// Создать встречу
        /// </summary>
        /// <param name="meeting"></param>
        /// <exception cref="Exception"></exception>
        public void CreateMeeting(Meeting meeting)
        {
            if(meetingList.Any(m => m.StartTime < meeting.EndTime && m.EndTime > meeting.StartTime))
            {
                throw new Exception("Встречи не должны пересекаться!");
            }
            if (meeting.Title == null || meeting.Title == String.Empty)
            {
                meeting.Title = $"Встреча без названия #{untitledMeetingCount}";
                untitledMeetingCount++;
            }                
            meetingList.Add(meeting);
            OnLogging?.Invoke("Встреча успешно добавлена!", ConsoleColor.Green);
        }

        /// <summary>
        /// Получить встречу по дате
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<Meeting>? ReadMeetingsWithDate(DateTime date)
        {
            var meetings = meetingList.Where(m => m.StartTime.Date == date.Date).OrderBy(m => m.StartTime).ToList();
            if (!meetings.Any())
                return null;
            return meetings;
        }

        /// <summary>
        /// Получить встречу по дате и времени её начала
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public Meeting? ReadMeetingWithStartDate(DateTime date)
        {
            return meetingList.FirstOrDefault(m => m.StartTime == date);
        }

        /// <summary>
        /// Изменить данные встречи
        /// </summary>
        /// <param name="oldMeeting"></param>
        /// <param name="newMeeting"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateMeeting(Meeting oldMeeting, Meeting newMeeting)
        {
            if (meetingList.Any(m => m != oldMeeting && m.StartTime < newMeeting.EndTime && m.EndTime > newMeeting.StartTime))
            {
                throw new Exception("Встречи не должны пересекаться!");
            }

            int index = meetingList.IndexOf(oldMeeting);
            meetingList[index] = newMeeting;
            OnLogging?.Invoke("Встреча успешно изменена!", ConsoleColor.Green);
        }

        /// <summary>
        /// Удалить встречу
        /// </summary>
        /// <param name="meeting"></param>
        public void DeleteMeeting(Meeting meeting)
        {
            meetingList.Remove(meeting);
            OnLogging?.Invoke("Встреча успешно удалена!", ConsoleColor.Green);
        }

        /// <summary>
        /// Экспорт
        /// </summary>
        /// <param name="date"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ExportMettingWithDate(DateTime date, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "exported_meetings.txt";
            else
            {
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                    fileName += ".txt";
            }

            var meetings = ReadMeetingsWithDate(date);

            if (meetings == null)
                throw new Exception("Список встреч пуст! Файл не будет создан.");

            if (File.Exists(fileName))
                throw new Exception("Файл с таким именем уже существует!");

            using (StreamWriter sw = new StreamWriter(fileName, true))
            {
                Console.WriteLine();
                await sw.WriteLineAsync($"Расписание встреч на {date.ToShortDateString()}");
                foreach(var meeting in meetings)
                    await sw.WriteLineAsync(meeting.ToString());
            }
            OnLogging?.Invoke($"Список встреч успешно записан в файл {fileName}", ConsoleColor.Green);
        }
    }
}
