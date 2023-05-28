using MeetU.model;

namespace MeetU.manager
{
    internal class ReminderManager
    {
        public Action<string, ConsoleColor> OnReminderLogging;

        private readonly MeetingManager meetingManager = MeetingManager.GetInstance();

        private static ReminderManager? instance;
        private List<Meeting> notifiedMeetings = new List<Meeting>();
        private ReminderManager(){}
        public static ReminderManager GetInstance()
        {
            if (instance == null)
                instance = new ReminderManager();
            return instance;
        }

        /// <summary>
        /// Запуск бесконечного цикла проверки уведомлений
        /// </summary>
        public void Start()
        {
            while(true)
            {
                List<Meeting>? meetings = meetingManager.ReadMeetingsWithDate(DateTime.Now.Date);
                if (meetings == null)
                    continue;
                foreach (Meeting meeting in meetings)
                {
                    TimeSpan timeToMeeting = meeting.StartTime - DateTime.Now;
                    if(!notifiedMeetings.Contains(meeting) && timeToMeeting <= meeting.ReminderTime)
                    {
                        OnReminderLogging?.Invoke($"Уведомление: Встреча \"{meeting.Title}\" скоро начнётся!", ConsoleColor.Green);
                        notifiedMeetings.Add(meeting);
                    }
                }
                Thread.Sleep(30000);
            }
        }
    }
}
