using System;
using Unity.Mathematics;
using static umath;
using static kmath;
using static Unity.Mathematics.math;

public interface IPolynomial
{
    public float Evaluate(float _x);
    public int GetRoots(out float[] _roots);
}

[Serializable]
public struct LinearPolynomial : IPolynomial
{
    public float linear, constant;
    
    public LinearPolynomial(float _linear, float _constant)
    {
        linear = _linear;
        constant = _constant;
    }
    public float Evaluate(float _x)=>  _x*linear + constant;
    public int GetRoots(out float[] _roots) => GetRoots(linear, constant, out _roots);

    public static int GetRoots(float _linear,float _constant,out float[] _roots){
        _roots = null;
        if (_linear == 0)
            return 0;
        
        _roots = new float[] {-_constant/_linear};
        return 1;
    }
}

[Serializable]
public struct QuadraticPolynomial :IPolynomial
{
    public float quadratic, linear, constant;
    public QuadraticPolynomial(float _quadratic, float _linear, float _constant)
    {
        quadratic = _quadratic;
        linear = _linear;
        constant = _constant;
    }
    
    public float Evaluate(float _x)=> umath.pow2(_x)*quadratic + _x*linear + constant;
    public int GetRoots(out float[] _roots) => GetRoots(quadratic, linear, constant, out _roots);

    public static int GetRoots(float _quadratic, float _linear, float _constant,out float[] _roots)
    {
        if (_quadratic == 0)
            return LinearPolynomial.GetRoots(_linear, _constant,out _roots);
        
        _roots = null;
        var d = _linear * _linear - 4 * _quadratic * _constant;
        if (d < 0)
            return 0;

        var sqrtD = math.sqrt(d);
        var value = (-_linear + sqrtD) / (2 * _quadratic);
        
        if (d == 0)
        {
            _roots = new float[] {value};
            return 1;
        }
        
        var value2 = (-_linear - sqrtD) / (2 * _quadratic);
        _roots = new float[] {value,value2};
        return 2;
    }
    
    public override string ToString() => $"{quadratic}x² {linear.ToStringSigned()}x {constant.ToStringSigned()}";

    
}

[Serializable]
public struct CubicPolynomial : IPolynomial
{
    public float cubic, quadratic, linear, constant;
    public CubicPolynomial(float _cubic, float _quadratic, float _linear, float _constant)
    {
        cubic = _cubic;
        quadratic = _quadratic;
        linear = _linear;
        constant = _constant;
    }
    
    public float Evaluate(float _x) => pow3(_x) * cubic + pow2(_x) * quadratic + _x * linear + constant;


    public static int GetRoots(float _cubic,float _quadratic, float _linear, float _constant, out float[] _roots)
    {
        if (_cubic == 0)
            return QuadraticPolynomial.GetRoots(_quadratic,_linear, _constant, out _roots);
        
        var t = _cubic;
        var a = _quadratic;
        var b = _linear;
        var c = _constant;
        var p = -kInv9 * pow2(a) + kInv3 * b;
        var q = (1f / 27) * pow3(a) - kInv6 * a * b +  kInv2 * c;
        var D = -(pow3(p) + pow2(q));

        var sqrtNegD = sqrt(-D);
        var r = pow(-q + sqrtNegD  , kInv3);

        if (D == 0)
        {
            _roots = new[] {r, -r};
            return 2;
        }

        var omega = kInv3 * pow(cos(-q / sqrt(-pow3(p))), -1);
        var x2 = 2 * sqrt(-p) * cos(omega + kPI2 / 3);
        var x3 = 2 * sqrt(-p) * cos(omega - kPI2 / 3);
        
        _roots = new []{r,x2,x3};
        return 3;
    }

    public int GetRoots(out float[] _roots) => GetRoots(cubic, quadratic, linear, constant, out _roots);

    public override string ToString() => $"{cubic}x³ {quadratic.ToStringSigned()}x² {linear.ToStringSigned()}x {constant.ToStringSigned()}";
}
