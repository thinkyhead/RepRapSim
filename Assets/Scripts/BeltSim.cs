/**
 * BeltSim.cs
 *
 * A simulated belt that attaches to a moving carriage.
 */

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used

using UnityEngine;
using System.Collections;

public struct BeltPart {
	public Transform T;						// The belt part Transform
	public Vector3	fixedOffset, 	// The fixed endpoint of the belt part
									gripOffset; 	// Offset to apply to the moving end
}

public class BeltSim : MonoBehaviour {
	Transform T;

	public Transform[]	belt = new Transform[2];	// The two belt slices
	public Transform		movingGrip;								// The carriage, most likely

	BeltPart[] beltParts = new BeltPart[2];

	Vector3 oldMovingPos = Vector3.one;

	void Awake() {

		T = transform;

		Vector3 movingPos = T.InverseTransformPoint(movingGrip.position);

		for (int i=0; i<=1; i++) {
			beltParts[i].T = belt[i];
			BeltPart bp = beltParts[i];
			Transform bpT = bp.T;

			Vector3 center = bpT.localPosition, end1, end2;												// Get the center point of the belt
			end1 = center + Vector3.right * bpT.localScale.x / 2;									// Get the far end of the un-rotated object
			end1 = end1.RotateAroundPivot(center, bpT.localRotation);							// Rotate around the center to get the true endpoint
			end2 = end1.RotateAroundPivot(center, Quaternion.Euler(0,0,180));			// Rotating 180° gives us the other end
			// Vector3 beltSlope = (end2 - end1).normalized; 											// useless for this code, but this is how you get it!

			if ((end1 - movingPos).magnitude < (end2 - movingPos).magnitude) {		// Which end is closer to the grip?
				beltParts[i].fixedOffset = end2;
				beltParts[i].gripOffset = end1 - movingPos;
			}
			else {
				beltParts[i].fixedOffset = end1;
				beltParts[i].gripOffset = end2 - movingPos;
			}

		}
	}

	void Update() {

		if (oldMovingPos == movingGrip.localPosition) return;
		oldMovingPos = movingGrip.localPosition;

		// Loop through the belt parts
		foreach (BeltPart bp in beltParts) {
			Transform bpT = bp.T;

			// Get the moving end in local space and apply the local offset
			Vector3 movingEndPos = T.InverseTransformPoint(movingGrip.position) + bp.gripOffset,
							diff = movingEndPos - bp.fixedOffset;

			bpT.localPosition = (bp.fixedOffset + movingEndPos) * 0.5f;
			bpT.localScale = new Vector3(diff.magnitude, 1, 6);
			bpT.localRotation = Quaternion.Euler(0, 0, Vector3.Angle(diff, Vector3.right));
		}
	}
}

public static class RotateAroundPivotExtensions {
	//Returns the rotated Vector3 using a Quaterion
	public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Quaternion Angle) {
		return Angle * (Point - Pivot) + Pivot;
	}
	//Returns the rotated Vector3 using Euler
	public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Vector3 Euler) {
		return RotateAroundPivot(Point, Pivot, Quaternion.Euler(Euler));
	}
	//Rotates the Transform's position using a Quaterion
	public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Quaternion Angle) {
		Me.position = Me.position.RotateAroundPivot(Pivot, Angle);
	}
	//Rotates the Transform's position using Euler
	public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Vector3 Euler) {
		Me.position = Me.position.RotateAroundPivot(Pivot, Quaternion.Euler(Euler));
	}
}
