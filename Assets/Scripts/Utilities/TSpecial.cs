﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public static class TSpecial_Extend
{
    public static string GetEntityPropertyDetail<T>(this EntityProperty<T> _property) => string.Format("{0}:C|{1:F1},D|{2:F1}", _property.m_Identity, _property.m_CurAmount,_property.m_AmountDelta_Start);
    public static string GetEntityPropertyDetail<T>(this EntityProperty_0Max<T> _property) => string.Format("{0}:C|{1:F1},M|{2:F1},MD|{3:F1}", _property.m_Identity, _property.m_CurAmount, _property.m_MaxAmount,_property.m_MaxModify);
}
#region Game
public class EntityExpRank
{
    public int m_Rank { get; private set; }
    public int m_TotalExpOwned { get; private set; }
    public int m_ExpCurRankOwned { get; private set; }
    public int m_ExpCurRankRequired { get; private set; }
    public int m_ExpLeftToNextRank => m_ExpCurRankRequired - m_ExpCurRankOwned;
    public float m_ExpCurRankScale => m_ExpCurRankOwned / (float)m_ExpCurRankRequired;
    Func<int, int> GetExpToNextLevel;
    public EntityExpRank(Func<int, int> GetExpToNextLevel)
    {
        this.GetExpToNextLevel = GetExpToNextLevel;
        m_TotalExpOwned = 0;
        m_Rank = 0;
        m_ExpCurRankOwned = 0;
    }
    public void OnExpSet(int totalExp)
    {
        m_TotalExpOwned = 0;
        m_Rank = 0;
        m_ExpCurRankOwned = 0;
        OnExpGainCheckLevelOffset(totalExp);
    }

    public int OnExpGainCheckLevelOffset(int exp)
    {
        int startRank = m_Rank;
        m_TotalExpOwned += exp;
        m_ExpCurRankOwned += exp;
        for (; ; )
        {
            m_ExpCurRankRequired = GetExpToNextLevel(m_Rank);
            if (m_ExpCurRankOwned < m_ExpCurRankRequired)
                break;
            m_ExpCurRankOwned -= m_ExpCurRankRequired;
            m_Rank++;
        }
        return m_Rank - startRank;
    }
}
public class EntityProperty<T>
{
    public T m_Identity { get; private set; }
    public float m_StartAmount { get; private set; }
    public float m_CurAmount { get; protected set; }
    public float m_AmountDelta_Start => m_CurAmount - m_StartAmount;
    public EntityProperty(T _identity,float _start)
    {
        m_Identity = _identity;
        m_StartAmount = _start;
        m_CurAmount = m_StartAmount;
    }

    public virtual void AddCurDelta(float _delta) => m_CurAmount += _delta;
    public void ResetAmount() => m_CurAmount = m_StartAmount;
}
public class EntityProperty_0Max<T> : EntityProperty<T>
{
    public float m_MaxStart { get; private set; }
    public float m_MaxModify { get; private set; }
    public float m_MaxAmount { get; private set; }
    public float m_MaxScale => m_CurAmount / m_MaxAmount;
    public float m_AmountDelta_Max => m_MaxAmount - m_CurAmount;
    public EntityProperty_0Max(T _identity, float _startValue,float _maxStart) : base(_identity, _startValue)
    {
        m_MaxStart = _maxStart;
        m_MaxModify = 0f;
        m_MaxAmount = m_MaxStart + m_MaxModify;
    }
    public override void AddCurDelta(float _delta)
    {
        m_CurAmount = (Mathf.Clamp(m_CurAmount + _delta, 0, m_MaxAmount));
    }

    public void AddModifyDelta(float _delta)
    {
        m_MaxModify += _delta;
        m_MaxAmount = m_MaxStart + m_MaxModify;
        m_CurAmount = (Mathf.Clamp(m_CurAmount + _delta, 0, m_MaxAmount));
    }
    public void ResetMaxModify()
    {
        m_MaxModify = 0;
        m_MaxAmount = m_MaxStart + m_MaxModify;
    }
}
public class EntitySheildItem:CClassPool
{
    public int m_ID { get; private set; }
    public float m_Amount { get; private set; }
    public EntitySheildItem Set(int _ID,float _amount)
    {
        m_ID = _ID;
        m_Amount = _amount;
        return this;
    }
    public void AddDelta(float _delta) => m_Amount += _delta;
}
public class EntityShieldCombine
{
    public Dictionary<int,EntitySheildItem> m_Shields = new Dictionary<int, EntitySheildItem>();
    public float m_TotalAmount { get; private set; }
    void CheckTotalAmount()
    {
        m_TotalAmount = 0;
        m_Shields.Traversal((EntitySheildItem shield) => { m_TotalAmount += shield.m_Amount; });
    }
    public void SpawnShield(int _id, float _amount)
    {
        EntitySheildItem shield = CClassPool.Spawn<EntitySheildItem>().Set(_id, _amount);
        m_Shields.Add(_id, shield);
        CheckTotalAmount();
    }
    public void RecycleShield(int _shieldID)
    {
        if (!m_Shields.ContainsKey(_shieldID))
            throw new Exception("Invalid Shield Found Of:" + _shieldID);
        m_Shields[_shieldID].Recycle();
        m_Shields.Remove(_shieldID);
        CheckTotalAmount();
    }

    public float DoShieldDamageReduction(float _amount,Comparison<EntitySheildItem> _sort=null)
    {
        List<EntitySheildItem> _shields = m_Shields.Values.ToList();
        if(_sort!=null)
            _shields.Sort(_sort);

        _shields.TraversalBreak((EntitySheildItem _shield) => {
            float delta = _amount- _shield.m_Amount;
            if (delta >= 0)
                delta = _shield.m_Amount;
            else
                delta = _amount;
            _shield.AddDelta(-delta);
            _amount -= delta;
            return _amount == 0;
        });
        CheckTotalAmount();
        return _amount;
    }
}
#endregion
#region ValueHelper
public class ValueLerpBase
{
    float m_check;
    float m_duration;
    protected float m_value { get; private set; }
    protected float m_previousValue { get; private set; }
    protected float m_targetValue { get; private set; }
    Action<float> OnValueChanged;
    public ValueLerpBase(float startValue, Action<float> _OnValueChanged)
    {
        m_targetValue = startValue;
        m_previousValue = startValue;
        m_value = m_targetValue;
        OnValueChanged = _OnValueChanged;
        OnValueChanged(m_value);
    }

    protected void SetLerpValue(float value, float duration)
    {
        if (value == m_targetValue)
            return;
        m_duration = duration;
        m_check = m_duration;
        m_previousValue = m_value;
        m_targetValue = value;
    }

    public void SetFinalValue(float value)
    {
        if (value == m_value)
            return;
        m_value = value;
        m_previousValue = m_value;
        m_targetValue = m_value;
        OnValueChanged(m_value);
    }

    public void TickDelta(float deltaTime)
    {
        if (m_check <= 0)
            return;
        m_check -= deltaTime;
        m_value = GetValue(m_check / m_duration);
        OnValueChanged(m_value);
    }
    protected virtual float GetValue(float checkLeftParam)
    {
        Debug.LogError("Override This Please");
        return 0;
    }
}
public class ValueLerpSeconds : ValueLerpBase
{
    float m_perSecondValue;
    float m_maxDuration;
    float m_maxDurationValue;
    public ValueLerpSeconds(float startValue, float perSecondValue, float maxDuration, Action<float> _OnValueChanged) : base(startValue, _OnValueChanged)
    {
        m_perSecondValue = perSecondValue;
        m_maxDuration = maxDuration;
        m_maxDurationValue = m_perSecondValue * maxDuration;
    }

    public void SetLerpValue(float value) => SetLerpValue(value, Mathf.Abs(value - m_value) > m_maxDurationValue ? m_maxDuration : Mathf.Abs((value - m_value)) / m_perSecondValue);

    protected override float GetValue(float checkLeftParam) => Mathf.Lerp(m_previousValue, m_targetValue, 1 - checkLeftParam);
}
public class ValueChecker<T>
{
    public T value1 { get; private set; }
    public ValueChecker(T _check)
    {
        value1 = _check;
    }

    public bool Check(T target)
    {
        if (value1.Equals(target))
            return false;
        value1 = target;
        return true;
    }
}
public class ValueChecker<T, Y> : ValueChecker<T>
{
    public Y value2 { get; private set; }
    public ValueChecker(T temp1, Y temp2) : base(temp1)
    {
        value2 = temp2;
    }

    public bool Check(T target1, Y target2)
    {
        bool check1 = Check(target1);
        bool check2 = Check(target2);
        return check1 || check2;
    }
    public bool Check(Y target2)
    {
        if (value2.Equals(target2))
            return false;
        value2 = target2;
        return true;
    }
}
public class Timer
{
    public float m_TimerDuration { get; private set; } = 0;
    public bool m_Timing { get; private set; } = false;
    public float m_TimeLeft { get; private set; } = -1;
    public float m_TimeElapsed => m_TimerDuration - m_TimeLeft;
    public float m_TimeLeftScale { get; private set; } = 0;
    protected virtual bool CheckTiming() => m_TimeLeft > 0;
    public Timer(float countDuration = 0,bool startOff=false) {
        Set(countDuration);
        if (startOff)
            Stop();
    }
    public void Set(float duration)
    {
        m_TimerDuration = duration;
        OnTimeCheck(m_TimerDuration);
    }

    void OnTimeCheck(float _timeCheck)
    {
        m_TimeLeft = _timeCheck;
        m_Timing = CheckTiming();
        m_TimeLeftScale = m_TimerDuration == 0 ? 0 : m_TimeLeft / m_TimerDuration;
        if (m_TimeLeftScale < 0)
            m_TimeLeftScale = 0;
    }

    public void Replay() => OnTimeCheck(m_TimerDuration);
    public void Stop() => OnTimeCheck(0);

    public void Tick(float deltaTime)
    {
        if (m_TimeLeft <= 0)
            return;
        OnTimeCheck(m_TimeLeft-deltaTime);
        if (!m_Timing)
            m_TimeLeft = 0;
    }
}
#endregion
#region Special
public static class TimeScaleController<T> where T:struct
{
    static Dictionary<T, float> m_TimeScales=new Dictionary<T, float>();
    public static void Clear() => m_TimeScales.Clear();

    static float GetLowestScale()
    {
        float scale = 1f;
        m_TimeScales.Traversal((float value) => { if (scale > value) scale = value; });
        return scale;
    }

    public static float GetScale(T index) => m_TimeScales.ContainsKey(index) ? m_TimeScales[index] : 1f;
    public static void SetScale(T scaleIndex,float scale)
    {
        if (!m_TimeScales.ContainsKey(scaleIndex))
            m_TimeScales.Add(scaleIndex,1f);
        m_TimeScales[scaleIndex] = scale;
    }
    static ValueChecker<float> m_BulletTimeChecker = new ValueChecker<float>(1f);

    public static void Tick()
    {
        if (m_BulletTimeChecker.Check(GetLowestScale()))
            Time.timeScale = m_BulletTimeChecker.value1;
    }
}
public class AnimationSingleControl
{
    public Animation m_Animation { get; private set; }
    public AnimationSingleControl(Animation _animation, bool startFromOn = true)
    {
        m_Animation = _animation;
        m_Animation.playAutomatically = false;
        SetPlayPosition(startFromOn);
    }
    public void SetPlayPosition(bool forward) => m_Animation.clip.SampleAnimation(m_Animation.gameObject, forward ? 0 : m_Animation.clip.length);
    public void Play(bool forward)
    {
        m_Animation[m_Animation.clip.name].speed = forward ? 1 : -1;
        m_Animation[m_Animation.clip.name].normalizedTime = forward ? 0 : 1;
        m_Animation.Play(m_Animation.clip.name);
    }
    public void Stop()
    {
        m_Animation.Stop();
    }
}
public class AnimationFrameControl<T> where T : Enum
{
    struct BoneTransformRecord
    {
        public Transform m_Transform { get; private set; }
        public Vector3 m_LocalPos { get; private set; }
        public Quaternion m_LocalRot { get; private set; }
        public Vector3 m_LocalScale { get; private set; }
        public BoneTransformRecord(Transform _transform)
        {
            m_Transform = _transform;
            m_LocalPos = _transform.localPosition;
            m_LocalRot = _transform.localRotation;
            m_LocalScale = _transform.localScale;
        }
        public void Reset()
        {
            m_Transform.localPosition = m_LocalPos;
            m_Transform.localRotation = m_LocalRot;
            m_Transform.localScale = m_LocalScale;
        }
    }
    public AnimationClip[] m_Animations { get; private set; }
    BoneTransformRecord[] m_BoneRecords;
    public GameObject gameObject { get; private set; }
    public float m_TimeElapsed { get; private set; }
    public float m_AnimSpeed { get; private set; }
    public int m_CurPlaying { get; private set; } = -1;
    public AnimationFrameControl(GameObject _gameObject, AnimationClip[] _animations)
    {
        gameObject = _gameObject;
        m_Animations = _animations;
        m_CurPlaying = -1;
        m_BoneRecords = _gameObject.GetComponentsInChildren<Transform>(false).Convert(trans=>new BoneTransformRecord(trans));
    }

    public void ResetAnimation()
    {
        m_CurPlaying = -1;
        m_TimeElapsed = 0f;
        m_BoneRecords.Traversal(boneRecord => boneRecord.Reset());
    }
    bool CheckIndex(int index)
    {
        if (index < 0 || index >= m_Animations.Length)
            return false;

        if (m_CurPlaying != index)
        {
            ResetAnimation();
            m_CurPlaying = index;
            m_TimeElapsed = 0f;
        }
        return true;
    }

    public void TickLoop(int index, float _deltaTime)
    {
        if (!CheckIndex(index))
            return;

        AnimationClip curClip = m_Animations[m_CurPlaying];
        m_TimeElapsed += _deltaTime;
        curClip.SampleAnimation(gameObject, m_TimeElapsed%curClip.length);
    }
    public void TickScale(int index, float _scale)
    {
        if (!CheckIndex(index))
            return;

        AnimationClip curClip = m_Animations[m_CurPlaying];
        curClip.SampleAnimation(gameObject, curClip.length * _scale);
    }
}
public class ParticleControlBase
{
    public Transform transform { get; private set; }
    public ParticleSystem[] m_Particles { get; private set; }
    public ParticleControlBase(Transform _transform)
    {
        transform = _transform;
        m_Particles = transform ? transform.GetComponentsInChildren<ParticleSystem>() : new ParticleSystem[0];
    }
    public void Play()
    {
        m_Particles.Traversal((ParticleSystem particle) => {
            particle.Simulate(0, true, true);
            particle.Play(true);
            ParticleSystem.MainModule main = particle.main;
            main.playOnAwake = true;
        });
    }
    public void Stop()
    {
        m_Particles.Traversal((ParticleSystem particle) => {
            particle.Stop(true);
            ParticleSystem.MainModule main = particle.main;
            main.playOnAwake = false;
        });
    }
    public void Clear()
    {
        m_Particles.Traversal((ParticleSystem particle) => { particle.Clear(); });
    }
    public void SetActive(bool active)
    {
        m_Particles.Traversal((ParticleSystem particle) => { particle.transform.SetActivate(active); });
    }
}
#endregion

#region UI Classes
public class AtlasLoader
{
    protected Dictionary<string, Sprite> m_SpriteDic { get; private set; } = new Dictionary<string, Sprite>();
    public bool Contains(string name) => m_SpriteDic.ContainsKey(name);
    public string m_AtlasName { get; private set; }
    public Sprite this[string name]
    {
        get
        {
            if (!m_SpriteDic.ContainsKey(name))
            {
                Debug.LogWarning("Null Sprites Found |" + name + "|"+m_AtlasName);
                return m_SpriteDic.Values.First();
            }
            return m_SpriteDic[name];
        }
    }
    public AtlasLoader(SpriteAtlas atlas)
    {
        m_AtlasName = atlas.name;
        Sprite[] allsprites=new Sprite[atlas.spriteCount];
        atlas.GetSprites(allsprites);
        allsprites.Traversal((Sprite sprite)=> { string name = sprite.name.Replace("(Clone)", ""); m_SpriteDic.Add(name, sprite); });
    }
}

public class AtlasAnim:AtlasLoader
{
    int animIndex=0;
    List<Sprite> m_Anims;
    public AtlasAnim(SpriteAtlas atlas):base(atlas)
    {
        m_Anims = m_SpriteDic.Values.ToList();
        m_Anims.Sort((a,b) =>
        {
            int index1 = int.Parse(System.Text.RegularExpressions.Regex.Replace(a.name, @"[^0-9]+", ""));
            int index2 = int.Parse(System.Text.RegularExpressions.Regex.Replace(b.name, @"[^0-9]+", ""));
            return   index1- index2;
        });
    }

    public Sprite Reset()
    {
        animIndex = 0;
        return m_Anims[animIndex];
    }

    public Sprite Tick()
    {
        animIndex++;
        if (animIndex == m_Anims.Count)
            animIndex = 0;
        return m_Anims[animIndex];
    }
}

class EnumSelection : TReflection.UI.CPropertyFillElement
{
    Text m_Text;
    List<string> m_Enums=new List<string>();
    ObjectPoolListComponent<int, Button> m_ChunkButton;
    public EnumSelection(Transform transform) : base(transform)
    {
        m_Text = transform.Find("Text").GetComponent<Text>();
        m_ChunkButton = new ObjectPoolListComponent<int, Button>(transform.Find("Grid"), "GridItem");
        transform.GetComponent<Button>().onClick.AddListener(() => {
            m_ChunkButton.transform.SetActivate(!m_ChunkButton.transform.gameObject.activeSelf);
        });
        m_ChunkButton.transform.SetActivate(false);
    }

    public void Init<T>(T defaultValue, Action<int> OnClick)
    {
        m_Text.text = defaultValue.ToString();
        m_ChunkButton.Clear();
        TCommon.TraversalEnum((T temp) =>
        {
            int index = (int)((object)temp);
            Button btn = m_ChunkButton.AddItem(index);
            btn.onClick.RemoveAllListeners();
            btn.GetComponentInChildren<Text>().text = temp.ToString();
            btn.onClick.AddListener(() => {
                m_Text.text = temp.ToString();
                OnClick(index);
                m_ChunkButton.transform.SetActivate(false);
            });
        });
    }
    public void Init(List<string> values, string defaultValue,Action<int> OnClick)
    {
        m_Text.text = defaultValue.ToString();
        m_ChunkButton.Clear();
        values.Traversal((int index,string temp) =>
        {
            Button btn = m_ChunkButton.AddItem(index);
            btn.onClick.RemoveAllListeners();
            btn.GetComponentInChildren<Text>().text = temp.ToString();
            btn.onClick.AddListener(() => {
                m_Text.text = temp.ToString();
                OnClick(index);
                m_ChunkButton.transform.SetActivate(false);
            });
        });
    }
}
#endregion

#if UNITY_EDITOR
#region GizmosExtend
public static class Gizmos_Extend
{
    public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, Vector3 _scale, float _radius, float _height)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, _scale)))
        {
            if (_height > _radius * 2)
            {
                Vector3 offsetPoint = Vector3.up * (_height - (_radius * 2)) / 2;

                UnityEditor.Handles.DrawWireArc(offsetPoint, Vector3.forward, Vector3.right, 180, _radius);
                UnityEditor.Handles.DrawWireArc(offsetPoint, Vector3.right, Vector3.forward, -180, _radius);
                UnityEditor.Handles.DrawWireArc(-offsetPoint, Vector3.forward, Vector3.right, -180, _radius);
                UnityEditor.Handles.DrawWireArc(-offsetPoint, Vector3.right, Vector3.forward, 180, _radius);

                UnityEditor.Handles.DrawWireDisc(offsetPoint, Vector3.up, _radius);
                UnityEditor.Handles.DrawWireDisc(-offsetPoint, Vector3.up, _radius);

                UnityEditor.Handles.DrawLine(offsetPoint + Vector3.left * _radius, -offsetPoint + Vector3.left * _radius);
                UnityEditor.Handles.DrawLine(offsetPoint - Vector3.left * _radius, -offsetPoint - Vector3.left * _radius);
                UnityEditor.Handles.DrawLine(offsetPoint + Vector3.forward * _radius, -offsetPoint + Vector3.forward * _radius);
                UnityEditor.Handles.DrawLine(offsetPoint - Vector3.forward * _radius, -offsetPoint - Vector3.forward * _radius);
            }
            else
            {
                UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.up, _radius);
                UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.right, _radius);
                UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
            }
        }
    }

    public static void DrawWireCube(Vector3 _pos, Quaternion _rot, Vector3 _cubeSize)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale)))
        {
            float halfWidth, halfHeight, halfLength;
            halfWidth = _cubeSize.x / 2;
            halfHeight = _cubeSize.y / 2;
            halfLength = _cubeSize.z / 2;

            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, halfHeight, halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, -halfLength), new Vector3(-halfWidth, halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, -halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, -halfHeight, -halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));

            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(halfWidth, -halfHeight, halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(-halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, -halfLength), new Vector3(halfWidth, -halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(-halfWidth, halfHeight, -halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));

            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(halfWidth, halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(-halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, -halfHeight, halfLength), new Vector3(halfWidth, -halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(-halfWidth, -halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));
        }
    }
    public static void DrawArrow(Vector3 _pos, Quaternion _rot, Vector3 _arrowSize)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale)))
        {
            Vector3 capBottom = Vector3.forward * _arrowSize.z / 2;
            Vector3 capTop = Vector3.forward * _arrowSize.z;
            float rootRadius = _arrowSize.x / 4;
            float capBottomSize = _arrowSize.x / 2;
            UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, rootRadius);
            UnityEditor.Handles.DrawWireDisc(capBottom, Vector3.forward, rootRadius);
            UnityEditor.Handles.DrawLine(Vector3.up * rootRadius, capBottom + Vector3.up * rootRadius);
            UnityEditor.Handles.DrawLine(-Vector3.up * rootRadius, capBottom - Vector3.up * rootRadius);
            UnityEditor.Handles.DrawLine(Vector3.right * rootRadius, capBottom + Vector3.right * rootRadius);
            UnityEditor.Handles.DrawLine(-Vector3.right * rootRadius, capBottom - Vector3.right * rootRadius);

            UnityEditor.Handles.DrawWireDisc(capBottom, Vector3.forward, capBottomSize);
            UnityEditor.Handles.DrawLine(capBottom + Vector3.up * capBottomSize, capTop);
            UnityEditor.Handles.DrawLine(capBottom - Vector3.up * capBottomSize, capTop);
            UnityEditor.Handles.DrawLine(capBottom + Vector3.right * capBottomSize, capTop);
            UnityEditor.Handles.DrawLine(capBottom + -Vector3.right * capBottomSize, capTop);
        }
    }
    public static void DrawCylinder(Vector3 _pos, Quaternion _rot, float _radius, float _height)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale)))
        {
            Vector3 top = Vector3.forward * _height;

            UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
            UnityEditor.Handles.DrawWireDisc(top, Vector3.forward, _radius);

            UnityEditor.Handles.DrawLine(Vector3.right * _radius, top + Vector3.right * _radius);
            UnityEditor.Handles.DrawLine(-Vector3.right * _radius, top - Vector3.right * _radius);
            UnityEditor.Handles.DrawLine(Vector3.up * _radius, top + Vector3.up * _radius);
            UnityEditor.Handles.DrawLine(-Vector3.up * _radius, top - Vector3.up * _radius);
        }
    }
    public static void DrawTrapezium(Vector3 _pos, Quaternion _rot, Vector4 trapeziumInfo)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale)))
        {
            Vector3 backLeftUp = -Vector3.right * trapeziumInfo.x / 2 + Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
            Vector3 backLeftDown = -Vector3.right * trapeziumInfo.x / 2 - Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
            Vector3 backRightUp = Vector3.right * trapeziumInfo.x / 2 + Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
            Vector3 backRightDown = Vector3.right * trapeziumInfo.x / 2 - Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;

            Vector3 forwardLeftUp = -Vector3.right * trapeziumInfo.w / 2 + Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
            Vector3 forwardLeftDown = -Vector3.right * trapeziumInfo.w / 2 - Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
            Vector3 forwardRightUp = Vector3.right * trapeziumInfo.w / 2 + Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
            Vector3 forwardRightDown = Vector3.right * trapeziumInfo.w / 2 - Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;

            UnityEditor.Handles.DrawLine(backLeftUp, backLeftDown);
            UnityEditor.Handles.DrawLine(backLeftDown, backRightDown);
            UnityEditor.Handles.DrawLine(backRightDown, backRightUp);
            UnityEditor.Handles.DrawLine(backRightUp, backLeftUp);

            UnityEditor.Handles.DrawLine(forwardLeftUp, forwardLeftDown);
            UnityEditor.Handles.DrawLine(forwardLeftDown, forwardRightDown);
            UnityEditor.Handles.DrawLine(forwardRightDown, forwardRightUp);
            UnityEditor.Handles.DrawLine(forwardRightUp, forwardLeftUp);

            UnityEditor.Handles.DrawLine(backLeftUp, forwardLeftUp);
            UnityEditor.Handles.DrawLine(backLeftDown, forwardLeftDown);
            UnityEditor.Handles.DrawLine(backRightUp, forwardRightUp);
            UnityEditor.Handles.DrawLine(backRightDown, forwardRightDown);
        }
    }
}
#endregion
#endif