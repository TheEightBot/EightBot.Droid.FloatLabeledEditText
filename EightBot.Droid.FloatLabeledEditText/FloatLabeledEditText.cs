using System;
using Android.Widget;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Graphics;
using Android.Content.Res;
using Android.Text;

namespace EightBot.Droid.FloatLabeledEditText
{
	public enum AnimationState { None, Shrink, Grow }

	public class FloatLabeledEditText : EditText
	{
		public const float DefaultHintScale = 0.6f;
		public const int DefaultAnimationSteps = 6;

		readonly Paint _floatingHintPaint = new Paint();
		ColorStateList _hintColors;
		bool _wasEmpty;
		int _animationFrame;
		AnimationState mAnimation = AnimationState.None;

		float _hintScale = DefaultHintScale;
		public float HintScale {
			get {
				return _hintScale;
			}
			set {
				_hintScale = value;
			}
		}

		int _animationSteps = DefaultAnimationSteps;
		public int AnimationSteps {
			get {
				return _animationSteps;
			}
			set {
				_animationSteps = value;
			}
		}

		public FloatLabeledEditText (Context context) : base(context)
		{
			Initialize ();
		}

		public FloatLabeledEditText (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			Initialize ();
		}

		public FloatLabeledEditText (Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Initialize ();
		}

		public FloatLabeledEditText (Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			Initialize ();
		}

		void Initialize()
		{
			_hintColors = HintTextColors;

			_wasEmpty = string.IsNullOrEmpty (this.Text);
		}
				
		public override int PaddingTop {
			get {
				var metrics = Paint.GetFontMetricsInt ();
				var floatingHintHeight = (int)((metrics.Bottom - metrics.Top) * HintScale);
				return base.CompoundPaddingTop + floatingHintHeight;
			}
		}

		protected override void OnTextChanged (Java.Lang.ICharSequence text, int start, int lengthBefore, int lengthAfter)
		{
			base.OnTextChanged (text, start, lengthBefore, lengthAfter);

			var isEmpty = string.IsNullOrEmpty (this.Text);

			if (_wasEmpty.Equals (isEmpty))
				return;

			_wasEmpty = isEmpty;

			if (!IsShown)
				return;

			if (isEmpty) {
				mAnimation = AnimationState.Grow;
				SetHintTextColor (Color.Transparent);
			} else
				mAnimation = AnimationState.Shrink;
		}
			
	
		protected override void OnDraw (Canvas canvas)
		{
			base.OnDraw (canvas);

			if(string.IsNullOrEmpty(this.Hint))
				return;

			var isAnimating = mAnimation != AnimationState.None;

			if (!isAnimating && string.IsNullOrEmpty (this.Text))
				return;

			_floatingHintPaint.Set (this.Paint);
			_floatingHintPaint.Color = new Color(_hintColors.GetColorForState (GetDrawableState (), new Color(_hintColors.DefaultColor)));

			var normalHintSize = this.TextSize;
			var floatingHintSize = normalHintSize * HintScale;

			var hintPosX = this.CompoundPaddingLeft + ScrollX;
			var normalHintPosY = this.Baseline;
			var floatingHintPosY = normalHintPosY + this.Paint.GetFontMetricsInt ().Top + this.ScrollY;
			floatingHintPosY = floatingHintPosY + (int)((floatingHintSize - floatingHintPosY) / 2f);


			// If we're not animating, we're showing the floating hint, so draw it and bail.
			if (!isAnimating) {
				_floatingHintPaint.TextSize = floatingHintSize;
				canvas.DrawText (Hint, hintPosX, floatingHintPosY, _floatingHintPaint);
				return;
			}

			if (mAnimation == AnimationState.Shrink)
				DrawAnimationFrame(canvas, normalHintSize, floatingHintSize, hintPosX, normalHintPosY, floatingHintPosY);
			else
				DrawAnimationFrame(canvas, floatingHintSize, normalHintSize, hintPosX, floatingHintPosY, normalHintPosY);

			_animationFrame++;

			if (_animationFrame == AnimationSteps) {
				if (mAnimation == AnimationState.Grow)
					SetHintTextColor (_hintColors);

				mAnimation = AnimationState.None;
				_animationFrame = 0;
			}

			Invalidate();
		}

		private void DrawAnimationFrame(Canvas canvas, float fromSize, float toSize, float hintPosX, float fromY, float toY) {
			float textSize = Lerp(fromSize, toSize);
			float hintPosY = Lerp(fromY, toY);
			_floatingHintPaint.TextSize = textSize;
			canvas.DrawText(Hint, hintPosX, hintPosY, _floatingHintPaint);
		}

		private float Lerp(float from, float to) {
			float alpha = (float) _animationFrame / (AnimationSteps - 1);
			return from * (1 - alpha) + to * alpha;
		}

	}
}

