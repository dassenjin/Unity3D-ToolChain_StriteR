using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class URuntime
{
    public static bool SetActive(this Transform _transform, bool _active) => SetActive(_transform.gameObject, _active);
    public static bool SetActive(this MonoBehaviour _monobehaviour, bool _active) => SetActive(_monobehaviour.gameObject, _active);
    public static bool SetActive(this GameObject _transform, bool _active)
    {
        if (_transform.activeSelf == _active)
            return false;

        _transform.SetActive(_active);
        return true;
    }

    #region Transform
    public static void DestroyChildren(this Transform _trans,bool immediately)
    {
        int count = _trans.childCount;
        if (count <= 0)
            return;
        Transform[] transforms = new Transform[count];
        for (int i = 0; i < count; i++)
            transforms[i] = _trans.GetChild(i);
        
        foreach (var transform in transforms)
        {
            if(transform==_trans)
                continue;
            if(immediately)
                GameObject.DestroyImmediate(transform.gameObject);
            else
                GameObject.Destroy(transform.gameObject);
        }
    }
    public static void SetChildLayer(this Transform trans, int layer)
    {
        foreach (Transform temp in trans.gameObject.GetComponentsInChildren<Transform>(true))
            temp.gameObject.layer = layer;
    }
    public static Transform FindInAllChild(this Transform trans, string name)
    {
        foreach (Transform temp in trans.gameObject.GetComponentsInChildren<Transform>(true))
            if (temp.name == name) return temp;
        Debug.LogWarning("Null Child Name:" + name + ",Find Of Parent:" + trans.name);
        return null;
    }

    public static T Find<T>(this T[,] array, Predicate<T> predicate)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                if (predicate(array[i, j])) return array[i, j];
        return default(T);
    }


    public static void SortChildByNameIndex(Transform transform, bool higherUpper = true)
    {
        List<Transform> childList = new List<Transform>();
        List<int> childIndexList = new List<int>();

        for (int i = 0; i < transform.childCount; i++)
        {
            childList.Add(transform.GetChild(i));
            childIndexList.Add(int.Parse(childList[i].gameObject.name));
        }
        childIndexList.Sort((a, b) => { return a <= b ? (higherUpper ? 1 : -1) : (higherUpper ? -1 : 1); });

        for (int i = 0; i < childList.Count; i++)
        {
            childList[i].SetSiblingIndex(childIndexList.FindIndex(p => p == int.Parse(childList[i].name)));
        }
    }
    #endregion

    public static Rect Reposition(this Rect _rect, float _newPositionX, float _newPositionY) => Reposition(_rect, new Vector2(_newPositionX, _newPositionY));
    public static Rect Reposition(this Rect _rect, Vector2 _newPosition) { _rect.position = _newPosition; return _rect; }
    public static Rect Resize(this Rect _rect, float _newSizeX, float _newSizeY) => Resize(_rect, new Vector2(_newSizeX, _newSizeY));
    public static Rect Resize(this Rect _rect, Vector2 _newSize) { _rect.size = _newSize; return _rect; }
    public static Rect Expand(this Rect _rect, Vector2 _size) { _rect.position -= _size / 2; _rect.size += _size; return _rect; }
    public static Rect Collapse(this Rect _rect,Vector2 _size) { _rect.position += _size / 2;_rect.size -= _size;return _rect; }

    public static Vector3 GetPoint(this Bounds _bound, Vector3 _normalizedSize) => _bound.center + _bound.size.Multiply(_normalizedSize);
    public static Bounds Resize(this Bounds _srcBounds,Bounds _dstBounds)
    {
        Vector3 min = Vector3.Min(_srcBounds.min, _dstBounds.min);
        Vector3 max = Vector3.Max(_srcBounds.max, _dstBounds.max);
        Vector3 size = min - max;
        return new Bounds(min + size / 2, size);
    }

    #region Camera Helper
    public static bool InputRayCheck(this Camera _camera, Vector2 _inputPos, out RaycastHit _hit, int _layerMask = -1)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            _hit = new RaycastHit();
            return false;
        }

        return Physics.Raycast(_camera.ScreenPointToRay(_inputPos), out _hit, 1000, _layerMask);
    }
    public static Quaternion CameraProjectionOnPlane(this Camera _camera, Vector3 _position) => Quaternion.LookRotation(Vector3.ProjectOnPlane(_position - _camera.transform.position, _camera.transform.right), _camera.transform.up);
    #endregion
}