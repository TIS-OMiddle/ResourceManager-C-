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

namespace WpfTest {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        private string CurrentPath = "";

        public MainWindow() {
            InitializeComponent();
            MyInitialize();
        }

        private void MyInitialize() {
            
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
            MyTreeViewItem root = new MyTreeViewItem("此电脑", "folder");
            root.Tag = "";
            foreach (string i in disks) {
                MyTreeViewItem tvic = new MyTreeViewItem(
                    disksInfo[j++].VolumeLabel + " (" + i.Substring(0, 2) + ")", "folder");
                tvic.Tag = i;
                root.Items.Add(tvic);
            }
            return root;
        }

        //为树结点创建子结点
        public void CreatSubTVItemFromTVItem(MyTreeViewItem item) {
            try {
                string newpath = item.Tag.ToString();
                string[] dirs = Directory.GetDirectories(newpath);//获取该目录的内容
                if (newpath.Length > 3) newpath += "\\"; ;
                foreach (string i in dirs) {
                    if ((new FileInfo(i).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                    MyTreeViewItem tvic = new MyTreeViewItem(i.Replace(newpath, ""), "folder");
                    tvic.Tag = i;
                    item.Items.Add(tvic);
                }
            }
            catch (Exception) { }
        }

        //根据路径返回listbox的Item集合
        public List<MyListBoxItem> CreatLBItemFromPath(string path) {
            List<MyListBoxItem> res = new List<MyListBoxItem>();
            if (path == "") {
                DriveInfo[] disksInfo = DriveInfo.GetDrives();
                int j = 0;
                foreach (var i in disksInfo) {
                    MyListBoxItem lbit = new MyListBoxItem(
                        disksInfo[j].VolumeLabel + " (" + i.Name.Substring(0, 2) + ")", "folder");
                    lbit.MyMessage = "可用空间:" + (disksInfo[j].TotalFreeSpace/1024/1024/1024).ToString()+"GB"
                        + "\n总大小:"+(disksInfo[j].TotalSize / 1024 / 1024 / 1024).ToString() + "GB";
                    res.Add(lbit);
                    j++;
                }
                return res;
            }
            DirectoryInfo _files = new DirectoryInfo(path);
            FileInfo[] files = _files.GetFiles("*", SearchOption.TopDirectoryOnly);
            DirectoryInfo[] dirs = _files.GetDirectories("*", SearchOption.TopDirectoryOnly);
            foreach (var i in files) {
                if ((i.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                MyListBoxItem lbit = new MyListBoxItem(i.Name, "file");
                lbit.MyMessage = i.Name + "\n类型:" + i.Extension + "文件\n修改日期:" + i.LastWriteTime.ToString();
                res.Add(lbit);
            }
            foreach (var i in dirs) {
                if ((i.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                MyListBoxItem lbit = new MyListBoxItem(i.Name, "folder");
                lbit.MyMessage = i.Name + "\n创建日期:" + i.CreationTime;
                res.Add(lbit);
            }
            return res;
        }

        //treeview选中项改变事件
        private void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            MyTreeViewItem my_select = (MyTreeViewItem)tv.SelectedItem;
            CurrentPath = my_select.Tag.ToString();
            if (my_select.Items.Count == 0) {
                tv.BeginInit();
                CreatSubTVItemFromTVItem(my_select);
                tv.EndInit();
            }
            List<MyListBoxItem> list = CreatLBItemFromPath(CurrentPath);
            lb.BeginInit();
            lb.Items.Clear();
            foreach (var i in list)
                lb.Items.Add(i);
            lb.EndInit();
            if (CurrentPath == "") address_box.Text = "此电脑";
            else address_box.Text = CurrentPath;
        }

        //listbox双击进入或打开
        private void lb_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource.GetType() != typeof(ScrollViewer)&&e.RightButton!=MouseButtonState.Pressed) {
                var m = (MyListBoxItem)lb.SelectedItem;
                if (m == null) return;
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
                    else CurrentPath += filename;//双击项为目录
                }
                else {//当前路径不为盘符不加斜杠
                    CurrentPath = CurrentPath + "\\";
                    if (File.Exists(CurrentPath + filename)) {//双击项为文件
                        System.Diagnostics.Process.Start(CurrentPath + filename);
                        return;
                    }
                    else CurrentPath += filename;//双击项为目录
                }

                //双击项为目录
                lb.BeginInit();
                var list = CreatLBItemFromPath(CurrentPath);
                lb.Items.Clear();
                foreach (var i in list)
                    lb.Items.Add(i);
                lb.EndInit();
                if (CurrentPath == "") address_box.Text = "此电脑";
                else address_box.Text = CurrentPath;
            }
        }

        //listbox空白区域点击取消选中
        private void lb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource.GetType() == typeof(ScrollViewer)) {
                lb.SelectedIndex = -1;
            }
        }

    }

    public class MyListBoxItem {
        private static byte[] folder;
        private static byte[] file;
        static MyListBoxItem() {
            FileStream fs = new FileStream("./icon/folder.ico", FileMode.Open);
            folder = new byte[fs.Length];
            fs.Read(folder, 0, folder.Length);
            fs.Close();

            //folder图标初始化
            fs = new FileStream("./icon/file.ico", FileMode.Open);
            file = new byte[fs.Length];
            fs.Read(file, 0, file.Length);
            fs.Close();
        }

        public BitmapImage MyIcon { set; get; }
        public string MyText { set; get; }
        public string MyMessage { set; get; }

        public MyListBoxItem(string text,string icon) {
            MyText = text;
            MyIcon = new BitmapImage();
            MyIcon.BeginInit();
            switch (icon) {
                case "folder":
                    MyIcon.StreamSource = new MemoryStream(folder);
                    break;
                case "file":
                    MyIcon.StreamSource = new MemoryStream(file);
                    break;
                default:
                    MyIcon.StreamSource = new MemoryStream(folder);
                    break;
            }
            MyIcon.EndInit();
        }
    }

    //带有图标-文本的重载控件
    public class MyTreeViewItem : TreeViewItem {
        private static byte[] folder;
        private static byte[] file;

        static MyTreeViewItem() {
            //folder图标初始化
            FileStream fs = new FileStream("./icon/folder.ico", FileMode.Open);
            folder = new byte[fs.Length];
            fs.Read(folder, 0, folder.Length);
            fs.Close();

            //folder图标初始化
            fs = new FileStream("./icon/file.ico", FileMode.Open);
            file = new byte[fs.Length];
            fs.Read(file, 0, file.Length);
            fs.Close();
        }

        private TextBlock tb;
        private Image img;
        private StackPanel sp;

        public MyTreeViewItem() {
            tb = new TextBlock();
            sp = new StackPanel();
            sp.Orientation = System.Windows.Controls.Orientation.Horizontal;
            Header = sp;
        }

        //text:显示的文本  icon:选择显示的图标
        public MyTreeViewItem(string text, string icon) {
            tb = new TextBlock();
            sp = new StackPanel();
            sp.Orientation = System.Windows.Controls.Orientation.Horizontal;
            Header = sp;
            tb.Text = text;
            HeaderIcon = icon;
        }

        public string HeaderText {
            get { return tb.Text; }
            set { tb.Text = value; }
        }

        public string HeaderIcon {
            set {
                BitmapImage icon = new BitmapImage();
                icon.BeginInit();
                icon.DecodePixelWidth = 13;
                icon.DecodePixelHeight = 13;
                switch (value) {
                    case "folder":
                        icon.StreamSource = new MemoryStream(folder);
                        break;
                    case "file":
                        icon.StreamSource = new MemoryStream(file);
                        break;
                    default:
                        icon.StreamSource = new MemoryStream(file);
                        break;
                }
                icon.EndInit();
                img = new Image();
                img.Source = icon;
                img.Margin = new Thickness(0, 0, 5, 0);
                sp.Children.Clear();
                sp.Children.Add(img);
                sp.Children.Add(tb);
            }
        }
    }

    //文本缩短显示
    public class StringformatConvert : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string s = value.ToString();
            int leng;
            if (int.TryParse(parameter.ToString(), out leng)) {
                if (s.Length <= leng)
                    return s;
                else
                    return s.Substring(0, leng) + "...";
            }
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
