using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StageEditor.Models {
    public class StageItem {
        public bool isEnemy { get { return TypeId == 1; } }
        public bool isBeastOrItem { get { return TypeId < 5; } }
        public bool isBackgroundObject { get { return TypeId == 5; } }

        public int TypeId { get; set; }
        public int Id { get; set; }
        public Point Position { get; set; }
        public int Version { get; set; }
        public double Ratio { get; set; }
        public double SpeedRatio { get; set; }
        public double YOffset { get; set; }
        public string Values { get; set; }
        public string Item { get; set; }
        public string Color { get; set; }

        public override string ToString() {
            return Math.Round(Position.Y, 3).ToString();
        }
    }

    public class StageType {
        public bool isBeastOrItem { get { return TypeId < 5; } }
        public bool isBackgroundObject { get { return TypeId == 5; } }

        public int TypeId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public Point Size { get; set; }
        public BitmapImage Image { get; set; }
        public int MaxVersion { get; set; }

        public override string ToString() {
            return Name;
        }
    }
    
}
