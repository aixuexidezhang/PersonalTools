using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSetting
{
    [Serializable]
    public class GameSettingData
    {
        /// <summary>是否是性能模式</summary>
        public bool Performance = false;

        /// <summary>背景音乐音量</summary>
        public int BGMVolume = 100;

        /// <summary>音效音量</summary>
        public int SoundVolume = 100;

        public void Save()
        {
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(this));
            if (gsd.Performance) Application.targetFrameRate = 60; else Application.targetFrameRate = 30;
        }
        public GameSettingData()
        {
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(this));
        }
    }
    static GameSettingData gsd;

    static readonly string key = string.Concat("ZhangGameSetting-", Application.productName, "-", Application.companyName);

    /// <summary>
    /// 设置
    /// </summary>
    public static GameSettingData SettingData { get => gsd; }

    /// <summary>
    /// 加载游戏设置
    /// </summary>
    /// <param name="setSound">加载完成后的回调.第一个参数是背景音量,第二个参数是音效音量</param>
    public static void LoadSet(Action<int, int> setSound)
    {
        if (PlayerPrefs.HasKey(key))
            gsd = JsonConvert.DeserializeObject<GameSettingData>(PlayerPrefs.GetString(key));
        if (gsd == null) gsd = new GameSettingData();

        //设置帧率
        if (gsd.Performance) Application.targetFrameRate = 60; else Application.targetFrameRate = 30;

        //设置音量
        setSound?.Invoke(gsd.BGMVolume, gsd.SoundVolume);
    }

}

