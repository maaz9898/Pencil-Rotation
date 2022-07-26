#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using NetMQ;
using NetMQ.Sockets;

namespace OpenCVForUnityExample
{
[RequireComponent(typeof(WebCamTextureToMatHelper))]
public class Main : MonoBehaviour
{
    /// <summary>
    /// The FPS monitor.
    /// </summary>
    FpsMonitor fpsMonitor;
    Texture2D texture;
    Mat rgbMat;
    WebCamTextureToMatHelper webCamTextureToMatHelper;
    public int CurrentState = 0;
    public int Counter = 0;
    public int TurnPoint = 3;
    public Text Status;
    public Text Rotation;
    public bool ShowBox = true;
    RequestSocket client;

    // Start is called before the first frame update
    void Start()
    {
        fpsMonitor = GetComponent<FpsMonitor> ();
        webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();
        webCamTextureToMatHelper.Initialize();
        Rotation.text = "315-45°";
        // Connecting to the server via ZMQ
        client = new RequestSocket();
        client.Connect("tcp://localhost:8002");
    }

    // Update is called once per frame
    void Update()
    {
        if (!webCamTextureToMatHelper.IsPlaying () || !webCamTextureToMatHelper.DidUpdateThisFrame ()) return;
        Mat rgbaMat = webCamTextureToMatHelper.GetMat(); // Get camera image
        Utils.fastMatToTexture2D(rgbaMat, texture); // display camera image on texture
        RequestInference(); // Request inference from server
    }

    // Class for deserialzing json result
    public class InferenceResult
    {
        public List<Double> location { get; set; }
        public String label { get; set; }
        public Double score { get; set; }

        public void display(){
            Debug.Log("InferenceResult: " + location[0].ToString("n1") + " " + location[1].ToString("n1") + " " + location[2].ToString("n1") + " " + location[3].ToString("n1"));
            Debug.Log("InferenceResult: " + label + ": " + score.ToString("n2"));
        }
    }

    public void RequestInference()
    {
        byte[] imageData = ImageConversion.EncodeToPNG(texture); // Convert texture to byte array
        client.SendFrame(imageData); // Send byte array to server
        var message = client.ReceiveFrameBytes(); // Receive utf-8 encoded json from server
        var bodyString = System.Text.Encoding.UTF8.GetString(message); // Convert utf-8 json to string
        InferenceResult result = Newtonsoft.Json.JsonConvert.DeserializeObject<InferenceResult>(bodyString); // Deserialize json to InferenceResult object
        if (result.score<0.3){return;} // If score is low, skip
        // result.display();
        Double h = result.location[2]-result.location[0]; // Calculate height of bounding box
        Double w = result.location[3]-result.location[1]; // Calculate width of bounding box
        // show bounding box on camera image if ShowBox is true
        if (ShowBox){
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();
            // texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);
            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
            // Draw bounding box on camera image
            Imgproc.rectangle(webCamTextureMat, new Point(result.location[1]*width, result.location[0]*height), new Point(result.location[3]*width, result.location[2]*height), new Scalar(0, 255, 0, 255), 2);
            Utils.fastMatToTexture2D(webCamTextureMat, texture);
        }

        // Conditions for changing state
        switch (CurrentState)
        {
            case 0: // Initial state (315-45°)
                if (h<w){
                    if(AddProgress()){
                        Debug.Log("45-135 Degrees reached");
                        Rotation.text = "45-135°";
                    }
                }
                break;
            case 1: // Second State (45-135°)
                if (h>w){
                    if(AddProgress()){
                        Debug.Log("135-225 Degrees reached");
                        Rotation.text = "135-225°";
                    }
                }
                break;
            case 2: // Third State (135-225°)
                if (h<w){
                    if(AddProgress()){
                        Debug.Log("225-315 Degrees reached");
                        Rotation.text = "225-315°";
                    }
                }
                break;
            case 3: // Fourth State (225-315°)
                if (h>w){
                    if(AddProgress()){
                        Debug.Log("315-45 Degrees reached");
                        Debug.Log("Rotation Complete!");
                        Rotation.text = "315-45°";
                        Status.text = "Success!";
                    }
                }
                break;
        }
    }

    // Function for handling progression to the next state
    public bool AddProgress(){
        Counter += 1;
        if (Counter >= TurnPoint){
            CurrentState = (CurrentState+1)%4; // Change state, and loop back to 0 if necessary
            Counter = 0; // Reset counter
            return true;
        }
        return false;
    }

    /// <summary>
    /// Raises the webcam texture to mat helper initialized event.
    /// </summary>
    public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");
            
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);
            Utils.fastMatToTexture2D(webCamTextureMat, texture);

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
            
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
           
            if (fpsMonitor != null) {
                fpsMonitor.Add ("width", webCamTextureMat.width().ToString());
                fpsMonitor.Add ("height", webCamTextureMat.height ().ToString ());
                fpsMonitor.Add ("orientation", Screen.orientation.ToString ());
            }

            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
            
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }

            rgbMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC3);
        }
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");

            if (rgbMat != null)
                rgbMat.Dispose ();

            if (texture != null) {
                Texture2D.Destroy (texture);
                texture = null;
            }

            client.Close();
        }

        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }
}
}

#endif