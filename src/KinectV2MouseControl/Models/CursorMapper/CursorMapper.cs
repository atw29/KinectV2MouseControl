﻿using System;

namespace KinectV2MouseControl
{
    public class CursorMapper
    {
        private MRect _inputRect;
        public MRect InputRect
        {
            get
            {
                return _inputRect;
            }
            set
            {
                _inputRect = value;
                UpdateMapping();
            }
        }

        private MRect _outputRect;
        public MRect OutputRect
        {
            get
            {
                return _outputRect;
            }
            set
            {
                _outputRect = value;
                UpdateMapping();
            }
        }

        private MVector2 _alignScale;
        public MVector2 AlignScale
        {
            get
            {
                return _alignScale;
            }
        }

        private double _moveScale = 1;
        public double MoveScale
        {
            get
            {
                return _moveScale;
            }
            set
            {
                _moveScale = value;
                totalScale = _moveScale * _alignScale;
            }
        }

        private MVector2 totalScale;
        private MVector2 moveOffset;

        private MVector2 smoothedPosition;

        public enum ScaleAlignment
        {
            None,
            Horizontal,
            Vertical,
            Both,
            ShorterRange,
            LongerRange
        }

        private ScaleAlignment _scaleAlign = ScaleAlignment.Both;
        public ScaleAlignment ScaleAlign
        {
            get
            {
                return _scaleAlign;
            }
            set
            {
                _scaleAlign = value;
                UpdateMapping();
            }
        }

        private const double SMOOTH_MAX = 1;
        private const double SMOOTH_MIN = 0;

        /*
         * Smoothing here is done by not moving cursor to exact target position but somewhere in between.
         * NewCursorPos = CurrentPos + (TargetPos - CurrentPos) * moveAmount;
         * moveAmount represents how much of the movement will be applied. e.g. 0.5 meaning the half way from current position to destination.
         */
        double moveAmount = 1;
        public double Smoothing
        {
            get
            {
                return 1 - moveAmount;
            }
            set
            {
                //Clamp value so it ranges from 0 to 1.
                if (value > SMOOTH_MAX)
                {
                    value = SMOOTH_MAX;
                }
                else if (value < SMOOTH_MIN)
                {
                    value = SMOOTH_MIN;
                }

                moveAmount = 1 - value;
            }
        }

        public CursorMapper(MRect inputRect, MRect outputRect, ScaleAlignment scaleAlign = ScaleAlignment.None)
        {
            ScaleAlign = scaleAlign;
            SetRects(inputRect, outputRect);
        }

        public MVector2 GetOutputPosition(MVector2 inputPosition)
        {
            return _outputRect.Center + (inputPosition - _inputRect.Center) * totalScale + moveOffset;
        }

        /// <summary>
        /// Get smoothed position.
        /// </summary>
        /// <param name="inputPosition">Position from input.</param>
        /// <param name="extraScale">
        /// Used as an extra control on how much the position is moving other than the moveAmount for smoothing.
        /// e.g. You can insert past time duration so the smoothing can be time-depended. And you may need to adjust Smoothing due to a big result change influenced by this value.
        /// </param>
        /// <returns></returns>
        public MVector2 GetSmoothedOutputPosition(MVector2 inputPosition, double extraScale = 1)
        {
            smoothedPosition += (GetOutputPosition(inputPosition) - smoothedPosition) * moveAmount * extraScale;
            return smoothedPosition;
        }

        public void SetRects(MRect inputRect, MRect outputRect)
        {
            _inputRect = inputRect;
            _outputRect = outputRect;
            UpdateMapping();
        }

        public void UpdateMapping()
        {
            if (_scaleAlign == ScaleAlignment.None) return;

            double scaleX = OutputRect.DeltaX / InputRect.DeltaX;
            double scaleY = OutputRect.DeltaY / InputRect.DeltaY;

            switch (_scaleAlign)
            {
                case ScaleAlignment.Both:
                    _alignScale.X = scaleX;
                    _alignScale.Y = scaleY;
                    break;
                case ScaleAlignment.Horizontal:
                    _alignScale.X = scaleX;
                    _alignScale.Y = scaleX;
                    break;
                case ScaleAlignment.LongerRange:
                    double scaleOfLongerRange = Math.Abs(OutputRect.Width > OutputRect.Height ? scaleX : scaleY);
                    _alignScale.X = scaleOfLongerRange * (scaleX < 0 ? -1 : 1);
                    _alignScale.Y = scaleOfLongerRange * (scaleY < 0 ? -1 : 1);
                    break;
                case ScaleAlignment.Vertical:
                    _alignScale.X = scaleY;
                    _alignScale.Y = scaleY;
                    break;
                case ScaleAlignment.ShorterRange:
                    double scaleOfShorterRange = Math.Abs(OutputRect.Width < OutputRect.Height ? scaleX : scaleY);
                    _alignScale.X = scaleOfShorterRange * (scaleX < 0 ? -1 : 1);
                    _alignScale.Y = scaleOfShorterRange * (scaleY < 0 ? -1 : 1);
                    break;
                default:
                    _alignScale.X = scaleX < 0 ? -1 : 1;
                    _alignScale.Y = scaleY < 0 ? -1 : 1;
                    break;
            }

            moveOffset.X = _outputRect.Left;
            moveOffset.Y = _outputRect.Top;

            totalScale = _moveScale * _alignScale;
        }

    }
}
