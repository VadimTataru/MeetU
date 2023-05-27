using MeetU.model;

namespace MeetU.manager
{
    public class MeetingManager
    {
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
        }

        public List<Meeting> ReadMeetingsWithDate(DateTime date)
        {
            return meetingList.Where(m => m.StartTime.Date == date.Date).ToList();
        }

        public void UpdateMeeting(Meeting oldMeeting, Meeting newMeeting)
        {
            if (meetingList.Any(m => m != oldMeeting && m.StartTime < newMeeting.EndTime && m.EndTime > newMeeting.StartTime))
            {
                throw new Exception("Встречи не должны пересекаться!");
            }

            int index = meetingList.IndexOf(oldMeeting);
            meetingList[index] = newMeeting;
        }

        public void DeleteMeeting(Meeting meeting)
        {
            meetingList.Remove(meeting);
        }

        public async Task<bool> ExportMettingWithDate(DateTime date, string fileName)
        {
            var meetings = ReadMeetingsWithDate(date);

            if (meetings == null)
                return false;

            if (File.Exists(fileName))
                return false;

            using (StreamWriter sw = new StreamWriter(fileName, true))
            {
                Console.WriteLine();
                await sw.WriteLineAsync($"Расписание встреч на {date.ToShortDateString()}");
                foreach(var meeting in meetings)
                    await sw.WriteLineAsync(meeting.ToString());
            }

            return true;
        }
    }
}
