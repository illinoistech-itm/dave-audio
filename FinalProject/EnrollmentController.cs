using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Verification;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FinalProject
{
    class EnrollmentController
    {
        private Guid speakerId = Guid.Empty;
        private SpeakerVerificationServiceClient serviceClient = null;
        private Profile profile = null;
        private bool profileCreated = false;
        public bool firstEnrollment = true;

        public EnrollmentController()
        {
            serviceClient = new SpeakerVerificationServiceClient(Keys.subscriptionKey);
        }

        public async Task<Enrollment> EnrollSpeaker()
        {
            if (profileCreated)
            {
                try
                {
                    //Get .wav file
                    var audioStream = await Recorder.GetStreamAsync();
                    //Enroll Speaker using API
                    Enrollment enrollmentResult = await serviceClient.EnrollAsync(audioStream.AsStream(), speakerId);
                    return enrollmentResult;
                }
                catch (Microsoft.ProjectOxford.SpeakerRecognition.Contract.EnrollmentException e)
                {
                    if (e.Message.Equals("InvalidPhrase"))
                    {
                        if (firstEnrollment)
                        {
                            Synthesizer.Speak("I am sorry, I couldn't recognize a phrase from the list.");
                            Debug.WriteLine("I am sorry, I couldn't recognize a phrase from the list.");
                        }
                        else
                        {
                            Synthesizer.Speak("I am sorry, I need you to say the same phrase the three times.");
                            Debug.WriteLine("I am sorry, I need you to say the same phrase the three times.");
                        }

                    }
                    else if (e.Message.Equals("SpeechNotRecognized"))
                    {
                        Synthesizer.Speak("I am sorry, I couldn't recognize a human's voice. Please try again.");
                        Debug.WriteLine("I am sorry, I couldn't recognize a human's voice. Please try again.");
                    }
                    else
                    {
                        Debug.WriteLine("Couldn't enroll Speaker: " + e.Message);
                        Synthesizer.Speak("Something went wrong. Please try again.");
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Synthesizer.Speak("Something went wrong. Please try again.");
                    Debug.WriteLine("Couldn't enrollSpeaker: " + e.Message);
                    return null;
                }
            }
            else
            {
                Debug.WriteLine("Create Profile first.");
                return null;
            }
        }

        public async Task<Guid> CreateProfile()
        {
            try
            {
                //Create prifile for the specified language
                CreateProfileResponse response = await serviceClient.CreateProfileAsync("en-us");
                speakerId = response.ProfileId;
                profile = await serviceClient.GetProfileAsync(speakerId);
                Debug.WriteLine("Profile Created with SpekaerID: " + speakerId);
                profileCreated = true;
                return speakerId;
            }
            catch (Microsoft.ProjectOxford.SpeakerRecognition.Contract.CreateProfileException e)
            {
                Debug.WriteLine("Couldn't create Profile in createProfile(): " + e.Message);
                Synthesizer.Speak("Something went wrong. Go back and try again.");
                profileCreated = false;
                return Guid.Empty;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Couldn't create Profile in createProfile(). Error: " + e.Message);
                Synthesizer.Speak("Something went wrong. Go back an try again.");
                profileCreated = false;
                return Guid.Empty;
            }

        }

        public async Task<VerificationPhrase[]> GetPhrases()
        {
            //Get all the possible registration phrases for the specified language
            VerificationPhrase[] phrases = await serviceClient.GetPhrasesAsync("en-us");
            return phrases;
        }
    }
}
