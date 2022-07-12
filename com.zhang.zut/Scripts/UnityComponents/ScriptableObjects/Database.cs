using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace MyTool.UnityComponents
{
    public class Database<T> : ScriptableObject
    {
        [Header("目标路径列表")]
        public string[] TaggetPaths;
        [SerializeField] List<T> datas = new List<T>();
        public List<T> Datas => datas;

        protected Dictionary<string, T> Dic;

        /// <summary>检查是否包含Key</summary>
        public bool HasKey(string name) 
        {
            return Dic.ContainsKey(name);
        }
    }
}