using StageEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

namespace StageEditor {

    public class Data {
        public const GameMode DEFAULT_GAMEMODE = GameMode.Survival;
        public const int DEFAULT_BACKGROUND_ID = 1;
        public const int DEFAULT_BACKGROUND_VERSION = 0;
        public const float DEFAULT_BACKGROUND_SPEED_RATIO = 1;
        public const int DEFAULT_OBJECTIVE_COUNT = 0;
        public const float DEFAULT_DELAY = 0;
        public const int DEFAULT_NOTIFICATION = 0;
        public const double DEFAULT_RATIO = 1;
        public const double DEFAULT_SPEED = 1;
        public const int DEFAULT_VERSION = 0;
        public const double DEFAULT_YOFFSET = 0;
        
        private static Data main;
        public static Data Main { get { if (main == null) main = new Data(); return main; } }

        public Dictionary<int,double> Enemies { get; private set; }
        public List<StageType> StageTypes { get; private set; }
        public List<Notification> StartNotifications { get; private set; }
        public List<Notification> EndNotifications { get; private set; }
        public List<GameMode> GameModes { get; private set; }
        
        public StageType GetStageType(int typeId, int id) {
            if (StageTypes!=null && StageTypes.Count > 0) {
                return StageTypes.SingleOrDefault(p => p.TypeId == typeId && p.Id == id);
            }
            return null;
        }
        
        public Notification GetStartNotification(int id) {
            var noti = StartNotifications.SingleOrDefault(p => p.Id == id);
            if (noti != null) {
                return noti;
            }
            return null;
        }

        public Notification GetEndNotification(int id) {
            var noti = EndNotifications.SingleOrDefault(p => p.Id == id);
            if (noti != null) {
                return noti;
            }
            return null;
        }

        public void Load() {
            StageTypes = new List<StageType>();
            GameModes = new List<GameMode>();
            StartNotifications = new List<Notification>();
            EndNotifications = new List<Notification>();

            foreach (GameMode item in Enum.GetValues(typeof(GameMode))) {
                GameModes.Add(item);
            }
            XmlDocument xml = new XmlDocument();

            Dictionary<int, int> coins = new Dictionary<int, int>();
            xml.Load("Editor_Data/Objects.xml");
            var itemNodes = xml.DocumentElement.SelectSingleNode("Items").SelectNodes("Item");
            for (int i = 0; i < itemNodes.Count; i++) {
                var node = itemNodes[i];
                int coinId = node.ReadInt("Id");
                int coinValue = node.ReadInt("CoinValue");
                if (coinValue > 0) {
                    coins.Add(coinId, coinValue);
                }
            }
            
            Enemies = new Dictionary<int, double>();
            xml.Load("Editor_Data/Enemies.xml");
            var enemyNodes = xml.DocumentElement.SelectSingleNode("Enemies").SelectNodes("Enemy");
            for(int i=0; i < enemyNodes.Count; i++) {
                var partNodes = enemyNodes[i].SelectNodes("Part");
                double val = 0;
                if (partNodes!=null && partNodes.Count > 0) {
                    for(int p=0; p < partNodes.Count; p++) {
                        double[] coinValues = partNodes[p].ReadDoubleArray("Coins");
                        if (coinValues != null && coinValues.Length > 1) {
                            int itemInstanceCount = (int)Math.Floor(((double)coinValues.Length - 1) / 2);
                            double chance = coinValues.Get(0, 1);
                            for (int k = 0; k < itemInstanceCount; k++) {
                                var id = (int)coinValues.Get(k * 2 + 1);
                                if (coins.ContainsKey(id)) {
                                    var amount = (int)coinValues.Get(k * 2 + 2);
                                    val += amount * coins[id];
                                }
                            }
                            val *= chance;
                        }
                    }
                }
                
                Enemies.Add(enemyNodes[i].ReadInt("Id"), val);
            }
            
            xml.Load("Editor_Data/GameData.xml");
            var root = xml.DocumentElement;

            var nodes = root.SelectNodes("//Items/Item");
            foreach (XmlNode node in nodes) {
                var uri = new Uri(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        string.Format("Editor_Data/Items/{0}.png", node.ReadString("Name"))));
                var img = new BitmapImage(uri);
                var e = new StageType() {
                    Id = node.ReadInt("Id"),
                    TypeId = node.ReadInt("Type"),
                    Name = node.ReadString("Name"),
                    MaxVersion= node.ReadInt("MaxVersion", 0),
                    Image = img,
                    Size = new Point(img.PixelWidth, img.PixelHeight)
                };
                StageTypes.Add(e);
            }

            StartNotifications.Add(new Notification() { Id = 0, Content = "None" });
            EndNotifications.Add(new Notification() { Id = 0, Content = "None" });

            nodes = root.SelectNodes("//Notifications/Notification");
            foreach (XmlNode node in nodes) {
                Notification n = new Notification() {
                    Id = node.ReadInt("Id"),
                    Type = node.ReadInt("Type"),
                    Content = node.ReadString("Content")
                };
                if (n.IsStart) {
                    StartNotifications.Add(n);
                } else {
                    EndNotifications.Add(n);
                }
            }

        }
    }
}
