/**
 * Stepper.cs
 *
 * While Planner computes stepper movement blocks based on XYZ destination and
 * decides what speeds to start and end with, Stepper manages actual block
 * execution, stepping all the axes together in unison so that they simultaneously
 * arrive at the target point for a given move.
 *
 * This class will do the very same thing, except it may also drive the "real axis"
 * positions more directly. It might be possible to run microstep often enough
 * that stepping alone can drive the axis in real-time, but it will be useful to
 * have a shortcut method here as well.
 *
 * We don't need to physically rotate any stepper motors or do realtime simulation
 * of stepping. However, the code which is essentially equivalent to Sprinter can
 * still be built-out here for future usage.
 *
 * Separation and pluggability:
 *
 * If you are simulating printing and switch from Cartesian to Delta, it should
 * be a seamless transition. So the main level of control for simulation is
 * high-level XYZ, not built up from steppers. So the emphasis should be to be
 * able to produce a snapshot of any machine with XYZ at any point.
 *
 * I was just reading about "Functional Programming" in which there is only one
 * solution to any set of inputs. That is to say, the machine is "pure" and
 * deterministic. That certainly applies to everything in this utility. Every XYZ
 * has definite corresponding "raw axis" values and definite "stepper" values.
 * This also applies to movements. Given a certain XY acceleration (mm/s/s) and
 * an XY offset, you should get the same results every time.
 *
 */

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class block_t {
  // Fields used by the bresenham algorithm for tracing the line
  long[] steps = new long[4];      // Step count along each axis
  long step_event_count;           // The number of step events required to complete this block
  long accelerate_until;           // The index of the step event on which to stop acceleration
  long decelerate_after;           // The index of the step event on which to start decelerating
  long acceleration_rate;          // The acceleration rate used for acceleration calculation
  char direction_bits;             // The direction bit set for this block (refers to *_DIRECTION_BIT in config.h)
  char active_extruder;            // Selects the active extruder
  #if ADVANCE
    long advance_rate;
    volatile long initial_advance;
    volatile long final_advance;
    float advance;
  #endif

  // Fields used by the motion planner to manage acceleration
  // float speed_x, speed_y, speed_z, speed_e;       // Nominal mm/sec for each axis
  float nominal_speed;                               // The nominal speed for this block in mm/sec
  float entry_speed;                                 // Entry speed at previous-current junction in mm/sec
  float max_entry_speed;                             // Maximum allowable junction entry speed in mm/sec
  float millimeters;                                 // The total travel of this block in mm
  float acceleration;                                // acceleration mm/sec^2
  char recalculate_flag;                    // Planner flag to recalculate trapezoids on entry junction
  char nominal_length_flag;                 // Planner flag for nominal speed always reached

  // Settings for the trapezoid generator
  long nominal_rate;                        // The nominal step rate for this block in step_events/sec
  long initial_rate;                        // The jerk-adjusted step rate at start of block
  long final_rate;                          // The minimal rate at exit
  long acceleration_st;                     // acceleration steps/sec^2
  long fan_speed;
  #if BARICUDA
    long valve_pressure;
    long e_to_p_pressure;
  #endif
  volatile char busy;

  public block_t() {
  	steps = new long[4];
  }
}

public class Stepper : MonoBehaviour {

	public block_t[] block_buffer = new block_t[10]; // A ring buffer

	block_t current_block;

	Motor[] motors = new Motor[4]; // each has a position expressed in steps

	void Awake() {
		block_buffer = new block_t[10];
	}

	void Start() {
	}

	// This is where we simulate the Stepper ISR
	void FixedUpdate() {
	}

	// Once per frame
	void Update () {
	}

	// Do all the things the Marlin Stepper ISR does.
	// Namely, get blocks and send pulses to Motors to turn them.
	void ISR() {

	}

}

