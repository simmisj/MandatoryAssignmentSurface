using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Collections.ObjectModel;
using FlickrNet;
using System.ComponentModel;
using System.Net.Sockets;
using System.IO;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Drawing.Imaging;


// Region with version control and things to do next.
#region
/*
 * 10.04.2013 
 * Added Tag Visualization. We will use it to detect our phones.
 * A problem with it is that the tag visualization tool seems to overlay the scatterview and hence resulting
 * in us being unable to manipuate photos. 
 * Possible solution 1: Have the scatterview take one part of the screen and the tag visualization take the other part.
 * Possible solution 2: Somehow make the touch gestures work through the tag visualization.
 * Solution 1: I think I have fixed the issue. I moved the scatterveiw under the tag visualization in the xaml file. Now I can interact with the pictures.
 **/
#endregion
namespace MandatoryAssignmentSurface
{

    


    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {

        // Observable collections.
        #region
        public ObservableCollection<object> phones { get; private set; }
        public ObservableCollection<string> pictures { get; private set; }
        
        public ObservableCollection<APicture> ass { get; private set; }
        #endregion

        // Background workers.
        #region
        // Create a backgroundworker that will ONLY load the images from Flickr. Another backgroundworker
        // will be used for other things. Like load images from phone.
        BackgroundWorker bwFlickrImageLoader = new BackgroundWorker();

        // Create a backgroundworker that will ONLY listen for incomming connections and handle them.
        BackgroundWorker incommingConnection = new BackgroundWorker();


        // Create a backgroundworker that will load simmis pictures.
        BackgroundWorker simmiWorker = new BackgroundWorker();

        // Create a backgroundworker that will load daniels pictures.
        BackgroundWorker danielWorker = new BackgroundWorker();
        #endregion

        // Various variables.
        #region
        Random rnd = new Random();
        TcpListener tcpListener;
        private int port = 3000;
        private string ip = "";
        #endregion

        //Bitmap galaxys2 = new Bitmap("galaxys2.jpg");

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {

            // Stuff that came with the empty project from Surface app.
            #region
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            #endregion

            // Set up variables.
            #region
            phones = new ObservableCollection<object>();
            pictures = new ObservableCollection<string>();
            
            ass = new ObservableCollection<APicture>();
            #endregion

            // Assign stuff to stuff.
            #region
            scatterView1.ItemsSource = pictures;
            #endregion

            // Flickr backgroundworker.
            #region
            // Set the backgroundworker up so it supports cancellation and reports progress.
            bwFlickrImageLoader.WorkerSupportsCancellation = true;
            bwFlickrImageLoader.WorkerReportsProgress = true;

            // Add all the event handlers for the backgroundworker.
            bwFlickrImageLoader.DoWork += new DoWorkEventHandler(bwFlickrImageLoader_DoWork);
            bwFlickrImageLoader.ProgressChanged +=
                new ProgressChangedEventHandler(bwFlickrImageLoader_ProgressChanged);
            bwFlickrImageLoader.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(bwFlickrImageLoader_RunWorkerCompleted);
            #endregion

            // incommingConnection  backgroundworker.
            #region
            // Set the backgroundworker up so it supports cancellation and reports progress.
            incommingConnection.WorkerReportsProgress = true;
            incommingConnection.WorkerSupportsCancellation = true;

            // Add all the event handlers for the backgroundworker.
            incommingConnection.DoWork += new DoWorkEventHandler(incommingConnection_DoWork);
            incommingConnection.ProgressChanged +=
                new ProgressChangedEventHandler(incommingConnection_ProgressChanged);
            incommingConnection.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(incommingConnection_RunWorkerCompleted);
            #endregion

            //simmis backgroundworker.
            #region
            // Set the backgroundworker up so it supports cancellation and reports progress.
            simmiWorker.WorkerSupportsCancellation = true;
            simmiWorker.WorkerReportsProgress = true;

            // Add all the event handlers for the backgroundworker.
            simmiWorker.DoWork += new DoWorkEventHandler(simmiWorker_DoWork);
            simmiWorker.ProgressChanged +=
                new ProgressChangedEventHandler(simmiWorker_ProgressChanged);
            simmiWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(simmiWorker_RunWorkerCompleted);
            #endregion

            //daniel backgroundworker.
            #region
            // Set the backgroundworker up so it supports cancellation and reports progress.
            danielWorker.WorkerSupportsCancellation = true;
            danielWorker.WorkerReportsProgress = true;

            // Add all the event handlers for the backgroundworker.
            danielWorker.DoWork += new DoWorkEventHandler(danielWorker_DoWork);
            danielWorker.ProgressChanged +=
                new ProgressChangedEventHandler(danielWorker_ProgressChanged);
            danielWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(danielWorker_RunWorkerCompleted);
            #endregion

            incommingConnection.RunWorkerAsync();

        }



        // Region with surface methods that I will not be using a lot or not at all.
        // I put them in a region so I can hide them.
        #region


        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        
        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }
        #endregion


        // A region for the incommingConnection backround methods.
        #region
        // This method takes care of the process heavy stuff. It can report progress.
        private void incommingConnection_DoWork(object sender, DoWorkEventArgs e)
        {
            //IPAddress ipAd = IPAddress.Parse( "10.6.6.137");   // pitlab
            IPAddress ipAd = IPAddress.Parse("10.25.239.235");   // itu
            /* Initializes the Listener */
            TcpListener myList = new TcpListener(ipAd, 8000);
            myList.Start();
            //IPAddress ipAd = IPAddress.Parse("10.25.235.182");
            
            //TcpClient tcpclnt = new TcpClient();
            //tcpclnt.Connect(ipAd, 8001);

            
            while (true)
            {
                int index = 1;






                Console.WriteLine("Listening....");
                Socket s = myList.AcceptSocket();
                Console.WriteLine("Connection accepted");

                byte[] b = new byte[4];
                int k = s.Receive(b);
                
                Console.WriteLine("Read: " + k);
                int sizeOfPic = toInt(b);
                Console.WriteLine("Size of pic: " + sizeOfPic);


                string name = "mypicture";

                // Delete when image transfer is working.
                #region
                /*
                byte[] name = new byte[100];
                int secondPass = s.Receive(name);
                var str = System.Text.Encoding.Default.GetString(name);
                for (int i = 0; i < secondPass; i++)
                    Console.WriteLine(Convert.ToChar(name[i]));
                Console.WriteLine("Read2: " + secondPass);
                Console.WriteLine("Name: " + Convert.ToString(str));
                */
                #endregion

                byte[] thePicture = new byte[sizeOfPic];

                int k2 = s.Receive(thePicture);
                Console.WriteLine("K2: " + k2);

                using (MemoryStream stream = new MemoryStream(thePicture))
                {
                    try
                    {
                        int randomint = rnd.Next(0, 10000);
                        Bitmap bmp = new Bitmap(stream);
                        bmp.Save(@"C:\new\" + randomint + ".jpg");

                        incommingConnection.ReportProgress(100, @"C:\new\" + randomint + ".jpg");
                        // Saves the bitmap as a file. We do not need that. 
                        //bmp.Save(@"C:\new\jpd.bmp");
                        index++;
                    }
                    catch (Exception eas)
                    {
                        int euhre = 0;
                    }

                }
                Console.WriteLine("Finished.  "+index);
            }
            // Delete when gotten image transfer to work
            #region 
            //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //socket.Connect("localhost", 8000);
                    //IPEndPoint localEndPoint = new IPEndPoint(ipAD, port);

                //socket.Bind(localEndPoint);
                    /*
                    // 5. loop to accept client connection requests
                    while (true)
                    {
                        Socket clientSocket;
                    try
                    {
                        clientSocket = socket.Accept();
                    }
                    catch
                    {
                    throw;
                    }*/

                //Socket socket2 = socket.Accept();

                //tcpListener = new TcpListener(IPAddress.Any, 3000);
                    //tcpListener.Start();

                //TcpClient client = tcpListener.AcceptTcpClient();


                /*
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(s);

                Console.WriteLine("Client thread started.."+index);
                */
                //index++;

            // }

            #endregion

        }

        private void incommingConnection_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Bitmap bmp = (Bitmap)e.UserState;
            pictures.Add((string)e.UserState);
            
        }

        private void incommingConnection_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Bitmap bmp = (Bitmap)e.Result;
            //pictures.Add(bmp);

            // Dispose it to free up memory. Test if it is okay to use it here.
            //bmp.Dispose();
        }

        private void HandleClientComm(object client)
        {
            Console.WriteLine("Started handleclient method...");
            
            
           
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            BufferedStream stream = new BufferedStream(clientStream);

            int length = readInt(clientStream);
            byte[] pic = new byte[length];


            //byte[] message = new byte[1024];
            //int bytesRead;

            for (int read = 0; read < length; )
            {
                read += stream.Read(pic, read, pic.Length - read);
            }
            stream.Close();

            System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(pic));
            
            image.Save("output.jpg", ImageFormat.Jpeg);  // Or Png
            //pictures.Add(image);                                          //This is a part of it
            Console.WriteLine("image saved");
            //FileStream fs = new FileStream(

            /*
            while (true)
            {
                bytesRead = 0;

                try
                {

                    //blocks until a client sends a message
                    while ((bytesRead = clientStream.Read(message, 0, message.Length)) > 0)
                    {
                        
                    }
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                

                //Bitmap bitmapimage = BitmapFactory.decodeByteArray(pic, 0, bytesRead);


                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
            }
            */

            tcpClient.Close();
        }

        private int readInt(NetworkStream inputStream) 
        {
            byte[] buf = new byte[4];
            for (int read = 0; read < 4; ) {
                read += inputStream.Read(buf, 0, 4);
            }
            return toInt(buf);


        }
        public int toInt(byte[] b)
        {
            return (b[0] << 24)
                    + ((b[1] & 0xFF) << 16)
                    + ((b[2] & 0xFF) << 8)
                    + (b[3] & 0xFF);
        }

        #endregion

        // A region for the Flickr background worker methods. Work, progress changed and work done.
        #region
        // This method takes care of the process heavy stuff. It can report progress and I will use that
        // to load each image.
        private void bwFlickrImageLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Console.WriteLine("Async backgroundworker started");
            
            if ((worker.CancellationPending == true))
            {
                Console.WriteLine("if");
                e.Cancel = true;
                // breaks out of the loop, if there is a loop.
                // break;
            }
            else
            {
                PhotoCollection photos;
                
                Console.WriteLine("else");
                // Perform a time consuming operation and report progress.
                Flickr flickr = new Flickr("59c64644ddddc2a52a089131c8ca0933", "b080535e26aa05df");
                PhotoSearchOptions options = new PhotoSearchOptions();
                //options.Tags = "blue,sky";
                options.Tags = (string)e.Argument;
                options.PerPage = 4; // 100 is the default
                
                try
                {
                    photos = flickr.PhotosSearch(options);
                    photos.Page = 1;
                }
                catch (Exception ea)
                {
                    e.Result = "Exception: "+ea;
                    return;
                }
                
                //scatterView1.ItemsSource = photos;
                //scatterView1.DataContext = photos;

                for (int i = 0; i < photos.Count; i++)
                {
                    // Report progress and pass the picture to the progresschanged method
                    // for the progresschanged method to update the observablecollection.
                    worker.ReportProgress(100, photos[i].Medium640Url);

                    // Add the picture to the ObservableCollection.
                    //pictures.Add(photos[i].Medium640Url);
                    //scatterView1.Items.Add(photos[i].ThumbnailUrl);

                }
                
            }
        }

        // This method I can call from the DO WORK method and send each image to be loaded.
        private void bwFlickrImageLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Add the picture to the ObservableCollection.
            pictures.Add((string)e.UserState);


            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }

        // This method is called when the backgrounworker finishes. I.e. when the DO WORK method is finished.
        private void bwFlickrImageLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                //this.tbProgress.Text = "Canceled!";
                labelDebug.Content = "Cancelled "+e.Result;
            }

            else if (!(e.Error == null))
            {
                //this.tbProgress.Text = ("Error: " + e.Error.Message);
                labelDebug.Content = "Error " + e.Result;
            }
                /*
            else if(string)e.Error.ToString().Substring(0,10).Equals("Exception:"))
            {
                labelDebug.Content = "Exception: " + e.Result;
            }
            */
            else
            {
                //this.tbProgress.Text = "Done!";
                labelDebug.Content = "All done! " + e.Result;
            }
        }
        #endregion


        // A region for danielworker
        #region
        // This method takes care of the process heavy stuff. It can report progress and I will use that
        // to load each image.
        private void danielWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Console.WriteLine("Async backgroundworker started");

            if ((worker.CancellationPending == true))
            {
                Console.WriteLine("if");
                e.Cancel = true;
                
            }
            else
            {
                NetworkStream stream;
                TcpClient tcpclnt;
                try
                {
                    IPAddress ipAd = IPAddress.Parse("10.6.6.129");  // Pitlab
                    //IPAddress ipAd = IPAddress.Parse("10.25.235.182");   // ITU
                    tcpclnt = new TcpClient();
                    Console.WriteLine("Connecting....");
                    tcpclnt.Connect(ipAd, 8000);
                    Console.WriteLine("Connection accepted");
                    stream = tcpclnt.GetStream();
                }
                catch (Exception eas)
                {
                    Console.WriteLine("Exception: " + eas);
                    return;
                }
                //for (int bla = 0; bla < 3; bla++)
                //{
                List<byte[]> pics = new List<byte[]>();

                /*
                byte[] numberOfPics = new byte[4];
                int numberOfBytesRead = stream.Read(numberOfPics, 0, 4);
                
                int numberOfPicsInt = toInt(numberOfPics);
                 * */
                int numberOfPicsInt = readInt(stream); 
                Console.WriteLine("Number of pics: " + numberOfPicsInt);
                int index = 0;

                for (int i = 0; i < numberOfPicsInt; i++)
                {
                    /*
                    byte[] sizeOfPic = new byte[4];
                    numberOfBytesRead = stream.Read(sizeOfPic, 0, 4);
                    
                    int sizeOfPicInt = toInt(sizeOfPic);
                     * */
                    int sizeOfPicInt = readInt(stream);
                    Console.WriteLine("  Int size: " + sizeOfPicInt);

                    int received = 0;

                    byte[] oneAndOnlyPic = new byte[sizeOfPicInt];
                    while (received < sizeOfPicInt)
                    {
                        if ((sizeOfPicInt - received) > 1024)
                        {
                            received += stream.Read(oneAndOnlyPic, received, sizeOfPicInt - received);
                        }
                        else
                        {
                            stream.Read(oneAndOnlyPic, received, sizeOfPicInt - received);
                            received = sizeOfPicInt;
                        }
                    }


                    

                    
                    Console.WriteLine("Number of bytes in pic: " + received);
                    pics.Add(oneAndOnlyPic);

                    Console.WriteLine("Pics read: " + index++);
                    
                }

                tcpclnt.Close();
                stream.Close();

                foreach (byte[] array in pics)
                {


                    using (MemoryStream stream2 = new MemoryStream(array))
                    {
                        try
                        {
                            int randomint = rnd.Next(0, 10000);
                            Bitmap bmp = new Bitmap(stream2);
                            bmp.Save(@"C:\new\" + randomint + ".jpg");

                            danielWorker.ReportProgress(100, @"C:\new\" + randomint + ".jpg");
                            // Saves the bitmap as a file. We do not need that. 
                            //bmp.Save(@"C:\new\jpd.bmp");

                        }
                        catch (Exception eas)
                        {
                            int euhre = 0;
                        }

                    }
                }
                Console.WriteLine("Finished.  ");
                //}
                

            }
        }

        // This method I can call from the DO WORK method and send each image to be loaded.
        private void danielWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Add the picture to the ObservableCollection.
            pictures.Add((string)e.UserState);


            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }

        // This method is called when the backgrounworker finishes. I.e. when the DO WORK method is finished.
        private void danielWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                //this.tbProgress.Text = "Canceled!";
                labelDebug.Content = "Cancelled " + e.Result;
            }

            else if (!(e.Error == null))
            {
                //this.tbProgress.Text = ("Error: " + e.Error.Message);
                labelDebug.Content = "Error " + e.Result;
            }
            /*
        else if(string)e.Error.ToString().Substring(0,10).Equals("Exception:"))
        {
            labelDebug.Content = "Exception: " + e.Result;
        }
        */
            else
            {
                //this.tbProgress.Text = "Done!";
                labelDebug.Content = "All done! " + e.Result;
            }
        }

        #endregion

        // A region for simmiWorker
        #region
        // This method takes care of the process heavy stuff. It can report progress and I will use that
        // to load each image.
        private void simmiWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Console.WriteLine("Async backgroundworker started");

            if ((worker.CancellationPending == true))
            {
                Console.WriteLine("if");
                e.Cancel = true;
                // breaks out of the loop, if there is a loop.
                // break;
            }
            else
            {
                Flickr flickr = new Flickr("59c64644ddddc2a52a089131c8ca0933", "b080535e26aa05df");
                FoundUser user = flickr.PeopleFindByUserName("simmisj");
                //FoundUser user = flickr.PeopleFindByUserName("simmisj");

                //PeoplePhotoCollection people = new PeoplePhotoCollection();

                //people = flickr.PeopleGetPhotosOf(user.UserId);

                PhotoSearchOptions options = new PhotoSearchOptions();
                options.UserId = user.UserId; // Your NSID
                options.PerPage = 4; // 100 is the default anyway
                PhotoCollection photos;

                try
                {
                    photos = flickr.PhotosSearch(options);
                    photos.Page = 1;
                }
                catch (Exception ea)
                {
                    //e.Result = "Exception: " + ea;
                    return;
                }

                for (int i = 0; i < photos.Count; i++)
                {
                    // Report progress and pass the picture to the progresschanged method
                    // for the progresschanged method to update the observablecollection.
                    worker.ReportProgress(100, photos[i].Medium640Url);
                    //pictures.Add(photos[i].Medium640Url);

                    // Add the picture to the ObservableCollection.
                    //pictures.Add(photos[i].Medium640Url);
                    //scatterView1.Items.Add(photos[i].ThumbnailUrl);

                }

            }
        }

        // This method I can call from the DO WORK method and send each image to be loaded.
        private void simmiWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Add the picture to the ObservableCollection.
            pictures.Add((string)e.UserState);


            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }

        // This method is called when the backgrounworker finishes. I.e. when the DO WORK method is finished.
        private void simmiWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                //this.tbProgress.Text = "Canceled!";
                labelDebug.Content = "Cancelled " + e.Result;
            }

            else if (!(e.Error == null))
            {
                //this.tbProgress.Text = ("Error: " + e.Error.Message);
                labelDebug.Content = "Error " + e.Result;
            }
            /*
        else if(string)e.Error.ToString().Substring(0,10).Equals("Exception:"))
        {
            labelDebug.Content = "Exception: " + e.Result;
        }
        */
            else
            {
                //this.tbProgress.Text = "Done!";
                labelDebug.Content = "All done! " + e.Result;
            }
        }

        #endregion

        // Region with buttons.
        #region
        private void surfaceButtonFlickrSearch_Click(object sender, RoutedEventArgs e)
        {
            labelDebug.Content = "Fetching...";
            string tags = surfaceTextBoxFlickrSearch.Text;

            if (!bwFlickrImageLoader.IsBusy)
            {
                bwFlickrImageLoader.RunWorkerAsync(tags);
            }
        }
        #endregion

        // Region concerning visualizing the phone.
        #region
        private void OnVisualizationAdded(object sender, TagVisualizerEventArgs e)
        {
            PhoneVisualization camera = (PhoneVisualization)e.TagVisualization;
            switch (camera.VisualizedTag.Value)
            {
                case 1:
                    camera.PhoneModel.Content = "SG2, Simmi";
                    //camera.myEllipse.Fill = SurfaceColors.Accent1Brush;
                    camera.image1.Source = new BitmapImage(new Uri("/MandatoryAssignmentSurface;component/Resources/galaxys2.jpg", UriKind.Relative));
                    if (!simmiWorker.IsBusy)
                    {
                        simmiWorker.RunWorkerAsync();
                    }
                    //camera.myImage.Source = galaxy
                    //fetchPhotosFromSpecifcUser("simmisj");
                    break;
                case 2:
                    camera.PhoneModel.Content = "Nexus Galaxy, Daniel";
                    camera.image1.Source = new BitmapImage(new Uri("/MandatoryAssignmentSurface;component/Resources/NexusOne.png", UriKind.Relative));
                    //camera.myEllipse.Fill = SurfaceColors.Accent2Brush;

                    if (!danielWorker.IsBusy)
                    {
                        danielWorker.RunWorkerAsync();
                    }
                    break;
                
                default:
                    camera.PhoneModel.Content = "UNKNOWN MODEL";
                    //camera.myEllipse.Fill = SurfaceColors.ControlAccentBrush;
                    break;
            }
        }
        #endregion

        // Region fetching photos from Flickr from a specific account.
        #region
        public void fetchPhotosFromSpecifcUser(string userName)
        {
            Flickr flickr = new Flickr("59c64644ddddc2a52a089131c8ca0933", "b080535e26aa05df");

            FoundUser user = flickr.PeopleFindByUserName(userName);

            //PeoplePhotoCollection people = new PeoplePhotoCollection();

            //people = flickr.PeopleGetPhotosOf(user.UserId);

            PhotoSearchOptions options = new PhotoSearchOptions();
            options.UserId = user.UserId; // Your NSID
            options.PerPage = 4; // 100 is the default anyway
            PhotoCollection photos = flickr.PhotosSearch(options);

            try
            {
                photos = flickr.PhotosSearch(options);
                photos.Page = 1;
            }
            catch (Exception ea)
            {
                //e.Result = "Exception: " + ea;
                return;
            }

            for (int i = 0; i < photos.Count; i++)
            {
                // Report progress and pass the picture to the progresschanged method
                // for the progresschanged method to update the observablecollection.
                pictures.Add(photos[i].Medium640Url);

                // Add the picture to the ObservableCollection.
                //pictures.Add(photos[i].Medium640Url);
                //scatterView1.Items.Add(photos[i].ThumbnailUrl);

            }
            
        }
        #endregion
        
        private void sendPicture(string ip, string picString)
        {
            NetworkStream stream;
            TcpClient tcpclnt;
            try
            {

                IPAddress ipAd = IPAddress.Parse(ip);  
                //IPAddress ipAd = IPAddress.Parse("10.6.6.129");  // Pitlab
                //IPAddress ipAd = IPAddress.Parse("10.25.235.182");   // ITU
                tcpclnt = new TcpClient();
                Console.WriteLine("Connecting....");
                tcpclnt.Connect(ipAd, 8001);
                Console.WriteLine("Connection accepted");
                stream = tcpclnt.GetStream();
            }
            catch (Exception eas)
            {
                Console.WriteLine("Exception: " + eas);
                return;
            }
             
            Bitmap bmp = new Bitmap(picString);

            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

            byte[] pic = new byte[memoryStream.Length];

            // Length of picture in bytes.
            byte[] mybyt = BitConverter.GetBytes(memoryStream.Length);
            stream.Write(mybyt, 0, 4);

            pic = memoryStream.ToArray();
            int sent = 0;
                
            while (sent < memoryStream.Length)
            {
                if ((memoryStream.Length - sent) > 1024)
                {
                    Console.WriteLine("Sending: " + sent + " to: " + (sent + 1024) + " out of: " + memoryStream.Length);
                    stream.Write(pic, sent, 1024);
                    
                    sent += 1024;
                }
                else
                {
                    Console.WriteLine("Sending: " + sent + " to:" + memoryStream.Length + " out of: " + memoryStream.Length);
                    stream.Write(pic, sent, (int)(memoryStream.Length - sent));
                    
                    
                    sent = (int)memoryStream.Length;
                }
            }




            Console.WriteLine("Sending complete.");
            tcpclnt.Close();
            memoryStream.Close();
        }

        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            Console.WriteLine("touchdown");
            /*
            var copy = new ObservableCollection<object>(pictures);
            foreach(var item in copy)
            {
                if(item.Equals(e.Source))
                {
                    pictures.Remove(item);
                
                }   

            }*/
            string f = "";
            System.Windows.Controls.Image test = (System.Windows.Controls.Image)sender;
            string image_source = (string)(test.Source.ToString()).Substring(8);

            //sendPicture("10.25.235.182", image_source);
            
            //System.Windows.Controls.Image img = (System.Windows.Controls.Image)e.Source;

            /*
            foreach(string i in pictures)
            {
                if (sender.Equals(i))
                {
                    Console.WriteLine("YES");
                }
            }

            Bitmap map = new Bitmap( pictures[0]);
*/            
            //System.Drawing.Image img2 = ConvertControlsImageToDrawingImage(img); // Does not work.
            //img2.Save(@"C:\new\bull.jpg");
           //string pathToPic = @"C:\new\bull\bull.jpg";

            //sendPicture("10.25.235.182", pathToPic);
            
            int y = 0;
            //System.Windows.Controls.Image img = (System.Windows.Controls.Image)e.Source;
            //pictures.Remove(img.Source);
            //Console.WriteLine(e.TouchDevice);
            //Random rnd = new Random();
           // pictures.Remove(0);
        }

        // Does not work.
        public System.Drawing.Image ConvertControlsImageToDrawingImage(System.Windows.Controls.Image imageControl)
        {
            RenderTargetBitmap rtb2 = new RenderTargetBitmap((int)imageControl.Width, (int)imageControl.Height, 90, 90, PixelFormats.Default);
            rtb2.Render(imageControl);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb2));

            Stream ms = new MemoryStream();
            png.Save(ms);

            ms.Position = 0;

            System.Drawing.Image retImg = System.Drawing.Image.FromStream(ms);
            return retImg;
        }

        private void surfaceButton1_Click(object sender, RoutedEventArgs e)
        {
            //pictures.Add(@"c:\new\9747.jpg");
            APicture pic = new APicture("9747.jpg", @"c:\new\9747.jpg", "simmi");
            pictures.Add(pic.ToString());
            //sendPicture("10.25.235.182", @"C:\new\9747.jpg");

            //Bitmap bmp = pictures[0];

            //sendPicture("10.25.235.182", "");
        }

        private void Image_TouchUp(object sender, TouchEventArgs e)
        {
            Console.WriteLine("touchup");
           // Console.WriteLine(e.Device.GetPosition(this));
            
                
            
        }

        private void Image_TouchMove(object sender, TouchEventArgs e)
        {
            Console.WriteLine("touchmove");
        }

        private void Image_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            Console.WriteLine("previewtouchup");
        }
    }
}