using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class DebugLine : MonoBehaviour
    {
        public int maxRenderers;

        List<LineRenderer> lines = new List<LineRenderer>();

        public float lineWidth = 0.05f;

        void Start()
        {
            for (int i = 0; i < maxRenderers; i++)
            {
                
            }
        }

        void CreateLine(int i)
        {
            GameObject go = new GameObject();
            lines.Add(go.AddComponent<LineRenderer>());
            lines[i].widthMultiplier = lineWidth;
        }

        public void SetLine(Vector3 startpos, Vector3 endpos, int index)
        {   if (index > lines.Count - 1)
                CreateLine(index);

            lines[index].SetPosition(0, startpos);
            lines[index].SetPosition(1, endpos);
        }

        public static DebugLine singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}