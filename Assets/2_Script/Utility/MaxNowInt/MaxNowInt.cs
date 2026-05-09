using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct MaxNowInt
{
    public int max { get; private set; }

    private int _now;
    public int now
    {
        get => _now;
        set
        {
            if (value > max) value = max;
            else if (value < 0) value = 0;
            _now = value;
        }
    }

    public MaxNowInt(int max)
    {
        this.max = max;
        this._now = max;
    }

    // += 연산자
    public static MaxNowInt operator +(MaxNowInt before, int value)
    {
        before.now += value;
        return before;
    }

    // -= 연산자
    public static MaxNowInt operator -(MaxNowInt before, int value)
    {
        before.now -= value;
        return before;
    }

    // ------------------------
    // MaxNowInt ↔ int 비교 연산자
    // ------------------------
    public static bool operator <(MaxNowInt a, int b) => a.now < b;
    public static bool operator >(MaxNowInt a, int b) => a.now > b;
    public static bool operator <=(MaxNowInt a, int b) => a.now <= b;
    public static bool operator >=(MaxNowInt a, int b) => a.now >= b;
    public static bool operator ==(MaxNowInt a, int b) => a.now == b;
    public static bool operator !=(MaxNowInt a, int b) => a.now != b;

    // 반대 방향 비교 (int ↔ MaxNowInt)
    public static bool operator <(int a, MaxNowInt b) => a < b.now;
    public static bool operator >(int a, MaxNowInt b) => a > b.now;
    public static bool operator <=(int a, MaxNowInt b) => a <= b.now;
    public static bool operator >=(int a, MaxNowInt b) => a >= b.now;
    public static bool operator ==(int a, MaxNowInt b) => a == b.now;
    public static bool operator !=(int a, MaxNowInt b) => a != b.now;

    // Equals / GetHashCode
    public override bool Equals(object obj)
    {
        if (obj is MaxNowInt other) return this.now == other.now && this.max == other.max;
        if (obj is int value) return this.now == value; // int 비교 지원
        return false;
    }

    public override int GetHashCode() => now.GetHashCode() ^ max.GetHashCode();


    // now를 max로 되돌림
    public void Reset()
    {
        this.now = this.max;
    }
}
