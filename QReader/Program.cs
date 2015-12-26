using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

//Estas referencias son necesarias para usar GLIDE
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;


namespace QReader
{
    enum State { Main, Camera, Webcam, Habilitado, Deshabilitado};
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.
        //Objetos de interface gráfica GLIDE
        private GHI.Glide.Display.Window resultWindow;
        private GHI.Glide.Display.Window mainWindow;
        private GHI.Glide.Display.Window cameraWindow;
        private GHI.Glide.UI.Button BtnLeer;//Sirve para regresar la pantalla a modo léctura QR
        private bool isStreaming;
        State systemState;
        Bitmap currentBitmap;
        GT.Timer timer = new GT.Timer(5000,GT.Timer.BehaviorType.RunOnce); // every second (1000ms)

        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            isStreaming = true;

            //Timer
            timer.Tick += timer_Tick;
            timer.Start();

            //Load windows
            mainWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.MainWindow));
            resultWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.ResultWindow));
            cameraWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.CameraWindow));
            BtnLeer = (GHI.Glide.UI.Button)resultWindow.GetChildByName("BtnLeer");
            BtnLeer.TapEvent += BtnLeer_TapEvent;
            GlideTouch.Initialize();
            //Selecciona mainWindow como la ventana de inicio
            Glide.MainWindow = mainWindow;

            //Funciones de Camara
            camera.BitmapStreamed += camera_BitmapStreamed;
            camera.CameraConnected += camera_CameraConnected;
            camera.PictureCaptured += camera_PictureCaptured;
            systemState = State.Camera;


            //button.ButtonPressed += button_ButtonPressed;

            //Conexion a Internet
            this.ethernetJ11D.NetworkInterface.Open();
            this.ethernetJ11D.NetworkInterface.EnableDhcp();
            this.ethernetJ11D.UseThisNetworkInterface();
            this.ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            this.ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;

            //imagen del qr
            currentBitmap = new Bitmap(camera.CurrentPictureResolution.Width, camera.CurrentPictureResolution.Height);

        }

        void BtnLeer_TapEvent(object sender)
        {
            Glide.MainWindow = cameraWindow;
            //camera.StartStreaming();
            systemState = State.Camera;
        }

        void timer_Tick(GT.Timer timer)
        {
            Glide.MainWindow = cameraWindow;
            GetQRContent("http://autismoespol.tk/codigoQR/pruebaqr.png");
            //camera.StartStreaming();
            systemState = State.Camera;
        }

        //void button_ButtonPressed(Button sender, Button.ButtonState state)
        //{
        //    camera.StopStreaming();
        //    GetQRContent("http://autismoespol.tk/codigoQR/pruebaqr.png");
        //    //camera.TakePicture();
        //}

        /// <summary>Sends bitmap to remote server using a POST request.</summary>
        private void sendBitmapToServer()
        {
            //try
            //{
            //    POSTContent content = POSTContent.CreateBinaryBasedContent(currentBitmap.GetBitmap());
            //    HttpRequest request = HttpHelper.CreateHttpPostRequest("http://" + SERVER_URL + ":" + SERVER_PORT + "/uploadImage", content, "multipart/form-data");

            //    request.SendRequest();
            //    request.ResponseReceived += request_ResponseReceived;

            //    Debug.Print("Imagen enviada");
            //}
            //catch (System.ObjectDisposedException oe)
            //{
            //    Debug.Print("Error in sendBitmapToCloud(): " + oe.Message);
            //}

        }

        /*
         * Se toma la foto
         * Se envia al servidor
         * Se hace la consulta al servidor
         * Se recibe los datos
         * Si los datos son correctos mostramos en la pantalla
         * Si los datos no son correctos repetimos el proceso
         */

        void camera_PictureCaptured(Camera sender, GT.Picture e)
        {
            if (systemState == State.Habilitado) { 
                //metodo para subir las imagenes
                GetQRContent("http://autismoespol.tk/codigoQR/pruebaqr.png");
            }
        }

        void camera_CameraConnected(Camera sender, EventArgs e)
        {
            Debug.Print("CameraConnected");
            camera.StartStreaming();
        }

        void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Conectado a Internet");
            systemState = State.Habilitado;
        }

        void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Desconectado a Internet");
            systemState = State.Deshabilitado;
        }

        void camera_BitmapStreamed(Camera sender, Bitmap e)
        {
            switch (systemState)
            {
                case State.Camera:
                    if (isStreaming)
                        displayT35.SimpleGraphics.DisplayImage(e, 0, 0);
                    break;
                case State.Webcam:
                    camera.StopStreaming();
                    break;

                default:
                    break;
            }
        }
        void GetQRContent(String ImageUrl) {
            var urlQR = "http://api.qrserver.com/v1/read-qr-code/?fileurl="+ImageUrl;            
            HttpRequest request = HttpHelper.CreateHttpGetRequest(urlQR.ToString());
            request.ResponseReceived += request_ResponseReceived;
            request.SendRequest();
            Debug.Print("Enviando request");
        }

        void request_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            //Aqui se
            var resultado = response.Text;
            String r = GetUrlFromJson(resultado.ToString());
            GHI.Glide.UI.TextBlock text = (GHI.Glide.UI.TextBlock)resultWindow.GetChildByName("TxtResult");
            text.Text = r;
            Glide.MainWindow = resultWindow;

            //HttpRequest pedido = HttpHelper.CreateHttpGetRequest(GetUrlFromJson(resultado.ToString()));
            //pedido.ResponseReceived += pedido_ResponseReceived;
            //pedido.SendRequest();
            //Debug.Print(resultado);
            //Debug.Print("Enviando Url de QR");

        }

        void pedido_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            //Aqui el resutlado final del QR
            var result = response.Text;
            Debug.Print(result);
        }

        void mostrarPantalla() {
        
        }
        String GetUrlFromJson(String value) {
            var texto = value;
            String[] newtxt = texto.Split(':');
            String[] data = newtxt.GetValue(4).ToString().Split(',');
            var dataUrl = data.GetValue(0).ToString();
            return dataUrl;
        }
    }
}
