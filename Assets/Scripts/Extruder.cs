/**
 * Extruder.cs
 *
 * A set consisting of a stepper, a nozzle, a heater, and a thermal sensor
 * The actual "position" of the extruder is set by the E axis in the high-level
 * simulation. At this level the axis value simply goes up or down.
 *
 * In this simulation an Extruder has an input (filament length x area = volume)
 * and an output, essentially the same volume, but with a different diameter and
 * length. For the purposes of extrusion, length is less useful than volume, since
 * layers are "squished" and are not perfect cylinders.
 *
 * Fortunately, here we are only concerned with the extruder in terms of its
 * position (with a Flow Multiplier applied). It's the slicer software that
 * worries about volume, and all axis movement for the extruder is pre-determined.
 * The main powers of this class are to perform these conversions, to be able to
 * state the amount of volume extruded and work back to the length of input filament.
 *
 * The idealized extruder makes extrusion happen instantly. A more accurate
 * simulation will calculate the pressure within the nozzle, and deal with delays
 * between advance/retract and the change in pressure that produces the output.
 * A simple interim solution can apply a delay factor rather than pressure.
 * The spans of time are relatively small, and we don't need to simulate ooze
 * or under-extrusion per se. But these can be shown to be semi-predictable.
 *
 * The extruder mechanics determine steps-per-mm, and motors can be considered
 * interchangeable, as with the other axes. An Extruder is therefore an Axis,
 * but so far we don't have a class for that. Should it _have_ an axis or _be_
 * an axis? is the question. In C++ it would inherit from Axis so it has the
 * axis methods and can be treated like any other Axis.
 *
 * Looking ahead to complex extruders, switching extruders, mixing extruders,
 * etc., yes a single extruder may have multiple axes. But at the high level
 * the GCode and the simulation can perhaps treat mixers as a single axis with
 * 3 dimensions. The usual GCode is G1 Ennn. Mixing could be G1 Errr:ggg:bbb,
 * for example. (Need to look closer at this.) In any case, there is more than
 * a single "E axis" active simultaneously for such extruders. Hopefully this
 * simulation work will help point the right direction to develop for these.
 * 
 */

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Extruder : MonoBehaviour {

	public float filament_diameter = 3; // sometimes 2.85
	public float nozzle_aperture = 0.5f; // for example

	public float flow_multiplier = 1; // flow control per-extruder
	public float e_position = 0; // position in mm of the private E axis

	public float distance_per_revolution;	// every axis has this, to be able to connect to a stepper

	float e_total_movement = 0;

	Heater heater = new Heater();

	void Awake() { }

	void Start() { }

	// Frequently, maybe more than 60fps
	void FixedUpdate() { }

	// Once per frame
	void Update () { }

	// Actual movement is based on flow
	public void apply_e_movement(float delta) {
		float actual_motion = delta * flow_multiplier;
		e_position += actual_motion;
		e_total_movement += actual_motion;
	}

	public void reset_e_auditing() { e_total_movement = 0; }

}

