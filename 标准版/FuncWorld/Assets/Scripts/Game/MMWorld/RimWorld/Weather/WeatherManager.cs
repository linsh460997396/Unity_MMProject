using UnityEngine;
using System.Collections.Generic;
using System;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 天气管理器
    /// 管理天气变化和天气事件
    /// </summary>
    public class WeatherManager : MonoBehaviour
    {
        #region 单例

        public static WeatherManager Instance { get; private set; }

        #endregion

        #region 当前天气

        /// <summary>
        /// 当前天气类型
        /// </summary>
        public WeatherType currentWeather;

        /// <summary>
        /// 当前温度
        /// </summary>
        public float currentTemperature;

        /// <summary>
        /// 当前风力
        /// </summary>
        public float currentWindSpeed;

        /// <summary>
        /// 当前降水强度 (0-1)
        /// </summary>
        public float currentPrecipitation;

        #endregion

        #region 设置

        /// <summary>
        /// 基础温度(根据季节调整)
        /// </summary>
        public float baseTemperature = 20f;

        /// <summary>
        /// 温度波动范围
        /// </summary>
        public float temperatureVariation = 10f;

        /// <summary>
        /// 天气持续时间(秒)
        /// </summary>
        public float weatherDuration = 180f;

        /// <summary>
        /// 天气变化概率(每小时)
        /// </summary>
        public float weatherChangeChance = 0.1f;

        #endregion

        #region 季节温度调整

        public float springTempOffset = 0f;
        public float summerTempOffset = 10f;
        public float fallTempOffset = -5f;
        public float winterTempOffset = -20f;

        #endregion

        #region 天气效果

        /// <summary>
        /// 是否在下雨
        /// </summary>
        public bool IsRaining => currentWeather == WeatherType.Rain || 
                                currentWeather == WeatherType.Thunderstorm;

        /// <summary>
        /// 是否在降雪
        /// </summary>
        public bool IsSnowing => currentWeather == WeatherType.Snow || 
                                 currentWeather == WeatherType.Blizzard;

        /// <summary>
        /// 是否有风暴
        /// </summary>
        public bool IsStorming => currentWeather == WeatherType.Thunderstorm || 
                                  currentWeather == WeatherType.Blizzard;

        #endregion

        #region 事件

        public event Action<WeatherType> OnWeatherChanged;
        public event Action<float> OnTemperatureChanged;
        public event Action<float> OnWindSpeedChanged;
        public event Action<float> OnPrecipitationChanged;

        #endregion

        #region 内部状态

        private float weatherTimer;
        private float lastWeatherChangeTime;

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
            // 初始化天气
            currentWeather = WeatherType.Clear;
            currentTemperature = baseTemperature;
            currentWindSpeed = 5f;
            currentPrecipitation = 0f;

            // 订阅季节变化事件
            TimeManager.Instance.OnSeasonChanged += HandleSeasonChanged;
        }

        private void Update()
        {
            // 更新温度
            UpdateTemperature();

            // 更新天气计时器
            weatherTimer += Time.deltaTime;

            // 检查是否需要改变天气
            if (weatherTimer >= weatherDuration)
            {
                TryChangeWeather();
                weatherTimer = 0;
            }
        }

        #endregion

        #region 温度更新

        /// <summary>
        /// 更新温度
        /// </summary>
        private void UpdateTemperature()
        {
            // 获取季节温度偏移
            float seasonOffset = GetSeasonTemperatureOffset();

            // 计算昼夜温度变化(白天暖,夜晚冷)
            float dayNightOffset = TimeManager.Instance.IsDaytime ? 5f : -5f;

            // 添加随机波动
            float randomVariation = UnityEngine.Random.Range(-temperatureVariation, temperatureVariation) * 0.1f;

            // 计算新温度
            float newTemperature = baseTemperature + seasonOffset + dayNightOffset + randomVariation;

            // 平滑过渡
            currentTemperature = Mathf.Lerp(currentTemperature, newTemperature, Time.deltaTime * 0.1f);

            OnTemperatureChanged?.Invoke(currentTemperature);
        }

        /// <summary>
        /// 获取季节温度偏移
        /// </summary>
        private float GetSeasonTemperatureOffset()
        {
            switch (TimeManager.Instance.currentSeason)
            {
                case Season.Spring: return springTempOffset;
                case Season.Summer: return summerTempOffset;
                case Season.Fall: return fallTempOffset;
                case Season.Winter: return winterTempOffset;
                default: return 0f;
            }
        }

        #endregion

        #region 天气变化

        /// <summary>
        /// 尝试改变天气
        /// </summary>
        private void TryChangeWeather()
        {
            // 根据当前季节和温度决定可能的天气
            List<WeatherType> possibleWeathers = GetPossibleWeathers();

            if (possibleWeathers.Count == 0) return;

            // 随机选择天气
            WeatherType newWeather = possibleWeathers[UnityEngine.Random.Range(0, possibleWeathers.Count)];

            ChangeWeather(newWeather);
        }

        /// <summary>
        /// 获取当前可能的天气类型
        /// </summary>
        private List<WeatherType> GetPossibleWeathers()
        {
            List<WeatherType> possible = new List<WeatherType>();
            Season season = TimeManager.Instance.currentSeason;

            // 基础天气总是可能
            possible.Add(WeatherType.Clear);
            possible.Add(WeatherType.Cloudy);

            // 根据季节添加天气
            switch (season)
            {
                case Season.Spring:
                    possible.Add(WeatherType.Rain);
                    possible.Add(WeatherType.Thunderstorm);
                    break;
                case Season.Summer:
                    possible.Add(WeatherType.Rain);
                    possible.Add(WeatherType.Thunderstorm);
                    possible.Add(WeatherType.Drought);
                    break;
                case Season.Fall:
                    possible.Add(WeatherType.Rain);
                    if (currentTemperature < 5f)
                        possible.Add(WeatherType.Snow);
                    break;
                case Season.Winter:
                    possible.Add(WeatherType.Snow);
                    possible.Add(WeatherType.Blizzard);
                    possible.Add(WeatherType.Fog);
                    break;
            }

            // 如果当前正在下雨/雪,有概率继续
            if (IsRaining || IsSnowing)
            {
                possible.Add(currentWeather); // 保持当前天气
            }

            return possible;
        }

        /// <summary>
        /// 改变天气
        /// </summary>
        public void ChangeWeather(WeatherType newWeather)
        {
            if (newWeather == currentWeather) return;

            WeatherType oldWeather = currentWeather;
            currentWeather = newWeather;

            // 更新天气相关属性
            UpdateWeatherProperties();

            // 触发事件
            OnWeatherChanged?.Invoke(newWeather);

            Debug.Log($"天气变化: {GetWeatherName(oldWeather)} -> {GetWeatherName(newWeather)}");
        }

        /// <summary>
        /// 更新天气属性
        /// </summary>
        private void UpdateWeatherProperties()
        {
            switch (currentWeather)
            {
                case WeatherType.Clear:
                    currentWindSpeed = 5f;
                    currentPrecipitation = 0f;
                    break;
                case WeatherType.Cloudy:
                    currentWindSpeed = 8f;
                    currentPrecipitation = 0f;
                    break;
                case WeatherType.Rain:
                    currentWindSpeed = 12f;
                    currentPrecipitation = 0.5f;
                    break;
                case WeatherType.Thunderstorm:
                    currentWindSpeed = 20f;
                    currentPrecipitation = 0.8f;
                    break;
                case WeatherType.Snow:
                    currentWindSpeed = 10f;
                    currentPrecipitation = 0.4f;
                    break;
                case WeatherType.Blizzard:
                    currentWindSpeed = 25f;
                    currentPrecipitation = 0.9f;
                    break;
                case WeatherType.Fog:
                    currentWindSpeed = 2f;
                    currentPrecipitation = 0f;
                    break;
                case WeatherType.Drought:
                    currentWindSpeed = 15f;
                    currentPrecipitation = 0f;
                    break;
            }

            OnWindSpeedChanged?.Invoke(currentWindSpeed);
            OnPrecipitationChanged?.Invoke(currentPrecipitation);
        }

        #endregion

        #region 季节变化处理

        private void HandleSeasonChanged(Season season)
        {
            // 季节变化时重置天气
            switch (season)
            {
                case Season.Winter:
                    // 冬季开始时可能下雪
                    if (currentTemperature < 0f)
                    {
                        ChangeWeather(WeatherType.Snow);
                    }
                    break;
                case Season.Summer:
                    // 夏季开始时可能变晴
                    ChangeWeather(WeatherType.Clear);
                    break;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取天气名称
        /// </summary>
        public string GetWeatherName(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Clear: return "晴朗";
                case WeatherType.Cloudy: return "多云";
                case WeatherType.Rain: return "下雨";
                case WeatherType.Thunderstorm: return "雷暴";
                case WeatherType.Snow: return "下雪";
                case WeatherType.Blizzard: return "暴风雪";
                case WeatherType.Fog: return "雾";
                case WeatherType.Drought: return "干旱";
                default: return "未知";
            }
        }

        /// <summary>
        /// 获取温度字符串
        /// </summary>
        public string GetTemperatureString()
        {
            return $"{currentTemperature:F1}°C";
        }

        /// <summary>
        /// 获取风力字符串
        /// </summary>
        public string GetWindString()
        {
            if (currentWindSpeed < 5) return "无风";
            if (currentWindSpeed < 10) return "微风";
            if (currentWindSpeed < 15) return "大风";
            if (currentWindSpeed < 20) return "强风";
            return "狂风";
        }

        #endregion

        #region 保存/加载

        public WeatherData Save()
        {
            return new WeatherData
            {
                currentWeather = this.currentWeather,
                currentTemperature = this.currentTemperature,
                currentWindSpeed = this.currentWindSpeed,
                currentPrecipitation = this.currentPrecipitation,
                weatherTimer = this.weatherTimer
            };
        }

        public void Load(WeatherData data)
        {
            currentWeather = data.currentWeather;
            currentTemperature = data.currentTemperature;
            currentWindSpeed = data.currentWindSpeed;
            currentPrecipitation = data.currentPrecipitation;
            weatherTimer = data.weatherTimer;
        }

        #endregion
    }

    #region 枚举类型

    public enum WeatherType
    {
        Clear,          // 晴朗
        Cloudy,         // 多云
        Rain,           // 下雨
        Thunderstorm,   // 雷暴
        Snow,           // 下雪
        Blizzard,       // 暴风雪
        Fog,            // 雾
        Drought         // 干旱
    }

    #endregion

    [Serializable]
    public class WeatherData
    {
        public WeatherType currentWeather;
        public float currentTemperature;
        public float currentWindSpeed;
        public float currentPrecipitation;
        public float weatherTimer;
    }
}