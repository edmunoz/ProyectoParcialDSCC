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

using ZXing;

namespace ProyectoParcialDSCC
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   

        IBarcodeReader reader = new BarcodeReader();
        
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
            //Ethernet
            this.ethernetJ11D.NetworkInterface.Open();
            this.ethernetJ11D.NetworkInterface.EnableDhcp();
            this.ethernetJ11D.UseThisNetworkInterface();


        }

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            String codigoEstudiante = "200832533";
            String url = "https://ws.espol.edu.ec/saac/wsandroid.asmx/wsInfoEstudianteCarrera?codigoEstudiante=" + codigoEstudiante;
            try
            {
                if (this.ethernetJ11D.IsNetworkConnected)
                {
                    Debug.Print("IsNetworkConnected");
                    HttpRequest request = HttpHelper.CreateHttpGetRequest(url);
                    request.ResponseReceived += request_ResponseReceived;
                    request.SendRequest();
                }
                else
                    Debug.Print("IsNotNetworkConnected");
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }


        void request_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            Debug.Print("request_ResponseReceived");

            var x = response.Text;
            Debug.Print(x);
            
        }
    }
}
