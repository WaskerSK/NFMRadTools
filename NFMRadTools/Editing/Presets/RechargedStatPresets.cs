using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing.Presets
{
    public static class RechargedStatPresets
    {
        public static RechargedStatsPreset Default =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 40, Gear2 = 165, Gear3 = 280},
                Acelf = new Gearbox<double>() { Gear1 = 10.0, Gear2 = 5.0, Gear3 = 3.0},
                Handbrake = 6,
                Airspeed = 1.0,
                Aircontrol = 60,
                Turnspeed = 5,
                Grip = 20.0,
                Bounce = 1.0,
                Simag = 0.9,
                Moment = 1.3,
                Comprad = 0.5,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 0,
                GetsLifted = 0,
                Powerloss = 2500000,
                Flipy = -50,
                Msquash = 7,
                Clrad = 3000,
                DamageMultiplier = 0.8,
                Maxmag = 7000,
                Dishandle = 0.6,
                Outdam = 0.6
            };


    }
}
