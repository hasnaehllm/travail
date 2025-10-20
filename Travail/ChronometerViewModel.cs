using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace Travail
{
    public class ChronometerViewModel : INotifyPropertyChanged
    {
        private readonly ChronometerModel _model;
        private readonly DispatcherTimer _timer;
        private bool _estDemarre;

        public event PropertyChangedEventHandler PropertyChanged;
        public IEnumerable<double> MarquesHeures => Enumerable.Range(0, 12).Select(i => i * 30.0);

        public ChronometerViewModel()
        {
            _model = new ChronometerModel();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            DemarrerCommand = new RelayCommand(Demarrer, PeutDemarrer);
            ArreterCommand = new RelayCommand(Arreter, PeutArreter);
            ReinitialiserCommand = new RelayCommand(Reinitialiser, PeutReinitialiser);
        }

        // Propriétés pour les angles des aiguilles
        public double AngleSecondes => _model.TempsEcoule.Seconds * 6; // 360° / 60 secondes = 6° par seconde
        public double AngleMinutes => (_model.TempsEcoule.Minutes + _model.TempsEcoule.Seconds / 60.0) * 6; // 6° par minute
        public double AngleHeures => ((_model.TempsEcoule.Hours % 12) + _model.TempsEcoule.Minutes / 60.0) * 30; // 30° par heure

        // Propriété pour afficher le temps formaté
        public string TempsAffiche => _model.TempsEcoule.ToString(@"hh\:mm\:ss");

        // Commands
        public ICommand DemarrerCommand { get; }
        public ICommand ArreterCommand { get; }
        public ICommand ReinitialiserCommand { get; }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _model.Incrementer();
            OnPropertyChanged(nameof(AngleSecondes));
            OnPropertyChanged(nameof(AngleMinutes));
            OnPropertyChanged(nameof(AngleHeures));
            OnPropertyChanged(nameof(TempsAffiche));
        }

        private void Demarrer()
        {
            _estDemarre = true;
            _timer.Start();
            RafraichirCommands();
        }

        private bool PeutDemarrer()
        {
            return !_estDemarre;
        }

        private void Arreter()
        {
            _estDemarre = false;
            _timer.Stop();
            RafraichirCommands();
        }

        private bool PeutArreter()
        {
            return _estDemarre;
        }

        private void Reinitialiser()
        {
            _model.Reinitialiser();
            OnPropertyChanged(nameof(AngleSecondes));
            OnPropertyChanged(nameof(AngleMinutes));
            OnPropertyChanged(nameof(AngleHeures));
            OnPropertyChanged(nameof(TempsAffiche));
            RafraichirCommands();
        }

        private bool PeutReinitialiser()
        {
            return !_estDemarre && _model.TempsEcoule.TotalSeconds > 0;
        }

        private void RafraichirCommands()
        {
            (DemarrerCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ArreterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ReinitialiserCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
