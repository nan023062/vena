// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using UnityEngine;

namespace Vena.Framework
{
    public class Shake : MonoBehaviour
    {
        [SerializeField]
        float startTime = 1.0f;
        [SerializeField]
        Vector3 directionStrength = new Vector3(0, 1, 0);
        [SerializeField]
        float Speed = 1.0f;
        [SerializeField]
        public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.13f, 0.4f), new Keyframe(0.33f, -0.33f), new Keyframe(0.5f, 0.17f), new Keyframe(0.71f, -0.12f), new Keyframe(1, 0));
        [SerializeField]
        bool enableReset = true; //显示后重置属性

        float timer;
        Vector3 thisPosition;
        Vector3 shakePosition;
        Vector3 initPos;

        private void Awake()
        {
            transform.localPosition = Vector3.zero;  //必须相对于局部坐标是为0 此为标准
            timer = Time.time;
        }

        private void OnEnable()
        {
            if(enableReset && gameObject.activeSelf)
            {
                timer = Time.time;
                transform.localPosition = Vector3.zero;
            }
        }

        void FixedUpdate()
        {
            thisPosition = transform.position - shakePosition;
            shakePosition = new Vector3(curve.Evaluate((Time.time - timer) * Speed) * directionStrength.x, curve.Evaluate((Time.time - timer) * Speed) * directionStrength.y, curve.Evaluate((Time.time - timer) * Speed) * directionStrength.z);
            if (timer >= startTime)
            {
                transform.position = shakePosition + thisPosition;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        public Vector3[] GetKeyFrames()
        {
            if (curve.keys.Length == 0)
                return null;
            Vector3[] positions = new Vector3[curve.keys.Length];
            for (int i = 0; i < curve.keys.Length; ++i)
            {
                positions[i] = new Vector3(curve.Evaluate((curve.keys[i].time) * Speed) * directionStrength.x, curve.Evaluate((curve.keys[i].time) * Speed) * directionStrength.y, curve.Evaluate((curve.keys[i].time) * Speed) * directionStrength.z);
            }
            return positions;
        }

        public float[] GetTimes()
        {
            if (curve.keys.Length == 0)
                return null;
            float[] times = new float[curve.keys.Length];
            for (int i = 0; i < curve.keys.Length; ++i)
            {
                times[i] = curve.keys[i].time;
            }
            return times;
        }
        
        public Vector3 GetDir()
        {
            return directionStrength;
        }
    }
}
