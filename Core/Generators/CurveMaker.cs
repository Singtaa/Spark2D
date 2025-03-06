using Unity.Mathematics;
using System;

namespace Spark2D {
    public class CurveMaker {
        float2[] _controlPoints;
        float[] _tempX;
        float[] _tempY;
        int _count;
        Array _resultPoints;

        public float2[] ControlPoints {
            get {
                float2[] copy = new float2[_controlPoints.Length];
                Array.Copy(_controlPoints, copy, _controlPoints.Length);
                return copy;
            }
            set {
                if (value.Length < 2)
                    throw new Exception("Control points must have at least 2 points");
                _controlPoints = value;
                _tempX = new float[value.Length];
                _tempY = new float[value.Length];
            }
        }

        public int Count {
            get { return _count; }
            set {
                if (value < 2)
                    throw new Exception("Count must be at least 2");
                _count = value;
                _resultPoints = Array.CreateInstance(typeof(float2), value);
            }
        }

        public CurveMaker(float2[] controlPoints, int count) {
            ControlPoints = controlPoints;
            Count = count;
        }

        public Array Generate() {
            if (_count < 2 || _controlPoints.Length < 2)
                return _resultPoints;

            for (int i = 0; i < _count; i++) {
                float t = _count > 1 ? (float)i / (_count - 1) : 0;

                for (int k = 0; k < _controlPoints.Length; k++) {
                    _tempX[k] = _controlPoints[k].x;
                    _tempY[k] = _controlPoints[k].y;
                }

                // Apply De Casteljau's algorithm iteratively
                for (int j = _controlPoints.Length - 1; j > 0; j--) {
                    for (int k = 0; k < j; k++) {
                        _tempX[k] = (1 - t) * _tempX[k] + t * _tempX[k + 1];
                        _tempY[k] = (1 - t) * _tempY[k] + t * _tempY[k + 1];
                    }
                }

                _resultPoints.SetValue(new float2(_tempX[0], _tempY[0]), i);
            }

            return _resultPoints;
        }
    }
}