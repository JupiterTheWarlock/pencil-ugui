using System;
using UnityEngine;

namespace JtheWL.PencilUguiImporter
{
    [Serializable]
    public class UiIrDocument
    {
        public int version;
        public string source;
        public string documentId;
        public UiIrNode[] nodes;
    }

    [Serializable]
    public class UiIrNode
    {
        public string id;
        public string name;
        public string type;
        public UiIrBounds bounds;
        public float cornerRadius;
        public UiIrFill[] fills;
        public UiIrText text;
        public UiIrNode[] children;
    }

    [Serializable]
    public class UiIrBounds
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }

    [Serializable]
    public class UiIrFill
    {
        public string type;
        public UiIrColor color;
    }

    [Serializable]
    public class UiIrColor
    {
        public float r;
        public float g;
        public float b;
        public float a = 1f;

        public Color ToUnityColor()
        {
            return new Color(r, g, b, a);
        }
    }

    [Serializable]
    public class UiIrText
    {
        public string characters;
        public float fontSize;
    }
}
