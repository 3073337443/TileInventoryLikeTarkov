# C# 知识点全面总结

## 一、基础语法

### 1. 数据类型

```csharp
// 值类型
int age = 25;                    // 整数 (32位)
long bigNumber = 9999999999L;    // 长整数 (64位)
float height = 1.75f;            // 单精度浮点数
double weight = 70.5;            // 双精度浮点数
decimal price = 99.99m;          // 高精度小数 (适合金融计算)
bool isActive = true;            // 布尔值
char grade = 'A';                // 字符

// 引用类型
string name = "张三";            // 字符串
object obj = new object();       // 所有类型的基类
int[] numbers = {1, 2, 3};       // 数组

// 可空类型
int? nullableInt = null;         // 可以为null的int
```

### 2. 变量与常量

```csharp
// 变量声明
var message = "Hello";           // 类型推断
int count;                       // 先声明后赋值
count = 10;

// 常量
const double PI = 3.14159;       // 编译时常量，不可修改
readonly int maxSize = 100;      // 运行时常量 (只能在字段中使用)
```

### 3. 运算符

```csharp
// 算术运算符
int sum = 10 + 5;       // 加法: 15
int diff = 10 - 5;      // 减法: 5
int product = 10 * 5;   // 乘法: 50
int quotient = 10 / 3;  // 整数除法: 3
int remainder = 10 % 3; // 取模: 1

// 比较运算符
bool isEqual = (5 == 5);    // true
bool isGreater = (10 > 5);  // true

// 逻辑运算符
bool result = true && false; // AND: false
bool result2 = true || false; // OR: true
bool result3 = !true;         // NOT: false

// 空合并运算符
string text = null;
string display = text ?? "默认值";  // 如果text为null，使用"默认值"

// 空条件运算符
string upper = text?.ToUpper();     // 如果text为null，返回null而不是抛异常

// 三元运算符
string status = age >= 18 ? "成年" : "未成年";
```

---

## 二、控制流程

### 1. 条件语句

```csharp
// if-else
int score = 85;
if (score >= 90)
{
    Console.WriteLine("优秀");
}
else if (score >= 60)
{
    Console.WriteLine("及格");
}
else
{
    Console.WriteLine("不及格");
}

// switch 语句
string day = "Monday";
switch (day)
{
    case "Monday":
        Console.WriteLine("星期一");
        break;
    case "Tuesday":
        Console.WriteLine("星期二");
        break;
    default:
        Console.WriteLine("其他");
        break;
}

// switch 表达式 (C# 8.0+)
string dayName = day switch
{
    "Monday" => "星期一",
    "Tuesday" => "星期二",
    _ => "其他"
};
```

### 2. 循环语句

```csharp
// for 循环
for (int i = 0; i < 5; i++)
{
    Console.WriteLine(i);
}

// foreach 循环
int[] nums = {1, 2, 3, 4, 5};
foreach (int num in nums)
{
    Console.WriteLine(num);
}

// while 循环
int counter = 0;
while (counter < 5)
{
    Console.WriteLine(counter);
    counter++;
}

// do-while 循环 (至少执行一次)
do
{
    Console.WriteLine(counter);
    counter--;
} while (counter > 0);

// 循环控制
for (int i = 0; i < 10; i++)
{
    if (i == 3) continue;  // 跳过本次迭代
    if (i == 7) break;     // 退出循环
    Console.WriteLine(i);
}
```

---

## 三、面向对象编程 (OOP)

### 1. 类与对象

```csharp
// 类的定义
public class Person
{
    // 字段 (私有)
    private string _name;
    private int _age;

    // 属性 (封装字段)
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    // 自动属性 (简化写法)
    public int Age { get; set; }

    // 只读属性
    public string Id { get; } = Guid.NewGuid().ToString();

    // 构造函数
    public Person()
    {
        Name = "Unknown";
        Age = 0;
    }

    // 带参数的构造函数
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }

    // 方法
    public void Introduce()
    {
        Console.WriteLine($"我是{Name}，今年{Age}岁");
    }

    // 静态成员
    public static int Population { get; private set; } = 0;

    public static void IncrementPopulation()
    {
        Population++;
    }
}

// 创建对象
Person person1 = new Person();
Person person2 = new Person("李四", 30);
person2.Introduce();
```


### 2. 继承

```csharp
// 基类
public class Animal
{
    public string Name { get; set; }

    public virtual void Speak()  // virtual 允许子类重写
    {
        Console.WriteLine("动物发出声音");
    }

    public void Eat()
    {
        Console.WriteLine($"{Name}正在吃东西");
    }
}

// 派生类
public class Dog : Animal
{
    public string Breed { get; set; }

    // 重写基类方法
    public override void Speak()
    {
        Console.WriteLine("汪汪汪！");
    }

    // 子类特有方法
    public void Fetch()
    {
        Console.WriteLine($"{Name}正在捡球");
    }
}

// 使用
Dog dog = new Dog { Name = "旺财", Breed = "金毛" };
dog.Speak();  // 汪汪汪！
dog.Eat();    // 旺财正在吃东西
```

### 3. 多态

```csharp
// 多态示例
Animal animal1 = new Dog { Name = "小黑" };
Animal animal2 = new Cat { Name = "咪咪" };

animal1.Speak();  // 汪汪汪！ (调用Dog的Speak)
animal2.Speak();  // 喵喵喵！ (调用Cat的Speak)

// 向下转型
if (animal1 is Dog dog)
{
    dog.Fetch();  // 安全地调用Dog特有方法
}

// as 转型
Dog myDog = animal1 as Dog;  // 如果转型失败返回null
```

### 4. 抽象类

```csharp
// 抽象类不能被实例化
public abstract class Shape
{
    public string Color { get; set; }

    // 抽象方法 - 子类必须实现
    public abstract double GetArea();

    // 普通方法 - 子类可以继承
    public void Display()
    {
        Console.WriteLine($"这是一个{Color}的图形，面积为{GetArea()}");
    }
}

public class Circle : Shape
{
    public double Radius { get; set; }

    public override double GetArea()
    {
        return Math.PI * Radius * Radius;
    }
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public override double GetArea()
    {
        return Width * Height;
    }
}
```

### 5. 接口

```csharp
// 接口定义契约
public interface IMovable
{
    void Move(int x, int y);
    int Speed { get; set; }
}

public interface IAttackable
{
    void Attack(IMovable target);
    int Damage { get; }
}

// 类可以实现多个接口
public class Player : IMovable, IAttackable
{
    public int Speed { get; set; } = 10;
    public int Damage { get; } = 25;

    public void Move(int x, int y)
    {
        Console.WriteLine($"玩家移动到 ({x}, {y})");
    }

    public void Attack(IMovable target)
    {
        Console.WriteLine($"玩家发起攻击，造成{Damage}点伤害");
    }
}

// 接口作为参数类型
public void ProcessMovable(IMovable entity)
{
    entity.Move(100, 200);
}
```

### 6. 封装与访问修饰符

```csharp
public class BankAccount
{
    // private: 仅类内部可访问
    private decimal _balance;

    // protected: 类内部和子类可访问
    protected string AccountType;

    // internal: 同一程序集可访问
    internal string BankName;

    // public: 任何地方都可访问
    public string AccountNumber { get; }

    // protected internal: 同一程序集或子类可访问
    protected internal void LogTransaction() { }

    // private protected: 同一程序集中的子类可访问
    private protected void InternalProcess() { }

    public decimal GetBalance()
    {
        return _balance;  // 通过方法安全地访问私有字段
    }

    public void Deposit(decimal amount)
    {
        if (amount > 0)
            _balance += amount;
    }
}
```

---

## 四、高级特性

### 1. 泛型

```csharp
// 泛型类
public class Box<T>
{
    private T _item;

    public void Put(T item)
    {
        _item = item;
    }

    public T Get()
    {
        return _item;
    }
}

// 使用泛型类
Box<int> intBox = new Box<int>();
intBox.Put(123);
int value = intBox.Get();

Box<string> stringBox = new Box<string>();
stringBox.Put("Hello");

// 泛型方法
public T Max<T>(T a, T b) where T : IComparable<T>
{
    return a.CompareTo(b) > 0 ? a : b;
}

// 泛型约束
public class Repository<T> where T : class, new()  // T必须是引用类型且有无参构造函数
{
    public T Create()
    {
        return new T();
    }
}

// 常用泛型集合
List<string> names = new List<string> { "张三", "李四" };
Dictionary<string, int> scores = new Dictionary<string, int>
{
    { "张三", 90 },
    { "李四", 85 }
};
```


### 2. 委托与事件
```csharp
// 委托定义
public delegate void MessageHandler(string message);

// 使用委托
public class Messenger
{
    public MessageHandler OnMessage;
    
    public void SendMessage(string msg)
    {
        OnMessage?.Invoke(msg);  // 调用委托
    }
}

// 内置委托类型
Action<string> print = msg => Console.WriteLine(msg);  // 无返回值
Func<int, int, int> add = (a, b) => a + b;             // 有返回值
Predicate<int> isEven = n => n % 2 == 0;               // 返回bool

// 事件 (比委托更安全)
public class Button
{
    // 事件只能在类内部触发
    public event EventHandler<ClickEventArgs> Clicked;
    
    protected virtual void OnClicked(ClickEventArgs e)
    {
        Clicked?.Invoke(this, e);
    }
    
    public void Click()
    {
        OnClicked(new ClickEventArgs { ClickTime = DateTime.Now });
    }
}

public class ClickEventArgs : EventArgs
{
    public DateTime ClickTime { get; set; }
}

// 订阅事件
Button button = new Button();
button.Clicked += (sender, e) => Console.WriteLine($"按钮被点击于 {e.ClickTime}");
```

### 3. Lambda 表达式
```csharp
// Lambda 基本语法
Func<int, int> square = x => x * x;
Func<int, int, int> multiply = (a, b) => a * b;

// 多语句Lambda
Func<int, string> describe = x =>
{
    if (x > 0) return "正数";
    if (x < 0) return "负数";
    return "零";
};

// 在LINQ中使用Lambda
List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6 };
var evenNumbers = numbers.Where(n => n % 2 == 0);
var doubled = numbers.Select(n => n * 2);
```

### 4. LINQ
```csharp
List<Student> students = new List<Student>
{
    new Student { Name = "张三", Age = 20, Score = 85 },
    new Student { Name = "李四", Age = 22, Score = 90 },
    new Student { Name = "王五", Age = 21, Score = 78 }
};

// 查询语法
var query1 = from s in students
             where s.Score >= 80
             orderby s.Score descending
             select new { s.Name, s.Score };

// 方法语法 (更常用)
var query2 = students
    .Where(s => s.Score >= 80)
    .OrderByDescending(s => s.Score)
    .Select(s => new { s.Name, s.Score });

// 常用LINQ方法
var first = students.First();                    // 第一个元素
var firstOrDefault = students.FirstOrDefault();  // 第一个或默认值
var any = students.Any(s => s.Score > 90);       // 是否存在
var all = students.All(s => s.Age >= 18);        // 是否全部满足
var count = students.Count();                    // 数量
var average = students.Average(s => s.Score);    // 平均值
var sum = students.Sum(s => s.Score);            // 总和
var max = students.Max(s => s.Score);            // 最大值
var grouped = students.GroupBy(s => s.Age);      // 分组
```

### 5. 异步编程 (async/await)
```csharp
// 异步方法
public async Task<string> DownloadDataAsync(string url)
{
    using (HttpClient client = new HttpClient())
    {
        string data = await client.GetStringAsync(url);
        return data;
    }
}

// 调用异步方法
public async Task ProcessDataAsync()
{
    string result = await DownloadDataAsync("https://api.example.com");
    Console.WriteLine(result);
}

// 并行执行多个异步任务
public async Task DownloadMultipleAsync()
{
    Task<string> task1 = DownloadDataAsync("url1");
    Task<string> task2 = DownloadDataAsync("url2");
    
    string[] results = await Task.WhenAll(task1, task2);
}

// 异步返回类型
public async Task DoWorkAsync() { }           // 无返回值
public async Task<int> GetValueAsync() { }    // 返回int
public async ValueTask<int> GetFastAsync() { } // 值类型Task，性能更好
```

## 五、异常处理
``` csharp
// 基本try-catch-finally
try
{
    int result = 10 / 0;  // 会抛出异常
}
catch (DivideByZeroException ex)
{
    Console.WriteLine($"除零错误: {ex.Message}");
}
catch (Exception ex)  // 捕获所有其他异常
{
    Console.WriteLine($"发生错误: {ex.Message}");
}
finally
{
    // 无论是否发生异常都会执行
    Console.WriteLine("清理资源");
}

// 抛出异常
public void ValidateAge(int age)
{
    if (age < 0)
        throw new ArgumentException("年龄不能为负数", nameof(age));
    
    if (age > 150)
        throw new ArgumentOutOfRangeException(nameof(age), "年龄超出合理范围");
}

// 自定义异常
public class InsufficientFundsException : Exception
{
    public decimal RequestedAmount { get; }
    public decimal AvailableBalance { get; }
    
    public InsufficientFundsException(decimal requested, decimal available)
        : base($"余额不足。请求: {requested}, 可用: {available}")
    {
        RequestedAmount = requested;
        AvailableBalance = available;
    }
}

// 异常过滤器 (C# 6.0+)
try
{
    // 代码
}
catch (Exception ex) when (ex.Message.Contains("特定错误"))
{
    // 只捕获包含特定信息的异常
}
```

## 六、集合与数据结构
``` csharp
// 数组
int[] arr = new int[5];
int[] arr2 = { 1, 2, 3, 4, 5 };
int[,] matrix = new int[3, 3];  // 二维数组
int[][] jagged = new int[3][];  // 交错数组

// List - 动态数组
List<string> list = new List<string>();
list.Add("item1");
list.AddRange(new[] { "item2", "item3" }); // 批量添加
list.Remove("item1");
list.RemoveAt(0);
bool contains = list.Contains("item2");

// Dictionary - 键值对
Dictionary<string, int> dict = new Dictionary<string, int>();
dict["key1"] = 100;
dict.Add("key2", 200);
bool hasKey = dict.ContainsKey("key1");
if (dict.TryGetValue("key1", out int value))
{
    Console.WriteLine(value);
}

// HashSet - 无重复元素集合
HashSet<int> set = new HashSet<int> { 1, 2, 3 };
set.Add(3);  // 不会添加重复元素
set.UnionWith(new[] { 4, 5 });

// Queue - 先进先出
Queue<string> queue = new Queue<string>();
queue.Enqueue("first");
queue.Enqueue("second");
string first = queue.Dequeue();  // "first"

// Stack - 后进先出
Stack<string> stack = new Stack<string>();
stack.Push("bottom");
stack.Push("top");
string top = stack.Pop();  // "top"

// LinkedList - 双向链表
LinkedList<int> linkedList = new LinkedList<int>();
linkedList.AddFirst(1);
linkedList.AddLast(3);
linkedList.AddAfter(linkedList.First, 2);
```

## 七、字符串操作
``` csharp
string str = "Hello, World!";

// 常用方法
int length = str.Length;                    // 长度
string upper = str.ToUpper();               // 转大写
string lower = str.ToLower();               // 转小写
string trimmed = str.Trim();                // 去除首尾空格
bool contains = str.Contains("World");      // 是否包含
bool starts = str.StartsWith("Hello");      // 是否以...开头
bool ends = str.EndsWith("!");              // 是否以...结尾
int index = str.IndexOf("o");               // 查找位置
string replaced = str.Replace("World", "C#"); // 替换
string[] parts = str.Split(',');            // 分割
string sub = str.Substring(0, 5);           // 截取子串

// 字符串插值 (推荐)
string name = "张三";
int age = 25;
string message = $"我叫{name}，今年{age}岁";

// 字符串格式化
string formatted = string.Format("我叫{0}，今年{1}岁", name, age);
string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

// StringBuilder (高性能字符串拼接)
StringBuilder sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
{
    sb.Append(i);
    sb.AppendLine();
}
string result = sb.ToString();

// 原始字符串 (C# 11)
string rawString = """
    这是一个
    多行字符串
    不需要转义"引号"
    """;
```

## 八、文件与IO操作
``` csharp
// 文件读写
string content = File.ReadAllText("file.txt");
string[] lines = File.ReadAllLines("file.txt");
File.WriteAllText("output.txt", "Hello");
File.AppendAllText("output.txt", "\nWorld");

// 流式读写 (大文件)
using (StreamReader reader = new StreamReader("large.txt"))
{
    string line;
    while ((line = reader.ReadLine()) != null)
    {
        Console.WriteLine(line);
    }
}

using (StreamWriter writer = new StreamWriter("output.txt"))
{
    writer.WriteLine("第一行");
    writer.WriteLine("第二行");
}

// 文件操作
bool exists = File.Exists("file.txt");
File.Copy("source.txt", "dest.txt");
File.Move("old.txt", "new.txt");
File.Delete("file.txt");

// 目录操作
Directory.CreateDirectory("newFolder");
string[] files = Directory.GetFiles("folder", "*.txt");
string[] dirs = Directory.GetDirectories("folder");

// Path工具类
string fileName = Path.GetFileName("c:/folder/file.txt");      // file.txt
string extension = Path.GetExtension("file.txt");              // .txt
string combined = Path.Combine("folder", "subfolder", "file"); // folder/subfolder/file
```

## 九、特性
```csharp
// 使用内置特性
[Obsolete("此方法已过时，请使用NewMethod")]
public void OldMethod() { }

[Serializable]
public class DataModel { }

// 自定义特性
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorAttribute : Attribute
{
    public string Name { get; }
    public string Version { get; set; }
    
    public AuthorAttribute(string name)
    {
        Name = name;
    }
}

// 应用自定义特性
[Author("张三", Version = "1.0")]
public class MyClass
{
    [Author("李四")]
    public void MyMethod() { }
}

// 通过反射读取特性
var attribute = typeof(MyClass).GetCustomAttribute<AuthorAttribute>();
Console.WriteLine(attribute.Name);  // 张三
```

## 十、反射
```csharp
// 获取类型信息
Type type = typeof(Person);
Type type2 = person.GetType();

// 获取属性信息
PropertyInfo[] properties = type.GetProperties();
foreach (var prop in properties)
{
    Console.WriteLine($"{prop.Name}: {prop.PropertyType}");
}

// 获取方法信息
MethodInfo[] methods = type.GetMethods();

// 动态创建实例
object instance = Activator.CreateInstance(type);

// 动态调用方法
MethodInfo method = type.GetMethod("Introduce");
method.Invoke(instance, null);

// 动态设置属性值
PropertyInfo nameProp = type.GetProperty("Name");
nameProp.SetValue(instance, "动态设置的名字");
```
## 十一、结构体与枚举
```csharp
// 结构体 (值类型)
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
    
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public double DistanceTo(Point other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

// 只读结构体 (不可变)
public readonly struct ImmutablePoint
{
    public int X { get; }
    public int Y { get; }
    
    public ImmutablePoint(int x, int y) => (X, Y) = (x, y);
}

// 枚举
public enum DayOfWeek
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}

// 标志枚举
[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    All = Read | Write | Execute
}

// 使用枚举
DayOfWeek today = DayOfWeek.Monday;
Permissions userPerms = Permissions.Read | Permissions.Write;
bool canWrite = (userPerms & Permissions.Write) == Permissions.Write;
```
## 十二、记录类型
```csharp
// Record - 不可变引用类型
public record Person(string Name, int Age);

// 使用
var person1 = new Person("张三", 25);
var person2 = person1 with { Age = 26 };  // 创建副本并修改

// 值相等性
var person3 = new Person("张三", 25);
bool areEqual = person1 == person3;  // true (比较值而非引用)

// Record struct (C# 10)
public record struct Point(int X, int Y);
```
## 十三、模式匹配
```csharp
// is 模式
if (obj is string s)
{
    Console.WriteLine(s.Length);
}

// switch 模式匹配
object value = 42;
string result = value switch
{
    int n when n > 0 => "正整数",
    int n when n < 0 => "负整数",
    int => "零",
    string s => $"字符串: {s}",
    null => "空值",
    _ => "其他类型"
};

// 属性模式
if (person is { Age: >= 18, Name: "张三" })
{
    Console.WriteLine("成年的张三");
}

// 列表模式 (C# 11)
int[] numbers = { 1, 2, 3 };
if (numbers is [1, 2, 3])
{
    Console.WriteLine("匹配 1, 2, 3");
}

if (numbers is [var first, .., var last])
{
    Console.WriteLine($"首: {first}, 尾: {last}");
}
```
## 十四、解构与元组
```csharp
// 元组
var tuple = (Name: "张三", Age: 25);
Console.WriteLine(tuple.Name);

// 方法返回元组
public (int Min, int Max) GetRange(int[] numbers)
{
    return (numbers.Min(), numbers.Max());
}

var range = GetRange(new[] { 1, 5, 3, 9, 2 });
Console.WriteLine($"最小: {range.Min}, 最大: {range.Max}");

// 解构
var (min, max) = GetRange(new[] { 1, 5, 3 });

// 自定义解构
public class Rectangle
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    public void Deconstruct(out int width, out int height)
    {
        width = Width;
        height = Height;
    }
}

var rect = new Rectangle { Width = 100, Height = 50 };
var (w, h) = rect;  // 解构
```
## 十五、扩展方法
```csharp
// 定义扩展方法
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }
    
    public static string Reverse(this string str)
    {
        char[] chars = str.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
    
    public static int WordCount(this string str)
    {
        return str.Split(new[] { ' ', '\t', '\n' }, 
                         StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

// 使用扩展方法
string text = "Hello World";
bool isEmpty = text.IsNullOrEmpty();
string reversed = text.Reverse();
int words = text.WordCount();
```
## 十六、索引器
```csharp
public class DataCollection
{
    private string[] _data = new string[10];
    
    // 通过索引访问
    public string this[int index]
    {
        get => _data[index];
        set => _data[index] = value;
    }
    
    // 通过键访问
    public string this[string key]
    {
        get => _data.FirstOrDefault(d => d?.StartsWith(key) == true);
    }
}

var collection = new DataCollection();
collection[0] = "Hello";
string value = collection[0];
```
## 十七、运算符重载
```csharp
public struct Vector2
{
    public float X { get; }
    public float Y { get; }
    
    public Vector2(float x, float y) => (X, Y) = (x, y);
    
    // 加法运算符
    public static Vector2 operator +(Vector2 a, Vector2 b)
        => new Vector2(a.X + b.X, a.Y + b.Y);
    
    // 减法运算符
    public static Vector2 operator -(Vector2 a, Vector2 b)
        => new Vector2(a.X - b.X, a.Y - b.Y);
    
    // 标量乘法
    public static Vector2 operator *(Vector2 v, float scalar)
        => new Vector2(v.X * scalar, v.Y * scalar);
    
    // 相等比较
    public static bool operator ==(Vector2 a, Vector2 b)
        => a.X == b.X && a.Y == b.Y;
    
    public static bool operator !=(Vector2 a, Vector2 b)
        => !(a == b);
    
    // 隐式转换
    public static implicit operator Vector2((float x, float y) tuple)
        => new Vector2(tuple.x, tuple.y);
}

// 使用
Vector2 v1 = new Vector2(1, 2);
Vector2 v2 = new Vector2(3, 4);
Vector2 sum = v1 + v2;
Vector2 scaled = v1 * 2.5f;
Vector2 fromTuple = (5.0f, 6.0f);  // 隐式转换
```
## 十八、using 与资源管理
```csharp
// using 语句 (自动释放资源)
using (var stream = new FileStream("file.txt", FileMode.Open))
{
    // 使用stream
} // 自动调用 stream.Dispose()

// using 声明 (C# 8.0+)
using var reader = new StreamReader("file.txt");
string content = reader.ReadToEnd();
// reader在作用域结束时自动释放

// 实现 IDisposable
public class ResourceHandler : IDisposable
{
    private bool _disposed = false;
    private IntPtr _handle;
    
    public void DoWork()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ResourceHandler));
        // 工作代码
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 释放托管资源
            }
            // 释放非托管资源
            _disposed = true;
        }
    }
    
    ~ResourceHandler()
    {
        Dispose(false);
    }
}
```
## 十九、nullable 引用类型
```csharp
#nullable enable

// 不可为null的引用类型
string nonNullable = "hello";
// nonNullable = null;  // 编译警告

// 可为null的引用类型
string? nullable = null;

// null 检查
if (nullable != null)
{
    Console.WriteLine(nullable.Length);
}

// 空合并
string value = nullable ?? "默认值";

// 空断言 (告诉编译器这里不会为null)
string definitelyNotNull = nullable!;

// 参数null检查 (C# 11)
public void Process(string name!!)
{
    // 如果name为null会自动抛出ArgumentNullException
}
```