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
using System.Threading;

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
            MyTreeViewItem root = MyItemManager.GetRootTVItem();
            tv.Items.Add(root);
            ((MyTreeViewItem)tv.Items[0]).IsSelected = true;
            tv.EndInit();
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

                //处理查找
                if (CurrentPath == "查找") {
                    menu_open_Click(sender, e);
                    return;
                }

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
        private void lb_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource.GetType() == typeof(ScrollViewer)) {
                var item = lb.SelectedItem as MyListBoxItem;
                if (item != null&&CurrentPath!="查找") {
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

        //lisibox键盘，监控快捷键事件
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
                    if (File.Exists(copyParent + oldname))
                        File.Move(copyParent + oldname, copyParent + newname);
                    else Directory.Move(copyParent + oldname, copyParent + newname);
                    item.MyVisialbe = Visibility.Hidden;
                    FlushLBByCurrentPath();
                }
            }
            catch (Exception) { }
        }

        //后退按钮事件
        private void goback_bt_Click(object sender, RoutedEventArgs e) {
            if (Back_History.Count != 0) {
                if (address_box.IsReadOnly) address_box.IsReadOnly = false;//解锁输入框
                //获取历史记录并移出历史记录
                string his = Back_History.Last.Value;
                Back_History.RemoveLast();
                if (CurrentPath != "查找") {
                    Ahead_History.AddLast(CurrentPath);//当前目录移入前进
                }
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

        //前进按钮事件
        private void goahead_bt_Click(object sender, RoutedEventArgs e) {
            if (Ahead_History.Count != 0) {
                if (address_box.IsReadOnly) address_box.IsReadOnly = false;//解锁输入框
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

        //查找按钮点击事件
        private CancellationTokenSource cts;
        private async void find_bt_Click(object sender, RoutedEventArgs e) {
            string pattern = "*" + search_box.Text + "*";
            if (pattern == "**") return;
            string path = CurrentPath;
            Back_History.AddLast(CurrentPath);
            address_box.Text = "查找";
            CurrentPath = "查找";
            //锁定两个输入框
            address_box.IsReadOnly = true;
            search_box.IsReadOnly = true;

            if (CurrentPath == "") {
                MessageBox.Show("请进入某个磁盘后搜索", "提示", MessageBoxButton.OK);
                return;
            }
            cts = new CancellationTokenSource();
            lb.BeginInit();
            lb.Items.Clear();
            lb.EndInit();
            await MyFindManeger.AddLBItemByThreads(lb, path, cts.Token, pattern);
            search_box.IsReadOnly = false;//解锁一个输入框
        }

        //取消按钮点击事件
        private void cancell_bt_Click(object sender, RoutedEventArgs e) {
            cts.Cancel();
        }


        //右键菜单显示事件
        private void ContextMenu_Opened(object sender, RoutedEventArgs e) {
            if (CurrentPath == "") {//根路径下右键
                menu_open.Visibility = Visibility.Collapsed;
                menu_open_dir.Visibility = Visibility.Collapsed;
                menu_copy.IsEnabled = false;
                menu_delete.IsEnabled = false;
                menu_rename.IsEnabled = false;
                menu_creat.IsEnabled = false;
                menu_paste.IsEnabled = false;

                menu_status.IsEnabled = true;
            }
            else if (CurrentPath=="查找") {//查找项目下右键
                menu_open.Visibility = Visibility.Visible;
                menu_open_dir.Visibility = Visibility.Visible;
                menu_copy.IsEnabled = false;
                menu_delete.IsEnabled = false;
                menu_rename.IsEnabled = false;
                menu_creat.IsEnabled = false;
                menu_paste.IsEnabled = false;

                menu_status.IsEnabled = true;
            }
            else if (lb.SelectedItem==null) {//空白处右键
                menu_open.Visibility = Visibility.Collapsed;
                menu_open_dir.Visibility = Visibility.Collapsed;
                menu_copy.IsEnabled = false;
                menu_delete.IsEnabled = false;
                menu_rename.IsEnabled = false;
                menu_status.IsEnabled = false;

                menu_creat.IsEnabled = true;
                menu_paste.IsEnabled = true;
            }
            else {//正常右键lb项时
                menu_open.Visibility = Visibility.Collapsed;
                menu_open_dir.Visibility = Visibility.Collapsed;
                menu_creat.IsEnabled = false;
                menu_paste.IsEnabled = false;

                menu_copy.IsEnabled = true;
                menu_delete.IsEnabled = true;
                menu_rename.IsEnabled = true;
                menu_status.IsEnabled = true;
            }
        }

        private void menu_rename_Click(object sender, RoutedEventArgs e) {
            var item = lb.SelectedItem as MyListBoxItem;
            lb.BeginInit();
            item.MyVisialbe = Visibility.Visible;
            lb.EndInit();
        }

        private void menu_delete_Click(object sender, RoutedEventArgs e) {
            var item = lb.SelectedItem as MyListBoxItem;
            string name = item.MyText;
            string _fileinfo;
            if (CurrentPath.Length < 4)
                _fileinfo = CurrentPath + name;
            else _fileinfo = CurrentPath + "\\" + name;

            var dr = MessageBox.Show("您确认要删除吗?", "删除",MessageBoxButton.YesNo);
            if (dr == MessageBoxResult.No)
                return;

            if (File.Exists(_fileinfo)) {
                File.Delete(_fileinfo);
                FlushLBByCurrentPath();
            }
            else if (Directory.Exists(_fileinfo)) {
                Directory.Delete(_fileinfo, true);
                FlushLBByCurrentPath();
            }
        }

        private void menu_creat_dirs_Click(object sender, RoutedEventArgs e) {
            string name = "新建文件夹";
            string _fileinfo;
            if (CurrentPath.Length < 4)
                _fileinfo = CurrentPath + name;
            else _fileinfo = CurrentPath + "\\" + name;
            while (Directory.Exists(_fileinfo)) {
                _fileinfo += " 副本";
            }
            Directory.CreateDirectory(_fileinfo);
            FlushLBByCurrentPath();
        }

        private void menu_open_Click(object sender, RoutedEventArgs e) {
            //获取父路径
            var lt = (lb.SelectedItem as MyListBoxItem);
            string chooes = lt.MyMessage;
            string parent = chooes.Substring(5);
            if (chooes.Substring(0, 5) == "所在路径:") {
                parent = chooes.Substring(5);
                if (parent.Length > 3) parent = parent + "\\";
                System.Diagnostics.Process.Start(parent+lt.MyText);
            }
            else {
                //刷新关联的三控件
                CurrentPath = parent;
                address_box.Text = CurrentPath;
                TVChangeByOther = true;
                MyItemManager.GetTVItemFromPath(tv, parent, true);
                TVChangeByOther = false;
                FlushLBByCurrentPath();
                address_box.IsReadOnly = false;//解锁输入框
            }
        }

        private void menu_open_dir_Click(object sender, RoutedEventArgs e) {
            //获取父路径
            string chooes = (lb.SelectedItem as MyListBoxItem).MyMessage;
            string parent;
            if (chooes.Substring(0, 5) == "所在路径:")
                parent = chooes.Substring(5);
            else {
                var reg = new System.Text.RegularExpressions.Regex(@".*(?:\\)");
                var match = reg.Match(chooes, 5);
                parent = match.Value;
            }
            if (parent.Length > 3) parent = parent + "\\";

            //刷新关联的三控件
            CurrentPath = parent;
            address_box.Text = CurrentPath;
            TVChangeByOther = true;
            MyItemManager.GetTVItemFromPath(tv, parent, true);
            TVChangeByOther = false;
            FlushLBByCurrentPath();
            address_box.IsReadOnly = false;//解锁输入框
        }
    }
}
