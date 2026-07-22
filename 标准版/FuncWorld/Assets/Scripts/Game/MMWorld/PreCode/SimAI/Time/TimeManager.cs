using UnityEngine;
using System;

namespace MMWorld.SimAI
{
    /// <summary>
    /// 时间管理器
    /// 管理游戏中的时间流逝、昼夜循环和季节变化
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        #region 单例

        public static TimeManager Instance { get; private set; }

        #endregion

        #region 时间设置

        /// <summary>
        /// 一天的游戏时间(秒)
        /// </summary>
        public float dayLength = 600f; // 10分钟

        /// <summary>
        /// 时间流逝速度倍率
        /// </summary>
        public float timeSpeed = 1f;

        /// <summary>
        /// 季节长度(天数)
        /// </summary>
        public int seasonLength = 15;

        #endregion

        #region 当前时间

        /// <summary>
        /// 当前游戏时间(秒)
        /// </summary>
        public float currentTime;

        /// <summary>
        /// 当前天数
        /// </summary>
        public int currentDay;

        /// <summary>
        /// 当前季节
        /// </summary>
        public Season currentSeason;

        /// <summary>
        /// 当前年份
        /// </summary>
        public int currentYear;

        /// <summary>
        /// 当前时间段
        /// </summary>
        public TimeOfDay timeOfDay;

        #endregion

        #region 昼夜设置

        /// <summary>
        /// 日出时间(一天中的秒数)
        /// </summary>
        public float sunriseTime = 180f; // 6:00 AM

        /// <summary>
        /// 日落时间(一天中的秒数)
        /// </summary>
        public float sunsetTime = 480f; // 8:00 PM

        #endregion

        #region 事件

        public event Action<float> OnTimeChanged;
        public event Action<int> OnDayChanged;
        public event Action<Season> OnSeasonChanged;
        public event Action<int> OnYearChanged;
        public event Action<TimeOfDay> OnTimeOfDayChanged;
        public event Action<bool> OnDayNightChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 是否是白天
        /// </summary>
        public bool IsDaytime => currentTime >= sunriseTime && currentTime < sunsetTime;

        /// <summary>
        /// 当前时间在一天中的百分比 (0-1)
        /// </summary>
        public float TimePercentOfDay => currentTime / dayLength;

        /// <summary>
        /// 当前时间在季节中的百分比 (0-1)
        /// </summary>
        public float TimePercentOfSeason => (currentDay % seasonLength) / (float)seasonLength;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 初始化时间
            currentTime = sunriseTime; // 从日出开始
            currentDay = 1;
            currentSeason = Season.Spring;
            currentYear = 1;
            UpdateTimeOfDay();
        }

        private void Update()
        {
            // 更新时间
            currentTime += Time.deltaTime * timeSpeed;

            // 检查是否进入新的一天
            while (currentTime >= dayLength)
            {
                currentTime -= dayLength;
                currentDay++;

                // 检查是否进入新的季节
                if (currentDay % seasonLength == 1)
                {
                    NextSeason();
                }

                // 检查是否进入新的一年
                if (currentDay > seasonLength * 4)
                {
                    currentDay = 1;
                    currentYear++;
                    OnYearChanged?.Invoke(currentYear);
                    Debug.Log($"进入第 {currentYear} 年");
                }

                OnDayChanged?.Invoke(currentDay);
                Debug.Log($"第 {currentDay} 天开始");
            }

            // 更新时间段
            UpdateTimeOfDay();

            // 触发时间变化事件
            OnTimeChanged?.Invoke(currentTime);
        }

        #endregion

        #region 时间更新

        /// <summary>
        /// 更新时间段
        /// </summary>
        private void UpdateTimeOfDay()
        {
            TimeOfDay newTimeOfDay = GetTimeOfDay(currentTime);

            if (newTimeOfDay != timeOfDay)
            {
                timeOfDay = newTimeOfDay;
                OnTimeOfDayChanged?.Invoke(timeOfDay);
            }

            // 检查昼夜变化
            bool isDaytime = IsDaytime;
        }

        /// <summary>
        /// 获取当前时间段
        /// </summary>
        public TimeOfDay GetTimeOfDay(float time)
        {
            // 夜晚: 8:00 PM - 4:00 AM
            if (time >= sunsetTime || time < 120f)
            {
                // 午夜: 12:00 AM - 4:00 AM
                if (time < 120f)
                    return TimeOfDay.Midnight;
                // 深夜: 8:00 PM - 12:00 AM
                return TimeOfDay.LateNight;
            }

            // 白天: 6:00 AM - 8:00 PM
            // 早晨: 6:00 AM - 9:00 AM
            if (time < 270f)
                return TimeOfDay.Morning;
            // 上午: 9:00 AM - 12:00 PM
            if (time < 360f)
                return TimeOfDay.Noon;
            // 下午: 12:00 PM - 5:00 PM
            if (time < 450f)
                return TimeOfDay.Afternoon;
            // 傍晚: 5:00 PM - 8:00 PM
            return TimeOfDay.Evening;
        }

        /// <summary>
        /// 进入下一个季节
        /// </summary>
        private void NextSeason()
        {
            switch (currentSeason)
            {
                case Season.Spring:
                    currentSeason = Season.Summer;
                    break;
                case Season.Summer:
                    currentSeason = Season.Fall;
                    break;
                case Season.Fall:
                    currentSeason = Season.Winter;
                    break;
                case Season.Winter:
                    currentSeason = Season.Spring;
                    break;
            }

            OnSeasonChanged?.Invoke(currentSeason);
            Debug.Log($"进入{GetSeasonName(currentSeason)}");
        }

        #endregion

        #region 时间控制

        /// <summary>
        /// 设置时间速度
        /// </summary>
        public void SetTimeSpeed(float speed)
        {
            timeSpeed = Mathf.Clamp(speed, 0.1f, 5f);
        }

        /// <summary>
        /// 暂停时间
        /// </summary>
        public void Pause()
        {
            timeSpeed = 0;
        }

        /// <summary>
        /// 恢复时间
        /// </summary>
        public void Resume()
        {
            timeSpeed = 1f;
        }

        /// <summary>
        /// 设置具体时间
        /// </summary>
        public void SetTime(float time)
        {
            currentTime = Mathf.Clamp(time, 0, dayLength);
        }

        /// <summary>
        /// 设置天数
        /// </summary>
        public void SetDay(int day)
        {
            currentDay = day;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取季节名称
        /// </summary>
        public string GetSeasonName(Season season)
        {
            switch (season)
            {
                case Season.Spring: return "春季";
                case Season.Summer: return "夏季";
                case Season.Fall: return "秋季";
                case Season.Winter: return "冬季";
                default: return "未知";
            }
        }

        /// <summary>
        /// 获取时间段名称
        /// </summary>
        public string GetTimeOfDayName(TimeOfDay timeOfDay)
        {
            switch (timeOfDay)
            {
                case TimeOfDay.Midnight: return "午夜";
                case TimeOfDay.LateNight: return "深夜";
                case TimeOfDay.Morning: return "早晨";
                case TimeOfDay.Noon: return "上午";
                case TimeOfDay.Afternoon: return "下午";
                case TimeOfDay.Evening: return "傍晚";
                default: return "未知";
            }
        }

        /// <summary>
        /// 格式化时间显示
        /// </summary>
        public string FormatTime(float time)
        {
            int hours = Mathf.FloorToInt(time / 60);
            int minutes = Mathf.FloorToInt(time % 60);
            return $"{hours:D2}:{minutes:D2}";
        }

        /// <summary>
        /// 获取当前时间字符串
        /// </summary>
        public string GetCurrentTimeString()
        {
            return FormatTime(currentTime);
        }

        #endregion

        #region 保存/加载

        public TimeData Save()
        {
            return new TimeData
            {
                currentTime = this.currentTime,
                currentDay = this.currentDay,
                currentSeason = this.currentSeason,
                currentYear = this.currentYear,
                timeSpeed = this.timeSpeed
            };
        }

        public void Load(TimeData data)
        {
            currentTime = data.currentTime;
            currentDay = data.currentDay;
            currentSeason = data.currentSeason;
            currentYear = data.currentYear;
            timeSpeed = data.timeSpeed;
        }

        #endregion
    }

    #region 枚举类型

    public enum Season
    {
        Spring,  // 春季
        Summer,  // 夏季
        Fall,    // 秋季
        Winter   // 冬季
    }

    public enum TimeOfDay
    {
        Midnight,    // 午夜 (12:00 AM - 4:00 AM)
        LateNight,   // 深夜 (8:00 PM - 12:00 AM)
        Morning,     // 早晨 (6:00 AM - 9:00 AM)
        Noon,        // 上午 (9:00 AM - 12:00 PM)
        Afternoon,   // 下午 (12:00 PM - 5:00 PM)
        Evening      // 傍晚 (5:00 PM - 8:00 PM)
    }

    #endregion

    [Serializable]
    public class TimeData
    {
        public float currentTime;
        public int currentDay;
        public Season currentSeason;
        public int currentYear;
        public float timeSpeed;
    }
}