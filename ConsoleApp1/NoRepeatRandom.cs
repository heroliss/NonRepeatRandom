using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRandom
{
    /// <summary>
    /// 不重复随机数生成器
    /// </summary>
    class NoRepeatRandom : IEnumerable<int>
    {
        public enum Mode { 自动选择, 剔除模式, 添加模式 }

        private Mode mode; //默认使用添加模式
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
        public NoRepeatRandom(int minValue, int maxValue, Mode mode = Mode.自动选择)
        {
            if (maxValue < minValue)
            {
                throw new ApplicationException("minValue不能大于maxValue！");
            }
            this.mode = mode;
            this.minValue = minValue;
            this.maxValue = maxValue;
            totalCount = maxValue - minValue;
            if (mode == Mode.自动选择)
            {
                mode = Mode.添加模式;
                reset();
                mode = Mode.自动选择;
            }
            else
            {
                reset();
            }
        }
        public void reset()
        {
            currentCount = 0;
            switch (mode)
            {
                case Mode.剔除模式:
                    currentRandomByDeleteMode = new RandomByDeleteMode(minValue, maxValue);
                    currentIEnumerator = currentRandomByDeleteMode.GetEnumerator();
                    break;
                case Mode.自动选择:
                case Mode.添加模式:
                    currentRandomByAddMode = new RandomByAddMode(minValue, maxValue);
                    currentIEnumerator = currentRandomByAddMode.GetEnumerator();
                    break;
                default:
                    throw new ApplicationException("debug!");
            }
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
            if (mode == Mode.自动选择 && currentCount == 0)
            {
                mode = Mode.剔除模式;
                reset();
                mode = Mode.自动选择;
                return currentIEnumerator;
            }
            else
            {
                return currentIEnumerator;
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
                while (count < total)
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
}
