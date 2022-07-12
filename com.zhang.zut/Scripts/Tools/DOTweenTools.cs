// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13


using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening.Core;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Options;
using Outline = UnityEngine.UI.Outline;
using Text = UnityEngine.UI.Text;
using DG.Tweening;

namespace MyTool.Tools
{
    public static class DOTweenTools
    {
        #region CanvasGroup

        public static TweenerCore<float, float, FloatOptions> Fade(this CanvasGroup target, float endValue, float duration)
        {
            TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.alpha, x => target.alpha = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        #endregion

        #region Graphic

       
        public static TweenerCore<Color, Color, ColorOptions> Color(this Graphic target, Color endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.To(() => target.color, x => target.color = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        /// <summary>Tweens an Graphic's alpha color to the given value.

        public static TweenerCore<Color, Color, ColorOptions> Fade(this Graphic target, float endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        #endregion

        #region Image

       
        public static TweenerCore<Color, Color, ColorOptions> Color(this Image target, Color endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.To(() => target.color, x => target.color = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }


        public static TweenerCore<Color, Color, ColorOptions> Fade(this Image target, float endValue, float duration)
        {
            TweenerCore<Color, Color, ColorOptions> t = DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

     
        public static TweenerCore<float, float, FloatOptions> FillAmount(this Image target, float endValue, float duration)
        {
            if (endValue > 1) endValue = 1;
            else if (endValue < 0) endValue = 0;
            TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.fillAmount, x => target.fillAmount = x, endValue, duration);
            t.SetTarget(target);
            return t;
        }

        #endregion
    }
}
