using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class HashHelper
{
    public static string HashToString(byte[] hash)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < hash.Length; ++i)
        {
            sb.Append(hash[i].ToString("x2"));
        }

        return sb.ToString();
    }

    public static uint CalculateXXHash32(byte[] buf, int len = -1, uint seed = 0)
    {
        uint hash = XXHash32.CalculateHash(buf, len, seed);
        return hash;
    }

    public static byte[] CalculateMD5(byte[] buf, int len = -1, uint seed = 0)
    {
        byte[] hash = MD5.Create().ComputeHash(buf);
        return hash;
    }
}
