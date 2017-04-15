using System;
using System.Collections.Generic;
using UnityEngine;

// provides distinct colours by halving the available space for every request.
// For example, a sample list of colours when calling getColour repeatedly
// should look like: 0, 180, 90, 270, 45, 135, 225, 315...
public class ColourPool {
	bool[] pool = new bool[360];
	Dictionary<int, int> leases = new Dictionary<int, int>();
	public ColourPool () {
		for (int i = 0; i < pool.Length; i++) {
			pool [i] = false;
		}

	}

	public int getColour () {
		for (int i = 0; i < pool.Length; i++) {
			if (!pool [i]) {
				pool [i] = true;

				int res = GetColourForIndex (i);

				leases [res] = i;

				return res;
			}
		}
		Debug.LogWarning ("Can't produce any new colours");
		return 0;
	}

	private int GetColourForIndex (int index) {

		if (index == 0) {
			return 0;
		}

		int t = (int) Math.Floor (Math.Log (index, 2));
		float inc = (float) (180f / Math.Pow (2, t));

		int offset = 0;
		for (int j = 0; j < t; j++) {
			offset += (int) Math.Pow (2, j);
		}

		int ri = index - offset;

		return (int) Math.Round(((ri * 2) - 1) * inc);
	}

	public void releaseColour (int colour) {
		pool [leases [colour]] = false;

		leases.Remove (colour);
	}
}


