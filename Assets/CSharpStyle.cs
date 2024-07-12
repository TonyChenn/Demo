using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

[APIInfo("C#编码规范", "程序集/类", @"
# 命名空间(namespace)

# 类(class)
1. 类的命名以 'Pascal命名法' 命名。
2. 每个花括号必须单独一行。对于没有任何内容的类括号与类名放在一行。
3. 
```csharp
public class Test1{}

public class Test2
{
	public int Age;
}
```


# 接口(interface)
- 以大写字母 'I' 开头。
```csharp
public interface ICSharpInterfaceStyle { }
```
")]
public interface ICSharpInterfaceStyle { }

[APIInfo("C#编码规范", "字段(Field)", @"
# 常量
- 以 'readonly' 修饰的常量使用 'Pascal命名法' 命名。
- 以 'const' 修饰的变量使用全大写命名，单词之间使用下划线'_'分割。如下：
```csharp
public readonly int MaxValue = 15;
public const int MAX_ROW_COUNT = 15;
```


# 私有变量(private/ protected)
以 '驼峰命名法' 命名。对于命名最好以：类型+名称。
如下：
```csharp
[SerializeField] Transform tranHome;
[SerializeField] Button btnEnter;
```


# 公有变量(public)
以 'Pascal命名法' 命名。对于命名最好以：类型+名称，如下:
```csharp
// 公有变量
public VersionInfo VerInfo;
public List<int> DataList;
```
")]
public interface ICSharpFieldStyle { }

[APIInfo("C#编码规范", "属性(Property)", @"
- 对于直接指向一个变量、只有 'get/set' 的写在一行。
- 对于get/set 块中有内容的:
	1. 只有一句话就返回的可以写一行。
	2. 超过一句的就多行写。

- 对于内部需要记录变量的字段，需要以 '_' 开头。
```csharp
private UpdateStatus _state;

public UpdateStatus CurUpdateState1 => _state;
public UpdateStatus CurUpdateState2 { get; set; }
public UpdateStatus CurUpdateState3 { get; private set; }
public UpdateStatus CurUpdateState4
{
	get { _player ??= new GameObject(); return _player; }
	set { _player = value; }
}
public UpdateStatus CurUpdateState5
{
	get
	{
		if(_player == null)
			_player = new GameObject(); 
		return _player; 
	}
	set
	{
		if(value != null)
			_player = value;
	}
}
```
")]
public interface ICSharpPropertyStyle { }

[APIInfo("C#编码规范","方法(Method)","")]
public interface ICSharpMethodStyle { }
