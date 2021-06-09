using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using Microsoft.Research.SEAL;
using System.Diagnostics;

namespace HEtest
{
    public class MainPage : ContentPage
    {
        private VS vs;
        private CS cs;
        private User user;

        Entry longitudeText;
        Entry latitudeText;
        Label printer;

        Button mainEvaluate;

        public MainPage()
        {
            Padding = new Thickness(10, 10, 10, 10);
            StackLayout panel = new StackLayout
            {
                Spacing = 15
            };

            panel.Children.Add(new Label
            {
                Text = "Input longitude:",
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });

            panel.Children.Add(longitudeText = new Entry
            {
                Text = "0.0",
            });

            panel.Children.Add(new Label
            {
                Text = "Input latitude:",
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
            });

            panel.Children.Add(latitudeText = new Entry
            {
                Text = "0.0",
            });

            panel.Children.Add(mainEvaluate = new Button
            {
                Text = "Login"
            });
            panel.Children.Add(printer = new Label
            {
                Text = "Ready to test."
            });
            mainEvaluate.Clicked += OnMainEvaluate;
            this.Content = panel;

            // Server information initialization
            double px = 2.53, py = -8.12;  // Password point

            // Create Verification Server, Computing Server and User terminal
            // The CS sends PublicKey to the user and RelinKeys to the VS
            cs = new CS();
            vs = new VS(cs.sendRelinKeys());
            user = new User(cs.sendPublicKey());

            // The user sets a Password point and saves it encrypted on VS.
            Ciphertext px_encrypted = new Ciphertext();
            Ciphertext py_encrypted = new Ciphertext();
            user.encryptedCoordinates(px, px_encrypted);
            user.encryptedCoordinates(py, py_encrypted);
            vs.setPasswordPoint(px_encrypted, py_encrypted);
        }

        private void OnMainEvaluate(object sender, EventArgs e)
        {
            this.printer.Text = "Just for the convenience of demonstration, we printed " +
                "the information of intermediate process. In practical application, only" +
                " whether the login is successful or not will be returned.\n\n";
            demonstrationMode();
        }

        private void demonstrationMode()
        {
            // Just for the convenience of demonstration, we printed the information
            // of intermediate process. In practical application, only whether the
            // login is successful or not will be returned.
            string printer = "";  // Print test information
            Stopwatch sw;  // Timing

            // Get response point
            printer += "Show password point and verification point.\n";
            double rx = System.Convert.ToDouble(this.longitudeText.Text);
            double ry = System.Convert.ToDouble(this.latitudeText.Text);
            printer += String.Format("Password point:  ( {0} , {1} )\n",
                                     vs.showPasswordPointLongitude(cs),
                                     vs.showPasswordPointLatitude(cs));
            printer += String.Format("Response point:  ( {0} , {1} )\n\n", rx, ry);

            // The user inputs a Response point and encrypts it.
            printer += "User encrypt response point and send it to VS...... ";
            sw = Stopwatch.StartNew();  // Timing start
            Ciphertext rx_encrypted = new Ciphertext();
            Ciphertext ry_encrypted = new Ciphertext();
            user.encryptedCoordinates(rx, rx_encrypted);  // Encrypt rx to rx_encrypted
            user.encryptedCoordinates(ry, ry_encrypted);
            sw.Stop();  // Timing stop
            printer += String.Format("finish.\n--------  Time: {0} ms.\n",
                                     sw.Elapsed.TotalMilliseconds.ToString());

            // In order to show clearly, we split the verification process.
            // User sends the encrypted Response point to VS. Then VS calculates
            // the encrypted distance.
            printer += "Calculate distance in VS ...... ";
            sw = Stopwatch.StartNew();
            Ciphertext encrypted_distance = new Ciphertext();
            vs.computeDistance(rx_encrypted, ry_encrypted, encrypted_distance);
            sw.Stop();
            printer += String.Format("finish.  --------  Time: {0} ms.\n",
                                     sw.Elapsed.TotalMilliseconds.ToString());

            // VS send the encrypted distance to CS. CS decrypts it and compares
            // it with the tolerant distance. Then the comparing result is sent
            // to VS.
            printer += "Verify the distance in CS ...... ";
            sw = Stopwatch.StartNew();
            bool comparingResult = vs.verifyDistanceFromCS(cs, encrypted_distance);
            sw.Stop();
            printer += String.Format("finish.  --------  Time: {0} ms.\n",
                                     sw.Elapsed.TotalMilliseconds.ToString());

            // Allow users to access the system after successful verification.
            printer += vs.showContent(comparingResult);

            // Show calculation results.
            printer += "\nShow calculation results.\nExpected result:  ";
            double px = vs.showPasswordPointLongitude(cs);
            double py = vs.showPasswordPointLatitude(cs);
            printer += ((px - rx) * (px - rx) + (py - ry) * (py - ry));
            printer += "\nComputed result:  ";
            printer += cs.showCiphertext(encrypted_distance);

            ulong megabytes = MemoryManager.GetPool().AllocByteCount >> 20;
            printer += String.Format("\n\n[{0,5} MB] Total allocation " +
                                     "from the memory pool.", megabytes);
            this.printer.Text += printer;
        }
    }
}