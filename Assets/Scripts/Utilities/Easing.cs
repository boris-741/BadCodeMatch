using UnityEngine;
using System;
using System.Collections;
 

public static class Easing {
	
    public enum Ease {
        Linear,
        EaseInOut,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        ALL
    } // enum Ease
    
    public static float Linear(float t) {
		return t;
	} // Linear(float, float, float, float)
	
	public static float EaseInQustom(float t)
    {
        const float pi2 = Mathf.PI;
		return t - (t * Mathf.Sin(t * pi2)/6f);
    }

	public static float EaseInQuad(float t) {
		return t * t;
	} // EaseInQuad(float, float, float, float)
	
	public static float EaseOutQuad(float t) {
		return -t * (t - 2.0F);
	} // EaseOutQuad(float, float, float, float)
	
    public static float EaseInOut(float t)
    {
        const float pi2 = Mathf.PI * 2f;
		return t - Mathf.Sin(t * pi2) / pi2;
    }

	public static float EaseInOutQuad(float t) {
		if ((t *= 2.0F) < 1.0F) return 0.5F * t * t;
		return -0.5F * (--t * (t - 2.0F) - 1.0F);
	} // EaseInOutQuad(float, float, float, float)
	
	public static float EaseInCubic(float t) {
		return t * t * t;
	} // EaseInCubic(float, float, float, float)
	
	public static float EaseOutCubic(float t) {
		return (--t * t * t + 1.0F);
	} // EaseOutCubic(float, float, float, float)
	
	public static float EaseInOutCubic(float t) {
		if ((t *= 2.0F) < 1.0F) return 0.5F * t * t * t;
		return 0.5F * ((t -= 2.0F) * t * t + 2.0F);
	} // EaseInOutCubic(float, float, float, float)
	
	public static float EaseInQuart(float t) {
		return t * t * t * t;
	} // EaseInQuart(float, float, float, float)
	
	public static float EaseOutQuart(float t) {
		return -(--t * t * t * t - 1.0F);
	} // EaseOutQuart(float, float, float, float)
	
	public static float EaseInOutQuart(float t) {
		if ((t *= 2.0F) < 1.0F) return 0.5F * t * t * t * t;
		return -0.5F * ((t -= 2.0F) * t * t * t - 2.0F);
	} // EaseInOutQuart(float, float, float, float)
	
	public static float EaseInQuint(float t) {
		return t * t * t * t * t;
	} // EaseInQuint(float, float, float, float)
	
	public static float EaseOutQuint(float t) {
		return (--t * t * t * t * t + 1.0F);
	} // EaseOutQuint(float, float, float, float)
	
	public static float EaseInOutQuint(float t) {
		if ((t *= 2.0F) < 1.0F) return 0.5F * t * t * t * t * t;
		return 0.5F * ((t -= 2.0F) * t * t * t * t + 2.0F);
	} // EaseInOutQuint(float, float, float, float)
	
	public static float EaseInSine(float t) {
		return -Mathf.Cos(t * (Mathf.PI * 0.5F)) + 1.0F;
	} //
	public static float EaseOutSine(float t) {
		return Mathf.Sin(t * (Mathf.PI * 0.5F));
	} // EaseInSine(float, float, float, float)
	
	public static float EaseInOutSine(float t) {
		return -0.5F * (Mathf.Cos(Mathf.PI * t) - 1.0F);
	} // EaseInOutSine(float, float, float, float)
	
	public static float EaseInExpo(float t) {
		return (t == 0.0F) ? 0.0F : Mathf.Pow(2.0F, 10.0F * (t - 1.0F));
	} // EaseInExpo(float, float, float, float)
	
	public static float EaseOutExpo(float t) {
		return (t == 1.0F) ? 1.0F : (-Mathf.Pow(2.0F, -10.0F * t) + 1.0F);
	} // EaseOutExpo(float, float, float, float)
	
	public static float EaseInOutExpo(float t) {
		if (t == 0.0F) return 0.0F;
		if (t == 1.0F) return 1.0F;
		if ((t *= 2.0F) < 1.0F) return 0.5F * Mathf.Pow(2.0F, 10.0F * (t - 1.0F));
		return 0.5F * (-Mathf.Pow(2.0F, -10.0F * --t) + 2.0F);
	} // EaseInOutExpo(float, float, float, float)
	
	public static float EaseInCirc(float t) {
		return -(Mathf.Sqrt(1.0F - t * t) - 1.0F);
	} // EaseInCirc(float, float, float, float)
	
	public static float EaseOutCirc(float t) {
		return Mathf.Sqrt(1.0F - --t * t);
	} // EaseOutCirc(float, float, float, float)
	
	public static float EaseInOutCirc(float t) {
		if ((t *= 2.0F) < 1.0F) return -0.5F * (Mathf.Sqrt(1.0F - t * t) - 1.0F);
		return 0.5F * (Mathf.Sqrt(1.0F - (t -= 2.0F) * t) + 1.0F);
	} // EaseInOutCirc(float, float, float, float)
	
	public static float EaseInElastic(float t) {
		if (t == 0.0F) return 0.0F;
		if (t == 1.0F) return 1.0F;
		return -(Mathf.Pow(2.0F, 10.0F * (t -= 1.0F)) * Mathf.Sin((t / 0.3F - 0.25F) * (2.0F * Mathf.PI)));
	} // EaseInElastic(float, float, float, float)
	
	public static float EaseOutElastic(float t) {
		if (t == 0.0F) return 0.0F;
		if (t == 1.0F) return 1.0F;
		return Mathf.Pow(2.0F, -10.0F * t) * Mathf.Sin((t / 0.3F - 0.25F) * (2.0F * Mathf.PI)) + 1.0F;
	} // EaseOutElastic(float, float, float, float)
	
	public static float EaseInOutElastic(float t) {
		if (t == 0.0F) return 0.0F;
		if ((t *= 2.0F) == 2.0F) return 1.0F;
		if (t < 1.0F) return -0.5F * (Mathf.Pow(2.0F, 10.0F * (t -= 1.0F)) * Mathf.Sin((t / (0.3F * 1.5F) - 0.25F) * (2.0F * Mathf.PI)));
		return Mathf.Pow(2.0F, -10.0F * (t -= 1.0F)) * Mathf.Sin((t / (0.3F * 1.5F) - 0.25F) * (2.0F * Mathf.PI)) * 0.5F + 1.0F;
	} // EaseInOutElastic(float, float, float, float)
	
	public static float EaseInBack(float t) {
		float s = 1.70158F;
		return t * t * ((s + 1.00000F) * t - s);
	} // EaseInBack(float, float, float, float)
	
	public static float EaseOutBack(float t) {
		float s = 1.70158F;
		return (--t * t * ((s + 1.0F) * t + s) + 1.0F);
	} // EaseOutBack(float, float, float, float)
	
	public static float EaseInOutBack(float t) {
		float s = 1.70158F;
		if ((t *= 2.0F) < 1.0F) return 0.5F * (t * t * (((s *= 1.525F) + 1.0F) * t - s));
		return 0.5F * ((t -= 2.0F) * t * (((s *= 1.525F) + 1.0F) * t + s) + 2);
	} // EaseInOutBack(float, float, float, float)
	
	public static float EaseInBounce(float t) {
		return 1.0F - Easing.EaseOutBounce(1.0F - t);
	} // EaseInBounce(float, float, float, float)
	
	public static float EaseOutBounce(float t) {
		if (t < (1 / 2.75F)) {
			return (7.5625F * t * t);
		} else if (t < (2.0F / 2.75F)) {
			return (7.5625F * (t -= 1.5F / 2.75F) * t + 0.75F);
		} else if (t < (2.5F / 2.75F)) {
			return (7.5625F * (t -= 2.25F / 2.75F) * t + 0.9375F);
		} else {
			return (7.5625F * (t -= 2.625F / 2.75F) * t + 0.984375F);
		}
	} // EaseOutBounce(float, float, float, float)
	
	public static float EaseInOutBounce(float t) {
		if (t < 0.5F) return Easing.EaseInBounce(t * 2.0F) * 0.5F;
		return Easing.EaseOutBounce(t * 2.0F - 1.0F) * 0.5F + 0.5F;
	} // EaseInOutBounce(float, float, float, float)
	

    public static float EaseTween(Ease ease, float t)
    {
        switch(ease)
        {
            case Ease.Linear:
                return Linear( t );
            case Ease.EaseInOut:
                return EaseInOut( t );
            case Ease.EaseInBack:
                return EaseInBack( t );
            case Ease.EaseInBounce:
                return EaseInBounce( t );
            case Ease.EaseInCirc:
                return EaseInCirc( t );
            case Ease.EaseInCubic:
                return EaseInCubic( t );
            case Ease.EaseInElastic:
                return EaseInElastic( t );
            case Ease.EaseInExpo:
                return EaseInExpo( t );
            case Ease.EaseInOutBack:
                return EaseInOutBack( t );
            case Ease.EaseInOutBounce:
                return EaseInOutBounce( t );
            case Ease.EaseInOutCirc:
                return EaseInOutCirc( t );
            case Ease.EaseInOutCubic:
                return EaseInOutCubic( t );
            case Ease.EaseInOutElastic:
                return EaseInOutElastic( t );
            case Ease.EaseInOutExpo:
                return EaseInOutExpo( t );
            case Ease.EaseInOutQuad:
                return EaseInOutQuad( t );
            case Ease.EaseInOutQuart:
                return EaseInOutQuart( t );
            case Ease.EaseInOutQuint:
                return EaseInOutQuint( t );
            case Ease.EaseInOutSine:
                return EaseInOutSine( t );
            case Ease.EaseInQuad:
                return EaseInQuad( t );
            case Ease.EaseInQuart:
                return EaseInQuart( t );
            case Ease.EaseInQuint:
                return EaseInQuint( t );
            case Ease.EaseInSine:
                return EaseInSine( t );
            case Ease.EaseOutBack:
                return EaseOutBack( t );
            case Ease.EaseOutBounce:
                return EaseOutBounce( t );
            case Ease.EaseOutCirc:
                return EaseOutCirc( t );
            case Ease.EaseOutCubic:
                return EaseOutCubic( t );
            case Ease.EaseOutElastic:
                return EaseOutElastic( t );
            case Ease.EaseOutExpo:
                return EaseOutExpo( t );
            case Ease.EaseOutQuad:
                return EaseOutQuad( t );
            case Ease.EaseOutQuart:
                return EaseOutQuart( t );
            case Ease.EaseOutQuint:
                return EaseOutQuint( t );
            case Ease.EaseOutSine:
                return EaseOutSine( t );
        }
        return 0;
    }
} // class Easing
