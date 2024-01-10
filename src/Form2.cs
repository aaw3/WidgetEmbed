using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using Microsoft.VisualBasic;

namespace WidgetEmbed
{
    public partial class Form2 : Form
    {
        ReSize resize = new ReSize();

        public ChromiumWebBrowser browser;

        string defaultTitle = "Title (Middle Click)";
        public Form2(string websiteData, LoadType loadType)
        {
            widget.WidgetWebData = websiteData;
            widget.WidgetLoadType = loadType;
            
            InitializeComponent();
            InitBrowser();

            this.Load += (s, eargs) =>
            {
                widget.WidgetLocation = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Size.Width) / 2, (Screen.PrimaryScreen.WorkingArea.Height - this.Size.Height) / 2);

                SetWidgetTitle(defaultTitle);
            };

        }

        public Form2(string websiteData, LoadType loadType, Size widgetSize, Point widgetLocation, double zoomScale, string widgetTitle)
        {

            widget.WidgetWebData = websiteData;
            widget.WidgetLoadType = loadType;
            widget.WidgetWindowSize = widgetSize;
            widget.WidgetLocation = widgetLocation;
            widget.WidgetZoomScale = zoomScale;

            InitializeComponent();
            InitBrowser();

            SetWidgetTitle(widgetTitle);
        }

        public enum LoadType
        {
            WebsiteURL,
            HTMLEmbed
        }


        public static bool cefInit = false;
        public static bool ignoreLinkLoadingStateChanged;

        public Widget widget = new Widget();

        bool waitingToSetZoom = true;

        public void InitBrowser()
        {
            if (!cefInit)
            {
                Cef.Initialize(new CefSettings());
                cefInit = true;
            }

            browser = new ChromiumWebBrowser();
            browser.RequestHandler = new BrowserRequestHandler();
            browser.LifeSpanHandler = new BrowserLifeSpanHandler();


            browser.IsBrowserInitializedChanged += (s, e) =>
            {
                if (!browser.IsBrowserInitialized)
                    return;


                if (widget.WidgetLoadType == LoadType.HTMLEmbed)
                    browser.LoadHtml(widget.WidgetWebData, false);
                else if (widget.WidgetLoadType == LoadType.WebsiteURL)
                    browser.Load(widget.WidgetWebData);

                this.Invoke(new Action(() => this.Location = widget.WidgetLocation));

                if (!(widget.WidgetWindowSize == default))
                    this.Invoke(new Action(() => this.Size = widget.WidgetWindowSize));


                this.Move += (sender, eargs) =>
                {
                    widget.WidgetLocation = this.Location;
                };

                this.SizeChanged += (sender, eargs) =>
                {
                    widget.WidgetWindowSize = this.Size;
                };

                if (!(widget.WidgetZoomScale == 0))
                browser.LoadingStateChanged += (sender, eargs) =>
                {
                    if (!eargs.IsLoading)
                        if (waitingToSetZoom)
                        {
                            browser.SetZoomLevel(widget.WidgetZoomScale);
                            waitingToSetZoom = false;
                        }
                };
            };


            this.panel1.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;

            this.ShowInTaskbar = false;
        }

        public async void UpdateTitle()
        {
            string Title = await InputBox.Instance.Show("Enter new widget title:", "Rename Widget", widget.WidgetTitle == defaultTitle ? "" : widget.WidgetTitle, 32); //Interaction.InputBox("Enter new widget title:\n(16 characters max)", "Rename Widget");

            if (Title == widget.WidgetTitle)
                return;

            if (Title.Length == 0)
                MessageBox.Show(new Form { TopMost = true }, "Title rename has been cancelled", "Rename Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                SetWidgetTitle(Title);

                MessageBox.Show(new Form { TopMost = true }, "Title has been renamed to: \"" + Title + "\"", "Rename Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }
        }

        public void SetWidgetTitle(string Title)
        {
            widget.WidgetTitle = Title;
            WindowTitle.Text = Title;
        }


        #region WndProc stuff
        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;
        private const int WM_NCLBUTTONDBLCLK = 0x00A3; //double click on a title bar a.k.a. non-client area of the form

        int Mx;
        int My;
        int Sw;
        int Sh;

        bool mov;

        void SizerMouseDown(object sender, MouseEventArgs e)
        {
            mov = true;
            My = MousePosition.Y;
            Mx = MousePosition.X;
            Sw = Width;
            Sh = Height;
        }

        void SizerMouseMove(object sender, MouseEventArgs e)
        {
            if (mov == true)
            {
                Width = MousePosition.X - Mx + Sw;
                Height = MousePosition.Y - My + Sh;
            }
        }

        void SizerMouseUp(object sender, MouseEventArgs e)
        {
            mov = false;
        }

        public override Size MinimumSize
        {
            get
            {
                return base.MinimumSize;
            }
            set
            {
                base.MinimumSize = new Size(179, 51);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {

            int x = (int)(m.LParam.ToInt64() & 0xFFFF);               //get x mouse position
            int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);   //get y mouse position  you can gave (x,y) it from "MouseEventArgs" too
            Point pt = PointToClient(new Point(x, y));

            if (m.Msg == 0x84)
            {
                switch (resize.getMosuePosition(pt, this))
                {
                    case "l": m.Result = (IntPtr)10; return;  // the Mouse on Left Form
                    case "r": m.Result = (IntPtr)11; return;  // the Mouse on Right Form
                    case "a": m.Result = (IntPtr)12; return;
                    case "la": m.Result = (IntPtr)13; return;
                    case "ra": m.Result = (IntPtr)14; return;
                    case "u": m.Result = (IntPtr)15; return;
                    case "lu": m.Result = (IntPtr)16; return;
                    case "ru": m.Result = (IntPtr)17; return; // the Mouse on Right_Under Form
                    case "": m.Result = pt.Y < 32 /*mouse on title Bar*/ ? (IntPtr)2 : (IntPtr)1; return;

                }
            }
            else if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                m.Result = IntPtr.Zero;
                return;
            } 

            base.WndProc(ref m);
        }
        #endregion

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }



        private void Browser_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultButtonDesign(Button b)
        {
            b.BackColor = Color.Black;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Color.White;
            b.FlatAppearance.BorderSize = 1;

            b.GotFocus += (s, e) =>
            {
                this.Focus();
            };
        }


        private void Form2_Load(object sender, EventArgs e)
        {

            Form1.WidgetWindows.Add(this);
            Form1.Widgets.Add(widget);

            foreach (var btn in this.Controls.OfType<Button>())
            {
                SetDefaultButtonDesign(btn);
            }

            this.FormClosing += (s, eargs) =>
            {
                if (deletingWidget)
                {
                    Form1.WidgetWindows.Remove(this);
                    Form1.Widgets.Remove(this.widget);

                    browser.Dispose();
                }
                else if (Form1.BeginningShutdown)
                {
                    //browser.Dispose();
                }
                else
                    eargs.Cancel = true;
            };

            WindowTitle.MouseDown += (s, eargs) => //allows for title (label) to still allow movement but comes at the cost of not being able to check double click with lmb
            {
                if (eargs.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            WindowTitle.MouseClick += (s, eargs) =>
            {
                if (eargs.Button == MouseButtons.Middle)
                {
                    UpdateTitle();
                }
            };
        }

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        public class BrowserLifeSpanHandler : ILifeSpanHandler
        {

            public bool DoClose(IWebBrowser browserControl, IBrowser browser)
            {
                return false;
            }

            public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
            {

            }
            public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
            {

            }

            //Popup Window
            public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {
                newBrowser = null;

                DialogResult dr = MessageBox.Show(new Form { TopMost = true }, "Would you like to open this 'Popup' in your browser?\n" + "\"" + targetUrl + "\"", "Popup Opening Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                    Process.Start(targetUrl);

                return true;
            }


        }



        public class BrowserRequestHandler : IRequestHandler
        {
            //Link Click
            public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
            {
                ignoreLinkLoadingStateChanged = true;

                if ((request.Url.StartsWith("http") || request.Url.StartsWith("www")) && (request.TransitionType == TransitionType.ClientRedirect || request.TransitionType == TransitionType.LinkClicked))
                {
                    DialogResult dr = MessageBox.Show("Would you like to open this 'Link' in your browser?\n" + "\"" + request.Url + "\"", "Link Opening Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dr == DialogResult.Yes)
                        Process.Start(request.Url);

                    return true;
                }
                return false;

            }

            public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
            {

                return false;
            }

            public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
            {
                callback.Dispose();
                return false;
            }

            public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
            {
                throw new Exception("Plugin crashed!");
            }

            public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
                IRequestCallback callback)
            {
                return CefReturnValue.Continue;
            }

            public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
            {

                callback.Dispose();
                return false;
            }

            public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
            {
                return;
            }

            public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
            {
                //NOTE: In most cases you examine the request.Url and only handle requests you are interested in
                if (request.Url.ToLower().StartsWith("https://cefsharp.example")
                    //|| request.Url.ToLower().StartsWith(CefSharpSchemeHandlerFactory.SchemeName) //idk!
                    || request.Url.ToLower().StartsWith("mailto:")
                    || request.Url.ToLower().StartsWith("https://googlechrome.github.io/samples/service-worker/"))
                {
                    return new ResourceRequestHandler();
                }

                return null;
            }

            public bool OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, bool isProxy, string host, int port,
                X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
            {
                callback.Dispose();
                return false;
            }

            public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
            {
                throw new Exception("Browser render process is terminated!");
            }

            public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize,
                IRequestCallback callback)
            {
                callback.Dispose();
                return false;
            }

            public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
            {
                var url = newUrl;
                newUrl = url;
            }

            public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
            {
                return url.StartsWith("mailto");
            }

            public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
            {

            }

            public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
            {
                return false;
            }

            public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
                IResponse response)
            {
                return null;
            }

            public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
                IResponse response, UrlRequestStatus status, long receivedContentLength)
            {

            }
        }

        
        private void ZoomOut_Click(object sender, EventArgs e)
        {
            SetZoomLevel(-0.25);
        }

        private void ZoomIn_Click(object sender, EventArgs e)
        {
            SetZoomLevel(+0.25);
        }

        public void SetZoomLevel(double scale)
        {
            double newZoom = browser.GetZoomLevelAsync().Result + scale;
            browser.SetZoomLevel(newZoom);

            widget.WidgetZoomScale = newZoom;
        }

        private void Reload_Click(object sender, EventArgs e)
        {
            if (widget.WidgetLoadType == LoadType.HTMLEmbed)
                browser.LoadHtml(widget.WidgetWebData, false);
            else if (widget.WidgetLoadType == LoadType.WebsiteURL)
                browser.Load(widget.WidgetWebData);

        }

        bool deletingWidget = false;
        private void Exit_Click(object sender, EventArgs e)
        {

            DialogResult dr = MessageBox.Show("Would you delete the widget \"" + WindowTitle.Text + "\"?\nIt will be permanently deleted.\n(This can be undone by not saving the current widgets)", "Delete Widget?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                deletingWidget = true;
                this.Close();
            }
        }

        public void RenameTitle(string Title)
        {
            WindowTitle.Text = Title;
        }
    }

    class ReSize
    {

        private bool Above, Right, Under, Left, Right_above, Right_under, Left_under, Left_above;


        int Thickness = 6;  //Thickness of border  u can cheang it
        int Area = 8;     //Thickness of Angle border 


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thickness">set thickness of form border</param>
        public ReSize(int thickness)
        {
            Thickness = thickness;
        }

        /// <summary>
        /// Constructor set thickness of form border=1
        /// </summary>
        public ReSize()
        {
            Thickness = 10;
        }

        //Get Mouse Position
        public string getMosuePosition(Point mouse, Form form)
        {
            bool above_underArea = mouse.X > Area && mouse.X < form.ClientRectangle.Width - Area; /* |\AngleArea(Left_Above)\(=======above_underArea========)/AngleArea(Right_Above)/| */ //Area===>(==)
            bool right_left_Area = mouse.Y > Area && mouse.Y < form.ClientRectangle.Height - Area;

            bool _Above = mouse.Y <= Thickness;  //Mouse in Above All Area
            bool _Right = mouse.X >= form.ClientRectangle.Width - Thickness;
            bool _Under = mouse.Y >= form.ClientRectangle.Height - Thickness;
            bool _Left = mouse.X <= Thickness;

            Above = _Above && (above_underArea); if (Above) return "a";   /*Mouse in Above All Area WithOut Angle Area */
            Right = _Right && (right_left_Area); if (Right) return "r";
            Under = _Under && (above_underArea); if (Under) return "u";
            Left = _Left && (right_left_Area); if (Left) return "l";


            Right_above =/*Right*/ (_Right && (!right_left_Area)) && /*Above*/ (_Above && (!above_underArea)); if (Right_above) return "ra";     /*if Mouse  Right_above */
            Right_under =/* Right*/((_Right) && (!right_left_Area)) && /*Under*/(_Under && (!above_underArea)); if (Right_under) return "ru";     //if Mouse  Right_under 
            Left_under = /*Left*/((_Left) && (!right_left_Area)) && /*Under*/ (_Under && (!above_underArea)); if (Left_under) return "lu";      //if Mouse  Left_under
            Left_above = /*Left*/((_Left) && (!right_left_Area)) && /*Above*/(_Above && (!above_underArea)); if (Left_above) return "la";      //if Mouse  Left_above

            return "";

        }

    }

    public class Widget
    {
        public string WidgetTitle;
        public string WidgetWebData;
        public Form2.LoadType WidgetLoadType;
        public double WidgetZoomScale;
        public Size WidgetWindowSize;
        public Point WidgetLocation;
    }
}