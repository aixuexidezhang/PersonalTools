using UnityEngine;

/// <summary>
/// 字段名称标签
/// 自定义 inspector 字段名称
/// </summary>
public class LabelAttribute : PropertyAttribute
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// 字段名称
    /// </summary>
    /// <param name="name">名称</param>
    public LabelAttribute(string name)
    {
        Name = name;
    }
}