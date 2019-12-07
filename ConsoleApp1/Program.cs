using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ConsoleApp1
{
    /// <summary>
    /// 不重复随机数生成器
    /// </summary>
    class NoRepeatRandom : IEnumerable<int>
    {
        public enum Mode { 剔除模式, 添加模式 }

        private Mode mode;
        private IEnumerator<int> currentIEnumerator;

        private int minValue;
        private int maxValue;

        private int totalCount; //范围内的整数总数
        private int currentCount = 0;

        private RandomByDeleteMode currentRandomByDeleteMode;
        private RandomByAddMode currentRandomByAddMode;

        public Mode CurrentMode { get => mode; }
        public IEnumerator<int> CurrentIEnumerator { get => currentIEnumerator; }
        public int MinValue { get => minValue; }
        public int MaxValue { get => maxValue; }

        /// <summary>
        /// 范围内所有整数总数
        /// </summary>
        public int TotalCount { get => totalCount; }
        /// <summary>
        /// 已取值总数
        /// </summary>
        public int CurrentCount { get => currentCount; }
        public RandomByDeleteMode CurrentRandomByDeleteMode { get => currentRandomByDeleteMode; }
        public RandomByAddMode CurrentRandomByAddMode { get => currentRandomByAddMode; }

        /// <summary>
        /// 不重复随机数生成器构造函数
        /// </summary>
        /// <param name="minValue">最小整数（包含该值）</param>
        /// <param name="maxValue">最大整数（不包含该值）</param>
        /// <param name="mode">若近似满足下面任何一个条件，请用使用“剔除模式”，
        /// 否则推荐使用默认的“添加模式”
        /// 1.要在超过6万个整数的范围内取出超过20%的随机数，
        /// 2.或者在超过1千万个整数的范围内取出超过5%的随机数，
        /// 3.或者要取出500万以上个随机数</param>
        public NoRepeatRandom(int minValue, int maxValue, Mode mode = Mode.添加模式)
        {
            if (maxValue < minValue)
            {
                throw new ApplicationException("minValue不能大于maxValue！");
            }
            this.mode = mode;
            this.minValue = minValue;
            this.maxValue = maxValue;
            totalCount = maxValue - minValue;
            currentIEnumerator = GetEnumerator();
        }
        public void reset()
        {
            currentCount = 0;
            currentIEnumerator = GetEnumerator();
        }
        public int Next()
        {
            if (currentCount >= totalCount)
            {
                throw new ApplicationException("再没有其他不重复随机整数了！（可以通过reset函数重置）");
            }
            currentIEnumerator.MoveNext();
            currentCount++;

            return currentIEnumerator.Current;
        }
        public IEnumerator<int> GetEnumerator()
        {
            switch (mode)
            {
                case Mode.剔除模式:
                    currentRandomByDeleteMode = new RandomByDeleteMode(minValue, maxValue);
                    return currentRandomByDeleteMode.GetEnumerator();

                case Mode.添加模式:
                    currentRandomByAddMode = new RandomByAddMode(minValue, maxValue);
                    return currentRandomByAddMode.GetEnumerator();

                default:
                    throw new ApplicationException("debug!");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public class RandomByDeleteMode  //剔除模式随机数生成器
        {
            private Random random;
            private int total;
            private int[] sequence;  //所有可能取值的列表
            public RandomByDeleteMode(int minValue, int maxValue)
            {
                total = maxValue - minValue;
                random = new Random();
                sequence = new int[total];
                for (int i = 0; i < total; i++)   //填充列表
                {
                    sequence[i] = i + minValue;
                }
            }

            public IEnumerator<int> GetEnumerator()
            {
                int endIndex = total - 1;  //最后一个元素索引
                while (endIndex >= 0)
                {
                    int index = random.Next(0, endIndex + 1);
                    yield return sequence[index];
                    sequence[index] = sequence[endIndex];
                    endIndex--;
                }
            }
        }

        public class RandomByAddMode  //添加模式随机数生成器
        {
            private Random random;
            private Dictionary<int, bool> dictionary; //所有已取值的哈希表
            private int minValue;
            private int maxValue;
            private int total; //范围内所有整数总数
            private int count; //已取值总数
            private int conflictCount; // 取值冲突次数
            public int conflictThreshold; //冲突阈值，当冲突次数超过此值则停止计算

            public int ConflictCount { get => conflictCount; }

            public RandomByAddMode(int minValue, int maxValue, int conflictThreshold = 9999999)
            {
                this.minValue = minValue;
                this.maxValue = maxValue;
                this.conflictThreshold = conflictThreshold;
                total = maxValue - minValue;
                random = new Random();
                dictionary = new Dictionary<int, bool>();
            }

            public IEnumerator<int> GetEnumerator()
            {
                while (count <= total)
                {
                    int num = random.Next(minValue, maxValue);
                    if (!dictionary.ContainsKey(num)) //没有取过该值
                    {
                        dictionary[num] = true;
                        count++;
                        yield return num;
                    }
                    else
                    {
                        conflictCount++;
                        if (conflictCount > conflictThreshold)
                        {
                            throw new ApplicationException("冲突次数超过阈值！");
                        }
                    }
                }
            }
        }
    }
    class Program
    {
        static void Main_2(string[] args)
        {
            Console.WriteLine("按任意键开始测试...");
            Console.ReadKey(true);
            StreamWriter file = new StreamWriter("性能分析报告-添加模式.txt", false);
            StreamWriter file2 = new StreamWriter("性能分析报告-剔除模式.txt", false);
            StreamWriter conflictFile = new StreamWriter("冲突记录.txt");
            file.AutoFlush = true;
            file2.AutoFlush = true;
            conflictFile.AutoFlush = true;

            Stopwatch stopwatch = new Stopwatch();
            int topTotalPower = 22;
            int topRandomPower = 22;

            createTable(file, stopwatch, topTotalPower, topRandomPower, NoRepeatRandom.Mode.添加模式, conflictFile);
            Console.WriteLine();
            createTable(file2, stopwatch, topTotalPower, topRandomPower, NoRepeatRandom.Mode.剔除模式);
            Console.WriteLine();
            //createTable(file, stopwatch, topTotalPower, topRandomPower, NoRepeatRandom.Mode.自动选择);

            file.Close();
            file2.Close();
            conflictFile.Close();
            Console.WriteLine("完成！");
            Console.Read();
        }

        private static void createTable(StreamWriter file, Stopwatch stopwatch, int topTotalPower, int topRandomPower, NoRepeatRandom.Mode mode, StreamWriter conflictFile = null)
        {
            int processCount = 0;
            file.WriteLine("{0,40}-性能分析报告(单位：ms)", mode.ToString());
            file.WriteLine();
            if (conflictFile != null)
            {
                conflictFile.WriteLine("{0,40}-冲突报告(单位：次)", mode.ToString());
                conflictFile.WriteLine();
            }

            //表头
            file.Write("{0,12}", "随机数\\总数(2的次幂)");
            for (int i = 0; i <= topTotalPower; i++)
            {
                file.Write("{0,10}", i);
            }
            file.WriteLine();

            if (conflictFile != null)
            {
                conflictFile.Write("{0,12}", "随机数\\总数(2的次幂)");
                for (int i = 0; i <= topTotalPower; i++)
                {
                    conflictFile.Write("{0,10}", i);
                }
                conflictFile.WriteLine();
            }



            for (int i = 0; i <= topRandomPower; i++)
            {
                file.Write("{0,-20}", i); //列表头
                if (conflictFile != null)
                {
                    conflictFile.Write("{0,-20}", i); //列表头
                }

                for (int j = 0; j <= topTotalPower; j++)
                {
                    if (i > j)
                    {
                        file.Write("{0,10}", "-");
                        if (conflictFile != null)
                        {
                            conflictFile.Write("{0,10}", "-");
                        }
                        processCount++;
                        continue;
                    }
                    NoRepeatRandom noRepeatRandom = null;
                    string errMessage = "";
                    try
                    {
                        stopwatch.Restart();
                        noRepeatRandom = new NoRepeatRandom(0, (int)Math.Pow(2, j), mode);
                        for (int n = 0; n < (int)Math.Pow(2, i); n++)
                        {
                            noRepeatRandom.Next();
                        }
                    }
                    catch (ApplicationException e)
                    {
                        if (e.Message == "冲突次数超过阈值！")
                        {
                            errMessage = "HCC "; //high conflict count 高冲突
                        }
                        else
                            errMessage = "ERR "; //应用错误（minValue>maxValue | 已取得所有值）
                    }
                    catch (OutOfMemoryException e)
                    {
                        errMessage = "OOM ";
                    }
                    finally
                    {
                        stopwatch.Stop();
                        file.Write("{0,10}", errMessage + stopwatch.ElapsedMilliseconds.ToString());
                    }

                    if (conflictFile != null &&
                        noRepeatRandom != null &&
                        noRepeatRandom.CurrentRandomByAddMode != null)
                    {
                        conflictFile.Write("{0,10}",errMessage + 
                            string.Format(errMessage=="" ? "{0}" : "{0:g1}",
                            noRepeatRandom.CurrentRandomByAddMode.ConflictCount));
                    }
                    processCount++;
                    showProsecc((topTotalPower + 1) * (topRandomPower + 1), processCount);
                }
                file.WriteLine();
                if (conflictFile != null)
                {
                    conflictFile.WriteLine();
                }

            }
        }

        private static void showProsecc(int totalCount, int processCount)
        {
            float percent = (float)processCount / totalCount;
            Console.SetCursorPosition(1, Console.CursorTop);
            Console.Write("{0:0.00%}", percent);
        }
    }
}
