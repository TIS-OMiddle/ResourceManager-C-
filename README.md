ĿǰMyControlItem.dll����Ҫ����<br>
<hr>
public class MyTreeViewItem : TreeViewItem//treeview�Ľ��<br>
public class MyListBoxItem//listbox��������<br>
MyIcons//ö�ٱ���Ϊ�����������캯���Ĳ�����ѡ��ͼ��<br>
<hr>
public class MyItemManager<br>
�����࣬�ṩ���Զ���������Ķ��ֿ��ƣ�ȫΪ��̬������ͨ����������<br>
<br>
//����·������listbox��Item����<br>
public static List<MyListBoxItem> CreatLBItemFromPath(string path)
<br>
//Ϊ����㴴���ӽ��<br>
public static void CreatSubTVItemFromTVItem(MyTreeViewItem item)
<br>
//����ָ��·����tvItem��������ʱ�Զ�Ϊ��·��������㣬��������changeSelectedΪ��ʱͬʱѡ������<br>
public static MyTreeViewItem GetTVItemFromPath(TreeView tv, string path, bool changeSelected = false)
<br>
//��ȡ�����<br>
public static MyTreeViewItem GetRootTVItem()
<br>
//��ǰ�����Թ���Ա�������ʱ������<br>
public static bool IsRunAsAdmin()