using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using MySql.Data;
using MySql.Data.MySqlClient;
using MapForm.publicClass;

namespace MapForm
{
    public partial class Form1 : Form
    {
        private int num;
        GMapOverlay objects = new GMapOverlay("objects"); //放置marker的图层
        private Timer blinkTimer = new Timer();
        private GMapMarkerImage currentMarker;
        private bool isLeftButtonDown = false;
        //private machine[] m_list = new machine[10000];
        private MapForm.publicClass.MatinInfoViewPar[] m_list = new MapForm.publicClass.MatinInfoViewPar[10000];
        public Form1()
        {
            InitializeComponent();
            try
            {
                System.Net.IPHostEntry e = System.Net.Dns.GetHostEntry("www.google.com.hk");
            }
            catch
            {
                gmap.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET Machine List", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            get_machine_data();
        }

        private void gmap_Load(object sender, EventArgs e)
        {
            gmap.MapProvider = GMap.NET.MapProviders.GoogleChinaMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            gmap.MinZoom = 1;  //最小缩放
            gmap.MaxZoom = 17; //最大缩放
            gmap.Zoom = 5;     //当前缩放
            gmap.ShowCenter = false; //不显示中心十字点
            gmap.DragButton = System.Windows.Forms.MouseButtons.Left;
            gmap.Position = new PointLatLng(32.064, 118.704); //地图中心位置：南京
            
            //int len = m_list.Length;
            for (int cnt = 0; cnt < num; ++cnt)
            {
                Bitmap bitmap = Bitmap.FromFile(m_list[cnt].PathName) as Bitmap;
                //GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(m_list[cnt].longitude, m_list[cnt].latitude), bitmap);
                GMapMarkerImage marker = new GMapMarkerImage(new PointLatLng( m_list[cnt].Latitude,m_list[cnt].Longitude), bitmap);
                marker.Longitude = m_list[cnt].Longitude;
                marker.Latitude = m_list[cnt].Latitude;
                marker.PathName = m_list[cnt].PathName;
                marker.IsFlash = m_list[cnt].IsFlash;
                marker.View = "";
                int view_len = m_list[cnt].View.Length;
                for (int i = 0; i < view_len; ++i)
                {
                    marker.View +=  "\n" + m_list[cnt].View[i];
                }
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                string status = marker.IsFlash ? "异常" : "正常";
                marker.ToolTipText = string.Format("经度：{0}\n纬度：{1}\n状态：{2}{3}", marker.Longitude, marker.Latitude, status, marker.View);
                objects.Markers.Add(marker);
                try
                {
                    objects.Markers.Add(marker);
                }
                catch
                {
                    System.Console.WriteLine("sorry for here");
                }
                
            }
            try
            {
                gmap.Overlays.Add(objects);
            }
            catch
            {
                System.Console.WriteLine("sorry for object");
            }

            //gmap.MouseClick += new MouseEventHandler(gmap_MouseClick);
            gmap.MouseDown += new MouseEventHandler(gmap_MouseDown);
            gmap.MouseUp += new MouseEventHandler(gmap_MouseUp);
            gmap.MouseMove += new MouseEventHandler(gmap_MouseMove);

            gmap.OnMarkerClick += new MarkerClick(gmap_OnMarkerClick);
            gmap.OnMarkerEnter += new MarkerEnter(gmap_OnMarkerEnter);
            gmap.OnMarkerLeave += new MarkerLeave(gmap_OnMarkerLeave);
            load_Flash();
        }
        void gmap_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //objects.Markers.Clear();
                PointLatLng point = gmap.FromLocalToLatLng(e.X, e.Y);
                //GMapMarker marker = new GMarkerGoogle(point, GMarkerGoogleType.green);
                Bitmap bitmap = Bitmap.FromFile("C:\\Users\\dell\\Documents\\Visual Studio 2010\\Projects\\WindowsFormsApplication6\\WindowsFormsApplication6\\images\\water1.png") as Bitmap;
                //GMapMarker marker = new GMarkerGoogle(point, bitmap);
                GMapMarker marker = new GMapMarkerImage(point, bitmap);
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                marker.ToolTipText = string.Format("{0},{1}", point.Lat, point.Lng);
                objects.Markers.Add(marker);
            }
        }
        void gmap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && isLeftButtonDown)
            {
                if (currentMarker != null)
                {
                    PointLatLng point = gmap.FromLocalToLatLng(e.X, e.Y);
                    currentMarker.Position = point;
                    currentMarker.ToolTipText = string.Format("{0},{1}", point.Lat, point.Lng);
                }
            }
        }
        void gmap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                isLeftButtonDown = false;
            }
        }
        void gmap_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                isLeftButtonDown = true;
            }
        }
        void gmap_OnMarkerLeave(GMapMarker item)
        {
            if (item is GMapMarkerImage)
            {
                currentMarker = null;
                GMapMarkerImage m = item as GMapMarkerImage;
                if (m.Pen != null)
                {
                    m.Pen.Dispose();
                    m.Pen = null;
                }
            }
        }
        void gmap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (item is GMapMarkerImage)
            {
                currentMarker = null;
                GMapMarkerImage m = item as GMapMarkerImage;
                String info_i = "经度:" + m.Longitude;
                info_i += "\n纬度:" + m.Latitude;
                info_i += "\n是否异常:" + m.IsFlash;
                info_i += m.View;
                MessageBox.Show(info_i);
            }
        }
        void gmap_OnMarkerEnter(GMapMarker item)
        {
            if (item is GMapMarkerImage)
            {
                currentMarker = item as GMapMarkerImage;
                currentMarker.Pen = new Pen(Brushes.Red, 2);
            }
        }
        
        void get_machine_data()
        {
            num = 0;
            MapForm.publicClass.MatinInfoViewPar tmp1 = new MapForm.publicClass.MatinInfoViewPar();
            tmp1.Longitude = 117.16;
            tmp1.Latitude = 31.62;
            tmp1.PathName = "C:\\Users\\dell\\Documents\\Visual Studio 2010\\Projects\\WindowsFormsApplication6\\WindowsFormsApplication6\\images\\water1.png";
            tmp1.IsFlash = false;
            string[] data1 = {"1号设备", "合 肥", "2015-12-04"};
            tmp1.View = data1;
            m_list[num++] = tmp1;
            MapForm.publicClass.MatinInfoViewPar tmp2 = new MapForm.publicClass.MatinInfoViewPar();
            tmp2.Longitude = 118.19;
            tmp2.Latitude = 30.55;
            tmp2.PathName = "C:\\Users\\dell\\Documents\\Visual Studio 2010\\Projects\\WindowsFormsApplication6\\WindowsFormsApplication6\\images\\water1.png";
            tmp2.IsFlash = false;
            string[] data2 = {"2号设备", "南 陵", "2015-12-03"};
            tmp2.View = data2;
            m_list[num++] = tmp2;
            MapForm.publicClass.MatinInfoViewPar tmp3 = new MapForm.publicClass.MatinInfoViewPar();
            tmp3.Longitude = 116.16;
            tmp3.Latitude = 30.25;
            tmp3.PathName = "C:\\Users\\dell\\Documents\\Visual Studio 2010\\Projects\\WindowsFormsApplication6\\WindowsFormsApplication6\\images\\water1.png";
            tmp3.IsFlash = true;
            string[] data3 = { "3号设备", "太 湖", "2015-12-02" };
            tmp3.View = data3;
            m_list[num++] = tmp3;

        }
        private void load_Flash()
        {
            blinkTimer.Interval = 100;
            blinkTimer.Tick += new EventHandler(blinkTimer_Tick);
            blinkTimer.Start();
           
        }
        void blinkTimer_Tick(object sender, EventArgs e)
        {
            foreach (GMapMarkerImage m in objects.Markers)
            {
                GMapMarkerImage marker = m as GMapMarkerImage;
                if (m.IsFlash == true)
                {
                    if (marker.OutPen == null)
                        marker.OutPen = new Pen(Brushes.Red, 2);
                    else
                    {
                        marker.OutPen.Dispose();
                        marker.OutPen = null;
                    }
                }
                gmap.Refresh();
            }
        }

    }

    class GMapMarkerImage : GMapMarker
    {
        public double Longitude;
        public double Latitude;
        public string PathName;
        public bool IsFlash;
        public string View;
        private Image image;
        public Image Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                if (image != null)
                {
                    this.Size = new Size(image.Width, image.Height);
                }
            }
        }
        public Pen Pen {get;set;}
        public Pen OutPen{get;set;}
        public GMapMarkerImage(GMap.NET.PointLatLng p, Image image)
            : base(p)
        {
            Size = new System.Drawing.Size(image.Width, image.Height);
            Offset = new System.Drawing.Point(-Size.Width / 2, -Size.Height / 2);
            this.image = image;
            Pen = null;
            OutPen = null;
        }
        public override void OnRender(Graphics g)
        {
            if (image == null)
                return;
            Rectangle rect = new Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
            g.DrawImage(image, rect);
            if (Pen != null)
            {
                g.DrawRectangle(Pen, rect);
            }
            if (OutPen != null)
            {
                g.DrawEllipse(OutPen, rect);
            }
        }

        public override void Dispose()
        {
            if (Pen != null)
            {
                Pen.Dispose();
                Pen = null;
            }
            if (OutPen != null)
            {
                OutPen.Dispose();
                OutPen = null;
            }
            base.Dispose();
        }
    }


}
