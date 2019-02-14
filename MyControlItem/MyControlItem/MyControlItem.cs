using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
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

namespace MyControlItem {

    /// <summary>
    /// 自定义转换器，文件名过长时缩短显示
    /// </summary>
    public class StringformatConvert : IValueConverter {
        /// <summary>
        /// 系统调用的函数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string s = value.ToString();
            int leng;
            if (int.TryParse(parameter.ToString(), out leng)) {
                if (s[0] >= 0x4e00 && s[0] <= 0x9fbb) leng /= 2;
                if (s.Length <= leng)
                    return s;
                else
                    return s.Substring(0, leng) + "...";
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// 回调转换，由于已有数据绑定，此处不使用
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 用于选择图标的枚举
    /// </summary>
    public enum MyIcons {
        /// <summary>
        /// 文件夹图标
        /// </summary>
        folder,
        /// <summary>
        /// 磁盘图标
        /// </summary>
        disk,
        /// <summary>
        /// 文件图标
        /// </summary>
        file
    }

    /// <summary>
    /// 加载icon文件夹下的图标
    /// </summary>
    public class MyIconResourse {
        public static byte[] folder;
        public static byte[] file;
        public static byte[] disk;
        static MyIconResourse() {
            //file图标初始化
            FileStream fs = new FileStream("./icon/folder.png", FileMode.Open);
            folder = new byte[fs.Length];
            fs.Read(folder, 0, folder.Length);
            fs.Close();

            //folder图标初始化
            fs = new FileStream("./icon/file.png", FileMode.Open);
            file = new byte[fs.Length];
            fs.Read(file, 0, file.Length);
            fs.Close();

            //disk图标初始化
            fs = new FileStream("./icon/disk.png", FileMode.Open);
            disk = new byte[fs.Length];
            fs.Read(disk, 0, disk.Length);
            fs.Close();
        }
    }

    /// <summary>
    /// 带有图标-文本-隐藏文本-悬浮显示文本的用于listbox的重载子控件
    /// </summary>
    public class MyListBoxItem {
        /// <summary>
        /// 显示的图标
        /// </summary>
        public BitmapImage MyIcon { set; get; }

        /// <summary>
        /// 显示的文本
        /// </summary>
        public string MyText { set; get; }

        /// <summary>
        /// 悬浮的文本
        /// </summary>
        public string MyMessage { set; get; }

        /// <summary>
        /// 设置可见性
        /// </summary>
        public Visibility MyVisialbe { set; get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="text">显示的文本</param>
        /// <param name="icon">显示的图标</param>
        public MyListBoxItem(string text, MyIcons icon) {
            MyText = text;
            MyIcon = new BitmapImage();
            MyIcon.BeginInit();
            MyVisialbe = Visibility.Hidden;
            switch (icon) {
                case MyIcons.folder:
                    MyIcon.StreamSource = new MemoryStream(MyIconResourse.folder);
                    break;
                case MyIcons.file:
                    MyIcon.StreamSource = new MemoryStream(MyIconResourse.file);
                    break;
                case MyIcons.disk:
                    MyIcon.StreamSource = new MemoryStream(MyIconResourse.disk);
                    break;
                default:
                    MyIcon.StreamSource = new MemoryStream(MyIconResourse.file);
                    break;
            }
            MyIcon.EndInit();
        }
    }

    /// <summary>
    /// 带有图标-文本的用于treeview的重载子控件
    /// </summary>
    public class MyTreeViewItem : TreeViewItem {
        private TextBlock tb;
        private Image img;
        private StackPanel sp;
        private bool hasExpanded = false;

        /// <summary>
        /// 默认构造方法，没有任何内容
        /// </summary>
        public MyTreeViewItem() {
            tb = new TextBlock();
            sp = new StackPanel();
            sp.Orientation = System.Windows.Controls.Orientation.Horizontal;
            Header = sp;
            Expanded += MyTreeViewItem_Expanded;
        }

        private void MyTreeViewItem_Expanded(object sender, RoutedEventArgs e) {
            if (hasExpanded) return;
            MyItemManager.CreatSubTVItemFromTVItem(this);
            hasExpanded = true;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="text">显示的文本</param>
        /// <param name="icon">显示的图标</param>
        public MyTreeViewItem(string text, MyIcons icon) {
            tb = new TextBlock();
            sp = new StackPanel();
            sp.Orientation = System.Windows.Controls.Orientation.Horizontal;
            Header = sp;
            tb.Text = text;
            HeaderIcon = icon;
            Expanded += MyTreeViewItem_Expanded;
        }

        /// <summary>
        /// 显示的文本
        /// </summary>
        public string HeaderText {
            get { return tb.Text; }
            set { tb.Text = value; }
        }

        /// <summary>
        /// 显示的图标
        /// </summary>
        public MyIcons HeaderIcon {
            set {
                BitmapImage icon = new BitmapImage();
                icon.BeginInit();
                icon.DecodePixelWidth = 13;
                icon.DecodePixelHeight = 13;
                switch (value) {
                    case MyIcons.folder:
                        icon.StreamSource = new MemoryStream(MyIconResourse.folder);
                        break;
                    case MyIcons.file:
                        icon.StreamSource = new MemoryStream(MyIconResourse.file);
                        break;
                    case MyIcons.disk:
                        icon.StreamSource = new MemoryStream(MyIconResourse.disk);
                        break;
                    default:
                        icon.StreamSource = new MemoryStream(MyIconResourse.file);
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

    /// <summary>
    /// 自定义控件管理，提供由路径获取对应控件的各种方法
    /// </summary>
    public class MyItemManager {
        /// <summary>
        /// 根据指定路径返回listboxItem集合
        /// </summary>
        /// <param name="path">指定路径</param>
        /// <returns>listboxItem集合</returns>
        public static List<MyListBoxItem> CreatLBItemFromPath(string path) {
            List<MyListBoxItem> res = new List<MyListBoxItem>();
            if (path == "") {
                DriveInfo[] disksInfo = DriveInfo.GetDrives();
                int j = 0;
                foreach (var i in disksInfo) {
                    MyListBoxItem lbit = new MyListBoxItem(
                        disksInfo[j].VolumeLabel + " (" + i.Name.Substring(0, 2) + ")", MyIcons.disk);
                    lbit.MyMessage = "可用空间:" + (disksInfo[j].TotalFreeSpace / 1024 / 1024 / 1024).ToString() + "GB"
                        + "\n总大小:" + (disksInfo[j].TotalSize / 1024 / 1024 / 1024).ToString() + "GB";
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
                MyListBoxItem lbit = new MyListBoxItem(i.Name, MyIcons.file);
                lbit.MyMessage = i.Name + "\n类型:" + i.Extension + "文件\n修改日期:" + i.LastWriteTime.ToString();
                res.Add(lbit);
            }
            foreach (var i in dirs) {
                if ((i.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                MyListBoxItem lbit = new MyListBoxItem(i.Name, MyIcons.folder);
                lbit.MyMessage = i.Name + "\n创建日期:" + i.CreationTime;
                res.Add(lbit);
            }
            return res;
        }

        /// <summary>
        /// 生成指定结点的至其二级子结点
        /// </summary>
        /// <param name="item">指定结点</param>
        public static void CreatSubTVItemFromTVItem(MyTreeViewItem item) {
            if(item.Items.Count==0)
                _CreatSubTVItemFromTVItem(item);

            foreach (var i in item.Items) {
                var ii = i as MyTreeViewItem;
                if (ii.Items.Count == 0)
                    _CreatSubTVItemFromTVItem(ii);
            }
        }

        /// <summary>
        /// 生成指定结点的子结点
        /// </summary>
        /// <param name="item">指定结点</param>
        private static void _CreatSubTVItemFromTVItem(MyTreeViewItem item) {
            try {
                string newpath = item.Tag.ToString();
                string[] dirs = Directory.GetDirectories(newpath);//获取该目录的内容
                if (newpath.Length > 3) newpath += "\\"; ;
                foreach (string i in dirs) {
                    if ((new FileInfo(i).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) continue;
                    MyTreeViewItem tvic = new MyTreeViewItem(i.Replace(newpath, ""), MyIcons.folder);
                    tvic.Tag = i;
                    item.Items.Add(tvic);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 返回代表指定路径的treeviewItem，不存在时自动为该路径创建结点
        /// </summary>
        /// <param name="tv"></param>
        /// <param name="path">指定的路径</param>
        /// <param name="changeSelected">是否设置为被选中项，默认false</param>
        /// <returns>对应结点</returns>
        public static MyTreeViewItem GetTVItemFromPath(TreeView tv, string path, bool changeSelected = false) {
            tv.BeginInit();
            MyTreeViewItem aimItem = tv.Items[0] as MyTreeViewItem;//获得root item
            if (path != "") {//不是代表“此电脑”的路径
                string disk = path.Substring(0, 3);//获取磁盘
                //使aimItem为某磁盘根目录
                foreach (var i in aimItem.Items) {
                    if (((MyTreeViewItem)i).Tag.ToString() == disk) {
                        aimItem = i as MyTreeViewItem;
                        break;
                    }
                }
                //获取磁盘子路径
                path = path.Substring(3);
                string[] dirs = path.Split('\\');

                //aimItem指向目标目录
                for (int j = 0; j < dirs.Length; j++) {
                    if (aimItem.Items.IsEmpty)//该目录没有子目录时，创建子目录
                        CreatSubTVItemFromTVItem(aimItem);
                    foreach (var i in aimItem.Items) {
                        if (((MyTreeViewItem)i).HeaderText == dirs[j]) {
                            aimItem = i as MyTreeViewItem;
                            break;
                        }
                    }
                }
            }
            if (changeSelected == true) {
                (tv.SelectedItem as MyTreeViewItem).IsSelected = false;
                aimItem.IsSelected = true;
            }
            tv.EndInit();
            return aimItem;
        }

        /// <summary>
        /// 判断是否以管理员权限运行
        /// </summary>
        /// <returns></returns>
        public static bool IsRunAsAdmin() {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// 返回treeview的根结点
        /// </summary>
        /// <returns></returns>
        public static MyTreeViewItem GetRootTVItem() {
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

        /// <summary>
        /// 根据FileInfo数组更新控件listbox
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="files"></param>
        public static void UpdateByFileInfos(ListBox lb, FileInfo[] files) {
            lb.BeginInit();
            foreach (var i in files) {
                var it = new MyListBoxItem(i.Name, MyIcons.file);
                it.MyMessage = "所在路径:" + i.DirectoryName;
                lb.Items.Add(it);
            }
            lb.EndInit();
        }

        /// <summary>
        /// 根据DirectoryInfo数组更新控件listbox
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="dir"></param>
        public static void UpdateByDirectoryInfos(ListBox lb, DirectoryInfo dir) {
            lb.BeginInit();
            var it = new MyListBoxItem(dir.Name, MyIcons.folder);
            it.MyMessage = "目录路径:" + dir.FullName;
            lb.Items.Add(it);
            lb.EndInit();
        }

        /// <summary>
        /// 调用主线程更新文件信息到listbox
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="files">文件信息</param>
        public static void UpdateByMainThread(ListBox lb, FileInfo[] files) {
            lb.Dispatcher.BeginInvoke(
                new Action<ListBox, FileInfo[]>(UpdateByFileInfos),
                lb, files);
        }

        /// <summary>
        /// 调用主线程更新目录信息到listbox
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="dir">路径信息</param>
        public static void UpdateByMainThread(ListBox lb, DirectoryInfo dir) {
            lb.Dispatcher.BeginInvoke(
                new Action<ListBox, DirectoryInfo>(UpdateByDirectoryInfos),
                lb, dir);
        }

        /// <summary>
        /// 由CurrentPath变量刷新listbox
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="CurrentPath"></param>
        public static void FlushLBByCurrentPath(ListBox lb, string CurrentPath) {
            lb.BeginInit();
            var list = CreatLBItemFromPath(CurrentPath);
            lb.Items.Clear();
            foreach (var i in list)
                lb.Items.Add(i);
            lb.EndInit();
        }
    }

    /// <summary>
    /// 查找管理,提供多线程查找操作
    /// </summary>
    public class MyFindManager {
        /// <summary>
        /// 4线程查找并把结果放入控件的异步方法
        /// </summary>
        /// <param name="lb">listbox控件</param>
        /// <param name="path">查找的起始路径</param>
        /// <param name="ct">用于安全结束线程的对象</param>
        /// <param name="pattern">查找的文件或目录名</param>
        /// <returns></returns>
        public static async Task AddLBItemByThreads(ListBox lb, string path, CancellationToken ct, string pattern) {
            Task t = new Task(() =>
            {
                var dirInfo = new DirectoryInfo(path);
                var _files = dirInfo.GetFiles(pattern);
                var _dirs = dirInfo.GetDirectories();
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern.Replace("*", ".*"));

                //Thread.Sleep(2000);
                MyItemManager.UpdateByMainThread(lb, _files);
                Queue<DirectoryInfo> quedir = new Queue<DirectoryInfo>();
                try {
                    foreach (var i in _dirs) {
                        quedir.Enqueue(i);
                        if (reg.IsMatch(i.Name)) {
                            MyItemManager.UpdateByMainThread(lb, i);
                        }
                        ct.ThrowIfCancellationRequested();
                    }
                    int j = 2;
                    while (j > 0 && quedir.Count != 0) {
                        var p = quedir.Dequeue();
                        Task tt = AddLBItemAnsyc(lb, p.FullName, ct, pattern);
                    }
                    while (quedir.Count != 0) {
                        //Thread.Sleep(2000);
                        ct.ThrowIfCancellationRequested();
                        var dir = quedir.Dequeue();
                        var files = dir.GetFiles(pattern);
                        foreach (var ii in _dirs) {
                            if (reg.IsMatch(ii.Name)) {
                                MyItemManager.UpdateByMainThread(lb, ii);
                            }
                            ct.ThrowIfCancellationRequested();
                        }
                        MyItemManager.UpdateByMainThread(lb, files);
                    }
                }
                catch (Exception) {

                }
            });
            t.Start();
            await t;
        }

        /// <summary>
        /// 查找并把结果放入控件的普通方法
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="path"></param>
        /// <param name="ct"></param>
        /// <param name="pattern"></param>
        public static void AddLBItem(ListBox lb, string path, CancellationToken ct, string pattern) {
            var dirInfo = new DirectoryInfo(path);
            var _files = dirInfo.GetFiles(pattern);
            var _dirs = dirInfo.GetDirectories();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern.Replace("*", ".*"));

            Queue<DirectoryInfo> quedir = new Queue<DirectoryInfo>();
            foreach (var i in _dirs) {
                quedir.Enqueue(i);
                if (reg.IsMatch(i.Name)) {
                    MyItemManager.UpdateByMainThread(lb, i);
                }
                ct.ThrowIfCancellationRequested();
            }
            //Thread.Sleep(2000);
            MyItemManager.UpdateByMainThread(lb, _files);

            while (quedir.Count != 0) {
                //Thread.Sleep(2000);
                ct.ThrowIfCancellationRequested();
                var dir = quedir.Dequeue();
                var dirs = dir.GetDirectories();
                var files = dir.GetFiles(pattern);
                foreach (var i in dirs) {
                    if (reg.IsMatch(i.Name)) {
                        MyItemManager.UpdateByMainThread(lb, i);
                    }
                    ct.ThrowIfCancellationRequested();
                }
                MyItemManager.UpdateByMainThread(lb, files);
            }
        }

        /// <summary>
        /// 新开1个线程查找并把结果放入控件的异步方法
        /// </summary>
        /// <param name="lb"></param>
        /// <param name="path"></param>
        /// <param name="ct"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static async Task AddLBItemAnsyc(ListBox lb, string path, CancellationToken ct, string pattern) {
            Task t = new Task(() =>
            {
                try {
                    AddLBItem(lb, path, ct, pattern);
                }
                catch (Exception) {

                }
            });
            t.Start();
            await t;
        }
    }

    /// <summary>
    /// 复制管理，提供复制操作
    /// </summary>
    public class MyCopyManager {
        /// <summary>
        /// 复制文件，已处理各种意外
        /// </summary>
        /// <param name="srcfile">源文件</param>
        /// <param name="destpath">目的路径</param>
        public static void CopyFile(string srcfile, string destpath) {
            try {
                FileInfo file = new FileInfo(srcfile);
                if (file.DirectoryName == destpath) return;//相同路径无需复制
                if (destpath.Length > 3) {//当前路径加斜杠
                    destpath += "\\";
                }
                //目标目录文件已出现
                if (File.Exists(destpath + file.Name)) {
                    var confirm = MessageBox.Show(file.Name + " 已经存在，是否替换?", "复制文件提示",
                        MessageBoxButton.YesNo);
                    if (confirm == MessageBoxResult.Yes) {
                        (new FileInfo(srcfile)).CopyTo(destpath + file.Name, true);
                        return;
                    }
                }
                else {//目标目录文件未出现
                    (new FileInfo(srcfile)).CopyTo(destpath + file.Name, true);
                }
            }
            catch (IOException) {
                MessageBox.Show("复制出错，可能文件被占用?", "复制错误", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// 复制目录，已处理各种意外
        /// </summary>
        /// <param name="srcdir">源路径</param>
        /// <param name="destpath">目的路径</param>
        public static void CopyFolder(string srcdir, string destpath) {
            try {
                DirectoryInfo src_dir = new DirectoryInfo(srcdir);
                if (destpath.Length > 3) {//当前路径加斜杠
                    destpath += "\\";
                }

                //获取目标路径(创建)目录
                if (!Directory.Exists(destpath + src_dir.Name))
                    Directory.CreateDirectory(destpath + src_dir.Name);
                destpath += src_dir.Name;
                //复制文件
                var files = Directory.GetFiles(srcdir);
                foreach (var i in files)
                    CopyFile(i, destpath);
                //复制子文件夹
                var dirs = Directory.GetDirectories(srcdir);
                foreach (var i in dirs)
                    CopyFolder(i, destpath);
            }
            catch (IOException e) {
                MessageBox.Show(e.Message, "复制错误", MessageBoxButton.OK);
            }
        }
    }
}
