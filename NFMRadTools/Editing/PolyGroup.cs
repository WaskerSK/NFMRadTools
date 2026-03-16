using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public class PolyGroup
    {
        private PolyGroupMode _mode;
        public string Name { get; set; }
        private List<Polygon> _polygons;
        private ReadOnlyCollection<Polygon> _publicPolygons;
        public ReadOnlyCollection<Polygon> Polygons => _publicPolygons ??= new ReadOnlyCollection<Polygon>(_polygons);
        //public Wheel WheelDefinition { get; set; }
        public PolyGroupMode Mode 
        {
            get => _mode;
            set
            {
                PolyGroupMode oldValue = _mode;
                _mode = value;
                if (oldValue == _mode) return;
                bool? alternativePoly = null;
                switch(_mode)
                {
                    case PolyGroupMode.Normal:
                        alternativePoly = false;
                        break;
                    case PolyGroupMode.DragShotWheel: break;
                    case PolyGroupMode.PhyrexianWheel:
                    case PolyGroupMode.G6Wheel:
                        alternativePoly = true;
                        break;
                }
                if(alternativePoly.HasValue)
                {
                    foreach(Polygon p in Polygons)
                    {
                        p.AlternativePolyMarkup = alternativePoly.Value;
                    }
                }
            }
        }

        public int CustomWheelIndex { get; set; }

        public PolyGroup()
        {
            _polygons = new List<Polygon>();
        }

        public void SetColor(Color color)
        {
            foreach (Polygon p in _polygons)
            {
                p.Color = color;
            }
        }

        public bool AddPolygon(Polygon polygon)
        {
            if(polygon is null) return false;
            if(polygon.PolyGroup == this) return false;
            _polygons.Add(polygon);
            polygon.PolyGroup = this;
            if (Mode == PolyGroupMode.PhyrexianWheel || Mode == PolyGroupMode.G6Wheel)
                polygon.AlternativePolyMarkup = true;
            return true;
        }

        public bool AddPolygons(IEnumerable<Polygon> polygons)
        {
            if(polygons is null) return false;
            IEnumerable<Polygon> en = polygons.Where(x => x is not null).Where(x => x.PolyGroup != this);
            _polygons.AddRange(polygons);
            bool altMarkup = Mode == PolyGroupMode.PhyrexianWheel || Mode == PolyGroupMode.G6Wheel;
            foreach(Polygon p in en)
            {
                p.PolyGroup = this;
                p.AlternativePolyMarkup = altMarkup;
            }
            return en.Any();
        }

        public bool RemovePolygon(Polygon polygon)
        {
            if(polygon is null) return false;
            bool removed = _polygons.Remove(polygon);
            if(removed)
            {
                if(polygon.PolyGroup == this)
                    polygon.PolyGroup = null;
            }
            return removed;
        }

        public void RemoveRange(int index, int count)
        {
            for(int i = index; i < index + count; i++)
            {
                if (_polygons[i].PolyGroup == this)
                    _polygons[i].PolyGroup = null;
            }
            _polygons.RemoveRange(index, count);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                sb.Append("<g=");
                sb.Append(Name);
                sb.AppendLine(">");
            }

            foreach (Polygon poly in _polygons)
            {
                sb.AppendLine(poly.ToString());
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                sb.Append("</g=");
                sb.Append(Name);
                sb.AppendLine(">");
            }

            return sb.ToString();
        }

        public PolyGroup Duplicate()
        {
            PolyGroup copy = new PolyGroup();
            copy._polygons.EnsureCapacity(_polygons.Count);
            copy._mode = _mode;
            copy.Name = Name;
            copy.CustomWheelIndex = CustomWheelIndex;
            foreach(Polygon poly in _polygons)
            {
                Polygon dupPoly = poly.Duplicate();
                dupPoly.PolyGroup = copy;
                copy._polygons.Add(dupPoly);
            }
            return copy;
        }
    }
}
