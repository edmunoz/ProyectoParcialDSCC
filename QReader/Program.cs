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

namespace QReader
{
    enum State { Main, Camera, Webcam, Habilitado, Deshabilitado};
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.
        private bool isStreaming;
        State systemState;
        Bitmap currentBitmap;
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
            isStreaming = false;
            //Funciones de Camara
            camera.BitmapStreamed += camera_BitmapStreamed;
            camera.CameraConnected += camera_CameraConnected;
            camera.PictureCaptured += camera_PictureCaptured;


            //Conexion a Internet
            this.ethernetJ11D.NetworkInterface.Open();
            this.ethernetJ11D.NetworkInterface.EnableDhcp();
            this.ethernetJ11D.UseThisNetworkInterface();
            this.ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            this.ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;

            //imagen del qr
            currentBitmap = new Bitmap(camera.CurrentPictureResolution.Width, camera.CurrentPictureResolution.Height);

        }

        void camera_PictureCaptured(Camera sender, GT.Picture e)
        {
            if (systemState == State.Habilitado) { 
                //metodo para subir las imagenes
            }
        }

        void camera_CameraConnected(Camera sender, EventArgs e)
        {
            Debug.Print("CameraConnected");
            //camera.StartStreaming();
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
            HttpRequest pedido = HttpHelper.CreateHttpGetRequest(GetUrlFromJson(resultado.ToString()));
            pedido.ResponseReceived += pedido_ResponseReceived;
            pedido.SendRequest();
            Debug.Print(resultado);
            Debug.Print("Enviando Url de QR");

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
