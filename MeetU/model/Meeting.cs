namespace MeetU.model
{
    public class Meeting
    {
        public string Title;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan ReminderTime;

        public Meeting(
            string title,
            DateTime startTime,
            DateTime endTime,
            TimeSpan reminderTime
        )
        {
            this.Title = title;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.ReminderTime = reminderTime;
        }

        public override string ToString()
        {
            return $"{StartTime.ToShortTimeString()} - {EndTime.ToShortTimeString()}. {Title}" ;
        }
    }
}
