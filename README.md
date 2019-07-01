# SharpBladeFlightAnalyzer

PX4日志分析工具，支持.ulog文件。

## 使用方法

随便点，崩了就发issue报bug。

## 配置文件

### Fields.csv

数据名翻译表，4列csv文件，多出的列数会被忽略。每一列分别表示：

数据原名：对应的数据原名。

短名称：如果不为空，在显示时会替换掉原名。

描述：数据的描述。

是否处理：为0表示不处理此数据。

### Quaternions.txt

四元数处理器，每一行为一条记录，`//`开头的行为注释。每一条记录为空格分隔的两部分，分别为数据的原名和处理后的名字。

例如：`vehicle_attitude.q vehicle_attitude` 将
```
vehicle_attitude.q[0]
vehicle_attitude.q[1]
vehicle_attitude.q[2]
vehicle_attitude.q[3]
```
处理为
```
vehicle_attitude.roll
vehicle_attitude.pitch
vehicle_attitude.yaw
```

如需要翻译处理后的数据，将处理后的数据名（如`vehicle_attitude.roll`）作为原名加入翻译表。
