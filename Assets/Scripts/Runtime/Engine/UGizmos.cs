#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Voxel;
using UnityEditor;
using UnityEngine;

public static class Gizmos_Extend
{
    public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, Vector3 _scale, float _radius, float _height)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawWireCapsule(_pos, _rot, _scale, _radius, _height);
    }

    public static void DrawWireCube(Vector3 _pos, Quaternion _rot, Vector3 _cubeSize)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawWireCube(_pos, _rot, _cubeSize);
    }

    public static void DrawArrow(Vector3 _pos, Vector3 _direction, float _length, float _radius) => DrawArrow(_pos, Quaternion.LookRotation(_direction), _length, _radius);
    public static void DrawArrow(Vector3 _pos, Quaternion _rot, float _length, float _radius)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawArrow(_pos, _rot, _length, _radius);
    }
    public static void DrawCylinder(Vector3 _pos, Quaternion _rot, float _radius, float _height)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawCylinder(_pos, _rot, _radius, _height);
    }

    public static void DrawTrapezium(Vector3 _pos, Quaternion _rot, Vector4 _trapeziumInfo)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawTrapezium(_pos, _rot, _trapeziumInfo);
    }
    public static void DrawLine(Vector3 _src, Vector3 _dest, float _normalizedLength=1f)
    {
        Gizmos.DrawLine(_src,(_src+(_dest-_src)*_normalizedLength));
    }

    public static void DrawLines(IList<Vector3> _points)
    {
        int count = _points.Count;
        for(int i=0;i<count-1;i++)
            Gizmos.DrawLine(_points[i],_points[i+1]);
    }

    public static void DrawLines<T>(IEnumerable<T> _points,Func<T,Vector3> _convert)
    {
        Vector3 tempPoint=default;
        foreach (var (index,value) in _points.LoopIndex())
        {
            var point = _convert(value);
            if (index == 0)
            {
                tempPoint = point;
                continue;
            }

            Gizmos.DrawLine(tempPoint,point);
            tempPoint = point;
        }
    }

    public static void DrawLinesConcat(params Vector3[] _lines) => DrawLinesConcat(_lines.ToList());
    public static void DrawLinesConcat(IList<Vector3> _points)
    {
        int count = _points.Count;
        for(int i=0;i<count;i++)
            Gizmos.DrawLine(_points[i],_points[(i+1)%count]);
    }
    public static void DrawLinesConcat<T>(IList<T> _points,Func<T,Vector3> _convert)
    {
        int count = _points.Count;
        for(int i=0;i<count;i++)
            Gizmos.DrawLine(_convert(_points[i]),_convert(_points[(i+1)%count]));
    }
    public static void DrawGizmos(this GHeightCone _cone)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawCone(_cone);
    }
    public static void DrawGizmos(this GLine _line)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawLine(_line);
    }

    public static void DrawString(Vector3 _positionLS,string _text,float _offset=1f)
    {
        Handles.matrix = Gizmos.matrix;
        Handles.Label(_positionLS+_offset*Vector3.up,_text);
    }
    
    public static void DrawGizmos(this GBox _box)=>Gizmos.DrawWireCube(_box.center,_box.size);
    public static void DrawGizmos(this GFrustumPoints _frustumPoints)
    {
        DrawLinesConcat(_frustumPoints.nearBottomLeft,_frustumPoints.nearBottomRight,_frustumPoints.nearTopRight,_frustumPoints.nearTopLeft);
        DrawLine(_frustumPoints.farBottomLeft,_frustumPoints.nearBottomLeft);
        DrawLine(_frustumPoints.farBottomRight,_frustumPoints.nearBottomRight);
        DrawLine(_frustumPoints.farTopLeft,_frustumPoints.nearTopLeft);
        DrawLine(_frustumPoints.farTopRight,_frustumPoints.nearTopRight);
        DrawLinesConcat(_frustumPoints.farBottomLeft,_frustumPoints.farBottomRight,_frustumPoints.farTopRight,_frustumPoints.farTopLeft);
    }
}

#endif