
using System;
using System.Threading;
using System.Speech.Recognition;

namespace MerlinSpeechRecongnition
{
    class Program
    {
        // Create a new SpeechRecognitionEngine instance.
        //static SpeechRecognizer recognizer = new SpeechRecognizer();

        static SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine
                (new System.Globalization.CultureInfo("es-ES"));

        public static Program instance;
        static bool wildcardExpected = false;

        static bool listening = false;

        static void Main(string[] args)
        {
            //Use only once to convert to binary XML files.
            //XMLEncryption encryptXML = new XMLEncryption();
            //encryptXML.EncryptXML();
            Console.WriteLine("Close this console after you are done with the game.");
            // Configure the input to the speech recognizer.
            recognizer.SetInputToDefaultAudioDevice();
            Console.WriteLine(recognizer.RecognizerInfo.Culture);
            Console.WriteLine(recognizer.RecognizerInfo.Description);
            AMQ_Connection.GetConnectionInstance();
        }

        /// <summary>
        /// Updated version of the recognizer. This excludes the Windows commands
        /// from being recognized and executed, and stops C# from opening a recognition
        /// window. 
        /// Info: http://bit.ly/20JKie8
        /// Info: http://bit.ly/1mr7awj
        /// <see cref="recognizeGrammar"/>
        /// <param name="choices">List of words in current grammar</param>
        /// </summary>
        public static void inProcRecognition(string[] choices)
        {
            Console.WriteLine("Recognizer before inProc " + recognizer.RecognizerInfo.Culture);
            Console.WriteLine("Recognizer before inProc " + recognizer.RecognizerInfo.Description);
            recognizer.UnloadAllGrammars();
                recognizer.RecognizeAsyncStop();
                recognizer.RequestRecognizerUpdate();
                GrammarBuilder gb = null;
                if (choices[0].Equals("wildcard"))
                {
                    Console.WriteLine("Adding wildcard to the grammar");
                    gb = new GrammarBuilder();
                    gb.AppendWildcard();
                    wildcardExpected = true;
                }
                else
                {
                    gb = new GrammarBuilder();
                    Choices responses = new Choices();
                    responses.Add(choices);
                    Console.WriteLine("Adding choices to the grammar " + choices.ToString());
                    gb.Append(responses);
                }

                Grammar g = new Grammar(gb);
                recognizer.LoadGrammarAsync(g);
                Thread.Sleep(150);
                recognizer.RequestRecognizerUpdate();
            Console.WriteLine("Recognizer after inProc " + recognizer.RecognizerInfo.Culture);
            Console.WriteLine("Recognizer after inProc " + recognizer.RecognizerInfo.Description);

            // Start asynchronous, continuous speech recognition.
            //recognizer.RecognizeAsync(RecognizeMode.Multiple);
            try
                {
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);
                }

                catch (System.InvalidOperationException rec) 
                { 

                }

                // Register a handler for the SpeechRecognized event.
                recognizer.SpeechRecognized +=
                    new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
                listening = true;
        }

        /// <summary>
        ///  Create a simple handler for the SpeechRecognized event.
        /// </summary>
        /// <seealso cref="recognizeGrammar"/>
        /// <param name="sender"></param>
        /// <param name="e"></param
        static void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (listening)
            {
                if (wildcardExpected)
                {
                    AMQ_Connection.GetConnectionInstance().SendMessage("wildcard");
                    wildcardExpected = false;
                    Console.WriteLine("Sent wildcard to Unity");
                }
                else
                {
                    Console.WriteLine("Sent " + e.Result.Text +  " to Unity");
                    AMQ_Connection.GetConnectionInstance().SendMessage(e.Result.Text);
                }

                listening = false;
                recognizer.UnloadAllGrammars();
                recognizer.RecognizeAsyncStop();
                recognizer.RequestRecognizerUpdate();
            }
        }
    }
}
