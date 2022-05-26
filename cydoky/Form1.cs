using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;

// Для теста
using System.Management;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Diagnostics;

namespace cydoky
{
    public partial class Form1 : Form
    {
        square9 pole = new square9();
        Point cell = new Point(0, 0);
        int countHelp;
        int countFill;
        string[,] guess = new string[9, 9];
        string guessTemplate = "     \n     \n     ";
        int countMusic = 0;

        [DllImport("winmm.dll")]
        private static extern long mciSendString( string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, int hwndCallback );

        public Form1()
        {
            InitializeComponent();
            SetDoubleBuffered(dataGridView1, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            toolTip1.SetToolTip(button2, "Открывает выделенную клетку\nили левую верхнюю");
            toolTip2.SetToolTip(btnMusic, "Музон включается не сразу");
            countHelp = helps.Value;
            lblHelpCount.Text = countHelp.ToString();
            this.KeyUp += new KeyEventHandler(press_num);

            using (var midiStream = new MemoryStream(Properties.Resources.m1))
            {
                var data = midiStream.ToArray();
                try
                {
                    using (var fs = new FileStream("midi1.mid", FileMode.CreateNew, FileAccess.Write))
                    {
                        fs.Write(data, 0, data.Length);
                    }
                }
                catch (IOException)
                { }
            }

            using (var midiStream = new MemoryStream(Properties.Resources.m2))
            {
                var data = midiStream.ToArray();
                try
                {
                    using (var fs = new FileStream("midi2.mid", FileMode.CreateNew, FileAccess.Write))
                    {
                        fs.Write(data, 0, data.Length);
                    }
                }
                catch (IOException)
                { }
            }

            using (var midiStream = new MemoryStream(Properties.Resources.m3))
            {
                var data = midiStream.ToArray();
                try
                {
                    using (var fs = new FileStream("midi3.mid", FileMode.CreateNew, FileAccess.Write))
                    {
                        fs.Write(data, 0, data.Length);
                    }
                }
                catch (IOException)
                { }
            }

            if (countHelp == 0)
            {
                button2.Enabled = false;
                lblHelpCount.Visible = false;
            }

            for (int a = 0; a < 9; a++)
            {
                for (int b = 0; b < 9; b++)
                {
                    guess[a, b] = guessTemplate;
                }
            }
            int i = 1;

            while (i < 10)
            {
                dataGridView1.Columns.Add("Column" + i, i.ToString());
                dataGridView1.Columns[i - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns[i - 1].Resizable = DataGridViewTriState.False;
                dataGridView2.Columns.Add("Column" + i, i.ToString());
                dataGridView2.Columns[i - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView2.Columns[i - 1].Resizable = DataGridViewTriState.False;
                i++;
            }

            dataGridView2.Columns.Add("NullName", "10");
            dataGridView2.Columns[9].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.Columns[9].Resizable = DataGridViewTriState.False;
            dataGridView1.RowTemplate.Height = dataGridView1.Height / 9;
            dataGridView2.RowTemplate.Height = dataGridView1.Height / 9;
            dataGridView1.Rows.Add(9);
            dataGridView1.Rows[8].Frozen = true;
            dataGridView2.Rows.Add(1);
            dataGridView2.Rows[0].Frozen = true;
            i = 0;

            while (i < 9)
            {
                dataGridView2.Rows[0].Cells[i].Value = i + 1;
                i++;
            }

            dataGridView2.Rows[0].Cells[9].Value = "";
            randomizator();
            set_pole();
            set_empty();
            progress();
        }

        public void SetDoubleBuffered( Control c, bool value )
        {
            PropertyInfo pi = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic);
            if (pi != null)
            {
                pi.SetValue(c, value, null);

                MethodInfo mi = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
                if (mi != null)
                {
                    mi.Invoke(c, new object[] { ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true });
                }

                mi = typeof(Control).GetMethod("UpdateStyles", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
                if (mi != null)
                {
                    mi.Invoke(c, null);
                }
            }
        }

        private void randomizator()
        {
            int r = 200, x, y, X, Y, temp;
            Random rnd = new Random();
            while (r > 0)
            {
                x = rnd.Next(0, 3);
                y = rnd.Next(0, 3);
                X = rnd.Next(0, 3);
                Y = rnd.Next(0, 2);

                if (Y != 0)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        temp = pole.x[i / 3, X].x[i % 3, x];
                        pole.x[i / 3, X].x[i % 3, x] = pole.x[i / 3, X].x[i % 3, y];
                        pole.x[i / 3, X].x[i % 3, y] = temp;
                    }
                }
                else
                {
                    for (int i = 0; i < 9; i++)
                    {
                        temp = pole.x[X, i / 3].x[x, i % 3];
                        pole.x[X, i / 3].x[x, i % 3] = pole.x[X, i / 3].x[y, i % 3];
                        pole.x[X, i / 3].x[y, i % 3] = temp;
                    }
                }
                r--;
            }
        }

        private void set_pole()
        {
            for (int a = 0; a < 3; a++)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            dataGridView1.Rows[j + 3 * a].Cells[k + 3 * i].Value = pole.x[a, i].x[j, k] == 0 ? "" : pole.x[a, i].x[j, k].ToString();
                            dataGridView1.Rows[j + 3 * a].Cells[k + 3 * i].Style.ForeColor = Color.Empty;
                            dataGridView1.Rows[j + 3 * a].Cells[k + 3 * i].Tag = 1;
                        }
                    }
                }
            }
        }

        private void set_empty()
        {
            int r = level.Value * 15, x, y;
            Random rnd = new Random();
            while (r > 0)
            {
                x = rnd.Next(0, 9);
                y = rnd.Next(0, 9);
                dataGridView1.Rows[x].Cells[y].Value = "";
                dataGridView1.Rows[x].Cells[y].Style.ForeColor = Color.Blue;
                dataGridView1.Rows[x].Cells[y].Tag = 0;
                r--;
            }
        }

        private void button1_Click( object sender, EventArgs e )
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.Empty;
                    dataGridView1.Rows[i].Cells[j].Style.ForeColor = Color.Empty;
                    dataGridView1.Rows[i].Cells[j].Style.WrapMode = DataGridViewTriState.False;
                    dataGridView1.Rows[i].Cells[j].Style.Font = new Font("Microsoft Sans Serif", 16f);
                    guess[i, j] = guessTemplate;
                }
            }

            if (guessBar.Value == 2)
            {
                button3.BackColor = SystemColors.Control;
                button3.ForeColor = SystemColors.ControlText;
                button3.Visible = false;
            }
            else
            {
                button3.Visible = true;
            }

            randomizator();
            set_pole();
            set_empty();
            cell.X = 0;
            cell.Y = 0;
            countHelp = helps.Value;
            lblHelpCount.Text = countHelp.ToString();

            if (countHelp == 0)
            {
                button2.Enabled = false;
                lblHelpCount.Visible = false;
            }
            else
            {
                button2.Enabled = true;
                lblHelpCount.Visible = true;
            }
            progress();
        }

        private void paintNumbers()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (dataGridView1.CurrentCell.Value.ToString() != ""
                        && dataGridView1.CurrentCell.Value.ToString() == dataGridView1.Rows[i].Cells[j].Value.ToString()
                        && dataGridView1.CurrentCell.Style.WrapMode != DataGridViewTriState.True)
                    {
                        dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.Aqua;
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.Empty;
                    }
                }
            }
        }

        private bool progressTest()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (dataGridView1.Rows[i].Cells[j].Value.ToString() == dataGridView1.Rows[i].Cells[i].Value.ToString()
                        && i != j)
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < 9; i += 3)
            {
                for (int j = 0; j < 9; j += 3)
                {
                    for (int a = i; a < i + 3; a++)
                    {
                        for (int b = j; b < j + 3; b++)
                        {
                            if (dataGridView1.Rows[a].Cells[b].Value.ToString() == dataGridView1.Rows[a].Cells[a].Value.ToString()
                                && a != b)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private void dataGridView1_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            using (Pen gridPen = new Pen(Color.Black, 4))
            {
                if ((e.RowIndex % 3) == 0)
                {
                    e.Graphics.DrawLine(gridPen, e.CellBounds.X, e.CellBounds.Top - 1, e.CellBounds.Right - 1, e.CellBounds.Top - 1);
                }

                if ((e.ColumnIndex % 3) == 0)
                {
                    e.Graphics.DrawLine(gridPen, e.CellBounds.X, e.CellBounds.Top, e.CellBounds.X, e.CellBounds.Bottom);
                }
            }

        }

        private void dataGridView1_SelectionChanged( object sender, EventArgs e )
        {
            dataGridView1.ClearSelection();
            dataGridView1.Refresh();
        }

        private void dataGridView1_Click( object sender, EventArgs e )
        {
            paintNumbers();

            if (dataGridView1.CurrentCell.Style.ForeColor != Color.Empty)
            {
                dataGridView1.Rows[cell.X].Cells[cell.Y].Style.BackColor = Color.White;
                dataGridView1.CurrentCell.Style.BackColor = Color.Yellow;
                cell.X = dataGridView1.CurrentCell.RowIndex;
                cell.Y = dataGridView1.CurrentCell.ColumnIndex;
            }
        }

        private void dataGridView2_SelectionChanged( object sender, EventArgs e )
        {
            dataGridView2.ClearSelection();
        }

        private void dataGridView2_Click( object sender, EventArgs e )
        {
            if (dataGridView1.CurrentCell.Style.ForeColor != Color.Empty)
            {
                if (button3.BackColor == SystemColors.Control)
                {
                    dataGridView1.Rows[cell.X].Cells[cell.Y].Style.WrapMode = DataGridViewTriState.False;
                    dataGridView1.Rows[cell.X].Cells[cell.Y].Style.Font = new Font("Microsoft Sans Serif", 16f);
                    dataGridView1.Rows[cell.X].Cells[cell.Y].Value = dataGridView2.CurrentCell.Value;
                    dataGridView1.Rows[cell.X].Cells[cell.Y].Style.ForeColor = Color.Blue;
                    guess[cell.X, cell.Y] = guessTemplate;

                    if (dataGridView2.CurrentCell.ColumnIndex == 9)
                    {
                        dataGridView1.Rows[cell.X].Cells[cell.Y].Tag = 0;
                        paintNumbers();
                        dataGridView1.CurrentCell.Style.BackColor = Color.Yellow;
                        return;
                    }
                    else
                    {
                        dataGridView1.Rows[cell.X].Cells[cell.Y].Tag = 1;
                    }

                    progress();
                    int index = (Convert.ToInt32(dataGridView2.CurrentCell.Value) - 1) * 2;

                    for (int i = 0; i < 9; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[cell.Y].Style.WrapMode == DataGridViewTriState.True
                            && i != cell.X)
                        {
                            guess[i, cell.Y] = guess[i, cell.Y].Remove(index, 1).Insert(index, " ");
                            dataGridView1.Rows[i].Cells[cell.Y].Value = guess[i, cell.Y];
                        }

                        if (dataGridView1.Rows[cell.X].Cells[i].Style.WrapMode == DataGridViewTriState.True
                            && i != cell.Y)
                        {
                            guess[cell.X, i] = guess[cell.X, i].Remove(index, 1).Insert(index, " ");
                            dataGridView1.Rows[cell.X].Cells[i].Value = guess[cell.X, i];
                        }
                    }

                    for (int i = cell.X / 3 * 3; i < cell.X / 3 * 3 + 3; i++)
                    {
                        for (int j = cell.Y / 3 * 3; j < cell.Y / 3 * 3 + 3; j++)
                        {
                            if (dataGridView1.Rows[i].Cells[j].Style.WrapMode == DataGridViewTriState.True
                                && i != cell.X
                                && j != cell.Y)
                            {
                                guess[i, j] = guess[i, j].Remove(index, 1).Insert(index, " ");
                                dataGridView1.Rows[i].Cells[j].Value = guess[i, j];
                            }
                        }
                    }
                }
                else
                {
                    dataGridView1.Rows[cell.X].Cells[cell.Y].Style.WrapMode = DataGridViewTriState.True;
                    dataGridView1.Rows[cell.X].Cells[cell.Y].Style.Font = new Font("Courier", 8.25f);
                    bool str = false, col = false, sq = false;

                    for (int i = 0; i < 9; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[cell.Y].Value.ToString() == dataGridView2.CurrentCell.Value.ToString()
                            && i != cell.X)
                        {
                            str = true;
                            break;
                        }

                        if (dataGridView1.Rows[cell.X].Cells[i].Value.ToString() == dataGridView2.CurrentCell.Value.ToString()
                            && i != cell.Y)
                        {
                            col = true;
                            break;
                        }
                    }

                    for (int i = cell.X / 3 * 3; i < cell.X / 3 * 3 + 3; i++)
                    {
                        for (int j = cell.Y / 3 * 3; j < cell.Y / 3 * 3 + 3; j++)
                        {
                            if (dataGridView1.Rows[i].Cells[j].Value.ToString() == dataGridView2.CurrentCell.Value.ToString()
                                && i != cell.X
                                && j != cell.Y)
                            {
                                sq = true;
                                break;
                            }
                        }
                    }

                    if (dataGridView2.CurrentCell.Value.ToString() != "")
                    {
                        int index = (Convert.ToInt32(dataGridView2.CurrentCell.Value) - 1) * 2;

                        if (guess[cell.X, cell.Y][index] == ' ' && !str && !col && !sq)
                        {
                            guess[cell.X, cell.Y] = guess[cell.X, cell.Y].Remove(index, 1).Insert(index, dataGridView2.CurrentCell.Value.ToString());
                        }
                        else
                        {
                            guess[cell.X, cell.Y] = guess[cell.X, cell.Y].Remove(index, 1).Insert(index, " ");
                        }

                        dataGridView1.Rows[cell.X].Cells[cell.Y].Value = guess[cell.X, cell.Y];
                    }
                    else
                    {
                        dataGridView1.Rows[cell.X].Cells[cell.Y].Style.WrapMode = DataGridViewTriState.False;
                        dataGridView1.Rows[cell.X].Cells[cell.Y].Style.Font = new Font("Microsoft Sans Serif", 16f);
                        dataGridView1.Rows[cell.X].Cells[cell.Y].Value = dataGridView2.CurrentCell.Value;
                        guess[cell.X, cell.Y] = guessTemplate;
                        return;
                    }
                }

                paintNumbers();
                dataGridView1.Rows[cell.X].Cells[cell.Y].Style.BackColor = Color.Yellow;
            }
        }

        private void button2_Click( object sender, EventArgs e )
        {
            if (dataGridView1.Rows[cell.X].Cells[cell.Y].Style.ForeColor == Color.Empty)
            {
                return;
            }

            dataGridView1.Rows[cell.X].Cells[cell.Y].Style.WrapMode = DataGridViewTriState.False;
            dataGridView1.Rows[cell.X].Cells[cell.Y].Style.Font = new Font("Microsoft Sans Serif", 16f);
            dataGridView1.Rows[cell.X].Cells[cell.Y].Value = pole.x[cell.X / 3, cell.Y / 3].x[cell.X % 3, cell.Y % 3];
            dataGridView1.Rows[cell.X].Cells[cell.Y].Style.ForeColor = Color.Empty;
            dataGridView1.Rows[cell.X].Cells[cell.Y].Style.BackColor = Color.Empty;
            dataGridView1.Rows[cell.X].Cells[cell.Y].Tag = 1;
            countHelp--;
            lblHelpCount.Text = countHelp.ToString();
            progress();

            int index = (Convert.ToInt32(dataGridView1.Rows[cell.X].Cells[cell.Y].Value) - 1) * 2;

            for (int i = 0; i < 9; i++)
            {
                if (dataGridView1.Rows[i].Cells[cell.Y].Style.WrapMode == DataGridViewTriState.True
                    && i != cell.X)
                {
                    guess[i, cell.Y] = guess[i, cell.Y].Remove(index, 1).Insert(index, " ");
                    dataGridView1.Rows[i].Cells[cell.Y].Value = guess[i, cell.Y];
                }

                if (dataGridView1.Rows[cell.X].Cells[i].Style.WrapMode == DataGridViewTriState.True
                    && i != cell.Y)
                {
                    guess[cell.X, i] = guess[cell.X, i].Remove(index, 1).Insert(index, " ");
                    dataGridView1.Rows[cell.X].Cells[i].Value = guess[cell.X, i];
                }
            }

            for (int i = cell.X / 3 * 3; i < cell.X / 3 * 3 + 3; i++)
            {
                for (int j = cell.Y / 3 * 3; j < cell.Y / 3 * 3 + 3; j++)
                {
                    if (dataGridView1.Rows[i].Cells[j].Style.WrapMode == DataGridViewTriState.True
                        && i != cell.X
                        && j != cell.Y)
                    {
                        guess[i, j] = guess[i, j].Remove(index, 1).Insert(index, " ");
                        dataGridView1.Rows[i].Cells[j].Value = guess[i, j];
                    }
                }
            }

            if (countHelp == 0)
            {
                button2.Enabled = false;
                lblHelpCount.Visible = false;
                return;
            }
        }

        private void progress()
        {
            countFill = 0;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    countFill += Convert.ToInt32(dataGridView1.Rows[i].Cells[j].Tag);
                }
            }

            progressBar1.Value = countFill;
            lblProgress.Text = (countFill * 100 / 81).ToString() + " %";

            if (countFill == 81)
            {
                for (int a = 0; a < 3; a++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                if (dataGridView1.Rows[j + 3 * a].Cells[k + 3 * i].Value.ToString() != pole.x[a, i].x[j, k].ToString())
                                {
                                    dataGridView1.Rows[j + 3 * a].Cells[k + 3 * i].Style.ForeColor = Color.Red;
                                    dataGridView1.Rows[j + 3 * a].Cells[k + 3 * i].Tag = 0;
                                    countFill--;
                                }
                                else
                                {
                                    dataGridView1.Rows[j + 3 * a].Cells[k + 3 * i].Style.ForeColor = Color.Empty;
                                }
                            }
                        }
                    }
                }

                if (countFill == 81 || progressTest())
                {
                    Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(this.Location.X, this.Location.Y, 0, 0, new Size(pictureBox1.Width, pictureBox1.Height));
                    }
                    pictureBox1.Image = bmp;
                    pictureBox1.Visible = true;
                    painting_goblet();
                    MessageBox.Show("Ты победитель!");
                    pictureBox1.Visible = false;
                }
                else
                {
                    progress();
                }
            }
        }

        private void painting_goblet()
        {
            Bitmap bmp = new Bitmap(Properties.Resources.star);
            Random rnd = new Random();
            Graphics g = Graphics.FromImage(pictureBox1.Image);

            for (int i = 0; i < 100; i++)
            {
                g.DrawImage(bmp, rnd.Next(-240, 933), rnd.Next(-200, 550));
            }
        }

        private void button3_Click( object sender, EventArgs e )
        {
            if (button3.BackColor == SystemColors.Control)
            {
                button3.BackColor = SystemColors.Highlight;
                button3.ForeColor = SystemColors.HighlightText;
            }
            else
            {
                button3.BackColor = SystemColors.Control;
                button3.ForeColor = SystemColors.ControlText;
            }

            dataGridView1.Refresh();
        }

        private void trackBar2_Scroll( object sender, EventArgs e )
        {
            switch (trackBar2.Value)
            {
            case 0:
                {
                    this.BackgroundImage = null;
                    break;
                }
            case 1:
                {
                    this.BackgroundImage = Properties.Resources.i01 as Bitmap;
                    break;
                }
            case 2:
                {
                    this.BackgroundImage = Properties.Resources.i02 as Bitmap;
                    break;
                }
            case 3:
                {
                    this.BackgroundImage = Properties.Resources.i03 as Bitmap;
                    break;
                }
            case 4:
                {
                    this.BackgroundImage = Properties.Resources.i04 as Bitmap;
                    break;
                }
            case 5:
                {
                    this.BackgroundImage = Properties.Resources.i05 as Bitmap;
                    break;
                }
            case 6:
                {
                    this.BackgroundImage = Properties.Resources.i06 as Bitmap;
                    break;
                }
            }
        }

        private void btnMusic_Click( object sender, EventArgs e )
        {
            string sCommand;

            switch (countMusic % 6)
            {
            case 0:
                {
                    btnMusic.BackgroundImage = Properties.Resources.i07;
                    lblNumTrack.Text = "1";
                    lblNumTrack.Visible = true;
                    sCommand = "open \"" + Application.StartupPath + "/midi1.mid" + "\" alias " + "MIDIapp";
                    mciSendString(sCommand, null, 0, 0);
                    sCommand = "play " + "MIDIapp";
                    mciSendString(sCommand, null, 0, 0);
                    break;
                }
            case 2:
                {
                    btnMusic.BackgroundImage = Properties.Resources.i07;
                    lblNumTrack.Text = "2";
                    lblNumTrack.Visible = true;
                    sCommand = "open \"" + Application.StartupPath + "/midi2.mid" + "\" alias " + "MIDIapp";
                    mciSendString(sCommand, null, 0, 0);
                    sCommand = "play " + "MIDIapp";
                    mciSendString(sCommand, null, 0, 0);
                    break;
                }
            case 4:
                {
                    btnMusic.BackgroundImage = Properties.Resources.i07;
                    lblNumTrack.Text = "3";
                    lblNumTrack.Visible = true;
                    sCommand = "open \"" + Application.StartupPath + "/midi3.mid" + "\" alias " + "MIDIapp";
                    mciSendString(sCommand, null, 0, 0);
                    sCommand = "play " + "MIDIapp";
                    mciSendString(sCommand, null, 0, 0);
                    break;
                }
            default:
                {
                    btnMusic.BackgroundImage = Properties.Resources.i08;
                    lblNumTrack.Visible = false;
                    mciSendString("close MIDIapp", null, 0, 0);
                    break;
                }
            }

            countMusic++;
        }

        private void btnHelp_Click( object sender, EventArgs e )
        {
            if(lblRules.Visible == false)
            {
                lblRules.Visible = true;
            }
            else
            {
                lblRules.Visible = false;
            }
        }

        private void btnClose_Click( object sender, EventArgs e )
        {
            this.Close();
        }

        private void Form1_Paint( object sender, PaintEventArgs e )
        {
            using (Pen pen = new Pen(Color.Black, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, 932, 549);
            }
        }

        private void press_num ( object sender, KeyEventArgs e )
        {
            int key = -1;

            if(e.KeyValue > 48 && e.KeyValue <=57)
            {
                key = e.KeyValue - 48;
            }

            if (e.KeyValue > 96 && e.KeyValue < 107)
            {
                key = e.KeyValue - 96;
            }

            if (e.KeyValue == 48 || e.KeyValue == 96)
            {
                key = 10;
            }

            if (key!=-1)
            {
                dataGridView2.Rows[0].Cells[key - 1].Selected = true;
                dataGridView2_Click(dataGridView2, null);
            }
        }

        //Тестовые махинации

        static string GetExplorerUser()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Process WHERE Name='explorer.exe' AND ProcessId=" + GetParentExplorerProcessId());
            var explorerProcesses = new ManagementObjectSearcher(query).Get();
            foreach (ManagementObject mo in explorerProcesses)
            {
                string[] ownerInfo = new string[2];
                mo.InvokeMethod("GetOwner", (object[])ownerInfo);
                return ownerInfo[0];
            }
            return "Тот-Кого-Нельзя-Называть";
        }

        private static uint GetParentExplorerProcessId()
        {
            return GetParentExplorerProcessId(Process.GetCurrentProcess().Id);
        }

        private static uint GetParentExplorerProcessId( int processId )
        {
            const uint ERROR = 0;

            IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (hSnapshot == IntPtr.Zero) return ERROR;

            PROCESSENTRY32 procEntry = new PROCESSENTRY32();
            procEntry.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));

            if (Process32First(hSnapshot, ref procEntry) == false) return ERROR;

            List<PROCESSENTRY32> ids = new List<PROCESSENTRY32>();
            do
            {
                ids.Add(procEntry);
            }
            while (Process32Next(hSnapshot, ref procEntry));
            CloseHandle(hSnapshot);

            uint parentId = ids.Where(pe => pe.th32ProcessID == processId).FirstOrDefault().th32ParentProcessID;
            for (; parentId != 0;)
            {
                string name = ids.Where(pe => pe.th32ProcessID == parentId).FirstOrDefault().szExeFile;
                if ("explorer.exe".Equals(name, StringComparison.OrdinalIgnoreCase)) return parentId;

                parentId = ids.Where(pe => pe.th32ProcessID == parentId).FirstOrDefault().th32ParentProcessID;
            }

            return ERROR;
        }

        const uint TH32CS_SNAPPROCESS = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateToolhelp32Snapshot( uint dwFlags, uint th32ProcessID );

        [DllImport("kernel32.dll")]
        static extern bool Process32First( IntPtr hSnapshot, ref PROCESSENTRY32 lppe );

        [DllImport("kernel32.dll")]
        static extern bool Process32Next( IntPtr hSnapshot, ref PROCESSENTRY32 lppe );

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle( IntPtr hObject );

        private void Form1_Load( object sender, EventArgs e )
        {
            MessageBox.Show("Привет, " + GetExplorerUser() + "!");
        }

        private void button4_Click( object sender, EventArgs e )
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }

    public class square3
    {
        public int[,] x;
        public square3()
        {
            x = new int[3, 3];
        }
        public square3(int i, int j)
        {
            string str = i.ToString() + j.ToString();
            switch (str)
            {
            case "00":
                {
                    x = new int[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
                    break;
                }
            case "01":
                {
                    x = new int[3, 3] { { 4, 5, 6 }, { 7, 8, 9 }, { 1, 2, 3 } };
                    break;
                }
            case "02":
                {
                    x = new int[3, 3] { { 7, 8, 9 },{ 1, 2, 3 }, { 4, 5, 6 } };
                    break;
                }
            case "10":
                {
                    x = new int[3, 3] { { 2, 3, 4 }, { 5, 6, 7 }, { 8, 9, 1 } };
                    break;
                }
            case "11":
                {
                    x = new int[3, 3] { { 5, 6, 7 }, { 8, 9, 1 }, { 2, 3, 4 } };
                    break;
                }
            case "12":
                {
                    x = new int[3, 3] { { 8, 9, 1 }, { 2, 3, 4 }, { 5, 6, 7 } };
                    break;
                }
            case "20":
                {
                    x = new int[3, 3] { { 3, 4, 5 }, { 6, 7, 8 }, { 9, 1, 2 } };
                    break;
                }
            case "21":
                {
                    x = new int[3, 3] { { 6, 7, 8 }, { 9, 1, 2 }, { 3, 4, 5 } };
                    break;
                }
            case "22":
                {
                    x = new int[3, 3] { { 9, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 } };
                    break;
                }
            }
        }
    }

    public class square9
    {
        public square3[,] x;
        public square9()
        {
            x = new square3[3, 3];
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    x[j, k] = new square3(j,k);
                }
            }
        }
    }
}
