using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WidgetEmbed
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public static InputBox Instance;
        private void InputBox_Load(object sender, EventArgs e)
        {

            this.TopMost = true;


            TextInput.TextChanged += (s, eargs) =>
            {
                if (TextInput.TextLength == 0)
                    OKButton.Enabled = false;
                else
                    OKButton.Enabled = true;
            };

            OKButton.Click += (s, eargs) =>
            {
                returnMessage.SetResult(TextInput.Text);
            };

            CancelButton.Click += (s, eargs) =>
            {
                returnMessage.SetResult("");
            };

            this.FormClosing += (s, eargs) =>
            {
                eargs.Cancel = true;

                if (returnMessage != null)
                    returnMessage.SetResult("");
            };
        }

        TaskCompletionSource<string> returnMessage;
        public async Task<string> Show(string Message, string Title, string DefaultText = "", int maxTextLength = 0) //Check if there are issues and things that need to be invoked.
        {

            OKButton.Enabled = false;

            if (DefaultText.Length > 0)
                OKButton.Enabled = true;

            MessageLabel.Text = Message;
            this.Text = Title;
            TextInput.MaxLength = maxTextLength;
            TextInput.Text = DefaultText;

            this.ShowInTaskbar = true;
            this.Visible = true;

            
            returnMessage = new TaskCompletionSource<string>();
            await returnMessage.Task;

            this.Visible = false;
            this.ShowInTaskbar = false;

            return returnMessage.Task.Result;
        }
    }
}
