using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Threading;

namespace serial_to_post
{
    public partial class Form1 : Form
    {
        string[] led1=new string[2]; string[] id = new string[2];
        private StringBuilder builder = new StringBuilder();//避免在事件处理方法中反复的创建，定义到外面。
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Closing(object sender,FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void boxenter()
        {

            string str, str1;
            int ii = 150;
            for (int i = 1; i < 9; i++)
            {
                ii = ii * 2;
                str1 = ii.ToString();
                comboBox2.Items.Add(str1);
            }
            comboBox2.Items.Add(43000);
            comboBox2.Items.Add(56000);
            comboBox2.Items.Add(57600);
            comboBox2.Items.Add(115200);

            //for (int i = 0; i < 20; i++)
            //{
            //    str = i.ToString();
            //    str = "COM" + str;
            //    comboBox1.Items.Add(str);
            //}
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            comboBox1.Items.AddRange(ports);//自动获取串口

            comboBox3.Items.Add("None");
            comboBox3.Items.Add("Odd");
            comboBox3.Items.Add("Event");
            comboBox3.Items.Add("Mark");
            comboBox3.Items.Add("Space");
            comboBox4.Items.Add(8);
            comboBox4.Items.Add(7);
            comboBox4.Items.Add(6);
            comboBox5.Items.Add("None");
            comboBox5.Items.Add("One");
            comboBox5.Items.Add("OnePointFive");
            comboBox5.Items.Add("Two");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            boxenter();
        }
        private void serialPort1_DataReceived(object sender, EventArgs e)
        {
            while (serialPort1.BytesToRead != 0)
            {
                Thread.Sleep(100);
                if (serialPort1.IsOpen)
                {
                    /*延时为了让所有数据到达,不延时会有数据丢失
                    如果数据太长延时100ms也是不够的，最好把接
                    收的数据存入缓存区中,校检数据完整后再发出*/
                    int count = serialPort1.BytesToRead;
                    byte[] data = new byte[count];
                    serialPort1.Read(data, 0, data.Length);
                    builder.Clear();//清除字符串构造器的内容
                    //因为要访问ui资源，所以需要使用invoke方式同步ui。
                    this.Invoke((EventHandler)(delegate
                    {
                        //判断是否是显示为16进制
                        if (radioButton2.Checked)
                        {
                            //依次的拼接出16进制字符串
                            foreach (byte b in data)
                            {
                                builder.Append(b.ToString("X2") + " ");
                            }
                        }
                        else if (radioButton1.Checked)
                        {
                            //直接按UTF8规则转换成字符串，ASCII码转中文乱码，UTF8不会
                            builder.Append(Encoding.UTF8.GetString(data));
                            if (checkBox1.Checked)//post发送到url
                            {
                                string url = textBox3.Text;
                                //转化为json格式  
                                //string json = saveLoad();
                                //led1=1&sensors1=11333&sensors2=11&temperature=11
                                string json = builder.ToString();
                                //string led1 = "1", temperature = "23", sensors1 = "23", sensors2 = "23";
                                //string json = "led1" + "=" + led1 + "&" + "sensors1" + "=" + sensors1
                                //    + "&" + "sensors2" + "=" + sensors2 + "&" + "temperature" +
                                //    "=" + temperature;
                                url = textBox3.Text;
                                //提交数据  
                                string value = HttpPost(url, json);
                                //textBox1.AppendText(value);//post后收到的回复
                            }
                        }
                        //追加的形式添加到文本框末端，并滚动到最后。
                        //textBox2.AppendText(System.DateTime.Now.ToString() + ": " + builder.ToString() +  "\n") ;
                        textBox1.AppendText(count.ToString() + "个字符: " + builder.ToString() + "\n");
                    }));
                }
                else Thread.CurrentThread.Abort();
            }
        }

        private void boxwrite()
        {
            serialPort1.PortName = comboBox1.Text;
            serialPort1.BaudRate = int.Parse(comboBox2.Text);

            //serialPort1.Parity = (System.IO.Ports)comboBox3.Text;//校验位
            if (comboBox3.Text == "Even")
                serialPort1.Parity = Parity.Even;
            if (comboBox3.Text == "Mark")
                serialPort1.Parity = Parity.Mark;
            if (comboBox3.Text == "None")
                serialPort1.Parity = Parity.None;
            if (comboBox3.Text == "Odd")
                serialPort1.Parity = Parity.Odd;
            if (comboBox3.Text == "Space")
                serialPort1.Parity = Parity.Space;

            if (comboBox4.Text != "")
                serialPort1.DataBits = int.Parse(comboBox4.Text);//数据位

            //serialPort1.StopBits = StopBits.None;//停止位
            if (comboBox5.Text == "None")
                serialPort1.StopBits = StopBits.None;
            if (comboBox5.Text == "One")
                serialPort1.StopBits = StopBits.One;
            if (comboBox5.Text == "OnePointFive")
                serialPort1.StopBits = StopBits.OnePointFive;
            if (comboBox5.Text == "Two")
                serialPort1.StopBits = StopBits.Two;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    boxwrite();//写入串口设置
                    serialPort1.Open();//打开串口
                    button2.Text = "关闭串口";
                    serialPort1.DataReceived += serialPort1_DataReceived;//串口接受程序
                    if (checkBox2.Checked)
                    {
                        int time=int.Parse(comboBox6.Text);
                        //Thread th = new Thread(new ThreadStart(() => ThreadMethod(client)));
                        Thread th = new Thread(new ThreadStart(() => receget(time)));
                        th.IsBackground = true;
                        th.Start();
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.ToString());
                }
            }
            else if (serialPort1.IsOpen)
            {
                button2.Text = "打开串口";
                serialPort1.Close();
            }
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);//执行事件
        }
        private delegate void settext();
        private void receget(int time)
        {
            while (true)
            {
                if (serialPort1.IsOpen)
                {
                    if (checkBox2.Checked)
                    {
                        settext t = new settext(getreceive);
                        this.Invoke(t);//通过代理调用刷新方法
                        Thread.Sleep(time);
                    }
                }
                else {
                    Thread.CurrentThread.Abort();
                }
            }
        }
        public void getreceive()
        {
            string Url = textBox4.Text;
             HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);//创建一个HTTP请求
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentType = "text/xml";
            //byte[] bData = UnicodeEncoding.UTF8.GetBytes(postDataStr);
            //request.ContentLength = bData.Length;
            //request.ContentLength = postDataStr.Length;
            //request.Proxy =null;
            //StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8);//获取request的相应流
            //writer.Write(postDataStr);
            //writer.Flush();
            try
            {
                id[1] = id[0];
                led1[1] = led1[0];
                   HttpWebResponse response = (HttpWebResponse)request.GetResponse();//发送响应
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //默认编码  
                }
                //Stream stream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(stream, encoding);
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                response.Close();
                request.Abort(); //string[] Strs = myString.Split(':', 3);
                JObject jo = (JObject)JsonConvert.DeserializeObject(retString);
                id[0] = jo["id"].ToString();
                if (id[0] != id[1])
                {
                    led1[0] = "led1=" + jo["led1"].ToString();//提取json数据中的led1,并写入（led1=1）
                   // textBox1.AppendText(led1[0]);
                    this.serialPort1.WriteLine(led1[0]);
                }
                else if(id[0] == id[1])
                {
                    led1[0] = "led1=" + jo["led1"].ToString();//提取json数据中的led1,并写入（led1=1）
                    if (led1[0] != led1[1])
                    {
                       // textBox1.AppendText(led1[0]);
                        this.serialPort1.WriteLine(led1[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                string retString = ex.ToString();
                MessageBox.Show(retString);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (textBox2.Text == "")
                {
                    MessageBox.Show("发送数据为空！");
                    return;
                }
                if (radioButton3.Checked)//ASCII码直接发送
                {
                    string serialStringTemp = this.textBox2.Text;
                    this.serialPort1.WriteLine(serialStringTemp);
                }
                else if (radioButton4.Checked)
                {
                    byte[] BSendTemp = System.Text.Encoding.UTF8.GetBytes(textBox2.Text); //string转字节存入数组
                    serialPort1.Write(BSendTemp, 0, BSendTemp.Length);//发送数据    
                }
            }
            else MessageBox.Show("串口关闭中，请开启串口");
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        public string HttpPost(string Url, string postDataStr)//post 代码
        {//respond发送    request接收
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);//创建一个HTTP请求
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.104 Safari/537.36 Core/1.53.2669.400 QQBrowser/9.6.10990.400";
            //byte[] bData = UnicodeEncoding.UTF8.GetBytes(postDataStr);
            //request.ContentLength = bData.Length;
            //request.ContentLength = postDataStr.Length;
            //request.Proxy =null;
            Thread.Sleep(100);
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8);//获取request的相应流
            writer.Write(postDataStr);
            writer.Flush();
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();//发送响应
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //默认编码  
                }
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                writer.Close();
                writer.Dispose();
                reader.Close();
                reader.Dispose();
                response.Close();
                request.Abort();
                return retString;
            }
            catch (Exception ex) { 
                string retString = ex.ToString();
                return retString;
            }
        }
        public string saveLoad(string id, string temperature, string sensors1, string sensors2, string led1)
        {
            //{ id":80,"temperature":33,"sensors1":33,"sensors2":33,"led1":1,
            //  "created_at":"2017 - 05 - 08 08:42:16",
            //    "updated_at":"2017 - 05 - 08 08:42:16}
            StringWriter sw = new StringWriter();
            JsonWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(id);
            writer.WritePropertyName("temperature");
            writer.WriteValue(temperature);
            writer.WritePropertyName("sensors1");
            writer.WriteValue(sensors1);
            writer.WritePropertyName("sensors2");
            writer.WriteValue(sensors2);
            writer.WritePropertyName("led1");
            writer.WriteValue(led1);
            writer.WritePropertyName("created_at");
            writer.WriteValue(DateTime.Now.ToLocalTime().ToString());//获取当前时间
            writer.WritePropertyName("updated_at");
            writer.WriteValue(DateTime.Now.ToLocalTime().ToString());
            writer.WriteEndObject();
            writer.Flush();
            string jsonText = sw.GetStringBuilder().ToString();
            return jsonText;
        }
        public string reLoad(string jsonText)
        {
            JArray ja = (JArray)JsonConvert.DeserializeObject(jsonText);
            return ja[0]["id"].ToString();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
