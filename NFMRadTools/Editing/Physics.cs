using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public struct Physics
    {
        public int Handbrake { get; set; } //0-100
        public int TurningSensitivity { get; set; } //0-100
        public int TireGrip { get; set; } //0-100
        public int Bouncing { get; set; } //0-100
        public int Unknown { get; set; }
        public int LiftsOthers { get; set; } //0-100
        public int GetsLifted {  get; set; } //0-100
        public int PushesOthers { get; set; } //0-100
        public int GetsPushed { get; set; } //0-100
        public int AerialRotationSpeed { get; set; } //0-100
        public int AerialControlGliding { get; set; } //0-100
        public int Radius { get; set; } //0-100
        public int Magnitude { get; set; } //0-100
        public int RoofDestruction { get; set; } //0-100
        public Engine Engine { get; set; }
        public int Unknown2 { get; set; }

        public override string ToString()
        {
            return $"physics({Handbrake},{TurningSensitivity},{TireGrip},{Bouncing},{Unknown},{LiftsOthers},{GetsLifted},{PushesOthers},{GetsPushed},{AerialRotationSpeed},{AerialControlGliding},{Radius},{Magnitude},{RoofDestruction},{(int)Engine},{Unknown2})";
        }
    }
}
