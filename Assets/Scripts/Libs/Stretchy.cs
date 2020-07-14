/**
 * Stretchy.cs
 *
 * Stretch an object (on its Z axis) between two point vectors.
 *
 * Subclasses can manipulate the targetPoint values to change the
 * target points in various ways. Our example is StretchyTethered
 * which attaches one or both ends of the stretch to a Transform.
 */
using UnityEngine;
using System.Collections;

//[NoExecuteInEditMode]
public class Stretchy : MonoBehaviour {

  protected Transform T;

  // This object might be nested in Transforms, so we need
  // the ratio between local and world scaling
  protected float scaleFactor = 1f;

  protected Vector3 stretchAxis = Vector3.forward;

  // Each end of this object will be tethered to a world point
  // the ratio between local and world scaling
  protected Vector3[] targetPoint = new Vector3[2];

  // Additional offsets from each target along the Z axis of the Stretchy
  public float[] targetMargin = new float[2] { 0, 0 };

  // Remember old positions to reduce computation
  Vector3[] oldTargetPoint = new Vector3[2];

  /**
   * Getter for the rotated end points of the "line" in world space
   */
  protected Vector3[] endPoints {
    get {
      Vector3 pos = T.position,
              ray = T.rotation * (stretchAxis * T.lossyScale.z / 2);   // Half the stretch (T.forward also works here)
      return new Vector3[2] { pos - ray, pos + ray };
    }
  }

  protected virtual void Awake() { T = transform; }

  /**
   * Prepare for dynamic stretching
   */
  protected virtual void Start() {
    // The proportion between world and local scale
    scaleFactor = (T.lossyScale.x != 0 && T.localScale.x != 0) ? T.lossyScale.x / T.localScale.x : 1;

    // Get the world positions of the ends and store as the current targetPoint
    InitTargetPoints();

    // Use the most-stretched axis as the stretch axis
    stretchAxis = Vector3.forward;
    // if (T.localScale.x > T.localScale.y) {
    //   if (T.localScale.x > T.localScale.z)
    //     stretchAxis = Vector3.right;
    // }
    // else if (T.localScale.y > T.localScale.z) // y>=x && y>z
    //   stretchAxis = Vector3.up;
  }

  /**
   * Re-scale and re-orient as-needed on each frame update
   */
  protected virtual void Update() {

    Vector3[] targetLocalPos = new Vector3[2];

    bool didMove = false;
    for (int i = 0; i < 2; i++) {
      // Get target position in world space
      Vector3 tlp = new Vector3(targetPoint[i].x, targetPoint[i].y, targetPoint[i].z);
      // Convert to the most local space, if different
      if (T.parent != null) tlp = T.parent.InverseTransformPoint(tlp);
      targetLocalPos[i] = tlp;
      if (oldTargetPoint[i] != tlp) { // Take note of any change in position
        oldTargetPoint[i] = tlp;
        didMove = true;
      }
    }
    if (!didMove) return;   // Exit if there was no relative change

    // Get the position of the second end relative to the first
    Vector3 targetDiff = targetLocalPos[1] - targetLocalPos[0];

    // Un-comment this to allow dynamic re-parenting
    // scaleFactor = T.lossyScale.z / T.localScale.z;

    float localDistance = targetDiff.magnitude,
          localMargin0 = targetMargin[0] / scaleFactor,
          localMargin1 = targetMargin[1] / scaleFactor,
          lengthScale = localDistance - (localMargin0 + localMargin1);

    // Only apply the stretch to Z scaling. XY scaling will be unaffected.
    Vector3 localScale = T.localScale;
    localScale.z = (lengthScale > 0) ? lengthScale : 0;
    T.localScale = localScale;

    // Position the line exactly half way between the targets
    T.localPosition = (targetLocalPos[0] + targetLocalPos[1]) * 0.5f;

    // Rotate the line to the angle between the objects
    if (localDistance > 0.0001f) T.localRotation = Quaternion.LookRotation(targetDiff);

    // Move off-center by the difference in localMargins
    T.localPosition += (localMargin0 - localMargin1) * T.forward * 0.5f;
  }

  /**
   * Init target points to the current line ends
   */
  protected void InitTargetPoints() {
    targetPoint = endPoints;
    oldTargetPoint = endPoints;
  }

  /**
   * Tether one of the ends to a world Point
   */
  public virtual void TetherEndToWorldPoint(int end, Vector3 point) {
    if (end == 0 || end == 1) targetPoint[end] = point;
  }

  /**
   * Swap which end attaches to which target point.
   *
   * On the next Update the object will be reoriented
   * (unless the target points are otherwise altered).
   */
  public void SwapTargetPoints() { targetPoint.Swap(); }

  public virtual void SwapTargets() { SwapTargetPoints(); }

} // end of Stretchy class
