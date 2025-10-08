using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Travail
{
    public partial class ToDoListWindow : Window
    {
        public ObservableCollection<TaskItem> Tasks { get; set; }

        public ToDoListWindow()
        {
            InitializeComponent();
            Tasks = new ObservableCollection<TaskItem>();  
            this.DataContext = this; 
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTextBox.Text;  
            if (!string.IsNullOrEmpty(title)) 
            {
                Tasks.Add(new TaskItem { Title = title, IsDone = false });  
                TaskTextBox.Clear();  
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TasksListBox.SelectedItem != null) 
            {
                Tasks.Remove((TaskItem)TasksListBox.SelectedItem);  
            }
        }
    }

    public class TaskItem
    {
        public string Title { get; set; } 
        public bool IsDone { get; set; }  
    }

    public class IsDoneToColorConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isDone = (bool)value;

            if (isDone)
            {
                return Brushes.Gray;  
            }

            return Brushes.Black;  
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;  
        }
    }

    public class IsDoneToTextDecorationConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isDone = (bool)value;

            if (isDone)
            {
                return TextDecorations.Strikethrough;  
            }

            return null;  
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;  
        }
    }
}
