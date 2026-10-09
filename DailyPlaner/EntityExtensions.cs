using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace DailyPlaner
{
    public partial class Task : INotifyPropertyChanged
    {
        public bool IsCompleted
        {
            get { return Status == "Выполнена" || Status == "Completed"; }
            set { Status = value ? "Выполнена" : "Ожидает"; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class Event : INotifyPropertyChanged
    {
        public bool IsCompleted
        {
            get { return Status == "Завершено" || Status == "Completed"; }
            set { Status = value ? "Завершено" : "Запланировано"; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class Note
    {
        public DateTime CreatedDate
        {
            get { return CreatedAt; }
            set { CreatedAt = value; }
        }
    }

    public partial class Reminder
    {
        public DateTime ReminderDate
        {
            get { return ReminderTime; }
            set { ReminderTime = value; }
        }

        public bool IsShown
        {
            get { return IsActive; }
            set { IsActive = value; }
        }
    }
}
