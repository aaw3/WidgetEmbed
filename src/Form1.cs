using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace WidgetEmbed
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static List<Form2> WidgetWindows = new List<Form2>();
        public static List<Widget> Widgets = new List<Widget>();
        public static bool BeginningShutdown = false;

        string directoryLocation = Environment.ExpandEnvironmentVariables(@"%Public%\Documents\WidgetEmbed\");
        string widgetFileName = "SavedWidgets.wgt";

        bool widgetsVisible = true;
        bool isAnimatingWidgets;
        bool isAnimatingForm;
        bool formVisible = true;


        public ContextMenuStrip contextMenu1 = new ContextMenuStrip();
        public ToolStripMenuItem MenuItem1 = new ToolStripMenuItem("HideWidgets");
        public ToolStripMenuItem MenuItem2 = new ToolStripMenuItem("HideAddWidgetsMenu");
        public ToolStripMenuItem MenuItem3 = new ToolStripMenuItem("CloseApplication");
        NotifyIcon ni = new NotifyIcon();

        public static bool ScreenBoundsDetection = true;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_QUERYENDSESSION || m.Msg == WM_ENDSESSION)
            {
                BeginningShutdown = true;
                ShuttingDown();
            }
            base.WndProc(ref m);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SystemEvents.SessionEnding += (s, eargs) =>
            {
                ShuttingDown();
                eargs.Cancel = true;
            };

            InputBox.Instance = new InputBox();

            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.Visible = false;
            this.Opacity = 0;

            this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Size.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - this.Size.Height) / 2);


            button1.Enabled = false;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.Add("Website URL");
            comboBox1.Items.Add("HTML Embed Widget");

            ni.Visible = true;
            ni.Icon = SystemIcons.Application;
            ni.Text = "WidgetEmbed (Visible)";
            ni.ContextMenuStrip = contextMenu1;

            contextMenu1.Items.Add(MenuItem1);
            contextMenu1.Items.Add(MenuItem2);
            contextMenu1.Items.Add(MenuItem3);

            MenuItem1.Text = "Hide Widgets";
            MenuItem2.Text = "Hide Add Widgets Menu";
            MenuItem3.Text = "Exit Application";

            MenuItem1.Image = SystemIcons.Application.ToBitmap();
            MenuItem2.Image = SystemIcons.Question.ToBitmap();
            MenuItem3.Image = SystemIcons.Error.ToBitmap();

            MenuItem1.Click += (s, eargs) =>
            {
                ChangeWidgetVisibility();
            };

            MenuItem2.Click += (s, eargs) =>
            {
                if (isAnimatingForm)
                    return;

                formVisible = (this.Opacity == 1 ? true : false);

                MenuItem2.Text = (formVisible ? "Show" : "Hide") + " Add Widgets Menu";

                Fade(this, (formVisible ? "FadeOut" : "FadeIn"), 10);
            };

            MenuItem3.Click += (s, eargs) =>
            {
                TryClose();
            };

            this.FormClosing += (s, fcea) =>
            {
                if (!BeginningShutdown)
                {
                    fcea.Cancel = TryClose();
                }
                else
                {
                    ShuttingDown();
                    fcea.Cancel = true;
                }
            };

            ni.MouseDoubleClick += (s, mea) =>
            {
                ChangeWidgetVisibility();
            };

            CheckBoundsButton.Click += (s, eargs) =>
            {
                CheckBounds();
            };


            //Serialiation
            if (!Directory.Exists(directoryLocation))
            {
                Directory.CreateDirectory(directoryLocation);
            }

            if (!File.Exists(directoryLocation + widgetFileName))
            {
                File.Create(directoryLocation + widgetFileName).Close();
            }
            else if (new FileInfo(directoryLocation + widgetFileName).Length == 0)
            {
                //file exists, but is empty!
            }
            else
            {
                LoadWidgets();
            }

            this.Visible = true;
            this.Opacity = 1;
        }

        const int WM_QUERYENDSESSION = 0x0011;
        const int WM_ENDSESSION = 0x0016;


        public void ShuttingDown()
        {
            if (SaveEmbedsCheckbox.Checked)
            {
                SaveWidgets();
            }

            for (int i = 0; i < WidgetWindows.Count; i++)
            {
                WidgetWindows[i].Close();
            }


            Environment.Exit(0);
        }




        public bool TryClose()
        {

            DialogResult dr = MessageBox.Show(new Form { TopMost = true }, "Are you sure you want to exit?", "Program Exiting Prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                if (SaveEmbedsCheckbox.Checked)
                    SaveWidgets();


                ni.Visible = false;
                ni.Icon = null;

                Environment.Exit(0);
            }
            return true;
        }

        public void CheckBounds()
        {
            for (int i = 0; i < WidgetWindows.Count; i++)
            {
                if (!IsOnScreen(WidgetWindows[i]))
                    if (MessageBox.Show(new Form { TopMost = true }, "Widget: \"" + Widgets[i].WidgetTitle + "\" was not detected within the safe bounds area.\nWould you like to relocate it to the center of the main screen?", "Safe Bounds Detection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        WidgetWindows[i].Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - WidgetWindows[i].Size.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - WidgetWindows[i].Size.Height) / 2);
                    }
            }

            MessageBox.Show(new Form { TopMost = true }, "Bound Check Complete!", "Safe Bounds Detection", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ChangeWidgetVisibility()
        {
            if (isAnimatingWidgets)
                return;

            widgetsVisible = !widgetsVisible;

            ni.Text = "WidgetEmbed " + (widgetsVisible ? "(Visible)" : "(Hidden)");
            MenuItem1.Text = (widgetsVisible ? "Hide" : "Show") + " Widgets";

            if (widgetsVisible)
            {
                for (int i = 0; i < WidgetWindows.Count; i++)
                {
                    Fade(WidgetWindows[i], "FadeIn", 10);

                }
            }
            else
            {
                for (int i = 0; i < WidgetWindows.Count; i++)
                {
                    //WidgetWindows[i].Enabled = false;
                    Fade(WidgetWindows[i], "FadeOut", 10);

                }
            }
        }

        SerializedWidgetData sd;





        public static bool IsOnScreen(Form form)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                Rectangle formRectangle = new Rectangle(form.Left, form.Top, 60, 60);

                if (screen.WorkingArea.Contains(formRectangle))
                {
                    return true;
                }
            }

            return false;
        }






        private async void Fade(Form o, string FadeType, int interval = 80)
        {
            bool AnimatingForm = (o == this);

            if (AnimatingForm)
                isAnimatingForm = true;
            else
                isAnimatingWidgets = true;
            
            if (FadeType == "FadeIn")
            {
                if (AnimatingForm)
                {

                    this.ShowInTaskbar = true;
                    this.Visible = true;
                }

                //Object is not fully invisible. Fade it in
                while (o.Opacity < 1.0)
                {
                    await Task.Delay(interval);
                    o.Opacity += 0.05;
                }
                o.Opacity = 1; //make fully visible

                if (AnimatingForm)
                    isAnimatingForm = false;
                else
                    isAnimatingWidgets = false;
            }
            else if (FadeType == "FadeOut")
            {
                //Object is fully visible. Fade it out
                while (o.Opacity > 0.0)
                {
                    await Task.Delay(interval);
                    o.Opacity -= 0.05;
                }
                o.Opacity = 0; //make fully invisible

                if (AnimatingForm)
                {
                    isAnimatingForm = false;
                    {
                        this.ShowInTaskbar = false;
                        this.Visible = false;
                    }
                }
                else
                    isAnimatingWidgets = false;
            }
        }


        public async void SaveWidgets()
        {
            if (sd == null)
                sd = new SerializedWidgetData();

            sd.SaveOnClose = SaveEmbedsCheckbox.Checked;
            sd.PrimaryScreenDetection = BoundsDetectionCheckbox.Checked;

            if (isAnimatingForm)
            {
                double check1 = this.Opacity;
                await Task.Delay(1000);
                double check2 = this.Opacity;

                sd.WidgetMenuShown = check1 < check2 ? true : false;
            }
            else
            {
                sd.WidgetMenuShown = this.Opacity == 1 ? true : false;
            }

            sd.Widgets = Widgets;

            SerializeXml<SerializedWidgetData>(sd, directoryLocation + widgetFileName);
        }

        public void LoadWidgets()
        {
            sd = DeserializeXml<SerializedWidgetData>(directoryLocation + widgetFileName);

            if (sd == null)
                return;

            SaveEmbedsCheckbox.Checked = sd.SaveOnClose;
            BoundsDetectionCheckbox.Checked = sd.PrimaryScreenDetection;

            if (sd.WidgetMenuShown)
            {
                this.Visible = true;
                this.Opacity = 1;
            }
            else
            {
                this.Opacity = 0;
                this.ShowInTaskbar = false;
                formVisible = false;
                MenuItem2.Text = "Show Add Widgets Menu";
            }


            for (int i = 0; i < sd.Widgets.Count; i++)
            {
                Form f2 = new Form2(sd.Widgets[i].WidgetWebData, sd.Widgets[i].WidgetLoadType, sd.Widgets[i].WidgetWindowSize, sd.Widgets[i].WidgetLocation, sd.Widgets[i].WidgetZoomScale, sd.Widgets[i].WidgetTitle);
                f2.Show();

                SetWindowPos(f2.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            }

            if (BoundsDetectionCheckbox.Checked)
            {
                this.Shown += (s, eargs) =>
                {
                    CheckBounds();
                };

            }
        }

        public T SerializeXml<T>(T objToSerialize, string outputFile)
        {
            try
            {

                XmlSerializer s = new XmlSerializer(typeof(SerializedWidgetData));

                using (TextWriter tw = new StreamWriter(outputFile))
                {
                    s.Serialize(tw, objToSerialize);
                    tw.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopMost = true }, "Error while saving data!\nError:" + ex.Message, "Error Has Occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return default(T);
        }

        public T DeserializeXml<T>(string inputFile)
        {
            try
            {

                XmlSerializer s = new XmlSerializer(typeof(T));
                T deserializedObject = default(T);

                using (TextReader tr = new StreamReader(inputFile))
                {
                    deserializedObject = (T)s.Deserialize(tr);
                    tr.Close();
                }

                return deserializedObject;
            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopMost = true }, "Save file corrupted!\nGenerating New File\nError:" + ex.Message, "Error Has Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return default(T);
        }



    private void button1_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2(textBox1.Text, (Form2.LoadType)comboBox1.SelectedIndex);
            f2.Show();

            SetWindowPos(f2.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
                button1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveWidgets();
        }

    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    public const UInt32 SWP_NOSIZE = 0x0001;
    public const UInt32 SWP_NOMOVE = 0x0002;
    public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    const int SW_MAXIMIZE = 3;
    const int SW_MINIMIZE = 6;
    const int SW_SHOW = 5;
    const int SW_HIDE = 0;
    const int SW_SHOWNORMAL = 1;
    const int SW_RESTORE = 9;
    }



    public class SerializedWidgetData
    {
        public List<Widget> Widgets = new List<Widget>();
        public bool SaveOnClose;
        public bool WidgetMenuShown;
        public bool PrimaryScreenDetection;
    }
}
