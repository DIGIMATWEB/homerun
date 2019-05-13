using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class BallController : MonoBehaviour {

	public bool debugDotProduct;

	public bool canMakeHomeRun = false;

	public GameObject ballObject;
	public SphereCollider mySphereCollider;
	GameObject parentBall;
	Vector3 localPositionsStartBall;
	Rigidbody myRigidbody;
	TrailRenderer myTrailRender;

	bool isCountingFeet = false;
	int numberMetersCounted = 0;
	//Vector2 initialPositionHitBat;

	[Header("Data to send ball")]
	public float timeSeparateFromHand = 1.0f;

	public Vector3 forceToSendBall;
	//public bool invertForceToSendBall;
	public ForceMode typeForce;

	public bool useRandomX_BallSend = false;
	public bool isBallBeingPlayed = false;

	[Header("Data to make homerun")]
	public Vector3 forceToMakeHomeRun;
	public ForceMode typeForceHomeRun;

	public bool useListPrefix = true;
	public bool useRandomX = true;
	public Vector2 rangeRandomX;
	public bool useForcesWithZero = true;

	public bool useRandomY = true;
	public Vector2 rangeRandomY;

	public bool useRandomZ = true;
	public Vector2 rangeRandomZ;

	public bool useMinForceX = false;
	public bool useMaxForceX = false;
	public bool useMinForceY = false;
	public bool useMaxForceY = false;
	public bool useMinForceZ = false;
	public bool useMaxForceZ = false;

	public GameObject objectToStartCountingMeters;
	public Vector2 positionObjectToStartCountingMeters;

	[Header("Extra animations")]
	public ParticleSystem homerunParticleSystem;

	[Header("Time to fix several collisions")]
	public float currentTimeCollision;

	[Header("Dot product")]
	public float resultDotProduct;
	public Transform batTransform;
	public GameObject extremeBat;
	public float rangeForDetectCollisionWithBatAndBall; //This value if to detect if the ball is near the bat.
	public float rangeForCollisionWithPerfectReference = 0; //This value is to determine is the bat is near to the perfect reference for a good hit bat hit.
	public GameObject objectPerfectReference;
	public List<float> listDotProductToDetectSwing;
	public List<Vector3> listPositionBat;
	public int numberPositionsToTakeSwing = 20;
	//For velocity:
	public float minVelocityBat = 0; 
	public List<float> listVelocities;
	public Vector3 previousBatPosition;
	public float previousTime;
	public bool ballWasAlreadyHit = false;
	bool use2DDotProduct = false;

	[Header("Data to make homerun using dot product")]
	public float rangeYDistanceForGoodHomeRun = 0.1f; // this value help to make a good home run. Its a range, if you are not in this range, the ball will go up or down.

	public List<Vector3> listPositionsFixedForHomeRun;
	Vector3 previousHomeRunForce = Vector3.zero;

	[Header("Debug")]
	public GameObject prefabBallShadowDebug;
	List<GameObject> listPrefabBallShadows;
	public GameObject prefabBatShadowDebug;
	List<GameObject> listPrefabBatShadows;
	public GameObject spherePerfectReference;

	public Texture2D textureBallWhite;
	public Texture2D textureBallRed;
	bool fixingBatPosition = false;

	[Header("Debug temp")]
	public bool useDebugTemp = false;
	public Vector3 foceFinal;
	public Text debugMessage;
	public Text lastVelocityBatDebug;

	[Header("Only to make ball still roll")]
	bool canCollideAgain = true;
	public Vector3 forceToMakeBallDown;

	[Header("Score")]
	public int hitsInARow = 0; //For calculating the number of consecutive hits.

	[Header("Fix Y position jump")]
	//public float deltaYFix = 0.05f;
	//public float amountYFix = 0.001f;
	//public float timeSlerpFixY = 0.1f;
	public bool checkBallToFixY = false;
	//float separationToFix = 0;
	//float speedToFix = 0;
	
    //V2 and V3
   // public float currentTimeFix = 0;
    //public float maxTimeFix = 0.3f;
    public float deltafix = 0;
    Vector3 initialPositionBallFix;
    float halfDistanceX_PerfectHit_Ball;

    [Header("Velocity ball:")]
    public float forceXSendBall;

    [HideInInspector]
    public Transform cameraToParent;

    float ballAltitudeVAR;
    public static bool YballH = false;
    public static bool FlagINtrajectory = false;
    public static bool FlagAnimator = false;
    void Start () {
        listDotProductToDetectSwing = new List<float>();
		listPositionBat = new List<Vector3>();
		listVelocities = new List<float>();
		listPrefabBallShadows = new List<GameObject>();
		listPrefabBatShadows = new List<GameObject>();
		myRigidbody = GetComponent<Rigidbody>();
		myTrailRender = ballObject.GetComponent<TrailRenderer>();

		//rangeForDetectCollisionWithBatAndBall = float.Parse (XMLConfiguration.GetSettings()["rangeForDetectCollisionWithBatAndBall"]);
		//rangeForCollisionWithPerfectReference = float.Parse(XMLConfiguration.GetSettings()["rangeForCollisionWithPerfectReference"]);
		//rangeYDistanceForGoodHomeRun = float.Parse(XMLConfiguration.GetSettings()["rangeYForGoodHomeRun"]);

		ActualizarValores();

		float scaleBall = float.Parse(XMLConfiguration.GetSettings()["ScaleBall"]);
		if(scaleBall != 1f)
			ballObject.transform.localScale = Vector3.one*scaleBall;

		//minVelocityBat = float.Parse(XMLConfiguration.GetSettings()["minVelocityBat"].Split(',')[Registration.GetDifficulty()]);

		positionObjectToStartCountingMeters = new Vector2(objectToStartCountingMeters.transform.position.x, objectToStartCountingMeters.transform.position.z);

		objectPerfectReference.SetActive( bool.Parse(XMLConfiguration.GetSettings()["showPerfectBatHitPoint"]));
		use2DDotProduct = bool.Parse(XMLConfiguration.GetSettings()["use2DDotProduct"]);

		//GamePlayMetsHomuRunDerby.myslf.myBatRenderModel.GetComponent<Renderer>().material.color = Color.red;

		hitsInARow = 0;
	}
	void ActualizarValores()
	{
		
		rangeForDetectCollisionWithBatAndBall = float.Parse (XMLConfiguration.GetSettings()["rangeForDetectCollisionWithBatAndBall"]);
		rangeForCollisionWithPerfectReference = float.Parse(XMLConfiguration.GetSettings()["rangeForCollisionWithPerfectReference"].Split(',')[ManageData.GetDifficulty()]);
		rangeYDistanceForGoodHomeRun = float.Parse(XMLConfiguration.GetSettings()["rangeYForGoodHomeRun"].Split(',')[ManageData.GetDifficulty()]);

        ballAltitudeVAR = float.Parse(XMLConfiguration.GetSettings()["ballAltitude"]);

        minVelocityBat = float.Parse(XMLConfiguration.GetSettings()["minVelocityBat"].Split(',')[ManageData.GetDifficulty()]);

		spherePerfectReference.gameObject.SetActive( bool.Parse(XMLConfiguration.GetSettings()["showSpherePerfectReference"]));

        forceXSendBall = float.Parse(XMLConfiguration.GetSettings()["VelocityBallFromPitcher"].Split(',')[ManageData.GetDifficulty()]);
        if (forceXSendBall > 0)
            forceXSendBall *= -1f;
        if (forceXSendBall == 0)
            forceXSendBall = -27f; //-27 default value
    }
	void Update()
	{

		if(isCountingFeet)
		{
			//If it is multiplied by 3.28084f;, means they are feets:
			//Debug.Log("Transform pelota for points="+ transform.position.x+","+ transform.position.z);
			//numberMetersCounted = (int) (Vector2.Distance(initialPositionHitBat, new Vector2(transform.position.x, transform.position.z)) * 3.28084f);
			numberMetersCounted = (int) (Vector2.Distance(positionObjectToStartCountingMeters, new Vector2(transform.position.x, transform.position.z)) * 3.28084f);



			GamePlayMetsHomuRunDerby.ChangeUIText(UIController.TYPE_UI_ELEMENT.FEETS, numberMetersCounted);
		}
#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.Alpha0))
		{
			myRigidbody.isKinematic = true;
		}
#endif

		if(bool.Parse(XMLConfiguration.GetSettings()["isDebug"]))
		{
			if(Input.GetKeyDown(KeyCode.B))
			{
				fixingBatPosition = !fixingBatPosition;
				if(fixingBatPosition)
					Time.timeScale = 0.1f;
				else
					Time.timeScale = 1f;
			}

			if(fixingBatPosition)
			{

				string[] valuesPos = XMLConfiguration.GetSettings()["positionPerfectHit"].Split(',');
				UDPServer.myslf.positionPerfectReference = new Vector3(float.Parse(valuesPos[0]), float.Parse(valuesPos[1]), float.Parse(valuesPos[2]));
				
				objectPerfectReference.transform.position = UDPServer.myslf.positionPerfectReference;
			}
		}

        //if(checkBallToFixY)
        //{
        //	//if(transform.position.y < objectPerfectReference.transform.position.y+0.05f)

        //	if(transform.position.y > objectPerfectReference.transform.position.y)
        //	{
        //		transform.position = Vector3.Slerp(transform.position, new Vector3(transform.position.x,objectPerfectReference.transform.position.y ,transform.position.z), timeSlerpFixY);
        //	}

        //	if(transform.position.y < objectPerfectReference.transform.position.y)
        //	{
        //		checkBallToFixY = false;

        //		FreezeY();
        //	}

        //	/*Debug.LogWarning("transform.position.y - previousPosFix.y > deltaYFix="+ (transform.position.y - previousPosFix.y).ToString());
        //	if(Mathf.Abs(transform.position.y - previousPosFix.y) > deltaYFix)
        //	{
        //		Vector3 newPos = transform.position;
        //		newPos.y += amountYFix;
        //		transform.position = newPos;

        //		Debug.Log("Se sube la pelota");
        //	}*/

        //	previousPosFix = transform.position;

        //	/*if(checkBallToFixY)
        //	{

        //	}*/
        //}

        //V2
        /* if (checkBallToFixY)
         {
             currentTimeFix += Time.deltaTime;


             if (currentTimeFix > maxTimeFix)
                 currentTimeFix = maxTimeFix;

             Debug.LogWarning("ANTES transform.position=" + transform.position.ToString("F5"));

             Debug.Log("currentTimeFix*deltafix=" + (currentTimeFix * deltafix).ToString());
             Vector3 newPos = transform.position;
             newPos.y = initialPositionBallFix.y + currentTimeFix*deltafix;
             transform.position = newPos;

             Debug.Log("DESPUES transform.position=" + transform.position.ToString("F5"));

             if (currentTimeFix >= maxTimeFix)
             {
                 currentTimeFix = 0;
                 checkBallToFixY = false;

             }
         }*/
        //V3:

        if (checkBallToFixY)
        {
            if (transform.position.z > objectPerfectReference.transform.position.z)
            {
                float currentSeparationX = (objectPerfectReference.transform.position.z - transform.position.z) / 2f;

                float percentSeparation = /*2f**/(1 - Mathf.Abs(currentSeparationX / halfDistanceX_PerfectHit_Ball)); //complement because it get closer.

                //Debug.Log("halfDistanceX_PerfectHit_Ball=" + halfDistanceX_PerfectHit_Ball);
                // Debug.Log("currentSeparationX=" + currentSeparationX);
                // Debug.LogWarning("percentSeparation=" + percentSeparation);

                if (percentSeparation > 1)
                {
                    percentSeparation = 1;
                    checkBallToFixY = false;
                }



                Vector3 newPos = transform.position;
                newPos.y = initialPositionBallFix.y + percentSeparation * deltafix;
                transform.position = newPos;
            }
        }
	}
    void FixedUpdate()
    {
        //debugMessage.text = (-1f).ToString();


        if (GamePlayMetsHomuRunDerby.GetCurrentStatus() != GamePlayMetsHomuRunDerby.CURRENT_STATUS.PLAYING)
            return;
        bool debugBall = false;
#if UNITY_EDITOR
        Color newColor = Color.red;
        Color newColorYOffset = Color.red;
#endif
        if (bool.Parse(XMLConfiguration.GetSettings()["isDebug"])) {
            ActualizarValores();
        }

		if(isBallBeingPlayed)
		{
			if( bool.Parse(XMLConfiguration.GetSettings()["isDebug"]))
			{
				GameObject go = (GameObject)Instantiate(prefabBallShadowDebug, transform.position, Quaternion.identity);
				listPrefabBallShadows.Add(go);
				go.transform.GetChild(0).GetComponent<TextMesh>().text = Time.time.ToString("F5");

				GameObject go2 = (GameObject)Instantiate(prefabBatShadowDebug, GamePlayMetsHomuRunDerby.GetBatCollider().gameObject.transform.position, Quaternion.identity);
				listPrefabBatShadows.Add(go2);
				go2.transform.GetChild(0).GetComponent<TextMesh>().text = Time.time.ToString("F5");
			}
		}
		//Debug.Log("Settings.isAutomaticGamePlay="+Settings.isAutomaticGamePlay+"; ballWasAlreadyHit="+ballWasAlreadyHit);
		//if(batTransform != null)
		//	Debug.Log("batTransform="+batTransform.name);
		//if(extremeBat != null)
		//	Debug.Log("batTransform="+extremeBat.name);
		if(!Settings.isAutomaticGamePlay /*&& isBallBeingPlayed*/ && batTransform!=null && extremeBat!=null && !ballWasAlreadyHit)
		{
			//Debug.Log("ADENTRO"+previousTime);
			if(previousTime == 0 /*&& previousBatPosition == Vector3.zero*/)
			{
				//Debug.Log("HERE");
				previousTime = Time.time;
				previousBatPosition = extremeBat.transform.position;
				return;
			}
				
			#if UNITY_EDITOR
			Debug.DrawLine(batTransform.position, extremeBat.transform.position, Color.yellow );
			Debug.DrawLine(batTransform.position, transform.position, Color.magenta );
			Debug.DrawLine(batTransform.position, objectPerfectReference.transform.position, Color.cyan );
			#endif


			//DUO CAMERA:---------------------------
			if( bool.Parse(XMLConfiguration.GetSettings()["usoDuoCameraForGamePlay"]))
			{
				bool achieveCamera = true;
				int statusDUO = DUOManager.GetStatusGlobal();
				if(statusDUO >= 0)
				{
					bool isGoingForward = DUOManager.IsGoingForwardGlobal();
					float speedInCamera = DUOManager.GetSpeedGlobal(ManageData.singleton.newUser.isLefty);
					
					//Debug.Log("isGoingForward="+isGoingForward+"; speedInCamera="+speedInCamera);
					
					if(speedInCamera < float.Parse(XMLConfiguration.GetSettings()["minSpeedCamera"]))
						achieveCamera = false;
					
					if(!isGoingForward)
						achieveCamera = false;
				}
				else
				{
					achieveCamera = false;
				}
				
				//Debug.Log("achieveCamera="+achieveCamera+", statusDUO="+statusDUO);
				if(!achieveCamera)
				{
					#if UNITY_EDITOR
					SetBatColor(newColor, newColorYOffset);
#endif
					return;
				}
				
				
			}
			//END DUO CAMERA

			//--------------
			//OFFSET Y BALL
			
			
			float distanceBallToBatHit = 0;

            if (!use2DDotProduct)
            { distanceBallToBatHit = transform.position.y - GamePlayMetsHomuRunDerby.GetBatCollider().gameObject.transform.position.y; } //POsition from bat to ball.
            else
            {    distanceBallToBatHit = objectPerfectReference.transform.position.y - extremeBat.transform.position.y; //POsition from bat to ball.

                if (ballObject.GetComponent<Transform>().position.z > -41.5f)//41,61
                {
                    //Debug.LogWarning("posicion de inicio ççççççççççççççççççççççççççççççççççççççççççççççç ");

               //     FlagAnimator = true;
                    FlagINtrajectory = true;
                }
                else if (ballObject.GetComponent<Transform>().position.z < -43.95f && ballObject.GetComponent<Transform>().position.z > -62)// -57.6f )
                {
                    YballH = true;
                }
                else if (ballObject.GetComponent<Transform>().position.z < -62)
                {//-57.78f) {
                   // FlagAnimator = false;
                    FlagINtrajectory = false;
                    YballH = false;
                    // Debug.LogWarning("posicion de HItçççççççççççççççççççççççççççççççççççççççççççççççç");
                }

            }
			if(debugBall){
				Debug.Log("transform.position="+transform.position.ToString("F5")+"; GamePlayMetsHomuRunDerby.GetBatCollider().gameObject.transform.position="+GamePlayMetsHomuRunDerby.GetBatCollider().gameObject.transform.position.ToString("F5"));
				Debug.Log("distanceBallToBatHit="+distanceBallToBatHit +"; rangeYDistanceForGoodHomeRun="+rangeYDistanceForGoodHomeRun);
			}

			#if UNITY_EDITOR
			if( Mathf.Abs(distanceBallToBatHit) < rangeYDistanceForGoodHomeRun){
				newColorYOffset = Color.green;
				//Debug.Log("Es verde");
			}
			else{
				if(distanceBallToBatHit > 0){
					newColorYOffset = Color.yellow;
					//Debug.Log("Es amarila");
				}
				else{
					newColorYOffset = Color.magenta;
					//Debug.Log("Es magenta");
				}
			}
#endif
			//----


			//Debug.Log("PASO");
			if(debugBall){
				Helper.DebugWithCategory("objectPerfectReference.transform.position="+objectPerfectReference.transform.position, Helper.DEBUG_CATEGORY.TITLE);
				Debug.Log("batTransform.transform.position="+batTransform.transform.position);
				Debug.Log("extremeBat.transform.position="+extremeBat.transform.position);
			}
			/*Vector3 vec1 = objectPerfectReference.transform.position - batTransform.position;
			Debug.Log("vec1="+vec1);
			vec1 = vec1.normalized;
			Debug.Log("vec1 normalized="+vec1);

			Vector3 vec2 = extremeBat.transform.position - batTransform.position;
			Debug.Log("vec2="+vec2);
			vec2 = vec2.normalized;
			Debug.Log("vec2 normalized="+vec2);

			float dotVec = Vector3.Dot(vec1, vec2);
			Debug.LogError("dotVec="+dotVec);*/

			float swingDot = -1f;

			Vector3 firstVec = (objectPerfectReference.transform.position - batTransform.position ).normalized;
			Vector3 secondVec = (extremeBat.transform.position - batTransform.position).normalized;
			////Debug.Log("firstVec="+firstVec.ToString("F5"));
			////Debug.Log("secondVec="+secondVec.ToString("F5"));
			if(!use2DDotProduct)
				swingDot= Vector3.Dot( firstVec, secondVec); //DOt product normal with Vector2
			else{
				firstVec.y = 0;
				secondVec.y = 0;
				firstVec = firstVec.normalized;
				secondVec = secondVec.normalized;
				swingDot = Vector3.Dot( firstVec, secondVec);
				//swingDot = Vector2.Dot( new Vector2(firstVec.x, firstVec.z), new Vector2(secondVec.x, secondVec.z) ); //Only use Z and X axis.
			}


			if(debugBall)
				Helper.DebugWithColor("swingDot="+swingDot, Helper.LOG_COLOR.GREEN);
			listDotProductToDetectSwing.Add(swingDot);

			if(listDotProductToDetectSwing.Count > numberPositionsToTakeSwing){
				listDotProductToDetectSwing.RemoveAt(0);
			}
			//Velocities------

			if(debugBall)
				Helper.DebugWithColor("previousTime="+previousTime+"; Time.time="+Time.time+"; previousBatPosition="+previousBatPosition+"; extremeBat.transform.position="+extremeBat.transform.position, Helper.LOG_COLOR.CYAN);

			listVelocities.Add( Mathf.Abs(Vector3.Distance(previousBatPosition, extremeBat.transform.position)) / Mathf.Abs(previousTime - Time.time) );
			if(listVelocities.Count > numberPositionsToTakeSwing){
				listVelocities.RemoveAt(0);
			}
			previousBatPosition = extremeBat.transform.position;
			previousTime = Time.time;
			//-----------------------------------

			listPositionBat.Add(extremeBat.transform.position);

			if(listPositionBat.Count > numberPositionsToTakeSwing){
				listPositionBat.RemoveAt(0);
			}
			//Check the dot product (bat againt perfect reference). It means, if the bat is near the perfect reference, it will be a good hit.

			if(debugBall)
				Helper.DebugWithCategory("listDotProductToDetectSwing[listDotProductToDetectSwing.Count-1]="+listDotProductToDetectSwing[listDotProductToDetectSwing.Count-1]+"; rangeForCollisionWithPerfectReference="+rangeForCollisionWithPerfectReference, Helper.DEBUG_CATEGORY.TITLE);
			#if UNITY_EDITOR
			newColor = Color.yellow;
#endif

			if(listDotProductToDetectSwing[listDotProductToDetectSwing.Count-1]  < rangeForCollisionWithPerfectReference)
			{
				if(bool.Parse(XMLConfiguration.GetSettings()["usePerfectHitForGamePlay"]))
				{
					if(debugBall)
						Debug.Log("return by not perfect collision");
					#if UNITY_EDITOR
					SetBatColor(newColor, newColorYOffset);
#endif
					return;
				}
			}

			if(debugBall)
				Debug.Log("Paso prueba de bat cerca de perfect collision");
			#if UNITY_EDITOR
			newColor = Color.green;
#endif
			for(int i=1; i<listVelocities.Count-1; i++)
			{
				if(listVelocities[i] == 0)
				{
					if((listVelocities[i-1] != 0) && (listVelocities[i+1] != 0))
					{
						listVelocities[i] = (listVelocities[i-1] + listVelocities[i+1])/2f;
					}
				}
			}
			float velocityToUSe = listVelocities[listVelocities.Count-1];

			if(bool.Parse(XMLConfiguration.GetSettings()["useAverageVelocity"]))
			{
				velocityToUSe = 0;
				for(int i=15; i<listVelocities.Count; i++)
				{
					velocityToUSe+=listVelocities[i];
				}
				velocityToUSe = velocityToUSe / 5f;
			}

			//debugMessage.text = velocityToUSe.ToString("F1");

			//lastVelocityBatDebug.text = velocityToUSe.ToString();
			//Debug.Log("velocityToUSe="+velocityToUSe);

			float multiplierVelocity = 1f;

			if( bool.Parse(XMLConfiguration.GetSettings()["useVelocityForGamePlay"])){

				if(velocityToUSe < minVelocityBat)
				{
					//Debug.LogError("velocityToUSe="+velocityToUSe+"; minVelocityBat="+minVelocityBat);
					multiplierVelocity = velocityToUSe / minVelocityBat;
					if(multiplierVelocity == 0)
						multiplierVelocity = 0.05f;
					if(debugBall)
						Debug.Log("return by not velocity");
					#if UNITY_EDITOR
					SetBatColor(newColor, newColorYOffset);
					#endif
					//return;
				}
			}

			if(multiplierVelocity > 1f)
				multiplierVelocity = 1f;


			//Debug.LogError("velocityToUSe="+velocityToUSe);

			if(debugBall)
				Debug.LogWarning("Paso prueba de velocidad");


			resultDotProduct = Vector3.Dot( (extremeBat.transform.position - batTransform.position).normalized, (transform.position - batTransform.position).normalized);
			//if(debugDotProduct){
			//Debug.LogWarning("resultDotProduct="+resultDotProduct);
			//}
			
			//if(debugBall)
			//	Helper.DebugWithCategory("Collision with bat resultDotProduct="+resultDotProduct+"; rangeForDetectCollisionWithBatAndBall="+rangeForDetectCollisionWithBatAndBall, Helper.DEBUG_CATEGORY.TITLE);

			//if(resultDotProduct > rangeForDetectCollisionWithBatAndBall)
			if(resultDotProduct > rangeForDetectCollisionWithBatAndBall)
			{
				if(debugBall)
					Debug.LogWarning("Paso prueba de pelota cerca de bat");

				#if UNITY_EDITOR
				newColor = Color.magenta;
#endif
				//if(debugBall){
				/*Helper.DebugWithCategory("HEIGHT BALL="+transform.position.ToString("F4") +
				                         "; HEIGHT BAT closest point="+GamePlayMetsHomuRunDerby.GetBatCollider().ClosestPointOnBounds(transform.position).ToString("F4")
				                         + "; HEIGHT ONLY BAT="+GamePlayMetsHomuRunDerby.GetBatCollider().gameObject.transform.position.ToString("F4")
				                         , Helper.DEBUG_CATEGORY.TITLE 
				                         );*/
				//float distanceBallToBatHit = transform.position.y - GamePlayMetsHomuRunDerby.GetBatCollider().ClosestPointOnBounds(transform.position).y;

				//Debug.Log("distanceBallToBatHit="+distanceBallToBatHit);
				//}
				//Check the list of extreme position bat in order to know if it is a correct swing:
				bool correctSwing = false;
				float previousZPosition = -10000f;
				int numberCorrectZ = 0;
				//for(int i=listPositionBat.Count-1; i>10; i--)
				for(int i=10; i<listPositionBat.Count; i++)
				{
					//Debug.Log("i="+i+", listPositionBat[i].z ="+listPositionBat[i].z + ", previousZPosition="+previousZPosition);
					if(listPositionBat[i].z >= previousZPosition)
					{
						numberCorrectZ++;
						previousZPosition = listPositionBat[i].z;
					}
					else
					{
						previousZPosition = listPositionBat[i].z;
						//numberCorrectZ--;
					}
				}
				//Debug.Log("numberCorrectZ="+numberCorrectZ);
				if(numberCorrectZ > 5)
				{
					correctSwing = true;
				}

				if(!bool.Parse(XMLConfiguration.GetSettings()["usePositionForGamePlay"]))
				{
					correctSwing = false;
				}

				//Debug.LogError("ME quede aqui. Arriba me dijo que distanceBallToBatHit daba verde, pero aca abajo la manda hacia arriba, creo es por el correct swing que esta dando algun valor raro");

				//EditorApplication.isPaused = true;

				//if(debugBall)

				//--------DISTANCE Z:

				float DistanceZ_Ball_PerfectReference = transform.position.z - objectPerfectReference.transform.position.z;
				//Debug.Log("DistanceZ_Ball_PerfectReference="+DistanceZ_Ball_PerfectReference);

				//------

				if(Mathf.Abs(DistanceZ_Ball_PerfectReference) > float.Parse(XMLConfiguration.GetSettings()["distanceBallFromPerfectReference"]))
				{
					#if UNITY_EDITOR
					SetBatColor(newColor, newColorYOffset);
#endif
					return;
				}



				PauseGame();

				//Debug.Log("correctSwing="+correctSwing);
				if(correctSwing)
				{
					//if(distanceBallToBatHit > rangeYDistanceForGoodHomeRun.x && distanceBallToBatHit < rangeYDistanceForGoodHomeRun.y){
					if( Mathf.Abs(distanceBallToBatHit) < rangeYDistanceForGoodHomeRun){
						Debug.LogWarning("Perfect collision");
						//MakeCollisionWithBat( (extremeBat.transform.position - batTransform.position)/2f );
						MakeCollisionWithBat(  batTransform.position, multiplierVelocity );
					}
					else{
						                                                    //MakeCollisionWithBat( (extremeBat.transform.position - batTransform.position)/2f, true, distanceBallToBatHit );
						MakeCollisionWithBat( batTransform.position, multiplierVelocity, true, distanceBallToBatHit );
					}
				}
				else
				{
					                                                        //MakeCollisionWithBat( (extremeBat.transform.position - batTransform.position)/2f, true, distanceBallToBatHit );
					MakeCollisionWithBat( batTransform.position,multiplierVelocity, true, distanceBallToBatHit );
				}
			}

		
		}

		//Debug.Log("Color final="+newColor + "; newColorYOffset="+newColorYOffset);
		#if UNITY_EDITOR
		newColor.a = 0.5f;
		SetBatColor(newColor, newColorYOffset);
#endif

	}
	#if UNITY_EDITOR
	void SetBatColor(Color _newColor, Color _newColorYOffset)
	{
		if(GamePlayMetsHomuRunDerby.myslf.myBatRenderModel != null)
		{
			//GamePlayMetsHomuRunDerby.myslf.myBatRenderModel.GetComponent<Renderer>().material.color = _newColor;
			GamePlayMetsHomuRunDerby.myslf.myCubeColorYOffset.color = _newColorYOffset;
		}
	}
#endif

	public void SendBall()
	{
		//Helper.DebugWithColor("Quitar esta linea para el final", Helper.LOG_COLOR.ORANGE);
		//isBallBeingPlayed = true;

		//if(GamePlayMetsHomuRunDerby.GetCurrentBall() == 5)
		//	ballObject.GetComponent<Renderer>().material.mainTexture = textureBallRed;
		//else
			ballObject.GetComponent<Renderer>().material.mainTexture = textureBallWhite;


		ballWasAlreadyHit = false;
		mySphereCollider.enabled = true;

		myTrailRender.enabled = false;
		transform.parent = parentBall.transform;
		transform.localPosition = localPositionsStartBall;
		transform.localRotation = Quaternion.Euler(Vector3.zero);

		myRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;


		ballObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
		myRigidbody.isKinematic = true;
		myRigidbody.velocity = Vector3.zero;
		myRigidbody.angularVelocity = Vector3.zero;
		//myRigidbody.angularDrag = 0;
		ballObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
		ballObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

		
		iTween.RotateBy(ballObject.gameObject, iTween.Hash("amount", new Vector3(0,0, -Random.Range(180f, 359f)), "time", 20f, "easetype", iTween.EaseType.linear ));

		Invoke("SeparateBall",timeSeparateFromHand);
		Invoke ("SoundSwing", timeSeparateFromHand+0.3f);
		//Invoke ("StartTrail", timeSeparateFromHand *0.25f);

	}
	void SoundSwing()
	{
		AudioController.PlaySound(AudioController.SOUNDS.SWING);
	}

	void StartTrail()
	{
		myTrailRender.enabled = true;
	}
	void SeparateBall()
	{
		transform.parent = null;
		Vector3 newPos = transform.position;
		newPos.y = objectPerfectReference.transform.position.y;
		newPos.x = 0;
		//separationToFix = objectPerfectReference.transform.position.y

		//PAra evitar el salto de la animacion:
		//---NNO volver a usar//transform.position = newPos;
		float distanceAdd = objectPerfectReference.transform.position.y - transform.position.y - ballAltitudeVAR; 
        //iTween.MoveAdd(gameObject, iTween.Hash("y", distanceAdd, "time", 1f, "easetype", iTween.EaseType.linear));
        ////iTween.MoveTo(gameObject, iTween.Hash("y", newPos.y, "time", 1f, "easetype", iTween.EaseType.linear, "islocal", true));

        deltafix =distanceAdd;

        myRigidbody.isKinematic = false;
        myRigidbody.useGravity = false;

		checkBallToFixY = true;
       // currentTimeFix = 0;
        initialPositionBallFix = transform.position;
        halfDistanceX_PerfectHit_Ball = (objectPerfectReference.transform.position.z - transform.position.z)/2f;

        myRigidbody.constraints = RigidbodyConstraints.FreezeRotation /*| RigidbodyConstraints.FreezePositionY*/;
		//Invoke ("FreezeY", 0.3f);

		if(useRandomX_BallSend)
			forceToSendBall.x = Random.Range(0.15f, 0.35f);

        
        forceToSendBall.z = forceXSendBall;

        /*if(invertForceToSendBall)
			myRigidbody.AddForce(forceToSendBall, typeForce);
		else*/
            myRigidbody.AddForce(forceToSendBall,typeForce);


	//	if(GamePlayMetsHomuRunDerby.IsBallForTakePhoto())
		//	WebCamera.StartTakePhotos();
		
		isBallBeingPlayed = true;
	}

	void FreezeY()
	{
		myRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

		/*if(transform.position.y != objectPerfectReference.transform.position.y)
		{
			Vector3 newPos = transform.position;
			newPos.y = objectPerfectReference.transform.position.y;
			transform.position = newPos;
		}*/
	}

	public void ConfigPositions(PitcherController _pitcher)
	{
		parentBall = _pitcher.parentBall;
		localPositionsStartBall = _pitcher.localPositionsStartBall;
	}

	public void MakeHomeRun(Vector3 _initialPositionHitBat, float _multiplierVelocity, bool determineYForce, float _yOffset)
	{
		//myRigidbody.constraints = RigidbodyConstraints.None;
		myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        myRigidbody.useGravity = true;
        checkBallToFixY = false;
        StartTrail();

		if(useListPrefix)
		{
			do{
				int side = Random.Range(0,2);

				if(side == 0)
				{
					//positive x;
					forceToMakeHomeRun = listPositionsFixedForHomeRun[ Random.Range(0, listPositionsFixedForHomeRun.Count) ];
				}
				else
				{
					//negative x, they are not in the list, they are ranges:
					/*
					RANGOS:
					x -> 0, -15
					y -> 19 , 25
					z = 58

					----
					x-> -28, -15
					y -> 18, 25
					z = 58
					 */
					forceToMakeHomeRun.z = 58;
					forceToMakeHomeRun.x = Random.Range(-28f, 0f);
					if(forceToMakeHomeRun.x < -20f)
						forceToMakeHomeRun.y = Random.Range(18f, 25f);
					else if(forceToMakeHomeRun.x < -15f)
						forceToMakeHomeRun.y = Random.Range(19f, 25f);
					else if(forceToMakeHomeRun.x < -11f)
						forceToMakeHomeRun.y = Random.Range(20f, 25f);
					else if(forceToMakeHomeRun.x < -8f)
						forceToMakeHomeRun.y = Random.Range(21f, 25f);
					else
						forceToMakeHomeRun.y = Random.Range(22, 25f);
				}
			}while(previousHomeRunForce == forceToMakeHomeRun);
			previousHomeRunForce = forceToMakeHomeRun;
		}
		else
		{
			if(!useForcesWithZero)
			{
				if(useRandomX)
					forceToMakeHomeRun.x = Random.Range(rangeRandomX.x,rangeRandomX.y);
				if(useRandomY)
					forceToMakeHomeRun.y = Random.Range(rangeRandomY.x,rangeRandomY.y);
				if(useRandomZ)
					forceToMakeHomeRun.z = Random.Range(rangeRandomZ.x,rangeRandomZ.y);

				if(useMinForceX){
					forceToMakeHomeRun.x = rangeRandomX.x;
				}
				else if(useMaxForceX){
					forceToMakeHomeRun.x = rangeRandomX.y;
				}
				if(useMinForceY){
					forceToMakeHomeRun.y = rangeRandomY.x;
				}
				else if(useMaxForceY){
					forceToMakeHomeRun.y = rangeRandomY.y;
				}
				if(useMinForceZ){
					forceToMakeHomeRun.z = rangeRandomX.x;
				}
				else if(useMaxForceZ){
					forceToMakeHomeRun.z = rangeRandomX.y;
				}
			}
			else
			{
				forceToMakeHomeRun.x = Random.Range(-28f, 16f);

				if(forceToMakeHomeRun.x > -4f)
				{
					forceToMakeHomeRun.y= Random.Range(20f, 25f);
					forceToMakeHomeRun.z = 35f;
				}
				else if(forceToMakeHomeRun.x > -12f)
				{
					forceToMakeHomeRun.y = Random.Range(20f, 25f);
					forceToMakeHomeRun.z = 36f;
				}
				else
				{
					forceToMakeHomeRun.y = Random.Range(18f, 25f);
					forceToMakeHomeRun.z = 36f;
				}
			}
		}

		//Debug.LogError("Checar las fuerzas para mandar la pelota hacia arriba o hacia abajo, que se sigan viendo como las anteriores pelotas");
		if(determineYForce)
		{
			if(_yOffset > 0){
				Debug.Log("Original force to up="+forceToMakeHomeRun);
				forceToMakeHomeRun.x = Random.Range(-5f, 5f);
				forceToMakeHomeRun.y += Random.Range(1f, 3f);
				forceToMakeHomeRun.z = Random.Range(10f, 15f);
				Debug.Log("Pelota se va hacia arriba");
			}
			else
			{
				forceToMakeHomeRun.y = Random.Range(-5f, 0);
				Debug.Log("Pelota se va hacia abajo");

				
				forceToMakeHomeRun = myRigidbody.velocity = forceToMakeBallDown;
				forceToMakeHomeRun.x = Random.Range(-4f, 4f);

			}
			AudioController.PlaySound(AudioController.SOUNDS.HIT_FAIL);
		}
		else
			AudioController.PlaySound(AudioController.SOUNDS.HIT_GOOD);

		if(useDebugTemp)
		{
			forceToMakeHomeRun = foceFinal;
		}
		

		Invoke ("PlayHorn", 0.5f);

		Invoke ("PauseGame", 0.5f);

		//velocity:
		Helper.DebugWithColor("_multiplierVelocity="+_multiplierVelocity, Helper.LOG_COLOR.CYAN);
		forceToMakeHomeRun *= _multiplierVelocity;

		myRigidbody.velocity = Vector3.zero;


		Helper.DebugWithCategory("forceToMakeHomeRun="+forceToMakeHomeRun, Helper.DEBUG_CATEGORY.GOOD_MESSAGE);
		//Debug.LogWarning("forceToMakeHomeRun.magnitude="+forceToMakeHomeRun.magnitude);
		myRigidbody.AddForce(forceToMakeHomeRun,typeForceHomeRun);

		//float randomForceTorque = Random.Range(1000f, 4000f);
		//ballObject.GetComponent<Rigidbody>().AddTorque(new Vector3(randomForceTorque,randomForceTorque,randomForceTorque));
		iTween.RotateBy(ballObject.gameObject, iTween.Hash("amount", new Vector3(Random.Range(30f, 50f), Random.Range(30f, 50f), Random.Range(30f, 50f)), "time", 20f, "easetype", iTween.EaseType.linear ));
		//Invoke("AddTor", 0.3f);
		isCountingFeet = true;
		GamePlayMetsHomuRunDerby.ToggleCheckCameraYPosition(true);
		GamePlayMetsHomuRunDerby.ToggleFeetsCounterAnimation(isCountingFeet);
		//numberMetersCounted = 0;
		//Debug.LogError("_initialPositionHitBat="+_initialPositionHitBat.ToString("F5"));

		//initialPositionHitBat = new Vector2(_initialPositionHitBat.x, _initialPositionHitBat.z);
		//Debug.LogError("initialPositionHitBat="+initialPositionHitBat.ToString("F5"));
		GamePlayMetsHomuRunDerby.ChangeUIText(UIController.TYPE_UI_ELEMENT.FEETS, 0);
	}
	void PauseGame()
	{
#if UNITY_EDITOR
		if(bool.Parse(XMLConfiguration.GetSettings()["isDebug"]) && bool.Parse(XMLConfiguration.GetSettings()["pauseApplication"])){
			EditorApplication.isPaused = true;
		}
#endif
	}
	/*
	void AddTor()
	{
		float randomForceTorque = Random.Range(100f, 400f);
		ballObject.GetComponent<Rigidbody>().AddTorque(new Vector3(randomForceTorque,randomForceTorque,randomForceTorque));
	}*/
	void OnTriggerEnter(Collider other) {

		#if DEBUG_FLOW
		Debug.Log("My object="+gameObject.name+", OnTriggerEnter with="+other.gameObject.name);
#endif
		if(other.gameObject.name == "TriggerCanHit")
		{
			canMakeHomeRun = true;
		}

        //NEW

        Debug.Log("canCollideAgain=" + canCollideAgain);
        if (!canCollideAgain)
            return;
        Debug.Log("My object=" + gameObject.name + ", Make trigger with=" + other.gameObject.tag + "; " + other.gameObject.name);

        Debug.Log("currentTimeCollision=" + currentTimeCollision + ",Time.time=" + Time.time);

#if DEBUG_FLOW

		Debug.Log("My object="+gameObject.name+", Make collision with="+collision.gameObject.tag+"; "+collision.gameObject.name);

		Debug.Log("currentTimeCollision="+currentTimeCollision+",Time.time="+Time.time);
#endif


        if (other.CompareTag("StadiumCollider") && currentTimeCollision != Time.time)
        {
            currentTimeCollision = Time.time;

            checkBallToFixY = false;

            iTween itune = ballObject.GetComponent<iTween>();
            if (itune != null)
                Destroy(itune);

            switch (other.gameObject.name)
            {
                case "missHit":
                    isCountingFeet = false;
                    hitsInARow = 0;
#if UNITY_EDITOR
                    if (bool.Parse(XMLConfiguration.GetSettings()["isDebug"]) && bool.Parse(XMLConfiguration.GetSettings()["pauseApplication"]))
                    {
                        EditorApplication.isPaused = true;
                    }
#endif
                    numberMetersCounted = 0;
                    GamePlayMetsHomuRunDerby.ChangeUIText(UIController.TYPE_UI_ELEMENT.FEETS, numberMetersCounted);
                    GamePlayMetsHomuRunDerby.MakeOut(false);
                    break;
                case "missHitFloor":
                case "green_field":
                case "pitcherStand_geo":
                    isCountingFeet = false;
                    hitsInARow = 0;
                    numberMetersCounted = 0;
                    GamePlayMetsHomuRunDerby.ChangeUIText(UIController.TYPE_UI_ELEMENT.FEETS, numberMetersCounted);
                    GamePlayMetsHomuRunDerby.MakeOut(true);

                    ballObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    ballObject.GetComponent<Rigidbody>().angularVelocity = Vector3.one * 100f;
                    isBallBeingPlayed = false;
                    canCollideAgain = false;
                    break;
                default:

                    isCountingFeet = false;

                    cameraToParent.SetParent(null);

                    if (GamePlayMetsHomuRunDerby.GetCurrentStatusGamePlay() == GamePlayMetsHomuRunDerby.STATUS_GAMEPLAY.PRACTICE_GAME)
                        numberMetersCounted = 0;

                    Debug.Log("I set distance " + numberMetersCounted);
                    ManageData.singleton.newUser.SetDistance(numberMetersCounted);

                    GamePlayMetsHomuRunDerby.myslf.myUI.AddToFinalScore(GamePlayMetsHomuRunDerby.myslf.currentStatusGameplay, hitsInARow);

                    GamePlayMetsHomuRunDerby.ToggleCheckCameraYPosition(false);
                    GamePlayMetsHomuRunDerby.ToggleFeetsCounterAnimation(isCountingFeet);
                    GamePlayMetsHomuRunDerby.FinishBallPlaying();
                    homerunParticleSystem.Play();

                    if (GamePlayMetsHomuRunDerby.GetCurrentStatusGamePlay() != GamePlayMetsHomuRunDerby.STATUS_GAMEPLAY.PRACTICE_GAME)
                        hitsInARow++;

                    //myRigidbody.isKinematic = true;
                    //myRigidbody.velocity = Vector3.zero;
                    //myRigidbody.angularVelocity = Vector3.zero;

                    //ballObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    //ballObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    mySphereCollider.enabled = false;
                    isBallBeingPlayed = false;
                    canCollideAgain = true;
                    break;
            }

            ManageData.singleton.newUser.SetDistance(numberMetersCounted);


            bool debugBall = true;
            if (debugBall)
                Invoke("DestroyDebugList", 3f);
        }
    }
	void OnTriggerExit(Collider other) {
		#if DEBUG_FLOW
		Debug.Log("My object="+gameObject.name+", OnTriggerExit with="+other.gameObject.name);
#endif
		if(other.gameObject.name == "TriggerCanHit")
		{
			canMakeHomeRun = false;
		}
	}
	void OnTriggerStay(Collider other) {
		#if DEBUG_FLOW
		Debug.Log("My object="+gameObject.name+", OnTriggerStay with="+other.gameObject.name);
#endif
		if(other.gameObject.name == "TriggerCanHit")
		{
			canMakeHomeRun = true;
		}
    }
	
	void PlayHorn()
	{
		AudioController.PlaySound(AudioController.SOUNDS.HORN);
	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log("canCollideAgain="+canCollideAgain);
		if(!canCollideAgain)
			return;
		Debug.Log("My object="+gameObject.name+", Make collision with="+collision.gameObject.tag+"; "+collision.gameObject.name);
		
		Debug.Log("currentTimeCollision="+currentTimeCollision+",Time.time="+Time.time);

#if DEBUG_FLOW

		Debug.Log("My object="+gameObject.name+", Make collision with="+collision.gameObject.tag+"; "+collision.gameObject.name);

		Debug.Log("currentTimeCollision="+currentTimeCollision+",Time.time="+Time.time);
#endif


		if(currentTimeCollision != Time.time )
		{
			currentTimeCollision = Time.time;

			switch(collision.gameObject.tag)
			{
			case "Bat":
				if(Settings.isAutomaticGamePlay)
				{
					MakeCollisionWithBat(collision.contacts[0].point, 1f);
				}
				break;
			case "StadiumCollider":
                checkBallToFixY = false;

				iTween itune = ballObject.GetComponent<iTween>();
				if(itune != null)
					Destroy(itune);

				switch(collision.gameObject.name)
				{
				case "missHit":
					isCountingFeet = false;
					hitsInARow = 0;
					#if UNITY_EDITOR
					if(bool.Parse(XMLConfiguration.GetSettings()["isDebug"]) && bool.Parse(XMLConfiguration.GetSettings()["pauseApplication"])){
						EditorApplication.isPaused = true;
					}
					#endif
					numberMetersCounted = 0;
					GamePlayMetsHomuRunDerby.ChangeUIText(UIController.TYPE_UI_ELEMENT.FEETS, numberMetersCounted);
					GamePlayMetsHomuRunDerby.MakeOut(false);
					break;
				case "missHitFloor":
				case "green_field":
				case "pitcherStand_geo":
					isCountingFeet = false;
					hitsInARow = 0;
					numberMetersCounted = 0;
					GamePlayMetsHomuRunDerby.ChangeUIText(UIController.TYPE_UI_ELEMENT.FEETS, numberMetersCounted);
					GamePlayMetsHomuRunDerby.MakeOut(true);
					break;
				default:

					isCountingFeet = false;

					if(GamePlayMetsHomuRunDerby.GetCurrentStatusGamePlay() == GamePlayMetsHomuRunDerby.STATUS_GAMEPLAY.PRACTICE_GAME)
						numberMetersCounted = 0;

                            Debug.Log("I set distance " + numberMetersCounted);
                            ManageData.singleton.newUser.SetDistance(numberMetersCounted);

                            GamePlayMetsHomuRunDerby.myslf.myUI.AddToFinalScore( GamePlayMetsHomuRunDerby.myslf.currentStatusGameplay, hitsInARow );

					GamePlayMetsHomuRunDerby.ToggleCheckCameraYPosition(false);
					GamePlayMetsHomuRunDerby.ToggleFeetsCounterAnimation(isCountingFeet);
					GamePlayMetsHomuRunDerby.FinishBallPlaying();
					homerunParticleSystem.Play();

					if(GamePlayMetsHomuRunDerby.GetCurrentStatusGamePlay() != GamePlayMetsHomuRunDerby.STATUS_GAMEPLAY.PRACTICE_GAME)
						hitsInARow++;
					break;
				}

                    ManageData.singleton.newUser.SetDistance(numberMetersCounted);
				switch(collision.gameObject.name)
				{
				case "missHitFloor":
				case "green_field":
				case "pitcherStand_geo":
					//myRigidbody.isKinematic = true;
					//myRigidbody.isKinematic = false;
					//myRigidbody.velocity = new Vector3(Random.Range(-4f, 4f), 1f, 28f);
					//myRigidbody.velocity = forceToMakeBallDown;
					//myRigidbody.angularVelocity = Vector3.zero;
					//myRigidbody.angularDrag = 0;
					
					ballObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
					ballObject.GetComponent<Rigidbody>().angularVelocity = Vector3.one*100f;
					//mySphereCollider.enabled = false;
					isBallBeingPlayed = false;
					canCollideAgain = false;
					break;
				default:
					myRigidbody.isKinematic = true;
					myRigidbody.velocity = Vector3.zero;
					myRigidbody.angularVelocity = Vector3.zero;
					//myRigidbody.angularDrag = 0;
					
					ballObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
					ballObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
					mySphereCollider.enabled = false;
					isBallBeingPlayed = false;
					canCollideAgain = true;
					break;
				}



				bool debugBall = true;
				if(debugBall)
					Invoke("DestroyDebugList", 3f);

				break;
			}
		}
	}



	void DestroyDebugList()
	{
		for(int i=listPrefabBallShadows.Count-1; i>=0; i--)
		{
			Destroy(listPrefabBallShadows[i]);
		}
		listPrefabBallShadows.Clear();
		
		for(int i=listPrefabBatShadows.Count-1; i>=0; i--)
		{
			Destroy(listPrefabBatShadows[i]);
		}
		listPrefabBatShadows.Clear();
	}
	//public void SetBatData(Vector3 _forwardVec, Vector3 _upVec, Vector3 _rightVec)
	public void SetBatData(GameObject _extremeBat, Transform _batTransform)
	{
		extremeBat = _extremeBat;
		batTransform = _batTransform;
	}
	void MakeCollisionWithBat(Vector3 startPointCollision, float _multiplierVelocity, bool determineYForce = false, float _yOffset = 0)
	{
		ballWasAlreadyHit = true;
		isBallBeingPlayed = false;
		Debug.Log("startPointCollision="+startPointCollision);
		GamePlayMetsHomuRunDerby.MakeHomeRun(startPointCollision, _multiplierVelocity, determineYForce, _yOffset);
	}
	public int GetNumberMeters(){
		return numberMetersCounted;
	}
	public void SetNumberMeters(int _numberMet){
		numberMetersCounted = _numberMet;
	}
	IEnumerator DestroyDebugCollision(GameObject _go)
	{
		yield return new WaitForSeconds(3f);
		Destroy(_go);
	}
	public void ReturnBallToPitcher()
	{
		canCollideAgain = true;
        myRigidbody.velocity = Vector3.zero;
        myRigidbody.angularVelocity = Vector3.zero;
		myRigidbody.isKinematic = true;

		myTrailRender.enabled = false;
		transform.parent = parentBall.transform;
		transform.localPosition = localPositionsStartBall;
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		
		myRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
	}
}
