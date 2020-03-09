using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Adan
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToString("ss.ff"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //User32Util.SpyStockForm();
            User32Util util = new User32Util();
            util.Spy();
        }
    }
}
