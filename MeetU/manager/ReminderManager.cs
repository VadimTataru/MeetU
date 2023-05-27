using MeetU.model;

namespace MeetU.manager
{
    internal class ReminderManager
    {
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
        public void Start()
        {
            while(true)
            {
                List<Meeting> meetings = meetingManager.ReadMeetingsWithDate(DateTime.Now.Date);
                foreach (Meeting meeting in meetings)
                {
                    TimeSpan timeToMeeting = meeting.StartTime - DateTime.Now;
                    if(!notifiedMeetings.Contains(meeting) && timeToMeeting <= meeting.ReminderTime)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Уведомление: Встреча \"{meeting.Title}\" скоро начнётся!");
                        notifiedMeetings.Add(meeting);
                        Console.ResetColor();
                    }
                }
                Thread.Sleep(60000);
            }
        }
    }
}
