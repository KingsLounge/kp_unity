using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ImageAndVideoPicker;

public class IVPickerExample : MonoBehaviour {

	string log = "Log ";
	Texture2D texture;

	void OnEnable()
	{
		PickerEventListener.onImageSelect += OnImageSelect;
		PickerEventListener.onImageLoad += OnImageLoad;
		PickerEventListener.onVideoSelect += OnVideoSelect;
		PickerEventListener.onError += OnError;
		PickerEventListener.onCancel += OnCancel;

		#if UNITY_ANDROID
		AndroidPicker.CheckPermissions();
		#endif
	}
	
	void OnDisable()
	{
		PickerEventListener.onImageSelect -= OnImageSelect;
		PickerEventListener.onImageLoad -= OnImageLoad;
		PickerEventListener.onVideoSelect -= OnVideoSelect;
		PickerEventListener.onError -= OnError;
		PickerEventListener.onCancel -= OnCancel;
	}

	
	void OnImageSelect(string imgPath, ImageAndVideoPicker.ImageOrientation imgOrientation)
	{
		Debug.Log ("Image Location : "+imgPath);
		log += "\n OnImageSelect : Image Path : " + imgPath;
		log += "\nImage Orientation : " + imgOrientation;
	}

		
	void OnImageLoad(string imgPath, Texture2D tex, ImageAndVideoPicker.ImageOrientation imgOrientation)
	{
		Debug.Log ("Image Location : "+imgPath);
		log += "\n OnImageLoad : Image Path : " + imgPath;
		texture = tex;
	
	}

	void OnVideoSelect(string vidPath)
	{
		Debug.Log ("Video Location : "+vidPath);
		log += "\nVideo Path : " + vidPath;
		#if UNITY_ANDROID || UNITY_IOS
		Handheld.PlayFullScreenMovie ("file://" + vidPath, Color.blue, FullScreenMovieControlMode.Full, FullScreenMovieScalingMode.AspectFill);
		#endif
	}

	void OnError(string errorMsg)
	{
		Debug.Log ("Error : "+errorMsg);
		log += "\nError :" +errorMsg;
	}

	void OnCancel()
	{
		Debug.Log ("Cancel by user");
		log += "\nCancel by user";
	}


	void OnGUI()
	{
        float y = Screen.height * 0.1f;
        float x = Screen.width * 0.05f;
        float width = Screen.width * 0.9f;
        float height = Screen.height * 0.1f;
		if(GUI.Button(new Rect(x, y, width, height),"Browse Image"))
		 {
			#if UNITY_ANDROID
			AndroidPicker.BrowseImage(false);
			#elif UNITY_IPHONE
			IOSPicker.BrowseImage(false); // true for pick and crop
			#endif

		}

        y += height + 25;
		if(GUI.Button(new Rect(x, y, width, height),"Browse & Crop Image"))
		{
			#if UNITY_ANDROID
			AndroidPicker.BrowseImage(true);
			#elif UNITY_IPHONE
			IOSPicker.BrowseImage(true); // true for pick and crop
			#endif
		}

        y += height + 25;
        if (GUI.Button(new Rect(x, y, width, height),"Browse Video"))
		{
			#if UNITY_ANDROID
			AndroidPicker.BrowseVideo();
			#elif UNITY_IPHONE
			IOSPicker.BrowseVideo();
			#endif
		}

        if (texture != null)
        { 
            y += height + 20;
            float texContainerW = Mathf.Min(Screen.width * 0.2f, Screen.height * 0.2f);
            GUI.DrawTexture(new Rect(x, y, texContainerW, texContainerW), texture, ScaleMode.ScaleToFit, true);
            y += texContainerW + 10;
        }
        else
        {
            y += height + 10;
        }

        
        GUI.Label(new Rect(20, y, Screen.width - 40, Screen.height - y), log);
    }

}
