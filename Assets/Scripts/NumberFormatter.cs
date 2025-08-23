using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NumberFormatter
{
    private const int BASE = 1000;
    private static string[] _symbols = BuildSymbols();

    private static string[] BuildSymbols()
    {
        List<string> stringList = new List<string>();

        //a부터 z까지 넣기
        for (char c = 'a'; c <= 'z'; c++)
        {
            stringList.Add(c.ToString());
        }

        //aa ~ zz까지 넣기
        for (char first = 'a'; first <= 'z'; first++)
        {
            for (char second = 'a'; second <= 'z'; second++)
            {
                stringList.Add($"{first}{second}");
            }
        }

        return stringList.ToArray();
    }

    public static string FormatNumber(double value)
    {
        if (value < BASE)
        {
            return value.ToString("N0");
        }

        int index = 0;

        //BASE단위로 나누며 몇 번째 알파벳 쓸건지 결정함
        //1530이 들어옴, value = 1.530, index = 1
        //표기 : 1.53a
        while (value >= BASE && index < _symbols.Length)
        {
            value /= BASE;
            index++;
        }

        return $"{value:F2}{_symbols[index - 1]}";
    }
}