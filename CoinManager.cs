using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static class CoinManager
{
    public static int currentCoinTotal;

    public static void AddGold(int amount)
    {
        currentCoinTotal += amount;
    }

    public static void RemoveGold(int amount)
    {
        currentCoinTotal -= amount;
    }

    public static string CalculateIncome(BigInteger number)
    {
        if (number < 1000) {
            return number.ToString("0");
        }

        if (number >= BigInteger.Pow(10, 48))
            return FormatBigInteger(number, 48, "Quind"); // Quindecillion (10^48)
        else if (number >= BigInteger.Pow(10, 45))
            return FormatBigInteger(number, 45, "Qd"); // Quattuordecillion (10^45)
        else if (number >= BigInteger.Pow(10, 42))
            return FormatBigInteger(number, 42, "Td"); // Tredecillion (10^42)
        else if (number >= BigInteger.Pow(10, 39))
            return FormatBigInteger(number, 39, "Dd"); // Duodecillion (10^39)
        else if (number >= BigInteger.Pow(10, 36))
            return FormatBigInteger(number, 36, "Ud"); // Undecillion (10^36)
        else if (number >= BigInteger.Pow(10, 33))
            return FormatBigInteger(number, 33, "Dc"); // Decillion (10^33)
        else if (number >= BigInteger.Pow(10, 30))
            return FormatBigInteger(number, 30, "No"); // Nonillion (10^30)
        else if (number >= BigInteger.Pow(10, 27))
            return FormatBigInteger(number, 27, "Oc"); // Octillion (10^27)
        else if (number >= BigInteger.Pow(10, 24))
            return FormatBigInteger(number, 24, "Se"); // Septillion (10^24)
        else if (number >= BigInteger.Pow(10, 21))
            return FormatBigInteger(number, 21, "Si"); // Sextillion (10^21)
        else if (number >= BigInteger.Pow(10, 18))
            return FormatBigInteger(number, 18, "Qi"); // Quintillion (10^18)
        else if (number >= BigInteger.Pow(10, 15))
            return FormatBigInteger(number, 15, "Q");  // Quadrillion (10^15)
        else if (number >= BigInteger.Pow(10, 12))
            return FormatBigInteger(number, 12, "T");  // Trillion (10^12)
        else if (number >= BigInteger.Pow(10, 9))
            return FormatBigInteger(number, 9, "B");   // Billion (10^9)
        else if (number >= BigInteger.Pow(10, 6))
            return FormatBigInteger(number, 6, "M");   // Million (10^6)
        else if (number >= BigInteger.Pow(10, 3))
            return FormatBigInteger(number, 3, "K");   // Thousand (10^3)
        else {
            return number.ToString("0.00");
        }
    }

    private static string FormatBigInteger(BigInteger number, int divisorExponent, string suffix)
    {
        BigInteger divisor = BigInteger.Pow(10, divisorExponent);
        double result = (double)(number / divisor) + (double)(number % divisor) / (double)divisor;

        return result.ToString("0.00") + suffix;
    }
}
