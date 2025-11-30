using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Travail
{
    public partial class CommunicationRéseau : Window
    {
        public CommunicationRéseau()
        {
            InitializeComponent();
        }

        private void Envoyer_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(message))
            {
                EchangesTextBox.Text += "\nMessage vide.";
                return;
            }

            if (socketClient == null || !socketClient.Connected)
            {
                EchangesTextBox.Text += "\nAucune connexion Socket active.";
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                socketClient.Send(data);

                EchangesTextBox.Text += $"\nMessage envoyé (Socket) : {message}";
            }
            catch (Exception ex)
            {
                EchangesTextBox.Text += "\nErreur d’envoi : " + ex.Message;
            }
        }



        #region Vérification Serveur IPV4 et Ping
        private void Verifier_Click(object sender, RoutedEventArgs e)
        {
            string serveur = ServeurTextBox.Text.Trim();

            try
            {
                IPAddress ipv4 = Dns.GetHostAddresses(serveur)
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4 == null)
                {
                    EchangesTextBox.Text = "Erreur : aucune adresse IPv4 trouvée.";
                    return;
                }

                Ping ping = new Ping();
                PingReply rep = ping.Send(ipv4);

                if (rep.Status == IPStatus.Success)
                {
                    IpTextBox.Text = ipv4.ToString();
                    EchangesTextBox.Text = $"Succès : {ipv4}\nPing OK ✔";
                }
                else
                {
                    EchangesTextBox.Text = "Ping échoué ❌";
                }
            }
            catch (Exception ex)
            {
                EchangesTextBox.Text = $"Erreur : {ex.Message}";
            }
        }

        #endregion

        #region UDP Connecter et Ecouter
        private void UdpConnecter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string serveur = ServeurTextBox.Text.Trim();
                string message = MessageTextBox.Text;

                IPAddress ipv4 = Dns.GetHostAddresses(serveur)
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4 == null)
                {
                    EchangesTextBox.Text = "Adresse serveur invalide.";
                    return;
                }

                using (UdpClient udp = new UdpClient())
                {
                    IPEndPoint ep = new IPEndPoint(ipv4, 8080);
                    byte[] data = Encoding.UTF8.GetBytes(message);

                    udp.Send(data, data.Length, ep);

                    EchangesTextBox.Text = $"Message envoyé : {message}";
                }
            }
            catch (Exception ex)
            {
                EchangesTextBox.Text = $"Erreur UDP : {ex.Message}";
            }
        }


        private void UdpEcouter_Click(object sender, RoutedEventArgs e)
        {
            // Démarrer l'écoute UDP dans un thread séparé
            Thread listenerThread = new Thread(new ThreadStart(ListenForUdpMessages));
            listenerThread.IsBackground = true;  // Permet à ce thread de se fermer lorsque l'application se ferme
            listenerThread.Start();
        }

        private void ListenForUdpMessages()
        {
            try
            {
                using (UdpClient listener = new UdpClient(8080))
                {
                    IPEndPoint ep = new IPEndPoint(IPAddress.Any, 8080);

                    while (true)
                    {
                        byte[] data = listener.Receive(ref ep);
                        string msg = Encoding.UTF8.GetString(data);

                        Dispatcher.Invoke(() =>
                        {
                            EchangesTextBox.Text = $"Message reçu : {msg}";
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => EchangesTextBox.Text = $"Erreur UDP : {ex.Message}");
            }
        }

        #endregion

        #region Listener/Client Ecouter et Connecter
        private void ListenerClientEcouter_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    TcpListener server = new TcpListener(IPAddress.Any, 8000);
                    server.Start();

                    Dispatcher.Invoke(() =>
                        EchangesTextBox.Text = "Serveur en attente de connexion (port 8000)...");

                    TcpClient client = server.AcceptTcpClient();

                    NetworkStream stream = client.GetStream();
                    BinaryWriter writer = new BinaryWriter(stream);
                    BinaryReader reader = new BinaryReader(stream);

                    // Envoi au client
                    writer.Write("Connexion réussie");

                    // Réception du client
                    string msg = reader.ReadString();

                    Dispatcher.Invoke(() =>
                        EchangesTextBox.Text += $"\n{msg}");

                    client.Close();
                    server.Stop();
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => EchangesTextBox.Text = $"Erreur : {ex.Message}");
                }
            });

            t.IsBackground = true;
            t.Start();
        }


        private void ListenerClientConnecter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress ipv4 = Dns.GetHostAddresses(ServeurTextBox.Text)
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4 == null)
                {
                    EchangesTextBox.Text = "Adresse serveur invalide.";
                    return;
                }

                TcpClient client = new TcpClient(ipv4.ToString(), 8000);
                NetworkStream stream = client.GetStream();
                BinaryWriter writer = new BinaryWriter(stream);
                BinaryReader reader = new BinaryReader(stream);

                // Réception : Connexion réussie
                string msgServeur = reader.ReadString();

                // Envoi du nom machine
                string machine = Environment.MachineName;
                writer.Write($"Machine {machine} connectée");

                EchangesTextBox.Text = msgServeur;

                client.Close();
            }
            catch (Exception ex)
            {
                EchangesTextBox.Text = $"Erreur TCP : {ex.Message}";
            }
        }

        #endregion

        #region Socket Ecouter, Connecter, Deconnecter

        private Socket socketServeur;
        private Socket socketClient;
        private Thread socketThread;

        private void SocketEcouter_Click(object sender, RoutedEventArgs e)
        {
            socketThread = new Thread(() =>
            {
                try
                {
                    socketServeur = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    socketServeur.Bind(new IPEndPoint(IPAddress.Any, 9000));

                    socketServeur.Listen(1);

                    Dispatcher.Invoke(() =>
                        EchangesTextBox.Text = "Socket en écoute sur le port 9000...");

                    socketClient = socketServeur.Accept();

                    Dispatcher.Invoke(() =>
                        EchangesTextBox.Text += "\nClient connecté !");

                    byte[] buffer = new byte[1024];
                    int length = socketClient.Receive(buffer);
                    string message = Encoding.UTF8.GetString(buffer, 0, length);

                    Dispatcher.Invoke(() =>
                        EchangesTextBox.Text += $"\nMessage reçu : {message}");
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                        EchangesTextBox.Text = $"Erreur Socket : {ex.Message}");
                }
            });

            socketThread.IsBackground = true;
            socketThread.Start();
        }


        private void SocketConnecter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string serveur = ServeurTextBox.Text;

                IPAddress ipv4 = Dns.GetHostAddresses(serveur)
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (ipv4 == null)
                {
                    EchangesTextBox.Text = "Adresse serveur invalide.";
                    return;
                }

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.Connect(new IPEndPoint(ipv4, 9000));
                socketClient = client;

                EchangesTextBox.Text = "Socket connecté au serveur.";

                string msg = "Connexion Socket réussie";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                client.Send(data);
            }
            catch (Exception ex)
            {
                EchangesTextBox.Text = $"Erreur de connexion Socket : {ex.Message}";
            }
        }


        private void SocketDeconnecter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                socketClient?.Close();
                socketServeur?.Close();
                socketThread?.Abort();

                EchangesTextBox.Text = "Déconnexion Socket réussie.";
            }
            catch
            {
                EchangesTextBox.Text = "Erreur lors de la déconnexion.";
            }
        }

        #endregion
    }
    }
