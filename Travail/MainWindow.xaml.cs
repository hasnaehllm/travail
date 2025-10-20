using System;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.ComponentModel;
using Travail;

namespace Travail
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenToDoList_Click(object sender, RoutedEventArgs e)
        {
            ToDoListWindow toDoListWindow = new ToDoListWindow();
            toDoListWindow.Show();
        }

        private void OpenChronometer_Click(object sender, RoutedEventArgs e)
        {
            Window chronometreWindow = new Window
            {
                Content = new ChronometreView()  
            };

            chronometreWindow.Show();
        }


        private void SendEmailButton_Click(object sender, RoutedEventArgs e)
        {
            string userEmail = UserEmailTextBox.Text;
            string userPassword = UserPasswordBox.Password;
            string recipientEmail = RecipientEmailTextBox.Text;
            string subject = SubjectTextBox.Text;
            string body = MessageTextBox.Text;

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userPassword) || string.IsNullOrEmpty(recipientEmail) ||
                string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                ErrorMessageTextBlock.Text = "Tous les champs doivent être remplis.";
                return;
            }

            ErrorMessageTextBlock.Text = "";
            StatusTextBlock.Text = "Envoi en cours...";
            ProgressBar.Visibility = Visibility.Visible;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, args) =>
            {
                try
                {
                    var client = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(userEmail, userPassword),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(userEmail),
                        Subject = subject,
                        Body = body,
                    };
                    mailMessage.To.Add(recipientEmail);

                    client.Send(mailMessage);
                    args.Result = "Email envoyé avec succès!";
                }
                catch (SmtpException smtpEx)
                {
                    args.Result = $"Erreur SMTP : {smtpEx.Message}";
                }
                catch (Exception ex)
                {
                    args.Result = $"Erreur générale : {ex.Message}";
                }
            };

            worker.RunWorkerCompleted += (s, args) =>
            {
                StatusTextBlock.Text = "";
                ErrorMessageTextBlock.Text = args.Result.ToString();

                ProgressBar.Visibility = Visibility.Collapsed;

                MessageBox.Show(args.Result.ToString(), "Statut de l'envoi", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            worker.RunWorkerAsync();
        }
    }
}