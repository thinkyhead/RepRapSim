/**
 * SwapExtension.cs
 *
 * Swap an array element with the element after it
 */
using UnityEngine;

public static class SwapExtension {

  public static void Swap<T>(this T[] v, int i=0) {
    if (i >= 0 && i <= v.Length - 2) {
      // Debug.Log("Before Swap: " + v[0] + " | " + v[1]);
      T temp = v[i];
      v[i] = v[++i];
      v[i] = temp;
      // Debug.Log(" After Swap: " + v[0] + " | " + v[1]);
    }
  }

}
