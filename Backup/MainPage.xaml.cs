using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Text;
using Venetasoft.WP.Net;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Venetasoft;
using System.Net.NetworkInformation;
using Venetasoft.WP7;

namespace MailMessageDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        MailMessage mailMessage = null;

        public MainPage()
        {
            InitializeComponent();

            if (mailMessage == null)
            {
                //create MailMessage object
                mailMessage = new MailMessage();
             
                //set message event handlers
                mailMessage.Error += mailMessage_Error;
                mailMessage.MailSent += mailMessage_MailSent;
                mailMessage.Progress += mailMessage_Progress;

            }


            //debug info: show attachments that will be added to mail (from resource) => see code in buttonSend_Click()
            this.listBoxAttachments.Items.Add("document.pdf - " + FormatBytes(MailMessage.FileSize("resources/document.pdf")));
            this.listBoxAttachments.Items.Add("sound.wav - " + FormatBytes(MailMessage.FileSize("resources/sound.wav")));
            this.listBoxAttachments.Items.Add("track.mp3 - " + FormatBytes(MailMessage.FileSize("resources/track.mp3")));
        }

        void SendMail()
        {
            try
            {
                #region validation checks
                if (NetworkInterface.GetIsNetworkAvailable() == false)
                {
                    MessageBox.Show("Network is unavailable.");
                    return;
                }
                if (mailMessage != null && mailMessage.Busy == true)
                {
                    MessageBox.Show("Pending operation in progress, please wait..");
                    return;
                }
                if (String.IsNullOrEmpty(textBoxUserName.Text.Trim()))
                {
                    MessageBox.Show("Please insert a valid value in the 'UserName' field (i.e username@hotmail.com).");
                    return;
                }
                if (String.IsNullOrEmpty(textBoxPassword.Password))
                {
                    MessageBox.Show("Please insert a valid value in the 'Password' field.");
                    return;
                }
                string[] recipients = textBoxMailTo.Text.Trim().Replace(',', ';').Split(';');
                foreach (var sto in recipients)
                {
                    if (MailMessage.IsValidEmailAddress(sto) == false)
                    {
                        MessageBox.Show("Please insert a valid email address in the 'To' field.");
                        return;
                    }
                }

                if (String.IsNullOrEmpty(textBoxSubject.Text.Trim()))
                {
                    MessageBox.Show("Please insert a valid value in the 'Subject' field.");
                    return;
                }
                #endregion

                //in case of large attachments/slow connection, disable phone auto-lock or wifi will be dropped and email sending  will be aborted
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

                mailMessage.UserName = textBoxUserName.Text; //i.e:  username@hotmail.com 
                mailMessage.Password = textBoxPassword.Password;

                mailMessage.AccountType = MailMessage.AccountTypeEnum.Unknown;
                if (textBoxUserName.Text.ToLower().Contains("@gmail"))
                    mailMessage.AccountType = MailMessage.AccountTypeEnum.Gmail;
                else if (textBoxUserName.Text.ToLower().Contains("@live") || textBoxUserName.Text.ToLower().Contains("@hotmail"))
                    mailMessage.AccountType = MailMessage.AccountTypeEnum.MicrosoftAccount;
                else
                    mailMessage.SetCustomSMTPServer("smtp.mySmtpMailServer.com", 25, true);  //custom smtp server <=============                

                if (MailMessage.IsValidEmailAddress(mailMessage.UserName) == true)
                    mailMessage.From = mailMessage.UserName;
                else
                    mailMessage.From = "foo@foo.com";


                mailMessage.To = textBoxMailTo.Text;   //you can add multiple recipients separated by ';'
                //mailMessage.ReplyTo = xxxx@yyy.com;  //you can add multiple recipients separated by ';' 
                //mailMessage.Cc = xxxx@yyy.com;       //you can add multiple recipients separated by ';'
                //mailMessage.Bcc = xxxx@yyy.com;      //you can add multiple recipients separated by ';'
                mailMessage.Subject = textBoxSubject.Text;
                mailMessage.Body = textBoxBody.Text;   //text or HTML (body must start with <html> or <!DOCTYPE HTML>)
                //mailMessage.CharSet = "gb2312";      //set for international charset ("gb2312", "big5", etc), default is "utf-8"


                mailMessage.AddAttachment(Encoding.UTF8.GetBytes("Hello from WP !!".ToCharArray()), "memoryfile.txt"); //in-memory file attachment

                mailMessage.AddAttachment("resources/document.pdf"); //resource or IsolatedStorage path
                mailMessage.AddAttachment("resources/sound.wav");    //any type of file can be attacched
                mailMessage.AddAttachment("resources/track.mp3");    //any type of file can be attacched

                mailMessage.Send(); //Asyncronous call

                buttonSend.Content = "ABORT";
                buttonAddAttachment.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                buttonSend.Content = "SEND";
                buttonAddAttachment.IsEnabled = true;
            }
        }

        #region UI handlers
        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mailMessage.Busy == false)
                    SendMail();
                else
                    mailMessage.Abort();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void chkHTMLBody_Click(object sender, RoutedEventArgs e)
        {
            if (chkHTMLBody.IsChecked == false)
                this.textBoxBody.Text = "My App can send email with every kind of attachment now!";
            else
                this.textBoxBody.Text = "<html><body>My App can send email <b>with every kind of attachment</b> now!</body></html>";
        }


        private void buttonBusy_Click(object sender, RoutedEventArgs e)
        {
            buttonBusy.Content = this.mailMessage.Busy.ToString();
        }

        void ResetUI()
        {
            textBlockProgress.Text = "";
            rectangleProgress.Width = 0;
            buttonAddAttachment.IsEnabled = true;
            buttonSend.Content = "SEND";
            buttonBusy.Content = "Busy?";
        }
        #endregion

        #region photo chooser
        private void buttonAddAttachment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask objPhotoChooser = new PhotoChooserTask();
                objPhotoChooser.Completed += new EventHandler<PhotoResult>(PhotoChooseCall);
                objPhotoChooser.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        void PhotoChooseCall(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                #region Copy picture from Library to isolated storage.
                string fileName = "smtp_tmp\\" + System.IO.Path.GetFileName(e.OriginalFileName);
                BinaryReader objReader = new BinaryReader(e.ChosenPhoto);
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isStore.DirectoryExists("smtp_tmp") == false) isStore.CreateDirectory("smtp_tmp");

                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages. 
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the image to isolated storage, 4K chunks at a time 
                        while ((bytesRead = e.ChosenPhoto.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }
                #endregion

                mailMessage.AddAttachment(fileName);

                this.listBoxAttachments.Items.Add(System.IO.Path.GetFileName(fileName) + " - " + FormatBytes(MailMessage.FileSize(fileName))); //debug

            }
        }
        #endregion

        #region mail handlers
        void mailMessage_Progress(object sender, Venetasoft.WP7.ValueEventArgs<int> e)
        {
            try
            {
                this.rectangleProgress.Width = (int)((421 * e.Value) / 100);

                if (e.Value == 0)
                    textBlockProgress.Text = "Connecting...";
                else
                    textBlockProgress.Text = "Sending " + e.Value.ToString() + "%";
            }
            catch (Exception)
            {
            }
        }

        void mailMessage_MailSent(object sender, Venetasoft.WP7.ValueEventArgs<bool> e)
        {
            if (e.Value == false)   //mail not sent         
            {
                string errMsg = mailMessage.LastError;
                if (errMsg.Contains("Connection lost") == false && errMsg.Contains("Aborted by user") == false)
                    errMsg += "\r\nLast server response: " + mailMessage.LastServerResponse;

                MessageBox.Show("Error sending mail: " + errMsg);

            }
            else
            {
                MessageBox.Show("Email successfully queued to Microsoft Live mail server.");
            }

            ResetUI();
        }

        void mailMessage_Error(object sender, Venetasoft.WP7.ErrorEventArgs e)
        {
            MessageBox.Show(e.Value, "Error sending mail", MessageBoxButton.OK);
            ResetUI();
        }
        #endregion

        #region page event handlers
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (mailMessage != null && mailMessage.Busy == true)
            {
                if (MessageBox.Show("Sending in progress, abort and exit ?", "", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (mailMessage != null && mailMessage.Busy == true)
                    mailMessage.Abort();
            }

            base.OnBackKeyPress(e);
        }


        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {

            if (mailMessage != null && mailMessage.Busy == true)
                mailMessage.Abort();

            base.OnNavigatedFrom(e);
        }
        #endregion

        #region utils
        private static string FormatBytes(long bytes, bool noFloat = false)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);
            decimal ret = 0;

            foreach (string order in orders)
            {
                if (bytes > max)
                {
                    ret = decimal.Divide(bytes, max);
                    if (noFloat == true)
                        ret = (int)ret;

                    return string.Format("{0:##.##} {1}", ret, order);
                }

                max /= scale;
            }
            return "0 Bytes";
        }
        #endregion

    }
}