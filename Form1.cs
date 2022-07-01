using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebAPIFileUpload.Models;

namespace ImageUploader
{
    public partial class frmImageUploader : Form
    {
        public frmImageUploader()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog opnfd = new OpenFileDialog();
            opnfd.Filter = "Image Files (*.jpg;*.jpeg;.*.gif;)|*.jpg;*.jpeg;.*.gif";
            if (opnfd.ShowDialog() == DialogResult.OK)
            {
                picImage.Image = new Bitmap(opnfd.FileName);
                txtUpload.Text = opnfd.FileName;
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            Boolean uploadStatus = false;
          
            string url = "http://localhost/WebAPIFileUpload/api/FileHandlingAPI";
            string filePath = @"\";
            Random rnd = new Random();
            string uploadFileName = "Imag" + rnd.Next(9999).ToString();
            uploadStatus = Upload(url, filePath, txtUpload.Text, uploadFileName);

            if (uploadStatus)
            {
                MessageBox.Show("File Uploaded");
            }
            else
            {
                MessageBox.Show("File Not Uploaded");
            }
        }

        bool Upload(string url, string filePath, string localFilename, string uploadFileName)
        {
            Boolean isFileUploaded = false;

            try
            {
                HttpClient httpClient = new HttpClient();
             
                var fileStream = new FileStream(localFilename, FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
                var fileInfo = new FileInfo(localFilename);
                UploadFIle uploadResult = null;
                bool _fileUploaded = false;

                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Headers.Add("filePath", filePath);
                content.Add(new StreamContent(fileStream), "\"file\"", string.Format("\"{0}\"", uploadFileName + fileInfo.Extension)
                        );

                Task taskUpload = httpClient.PostAsync(url, content).ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        var response = task.Result;

                        if (response.IsSuccessStatusCode)
                        {
                            uploadResult = response.Content.ReadAsAsync<UploadFIle>().Result;
                            if (uploadResult != null)
                                _fileUploaded = true;
                        }
                    }
                    fileStream.Dispose();
                });

                taskUpload.Wait();
                if (_fileUploaded)
                    isFileUploaded = true;
                httpClient.Dispose();

            }
            catch (Exception ex)
            {
                isFileUploaded = false;
            }
            return isFileUploaded;
        }
    }
}
