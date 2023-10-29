using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System;

namespace StageEditor.Models {

    public class StageInfo {
        public GameMode GameMode { get; set; }
        public int ObjectiveCount { get; set; }
        public double Ratio { get; set; }
        public double Speed { get; set; }
        public List<Wave> Waves { get; set; }
        public string FileName { get; set; }
        public string Folder { get; set; }
        public int BackgroundId { get; set; }
        public int BackgroundVersion { get; set; }
        public double BackgroundSpeedRatio { get; set; }

        public StageInfo() {
            Waves = new List<Wave>();
        }

        public void Save() {
            XmlDocument xml = new XmlDocument();
            XmlNode root = xml.CreateElement("StageDetail");
            xml.AppendChild(root);

            if (GameMode != Data.DEFAULT_GAMEMODE) {
                root.AddAttribute(xml, "GameMode", (int)GameMode);
            }
            if (ObjectiveCount != Data.DEFAULT_OBJECTIVE_COUNT) {
                root.AddAttribute(xml, "ObjectiveCount", ObjectiveCount);
            }
            if (Ratio != Data.DEFAULT_RATIO) {
                root.AddAttribute(xml, "Ratio", Ratio.ToFixedString());
            }
            if (Speed != Data.DEFAULT_SPEED) {
                root.AddAttribute(xml, "Speed", Speed.ToFixedString());
            }

            if (BackgroundId != Data.DEFAULT_BACKGROUND_ID) {
                root.AddAttribute(xml, "BackgroundId", BackgroundId);
            }

            if (BackgroundVersion != Data.DEFAULT_BACKGROUND_VERSION) {
                root.AddAttribute(xml, "BackgroundVersion", BackgroundVersion);
            }

            if (BackgroundSpeedRatio!= Data.DEFAULT_BACKGROUND_SPEED_RATIO) {
                root.AddAttribute(xml, "BackgroundSpeedRatio", BackgroundSpeedRatio);
            }

            double coinValue = 0;

            for (int i = 0; i < Waves.Count; i++) {
                if (Waves[i].Items != null && Waves[i].Items.Count > 0) {
                    var waveNode = root.AddChild(xml, "Wave");
                    Waves[i].Items =  Waves[i].Items.OrderBy(p => p.Position.Y).ToList();

                    if (Waves[i].Delay != Data.DEFAULT_DELAY) {
                        waveNode.AddAttribute(xml, "Delay", Waves[i].Delay.ToFixedString());
                    }
                    
                    if (Waves[i].EndDelay != Data.DEFAULT_DELAY) {
                        waveNode.AddAttribute(xml, "EndDelay", Waves[i].EndDelay.ToFixedString());
                    }
                    if (Waves[i].StartNotification != Data.DEFAULT_NOTIFICATION) {
                        waveNode.AddAttribute(xml, "StartNotification", Waves[i].StartNotification);
                    }
                    if (Waves[i].EndNotification != Data.DEFAULT_NOTIFICATION) {
                        waveNode.AddAttribute(xml, "EndNotification", Waves[i].EndNotification);
                    }

                    for (int j = 0; j < Waves[i].Items.Count; j++) {
                        var itemNode = waveNode.AddChild(xml, "Item");
                        itemNode.AddAttribute(xml, "Type", Waves[i].Items[j].TypeId);
                        itemNode.AddAttribute(xml, "Id", Waves[i].Items[j].Id);

                        if (Waves[i].Items[j].Values != "") {
                            itemNode.AddAttribute(xml, "Values", Waves[i].Items[j].Values);
                        }
                        if (Waves[i].Items[j].Item != "") {
                            itemNode.AddAttribute(xml, "Item", Waves[i].Items[j].Item);
                        }
                        
                        if (Waves[i].Items[j].Version != Data.DEFAULT_VERSION) {
                            itemNode.AddAttribute(xml, "Version", Waves[i].Items[j].Version);
                        }
                        var delay = Math.Round(Waves[i].Items[j].Position.Y, 3);
                        if (delay != Data.DEFAULT_DELAY) {
                            itemNode.AddAttribute(xml, "Delay", delay.ToFixedString());
                        }
                        if (Waves[i].Items[j].Ratio!= Data.DEFAULT_RATIO) {
                            itemNode.AddAttribute(xml, "Ratio", Waves[i].Items[j].Ratio.ToFixedString());
                        }
                        if (Waves[i].Items[j].SpeedRatio != Data.DEFAULT_SPEED) {
                            itemNode.AddAttribute(xml, "Speed", Waves[i].Items[j].SpeedRatio.ToFixedString());
                        }

                        if (!string.IsNullOrEmpty(Waves[i].Items[j].Color)) {
                            itemNode.AddAttribute(xml, "Color", Waves[i].Items[j].Color);
                        }

                        itemNode.AddAttribute(xml, "Position", string.Format("{0},{1}",
                            Math.Round(Waves[i].Items[j].Position.X, 3).ToFixedString(), Math.Round(Waves[i].Items[j].YOffset, 3).ToFixedString()));


                        if (Waves[i].Items[j].isEnemy && Data.Main.Enemies.ContainsKey(Waves[i].Items[j].Id)) {
                            coinValue += Data.Main.Enemies[Waves[i].Items[j].Id];
                        }
                    }
                }
            }

            if (coinValue >0) {
                root.AddAttribute(xml, "CoinValue", Math.Round(coinValue, 3).ToFixedString());
            }

            xml.Save(Path.Combine(Folder, FileName));
        }

        public void Load() {
            XmlDocument xml = new XmlDocument();
            string path = Path.Combine(Folder, FileName);
            xml.Load(path);
            XmlNode root = xml.SelectSingleNode("StageDetail");
            GameMode = root.ReadEnum<GameMode>("GameMode", Data.DEFAULT_GAMEMODE);
            ObjectiveCount = root.ReadInt("ObjectiveCount", Data.DEFAULT_OBJECTIVE_COUNT);
            Ratio = root.ReadDouble("Ratio", Data.DEFAULT_RATIO);
            Speed = root.ReadDouble("Speed", Data.DEFAULT_SPEED);
            BackgroundId = root.ReadInt("BackgroundId", Data.DEFAULT_BACKGROUND_ID);
            BackgroundVersion = root.ReadInt("BackgroundVersion", Data.DEFAULT_BACKGROUND_VERSION);
            BackgroundSpeedRatio = root.ReadDouble("BackgroundSpeedRatio", Data.DEFAULT_BACKGROUND_SPEED_RATIO);
            Waves = new List<Wave>();
            
            XmlNodeList nodes = root.SelectNodes("Wave");
            for (int i = 0; i < nodes.Count; i++) {
                Wave wave = new Wave() {
                    Delay = nodes[i].ReadDouble("Delay", Data.DEFAULT_DELAY),
                    EndDelay = nodes[i].ReadDouble("EndDelay", Data.DEFAULT_DELAY),
                    
                    StartNotification = nodes[i].ReadInt("StartNotification", Data.DEFAULT_NOTIFICATION),
                    EndNotification = nodes[i].ReadInt("EndNotification", Data.DEFAULT_NOTIFICATION),
                    Items = new List<StageItem>()
                };
                XmlNodeList itemNodes = nodes[i].SelectNodes("Item");
                foreach (XmlNode itemNode in itemNodes) {
                    var stageItem = new StageItem() {
                        Id = itemNode.ReadInt("Id"),
                        TypeId = itemNode.ReadInt("Type"),
                        Values = itemNode.ReadString("Values"),
                        Item = itemNode.ReadString("Item"),
                        Ratio = itemNode.ReadDouble("Ratio", Data.DEFAULT_RATIO),
                        SpeedRatio = itemNode.ReadDouble("Speed", Data.DEFAULT_SPEED),
                        Version = itemNode.ReadInt("Version", Data.DEFAULT_VERSION),
                        YOffset = itemNode.ReadVector("Position").Y,
                        Color = itemNode.ReadString("Color"),
                        Position = new System.Windows.Point(itemNode.ReadVector("Position").X, itemNode.ReadDouble("Delay", Data.DEFAULT_DELAY))
                    };

                    
                    wave.Items.Add(stageItem);
                }
                Waves.Add(wave);
            }
        }
    }

}
