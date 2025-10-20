using System;

namespace Travail
{
    public class ChronometerModel
    {
        public TimeSpan TempsEcoule { get; set; }

        public ChronometerModel()
        {
            TempsEcoule = TimeSpan.Zero;
        }

        public void Incrementer()
        {
            TempsEcoule = TempsEcoule.Add(TimeSpan.FromSeconds(1));
        }

        public void Reinitialiser()
        {
            TempsEcoule = TimeSpan.Zero;
        }
    }
}
