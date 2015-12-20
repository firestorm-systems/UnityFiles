using UnityEngine;
using System.Collections;

public class Emotion_Detection : MonoBehaviour
{
	//As of DEC 8 7:47:10 contains 0 errors and works in concept
	//needs other systems to be created in order for it to be 
	//useful.
	public double Fear_Key;
	public double Fear_Mouse;
	public double[] pressTimes = new double[5];
	public double[] RotationTimes = new double[5];
	
	public float[] PredictionDATA;

	public double timeDiff;
	private bool TakeMouseDATA;

	private float LeftAverage;
	private float RightAverage;
	private float UpAverage;
	private float DownAverage;

	private int LeftAverage_N;
	private int RightAverage_N;
	private int UpAverage_N;
	private int DownAverage_N;

	public GameObject PlayerOBJ;

	private bool reseti;
//	private bool Predict_LR;

	private int OutOF;
	private int OutOF_4;
	
	public void Start() {
		TakeMouseDATA = true;
	}

	//#SECTION 1 Keyboard
	public double[] ShiftDown ()
	{
		print ("SHIFTED DOWN");
		double[] newArr = new double[pressTimes.Length];
		for (int i = 1; i < pressTimes.Length; i++) {
			newArr [i-1] = pressTimes [i];
		}
		return newArr;
	}

	public double ArrAverage_Key(double[] array) {
		double arrSum = 0.0;
		for (int i = 0; i < array.Length - 1; i++) {
			arrSum += (array[i+1] - array[i]);
		}
		double result = arrSum / (array.Length - 1);
		return result;
	}

	//#SECTION 2 Mouse
	public double[] Rotation ()
	{
		print ("SHIFTED DOWN");
		double[] newArr = new double[RotationTimes.Length];
		for (int i = 1; i < RotationTimes.Length; i++) {
			newArr [i-1] = RotationTimes [i];
		}
		return newArr;
	}
	
	public double ArrAverage_Mouse(double[] array) {
		double arrSum = 0.0;
		for (int i = 0; i < array.Length - 1; i++) {
			arrSum += (array[i+1] - array[i]);
		}
		double result = arrSum / (array.Length - 1);
		return result;
	}

	public void Init ()
	{
	}

	public void Update ()
	{
		//Keyboard DATA
		if (Input.GetKeyDown ("w")
		    || Input.GetKeyDown ("a")
		    || Input.GetKeyDown ("s")
		    || Input.GetKeyDown ("d")) {
			pressTimes = ShiftDown ();
			pressTimes [pressTimes.Length - 1] = Time.fixedTime - timeDiff;
		}
		Fear_Key = ArrAverage_Key (pressTimes);


		//Mouse DATA
		if (TakeMouseDATA)
		{

			int i;
			if(reseti){
				i = 0;
				reseti = false;
			}
			//double DisToMouse = Vector3.Distance(Player.position, Input.mousePosition);
		    for (i=0; i<5; i++){
				if(i==1){
					//double A_rot = Player.rotation.z;
				} else if (i==5){
					//double B_rot = Player.rotation.z;
					i=0;
					reseti = true;
					//MouseTrackingAverage(A_rot, B_rot, DisToMouse);
				}
			}
			Fear_Mouse = ArrAverage_Mouse(RotationTimes);
		}
	}


	//#SECTION 3 Takes mouse averages and makes a array of data
	public void MouseTrackingAverage (double A_rot, double B_rot, double DisToMouse)
	{
		double RotationChange = B_rot - A_rot;

		RotationTimes = Rotation ();
		RotationTimes [RotationTimes.Length - 1] = RotationChange;
//		#--or--
//		RotationTimes = Rotation ();
//		RotationTimes [RotationTimes.Length - 1] = Player.rotation.z;

	}

	//--------------------------------Prediction.cs----------------------------------
	//#SECTION 4 Takes raw data from instances of the player moving and calculates
	//if they made a right or left turn at an intersection. Then sends that data 
	//to be analyzed as a 1 or 0 for a running average.
	IEnumerator LeftRightAverageCalculation () {
		Vector3 Pos_A = PlayerOBJ.transform.position;
		yield return new WaitForSeconds(3);
		Vector3 Pos_B = PlayerOBJ.transform.position;

		if (Pos_A.x - Pos_B.x > 3){
			print ("Went Left");
			LeftRightAverageStorage(1,0,false);
		} else if (Pos_A.x - Pos_B.x < -3){
			print ("Went Right");
			LeftRightAverageStorage(0,1,false);
		}
	}

	//#SECTION 5 Takes raw data from instances of the player moving and calculates
	//if they went up or turned around at an intersection. Then sends that data 
	//to be analyzed as a 1 or 0 for a running average.
	IEnumerator UpDownAverageCalculation () {
		Vector3 Pos_A = PlayerOBJ.transform.position;
		yield return new WaitForSeconds(3);
		Vector3 Pos_B = PlayerOBJ.transform.position;
		
		if (Pos_A.y - Pos_B.y > 3){
			print ("Went Up");
			UpDownAverageStorage(1,0,false);
		} else if (Pos_A.y - Pos_B.y < -3){
			print ("Turned Around");
			UpDownAverageStorage(0,1,false);
		}
	}


	//#SECTION 6 Detects likelyhood to make a left or right turn. Messy but functioning.
	public void LeftRightAverageStorage (int InstanceLeft, int InstanceRight, bool Reset) 
	{
		if (InstanceLeft == 1){
			LeftAverage_N ++;
			OutOF ++;
		} else {
			RightAverage_N ++;
			OutOF ++;
		}
		
		LeftAverage  =  LeftAverage_N/OutOF;
		RightAverage =  RightAverage_N/OutOF;
		
		if(LeftAverage > RightAverage){ 
			PredictLRUD(0, LeftAverage);
			
		} else {
			float RightAverage_C = RightAverage*-1;
			PredictLRUD(0, RightAverage_C);	
		}
		
		if(Reset)
		{
			LeftAverage_N = 0;
			RightAverage_N = 0;
			OutOF = 0;
		}
		
	}

	//#SECTION 7 Detects likelyhood to move up or turn around. Messy but functioning.
	public void UpDownAverageStorage (int InstanceUp, int InstanceDown, bool Reset) 
	{
		if (InstanceUp == 1){
			UpAverage_N ++;
			OutOF_4 ++;
		} else {
			DownAverage_N ++;
			OutOF_4 ++;
		}
		
		UpAverage  =  UpAverage_N/OutOF_4;
		DownAverage =  DownAverage_N/OutOF_4;
		
		if(UpAverage > DownAverage){ 
			PredictLRUD(1, UpAverage);
			
		} else {
			float UpAverage_C = DownAverage*-1;
			PredictLRUD(1, UpAverage_C);	
		}
		
		if(Reset)
		{
			LeftAverage_N = 0;
			RightAverage_N = 0;
			OutOF = 0;
		}
		
	}

	//#SECTION 8 Takes data from both storage functions and uses it to predict the
	//player's next movement at an intersection. Currently has no working function
	//This data is calculated far in advance of a player approaching an intersection
	//so "traps" can be set. In a perfect version there would be several cases to 
	//look at.
	//----------------		//---		   ---
	//----------------		//---		   ---
	//						//---		   ---
	//		   				//---		   ---
	//---		   ---		//---		   ---
	//---		   ---		//---		   ---
	//---   	   ---		//---		   ---
	//	CASE 0: LR			//	CASE 1: UD
	//
	//---		   ---		//---		   ---
	//---		   ---		//---		   ---
	//						//---		   ---
	//		   				//		  	   ---
	//						//		  	   ---
	//---		   ---		//---		   ---
	//---   	   ---		//---		   ---
	//	CASE 2: LRUD		//	CASE 1: UDL
	//
	//And so on and so on. Most CASEs are variants of CASE 0 and 1 but none the less 
	//will require diffrent and individual functions. Why? A: Making a system that
	//can make interpret, understand, and predict this would be hard. It's easier
	//to make modules that are activated on a CASE by CASE basis. B: Players will
	//most likely behave diffrently depending on the oreintation of the intersection.
	public void PredictLRUD (int Case, float LeftRight)
	{
//		LeftRight = LeftAverage + RightAverage;

		if (Case == 0) {
			float LeftRightPredicton = PredictionDATA[0];
			if((LeftRight < .1)&&(LeftRight > -.1)){
				LeftRightPredicton = Random.Range(0,10);
				if(LeftRightPredicton > 5) {
					print ("Going Left");
					LeftRightPredicton = 1;
				} else if (LeftRightPredicton < 5){
					print ("Going Right");
					LeftRightPredicton = 0;
				}
			}

			if(LeftRight > .1){
				LeftRightPredicton = Random.Range(0,5);
				if(LeftRightPredicton > 3) {
					print ("Going Right");
					LeftRightPredicton = 0;
				} else {
					print ("Going Left");
					LeftRightPredicton = 1;
				}
			}

			if(LeftRight < -.1){
				LeftRightPredicton = Random.Range(0,5);
				if(LeftRightPredicton < 4) {
					print ("Going Left");
					LeftRightPredicton = 1;
				} else {
					print ("Going Right");
					LeftRightPredicton = 0;
				}
			}

			if(LeftRight > .5){
				LeftRightPredicton = Random.Range(0,3);
				if(LeftRightPredicton > 2) {
					print ("Going Right");
					LeftRightPredicton = 0;
				} else {
					print ("Going Left");
					LeftRightPredicton = 1;
				}
			}
			
			if(LeftRight < -.5){
				LeftRightPredicton = Random.Range(0,3);
				if(LeftRightPredicton < 3) {
					print ("Going Left");
					LeftRightPredicton = 1;
				} else {
					print ("Going Right");
					LeftRightPredicton = 0;
				}
			}
			PredictionDATA[0] = LeftRightPredicton;
		}

		if (Case == 1) {
			float UpDownPredicton = PredictionDATA[1];
			if((LeftRight < .1)&&(LeftRight > -.1)){
				UpDownPredicton = Random.Range(0,10);
				if(UpDownPredicton > 5) {
					print ("Going Up");
					UpDownPredicton = 1;
				} else if (UpDownPredicton < 5){
					print ("Going Down");
					UpDownPredicton = 0;
				}
			}
			
			if(LeftRight > .1){
				UpDownPredicton = Random.Range(0,5);
				if(UpDownPredicton > 3) {
					print ("Going Down");
					UpDownPredicton = 0;
				} else {
					print ("Going Up");
					UpDownPredicton = 1;
				}
			}
			
			if(LeftRight < -.1){
				UpDownPredicton = Random.Range(0,5);
				if(UpDownPredicton < 4) {
					print ("Going Up");
					UpDownPredicton = 1;
				} else {
					print ("Going Down");
					UpDownPredicton = 0;
				}
			}
			
			if(LeftRight > .5){
				UpDownPredicton = Random.Range(0,3);
				if(UpDownPredicton > 2) {
					print ("Going Down");
					UpDownPredicton = 0;
				} else {
					print ("Going Up");
					UpDownPredicton = 1;
				}
			}
			
			if(LeftRight < -.5){
				UpDownPredicton = Random.Range(0,3);
				if(UpDownPredicton < 3) {
					print ("Going Up");
					UpDownPredicton = 1;
				} else {
					print ("Going Down");
					UpDownPredicton = 0;
				}
			}
			PredictionDATA[1] = UpDownPredicton;
		}
	}	

}
