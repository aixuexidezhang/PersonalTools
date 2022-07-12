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
        /// <summary>�Ƿ�������ģʽ</summary>
        public bool Performance = false;

        /// <summary>������������</summary>
        public int BGMVolume = 100;

        /// <summary>��Ч����</summary>
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
    /// ����
    /// </summary>
    public static GameSettingData SettingData { get => gsd; }

    /// <summary>
    /// ������Ϸ����
    /// </summary>
    /// <param name="setSound">������ɺ�Ļص�.��һ�������Ǳ�������,�ڶ�����������Ч����</param>
    public static void LoadSet(Action<int, int> setSound)
    {
        if (PlayerPrefs.HasKey(key))
            gsd = JsonConvert.DeserializeObject<GameSettingData>(PlayerPrefs.GetString(key));
        if (gsd == null) gsd = new GameSettingData();

        //����֡��
        if (gsd.Performance) Application.targetFrameRate = 60; else Application.targetFrameRate = 30;

        //��������
        setSound?.Invoke(gsd.BGMVolume, gsd.SoundVolume);
    }

}

