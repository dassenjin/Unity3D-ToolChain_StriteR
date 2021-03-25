﻿using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class CullingMaskAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class RangeVectorAttribute : PropertyAttribute 
{
    public float m_Min { get; private set; }
    public float m_Max { get; private set; }
    public RangeVectorAttribute(float _min,float _max)
    {
        m_Min = _min;
        m_Max = _max;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class RangeIntAttribute:PropertyAttribute
{
    public int m_Min { get; private set; }
    public int m_Max { get; private set; }
    public RangeIntAttribute(int _min,int _max)
    {
        m_Min = _min;
        m_Max = _max;
    }

}