# NonRepeatRandom

C# / .NET Framework 4.6.1 控制台算法实验，实现指定半开区间内不重复的随机整数生成。

## 项目用途

项目包含两种不重复随机数生成思路：

- 预先生成候选序列，再通过 Fisher-Yates 洗牌和索引删除逐个抽取。
- 使用 `Dictionary` 记录已生成值，按需添加并跳过重复结果。

它更适合作为随机数算法、集合去重和性能取舍的学习示例，而不是通用 NuGet 库。

## 当前状态

- 状态：历史算法实验，已冻结
- 主要语言：C#
- 目标框架：.NET Framework 4.6.1
- 默认分支：`master`
- 最近提交：2019-12-07
- 许可证：仓库中未发现许可证文件

当前环境缺少 .NET Framework 4.6.1 的引用程序集，因此尚未在本机完成完整编译验证。

## 运行方式

1. 在 Windows 上安装 Visual Studio 和 .NET Framework 4.6.1 Developer Pack。
2. 打开 `ConsoleApp1.sln`。
3. 还原并生成解决方案。
4. 运行控制台项目。

实际示例入口是 `TestTheRandomClass.Main`：它会输出开始提示和一组 `[0, 1000)` 范围内的不重复整数，然后等待控制台输入。`Program.Main_2` 是单独的性能测试例程，不是默认入口。

## 目录结构

```text
ConsoleApp1.sln
ConsoleApp1/
└── ConsoleApp1.csproj
```

核心随机数实现位于 `ConsoleApp1` 项目中；相关类型目前以 `internal` 为主，若要在其他程序集复用，需要先调整访问级别并补充测试。

## 已知限制

- 项目依赖旧版 .NET Framework，现代 .NET SDK 不能直接替代目标框架。
- 当前仓库没有自动化测试、NuGet 打包配置或许可证声明。
- 本 README 记录的是历史代码的用途和运行线索，不代表项目仍在维护。