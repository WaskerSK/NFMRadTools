using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing.Presets
{
    public readonly struct RechargedStatsPreset
    {
        public Gearbox<int> Swits { get; init; }
        public Gearbox<double> Acelf { get; init; }
        public int Handbrake { get; init; }
        public double Airspeed { get; init; }
        public int Aircontrol { get; init; }
        public int Turnspeed { get; init; }
        public double Grip { get; init; }
        public double Bounce { get; init; }
        public double Simag { get; init; }
        public double Moment { get; init; }
        public double Comprad { get; init; }
        public int PushesOthers { get; init; }
        public int GetsPushed { get; init; }
        public int LiftsOthers { get; init; }
        public int GetsLifted { get; init; }
        public long Powerloss { get; init; }
        public int Flipy { get; init; }
        public int Msquash { get; init; }
        public int Clrad { get; init; }
        public double DamageMultiplier { get; init; }
        public int Maxmag { get; init; }
        public double Dishandle { get; init; }
        public double Outdam { get; init; }
    }
}
