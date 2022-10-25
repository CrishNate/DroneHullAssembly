// https://answers.unity.com/questions/1840803/calculate-collision-between-2-rotated-boxes-withou.html

using UnityEngine;

namespace DronesAssembly.Math
{
    public class BoxSolver
    {
         private struct Box
         {
             public Vector3 pos, n1, n2, n3;
             public float min, max;
             private void UpdateMinMax(Vector3 aPos, ref Vector3 aNormal)
             {
                 float p = Vector3.Dot(aPos, aNormal);
                 if (p > max) max = p;
                 if (p < min) min = p;
             }
             public void GetMinMax(ref Vector3 aAxis)
             {
                 min = float.PositiveInfinity;
                 max = float.NegativeInfinity;
                 UpdateMinMax(pos + n1 + n2 + n3, ref aAxis);
                 UpdateMinMax(pos + n1 + n2 - n3, ref aAxis);
                 UpdateMinMax(pos + n1 - n2 + n3, ref aAxis);
                 UpdateMinMax(pos + n1 - n2 - n3, ref aAxis);
                 UpdateMinMax(pos - n1 + n2 + n3, ref aAxis);
                 UpdateMinMax(pos - n1 + n2 - n3, ref aAxis);
                 UpdateMinMax(pos - n1 - n2 + n3, ref aAxis);
                 UpdateMinMax(pos - n1 - n2 - n3, ref aAxis);
             }
         }
         
         private struct TwoBoxes
         {
             public Box A, B;
             // returns true if there is no overlap, false if they do overlap
             public bool SAT(Vector3 aAxis)
             {
                 A.GetMinMax(ref aAxis);
                 B.GetMinMax(ref aAxis);
                 return A.min > B.max || B.min > A.max;
             }
         }
         
         public static bool Intersects(Vector3 positionA, Vector3 sizeA, Quaternion rotationA, Vector3 positionB, Vector3 sizeB, Quaternion rotationB)
         {
             TwoBoxes data = new TwoBoxes();
             data.A.pos = positionA;
             data.A.n1 = rotationA * Vector3.right * sizeA.x;
             data.A.n2 = rotationA * Vector3.up * sizeA.y;
             data.A.n3 = rotationA * Vector3.forward * sizeA.z;
             data.B.pos = positionB;
             data.B.n1 = rotationB * Vector3.right * sizeB.x;
             data.B.n2 = rotationB * Vector3.up * sizeB.y;
             data.B.n3 = rotationB * Vector3.forward * sizeB.z;
             
             if (data.SAT(data.A.n1)) return false;
             if (data.SAT(data.A.n2)) return false;
             if (data.SAT(data.A.n3)) return false;
             if (data.SAT(data.B.n1)) return false;
             if (data.SAT(data.B.n2)) return false;
             if (data.SAT(data.B.n3)) return false;
         
             if (data.SAT(Vector3.Cross(data.A.n1, data.B.n1))) return false;
             if (data.SAT(Vector3.Cross(data.A.n1, data.B.n2))) return false;
             if (data.SAT(Vector3.Cross(data.A.n1, data.B.n3))) return false;
             if (data.SAT(Vector3.Cross(data.A.n2, data.B.n1))) return false;
             if (data.SAT(Vector3.Cross(data.A.n2, data.B.n2))) return false;
             if (data.SAT(Vector3.Cross(data.A.n2, data.B.n3))) return false;
             if (data.SAT(Vector3.Cross(data.A.n3, data.B.n1))) return false;
             if (data.SAT(Vector3.Cross(data.A.n3, data.B.n2))) return false;
             if (data.SAT(Vector3.Cross(data.A.n3, data.B.n3))) return false;
             
             return true;
         }
    }
}