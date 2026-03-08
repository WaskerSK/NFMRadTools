using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing.Presets
{
    public static class RechargedStatPresets
    {
        public static RechargedStatsPreset Tornado_Shark =>
    new RechargedStatsPreset()
    {
        Swits = new Gearbox<int>() { Gear1 = 50, Gear2 = 185, Gear3 = 282 },
        Acelf = new Gearbox<double>() { Gear1 = 11.0, Gear2 = 5.0, Gear3 = 3.0 },
        Handbrake = 7,
        Airspeed = 1.0,
        Aircontrol = 70,
        Turnspeed = 6,
        Grip = 20.0,
        Bounce = 1.2,
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
        Clrad = 3300,
        DamageMultiplier = 0.75,
        Maxmag = 7600,
        Dishandle = 0.65,
        Outdam = 0.68
    };

        public static RechargedStatsPreset Formula_7 =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 100, Gear2 = 200, Gear3 = 310 },
                Acelf = new Gearbox<double>() { Gear1 = 14.0, Gear2 = 7.0, Gear3 = 5.0 },
                Handbrake = 10,
                Airspeed = 1.2,
                Aircontrol = 30,
                Turnspeed = 9,
                Grip = 27.0,
                Bounce = 1.05,
                Simag = 0.85,
                Moment = 0.75,
                Comprad = 0.4,
                PushesOthers = 2,
                GetsPushed = 3,
                LiftsOthers = 30,
                GetsLifted = 0,
                Powerloss = 2500000,
                Flipy = -60,
                Msquash = 4,
                Clrad = 1700,
                DamageMultiplier = 0.8,
                Maxmag = 4200,
                Dishandle = 0.6,
                Outdam = 0.35
            };

        public static RechargedStatsPreset Wow_Caninaro =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 60, Gear2 = 180, Gear3 = 275 },
                Acelf = new Gearbox<double>() { Gear1 = 10.0, Gear2 = 5.0, Gear3 = 3.5 },
                Handbrake = 7,
                Airspeed = 0.95,
                Aircontrol = 40,
                Turnspeed = 5,
                Grip = 18.0,
                Bounce = 1.3,
                Simag = 1.05,
                Moment = 1.4,
                Comprad = 0.8,
                PushesOthers = 3,
                GetsPushed = 2,
                LiftsOthers = 0,
                GetsLifted = 15,
                Powerloss = 3500000,
                Flipy = -92,
                Msquash = 7,
                Clrad = 4700,
                DamageMultiplier = 0.45,
                Maxmag = 7200,
                Dishandle = 0.55,
                Outdam = 0.8
            };

        public static RechargedStatsPreset La_Vita_Crab =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 76, Gear2 = 195, Gear3 = 298 },
                Acelf = new Gearbox<double>() { Gear1 = 11.0, Gear2 = 6.0, Gear3 = 3.5 },
                Handbrake = 15,
                Airspeed = 1.0,
                Aircontrol = 40,
                Turnspeed = 7,
                Grip = 22.0,
                Bounce = 1.15,
                Simag = 0.9,
                Moment = 1.2,
                Comprad = 0.5,
                PushesOthers = 3,
                GetsPushed = 2,
                LiftsOthers = 20,
                GetsLifted = 0,
                Powerloss = 2500000,
                Flipy = -44,
                Msquash = 2,
                Clrad = 3000,
                DamageMultiplier = 0.8,
                Maxmag = 6000,
                Dishandle = 0.77,
                Outdam = 0.5
            };

        public static RechargedStatsPreset Nimi =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 70, Gear2 = 170, Gear3 = 275 },
                Acelf = new Gearbox<double>() { Gear1 = 10.0, Gear2 = 5.0, Gear3 = 3.5 },
                Handbrake = 12,
                Airspeed = 2.2,
                Aircontrol = 30,
                Turnspeed = 8,
                Grip = 19.0,
                Bounce = 1.3,
                Simag = 0.85,
                Moment = 1.1,
                Comprad = 0.4,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 0,
                GetsLifted = 0,
                Powerloss = 4000000,
                Flipy = -60,
                Msquash = 8,
                Clrad = 2000,
                DamageMultiplier = 0.42,
                Maxmag = 6000,
                Dishandle = 0.62,
                Outdam = 0.42
            };

        public static RechargedStatsPreset MAX_Revenge =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 70, Gear2 = 202, Gear3 = 293 },
                Acelf = new Gearbox<double>() { Gear1 = 12.0, Gear2 = 6.0, Gear3 = 3.0 },
                Handbrake = 8,
                Airspeed = 1.0,
                Aircontrol = 50,
                Turnspeed = 7,
                Grip = 20.0,
                Bounce = 1.2,
                Simag = 0.9,
                Moment = 1.38,
                Comprad = 0.5,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 30,
                GetsLifted = 0,
                Powerloss = 2500000,
                Flipy = -57,
                Msquash = 4,
                Clrad = 4500,
                DamageMultiplier = 0.7,
                Maxmag = 15000,
                Dishandle = 0.9,
                Outdam = 0.76
            };

        public static RechargedStatsPreset Lead_Oxide =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 60, Gear2 = 170, Gear3 = 289 },
                Acelf = new Gearbox<double>() { Gear1 = 7.0, Gear2 = 9.0, Gear3 = 4.0 },
                Handbrake = 9,
                Airspeed = 0.9,
                Aircontrol = 40,
                Turnspeed = 5,
                Grip = 25.0,
                Bounce = 1.15,
                Simag = 1.05,
                Moment = 1.43,
                Comprad = 0.5,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 0,
                GetsLifted = 0,
                Powerloss = 3200000,
                Flipy = -54,
                Msquash = 6,
                Clrad = 3500,
                DamageMultiplier = 0.72,
                Maxmag = 17200,
                Dishandle = 0.6,
                Outdam = 0.82
            };

        public static RechargedStatsPreset Kool_Kat =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 70, Gear2 = 206, Gear3 = 291 },
                Acelf = new Gearbox<double>() { Gear1 = 11.0, Gear2 = 5.0, Gear3 = 3.0 },
                Handbrake = 10,
                Airspeed = 0.8,
                Aircontrol = 90,
                Turnspeed = 5,
                Grip = 20.0,
                Bounce = 1.1,
                Simag = 0.9,
                Moment = 1.48,
                Comprad = 0.5,
                PushesOthers = 4,
                GetsPushed = 1,
                LiftsOthers = 0,
                GetsLifted = 0,
                Powerloss = 3200000,
                Flipy = -60,
                Msquash = 4,
                Clrad = 5000,
                DamageMultiplier = 0.6,
                Maxmag = 17000,
                Dishandle = 0.72,
                Outdam = 0.76
            };

        public static RechargedStatsPreset Drifter_X =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 90, Gear2 = 210, Gear3 = 295 },
                Acelf = new Gearbox<double>() { Gear1 = 12.0, Gear2 = 7.0, Gear3 = 4.0 },
                Handbrake = 5,
                Airspeed = 1.0,
                Aircontrol = 40,
                Turnspeed = 9,
                Grip = 19.0,
                Bounce = 1.2,
                Simag = 1.0,
                Moment = 1.35,
                Comprad = 0.5,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 20,
                GetsLifted = 0,
                Powerloss = 2750000,
                Flipy = -77,
                Msquash = 3,
                Clrad = 10000,
                DamageMultiplier = 0.58,
                Maxmag = 18000,
                Dishandle = 0.45,
                Outdam = 0.72
            };

        public static RechargedStatsPreset Sword_of_Justice =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 90, Gear2 = 190, Gear3 = 276 },
                Acelf = new Gearbox<double>() { Gear1 = 12.0, Gear2 = 7.0, Gear3 = 3.5 },
                Handbrake = 7,
                Airspeed = 0.9,
                Aircontrol = 50,
                Turnspeed = 7,
                Grip = 24.0,
                Bounce = 1.1,
                Simag = 1.05,
                Moment = 1.7,
                Comprad = 0.8,
                PushesOthers = 2,
                GetsPushed = 1,
                LiftsOthers = 0,
                GetsLifted = 0,
                Powerloss = 5500000,
                Flipy = -57,
                Msquash = 8,
                Clrad = 15000,
                DamageMultiplier = 0.41,
                Maxmag = 11000,
                Dishandle = 0.8,
                Outdam = 0.62
            };

        public static RechargedStatsPreset High_Rider =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 70, Gear2 = 200, Gear3 = 295 },
                Acelf = new Gearbox<double>() { Gear1 = 11.5, Gear2 = 6.5, Gear3 = 3.5 },
                Handbrake = 8,
                Airspeed = 1.15,
                Aircontrol = 75,
                Turnspeed = 7,
                Grip = 22.5,
                Bounce = 1.15,
                Simag = 0.9,
                Moment = 1.42,
                Comprad = 0.5,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 0,
                GetsLifted = 0,
                Powerloss = 2750000,
                Flipy = -82,
                Msquash = 4,
                Clrad = 4000,
                DamageMultiplier = 0.67,
                Maxmag = 19000,
                Dishandle = 0.95,
                Outdam = 0.79
            };

        public static RechargedStatsPreset EL_KING =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 50, Gear2 = 160, Gear3 = 270 },
                Acelf = new Gearbox<double>() { Gear1 = 9.0, Gear2 = 5.0, Gear3 = 3.0 },
                Handbrake = 10,
                Airspeed = 0.8,
                Aircontrol = 10,
                Turnspeed = 4,
                Grip = 25.0,
                Bounce = 0.8,
                Simag = 1.1,
                Moment = 2.0,
                Comprad = 1.5,
                PushesOthers = 4,
                GetsPushed = 1,
                LiftsOthers = 0,
                GetsLifted = 0,
                Powerloss = 4500000,
                Flipy = -85,
                Msquash = 10,
                Clrad = 7000,
                DamageMultiplier = 0.45,
                Maxmag = 10700,
                Dishandle = 0.4,
                Outdam = 0.95
            };

        public static RechargedStatsPreset Mighty_Eight =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 90, Gear2 = 200, Gear3 = 305 },
                Acelf = new Gearbox<double>() { Gear1 = 13.0, Gear2 = 7.0, Gear3 = 4.5 },
                Handbrake = 8,
                Airspeed = 1.0,
                Aircontrol = 50,
                Turnspeed = 6,
                Grip = 30.0,
                Bounce = 1.05,
                Simag = 0.9,
                Moment = 1.26,
                Comprad = 0.5,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 10,
                GetsLifted = 0,
                Powerloss = 3500000,
                Flipy = -28,
                Msquash = 3,
                Clrad = 10000,
                DamageMultiplier = 0.61,
                Maxmag = 13000,
                Dishandle = 0.87,
                Outdam = 0.77
            };

        public static RechargedStatsPreset M_A_S_H_E_E_N =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 50, Gear2 = 130, Gear3 = 210 },
                Acelf = new Gearbox<double>() { Gear1 = 7.5, Gear2 = 3.5, Gear3 = 3.0 },
                Handbrake = 12,
                Airspeed = 0.3,
                Aircontrol = 0,
                Turnspeed = 5,
                Grip = 27.0,
                Bounce = 0.8,
                Simag = 1.3,
                Moment = 3.0,
                Comprad = 0.8,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 0,
                GetsLifted = 0,
                Powerloss = 16700000,
                Flipy = -100,
                Msquash = 20,
                Clrad = 15000,
                DamageMultiplier = 0.25,
                Maxmag = 45000,
                Dishandle = 0.42,
                Outdam = 1.0
            };

        public static RechargedStatsPreset Radical_One =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 80, Gear2 = 200, Gear3 = 300 },
                Acelf = new Gearbox<double>() { Gear1 = 11.0, Gear2 = 7.5, Gear3 = 4.0 },
                Handbrake = 7,
                Airspeed = 1.3,
                Aircontrol = 100,
                Turnspeed = 7,
                Grip = 25.0,
                Bounce = 1.1,
                Simag = 0.9,
                Moment = 1.5,
                Comprad = 0.5,
                PushesOthers = 2,
                GetsPushed = 2,
                LiftsOthers = 30,
                GetsLifted = 0,
                Powerloss = 3000000,
                Flipy = -63,
                Msquash = 3,
                Clrad = 5500,
                DamageMultiplier = 0.38,
                Maxmag = 5800,
                Dishandle = 1.0,
                Outdam = 0.85
            };

        public static RechargedStatsPreset DR_Monstaa =>
            new RechargedStatsPreset()
            {
                Swits = new Gearbox<int>() { Gear1 = 70, Gear2 = 210, Gear3 = 290 },
                Acelf = new Gearbox<double>() { Gear1 = 12.0, Gear2 = 6.0, Gear3 = 3.5 },
                Handbrake = 7,
                Airspeed = 1.0,
                Aircontrol = 60,
                Turnspeed = 6,
                Grip = 27.0,
                Bounce = 1.15,
                Simag = 1.15,
                Moment = 2.0,
                Comprad = 0.8,
                PushesOthers = 2,
                GetsPushed = 1,
                LiftsOthers = 0,
                GetsLifted = 32,
                Powerloss = 5500000,
                Flipy = -127,
                Msquash = 8,
                Clrad = 5000,
                DamageMultiplier = 0.52,
                Maxmag = 18000,
                Dishandle = 0.95,
                Outdam = 1.0
            };

        private static SortedDictionary<string, RechargedStatsPreset> presetDictionary = InitPresetDictionary();

        private static SortedDictionary<string, RechargedStatsPreset> InitPresetDictionary()
        {
            SortedDictionary<string, RechargedStatsPreset> d = new SortedDictionary<string, RechargedStatsPreset>(new PresetNameComparer());
            foreach(var prop in typeof(RechargedStatPresets).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                d.Add(prop.Name, (RechargedStatsPreset)prop.GetValue(null));
            }
            return d;
        }

        public static IEnumerable<string> PresetNames() => presetDictionary.Keys;

        public static bool GetPreset(string presetName, out RechargedStatsPreset result)
        {
            result = High_Rider;
            if (string.IsNullOrWhiteSpace(presetName)) return false;
            if(presetDictionary.TryGetValue(presetName, out result)) return true;
            result = High_Rider;
            return false;
        }

        private sealed class PresetNameComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if(x is null || y is null)
                {
                    if(x is null && y is null) return 0;
                    if(x is null) return -1;
                    return 1;
                }
                if(x.Length <= 0 || y.Length <= 0)
                {
                    return x.Length.CompareTo(y.Length);
                }
                int shorter = int.Min(x.Length, y.Length);
                ReadOnlySpan<char> X = x.AsSpan(0, shorter);
                ReadOnlySpan<char> Y = y.AsSpan(0, shorter);
                for(int i = 0; i < shorter; i++)
                {
                    char a = X[i];
                    char b = Y[i];
                    if(char.ToLowerInvariant(a) == char.ToLowerInvariant(b)) continue;
                    if ((a == ' ' || a == '_') && (b == ' ' || b == '_')) continue;
                    return char.ToLowerInvariant(a).CompareTo(char.ToLowerInvariant(b));
                }
                return x.Length.CompareTo(y.Length);
            }
        }
    }
}
