using System;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.ComponentModel;

namespace Travail
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

            // Masquer les anciens messages d'erreur
            ErrorMessageTextBlock.Text = "";
            StatusTextBlock.Text = "Envoi en cours...";
            ProgressBar.Visibility = Visibility.Visible;

            // Créer un BackgroundWorker pour envoyer l'email en arrière-plan
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

                    // Envoi de l'email
                    client.Send(mailMessage);
                    args.Result = "Email envoyé avec succès!";
                }
                catch (Exception ex)
                {
                    args.Result = $"Erreur : {ex.Message}"; 
                }
            };

            worker.RunWorkerCompleted += (s, args) =>
            {
                // Mettre à jour l'interface avec le message de succès ou d'erreur
                StatusTextBlock.Text = "";
                ErrorMessageTextBlock.Text = args.Result.ToString();

                ProgressBar.Visibility = Visibility.Collapsed;

                MessageBox.Show(args.Result.ToString(), "Statut de l'envoi", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            worker.RunWorkerAsync();
        }
    }
}
