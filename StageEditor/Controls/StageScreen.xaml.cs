using StageEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StageEditor.Controls {
    public partial class StageScreen : UserControl {

        public enum EventType {
            Created = 1,
            Selected = 2,
            Moved = 3
        }

        public delegate void OnItemEventdHandler(EventType type);
        public event OnItemEventdHandler OnItemEvent;

        public const string DROP_IDENTIFIER = "STAGEITEM";
        
        private float secondHeight = 140;
        private float duration = 60;
        private float screenWidthRatio = 0.4f;
        private const float SCREEN_WIDTH = 792f;
        private float scale;

        private Border draggedItem;
        private Point mousePosition;
        private List<Border> borders;
        private Point _startPoint;
        
        public int SelectedIndex { get; private set; }
        public List<StageItem> Items { get; private set; }
        public StageItem SelectedItem {
            get {
                if (Items != null && SelectedIndex >= 0 && SelectedIndex < Items.Count) {
                    return Items[SelectedIndex];
                } else {
                    return null;
                }
            }
        }

        public StageScreen() {
            InitializeComponent();
            Loaded += StageScreen_Loaded;
            SizeChanged += StageScreen_SizeChanged;
            canMain.MouseDown += CanMain_MouseDown;
            canMain.MouseMove += CanMain_MouseMove;
            canMain.Drop += CanMain_Drop;
            canMain.DragEnter += CanMain_DragEnter;
            canMain.MouseLeftButtonDown += CanMain_MouseLeftButtonDown;
            canMain.MouseLeftButtonUp += CanMain_MouseLeftButtonUp;

            lstItems.PreviewMouseLeftButtonDown += LstItems_PreviewMouseLeftButtonDown;
            lstItems.PreviewMouseMove += LstItems_PreviewMouseMove;
            lstBackgroundObjects.PreviewMouseLeftButtonDown += LstItems_PreviewMouseLeftButtonDown;
            lstBackgroundObjects.PreviewMouseMove += LstItems_PreviewMouseMove;

            chkBackground.Checked += CheckedChanged;
            chkBackground.Unchecked += CheckedChanged;
            chkBeast.Checked += CheckedChanged;
            chkBeast.Unchecked += CheckedChanged;

            tabItemLists.SelectionChanged += TabItemLists_SelectionChanged;

            chkBeast.IsChecked = true;
            chkBackground.IsChecked = true;
        }

        private void TabItemLists_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            lstBackgroundObjects.Visibility = tabItemLists.SelectedIndex == 1? Visibility.Visible : Visibility.Collapsed;
            lstItems.Visibility = tabItemLists.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CheckedChanged(object sender, RoutedEventArgs e) {
            
            if (chkBeast.IsChecked.HasValue && chkBackground.IsChecked.HasValue) {
                tabItemLists.SelectedIndex = !chkBeast.IsChecked.Value && chkBackground.IsChecked.Value ? 1 : 0;
            }
            
            if (Items != null) {
                for(int i=0; i < Items.Count; i++) {
                    if (Items[i].isBackgroundObject) {
                        borders[i].Visibility = chkBackground.IsChecked.HasValue && chkBackground.IsChecked.Value 
                            ? Visibility.Visible: Visibility.Collapsed;
                    } else {
                        borders[i].Visibility = chkBeast.IsChecked.HasValue && chkBeast.IsChecked.Value
                            ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        public void SetItemTypes(List<StageType> types) {
            if (types != null) {
                lstItems.ItemsSource = types.Where(p => p.isBeastOrItem);
                lstBackgroundObjects.ItemsSource = types.Where(p=> p.isBackgroundObject);
            }
        }

        public void SelectItem(int index) {
            if (SelectedIndex >= 0) {
                borders[SelectedIndex].BorderThickness = new Thickness(0);
            }
            if (index >= 0 && index < borders.Count) {
                borders[index].BorderThickness = new Thickness(1);
                SelectedIndex = index;
                OnItemEvent?.Invoke(EventType.Selected);
            } else {
                SelectedIndex = -1;
            }
        }

        public void DeleteItem(int index) {
            if (Items!=null && index>=0 && index < Items.Count) {
                Items.RemoveAt(index);
                SelectedIndex = -1;
                canMain.Children.Remove(borders[index]);
                borders.RemoveAt(index);

                for(int i=0; i < borders.Count; i++) {
                    borders[i].Tag = i.ToString();
                }
            }
        }

        public void ShowWave(Wave wave){
            canMain.Children.Clear();
            borders.Clear();
            Items.Clear();
            SelectedIndex = -1;
            if (wave!=null && wave.Items != null) {
                Items = new List<StageItem>(wave.Items);

                for (int i = 0; i < Items.Count; i++) {
                    var item = Items[i];
                    var itemType = Data.Main.GetStageType(item.TypeId, item.Id);
                    var border = new Border() {
                        Background = new ImageBrush(itemType.Image),
                        Width = itemType.Size.X * scale,
                        Height = itemType.Size.Y * scale,
                        Tag = borders.Count.ToString(),
                        BorderBrush = new SolidColorBrush(Colors.Gray)
                    };

                    Canvas.SetLeft(border, canMain.ActualWidth / 2 + item.Position.X * 100 * scale - border.Width / 2);
                    Canvas.SetTop(border, canMain.ActualHeight - screen.ActualHeight - border.Height / 2 - item.Position.Y * secondHeight);
                    canMain.Children.Add(border);
                    borders.Add(border);
                }
            }
        }
        
        private void CanMain_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DROP_IDENTIFIER)) {
                if (e.Data.GetData(DROP_IDENTIFIER) is StageType itemType) {
                    Point canvasPoint = e.GetPosition(canMain);
                    var border = new Border() {
                        Background = new ImageBrush(itemType.Image),
                        Width = itemType.Size.X * scale,
                        Height = itemType.Size.Y * scale,
                        Tag = borders.Count.ToString(),
                        BorderBrush = new SolidColorBrush(Colors.Gray)
                    };
                    Canvas.SetTop(border, canvasPoint.Y - border.Height / 2);
                    Canvas.SetLeft(border, canvasPoint.X - border.Width / 2);
                    canMain.Children.Add(border);
                    borders.Add(border);
                    
                    Items.Add(new StageItem() {
                        Id = itemType.Id,
                        TypeId = itemType.TypeId,
                        Version = Data.DEFAULT_VERSION,
                        Position = GetBorderPosition(border),
                        Ratio = Data.DEFAULT_RATIO,
                        SpeedRatio = Data.DEFAULT_SPEED,
                        YOffset = Data.DEFAULT_YOFFSET,
                        Values = ""
                    });

                    OnItemEvent?.Invoke(EventType.Created);
                    SelectItem(Items.Count - 1);
                }
            }
        }


        private void StageScreen_Loaded(object sender, RoutedEventArgs e) {
            scrollMain.ScrollToEnd();
            borders = new List<Border>();
            Items = new List<StageItem>();
            SelectedIndex = -1;
        }
        
        private void UpdateSize() {
            screen.Width = canMain.ActualWidth * screenWidthRatio;
            screen.Height = screen.Width * 16 / 9;
            Content.Height = screen.Height + duration * secondHeight;
            scale = (float)screen.Width / SCREEN_WIDTH;

            int blockCount = (int)duration * 2;
            if (timeline.Children != null && timeline.Children.Count < blockCount) {
                for (int i = 0; i < blockCount; i++) {
                    float time = (((float)blockCount - i - 1) / 2);
                    Border brd = new Border() {
                        BorderBrush = new SolidColorBrush(Colors.DarkGray),
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        Height = secondHeight / 2,
                        Width = Content.Width,
                        Child = new TextBlock() {
                            Text = time.ToString() + "s",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Padding = new Thickness(2),
                            Foreground = new SolidColorBrush(Colors.DarkGray)
                        }
                    };

                    timeline.Children.Add(brd);
                }
            }

            if (borders != null && borders.Count > 0) {
                for (int i = 0; i < borders.Count; i++) {
                    var pos = Items[i].Position;
                    var type = Data.Main.GetStageType(Items[i].TypeId, Items[i].Id);
                    borders[i].Width = type.Size.X * scale;
                    borders[i].Height = type.Size.Y * scale;
                    Canvas.SetLeft(borders[i], canMain.ActualWidth / 2 + pos.X * 100 * scale - borders[i].Width / 2);
                    Canvas.SetTop(borders[i], canMain.ActualHeight - screen.ActualHeight
                        - borders[i].Height / 2 - pos.Y * secondHeight);
                }
            }
        }

        private void StageScreen_SizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateSize();
        }

        private void CanMain_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource is Border brd && int.TryParse(brd.Tag.ToString(), out int index)) {
                SelectItem(index);
            }
        }



        private void CanMain_MouseMove(object sender, MouseEventArgs e) {
            if (draggedItem != null) {
                var position = e.GetPosition(canMain);
                var offset = position - mousePosition;
                mousePosition = position;
                Canvas.SetLeft(draggedItem, Canvas.GetLeft(draggedItem) + offset.X);
                Canvas.SetTop(draggedItem, Canvas.GetTop(draggedItem) + offset.Y);
            }
        }

        private void CanMain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (draggedItem != null) {
                canMain.ReleaseMouseCapture();
                Panel.SetZIndex(draggedItem, 0);
                if (int.TryParse(draggedItem.Tag.ToString(), out int index)) {
                    Items[index].Position = GetBorderPosition(draggedItem);
                }
                OnItemEvent?.Invoke(EventType.Moved);
                draggedItem = null;
            }
        }

        private void CanMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var image = e.Source as Border;

            if (image != null && canMain.CaptureMouse()) {
                mousePosition = e.GetPosition(canMain);
                draggedItem = image;
                Panel.SetZIndex(draggedItem, 1); // in case of multiple images
            }
        }

        private void CanMain_DragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DROP_IDENTIFIER) || sender == e.Source) {
                e.Effects = DragDropEffects.None;
            }
        }

        

        private void LstItems_PreviewMouseMove(object sender, MouseEventArgs e) {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) {
                // Get the dragged ListBoxItem
                var listBox = sender as ListBox;
                var listBoxItem = listBox.SelectedItem;

                if (listBox!= null && listBoxItem != null && listBox.Visibility == Visibility.Visible) {
                    // Initialize the drag & drop operation
                    DataObject dragData = new DataObject(DROP_IDENTIFIER, listBoxItem);
                    DragDrop.DoDragDrop(listBox, dragData, DragDropEffects.Move);
                }
            }
        }

        private void LstItems_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(null);
        }

        private Point GetBorderPosition(Border brd) {
            var canvasPoint = new Point(Canvas.GetLeft(brd) + brd.Width / 2, Canvas.GetTop(brd) + brd.Height / 2);

            var oy = canMain.ActualWidth / 2;
            var ox = canMain.ActualHeight - screen.Height;
            var x = (canvasPoint.X - oy) / (scale * 100);
            var y = (ox - canvasPoint.Y) / secondHeight;
            return new Point(x, y);
        }
    }
}
