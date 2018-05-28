using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyControlItem;

namespace WpfTest {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        private string CurrentPath = "";
        private bool TVChangeByOther = false;
        private LinkedList<string> Back_History = new LinkedList<string>();
        private LinkedList<string> Ahead_History = new LinkedList<string>();

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            tv.BeginInit();
            MyTreeViewItem root = GetRootTVItem();
            tv.Items.Add(root);
            ((MyTreeViewItem)tv.Items[0]).IsSelected = true;
            tv.EndInit();
        }

        //获取根结点
        public MyTreeViewItem GetRootTVItem() {
            string[] disks = Directory.GetLogicalDrives();
            DriveInfo[] disksInfo = DriveInfo.GetDrives();
            int j = 0;
            MyTreeViewItem root = new MyTreeViewItem("此电脑", MyIcons.disk);
            root.Tag = "";
            foreach (string i in disks) {
                MyTreeViewItem tvic = new MyTreeViewItem(
                    disksInfo[j++].VolumeLabel + " (" + i.Substring(0, 2) + ")", MyIcons.disk);
                tvic.Tag = i;
                root.Items.Add(tvic);
            }
            return root;
        }


        //treeview选中项改变事件
        private void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (TVChangeByOther) return;//不来自tv本身的选择改变则直接退出

            Back_History.AddLast(CurrentPath);//记录进栈
            MyTreeViewItem my_select = (MyTreeViewItem)tv.SelectedItem;
            CurrentPath = my_select.Tag.ToString();
            if (my_select.Items.IsEmpty) {
                tv.BeginInit();
                MyItemManager.CreatSubTVItemFromTVItem(my_select);
                tv.EndInit();
            }
            List<MyListBoxItem> list = MyItemManager.CreatLBItemFromPath(CurrentPath);
            lb.BeginInit();
            lb.Items.Clear();
            foreach (var i in list)
                lb.Items.Add(i);
            lb.EndInit();
            if (CurrentPath == "") address_box.Text = "此电脑";
            else address_box.Text = CurrentPath;
        }

        //listbox双击进入或打开事件
        private void lb_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource.GetType() != typeof(ScrollViewer)&&e.RightButton!=MouseButtonState.Pressed) {
                var m = (MyListBoxItem)lb.SelectedItem;
                string filename = m.MyText;

                //处理当前路径+双击项
                if (CurrentPath == "") {//双击项为盘符，加上双击项
                    CurrentPath = filename.Substring(filename.Length - 3, 2) + "\\";
                }
                else if (CurrentPath.Length < 4) {//当前路径为盘符，不加斜杠
                    if (File.Exists(CurrentPath + filename)) {//双击项为文件
                        System.Diagnostics.Process.Start(CurrentPath + filename);
                        return;
                    }
                    else {
                        Back_History.AddLast(CurrentPath);//记录进栈
                        CurrentPath += filename;//双击项为目录
                    }
                }
                else {//当前路径不为盘符加斜杠
                    CurrentPath = CurrentPath + "\\";
                    if (File.Exists(CurrentPath + filename)) {//双击项为文件
                        System.Diagnostics.Process.Start(CurrentPath + filename);
                        return;
                    }
                    else {
                        Back_History.AddLast(CurrentPath);//记录进栈
                        CurrentPath += filename;//双击项为目录
                    }
                }
                //刷新listbox
                FlushLBByCurrentPath();
                //刷新地址栏
                address_box.Text = CurrentPath;
                //使treeview选中项同步变化
                TVChangeByOther = true;
                MyItemManager.GetTVItemFromPath(tv, CurrentPath, true);
                TVChangeByOther = false;

                //清空前进
                if (Ahead_History.Count > 0) Ahead_History.Clear();
            }
        }

        //listbox空白区域点击取消选中事件
        private void lb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource.GetType() == typeof(ScrollViewer)) {
                var item = lb.SelectedItem as MyListBoxItem;
                if (item != null) {
                    FlushLBByCurrentPath();
                }
                lb.SelectedIndex = -1;
            }
        }

        //由CurrentPath变量刷新listbox
        private void FlushLBByCurrentPath() {
            lb.BeginInit();
            var list = MyItemManager.CreatLBItemFromPath(CurrentPath);
            lb.Items.Clear();
            foreach (var i in list)
                lb.Items.Add(i);
            lb.EndInit();
        }

        //lisibox键盘，监控快捷键F2，重命名用事件
        private void lb_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.F2) {
                var item = lb.SelectedItem as MyListBoxItem;
                if (item == null) return;
                lb.BeginInit();
                item.MyVisialbe = Visibility.Visible;
                lb.EndInit();
            }
        }

        //listbox子控件textbox，重命名用事件
        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            try {
                if (e.Key == Key.Enter) {
                    var item = lb.SelectedItem as MyListBoxItem;
                    var tb = sender as TextBox;
                    string newname = tb.Text, oldname = item.MyText, copyParent = CurrentPath;
                    if (copyParent == "") return;
                    else if (copyParent.Length > 3) copyParent += "\\";
                    if (File.Exists(CurrentPath + oldname))
                        File.Move(CurrentPath + oldname, CurrentPath  + newname);
                    else Directory.Move(CurrentPath  + oldname, CurrentPath + newname);
                    item.MyVisialbe = Visibility.Hidden;
                    FlushLBByCurrentPath();
                }
            }
            catch (Exception) { }
        }

        //后退按钮
        private void goback_bt_Click(object sender, RoutedEventArgs e) {
            if (Back_History.Count != 0) {
                //获取历史记录并移出历史记录
                string his = Back_History.Last.Value;
                Back_History.RemoveLast();
                Ahead_History.AddLast(CurrentPath);//当前目录移入前进
                CurrentPath = his;
                address_box.Text = CurrentPath == "" ? "此电脑" : CurrentPath;
                //刷新tv
                TVChangeByOther = true;
                MyItemManager.GetTVItemFromPath(tv, his, true);
                TVChangeByOther = false;
                //刷新listbox
                FlushLBByCurrentPath();
            }
        }

        private void goahead_bt_Click(object sender, RoutedEventArgs e) {
            if (Ahead_History.Count != 0) {
                //当前目录移入历史记录
                Back_History.AddLast(CurrentPath);
                //获取前进并移出
                string go = Ahead_History.Last.Value;
                Ahead_History.RemoveLast();
                //刷新tv
                TVChangeByOther = true;
                MyItemManager.GetTVItemFromPath(tv, go, true);
                TVChangeByOther = false;
                //刷新listbox
                CurrentPath = go;
                address_box.Text = CurrentPath == "" ? "此电脑" : CurrentPath;
                FlushLBByCurrentPath();
            }
        }
    }
}
