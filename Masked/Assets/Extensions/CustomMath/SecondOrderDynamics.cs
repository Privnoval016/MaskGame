using Unity.Mathematics;
using UnityEngine;

namespace Extensions.CustomMath
{
    public class SecondOrderDynamics
    {
        private Vector3? _x;
        private Vector3? _y, _yD;
        private readonly float _w, _z, _d, _k1, _k2, _k3;

        public SecondOrderDynamics(float f, float z, float r, Vector3 x0)
        {
            _w = 2 * math.PI * f;
            _z = z;
            _d = _w * math.sqrt(math.abs(z * z - 1));
            _k1 = z / (math.PI * f);
            _k2 = 1 / (_w * _w);
            _k3 = r * z / _w;

            _x = x0;
            _y = x0;
            _yD = Vector3.zero;
        }

        public Vector3? Update(float T, Vector3 x, Vector3? xd = null)
        {
            if (xd == null)
            {
                xd = (x - _x) / T;
                _x = x;
            }

            float k1Stable, k2Stable;
            if (_w * T < _z)
            {
                k1Stable = _k1;
                k2Stable = Mathf.Max(_k2, T * T / 2 + T * _k1 / 2, T * _k1);
            }
            else
            {
                float t1 = math.exp(-_z * _w * T);
                float alpha = 2 * t1 * (_z <= 1 ? math.cos(T * _d) : math.cosh(T * _d));
                float beta = t1 * t1;
                float t2 = T / (1 + beta - alpha);
                k1Stable = (1 - beta) * t2;
                k2Stable = T * t2;
            }

            _y = _y + T * _yD;
            _yD = _yD + T * (x + _k3 * xd - _y - k1Stable * _yD) / k2Stable;
            return _y;
        }
    }
}
