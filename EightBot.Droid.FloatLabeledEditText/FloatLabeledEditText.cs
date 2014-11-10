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
	public class FloatLabeledEditText : EditText
	{
		enum AnimationState { None, Shrink, Grow }

		public const float DefaultHintScale = 0.6f;
		public const int DefaultAnimationSteps = 6;

		readonly Paint _floatingHintPaint = new Paint();
		ColorStateList _hintColors;
		bool _wasEmpty;
		int _currentAnimationFrame;
		AnimationState _currentAnimationState = AnimationState.None;

		float _hintScale = DefaultHintScale;
		public float HintScale {
			get { return _hintScale; }
			set { _hintScale = value; }
		}

		int _animationSteps = DefaultAnimationSteps;
		public int AnimationSteps {
			get { return _animationSteps; }
			set { _animationSteps = value; }
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
				_currentAnimationState = AnimationState.Grow;
				SetHintTextColor (Color.Transparent);
			} else
				_currentAnimationState = AnimationState.Shrink;
		}

		protected override void OnDraw (Canvas canvas)
		{
			base.OnDraw (canvas);

			if(string.IsNullOrEmpty(this.Hint))
				return;

			var isAnimating = _currentAnimationState != AnimationState.None;

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

			if (_currentAnimationState == AnimationState.Shrink)
				DrawAnimationFrame(canvas, normalHintSize, floatingHintSize, hintPosX, normalHintPosY, floatingHintPosY);
			else
				DrawAnimationFrame(canvas, floatingHintSize, normalHintSize, hintPosX, floatingHintPosY, normalHintPosY);

			_currentAnimationFrame++;

			if (_currentAnimationFrame == AnimationSteps) {
				if (_currentAnimationState == AnimationState.Grow)
					SetHintTextColor (_hintColors);

				_currentAnimationState = AnimationState.None;
				_currentAnimationFrame = 0;
			}

			Invalidate();
		}

		void DrawAnimationFrame(Canvas canvas, float fromSize, float toSize, float hintPosX, float fromY, float toY) {
			float textSize = Lerp(fromSize, toSize);
			float hintPosY = Lerp(fromY, toY);
			_floatingHintPaint.TextSize = textSize;
			canvas.DrawText(Hint, hintPosX, hintPosY, _floatingHintPaint);
		}

		float Lerp(float from, float to) {
			float alpha = (float) _currentAnimationFrame / (AnimationSteps - 1);
			return from * (1 - alpha) + to * alpha;
		}

	}
}

