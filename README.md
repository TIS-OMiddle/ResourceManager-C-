目前MyControlItem.dll里主要的类<br>
<hr>
public class MyTreeViewItem : TreeViewItem//treeview的结点<br>
public class MyListBoxItem//listbox的数据项<br>
MyIcons//枚举变量为上面两个构造函数的参数，选择图标<br>
<hr>
public class MyItemManager<br>
管理类，提供对自定义数据类的多种控制，全为静态函数，通过类名调用<br>
<br>
//根据路径返回listbox的Item集合<br>
public static List<MyListBoxItem> CreatLBItemFromPath(string path)
<br>
//为树结点创建子结点<br>
public static void CreatSubTVItemFromTVItem(MyTreeViewItem item)
<br>
//返回指定路径的tvItem，不存在时自动为该路径创建结点，第三参数changeSelected为真时同时选定该项<br>
public static MyTreeViewItem GetTVItemFromPath(TreeView tv, string path, bool changeSelected = false)
<br>
//获取根结点<br>
public static MyTreeViewItem GetRootTVItem()
<br>
//当前程序以管理员身份运行时返回真<br>
public static bool IsRunAsAdmin()