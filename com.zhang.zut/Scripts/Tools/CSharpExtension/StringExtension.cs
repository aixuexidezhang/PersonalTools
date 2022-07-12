using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Tools.CSharpExtension
{
	public static class StringExtension
	{
		/// <summary>
		/// 转换为年月日
		/// </summary>
		/// <param name="time"></param>
		/// <param name="format">格式默认yyyy-MM-dd HH:mm:ss.fff zzz</param>
		/// <returns></returns>
		public static string ToTimeString(this DateTime time, string format = "yyyy-MM-dd HH:mm:ss.fff zzz")
		{
			return time.ToString(format);
		}

		/// <summary>
		/// 转换为合法的ID
		/// </summary>
		/// <returns></returns>
		public static string ToIdString(this string str, params char[] exceptchars)
		{
			exceptchars = exceptchars.Length == 0 ? new char[] { '+', '/', '（', '）', '~', '\n', '\t', '\r', '、', '|', '*', '“', '”', '—', '。', '…', '=', '#', ' ', ';', '；', '-', ',', '，', '<', '>', '【', '】', '[', ']', '{', '}', '!', '！', '?', '？', '.', '\'', '‘', '’', '\"', ':', '：', '_', '(', ')', '&', '^', '%', '$', '@', '`' } : exceptchars;
			return str.RemveChars(exceptchars);
		}

		static string RemveChars(this string str, params char[] exceptchars)
		{
			foreach (var c in exceptchars)
			{
				str = str.Replace(c.ToString(), "");
			}
			return str;
		}

		/// <summary>
		/// 检测非法字符true代表合法
		/// </summary>
		public static bool ToIllegal(this string name, params char[] exceptchars)
		{
			if (name.Length == 0) return false;
			exceptchars = exceptchars.Length == 0 ? new char[] { '+', '-', '*', '/', '[', ']', '{', '}', '【', '】', '.', '`', '~', ' ', '"' } : exceptchars;
			foreach (var item in exceptchars) if (name.IndexOf(item) > 0) return false;
			return (true && !string.IsNullOrWhiteSpace(name));
		}



		/// <summary>
		/// 获得时间戳
		/// </summary>
		public static long GetTimeStamp(this DateTime t)
		{
			DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan ts = DateTime.UtcNow - start;
			long a = Convert.ToInt64(ts.TotalMilliseconds);
			return a;
		}

		/// <summary>
		/// int转换为中文
		/// </summary>
		public static string ToStringChinese(this int i)
		{
			return i.ToString().Replace('0', '零')
				.Replace('1', '一')
				.Replace('2', '二')
				.Replace('3', '三')
				.Replace('4', '四')
				.Replace('5', '五')
				.Replace('6', '六')
				.Replace('7', '七')
				.Replace('8', '八')
				.Replace('9', '九');
		}

		/// <summary>
		/// 某个字符在字符串中出现的次数
		/// </summary>
		public static int SubstringCount(this string str, string substring)
		{
			if (str.Contains(substring))
			{
				string strReplaced = str.Replace(substring, "");
				return (str.Length - strReplaced.Length) / substring.Length;
			}
			return 0;
		}
	}
}
