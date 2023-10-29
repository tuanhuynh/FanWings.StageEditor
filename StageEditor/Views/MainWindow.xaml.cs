using Microsoft.Win32;
using StageEditor.Models;
using StageEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace StageEditor.Views {
    public partial class MainWindow : Window {

        private MainViewModel viewModel;

        private bool itemChanged;
        private bool waveChanged;

        private int lastWaveIndex;

        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            btnNew.Click += ButtonClick;
            btnSave.Click += ButtonClick;
            btnOpen.Click += ButtonClick;
            btnAddWave.Click += ButtonClick;
            btnUpdateWave.Click += ButtonClick;
            btnDeleteWave.Click += ButtonClick;
            btnUpdateItem.Click += ButtonClick;
            btnDeleteItem.Click += ButtonClick;
            btnUpdateFile.Click += ButtonClick;

            lstWaves.SelectionChanged += LstWaves_SelectionChanged;
            txtWaveStartDelay.TextChanged += TextChanged;
            txtWaveEndDelay.TextChanged += TextChanged;
            txtItemRatio.TextChanged += TextChanged;
            txtItemSpeed.TextChanged += TextChanged;
            txtItemValues.TextChanged += TextChanged;
            txtItemItem.TextChanged += TextChanged;
            txtItemColor.TextChanged += TextChanged;
            cboItemVersion.SelectionChanged += CboSelectionChanged;
            txtItemYOffset.TextChanged += TextChanged;
            cboWaveEndNotification.SelectionChanged += CboSelectionChanged;
            stageScreen.OnItemEvent += StageScreen_OnItemEvent;
            btnUpdateColor.Click += BtnUpdateColor_Click;
        }

        private void BtnUpdateColor_Click(object sender, RoutedEventArgs e) {
            if (viewModel.HasFile &&
                (!string.IsNullOrEmpty(txtMountainColor.Text) || !string.IsNullOrEmpty(txtCloudColor.Text)
                || !string.IsNullOrEmpty(txtShipsColor.Text))) {
                viewModel.UpdateColors(txtCloudColor.Text, txtMountainColor.Text, txtShipsColor.Text);
                MessageBox.Show("Colors changed");
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Data.Main.Load();
            viewModel = new MainViewModel();
            viewModel.Load();
            stageScreen.SetItemTypes(Data.Main.StageTypes);
            cboGameMode.ItemsSource = Data.Main.GameModes;
            cboWaveStartNotification.ItemsSource = Data.Main.StartNotifications;
            cboWaveEndNotification.ItemsSource = Data.Main.EndNotifications;
            UpdateVisibility();
        }

        private void StageScreen_OnItemEvent(Controls.StageScreen.EventType type) {
            if (type == Controls.StageScreen.EventType.Created) {
                viewModel.AddItemToWave(stageScreen.Items[stageScreen.Items.Count - 1]);
                lstWaves.ItemsSource = null;
                lstWaves.ItemsSource = viewModel.Data.Waves;
                lstWaves.SelectedIndex = viewModel.SelectedWaveIndex;
            } else if (type == Controls.StageScreen.EventType.Moved) {
                viewModel.UpdateItemPosition(stageScreen.SelectedItem.Position);
            } else if (type == Controls.StageScreen.EventType.Selected) {
                viewModel.SelectedItemIndex = stageScreen.SelectedIndex;
                var item = viewModel.SelectedItem;
                txtItemRatio.Text = item.Ratio.ToFixedString();
                txtItemSpeed.Text = item.SpeedRatio.ToFixedString();
                txtItemValues.Text = item.Values;
                txtItemItem.Text = item.Item;
                txtItemColor.Text = item.Color;
                var model = Data.Main.GetStageType(item.TypeId, item.Id);
                cboItemVersion.Items.Clear();
                for (int i = 0; i <= model.MaxVersion; i++) {
                    cboItemVersion.Items.Add(i);
                }
                cboItemVersion.SelectedItem = item.Version;

                txtItemYOffset.Text = item.YOffset.ToFixedString();
            }
            UpdateVisibility();
        }

        private void CboSelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cbo = sender as ComboBox;
            if (cbo == cboWaveEndNotification || cbo == cboWaveStartNotification) {
                waveChanged = true;
            } else if (cbo == cboItemVersion) {
                itemChanged = true;
            }
            UpdateVisibility();
        }

        private void TextChanged(object sender, TextChangedEventArgs e) {
            TextBox txt = sender as TextBox;
            if (txt == txtWaveStartDelay || txt == txtWaveEndDelay) {
                waveChanged = true;
            } else if (txt == txtItemRatio || txt == txtItemSpeed || txt == txtItemYOffset
                || txt == txtItemValues || txt == txtItemItem || txt == txtItemColor) {
                itemChanged = true;
            }
            UpdateVisibility();
        }

        private void LstWaves_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (lstWaves.SelectedItem != null) {

                viewModel.SelectedWaveIndex = lstWaves.SelectedIndex;
                viewModel.SelectedItemIndex = -1;
                grdItemDetail.Visibility = Visibility.Collapsed;
                grdWaveDetail.Visibility = Visibility.Visible;

                var wave = viewModel.SelectedWave;

                txtWaveStartDelay.Text = wave.Delay.ToFixedString();
                txtWaveEndDelay.Text = wave.EndDelay.ToFixedString();
                cboWaveEndNotification.SelectedItem = Data.Main.GetEndNotification(wave.EndNotification);
                cboWaveStartNotification.SelectedItem = Data.Main.GetStartNotification(wave.StartNotification);

                if (lastWaveIndex != lstWaves.SelectedIndex && viewModel.SelectedWave != null) {
                    stageScreen.ShowWave(viewModel.SelectedWave);
                }
                lastWaveIndex = lstWaves.SelectedIndex;

                UpdateVisibility();
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            if (btn == btnNew) {
                NewFile();
            } else if (btn == btnSave) {
                SaveFile();
            } else if (btn == btnAddWave) {
                NewWave();
            } else if (btn == btnUpdateWave) {
                UpdateSelectedWave();
            } else if (btn == btnDeleteWave) {
                DeleteSelectedWave();
            } else if (btn == btnDeleteItem) {
                DeleteSelectedItem();
            } else if (btn == btnUpdateItem) {
                UpdateSelectedItem();
            } else if (btn == btnOpen) {
                OpenFile();
            } else if (btn == btnUpdateFile) {
                SaveFile();
            }
            UpdateVisibility();
        }

        private void OpenFile() {
            OpenFileDialog dialog = new OpenFileDialog() {
                InitialDirectory = !viewModel.HasFile || string.IsNullOrEmpty(viewModel.Data.Folder)
                    ? AppDomain.CurrentDomain.BaseDirectory : viewModel.Data.Folder,
                DefaultExt = "*.xml",
                Filter = "Stage File|*.xml"
            };
            if (dialog.ShowDialog() == true) {
                viewModel.OpenFile(dialog.FileName);
                //Init view
                tabMain.SelectedItem = tabStage;
                txtFile.Text = viewModel.Data.FileName;
                txtRatio.Text = viewModel.Data.Ratio.ToFixedString();
                txtSpeed.Text = viewModel.Data.Speed.ToFixedString();
                txtObjective.Text = viewModel.Data.ObjectiveCount.ToString();
                txtBackgroundId.Text = viewModel.Data.BackgroundId.ToString();
                txtBackgroundVersion.Text = viewModel.Data.BackgroundVersion.ToString();
                txtBackgroundSpeedRatio.Text = viewModel.Data.BackgroundSpeedRatio.ToString();

                cboGameMode.SelectedItem = viewModel.Data.GameMode;
                lstWaves.ItemsSource = null;
                lstWaves.ItemsSource = viewModel.Data.Waves;
                stageScreen.ShowWave(null);
                viewModel.SelectedItemIndex = -1;
            }
        }

        private void SaveFile() {
            if (viewModel.HasFile) {

                if (cboGameMode.SelectedItem != null) {
                    viewModel.Data.GameMode = (GameMode)cboGameMode.SelectedItem;
                }
                if (int.TryParse(txtObjective.Text, out int count)) {
                    viewModel.Data.ObjectiveCount = count;
                }

                if (int.TryParse(txtBackgroundId.Text, out int backgroundId)) {
                    viewModel.Data.BackgroundId = backgroundId;
                }

                if (int.TryParse(txtBackgroundVersion.Text, out int backgroundVersion)) {
                    viewModel.Data.BackgroundVersion = backgroundVersion;
                }

                if (double.TryParse(txtBackgroundSpeedRatio.Text, out double backgroundSpeedRatio)) {
                    viewModel.Data.BackgroundSpeedRatio = backgroundSpeedRatio;
                }

                viewModel.Data.Ratio = txtRatio.Text.ParseToDouble(Data.DEFAULT_RATIO);
                viewModel.Data.Speed = txtSpeed.Text.ParseToDouble(Data.DEFAULT_SPEED);

                if (viewModel.SaveToFile()) {
                    stageScreen.ShowWave(null);
                    MessageBox.Show("Data is saved", "SUCCESS");
                }
            } else {
                MessageBox.Show("No file to save", "ERROR");
            }
        }

        private void NewFile() {
            SaveFileDialog dialog = new SaveFileDialog() {
                InitialDirectory = !viewModel.HasFile || string.IsNullOrEmpty(viewModel.Data.Folder)
                    ? AppDomain.CurrentDomain.BaseDirectory : viewModel.Data.Folder,
                DefaultExt = "*.xml",
                Filter = "Stage File|*.xml"
            };
            if (dialog.ShowDialog() == true) {
                viewModel.NewFile(dialog.FileName);
                tabMain.SelectedItem = tabStage;
                cboGameMode.SelectedItem = GameMode.Survival;
                txtFile.Text = viewModel.Data.FileName;
                txtRatio.Text = Data.DEFAULT_RATIO.ToString();
                txtSpeed.Text = Data.DEFAULT_SPEED.ToString();
                txtBackgroundId.Text = Data.DEFAULT_BACKGROUND_ID.ToString();
                txtBackgroundVersion.Text = Data.DEFAULT_BACKGROUND_VERSION.ToString();
                txtBackgroundSpeedRatio.Text = Data.DEFAULT_BACKGROUND_SPEED_RATIO.ToString();
                viewModel.SelectedItemIndex = -1;
            }
        }

        private void NewWave() {
            viewModel.AddNewWave();
            lstWaves.ItemsSource = null;
            lstWaves.ItemsSource = viewModel.Data.Waves;
            lstWaves.SelectedIndex = lstWaves.Items.Count - 1;
            btnUpdateWave.Visibility = Visibility.Collapsed;
        }

        private void UpdateSelectedWave() {
            viewModel.UpdateSelectedWave(txtWaveStartDelay.Text.ParseToDouble(), txtWaveEndDelay.Text.ParseToDouble(),
                    cboWaveStartNotification.SelectedItem as Notification,
                    cboWaveEndNotification.SelectedItem as Notification);
            waveChanged = false;
            UpdateVisibility();
            stageScreen.ShowWave(viewModel.SelectedWave);
        }

        private void DeleteSelectedWave() {
            if (viewModel.SelectedWave != null && MessageBox.Show(
                "Do you want to delete wave " + (viewModel.SelectedWaveIndex + 1).ToString() + "?",
                "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                viewModel.DeleteSelectedWave();
                lstWaves.ItemsSource = null;
                lstWaves.ItemsSource = viewModel.Data.Waves;

            }
        }

        private void UpdateSelectedItem() {
            if (cboItemVersion.SelectedItem != null) {
                viewModel.UpdateSelectedItemValues(
                        (int)cboItemVersion.SelectedItem,
                        txtItemRatio.Text.ParseToDouble(), txtItemSpeed.Text.ParseToDouble(),
                        txtItemYOffset.Text.ParseToDouble(),
                        txtItemValues.Text, txtItemItem.Text, txtItemColor.Text);
                itemChanged = false;

                UpdateVisibility();
            }
        }

        private void DeleteSelectedItem() {
            if (viewModel.SelectedItem != null && MessageBox.Show(
                "Do you want to delete this item?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                stageScreen.DeleteItem(viewModel.SelectedItemIndex);
                viewModel.DeleteSelectedItem();

                lstWaves.ItemsSource = null;
                lstWaves.ItemsSource = viewModel.Data.Waves;
                lstWaves.SelectedIndex = viewModel.SelectedWaveIndex;

                UpdateVisibility();
            }
        }

        private void UpdateVisibility() {

            grdStage.Visibility = viewModel.HasFile ? Visibility.Visible : Visibility.Hidden;
            stkWaveCommands.Visibility = viewModel.HasFile ? Visibility.Visible : Visibility.Hidden;
            btnAddWave.IsEnabled = viewModel.HasFile;
            btnDeleteWave.Visibility = lstWaves.SelectedItem != null ? Visibility.Visible : Visibility.Hidden;
            btnSave.IsEnabled = viewModel.HasFile;

            grdWaveDetail.Visibility = viewModel.SelectedWave != null && viewModel.SelectedItem == null ?
                Visibility.Visible : Visibility.Collapsed;
            grdItemDetail.Visibility = viewModel.SelectedItem != null ? Visibility.Visible : Visibility.Collapsed;
            btnUpdateWave.Visibility = waveChanged ? Visibility.Visible : Visibility.Collapsed;
            btnUpdateItem.Visibility = itemChanged ? Visibility.Visible : Visibility.Collapsed;

            stageScreen.IsEnabled = viewModel.SelectedWave != null;

        }

    }
}
