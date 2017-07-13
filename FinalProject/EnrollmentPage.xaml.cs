using System;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Verification;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FinalProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EnrollmentPage : Page
    {
        private EnrollmentController enrollmentController;
        private Recorder recorder;
        private User user;
        private bool recording = false;
        private bool recorded = false;
        private bool newUser = true;
        private BitmapImage record = new BitmapImage(new Uri("ms-appx:///Assets/recording.png"));
        private BitmapImage stopRecording = new BitmapImage(new Uri("ms-appx:///Assets/stopRecording.png"));

        public EnrollmentPage()
        {
            this.InitializeComponent();

            enrollmentController = new EnrollmentController();
            recorder = new Recorder();

            LoadPhrases();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Welom the User when opening the page
            user = (User)e.Parameter;
            Synthesizer.Speak("Welcome " + user.firstName + " " + user.lastName + ". I will need you to select one of the phrases and record your voice.");
        }

        private async void LoadPhrases()
        {
            //Create user profile
            Guid speakerId = await enrollmentController.CreateProfile();
            user.speakerId = speakerId.ToString();
            //Get and show all available phrases for the user to select
            VerificationPhrase[] verificationPhrases = await enrollmentController.GetPhrases();
            foreach (VerificationPhrase verificationPhrase in verificationPhrases)
            {
                phrasesDropdown.Items.Add(verificationPhrase.Phrase);
            }
        }

        private async void recordingButtonClicked(object sender, RoutedEventArgs e)
        {
            //If the user has not selected a phrase
            if (phrasesDropdown.SelectedItem == null)
            {
                Synthesizer.Speak("Select a Phrase before recording your voice.");
            }
            else
            {
                //if the user is not recording
                if (!recording)
                {
                    //set user's phrase from the one selected
                    user.phrase = phrasesDropdown.SelectedItem.ToString();
                    //start recording
                    await recorder.StartRecording();
                    //Change image shown in UI 
                    recordingImage.Source = stopRecording;
                    recording = true;

                }
                else
                {
                    //if the user is recording
                    recorder.StopRecording();
                    //Change image shown in UI 
                    recordingImage.Source = record;
                    newUser = false;
                    recording = false;
                    //set counter to make sure user doesn't user the same .wav file for sucessive enrollments
                    recorded = true;
                }
            }
        }

        private async void enrollmentButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!newUser)
            {
                if (recorded)
                {
                    progressRing.IsActive = true;
                    //Enroll speaker
                    Enrollment enrollmentResult = await enrollmentController.EnrollSpeaker();
                    progressRing.IsActive = false;
                    if (enrollmentResult != null)
                    {
                        enrollmentController.firstEnrollment = false;
                        int remainingEnrollments = enrollmentResult.RemainingEnrollments;
                        string enrollmentStatus = enrollmentResult.EnrollmentStatus.ToString();

                        if (!enrollmentStatus.Equals("Enrolled"))
                        {
                            if (remainingEnrollments == 1)
                            {
                                Synthesizer.Speak("The system needs " + remainingEnrollments + " last enrollment, please repeat the previous step.");
                            }
                            else
                            {
                                Synthesizer.Speak("The system needs " + remainingEnrollments + " more enrollments, please repeat the previous step.");
                            }
                            recorded = false;
                        }
                        else
                        {
                            //Add user to database
                            SqliteTransactions.AddUser(user);
                            Synthesizer.Speak("You have been succesfully enrolled. Contact the Administrator to get access to the room.");
                            this.Frame.GoBack();
                        }
                    }
                }
                else
                {
                    Synthesizer.Speak("I need a different audio sample. Please repeat the previous step.");
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
