using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Verification;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FinalProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VerificationPage : Page
    {
        private Recorder recorder;
        private VerificationController verificationController;
        private User user;
        private bool recording = false;
        private bool newUser = true;
        private bool isAdmin = false;
        private int failedVerifications = 0;
        private BitmapImage record = new BitmapImage(new Uri("ms-appx:///Assets/recording.png"));
        private BitmapImage stopRecording = new BitmapImage(new Uri("ms-appx:///Assets/stopRecording.png"));
        private BitmapImage forwardArrow = new BitmapImage(new Uri("ms-appx:///Assets/forward-arrow.png"));
        private Guid speakerID;

        public VerificationPage()
        {
            this.InitializeComponent();

            recorder = new Recorder();
            verificationController = new VerificationController();
            speakerID = new Guid();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Welcome the user when opening the page
            user = (User)e.Parameter;
            Synthesizer.Speak("Welcome. I need to make sure you are " + user.firstName + user.lastName);
            //Show the user the phrase he or she has to say
            phrase.Text = user.phrase;
            speakerID = Guid.Parse(user.speakerId);
            if (user.userId.Equals("A20387795"))
            {
                enrollmentImage.Source = forwardArrow;
                isAdmin = true;
                user.access = true;
            }
        }

        private async void recordingButtonClicked(object sender, RoutedEventArgs e)
        {
            //If the user is not recording
            if (!recording)
            {
                //Start recording
                await recorder.StartRecording();
                //Change image shown in UI
                recordingImage.Source = stopRecording;
                recording = true;

            }
            else
            {
                //Stop recoring
                recorder.StopRecording();
                //Change image shown in UI
                recordingImage.Source = record;
                newUser = false;
                recording = false;
            }
        }

        private async void verificationButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!newUser)
            {
                progressRing.IsActive = true;
                Verification verificationResult = await verificationController.VerifySpeaker(speakerID);
                progressRing.IsActive = false;
                if (verificationResult != null)
                {
                    if (verificationResult.Result.ToString().Equals("Accept") &&
                    (verificationResult.Confidence.ToString().Equals("High") || verificationResult.Confidence.ToString().Equals("Normal")))
                    {
                        if (!isAdmin)
                        {
                            if (user.access)
                            {
                                Synthesizer.Speak("Please come on in.");
                                //OPEN DOOR
                                MainPage.DLPin.Write(Windows.Devices.Gpio.GpioPinValue.High);
                                await Task.Delay(10000);
                                MainPage.DLPin.Write(Windows.Devices.Gpio.GpioPinValue.Low);
                                this.Frame.GoBack();
                            }
                            else
                            {
                                Synthesizer.Speak("I am sorry " + user.firstName + ", you are not allowed in this room. You should contact the Administrator.");
                                this.Frame.GoBack();
                            }
                        }
                        else
                        {
                            Synthesizer.Speak("Opening Administrator page.");
                            this.Frame.Navigate(typeof(AdminPage));
                        }
                    }
                    else
                    {
                        failedVerifications++;
                        if (failedVerifications < 3)
                        {
                            Synthesizer.Speak("I am sorry Dave, I'm afraid I cannot do that."); 
                        }
                        else
                        {
                            SqliteTransactions.ModifyAccessByUserId(user.userId, false);
                            Synthesizer.Speak("For security purposes, I have to deny your access to the room. You should contact the Administrator.");
                            this.Frame.GoBack();
                        }
                    }
                }
            }
            else
            {
                Synthesizer.Speak("I need you to record your voice first.");
            }
        }

        private void backButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
