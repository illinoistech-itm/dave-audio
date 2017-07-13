using System;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FinalProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int D0 = 6;
        private const int D1 = 5;
        private const int DL = 13;
        private GpioPin D0Pin;
        private GpioPin D1Pin;
        public static GpioPin DLPin;
        private int bitCount = 0;
        private int[] array = new int[100];
        private int[] cardNumber = new int[20];
        String cardNum;
        private int ones = 0;

        private DispatcherTimer timer;
        private SoapService soapService;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            soapService = new SoapService();
            StartScenario();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            D0Pin.ValueChanged -= D0Pin_ValueChanged;
            D1Pin.ValueChanged -= D1Pin_ValueChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            D0Pin.ValueChanged += D0Pin_ValueChanged;
            D1Pin.ValueChanged += D1Pin_ValueChanged;
        }

        private void StartScenario()
        {
            //Start GPIO pins as input
            var gpio = GpioController.GetDefault();
            D0Pin = gpio.OpenPin(D0);
            D1Pin = gpio.OpenPin(D1);
            DLPin = gpio.OpenPin(DL);
            D0Pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            D1Pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            DLPin.SetDriveMode(GpioPinDriveMode.Output);
            DLPin.Write(GpioPinValue.Low);
        }

        private async void Timer_Tick(object sender, object e)
        {
            //This part of the code will be reached if a time equal to timer.Interval is exceded after reading a bit
            Debug.WriteLine(bitCount);
            Debug.WriteLine(ones);
            timer.Stop();
            //We are working wih 35 bit Cards
            if (bitCount == 35 && ones%2 != 0)
            {
                bitCount = 0;
                progressRing.IsActive = true;
                //Get user information through the SOAP service using 6 bits Card Number
                String[] userData = await soapService.GetUserInfo(FormatCard());
                progressRing.IsActive = false;

                //If there is a user in IIT's database with that 6 digit card number
                if (userData != null)
                {
                    String lastName = userData[0];
                    String firstName = userData[1];
                    String userId = userData[3];
                    Debug.WriteLine(userId + ": " + firstName + " " + lastName);

                    //Check if user is already registered in the Speaker Recognition Database
                    User user = SqliteTransactions.GetUserByUserId(userId);

                    //if not
                    if (user == null)
                    {
                        //Create new user with user info and open Enrollment Page
                        user = new User();
                        user.firstName = firstName;
                        user.lastName = lastName;
                        user.userId = userId;
                        user.access = false;
                        OpenEnrollment(user);
                    }
                    else
                    {
                        OpenVerification(user);
                    }
                }
                else
                {
                    Debug.WriteLine("Card reading was wrong, or user not found.");
                    Synthesizer.Speak("Scan your card again.");
                }

            }
            else
            {
                //If the number of bits is different from 35 or odd number of ones, we will take it as a wrong read
                Synthesizer.Speak("Scan your card again.");
                bitCount = 0;
                FormatCard();
            }
        }

        private void D0Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            //When a Falling Edge is detected in D0, a 0 has to be read
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                bitCount++;
                var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //Start timer to see if there are any bits left
                    timer.Start();
                });
            }
        }

        private void D1Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            //When a Falling Edge is detected in D1, a 1 has to be read
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                ones++;
                array[bitCount] = 1;
                bitCount++;
                var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //Start timer to see if there are any bits left
                    timer.Start();
                });
            }
        }

        private string FormatCard()
        {
            Debug.WriteLine("Data: ");
            foreach (var item in array)
            {
                //Print all bits 
                Debug.Write(item);
            }
            Debug.WriteLine("");

            Array.ConstrainedCopy(array, 14, cardNumber, 0, cardNumber.Length);
            Debug.WriteLine("Card Number: ");
            foreach (var item in cardNumber)
            {
                //Print Card Number section
                Debug.Write(item);
            }
            //Convert Card  Number to decimal value
            cardNum = Binary2decimal(cardNumber).ToString();
            Debug.Write("(" + Binary2decimal(cardNumber) + ")");
            Debug.WriteLine("");
            Debug.WriteLine("");
            ClearDataArrays();
            ones = 0;
            return cardNum;
        }

        private void ClearDataArrays()
        {
            Array.Clear(array, 0, array.Length);
            Array.Clear(cardNumber, 0, cardNumber.Length);
        }

        private int Binary2decimal(int[] binaryArray)
        {
            int dec = 0;
            for (int i = 0; i < binaryArray.Length; i++)
            {
                dec += binaryArray[i] * (int)Math.Pow(2, binaryArray.Length - 1 - i);
            }
            return dec;
        }

        private void OpenEnrollment(User user)
        {
            //Open Enrollment Page
            this.Frame.Navigate(typeof(EnrollmentPage), user);
        }

        private void OpenVerification(User user)
        {
            //Open Verification Page
            this.Frame.Navigate(typeof(VerificationPage), user);
        }
    }
}
