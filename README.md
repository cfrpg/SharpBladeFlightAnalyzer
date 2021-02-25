# SharpBladeFlightAnalyzer

PX4日志分析工具，支持.ulog文件。

## 使用方法

随便点，崩了就发issue报bug。

## 配置文件

### Fields.csv

暂时弃用

### Quaternions.txt

四元数处理器，每一行为一条记录，`//`开头的行为注释。每一条记录为空格分隔的两部分，分别为处理前数据的全名和处理后的字段名前缀，若无前缀则留空。

例如：

`vehicle_attitude.q` 将
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

`vehicle_attitude_setpoint.q_d d` 将
```
vehicle_attitude_setpoint.q_d[0]
vehicle_attitude_setpoint.q_d[1]
vehicle_attitude_setpoint.q_d[2]
vehicle_attitude_setpoint.q_d[3]
```
处理为
```
vehicle_attitude_setpoint.d.roll
vehicle_attitude_setpoint.d.pitch
vehicle_attitude_setpoint.d.yaw
```
