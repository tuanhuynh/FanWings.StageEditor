using StageEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace StageEditor.ViewModels {
    public class MainViewModel {
        public StageInfo Data { get; private set; }
        public bool HasFile { get { return Data != null && !string.IsNullOrEmpty(Data.FileName); } }

        public List<StageItem> Items { get; set; }


        public int SelectedItemIndex { get; set; }
        public int SelectedWaveIndex { get; set; }
        public Wave SelectedWave {
            get {
                if (SelectedWaveIndex >= 0 && SelectedWaveIndex < Data.Waves.Count)
                    return Data.Waves[SelectedWaveIndex];
                else
                    return null;
            }
        }
        public StageItem SelectedItem {
            get {
                if (SelectedWave != null && SelectedItemIndex >= 0 && SelectedItemIndex < Data.Waves[SelectedWaveIndex].Items.Count)
                    return Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex];
                else
                    return null;
            }
        }

        public void Load() {
            Data = new StageInfo();
        }

        public bool NewFile(string fileName) {
            try {
                FileInfo fileInfo = new FileInfo(fileName);
                Data = new StageInfo() {
                    FileName = fileInfo.Name,
                    Folder = fileInfo.DirectoryName,
                };
                Data.Save();
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public bool OpenFile(string fileName) {
            try {
                FileInfo fileInfo = new FileInfo(fileName);
                Data = new StageInfo() {
                    FileName = fileInfo.Name,
                    Folder = fileInfo.DirectoryName,
                };
                Data.Load();
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public void UpdateColors(string cloudColor, string mountainColor, string shipColor) {
            List<int> cloudIds = new List<int>() {8,9,10,36,37,38 };
            List<int> mountainIds = new List<int>() { 14,15,16,17,18,19,4,5,6,7 };
            List<int> shipIds = new List<int>() { 1, 2, 3};
            for(int i = 11; i <= 35; i++) {
                shipIds.Add(i);
            }
            for (int i = 0; i < Data.Waves.Count; i++) {
                for (int j = 0; j < Data.Waves[i].Items.Count; j++) {
                    if (Data.Waves[i].Items[j].TypeId == 5 && string.IsNullOrEmpty(Data.Waves[i].Items[j].Color)) {
                        int id = Data.Waves[i].Items[j].Id;
                        if (!string.IsNullOrEmpty(cloudColor) && cloudIds.Contains(id)) {
                            Data.Waves[i].Items[j].Color = cloudColor;
                        } else if (!string.IsNullOrEmpty(mountainColor) && mountainIds.Contains(id)) {
                            Data.Waves[i].Items[j].Color = mountainColor;
                        } else if (!string.IsNullOrEmpty(shipColor) && shipIds.Contains(id)) {
                            Data.Waves[i].Items[j].Color = shipColor;
                        }
                    }
                }
            }
        }

        public bool SaveToFile() {
            try {
                Data.Save();
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public void AddNewWave() {
            Data.Waves.Add(new Wave() {
                Delay = 0,
                EndDelay = 0,
                StartNotification = 0,
                EndNotification = 0,
                Items = new List<StageItem>()
            });
        }

        public void UpdateSelectedWave(double startDelay, double endDelay, Notification start, Notification end) {
            Data.Waves[SelectedWaveIndex].Delay = startDelay;
            Data.Waves[SelectedWaveIndex].EndDelay = endDelay;
            if (start != null) {
                Data.Waves[SelectedWaveIndex].StartNotification = start.Id;
            }
            if (end != null) {
                Data.Waves[SelectedWaveIndex].EndNotification = end.Id;
            }
        }

        public void DeleteSelectedWave() {
            Data.Waves.RemoveAt(SelectedWaveIndex);
            SelectedWaveIndex = -1;
            SelectedItemIndex = -1;
        }

        public void AddItemToWave(StageItem item) {
            if (item != null && SelectedWave != null) {
                Data.Waves[SelectedWaveIndex].Items.Add(item);
            }
        }

        public void UpdateItemPosition(Point position) {
            if (SelectedItem != null) {
                Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex].Position = position;
            }
        }

        public void UpdateSelectedItemValues(int version, double ratio, double speed,
            double yOffset, string values, string item, string color) {
            if (SelectedItem != null) {
                Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex].Version = version;
                Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex].Ratio = ratio;
                Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex].SpeedRatio = speed;
                Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex].YOffset = yOffset;
                Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex].Values = values;
                Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex].Item = item;
                Data.Waves[SelectedWaveIndex].Items[SelectedItemIndex].Color = color;
            }
        }

        public void DeleteSelectedItem() {
            if (SelectedItem != null) {
                Data.Waves[SelectedWaveIndex].Items.RemoveAt(SelectedItemIndex);
                SelectedItemIndex = -1;
            }
        }
    }
}

