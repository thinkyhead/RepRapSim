/**
 * BasicPrinter.cs
 *
 * A basic printer which can handle cartesian motion.
 *
 * In this Unity project each unit is a millimeter, so this is pretty easy.
 *
 * This models the physical components of the 3D printer. For most (if not
 * all) designs, the model will be parametric, so it needs to expose some of
 * these parameters to the simulation. The idea is that the low-level print
 * simulation can be adapted to a changed environment on the fly.
 * So, things like steps-per-mm are broken up into two parts. In this case,
 * steps-per-revolution and distance-per-revolution. Steppers are programmed
 * simply to perform a certain number of steps in real-time, applying
 * acceleration logic. The feedback to the printer itself will be driven by
 * the steppers.
 *
 * Note that this makes it harder to maintain the print simulation while
 * changing from one printer type to another. The number of steppers may
 * change altogether. The implication is that there must be an intermediate
 * model of the XYZE positioning independent of any stepper-driven version.
 * This can be accomplished by applying two modes. In the basic mode the
 * GCode simulator simply commands the XYZE axes directly, and this feeds
 * directly to the printer model. (So Printer sub-classes must be XYZE
 * positionable.)
 *
 * There will be a basic G-Code parser as a separate module, at least
 * capable of G0 / G1. Each add-on command (e.g., G29) will be in its
 * own module.
 *
 * Initially going for a single extruder. This will become its own class
 * later on, as will each component. For now we simply calculate here
 * the various numbers that a firmware like Marlin uses to assign actual
 * steps to the floating-point XYZE positions.
 *
 * The most challenging thing to model here is Acceleration and Jerk –
 * fairly high-level subjects. Coordination between at least 2 (and
 * potentially 3) motors is simple enough, if all you want is instant
 * steps-per-mm, as a simulator can easily fake. But for accurate acceleration
 * we have to crib from dissertations on the workings of firmware.
 */

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public enum PrinterState {
	UNHOMED,
	HOMED,
	CALIBRATING_BED,
	SD_PRINTING,
	ENDSTOP_ABORT
}

public enum BuildPlateType {
	ALUMINIUM,
	GLASS,
	MIRROR,
	KAPTON,
	BLUE_TAPE,
	UNSET
}

public enum ExtruderType {
	WADE1,
	WADE2,
	MINI,
	TWEETY,
	UNSET
}

public enum FrameType {
	LASER_CUT_WOOD,
	LASER_CUT_ALUM,
	LASER_CUT_STEEL,
	BOX_FRAME,
	UNSET
}

// const float UNKNOWN_POSITION = -9999f;

[ExecuteInEditMode]
public class BasicPrinter : MonoBehaviour {

	string[] axisName;

	int code_value_int;
	float code_value_float;

	Vector3 objectHome = new Vector3(-95, 124, 108.4f);

	public BuildPlateType buildPlateType = (BuildPlateType)0;
	public ExtruderType extruderType = (ExtruderType)0;
	public FrameType frameType = (FrameType)0;
	[Range(0, 720)] public float sliderAngle = 0;
	[Range(0, 190)] public float sliderX = 95;
	[Range(0, 190)] public float sliderY = 95;
	[Range(0, 200)] public float sliderZ = 95;
	[Range(0, 200)] public float smoothRodLength;

	float oldX = -1, oldY = -1, oldZ = -1, oldA = 0;
	BuildPlateType oldBuildPlateType = BuildPlateType.UNSET;
	ExtruderType oldExtruderType = ExtruderType.UNSET;
	FrameType oldFrameType = FrameType.UNSET;

	// Printer parts that we need to know about
	GameObject motorX, motorY, motorZ1, motorZ2, motorE0;
	GameObject[] endstop = new GameObject[3];
	GameObject[] carriage = new GameObject[3];
	GameObject[] buildPlate = new GameObject[4];

	// Printer state.
	bool[] known_position = new bool[3] { false, false, false }; // Yes, C# will do this for us

	// The current printer XYZ
	// This is distinct from "nozzlePosition" which might be different if a current offset is applied
	Vector3 currentPosition, destination;

	// A test G-Code string to play
	string test_gcode = "G28\nG0 Z10 F9000\nG0 X100 Y100 F3000\nG0 Z50 F9000\nG0 Z10";

	// UI objects
	public Text labelX, labelY, labelZ;

	float fixedUpdateCount = 1, updateCount = 1;

	// UI Touches
	float uiTouchDelta, uiTouchSpeed, uiTouchLastX, uiTouchRatio;
	int uiTouchPause, uiTouchID;
	bool uiTouchHasData;

	// Init early
	void Awake() {
		sliderAngle = 0;
		sliderX = sliderY = 95;
		sliderZ = 0;
	}

	// Init when adding to the world
	void Start() {
		axisName = new string[3] { "X", "Y", "Z" };
		currentPosition = destination = Vector3.zero;
		for (int i=0; i<3; i++) {
			carriage[i] = GameObject.FindGameObjectsWithTag(axisName[i] + "_CARRIAGE")[0];
			// endstop[i] = GameObject.FindGameObjectsWithTag(axisName[i] + "_ENDSTOP")[0];
		}

		// Find all build plates, disable them
		/*
		int j = 0;
		foreach (GameObject o in GameObject.FindGameObjectsWithTag("BUILD_PLATE")) {
			buildPlate[j++] = o;
			o.SetActive(false);
		}
		*/

		// Touch events
		uiTouchDelta = uiTouchSpeed = uiTouchPause = 0;
		uiTouchRatio = 1;
		uiTouchID = -1;
		uiTouchHasData = false;

		fixedUpdateCount = updateCount = 1;
	}

	//
	// Rotate according to a finger slide - code adapted from GOTH
	//
	void FixedUpdate() {
		// Track the ratio between update and fixed update
		// so delta values from Update can be scaled for FixedUpdate
		float updateRatio = updateCount / ++fixedUpdateCount;

		if (uiTouchID == -1) {															// no finger touching or dragging?
			if (uiTouchSpeed < -0.01f || uiTouchSpeed > 0.01f) {	// is there some velocity?
				transform.eulerAngles += Vector3.up * uiTouchSpeed;	// then apply it
				uiTouchSpeed *= 0.94f;												// and slow down by 5%
			}
		}
		else if (uiTouchHasData) { 												// whenever a finger is touching
			uiTouchHasData = false;													// got the data
			uiTouchPause = 200;															// restart the finger pause counter
			transform.eulerAngles += Vector3.up * uiTouchDelta;	// move the printer directly with the finger
			uiTouchSpeed = uiTouchDelta * updateRatio * 0.90f; // save the last movement for momentum when let-go
			uiTouchDelta = 0;																// clear the motion since we just used it
		}
	}

	//
	// Deal with mouse and touch events
	//
	// 1. Touch and drag on the printer to rotate it on the Y axis.
	// 2. Tap on the printer's X, Y, or Z axis to move it directly.
	//
	void HandleTouches() {
		// This code also works for touch devices
		if (Input.GetMouseButton(0)) {
			float mouseX = Input.mousePosition.x, mouseY = Input.mousePosition.y;
			if (uiTouchID == -1 && mouseX < Screen.width / 2) {
				uiTouchDelta = 0;
				uiTouchID = 1;
				uiTouchRatio = (1f + mouseY / Screen.height) * 0.25f;
			}
			else {
				uiTouchDelta += (uiTouchLastX - mouseX) * uiTouchRatio;
				uiTouchHasData = true;
			}
			uiTouchLastX = mouseX;
		}
		else uiTouchID = -1;
	}

	// Update is called once per frame
	void Update () {
		float d = Time.deltaTime;
		updateCount++;

		HandleTouches();

		if (sliderAngle != oldA) {
			oldA = sliderAngle;
			float a = sliderAngle * 3.1415926f / 180f;
			// float a = sliderAngle;
			sliderX = Mathf.Sin(a) * 80f + 95f;
			sliderY = Mathf.Cos(a) * 80f + 95f;
		}

		if (sliderX != oldX) {
			oldX = sliderX;
			Vector3	xpos = carriage[0].transform.localPosition;
			xpos.x = sliderX + objectHome.x;
			carriage[0].transform.localPosition = xpos;
			labelX.text = "X " + (int)sliderX;
		}

		if (sliderY != oldY) {
			oldY = sliderY;
			Vector3	ypos = carriage[1].transform.localPosition;
			ypos.z = objectHome.y - sliderY;
			carriage[1].transform.localPosition = ypos;
			labelY.text = "Y " + (int)sliderY;
		}

		if (sliderZ != oldZ) {
			oldZ = sliderZ;
			Vector3	zpos = carriage[2].transform.localPosition;
			zpos.y = sliderZ + objectHome.z;
			carriage[2].transform.localPosition = zpos;
			labelZ.text = "Z " + sliderZ.ToString("F2");
		}

		if (buildPlateType != oldBuildPlateType) {
			oldBuildPlateType = buildPlateType;
			/*
			Debug.Log("Bed Type = " + buildPlateType);
			int j = 0;
			foreach (GameObject o in buildPlate) {
				if (o != null) {
					bool ena = (int)buildPlateType == j;
					if (ena) Debug.Log("Set " + o.name + " active");
					o.SetActive(ena);
				}
				j++;
			}
			*/
		}

		if (extruderType != oldExtruderType) {
			oldExtruderType = extruderType;
		}

		if (frameType != oldFrameType) {
			oldFrameType = frameType;
		}
	}

	public void onXChange(float value) {
		sliderX = value;
		// Debug.Log("X = " + value);
	}

	public void onYChange(float value) {
		sliderY = value;
		// Debug.Log("Y = " + value);
	}

	public void onZChange(float value) {
		sliderZ = value;
		// Debug.Log("Z = " + value);
	}

}

