using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Single2013
{
    public partial class AddModify2 : Form
    {
        public AddModify2()
        {
            InitializeComponent();
        }

        public string text1
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }
        public string text2
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }
        public string text3
        {
            get { return textBox3.Text; }
            set { textBox3.Text = value; }
        }
        public string text4
        {
            get { return textBox4.Text; }
            set { textBox4.Text = value; }
        }

        public string static1
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public string static2
        {
            get { return label2.Text; }
            set { label2.Text = value; }
        }
        public string static3
        {
            get { return label3.Text; }
            set { label3.Text = value; }
        }

        public string static4
        {
            get { return label4.Text; }
            set { label4.Text = value; }
        }
    }
}
