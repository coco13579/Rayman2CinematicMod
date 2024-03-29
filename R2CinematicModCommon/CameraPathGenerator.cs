﻿using System.Collections.Generic;
using Toe;

namespace R2CinematicModCommon
{
    public class CameraPathGenerator
    {
        public static List<CurvePoint> GenerateCameraPath(List<CurvePoint> keyPoints, float segments)
        {
            if (keyPoints.Count == 1)
                return keyPoints;

            List<CurvePoint> pathPoints = new List<CurvePoint>();

            // Loop through key points, 4 at a time
            for (int i = 0; i < keyPoints.Count - 1; i += 3)
            {
                CurvePoint p0 = GetKeyPoint(keyPoints, i); // Anchor point 1
                CurvePoint p1 = GetKeyPoint(keyPoints, i + 1); // Control point 1
                CurvePoint p2 = GetKeyPoint(keyPoints, i + 2); // Control point 2
                CurvePoint p3 = GetKeyPoint(keyPoints, i + 3); // Anchor point 2

                if (i > 0)
                {
                    // Adjust the last control point of the previous group
                    CurvePoint previousP2 = GetKeyPoint(keyPoints, i - 1);
                    Vector3 p2OffsetPos = p0.Position - previousP2.Position;
                    p1.Position = p0.Position + p2OffsetPos;
                }

                if (i < keyPoints.Count - 4)
                {
                    // Adjust the first control point of the next group
                    CurvePoint nextP0 = GetKeyPoint(keyPoints, i + 4);
                    Vector3 p1Offset = p3.Position - nextP0.Position;
                    p2.Position = p3.Position + p1Offset;
                }

                // Generate interpolation on position, rotation and fov
                for (int t = 0; t < segments; t++)
                {
                    float tValue = t / (float)segments;
                    Vector3 pointOnCurve = CubicBezier(p0.Position, p1.Position, p2.Position, p3.Position, tValue);
                    Vector3 rotationOnCurve = Slerp(p0.Rotation, p1.Rotation, p2.Rotation, p3.Rotation, tValue);
                    float fovOnCurve = Lerp(p0.Fov, p1.Fov, p2.Fov, p3.Fov, tValue);

                    pathPoints.Add(new CurvePoint(pointOnCurve, rotationOnCurve, fovOnCurve));
                }
            }

            return pathPoints;
        }

        private static CurvePoint GetKeyPoint(List<CurvePoint> keyPoints, int index)
        {
            // Adjust the index if it is out of range
            if (index >= keyPoints.Count)
            {
                // Set the index to the last key point
                index = keyPoints.Count - 1;
            }

            return keyPoints[index];
        }

        private static Vector3 Slerp(Vector3 start, Vector3 mid1, Vector3 mid2, Vector3 end, float t)
        {
            Quaternion qStart = Quaternion.EulerRotation(start.Y, start.Z, start.X);
            Quaternion qMid1 = Quaternion.EulerRotation(mid1.Y, mid1.Z, mid1.X);
            Quaternion qMid2 = Quaternion.EulerRotation(mid2.Y, mid2.Z, mid2.X);
            Quaternion qEnd = Quaternion.EulerRotation(end.Y, end.Z, end.X);

            Quaternion q1 = Quaternion.Slerp(qStart, qMid1, t);
            Quaternion q2 = Quaternion.Slerp(qMid1, qMid2, t);
            Quaternion q3 = Quaternion.Slerp(qMid2, qEnd, t);

            Quaternion q4 = Quaternion.Slerp(q1, q2, t);
            Quaternion q5 = Quaternion.Slerp(q2, q3, t);

            Quaternion q6 = Quaternion.Slerp(q4, q5, t);

            return Matrix.QuaternionToEuler(q6);
        }

        private static float Lerp(float a, float b, float c, float d, float t)
        {
            float ab = Lerp(a, b, t);
            float cd = Lerp(c, d, t);
            return Lerp(ab, cd, t);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        private static Vector3 CubicBezier(Vector3 start, Vector3 mid1, Vector3 mid2, Vector3 end, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * start;
            p += 3 * uu * t * mid1;
            p += 3 * u * tt * mid2;
            p += ttt * end;

            return p;
        }
    }
}
