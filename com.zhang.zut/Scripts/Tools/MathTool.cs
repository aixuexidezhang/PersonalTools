using System.Collections.Generic;
using UnityEngine;

namespace MyTool.Tools
{
    /// <summary>
    /// 数学运算工具
    /// </summary>
    public static class MathTool
    {

        /// <summary>
        /// 获取A和B的旋转夹角
        /// </summary>
        /// <param name="a">当前的位置</param>
        /// <param name="target">目标位置</param>
        /// <returns>角度</returns>
        public static float GetAngle(this Vector3 a, Vector3 target)
        {
            target.x -= a.x;
            target.z -= a.z;
            float deltaAngle = 0;
            if (target.x == 0 && target.z == 0)
            {
                return 0;
            }
            else if (target.x > 0 && target.z > 0)
            {
                deltaAngle = 0;
            }
            else if (target.x > 0 && target.z == 0)
            {
                return 90;
            }
            else if (target.x > 0 && target.z < 0)
            {
                deltaAngle = 180;
            }
            else if (target.x == 0 && target.z < 0)
            {
                return 180;
            }
            else if (target.x < 0 && target.z < 0)
            {
                deltaAngle = -180;
            }
            else if (target.x < 0 && target.z == 0)
            {
                return -90;
            }
            else if (target.x < 0 && target.z > 0)
            {
                deltaAngle = 0;
            }
            return Mathf.Atan(target.x / target.z) * Mathf.Rad2Deg + deltaAngle;
        }

        /// <summary>
        /// 区域检测
        /// </summary>
        /// <param name="Target">目标</param>
        /// <param name="vertexs">顶点List集合</param>
        /// <returns></returns>
        public static bool RegionDetection(Vector2 Target, List<Vector2> vertexs)
        {
            int crossNum = 0;
            int vertexCount = vertexs.Count;

            for (int i = 0; i < vertexCount; i++)
            {
                Vector2 v1 = vertexs[i];
                Vector2 v2 = vertexs[(i + 1) % vertexCount];

                if (((v1.y <= Target.y) && (v2.y > Target.y))
                    || ((v1.y > Target.y) && (v2.y <= Target.y)))
                {
                    if (Target.x < v1.x + (Target.y - v1.y) / (v2.y - v1.y) * (v2.x - v1.x))
                    {
                        crossNum += 1;
                    }
                }
            }
            if (crossNum % 2 == 0) return false;
            else return true;
        }

    }

}
