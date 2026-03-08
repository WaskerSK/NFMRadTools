using NFMRadTools.Editing.Presets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class RechargedStats
    {
        private Gearbox<int> swits;
        public ref Gearbox<int> Swits => ref swits; //speed
        private Gearbox<double> acelf;
        public ref Gearbox<double> Acelf => ref acelf; //acceleration
        public int Handbrake { get; set; } //hanbrake power
        public double Airspeed { get; set; } // stunt rotation speed
        public int Aircontrol { get; set; } //air control
        public int Turnspeed { get; set; } //turn speed
        public double Grip { get; set; } //ground grip
        public double Bounce { get; set; } //bounce on landing/hit
        public double Simag { get; set; } //dirt kick up while driving
        public double Moment { get; set; } //strength
        public double Comprad { get; set; } //hitbox size
        public int PushesOthers { get; set; } //push others power
        public int GetsPushed { get; set; } //gets pushed power
        public int LiftsOthers { get; set; } //lift others power
        public int GetsLifted { get; set; } //gets lifted power
        public long Powerloss { get; set; } //power drain, higher value = slower drain
        public int Flipy { get; set; } // height of car roof when landing upside down
        public int Msquash { get; set; } //roof damage
        public int Clrad { get; set; } //visual damage intensity, lower value = less deformation
        public double DamageMultiplier { get; set; } //damage multiplier upon impact
        public int Maxmag { get; set; } // endurance
        public double Dishandle { get; set; } //handling stat bar value on car select screen
        public double Outdam { get; set; } //endurance stat bar value on car select screen

        public RechargedStats() : this(RechargedStatPresets.Default)
        {

        }

        public RechargedStats(RechargedStatsPreset preset)
        {
            swits = preset.Swits;
            acelf = preset.Acelf;
            Handbrake = preset.Handbrake;
            Airspeed = preset.Airspeed;
            Aircontrol = preset.Aircontrol;
            Turnspeed = preset.Turnspeed;
            Grip = preset.Grip;
            Bounce = preset.Bounce;
            Simag = preset.Simag;
            Moment = preset.Moment;
            Comprad = preset.Comprad;
            PushesOthers = preset.PushesOthers;
            GetsPushed = preset.GetsPushed;
            LiftsOthers = preset.LiftsOthers;
            GetsLifted = preset.GetsLifted;
            Powerloss = preset.Powerloss;
            Flipy = preset.Flipy;
            Msquash = preset.Msquash;
            Clrad = preset.Clrad;
            DamageMultiplier = preset.DamageMultiplier;
            Maxmag = preset.Maxmag;
            Dishandle = preset.Dishandle;
            Outdam = preset.Outdam;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("swits(").Append(Swits.ToString()).AppendLine(")");
            sb.Append("acelf(").Append(Acelf.ToString(Program.NFMDemicalFormat, null)).AppendLine(")");
            sb.Append("handb(").Append(Handbrake).AppendLine(")");
            sb.Append("airs(").Append(Airspeed.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("airc(").Append(Aircontrol.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("turn(").Append(Turnspeed).AppendLine(")");
            sb.Append("grip(").Append(Grip.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("bounce(").Append(Bounce.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("simag(").Append(Simag.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("moment(").Append(Moment.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("comprad(").Append(Comprad.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("push(").Append(PushesOthers).AppendLine(")");
            sb.Append("revpush(").Append(GetsPushed).AppendLine(")");
            sb.Append("lift(").Append(LiftsOthers).AppendLine(")");
            sb.Append("revlift(").Append(GetsLifted).AppendLine(")");
            sb.Append("powerloss(").Append(Powerloss).AppendLine(")");
            sb.Append("flipy(").Append(Flipy).AppendLine(")");
            sb.Append("msquash(").Append(Msquash).AppendLine(")");
            sb.Append("clrad(").Append(Clrad).AppendLine(")");
            sb.Append("dammult(").Append(DamageMultiplier.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("maxmag(").Append(Maxmag).AppendLine(")");
            sb.Append("dishandle(").Append(Dishandle.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            sb.Append("outdam(").Append(Outdam.ToString(Program.NFMDemicalFormat)).AppendLine(")");
            return sb.ToString();
        }
    }
}
