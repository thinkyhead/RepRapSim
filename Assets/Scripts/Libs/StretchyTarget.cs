/**
 * StretchyTarget.cs
 *
 * A simple behavior for a Stretchy target.
 *
 * This class gathers target properties together and provides
 * convenience methods to get and set some target properties.
 *
 */
using UnityEngine;
using System.Collections;

public class StretchyTarget : MonoBehaviour {

  protected Transform T;

  // Flag whether this target is used
  private bool _isFree = true;

  public bool isFree {
    get { return _isFree; }
    set { _isFree = value; }
  }

  // Use this as the Stretchy's margin for the end on on initilization
  public float defaultMargin = 0;

  // Flags to make processing simpler, either world or local
  public bool useWorldOffset = false,
              useLocalOffset = true;

  // Offsets from the target allow anchoring to off-center points
  // [SerializeField]
  private Vector3 _worldOffset = Vector3.zero, // This world-level offset is applied last
                  _localOffset = Vector3.zero; // This offset scales and rotates with the target

  // Getter and setter for a local offset that rotates and scales with the target
  // If set to Vector3.zero the useLocalOffset flag will be cleared
  public Vector3 localOffset {
    get { return _localOffset; }
    set {
      _localOffset = value;
      useLocalOffset = value != Vector3.zero;
    }
  }

  // Getter and setter for a world offset
  // If set to Vector3.zero the useWorldOffset flag will be cleared
  public Vector3 worldOffset {
    get { return _worldOffset; }
    set {
      _worldOffset = value;
      useWorldOffset = value != Vector3.zero;
    }
  }

  // Getter and setter for my position in world coordinates
  public Vector3 position {
    get { return T.position; }
    set { T.position = value; }
  }

  // Getter for my tether point in world coordinates
  // Use this point in Update as the end of the Stretchy
  // Adds local and world offsets to the current world position.
  public Vector3 worldTetherPoint {
    get {
      Vector3 wtp = useLocalOffset ? T.TransformPoint(localOffset) : T.position;
      return useWorldOffset ? wtp + worldOffset : wtp;
    }
  }

  // Difference vector (in world space) from my center to a (world) point
  public Vector3 WorldOffsetToPoint(Vector3 point) { return point - T.position; }

  // Difference vector (in the local coordinate space) from my center to a (world) point
  public Vector3 LocalOffsetToPoint(Vector3 point) { return T.InverseTransformPoint(point); }

  // Linear distance (in world space) from my center to a (world) point
  public float WorldDistanceToPoint(Vector3 point) { return WorldOffsetToPoint(point).magnitude; }

  // Linear distance (in the local coordinate space) from my center to a (world) point
  public float LocalDistanceToPoint(Vector3 point) { return LocalOffsetToPoint(point).magnitude; }

  // Awake: Cache the transform
  public void Awake() { T = transform; }

  // Set both offsets at once. Useful for initialization
  public void SetOffsets(Vector3 localOffs, Vector3 worldOffs) {
    localOffset = localOffs;
    worldOffset = worldOffs;
  }

  // To set a world offset and disable the local offset...
  public void SetWorldOffsetOnly(Vector3 worldOffs) {
    worldOffset = worldOffs;
    useLocalOffset = false;
  }

  // To set a local offset and disable the world offset...
  public void SetLocalOffsetOnly(Vector3 localOffs) {
    localOffset = localOffs;
    useWorldOffset = false;
  }

  // To use the current world offset only...
  public void UseWorldOffsetOnly() {
    useWorldOffset = true;
    useLocalOffset = false;
  }

  // To use the current local offset only...
  public void UseLocalOffsetOnly() {
    useLocalOffset = true;
    useWorldOffset = false;
  }

  // To clear and disable the offsets completely
  public void ClearOffsets() { localOffset = worldOffset = Vector3.zero; }

} // end of StretchyTarget class
