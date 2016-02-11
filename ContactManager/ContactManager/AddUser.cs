//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Net;
using Android.Util;
using Android.Database;
using System.Net;
using System.Json;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;


namespace ContactManager
{
	[Activity (Label = "AddUser")]			
	public class AddUser : Activity
	{
		EditText contact_name, contact_lastname, phone;
		Button add_contact;
		ImageView add_photo;
		string photo_path;
		string add_contact_url = "http://172.17.72.6:8000/add";
		public static readonly int PickImageId = 1000;
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.AddUser); 
			contact_name = FindViewById<EditText> (Resource.Id.editName);
			contact_lastname = FindViewById<EditText> (Resource.Id.editLastName);
			phone = FindViewById<EditText> (Resource.Id.editPhone);
			add_contact = FindViewById<Button> (Resource.Id.button1);
			add_photo = FindViewById<ImageView> (Resource.Id.addPhoto);
			//action on clicking 'Add Contact' button
			add_contact.Click += delegate {	
				//showing message if name field is not empty(name is required)
				if(contact_name.Text.Length <= 0)
				{
					Android.Widget.Toast.MakeText(this, "Name is required!", Android.Widget.ToastLength.Long).Show ();
				}
				//if name exists
				else
				{
					//phone number required too
					if(phone.Text.Length <= 0)
					{
						Android.Widget.Toast.MakeText(this, "Phone is required!", Android.Widget.ToastLength.Long).Show ();
					}
					//if all necessary fields are filled - add contact and back to Main Activity
					else
					{
						AddContact(add_contact_url);
						StartActivity(typeof(MainActivity));
					}
				}
			};
			//Loading image gallery on image view click
			add_photo.Click += delegate {
				Intent = new Intent();
				Intent.SetType("image/*");
				Intent.SetAction(Intent.ActionGetContent);
				StartActivityForResult(Intent.CreateChooser(Intent,"Select photo"), PickImageId);
			};
				
		}
		/**
		 * Adding contact to database
		 * string returnedId - stores id of contact that returnes first request to service
		 * HttpWebRequest addRequest - creates request for adding contact info to http://172.17.72.6:8000/add
		 * var obj - serialized JSON object with name, id(temporary unknown), lastname, phone (data from AddUser Activite except photo)
		 * var addResponse - response from service
		 * 
		 * HttpWebRequest addPhotoRequest - creates request for adding photo to contact http://172.17.72.6:8000/add/{returned_id}
		 * byte[] fileToSend - array that stores converted to bytes image
		 */
		private void AddContact(string url)
		{
			string returnedId; 
			HttpWebRequest addRequest = (HttpWebRequest)HttpWebRequest.Create ("http://172.17.72.6:8000/add");
			addRequest.Method = "POST";
			addRequest.ContentType = "application/json";
			var obj = JsonConvert.SerializeObject(new JSONAdapter(contact_name.Text, null, contact_lastname.Text, phone.Text));
			//writing JSON object in StreamWriter
			using (var streamWriter = new StreamWriter (addRequest.GetRequestStream ())) {
				streamWriter.Write (obj);
			}
				
			var addResponse = (HttpWebResponse)addRequest.GetResponse ();
			//Reading service's response and getting id of added contact
			using(var streamReader = new StreamReader(addResponse.GetResponseStream()))
			{
				returnedId = streamReader.ReadToEnd ();
			}

			//if photo exists
			if (add_photo.Tag != null) {
				HttpWebRequest addPhotoRequest = (HttpWebRequest)HttpWebRequest.Create ("http://172.17.72.6:8000/add/" + returnedId);
				addPhotoRequest.Method = "POST"; 
				addPhotoRequest.ContentType = "text/plain"; 

				//add_photo.Tag stores image path
				byte[] fileToSend = File.ReadAllBytes (add_photo.Tag.ToString ());
				addPhotoRequest.ContentLength = fileToSend.Length; 

				using (Stream requestStream = addPhotoRequest.GetRequestStream ()) { 
					//Send the file as body request 
					requestStream.Write (fileToSend, 0, fileToSend.Length); 
					requestStream.Close (); 
				} 

				//getting answer from service
				var addPhotoResponse = (HttpWebResponse)addPhotoRequest.GetResponse ();
				using (var streamReader = new StreamReader (addPhotoResponse.GetResponseStream ())) {
					var responseText = streamReader.ReadToEnd ();
				}
			}

		}

		/**
		 * Actions after choosing image from gallery
		 * Uri uri - uri of choosen file
		 * var path - URI converted to path
		 * var converted_image - image converted to base64 string
		 */
		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null)) {
				Uri uri = data.Data;
				add_photo.SetImageResource (0);
				add_photo.SetImageURI (uri);
				var path = GetPathToImage(uri);
				var converted_image = ConvertImageToString (path);
				add_photo.Tag = path; //adding image path as ImageView tag
			}
		}

		/**
		 * Method that converts image to base64 string
		 * 
		 * FileStream filestream - stream for converting image to byte array
		 * byte[] buffer - array that stores bytes from filestream
		 * var convertedImage - converted byte[] buffer to base64 string
		 */
		public string ConvertImageToString(string path)
		{
			FileStream filestream = new FileStream (path, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[filestream.Length];
			filestream.Read (buffer, 0, (int)filestream.Length);
			filestream.Close ();
			var convertedImage = System.Convert.ToBase64String(buffer);
			return convertedImage;
		}
			
		/**
		 * Method that gets image's path based on URI
		 * string path - image path
		 */
		private string GetPathToImage(Uri uri)
		{
			string path = null;
			string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
			using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
			{
				if (cursor != null)
				{
					int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
					cursor.MoveToFirst();
					path = cursor.GetString(columnIndex);
				}
			}
			//add_photo.Tag = path;
			return path;
		}

	}
		

}

